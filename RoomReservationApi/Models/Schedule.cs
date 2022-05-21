using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Schedule
    {
        [JsonProperty("reservations")]
        public List<Reservation> Reservations { get; set; }

        public static Schedule FromJson(string json)
        {
            return new Schedule() { Reservations = JsonConvert.DeserializeObject<List<Reservation>>(json) };
        }
    }
}
