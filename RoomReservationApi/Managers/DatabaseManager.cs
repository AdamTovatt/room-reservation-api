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

        public async Task<Dictionary<string, RoomMetadata>> GetRoomMetadata()
        {
            try
            {
                string query = @"SELECT filedata FROM templatefile t WHERE t.filename = @filename";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    await connection.OpenAsync();

                    command.Parameters.Add("@filename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = fileName;

                    string commandResult = (string)await command.ExecuteScalarAsync();

                    return new RetrivaFile(fileName, commandResult);
                }
            }
            catch
            {
                throw new ApiException("Error when getting room metadata, the database might be unavailable", System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
