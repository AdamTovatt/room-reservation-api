using GeoCoordinatePortable;
using Npgsql;
using RoomReservationApi.Helpers;
using RoomReservationApi.Models;
using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                string query = @"SELECT r.""Name"" ""RoomName"", b.""Name"" ""BuildingName"" FROM ""Room"" r JOIN ""Building"" b ON r.""BuildingId"" = b.""Id""";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new Room(reader["RoomName"] as string, reader["BuildingName"] as string));
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
    }
}
