using Npgsql;
using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Helpers
{
    public class ConnectionStringHelper
    {
        public static string GetConnectionStringFromUrl(string url)
        {
            if (url == null)
                throw new ApiException("Missing database url", System.Net.HttpStatusCode.InternalServerError);
            try
            {
                Uri databaseUri = new Uri(url);
                string[] userInfo = databaseUri.UserInfo.Split(':');

                NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder
                {
                    Host = databaseUri.Host,
                    Port = databaseUri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = databaseUri.LocalPath.TrimStart('/'),
                    SslMode = SslMode.Prefer,
                    TrustServerCertificate = true
                };

                return builder.ToString();
            }
            catch
            {
                throw new ApiException(new { message = "An error occured when the database url was being parsed, url length: ", urlLength = url.Length }, System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
