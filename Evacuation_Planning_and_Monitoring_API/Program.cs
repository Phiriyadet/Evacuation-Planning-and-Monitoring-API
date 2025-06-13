using Evacuation_Planning_and_Monitoring_API.Data;
using Microsoft.EntityFrameworkCore;
using Evacuation_Planning_and_Monitoring_API.Controllers;
using Evacuation_Planning_and_Monitoring_API.Interfaces;
using Evacuation_Planning_and_Monitoring_API.Repositories;
using StackExchange.Redis;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

//var keyVaultEndpoint = new Uri(Environment.GetEnvironmentVariable("VaultUri")!);
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());

var sqlConnectionString = builder.Configuration.GetConnectionString("DatabaseConnection");
//var sqlConnectionString = builder.Configuration["DatabaseConnection"] ?? throw new InvalidOperationException("DatabaseConnection is not set in the configuration.");

//Console.WriteLine($"SQL Connection String: {sqlConnectionString}");
builder.Services.AddDbContext<ApplicationDBContext>(options =>
{
    //options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseSqlServer(sqlConnectionString);
});
var redisConn = builder.Configuration.GetConnectionString("CacheRedisConnection");
//var redisConn = builder.Configuration["CacheRedisConnection"] ?? throw new InvalidOperationException("CacheRedisConnection is not set in the configuration.");
//Console.WriteLine($"Redis Connection String: {redisConn}");
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
builder.Services.AddScoped<IEvacuationZoneRepository, EvacuationZoneRepository>();
builder.Services.AddScoped<IEvacuationRepository, EvacuationRepository>();
builder.Services.AddScoped<IRedisRepository, RedisRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
