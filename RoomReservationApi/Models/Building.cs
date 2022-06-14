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
        public string Name { get; set; }
        public double? Distance { get; set; }
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
            room.Building = this;
            Rooms.Add(room);
        }

        public void UpdateDistance(GeoCoordinate coordinate = null)
        {
            if (coordinate == null)
                coordinate = new GeoCoordinate(59.34691090376268, 18.072202439834772); //coordinates for KTH Entré

            if (Metadata != null && Metadata.Position != null)
                Distance = Metadata.Position.GetDistanceTo(coordinate);
        }

        public double GetRelevance()
        {
            double relevance = double.MinValue;

            if (Distance != null)
                relevance = -(double)Distance;

            return relevance;
        }

        public void OrderRooms()
        {
            Rooms = Rooms.OrderByDescending(x => x.Name).ToList();
        }

        public override string ToString()
        {
            return string.Format("{0} - {1} rooms", Name, Rooms.Count);
        }
    }
}
