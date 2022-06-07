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

        public async Task<Dictionary<string, BuildingMetadata>> GetBuildingMetadata()
        {
            Dictionary<string, BuildingMetadata> result = new Dictionary<string, BuildingMetadata>();

            try
            {
                string query = @"SELECT ""Name"", ""PositionX"", ""PositionY"" FROM ""Building""";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            double? x = reader["PositionX"] as double?;
                            double? y = reader["PositionY"] as double?;

                            BuildingMetadata metadata = new BuildingMetadata();
                            if (x != null && y != null)
                                metadata.Position = new Vector2((double)x, (double)y);

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
