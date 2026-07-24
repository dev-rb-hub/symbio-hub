param location string = resourceGroup().location
param appServicePlanName string = 'symbioHub-plan'
param webAppName string = 'symbio-hub-app'
param sqlServerName string = 'symbio-hub-sql'
param sqlDbName string = 'SymbioHubDb'
param administratorLogin string = 'symbioadmin'
param administratorLoginPassword string

resource appServicePlan 'Microsoft.Web/serverfarms@2023-10-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
    size: 'F1'
    family: 'F'
    capacity: 1
  }
  properties: {
    reserved: false
  }
}

resource webApp 'Microsoft.Web/sites@2023-10-01' = {
  name: webAppName
  location: location
  kind: 'app'
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
    }
  }
}

resource sqlServer 'Microsoft.Sql/servers@2022-08-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    version: '12.0'
  }
  sku: {
    name: 'GP_S_Gen5_2'
    tier: 'GeneralPurpose'
    capacity: 2
  }
}

resource sqlDb 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  name: '${sqlServer.name}/${sqlDbName}'
  location: location
  properties: {
    sku: {
      name: 'S0'
      tier: 'Standard'
      capacity: 10
    }
  }
  dependsOn: [sqlServer]
}

output webAppUrl string = webApp.defaultHostName
