using Microsoft.AspNetCore.Http;
using RoomReservationApi.RateLimiting;
using System.Net;

namespace RoomReservationApi.Tests
{
    /// <summary>
    /// Covers which caller a request is counted against. Getting this wrong in either direction breaks the rate
    /// limiting: trust the forwarding headers too little and everyone behind the reverse proxy shares one bucket,
    /// trust them too much and a caller picks its own bucket and is never limited at all.
    /// </summary>
    public class RateLimitPartitionKeyTests
    {
        private const string proxyAddress = "192.168.1.103";
        private const string publicAddress = "203.0.113.7";
        private const string clientAddress = "198.51.100.42";

        [Theory]
        [InlineData("X-Forwarded-For")]
        [InlineData("CF-Connecting-IP")]
        public void Resolve_WhenTheProxyForwardsTheCaller_CountsTheCallerAndNotTheProxy(string headerName)
        {
            HttpContext httpContext = CreateHttpContext(connectionAddress: proxyAddress, headerName, clientAddress);

            Assert.Equal(clientAddress, RateLimitPartitionKey.Resolve(httpContext));
        }

        [Theory]
        [InlineData("X-Forwarded-For")]
        [InlineData("CF-Connecting-IP")]
        public void Resolve_WhenACallerFromOutsideSetsTheHeaderItself_CountsTheAddressItConnectedFrom(string headerName)
        {
            HttpContext httpContext = CreateHttpContext(connectionAddress: publicAddress, headerName, clientAddress);

            Assert.Equal(publicAddress, RateLimitPartitionKey.Resolve(httpContext));
        }

        [Fact]
        public void Resolve_WhenACallerFromOutsideVariesTheHeader_KeepsCountingTheSameCaller()
        {
            string firstKey = RateLimitPartitionKey.Resolve(CreateHttpContext(publicAddress, "X-Forwarded-For", "10.0.0.1"));
            string secondKey = RateLimitPartitionKey.Resolve(CreateHttpContext(publicAddress, "X-Forwarded-For", "10.0.0.2"));

            Assert.Equal(firstKey, secondKey);
        }

        [Fact]
        public void Resolve_WithNoForwardingHeader_CountsTheAddressItConnectedFrom()
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(publicAddress);

            Assert.Equal(publicAddress, RateLimitPartitionKey.Resolve(httpContext));
        }

        [Fact]
        public void Resolve_WithNoAddressAtAll_StillGivesAKey()
        {
            Assert.False(string.IsNullOrEmpty(RateLimitPartitionKey.Resolve(new DefaultHttpContext())));
        }

        [Theory]
        [InlineData("127.0.0.1", true)]
        [InlineData("10.1.2.3", true)]
        [InlineData("172.16.0.1", true)]
        [InlineData("172.31.255.254", true)]
        [InlineData("192.168.1.103", true)]
        [InlineData("::1", true)]
        [InlineData("172.15.0.1", false)] // just below the 172.16.0.0/12 block
        [InlineData("172.32.0.1", false)] // just above it
        [InlineData("192.169.0.1", false)]
        [InlineData("11.0.0.1", false)]
        [InlineData("203.0.113.7", false)]
        [InlineData("2001:4860:4860::8888", false)]
        public void IsLocalNetworkAddress_KnowsWhichAddressesAreInsideTheNetwork(string address, bool expected)
        {
            Assert.Equal(expected, RateLimitPartitionKey.IsLocalNetworkAddress(IPAddress.Parse(address)));
        }

        private static HttpContext CreateHttpContext(string connectionAddress, string headerName, string headerValue)
        {
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse(connectionAddress);
            httpContext.Request.Headers[headerName] = headerValue;

            return httpContext;
        }
    }
}
