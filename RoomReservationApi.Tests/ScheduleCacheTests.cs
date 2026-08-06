using RoomReservationApi.Helpers;
using RoomReservationApi.Models;

namespace RoomReservationApi.Tests
{
    public class ScheduleCacheTests
    {
        private static readonly TimeSpan lifetime = TimeSpan.FromMinutes(1);
        private static readonly DateTime day = new DateTime(2026, 8, 6);

        [Fact]
        public async Task GetOrFetchAsync_SameDayWithinLifetime_FetchesOnce()
        {
            FetchCounter fetches = new FetchCounter();
            DateTimeOffset currentTime = DateTimeOffset.UnixEpoch;
            ScheduleCache cache = new ScheduleCache(lifetime, () => currentTime);

            await cache.GetOrFetchAsync(day, fetches.FetchAsync);
            currentTime += lifetime - TimeSpan.FromSeconds(1);
            await cache.GetOrFetchAsync(day, fetches.FetchAsync);

            Assert.Equal(1, fetches.Count);
        }

        [Fact]
        public async Task GetOrFetchAsync_AfterLifetime_FetchesAgain()
        {
            FetchCounter fetches = new FetchCounter();
            DateTimeOffset currentTime = DateTimeOffset.UnixEpoch;
            ScheduleCache cache = new ScheduleCache(lifetime, () => currentTime);

            await cache.GetOrFetchAsync(day, fetches.FetchAsync);
            currentTime += lifetime;
            await cache.GetOrFetchAsync(day, fetches.FetchAsync);

            Assert.Equal(2, fetches.Count);
        }

        /// <summary>
        /// A zero lifetime is how the integration tests switch caching off, so it has to actually switch it off
        /// even though no time passes between the two calls.
        /// </summary>
        [Fact]
        public async Task GetOrFetchAsync_WithZeroLifetime_FetchesEveryTime()
        {
            FetchCounter fetches = new FetchCounter();
            ScheduleCache cache = new ScheduleCache(TimeSpan.Zero, () => DateTimeOffset.UnixEpoch);

            await cache.GetOrFetchAsync(day, fetches.FetchAsync);
            await cache.GetOrFetchAsync(day, fetches.FetchAsync);

            Assert.Equal(2, fetches.Count);
        }

        [Fact]
        public async Task GetOrFetchAsync_DifferentDays_FetchesOncePerDay()
        {
            FetchCounter fetches = new FetchCounter();
            ScheduleCache cache = new ScheduleCache(lifetime, () => DateTimeOffset.UnixEpoch);

            await cache.GetOrFetchAsync(day, fetches.FetchAsync);
            await cache.GetOrFetchAsync(day.AddDays(1), fetches.FetchAsync);
            await cache.GetOrFetchAsync(day, fetches.FetchAsync);

            Assert.Equal(2, fetches.Count);
        }

        [Fact]
        public async Task GetOrFetchAsync_WhileFetchIsStillRunning_DoesNotStartASecondFetch()
        {
            FetchCounter fetches = new FetchCounter();
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            DateTimeOffset currentTime = DateTimeOffset.UnixEpoch;
            ScheduleCache cache = new ScheduleCache(lifetime, () => currentTime);

            Task<Schedule> firstCall = cache.GetOrFetchAsync(day, () => fetches.FetchAfterAsync(release.Task));

            // the fetch is still running, and it has now been running for longer than the entry lifetime
            currentTime += lifetime * 10;
            Task<Schedule> secondCall = cache.GetOrFetchAsync(day, fetches.FetchAsync);

            release.SetResult();
            await Task.WhenAll(firstCall, secondCall);

            Assert.Equal(1, fetches.Count);
            Assert.Same(await firstCall, await secondCall);
        }

        [Fact]
        public async Task GetOrFetchAsync_WhenFetchFails_DoesNotCacheTheFailure()
        {
            FetchCounter fetches = new FetchCounter();
            ScheduleCache cache = new ScheduleCache(lifetime, () => DateTimeOffset.UnixEpoch);

            await Assert.ThrowsAsync<InvalidOperationException>(() => cache.GetOrFetchAsync(day, fetches.FailAsync));

            // the clock has not moved, so a cached failure would still be inside the entry lifetime here
            Schedule schedule = await cache.GetOrFetchAsync(day, fetches.FetchAsync);

            Assert.Equal(2, fetches.Count);
            Assert.NotNull(schedule);
        }

        [Fact]
        public async Task GetOrFetchAsync_WhenFetchFails_PropagatesTheFailureToEveryWaitingCaller()
        {
            FetchCounter fetches = new FetchCounter();
            TaskCompletionSource release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            ScheduleCache cache = new ScheduleCache(lifetime, () => DateTimeOffset.UnixEpoch);

            Task<Schedule> firstCall = cache.GetOrFetchAsync(day, () => fetches.FailAfterAsync(release.Task));
            Task<Schedule> secondCall = cache.GetOrFetchAsync(day, fetches.FetchAsync);

            release.SetResult();

            await Assert.ThrowsAsync<InvalidOperationException>(() => firstCall);
            await Assert.ThrowsAsync<InvalidOperationException>(() => secondCall);
            Assert.Equal(1, fetches.Count);
        }

        /// <summary>
        /// Stands in for the outbound call to the KTH api and counts how many times it was made.
        /// </summary>
        private class FetchCounter
        {
            private int count;

            public int Count { get { return count; } }

            public Task<Schedule> FetchAsync()
            {
                Interlocked.Increment(ref count);

                return Task.FromResult(new Schedule());
            }

            public async Task<Schedule> FetchAfterAsync(Task release)
            {
                Interlocked.Increment(ref count);
                await release;

                return new Schedule();
            }

            public Task<Schedule> FailAsync()
            {
                Interlocked.Increment(ref count);

                throw new InvalidOperationException("the KTH api is unhappy");
            }

            public async Task<Schedule> FailAfterAsync(Task release)
            {
                Interlocked.Increment(ref count);
                await release;

                throw new InvalidOperationException("the KTH api is unhappy");
            }
        }
    }
}
