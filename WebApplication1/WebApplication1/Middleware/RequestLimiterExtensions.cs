public static class RequestLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLimiter(
        this IApplicationBuilder builder,
        int authLimit,
        int anonLimit,
        TimeSpan window)
    {
        return builder.UseMiddleware<RequestLimitingMiddleware>(authLimit, anonLimit, window);
    }
}