param location string = resourceGroup().location
param appServicePlanName string = 'symbioHub-plan'
param webAppName string = 'symbio-hub-app'
param sqlServerName string = 'symbio-hub-sql'
param sqlDbName string = 'SymbioHubDb'
param administratorLogin string = 'symbioadmin'
@secure()
param administratorLoginPassword string
@secure()
param cosmosConnectionString string = ''

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
        {
          name: 'Cosmos__ConnectionString'
          value: cosmosConnectionString
        }
        {
          name: 'Cosmos__DatabaseName'
          value: 'SymbioHub'
        }
        {
          name: 'Cosmos__ContainerName'
          value: 'Projects'
        }
        {
          name: 'Cosmos__TalentContainerName'
          value: 'TalentProfiles'
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
}

resource sqlDb 'Microsoft.Sql/servers/databases@2022-08-01-preview' = {
  parent: sqlServer
  name: sqlDbName
  location: location
}

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2023-05-15' = {
  name: 'symbio-hub-cosmos'
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableServerless'
      }
      {
        name: 'EnableMultipleWriteLocations'
      }
    ]
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
  }
}

resource cosmosDb 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2023-05-15' = {
  parent: cosmosAccount
  name: 'SymbioHub'
  properties: {
    resource: {
      id: 'SymbioHub'
    }
  }
}

resource cosmosContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-05-15' = {
  parent: cosmosDb
  name: 'Projects'
  properties: {
    resource: {
      id: 'Projects'
      partitionKey: {
        paths: ['/Category']
        kind: 'Hash'
      }
      defaultTtl: -1
    }
  }
}

resource talentProfilesContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2023-05-15' = {
  parent: cosmosDb
  name: 'TalentProfiles'
  properties: {
    resource: {
      id: 'TalentProfiles'
      partitionKey: {
        paths: ['/Role']
        kind: 'Hash'
      }
      defaultTtl: -1
    }
  }
}

output webAppUrl string = webApp.properties.defaultHostName
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
