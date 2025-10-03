targetScope = 'resourceGroup'


// =====================
// Parameters
// =====================

@description('The name of the environment')
@allowed([ 'development', 'test', 'production' ])
param environmentName string = 'development'

@description('The Azure location where resources will be deployed')
@allowed([ 'eastus', 'westeurope', 'centralus', 'southeastasia' ])
param location string = 'eastus'

@description('The name of the web app')
param webAppName string

@description('ASP.NET Core environment setting')
@allowed([ 'development', 'test', 'production' ])
param aspnetcoreEnvironment string = 'development'

@description('Serilog minimum log level')
@allowed([ 'Verbose', 'Debug', 'Information', 'Warning', 'Error', 'Fatal' ])
param serilogMinimumLevel string = 'Information'

@description('Health check evaluation time in seconds')
@minValue(10)
@maxValue(600)
param healthCheckEvaluationTime int = 60

@description('PostgreSQL server name (existing)')
param postgresqlServerName string = ''

@description('PostgreSQL database name (existing)')
param postgresqlDatabaseName string = ''

// JWT Configuration parameters
@description('JWT issuer')
param jwtIssuer string

@description('JWT audience')
param jwtAudience string

@description('JWT token expiration time in minutes')
@minValue(5)
@maxValue(1440)
param jwtExpirationInMinutes int = 60

@description('Seq server URL for centralized logging (optional)')
param seqServerUrl string = ''

// =====================
// Secure Parameters (expected to be set externally in Key Vault)
// =====================
@description('Database connection string (set externally in Key Vault)')
@secure()
param databaseConnectionString string = ''

@description('JWT secret (set externally in Key Vault)')
@secure()
param jwtSecret string = ''


// =====================
// Naming
// =====================
var resourceToken = uniqueString(subscription().id, location, environmentName)
var resourcePrefix = 'sar'
var appServicePlanName = '${webAppName}-plan'
var appServiceName = webAppName
var keyVaultName = 'az-${resourcePrefix}-${resourceToken}-kv'
var logAnalyticsName = 'az-${resourcePrefix}-${resourceToken}-logs'
var appInsightsName = 'az-${resourcePrefix}-${resourceToken}-ai'
var managedIdentityName = 'az-${resourcePrefix}-${resourceToken}-id'
var postgresqlName = postgresqlServerName


// =====================
// Resources
// =====================

// User-assigned managed identity for secure access
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: managedIdentityName
  location: location
  tags: {
    environment: environmentName
    project: 'saral'
    owner: 'devops'
  }
}


// Log Analytics Workspace for centralized logging
@description('Centralized logging workspace')
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  tags: {
    environment: environmentName
    project: 'saral'
    owner: 'devops'
  }
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      immediatePurgeDataOn30Days: true
    }
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}


// Application Insights for application monitoring
@description('App monitoring')
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  kind: 'web'
  tags: {
    environment: environmentName
    project: 'saral'
    owner: 'devops'
  }
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// PostgreSQL resources are not managed by this template


// Key Vault for secure storage of connection strings and secrets
@description('Key Vault for secrets')
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: {
    environment: environmentName
    project: 'saral'
    owner: 'devops'
  }
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enabledForTemplateDeployment: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }

  // Nested secrets
  resource jwtIssuerSecret 'secrets@2023-07-01' = {
    name: 'Jwt--Issuer'
    properties: {
      value: jwtIssuer
      attributes: {
        enabled: true
      }
    }
  }

  resource jwtAudienceSecret 'secrets@2023-07-01' = {
    name: 'Jwt--Audience'
    properties: {
      value: jwtAudience
      attributes: {
        enabled: true
      }
    }
  }

  resource jwtExpirationSecret 'secrets@2023-07-01' = {
    name: 'Jwt--ExpirationInMinutes'
    properties: {
      value: string(jwtExpirationInMinutes)
      attributes: {
        enabled: true
      }
    }
  }

  resource seqServerUrlSecret 'secrets@2023-07-01' = if (!empty(seqServerUrl)) {
    name: 'Serilog--WriteTo--1--Args--ServerUrl'
    properties: {
      value: seqServerUrl
      attributes: {
        enabled: true
      }
    }
  }
}

// Grant Key Vault Secrets Officer role to managed identity
resource keyVaultSecretOfficerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(keyVault.id, managedIdentity.id, 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7') // Key Vault Secrets Officer
    principalId: managedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Note: Database connection string secret is not created here; expected to be set externally in Key Vault as 'ConnectionStrings--Database'

// Note: JWT secret is not created here; expected to be set externally in Key Vault as 'Jwt--Secret'

// Store Seq server URL in Key Vault if provided (nested inside key vault above)


// App Service Plan
@description('App Service Plan for Linux')
param appServicePlanSkuName string = 'B1'
param appServicePlanSkuTier string = 'Basic'
param appServicePlanCapacity int = 1

resource appServicePlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSkuName
    tier: appServicePlanSkuTier
    size: appServicePlanSkuName
    family: take(appServicePlanSkuName, 1)
    capacity: appServicePlanCapacity
  }
  tags: {
    environment: environmentName
    project: 'saral'
    owner: 'devops'
  }
  properties: {
    reserved: true // Required for Linux App Service
    targetWorkerCount: appServicePlanCapacity
    targetWorkerSizeId: 0
  }
}


// App Service
@description('Main Web App')
resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: {
    environment: environmentName
    project: 'saral'
    owner: 'devops'
  }
  properties: {
    serverFarmId: appServicePlan.id
    reserved: true
    httpsOnly: true
    clientAffinityEnabled: false
    publicNetworkAccess: 'Enabled'
    keyVaultReferenceIdentity: managedIdentity.id
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'Disabled'
      http20Enabled: true
      healthCheckPath: '/health'
      cors: {
        allowedOrigins: ['*']
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: aspnetcoreEnvironment
        }
        {
          name: 'Serilog__MinimumLevel__Default'
          value: serilogMinimumLevel
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'HealthChecks__UI__EvaluationTimeInSeconds'
          value: string(healthCheckEvaluationTime)
        }
        {
          name: 'ConnectionStrings__Database'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=ConnectionStrings--Database)'
        }
        {
          name: 'Jwt__Secret'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Jwt--Secret)'
        }
        {
          name: 'Jwt__Issuer'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Jwt--Issuer)'
        }
        {
          name: 'Jwt__Audience'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Jwt--Audience)'
        }
        {
          name: 'Jwt__ExpirationInMinutes'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Jwt--ExpirationInMinutes)'
        }
      ]
      connectionStrings: !empty(seqServerUrl) ? [
        {
          name: 'SeqServerUrl'
          connectionString: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=Serilog--WriteTo--1--Args--ServerUrl)'
          type: 'Custom'
        }
      ] : []
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
    }
  }
}

// Diagnostic Settings for App Service
resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: appService
  name: 'appservice-diagnostics'
  properties: {
    workspaceId: logAnalytics.id
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceAuditLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceIPSecAuditLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServicePlatformLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}


// =====================
// Outputs
// =====================
output APPLICATIONINSIGHTS_CONNECTION_STRING string = appInsights.properties.ConnectionString
output AZURE_KEY_VAULT_ENDPOINT string = keyVault.properties.vaultUri
output WEB_APP_NAME string = appService.name
output WEB_APP_URL string = 'https://${appService.properties.defaultHostName}'
output POSTGRESQL_SERVER_NAME string = empty(postgresqlName) ? '' : postgresqlName
output POSTGRESQL_DATABASE_NAME string = empty(postgresqlDatabaseName) ? '' : postgresqlDatabaseName
output MANAGED_IDENTITY_CLIENT_ID string = managedIdentity.properties.clientId
output MANAGED_IDENTITY_NAME string = managedIdentity.name
output LOG_ANALYTICS_WORKSPACE_ID string = logAnalytics.id
output APPINSIGHTS_RESOURCE_ID string = appInsights.id
output RESOURCE_GROUP_LOCATION string = location
