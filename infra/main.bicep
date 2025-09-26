targetScope = 'subscription'

// Parameters
@description('The name of the environment')
param environmentName string = 'development'

@description('The Azure location where resources will be deployed')
param location string = 'eastus'

@description('The name of the resource group')
param resourceGroupName string = 'saral'

@description('The name of the web app')
param webAppName string = 'saral-backend-app'

// Additional environment variables
@description('ASP.NET Core environment setting')
param aspnetcoreEnvironment string = 'development'

@description('Serilog minimum log level')
param serilogMinimumLevel string = 'Information'

@description('Health check evaluation time in seconds')
param healthCheckEvaluationTime int = 30

// Database connection parameters
@description('PostgreSQL server name')
param postgresqlServerName string = 'saral-dev-db'

@description('PostgreSQL database name')
param postgresqlDatabaseName string = 'saral-dev-db'

// PostgreSQL admin username/password not managed here; server treated as existing

// JWT Configuration parameters
// JWT secret not managed here; expected in Key Vault already

@description('JWT issuer')
param jwtIssuer string = 'dev.saral.backend'

@description('JWT audience')
param jwtAudience string = 'dev.saral.frontend'

@description('JWT token expiration time in minutes')
param jwtExpirationInMinutes int = 60

// Logging configuration
@description('Seq server URL for centralized logging')
param seqServerUrl string = 'http://seq:5341'

// Resource Group - Create if it doesn't exist
resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: resourceGroupName
  location: location
  tags: {
    environment: environmentName
  }
}

// Deploy resources to the resource group using a module
module resources 'resources.bicep' = {
  name: 'saral-resources'
  scope: resourceGroup
  params: {
    environmentName: environmentName
    location: location
    webAppName: webAppName
    aspnetcoreEnvironment: aspnetcoreEnvironment
    serilogMinimumLevel: serilogMinimumLevel
    healthCheckEvaluationTime: healthCheckEvaluationTime
    postgresqlServerName: postgresqlServerName
    postgresqlDatabaseName: postgresqlDatabaseName
    jwtIssuer: jwtIssuer
    jwtAudience: jwtAudience
    jwtExpirationInMinutes: jwtExpirationInMinutes
    seqServerUrl: seqServerUrl
  }
}

// Output values
output RESOURCE_GROUP_ID string = resourceGroup.id
output AZURE_LOCATION string = location
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.APPLICATIONINSIGHTS_CONNECTION_STRING
output AZURE_KEY_VAULT_ENDPOINT string = resources.outputs.AZURE_KEY_VAULT_ENDPOINT
output WEB_APP_NAME string = resources.outputs.WEB_APP_NAME
output WEB_APP_URL string = resources.outputs.WEB_APP_URL
output POSTGRESQL_SERVER_NAME string = resources.outputs.POSTGRESQL_SERVER_NAME
output POSTGRESQL_DATABASE_NAME string = resources.outputs.POSTGRESQL_DATABASE_NAME
output MANAGED_IDENTITY_CLIENT_ID string = resources.outputs.MANAGED_IDENTITY_CLIENT_ID
output MANAGED_IDENTITY_NAME string = resources.outputs.MANAGED_IDENTITY_NAME
