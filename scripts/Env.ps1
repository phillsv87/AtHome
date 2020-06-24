#!/usr/local/bin/pwsh
param(
    [switch]$kill,
    [switch]$noCode
)
$ErrorActionPreference="Stop"

$name="quad-62d3dd66-3d70-4ca6-9897-8924029a4482"

if($kill){
    tmux kill-session -t $name
    return
}

if(!$noCode){
    code "$PSScriptRoot/.."
}

tmux has-session -t $name | Out-Null
if($?){
    sleep 1
    tmux attach -t $name
}else{ 
    tmux new-session -d -s $name
    tmux split-window -v
    tmux split-window -h
    tmux select-pane -t 0
    tmux split-window -h
    tmux select-pane -t 0

    tmux send-keys -t 0 C-z "cd $PSScriptRoot/../scripts" Enter
    tmux send-keys -t 1 C-z "cd $PSScriptRoot/.." Enter 'ssh pi@192.168.77.216' Enter
    tmux send-keys -t 2 C-z "cd $PSScriptRoot/../api/HomeSecureApi" Enter 'dotnet run' Enter
    tmux send-keys -t 3 C-z "cd $PSScriptRoot/../frontend/app" Enter 'npm run start' Enter

    tmux -2 attach-session -d
}