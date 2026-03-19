# Update Azure App Service Connection String
# Replace <your-app-name> with your actual App Service name

$appName = "<your-app-name>"
$resourceGroup = "<your-resource-group>"
$connectionString = "Server=pbmchurch.database.windows.net,1433;Initial Catalog=pbm;Persist Security Info=False;User ID=church;Password=6731@Pbm;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=60;"

az webapp config connection-string set `
    --name $appName `
    --resource-group $resourceGroup `
    --connection-string-type SQLAzure `
    --settings DefaultConnection=$connectionString

Write-Host "Connection string updated. Restarting app..."
az webapp restart --name $appName --resource-group $resourceGroup
