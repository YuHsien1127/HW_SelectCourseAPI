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
// HttpContextAccessor �� ���\�b Service �����o�ثe�� HttpContext
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
    options.Filters.AddService<LoggingFilter>(); // ����M��
});
// �ۭq�A�ȵ��U
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

// JWT ����
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);
// Swagger �[�J JWT Bearer Token ����
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "��J�榡�GBearer �A�� JWT token",
        Name = "Authorization", //HTTP Header �W��
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    // �� Swagger UI �M������
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
// ���U JWT Bearer Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,           // ���ҵo���(Issuer)
        ValidateAudience = true,         // ���ұ�����(Audience)
        ValidateLifetime = true,         // ���� token �O�_�L��
        ValidateIssuerSigningKey = true, // ����ñ��

        // �]�w���Ī� Issuer / Audience / ���_
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
// �ҥ� JWT ���� / ���v
app.UseAuthentication();
app.UseAuthorization();
// Middleware
app.UseMiddleware<LoggingMiddleware>();
app.MapControllers();

app.Run();
