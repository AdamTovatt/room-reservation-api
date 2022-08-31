using GeoCoordinatePortable;
using Npgsql;
using RoomReservationApi.Helpers;
using RoomReservationApi.Models;
using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoomReservationApi.Managers
{
    public class DatabaseManager
    {
        private string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public static DatabaseManager CreateFromEnvironmentVariables(string variableName = "DATABASE_URL")
        {
            string databaseUrl = EnvironmentHelper.GetEnvironmentVariable(variableName);

            return new DatabaseManager(ConnectionStringHelper.GetConnectionStringFromUrl(databaseUrl));
        }

        public async Task<Dictionary<string, Building>> GetBuildingsAsync()
        {
            Dictionary<string, Building> result = new Dictionary<string, Building>();

            try
            {
                string query = @"SELECT ""Name"", ""Latitude"", ""Longitude"" FROM ""Building""";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            BuildingMetadata metadata = new BuildingMetadata()
                            {
                                Position = new GeoCoordinate((double)reader["Latitude"], (double)reader["Longitude"])
                            };

                            Building building = new Building(reader["Name"] as string);
                            building.Metadata = metadata;

                            result.Add(building.Name, building);
                        }
                    }
                }
            }
            catch
            {
                throw new ApiException("Error when getting buildings, the database might be unavailable", System.Net.HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public async Task<List<Room>> GetRoomsAsync()
        {
            List<Room> result = new List<Room>();

            try
            {
                string query = @"SELECT r.""Name"" ""RoomName"", b.""Name"" ""BuildingName"", r.""Hide"", r.""ExternalId"", r.""RequiresAccess"" FROM ""Room"" r JOIN ""Building"" b ON r.""BuildingId"" = b.""Id""";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new Room(reader["RoomName"] as string, reader["BuildingName"] as string, reader["ExternalId"] as Guid?, (bool)reader["RequiresAccess"]) { Hide = (bool)reader["Hide"] });
                        }
                    }
                }
            }
            catch
            {
                throw new ApiException("Error when getting rooms, the database might be unavailable", System.Net.HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public async Task<bool> UpdateDatabaseRooms(List<Room> rooms)
        {
            bool result = true;

            List<Room> databaseRooms = await GetRoomsAsync();
            rooms = rooms.Where(x => databaseRooms.ContainsRoom(x)).ToList();

            foreach (Room room in rooms)
            {
                if (room.ExternalId == null)
                    continue;

                try
                {
                    string query = @"UPDATE ""Room"" SET ""ExternalId"" = @ExternalId WHERE ""Name"" = @Name";

                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        await connection.OpenAsync();

                        command.Parameters.Add("@ExternalId", NpgsqlTypes.NpgsqlDbType.Uuid).Value = (Guid)room.ExternalId;
                        command.Parameters.Add("@Name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = room.Name;

                        result = result && (await command.ExecuteNonQueryAsync() == 1);
                    }
                }
                catch
                {
                    throw new ApiException("Error when updating rooms, the database might be unavailable", System.Net.HttpStatusCode.InternalServerError);
                }
            }

            return result;
        }

        public async Task<Dictionary<string, BuildingMetadata>> GetBuildingMetadata()
        {
            Dictionary<string, BuildingMetadata> result = new Dictionary<string, BuildingMetadata>();

            try
            {
                string query = @"SELECT ""Id"", ""Name"", ""Latitude"", ""Longitude"" FROM ""Building""";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {

                            BuildingMetadata metadata = new BuildingMetadata()
                            {
                                Position = new GeoCoordinate((double)reader["Latitude"], (double)reader["Longitude"])
                            };

                            result.Add(reader["Name"] as string, metadata);
                        }
                    }
                }
            }
            catch
            {
                throw new ApiException("Error when getting room metadata, the database might be unavailable", System.Net.HttpStatusCode.InternalServerError);
            }

            return result;
        }

        public async Task RegisterView(IPAddress ip)
        {
            try
            {
                string query = @"INSERT INTO ""PageView"" (""Ip"") VALUES (@ip)";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    command.Parameters.Add("@ip", NpgsqlTypes.NpgsqlDbType.Inet).Value = ip;

                    if((await command.ExecuteNonQueryAsync()) == 0)
                    {
                        throw new ApiException("Write view to database failed", System.Net.HttpStatusCode.InternalServerError);
                    }
                }
            }
            catch (Exception exception)
            {
                throw new ApiException("Error when getting room metadata, the database might be unavailable", System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
