using Microsoft.Extensions.Logging.Abstractions;
using RoomReservationApi.Helpers;
using Sakur.WebApiUtilities.Models;
using System.Net;

namespace RoomReservationApi.Tests
{
    /// <summary>
    /// Covers what a caller is told when the KTH api call fails. The response used to carry the whole exception,
    /// including the source file and line of every frame, which is not something a public api should hand out.
    /// </summary>
    public class ApiServiceFailureTests
    {
        [Fact]
        public async Task GetScheduleAsync_WhenTheKthApiRejectsTheRequest_TellsTheCallerNothingAboutTheInside()
        {
            ApiService apiService = CreateApiService(new StubHandler(HttpStatusCode.Unauthorized));

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => apiService.GetScheduleAsync(0));

            // What the response used to carry: the upstream status, and a stack trace naming this project's types
            AssertCarriesNothingButTheGenericMessage(exception, "401", "Unauthorized", "RoomReservationApi.Helpers.ApiService");
        }

        [Fact]
        public async Task GetScheduleAsync_WhenTheKthApiCannotBeReached_TellsTheCallerNothingAboutTheInside()
        {
            ApiService apiService = CreateApiService(new StubHandler(new HttpRequestException("no route to host")));

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => apiService.GetScheduleAsync(0));

            AssertCarriesNothingButTheGenericMessage(exception, "no route to host", "RoomReservationApi.Helpers.ApiService");
        }

        [Fact]
        public async Task GetScheduleAsync_WhenTheKthApiReturnsNonsense_TellsTheCallerNothingAboutTheInside()
        {
            ApiService apiService = CreateApiService(new StubHandler(HttpStatusCode.OK, "this is not json"));

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => apiService.GetScheduleAsync(0));

            // The parser's own complaint, which quotes the position it gave up at
            AssertCarriesNothingButTheGenericMessage(exception, "Unexpected character");
        }

        [Theory]
        [InlineData(-8)]
        [InlineData(31)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public async Task GetScheduleAsync_WhenTheDayOffsetIsOutsideTheWindow_IsRejectedWithoutCallingTheKthApi(int dayOffset)
        {
            StubHandler handler = new StubHandler(HttpStatusCode.OK);
            ApiService apiService = CreateApiService(handler);

            ApiException exception = await Assert.ThrowsAsync<ApiException>(() => apiService.GetScheduleAsync(dayOffset));

            Assert.Equal(HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.Equal(0, handler.RequestCount);
        }

        [Theory]
        [InlineData(ApiService.MinimumDayOffset)]
        [InlineData(0)]
        [InlineData(ApiService.MaximumDayOffset)]
        public async Task GetScheduleAsync_WhenTheDayOffsetIsInsideTheWindow_CallsTheKthApi(int dayOffset)
        {
            StubHandler handler = new StubHandler(HttpStatusCode.OK);
            ApiService apiService = CreateApiService(handler);

            await apiService.GetScheduleAsync(dayOffset);

            Assert.Equal(1, handler.RequestCount);
        }

        /// <summary>
        /// Asserts that the caller is told nothing about the inside. The forbidden strings are checked before the
        /// message is compared to the constant, so these tests fail on what actually leaked rather than only on
        /// the message no longer matching a constant that this project owns and could change.
        /// </summary>
        private static void AssertCarriesNothingButTheGenericMessage(ApiException exception, params string[] forbidden)
        {
            string carried = $"{exception.ErrorMessage} {exception.ErrorObject}";

            foreach (string leak in forbidden)
                Assert.DoesNotContain(leak, carried, StringComparison.Ordinal);

            Assert.Equal(HttpStatusCode.InternalServerError, exception.StatusCode);
            Assert.Null(exception.ErrorObject);
            Assert.Equal(ApiService.ScheduleFetchFailedMessage, exception.ErrorMessage);
        }

        private static ApiService CreateApiService(StubHandler handler)
        {
            HttpClient httpClient = new HttpClient(handler);
            ScheduleCache scheduleCache = new ScheduleCache(TimeSpan.FromMinutes(1));

            return new ApiService(httpClient, "an-api-key", scheduleCache, NullLogger<ApiService>.Instance);
        }

        /// <summary>
        /// Stands in for the KTH api, either answering with a given response or failing outright.
        /// </summary>
        private class StubHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode statusCode;
            private readonly string content;
            private readonly Exception? failure;
            private int requestCount;

            /// <summary>
            /// How many requests reached the KTH api.
            /// </summary>
            public int RequestCount { get { return requestCount; } }

            public StubHandler(HttpStatusCode statusCode, string content = "[]")
            {
                this.statusCode = statusCode;
                this.content = content;
            }

            public StubHandler(Exception failure)
            {
                this.failure = failure;
                statusCode = HttpStatusCode.OK;
                content = string.Empty;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref requestCount);

                if (failure != null)
                    throw failure;

                return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(content) });
            }
        }
    }
}
