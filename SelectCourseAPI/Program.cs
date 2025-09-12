using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using SelectCourseAPI.Middleware_Filter;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using SelectCourseAPI.Services;
using System.Text;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

// NLog 初始化
// 載入 NLog 設定（從 appsettings.json / nlog.config）
// 建立 Logger 物件，方便在此 Program 中記錄錯誤或啟動訊息
LogManager.Setup().LoadConfigurationFromAppSettings();
var logger = LogManager.GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);

// Logging 設定
// 移除內建 Logging Provider（Console, Debug...）
// 改為使用 NLog 作為 Logging 提供者
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(MicrosoftLogLevel.Trace);
builder.Host.UseNLog();
// DbContext 註冊(SQL Server)
builder.Services.AddDbContext<SelectCourseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// HttpContextAccessor → 允許在 Service 中取得目前的 HttpContext
builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});
builder.Services.AddScoped<LoggingFilter>();
builder.Services.AddControllers(options =>
{
    options.Filters.AddService<LoggingFilter>(); // 全域套用
});
// 自訂服務註冊
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT 驗證
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);
// Swagger 加入 JWT Bearer Token 驗證
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "輸入格式：Bearer 你的 JWT token",
        Name = "Authorization", //HTTP Header 名稱
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    // 讓 Swagger UI 套用驗證
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
});
// 註冊 JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           // 驗證發行者(Issuer)
        ValidateAudience = true,         // 驗證接收者(Audience)
        ValidateLifetime = true,         // 驗證 token 是否過期
        ValidateIssuerSigningKey = true, // 驗證簽章

        // 設定有效的 Issuer / Audience / 金鑰
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// 啟用 JWT 驗證 / 授權
app.UseAuthentication();
app.UseAuthorization();
// Middleware
app.UseMiddleware<LoggingMiddleware>();
app.MapControllers();

app.Run();
