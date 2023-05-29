using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RoomReservationApi.Helpers;
using RoomReservationApi.Managers;
using RoomReservationApi.Models;
using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RoomReservationApi.Controllers
{
    [ApiController]
    [Route("schedule")]
    public class ScheduleController : Controller
    {
        [HttpGet("get")]
        public async Task<ActionResult> GetSchedule(int dayOffset)
        {
            try
            {
                Schedule schedule = await ApiHelper.GetScheduleAsync(dayOffset);

                DatabaseManager database = DatabaseManager.CreateFromEnvironmentVariables();

                RoomManager roomManager = new RoomManager();
                await roomManager.InitializeAsync(database);
                roomManager.ApplySchedule(schedule);

                return new ApiResponse(new { dayOffset, buildings = roomManager.Buildings });
            }
            catch (ApiException exception)
            {
                Console.WriteLine(exception);
                Console.WriteLine(exception.ErrorMessage ?? "");
                Console.WriteLine(exception.ErrorObject ?? "");
                throw;
            }
        }

        [HttpPost("updateRoomIds")]
        public async Task<ActionResult> UpdateRoomIds()
        {
            try
            {
                Schedule schedule = await ApiHelper.GetScheduleAsync(0);

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
