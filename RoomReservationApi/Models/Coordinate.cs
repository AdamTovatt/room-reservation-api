using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Models
{
    public class Coordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public Coordinate(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1})", Latitude, Longitude);
        }

        public double GeographicalDistanceTo(Coordinate otherPosition) //magic math code
        {
            double d1 = Latitude * (Math.PI / 180.0);
            double num1 = Longitude * (Math.PI / 180.0);
            double d2 = otherPosition.Latitude * (Math.PI / 180.0);
            double num2 = otherPosition.Longitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);
            return 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
        }
    }
}
