using Microsoft.AspNetCore.Builder;
using Nop.Web.Framework.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure application services
builder.Services.ConfigureApplicationServices(builder);

var app = builder.Build();

// Configure the application HTTP request pipeline
app.ConfigureRequestPipeline();

app.Run();

