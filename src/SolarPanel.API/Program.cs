using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SolarPanel.Application.Interfaces;
using SolarPanel.Core.Interfaces;
using SolarPanel.Infrastructure.BackgroundServices;
using SolarPanel.Infrastructure.Data;
using SolarPanel.Infrastructure.Options;
using SolarPanel.Infrastructure.Repositories;
using SolarPanel.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SolarPanelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SolarData")));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MqttSettings"));
builder.Services.Configure<RemoteScriptOptions>(builder.Configuration.GetSection("RemoteScript"));

builder.Services.AddScoped<ISolarDataRepository, SolarDataRepository>();
builder.Services.AddScoped<ISolarDataService, SolarDataService>();

builder.Services.AddSingleton<IFileHashService, FileHashService>();
builder.Services.AddSingleton<IScriptRepository, FileScriptRepository>();
builder.Services.AddSingleton<IScriptService, ScriptService>();
builder.Services.AddSingleton<MqttService>();

if (!builder.Environment.IsDevelopment())
{
    if (builder.Configuration.GetValue<bool>("MqttSettings:UseMockData"))
        builder.Services.AddHostedService<MockMqttBackgroundService>();
    else
        builder.Services.AddHostedService<MqttBackgroundService>();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    appDbContext.Database.Migrate();
    if (!app.Environment.IsDevelopment())
    {
        var solarPanelContext = scope.ServiceProvider.GetRequiredService<SolarPanelDbContext>();
        solarPanelContext.Database.Migrate();
    }
}

app.Run();