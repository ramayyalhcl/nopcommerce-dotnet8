using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Services.Installation;
using Nop.Services.Security;
using Nop.Data;
using Nop.Web.Framework.Security;
using Nop.Web.Infrastructure.Installation;
using Nop.Web.Models.Install;
using Microsoft.EntityFrameworkCore;

namespace Nop.Web.Controllers
{
    public partial class InstallController : BasePublicController
    {
        #region Fields

        private readonly IInstallationLocalizationService _locService;
        private readonly NopConfig _config;
        private readonly ILogger<InstallController> _logger;

        #endregion

        #region Ctor

        public InstallController(IInstallationLocalizationService locService, NopConfig config, ILogger<InstallController> logger)
        {
            this._locService = locService;
            this._config = config;
            this._logger = logger;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// A value indicating whether we use MARS (Multiple Active Result Sets)
        /// </summary>
        protected virtual bool UseMars
        {
            get { return false; }
        }

        /// <summary>
        /// Checks if the specified database exists, returns true if database exists
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Returns true if the database exists.</returns>
        [NonAction]
        protected virtual bool SqlServerDatabaseExists(string connectionString)
        {
            try
            {
                //just try to connect
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a database on the server.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <param name="collation">Server collation; the default one will be used if not specified</param>
        /// <param name="triesToConnect">
        /// Number of times to try to connect to database. 
        /// If connection cannot be open, then error will be returned. 
        /// Pass 0 to skip this validation.
        /// </param>
        /// <returns>Error</returns>
        [NonAction]
        protected virtual string CreateDatabase(string connectionString, string collation, int triesToConnect = 10)
        {
            try
            {
                //parse database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog;
                //now create connection string to 'master' dabatase. It always exists.
                builder.InitialCatalog = "master";
                var masterCatalogConnectionString = builder.ToString();
                string query = string.Format("CREATE DATABASE [{0}]", databaseName);
                if (!String.IsNullOrWhiteSpace(collation))
                    query = string.Format("{0} COLLATE {1}", query, collation);
                using (var conn = new SqlConnection(masterCatalogConnectionString))
                {
                    conn.Open();
                    using (var command = new SqlCommand(query, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                //try connect
                if (triesToConnect > 0)
                {
                    //Sometimes on slow servers (hosting) there could be situations when database requires some time to be created.
                    //But we have already started creation of tables and sample data.
                    //As a result there is an exception thrown and the installation process cannot continue.
                    //That's why we are in a cycle of "triesToConnect" times trying to connect to a database with a delay of one second.
                    for (var i = 0; i <= triesToConnect; i++)
                    {
                        if (i == triesToConnect)
                            throw new Exception("Unable to connect to the new database. Please try one more time");

                        if (!this.SqlServerDatabaseExists(connectionString))
                            Thread.Sleep(1000);
                        else
                            break;
                    }
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                return string.Format(_locService.GetResource("DatabaseCreationError"), ex.Message);
            }
        }

        /// <summary>
        /// Create contents of connection strings used by the SqlConnection class
        /// </summary>
        /// <param name="trustedConnection">Avalue that indicates whether User ID and Password are specified in the connection (when false) or whether the current Windows account credentials are used for authentication (when true)</param>
        /// <param name="serverName">The name or network address of the instance of SQL Server to connect to</param>
        /// <param name="databaseName">The name of the database associated with the connection</param>
        /// <param name="userName">The user ID to be used when connecting to SQL Server</param>
        /// <param name="password">The password for the SQL Server account</param>
        /// <param name="timeout">The connection timeout</param>
        /// <returns>Connection string</returns>
        [NonAction]
        protected virtual string CreateConnectionString(bool trustedConnection,
            string serverName, string databaseName,
            string userName, string password, int timeout = 0)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.IntegratedSecurity = trustedConnection;
            builder.DataSource = serverName;
            builder.InitialCatalog = databaseName;
            if (!trustedConnection)
            {
                builder.UserID = userName;
                builder.Password = password;
            }
            builder.PersistSecurityInfo = false;
            if (this.UseMars)
            {
                builder.MultipleActiveResultSets = true;
            }
            if (timeout > 0)
            {
                builder.ConnectTimeout = timeout;
            }
            return builder.ConnectionString;
        }

        #endregion

        #region Methods

        public virtual ActionResult Index()
        {
            _logger.LogDebug("Install Index GET requested");
            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                _logger.LogInformation("Database already installed; redirecting to HomePage");
                return RedirectToRoute("HomePage");
            }

            //set page timeout to 5 minutes
            // this.Server.ScriptTimeout = 300; - Server doesn't exist in ASP.NET Core (use middleware timeout)


            var model = new InstallModel
            {
                AdminEmail = "admin@yourStore.com",
                InstallSampleData = false,
                DatabaseConnectionString = "",
                DataProvider = "sqlserver",
                //fast installation service does not support SQL compact
                DisableSqlCompact = _config.UseFastInstallationService,
                DisableSampleDataOption = _config.DisableSampleDataDuringInstallation,
                SqlAuthenticationType = "sqlauthentication",
                SqlConnectionInfo = "sqlconnectioninfo_values",
                SqlServerCreateDatabase = false,
                UseCustomCollation = false,
                Collation = "SQL_Latin1_General_CP1_CI_AS",
            };
            foreach (var lang in _locService.GetAvailableLanguages())
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = lang.Code }),
                    Text = lang.Name,
                    Selected = _locService.GetCurrentLanguage().Code == lang.Code,
                });
            }

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Index([Bind(Prefix = "")] InstallModel model)
        {
            // DEBUG: Log raw form data so we can see what the server receives (writes to log file)
            _logger.LogInformation("Install POST: Request Method={Method}, ContentType={ContentType}",
                Request?.Method, Request?.ContentType);
            if (Request?.Form != null)
            {
                foreach (string key in Request.Form.Keys)
                {
                    var value = Request.Form[key];
                    _logger.LogInformation("Install POST Form[{Key}]={Value}", key, value.ToString());
                }
            }

            if (model == null)
                model = new InstallModel();
            if (model.AvailableLanguages == null)
                model.AvailableLanguages = new List<SelectListItem>();

            _logger.LogInformation("Install Index POST received. DataProvider={DataProvider}, InstallSampleData={InstallSampleData}, SqlConnectionInfo={SqlConnectionInfo}, SqlServerName={SqlServerName}, AdminEmail={AdminEmail}",
                model?.DataProvider, model?.InstallSampleData, model?.SqlConnectionInfo, model?.SqlServerName, model?.AdminEmail);
            _logger.LogInformation("Install POST ModelState: IsValid={IsValid}, ErrorCount={ErrorCount}",
                ModelState?.IsValid, ModelState?.ErrorCount ?? 0);

            // Fallback: if model binding left key fields null but form has them, bind from form (e.g. wrong prefix)
            if (Request?.Form != null && string.IsNullOrEmpty(model?.DataProvider) && !string.IsNullOrEmpty(Request.Form["DataProvider"]))
            {
                _logger.LogInformation("Install POST: Binding from Request.Form (prefix fallback)");
                model.DataProvider = Request.Form["DataProvider"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["SqlConnectionInfo"])) model.SqlConnectionInfo = Request.Form["SqlConnectionInfo"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["SqlAuthenticationType"])) model.SqlAuthenticationType = Request.Form["SqlAuthenticationType"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["SqlServerName"])) model.SqlServerName = Request.Form["SqlServerName"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["SqlDatabaseName"])) model.SqlDatabaseName = Request.Form["SqlDatabaseName"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["AdminEmail"])) model.AdminEmail = Request.Form["AdminEmail"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["AdminPassword"])) model.AdminPassword = Request.Form["AdminPassword"].ToString();
                if (Request.Form.ContainsKey("InstallSampleData")) model.InstallSampleData = string.Equals(Request.Form["InstallSampleData"], "true", StringComparison.OrdinalIgnoreCase) || Request.Form["InstallSampleData"].ToString().Contains("true");
                if (!string.IsNullOrEmpty(Request.Form["DatabaseConnectionString"])) model.DatabaseConnectionString = Request.Form["DatabaseConnectionString"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["SqlServerUsername"])) model.SqlServerUsername = Request.Form["SqlServerUsername"].ToString();
                if (!string.IsNullOrEmpty(Request.Form["SqlServerPassword"])) model.SqlServerPassword = Request.Form["SqlServerPassword"].ToString();
                if (Request.Form.ContainsKey("SqlServerCreateDatabase")) model.SqlServerCreateDatabase = string.Equals(Request.Form["SqlServerCreateDatabase"], "true", StringComparison.OrdinalIgnoreCase) || Request.Form["SqlServerCreateDatabase"].ToString().Contains("true");
                if (Request.Form.ContainsKey("UseCustomCollation")) model.UseCustomCollation = string.Equals(Request.Form["UseCustomCollation"], "true", StringComparison.OrdinalIgnoreCase) || Request.Form["UseCustomCollation"].ToString().Contains("true");
                if (!string.IsNullOrEmpty(Request.Form["Collation"])) model.Collation = Request.Form["Collation"].ToString();
            }

            if (DataSettingsHelper.DatabaseIsInstalled())
            {
                _logger.LogInformation("Database already installed; redirecting to HomePage");
                return RedirectToRoute("HomePage");
            }

            //set page timeout to 5 minutes
            // this.Server.ScriptTimeout = 300; - Server doesn't exist in ASP.NET Core (use middleware timeout)

            if (model.DatabaseConnectionString != null)
                model.DatabaseConnectionString = model.DatabaseConnectionString.Trim();


            //prepare language list
            foreach (var lang in _locService.GetAvailableLanguages())
            {
                model.AvailableLanguages.Add(new SelectListItem
                {
                    Value = Url.Action("ChangeLanguage", "Install", new { language = lang.Code }),
                    Text = lang.Name,
                    Selected = _locService.GetCurrentLanguage().Code == lang.Code,
                });
            }

            model.DisableSqlCompact = _config.UseFastInstallationService;
            model.DisableSampleDataOption = _config.DisableSampleDataDuringInstallation;

            if (string.IsNullOrEmpty(model.DataProvider))
                ModelState.AddModelError("", _locService.GetResource("DataProviderRequired"));

            //SQL Server
            if (string.Equals(model.DataProvider, "sqlserver", StringComparison.InvariantCultureIgnoreCase))
            {
                if (string.Equals(model.SqlConnectionInfo, "sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
                {
                    //raw connection string
                    if (string.IsNullOrEmpty(model.DatabaseConnectionString))
                        ModelState.AddModelError("", _locService.GetResource("ConnectionStringRequired"));

                    try
                    {
                        //try to create connection string
                        new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                    }
                    catch
                    {
                        ModelState.AddModelError("", _locService.GetResource("ConnectionStringWrongFormat"));
                    }
                }
                else
                {
                    //values
                    if (string.IsNullOrEmpty(model.SqlServerName))
                        ModelState.AddModelError("", _locService.GetResource("SqlServerNameRequired"));
                    if (string.IsNullOrEmpty(model.SqlDatabaseName))
                        ModelState.AddModelError("", _locService.GetResource("DatabaseNameRequired"));

                    //authentication type
                    if (string.Equals(model.SqlAuthenticationType, "sqlauthentication", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //SQL authentication
                        if (string.IsNullOrEmpty(model.SqlServerUsername))
                            ModelState.AddModelError("", _locService.GetResource("SqlServerUsernameRequired"));
                        if (string.IsNullOrEmpty(model.SqlServerPassword))
                            ModelState.AddModelError("", _locService.GetResource("SqlServerPasswordRequired"));
                    }
                }
            }


            //Consider granting access rights to the resource to the ASP.NET request identity. 
            //ASP.NET has a base process identity 
            //(typically {MACHINE}\ASPNET on IIS 5 or Network Service on IIS 6 and IIS 7, 
            //and the configured application pool identity on IIS 7.5) that is used if the application is not impersonating.
            //If the application is impersonating via <identity impersonate="true"/>, 
            //the identity will be the anonymous user (typically IUSR_MACHINENAME) or the authenticated request user.
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            //validate permissions
            var dirsToCheck = FilePermissionHelper.GetDirectoriesWrite();
            foreach (string dir in dirsToCheck)
                if (!FilePermissionHelper.CheckPermissions(dir, false, true, true, false))
                    ModelState.AddModelError("", string.Format(_locService.GetResource("ConfigureDirectoryPermissions"), WindowsIdentity.GetCurrent().Name, dir));

            var filesToCheck = FilePermissionHelper.GetFilesWrite();
            foreach (string file in filesToCheck)
                if (!FilePermissionHelper.CheckPermissions(file, false, true, true, true))
                    ModelState.AddModelError("", string.Format(_locService.GetResource("ConfigureFilePermissions"), WindowsIdentity.GetCurrent().Name, file));

            if (ModelState.IsValid)
            {
                var settingsManager = new DataSettingsManager();
                try
                {
                    _logger.LogInformation("Install validation passed; starting database and data setup");
                    string connectionString;
                    if (string.Equals(model.DataProvider, "sqlserver", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //SQL Server

                        if (string.Equals(model.SqlConnectionInfo, "sqlconnectioninfo_raw", StringComparison.InvariantCultureIgnoreCase))
                        {
                            //raw connection string

                            //we know that MARS option is required when using Entity Framework
                            //let's ensure that it's specified
                            var sqlCsb = new SqlConnectionStringBuilder(model.DatabaseConnectionString);
                            if (this.UseMars)
                            {
                                sqlCsb.MultipleActiveResultSets = true;
                            }
                            connectionString = sqlCsb.ToString();
                        }
                        else
                        {
                            //values
                            connectionString = CreateConnectionString(model.SqlAuthenticationType == "windowsauthentication",
                                model.SqlServerName, model.SqlDatabaseName,
                                model.SqlServerUsername, model.SqlServerPassword);
                        }

                        if (model.SqlServerCreateDatabase)
                        {
                            if (!SqlServerDatabaseExists(connectionString))
                            {
                                //create database
                                var collation = model.UseCustomCollation ? model.Collation : "";
                                var errorCreatingDatabase = CreateDatabase(connectionString, collation);
                                if (!String.IsNullOrEmpty(errorCreatingDatabase))
                                    throw new Exception(errorCreatingDatabase);
                            }
                        }
                        else
                        {
                            //check whether database exists
                            if (!SqlServerDatabaseExists(connectionString))
                                throw new Exception(_locService.GetResource("DatabaseNotExists"));
                        }
                    }
                    else
                    {
                        //SQL CE
                        string databaseFileName = "Nop.Db.sdf";
                        string databasePath = @"|DataDirectory|\" + databaseFileName;
                        connectionString = "Data Source=" + databasePath + ";Persist Security Info=False";

                        //drop database if exists
                        string databaseFullPath = CommonHelper.MapPath("~/App_Data/") + databaseFileName;
                        if (System.IO.File.Exists(databaseFullPath))
                        {
                            System.IO.File.Delete(databaseFullPath);
                        }
                    }

                    //save settings
                    var dataProvider = model.DataProvider;
                    var settings = new DataSettings
                    {
                        DataProvider = dataProvider,
                        DataConnectionString = connectionString
                    };
                    settingsManager.SaveSettings(settings);
                    _logger.LogInformation("Data settings saved. DataProvider={DataProvider}", dataProvider);

                    //init data provider
                    var dataProviderInstance = EngineContext.Current.Resolve<BaseDataProviderManager>().LoadDataProvider();
                    dataProviderInstance.InitDatabase();
                    _logger.LogInformation("Database initialized");

                    // Ensure schema exists (EF Core: InitDatabase is no-op; create tables from model)
                    var dbContext = EngineContext.Current.Resolve<NopObjectContext>();
                    dbContext.Database.EnsureCreated();
                    _logger.LogInformation("Database schema ensured (EnsureCreated).");

                    //now resolve installation service
                    var installationService = EngineContext.Current.Resolve<IInstallationService>();
                    _logger.LogInformation("Installing seed data. InstallSampleData={InstallSampleData}", model.InstallSampleData);
                    installationService.InstallData(model.AdminEmail, model.AdminPassword, model.InstallSampleData);

                    //reset cache
                    DataSettingsHelper.ResetCache();
                    _logger.LogInformation("Cache reset; installing plugins");

                    //install plugins
                    PluginManager.MarkAllPluginsAsUninstalled();
                    var pluginFinder = EngineContext.Current.Resolve<IPluginFinder>();
                    var plugins = pluginFinder.GetPlugins<IPlugin>(LoadPluginsMode.All)
                        .ToList()
                        .OrderBy(x => x.PluginDescriptor.Group)
                        .ThenBy(x => x.PluginDescriptor.DisplayOrder)
                        .ToList();
                    var pluginsIgnoredDuringInstallation = String.IsNullOrEmpty(_config.PluginsIgnoredDuringInstallation) ?
                        new List<string>() :
                        _config.PluginsIgnoredDuringInstallation
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();
                    foreach (var plugin in plugins)
                    {
                        if (pluginsIgnoredDuringInstallation.Contains(plugin.PluginDescriptor.SystemName))
                            continue;
                        plugin.Install();
                    }

                    //register default permissions
                    //var permissionProviders = EngineContext.Current.Resolve<ITypeFinder>().FindClassesOfType<IPermissionProvider>();
                    var permissionProviders = new List<Type>();
                    permissionProviders.Add(typeof(StandardPermissionProvider));
                    foreach (var providerType in permissionProviders)
                    {
                        dynamic provider = Activator.CreateInstance(providerType);
                        EngineContext.Current.Resolve<IPermissionService>().InstallPermissions(provider);
                    }

                    // Redirect to home page (skip RestartAppDomain so app stays running and user reaches landing page)
                    _logger.LogInformation("Install completed successfully; redirecting to HomePage");
                    return RedirectToRoute("HomePage");
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Install failed. DataProvider={DataProvider}, Message={Message}", model?.DataProvider, exception.Message);
                    //reset cache
                    DataSettingsHelper.ResetCache();

                    var cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
                    cacheManager.Clear();

                    //clear provider settings if something got wrong
                    settingsManager.SaveSettings(new DataSettings
                    {
                        DataProvider = null,
                        DataConnectionString = null
                    });

                    ModelState.AddModelError("", string.Format(_locService.GetResource("SetupFailed"), exception.Message));
                }
            }
            else
            {
                _logger.LogWarning("Install POST validation failed. ErrorCount={ErrorCount}", ModelState.ErrorCount);
            }
            return View(model);
        }

        public virtual ActionResult ChangeLanguage(string language)
        {
            _logger.LogDebug("Install ChangeLanguage. Language={Language}", language);
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            _locService.SaveCurrentLanguage(language);

            //Reload the page
            return RedirectToAction("Index", "Install");
        }

        [HttpPost]
        public virtual ActionResult RestartInstall()
        {
            _logger.LogInformation("Install RestartInstall requested");
            if (DataSettingsHelper.DatabaseIsInstalled())
                return RedirectToRoute("HomePage");

            //restart application
            var webHelper = EngineContext.Current.Resolve<IWebHelper>();
            webHelper.RestartAppDomain();

            //Redirect to home page
            return RedirectToRoute("HomePage");
        }

        #endregion
    }
}
