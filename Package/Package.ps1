######################################################################################################
# Package Essential.Diagnostics
# 
# WhatIf: If WhatIf is specified, then messages are printed to indicate which files will be
#           included, but no action is taken.
#
# ./Package.ps1 -Build       : to do a build release and then package BinariesOnly, nuget and examples
# ./Package.ps1 -Package     : to package (only) BinariesOnly, nuget and examples
# ./Package.ps1 -PackageFull : to package the examples (including a copy of the binaries)
#
# Note: For PackageFull the examples should first be manually updated to use the latest nuget package.
#
######################################################################################################

# ./Package.ps1 -PackageBin:$false -nuPack:$false

param (
    [switch]$Build = $false,
	[switch]$Package = $false,
    [switch]$PackageBin = $true,
    [switch]$nuPack = $true,
    [switch]$UpdateExamples = $true,
    [switch]$PackageFull = $false,
    [switch]$WhatIf = $false
)
if ($WhatIf) {
    $pre = "What if: "
}

function Add-File($zipFile, $zipPath, $path, $file) {
    $fileLocation = (Join-Path $path $file)
    $fileRelativePath = (Split-Path $file)
    if ($fileRelativePath) {
		if ($zipPath) {
			$zipLocation = (Join-Path $zipPath $fileRelativePath)
		} else {
			$zipLocation = $fileRelativePath
		}
    } else {
        $zipLocation = $zipPath
    }
	Write-Host "$($pre)Add '$fileLocation' to '\$zipLocation'."
	if (-not $WhatIf) {
		$dummy = $zipFile.AddFile($fileLocation, $zipLocation)
	}
}

function Copy-File($outputPath, $path, $file) {
    $fileLocation = (Join-Path $path $file)
    $fileRelativePath = (Split-Path $file)
    if ($fileRelativePath) {
        $outputLocation = (Join-Path $outputPath $fileRelativePath)
    } else {
        $outputLocation = $outputPath
    }
	Write-Host "$($pre)Copy '$fileLocation' to '$outputLocation'."
	if (-not $WhatIf) {
        Copy-Item $fileLocation $outputLocation
	}
}

function Ensure-Directory($path) {
    if (!(Test-Path $path -PathType container)) {
    	Write-Host "$($pre)Creating path '$path'"
    	if (-not $WhatIf) {
    		New-Item $path -Type directory
    	}
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

function Add-Binaries($zipFile, $zipPath, $path) {
	Add-File $zipFile $zipPath $path "ReadMe.txt"
	Add-File $zipFile $zipPath $path "License.txt"
	Add-File $zipFile $zipPath $path "lib\Essential.Diagnostics.dll" 
	Add-File $zipFile $zipPath $path "lib\Essential.Diagnostics.xml" 
	Add-File $zipFile $zipPath $path "tools\diagnostics_regsql.exe"
	Add-File $zipFile $zipPath $path "tools\diagnostics_regsql.exe.config"
	Add-File $zipFile $zipPath $path "tools\InstallTrace.sql"
	Add-File $zipFile $zipPath $path "tools\UninstallTrace.sql"
}

function Add-Examples($zipFile, $zipPath, $solutionPath) {
	$path = (Join-Path $solutionPath "Examples")
    
	Add-File $zipFile $zipPath $path "Essential.Diagnostics.Examples.sln" 
	Add-File $zipFile $zipPath $path "Diagnostics.Sample.config" 

	Add-File $zipFile $zipPath $path "FilteringExample\FilteringExample.csproj" 
	Add-File $zipFile $zipPath $path "FilteringExample\packages.config" 
	Add-File $zipFile $zipPath $path "FilteringExample\Program.cs" 
	Add-File $zipFile $zipPath $path "FilteringExample\App.config"
	Add-File $zipFile $zipPath $path "FilteringExample\FilteringExample_ReadMe.txt"
	Add-File $zipFile $zipPath $path "FilteringExample\Properties\AssemblyInfo.cs"
    
	Add-File $zipFile $zipPath $path "HelloLogging\HelloLogging.csproj" 
	Add-File $zipFile $zipPath $path "HelloLogging\packages.config" 
	Add-File $zipFile $zipPath $path "HelloLogging\HelloLogging.cs" 
	Add-File $zipFile $zipPath $path "HelloLogging\App.config"
	Add-File $zipFile $zipPath $path "HelloLogging\HelloLogging_ReadMe.txt" 
	Add-File $zipFile $zipPath $path "HelloLogging\Properties\AssemblyInfo.cs"
	Add-File $zipFile $zipPath $path "HelloLogging\ColoredConsole\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\Console\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\EventLog\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\FileLog\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\InMemory\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\RollingFile\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\RollingXml\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\SqlDatabase\HelloLogging.exe.config"
	Add-File $zipFile $zipPath $path "HelloLogging\XmlWriter\HelloLogging.exe.config"

	Add-File $zipFile $zipPath $path "MonitorConfig\MonitorConfig.csproj" 
	Add-File $zipFile $zipPath $path "MonitorConfig\packages.config" 
	Add-File $zipFile $zipPath $path "MonitorConfig\Program.cs" 
	Add-File $zipFile $zipPath $path "MonitorConfig\App.config"
	Add-File $zipFile $zipPath $path "MonitorConfig\MonitorConfig_ReadMe.txt"
	Add-File $zipFile $zipPath $path "MonitorConfig\Properties\AssemblyInfo.cs"	
}

function Package-ApplicationBinaries($solutionPath, $configuration, $version) {
	Write-Host ""
	Write-Host "# Packaging application binaries..."

	if (-not $WhatIf) {
	    $zipFile =  new-object Ionic.Zip.ZipFile
	}

	$zipPath = ""
	$binariesPath = (Join-Path $solutionPath "Package\$configuration")
    Add-Binaries $zipFile $zipPath $binariesPath

	$zipFilePath = (Join-Path $solutionPath "Package\Output\Essential.Diagnostics_BinariesOnly_$($version).zip")
	Write-Host "$($pre)Saving application binaries package to '$zipFilePath'"
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

	$zipPath = ""
	Add-File $zipFile $zipPath $solutionPath "ReadMe.txt"
	Add-File $zipFile $zipPath $solutionPath "License.txt"

    Add-Examples $zipFile $zipPath $solutionPath
	
	$binariesPath = (Join-Path $solutionPath "Examples\packages\Essential.Diagnostics.$version")
	$zipPath = "packages\Essential.Diagnostics.$version"
    Add-Binaries $zipFile $zipPath $binariesPath

	$zipFilePath = (Join-Path $solutionPath "Package\Output\Essential.Diagnostics_$($version).zip")
	Write-Host "$($pre)Saving examples package to '$zipFilePath'"
	if (-not $WhatIf) {
		$zipFile.Save($zipFilePath)
		$zipFile.Dispose()
	}
}

function Package-NuPack($solutionPath, $configuration, $version) {
	Write-Host ""
	Write-Host "# Creating NuGet package..."
    
	$path = (Join-Path $solutionPath "Package\$configuration")

  	$nuspecTemplatePath = (Join-Path $solutionPath "Package\Essential.Diagnostics.nuspec")
    $nuspec = [xml](Get-Content -Path $nuspecTemplatePath)
    $nuspec.package.metadata.version = "$version"
    #$now = [System.DateTimeOffset]::UtcNow
    #$nuspec.package.metadata.created = $now.ToString("s")
    #$nuspec.package.metadata.modified = $now.ToString("s")
    #$now.SelectSingleNode("/")
        
    $outputNuspecPath = (Join-Path $path "Essential.Diagnostics.nuspec")
	Write-Host "$($pre)Creating nuspec file '$outputNuspecPath'"
	if (-not $WhatIf) {
      $nuspec.Save($outputNuspecPath)
    }
    
    $packagePath = (Join-Path $solutionPath "Package\Output")
	Write-Host "$($pre)Running: nupack ""$outputNuspecPath"" ""$packagePath"""
	if (-not $WhatIf) {
        & .\NuGet.exe pack "$outputNuspecPath" -OutputDirectory "$packagePath"
    }
}

function Update-Examples($solutionPath, $configuration, $version) {
	Write-Host ""
	Write-Host "# Updating references in examples..."
    
	$path = (Join-Path $solutionPath "Package\$configuration")
	$target = (Join-Path $solutionPath "Examples\packages\Essential.Diagnostics.$version")
	
	foreach($existingPackage in (Get-ChildItem "$solutionPath\Examples\packages\Essential.Diagnostics*")) {
		Write-Host "$($pre)Removing package folder '$($existingPackage.FullName)'"
		if (-not $WhatIf) {
			Remove-Item $existingPackage.FullName -Recurse
		}	
	}
	
	Write-Host "$($pre)Copying package binaries to '$target'"
	if (-not $WhatIf) {
	  Ensure-Directory $target
      Copy-Item "$path\*.txt" "$target"
      Copy-Item "$path\lib" "$target\lib" -Container -Recurse
      Copy-Item "$path\tools" "$target\tools" -Container -Recurse
    }
    
	$majorMinor = $version -replace "(\d+\.\d+)\.\d+\.\d+", "`$1.0.0"
	$referenceRegex = "Essential\.Diagnostics, Version=\d+\.\d+\.\d+\.\d+"
	$referenceReplace = "Essential.Diagnostics, Version=$majorMinor"
	$pathRegex = "packages\\Essential\.Diagnostics\.\d+\.\d+\.\d+\.\d+\\lib"
	$pathReplace = "packages\Essential.Diagnostics.$version\lib"
	$packageRegex = "id=`"Essential\.Diagnostics`" version=`"\d+\.\d+\.\d+\.\d+`""
	$packageReplace = "id=`"Essential.Diagnostics`" version=`"$version`""
	
	Write-Host "Update reference to '$referenceReplace' and path to '$pathReplace'"

	$exampleProjects = @( "HelloLogging", "MonitorConfig", "FilteringExample", 
		"ScopeExample", "AbstractionDependency" )
	
	foreach ($project in $exampleProjects) {
		$projectPath = Join-Path $solutionPath "Examples\$project\$project.csproj"
		$projectContent = Get-Content $projectPath
		$projectContent = $projectContent -replace $referenceRegex, $referenceReplace
		$projectContent = $projectContent -replace $pathRegex, $pathReplace
		$packagesPath = Join-Path $solutionPath "Examples\$project\packages.config"
		$packagesContent = Get-Content $packagesPath
		$packagesContent = $packagesContent -replace $packageRegex, $packageReplace
		
		Write-Host "$($pre)Updating references in '$projectPath' and '$packagesPath'"
		if (-not $WhatIf) {
			Set-Content -Path $projectPath -Value $projectContent
			Set-Content -Path $packagesPath -Value $packagesContent
		}
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
$outputPath = (Join-Path $solutionPath "Package\Output")
Ensure-Directory $outputPath

if ($PackageFull) {
	$version = (Get-Content (Join-Path $solutionPath "Essential.Diagnostics\Version.txt"))
	
	Write-Host ""
	Write-Host "# Packaging examples for version $version to '$outputPath'"
	
	Package-Complete $solutionPath $version
} else {
	if ($Package -or $Build) {
		if ($Build) {
			Build-Solution $solutionPath "Release"
		}

		$version = (Get-Content (Join-Path $solutionPath "Essential.Diagnostics\Version.txt"))
		Write-Host ""
		Write-Host "# Packaging version $version to '$outputPath'"

		if ($PackageBin) {
			Package-ApplicationBinaries $solutionPath "Release" $version
		}

		if ($nuPack) {
			Package-NuPack $solutionPath "Release" $version
		}
			
		if ($UpdateExamples) {
			Update-Examples $solutionPath "Release" $version
		}
	}
	else
	{
		Write-Host "./Package.ps1 -Build       : to do a build release and then package BinariesOnly, nuget and examples"
		Write-Host "./Package.ps1 -Package     : to package (only) BinariesOnly, nuget and examples"
		Write-Host "./Package.ps1 -PackageFull : to package the examples (including a copy of the binaries)"
	}
}

Write-Host ""
Write-Host "# Packaging complete"
Write-Host ""
