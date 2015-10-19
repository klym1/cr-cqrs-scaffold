$version = "0.0.0";

if ($args[0]) { $version=$args[0] }

Write-Host "Building with Version Number: " $version

Function Build-Fake([string]$folderName)
{
    pushd $folderName
    ./build.cmd package release $version
    popd

    if ($LastExitCode -ne 0) {
        throw "$folderName build failed"
    } else {
        cp $folderName\dist\*.zip .\dist
        cp $folderName\dist\*.nupkg .\dist
        cp $folderName\tests\*TestResults.xml .\tests
    }
}

rmdir -R dist
mkdir dist

rmdir -R tests
mkdir tests

Build-Fake "widget-api"
