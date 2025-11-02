using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Course
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }
    }
}
