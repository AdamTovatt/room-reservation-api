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

        public double GetRelevance(Coordinate coordinate = null)
        {
            if (coordinate == null)
                coordinate = new Coordinate(59.34691090376268, 18.072202439834772); //coordinates for KTH Entré

            double relevance = 0;

            if (Metadata != null && Metadata.Position != null)
                relevance = double.MaxValue - Metadata.Position.GeographicalDistanceTo(coordinate);

            return relevance;
        }

        public void OrderRooms()
        {
            Rooms = Rooms.OrderByDescending(x => x.Name).ToList();
        }
    }
}
