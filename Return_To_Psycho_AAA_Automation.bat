@echo off
setlocal EnableExtensions EnableDelayedExpansion
title Psycho AAA Resume + Automation

set "PROJECT=C:\Users\xzero\Downloads\kandarin\necrotic_server-item_attributes"
set "UNITY_PROJECT=%PROJECT%\psycho_unity_client"
set "UNITY_EXE=C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe"
set "CODEX_EXE=C:\Users\xzero\AppData\Roaming\npm\codex.cmd"
set "CODEX_YOLO=--yolo -s danger-full-access -a never --search"
set "PROMPT_FILE=%PROJECT%\PSYCHO_AAA_AUTOMATION_RESUME_PROMPT.md"
set "PLAYABLE=%UNITY_PROJECT%\Builds\PsychoUnityClient\Psycho.exe"
set "ICLOUD_BACKUPS=C:\Users\xzero\iCloudDrive\PsychoBackups"

if not exist "%PROJECT%" (
  echo Project folder not found:
  echo   %PROJECT%
  pause
  exit /b 1
)

cd /d "%PROJECT%"
if errorlevel 1 (
  echo Could not enter project folder.
  pause
  exit /b 1
)

:MENU
cls
echo ============================================================
echo Psycho AAA Resume + Automation
echo ============================================================
echo Project: %PROJECT%
echo Branch:  codex/psycho-rebrand
echo Latest:  cd362b97 Improve intro sequence and nature staging
echo.
echo This launcher uses:
echo   Codex mode: codex --yolo
echo   Codex sandbox: danger-full-access
echo   Codex approval policy: never
echo   Codex web search: enabled
echo.
echo 1. Resume last Codex session here
echo 2. Start one autonomous Codex AAA graphics/gameplay pass
echo 3. Run verified Unity QA/build/backup automation
echo 4. Launch current Psycho playable
echo 5. Open Unity project
echo 6. Show resume prompt
echo 7. Git status and latest commit
echo 8. Exit
echo.
set /p "CHOICE=Choose an option: "

if "%CHOICE%"=="1" goto RESUME_CODEX
if "%CHOICE%"=="2" goto CODEX_AUTOPASS
if "%CHOICE%"=="3" goto UNITY_AUTOMATION
if "%CHOICE%"=="4" goto LAUNCH_PLAYABLE
if "%CHOICE%"=="5" goto OPEN_UNITY
if "%CHOICE%"=="6" goto SHOW_PROMPT
if "%CHOICE%"=="7" goto GIT_STATUS
if "%CHOICE%"=="8" goto DONE
goto MENU

:RESUME_CODEX
if not exist "%CODEX_EXE%" (
  echo Codex CLI not found: %CODEX_EXE%
  pause
  goto MENU
)
echo Resuming the last Codex session in this project...
"%CODEX_EXE%" resume --last -C "%PROJECT%" %CODEX_YOLO% "Read PSYCHO_AAA_AUTOMATION_RESUME_PROMPT.md first. Resume the Psycho Unity MMO work from the latest checkpoint, obey the hard rules, and continue the recommended next pass with safe backups, verification, commit, and push."
pause
goto MENU

:CODEX_AUTOPASS
if not exist "%CODEX_EXE%" (
  echo Codex CLI not found: %CODEX_EXE%
  pause
  goto MENU
)
if not exist "%PROJECT%\run-logs" mkdir "%PROJECT%\run-logs"
echo Starting one non-interactive autonomous Codex work pass...
echo Codex will read the resume prompt, choose the next strongest graphics/gameplay pass, verify, back up, commit, and push if stable.
"%CODEX_EXE%" exec -C "%PROJECT%" %CODEX_YOLO% --output-last-message "%PROJECT%\run-logs\codex-aaa-automation-last-message.txt" "Read PSYCHO_AAA_AUTOMATION_RESUME_PROMPT.md first. Continue the Psycho AAA-quality Unity MMO work autonomously. Pick the next strongest pass from the prompt, keep server/cache/protocol untouched unless explicitly justified, use only licensed/original/local assets, rebuild, render proof screenshots, write budget report, build Windows playable, smoke launch, create iCloud backup, commit and push stable milestones, then report results and next pass."
pause
goto MENU

:UNITY_AUTOMATION
if not exist "%UNITY_EXE%" (
  echo Unity Editor not found:
  echo   %UNITY_EXE%
  pause
  goto MENU
)
if not exist "%PROJECT%\run-logs" mkdir "%PROJECT%\run-logs"
for /f %%I in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd-HHmmss"') do set "STAMP=%%I"
echo Running full verified Unity automation with stamp %STAMP%...
call :RUN_UNITY Psycho.Editor.PsychoHostedWorldSceneBuilder.BuildHostedTestWorldSceneBatch unity-hosted-scene-build-%STAMP%.log || goto AUTOMATION_FAILED
call :RUN_UNITY Psycho.Editor.PsychoLoginSceneBuilder.RenderLoginScenePreviewBatch unity-login-preview-%STAMP%.log || goto AUTOMATION_FAILED
call :RUN_UNITY Psycho.Editor.PsychoHostedWorldSceneBuilder.RenderHostedPrisonIntroPreviewBatch unity-prison-preview-%STAMP%.log || goto AUTOMATION_FAILED
call :RUN_UNITY Psycho.Editor.PsychoHostedWorldSceneBuilder.RenderHostedLushMeadowPreviewBatch unity-lush-meadow-preview-%STAMP%.log || goto AUTOMATION_FAILED
call :RUN_UNITY Psycho.Editor.PsychoHostedWorldSceneBuilder.RenderHostedGiantMammothPreviewBatch unity-giant-mammoth-preview-%STAMP%.log || goto AUTOMATION_FAILED
call :RUN_UNITY Psycho.Editor.PsychoSceneBudgetReporter.WriteHostedSceneBudgetReportBatch unity-budget-%STAMP%.log || goto AUTOMATION_FAILED
call :RUN_UNITY Psycho.Editor.PsychoLoginSceneBuilder.BuildWindowsLoginPlayableBatch unity-login-playable-build-%STAMP%.log || goto AUTOMATION_FAILED

echo Smoke launching playable for 12 seconds...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$exe='%PLAYABLE%'; if (!(Test-Path $exe)) { throw 'Playable not found: ' + $exe }; $p=Start-Process -FilePath $exe -PassThru -WindowStyle Hidden; Start-Sleep -Seconds 12; if (-not $p.HasExited) { Stop-Process -Id $p.Id -Force; 'Smoke launch alive after 12 seconds; stopped process ' + $p.Id } else { throw 'Smoke launch exited early with code ' + $p.ExitCode }"
if errorlevel 1 goto AUTOMATION_FAILED

echo Backing up runnable build to iCloud...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$stamp=Get-Date -Format 'yyyyMMdd-HHmmss'; $target=Join-Path '%ICLOUD_BACKUPS%' ('Psycho_Runnable_' + $stamp + '_BatchAutomation'); New-Item -ItemType Directory -Path $target -Force | Out-Null; Copy-Item -Path '%UNITY_PROJECT%\Builds\PsychoUnityClient' -Destination $target -Recurse -Force; $sum=Get-ChildItem -Path $target -Recurse -File | Measure-Object -Property Length -Sum; 'Backup: ' + $target; 'Files: ' + $sum.Count; 'Bytes: ' + $sum.Sum"
if errorlevel 1 goto AUTOMATION_FAILED

echo.
echo Unity automation completed successfully.
pause
goto MENU

:RUN_UNITY
set "METHOD=%~1"
set "LOGNAME=%~2"
echo.
echo Running %METHOD%
"%UNITY_EXE%" -batchmode -quit -projectPath "%UNITY_PROJECT%" -executeMethod %METHOD% -logFile "%PROJECT%\run-logs\%LOGNAME%"
if errorlevel 1 (
  echo FAILED: %METHOD%
  echo Log: %PROJECT%\run-logs\%LOGNAME%
  exit /b 1
)
exit /b 0

:AUTOMATION_FAILED
echo.
echo Automation failed. Check the latest log in:
echo   %PROJECT%\run-logs
pause
goto MENU

:LAUNCH_PLAYABLE
if not exist "%PLAYABLE%" (
  echo Playable not found:
  echo   %PLAYABLE%
  pause
  goto MENU
)
start "" "%PLAYABLE%"
goto MENU

:OPEN_UNITY
if not exist "%UNITY_EXE%" (
  echo Unity Editor not found:
  echo   %UNITY_EXE%
  pause
  goto MENU
)
start "" "%UNITY_EXE%" -projectPath "%UNITY_PROJECT%"
goto MENU

:SHOW_PROMPT
if exist "%PROMPT_FILE%" (
  start "" notepad "%PROMPT_FILE%"
) else (
  echo Resume prompt not found:
  echo   %PROMPT_FILE%
  pause
)
goto MENU

:GIT_STATUS
git status --short --branch
git log -1 --oneline
pause
goto MENU

:DONE
endlocal
exit /b 0
