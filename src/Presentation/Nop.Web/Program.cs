using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Nop.Web.Framework.Infrastructure.Extensions;
using Serilog;

// Log to a file so the agent (or anyone) can read logs without holding the terminal
var logPath = Path.Combine(AppContext.BaseDirectory, "App_Data", "logs", "agent-app.log");
var logDir = Path.GetDirectoryName(logPath);
if (!string.IsNullOrEmpty(logDir) && !Directory.Exists(logDir))
    Directory.CreateDirectory(logDir);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .WriteTo.Console()
    .WriteTo.File(logPath, shared: true)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Configure application services
builder.Services.ConfigureApplicationServices(builder);

var app = builder.Build();

// Configure the application HTTP request pipeline
app.ConfigureRequestPipeline();

app.Run();

