#Requires -Version 5.1
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$buildScript = Join-Path $repoRoot 'src\build\build-portable.ps1'
$expectedInstaller = Join-Path $repoRoot "src\bin\$Configuration\net10.0-windows\publish\J-PDF's Setup.exe"
$rootInstaller = Join-Path $repoRoot "J-PDF's Setup.exe"

Write-Host ''
Write-Host "  J-PDF's | Compilador del instalador" -ForegroundColor Cyan
Write-Host '  -----------------------------------' -ForegroundColor DarkGray
Write-Host ''

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw '.NET SDK no encontrado. Instala el SDK requerido por src\global.json y vuelve a intentarlo.'
}
if (-not (Test-Path -LiteralPath $buildScript -PathType Leaf)) {
    throw "No se encontro el script de compilacion: $buildScript"
}
if (-not (Test-Path -LiteralPath (Join-Path $repoRoot 'src\JPDFs.csproj') -PathType Leaf)) {
    throw "No se encontro src\JPDFs.csproj. Comprueba que el repositorio este completo."
}

Write-Host "[1/2] Compilando el paquete Installer ($Configuration)..." -ForegroundColor Yellow
& (Join-Path $PSHOME 'powershell.exe') -NoLogo -NoProfile -ExecutionPolicy Bypass -File $buildScript -Configuration $Configuration -PackageKind Installer
if ($LASTEXITCODE -ne 0) {
    throw "La compilacion fallo con el codigo $LASTEXITCODE. Revisa los mensajes anteriores."
}

if (-not (Test-Path -LiteralPath $expectedInstaller -PathType Leaf)) {
    throw "La compilacion termino, pero no se encontro el instalador esperado: $expectedInstaller"
}

Write-Host '[2/2] Copiando el instalador a la raiz del repositorio...' -ForegroundColor Yellow
Copy-Item -LiteralPath $expectedInstaller -Destination $rootInstaller -Force

$file = Get-Item -LiteralPath $rootInstaller
$hash = (Get-FileHash -LiteralPath $rootInstaller -Algorithm SHA256).Hash

Write-Host ''
Write-Host 'COMPILACION COMPLETADA' -ForegroundColor Green
Write-Host "Archivo: $($file.FullName)"
Write-Host ('Tamano:  {0:N2} MB' -f ($file.Length / 1MB))
Write-Host "SHA256:  $hash"
Write-Host ''
