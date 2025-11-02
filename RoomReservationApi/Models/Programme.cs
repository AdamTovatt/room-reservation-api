using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Programme
    {
        [JsonProperty("code")]
        public string? Code { get; set; }

        [JsonProperty("year")]
        public long Year { get; set; }

        [JsonProperty("specialisation", NullValueHandling = NullValueHandling.Ignore)]
        public string? Specialisation { get; set; }

        [JsonProperty("class", NullValueHandling = NullValueHandling.Ignore)]
        public string? Class { get; set; }
    }
}
