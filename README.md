# nopCommerce 3.90 → .NET 8 Migration

> **Status:** Core libraries migrated ✅ | Runtime needs expert wireup ⚠️

This repository contains nopCommerce 3.90 migrated from .NET Framework 4.5.1 to .NET 8.0.

---

## 🎯 Migration Overview

| Component | Status | Compilation | Notes |
|-----------|--------|-------------|-------|
| **Nop.Core** | ✅ Complete | 0 errors | IHttpContextAccessor pattern implemented |
| **Nop.Data** | ✅ Complete | 0 errors | EF Core 8, 103 entity mappings migrated |
| **Nop.Services** | ✅ Complete | 0 errors | 50+ services, Cookie auth, SkiaSharp |
| **Nop.Web.Framework** | ✅ Complete | 0 errors | 399→0 errors fixed |
| **Nop.Web** | ✅ Complete | 0 errors | 230 files, 5000+ errors fixed |
| **Nop.Admin** | ⏸️ Not started | - | 9,325 issues expected |
| **Plugins (20)** | ⏸️ Skipped | - | Optional, can migrate later |
| **Tests (5)** | ⏸️ Skipped | - | Can migrate after core works |

---

## ✅ What's Been Completed

### **1. Project Conversions**
- All 5 core projects converted to SDK-style `.csproj`
- Target framework updated: `net8.0`
- Package references updated to .NET 8 compatible versions

### **2. Major API Migrations**

| Old API (.NET Framework) | New API (.NET 8) | Status |
|-------------------------|------------------|--------|
| `HttpContext.Current` | `IHttpContextAccessor` | ✅ Done |
| `Session[key] = value` | `Session.SetString()` | ✅ Done |
| `Request.InputStream` | `Request.Body` | ✅ Done |
| `Request["key"]` | `Request.Query["key"]` | ✅ Done |
| `HttpUnauthorizedResult` | `UnauthorizedResult` | ✅ Done |
| `FluentValidation.Custom()` | `RuleFor().Custom()` | ✅ Done |
| `IFormFile.GetPictureBits()` | Stream → byte[] | ✅ Done |
| StringValues comparisons | `.ToString()` | ✅ Done |

### **3. Package Updates**

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.10" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation" Version="8.0.10" />
<PackageReference Include="Autofac.Extensions.DependencyInjection" Version="9.0.0" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
<PackageReference Include="System.Linq.Dynamic.Core" Version="1.6.0" />
```

### **4. Build Status**
- ✅ All 5 projects compile with **0 errors**
- ⚠️ 24 warnings (legacy packages: ImageResizer, iTextSharp)

---

## ⚠️ What Needs Expert Attention

### **CRITICAL: Runtime Issues**

The code **compiles** but **won't run properly** yet. An expert needs to fix:

#### **1. Program.cs - DI Wireup (PRIORITY 1)**

**Location:** `Presentation/Nop.Web/Program.cs`

**Issue:** Custom DI container setup needs validation:

```csharp
// Current implementation - needs review
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    var typeFinder = new WebAppTypeFinder();
    var registrars = typeFinder.FindClassesOfType<IDependencyRegistrar>()
        .Select(type => (IDependencyRegistrar)Activator.CreateInstance(type))
        .OrderBy(r => r.Order);
    
    foreach (var registrar in registrars)
        registrar.Register(containerBuilder, typeFinder, new NopConfig());
});
```

**What expert needs to do:**
- Verify all services are registered correctly
- Check nopCommerce engine initialization
- Validate middleware pipeline order
- Test dependency resolution at runtime

#### **2. View Helpers - Html.Widget() & Html.Action() (PRIORITY 2)**

**Issue:** These helpers don't exist in .NET 8:

**Locations:**
- `Presentation/Nop.Web/Views/Home/Index.cshtml`
- `Presentation/Nop.Web/Views/Shared/_ColumnsOne.cshtml`
- Many other views

**Errors at runtime:**
```
'IHtmlHelper<dynamic>' does not contain a definition for 'Widget'
'IHtmlHelper<dynamic>' does not contain a definition for 'Action'
```

**What expert needs to do:**
- Implement `Html.Widget()` extension method
- Convert `Html.Action()` calls to **ViewComponents** (ASP.NET Core pattern)
- See: https://learn.microsoft.com/en-us/aspnet/core/mvc/views/view-components

#### **3. Files with TODOs (PRIORITY 3)**

| File | TODO | Reason |
|------|------|--------|
| `CheckoutController.cs` | Session JSON serialization | Complex object storage stubbed |
| `ShoppingCartModelFactory.cs` | Session retrieval | JSON deserialization needed |
| `CommonModelFactory.cs` | Server.MapPath | Need `IWebHostEnvironment` injection |
| `RouteProvider.cs` | Route configuration | May need ASP.NET Core routing adjustments |
| `Program.cs` | Engine initialization | nopCommerce startup sequence incomplete |

**Search for:** `// TODO:` in codebase

---

## 🚀 How to Build & Run

### **Prerequisites**
- .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0
- Visual Studio 2022 (17.8+) OR VS Code

### **Build**
```powershell
cd src
dotnet build Presentation\Nop.Web\Nop.Web.csproj
```

**Expected:** 0 errors, ~24 warnings (safe to ignore)

### **Run**
```powershell
cd src\Presentation\Nop.Web
dotnet run
```

**Expected behavior:**
- ✅ Site starts listening on https://localhost:64911
- ❌ Browser shows errors (Html.Widget not found)
- ❌ DI resolution may fail

**This is expected!** The code compiles but runtime needs the expert fixes above.

---

## 📋 Next Steps for Expert

### **Phase 1: Get Site Running (Est: 4-6 hours)**

1. **Fix Program.cs DI wireup**
   - Review `DependencyRegistrar` implementations in:
     - `Nop.Web/Infrastructure/DependencyRegistrar.cs`
     - `Nop.Web.Framework/DependencyRegistrar.cs`
   - Verify all factories, services, repositories are registered
   - Add missing service registrations

2. **Implement Html.Widget() helper**
   - Create in: `Nop.Web.Framework/HtmlExtensions.cs`
   - Logic: Render widgets from `IWidgetService`
   - Reference: nopCommerce 4.70.5 implementation

3. **Convert Html.Action() to ViewComponents**
   - Priority views:
     - `TopicBlock` → `TopicViewComponent`
     - `HomepageCategories` → `CatalogViewComponent`
     - `HomepageProducts` → `ProductViewComponent`
   - See ASP.NET Core ViewComponent docs

4. **Test basic page load**
   - Homepage should render without errors
   - Verify DI works (no missing service exceptions)

### **Phase 2: Complete TODOs (Est: 2-3 hours)**

1. Implement Session JSON serialization for complex objects
2. Replace `Server.MapPath` with `IWebHostEnvironment.ContentRootPath`
3. Validate routing works with ASP.NET Core endpoint routing

### **Phase 3: Migrate Nop.Admin (Est: 8-12 hours)**

1. Apply same migration patterns as Nop.Web
2. Handle admin-specific features (grid, tabs, etc.)

### **Phase 4: Migrate Plugins (Optional)**

1. Start with essential plugins:
   - `Payments.Manual`
   - `Shipping.FixedOrByWeight`
   - `Tax.FixedOrByCountryStateZip`

---

## 🔧 Technical Details

### **Target Frameworks**
```xml
<TargetFramework>net8.0</TargetFramework>
```

### **Key Dependencies**
- **ASP.NET Core:** 8.0.10
- **Entity Framework Core:** 8.0.10
- **Autofac:** 9.0.0
- **FluentValidation:** 11.3.0

### **Known Warnings (Safe to Ignore)**
- `NU1701`: ImageResizer, iTextSharp using .NET Framework
- `NETSDK1086`: Redundant Microsoft.AspNetCore.App reference

### **Legacy Packages to Replace (Future)**
- `ImageResizer 4.2.8` → `ImageSharp` or `SkiaSharp`
- `iTextSharp 5.5.13.4` → `iText7` or `QuestPDF`

---

## 📚 Resources

### **Migration Documentation**
- See: `.github/upgrades/` folder
  - `assessment.md` - Initial analysis
  - `plan.md` - Migration plan
  - `tasks.md` - Execution tasks
  - `execution_log.md` - Detailed log

### **Microsoft Docs**
- [ASP.NET Core Migration Guide](https://learn.microsoft.com/en-us/aspnet/core/migration/proper-to-2x/)
- [Dependency Injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [View Components](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/view-components)

### **nopCommerce References**
- [nopCommerce 4.70.5 Source](https://github.com/nopSolutions/nopCommerce) - See how they did .NET 8
- [nopCommerce Forums](https://www.nopcommerce.com/boards/)

---

## 👥 Team

**Migration Work:** Completed by Ramasubbarao Ayyal  
**Next:** Expert review of DI wireup and view helpers

---

## 📝 Change Log

### v1.0 - Core Libraries Migration (Current)
- Migrated 5 core projects to .NET 8
- 0 compilation errors achieved
- Program.cs basic wireup implemented
- Ready for expert runtime fixes

---

## ❓ FAQ

**Q: Can I run the site now?**  
A: It will start but show errors. Needs expert fixes to Program.cs and view helpers.

**Q: Why are plugins not migrated?**  
A: They're optional and can be migrated incrementally after core works.

**Q: What's the biggest remaining issue?**  
A: DI container setup in Program.cs needs validation by someone familiar with nopCommerce's architecture.

**Q: How long to complete migration?**  
A: Expert needs 4-6 hours for runtime fixes, then site should be functional.

---

**Status as of:** January 2026  
**Repository:** https://github.com/ramayyalhcl/nopcommerce-dotnet8  
**Branch:** main
