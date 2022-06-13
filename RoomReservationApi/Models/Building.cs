using GeoCoordinatePortable;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Building
    {
        private static readonly List<string> relevantRooms = new List<string>() { "Q", "E", "D", "M", "U", "V" };

        public string Name { get; set; }
        public List<Room> Rooms { get; set; }

        [JsonIgnore]
        public BuildingMetadata Metadata { get; set; }

        public Building(string name)
        {
            Rooms = new List<Room>();
            Name = name;
        }

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        public double GetRelevance(GeoCoordinate coordinate = null)
        {
            if (coordinate == null)
                coordinate = new GeoCoordinate(59.34691090376268, 18.072202439834772); //coordinates for KTH Entré

            double relevance = double.MinValue;

            if (Metadata != null && Metadata.Position != null)
                relevance = -Metadata.Position.GetDistanceTo(coordinate);

            return relevance;
        }

        public void OrderRooms()
        {
            Rooms = Rooms.OrderByDescending(x => x.Name).ToList();
        }
    }
}
