using RoomReservationApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoomReservationApi.Helpers
{
    /// <summary>
    /// Holds one schedule per day in memory for a limited time so that repeated requests for the same day are
    /// served without a new outbound request to the KTH api. A request for a day that is already being fetched
    /// waits for that fetch instead of starting a second one, so the number of outbound requests is bounded by
    /// the number of distinct days that can be asked for, not by the number of callers.
    /// </summary>
    public class ScheduleCache
    {
        private readonly TimeSpan entryLifetime;
        private readonly Func<DateTimeOffset> getCurrentTime;
        private readonly Dictionary<DateTime, CacheEntry> entriesByDay = new Dictionary<DateTime, CacheEntry>();
        private readonly object entriesLock = new object();

        /// <summary>
        /// Creates a cache where an entry is kept for the given lifetime.
        /// </summary>
        /// <param name="entryLifetime">How long a fetched schedule is served before it is fetched again.</param>
        /// <param name="getCurrentTime">Source of the current time, for tests. Defaults to the system clock.</param>
        public ScheduleCache(TimeSpan entryLifetime, Func<DateTimeOffset>? getCurrentTime = null)
        {
            this.entryLifetime = entryLifetime;
            this.getCurrentTime = getCurrentTime ?? (() => DateTimeOffset.UtcNow);
        }

        /// <summary>
        /// Gets the cached schedule for the given day, fetching it if it is missing or too old.
        /// </summary>
        /// <param name="day">The day the schedule is for. This is the cache key.</param>
        /// <param name="fetchAsync">Fetches the schedule. Only called when there is nothing usable in the cache.</param>
        public Task<Schedule> GetOrFetchAsync(DateTime day, Func<Task<Schedule>> fetchAsync)
        {
            CacheEntry entry;

            lock (entriesLock)
            {
                RemoveExpiredEntries();

                if (entriesByDay.TryGetValue(day, out CacheEntry? existingEntry))
                    return existingEntry.ScheduleTask;

                entry = new CacheEntry(getCurrentTime());
                entriesByDay[day] = entry;
            }

            _ = FillAsync(day, entry, fetchAsync); // the result of the fetch is observed through the entry, not through this task

            return entry.ScheduleTask;
        }

        private async Task FillAsync(DateTime day, CacheEntry entry, Func<Task<Schedule>> fetchAsync)
        {
            try
            {
                Schedule schedule = await fetchAsync();

                entry.SetSchedule(schedule);
            }
            catch (Exception exception)
            {
                Remove(day, entry); // a failed fetch is not kept, the next caller is allowed to try again immediately

                entry.SetException(exception);
            }
        }

        private void Remove(DateTime day, CacheEntry entry)
        {
            lock (entriesLock)
            {
                if (entriesByDay.TryGetValue(day, out CacheEntry? existingEntry) && existingEntry == entry)
                    entriesByDay.Remove(day); // only remove our own entry, a newer one for the same day is still valid
            }
        }

        private void RemoveExpiredEntries()
        {
            DateTimeOffset currentTime = getCurrentTime();

            List<DateTime> expiredDays = entriesByDay
                .Where(entry => entry.Value.IsExpired(currentTime, entryLifetime))
                .Select(entry => entry.Key)
                .ToList();

            foreach (DateTime expiredDay in expiredDays)
                entriesByDay.Remove(expiredDay);
        }

        private class CacheEntry
        {
            private readonly TaskCompletionSource<Schedule> completionSource;
            private readonly DateTimeOffset createdAt;

            public Task<Schedule> ScheduleTask { get { return completionSource.Task; } }

            public CacheEntry(DateTimeOffset createdAt)
            {
                this.createdAt = createdAt;
                completionSource = new TaskCompletionSource<Schedule>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public void SetSchedule(Schedule schedule)
            {
                completionSource.TrySetResult(schedule);
            }

            public void SetException(Exception exception)
            {
                completionSource.TrySetException(exception);
            }

            /// <summary>
            /// Whether this entry should be replaced. An entry that is still being fetched never counts as expired,
            /// otherwise a slow fetch would let a second fetch for the same day start alongside it.
            /// </summary>
            public bool IsExpired(DateTimeOffset currentTime, TimeSpan lifetime)
            {
                return completionSource.Task.IsCompleted && currentTime - createdAt >= lifetime;
            }
        }
    }
}
