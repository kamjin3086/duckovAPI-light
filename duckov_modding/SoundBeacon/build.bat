@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ===============================================================================
echo                    SoundBeacon Mod Build Script
echo ===============================================================================
echo.

:: Check dotnet
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] dotnet not found!
    echo Please install .NET SDK: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/5] Cleaning old files...
if exist "bin\Release" (
    rmdir /s /q "bin\Release"
    echo   OK Cleaned bin\Release
)
if exist "obj" (
    rmdir /s /q "obj"
    echo   OK Cleaned obj
)

:: Delete old DLL from game directory
set GAME_MOD_DIR=E:\SteamLibrary\steamapps\common\Escape from Duckov\Duckov_Data\Mods\SoundBeacon
if exist "%GAME_MOD_DIR%\SoundBeacon.dll" (
    del /q "%GAME_MOD_DIR%\SoundBeacon.dll" 2>nul
    if %errorlevel% equ 0 (
        echo   OK Deleted old DLL from game directory
    ) else (
        echo   ! Cannot delete old DLL (game may be running)
    )
)

echo.
echo [2/5] Building...
dotnet build -c Release
if %errorlevel% neq 0 (
    echo.
    echo [ERROR] Build failed!
    echo Please check code errors or project configuration.
    pause
    exit /b 1
)
echo   OK Build succeeded

echo.
echo [3/5] Preparing release files...

:: Create release directory
set RELEASE_DIR=Release_Package\SoundBeacon
if not exist "%RELEASE_DIR%" mkdir "%RELEASE_DIR%"

:: Copy DLL
if exist "bin\Release\netstandard2.1\SoundBeacon.dll" (
    copy /y "bin\Release\netstandard2.1\SoundBeacon.dll" "%RELEASE_DIR%\"
    echo   OK Copied SoundBeacon.dll
) else (
    echo [ERROR] Cannot find compiled DLL!
    pause
    exit /b 1
)

:: Copy config
if exist "info.ini" (
    copy /y "info.ini" "%RELEASE_DIR%\"
    echo   OK Copied info.ini
)

:: Copy preview.png
if exist "preview.png" (
    copy /y "preview.png" "%RELEASE_DIR%\"
    echo   OK Copied preview.png
) else (
    echo   ! preview.png not found, please add manually
)

echo.
echo [4/5] Creating audio file placeholder...
if not exist "%RELEASE_DIR%\beacon_sound.wav" (
    echo Audio file naming options: > "%RELEASE_DIR%\audio_readme.txt"
    echo   - beacon_sound.wav (Recommended - Lossless) >> "%RELEASE_DIR%\audio_readme.txt"
    echo   - beacon_sound.ogg (Recommended - Compressed but good quality) >> "%RELEASE_DIR%\audio_readme.txt"
    echo   - beacon_sound.mp3 (Common format) >> "%RELEASE_DIR%\audio_readme.txt"
    echo   - sound.wav / sound.ogg / sound.mp3 >> "%RELEASE_DIR%\audio_readme.txt"
    echo. >> "%RELEASE_DIR%\audio_readme.txt"
    echo This mod uses FMOD engine, supports WAV/OGG/MP3 formats! >> "%RELEASE_DIR%\audio_readme.txt"
    echo NAudio.dll is no longer needed! >> "%RELEASE_DIR%\audio_readme.txt"
    echo   OK Created audio_readme.txt
    echo   ! Please remember to add audio file (WAV/OGG/MP3)
)

echo.
echo [5/5] Auto-deploying to game directory...
:: Auto copy new DLL to game mod directory
if exist "%GAME_MOD_DIR%" (
    copy /y "bin\Release\netstandard2.1\SoundBeacon.dll" "%GAME_MOD_DIR%\" >nul 2>&1
    if %errorlevel% equ 0 (
        echo   OK Auto-deployed new DLL to game directory
    ) else (
        echo   ! Cannot copy DLL (game may be running)
        echo   ! Please manually copy: %RELEASE_DIR%\SoundBeacon.dll
    )
) else (
    echo   ! Game mod directory does not exist, skipping auto-deploy
    echo   ! Directory: %GAME_MOD_DIR%
)

echo.
echo ===============================================================================
echo                           Build Complete!
echo ===============================================================================
echo.
echo Release files location: %RELEASE_DIR%
echo.
echo File list:
dir /b "%RELEASE_DIR%"
echo.
echo Next steps:
echo 1. Add audio file to release folder (WAV/OGG/MP3 - see audio readme)
echo 2. Add preview.png (256x256, square)
echo 3. Test mod functionality
echo 4. Package and release!
echo.
echo NOTE: This version uses FMOD audio engine (no NAudio.dll needed!)
echo.
echo ===============================================================================
pause
