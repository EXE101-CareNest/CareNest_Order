using CareNest_Order.Application.Common;
using CareNest_Order.Application.Features.Commands.Create;
using CareNest_Order.Application.Features.Commands.Delete;
using CareNest_Order.Application.Features.Commands.Update;
using CareNest_Order.Application.Features.Queries.GetAllPaging;
using CareNest_Order.Application.Features.Queries.GetById;
using CareNest_Order.Application.Features.Queries.Dashboard;
using CareNest_Order.Application.Interfaces.CQRS;
using CareNest_Order.Application.Interfaces.CQRS.Commands;
using CareNest_Order.Application.Interfaces.CQRS.Queries;
using CareNest_Order.Application.Interfaces.Services;
using CareNest_Order.Application.Common.Options;
using CareNest_Order.Application.Interfaces.UOW;
using CareNest_Order.Application.UseCases;
using CareNest_Order.Domain.Entitites;
using CareNest_Order.Domain.Repositories;
using CareNest_Order.Infrastructure.Persistences.Configuration;
using CareNest_Order.Infrastructure.Persistences.Database;
using CareNest_Order.Infrastructure.Persistences.Repository;
using CareNest_Order.Infrastructure.Services;
using CareNest_Order.Infrastructure.UOW;
using CareNest_Order.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Lấy DatabaseSettings từ configuration
DatabaseSettings dbSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>()!;
dbSettings.Display();
string connectionString = dbSettings?.GetConnectionString()
                        ?? "Host=localhost;Port=5432;Database=order-dev;Username=exe-carenest-dev;Password=nghi123";


// Đăng ký DbContext với PostgreSQL
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));

builder.Services.AddTransient<DatabaseSeeder>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Đăng ký service thêm chú thích cho api
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    //ADD JWT BEARER SECURITY DEFINITION
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token theo định dạng: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        //Type = SecuritySchemeType.ApiKey,
        Type = SecuritySchemeType.Http,//ko cần thêm token phía trước
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                In = ParameterLocation.Header,
                Name = "Bearer",
                Scheme = "Bearer"
            },
            new List<string>()
        }
    });
});

// Đăng ký các repository
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
//command
builder.Services.AddScoped<ICommandHandler<CreateCommand, Order>, CreateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateCommand, Order>, UpdateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteCommand>, DeleteCommandHandler>();
//query
builder.Services.AddScoped<IQueryHandler<GetAllPagingQuery, PageResult<OrderResponse>>, GetAllPagingQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetByIdQuery, Order>, GetByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<OrderDashboardQuery, OrderDashboardResult>, OrderDashboardQueryHandler>();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

// Đăng ký APIServiceOption và IAPIService
builder.Services.Configure<APIServiceOption>(
    builder.Configuration.GetSection("APIService")
);
builder.Services.AddHttpClient<IAPIService, APIService>();

// Email service
builder.Services.AddHttpClient<IEmailService, EmailService>();


//Đăng ký cho FE
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
});


//Đăng ký lấy thông tin từ token
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

//Đăng ký HttpClient
//builder.Services.AddHttpClient<IAccountService, AccountService>(client =>
//{
//    client.BaseAddress = new Uri("https://authorize-api-dev.lighttail.com/api/");
//}).AddPolicyHandler(HttpPolicyExtensions
//    .HandleTransientHttpError()
//    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2)));

//builder.Services.AddHttpClient<IScreenServicce, ScreenService>(client =>
//{
//    client.BaseAddress = new Uri("https://authorize-api-dev.lighttail.com/swagger/index.html");
//}).AddPolicyHandler(HttpPolicyExtensions
//    .HandleTransientHttpError()
//    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2)));

//builder.Services.Configure<RouteOptions>(options =>
//{
//    options.LowercaseUrls = true;
//});

//builder.Services.AddSwaggerGen(c =>
//{
//    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
//    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
//    c.IncludeXmlComments(xmlPath);
//});

//var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
//builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidIssuer = jwtSettings!.Issuer,
//        ValidAudience = jwtSettings.Audience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,

//        RoleClaimType = ClaimTypes.Role
//    };

//    options.Events = new JwtBearerEvents
//    {
//        OnChallenge = async context =>
//        {
//            context.HandleResponse();
//            if (!context.Response.HasStarted)
//            {
//                context.Response.StatusCode = 401;
//                context.Response.ContentType = "application/json";
//                await context.Response.WriteAsync(JsonSerializer.Serialize(new
//                {
//                    statusCode = 401,
//                    message = "Unauthorized – Token missing or invalid.",
//                    timestamp = DateTime.UtcNow
//                }));
//            }
//        },
//        OnForbidden = async context =>
//        {
//            if (!context.Response.HasStarted)
//            {
//                context.Response.StatusCode = 403;
//                context.Response.ContentType = "application/json";
//                await context.Response.WriteAsync(JsonSerializer.Serialize(new
//                {
//                    statusCode = 403,
//                    message = "Forbidden – You don't have permission.",
//                    timestamp = DateTime.UtcNow
//                }));
//            }
//        }
//    };
//});

builder.Services.AddScoped<IUseCaseDispatcher, UseCaseDispatcher>();



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();