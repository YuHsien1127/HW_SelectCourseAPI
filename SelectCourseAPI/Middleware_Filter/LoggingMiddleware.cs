using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SelectCourseAPI.Middleware_Filter
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {

            var stopwatch = System.Diagnostics.Stopwatch.StartNew(); // 開始計時

            var email = context.User.Identity?.IsAuthenticated == true
                ? context.User?.FindFirst(ClaimTypes.Email)?.Value
                ?? "Unknown" : "Anonymous";
            var role = string.Join(",", context.User.Claims // 取得所有 Claims
                .Where(r => r.Type == ClaimTypes.Role)       // 過濾出 Type 為 Role 的 Claim
                .Select(r => r.Value));                      // 取出每個 Claim 的值 (角色名稱)

            _logger.LogInformation("【Middleware】Request：{Path} | From：{ip} | Email：{email} | Role：{role}",
                context.Request.Path,
                context.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
                email, role);

            await _next(context);

            stopwatch.Stop(); // 停止計時
            _logger.LogInformation("【Middleware】Response：{StatusCode} | Time：{Elapsed}",
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
