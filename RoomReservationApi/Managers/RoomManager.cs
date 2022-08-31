using GeoCoordinatePortable;
using RoomReservationApi.Helpers;
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
        public bool AllowUnknownBuildings { get; set; }
        public Dictionary<string, Building> BuildingsDictionary { get; set; }
        public List<Building> Buildings { get { return BuildingsDictionary.Values.Where(x => !x.Rooms.All(r => r.Hide)).OrderByDescending(x => x.GetRelevance()).ToList().OrderBuildingRooms(); } }
        public GeoCoordinate Location { get; private set; }

        private Dictionary<string, string> roomBuildings = new Dictionary<string, string>();

        public RoomManager(GeoCoordinate location = null)
        {
            Location = location;
        }

        public async Task InitializeAsync(DatabaseManager database = null)
        {
            if (database == null)
                database = DatabaseManager.CreateFromEnvironmentVariables();

            BuildingsDictionary = await database.GetBuildingsAsync();

            foreach (string key in BuildingsDictionary.Keys)
            {
                BuildingsDictionary[key].UpdateDistance(Location);
            }

            foreach (Room room in await database.GetRoomsAsync())
            {
                AddRoom(room);
            }
        }

        private void AddRoom(Room room)
        {
            string buildingName = room.BuildingName;

            if (!BuildingsDictionary.ContainsKey(buildingName))
            {
                if (AllowUnknownBuildings)
                    BuildingsDictionary.Add(buildingName, new Building(buildingName)); //only add the building if we allow adding unknown buildings
                else
                    return;
            }

            BuildingsDictionary[buildingName].AddRoom(room);

            if (!roomBuildings.ContainsKey(room.Name))
                roomBuildings.Add(room.Name, buildingName);
        }

        public void ApplySchedule(Schedule schedule)
        {
            foreach (Reservation reservation in schedule.Reservations)
            {
                foreach (Location location in reservation.Locations)
                {
                    string buildingName;

                    if (roomBuildings.ContainsKey(location.Name))
                        buildingName = roomBuildings[location.Name];
                    else
                        buildingName = location.Name.ExtractBuildingName();

                    if (!BuildingsDictionary.ContainsKey(buildingName))
                    {
                        if (AllowUnknownBuildings)
                            BuildingsDictionary.Add(buildingName, new Building(buildingName)); //only add the building if we allow adding unknown buildings
                        else
                            continue;
                    }

                    Building building = BuildingsDictionary[buildingName];

                    Room room = building.Rooms.Where(x => x.Name == location.Name).FirstOrDefault();

                    Guid? externalId = null;
                    if (Guid.TryParse(location.Id, out Guid parsedId))
                        externalId = parsedId;

                    if (room != null)
                    {
                        if(room.ExternalId == null)
                            room.ExternalId = externalId;
                        
                        room.AddReservedTime(new ReservedTime(reservation));
                    }
                    else
                    {
                        room = new Room(location.Name, buildingName, externalId, false); //we really don't know if the room requires special access but we'll set it to false
                        room.AddReservedTime(new ReservedTime(reservation));

                        building.AddRoom(room);
                    }
                }
            }
        }
    }
}
