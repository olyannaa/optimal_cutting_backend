using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using vega.Migrations.EF;
using vega.Services;
using vega.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("VegaDB");
builder.Services.AddDbContext<VegaContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        });
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["RedisCacheOptions:Configuration"];
    options.InstanceName = builder.Configuration["RedisCacheOptions:InstanceName"];
});

builder.Services.AddAuthorization();

builder.Services.AddCors();

var authOptions = builder.Configuration.GetSection("AuthOptions");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authOptions["Issuer"],
            ValidateAudience = true,
            ValidAudience = authOptions["Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authOptions["Key"])),
            ValidateIssuerSigningKey = true
         };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = (TokenValidatedContext context) =>
            {
                var tokenManager = context.HttpContext.RequestServices.GetService<ITokenManagerService>();
                if (!tokenManager.IsTokenValid())
                {
                    context.Fail("Failed additional validation");
                }

                return Task.CompletedTask;
            }
        };
});

builder.Services.AddScoped<ITokenManagerService, TokenManagerService>();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ICSVService, CSVService>();
builder.Services.AddScoped<ICutting1DService, Cutting1DService>();
builder.Services.AddScoped<ICutting2DService, Cutting2DService>();
builder.Services.AddScoped<IDrawService, DrawService>();
builder.Services.AddHttpClient<IDXFService, DXFService>(client =>
{
    client.BaseAddress = new Uri("http://127.0.0.1:8000/");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

var app = builder.Build();

app.UseCors(builder =>
     builder.AllowAnyOrigin()
         .AllowAnyHeader()
         .AllowAnyMethod()
         .WithExposedHeaders("Content-Disposition")
     );

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

app.UseAuthorization();

app.MapControllers();

app.Run();