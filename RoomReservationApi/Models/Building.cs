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
        public float Relevance { get { return CalculateRelevance(); } }

        public Building(string name)
        {
            Rooms = new List<Room>();
            Name = name;
        }

        public void AddRoom(Room room)
        {
            Rooms.Add(room);
        }

        private float CalculateRelevance()
        {
            float relevance = Rooms.Count();

            if(!string.IsNullOrEmpty(Name))
            {
                if(relevantRooms.Contains(Name[0].ToString()))
                {
                    if(Name.Length < 6)
                    {
                        relevance += 100;
                    }
                }
            }

            return relevance;
        }

        public void OrderRooms()
        {
            Rooms = Rooms.OrderByDescending(x => x.IsAvailable).ToList();
        }
    }
}
