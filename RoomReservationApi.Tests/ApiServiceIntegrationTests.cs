using EasyReasy.EnvironmentVariables;
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
            apiService = new ApiService(httpClient, apiKey);
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
