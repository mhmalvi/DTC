# Restore NuGet packages
dotnet restore

# Install Material Design packages
dotnet add DTCBillingSystem.UI/DTCBillingSystem.UI.csproj package MaterialDesignThemes
dotnet add DTCBillingSystem.UI/DTCBillingSystem.UI.csproj package MaterialDesignColors

# Install other required packages
dotnet add DTCBillingSystem.UI/DTCBillingSystem.UI.csproj package Microsoft.Extensions.DependencyInjection
dotnet add DTCBillingSystem.UI/DTCBillingSystem.UI.csproj package Microsoft.Extensions.Configuration
dotnet add DTCBillingSystem.UI/DTCBillingSystem.UI.csproj package Microsoft.Extensions.Configuration.Json 