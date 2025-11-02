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
        private readonly HttpClient httpClient;
        private readonly string apiKey;

        public ApiService(HttpClient httpClient, string apiKey)
        {
            this.httpClient = httpClient;
            this.apiKey = apiKey;
        }

        public async Task<string> GetAsync(string uri)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Ocp-Apim-Subscription-key", apiKey);

            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<Schedule> GetScheduleAsync(int dayOffset)
        {
            DateTime start = DateTime.Today + TimeSpan.FromDays(dayOffset);
            DateTime end = start + TimeSpan.FromHours(23);

            string url = string.Format("https://integral-api.sys.kth.se/api/schema/v1/reservations/search?start={0}&end={1}", start.ToFormattedString(), end.ToFormattedString());

            try
            {
                string json = await GetAsync(url);

                return Schedule.FromJson(json);
            }
            catch (HttpRequestException exception)
            {
                throw new ApiException(new { scheduleResponseMessage = exception.Message }, HttpStatusCode.InternalServerError);
            }
            catch (Exception exception)
            {
                throw new ApiException(exception.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}

