#!/usr/local/bin/pwsh

## this script requires imagemagick
## brew install imagemagick

param(
    $projectPath=$(throw "-projectPath required"),
    $icon="iosIcon.png"
)

if(!(Test-Path $projectPath)){
    throw "project path does not exist - $projectPath"
}

if(!(Test-Path $icon)){
    throw "icon path does not exist - $icon"
}

$root=$PSScriptRoot;

$dest="$projectPath/Images.xcassets/AppIcon.appiconset"

Write-Host "Copying icons to $dest"
ls $dest
rm -r "$dest/*"

cp "$root/iosIconsContents.json" "$dest/Contents.json"
if(!$?){
    throw "Copy Contents.json failed"
}

[int32[]]$sizes=20,29,40,60,58,76,80,87,120,152,167,180,1024

foreach($size in $sizes){
    convert $icon -resize "$($size)x$($size)" "$dest/icon@$size.png"
    if(!$?){
        throw "Failed to create icon@$size.png"
    }
}

Write-Host "Icons updated" -Foreground DarkGreen