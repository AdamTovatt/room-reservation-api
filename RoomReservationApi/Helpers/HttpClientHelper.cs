using System.Net;
using System.Net.Http;

namespace RoomReservationApi.Helpers
{
    public static class HttpClientHelper
    {
        public static HttpClientHandler CreateHandler()
        {
            return new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
        }

        public static HttpClient CreateClient()
        {
            return new HttpClient(CreateHandler());
        }
    }
}
