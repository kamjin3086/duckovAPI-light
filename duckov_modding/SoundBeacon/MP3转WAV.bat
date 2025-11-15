@echo off
chcp 65001 >nul
setlocal EnableDelayedExpansion

:: ═══════════════════════════════════════════════════════════
::          MP3 转 WAV 一键转换工具 (自动安装FFmpeg)
:: ═══════════════════════════════════════════════════════════

echo.
echo ╔═══════════════════════════════════════════════════════════╗
echo ║        MP3 转 WAV 一键转换工具                            ║
echo ║        自动下载FFmpeg并转换当前目录所有MP3文件            ║
echo ╚═══════════════════════════════════════════════════════════╝
echo.

:: 设置FFmpeg下载URL和本地路径
set "FFMPEG_URL=https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"
set "FFMPEG_DIR=%~dp0ffmpeg"
set "FFMPEG_EXE=%FFMPEG_DIR%\ffmpeg.exe"

:: ═══════════════════════════════════════════════════════════
:: 第一步: 检查和安装FFmpeg
:: ═══════════════════════════════════════════════════════════

echo [1/3] 检查FFmpeg...
echo.

:: 检查当前目录是否有ffmpeg.exe
if exist "%~dp0ffmpeg.exe" (
    set "FFMPEG_EXE=%~dp0ffmpeg.exe"
    echo ✓ 发现本地FFmpeg: ffmpeg.exe
    goto :ffmpeg_ready
)

:: 检查ffmpeg子目录
if exist "%FFMPEG_EXE%" (
    echo ✓ 发现本地FFmpeg: ffmpeg\ffmpeg.exe
    goto :ffmpeg_ready
)

:: 检查系统PATH中的ffmpeg
where ffmpeg >nul 2>&1
if %errorlevel% equ 0 (
    set "FFMPEG_EXE=ffmpeg"
    echo ✓ 发现系统FFmpeg
    goto :ffmpeg_ready
)

:: 需要下载FFmpeg
echo ⚠ 未找到FFmpeg，准备自动下载...
echo.
echo 下载选项:
echo   1. 自动下载 ^(推荐^) - 约60MB，需要网络连接
echo   2. 手动下载 - 获取下载链接，自行下载后再运行
echo   3. 取消
echo.
set /p "download_choice=请选择 (1/2/3): "

if "%download_choice%"=="2" goto :manual_download
if "%download_choice%"=="3" goto :cancel_install
if not "%download_choice%"=="1" goto :cancel_install

:: 自动下载FFmpeg
echo.
echo ─────────────────────────────────────────────────────────
echo 正在下载FFmpeg...
echo 这可能需要几分钟，请稍候...
echo ─────────────────────────────────────────────────────────
echo.

:: 创建临时目录
set "TEMP_DIR=%~dp0temp_ffmpeg"
if not exist "%TEMP_DIR%" mkdir "%TEMP_DIR%"

:: 使用PowerShell下载
powershell -Command "& {[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12; $ProgressPreference = 'SilentlyContinue'; try { Write-Host '正在下载...'; Invoke-WebRequest -Uri '%FFMPEG_URL%' -OutFile '%TEMP_DIR%\ffmpeg.zip' -UseBasicParsing; Write-Host '✓ 下载完成'; exit 0 } catch { Write-Host '✗ 下载失败:' $_.Exception.Message; exit 1 }}"

if %errorlevel% neq 0 (
    echo.
    echo [错误] FFmpeg下载失败！
    echo.
    echo 可能的原因:
    echo   1. 网络连接问题
    echo   2. GitHub访问受限
    echo.
    echo 请尝试:
    echo   1. 检查网络连接
    echo   2. 使用VPN/代理
    echo   3. 选择手动下载方式
    echo.
    goto :manual_download_info
)

echo.
echo 正在解压FFmpeg...

:: 使用PowerShell解压
powershell -Command "& {Add-Type -AssemblyName System.IO.Compression.FileSystem; try { [System.IO.Compression.ZipFile]::ExtractToDirectory('%TEMP_DIR%\ffmpeg.zip', '%TEMP_DIR%'); Write-Host '✓ 解压完成'; exit 0 } catch { Write-Host '✗ 解压失败:' $_.Exception.Message; exit 1 }}"

if %errorlevel% neq 0 (
    echo [错误] 解压失败！
    rd /s /q "%TEMP_DIR%" 2>nul
    pause
    exit /b 1
)

:: 查找并复制ffmpeg.exe
echo 正在配置FFmpeg...
for /r "%TEMP_DIR%" %%f in (ffmpeg.exe) do (
    if not exist "%FFMPEG_DIR%" mkdir "%FFMPEG_DIR%"
    copy "%%f" "%FFMPEG_EXE%" >nul
    goto :cleanup_temp
)

:cleanup_temp
:: 清理临时文件
rd /s /q "%TEMP_DIR%" 2>nul

if exist "%FFMPEG_EXE%" (
    echo ✓ FFmpeg安装成功！
    echo.
) else (
    echo [错误] FFmpeg安装失败！
    pause
    exit /b 1
)

:ffmpeg_ready
echo ─────────────────────────────────────────────────────────
echo.

:: ═══════════════════════════════════════════════════════════
:: 第二步: 查找MP3文件
:: ═══════════════════════════════════════════════════════════

echo [2/3] 扫描MP3文件...
echo.

set "found_mp3=0"
set "count=0"
for %%f in (*.mp3) do (
    set "found_mp3=1"
    set /a count+=1
)

if %found_mp3%==0 (
    echo ⚠ 当前目录没有找到MP3文件
    echo.
    echo 当前目录: %CD%
    echo.
    pause
    exit /b 0
)

echo 发现 %count% 个MP3文件:
echo.
set "index=0"
for %%f in (*.mp3) do (
    set /a index+=1
    echo   !index!. %%f
)

echo.
echo ─────────────────────────────────────────────────────────
echo.

:: ═══════════════════════════════════════════════════════════
:: 第三步: 转换MP3到WAV
:: ═══════════════════════════════════════════════════════════

echo [3/3] 开始转换...
echo.
echo 转换设置:
echo   • 采样率: 44100 Hz ^(CD音质^)
echo   • 声道:   立体声 ^(2声道^)
echo   • 格式:   16-bit PCM WAV
echo.
echo ─────────────────────────────────────────────────────────
echo.

set "success_count=0"
set "skip_count=0"
set "fail_count=0"
set "current=0"

for %%f in (*.mp3) do (
    set /a current+=1
    set "input_file=%%f"
    set "output_file=%%~nf.wav"
    
    echo [!current!/%count%] %%f
    
    if exist "!output_file!" (
        echo       → 跳过 ^(WAV已存在^)
        set /a skip_count+=1
    ) else (
        "%FFMPEG_EXE%" -i "!input_file!" -ar 44100 -ac 2 -y "!output_file!" -loglevel error -hide_banner
        
        if !errorlevel! equ 0 (
            echo       → ✓ 成功转换
            set /a success_count+=1
        ) else (
            echo       → ✗ 转换失败
            set /a fail_count+=1
        )
    )
)

echo.
echo ═══════════════════════════════════════════════════════════
echo                    转换完成！
echo ═══════════════════════════════════════════════════════════
echo.
echo 统计:
echo   ✓ 成功: %success_count% 个
if %skip_count% gtr 0 echo   ⊘ 跳过: %skip_count% 个
if %fail_count% gtr 0 echo   ✗ 失败: %fail_count% 个
echo   ━ 总计: %count% 个
echo.

:: 询问是否删除原MP3
if %success_count% gtr 0 (
    echo ─────────────────────────────────────────────────────────
    set /p "delete_mp3=是否删除已转换的MP3文件? (Y/N): "
    if /i "!delete_mp3!"=="Y" (
        echo.
        for %%f in (*.mp3) do (
            set "wav_file=%%~nf.wav"
            if exist "!wav_file!" (
                del "%%f" 2>nul
                if !errorlevel! equ 0 (
                    echo   已删除: %%f
                )
            )
        )
        echo 完成！
    )
)

echo.
echo ═══════════════════════════════════════════════════════════
pause
exit /b 0

:: ═══════════════════════════════════════════════════════════
:: 辅助功能
:: ═══════════════════════════════════════════════════════════

:manual_download
echo.
echo ─────────────────────────────────────────────────────────
echo              手动下载说明
echo ─────────────────────────────────────────────────────────
echo.
:manual_download_info
echo 请按以下步骤操作:
echo.
echo 1. 访问以下网址下载FFmpeg:
echo    https://www.gyan.dev/ffmpeg/builds/
echo.
echo 2. 下载 "ffmpeg-release-essentials.zip"
echo.
echo 3. 解压后找到 ffmpeg.exe
echo.
echo 4. 将 ffmpeg.exe 复制到以下任一位置:
echo    • 本脚本所在目录
echo    • 本脚本所在目录\ffmpeg\ 子文件夹
echo.
echo 5. 重新运行本脚本
echo.
pause
exit /b 1

:cancel_install
echo.
echo 已取消安装
echo.
pause
exit /b 0

