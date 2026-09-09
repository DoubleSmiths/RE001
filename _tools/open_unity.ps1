$ErrorActionPreference = 'Stop'

# 일부 자동화 호스트는 이 Windows 기본 변수를 넘기지 않는다. UPM은 이 값으로
# 전역 설정 경로를 조합하므로, 누락된 경우에만 Windows 공용 데이터 경로로 보완한다.
if ([string]::IsNullOrWhiteSpace($env:ALLUSERSPROFILE))
{
    $commonDataPath = [Environment]::GetFolderPath([Environment+SpecialFolder]::CommonApplicationData)
    if ([string]::IsNullOrWhiteSpace($commonDataPath) -or -not (Test-Path -LiteralPath $commonDataPath -PathType Container))
    {
        throw 'Windows common application data folder could not be resolved.'
    }
    $env:ALLUSERSPROFILE = $commonDataPath
}

# 다른 에디터가 백업을 쓰고 있을 수 있으므로 실행 중에는 손대지 않는다.
if (Get-Process -Name Unity -ErrorAction SilentlyContinue)
{
    Write-Host 'Unity is already running. No files changed.'
    exit 0
}

$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$versionFile = Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt'
$version = (Select-String -LiteralPath $versionFile -Pattern '^m_EditorVersion: (.+)$').Matches.Groups[1].Value
$unityExe = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Unity.exe"
if (-not (Test-Path -LiteralPath $unityExe -PathType Leaf))
{
    throw "Unity $version was not found: $unityExe"
}

# 복구 자료는 삭제하지 않고 프로젝트 안에 보관한다. Unity 실행 중에는 절대 이동하지 않는다.
$backupPath = [IO.Path]::GetFullPath((Join-Path $projectRoot 'Temp/__Backupscenes'))
$archiveRoot = [IO.Path]::GetFullPath((Join-Path $projectRoot 'Documents/UnityRecovery'))
foreach ($targetPath in @($backupPath, $archiveRoot))
{
    if (-not $targetPath.StartsWith($projectRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase))
    {
        throw "Path is outside this project: $targetPath"
    }
}
if (Test-Path -LiteralPath $backupPath)
{
    # 재분석 지점으로 연결된 외부 폴더는 이동하지 않는다.
    foreach ($checkPath in @((Join-Path $projectRoot 'Temp'), $backupPath, (Join-Path $projectRoot 'Documents'), $archiveRoot))
    {
        if ((Test-Path -LiteralPath $checkPath) -and
            ((Get-Item -LiteralPath $checkPath).Attributes -band [IO.FileAttributes]::ReparsePoint))
        {
            throw "Refusing to move backups through a reparse point: $checkPath"
        }
    }
    $null = New-Item -ItemType Directory -Path $archiveRoot -Force
    $archivePath = [IO.Path]::GetFullPath((Join-Path $archiveRoot ((Get-Date -Format 'yyyyMMdd-HHmmss-fff') + '-' + [Guid]::NewGuid().ToString('N'))))
    if (-not $archivePath.StartsWith($archiveRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase))
    {
        throw "Archive destination is outside the recovery folder: $archivePath"
    }
    Move-Item -LiteralPath $backupPath -Destination $archivePath
    Write-Host "Scene backups preserved at: $archivePath"
}

Start-Process -FilePath $unityExe -ArgumentList @('-force-d3d11', '-projectPath', ('"' + $projectRoot + '"')) -WindowStyle Hidden
Write-Host "Opened Unity $version with Direct3D 11."
