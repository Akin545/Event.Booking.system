using AutoMapper;

using Event.Booking.system;
using Event.Booking.system.Filters;
using Event.Booking.system.GlobalFilters;
using Event.Booking.system.MappingProfile;
using Event.Booking.System.Core.Dtos.Auth;
using Event.Booking.System.Core.Models;
using Event.Booking.System.Repository;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = new SqlConnection(builder.Configuration.GetConnectionString("SqlDb"));

builder.Services.AddDbContext<AppDbContext>(
            options => options.UseSqlServer(connectionString,
            options => options.MigrationsAssembly("Event.Booking.System.Repository"))
            );

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var jwtSettings = new JwtSettings { Key = builder.Configuration["Jwt:Key"] ?? "VerySecretKey1234567890655544543344565767664545455456", ExpiresMinutes = 60 };
builder.Services.AddSingleton(jwtSettings);
// Authentication
var key = Encoding.UTF8.GetBytes(jwtSettings.Key);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});


builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Event.Booking.Api", Version = "v1" });
    option.EnableAnnotations();
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
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



var MyAllowedOrigins = "_myCORS";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowedOrigins,
                        policy =>
                        {
                            policy.WithOrigins("http://localhost:4200")
                             .AllowAnyHeader()
                             .AllowAnyMethod()
                             .AllowCredentials();
                        });
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(GlobalErrorFilters));
    options.Filters.Add(typeof(HttpsHandleFilter));
}).AddJsonOptions(options =>
{
    // Preserve property names as defined in the C# models (disable camelCase naming)
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.WriteIndented = true;
    options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 32;
});

AppDependenceInjectionClients.Resgister(builder);
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var mapperConfig = new MapperConfiguration(mp =>
{
    mp.AddProfile(new MappingProfiles());
});

IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10485760;
});

var app = builder.Build();

/// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        dbContext.Database.Migrate();  // Automatically applies pending migrations
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database and seeding roles or Super Admin.");
    }
}

    app.UseSwaggerUI(c =>
{
    //c.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event.Booking.Api v1");

});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
