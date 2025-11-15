# SoundBeacon Mod - Shader扫描功能使用说明

## 📋 功能概述

为了帮助你为Mod制作3D模型时选择正确的Shader，我们添加了两个强大的Shader扫描功能。

---

## 🎯 新增快捷键

| 快捷键 | 功能 | 说明 |
|--------|------|------|
| **H** | 扫描场景Shader | 扫描整个场景中所有使用的Shader，按使用频率排序 |
| **J** | 扫描附近材质 | 扫描玩家周围20米内物体的详细材质信息 |

---

## 🔍 功能详解

### 1. 场景Shader扫描 (按H键)

**作用**: 全面扫描当前场景中所有使用的Shader，统计使用频率，找出游戏最常用的Shader。

**使用步骤**:
1. 进入游戏场景(建议在游戏地图中，不要在大厅)
2. 按 **H** 键
3. 查看控制台日志

**输出内容**:
- 场景中发现的Shader总数
- 每个Shader的使用次数(按使用频率排序)
- 使用该Shader的材质数量
- 示例材质名称(前5个)

**示例输出**:
```
╔═══════════════════════════════════════════════════════════════════╗
║                     场景Shader扫描报告                            ║
╚═══════════════════════════════════════════════════════════════════╝
[SoundBeacon] 找到 1523 个Renderer对象
─────────────────────────────────────────────────────────────────
[SoundBeacon] 共发现 8 种不同的Shader:
─────────────────────────────────────────────────────────────────

[1] 🎨 Universal Render Pipeline/Lit
     使用次数: 856
     使用的材质数: 124
     示例材质 (前5个):
       • Ground_Material
       • Tree_Bark
       • Rock_Surface
       • Building_Wall
       • Character_Skin

[2] 🎨 Universal Render Pipeline/Simple Lit
     使用次数: 342
     使用的材质数: 67
     示例材质 (前5个):
       • Prop_Material
       • Vegetation
       ...
```

---

### 2. 附近物体材质扫描 (按J键)

**作用**: 详细扫描玩家周围20米内的物体，获取每个物体的材质和Shader详细信息。

**使用步骤**:
1. 走到你想参考的物体附近
2. 按 **J** 键
3. 查看控制台日志

**输出内容**:
- 物体名称
- 距离玩家的距离
- 物体位置坐标
- 物体所在Layer
- 材质数量
- 每个材质的详细信息:
  - 材质名称
  - Shader名称
  - 主纹理名称
  - 颜色值(RGBA)

**示例输出**:
```
╔═══════════════════════════════════════════════════════════════════╗
║                   玩家附近物体材质扫描                            ║
╚═══════════════════════════════════════════════════════════════════╝
[SoundBeacon] 扫描位置: (15.3, 2.1, 28.7)
[SoundBeacon] 扫描半径: 20米
─────────────────────────────────────────────────────────────────
[SoundBeacon] 找到 45 个物体

[1] 📦 Tree_Pine_01
     距离: 3.2米
     位置: (12.5, 0.0, 26.8)
     Layer: Default
     材质数量: 2
     材质 [1]:
       名称: Tree_Bark_Material
       Shader: Universal Render Pipeline/Lit
       主纹理: Tree_Bark_Diffuse
       颜色: RGBA(0.85, 0.75, 0.65, 1.00)
     材质 [2]:
       名称: Tree_Leaves_Material
       Shader: Universal Render Pipeline/Lit
       主纹理: Leaves_Atlas
       颜色: RGBA(0.35, 0.65, 0.25, 1.00)
```

---

## 💡 使用建议

### 确定游戏使用的Shader

1. **进入游戏地图** (不要在大厅或菜单)
2. **按H键** 进行场景扫描
3. 查看排名第1的Shader，这通常是游戏的标准Shader
4. 常见的游戏Shader:
   - `Universal Render Pipeline/Lit` (URP标准Lit)
   - `Universal Render Pipeline/Simple Lit` (URP简化Lit)
   - `Standard` (内置渲染管线标准Shader)
   - `HDRP/Lit` (HDRP标准Shader)

### 参考具体物体

1. 走到游戏中你想模仿的物体附近
2. **按J键** 扫描附近物体
3. 找到目标物体，记录其Shader和材质设置
4. 在Unity中为你的3D模型使用相同的Shader

### 为你的3D模型选择Shader

根据扫描结果，在Unity中为你的模型材质选择相同的Shader:

1. **如果游戏使用URP**:
   - 在Unity中将项目设置为URP
   - 为材质选择 `Universal Render Pipeline/Lit`
   
2. **如果游戏使用标准渲染管线**:
   - 使用 `Standard` Shader
   
3. **如果游戏使用HDRP**:
   - 在Unity中将项目设置为HDRP
   - 使用 `HDRP/Lit`

---

## 🎨 实际工作流程示例

### 为SoundBeacon制作音乐信标3D模型

1. **启动游戏**，加载SoundBeacon Mod
2. **进入游戏地图** (不是大厅)
3. **按H键** 查看场景Shader统计
   ```
   发现: Universal Render Pipeline/Lit 使用最多
   ```
4. **按J键** 查看附近的信标物体或类似的发光物体
   ```
   发现某个发光道具使用:
   - Shader: Universal Render Pipeline/Lit
   - 自发光属性: _EmissionColor
   ```
5. **在Unity中创建你的3D模型**
6. **为模型材质选择** `Universal Render Pipeline/Lit`
7. **配置材质属性** (参考J键扫描的结果)
8. **导出AssetBundle**
9. **在Mod中加载使用**

---

## 📝 其他快捷键提醒

| 快捷键 | 功能 |
|--------|------|
| P | 在玩家位置生成音乐信标 |
| L | 打印玩家当前位置坐标 |
| K | 清除所有已生成的信标 |
| H | **扫描场景Shader** ⭐ |
| J | **扫描附近材质** ⭐ |

---

## ⚠️ 注意事项

1. **扫描时机**: 建议在游戏地图中扫描，而不是在大厅或加载场景
2. **性能影响**: H键扫描整个场景可能需要1-2秒，这是正常的
3. **日志输出**: J键扫描限制显示前15个物体，避免刷屏
4. **控制台查看**: 日志输出在Unity的Player.log文件中，路径通常在:
   - `C:\Users\[用户名]\AppData\LocalLow\TeamSoda\Duckov\Player.log`

---

## 🚀 下一步

1. 使用H和J键扫描游戏，确定正确的Shader
2. 在Unity中创建你的3D模型
3. 应用相同的Shader和材质设置
4. 将模型集成到Mod中

祝你制作顺利！🎵

