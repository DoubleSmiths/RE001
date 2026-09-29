# 구글 플레이 업로드 키스토어 생성 — 한 번만 실행하고 결과 파일과 비밀번호를 안전한 곳에 보관한다.
#  키스토어는 저장소에 넣지 않는다(.gitignore). 잃어버리면 플레이 콘솔에서 업로드 키 재설정을 요청해야 한다.
#
#  사용:  powershell -ExecutionPolicy Bypass -File _tools/make_upload_keystore.ps1
#         (비밀번호는 입력 창에서 받는다. 출력 경로 기본값: %USERPROFILE%\.minimonquest\upload.keystore)
#
#  빌드 전 환경변수(현재 세션):
#    $env:RE001_KEYSTORE      = "<키스토어 경로>"
#    $env:RE001_KEYSTORE_PASS = "<키스토어 비밀번호>"
#    $env:RE001_KEY_ALIAS     = "upload"
#    $env:RE001_KEY_PASS      = "<키 비밀번호>"
param(
    [string]$OutPath = (Join-Path $env:USERPROFILE '.minimonquest\upload.keystore'),
    [string]$Alias = 'upload',
    [string]$DName = 'CN=Minimon Quest, O=DoubleSmiths, C=KR'
)
$ErrorActionPreference = 'Stop'

$projectRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$version = (Select-String -LiteralPath (Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt') -Pattern '^m_EditorVersion: (.+)$').Matches.Groups[1].Value
$keytool = Join-Path $env:ProgramFiles "Unity/Hub/Editor/$version/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin/keytool.exe"
if (-not (Test-Path -LiteralPath $keytool)) { throw "keytool not found: $keytool" }

if (Test-Path -LiteralPath $OutPath) { throw "Already exists (not overwriting): $OutPath" }
$full = [IO.Path]::GetFullPath($OutPath)
if ($full.StartsWith($projectRoot, [StringComparison]::OrdinalIgnoreCase)) { throw 'Keep the keystore outside the repository.' }
$null = New-Item -ItemType Directory -Force -Path (Split-Path $full)

$pass = Read-Host 'Keystore password (6+ chars)' -AsSecureString
$plain = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pass))
if ($plain.Length -lt 6) { throw 'Password must be at least 6 characters.' }

# PKCS12 는 키 비밀번호 = 저장소 비밀번호. 유효기간 25년 이상이 플레이 요구사항.
& $keytool -genkeypair -v -keystore $full -storetype PKCS12 -alias $Alias -keyalg RSA -keysize 2048 `
    -validity 10000 -dname $DName -storepass $plain -keypass $plain
if ($LASTEXITCODE -ne 0) { throw "keytool failed ($LASTEXITCODE)" }

Write-Host ''
Write-Host "Created: $full"
Write-Host 'Back up this file and the password. Then, before building:'
Write-Host "  `$env:RE001_KEYSTORE = '$full'"
Write-Host "  `$env:RE001_KEYSTORE_PASS = '<password>'"
Write-Host "  `$env:RE001_KEY_ALIAS = '$Alias'"
Write-Host "  `$env:RE001_KEY_PASS = '<password>'"
