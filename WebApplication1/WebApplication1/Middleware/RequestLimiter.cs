using System.Collections.Concurrent;

public class RequestLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly ConcurrentDictionary<string, RequestInfo> _requests = new();

    private readonly int _authLimit;
    private readonly int _anonLimit;
    private readonly TimeSpan _timeWindow;

    public RequestLimitingMiddleware(RequestDelegate next, int authLimit, int anonLimit, TimeSpan timeWindow)
    {
        _next = next;
        _authLimit = authLimit;
        _anonLimit = anonLimit;
        _timeWindow = timeWindow;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Use username for authenticated users, otherwise - IP address
        var key = context.User.Identity?.IsAuthenticated == true
            ? $"user:{context.User.Identity!.Name}"
            : $"ip:{context.Connection.RemoteIpAddress}";

        var now = DateTime.UtcNow;
        var requestInfo = _requests.GetOrAdd(key, _ => new RequestInfo(now, 0));

        lock (requestInfo)
        {
            if ((now - requestInfo.StartTime) > _timeWindow)
            {
                requestInfo.StartTime = now;
                requestInfo.Count = 0;
            }

            requestInfo.Count++;
        }

        var limit = context.User.Identity?.IsAuthenticated == true ? _authLimit : _anonLimit;

        if (requestInfo.Count > limit)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.Response.WriteAsync("Too many requests. Please try again later.");
            return;
        }

        await _next(context);
    }

    private class RequestInfo
    {
        public DateTime StartTime { get; set; }
        public int Count { get; set; }

        public RequestInfo(DateTime startTime, int count)
        {
            StartTime = startTime;
            Count = count;
        }
    }
}
