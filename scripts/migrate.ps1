param(
    [ValidateSet("add", "update", "bundle")]
    [string]$Action = "update",
    [string]$Migration = "InitialCreate",
    [string]$Environment = "Development",
    [string]$DatabaseConnection = "",
    [string]$RedisConnection = ""
)

$ErrorActionPreference = "Stop"

$scriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Resolve-Path (Join-Path $scriptDirectory "..")
Set-Location $solutionRoot

$env:ASPNETCORE_ENVIRONMENT = $Environment
$env:DOTNET_ENVIRONMENT = $Environment

if (![string]::IsNullOrWhiteSpace($DatabaseConnection)) {
    $env:ConnectionStrings__Database = $DatabaseConnection
}

if (![string]::IsNullOrWhiteSpace($RedisConnection)) {
    $env:ConnectionStrings__Redis = $RedisConnection
}

$infrastructureProject = "src/Infrastructure/Ore.Infrastructure.csproj"
$startupProject = "src/Api/Ore.Api.csproj"

switch ($Action) {
    "add" {
        Write-Host "Adding migration '$Migration' for environment '$Environment'..."
    dotnet tool run dotnet-ef migrations add $Migration --project $infrastructureProject --startup-project $startupProject
    }
    "bundle" {
        Write-Host "Building migration bundle for environment '$Environment'..."
    dotnet tool run dotnet-ef migrations bundle --project $infrastructureProject --startup-project $startupProject
    }
    default {
        Write-Host "Updating database to migration '$Migration' for environment '$Environment'..."
    dotnet tool run dotnet-ef database update $Migration --project $infrastructureProject --startup-project $startupProject
    }
}
