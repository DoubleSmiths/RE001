@echo off
setlocal

rem ===========================================================================
rem  RE001 에디터 실행기 - "Recovering Scene Backups" 창 없이 바로 연다.
rem
rem  왜 그 창이 뜨나
rem    Temp\__Backupscenes\*.backup 이 남아 있을 때만 뜬다. 유니티는 정상 종료하면
rem    이 폴더를 스스로 지운다. 즉 창이 떴다는 건 직전 세션이 제대로 못 끝났다는 뜻.
rem
rem    실측: Editor-prev.log 끝이 d3d12 fence 오류 3034줄, 정상 종료 흔적 0줄.
rem    GPU가 죽어 에디터가 멈춘 것이다 (RTX 5070 + Direct3D 12).
rem    그래서 아래에서 -force-d3d11 로 띄운다.
rem
rem  하는 일
rem    1) 유니티가 이미 떠 있으면 아무것도 안 한다
rem    2) 남은 씬 백업을 지운다 - 복구 창이 뜰 이유를 없앤다
rem    3) Direct3D 11 로 연다
rem
rem  [주의] 백업을 지운다. 크래시에서 씬을 되살려야 하는 상황이라면 이걸 쓰지 말고
rem         유니티 허브로 열어 [Yes]를 누를 것.
rem ===========================================================================

set "PROJECT=%~dp0.."
set "UNITY=C:\Program Files\Unity\Hub\Editor\6000.3.10f1\Editor\Unity.exe"

rem 윈도우 기본 도구는 **절대경로**로 부른다. Git Bash 같은 셸에서 실행하면
rem  PATH 앞쪽의 동명이인(예: unix find)이 가려버려 판정이 통째로 뒤집힌다.
rem  실제로 그 때문에 가드가 뚫려 실행 중인데도 정리를 해버린 적이 있다.
set "TASKLIST=%SystemRoot%\System32\tasklist.exe"
set "FINDSTR=%SystemRoot%\System32\findstr.exe"

if not exist "%UNITY%" (
    echo [오류] 유니티를 찾지 못했습니다: "%UNITY%"
    echo        버전을 바꿨다면 이 파일의 UNITY 경로를 고치세요.
    pause
    exit /b 1
)

rem --- 1) 실행 중이면 손대지 않는다 ----------------------------------------
rem  판정 도구가 없으면 "모른다"로 보고 그냥 빠진다 - 지우는 쪽으로 기울지 않는다.
if not exist "%TASKLIST%" goto :unknown
if not exist "%FINDSTR%" goto :unknown

"%TASKLIST%" /nh /fi "imagename eq Unity.exe" 2>nul | "%FINDSTR%" /i /c:"Unity.exe" >nul
if not errorlevel 1 (
    echo 유니티가 이미 실행 중입니다. 그대로 둡니다.
    exit /b 0
)
goto :clean

:unknown
echo [경고] 실행 여부를 확인할 수 없어 정리를 건너뜁니다.
exit /b 1

rem --- 2) 지난 세션의 잔해 정리 --------------------------------------------
:clean
if exist "%PROJECT%\Temp\__Backupscenes" (
    echo 지난 세션의 씬 백업을 지웁니다.
    rmdir /s /q "%PROJECT%\Temp\__Backupscenes"
)

rem --- 3) D3D11 로 실행 -----------------------------------------------------
echo 에디터를 엽니다 ^(Direct3D 11^)...
start "" "%UNITY%" -force-d3d11 -projectPath "%PROJECT%"

endlocal
