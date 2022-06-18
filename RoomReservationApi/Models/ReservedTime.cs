using Newtonsoft.Json;
using RoomReservationApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class ReservedTime
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public int OccupiedRooms { get; set; }
        public string ReservationType { get; set; }
        public List<string> Staff { get; set; }
        public List<string> Programmes { get; set; }

        [JsonIgnore]
        public bool IsNow { get { return CacluateIsNow(); } }

        [JsonIgnore]
        public int Duration {get { return (int)(End - Start).TotalMinutes; } }

        public ReservedTime(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            Staff = new List<string>();
        }

        public ReservedTime(Reservation reservation)
        {
            Start = reservation.Start;
            End = reservation.End;
            Description = reservation.Description;
            Department = reservation.Department?.Name;

            Staff = new List<string>();
            if (reservation.Staff != null)
            {
                foreach (Staff staff in reservation.Staff)
                {
                    Staff.Add(staff.FullName);
                }
            }

            Programmes = new List<string>();
            if(reservation.Programmes != null)
            {
                foreach(Programme programme in reservation.Programmes)
                {
                    Programmes.Add(programme.Code);
                }
            }

            OccupiedRooms = reservation.Locations.Count;
            ReservationType = reservation.Typedesc;
        }

        private bool CacluateIsNow()
        {
            DateTimeOffset currentTime = TimeHelper.GetCachedSwedenTime();

            return Start < currentTime && End > currentTime;
        }
    }
}
