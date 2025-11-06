using CareNest_Order.Application.Common;
using CareNest_Order.Application.Features.Commands.Create;
using CareNest_Order.Application.Features.Commands.Delete;
using CareNest_Order.Application.Features.Commands.Update;
using CareNest_Order.Application.Features.Queries.GetAllPaging;
using CareNest_Order.Application.Features.Queries.GetById;
using CareNest_Order.Application.Features.Queries.CheckOrderStatus;
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
using System.Net.Http;
using CareNest_Order.Application.Features.Commands.UpdateStatus;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
// Lấy DatabaseSettings ưu tiên từ DATABASE_URL (Koyeb), fallback sang biến môi trường phẳng/ip-port và configuration
var config = builder.Configuration;
string? databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
string baseConnectionString;
if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    // Parse postgres://username:password@host:port/dbname
    var uri = new Uri(databaseUrl);
    var userInfoParts = (uri.UserInfo ?? string.Empty).Split(':', 2);
    var user = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : string.Empty;
    var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;
    var host = uri.Host;
    var port = uri.Port > 0 ? uri.Port : 5432;
    var database = uri.AbsolutePath.TrimStart('/');

    baseConnectionString = $"Host={host};Port={port};Database={database};Username={user};Password={password}";
}
else
{
    DatabaseSettings dbSettings = new DatabaseSettings
    {
        Ip = config["DB_HOST"] ?? config["DatabaseSettings:Ip"],
        Port = int.TryParse(config["DB_PORT"], out var port)
            ? port
            : (config.GetSection("DatabaseSettings").GetValue<int?>("Port") ?? 5432),
        User = config["DB_USER"] ?? config["DatabaseSettings:User"],
        Password = config["DB_PASSWORD"] ?? config["DatabaseSettings:Password"],
        Database = config["DB_NAME"] ?? config["DatabaseSettings:Database"]
    };
    // In ra cấu hình DB (tránh null ref trong môi trường không có console)
    try { dbSettings.Display(); } catch { }
    baseConnectionString = dbSettings.GetConnectionString();
}
// Bổ sung tham số pooling/timeouts phù hợp môi trường cloud
string connectionString = baseConnectionString + ";Pooling=true;Maximum Pool Size=1;Minimum Pool Size=0;Timeout=15;";


// Đăng ký DbContext với PostgreSQL
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
        // npgsqlOptions.CommandTimeout(60);
    }));

builder.Services.AddTransient<DatabaseSeeder>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Đăng ký service thêm chú thích cho api
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

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
builder.Services.AddScoped<ICommandHandler<CreateCommand, CreateOrderResult>, CreateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateCommand, Order>, UpdateCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateOrderStatusToCancelCommand, Order>, UpdateOrderStatusToCancelCommandHandler>();
builder.Services.AddScoped<ICommandHandler<UpdateOrderStatusCommand, Order>, UpdateOrderStatusCommandHandler>();
builder.Services.AddScoped<ICommandHandler<DeleteCommand>, DeleteCommandHandler>();
//query
builder.Services.AddScoped<IQueryHandler<GetAllPagingQuery, PageResult<OrderResponse>>, GetAllPagingQueryHandler>();
builder.Services.AddScoped<IQueryHandler<GetByIdQuery, Order>, GetByIdQueryHandler>();
builder.Services.AddScoped<IQueryHandler<OrderDashboardQuery, OrderDashboardResult>, OrderDashboardQueryHandler>();
builder.Services.AddScoped<IQueryHandler<CheckOrderStatusQuery, bool>, CheckOrderStatusQueryHandler>();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings")
);

// Đăng ký APIServiceOption và IAPIService
builder.Services.Configure<APIServiceOption>(
    builder.Configuration.GetSection("APIService")
);
builder.Services.AddHttpClient<IAPIService, APIService>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        MaxConnectionsPerServer = 1,
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    });

// Email service
builder.Services.AddHttpClient<IEmailService, EmailService>()
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        MaxConnectionsPerServer = 1,
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    });

// Override APIServiceOption từ các biến môi trường dạng phẳng (APIServiceBaseUrlXxx)
builder.Services.PostConfigure<APIServiceOption>(opt =>
{
    string? GetEnv(string key) => Environment.GetEnvironmentVariable(key);

    opt.BaseUrlOrderDetail = GetEnv("APIServiceBaseUrlOrderDetail") ?? opt.BaseUrlOrderDetail;
    opt.BaseUrlShop = GetEnv("APIServiceBaseUrlShop") ?? opt.BaseUrlShop;
    opt.BaseUrlAddress = GetEnv("APIServiceBaseUrlAddress") ?? opt.BaseUrlAddress;
    opt.BaseUrlProduct = GetEnv("APIServiceBaseUrlProduct") ?? opt.BaseUrlProduct;
    opt.BaseUrlAuthorize = GetEnv("APIServiceBaseUrlAuthorize") ?? opt.BaseUrlAuthorize;
    opt.BaseUrlPay = GetEnv("APIServiceBaseUrlPay") ?? opt.BaseUrlPay;
    opt.BaseUrlReview = GetEnv("APIServiceBaseUrlReview") ?? opt.BaseUrlReview;
});

//Đăng ký cho FE
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
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
var swaggerEnabled = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled");
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tùy chọn chạy migrations khi được bật qua biến môi trường RUN_MIGRATIONS=true
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    var runMigrations = Environment.GetEnvironmentVariable("RUN_MIGRATIONS");
    if (!string.IsNullOrWhiteSpace(runMigrations) && runMigrations.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        context.Database.Migrate();
    }
}
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();