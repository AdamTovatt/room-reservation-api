using Newtonsoft.Json;
using RoomReservationApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Room
    {
        public Guid? ExternalId { get; set; }
        public string Name { get; set; }
        public bool Hide { get; set; }
        public bool RequiresAccess { get; set; }
        public List<ReservedTime> ReservedTimes { get; set; }

        [JsonIgnore]
        public Building Building { get; set; }

        [JsonIgnore]
        public string BuildingName { get { if (!string.IsNullOrEmpty(buildingName)) return buildingName; return Name.ExtractBuildingName(); } }

        private string buildingName;

        public Room(string name, string buildingName, Guid? id, bool requiresAccess)
        {
            ExternalId = id;
            Name = name;
            this.buildingName = buildingName;
            RequiresAccess = requiresAccess;
            ReservedTimes = new List<ReservedTime>();
        }

        public void AddReservedTime(ReservedTime time)
        {
            ReservedTimes.Add(time);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
