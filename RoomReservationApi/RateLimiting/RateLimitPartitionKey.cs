using Microsoft.AspNetCore.Http;
using RoomReservationApi.Helpers;
using System.Net;
using System.Net.Sockets;

namespace RoomReservationApi.RateLimiting
{
    /// <summary>
    /// The single definition of "who is the caller" for rate limiting, shared by the global limiter and every
    /// named policy so all tiers count the same caller the same way. The api is anonymous, so a caller is only
    /// identifiable by ip address.
    /// </summary>
    public static class RateLimitPartitionKey
    {
        /// <summary>
        /// Identifies the caller, preferring the forwarding headers over the address the connection came from,
        /// but only when the connection came from the local network.
        /// </summary>
        /// <remarks>
        /// Both halves of that are needed. The api is served through a reverse proxy on another machine, so
        /// partitioning on the connection address alone would put every caller behind that proxy into one bucket,
        /// where a single caller could use up the limit for everyone. But the forwarding headers are set by
        /// whoever sends the request, so trusting them unconditionally would let a caller that reaches this api
        /// without passing the proxy pick a fresh bucket per request and ignore every limit. Only a caller that
        /// is already inside the network can do that here, and the connection address is the one thing such a
        /// caller cannot choose.
        /// </remarks>
        public static string Resolve(HttpContext httpContext)
        {
            IPAddress? connectionAddress = httpContext.Connection.RemoteIpAddress;
            IPAddress? address = httpContext.GetRemoteIPAddress(allowForwarded: IsLocalNetworkAddress(connectionAddress));

            return address?.ToString() ?? "unknown";
        }

        /// <summary>
        /// Whether the address is one that only something inside this network can be connecting from, which is
        /// where the reverse proxy is.
        /// </summary>
        public static bool IsLocalNetworkAddress(IPAddress? address)
        {
            if (address == null)
                return false;

            if (IPAddress.IsLoopback(address))
                return true;

            if (address.IsIPv6LinkLocal || address.IsIPv6UniqueLocal)
                return true;

            if (address.IsIPv4MappedToIPv6)
                address = address.MapToIPv4();

            if (address.AddressFamily != AddressFamily.InterNetwork)
                return false;

            byte[] bytes = address.GetAddressBytes();

            if (bytes[0] == 10) // 10.0.0.0/8
                return true;

            if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) // 172.16.0.0/12
                return true;

            if (bytes[0] == 192 && bytes[1] == 168) // 192.168.0.0/16
                return true;

            if (bytes[0] == 169 && bytes[1] == 254) // 169.254.0.0/16, link local
                return true;

            return false;
        }
    }
}
