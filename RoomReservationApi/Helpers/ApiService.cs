using Microsoft.Extensions.Logging;
using RoomReservationApi.Models;
using Sakur.WebApiUtilities.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RoomReservationApi.Helpers
{
    public class ApiService
    {
        /// <summary>
        /// What a caller is told when the schedule could not be fetched. It is deliberately free of detail about
        /// why; the reason is logged on the server instead.
        /// </summary>
        public const string ScheduleFetchFailedMessage = "Could not get the schedule from the KTH api";

        /// <summary>
        /// The days that can be asked for. The window is what makes the schedule cache worth having: without it a
        /// caller could walk through arbitrary offsets, miss the cache on every request and turn each one into an
        /// outbound request to the KTH api. It is enforced here rather than in a caller because this is where the
        /// paths to the KTH api converge, so a new caller cannot forget it.
        /// </summary>
        public const int MinimumDayOffset = -7;
        public const int MaximumDayOffset = 30;

        private readonly HttpClient httpClient;
        private readonly string apiKey;
        private readonly ScheduleCache scheduleCache;
        private readonly ILogger<ApiService> logger;

        public ApiService(HttpClient httpClient, string apiKey, ScheduleCache scheduleCache, ILogger<ApiService> logger)
        {
            this.httpClient = httpClient;
            this.apiKey = apiKey;
            this.scheduleCache = scheduleCache;
            this.logger = logger;
        }

        public async Task<string> GetAsync(string uri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Ocp-Apim-Subscription-key", apiKey);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Gets the schedule for the day that is the given number of days from today. The schedule is cached per
        /// day, so several callers asking for the same day within the cache lifetime share a single request to
        /// the KTH api.
        /// </summary>
        /// <exception cref="ApiException">
        /// If the day offset is outside <see cref="MinimumDayOffset"/> and <see cref="MaximumDayOffset"/>.
        /// </exception>
        public Task<Schedule> GetScheduleAsync(int dayOffset)
        {
            if (dayOffset < MinimumDayOffset || dayOffset > MaximumDayOffset)
                throw new ApiException($"dayOffset must be between {MinimumDayOffset} and {MaximumDayOffset}", HttpStatusCode.BadRequest);

            DateTime day = DateTime.Today.AddDays(dayOffset);

            return scheduleCache.GetOrFetchAsync(day, () => FetchScheduleAsync(day));
        }

        /// <summary>
        /// Fetches one day from the KTH api. No cancellation token is threaded into this on purpose: the fetch is
        /// shared by every caller waiting for the same day, so cancelling it for the one that happened to start it
        /// would cancel it for all of them. The only cancellation that can reach the catch below is the http
        /// client's own timeout, which is a failed fetch and is meant to be treated as one.
        /// </summary>
        private async Task<Schedule> FetchScheduleAsync(DateTime start)
        {
            DateTime end = start + TimeSpan.FromHours(23);

            string url = string.Format("https://integral-api.sys.kth.se/api/schema/v1/reservations/search?start={0}&end={1}", start.ToFormattedString(), end.ToFormattedString());

            try
            {
                string json = await GetAsync(url);

                return Schedule.FromJson(json);
            }
            catch (Exception exception)
            {
                // The detail stays on the server. It named the source file and line of every frame, which is
                // more than a caller of a public api needs to know about the inside of this one.
                logger.LogError(exception, "Could not get the schedule for {day} from the KTH api.", start);

                throw new ApiException(ScheduleFetchFailedMessage, HttpStatusCode.InternalServerError);
            }
        }
    }
}

