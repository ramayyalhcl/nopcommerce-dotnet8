# Program.cs Refactoring Plan - Following Official nopCommerce .NET 8.0 Pattern

**Date**: January 28, 2026  
**Reference**: Official nopCommerce 4.70.5 Program.cs  
**Goal**: Refactor our Program.cs to match official modular architecture

---

## Current State (Our Implementation)

Our `Program.cs` is **~250 lines** with all logic inline:
- Manual service registration
- Manual engine initialization
- Manual middleware pipeline setup
- Manual route registration

**Problem**: Too much code in one file, hard to maintain, doesn't match official pattern

---

## Target State (Official Pattern)

Official `Program.cs` is **~52 lines** using extension methods:
```csharp
public static async Task Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    
    // Configuration
    builder.Configuration.AddJsonFile(...);
    
    // Services
    builder.Services.ConfigureApplicationSettings(builder);
    builder.Services.ConfigureApplicationServices(builder);
    
    var app = builder.Build();
    
    // Pipeline
    app.ConfigureRequestPipeline();
    await app.PublishAppStartedEventAsync();
    
    await app.RunAsync();
}
```

**Key Pattern**: Extension methods handle all complexity

---

## Required Extension Methods

### 1. ServiceCollectionExtensions.cs
**Location**: `Nop.Web.Framework/Infrastructure/Extensions/ServiceCollectionExtensions.cs`

**Methods**:
- `ConfigureApplicationSettings(IServiceCollection, WebApplicationBuilder)` - Load config
- `ConfigureApplicationServices(IServiceCollection, WebApplicationBuilder)` - Register all services

**Responsibilities**:
- Load NopConfig from appsettings.json
- Register Autofac container
- Register ITypeFinder
- Register all DependencyRegistrars
- Initialize engine
- Register startup tasks

### 2. ApplicationBuilderExtensions.cs
**Location**: `Nop.Web.Framework/Infrastructure/Extensions/ApplicationBuilderExtensions.cs`

**Methods**:
- `ConfigureRequestPipeline(IApplicationBuilder)` - Setup middleware pipeline

**Responsibilities**:
- Exception handling
- Static files
- Routing
- Session
- Authentication
- Authorization
- Route registration via RoutePublisher
- Endpoint mapping

---

## Implementation Steps

### Step 1: Create Infrastructure/Extensions Folder Structure
```
Nop.Web.Framework/
└── Infrastructure/
    └── Extensions/
        ├── ServiceCollectionExtensions.cs
        └── ApplicationBuilderExtensions.cs
```

### Step 2: Move Service Registration Logic
Extract from Program.cs:
- Configuration loading
- Autofac setup
- DependencyRegistrar registration
- Engine initialization
- Startup tasks

### Step 3: Move Middleware Pipeline Logic
Extract from Program.cs:
- Exception handling
- Static files
- Routing
- Session/Auth/Authorization
- Route registration
- Endpoint mapping

### Step 4: Simplify Program.cs
Replace with:
- WebApplicationBuilder creation
- Extension method calls
- app.RunAsync()

### Step 5: Test
- Verify application starts
- Verify routes work
- Verify services resolve

---

## Benefits

1. **Modularity**: Each extension method has single responsibility
2. **Testability**: Extension methods can be tested independently
3. **Maintainability**: Changes isolated to specific extension methods
4. **Consistency**: Matches official nopCommerce architecture
5. **Readability**: Program.cs becomes simple and clear

---

## Migration Notes

- Keep existing DependencyRegistrar pattern (it works)
- Keep Autofac integration (official uses it too)
- Keep EngineContext pattern (official uses it)
- Only refactor structure, not functionality

---

**Next Action**: Implement ServiceCollectionExtensions.cs first
