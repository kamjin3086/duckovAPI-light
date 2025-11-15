# SoundBeacon Mod

一个在游戏地图中生成带有3D空间音效信标的Unity Mod。现已使用FMOD音频引擎进行高质量音频播放!

## 🎵 新特性 (FMOD版本)

- ✅ **使用游戏自带FMOD引擎** - 完美兼容,不再依赖NAudio
- ✅ **支持更多音频格式** - WAV, OGG, MP3 全部支持
- ✅ **更稳定的音频加载** - 解决了Unity AudioClip.SetData的bug
- ✅ **更好的3D音效** - FMOD专业级3D音频定位

## 项目结构

```
SoundBeacon/
├── ModBehaviour.cs          # 主Mod逻辑
├── SoundBeaconObject.cs     # 信标对象组件
├── SoundBeacon.csproj       # 项目文件
├── SoundBeacon.sln          # 解决方案文件
├── info.ini                 # Mod元数据
├── build.bat                # 构建和打包脚本
└── Release_Package/         # 打包好的Mod文件
```

## 快速开始

### 1. 构建Mod
```bash
build.bat
```

### 2. 准备音频文件

支持的音频文件名 (任选其一):
- `beacon_sound.wav` (推荐 - 无损)
- `beacon_sound.ogg` (推荐 - 压缩但质量好)
- `beacon_sound.mp3` (常见格式)
- `sound.wav` / `sound.ogg` / `sound.mp3`

**推荐设置:**
- 采样率: 44100Hz
- 声道: 立体声 (2通道)
- 格式: WAV 或 OGG

### 3. 部署
将文件复制到游戏Mod目录:
```
<游戏安装目录>/Duckov_Data/Mods/SoundBeacon/
  ├── SoundBeacon.dll
  ├── info.ini
  └── beacon_sound.wav  (或其他支持的音频文件)
```

**注意:** 不再需要 NAudio.dll!

## 音频格式支持

| 格式 | 文件大小 | FMOD支持 | 推荐度 |
|------|---------|---------|-------|
| **WAV** | 较大 (~29MB) | ✓ 原生支持 | ⭐⭐⭐⭐⭐ (最佳) |
| **OGG** | 中等 (~4MB) | ✓ 原生支持 | ⭐⭐⭐⭐ (推荐) |
| **MP3** | 中等 (~5MB) | ✓ 原生支持 | ⭐⭐⭐ (兼容) |

## 功能特性

- 🔊 **3D空间音效** - 随距离衰减的真实音效
- 💡 **可视化信标** - 发光立方体 + 点光源
- 🎯 **多种生成模式** - 固定位置 / 相对玩家位置
- ⌨️ **调试快捷键**:
  - `P` - 打印玩家位置
  - `[` - 在当前位置生成信标
  - `]` - 清除所有信标

## 配置参数

可在代码中调整:
- `minInterval` / `maxInterval` - 播放间隔 (3-10秒)
- `minDistance` / `maxDistance` - 音效范围 (5-50米)
- `volume` - 音量 (0.8)
- `spawnMode` - 生成模式 (FixedPositions/RelativeToPlayer)

## 开发信息

- **框架**: .NET Standard 2.1
- **音频引擎**: FMOD (游戏自带)
- **Unity版本**: 与游戏版本兼容
- **依赖**: FMODUnity.dll, FMOD.dll (游戏自带)

## 技术说明

### 为什么切换到FMOD?

1. **Unity AudioClip.SetData Bug**: Unity的AudioClip在运行时创建时存在已知bug,即使SetData返回true,AudioClip也可能为空
2. **FMOD是游戏原生音频引擎**: 直接使用游戏自带的FMOD系统,完美兼容
3. **更专业的3D音效**: FMOD提供更精确的3D音频定位和衰减控制
4. **支持更多格式**: FMOD原生支持WAV/OGG/MP3等多种格式

### FMOD使用示例

```csharp
// 加载音频
FMOD.System coreSystem = FMODUnity.RuntimeManager.StudioSystem.getCoreSystem();
coreSystem.createSound(audioPath, FMOD.MODE._3D, out FMOD.Sound sound);

// 播放音频
coreSystem.playSound(sound, null, false, out FMOD.Channel channel);

// 设置3D位置和距离衰减
channel.set3DAttributes(ref position, ref velocity);
channel.set3DMinMaxDistance(minDistance, maxDistance);
channel.setVolume(volume);
```

## 常见问题

**Q: 为什么不再使用NAudio?**  
A: NAudio虽然能解码MP3,但创建的AudioClip在某些Unity版本中存在bug。FMOD是更可靠的选择。

**Q: 音频文件没有播放?**  
A: 检查日志输出,确保音频文件名正确,路径存在,且FMOD能成功加载。

**Q: 听不到声音?**  
A: 确保玩家在音效范围内 (maxDistance 默认50米),并且音量设置合理。

## 更新日志

### v2.0 (FMOD版本)
- ✨ 切换到FMOD音频引擎
- ✨ 移除NAudio依赖
- ✨ 支持WAV/OGG/MP3所有格式
- 🐛 修复AudioClip.SetData的bug
- ⚡ 提升音频加载稳定性

### v1.0 (NAudio版本)
- 初始版本
- 使用NAudio解码MP3
- 基础3D音效功能

## 许可证

本项目仅供学习和研究使用。
