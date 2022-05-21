using RoomReservationApi.Models;
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
        public static async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

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

            string url = string.Format("https://www.kth.se/api/timetable/v1/reservations/search?start={0}&end={1}", start.ToFormattedString(), end.ToFormattedString());

            return Schedule.FromJson(await GetAsync(url));
        }
    }
}
