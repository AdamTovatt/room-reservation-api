using RoomReservationApi.Models;
using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoomReservationApi.Helpers
{
    public class ApiHelper
    {
        private static string apiKey = null;

        public static void Initialize() // we have a separate initialize method that we call from the controller to be able to catch exceptions in a better way
        {
            if (apiKey == null)
                apiKey = EnvironmentHelper.GetEnvironmentVariable("KTH_API_KEY");
        }

        public static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Add("Ocp-Apim-Subscription-key", apiKey);

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public static async Task<Schedule> GetScheduleAsync(int dayOffset)
        {
            DateTime start = DateTime.Today + TimeSpan.FromDays(dayOffset);
            DateTime end = start + TimeSpan.FromHours(23);

            string url = string.Format("https://integral-api.sys.kth.se/api/schema/v1/reservations/search?start={0}&end={1}", start.ToFormattedString(), end.ToFormattedString());

            try
            {
                string json = await GetAsync(url);

                return Schedule.FromJson(json);
            }
            catch (WebException exception)
            {
                HttpWebResponse? response = exception.Response as HttpWebResponse;

                if (response != null)
                    throw new ApiException(new { scheduleResponseCode = response.StatusCode, scheduleResponseMessage = exception.Message }, HttpStatusCode.InternalServerError);
                else
                    throw new ApiException(new { scheduleResponseMessage = exception.Message }, HttpStatusCode.InternalServerError);
            }
            catch (Exception exception)
            {
                throw new ApiException(exception.Message, HttpStatusCode.InternalServerError);
            }
        }
    }
}
