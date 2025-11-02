using Newtonsoft.Json;
using RoomReservationApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Offering
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("course")]
        public Course? Course { get; set; }

        [JsonProperty("semester")]
        public string? Semester { get; set; }

        [JsonProperty("ladokId")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long LadokId { get; set; }
    }
}
