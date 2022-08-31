using Microsoft.AspNetCore.Http;
using RoomReservationApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
            foreach (Building building in buildings)
            {
                building.OrderRooms();
            }

            TimeHelper.ClearCachedTime(); //the time in sweden is cached so it won't have to be calculated every comparison, now we will clear the cache

            return buildings;
        }

        public static string ExtractBuildingName(this string roomName)
        {
            if (string.IsNullOrEmpty(roomName))
                return roomName;

            string firstPart = roomName.Split()[0];

            int stopIndex = 0;
            for (int i = 0; i < firstPart.Length; i++)
            {
                stopIndex = i;
                if (!char.IsLetter(firstPart[i]))
                    break;
            }

            return firstPart.Substring(0, stopIndex);
        }

        /// <summary>
        /// Get remote ip address, optionally allowing for x-forwarded-for header check
        /// </summary>
        /// <param name="context">Http context</param>
        /// <param name="allowForwarded">Whether to allow x-forwarded-for header check</param>
        /// <returns>IPAddress</returns>
        public static IPAddress GetRemoteIPAddress(this HttpContext context, bool allowForwarded = true)
        {
            if (allowForwarded)
            {
                // if you are allowing these forward headers, please ensure you are restricting context.Connection.RemoteIpAddress
                // to cloud flare ips: https://www.cloudflare.com/ips/
                string header = (context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? context.Request.Headers["X-Forwarded-For"].FirstOrDefault());
                if (IPAddress.TryParse(header, out IPAddress ip))
                {
                    return ip;
                }
            }
            return context.Connection.RemoteIpAddress;
        }

        public static List<Room> GetRooms(this List<Building> buildings)
        {
            List<Room> rooms = new List<Room>();
            
            foreach(Building building in buildings)
            {
                rooms.AddRange(building.Rooms);
            }

            return rooms;
        }

        public static bool ContainsRoom(this List<Room> rooms, Room room)
        {
            foreach(Room r in rooms)
            {
                if (room.Name == r.Name)
                    return true;
            }

            return false;
        }
    }
}
