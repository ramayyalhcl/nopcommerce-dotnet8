# Install form values (for agent independent workflow)

**Auth:** Windows authentication (do not use SQL login).

| Field | Value |
|-------|--------|
| SqlConnectionInfo | sqlconnectioninfo_values |
| SqlAuthenticationType | windowsauthentication |
| SqlServerName | localhost\MSSQLSERVER01 |
| SqlDatabaseName | nopecommerce |
| SqlServerCreateDatabase | true |
| AdminEmail | ram@storeadmin.com |
| AdminPassword | admin |
| InstallSampleData | true |

**Connection string** (reference; form uses values above):  
`Data Source=localhost\MSSQLSERVER01;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False;Command Timeout=0`

When building connection for install, use **Initial Catalog=nopecommerce** (not master) for the target database.
