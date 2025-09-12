using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SelectCourseAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateJwtToken(string email, string role)
        {
            // 從 appsettings.json 取得 JWT 設定節點
            var jwtSettings = _configuration.GetSection("JwtSettings");
            // 從 appsettings.json 讀取 SecretKey，並轉成對稱加密金鑰
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));
            // 使用金鑰建立簽章憑證，指定使用 HMAC SHA256 演算法
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            // 建立 JWT 的 Claims（聲明）
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            // 建立 JWT Token
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"], // 發行者
                audience: jwtSettings["Audience"], // 接收者
                claims: claims, // 包含的 claims
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])), // 過期時間
                signingCredentials: credentials // 使用憑證簽名
            );
            /*
             * 「取得過期分鐘數，將 UTC 時間加上過期分鐘數，轉成本地時間」
             * DateTime.UtcNow_取得現在的世界標準時間（UTC），不是台灣本地時間
             * .AddMinutes(...)_在現在的 UTC 時間加上指定分鐘數
             * Convert.ToDouble(...)_將字串形式的數字（例如 "60"）轉成 double 型別（方便加分鐘）
             * .ToLocalTime()_將 UTC 時間轉換為電腦本地時間（台灣預設為 UTC+8）
             */
            _logger.LogInformation("【Info】產生JWT Token成功，UserId：{userId}，到期時間：{Expire}", email, DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])).ToLocalTime());

            // 將 JWT Token 實體轉為字串格式（Base64 編碼）
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
