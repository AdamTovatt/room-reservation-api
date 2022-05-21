using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RoomReservationApi.Helpers;

namespace RoomReservationApi.Models
{
    public partial class Reservation
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("start")]
        public DateTime Start { get; set; }

        [JsonProperty("end")]
        public DateTime End { get; set; }

        [JsonProperty("typename")]
        public string Typename { get; set; }

        [JsonProperty("typedesc")]
        public string Typedesc { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("lastchanged")]
        public DateTime Lastchanged { get; set; }

        [JsonProperty("lastrevised")]
        public DateTime Lastrevised { get; set; }

        [JsonProperty("locations")]
        public List<Location> Locations { get; set; }

        [JsonProperty("staffs")]
        public List<Staff> Staffs { get; set; }

        [JsonProperty("department", NullValueHandling = NullValueHandling.Ignore)]
        public Department Department { get; set; }

        [JsonProperty("programmes")]
        public List<Programme> Programmes { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("courses")]
        public List<Course> Courses { get; set; }

        [JsonProperty("offerings")]
        public List<Offering> Offerings { get; set; }
    }

    public enum Status { Cancelled, Published };

    public partial class Reservation
    {
        public static List<Reservation> FromJson(string json) => JsonConvert.DeserializeObject<List<Reservation>>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this List<Reservation> self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }    
}
