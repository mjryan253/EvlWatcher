@echo off
echo Creating EvlWatcher Release Package v2.1.63
echo ==========================================

REM Create release directory
if not exist "Release" mkdir Release

REM Copy the latest setup file
copy "Versions\v2\EvlWatcher-v2.1.62-setup.exe" "Release\EvlWatcher-v2.1.63-setup.exe"

REM Copy release notes
copy "Versions\v2\EvlWatcher-v2.1.63\EvlWatcher-v2.1.63 release notes.txt" "Release\"

echo.
echo Release package created successfully!
echo Files created:
echo - Release\EvlWatcher-v2.1.63-setup.exe
echo - Release\EvlWatcher-v2.1.63 release notes.txt
echo.
echo The setup file is ready for Windows deployment.
pause
