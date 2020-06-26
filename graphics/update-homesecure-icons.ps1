#!/usr/local/bin/pwsh

&"$PSScriptRoot/generate-ios-icons.ps1" `
    -icon "$PSScriptRoot/icon.png" `
    -projectPath "$PSScriptRoot/../frontend/app/ios/HomeSecure"