#!/usr/local/bin/pwsh
param(
    [string]$deviceFile='Amcrest-IP4M-1055E.backup',
    [string]$name=$(throw '-name required'),
    [string]$apiAddress=$(throw '-apiAddress required'),
    [string]$ftpUser=$(throw '-ftpUser required'),
    [string]$ftpPwd=$(throw '-ftpPwd required')
)
$ErrorActionPreference="Stop"

if(![System.IO.Path]::IsPathRooted($deviceFile)){
    $deviceFile="$PSScriptRoot/../device-configs/$deviceFile"
}

$config=Get-Content -Raw -Path $deviceFile

$config=$config.Replace('{{{NAME}}}',$name)
$config=$config.Replace('{{{API_ADDRESS}}}',$apiAddress)
$config=$config.Replace('{{{FTP_PASSWORD}}}',$ftpPwd)
$config=$config.Replace('{{{FTP_USER}}}',$ftpUser)

$fileName=[System.IO.Path]::GetFileName($deviceFile)

$dir="$PSScriptRoot/../tmp"

if(!(Test-Path $dir)){
    New-Item -ItemType Directory -Path "$dir" | Out-Null
}

$fullName=[System.IO.Path]::GetFullPath("$dir/$name-$fileName")
$config | Out-File -FilePath $fullName
Write-Host "config file written to $fullName"
