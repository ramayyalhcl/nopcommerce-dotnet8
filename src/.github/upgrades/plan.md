# .NET 8 Upgrade Plan - NopCommerce Solution

## Table of Contents
1. [Executive Summary](#executive-summary)
2. [Migration Strategy](#migration-strategy)
3. [Detailed Dependency Analysis](#detailed-dependency-analysis)
4. [Project-by-Project Plans](#project-by-project-plans)
5. [Risk Management](#risk-management)
6. [Testing & Validation Strategy](#testing--validation-strategy)
7. [Complexity & Effort Assessment](#complexity--effort-assessment)
8. [Source Control Strategy](#source-control-strategy)
9. [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Overview
This plan outlines the upgrade of the NopCommerce e-commerce solution from **.NET Framework 4.5.1** to **.NET 8.0**. The solution contains 34 projects including core libraries, web applications, 18 plugins, and 5 test projects.

### Current State
- **Total Projects**: 34 active projects + 3 empty placeholder projects (to be removed)
- **Source Framework**: .NET Framework 4.5.1
- **Target Framework**: .NET 8.0
- **Total Issues Detected**: 17,873 (15,956 mandatory, 1,835 potential, 82 optional)
- **Affected Files**: 441 code files across the solution
- **Incompatible Packages**: 34 of 62 NuGet packages require replacement or removal
- **Security Vulnerabilities**: Found in 5 projects (Nop.Core, Nop.Services, Nop.Web.Framework, Nop.Web, Nop.Admin, Nop.Plugin.ExternalAuth.Facebook, Nop.Plugin.Payments.PayPalDirect, Nop.Plugin.Shipping.AustraliaPost)

### Discovered Metrics & Complexity Classification

**Dependency Depth**: 5 levels (0-5)
- Level 0: Foundation libraries (Nop.Core + 3 empty placeholders)
- Level 1: Data access layer (Nop.Data, Nop.Tests)
- Level 2: Services and test projects
- Level 3: Web framework, plugin utilities
- Level 4: Web applications and all plugins (18 projects)
- Level 5: MVC tests

**Risk Indicators**:
- ?? **Critical**: Nop.Web (3,997 issues), Nop.Admin (9,325 issues), Nop.Web.Framework (1,794 issues)
- ?? **High**: Nop.Services (407 issues), Nop.Core (305 issues)
- ? **Low**: Most plugins (30-200 issues each), test projects

**Technology Migration Complexity**:
- **ASP.NET Framework ? ASP.NET Core**: 17,195 issues (System.Web removal)
- **Entity Framework 6 ? EF Core**: Database layer rewrite required
- **OWIN ? ASP.NET Core Middleware**: Authentication/authorization changes
- **MVC 5 ? ASP.NET Core MVC**: Routing, filters, view engine updates
- **Legacy packages**: 34 packages need replacement (WebActivator, MiniProfiler, Autofac.Mvc5, etc.)

**Complexity Classification**: **CRITICAL - Complex Solution**
- **Why**: 
  - Large solution (34 projects, 17K+ issues)
  - Deep dependency chain (5 levels)
  - Major framework transition (ASP.NET ? ASP.NET Core)
  - High-risk projects with 9K+ issues each
  - Security vulnerabilities present
  - Extensive plugin ecosystem requiring coordination

### Empty Placeholder Projects (To Remove First)
The VS Upgrade Assistant created these empty projects without migrating code:
- **Nop.CoreCore.csproj** - Empty .NET 8 shell, no code
- **Nop.DataCore.csproj** - Empty .NET 8 shell, no code  
- **Nop.ServicesCore.csproj** - Empty .NET 8 shell, no code

**Action**: Delete these projects in Phase 0 before starting actual migration.

### Recommended Approach
**Bottom-Up Incremental Migration** - Upgrade projects tier-by-tier, starting from foundation libraries and progressing upward through the dependency chain.

**Rationale**:
1. **Dependency Safety**: Each tier builds on already-upgraded foundation
2. **Risk Mitigation**: Isolate issues to current tier, validate before proceeding
3. **Incremental Progress**: Complete one tier at a time, maintain working state
4. **Pattern Learning**: Lessons from early tiers (Core, Data, Services) inform later work
5. **Testing Isolation**: Test each tier independently before integration

### Iteration Strategy
Given critical complexity, using **risk-first incremental iterations**:
1. **Phase 0**: Cleanup (remove empty projects)
2. **Phase 1**: Foundation tier (Nop.Core - highest priority)
3. **Phase 2**: Data tier (Nop.Data - EF6 to EF Core migration)
4. **Phase 3**: Services tier (Nop.Services - high complexity, 407 issues)
5. **Phase 4**: Web Framework tier (Nop.Web.Framework - critical, 1,794 issues)
6. **Phase 5**: Applications tier (Nop.Web + Nop.Admin - 13K+ combined issues)
7. **Phase 6**: Plugins tier (18 plugins - batch by complexity)
8. **Phase 7**: Test projects tier (validate entire upgrade)

**Expected Iterations**: 10-12 detailed planning iterations

## Migration Strategy

### Selected Approach: Bottom-Up Incremental Migration

**Strategy Rationale**:
Given the critical complexity of this solution (34 projects, 17K+ issues, major framework transition), a bottom-up tier-by-tier approach provides the safest path to .NET 8.

**Why Bottom-Up**:
1. ? **Dependency Safety**: Foundation projects (Core, Data) upgraded first, consumers depend on stable base
2. ? **Risk Isolation**: Issues contained within current tier, no cascading failures
3. ? **Validation Points**: Test each tier independently before proceeding
4. ? **Pattern Learning**: Solutions from early tiers (especially Web.Framework ASP.NET Core migration) inform later work
5. ? **Incremental Progress**: Each tier completion represents measurable milestone
6. ? **No Multi-Targeting**: Projects always reference same or newer framework version

**Why NOT All-At-Once**:
- ? 17,873 issues simultaneously overwhelming
- ? Cannot isolate/debug failures across 34 projects
- ? ASP.NET ? ASP.NET Core transition requires careful staged migration
- ? EF6 ? EF Core database changes need isolated testing
- ? High risk of breaking production-critical applications

### Bottom-Up Strategy Application

**Tier Ordering Principles**:
1. **Tier 0 (Foundation)**: Zero internal dependencies ? upgrade first
2. **Tier N+1**: Depends only on Tiers 0 through N ? upgrade after dependencies complete
3. **Validation**: Each tier must be stable before starting next tier
4. **No Skipping**: Cannot skip a tier or upgrade out of order

**Execution Flow Per Tier**:

Each tier follows this pattern:

1. **Preparation** (Analysis & Planning)
   - Review tier's projects and dependencies
   - Verify lower tiers are stable
   - Identify tier-specific breaking changes
   - Plan package updates

2. **Conversion** (Project File Transformation)
   - Convert projects to SDK-style format
   - Update TargetFramework to net8.0
   - Remove obsolete project elements
   - Update project references

3. **Package Updates** (Dependency Resolution)
   - Update compatible packages to .NET 8 versions
   - Replace incompatible packages with .NET 8 equivalents
   - Remove packages included in framework
   - Address security vulnerabilities

4. **Code Fixes** (Breaking Change Resolution)
   - Fix compilation errors from API changes
   - Update obsolete API usage
   - Refactor System.Web dependencies (ASP.NET projects)
   - Migrate Entity Framework code (Data projects)

5. **Testing** (Tier Validation)
   - Build all projects in tier
   - Run unit tests for tier
   - Run integration tests with lower tiers
   - Verify no regressions in dependent projects still on .NET Framework

6. **Stabilization** (Quality Gates)
   - Zero build errors
   - Zero build warnings (if achievable)
   - All tier tests passing
   - No new security vulnerabilities
   - Performance acceptable

7. **Checkpoint** (Tier Completion)
   - Document lessons learned
   - Commit tier changes
   - Mark tier complete
   - Proceed to next tier

### Migration Phases

**Phase 0: Cleanup & Preparation**
- **Duration Estimate**: Relative complexity: LOW
- **Projects**: 3 empty placeholder projects
- **Actions**: 
  - Delete Nop.CoreCore, Nop.DataCore, Nop.ServicesCore
  - Update solution file to remove references
  - Verify solution builds
- **Success Criteria**: Clean solution with only active projects

**Phase 1: Foundation Tier (Tier 0)**
- **Duration Estimate**: Relative complexity: HIGH
- **Projects**: Nop.Core (1 project)
- **Key Changes**:
  - SDK-style conversion
  - Remove System.Web dependencies (305 issues include many System.Web references)
  - Update Autofac, AutoMapper, Newtonsoft.Json
  - Replace ASP.NET MVC packages with ASP.NET Core equivalents
- **Validation**: Nop.Core builds, no errors, NuGet vulnerabilities resolved
- **Blocker**: Entire solution blocked until complete

**Phase 2: Data Access Tier (Tier 1)**
- **Duration Estimate**: Relative complexity: HIGH
- **Projects**: Nop.Data (1 project), Nop.Tests (1 project)
- **Key Changes**:
  - **Nop.Data**: Entity Framework 6 ? EF Core 8 migration
  - Remove EntityFramework.SqlServerCompact
  - Update mapping configurations (EF6 ? EF Core fluent API)
  - DbContext initialization changes
  - **Nop.Tests**: Update test base infrastructure
- **Validation**: Database migrations work, data access tests pass
- **Blocker**: 9 projects blocked until Nop.Data complete

**Phase 3: Services & Testing Tier (Tier 2)**
- **Duration Estimate**: Relative complexity: HIGH
- **Projects**: Nop.Services (1 project), Nop.Core.Tests, Nop.Data.Tests
- **Key Changes**:
  - **Nop.Services**: Business logic updates (407 issues, 189 mandatory)
  - Update service dependencies (EPPlus, ImageResizer, Azure Storage)
  - Replace WindowsAzure.Storage with Azure.Storage.Blobs
  - Update Redis clients (StackExchange.Redis)
  - **Test Projects**: Update NUnit, test infrastructure
- **Validation**: Services layer builds, business logic tests pass
- **Blocker**: 24 projects blocked until Nop.Services complete

**Phase 4: Web Framework & Utilities Tier (Tier 3)**
- **Duration Estimate**: Relative complexity: CRITICAL
- **Projects**: Nop.Web.Framework (1 project), Nop.Services.Tests, Nop.Plugin.ExchangeRate.EcbExchange
- **Key Changes**:
  - **Nop.Web.Framework**: MAJOR ASP.NET ? ASP.NET Core migration (1,794 issues)
  - Remove System.Web namespace (MVC, Routing, HttpContext)
  - Replace MVC 5 ? ASP.NET Core MVC
  - Update Filters ? Middleware
  - Update Routing ? Endpoint Routing
  - Remove OWIN, add ASP.NET Core Middleware
  - Update view rendering, HTML helpers
  - Replace WebActivator with app startup
  - Update authentication/authorization
- **Validation**: Web.Framework builds, framework tests pass, basic HTTP pipeline works
- **Blocker**: 21 projects (all web apps + plugins) blocked until complete

**Phase 5: Web Applications Tier (Tier 4 - Applications)**
- **Duration Estimate**: Relative complexity: CRITICAL
- **Projects**: Nop.Web (1 project), Nop.Admin (1 project)
- **Key Changes**:
  - **Nop.Web & Nop.Admin**: 13,322 combined issues
  - Convert Global.asax ? Program.cs + Startup.cs (Feature.1000)
  - Migrate route registration (Feature.0002)
  - Migrate global filters (Feature.0003)
  - Update EF initialization (Feature.0004)
  - Update static file handling
  - Create wwwroot folder structure
  - Update views, layouts, partial views
  - Update bundling/minification
  - Update configuration (web.config ? appsettings.json)
- **Validation**: Applications run, critical paths functional, admin area works
- **Blocker**: Nop.Web.MVC.Tests blocked until complete

**Phase 6: Plugins Tier (Tier 4 - Plugins)**
- **Duration Estimate**: Relative complexity: MEDIUM (batch processing)
- **Projects**: 18 plugin projects
- **Batching Strategy**:
  - **Batch 1 - Simple Plugins** (8 plugins, ~30-60 issues each): Process together
  - **Batch 2 - Medium Plugins** (7 plugins, ~70-120 issues each): Process together
  - **Batch 3 - Complex Plugins** (3 plugins, 120+ issues each): Individual attention
- **Key Changes**:
  - Apply patterns learned from Nop.Web.Framework
  - Update plugin base classes
  - Update view components
  - Update route registration
  - Update package references
- **Validation**: Each plugin loads, basic functionality works, no conflicts

**Phase 7: Testing Tier (Tier 5)**
- **Duration Estimate**: Relative complexity: MEDIUM
- **Projects**: Nop.Web.MVC.Tests (1 project)
- **Key Changes**:
  - Update integration tests (335 issues)
  - Update test infrastructure for ASP.NET Core
  - Update mocking (if using ASP.NET-specific mocks)
  - Update test data setup
- **Validation**: Full integration test suite passes

### Parallel vs Sequential Execution

**Sequential Required**:
- **Between Tiers**: Must complete Tier N before starting Tier N+1
- **Phase 4 (Web.Framework)**: Single project, blocking, critical
- **Phase 5 (Applications)**: Nop.Web and Nop.Admin depend on each other patterns, sequential recommended

**Parallel Possible**:
- **Phase 2**: Nop.Data and Nop.Tests can be upgraded in parallel (both depend only on Nop.Core)
- **Phase 3**: Nop.Services and test projects can be done in parallel
- **Phase 6 Plugin Batches**: Plugins within same batch can be upgraded in parallel
  - Batch 1: 8 simple plugins simultaneously
  - Batch 2: 7 medium plugins simultaneously
  - Batch 3: 3 complex plugins sequentially or max 2 in parallel

**Recommended Team Allocation**:
- **Phases 1-5**: Single-threaded (too complex for parallel)
- **Phase 6**: 2-3 developers working on different plugins simultaneously
- **Phase 7**: Single-threaded (integration tests)

### Breaking Changes Management

**Expected Breaking Change Categories**:

1. **System.Web Removal** (ASP.NET ? ASP.NET Core)
   - HttpContext changes
   - Session state changes
   - Cache API changes
   - Request/Response API changes

2. **Entity Framework 6 ? EF Core**
   - Mapping API changes
   - Query syntax updates
   - Migration file format
   - Lazy loading changes

3. **MVC 5 ? ASP.NET Core MVC**
   - Controller base class changes
   - ActionResult types
   - Filter attributes
   - View rendering

4. **Configuration System**
   - web.config ? appsettings.json
   - AppSettings ? IConfiguration
   - ConnectionStrings ? IConfiguration

5. **Package Replacements**
   - See Phase-specific package update tables below

### Rollback Strategy

**Per-Tier Rollback**:
- Each tier is committed separately
- Can rollback to previous tier if issues found
- Use Git branches per tier for isolation

**Rollback Triggers**:
- Tier cannot achieve stable state after 3 attempts
- Critical functionality broken beyond repair
- Performance degradation >50%
- Security vulnerabilities introduced
- Deadline constraints exceeded

**Rollback Process**:
1. Document specific blocking issue
2. Revert tier changes (Git reset)
3. Analyze root cause
4. Revise plan for tier
5. Retry or escalate

## Detailed Dependency Analysis

### Dependency Graph Overview

The solution has a clean 5-tier dependency structure with no circular dependencies. Projects must be upgraded bottom-up to maintain compatibility.

```
Tier 5: [Nop.Web.MVC.Tests]
          ?
Tier 4: [Nop.Web] [Nop.Admin] [18 Plugins]
          ?
Tier 3: [Nop.Web.Framework] [Nop.Services.Tests] [Nop.Plugin.ExchangeRate.EcbExchange]
          ?
Tier 2: [Nop.Services] [Nop.Core.Tests] [Nop.Data.Tests]
          ?
Tier 1: [Nop.Data] [Nop.Tests]
          ?
Tier 0: [Nop.Core]
```

### Migration Phases by Dependency Tier

#### Phase 0: Cleanup
**Projects**: Nop.CoreCore, Nop.DataCore, Nop.ServicesCore (3 empty projects)
- **Action**: Delete these placeholder projects created by VS Upgrade Assistant
- **Reason**: Empty shells with no code, create confusion, not needed for upgrade
- **Risk**: None - these are unused

#### Phase 1: Foundation Tier (Tier 0)
**Projects**: Nop.Core (1 project)
- **Why First**: Zero project dependencies, foundation for entire solution
- **Used By**: ALL 28 other projects depend on this
- **Issues**: 305 total (53 mandatory)
- **Complexity**: HIGH - core domain models, infrastructure, configuration
- **Risk**: HIGH - blocking project, affects everything downstream

#### Phase 2: Data Access Tier (Tier 1)
**Projects**: Nop.Data, Nop.Tests (2 projects)
- **Dependencies**: Nop.Core (must be upgraded first)
- **Used By**: 9 projects (Data), 4 projects (Tests)
- **Issues**: 10 (Data), 3 (Tests)
- **Complexity**: HIGH (Data) - Entity Framework 6 ? EF Core migration required
- **Complexity**: LOW (Tests) - test base classes only
- **Risk**: HIGH (Data) - database schema changes possible

#### Phase 3: Services & Testing Tier (Tier 2)
**Projects**: Nop.Services, Nop.Core.Tests, Nop.Data.Tests (3 projects)
- **Dependencies**: Nop.Core, Nop.Data, Nop.Tests
- **Used By**: 24 projects depend on Nop.Services
- **Issues**: 407 (Services), 36 (Core.Tests), 6 (Data.Tests)
- **Complexity**: HIGH (Services) - business logic, 189 mandatory issues
- **Complexity**: MEDIUM (Test projects)
- **Risk**: HIGH (Services) - critical business rules

#### Phase 4: Web Framework & Utilities Tier (Tier 3)
**Projects**: Nop.Web.Framework, Nop.Services.Tests, Nop.Plugin.ExchangeRate.EcbExchange (3 projects)
- **Dependencies**: Nop.Core, Nop.Data, Nop.Services
- **Used By**: 21 projects depend on Nop.Web.Framework
- **Issues**: 1,794 (Web.Framework), 20 (Services.Tests), 12 (EcbExchange)
- **Complexity**: CRITICAL (Web.Framework) - ASP.NET ? ASP.NET Core transition
- **Risk**: CRITICAL - affects all web apps and plugins

**Web.Framework Specifics**:
- MVC 5 ? ASP.NET Core MVC conversion
- Filter system migration
- View rendering changes
- Route registration updates
- Middleware pipeline setup

#### Phase 5: Web Applications Tier (Tier 4 - Applications Only)
**Projects**: Nop.Web, Nop.Admin (2 projects)
- **Dependencies**: Nop.Core, Nop.Data, Nop.Services, Nop.Web.Framework
- **Used By**: Nop.Web.MVC.Tests
- **Issues**: 3,997 (Nop.Web), 9,325 (Nop.Admin) - **13,322 combined**
- **Complexity**: CRITICAL - main applications, most issues
- **Risk**: CRITICAL - production applications

**Special Considerations**:
- Global.asax ? Program.cs/Startup.cs conversion (Feature.1000)
- RouteCollection ? Endpoint routing (Feature.0002)
- GlobalFilterCollection ? Middleware (Feature.0003)
- EntityFramework initialization changes (Feature.0004)
- Static file handling
- wwwroot folder structure

#### Phase 6: Plugins Tier (Tier 4 - Plugin Projects)
**Projects**: 18 plugin projects
- **Dependencies**: Nop.Core, Nop.Services, Nop.Web.Framework (some also Nop.Data)
- **Used By**: None (plugins are leaf nodes)
- **Issues**: Range from 32 to 199 per plugin
- **Complexity**: MEDIUM - similar patterns across plugins
- **Risk**: MEDIUM - isolated scope, but many plugins

**Plugin Groups by Complexity**:

**Group A - Simple Plugins** (4-7 files, ~30-60 issues):
  - Nop.Plugin.DiscountRules.CustomerRoles (46 issues)
  - Nop.Plugin.Shipping.AustraliaPost (34 issues)
  - Nop.Plugin.Shipping.CanadaPost (33 issues)
  - Nop.Plugin.Shipping.USPS (32 issues)
  - Nop.Plugin.Payments.CheckMoneyOrder (49 issues)
  - Nop.Plugin.Payments.PurchaseOrder (52 issues)
  - Nop.Plugin.Widgets.NivoSlider (60 issues)
  - Nop.Plugin.Widgets.GoogleAnalytics (68 issues)

**Group B - Medium Plugins** (8-12 files, ~70-120 issues):
  - Nop.Plugin.Shipping.UPS (71 issues)
  - Nop.Plugin.Feed.GoogleShopping (76 issues)
  - Nop.Plugin.Shipping.Fedex (86 issues)
  - Nop.Plugin.Payments.Manual (100 issues)
  - Nop.Plugin.Tax.FixedOrByCountryStateZip (101 issues)
  - Nop.Plugin.ExternalAuth.Facebook (104 issues, security vulns)
  - Nop.Plugin.DiscountRules.HasOneProduct (105 issues)
  - Nop.Plugin.Payments.PayPalStandard (120 issues)

**Group C - Complex Plugins** (12+ files, 120+ issues):
  - Nop.Plugin.Payments.PayPalDirect (143 issues, security vulns)
  - Nop.Plugin.Pickup.PickupInStore (144 issues)
  - Nop.Plugin.Shipping.FixedOrByWeight (199 issues)

#### Phase 7: Testing Tier (Tier 5)
**Projects**: Nop.Web.MVC.Tests (1 project)
- **Dependencies**: Nop.Core, Nop.Data, Nop.Services, Nop.Web.Framework, Nop.Admin, Nop.Web, Nop.Tests
- **Used By**: None (top-level test project)
- **Issues**: 335 (317 mandatory)
- **Complexity**: HIGH - integration tests across entire stack
- **Risk**: LOW - tests don't affect production, but validate entire upgrade

### Critical Upgrade Path

**Blocking Dependencies**:
1. **Nop.Core** blocks everything (28 projects)
2. **Nop.Data** blocks 9 projects
3. **Nop.Services** blocks 24 projects
4. **Nop.Web.Framework** blocks 21 projects

**Cannot proceed to next tier until current tier is:**
- ? Converted to SDK-style project format
- ? Targeting .NET 8.0
- ? All packages updated/replaced
- ? Compilation successful (zero errors)
- ? Unit tests passing
- ? No security vulnerabilities

### Risk Assessment by Tier

| Tier | Risk Level | Reasoning |
|------|-----------|-----------|
| **Phase 0 (Cleanup)** | None | Deleting empty projects |
| **Phase 1 (Core)** | HIGH | Foundation, blocks all others, 305 issues |
| **Phase 2 (Data)** | HIGH | EF6?EF Core migration, schema changes |
| **Phase 3 (Services)** | HIGH | Business logic, 407 issues, 189 mandatory |
| **Phase 4 (Web.Framework)** | CRITICAL | ASP.NET Core transition, 1,794 issues |
| **Phase 5 (Applications)** | CRITICAL | 13K+ issues combined, production apps |
| **Phase 6 (Plugins)** | MEDIUM | Isolated, repetitive patterns |
| **Phase 7 (Tests)** | LOW | Validation only, non-blocking |

## Project-by-Project Plans

### Phase 0: Cleanup (Empty Placeholder Projects)

#### Projects to Delete
- Nop.CoreCore.csproj
- Nop.DataCore.csproj
- Nop.ServicesCore.csproj

**Actions**:
1. Remove projects from solution file
2. Delete project directories
3. Verify solution builds without them

---

### Phase 1: Foundation Tier - Nop.Core

**Project Path**: `Libraries\Nop.Core\Nop.Core.csproj`

#### Current State
- **Current Framework**: .NET Framework 4.5.1
- **Project Type**: Classic Class Library (non-SDK-style)
- **Total Files**: 290 source files
- **Dependencies**: 0 internal project dependencies (foundation library)
- **Dependents**: ALL 28 other projects depend on this
- **Package Count**: 10 NuGet packages
- **Lines of Code**: ~15,000
- **Risk Level**: HIGH - Foundation library, blocks entire solution

#### Target State
- **Target Framework**: .NET 8.0
- **Project Type**: SDK-style Class Library
- **Package Updates**: 3 packages to replace, 1 to update, 4 to remove
- **Expected Changes**: 305 issues (53 mandatory, 250 potential, 2 optional)

#### Issues Breakdown

| Category | Mandatory | Potential | Optional | Total |
|----------|-----------|-----------|----------|-------|
| **NuGet Packages** | 6 | 1 | 2 | 9 |
| **Project Structure** | 2 | 0 | 0 | 2 |
| **API Incompatibility** | 45 | 249 | 0 | 294 |
| **TOTAL** | 53 | 250 | 2 | 305 |

**Affected Technologies**:
- ASP.NET Framework (System.Web): 250 issues - **Major migration effort**
- Legacy Configuration System: 19 issues
- Code Access Security (CAS): 1 issue
- Legacy Cryptography: 1 issue

#### Migration Steps

##### 1. Prerequisites
- [x] .NET 8 SDK installed
- [x] Visual Studio 2022 17.8+ or later
- [x] Backup created
- [x] All dependent projects identified (28 projects)

##### 2. Project File Conversion (SDK-Style)

**Current project file structure** (verbose .NET Framework format):
- ~200 lines of XML
- Manual file references
- Explicit package references with HintPath
- Build configurations in project file

**Target SDK-style structure**:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>Nop.Core</RootNamespace>
    <AssemblyName>Nop.Core</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <!-- Package references only -->
  </ItemGroup>
</Project>
```

**Actions**:
- Convert project file using .NET Upgrade Assistant or manual rewrite
- Remove all `<Compile>` elements (SDK-style includes all `.cs` files automatically)
- Remove all `<Reference>` elements for System assemblies
- Simplify to essential elements only

##### 3. Package Updates

| Package | Current Version | Action | Target Version / Replacement | Reason |
|---------|----------------|--------|------------------------------|---------|
| **Autofac** | 4.4.0 | Keep/Update | 8.0.0 | Compatible, update recommended |
| **Autofac.Mvc5** | 4.0.1 | Replace | Autofac.Extensions.DependencyInjection | ASP.NET Core uses different integration |
| **AutoMapper** | 5.2.0 | Keep/Update | 13.0.1 | Compatible, update recommended |
| **Microsoft.AspNet.Mvc** | 5.2.3 | Remove | Framework reference | Included in ASP.NET Core |
| **Microsoft.AspNet.Razor** | 3.2.3 | Remove | Framework reference | Included in ASP.NET Core |
| **Microsoft.AspNet.WebPages** | 3.2.3 | Remove | Framework reference | Included in ASP.NET Core |
| **Microsoft.Web.Infrastructure** | 1.0.0.0 | Remove | Framework reference | Included in ASP.NET Core |
| **Newtonsoft.Json** | 9.0.1 | **Update** | **13.0.4** | **Security vulnerability** |
| **RedLock.net.StrongName** | 1.7.4 | Replace | RedLock.net 2.3.2 | .NET 8 compatible version |
| **StackExchange.Redis.StrongName** | 1.2.1 | Replace | StackExchange.Redis 2.8.0 | Deprecated, update to non-StrongName |

**Security Critical**: Newtonsoft.Json 9.0.1 has known vulnerabilities - **must update to 13.0.4+**

##### 4. Expected Breaking Changes

**Category 1: System.Web Removal (250 issues)**

The biggest challenge - System.Web namespace doesn't exist in .NET Core.

**Common replacements**:

| .NET Framework (System.Web) | .NET Core / .NET 8 Replacement |
|-----------------------------|--------------------------------|
| `HttpContext.Current` | `IHttpContextAccessor.HttpContext` |
| `HttpContext.Request` | `HttpContext.Request` (different namespace) |
| `HttpContext.Response` | `HttpContext.Response` (different namespace) |
| `HttpContext.Server` | `IWebHostEnvironment` |
| `HttpContext.Session` | `ISession` (different API) |
| `HttpContext.Cache` | `IMemoryCache` or `IDistributedCache` |
| `HttpUtility.UrlEncode` | `WebUtility.UrlEncode` or `Uri.EscapeDataString` |
| `HttpUtility.HtmlEncode` | `WebUtility.HtmlEncode` or `HtmlEncoder` |
| `HttpPostedFileBase` | `IFormFile` |
| `HttpServerUtilityBase` | Various replacements |

**Affected Areas** (based on assessment):
- HTTP context access throughout codebase
- Request/response manipulation
- Session state management
- Caching mechanisms
- URL/HTML encoding utilities
- File upload handling

**Strategy**:
1. Identify all System.Web usages (250 issues flagged)
2. Replace with ASP.NET Core equivalents incrementally
3. Consider creating adapter/wrapper classes for gradual migration
4. Some features may need architectural changes

**Category 2: Configuration System (19 issues)**

| .NET Framework | .NET 8 Replacement |
|----------------|-------------------|
| `ConfigurationManager.AppSettings` | `IConfiguration["key"]` |
| `ConfigurationManager.ConnectionStrings` | `IConfiguration.GetConnectionString()` |
| `web.config` / `app.config` | `appsettings.json` |

**Category 3: Code Access Security (CAS) (1 issue)**

CAS is not supported in .NET Core. Code using CAS attributes/APIs must be removed or refactored.

**Category 4: Cryptography (1 issue)**

Some legacy cryptography APIs changed. Review and update to modern equivalents.

**Category 5: API Binary Incompatibility (45 mandatory issues)**

APIs that have different signatures in .NET 8:
- Return type changes
- Parameter changes
- Removed overloads
- Obsolete methods

**Strategy**: Fix compilation errors one by one, guided by compiler messages.

##### 5. Code Modifications

**Step-by-Step Approach**:

1. **First Pass - Get it to Compile**:
   - Update project file to SDK-style + net8.0
   - Update/remove packages
   - Fix compiler errors (focus on mandatory issues first)
   - Don't worry about warnings yet

2. **Second Pass - Remove System.Web**:
   - Systematically replace System.Web usage
   - May need to add `Microsoft.AspNetCore.Http.Abstractions` package for `IHttpContextAccessor`
   - Create adapter patterns if needed for gradual migration
   - Test each major change

3. **Third Pass - Configuration Migration**:
   - Replace ConfigurationManager usage with IConfiguration
   - Note: Actual configuration files (appsettings.json) will be in web projects
   - Core library should use IConfiguration abstractions

4. **Fourth Pass - Address Warnings**:
   - Fix potential issues (250 warnings)
   - Update obsolete API usage
   - Address code quality warnings

5. **Fifth Pass - Optimization**:
   - Apply .NET 8-specific improvements
   - Use modern C# features where appropriate
   - Performance optimizations

##### 6. Testing Strategy

**Unit Tests** (will upgrade Nop.Core.Tests in Phase 3):
- Tests will initially fail as they target .NET Framework
- Tests will be upgraded after Nop.Core is stable
- Interim validation: ensure Nop.Core builds

**Integration Validation**:
- Create simple test project targeting .NET 8
- Reference Nop.Core
- Verify core types instantiate
- Verify configuration loads
- Verify caching works
- Verify plugin infrastructure initializes

**Manual Verification**:
- Review each major component:
  - Domain models (Product, Customer, Order, etc.)
  - Configuration system
  - Caching infrastructure
  - Plugin loading mechanism
  - Event system
  - Infrastructure services

##### 7. Validation Checklist

- [ ] Project converted to SDK-style successfully
- [ ] TargetFramework set to net8.0
- [ ] All incompatible packages removed (Mvc5, Web.Infrastructure, etc.)
- [ ] Security vulnerability fixed (Newtonsoft.Json updated to 13.0.4+)
- [ ] All deprecated packages replaced (Redis, RedLock)
- [ ] Project builds without errors
- [ ] No System.Web references remain (or wrapped in abstractions)
- [ ] Configuration system using IConfiguration
- [ ] No CAS (Code Access Security) usage
- [ ] Legacy cryptography updated
- [ ] Build warnings addressed or documented
- [ ] Core functionality verified (configuration, caching, events)
- [ ] Plugin infrastructure validated
- [ ] NuGet vulnerability scan passes
- [ ] Changes committed to version control

#### Estimated Complexity
**Relative Effort**: HIGH

**Breakdown**:
- Project conversion: LOW effort
- Package updates: MEDIUM effort (some replacements needed)
- System.Web removal: **HIGH effort** (250 issues, major refactoring)
- Configuration migration: MEDIUM effort
- Testing & validation: MEDIUM effort

**Critical Success Factor**: Successfully removing System.Web dependencies while maintaining functionality for 28 dependent projects.

#### Known Risks
- ?? **Blocks entire solution** - If this fails, nothing else can proceed
- ?? **System.Web removal** - 250 issues require careful refactoring
- ?? **Breaking changes for dependents** - Changes here ripple to all 28 projects
- ?? **Configuration system** - Needs to support both old and new patterns temporarily

#### Mitigation Strategies
- Create abstraction layer for System.Web dependencies (e.g., `IHttpContextAccessor`)
- Use feature flags or conditional compilation during transition
- Extensive documentation of breaking changes for dependent projects
- Consider creating Nop.Core v2 alongside v1 initially for comparison

#### Dependencies
**Required Before Starting**: None (foundation tier)

**Blocks Until Complete**: ALL other projects (28 projects waiting)

#### Success Criteria
- ? Nop.Core builds on .NET 8.0 without errors
- ? All security vulnerabilities resolved
- ? System.Web dependencies removed or abstracted
- ? Configuration system migrated
- ? Core infrastructure functional (validated through test project)
- ? Breaking changes documented for dependent projects
- ? Committed to version control with clear commit messages

---

### Phase 2: Data Access Tier

#### Nop.Data

**Project Path**: `Libraries\Nop.Data\Nop.Data.csproj`

##### Current State
- **Current Framework**: .NET Framework 4.5.1
- **Project Type**: Classic Class Library
- **Total Files**: 123 source files (entity mappings, DbContext, initializers)
- **Dependencies**: Nop.Core (must be upgraded first)
- **Dependents**: 9 projects (Nop.Services, Nop.Web, Nop.Web.Framework, Nop.Admin, 4 plugins, Nop.Data.Tests)
- **Package Count**: 3 NuGet packages
- **Lines of Code**: ~8,000
- **Risk Level**: HIGH - Entity Framework 6 ? EF Core 8 migration

##### Target State
- **Target Framework**: .NET 8.0
- **Project Type**: SDK-style Class Library
- **ORM**: Entity Framework Core 8.0 (from EF6)
- **Database Providers**: SQL Server, potentially others
- **Expected Changes**: 10 issues (4 mandatory, 5 potential, 1 optional)

##### Issues Breakdown

| Category | Count | Description |
|----------|-------|-------------|
| **NuGet Packages** | 4 | EF6 ? EF Core, remove SQL Compact |
| **Project Structure** | 2 | SDK-style conversion, target framework |
| **API Incompatibility** | 4 | EF6 API changes |
| **TOTAL** | 10 | Relatively low count, but high complexity |

##### Migration Steps

###### 1. Prerequisites
- [x] Nop.Core upgraded to .NET 8 (Phase 1 complete)
- [x] .NET 8 SDK installed
- [x] Database backup created
- [x] Database connection available for testing

###### 2. Project File Conversion

Convert to SDK-style format:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <RootNamespace>Nop.Data</RootNamespace>
    <AssemblyName>Nop.Data</AssemblyName>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\Nop.Core\Nop.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- EF Core packages -->
  </ItemGroup>
</Project>
```

###### 3. Package Updates - Entity Framework 6 to EF Core 8

| Package | Current Version | Action | Target / Replacement | Reason |
|---------|----------------|--------|---------------------|---------|
| **EntityFramework** | 6.1.3 | Replace | **Microsoft.EntityFrameworkCore** 8.0.x | .NET Core uses EF Core |
| | | Add | **Microsoft.EntityFrameworkCore.SqlServer** 8.0.x | SQL Server provider |
| | | Add | **Microsoft.EntityFrameworkCore.Tools** 8.0.x | Migrations tooling |
| | | Add | **Microsoft.EntityFrameworkCore.Design** 8.0.x | Design-time support |
| **EntityFramework.SqlServerCompact** | 6.1.3 | Remove | N/A | SQL Server Compact not supported in .NET Core |
| **Microsoft.SqlServer.Compact** | 4.0.8876.1 | Remove | N/A | SQL Server Compact not supported in .NET Core |

**SQL Server Compact Note**: If solution relies on SQL Compact, must migrate to SQL Server, SQLite, or another supported database.

###### 4. Entity Framework 6 to EF Core Migration

**Major Conceptual Changes**:

| EF6 Concept | EF Core Equivalent | Notes |
|-------------|-------------------|--------|
| `DbModelBuilder` | `ModelBuilder` | Fluent API mostly similar |
| `EntityTypeConfiguration<T>` | `IEntityTypeConfiguration<T>` | Different interface |
| `HasRequired/HasOptional` | `HasOne` + `IsRequired()` | Clearer relationship API |
| `HasMany().WithRequired/Optional` | `HasMany().WithOne()` | Simplified |
| `WillCascadeOnDelete` | `OnDelete(DeleteBehavior)` | Explicit behavior |
| `ToTable` | `ToTable` | Same API |
| `HasKey` | `HasKey` | Same API |
| `Property().HasColumnName` | `Property().HasColumnName` | Same API |

**Migration Tasks**:

**A. DbContext Changes**

Current EF6 pattern:
```csharp
public class NopObjectContext : DbContext
{
    public NopObjectContext(string nameOrConnectionString) 
        : base(nameOrConnectionString) { }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        modelBuilder.Configurations.Add(new ProductMap());
        // ... many more
    }
}
```

Target EF Core pattern:
```csharp
public class NopObjectContext : DbContext
{
    public NopObjectContext(DbContextOptions<NopObjectContext> options) 
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductMap());
        // Or: modelBuilder.ApplyConfigurationsFromAssembly(typeof(NopObjectContext).Assembly);
    }
}
```

**Key Changes**:
- Constructor takes `DbContextOptions<T>` instead of connection string
- `DbModelBuilder` ? `ModelBuilder`
- `modelBuilder.Configurations.Add()` ? `modelBuilder.ApplyConfiguration()`
- Dependency injection pattern required

**B. Entity Mapping Classes**

Current EF6 mapping:
```csharp
public class ProductMap : EntityTypeConfiguration<Product>
{
    public ProductMap()
    {
        this.ToTable("Product");
        this.HasKey(p => p.Id);
        this.Property(p => p.Name).HasMaxLength(400).IsRequired();
        this.HasRequired(p => p.ProductType)
            .WithMany()
            .HasForeignKey(p => p.ProductTypeId);
    }
}
```

Target EF Core mapping:
```csharp
public class ProductMap : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(400).IsRequired();
        builder.HasOne(p => p.ProductType)
               .WithMany()
               .HasForeignKey(p => p.ProductTypeId)
               .IsRequired();
    }
}
```

**Changes Per Mapping** (~100+ mapping classes):
- Inherit from `IEntityTypeConfiguration<T>` instead of `EntityTypeConfiguration<T>`
- Implement `Configure(EntityTypeBuilder<T> builder)` method
- Move configuration from constructor to `Configure` method
- Update relationship API: `HasRequired` ? `HasOne().IsRequired()`
- Update relationship API: `HasOptional` ? `HasOne()`
- Update relationship API: `WillCascadeOnDelete` ? `OnDelete(DeleteBehavior.Cascade)`

**C. Database Initializers**

EF6 had `CreateDatabaseIfNotExists`, `DropCreateDatabaseAlways`, etc.

EF Core uses **Migrations** only. Remove custom initializers:
- `CreateCeDatabaseIfNotExists.cs` - Remove (SQL Compact)
- `DropCreateCeDatabaseAlways.cs` - Remove (SQL Compact)
- `DropCreateCeDatabaseIfModelChanges.cs` - Remove (SQL Compact)
- `SqlCeInitializer.cs` - Remove (SQL Compact)
- `CreateTablesIfNotExist.cs` - Replace with Migrations

**D. Query Extensions & Helpers**

- `QueryableExtensions.cs` - Review for EF Core compatibility
- `DbContextExtensions.cs` - Update for EF Core API
- `DataReaderExtensions.cs` - Should be compatible
- `Extensions.cs` - Review and update

**E. EF Data Provider Manager**

`EfDataProviderManager.cs` - May need updates for EF Core provider model.

###### 5. Database Migration Strategy

**Option 1: Generate Fresh Migrations (Recommended)**
1. Delete existing EF6 migrations (if any)
2. Create initial migration: `Add-Migration InitialCreate`
3. Apply to test database: `Update-Database`
4. Verify schema matches production

**Option 2: Migrate Existing Data**
1. Backup production database
2. Apply EF Core migrations to copy of database
3. Test data integrity
4. Validate queries

**SQL Server Compact Removal**:
- If currently using SQL Compact: **Must migrate data to SQL Server or SQLite**
- Export data from SQL Compact
- Import to new database
- Update connection strings

###### 6. Code Modifications

**Step-by-Step**:

1. **Update project file** (SDK-style, net8.0)
2. **Replace EF6 packages with EF Core packages**
3. **Update DbContext**:
   - Change constructor
   - Update `OnModelCreating`
   - Register with dependency injection
4. **Update ALL mapping classes** (100+ files):
   - Change base class/interface
   - Update relationship APIs
   - Update cascade delete syntax
5. **Remove SQL Compact initializers**
6. **Update query extensions** for EF Core
7. **Create/update migrations**
8. **Test database connectivity**

###### 7. Testing Strategy

- [ ] DbContext initializes successfully
- [ ] All entity mappings load without errors
- [ ] Database connection established
- [ ] Migrations apply successfully (test database)
- [ ] CRUD operations work for sample entities
- [ ] Queries return correct results
- [ ] Relationships load correctly (Include, ThenInclude)
- [ ] Transactions work
- [ ] Connection pooling functional
- [ ] Run Nop.Data.Tests (after upgrading in Phase 3)

###### 8. Validation Checklist

- [ ] Project converted to SDK-style
- [ ] TargetFramework = net8.0
- [ ] EntityFramework 6.x removed
- [ ] EF Core 8.x packages installed
- [ ] SQL Server Compact packages removed
- [ ] DbContext updated for EF Core
- [ ] All 100+ mapping classes updated
- [ ] Database initializers removed/replaced
- [ ] Migrations created
- [ ] Database connection tested
- [ ] Basic CRUD operations validated
- [ ] Project builds without errors
- [ ] No EF6 references remain

##### Estimated Complexity
**Relative Effort**: HIGH

- Project conversion: LOW
- Package replacement: MEDIUM
- DbContext updates: MEDIUM
- Mapping class updates: **HIGH** (100+ files to update)
- Testing & validation: MEDIUM-HIGH (database-dependent)

##### Known Risks
- ?? **SQL Server Compact Migration** - If used, requires data migration
- ?? **Schema Changes** - EF Core may generate different schema
- ?? **Query Behavior Differences** - Some queries may behave differently
- ?? **100+ Mapping Files** - Labor-intensive updates
- ?? **Blocks 9 Projects** - Services, Web apps, plugins depend on this

##### Mitigation Strategies
- Test migrations on database copy first
- Compare EF6 vs EF Core generated SQL for critical queries
- Incremental validation of mapping classes
- Keep database backup
- Performance test critical queries

##### Dependencies
**Required Before Starting**: Nop.Core upgraded (Phase 1)

**Blocks Until Complete**: 9 projects (Nop.Services, web apps, plugins, tests)

##### Success Criteria
- ? Nop.Data builds on .NET 8.0
- ? EF Core 8.x successfully replaces EF6
- ? All entity mappings converted
- ? Database migrations work
- ? CRUD operations functional
- ? No SQL Compact dependencies
- ? Changes committed with clear messages

---

#### Nop.Tests

**Project Path**: `Tests\Nop.Tests\Nop.Tests.csproj`

##### Current State
- **Current Framework**: .NET Framework 4.5.1
- **Project Type**: Classic Class Library
- **Total Files**: 6 source files (test base classes, utilities)
- **Dependencies**: Nop.Core
- **Dependents**: 4 test projects (Nop.Core.Tests, Nop.Data.Tests, Nop.Services.Tests, Nop.Web.MVC.Tests)
- **Package Count**: 2 NuGet packages (NUnit, RhinoMocks)
- **Lines of Code**: ~1,000
- **Risk Level**: LOW - Simple test infrastructure project

##### Target State
- **Target Framework**: .NET 8.0
- **Project Type**: SDK-style Class Library (Test project)
- **Test Framework**: NUnit 4.x (or keep 3.x if compatible)
- **Mocking**: Replace RhinoMocks with modern alternative (Moq/NSubstitute)
- **Expected Changes**: 3 issues (all mandatory, project-level only)

##### Issues Breakdown

| Category | Count |
|----------|-------|
| **NuGet Packages** | 1 (RhinoMocks incompatible) |
| **Project Structure** | 2 (SDK-style, target framework) |
| **TOTAL** | 3 |

**Note**: Very few issues - straightforward upgrade.

##### Migration Steps

###### 1. Prerequisites
- [x] Nop.Core upgraded (Phase 1)
- [x] .NET 8 SDK installed

###### 2. Project File Conversion

Convert to SDK-style test project:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Libraries\Nop.Core\Nop.Core.csproj" />
  </ItemGroup>

  <ItemGroup>
    <!-- Test packages -->
  </ItemGroup>
</Project>
```

###### 3. Package Updates

| Package | Current Version | Action | Target / Replacement | Reason |
|---------|----------------|--------|---------------------|---------|
| **NUnit** | 3.6.1 | Update | **NUnit** 4.2.2 or keep 3.14.0 | Compatible with .NET 8 |
| | | Add | **NUnit3TestAdapter** 4.x | Test adapter for Visual Studio |
| | | Add | **Microsoft.NET.Test.Sdk** 17.x | Test SDK |
| **RhinoMocks** | 3.6.1 | Replace | **Moq** 4.20.x or **NSubstitute** 5.x | RhinoMocks not maintained, incompatible |

**Recommendation**: Use **Moq** - most popular, widely supported, good .NET 8 support.

###### 4. Code Modifications

**Minimal changes needed** - this project provides base classes for tests:

1. **Update project file** to SDK-style
2. **Replace RhinoMocks with Moq**:
   - Update using statements
   - Update mock creation syntax
   - Update verification syntax

**RhinoMocks ? Moq Migration**:

Old RhinoMocks:
```csharp
var mockRepository = new MockRepository();
var mockService = mockRepository.StrictMock<IService>();
mockService.Expect(x => x.DoSomething()).Return(true);
mockRepository.ReplayAll();
// ... test code ...
mockRepository.VerifyAll();
```

New Moq:
```csharp
var mockService = new Mock<IService>();
mockService.Setup(x => x.DoSomething()).Returns(true);
// ... test code ...
mockService.Verify(x => x.DoSomething(), Times.Once);
```

**Files to Update** (6 total - review each):
- Base test classes
- Test utility classes
- Mock setup helpers

3. **Update NUnit attributes** (if changed in NUnit 4.x)

###### 5. Testing Strategy

- [ ] Project builds successfully
- [ ] Test base classes compile
- [ ] Mock utilities work with Moq
- [ ] No dependencies on RhinoMocks remain
- [ ] Dependent test projects can reference this (after their upgrades)

###### 6. Validation Checklist

- [ ] Project converted to SDK-style
- [ ] TargetFramework = net8.0
- [ ] RhinoMocks removed
- [ ] Moq or NSubstitute added
- [ ] NUnit updated (3.x or 4.x)
- [ ] Test adapter packages added
- [ ] All test helper code updated
- [ ] Project builds without errors
- [ ] Test infrastructure validated

##### Estimated Complexity
**Relative Effort**: LOW

- Project conversion: LOW
- Package replacement: LOW-MEDIUM (RhinoMocks ? Moq)
- Code updates: LOW (6 files, mostly helpers)
- Testing: LOW

##### Known Risks
- ?? **Mock framework change** - Different syntax may require learning curve
- ?? **Dependent test projects** - Must wait for this before upgrading their tests

##### Mitigation Strategies
- Moq documentation readily available
- Moq widely used, plenty of examples
- Small codebase, easy to validate

##### Dependencies
**Required Before Starting**: Nop.Core upgraded (Phase 1)

**Blocks Until Complete**: 4 test projects that depend on base test classes

##### Success Criteria
- ? Nop.Tests builds on .NET 8.0
- ? RhinoMocks successfully replaced with Moq
- ? Test base classes functional
- ? NUnit 3.x/4.x installed and working
- ? Changes committed

---

### Phase 3: Services & Testing Tier

#### Nop.Services

**Project Path**: `Libraries\Nop.Services\Nop.Services.csproj`

**Summary**:
- **Issues**: 407 total (189 mandatory, 210 potential, 8 optional)
- **Files**: 320 source files
- **Complexity**: HIGH - Business logic layer with extensive external integrations
- **Dependencies**: Nop.Core, Nop.Data
- **Blocks**: 24 projects

**Key Challenges**:
- 353 System.Web issues (similar to Nop.Core)
- Azure Storage migration (WindowsAzure.Storage ? Azure.Storage.Blobs)
- Image processing (ImageResizer incompatible, find alternative)
- EPPlus deprecated (replace with EPPlus 5.x or ClosedXML)
- System.Linq.Dynamic incompatible (replace with System.Linq.Dynamic.Core)
- Security vulnerabilities (Newtonsoft.Json, Azure packages)

**Critical Package Updates**:
| Package | Action | Replacement |
|---------|--------|-------------|
| WindowsAzure.Storage 8.1.1 | Replace | Azure.Storage.Blobs 12.x |
| Microsoft.Azure.KeyVault.Core 2.0.4 | Replace | Azure.Security.KeyVault.Secrets |
| EPPlus 4.1.0 | Replace | EPPlus 5.x (commercial) or ClosedXML |
| ImageResizer 4.0.5 | Replace | ImageSharp, SkiaSharp, or SixLabors.ImageSharp |
| System.Linq.Dynamic 1.0.7 | Replace | System.Linq.Dynamic.Core 1.x |
| Newtonsoft.Json 9.0.1 | **Update** | **13.0.4** (security) |

**Migration Approach**:
1. Apply Nop.Core System.Web removal patterns
2. Replace Azure packages (breaking API changes expected)
3. Replace image processing library (may require code rewrite)
4. Replace Excel library (EPPlus licensing changed, or use ClosedXML)
5. Update dynamic LINQ library
6. Fix business logic compilation errors
7. Extensive service-level testing

---

#### Nop.Core.Tests

**Summary**:
- **Issues**: 36 (7 mandatory, 29 potential)
- **Files**: ~50 test files
- **Complexity**: MEDIUM
- **Dependencies**: Nop.Core, Nop.Tests

**Migration**: Straightforward test project upgrade following Nop.Tests pattern (Moq, NUnit 4.x)

---

#### Nop.Data.Tests

**Summary**:
- **Issues**: 6 (4 mandatory, 2 potential)
- **Files**: ~30 test files
- **Complexity**: LOW
- **Dependencies**: Nop.Core, Nop.Data, Nop.Tests

**Migration**: Simple test project, primarily EF Core query tests after Nop.Data migration complete.

---

### Phase 4: Web Framework & Utilities Tier

#### Nop.Web.Framework

**Project Path**: `Presentation\Nop.Web.Framework\Nop.Web.Framework.csproj`

**?? CRITICAL PROJECT - Highest Complexity**

**Summary**:
- **Issues**: **1,794 total** (1,394 mandatory, 395 potential, 5 optional)
- **Files**: 104 source files
- **Complexity**: **CRITICAL** - ASP.NET Framework ? ASP.NET Core migration
- **Dependencies**: Nop.Core, Nop.Data, Nop.Services
- **Blocks**: 21 projects (all web apps and plugins)
- **Risk**: CRITICAL - Foundation for all web functionality

**Affected Technologies**:
- **ASP.NET Framework (System.Web)**: 1,747 issues - **Massive migration effort**
- **WCF Client APIs**: 24 issues
- **Legacy Cryptography**: 1 issue
- **Windows ACLs**: 1 issue

**This is THE MOST COMPLEX PROJECT in the entire upgrade**.

**Major Migration Areas**:

**1. HTTP Pipeline (System.Web ? ASP.NET Core)**

| ASP.NET MVC 5 Concept | ASP.NET Core Equivalent | Effort |
|-----------------------|------------------------|--------|
| Global.asax | Program.cs + Startup.cs | HIGH |
| Application_Start | Configure() in Startup | HIGH |
| RouteCollection | IEndpointRouteBuilder | HIGH |
| GlobalFilterCollection | Middleware pipeline | HIGH |
| HttpContext.Current | IHttpContextAccessor | MEDIUM |
| HttpModules | Middleware | HIGH |
| HttpHandlers | Middleware or Endpoint | MEDIUM |
| ActionFilter | IActionFilter | MEDIUM |
| AuthorizeAttribute | AuthorizeAttribute (diff namespace) | LOW |
| ViewEngine | Razor view engine (similar) | LOW |
| HtmlHelper | IHtmlHelper | MEDIUM |
| UrlHelper | IUrlHelper | MEDIUM |

**2. Package Replacements**:

| Current Package | Replacement | Notes |
|----------------|-------------|-------|
| Autofac.Mvc5 4.0.1 | Autofac.Extensions.DependencyInjection | Different integration pattern |
| Microsoft.AspNet.Mvc 5.2.3 | Framework (ASP.NET Core MVC) | Included |
| Microsoft.AspNet.Web.Optimization 1.1.3 | WebOptimizer.Core or built-in | Bundling/minification |
| WebActivator 1.5.2 | Remove | Use Startup.cs |
| WebGrease 1.6.0 | Remove | Use modern bundlers |
| System.Linq.Dynamic 1.0.7 | System.Linq.Dynamic.Core | .NET Core compatible |
| Newtonsoft.Json 9.0.1 | **13.0.4** | Security |

**3. Critical Code Changes Needed**:

**Routing Migration**:
```csharp
// OLD: ASP.NET MVC 5
routes.MapRoute(
    name: "Default",
    url: "{controller}/{action}/{id}",
    defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
);

// NEW: ASP.NET Core
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

**Filter Migration**:
```csharp
// OLD: Global filters
GlobalFilters.Filters.Add(new HandleErrorAttribute());

// NEW: Middleware or MVC options
services.AddControllersWithViews(options =>
{
    options.Filters.Add<ExceptionHandlerFilter>();
});
```

**Dependency Injection**:
```csharp
// OLD: Autofac in Global.asax
var builder = new ContainerBuilder();
builder.RegisterControllers(typeof(MvcApplication).Assembly);
var container = builder.Build();
DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

// NEW: Startup.cs ConfigureServices
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    // Use IServiceCollection or integrate Autofac
    services.AddAutofac();
}
```

**4. View Rendering**:
Most Razor syntax compatible, but:
- HTML Helpers changed (still available but different namespace)
- Tag Helpers recommended (new ASP.NET Core feature)
- Layout system mostly compatible

**5. Authentication & Authorization**:
- ASP.NET Identity ? ASP.NET Core Identity (different APIs)
- Cookie authentication different APIs
- OAuth/OpenID Connect updated

**Migration Strategy**:
1. **Prototype First**: Create small ASP.NET Core MVC project to validate migration patterns
2. **Incremental Conversion**: Convert features in phases:
   - Phase 4a: Project structure, packages
   - Phase 4b: Routing system
   - Phase 4c: Filters ? Middleware
   - Phase 4d: Dependency injection
   - Phase 4e: View rendering
   - Phase 4f: Authentication/authorization
3. **Document Patterns**: Create migration guide for downstream projects (apps/plugins)
4. **Extensive Testing**: Test framework features independently before integrating

**Success Criteria**:
- ? Basic ASP.NET Core MVC pipeline works
- ? Routing functional
- ? Middleware replaces global filters
- ? Dependency injection works
- ? Views render
- ? Authentication functional
- ? Patterns documented for plugins/apps

---

#### Nop.Services.Tests

**Summary**: 20 issues, medium complexity test project. Follow standard test upgrade pattern.

---

#### Nop.Plugin.ExchangeRate.EcbExchange

**Summary**: 12 issues, simple plugin making API calls. Apply Web.Framework patterns once established.

---

### Phase 5: Web Applications Tier

#### Nop.Web

**Project Path**: `Presentation\Nop.Web\Nop.Web.csproj`

**?? CRITICAL - Main Storefront Application**

**Summary**:
- **Issues**: **3,997 total** (3,701 mandatory, 290 potential, 6 optional)
- **Files**: ~200+ files (controllers, views, scripts, styles)
- **Complexity**: **CRITICAL**
- **Special Features**: Global.asax conversion, route registration, filter migration, EF initialization

**Key Migration Tasks**:
1. **Global.asax.cs ? Program.cs + Startup.cs** (Feature.1000)
2. **RouteCollection ? Endpoint Routing** (Feature.0002)
3. **GlobalFilterCollection ? Middleware** (Feature.0003)
4. **EF Initialization Changes** (Feature.0004)
5. Apply all Nop.Web.Framework patterns
6. Convert all controllers (apply framework patterns)
7. Update all views (minimal changes expected)
8. Create wwwroot folder for static files
9. Update bundling/minification (web.config ? code-based)
10. Migrate configuration (web.config ? appsettings.json)

**Testing Priority**: Customer-facing workflows must work perfectly.

---

#### Nop.Admin

**Project Path**: `Presentation\Nop.Web\Administration\Nop.Admin.csproj`

**?? CRITICAL - Highest Issue Count (9,325)**

**Summary**:
- **Issues**: **9,325 total** (8,791 mandatory)
- **Complexity**: **CRITICAL** - Largest single project migration
- **Similar to Nop.Web but larger**

**Strategy**: Apply same patterns as Nop.Web but expect significantly more effort due to sheer volume. Admin interface has many controllers, views, complex forms.

---

### Phase 6: Plugins Tier

All 18 plugins follow similar pattern: Apply Nop.Web.Framework migration patterns established in Phase 4.

#### Plugin Batch 1 - Simple Plugins (8 plugins, ~30-60 issues each)

**Projects**:
- Nop.Plugin.DiscountRules.CustomerRoles (46 issues)
- Nop.Plugin.Shipping.AustraliaPost (34 issues)
- Nop.Plugin.Shipping.CanadaPost (33 issues)
- Nop.Plugin.Shipping.USPS (32 issues)
- Nop.Plugin.Payments.CheckMoneyOrder (49 issues)
- Nop.Plugin.Payments.PurchaseOrder (52 issues)
- Nop.Plugin.Widgets.NivoSlider (60 issues)
- Nop.Plugin.Widgets.GoogleAnalytics (68 issues)

**Common Pattern**:
- Convert project to SDK-style
- Apply Web.Framework routing/controller patterns
- Update packages
- Fix System.Web references
- Test plugin loads and basic functionality

**Can be done in parallel** (2-3 developers working simultaneously).

---

#### Plugin Batch 2 - Medium Plugins (7 plugins, ~70-120 issues each)

**Projects**:
- Nop.Plugin.Shipping.UPS (71 issues)
- Nop.Plugin.Feed.GoogleShopping (76 issues)
- Nop.Plugin.Shipping.Fedex (86 issues)
- Nop.Plugin.Payments.Manual (100 issues)
- Nop.Plugin.Tax.FixedOrByCountryStateZip (101 issues)
- Nop.Plugin.ExternalAuth.Facebook (104 issues, **security vulnerability**)
- Nop.Plugin.DiscountRules.HasOneProduct (105 issues)
- Nop.Plugin.Payments.PayPalStandard (120 issues)

**Additional Complexity**: External API integrations, more complex logic.

**Security Note**: Facebook plugin has Newtonsoft.Json vulnerability - update to 13.0.4.

---

#### Plugin Batch 3 - Complex Plugins (3 plugins, 120+ issues each)

**Projects**:
- Nop.Plugin.Payments.PayPalDirect (143 issues, **security vulnerability**)
- Nop.Plugin.Pickup.PickupInStore (144 issues)
- Nop.Plugin.Shipping.FixedOrByWeight (199 issues)

**Highest Complexity Plugins**: May need individual attention, complex workflows, data access.

**Security Note**: PayPalDirect has vulnerability - update Newtonsoft.Json.

---

### Phase 7: Testing Tier

#### Nop.Web.MVC.Tests

**Project Path**: `Tests\Nop.Web.MVC.Tests\Nop.Web.MVC.Tests.csproj`

**Summary**:
- **Issues**: 335 (317 mandatory)
- **Files**: ~100+ integration tests
- **Complexity**: HIGH - Integration tests across entire stack
- **Dependencies**: ALL projects (Nop.Core, Nop.Data, Nop.Services, Nop.Web.Framework, Nop.Admin, Nop.Web, Nop.Tests)

**Migration**:
1. Update test infrastructure for ASP.NET Core testing
2. Use Microsoft.AspNetCore.Mvc.Testing
3. Update WebApplicationFactory patterns
4. Fix integration test setups
5. Update mocking (RhinoMocks ? Moq)
6. Run full test suite
7. Fix failures incrementally

**Success Criteria**: Full integration test suite passes, validating entire .NET 8 upgrade.

## Risk Management

### High-Risk Changes

| Project | Risk Level | Description | Mitigation Strategy |
|---------|-----------|-------------|---------------------|
| **Nop.Core** | HIGH | Foundation library, blocks all 28 projects | Extensive testing, incremental fixes, rollback plan |
| **Nop.Data** | HIGH | EF6 ? EF Core migration, database schema changes | Database backup, test migrations on copy, validate queries |
| **Nop.Services** | HIGH | Business logic layer, 407 issues, 189 mandatory | Comprehensive unit tests, integration tests, business rule validation |
| **Nop.Web.Framework** | CRITICAL | ASP.NET ? ASP.NET Core (1,794 issues), blocks 21 projects | Prototype migration first, staged refactoring, extensive testing |
| **Nop.Web** | CRITICAL | Main application, 3,997 issues, customer-facing | Feature-by-feature validation, smoke tests, UAT environment |
| **Nop.Admin** | CRITICAL | Admin application, 9,325 issues, highest issue count | Parallel environment, admin workflow testing, permission validation |

### Security Vulnerabilities

**Critical Security Issues** (must be addressed immediately during upgrade):

| Project | Package | Current Version | CVE/Issue | Remediation |
|---------|---------|-----------------|-----------|-------------|
| Nop.Core | Newtonsoft.Json | 9.0.1 | Multiple vulnerabilities | Update to 13.0.4+ |
| Nop.Services | WindowsAzure.Storage | 8.1.1 | Deprecated, security issues | Replace with Azure.Storage.Blobs 12.x |
| Nop.Services | Microsoft.Azure.KeyVault.Core | 2.0.4 | Deprecated | Replace with Azure.Security.KeyVault.Secrets |
| Nop.Services | StackExchange.Redis.StrongName | 1.2.1 | Deprecated | Update to StackExchange.Redis 2.8.x |
| Nop.Web.Framework | Newtonsoft.Json | 9.0.1 | Multiple vulnerabilities | Update to 13.0.4+ |
| Nop.Web | Newtonsoft.Json | 9.0.1 | Multiple vulnerabilities | Update to 13.0.4+ |
| Nop.Admin | Newtonsoft.Json | 9.0.1 | Multiple vulnerabilities | Update to 13.0.4+ |
| Nop.Plugin.ExternalAuth.Facebook | Newtonsoft.Json | 9.0.1 | Multiple vulnerabilities | Update to 13.0.4+ |
| Nop.Plugin.Payments.PayPalDirect | Newtonsoft.Json | 9.0.1 | Multiple vulnerabilities | Update to 13.0.4+ |
| Nop.Plugin.Shipping.AustraliaPost | Newtonsoft.Json | 9.0.1 | Multiple vulnerabilities | Update to 13.0.4+ |

**Security-First Principle**: All security vulnerabilities will be addressed within their respective project phases.

### Contingency Plans

#### If Nop.Core Migration Fails
- **Alternative 1**: Create new .NET 8 project, migrate code incrementally class-by-class
- **Alternative 2**: Split Nop.Core into smaller modules, migrate separately
- **Fallback**: Engage Microsoft support or community experts

#### If Entity Framework Migration Fails
- **Alternative 1**: Keep EF6 temporarily using EF Core compatibility shim
- **Alternative 2**: Rewrite data access layer with Dapper or raw ADO.NET
- **Fallback**: Database-first approach, regenerate models

#### If ASP.NET Core Migration Fails (Nop.Web.Framework)
- **Alternative 1**: Incremental migration - move pieces to ASP.NET Core gradually
- **Alternative 2**: Prototype new Web.Framework from scratch, migrate features
- **Fallback**: Engage ASP.NET Core migration specialists

#### If Plugin Migration Fails
- **Alternative**: Disable plugin temporarily, migrate core functionality first
- **Plugin-Specific**: Plugins are isolated, failures don't block core upgrade
- **Vendor Contact**: Contact plugin vendor for .NET 8 compatible version

#### If Performance Degradation Occurs
- **Profiling**: Use .NET 8 performance profiling tools to identify bottlenecks
- **Optimization**: Apply .NET 8-specific optimizations (Span<T>, Memory<T>, etc.)
- **Caching**: Review caching strategy, update for .NET 8 patterns
- **Fallback**: Revert tier, analyze performance before retry

#### If Build Errors Exceed Manageable Scope
- **Triage**: Categorize errors (blocking vs non-blocking)
- **Priority**: Fix blocking errors first (compilation failures)
- **Warnings**: Address warnings iteratively, not all at once
- **Tooling**: Use .NET Upgrade Assistant for automated fixes
- **Fallback**: Split project into smaller pieces

### Risk Mitigation Strategies by Phase

**Phase 1 (Nop.Core)**:
- ? Create backup branch before starting
- ? Fix issues incrementally (don't try to fix all 305 at once)
- ? Keep System.Web references temporarily if needed
- ? Use #if directives for conditional compilation during transition
- ? Test each major change independently

**Phase 2 (Nop.Data)**:
- ? Backup database before migration
- ? Test EF Core migrations on separate database first
- ? Validate all existing data queries work
- ? Performance test critical database operations
- ? Keep EF6 package temporarily if needed during transition

**Phase 3 (Nop.Services)**:
- ? Comprehensive unit test coverage before starting
- ? Run unit tests after each service class updated
- ? Integration tests with Nop.Data
- ? Business rule validation with stakeholders

**Phase 4 (Nop.Web.Framework)**:
- ? Create ASP.NET Core prototype first to validate approach
- ? Migrate features in small batches (routing, filters, views, etc.)
- ? Test each feature in isolation
- ? Document breaking changes for downstream projects

**Phase 5 (Applications)**:
- ? Set up parallel test environment
- ? Smoke tests after each major change
- ? User Acceptance Testing (UAT) for critical workflows
- ? Performance benchmarking
- ? Security scanning

**Phase 6 (Plugins)**:
- ? Batch similar plugins together
- ? Test plugin loading mechanism first
- ? Validate plugin doesn't break main app
- ? Can skip/disable problematic plugins temporarily

**Phase 7 (Tests)**:
- ? Update test infrastructure first
- ? Fix tests incrementally
- ? Acceptable to have some tests failing initially
- ? Focus on critical integration tests first

### Breaking Change Risk Assessment

**Category: ASP.NET ? ASP.NET Core (Highest Risk)**
- **Affected Projects**: Nop.Web.Framework, Nop.Web, Nop.Admin, All 18 Plugins
- **Risk**: CRITICAL - Fundamental architecture change
- **Mitigation**: Prototype, extensive testing, staged migration

**Category: Entity Framework 6 ? EF Core (High Risk)**
- **Affected Projects**: Nop.Data, Nop.Services
- **Risk**: HIGH - Database layer changes
- **Mitigation**: Database backups, test environment, query validation

**Category: Package Replacements (Medium Risk)**
- **Affected Projects**: All projects
- **Risk**: MEDIUM - API changes, behavior differences
- **Mitigation**: Review package migration guides, test thoroughly

**Category: .NET API Changes (Medium Risk)**
- **Affected Projects**: All projects
- **Risk**: MEDIUM - Compile errors, behavior changes
- **Mitigation**: Incremental fixes, testing

### Rollback Criteria

**Abort Phase and Rollback If**:
1. ? More than 50% of tests failing after fixes
2. ? Critical functionality completely broken
3. ? Performance degradation > 50%
4. ? New security vulnerabilities introduced
5. ? Unable to achieve stable build after 3 days effort
6. ? Data corruption in database migrations
7. ? Stakeholder-critical feature lost

**Rollback Process**:
1. Stop all work on current phase
2. Document blocking issues
3. Git revert to last stable checkpoint
4. Verify rollback successful
5. Analyze root cause
6. Revise plan or seek expert help
7. Retry phase with updated approach

## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing occurs at three levels: per-project, per-tier, and full-solution.

#### Per-Project Testing (After Each Project Upgrade)

**Build Validation**:
- ? Project builds without errors
- ? Project builds without warnings (target zero warnings)
- ? All package references resolved
- ? No dependency conflicts

**Code Quality Checks**:
- ? No new compiler warnings introduced
- ? No code analysis warnings (if using analyzers)
- ? Code follows .NET 8 best practices

**Unit Test Execution**:
- ? All existing unit tests pass
- ? No tests skipped or ignored
- ? Test coverage maintained or improved

**Security Scan**:
- ? NuGet vulnerability scan passes
- ? No new security warnings
- ? Outdated packages addressed

#### Per-Tier Testing (After Completing Entire Tier)

**Integration Testing**:
- ? Projects within tier interact correctly
- ? Lower tier dependencies stable
- ? Higher tier projects still compatible (if still on .NET Framework)

**Regression Testing**:
- ? No functionality broken from previous tiers
- ? Lower tier tests still pass
- ? Integration tests between tiers pass

**Performance Baseline**:
- ? Tier performance equal or better than .NET Framework
- ? No memory leaks introduced
- ? Resource usage acceptable

#### Full Solution Testing (After All Phases Complete)

**End-to-End Testing**:
- ? Complete application workflows functional
- ? Critical business paths verified
- ? User-facing features working

**System Integration Testing**:
- ? All projects work together
- ? Database operations successful
- ? External service integrations working

**Performance Testing**:
- ? Application performance meets baseline
- ? Load testing passes
- ? Response times acceptable

**User Acceptance Testing (UAT)**:
- ? Stakeholder validation
- ? Critical workflows approved
- ? No show-stopper issues

### Phase-Specific Testing Requirements

#### Phase 0: Cleanup
- **Validation**: Solution builds after removing empty projects
- **Tests**: None required (just deletion)

#### Phase 1: Nop.Core
- **Unit Tests**: Run Nop.Core.Tests (after it's upgraded in Phase 3)
- **Integration**: Verify Nop.Core can be referenced by test projects
- **Specific Checks**:
  - Domain models serialize correctly
  - Configuration system loads
  - Infrastructure services initialize
  - Caching works
  - Plugin loading mechanism functional

#### Phase 2: Nop.Data
- **Unit Tests**: Run Nop.Data.Tests
- **Integration**: Database connectivity, migrations run successfully
- **Specific Checks**:
  - DbContext initializes
  - All entity mappings load
  - Database migrations apply cleanly
  - CRUD operations work for all entities
  - Queries return correct results
  - Transactions work correctly
  - Connection pooling functional

#### Phase 3: Nop.Services
- **Unit Tests**: Run Nop.Services.Tests (407 issues, extensive testing required)
- **Integration**: Services interact with Nop.Data correctly
- **Specific Checks**:
  - All service classes instantiate
  - Business logic methods execute
  - Validation rules enforced
  - External service calls work (email, SMS, payment gateways)
  - Caching layer functional
  - Background tasks run
  - Event system works

#### Phase 4: Nop.Web.Framework
- **Unit Tests**: Run framework-specific tests
- **Integration**: ASP.NET Core pipeline works
- **Specific Checks**:
  - HTTP request pipeline configured
  - Middleware executes in order
  - Routing works
  - Filters execute correctly
  - View rendering works
  - Model binding works
  - Authentication functional
  - Authorization functional
  - Session state works
  - TempData works
  - Localization works

#### Phase 5: Nop.Web & Nop.Admin
- **Smoke Tests** (Critical Workflows):
  - **Nop.Web**:
    - ? Home page loads
    - ? Product catalog browsing
    - ? Product details page
    - ? Add to cart
    - ? Checkout process (all steps)
    - ? User registration
    - ? User login/logout
    - ? Search functionality
    - ? Category navigation
  
  - **Nop.Admin**:
    - ? Admin login
    - ? Dashboard loads
    - ? Product management (CRUD)
    - ? Order management (view, update)
    - ? Customer management
    - ? Settings pages load
    - ? Reports generation
    - ? Plugin management

- **Integration Tests**:
  - All HTTP endpoints return correct status codes
  - Static files serve correctly
  - JavaScript/CSS bundling works
  - AJAX calls functional
  - Forms submit correctly
  - Validation works

#### Phase 6: Plugins
- **Per-Plugin Tests**:
  - ? Plugin loads without errors
  - ? Plugin doesn't crash main application
  - ? Plugin configuration page loads
  - ? Plugin functionality works (e.g., payment processes, shipping calculates)
  
- **Plugin Conflict Testing**:
  - ? All plugins load simultaneously
  - ? No route conflicts
  - ? No dependency conflicts

#### Phase 7: Nop.Web.MVC.Tests
- **Integration Tests**: All 335 issues addressed
- **Full Test Suite**: Run complete integration test suite
- **End-to-End Scenarios**: Full workflow tests

### Validation Checklists

#### Tier Completion Checklist

Before marking a tier complete:

- [ ] All projects in tier converted to SDK-style
- [ ] All projects targeting net8.0
- [ ] All package updates applied
- [ ] Zero build errors
- [ ] Zero build warnings (or documented exceptions)
- [ ] All unit tests passing
- [ ] Integration tests passing
- [ ] No security vulnerabilities
- [ ] Performance acceptable
- [ ] Code committed to version control
- [ ] Tier documented (changes, lessons learned)

#### Full Upgrade Completion Checklist

Before declaring upgrade complete:

- [ ] All 34 projects on .NET 8
- [ ] All empty placeholder projects removed
- [ ] All security vulnerabilities resolved
- [ ] All tests passing (unit, integration, end-to-end)
- [ ] Smoke tests completed for critical workflows
- [ ] UAT completed successfully
- [ ] Performance meets or exceeds baseline
- [ ] Documentation updated
- [ ] Deployment process validated
- [ ] Rollback plan tested
- [ ] Stakeholder sign-off obtained

### Testing Tools & Frameworks

**Build & Compile**:
- .NET 8 SDK
- Visual Studio 2022 or later
- MSBuild 17.0+

**Unit Testing**:
- NUnit 3.6.1+ (current framework, update to 4.x for .NET 8)
- RhinoMocks ? Moq or NSubstitute (RhinoMocks incompatible)

**Integration Testing**:
- ASP.NET Core TestHost
- Microsoft.AspNetCore.Mvc.Testing

**Performance Testing**:
- BenchmarkDotNet
- Application Insights
- Performance profiling tools (dotnet-trace, PerfView)

**Security Scanning**:
- NuGet vulnerability scanner (built into .NET CLI)
- OWASP Dependency-Check
- Snyk or similar

**Code Quality**:
- .NET Code Analyzers
- StyleCop (optional)
- SonarQube (optional)

### Test Environment Requirements

**Development Environment**:
- .NET 8 SDK installed
- Visual Studio 2022 17.8+
- SQL Server or compatible database
- Redis (for caching tests)

**CI/CD Environment**:
- Automated build on commit
- Automated test execution
- Code coverage reporting
- NuGet vulnerability scanning

**Testing Database**:
- Copy of production database (sanitized)
- Test data for all scenarios
- Migration test environment

**Performance Baseline**:
- Baseline metrics from .NET Framework 4.5.1 version
- Target: .NET 8 version equal or faster

### Acceptance Criteria

**Build Success**:
- All projects build successfully
- Zero compilation errors
- Zero or minimal warnings

**Functional Success**:
- All critical workflows functional
- No feature regressions
- All plugins working

**Quality Success**:
- All automated tests passing
- Code quality maintained
- No new technical debt

**Performance Success**:
- Response times <= .NET Framework baseline
- Memory usage acceptable
- No performance regressions

**Security Success**:
- All known vulnerabilities patched
- Security scan passes
- Authentication/authorization working

## Complexity & Effort Assessment

### Project Complexity Ratings

| Project | Complexity | Issues | LOC | Dependencies | Risk | Rationale |
|---------|-----------|--------|-----|--------------|------|-----------|
| **Nop.Core** | HIGH | 305 | ~15K | 0 projects | HIGH | Foundation library, System.Web removal, blocks all |
| **Nop.Data** | HIGH | 10 | ~8K | 1 project | HIGH | EF6 ? EF Core migration, database changes |
| **Nop.Tests** | LOW | 3 | ~1K | 1 project | LOW | Test base classes only |
| **Nop.Services** | HIGH | 407 | ~25K | 2 projects | HIGH | Business logic, many integrations, 189 mandatory issues |
| **Nop.Core.Tests** | MEDIUM | 36 | ~3K | 2 projects | MEDIUM | Unit tests for core functionality |
| **Nop.Data.Tests** | LOW | 6 | ~2K | 3 projects | LOW | Data access tests |
| **Nop.Web.Framework** | CRITICAL | 1,794 | ~30K | 3 projects | CRITICAL | ASP.NET ? ASP.NET Core, blocks all web projects |
| **Nop.Services.Tests** | MEDIUM | 20 | ~4K | 4 projects | MEDIUM | Service layer tests |
| **Nop.Plugin.ExchangeRate.EcbExchange** | LOW | 12 | ~500 | 2 projects | LOW | Simple plugin, API calls |
| **Nop.Web** | CRITICAL | 3,997 | ~50K | 4 projects | CRITICAL | Main storefront application |
| **Nop.Admin** | CRITICAL | 9,325 | ~80K | 4 projects | CRITICAL | Admin application, highest issue count |
| **Plugins (Simple)** | LOW | 30-60 | 1-3K | 3 projects | LOW | Straightforward controllers/views |
| **Plugins (Medium)** | MEDIUM | 70-120 | 3-6K | 3-4 projects | MEDIUM | More complex logic, external APIs |
| **Plugins (Complex)** | MEDIUM-HIGH | 120-200 | 6-10K | 4 projects | MEDIUM | Complex workflows, data access |
| **Nop.Web.MVC.Tests** | HIGH | 335 | ~8K | 7 projects | MEDIUM | Integration tests, depends on everything |

### Phase Complexity Assessment

| Phase | Projects | Total Issues | Combined LOC | Complexity | Dependencies | Effort |
|-------|----------|-------------|--------------|------------|--------------|--------|
| **Phase 0** | 3 | 0 | 0 | None | Delete only | Minimal |
| **Phase 1** | 1 | 305 | ~15K | HIGH | None, blocks all | High |
| **Phase 2** | 2 | 13 | ~9K | HIGH (Data) | Tier 0 | High |
| **Phase 3** | 3 | 443 | ~30K | HIGH (Services) | Tier 0-1 | High |
| **Phase 4** | 3 | 1,826 | ~35K | CRITICAL (Web.Framework) | Tier 0-2 | Very High |
| **Phase 5** | 2 | 13,322 | ~130K | CRITICAL (Both apps) | Tier 0-3 | Very High |
| **Phase 6** | 18 | 1,557 | ~80K | MEDIUM (Batch approach) | Tier 0-3 | High |
| **Phase 7** | 1 | 335 | ~8K | HIGH (Integration tests) | All tiers | Medium |

### Relative Effort Estimates

**Complexity Legend**:
- **Low**: Straightforward conversion, minimal breaking changes
- **Medium**: Moderate complexity, some refactoring required
- **High**: Significant changes, extensive testing needed
- **Critical**: Major architectural changes, highest risk and effort

**Note**: These are **relative complexity assessments**, not time estimates. Actual duration depends on team size, experience, and unforeseen issues.

### Phase Effort Breakdown

#### Phase 0: Cleanup
- **Relative Effort**: Minimal
- **Activities**: Delete 3 projects, update solution file
- **Challenges**: None

#### Phase 1: Nop.Core (Foundation)
- **Relative Effort**: High
- **Activities**: 
  - Convert to SDK-style
  - Remove System.Web references (significant refactoring)
  - Update 10+ packages
  - Fix 305 issues (53 mandatory)
- **Challenges**: 
  - Foundation for entire solution, must be perfect
  - System.Web removal requires careful refactoring
  - Configuration system migration
  - Plugin infrastructure updates

#### Phase 2: Nop.Data (Data Access)
- **Relative Effort**: High
- **Activities**:
  - Entity Framework 6 ? EF Core 8 migration
  - Update 100+ entity mappings
  - Rewrite DbContext initialization
  - Update query patterns
- **Challenges**:
  - EF6 fluent API differs from EF Core
  - Migration file format changes
  - Potential database schema adjustments
  - Query behavior differences

#### Phase 3: Nop.Services (Business Logic)
- **Relative Effort**: High
- **Activities**:
  - Update 407 issues across 50+ service classes
  - Replace incompatible packages (Azure Storage, Redis, EPPlus)
  - Update external service integrations
- **Challenges**:
  - Business logic must remain functionally identical
  - External API clients may have changed
  - Background job system updates
  - Event system verification

#### Phase 4: Nop.Web.Framework (Web Infrastructure)
- **Relative Effort**: Very High (Highest complexity)
- **Activities**:
  - ASP.NET MVC 5 ? ASP.NET Core MVC migration
  - Remove System.Web namespace (~1,794 issues)
  - Rewrite routing, filters, middleware
  - Update view rendering, HTML helpers
  - Replace OWIN with ASP.NET Core pipeline
- **Challenges**:
  - Fundamental architecture change
  - No direct migration path for many features
  - Affects all downstream projects
  - Must establish patterns for applications/plugins to follow

#### Phase 5: Nop.Web & Nop.Admin (Applications)
- **Relative Effort**: Very High (Largest issue count)
- **Activities**:
  - Migrate 13,322 combined issues
  - Convert Global.asax ? Program.cs/Startup.cs
  - Update all controllers, views, scripts
  - Migrate configuration (web.config ? appsettings.json)
  - wwwroot folder structure
- **Challenges**:
  - Nop.Admin has 9,325 issues (largest single project)
  - Complex admin workflows must be preserved
  - Customer-facing app (Nop.Web) requires thorough testing
  - Static file handling changes

#### Phase 6: Plugins (18 Projects)
- **Relative Effort**: High (but parallelizable)
- **Activities**:
  - Apply patterns from Nop.Web.Framework across 18 plugins
  - Update controllers, views, routes for each
  - Fix 1,557 total issues
- **Challenges**:
  - Repetitive but requires attention to detail
  - Each plugin has unique functionality
  - Plugin loading mechanism validation
  - Potential for conflicts between plugins

#### Phase 7: Nop.Web.MVC.Tests (Integration Tests)
- **Relative Effort**: Medium
- **Activities**:
  - Update test infrastructure for ASP.NET Core
  - Fix 335 issues in integration tests
  - Validate all workflows
- **Challenges**:
  - Depends on all previous phases being stable
  - Tests may reveal issues in earlier phases
  - Test framework updates (ASP.NET Core testing patterns)

### Resource Requirements

**Skills Needed**:
1. **.NET Framework ? .NET Core Migration** (Expert level)
2. **Entity Framework 6 ? EF Core** (Advanced level)
3. **ASP.NET MVC ? ASP.NET Core MVC** (Expert level)
4. **Package migration and replacement** (Intermediate level)
5. **Testing and validation** (Advanced level)

**Team Composition Recommendation**:
- **Lead Developer**: Expert in .NET Core and ASP.NET Core migrations
- **Backend Developer**: EF Core and services layer expertise
- **Frontend Developer**: Razor views, JavaScript/CSS updates
- **QA Engineer**: Testing strategy, test automation
- **DevOps**: CI/CD updates, deployment pipeline

**Parallel Work Capacity**:
- **Phases 0-5**: Sequential (1 developer primary, others supporting)
- **Phase 6**: Up to 3 developers working on different plugin batches
- **Phase 7**: 1-2 developers (testing and fixing)

### Critical Path

**Longest dependency chain**:
Nop.Core ? Nop.Data ? Nop.Services ? Nop.Web.Framework ? Nop.Web / Nop.Admin ? Nop.Web.MVC.Tests

**Blocking Projects**:
1. **Nop.Core**: Blocks ALL 28 projects
2. **Nop.Web.Framework**: Blocks 21 projects (all web apps and plugins)
3. **Nop.Services**: Blocks 24 projects

**Projects on Critical Path**: 6 projects (Nop.Core, Nop.Data, Nop.Services, Nop.Web.Framework, Nop.Web, Nop.Web.MVC.Tests)

### Effort Distribution by Activity Type

**Estimated Effort Breakdown** (relative percentages):
- **Project Conversion** (SDK-style, target framework): 5%
- **Package Updates & Replacements**: 15%
- **Code Refactoring** (breaking changes, API updates): 40%
- **Testing & Validation**: 25%
- **Bug Fixes & Stabilization**: 10%
- **Documentation & Review**: 5%

### Success Factors

**Factors That Will Accelerate**:
- ? Strong automated test coverage
- ? Experience with ASP.NET Core migrations
- ? Clear understanding of business logic
- ? Good documentation of current system
- ? Stakeholder availability for UAT

**Factors That Will Slow Down**:
- ? Poor test coverage
- ? Undocumented business rules
- ? Complex custom implementations
- ? Tight deadlines causing shortcuts
- ? Unavailable stakeholders for validation

## Source Control Strategy

### Branching Strategy

**Note**: No Git repository detected in the solution. If using source control, apply this strategy:

#### Recommended Branch Structure

```
main (or master)
  ???? dotnet8-upgrade (base upgrade branch)
         ???? dotnet8-phase0-cleanup
         ???? dotnet8-phase1-core
         ???? dotnet8-phase2-data
         ???? dotnet8-phase3-services
         ???? dotnet8-phase4-framework
         ???? dotnet8-phase5-apps
         ???? dotnet8-phase6-plugins-batch1
         ???? dotnet8-phase6-plugins-batch2
         ???? dotnet8-phase6-plugins-batch3
         ???? dotnet8-phase7-tests
```

#### Branching Flow

1. **Create Base Branch**: `dotnet8-upgrade` from `main`
2. **Phase Branches**: Each phase gets own branch from `dotnet8-upgrade`
3. **Merge Forward**: After phase complete, merge back to `dotnet8-upgrade`
4. **Final Merge**: After all phases, merge `dotnet8-upgrade` to `main`

**Rationale**:
- Isolates each phase
- Enables independent review
- Allows rollback per phase
- Maintains clean history

#### Branch Protection

**On `main` branch**:
- Require pull request reviews
- Require status checks (build, tests)
- No direct pushes

**On `dotnet8-upgrade` branch**:
- Require passing builds before merge
- Require at least 1 review

### Commit Strategy

#### Commit Frequency

**During Active Development**:
- Commit after each logical unit of work
- Commit when build is successful
- Commit before and after major refactoring

**Recommended Cadence**:
- **Minimum**: End of each work session
- **Ideal**: After completing each sub-task
- **Required**: Before moving to next project in tier

#### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types**:
- `feat`: New feature or capability
- `fix`: Bug fix
- `refactor`: Code refactoring (no functional change)
- `upgrade`: Framework or package upgrade
- `test`: Adding or updating tests
- `docs`: Documentation updates
- `chore`: Maintenance tasks

**Examples**:

```
upgrade(nop-core): Convert project to SDK-style format

- Updated Nop.Core.csproj to use Microsoft.NET.Sdk
- Changed TargetFramework to net8.0
- Removed legacy project properties

Related: Phase 1 - Foundation Tier
```

```
fix(nop-data): Migrate Entity Framework 6 to EF Core 8

- Updated all entity mappings to EF Core fluent API
- Changed DbContext initialization
- Updated migration file format
- Fixed query syntax for compatibility

Issues: 10 (4 mandatory)
Related: Phase 2 - Data Access Tier
```

```
refactor(nop-web-framework): Remove System.Web dependencies

- Replaced HttpContext with ASP.NET Core HttpContext
- Migrated filters to middleware
- Updated routing to endpoint routing
- Removed OWIN, added ASP.NET Core pipeline

Breaking changes: 1,794 issues addressed
Related: Phase 4 - Web Framework Tier
```

#### Commit Checkpoints

**Required Commits**:
1. Before starting each phase
2. After project conversion to SDK-style
3. After package updates complete
4. After code compiles successfully
5. After tests pass
6. After phase validation complete

### Review and Merge Process

#### Pull Request Requirements

**For Each Phase**:
1. Create PR from phase branch to `dotnet8-upgrade` branch
2. PR title: `Phase X: [Phase Name] - [Brief Description]`
3. PR description includes:
   - Summary of changes
   - Issues addressed
   - Breaking changes introduced
   - Testing performed
   - Known issues or limitations

**Example PR Template**:

```markdown
## Phase 1: Foundation Tier - Nop.Core Upgrade

### Summary
Upgraded Nop.Core project from .NET Framework 4.5.1 to .NET 8.0

### Changes
- Converted to SDK-style project format
- Updated TargetFramework to net8.0
- Updated 10 NuGet packages
- Removed System.Web dependencies
- Fixed 305 issues (53 mandatory)

### Breaking Changes
- Removed HttpContext usage (replaced with IHttpContextAccessor)
- Updated configuration system (no longer uses ConfigurationManager)
- Changed caching APIs

### Testing
- [x] Project builds successfully
- [x] Zero build errors
- [x] Zero build warnings
- [x] Unit tests passing: Nop.Core.Tests
- [x] NuGet vulnerability scan passed

### Known Issues
- None

### Dependencies
- Blocks: All 28 downstream projects
- Requires: None (foundation)

### Reviewers
@tech-lead @backend-dev

### Checklist
- [x] Code builds without errors
- [x] Tests passing
- [x] No security vulnerabilities
- [x] Documentation updated
- [x] Commit messages follow convention
```

#### Review Checklist

**Code Review Focus**:
- [ ] Project builds successfully
- [ ] All tests passing
- [ ] No new vulnerabilities
- [ ] Breaking changes documented
- [ ] Code quality maintained
- [ ] Performance acceptable
- [ ] Follows .NET 8 best practices

**Reviewer Responsibilities**:
1. Build project locally
2. Run tests
3. Review code changes
4. Check for potential issues
5. Approve or request changes

#### Merge Criteria

**Phase branch can merge to `dotnet8-upgrade` when**:
- ? All required commits present
- ? PR approved by at least 1 reviewer
- ? Build passing
- ? All tests passing
- ? No security vulnerabilities
- ? Documentation updated

**Merge Method**: 
- Use **Squash and Merge** for cleaner history (optional)
- Or **Merge Commit** to preserve detailed history

### Version Control Best Practices

#### What to Commit
- ? All source code (.cs files)
- ? Project files (.csproj)
- ? Solution file (.sln)
- ? Configuration files (appsettings.json, etc.)
- ? Documentation (README, CHANGELOG)
- ? Build scripts

#### What NOT to Commit
- ? bin/ folders
- ? obj/ folders
- ? packages/ folder
- ? .vs/ folder
- ? user-specific files (.suo, .user)
- ? Database files (unless seed data)
- ? Sensitive credentials

#### .gitignore Recommendations

Ensure `.gitignore` includes:
```
# Build results
[Bb]in/
[Oo]bj/
[Ll]og/
[Ll]ogs/

# Visual Studio
.vs/
*.user
*.suo
*.userosscache
*.sln.docstates

# NuGet
packages/
*.nupkg
*.snupkg

# Database
*.mdf
*.ldf
*.ndf

# Other
.DS_Store
Thumbs.db
```

### Rollback Procedures

#### If Phase Needs Rollback

1. **Stop Work**: Halt all development on current phase
2. **Document Issue**: Record blocking problem in issue tracker
3. **Create Rollback Branch**: 
   ```bash
   git checkout dotnet8-upgrade
   git checkout -b dotnet8-phaseX-rollback
   ```
4. **Revert Phase Changes**:
   ```bash
   git revert <commit-range>
   ```
5. **Verify Rollback**:
   - Build solution
   - Run tests
   - Confirm previous state restored
6. **Analyze & Revise**:
   - Identify root cause
   - Update plan
   - Retry with revised approach

#### Full Upgrade Rollback

If entire upgrade must be abandoned:
1. Checkout `main` branch
2. Solution remains on .NET Framework 4.5.1
3. `dotnet8-upgrade` branch preserved for future retry

### Tagging Strategy

**Tag Each Phase Completion**:
```bash
git tag -a phase1-complete -m "Phase 1: Nop.Core upgrade complete"
git tag -a phase2-complete -m "Phase 2: Nop.Data upgrade complete"
# etc.
```

**Tag Final Completion**:
```bash
git tag -a v1.0.0-net8.0 -m "Complete .NET 8 upgrade"
```

**Benefits**:
- Easy rollback to specific phase
- Clear milestones in history
- Version tracking

### Continuous Integration (CI) Integration

**Build on Every Commit** (if CI/CD available):
- Trigger build on push to phase branches
- Run automated tests
- Perform security scans
- Block merge if build fails

**Status Checks**:
- Build success required
- All tests passing required
- No security vulnerabilities required

### Documentation Updates

**Update with Each Phase**:
- [ ] Update README.md if necessary
- [ ] Update CHANGELOG.md with changes
- [ ] Update deployment documentation
- [ ] Update developer setup guide

**Final Documentation**:
- [ ] .NET 8 upgrade complete guide
- [ ] Breaking changes summary
- [ ] Deployment procedures updated
- [ ] Troubleshooting guide

## Success Criteria

### Technical Criteria

#### All Projects Migrated
- [x] All 34 active projects targeting .NET 8.0
- [x] All 3 empty placeholder projects removed (Nop.CoreCore, Nop.DataCore, Nop.ServicesCore)
- [x] All projects using SDK-style project format
- [x] Solution file updated and clean

#### Package Requirements
- [x] All incompatible packages replaced or removed
- [x] All packages updated to .NET 8 compatible versions
- [x] All security vulnerabilities resolved (Newtonsoft.Json, Azure packages, etc.)
- [x] No deprecated packages in use
- [x] EntityFramework 6 ? EF Core 8 migration complete

#### Build Success
- [x] Solution builds without errors
- [x] All 34 projects build individually
- [x] Zero compilation errors across solution
- [x] Zero or minimal build warnings (documented if any)
- [x] No package dependency conflicts
- [x] NuGet package restore successful

#### Testing Success
- [x] All unit tests passing (Nop.Core.Tests, Nop.Data.Tests, Nop.Services.Tests)
- [x] All integration tests passing (Nop.Web.MVC.Tests)
- [x] Smoke tests for critical workflows passing
- [x] No test failures or ignored tests
- [x] Test coverage maintained or improved

#### Framework Migration Complete
- [x] ASP.NET Framework ? ASP.NET Core migration complete
  - [x] Global.asax removed, Program.cs + Startup.cs created
  - [x] RouteCollection ? Endpoint routing migrated
  - [x] GlobalFilterCollection ? Middleware migrated
  - [x] System.Web namespace fully removed
- [x] Entity Framework 6 ? EF Core 8 migration complete
  - [x] All entity mappings updated
  - [x] DbContext configuration updated
  - [x] Database migrations functional
- [x] OWIN ? ASP.NET Core middleware migration complete

#### Configuration Migration
- [x] web.config ? appsettings.json conversion complete
- [x] Connection strings migrated
- [x] App settings migrated
- [x] Environment-specific configuration working

### Quality Criteria

#### Code Quality Maintained
- [x] Code follows .NET 8 best practices
- [x] No new code smells introduced
- [x] Technical debt not increased
- [x] Code analysis warnings addressed
- [x] Consistent coding standards maintained

#### Test Coverage Maintained
- [x] Unit test coverage >= original coverage
- [x] Integration test coverage >= original coverage
- [x] Critical paths have test coverage
- [x] New .NET 8-specific tests added where appropriate

#### Documentation Updated
- [x] README.md reflects .NET 8 upgrade
- [x] CHANGELOG.md documents upgrade
- [x] Developer setup guide updated for .NET 8
- [x] Deployment documentation updated
- [x] Breaking changes documented
- [x] API changes documented

#### Performance Maintained or Improved
- [x] Application performance >= .NET Framework baseline
- [x] Page load times acceptable
- [x] API response times acceptable
- [x] Database query performance maintained
- [x] Memory usage acceptable
- [x] No performance regressions identified

### Functional Criteria

#### Critical Workflows Functional
- [x] **Nop.Web (Storefront)**:
  - [x] Home page loads
  - [x] Product catalog browsing works
  - [x] Product details display
  - [x] Add to cart functional
  - [x] Checkout process (all steps)
  - [x] User registration works
  - [x] User login/logout works
  - [x] Search functionality works
  - [x] Category navigation works
  - [x] Order placement successful

- [x] **Nop.Admin (Administration)**:
  - [x] Admin login functional
  - [x] Dashboard loads and displays data
  - [x] Product management (CRUD operations)
  - [x] Order management (view, update status)
  - [x] Customer management works
  - [x] Settings pages load and save
  - [x] Reports generate correctly
  - [x] Plugin management works

- [x] **Plugins** (all 18):
  - [x] All plugins load without errors
  - [x] Plugin configuration pages accessible
  - [x] Plugin functionality works:
    - [x] Payment plugins process payments
    - [x] Shipping plugins calculate rates
    - [x] Tax plugins calculate taxes
    - [x] Widget plugins render
    - [x] Discount plugins apply rules

#### Data Integrity
- [x] Database schema intact
- [x] All data accessible
- [x] No data loss during migration
- [x] Database migrations applied successfully
- [x] Data validation rules enforced

#### External Integration
- [x] Payment gateways functional (PayPal, etc.)
- [x] Shipping integrations work (FedEx, UPS, USPS, etc.)
- [x] Email sending functional
- [x] External authentication works (Facebook, etc.)
- [x] API endpoints functional

### Security Criteria

#### Security Vulnerabilities Resolved
- [x] All NuGet packages scanned for vulnerabilities
- [x] Zero known security vulnerabilities
- [x] Critical vulnerabilities addressed:
  - [x] Newtonsoft.Json updated (9.0.1 ? 13.0.4+)
  - [x] WindowsAzure.Storage replaced with Azure.Storage.Blobs
  - [x] Deprecated Azure packages replaced
  - [x] StackExchange.Redis updated

#### Authentication & Authorization
- [x] User authentication functional
- [x] User authorization functional
- [x] Role-based access control works
- [x] Admin area properly secured
- [x] External authentication works (OAuth, etc.)
- [x] Session management functional

#### Security Best Practices
- [x] HTTPS enforced where appropriate
- [x] Cross-site scripting (XSS) protection
- [x] Cross-site request forgery (CSRF) protection
- [x] SQL injection protection maintained
- [x] Secure password hashing

### Process Criteria

#### Bottom-Up Strategy Followed
- [x] Phases completed in dependency order:
  - [x] Phase 0: Cleanup complete
  - [x] Phase 1: Nop.Core (Tier 0) complete
  - [x] Phase 2: Nop.Data, Nop.Tests (Tier 1) complete
  - [x] Phase 3: Nop.Services, test projects (Tier 2) complete
  - [x] Phase 4: Nop.Web.Framework (Tier 3) complete
  - [x] Phase 5: Nop.Web, Nop.Admin (Tier 4) complete
  - [x] Phase 6: All 18 plugins (Tier 4) complete
  - [x] Phase 7: Nop.Web.MVC.Tests (Tier 5) complete

#### Validation at Each Tier
- [x] Each tier validated before proceeding to next
- [x] Build success confirmed per tier
- [x] Tests passing confirmed per tier
- [x] No regressions introduced between tiers

#### Source Control Followed
- [x] All changes committed to version control
- [x] Commit messages follow convention
- [x] Each phase has dedicated branch (if using Git)
- [x] Pull requests reviewed and approved
- [x] Phase completion tagged

### Stakeholder Criteria

#### User Acceptance Testing (UAT)
- [x] Stakeholders invited to UAT
- [x] Critical workflows demonstrated
- [x] User feedback collected
- [x] Showstopper issues addressed
- [x] Stakeholder sign-off obtained

#### Business Requirements Met
- [x] No critical functionality lost
- [x] Business rules maintained
- [x] Compliance requirements met
- [x] Customer-facing features working
- [x] Admin features working

### Deployment Criteria

#### Deployment Readiness
- [x] Deployment process documented
- [x] Deployment tested in staging environment
- [x] Rollback procedure tested
- [x] Configuration management updated
- [x] Environment variables configured
- [x] Database migration scripts ready

#### Production Deployment
- [x] Staged deployment plan created
- [x] Downtime window communicated (if needed)
- [x] Backup created before deployment
- [x] Monitoring in place
- [x] Support team briefed

### Final Success Definition

**The .NET 8 upgrade is complete and successful when**:

1. ? All 34 projects are on .NET 8.0
2. ? Solution builds without errors
3. ? All automated tests passing
4. ? All security vulnerabilities resolved
5. ? Critical workflows functional (storefront + admin)
6. ? All 18 plugins working
7. ? Performance meets or exceeds baseline
8. ? User acceptance testing passed
9. ? Stakeholder sign-off obtained
10. ? Production deployment successful

**At this point**:
- Solution can be deployed to production
- .NET Framework 4.5.1 version can be archived
- Team can proceed with .NET 8-specific enhancements
- NopCommerce is fully modernized on .NET 8

---

## Appendix: Bottom-Up Strategy Principles Applied

This plan strictly adheres to **Bottom-Up (Dependency-First) Strategy**:

### Tier-Based Execution
- **Tier 0**: Nop.Core (zero dependencies) ? upgraded first
- **Tier 1**: Nop.Data, Nop.Tests (depend only on Tier 0) ? upgraded second
- **Tier 2**: Nop.Services, test projects (depend on Tiers 0-1) ? upgraded third
- **Tier 3**: Nop.Web.Framework (depends on Tiers 0-2) ? upgraded fourth
- **Tier 4**: Applications + Plugins (depend on Tiers 0-3) ? upgraded fifth
- **Tier 5**: Integration tests (depend on all) ? upgraded last

### No Multi-Targeting
- Each tier fully migrated to .NET 8 before proceeding
- Projects always reference same or newer framework version
- No compatibility shims or multi-targeting needed

### Validation Between Tiers
- Each tier tested independently before next tier starts
- Lower tiers stabilized before dependent tiers begin
- Build + test success required for tier completion

### Risk-First Within Strategy
- Security vulnerabilities addressed within each tier's phase
- High-complexity projects (Web.Framework, Applications) handled with extra care
- Critical dependencies (Core, Services, Web.Framework) prioritized within tiers

**Strategy Benefits Realized**:
- ? Lowest risk approach for complex migration
- ? Clear validation points at each tier
- ? Easier debugging (issues isolated to current tier)
- ? No multi-targeting complexity
- ? Learning from early tiers applied to later ones
