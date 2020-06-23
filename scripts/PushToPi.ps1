#!/usr/local/bin/pwsh
param(
    [switch]$noBuild
)
$ErrorActionPreference="Stop"

Push-Location "$PSScriptRoot/../api/HomeSecureApi"

try{

    if(!$noBuild){
        dotnet publish
        if(!$?){
            throw "dotnet publish failed"
        }
    }

    rsync -r -P --delete bin/Debug/netcoreapp3.1/publish/ 'pi@192.168.77.216:api/HomeSecureApi/'

}finally{
    Pop-Location
}