using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Room
    {
        public string Name { get; set; }
        public List<ReservedTime> ReservedTimes { get; set; }

        public Room(string name)
        {
            Name = name;
            ReservedTimes = new List<ReservedTime>();
        }

        public void AddReservedTime(ReservedTime time)
        {
            ReservedTimes.Add(time);
        }
    }
}
