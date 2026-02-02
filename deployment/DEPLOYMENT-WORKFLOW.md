# Deployment Workflow for GCP Test Servers

This document explains how to deploy to test servers using the distribution package.

---

## 🎯 Your Use Case

**Goal:** Deploy to test servers in GCP from a deployment/build server

**Best Approach:** Check in scripts → Build on deployment server → Deploy to test servers

---

## 📋 Step-by-Step Workflow

### **Step 1: Check Scripts into Git** (One-time setup)

What to commit:

```bash
# Check git status
git status

# Should commit:
# - deployment/Create-DistributionPackage.ps1 ✅
# - deployment/Deploy-All.ps1 ✅
# - deployment/Deploy-IIS.ps1 ✅
# - deployment/Install-Prerequisites.ps1 ✅
# - deployment/Verify-Prerequisites.ps1 ✅
# - deployment/DEPLOYMENT-GUIDE.md ✅
# - deployment/DEPLOYMENT-WORKFLOW.md ✅
# - .gitignore ✅

# Commit
git add deployment/*.ps1 deployment/*.md .gitignore
git commit -m "Add distribution package deployment scripts"
git push origin main
```

**Note:** Do NOT commit `dist/` or `publish/` folders (they're in .gitignore)

---

### **Step 2: On Your GCP Deployment Server**

#### 2a. Pull the code:
```powershell
# On GCP deployment server
cd C:\DeploymentServer  # Or your preferred location
git clone <your-repo-url>
cd My-Nop-Migration-Workspace\nop-working-code
```

#### 2b. Build the distribution package:
```powershell
cd deployment
.\Create-DistributionPackage.ps1
```

**Output:** `dist\nopcommerce-iis-package\` (~300 MB)

This creates a folder with:
```
dist/nopcommerce-iis-package/
├── app/              # All binaries
├── Deploy.ps1        # Simple deployment script
├── Install-Prerequisites.ps1
├── Verify-Prerequisites.ps1
└── README.md
```

#### 2c. (Optional) Zip for transfer:
```powershell
# If test servers are on different machines
Compress-Archive -Path ..\dist\nopcommerce-iis-package `
                 -DestinationPath ..\NopCommerce-Package.zip
```

---

### **Step 3: Deploy to Test Servers**

You have **two options** depending on your setup:

#### **Option A: Deploy from GCP Deployment Server** (Recommended if same network)

If test servers can access a network share:

```powershell
# On deployment server - share the package
Copy-Item -Path ..\dist\nopcommerce-iis-package `
          -Destination "\\FileServer\Deployments\NopCommerce" `
          -Recurse -Force
```

Then on each test server:
```powershell
# On test server
cd \\FileServer\Deployments\NopCommerce
.\Deploy.ps1
```

---

#### **Option B: Copy Package to Each Test Server**

If servers are isolated:

```powershell
# On deployment server - copy to test server
$testServers = @("TestServer1", "TestServer2", "TestServer3")

foreach ($server in $testServers) {
    Write-Host "Deploying to $server..." -ForegroundColor Green
    
    # Copy package
    Copy-Item -Path ..\dist\nopcommerce-iis-package `
              -Destination "\\$server\C$\Temp\nopcommerce-iis-package" `
              -Recurse -Force
    
    # Deploy remotely
    Invoke-Command -ComputerName $server -ScriptBlock {
        cd C:\Temp\nopcommerce-iis-package
        .\Deploy.ps1
    }
}
```

Or manually on each test server:
```powershell
# Copy the package folder to test server
# Then on test server:
cd C:\Temp\nopcommerce-iis-package
.\Deploy.ps1
```

---

## 🔄 Complete Workflow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. Your Dev Machine                                             │
│    - Make changes                                               │
│    - Test with Deploy-All.ps1                                   │
│    - Commit to Git                                              │
└────────────────────────┬────────────────────────────────────────┘
                         │ git push
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. GCP Deployment Server                                        │
│    - git pull                                                   │
│    - Run: Create-DistributionPackage.ps1                        │
│    - Output: dist/nopcommerce-iis-package/                      │
└────────────────────────┬────────────────────────────────────────┘
                         │ Copy/Share package
                         ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. Test Servers (TestServer1, TestServer2, etc.)               │
│    - Receive package                                            │
│    - Run: Deploy.ps1                                            │
│    - Site deployed to IIS                                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🤔 Do You Need to Zip?

**Short answer:** Only if copying between machines

| Scenario | Zip? | Why |
|----------|------|-----|
| Same network share | ❌ No | Copy folder directly |
| Remote copy (RDP/SFTP) | ✅ Yes | Easier to transfer 1 file |
| Git (NOT recommended) | ❌ No | Too large for Git |
| Artifact storage (GCS) | ✅ Yes | Upload/download 1 file |

---

## 💡 Recommended Setup for GCP

### Option 1: Network Share (Simplest)
```powershell
# On deployment server (after building)
Copy-Item ..\dist\nopcommerce-iis-package `
          -Destination "\\FileServer\Deployments\NopCommerce" -Recurse

# On test servers
\\FileServer\Deployments\NopCommerce\Deploy.ps1
```

### Option 2: Google Cloud Storage
```powershell
# On deployment server
Compress-Archive -Path ..\dist\nopcommerce-iis-package `
                 -DestinationPath NopCommerce-Package.zip

gsutil cp NopCommerce-Package.zip gs://your-bucket/deployments/

# On test servers
gsutil cp gs://your-bucket/deployments/NopCommerce-Package.zip .
Expand-Archive NopCommerce-Package.zip
cd nopcommerce-iis-package
.\Deploy.ps1
```

---

## 🚀 Quick Commands Reference

### On GCP Deployment Server:
```powershell
# 1. Pull latest code
git pull origin main

# 2. Build distribution package
cd deployment
.\Create-DistributionPackage.ps1

# 3. Output is in: ..\dist\nopcommerce-iis-package\
```

### On Test Servers:
```powershell
# If using network share:
\\DeploymentServer\Share\nopcommerce-iis-package\Deploy.ps1

# If copied locally:
cd C:\Temp\nopcommerce-iis-package
.\Deploy.ps1
```

---

## 📝 Summary

**What to check into Git:**
- ✅ All `.ps1` scripts in `deployment/`
- ✅ All `.md` documentation
- ✅ `.gitignore`
- ❌ NOT `dist/` folder
- ❌ NOT `publish/` folder

**Deployment flow:**
1. Commit scripts to Git
2. Pull on GCP deployment server
3. Run `Create-DistributionPackage.ps1` on deployment server
4. Share/copy `dist/nopcommerce-iis-package/` to test servers
5. Run `Deploy.ps1` on each test server

**Zip only if:**
- Copying between machines
- Using cloud storage (GCS)
- Transferring via RDP/SFTP

---

**Next step:** Commit the scripts to Git, then pull on your GCP server!
