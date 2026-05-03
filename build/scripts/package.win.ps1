Remove-Item -Path build\Suity.Agentic\*.pdb -Force

# Create ZIP archive
Compress-Archive -Path build\Suity.Agentic -DestinationPath "build\suity-agentic_${env:VERSION}.${env:RUNTIME}.zip" -Force

# Build Installer using Inno Setup
$issPath = "build\scripts\Suity.Agentic.iss"
if (Test-Path $issPath) {
    Write-Host "Building Installer..."
    $env:SOURCE_DIR = (Resolve-Path "build\Suity.Agentic").Path
    & "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" $issPath
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup failed with exit code $LASTEXITCODE"
    }
    Write-Host "Installer built successfully."
} else {
    Write-Warning "Inno Setup script not found at $issPath. Skipping installer build."
}