######################################################################################################
# Package Essential.Diagnostics
# 
# WhatIf: If WhatIf is specified, then messages are printed to indicate which files will be
#           included, but no action is taken.
#
######################################################################################################

param (
    [switch]$Build = $false,
    [switch]$WhatIf = $false
)
if ($WhatIf) {
    $pre = "What if: "
}

function Add-File($zipFile, $zipLocation, $path, $file) {
    $fileLocation = (Join-Path $path $file)
	Write-Host "$($pre)Add '$fileLocation' to package in '\$zipLocation'."
	if (-not $WhatIf) {
		$dummy = $zipFile.AddFile($fileLocation, $zipLocation)
	}
}

function Build-Solution($configuration) {
	Write-Host ""
	Write-Host "# Building solution..."
	Write-Host "$($pre)Build $configuration version (using MSBuild)"
	if (-not $WhatIf) {
		& "$($env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" ..\EssentialDiagnostics.sln /t:Rebuild /p:Configuration=$configuration
	}
}

function Package-ApplicationBinaries($solutionPath, $version) {
	Write-Host ""
	Write-Host "# Packaging application binaries..."

	if (-not $WhatIf) {
	    $zipFile =  new-object Ionic.Zip.ZipFile
	}

	$zipLocation = ""
	Add-File $zipFile $zipLocation $solutionPath "ReadMe.txt"
	Add-File $zipFile $zipLocation $solutionPath "License.txt"

	$path = (Join-Path $solutionPath "Essential.Diagnostics\bin\Release")
	Add-File $zipFile $zipLocation $path "Essential.Diagnostics.dll" 

	$path = (Join-Path $solutionPath "Essential.Diagnostics.RegSql\bin\Release")
	Add-File $zipFile $zipLocation $path "diagnostics_regsql.exe"
	Add-File $zipFile $zipLocation $path "diagnostics_regsql.exe.config"
	Add-File $zipFile $zipLocation $path "InstallTrace.sql"
	Add-File $zipFile $zipLocation $path "UninstallTrace.sql"

	$zipFilePath = (Join-Path $solutionPath "Package\Output\Essential.Diagnostics_Binaries_$($version).zip")
	Write-Host "$($pre)Saving package to '$zipFilePath'"
	if (-not $WhatIf) {
		$zipFile.Save($zipFilePath)
		$zipFile.Dispose()
	}
}

function Package-Documentation() {
}

function Package-Examples() {
}

function Package-SourceCode() {
}

######################################################################################################
# Main Script

Write-Host ""
Write-Host "## Packaging Essential.Diagnostics"

Write-Host ""
Write-Host "Load ZIP library"
# NOTE: The Ionic.Zip library treats paths as relevant to the current Windows directory, so convert to the correct absolute location if necessary.
$executingScriptDirectory = Split-Path -Path $MyInvocation.MyCommand.Definition -Parent
Add-Type -Path $executingScriptDirectory\Ionic.Zip.dll

$solutionPath = (Split-Path $executingScriptDirectory -Parent)
$version = (Get-Content (Join-Path $solutionPath "Essential.Diagnostics\Version.txt"))

$outputPath = (Join-Path $solutionPath "Package\Output")
if (!(Test-Path $outputPath -PathType container)) {
	Write-Host "$($pre)Creating path '$outputPath'"
	if (-not $WhatIf) {
		New-Item $outputPath -Type directory
	}
}

if ($Build) {
	Build-Solution "Release"
}

Package-ApplicationBinaries $solutionPath $version

Write-Host ""
Write-Host "# Packaging complete"
Write-Host ""
