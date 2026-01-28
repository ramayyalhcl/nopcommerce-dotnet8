# nopCommerce .NET 8.0 - Launch Instructions

## ✅ Application Status

**The application is currently running!**

- **URL**: http://localhost:5001
- **Status**: Running
- **Environment**: Development
- **Port**: 5001 (changed from 5000 due to port conflict)

## 🚀 How to Launch (Option 3 - Build then Run)

### Step 1: Build the Project
```powershell
cd c:\Users\avish\Ram\nopcommerce-dotnet8\src\Presentation\Nop.Web
dotnet build
```

### Step 2: Run the Application
```powershell
dotnet run --no-build
```

Or with a specific port:
```powershell
$env:ASPNETCORE_URLS="http://localhost:5001"
dotnet run --no-build
```

## 🌐 Access the Application

Open your web browser and navigate to:
**http://localhost:5001**

## 📋 What You Should See

When you first access the application, you may see one of the following:

1. **Installation Page** - If the database is not configured, you'll see the nopCommerce installation wizard
2. **Home Page** - If the database is already set up, you'll see the nopCommerce storefront
3. **Error Page** - If there are configuration issues, you may see an error message

## 🔧 Troubleshooting

### Port Already in Use
If you get a port binding error, change the port:
```powershell
$env:ASPNETCORE_URLS="http://localhost:8080"
dotnet run --no-build
```

### Database Connection Issues
If you see database-related errors, you'll need to:
1. Configure the connection string in `appsettings.json` or `Web.config`
2. Run the database migrations
3. Complete the installation wizard

### Stop the Application
Press `Ctrl+C` in the terminal where the application is running

## 📝 Notes

- The application is configured to run on HTTP (not HTTPS) in development mode
- HTTPS redirection is disabled in development to avoid certificate issues
- The application uses Autofac for dependency injection
- All nopCommerce services are registered via the DependencyRegistrar pattern
