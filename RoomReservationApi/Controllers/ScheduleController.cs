using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RoomReservationApi.Helpers;
using RoomReservationApi.Managers;
using RoomReservationApi.Models;
using RoomReservationApi.RateLimiting;
using Sakur.WebApiUtilities.Models;
using System.Threading.Tasks;

namespace RoomReservationApi.Controllers
{
    [ApiController]
    [Route("schedule")]
    public class ScheduleController : Controller
    {
        private readonly ApiService apiService;

        public ScheduleController(ApiService apiService)
        {
            this.apiService = apiService;
        }

        [HttpGet("get")]
        [EnableRateLimiting(RateLimitPolicies.Default)]
        public async Task<ActionResult> GetSchedule(int dayOffset)
        {
            try
            {
                Schedule schedule = await apiService.GetScheduleAsync(dayOffset);

                DatabaseManager database = DatabaseManager.CreateFromEnvironmentVariables();

                RoomManager roomManager = new RoomManager();
                await roomManager.InitializeAsync(database);
                roomManager.ApplySchedule(schedule);

                return new ApiResponse(new { dayOffset, buildings = roomManager.Buildings });
            }
            catch (ApiException exception)
            {
                return new ApiResponse(exception);
            }
        }

        [HttpPost("updateRoomIds")]
        [EnableRateLimiting(RateLimitPolicies.VeryStrict)]
        public async Task<ActionResult> UpdateRoomIds()
        {
            try
            {
                Schedule schedule = await apiService.GetScheduleAsync(0);

                DatabaseManager database = DatabaseManager.CreateFromEnvironmentVariables();

                RoomManager roomManager = new RoomManager();
                await roomManager.InitializeAsync(database);
                roomManager.ApplySchedule(schedule);

                bool updateResult = await database.UpdateDatabaseRooms(roomManager.Buildings.GetRooms());

                return new ApiResponse(new { updateResult });
            }
            catch (ApiException exception)
            {
                return new ApiResponse(exception);
            }
        }
    }
}
