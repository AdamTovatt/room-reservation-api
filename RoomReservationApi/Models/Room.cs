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

        //will calculate this in front end for now
        //public bool IsAvailable { get { if (ReservedTimes == null || ReservedTimes.Count == 0) return true; return !ReservedTimes.Any(x => x.IsNow); } }

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
