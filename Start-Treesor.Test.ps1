#Start-Process  -WorkingDirectory $PSScriptRoot\Treesor.PSDriveProvider\bin\Debug -NoNewWindow -NoProfile -FilePath

Start-Process `
    -FilePath "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" `
    -WorkingDirectory (Join-Path $PSScriptRoot Treesor.PSDriveProvider\bin\Debug) `
    -ArgumentList @(
        "-noprofile"
    )
    
    #-ArgumentList @(
    #    "-Command","Import-Module TreesorDriveProvider.dll",
    #    "-noprofile"
    #)