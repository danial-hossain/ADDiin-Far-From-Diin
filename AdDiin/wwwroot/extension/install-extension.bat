@echo off
title ADDiin Focus Shield Installer
color 0A
echo ========================================================
echo         ADDiin Focus Shield - 1-Click Extension Setup
echo ========================================================
echo.
echo Installing ADDiin Focus Shield for Google Chrome & Microsoft Edge...
echo.

set "EXT_PATH=C:\Addin SD 5\Ad-Diin\AdDiin\wwwroot\extension"

:: Open Chrome extensions page and folder
echo [1/2] Opening Chrome / Edge extensions manager...
start chrome "chrome://extensions" 2>nul || start msedge "edge://extensions" 2>nul

echo [2/2] Opening extension folder...
explorer "%EXT_PATH%"

echo.
echo ========================================================
echo  DONE! Just drag the opened folder into your browser 
echo  or click 'Load unpacked' and paste:
echo  %EXT_PATH%
echo ========================================================
echo.
pause
