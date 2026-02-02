# NopCommerce .NET 8.0 - Deployment Guide

This guide explains the two deployment approaches available.

---

## 📋 Two Deployment Approaches

### ✅ Approach 1: Development Deployment
**For you** - when making code changes

### ✅ Approach 2: Distribution Package  
**For others** - pre-built, no source code needed

---

## 🔧 Approach 1: Development Deployment

**Use when:** You have the source code and are making changes

**What it does:** Compiles from source and deploys to IIS

### Quick Start:
```powershell
cd deployment
.\Deploy-All.ps1
```

**What happens:**
1. ✅ Compiles source code (`dotnet publish`)
2. ✅ Checks/installs prerequisites
3. ✅ Creates IIS app pool and website
4. ✅ Deploys to C:\inetpub\nopcommerce
5. ✅ Configures permissions

**Requirements:**
- ✅ Source code
- ✅ .NET 8.0 SDK
- ✅ Administrator rights

**Files:**
- `Deploy-All.ps1` - Main script (compile + deploy)
- `Deploy-IIS.ps1` - Deploy only (called by Deploy-All)
- `Install-Prerequisites.ps1` - Install IIS, .NET 8.0
- `Verify-Prerequisites.ps1` - Check prerequisites

---

## 📦 Approach 2: Distribution Package

**Use when:** Sharing with others who don't have source code

**What it does:** Creates a package with pre-built binaries

### Step 1: Create Package (one time)
```powershell
cd deployment
.\Create-DistributionPackage.ps1
```

**Output:** `dist\nopcommerce-iis-package\`  
**Size:** ~200-300 MB

**Package contents:**
```
nopcommerce-iis-package/
├── app/                    # All binaries (DLLs, Views, wwwroot, etc.)
├── Deploy.ps1              # Simple deployment script
├── Install-Prerequisites.ps1
├── Verify-Prerequisites.ps1
└── README.md               # Instructions for end users
```

### Step 2: Distribute Package

**Option A: Zip and Share**
```powershell
Compress-Archive -Path dist\nopcommerce-iis-package -DestinationPath NopCommerce-IIS-Package.zip
```

Share `NopCommerce-IIS-Package.zip` via:
- Email / File share
- Network drive
- Cloud storage
- GitHub Releases

**Option B: Network Share**
- Copy `dist\nopcommerce-iis-package\` to network location
- Others access directly

### Step 3: Others Deploy

Users receive the package and run:

```powershell
# 1. Unzip (if zipped)
Expand-Archive NopCommerce-IIS-Package.zip -DestinationPath C:\Temp

# 2. Open PowerShell as Administrator
cd C:\Temp\nopcommerce-iis-package

# 3. Deploy
.\Deploy.ps1
```

**That's it!** No source code, no compilation needed.

---

## 🔄 Comparison

| Feature | Approach 1 (Dev) | Approach 2 (Distribution) |
|---------|------------------|---------------------------|
| **Use case** | Development | Production/Distribution |
| **Source code** | ✅ Required | ❌ Not needed |
| **.NET SDK** | ✅ Required | ❌ Not needed |
| **Compilation** | ✅ Every deploy | ❌ Pre-built |
| **Deploy speed** | Slower (compile) | Faster (copy only) |
| **Package size** | Small (source) | Large (~300 MB) |
| **Best for** | Developers | End users/customers |

---

## 🎯 Recommended Workflow

### For Developers:
1. Make code changes
2. Test with `Deploy-All.ps1`
3. Commit to Git
4. **Don't commit** `dist/` or `publish/` folders

### For Releases:
1. Run `Create-DistributionPackage.ps1`
2. Test the package on clean VM
3. Zip and share: `NopCommerce-IIS-Package.zip`
4. Users deploy with `Deploy.ps1`

---

## 📁 What to Check into Git

### ✅ Check in:
- `deployment/*.ps1` (all scripts)
- `src/` (source code)
- `.gitignore`
- Documentation

### ❌ Don't check in:
- `publish/` (build outputs)
- `dist/` (distribution packages - too large)
- `bin/`, `obj/` (build artifacts)
- `App_Data/Settings.txt` (created at runtime)

---

## 🔍 Troubleshooting

### Issue: Deploy-All.ps1 fails with compilation errors
**Solution:** Fix source code errors first, then redeploy

### Issue: Distribution package Deploy.ps1 fails
**Solution:** 
- Ensure .NET 8.0 Hosting Bundle is installed
- Run as Administrator
- Check IIS is enabled

### Issue: Port 80 already in use
**Solution:** 
```powershell
.\Deploy.ps1 -Port 8080
```

### Issue: Package too large for email
**Solution:** 
- Use file share or cloud storage
- Or use Approach 1 (share source code instead)

---

## 🚀 Advanced Options

### Custom deployment location:
```powershell
.\Deploy.ps1 -PhysicalPath "D:\Websites\NopCommerce"
```

### Custom site/pool names:
```powershell
.\Deploy.ps1 -SiteName "MyShop" -AppPoolName "MyShopPool"
```

### Skip prerequisites check:
```powershell
.\Deploy.ps1 -SkipPrerequisites
```

---

## 📞 Support

For deployment issues:
1. Check logs: `C:\inetpub\nopcommerce\logs\`
2. Check Event Viewer: Windows Logs → Application
3. Verify prerequisites: `.\Verify-Prerequisites.ps1`

---

**Created:** $(Get-Date -Format "yyyy-MM-dd")  
**Version:** NopCommerce 8.0
