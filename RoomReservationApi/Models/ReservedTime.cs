using Newtonsoft.Json;
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
        public List<string> Staffs { get; set; }

        [JsonIgnore]
        public int Duration {get { return (int)(End - Start).TotalMinutes; } }

        public ReservedTime(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
            Staffs = new List<string>();
        }

        public ReservedTime(Reservation reservation)
        {
            Start = reservation.Start;
            End = reservation.End;
            Description = reservation.Description;
            Department = reservation.Department?.Name;

            Staffs = new List<string>();
            if (reservation.Staffs != null)
            {
                foreach (Staff staff in reservation.Staffs)
                {
                    Staffs.Add(staff.FullName);
                }
            }
        }
    }
}
