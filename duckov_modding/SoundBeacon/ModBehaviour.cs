using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Duckov;
using Duckov.Scenes;
using FMODUnity;

// 使用别名解决 Debug 类名冲突
using Debug = UnityEngine.Debug;

namespace SoundBeacon
{
    /// <summary>
    /// SoundBeacon Mod 主类
    /// 在游戏地图中生成发出声音的信标
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // ============ 生成模式选择 ============
        [Header("生成模式")]
        public SpawnMode spawnMode = SpawnMode.FixedPositions;  // 默认使用固定位置,方便调试

        // ============ 固定位置配置 ============
        [Header("固定位置列表(用于调试)")]
        public Vector3[] fixedPositions = new Vector3[]
        {
            new Vector3(345.6f, 0.0f, 116.8f),    // 位置1: 可自定义
            new Vector3(-50f, 0f, 50f),     // 位置2: 可自定义
            new Vector3(0f, 0f, -80f),      // 位置3: 可自定义
        };

        // ============ 相对玩家位置配置 ============
        [Header("相对玩家位置生成")]
        public int beaconCount = 3;           // 生成信标数量
        public float spawnRadius = 100f;      // 生成半径

        // ============ 共同配置 ============
        [Header("共同配置")]
        public float spawnHeight = 1f;        // 生成高度偏移
        public bool useGroundDetection = true; // 是否检测地面高度

        [Header("音频配置")]
        public float minInterval = 3f;         // 最小播放间隔(秒)
        public float maxInterval = 10f;        // 最大播放间隔(秒)
        public float minDistance = 5f;         // 最小听到距离
        public float maxDistance = 50f;        // 最大听到距离
        public float volume = 0.8f;            // 音量

        [Header("调试功能")]
        public bool printPlayerPosition = true;  // 是否打印玩家位置
        public KeyCode printPositionKey = KeyCode.P;  // 按P键打印玩家位置
        public KeyCode spawnBeaconKey = KeyCode.LeftBracket;  // 按[键在当前位置生成信标
        public KeyCode clearBeaconsKey = KeyCode.RightBracket;  // 按]键清除所有信标

        // 内部变量
        private List<GameObject> spawnedBeacons = new List<GameObject>();
        private FMOD.Sound fmodSound;
        private bool soundLoaded = false;
        private bool isInitialized = false;
        private bool levelInitialized = false;

        /// <summary>
        /// 生成模式枚举
        /// </summary>
        public enum SpawnMode
        {
            FixedPositions,      // 固定位置(调试用)
            RelativeToPlayer,    // 相对玩家位置
        }

        void Awake()
        {
            Debug.Log("=".PadRight(80, '='));
            Debug.Log("[SoundBeacon] Mod 正在初始化...");
            Debug.Log($"[SoundBeacon] 当前生成模式: {spawnMode}");
            Debug.Log("=".PadRight(80, '='));

            // 订阅多个事件来追踪场景加载流程
            LevelManager.OnLevelBeginInitializing += OnLevelBeginInitializing;
            LevelManager.OnLevelInitialized += OnLevelInitialized;
            LevelManager.OnAfterLevelInitialized += OnAfterLevelInitialized;

            // 订阅场景加载事件
            SceneLoader.onStartedLoadingScene += OnSceneLoadingStarted;
            SceneLoader.onFinishedLoadingScene += OnSceneLoadingFinished;
            SceneLoader.onAfterSceneInitialize += OnAfterSceneInitialize;

            // 订阅子场景事件
            MultiSceneCore.OnSubSceneLoaded += OnSubSceneLoaded;
        }

        void Start()
        {
            try
            {
                Debug.Log("[SoundBeacon] 开始加载音频资源...");
                LoadAudioWithFMOD();

                if (!soundLoaded)
                {
                    Debug.LogWarning("[SoundBeacon] 音频文件未加载,信标将不发出声音");
                }

                isInitialized = true;
                Debug.Log("[SoundBeacon] ✓ Mod 初始化完成");
                Debug.Log("[SoundBeacon] 等待进入游戏世界后生成信标...");

                if (printPlayerPosition)
                {
                    Debug.Log($"[SoundBeacon] 快捷键说明:");
                    Debug.Log($"[SoundBeacon]   {printPositionKey} - 打印玩家位置");
                    Debug.Log($"[SoundBeacon]   {spawnBeaconKey} - 在当前位置生成信标");
                    Debug.Log($"[SoundBeacon]   {clearBeaconsKey} - 清除所有信标");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] ❌ 初始化失败: {e.Message}");
                Debug.LogException(e);
            }
        }

        void Update()
        {
            // 调试: 打印玩家位置
            if (printPlayerPosition && Input.GetKeyDown(printPositionKey))
            {
                PrintPlayerPosition();
            }

            // 快捷键: 在当前位置生成信标
            if (Input.GetKeyDown(spawnBeaconKey))
            {
                SpawnBeaconAtPlayer();
            }

            // 快捷键: 清除所有信标
            if (Input.GetKeyDown(clearBeaconsKey))
            {
                ClearAllBeacons();
            }
        }

        /// <summary>
        /// 关卡开始初始化时调用
        /// </summary>
        private void OnLevelBeginInitializing()
        {
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("[SoundBeacon] 📍 关卡开始初始化...");
            LogCurrentSceneInfo("OnLevelBeginInitializing");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// 关卡初始化完成时调用
        /// </summary>
        private void OnLevelInitialized()
        {
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("[SoundBeacon] 📍 关卡已初始化");
            LogCurrentSceneInfo("OnLevelInitialized");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            levelInitialized = true;

            // 检查是否应该在此场景生成信标
            if (ShouldSpawnInCurrentScene())
            {
                // 自动生成信标
                if (isInitialized && spawnedBeacons.Count == 0)
                {
                    SpawnBeacons();
                }
            }
            else
            {
                Debug.Log("[SoundBeacon] ⚠️ 当前场景不适合生成信标,跳过");
            }
        }

        /// <summary>
        /// 关卡初始化后调用
        /// </summary>
        private void OnAfterLevelInitialized()
        {
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("[SoundBeacon] 📍 关卡初始化完成后");
            LogCurrentSceneInfo("OnAfterLevelInitialized");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// 场景开始加载时调用
        /// </summary>
        private void OnSceneLoadingStarted(SceneLoadingContext context)
        {
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("[SoundBeacon] 📍 场景开始加载");
            Debug.Log($"[SoundBeacon]    场景名称: {context.sceneName}");
            Debug.Log($"[SoundBeacon]    使用位置: {context.useLocation}");
            if (context.useLocation)
            {
                Debug.Log($"[SoundBeacon]    位置信息: {context.location}");
            }
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// 场景加载完成时调用
        /// </summary>
        private void OnSceneLoadingFinished(SceneLoadingContext context)
        {
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("[SoundBeacon] 📍 场景加载完成");
            Debug.Log($"[SoundBeacon]    场景名称: {context.sceneName}");
            LogCurrentSceneInfo("OnSceneLoadingFinished");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// 场景初始化后调用
        /// </summary>
        private void OnAfterSceneInitialize(SceneLoadingContext context)
        {
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("[SoundBeacon] 📍 场景初始化完成");
            Debug.Log($"[SoundBeacon]    场景名称: {context.sceneName}");
            LogCurrentSceneInfo("OnAfterSceneInitialize");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// 子场景加载完成时调用
        /// </summary>
        private void OnSubSceneLoaded(MultiSceneCore core, UnityEngine.SceneManagement.Scene scene)
        {
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log("[SoundBeacon] 📍 子场景已加载");
            Debug.Log($"[SoundBeacon]    子场景名称: {scene.name}");
            Debug.Log($"[SoundBeacon]    子场景路径: {scene.path}");
            Debug.Log($"[SoundBeacon]    BuildIndex: {scene.buildIndex}");
            LogCurrentSceneInfo("OnSubSceneLoaded");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>
        /// 打印当前场景详细信息
        /// </summary>
        private void LogCurrentSceneInfo(string eventName)
        {
            try
            {
                Debug.Log($"[SoundBeacon] === 当前场景信息 ({eventName}) ===");

                // 主场景信息
                var mainScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                Debug.Log($"[SoundBeacon] 主场景: {mainScene.name} (路径: {mainScene.path})");

                // LevelManager信息
                if (LevelManager.Instance != null)
                {
                    var levelInfo = LevelManager.GetCurrentLevelInfo();
                    Debug.Log($"[SoundBeacon] LevelInfo:");
                    Debug.Log($"[SoundBeacon]    - 是否基础关卡: {levelInfo.isBaseLevel}");
                    Debug.Log($"[SoundBeacon]    - 场景名称: {levelInfo.sceneName}");
                    Debug.Log($"[SoundBeacon]    - 活动子场景ID: {levelInfo.activeSubSceneID}");
                }
                else
                {
                    Debug.Log($"[SoundBeacon] LevelManager.Instance == null");
                }

                // MultiSceneCore信息
                if (MultiSceneCore.Instance != null)
                {
                    Debug.Log($"[SoundBeacon] MultiSceneCore:");
                    Debug.Log($"[SoundBeacon]    - DisplayName: {MultiSceneCore.Instance.DisplayName}");
                    Debug.Log($"[SoundBeacon]    - DisplayNameRaw: {MultiSceneCore.Instance.DisplaynameRaw}");

                    var mainSceneInfo = MultiSceneCore.MainScene;
                    if (mainSceneInfo.HasValue)
                    {
                        Debug.Log($"[SoundBeacon]    - 主场景: {mainSceneInfo.Value.name}");
                    }

                    var activeSubScene = MultiSceneCore.ActiveSubScene;
                    if (activeSubScene.HasValue)
                    {
                        Debug.Log($"[SoundBeacon]    - 活动子场景: {activeSubScene.Value.name}");
                    }

                    string activeSubSceneID = MultiSceneCore.ActiveSubSceneID;
                    if (!string.IsNullOrEmpty(activeSubSceneID))
                    {
                        Debug.Log($"[SoundBeacon]    - 活动子场景ID: {activeSubSceneID}");
                    }
                }
                else
                {
                    Debug.Log($"[SoundBeacon] MultiSceneCore.Instance == null");
                }

                // 玩家信息
                if (CharacterMainControl.Main != null)
                {
                    Debug.Log($"[SoundBeacon] 玩家存在: {CharacterMainControl.Main.transform.position}");
                }
                else
                {
                    Debug.Log($"[SoundBeacon] CharacterMainControl.Main == null");
                }

                Debug.Log($"[SoundBeacon] ================================");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] 打印场景信息时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 判断是否应该在当前场景生成信标
        /// </summary>
        private bool ShouldSpawnInCurrentScene()
        {
            try
            {
                // 必须有LevelManager
                if (LevelManager.Instance == null)
                {
                    Debug.Log("[SoundBeacon] ❌ LevelManager不存在");
                    return false;
                }

                var levelInfo = LevelManager.GetCurrentLevelInfo();

                // 检查场景名称
                string sceneName = levelInfo.sceneName?.ToLower() ?? "";
                Debug.Log($"[SoundBeacon] 🔍 检查场景: {sceneName}");

                // 排除不应该生成信标的场景
                string[] excludedScenes = new string[]
                {
                    "lobby",        // 大厅
                    "menu",         // 菜单
                    "mainmenu",     // 主菜单
                    "sewer",        // 下水道
                    "sewers",       // 下水道(复数)
                    "tutorial",     // 教程
                    "loading",      // 加载场景
                    "intro",        // 介绍
                    "base",         // 基地
                };

                foreach (string excluded in excludedScenes)
                {
                    if (sceneName.Contains(excluded))
                    {
                        Debug.Log($"[SoundBeacon] ❌ 场景包含排除关键词: {excluded}");
                        return false;
                    }
                }

                // 如果是基础关卡(base level),可能是大厅/基地,跳过
                if (levelInfo.isBaseLevel)
                {
                    Debug.Log($"[SoundBeacon] ❌ 这是基础关卡(可能是大厅/基地)");
                    return false;
                }

                // 必须有活动的子场景
                if (string.IsNullOrEmpty(levelInfo.activeSubSceneID))
                {
                    Debug.Log($"[SoundBeacon] ❌ 没有活动子场景");
                    return false;
                }

                Debug.Log($"[SoundBeacon] ✅ 当前场景适合生成信标!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] 检查场景时出错: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 在玩家当前位置生成一个信标(快捷键功能)
        /// </summary>
        private void SpawnBeaconAtPlayer()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("[SoundBeacon] Mod未初始化完成");
                return;
            }

            if (CharacterMainControl.Main == null)
            {
                Debug.LogWarning("[SoundBeacon] 玩家角色未找到");
                return;
            }

            Vector3 playerPos = CharacterMainControl.Main.transform.position;
            Vector3 spawnPos = playerPos + CharacterMainControl.Main.transform.forward * 3f; // 玩家前方3米

            if (useGroundDetection)
            {
                float groundY = GetGroundHeight(spawnPos);
                spawnPos.y = groundY + spawnHeight;
            }

            int index = spawnedBeacons.Count;
            GameObject? beacon = SpawnBeacon(spawnPos, index);

            if (beacon != null)
            {
                spawnedBeacons.Add(beacon);
                Debug.Log($"[SoundBeacon] ✓ 手动生成信标成功!");
                Debug.Log($"[SoundBeacon]    位置: ({spawnPos.x:F1}, {spawnPos.y:F1}, {spawnPos.z:F1})");
                Debug.Log($"[SoundBeacon]    当前总数: {spawnedBeacons.Count}");
            }
        }

        /// <summary>
        /// 打印玩家当前位置(用于记录坐标)
        /// </summary>
        private void PrintPlayerPosition()
        {
            if (CharacterMainControl.Main == null)
            {
                Debug.Log("[SoundBeacon] ⚠ 玩家角色未找到");
                return;
            }

            Vector3 pos = CharacterMainControl.Main.transform.position;
            Debug.Log("=".PadRight(60, '='));
            Debug.Log($"[SoundBeacon] 📍 玩家当前位置:");
            Debug.Log($"[SoundBeacon]    X: {pos.x:F2}");
            Debug.Log($"[SoundBeacon]    Y: {pos.y:F2}");
            Debug.Log($"[SoundBeacon]    Z: {pos.z:F2}");
            Debug.Log($"[SoundBeacon] 复制代码: new Vector3({pos.x:F1}f, {pos.y:F1}f, {pos.z:F1}f)");
            Debug.Log("=".PadRight(60, '='));
        }

        /// <summary>
        /// 扫描场景中所有使用的Shader (按H键触发)
        /// </summary>
        private void ScanSceneShaders()
        {
            Debug.Log("╔═══════════════════════════════════════════════════════════════════╗");
            Debug.Log("║                     场景Shader扫描报告                            ║");
            Debug.Log("╚═══════════════════════════════════════════════════════════════════╝");

            try
            {
                // 获取场景中所有的Renderer组件
                var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
                Debug.Log($"[SoundBeacon] 找到 {allRenderers.Length} 个Renderer对象");

                // 统计Shader使用情况
                Dictionary<string, int> shaderCount = new Dictionary<string, int>();
                Dictionary<string, List<string>> shaderToMaterials = new Dictionary<string, List<string>>();

                foreach (var renderer in allRenderers)
                {
                    if (renderer.sharedMaterials == null) continue;

                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material == null || material.shader == null) continue;

                        string shaderName = material.shader.name;
                        string materialName = material.name;

                        // 统计shader数量
                        if (!shaderCount.ContainsKey(shaderName))
                        {
                            shaderCount[shaderName] = 0;
                            shaderToMaterials[shaderName] = new List<string>();
                        }
                        shaderCount[shaderName]++;

                        // 记录材质名称(去重)
                        if (!shaderToMaterials[shaderName].Contains(materialName))
                        {
                            shaderToMaterials[shaderName].Add(materialName);
                        }
                    }
                }

                // 按使用次数排序输出
                var sortedShaders = shaderCount.OrderByDescending(x => x.Value);

                Debug.Log("─────────────────────────────────────────────────────────────────");
                Debug.Log($"[SoundBeacon] 共发现 {shaderCount.Count} 种不同的Shader:");
                Debug.Log("─────────────────────────────────────────────────────────────────");

                int index = 1;
                foreach (var kvp in sortedShaders)
                {
                    string shaderName = kvp.Key;
                    int count = kvp.Value;

                    Debug.Log($"\n[{index}] 🎨 {shaderName}");
                    Debug.Log($"     使用次数: {count}");
                    Debug.Log($"     使用的材质数: {shaderToMaterials[shaderName].Count}");

                    // 显示前5个使用该shader的材质
                    int matCount = Math.Min(5, shaderToMaterials[shaderName].Count);
                    Debug.Log($"     示例材质 (前{matCount}个):");
                    for (int i = 0; i < matCount; i++)
                    {
                        Debug.Log($"       • {shaderToMaterials[shaderName][i]}");
                    }
                    if (shaderToMaterials[shaderName].Count > 5)
                    {
                        Debug.Log($"       ... 还有 {shaderToMaterials[shaderName].Count - 5} 个材质");
                    }

                    index++;
                }

                Debug.Log("\n═════════════════════════════════════════════════════════════════");
                Debug.Log("[SoundBeacon] 💡 建议:");
                Debug.Log("[SoundBeacon]    - 最常用的Shader通常是游戏的标准着色器");
                Debug.Log("[SoundBeacon]    - 使用 J 键扫描玩家附近的物体获取更详细信息");
                Debug.Log("[SoundBeacon]    - Universal Render Pipeline/Lit 是常见的URP标准shader");
                Debug.Log("═════════════════════════════════════════════════════════════════");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] 扫描Shader时出错: {e.Message}");
                Debug.LogError($"[SoundBeacon] 堆栈: {e.StackTrace}");
            }
        }

        /// <summary>
        /// 扫描玩家附近的物体材质 (按J键触发)
        /// </summary>
        private void ScanNearbyMaterials()
        {
            if (CharacterMainControl.Main == null)
            {
                Debug.LogWarning("[SoundBeacon] 玩家角色未找到");
                return;
            }

            Debug.Log("╔═══════════════════════════════════════════════════════════════════╗");
            Debug.Log("║                   玩家附近物体材质扫描                            ║");
            Debug.Log("╚═══════════════════════════════════════════════════════════════════╝");

            try
            {
                Vector3 playerPos = CharacterMainControl.Main.transform.position;
                float scanRadius = 20f; // 扫描半径20米

                Debug.Log($"[SoundBeacon] 扫描位置: ({playerPos.x:F1}, {playerPos.y:F1}, {playerPos.z:F1})");
                Debug.Log($"[SoundBeacon] 扫描半径: {scanRadius}米");
                Debug.Log("─────────────────────────────────────────────────────────────────");

                // 获取附近的所有Collider
                Collider[] nearbyObjects = Physics.OverlapSphere(playerPos, scanRadius);
                Debug.Log($"[SoundBeacon] 找到 {nearbyObjects.Length} 个物体\n");

                int objectIndex = 1;
                foreach (var collider in nearbyObjects)
                {
                    if (collider == null) continue;

                    GameObject obj = collider.gameObject;
                    float distance = Vector3.Distance(playerPos, obj.transform.position);

                    // 获取Renderer组件
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        renderer = obj.GetComponentInChildren<Renderer>();
                    }

                    if (renderer != null && renderer.sharedMaterials != null)
                    {
                        Debug.Log($"[{objectIndex}] 📦 {obj.name}");
                        Debug.Log($"     距离: {distance:F1}米");
                        Debug.Log($"     位置: ({obj.transform.position.x:F1}, {obj.transform.position.y:F1}, {obj.transform.position.z:F1})");
                        Debug.Log($"     Layer: {LayerMask.LayerToName(obj.layer)}");
                        Debug.Log($"     材质数量: {renderer.sharedMaterials.Length}");

                        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        {
                            var material = renderer.sharedMaterials[i];
                            if (material != null)
                            {
                                Debug.Log($"     材质 [{i + 1}]:");
                                Debug.Log($"       名称: {material.name}");
                                Debug.Log($"       Shader: {material.shader.name}");

                                // 尝试获取主纹理
                                if (material.HasProperty("_MainTex"))
                                {
                                    var mainTex = material.GetTexture("_MainTex");
                                    if (mainTex != null)
                                    {
                                        Debug.Log($"       主纹理: {mainTex.name}");
                                    }
                                }

                                // 尝试获取颜色
                                if (material.HasProperty("_Color"))
                                {
                                    Color color = material.GetColor("_Color");
                                    Debug.Log($"       颜色: RGBA({color.r:F2}, {color.g:F2}, {color.b:F2}, {color.a:F2})");
                                }
                            }
                        }
                        Debug.Log("");
                        objectIndex++;

                        // 限制输出数量，避免刷屏
                        if (objectIndex > 15)
                        {
                            Debug.Log($"[SoundBeacon] ... 还有更多物体，已显示前15个");
                            break;
                        }
                    }
                }

                Debug.Log("═════════════════════════════════════════════════════════════════");
                Debug.Log($"[SoundBeacon] ✓ 扫描完成，共分析了 {Math.Min(objectIndex - 1, 15)} 个有材质的物体");
                Debug.Log("═════════════════════════════════════════════════════════════════");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] 扫描附近材质时出错: {e.Message}");
                Debug.LogError($"[SoundBeacon] 堆栈: {e.StackTrace}");
            }
        }

        /// <summary>
        /// 判断是否应该生成信标
        /// </summary>
        private bool ShouldSpawnBeacons()
        {
            // 必须等待关卡初始化完成
            if (!levelInitialized)
            {
                return false;
            }

            // 检查是否在游戏世界中(有玩家角色)
            if (CharacterMainControl.Main == null)
            {
                return false;
            }

            // 检查LevelManager是否存在
            if (LevelManager.Instance == null)
            {
                return false;
            }

            // 检查MultiSceneCore是否存在
            if (MultiSceneCore.Instance == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 生成信标
        /// </summary>
        private void SpawnBeacons()
        {
            Debug.Log($"[SoundBeacon] 开始生成信标...");
            Debug.Log($"[SoundBeacon] 生成模式: {spawnMode}");

            List<Vector3> positions = GetSpawnPositions();

            Debug.Log($"[SoundBeacon] 将生成 {positions.Count} 个信标");

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 spawnPosition = positions[i];

                // 应用地面检测
                if (useGroundDetection)
                {
                    float groundY = GetGroundHeight(spawnPosition);
                    spawnPosition.y = groundY + spawnHeight;
                }
                else
                {
                    spawnPosition.y += spawnHeight;
                }

                GameObject? beacon = SpawnBeacon(spawnPosition, i);

                if (beacon != null)
                {
                    spawnedBeacons.Add(beacon);
                    Debug.Log($"[SoundBeacon] ✓ 信标 {i + 1}/{positions.Count} 已生成");
                    Debug.Log($"[SoundBeacon]    位置: ({spawnPosition.x:F1}, {spawnPosition.y:F1}, {spawnPosition.z:F1})");
                }
            }

            Debug.Log($"[SoundBeacon] ✓✓✓ 共生成 {spawnedBeacons.Count} 个信标 ✓✓✓");

            // 打印玩家位置作为参考
            if (CharacterMainControl.Main != null)
            {
                Vector3 playerPos = CharacterMainControl.Main.transform.position;
                Debug.Log($"[SoundBeacon] 📍 玩家位置: ({playerPos.x:F1}, {playerPos.y:F1}, {playerPos.z:F1})");
            }
        }

        /// <summary>
        /// 根据模式获取生成位置列表
        /// </summary>
        private List<Vector3> GetSpawnPositions()
        {
            List<Vector3> positions = new List<Vector3>();

            switch (spawnMode)
            {
                case SpawnMode.FixedPositions:
                    // 固定位置模式
                    foreach (Vector3 pos in fixedPositions)
                    {
                        positions.Add(pos);
                    }
                    Debug.Log($"[SoundBeacon] 使用固定位置模式,共 {positions.Count} 个位置");
                    break;

                case SpawnMode.RelativeToPlayer:
                    // 相对玩家位置模式
                    Vector3 center = Vector3.zero;
                    if (CharacterMainControl.Main != null)
                    {
                        center = CharacterMainControl.Main.transform.position;
                    }

                    for (int i = 0; i < beaconCount; i++)
                    {
                        positions.Add(GetRandomPositionAroundCenter(center));
                    }
                    Debug.Log($"[SoundBeacon] 使用相对玩家位置模式,中心: ({center.x:F1}, {center.z:F1})");
                    break;
            }

            return positions;
        }

        /// <summary>
        /// 获取中心点周围的随机位置
        /// </summary>
        private Vector3 GetRandomPositionAroundCenter(Vector3 center)
        {
            // 在水平圆形范围内随机生成
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = UnityEngine.Random.Range(spawnRadius * 0.3f, spawnRadius);

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * distance,
                0f,
                Mathf.Sin(angle) * distance
            );

            return center + offset;
        }

        /// <summary>
        /// 获取地面高度(使用射线检测)
        /// </summary>
        private float GetGroundHeight(Vector3 position)
        {
            RaycastHit hit;
            Vector3 rayStart = position + Vector3.up * 100f;

            if (Physics.Raycast(rayStart, Vector3.down, out hit, 200f))
            {
                return hit.point.y;
            }

            // 如果没有检测到地面,返回当前高度
            return position.y;
        }

        /// <summary>
        /// 生成单个信标
        /// </summary>
        private GameObject? SpawnBeacon(Vector3 position, int index)
        {
            try
            {
                GameObject beaconObj = new GameObject($"SoundBeacon_{index}");
                beaconObj.transform.position = position;

                // 添加信标组件
                SoundBeaconObject beacon = beaconObj.AddComponent<SoundBeaconObject>();

                // 初始化信标(传入音频和参数)
                if (soundLoaded)
                {
                    beacon.Initialize(
                        fmodSound,
                        minInterval,
                        maxInterval,
                        minDistance,
                        maxDistance,
                        volume
                    );
                }
                else
                {
                    Debug.LogWarning("[SoundBeacon] 音频未加载,信标将不发声");
                }

                // ⚠️ 关键: 将物体移动到主场景,确保持久化
                if (MultiSceneCore.Instance != null)
                {
                    MultiSceneCore.MoveToMainScene(beaconObj);
                    Debug.Log($"[SoundBeacon] 已将信标移动到主场景");
                }
                else
                {
                    Debug.LogWarning("[SoundBeacon] MultiSceneCore不存在,信标可能会被销毁");
                }

                return beaconObj;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] 生成信标失败: {e.Message}");
                Debug.LogException(e);
                return null;
            }
        }

        /// <summary>
        /// 使用FMOD加载音频文件
        /// </summary>
        private void LoadAudioWithFMOD()
        {
            try
            {
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log("[SoundBeacon] 🔊 使用FMOD加载音频资源...");
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                // 获取Mod路径
                string modPath = GetModPath();
                Debug.Log($"[SoundBeacon] 📁 Mod根路径: {modPath}");

                // 列出Mod文件夹中的所有文件
                if (System.IO.Directory.Exists(modPath))
                {
                    Debug.Log("[SoundBeacon] 📂 Mod文件夹内容:");
                    string[] files = System.IO.Directory.GetFiles(modPath);
                    foreach (string file in files)
                    {
                        string fileName = System.IO.Path.GetFileName(file);
                        long fileSize = new System.IO.FileInfo(file).Length;
                        Debug.Log($"[SoundBeacon]    - {fileName} ({fileSize} bytes)");
                    }
                }
                else
                {
                    Debug.LogError($"[SoundBeacon] ❌ Mod文件夹不存在: {modPath}");
                    return;
                }

                // 尝试多种音频文件名 (FMOD支持WAV, OGG, MP3等多种格式)
                string[] possibleNames = new string[]
                {
                    "beacon_sound.wav",   // WAV - 推荐,无损
                    "beacon_sound.ogg",   // OGG - 压缩但质量好
                    "beacon_sound.mp3",   // MP3 - 最常见
                    "sound.wav",
                    "sound.ogg",
                    "sound.mp3",
                    "test_short.wav",     // 测试文件
                };

                string? foundAudioPath = null;
                foreach (string name in possibleNames)
                {
                    string testPath = System.IO.Path.Combine(modPath, name);
                    if (System.IO.File.Exists(testPath))
                    {
                        foundAudioPath = testPath;
                        Debug.Log($"[SoundBeacon] ✓ 找到音频文件: {name}");
                        break;
                    }
                }

                if (foundAudioPath == null)
                {
                    Debug.LogWarning("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    Debug.LogWarning("[SoundBeacon] ⚠️ 未找到音频文件!");
                    Debug.LogWarning($"[SoundBeacon] 请将音频文件命名为以下任一名称:");
                    foreach (string name in possibleNames)
                    {
                        Debug.LogWarning($"[SoundBeacon]    - {name}");
                    }
                    Debug.LogWarning($"[SoundBeacon] 并放入文件夹: {modPath}");
                    Debug.LogWarning("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                    return;
                }

                long audioFileSize = new FileInfo(foundAudioPath).Length;
                Debug.Log($"[SoundBeacon] 📊 音频文件大小: {audioFileSize} bytes ({audioFileSize / 1024.0:F2} KB)");

                // 获取FMOD Studio System
                FMOD.Studio.System studioSystem = FMODUnity.RuntimeManager.StudioSystem;
                FMOD.System coreSystem;

                FMOD.RESULT result = studioSystem.getCoreSystem(out coreSystem);
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"[SoundBeacon] ❌ 无法获取FMOD Core System: {result}");
                    return;
                }

                Debug.Log("[SoundBeacon] ✓ FMOD System已获取");

                // 创建音频文件
                FMOD.MODE mode = FMOD.MODE.DEFAULT | FMOD.MODE._3D | FMOD.MODE.LOOP_OFF;

                result = coreSystem.createSound(foundAudioPath, mode, out fmodSound);

                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"[SoundBeacon] ❌ FMOD加载音频失败: {result}");
                    return;
                }

                // 获取音频信息
                uint length = 0;
                result = fmodSound.getLength(out length, FMOD.TIMEUNIT.MS);

                FMOD.SOUND_TYPE soundType;
                FMOD.SOUND_FORMAT soundFormat;
                int channels = 0;
                int bits = 0;

                fmodSound.getFormat(out soundType, out soundFormat, out channels, out bits);

                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log("[SoundBeacon] 🎉 FMOD加载成功!");
                Debug.Log($"[SoundBeacon] 📊 音频信息:");
                Debug.Log($"[SoundBeacon]    - 长度: {length / 1000.0:F2}秒");
                Debug.Log($"[SoundBeacon]    - 类型: {soundType}");
                Debug.Log($"[SoundBeacon]    - 格式: {soundFormat}");
                Debug.Log($"[SoundBeacon]    - 声道: {channels}");
                Debug.Log($"[SoundBeacon]    - 位深: {bits}bit");
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

                soundLoaded = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] ❌ FMOD加载音频异常: {e.Message}");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 获取Mod文件夹路径
        /// </summary>
        private string GetModPath()
        {
            // 尝试获取当前Mod的路径
            string dataPath = Application.dataPath;
            // 使用Path.Combine确保路径分隔符正确
            string modPath = System.IO.Path.Combine(dataPath, "Mods", "SoundBeacon");

            // 规范化路径,统一使用正确的分隔符
            modPath = System.IO.Path.GetFullPath(modPath);

            Debug.Log($"[SoundBeacon] Mod路径: {modPath}");
            return modPath;
        }

        /// <summary>
        /// 清理所有生成的信标
        /// </summary>
        public void ClearAllBeacons()
        {
            Debug.Log("[SoundBeacon] 清理所有信标...");

            foreach (GameObject beacon in spawnedBeacons)
            {
                if (beacon != null)
                {
                    Destroy(beacon);
                }
            }

            spawnedBeacons.Clear();
            Debug.Log("[SoundBeacon] ✓ 所有信标已清理");
        }

        void OnDestroy()
        {
            Debug.Log("[SoundBeacon] Mod 正在卸载...");

            // 取消订阅所有事件
            LevelManager.OnLevelBeginInitializing -= OnLevelBeginInitializing;
            LevelManager.OnLevelInitialized -= OnLevelInitialized;
            LevelManager.OnAfterLevelInitialized -= OnAfterLevelInitialized;

            SceneLoader.onStartedLoadingScene -= OnSceneLoadingStarted;
            SceneLoader.onFinishedLoadingScene -= OnSceneLoadingFinished;
            SceneLoader.onAfterSceneInitialize -= OnAfterSceneInitialize;

            MultiSceneCore.OnSubSceneLoaded -= OnSubSceneLoaded;

            // 清理FMOD Sound
            if (soundLoaded && fmodSound.hasHandle())
            {
                fmodSound.release();
                Debug.Log("[SoundBeacon] FMOD Sound已释放");
            }

            ClearAllBeacons();
            Debug.Log("[SoundBeacon] Mod 已卸载");
        }
    }
}
