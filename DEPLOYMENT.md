# Deployment Setup Guide

## Required Azure DevOps Variables

Before deploying your Saral Backend application, you need to configure the following variables in Azure DevOps as **secret variables**:

### 1. Azure Service Connection

You need to create an Azure Resource Manager service connection in Azure DevOps:

#### Steps to Create Service Connection:
1. **Navigate to Azure DevOps Project Settings**:
   - Go to your Azure DevOps project
   - Click **Project Settings** (bottom left)
   - Under **Pipelines**, click **Service connections**

2. **Create New Service Connection**:
   - Click **"New service connection"**
   - Select **"Azure Resource Manager"** 
   - Choose **"Service principal (automatic)"**

3. **Configure Service Connection**:
   - **Subscription**: Select your Azure subscription
   - **Resource group**: Leave empty (for subscription-level access)
   - **Service connection name**: `azureServiceConnection` (or update the pipeline variable)
   - **Description**: "Service connection for Saral Backend deployment"
   - ✅ Check **"Grant access permission to all pipelines"**
   - Click **"Save"**

#### Alternative: Update Pipeline Variable
If you prefer to use a different service connection name, update this variable in `azure-pipelines.yml`:
```yaml
variables:
  azureServiceConnection: 'YourServiceConnectionName'
```

### 2. Required Secret Variables

Set these as **secret variables** in your Azure DevOps pipeline:

#### Database Configuration
- **POSTGRESQL_ADMIN_PASSWORD**
  - **Type**: Secret
  - **Description**: Strong password for PostgreSQL administrator
  - **Example**: `MyStr0ngP@ssw0rd123!`

#### JWT Configuration
- **JWT_SECRET** 
  - **Type**: Secret
  - **Description**: Base64 encoded secret for JWT token signing
  - **Generate with**: `openssl rand -base64 64`
  - **Example**: `YourGeneratedBase64SecretHere==`

#### Optional Logging Configuration
- **SEQ_SERVER_URL** (Optional)
  - **Type**: Secret
  - **Description**: URL for Seq logging server
  - **Example**: `http://your-seq-server:5341`
  - **Leave empty if not using Seq**

### 3. Environment Setup

Create an environment named **"Development"** in Azure DevOps:
1. Go to Pipelines → Environments
2. Click "New environment" 
3. Name: `Development`
4. Resource type: None
5. Add approval gates if needed

### 4. Parameterizable Pipeline Variables

These can be overridden at runtime without being secrets:

```yaml
variables:
  resourceGroupName: 'Saral'              # Target resource group
  webAppName: 'saral-backend-app'         # App Service name
  environmentName: 'Development'          # Deployment environment
  location: 'East US'                     # Azure region
```

## Application Configuration

The deployment will automatically configure your application with:

### Connection Strings (from Key Vault)
- **ConnectionStrings__Database**: PostgreSQL connection string
- **Jwt__Secret**: JWT signing secret
- **Jwt__Issuer**: `saral-backend`
- **Jwt__Audience**: `saral-frontend` 
- **Jwt__ExpirationInMinutes**: `60`

### Environment Settings
- **ASPNETCORE_ENVIRONMENT**: Based on `aspnetcoreEnvironment` parameter
- **Serilog__MinimumLevel__Default**: Based on `serilogMinimumLevel` parameter
- **HealthChecks__UI__EvaluationTimeInSeconds**: Based on `healthCheckEvaluationTime` parameter

## Quick Setup Commands

### Generate JWT Secret
```bash
# Generate a secure JWT secret
openssl rand -base64 64
```

### Test Database Connection (locally)
```bash
# Test PostgreSQL connection after deployment
psql "Host=your-postgres-server.postgres.database.azure.com;Database=saraldb;Username=saraladmin;Password=YourPassword;SSL Mode=Require"
```

## Deployment Flow

1. **Build Stage**: Compiles application and creates artifacts
2. **Deploy Stage**: 
   - Deploys Bicep infrastructure with parameterized values
   - Stores all secrets in Azure Key Vault
   - Deploys application to App Service
   - Configures app settings to reference Key Vault secrets

## Security Features

- ✅ All secrets stored in Azure Key Vault
- ✅ App Service uses Managed Identity to access Key Vault  
- ✅ No hardcoded secrets in configuration files
- ✅ HTTPS enforced on App Service
- ✅ PostgreSQL with SSL required
- ✅ Application Insights monitoring enabled

## Troubleshooting

### Common Issues

#### 1. Service Connection Error
**Error**: `service connection $(azureServiceConnection) which could not be found`

**Solution**: 
- Create a service connection named `azureServiceConnection` (see steps above)
- OR update the `azureServiceConnection` variable in the pipeline to match your existing service connection name

#### 2. KeyVault Access Denied
**Error**: Managed Identity cannot access Key Vault

**Solution**: 
- Ensure Managed Identity has Key Vault Secrets Officer role
- Check that the Key Vault allows access from Azure services

#### 3. Database Connection Failed  
**Error**: Cannot connect to PostgreSQL

**Solution**: 
- Check PostgreSQL firewall rules allow Azure services
- Verify credentials in `POSTGRESQL_ADMIN_PASSWORD` variable
- Ensure SSL is properly configured

#### 4. Pipeline Fails on Secret Variables
**Error**: Missing or empty secret variables

**Solution**: 
- Verify all required secret variables are set in Azure DevOps:
  - `POSTGRESQL_ADMIN_PASSWORD`
  - `JWT_SECRET` 
  - `SEQ_SERVER_URL` (optional)

#### 5. Environment Not Found
**Error**: Environment 'Development' does not exist

**Solution**:
- Create environment in Azure DevOps: Pipelines → Environments → New Environment
- Name it exactly: `Development`

### Logs and Monitoring
- **Application Insights**: Automatic APM monitoring
- **Log Analytics**: Centralized logging
- **App Service Logs**: Available in Azure portal
- **Health Checks**: Available at `/health` endpoint
