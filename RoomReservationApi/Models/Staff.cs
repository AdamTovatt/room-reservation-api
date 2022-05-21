using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Staff
    {
        [JsonProperty("kthId")]
        public string KthId { get; set; }

        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [JsonProperty("acronym")]
        public string Acronym { get; set; }
    }
}
