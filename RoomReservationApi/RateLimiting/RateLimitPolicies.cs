namespace RoomReservationApi.RateLimiting
{
    /// <summary>
    /// Named rate-limit policy identifiers. Routes opt into a stricter tier via
    /// <c>[EnableRateLimiting(RateLimitPolicies.Strict)]</c>; unattributed routes are covered by
    /// the global per-caller limiter only.
    /// </summary>
    /// <remarks>
    /// There is no tier looser than <see cref="Default"/>, because there cannot be one: every request also has
    /// to pass the global per-caller limiter, so a policy with a higher limit than that is silently clamped to
    /// it and would only read as permission it does not grant. A looser tier means raising the global limiter.
    /// </remarks>
    public static class RateLimitPolicies
    {
        public const string Default = "default";
        public const string Strict = "strict";
        public const string VeryStrict = "very-strict";
    }
}
