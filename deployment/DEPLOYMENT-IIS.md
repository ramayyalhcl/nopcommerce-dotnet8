  # NopCommerce .NET 8.0 – IIS Deployment

  Deploy NopCommerce .NET 8.0 to IIS 8+ (Windows Server or Windows 10/11).

  ---

  ## What Gets Installed

  | Component | What it includes |
  |-----------|------------------|
  | **IIS** | Web server, ASP.NET 4.x, WebSockets, compression, management tools |
  | **.NET 8.0 Hosting Bundle** | .NET 8 Runtime + ASP.NET Core 8 Runtime + ASP.NET Core Module (ANCM) for IIS |
  | **IIS URL Rewrite** | HTTPS redirects, clean URLs |
  | **Nop.Web** | Your application files (from the `app` folder or published by the script) |

  **Requirements:** Administrator rights, internet (for downloads). SQL Server for the database (you configure this in the NopCommerce install wizard).

**App pool:** The script sets the pool to **No Managed Code** – this is correct for .NET 8.0. IIS does not load .NET; the ASP.NET Core Module (from the Hosting Bundle) runs your app with .NET 8.

  ---

  ## How to Run

  **Run PowerShell as Administrator.** If scripts are blocked, run once:
  ```powershell
  Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
  ```

  ### Option 1: Single script (recommended)

  Does everything: prerequisites → IIS setup → Nop.Web deployment.

  ```powershell
  cd deployment
  .\Deploy-All.ps1
  ```

  - **App source:** Put published Nop.Web in the **`app`** subfolder next to the script.  
    Or run from the repo (script in `nop-working-code\deployment`): it will publish into `app` and then deploy.
  - **Custom site/port:** `.\Deploy-All.ps1 -SiteName "MyShop" -Port 8080`
  - **Custom app folder:** `.\Deploy-All.ps1 -AppSourcePath "D:\Build\publish\nopcommerce"`
  - **Skip prerequisites:** `.\Deploy-All.ps1 -SkipPrerequisites`

  **Folder layout:**
  ```
  deployment\
    Deploy-All.ps1
    Install-Prerequisites.ps1
    Verify-Prerequisites.ps1
    app\          <-- put published Nop.Web here (Nop.Web.dll, wwwroot, etc.)
  ```

  ---

  ### Option 2: Run scripts separately

  ```powershell
  # 1. Install prerequisites only (IIS, .NET 8 Hosting Bundle, URL Rewrite)
  .\deployment\Install-Prerequisites.ps1

  # 2. Deploy app (publish + IIS site + copy files). Use -SkipPublish to use existing publish folder.
  .\deployment\Deploy-IIS.ps1 -SiteName "NopCommerce" -Port 80
  ```

  **Other scripts:**
  - **Verify prerequisites only:** `.\deployment\Verify-Prerequisites.ps1`
  - **Publish only (no IIS):** `.\deployment\Publish-Only.ps1` → output in `publish\nopcommerce-iis`

  ---

  ## After Deployment

  1. Open the site URL (e.g. `http://localhost` or `http://yourserver:80`).
  2. Complete the NopCommerce installation wizard (database, admin account, optional sample data).
  3. For HTTPS: add a binding in IIS and (if needed) a URL Rewrite rule to redirect HTTP to HTTPS.

  ---

  ## Quick troubleshooting

  - **500.19 / ANCM missing:** Install or repair .NET 8.0 **Hosting Bundle** (not only SDK/Developer Bundle), then `iisreset`.
  - **502.5 / app crash:** In site folder, check `logs\` and/or set `stdoutLogEnabled="true"` in `web.config`, then check `logs\stdout`.
  - **Scripts disabled:** Run `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser` in an elevated PowerShell.
