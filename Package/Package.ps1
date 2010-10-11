######################################################################################################
# Package Essential.Diagnostics
# 
# WhatIf: If WhatIf is specified, then messages are printed to indicate which files will be
#           included, but no action is taken.
#
######################################################################################################

param (
    [switch]$Build = $false,
    [switch]$BuildDocs = $false,
    [switch]$WhatIf = $false
)
if ($WhatIf) {
    $pre = "What if: "
}

function Add-File($zipFile, $zipPath, $path, $file) {
    $fileLocation = (Join-Path $path $file)
    $fileRelativePath = (Split-Path $file)
    if ($fileRelativePath) {
        $zipLocation = (Join-Path $zipPath $fileRelativePath)
    } else {
        $zipLocation = $zipPath
    }
	Write-Host "$($pre)Add '$fileLocation' to '\$zipLocation'."
	if (-not $WhatIf) {
		$dummy = $zipFile.AddFile($fileLocation, $zipLocation)
	}
}

function Build-Solution($solutionPath, $configuration) {
	Write-Host ""
	Write-Host "# Building solution..."
    $solution = (Join-Path $solutionPath "EssentialDiagnostics.sln")
	Write-Host "$($pre)Build $configuration version of '$solution' (using MSBuild)"
	if (-not $WhatIf) {
		& "$($env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" $solution /t:Rebuild /p:Configuration=$configuration
	}
}

function Build-Documentation($solutionPath, $configuration) {
	Write-Host ""
	Write-Host "# Building documentation..."
    $documentationProject = (Join-Path $solutionPath "Documentation\Essential.Diagnostics.shfbproj")
	Write-Host "$($pre)Build documentation '$documentationProject' (using Sandcastle)"
	if (-not $WhatIf) {
        # TODO: Run Sandcastle project
		& "$($env:windir)\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" $documentationProject	/p:Configuration=$configuration
	}
}

function Add-Binaries($zipFile, $zipPath, $solutionPath) {
	$path = (Join-Path $solutionPath "Examples\Binaries")

	Add-File $zipFile $zipPath $path "Essential.Diagnostics.dll" 
	Add-File $zipFile $zipPath $path "Essential.Diagnostics.XML" 
	Add-File $zipFile $zipPath $path "diagnostics_regsql.exe"
	Add-File $zipFile $zipPath $path "diagnostics_regsql.exe.config"
	Add-File $zipFile $zipPath $path "InstallTrace.sql"
	Add-File $zipFile $zipPath $path "UninstallTrace.sql"
}

function Add-Examples($zipFile, $zipPath, $solutionPath) {
	$path = (Join-Path $solutionPath "Examples")
    
	Add-File $zipFile $zipPath $path "Essential.Diagnostics.Examples.sln" 

	Add-File $zipFile $zipPath $path "MonitorConfig\MonitorConfig.csproj" 
	Add-File $zipFile $zipPath $path "MonitorConfig\Program.cs" 
	Add-File $zipFile $zipPath $path "MonitorConfig\App.config"
	Add-File $zipFile $zipPath $path "MonitorConfig\MonitorConfig_ReadMe.txt"
	Add-File $zipFile $zipPath $path "MonitorConfig\Properties\AssemblyInfo.cs"
    
	Add-File $zipFile $zipPath $path "HelloLogging\HelloLogging.csproj" 
	Add-File $zipFile $zipPath $path "HelloLogging\HelloLogging.cs" 
	Add-File $zipFile $zipPath $path "HelloLogging\App.config"
	Add-File $zipFile $zipPath $path "HelloLogging\HelloLogging_ReadMe.txt" 
	Add-File $zipFile $zipPath $path "HelloLogging\Properties\AssemblyInfo.cs"
	Add-File $zipFile $zipPath $path "HelloLogging\ColoredConsole\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\EventLog\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\FileLog\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\XmlWriter\HelloLogging.exe.config"
}

function Package-ApplicationBinaries($solutionPath, $version) {
	Write-Host ""
	Write-Host "# Packaging application binaries..."

	if (-not $WhatIf) {
	    $zipFile =  new-object Ionic.Zip.ZipFile
	}

	$zipPath = ""
	Add-File $zipFile $zipPath $solutionPath "ReadMe.txt"
	Add-File $zipFile $zipPath $solutionPath "License.txt"

    Add-Binaries $zipFile "" $solutionPath

	$zipFilePath = (Join-Path $solutionPath "Package\Output\Essential.Diagnostics_BinariesOnly_$($version).zip")
	Write-Host "$($pre)Saving application binaries package to '$zipFilePath'"
	if (-not $WhatIf) {
		$zipFile.Save($zipFilePath)
		$zipFile.Dispose()
	}
}

function Package-Documentation($solutionPath, $version) {
	Write-Host ""
	Write-Host "# Packaging documentation..."

	if (-not $WhatIf) {
	    $zipFile =  new-object Ionic.Zip.ZipFile
	}

	$zipPath = "Help"
	Add-File $zipFile $zipPath $solutionPath "ReadMe.txt"
	Add-File $zipFile $zipPath $solutionPath "License.txt"

	$path = (Join-Path $solutionPath "Documentation\Help")
	Add-File $zipFile $zipPath $path "Essential.Diagnostics.chm" 

	$zipFilePath = (Join-Path $solutionPath "Package\Output\Essential.Diagnostics_Documentation_$($version).zip")
	Write-Host "$($pre)Saving documentation package to '$zipFilePath'"
	if (-not $WhatIf) {
		$zipFile.Save($zipFilePath)
		$zipFile.Dispose()
	}
}

function Package-Examples($solutionPath, $version) {
	Write-Host ""
	Write-Host "# Packaging examples..."

	if (-not $WhatIf) {
	    $zipFile =  new-object Ionic.Zip.ZipFile
	}

	$zipPath = "Examples"
	Add-File $zipFile $zipPath $solutionPath "ReadMe.txt"
	Add-File $zipFile $zipPath $solutionPath "License.txt"

    Add-Examples $zipFile $zipPath $solutionPath

	$zipFilePath = (Join-Path $solutionPath "Package\Output\Essential.Diagnostics_Examples_$($version).zip")
	Write-Host "$($pre)Saving examples package to '$zipFilePath'"
	if (-not $WhatIf) {
		$zipFile.Save($zipFilePath)
		$zipFile.Dispose()
	}
}

function Package-Complete($solutionPath, $version) {
	Write-Host ""
	Write-Host "# Packaging complete (full) files..."

	if (-not $WhatIf) {
	    $zipFile =  new-object Ionic.Zip.ZipFile
	}

	$zipPath = "Essential.Diagnostics_$version"
	Add-File $zipFile $zipPath $solutionPath "ReadMe.txt"
	Add-File $zipFile $zipPath $solutionPath "License.txt"

    Add-Binaries $zipFile "$zipPath\Binaries" $solutionPath

    Add-Examples $zipFile "$zipPath" $solutionPath

	$path = (Join-Path $solutionPath "Documentation\Help")
	Add-File $zipFile "$zipPath\Binaries" $path "Essential.Diagnostics.chm" 

	$zipFilePath = (Join-Path $solutionPath "Package\Output\Essential.Diagnostics_$($version).zip")
	Write-Host "$($pre)Saving examples package to '$zipFilePath'"
	if (-not $WhatIf) {
		$zipFile.Save($zipFilePath)
		$zipFile.Dispose()
	}
}

# Source code should be accessed from the Mercurial repository
#function Package-SourceCode() {
#}

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

if ($Build) {
	Build-Solution $solutionPath "Release"
    Build-Documentation $solutionPath "Release"
}

if ($BuildDocs) {
    Build-Documentation $solutionPath "Release"
}

$version = (Get-Content (Join-Path $solutionPath "Essential.Diagnostics\Version.txt"))
$outputPath = (Join-Path $solutionPath "Package\Output")
Write-Host ""
Write-Host "# Packaging version $version to '$outputPath'"
if (!(Test-Path $outputPath -PathType container)) {
	Write-Host "$($pre)Creating path '$outputPath'"
	if (-not $WhatIf) {
		New-Item $outputPath -Type directory
	}
}

Package-ApplicationBinaries $solutionPath $version

Package-Complete $solutionPath $version

#Package-Documentation $solutionPath $version
#Package-Examples $solutionPath $version

Write-Host ""
Write-Host "# Packaging complete"
Write-Host ""
