using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RoomReservationApi.Helpers;
using RoomReservationApi.Managers;
using RoomReservationApi.Models;
using Sakur.WebApiUtilities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Schedule schedule = await ApiHelper.GetScheduleAsync(dayOffset);

            RoomManager roomManager = new RoomManager(schedule);
            roomManager.ApplySchedule(schedule);

            return new ApiResponse(roomManager.GetBuildings());
        }

        [HttpGet("searchStaff")]
        public async Task<ActionResult> SearchStaff(string name, int dayRange)
        {
            Room searchResult = null;

            for (int i = 0; i < dayRange; i++)
            {
                Schedule schedule = await ApiHelper.GetScheduleAsync(i);

                RoomManager roomManager = new RoomManager(schedule);
                roomManager.ApplySchedule(schedule);

                searchResult = roomManager.SearchStaff(name);
                if (searchResult != null)
                    break;
            }

            return new ApiResponse(searchResult);
        }
    }
}
