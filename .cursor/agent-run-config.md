# Agent independent run config

Used by the agent to build, run, and test the app (e.g. Install page) without manual steps. **User provides only form variable field data (values); agent gets form structure from codebase.**

---

## Workspace rule (single file, equal importance)

- **Rule:** `.cursor/rules/workspace-operating-context.mdc` — the agent must follow **all** sections with equal importance: (1) No code changes without your approval; (2) Checkpoints before complex work; (3) Migration diagnostic protocol. Propose → ask "Shall I apply?" → wait for yes → then apply.

---

## IMPORTANT: Do not hold the app process

- **Do NOT start `dotnet watch run` or `dotnet run` in the background** when the user needs to run the app and see logs (or attach a debugger, or use a different port). A background process holds the port and locks `bin\Debug\net8.0\*.dll`, so the user cannot build/run or see logs.
- **Preferred:** User runs the app in their own terminal (e.g. `dotnet watch run` or `dotnet run` on their chosen port). Agent reads logs from the terminal output files in `terminals/` or from pasted logs. Agent only runs short-lived commands (e.g. `dotnet build`, or one-off tests).
- **If the agent must run the app:** Use a short run (e.g. `dotnet run` with a timeout) or document that the user should stop the background process when done. Before suggesting a build, remind the user to stop any running Nop.Web/dotnet process if they see "The process cannot access the file ... Nop.Data.dll".

---

## Checkpoints & rollback

- Covered in `.cursor/rules/workspace-operating-context.mdc` (section 2). Before complex fixes, the agent creates a checkpoint (git commit/branch). **Rollback:** `git checkout .` or `git reset --hard <commit-hash>`.

---

## Startup project

- **Path (from workspace root):** `nop-working-code/src/Presentation/Nop.Web`
- **Absolute (Windows):** `c:\Users\avish\Ram\My-Nop-Migration-Workspace\nop-working-code\src\Presentation\Nop.Web`
- **Project file:** `nop-working-code\src\Presentation\Nop.Web\Nop.Web.csproj`

---

## URL and port

- **Base URL:** `http://127.0.0.1:5001`
- **Install page:** `http://127.0.0.1:5001/install` or `http://127.0.0.1:5001/Install`
- **Port:** 5001 (`ASPNETCORE_URLS="http://127.0.0.1:5001"`)

---

## Commands (from workspace root)

**Build:**
```powershell
dotnet build "nop-working-code\src\Presentation\Nop.Web\Nop.Web.csproj"
```

**Run (user runs in their terminal; agent does not start in background)** — only one instance per port. User can use any port (e.g. 5001 or 5002) and see logs:
```powershell
cd "nop-working-code\src\Presentation\Nop.Web"
$env:ASPNETCORE_URLS="http://127.0.0.1:5001"; dotnet watch run
# Or: $env:ASPNETCORE_URLS="http://127.0.0.1:5002"; dotnet run
```
Without watch (manual build then run):
```powershell
$env:ASPNETCORE_URLS="http://127.0.0.1:5001"; dotnet run --project "nop-working-code\src\Presentation\Nop.Web\Nop.Web.csproj" --no-build
```

**Stop process before build** — if build fails with "file is being used by another process", find and kill the Nop.Web/dotnet process:
```powershell
Get-Process -Name "Nop.Web" -ErrorAction SilentlyContinue | Stop-Process -Force
# Or find PID from build error (e.g. "25476") then: Stop-Process -Id 25476 -Force
```

**From project directory:**
```powershell
cd "nop-working-code\src\Presentation\Nop.Web"
$env:ASPNETCORE_URLS="http://127.0.0.1:5001"
dotnet run --no-build
```

---

## Agent independent workflow

1. **Build** — Run `dotnet build` on Nop.Web from workspace root.
2. **Run** — Start app in background (same port = only one instance; stop existing if needed).
3. **Form structure** — Agent reads from codebase: `InstallModel`, `Views/Install/Index.cshtml` (field names, types, validation). No need for user to paste form structure.
4. **Form values** — User provides only **form variable field data** (e.g. DB server, database name, admin email, password). Agent uses these when filling the install form in browser MCP.
5. **Test (Install page)** — Agent: navigate to install URL (browser MCP), fill form (structure from code + values from user), submit, take snapshot for result (success, 404, validation errors).
6. **See errors** — Build: command output. Runtime: read terminal log file for the background process (see "Where agent sees logs" below). User-facing: browser MCP snapshot after submit.
7. **Fix** — Edit code from errors, rebuild, re-run if needed, re-test.

---

## Where agent sees logs

- **Log file (preferred):** The app writes logs to a file so the agent can read them even when the user runs the app in their own terminal. Path (after at least one run):
  - `nop-working-code\src\Presentation\Nop.Web\bin\Debug\net8.0\App_Data\logs\agent-app.log`
  - Or from workspace root: `nop-working-code/src/Presentation/Nop.Web/bin/Debug/net8.0/App_Data/logs/agent-app.log`
  - Agent: read this file to see Install flow, exceptions, and server-side messages after the user submits the form.
- **Terminal output:** If the app was run by the agent in background, terminal files in `terminals/` — read the `.txt` for that shell.
- **Browser MCP:** Snapshot after submit shows page result (success, 404, validation messages).

---

## Form values from user

- User supplies **only form variable field data** (values).
- **Saved values:** `.cursor/agent-install-form-values.md` — agent reads this when running the Install test flow (Windows auth; SqlServerName, SqlDatabaseName, AdminEmail, AdminPassword, InstallSampleData, etc.).
- Agent uses codebase for field names and form layout; values from that file (or from user in chat) are used when filling the form in browser MCP.

---

## Optional: test DB credentials

If user provides one set of valid DB credentials for their environment, agent uses them for full install submit tests. Otherwise agent uses placeholders and fixes validation/connection errors as they appear.
