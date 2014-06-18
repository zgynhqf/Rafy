#如果这个能成功执行，则这个文件可以在 VS 中直接右键执行：Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Scope Process

#$machineName = [System.Environment]::MachineName;
$deployDir = "C:\Users\HuQingfang\Documents\Visual Studio 2012\Templates"

#"注册 GAC "
#gacutil -i bin\Debug\RafyProjects.dll
#以下代码需要有管理员权限！
#[System.Diagnostics.Process]::Start("C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\x64\gacutil.exe", "-i bin\Debug\RafyProjects.dll")

function Remove([string]$targetPath){
    if([System.IO.Directory]::Exists($targetPath))
    {
        "Delete $targetPath"
        Remove-Item -LiteralPath $targetPath -Force -Recurse #-Verbose
    }
}
function CopyProjectTemplate([string]$subDir){
    $targetPath = "$deployDir\ProjectTemplates\$subDir"
    Remove($targetPath)
    "CopyTo $targetPath"
    Copy-Item -Path "Projects\$subDir" -Destination $targetPath -Recurse #-Verbose
}
function CopyItemTemplate([string]$subDir){
    $targetPath = "$deployDir\ItemTemplates\$subDir"
    Remove($targetPath)
    "CopyTo $targetPath"
    Copy-Item -Path "Items\$subDir" -Destination $targetPath -Recurse #-Verbose
}

CopyProjectTemplate "Rafy Domain Library"
CopyProjectTemplate "Rafy Console App"

CopyItemTemplate "DomainEntity"


#explorer.exe $deployDir

#Read-Host