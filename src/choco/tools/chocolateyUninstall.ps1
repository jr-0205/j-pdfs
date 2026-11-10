$ErrorActionPreference = 'Stop'

$installedExe = Join-Path $env:ProgramFiles 'JPDFs\JPDFs.App.exe'
if (Test-Path -LiteralPath $installedExe) {
    Start-ChocolateyProcessAsAdmin -exeToRun $installedExe -statements '/uninstall-silent'
}
