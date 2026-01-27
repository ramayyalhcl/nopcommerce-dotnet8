using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Configure basic ASP.NET Core services
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();
builder.Services.AddSession();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Use Autofac for DI
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Register all nopCommerce services via DependencyRegistrar pattern
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    // Find all DependencyRegistrar implementations
    var typeFinder = new WebAppTypeFinder();
    var registrarTypes = typeFinder.FindClassesOfType<IDependencyRegistrar>();
    
    var registrars = registrarTypes
        .Select(type => (IDependencyRegistrar)Activator.CreateInstance(type))
        .OrderBy(r => r.Order)
        .ToList();

    // Register all services
    var nopConfig = new NopConfig();
    foreach (var registrar in registrars)
    {
        registrar.Register(containerBuilder, typeFinder, nopConfig);
    }
});

var app = builder.Build();

// ASP.NET Core 8 middleware pipeline
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Map routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
