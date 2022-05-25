using RoomReservationApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Helpers
{
    public static class ExtensionMethods
    {
        public static string ToFormattedString(this DateTime time)
        {
            return time.ToString("yyyy-MM-ddTHH:mm:ss");
        }

        public static List<Building> OrderBuildingRooms(this List<Building> buildings)
        {
            foreach(Building building in buildings)
            {
                building.OrderRooms();
            }

            return buildings;
        }
    }
}
