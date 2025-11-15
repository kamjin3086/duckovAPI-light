# DuckovModding 通用工具类库

## 📚 概述

这个通用类库包含了多个Mod开发中常用的工具类,用于简化Mod开发流程并提高代码复用性。

**从以下Mod项目中提取的通用功能:**
- ✅ **SoundBeacon** - 场景管理、Shader扫描、调试工具、FMOD音频
- ✅ **ItemDropOnDrag** - UI射线检测、空白区域判断
- ✅ **UIScaleAdjuster** - Harmony补丁管理、设置持久化、组件标记

---

## 📦 包含的工具类

### 1. SceneUtils - 场景工具类 ⭐

提供场景相关的常用功能。

**主要功能**:
- `LogCurrentSceneInfo()` - 打印当前场景详细信息
- `ShouldOperateInCurrentScene()` - 判断是否应该在当前场景执行操作
- `GetCurrentSceneDisplayName()` - 获取当前场景显示名称
- `IsPlayerPresent()` - 检查玩家是否存在
- `GetPlayerPosition()` - 获取玩家位置

**使用示例**:
```csharp
using DuckovModding.Common;

// 打印场景信息
SceneUtils.LogCurrentSceneInfo("OnLevelInitialized", "[MyMod]");

// 判断是否在游戏地图(排除大厅、菜单等)
if (SceneUtils.ShouldOperateInCurrentScene(logPrefix: "[MyMod]"))
{
    // 在游戏地图中执行操作
    SpawnGameObjects();
}

// 获取玩家位置
Vector3? playerPos = SceneUtils.GetPlayerPosition();
if (playerPos.HasValue)
{
    Debug.Log($"玩家在: {playerPos.Value}");
}
```

---

### 2. ShaderScanner - Shader扫描工具类 ⭐

帮助分析场景中使用的Shader和材质,用于制作3D模型时选择合适的Shader。

**主要功能**:
- `ScanSceneShaders()` - 扫描场景中所有Shader
- `ScanNearbyMaterials()` - 扫描指定位置附近物体的材质

**使用示例**:
```csharp
using DuckovModding.Common;

// 扫描整个场景的Shader
var shaderInfos = ShaderScanner.ScanSceneShaders("[MyMod]");
// 获取最常用的Shader
var mostUsedShader = shaderInfos.Values.OrderByDescending(x => x.UsageCount).First();
Debug.Log($"最常用的Shader: {mostUsedShader.ShaderName}");

// 扫描玩家周围20米内的物体材质
Vector3 playerPos = CharacterMainControl.Main.transform.position;
var nearbyMaterials = ShaderScanner.ScanNearbyMaterials(playerPos, 20f, 15, "[MyMod]");
```

---

### 3. DebugUtils - 调试工具类 ⭐

提供各种调试辅助功能。

**主要功能**:
- `PrintPlayerPosition()` - 打印玩家位置
- `PrintBoxedLog()` - 打印带标题和边框的日志
- `PrintSeparator()` - 打印分隔线
- `PrintObjectHierarchy()` - 打印物体层级结构
- `FormatVector3()` - 格式化Vector3输出
- `FormatColor()` - 格式化颜色输出
- `DrawDebugSphere()` - 在场景中绘制调试球体
- `DrawDebugCircle()` - 在场景中绘制调试圆

**使用示例**:
```csharp
using DuckovModding.Common;

// 打印玩家位置
DebugUtils.PrintPlayerPosition("[MyMod]");

// 打印带边框的日志
DebugUtils.PrintBoxedLog("初始化完成", "所有系统已就绪", "[MyMod]");

// 打印物体层级
GameObject myObject = GameObject.Find("MyObject");
DebugUtils.PrintObjectHierarchy(myObject, 3, "[MyMod]");

// 格式化输出
Vector3 pos = new Vector3(1.234f, 5.678f, 9.012f);
Debug.Log($"位置: {DebugUtils.FormatVector3(pos, 2)}");

// 绘制调试球体(在Scene视图中可见)
DebugUtils.DrawDebugSphere(playerPos, 5f, Color.red, 10f);
```

---

### 4. AudioUtils - 音频工具类 (FMOD) ⭐

提供FMOD音频加载和播放的封装。

**主要功能**:
- `LoadAudioWithFMOD()` - 使用FMOD加载音频文件
- `Play3DSound()` - 在指定位置播放3D音频
- `ReleaseSound()` - 释放FMOD Sound资源
- `StopChannel()` - 停止并释放Channel
- `FindAudioFile()` - 在Mod目录中查找音频文件

**使用示例**:
```csharp
using DuckovModding.Common;

// 查找音频文件
string modPath = Path.GetDirectoryName(GetType().Assembly.Location);
string audioPath = AudioUtils.FindAudioFile(modPath);

if (!string.IsNullOrEmpty(audioPath))
{
    // 加载音频(3D + 不循环)
    FMOD.Sound sound = AudioUtils.LoadAudioWithFMOD(
        audioPath, 
        FMOD.MODE.DEFAULT | FMOD.MODE._3D | FMOD.MODE.LOOP_OFF,
        "[MyMod]"
    );

    if (sound.hasHandle())
    {
        // 在指定位置播放3D音频
        Vector3 playPos = new Vector3(10f, 0f, 20f);
        FMOD.Channel channel = AudioUtils.Play3DSound(
            sound, 
            playPos, 
            volume: 0.8f,
            minDistance: 5f,
            maxDistance: 50f,
            "[MyMod]"
        );
    }
}

// 清理时释放资源
void OnDestroy()
{
    AudioUtils.ReleaseSound(ref sound, "[MyMod]");
}
```

### 5. HarmonyUtils - Harmony补丁工具类 🆕

提供Harmony补丁的初始化和管理功能。

**主要功能**:
- `InitializeAndPatch()` - 初始化并应用Harmony补丁
- `UnpatchAll()` - 移除所有补丁
- `GetPatchedMethodsInfo()` - 获取补丁信息
- `PrintPatchedMethods()` - 打印补丁详情

**使用示例**:
```csharp
using DuckovModding.Common;
using HarmonyLib;

// 在Awake中初始化Harmony
private Harmony harmony;

void Awake()
{
    harmony = HarmonyUtils.InitializeAndPatch(
        "com.mymod.modname",
        typeof(ModBehaviour).Assembly,
        "[MyMod]"
    );
    
    // 打印补丁信息
    HarmonyUtils.PrintPatchedMethods(harmony, "[MyMod]");
}

void OnDestroy()
{
    HarmonyUtils.UnpatchAll(harmony, "com.mymod.modname", "[MyMod]");
}
```

---

### 6. UIUtils - UI工具类 🆕

提供UI创建和操作的常用功能。

**主要功能**:
- `CreateCanvas()` - 创建Canvas
- `CreateText()` - 创建TextMeshProUGUI文本
- `CreateButton()` - 创建按钮
- `IsPointerOverUI()` - 检查鼠标是否在UI上
- `GetUIElementsUnderMouse()` - 获取鼠标下的UI元素
- `IsMouseOverEmptyArea()` - 检查是否在空白区域
- `GetGameObjectPath()` - 获取GameObject完整路径
- `SetFullScreen()` / `SetCenter()` - 设置RectTransform
- `MakeDraggable()` - 使UI可拖动

**使用示例**:
```csharp
using DuckovModding.Common;

// 创建Canvas
Canvas canvas = UIUtils.CreateCanvas("MyModCanvas", sortingOrder: 100);

// 创建按钮
Button button = UIUtils.CreateButton(
    canvas.transform,
    "MyButton",
    "点击我",
    onClick: () => Debug.Log("按钮被点击!"),
    size: new Vector2(200f, 50f)
);

// 检查鼠标是否在空白区域(排除自己的UI)
if (UIUtils.IsMouseOverEmptyArea(canvas.gameObject))
{
    Debug.Log("鼠标在空白区域");
}

// 使UI可拖动
UIUtils.MakeDraggable(panelGameObject);
```

---

### 7. SettingsUtils - 设置工具类 🆕

提供游戏设置的保存和加载功能。

**主要功能**:
- `SaveInt()` / `LoadInt()` - 保存/加载int
- `SaveFloat()` / `LoadFloat()` - 保存/加载float
- `SaveBool()` / `LoadBool()` - 保存/加载bool
- `SaveString()` / `LoadString()` - 保存/加载string
- `Save<T>()` / `Load<T>()` - 保存/加载泛型(枚举等)
- `ModSettingsManager` - Mod专用设置管理器

**使用示例**:
```csharp
using DuckovModding.Common;

// 直接使用
SettingsUtils.SaveInt("MyMod_Volume", 80, "[MyMod]");
int volume = SettingsUtils.LoadInt("MyMod_Volume", 100, "[MyMod]");

// 使用Mod专用管理器(推荐)
var settings = new SettingsUtils.ModSettingsManager("MyMod", "[MyMod]");
settings.SaveInt("Volume", 80);
int volume = settings.LoadInt("Volume", 100);

// 保存枚举
settings.Save<QualityLevel>("GraphicsQuality", QualityLevel.High);
QualityLevel quality = settings.Load<QualityLevel>("GraphicsQuality", QualityLevel.Medium);
```

---

### 8. ComponentUtils - 组件工具类 🆕

提供Unity组件创建和管理的常用功能。

**主要功能**:
- `CreatePersistentObject()` - 创建持久化GameObject
- `GetOrAddComponent()` - 安全地获取或添加组件
- `SafeDestroy()` - 安全地销毁对象
- `FindComponent()` / `FindAllComponents()` - 查找组件
- `SetActiveRecursive()` - 递归设置激活状态
- `SetLayerRecursive()` - 递归设置Layer
- `Clone()` - 克隆GameObject
- `DelayedCall()` / `NextFrameCall()` - 延迟调用
- `AddMarker()` / `HasMarker()` / `RemoveMarker()` - 组件标记

**使用示例**:
```csharp
using DuckovModding.Common;

// 创建持久化对象
GameObject manager = ComponentUtils.CreatePersistentObject("MyModManager");

// 或者直接添加组件
MyComponent comp = ComponentUtils.CreatePersistentObject<MyComponent>("MyModManager");

// 安全地获取或添加组件
AudioSource audio = ComponentUtils.GetOrAddComponent<AudioSource>(gameObject);

// 下一帧执行
ComponentUtils.NextFrameCall(this, () => {
    Debug.Log("下一帧执行");
});

// 延迟2秒执行
ComponentUtils.DelayedCall(this, 2f, () => {
    Debug.Log("2秒后执行");
});

// 使用标记系统
ComponentUtils.AddMarker<UIScaleMarker>(canvasObject);
if (ComponentUtils.HasMarker<UIScaleMarker>(canvasObject))
{
    Debug.Log("该对象已处理过");
}
```

---

## 🔧 集成到你的Mod项目

### 方法1: 复制文件

1. 将整个 `Common` 文件夹复制到你的Mod项目中
2. 在 `.csproj` 文件中添加这些文件的引用
3. 在代码中添加 `using DuckovModding.Common;`

### 方法2: 创建共享库项目

1. 创建一个独立的 `DuckovModding.Common.csproj` 项目
2. 将所有工具类放入其中
3. 编译成DLL
4. 在你的Mod项目中引用这个DLL

**推荐使用方法1**,因为每个Mod独立打包更方便。

---

## 📝 在 .csproj 中添加这些文件

```xml
<ItemGroup>
  <!-- 场景和调试相关 -->
  <Compile Include="Common\SceneUtils.cs" />
  <Compile Include="Common\DebugUtils.cs" />
  
  <!-- Shader和材质扫描 -->
  <Compile Include="Common\ShaderScanner.cs" />
  
  <!-- 音频相关(FMOD) -->
  <Compile Include="Common\AudioUtils.cs" />
  
  <!-- Harmony补丁管理 -->
  <Compile Include="Common\HarmonyUtils.cs" />
  
  <!-- UI工具 -->
  <Compile Include="Common\UIUtils.cs" />
  
  <!-- 设置持久化 -->
  <Compile Include="Common\SettingsUtils.cs" />
  
  <!-- 组件管理 -->
  <Compile Include="Common\ComponentUtils.cs" />
</ItemGroup>
```

---

## 🎯 实际应用示例: 重构SoundBeacon Mod

### 重构前:
```csharp
// ModBehaviour.cs中有450+行的代码,包括大量辅助方法
private void LogCurrentSceneInfo(string eventName) { ... }
private void ScanSceneShaders() { ... }
private void ScanNearbyMaterials() { ... }
private void PrintPlayerPosition() { ... }
// ... 更多代码
```

### 重构后:
```csharp
using DuckovModding.Common;

// ModBehaviour.cs只保留核心逻辑
private void OnLevelInitialized()
{
    SceneUtils.LogCurrentSceneInfo("OnLevelInitialized", "[SoundBeacon]");
    
    if (SceneUtils.ShouldOperateInCurrentScene(logPrefix: "[SoundBeacon]"))
    {
        SpawnBeacons();
    }
}

void Update()
{
    if (Input.GetKeyDown(KeyCode.H))
    {
        ShaderScanner.ScanSceneShaders("[SoundBeacon]");
    }
    
    if (Input.GetKeyDown(KeyCode.J))
    {
        var playerPos = SceneUtils.GetPlayerPosition();
        if (playerPos.HasValue)
        {
            ShaderScanner.ScanNearbyMaterials(playerPos.Value, 20f, 15, "[SoundBeacon]");
        }
    }
}
```

**优势**:
- 代码更简洁,从1000+行减少到600+行
- 逻辑更清晰,核心功能和辅助功能分离
- 可以在其他Mod中直接复用这些工具类
- 更容易维护和测试

---

## 🚀 使用建议

1. **统一日志前缀**: 建议每个Mod使用自己的日志前缀,如 `[MyMod]`
2. **异常处理**: 这些工具类已经包含了异常处理,可以安全使用
3. **性能考虑**: 
   - `ScanSceneShaders()` 会遍历所有Renderer,可能需要1-2秒
   - `ScanNearbyMaterials()` 使用物理查询,建议不要太频繁调用
4. **FMOD音频**: 确保你的项目引用了 `FMODUnity.dll` 和 `fmodstudio.dll`

---

## 📖 扩展这个库

如果你开发了新的通用功能,欢迎添加到这个库中:

1. 创建新的工具类文件
2. 使用 `namespace DuckovModding.Common`
3. 所有方法使用 `static` 和 `public`
4. 添加详细的XML注释
5. 更新这个README文档

---

## 🤝 贡献

这个工具库是社区共享的,如果你有好的想法或改进建议,欢迎贡献!

---

## ⚠️ 注意事项

1. 这些工具类依赖游戏的API,确保引用了必要的DLL:
   - `UnityEngine.dll`
   - `UnityEngine.CoreModule.dll`
   - `UnityEngine.PhysicsModule.dll`
   - `TeamSoda.Duckov.Core.dll`
   - `FMODUnity.dll`
   - `fmodstudio.dll`

2. 命名空间统一使用 `DuckovModding.Common`

3. 所有工具类都是静态类,不需要实例化

---

## 📄 许可

这些工具类可以自由使用和修改,用于任何Duckov游戏的Mod开发。

---

**祝你Mod开发愉快! 🎮**

