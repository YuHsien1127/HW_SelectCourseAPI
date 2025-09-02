using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;
using SelectCourseAPI.Models;
using SelectCourseAPI.Repositorys;
using SelectCourseAPI.Services;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

// NLog ��l��
// ���J NLog �]�w�]�q appsettings.json / nlog.config�^
// �إ� Logger ����A��K�b�� Program ���O�����~�αҰʰT��
LogManager.Setup().LoadConfigurationFromAppSettings();
var logger = LogManager.GetCurrentClassLogger();

var builder = WebApplication.CreateBuilder(args);

// Logging �]�w
// �������� Logging Provider�]Console, Debug...�^
// �אּ�ϥ� NLog �@�� Logging ���Ѫ�
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(MicrosoftLogLevel.Trace);
builder.Host.UseNLog();
// DbContext ���U(SQL Server)
builder.Services.AddDbContext<SelectCourseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// �ۭq�A�ȵ��U
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
