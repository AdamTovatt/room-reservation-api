using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RoomReservationApi.Managers
{
    public class ViewManager
    {
        private static Dictionary<IPAddress, DateTime> views = new Dictionary<IPAddress, DateTime>();

        private DatabaseManager database;

        public ViewManager(DatabaseManager database)
        {
            this.database = database;
        }

        public async Task RegisterView(IPAddress ipAddress)
        {
            if(views.TryGetValue(ipAddress, out DateTime time))
            {
                if((DateTime.UtcNow - time).TotalSeconds < 60)
                {
                    return;
                }
                else
                {
                    views.Remove(ipAddress);
                }
            }

            await database.RegisterView(ipAddress);

            views.Add(ipAddress, DateTime.UtcNow);
        }
    }
}
