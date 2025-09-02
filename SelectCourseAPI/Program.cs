using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using SelectCourseAPI.Services;
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
// 自訂服務註冊
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
