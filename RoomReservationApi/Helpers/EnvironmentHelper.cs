using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Helpers
{
    public static class EnvironmentHelper
    {
        public static string GetEnvironmentVariable(string environmentVariableName, int minLength = 0)
        {
            string result = Environment.GetEnvironmentVariable(environmentVariableName);
            if (result == null)
                throw new ApiException(string.Format("No {0} in environment variables", environmentVariableName), System.Net.HttpStatusCode.InternalServerError);
            if (result.Length < minLength)
                throw new ApiException(string.Format("Invalid {0} in environment variables", environmentVariableName), System.Net.HttpStatusCode.InternalServerError);
            return result;
        }
    }
}
