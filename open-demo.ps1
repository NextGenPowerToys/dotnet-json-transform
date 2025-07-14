# PowerShell script to generate and open dynamic JSON Transform Library Demo
Write-Host "🎬 JSON Transform Library Demo Generator" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Get the directory where this script is located
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$DemoRunnerDir = Join-Path $ScriptDir "src\Json.Transform\DemoRunner"

# Check if we should run tests first
$RunTests = ""
if ($args -contains "--run-tests" -or $args -contains "-t") {
    $RunTests = "--run-tests"
    Write-Host "🧪 Will run tests before generating demo" -ForegroundColor Yellow
}

# Build and run the demo generator
Write-Host "🔧 Building demo generator..." -ForegroundColor Green
Push-Location $DemoRunnerDir

try {
    $buildResult = dotnet build --configuration Release 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Error: Failed to build demo generator" -ForegroundColor Red
        Write-Host $buildResult -ForegroundColor Red
        exit 1
    }

    Write-Host "🔄 Generating dynamic demo with real transformation results..." -ForegroundColor Green
    $outputPath = Join-Path $ScriptDir "demo.html"
    $arguments = @($RunTests, "--output", $outputPath) | Where-Object { $_ -ne "" }
    
    & dotnet run --configuration Release -- @arguments
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Demo generated and opened successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Error generating demo" -ForegroundColor Red
        exit 1
    }
}
finally {
    Pop-Location
}
