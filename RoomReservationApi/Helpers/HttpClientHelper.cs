using System.Net;
using System.Net.Http;
using System.Net.Security;

namespace RoomReservationApi.Helpers
{
    public static class HttpClientHelper
    {
        public static HttpClientHandler CreateHandler()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CheckCertificateRevocationList = false,
            };

            // For Linux systems (like Raspberry Pi), add custom certificate validation
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                // Log SSL errors for debugging
                if (sslPolicyErrors != SslPolicyErrors.None)
                {
                    System.Console.WriteLine($"SSL Policy Error: {sslPolicyErrors}");
                    if (chain != null)
                    {
                        System.Console.WriteLine($"Chain Status: {string.Join(", ", chain.ChainStatus.Select(s => s.Status))}");
                    }
                }

                // Accept all certificates (temporary for debugging - should be more restrictive in production)
                return true;
            };

            return handler;
        }

        public static HttpClient CreateClient()
        {
            return new HttpClient(CreateHandler());
        }
    }
}
