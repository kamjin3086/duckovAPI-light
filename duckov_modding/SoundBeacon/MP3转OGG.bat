@echo off
setlocal EnableDelayedExpansion

echo.
echo ================================================
echo       MP3 to OGG Converter
echo ================================================
echo.

:: Check FFmpeg
set "FFMPEG_EXE=ffmpeg"

where ffmpeg >nul 2>&1
if errorlevel 1 (
    if exist "%~dp0ffmpeg.exe" (
        set "FFMPEG_EXE=%~dp0ffmpeg.exe"
    ) else (
        if exist "%~dp0ffmpeg\ffmpeg.exe" (
            set "FFMPEG_EXE=%~dp0ffmpeg\ffmpeg.exe"
        ) else (
            echo [Error] FFmpeg not found
            echo.
            echo Please place ffmpeg.exe in:
            echo   - Current directory
            echo   - ffmpeg\ subdirectory
            echo.
            pause
            exit /b 1
        )
    )
)

echo [OK] Found FFmpeg
echo.

:: Count MP3 files
set "count=0"
for %%f in (*.mp3) do (
    set /a count+=1
)

if %count%==0 (
    echo [Error] No MP3 files found
    pause
    exit /b 0
)

echo Found %count% MP3 file(s)
echo.
echo Conversion settings:
echo   Format: OGG Vorbis
echo   Sample rate: 44100 Hz
echo   Channels: Stereo
echo   Quality: High (level 6)
echo.
echo Starting conversion...
echo.

set "success=0"
set "current=0"

for %%f in (*.mp3) do (
    set /a current+=1
    set "output=%%~nf.ogg"
    
    echo [!current!/%count%] Converting: %%f
    
    if exist "!output!" (
        echo       Skip - OGG already exists
    ) else (
        "%FFMPEG_EXE%" -i "%%f" -ar 44100 -ac 2 -q:a 6 -y "!output!" -loglevel error -hide_banner
        
        if errorlevel 1 (
            echo       [Failed] Conversion error
        ) else (
            echo       [OK] Success
            set /a success+=1
        )
    )
    echo.
)

echo ================================================
echo Conversion completed
echo ================================================
echo.
echo Success: %success% / %count%
echo.

if %success% gtr 0 (
    echo [Notice] OGG files are ready to use
    echo Copy them to your game Mod directory
    echo.
)

pause

