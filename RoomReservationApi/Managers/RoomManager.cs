using RoomReservationApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RoomReservationApi.Managers
{
    public class RoomManager
    {
        public Dictionary<string, Room> Rooms { get; set; }

        public RoomManager(Schedule schedule = null)
        {
            Rooms = new Dictionary<string, Room>();

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("RoomReservationApi.rooms.csv"))
            using (var reader = new StreamReader(stream))
            {
                string csv = reader.ReadToEnd();

                foreach(string line in csv.Split("\n"))
                {
                    string[] lineParts = line.Split("\t");
                    if (lineParts.Length > 0)
                    {
                        string roomName = lineParts[0].Trim();
                        if (!Rooms.ContainsKey(roomName))
                        {
                            Rooms.Add(roomName, new Room(roomName));
                        }
                    }
                }
            }

            if (schedule != null)
            {
                foreach (Reservation reservation in schedule.Reservations)
                {
                    foreach (Location location in reservation.Locations)
                    {
                        if (!Rooms.ContainsKey(location.Name))
                        {
                            Rooms.Add(location.Name, new Room(location.Name));
                        }
                    }
                }
            }
        }

        public void ApplySchedule(Schedule schedule)
        {
            foreach(Reservation reservation in schedule.Reservations)
            {
                foreach(Location location in reservation.Locations)
                {
                    if(Rooms.ContainsKey(location.Name))
                    {
                        Rooms[location.Name].AddReservedTime(new ReservedTime(reservation));
                    }
                }
            }
        }

        public Room SearchStaff(string name)
        {
            foreach (Room room in Rooms.Values)
            {
                foreach (ReservedTime time in room.ReservedTimes)
                {
                    if (time.Staffs.Contains(name))
                    {
                        return room;
                    }
                }
            }

            return null;
        }
    }
}
