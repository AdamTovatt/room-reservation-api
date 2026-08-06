using EasyReasy.EnvironmentVariables;
using Microsoft.Extensions.Logging.Abstractions;
using RoomReservationApi.Helpers;
using RoomReservationApi.Models;

namespace RoomReservationApi.Tests
{
    public class ApiServiceIntegrationTests
    {
        protected static ApiService apiService;

        static ApiServiceIntegrationTests()
        {
            string filePath = Path.Combine("..", "..", "environment-variables.txt");
            EnvironmentVariableHelper.LoadVariablesFromFile(filePath);
            EnvironmentVariableHelper.ValidateVariableNamesIn(typeof(EnvironmentVariables));

            string apiKey = EnvironmentVariables.KthApiKey.GetValue();

            HttpClient httpClient = HttpClientHelper.CreateClient();
            // Zero lifetime, so every test in here reaches the KTH api. This service is shared by the whole class,
            // and a test that is quietly served a schedule another test already fetched proves nothing.
            ScheduleCache scheduleCache = new ScheduleCache(TimeSpan.Zero);
            apiService = new ApiService(httpClient, apiKey, scheduleCache, NullLogger<ApiService>.Instance);
        }

        [Fact]
        public async Task GetScheduleForToday_ShouldReturnReservations()
        {
            Schedule schedule = await apiService.GetScheduleAsync(0);

            Assert.NotNull(schedule);
            Assert.NotNull(schedule.Reservations);
            Assert.NotEmpty(schedule.Reservations);
        }
    }
}
