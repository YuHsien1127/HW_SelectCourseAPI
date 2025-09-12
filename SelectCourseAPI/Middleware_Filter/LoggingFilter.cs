using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;
using System.Security.Claims;

namespace SelectCourseAPI.Middleware_Filter
{
    public class LoggingFilter : IActionFilter
    {
        private readonly ILogger<LoggingFilter> _logger;
        private readonly Stopwatch _stopwatch;
        public LoggingFilter(ILogger<LoggingFilter> logger)
        {
            _logger = logger;
            _stopwatch = new Stopwatch();
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _stopwatch.Start();
            var email = context.HttpContext.User.Identity?.IsAuthenticated == true
                ? context.HttpContext.User?.FindFirst(ClaimTypes.Email)?.Value
                ?? "Unknown" : "Anonymous";
            var role = string.Join(",", context.HttpContext.User.Claims // 取得所有 Claims
                .Where(r => r.Type == ClaimTypes.Role)       // 過濾出 Type 為 Role 的 Claim
                .Select(r => r.Value));                      // 取出每個 Claim 的值 (角色名稱)

            _logger.LogInformation("【Filter】Request：{Path} | From：{ip} | Email：{email} | Role：{role}",
                context.HttpContext.Request.Path,
                context.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString(),
                email, role);
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();
            // 取得響應狀態碼
            var statusCode = context.HttpContext.Response.StatusCode;
            // 若 Action 發生異常，記錄異常狀態碼
            if (context.Exception != null)
                _logger.LogError(context.Exception, "【Filter】Action 執行異常：{Path}", context.HttpContext.Request.Path);

            _logger.LogInformation("【Middleware】Response：{StatusCode} | Time：{Elapsed}",
                statusCode, _stopwatch.ElapsedMilliseconds);
        }
    }
}
