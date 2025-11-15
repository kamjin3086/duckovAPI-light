using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// Shader和材质扫描工具类
    /// 用于分析场景中使用的Shader，帮助Mod开发时选择合适的Shader
    /// </summary>
    public static class ShaderScanner
    {
        /// <summary>
        /// 扫描场景中所有使用的Shader
        /// </summary>
        /// <param name="logPrefix">日志前缀</param>
        /// <param name="maxMaterialSamples">每个Shader显示的最大材质示例数</param>
        public static Dictionary<string, ShaderInfo> ScanSceneShaders(string logPrefix = "[ShaderScanner]", int maxMaterialSamples = 5)
        {
            Debug.Log("╔═══════════════════════════════════════════════════════════════════╗");
            Debug.Log("║                     场景Shader扫描报告                            ║");
            Debug.Log("╚═══════════════════════════════════════════════════════════════════╝");

            Dictionary<string, ShaderInfo> shaderInfos = new Dictionary<string, ShaderInfo>();

            try
            {
                // 获取场景中所有的Renderer组件
                var allRenderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
                Debug.Log($"{logPrefix} 找到 {allRenderers.Length} 个Renderer对象");

                foreach (var renderer in allRenderers)
                {
                    if (renderer.sharedMaterials == null) continue;

                    foreach (var material in renderer.sharedMaterials)
                    {
                        if (material == null || material.shader == null) continue;

                        string shaderName = material.shader.name;
                        string materialName = material.name;

                        // 统计shader信息
                        if (!shaderInfos.ContainsKey(shaderName))
                        {
                            shaderInfos[shaderName] = new ShaderInfo
                            {
                                ShaderName = shaderName,
                                UsageCount = 0,
                                MaterialNames = new List<string>()
                            };
                        }

                        shaderInfos[shaderName].UsageCount++;

                        // 记录材质名称(去重)
                        if (!shaderInfos[shaderName].MaterialNames.Contains(materialName))
                        {
                            shaderInfos[shaderName].MaterialNames.Add(materialName);
                        }
                    }
                }

                // 按使用次数排序输出
                var sortedShaders = shaderInfos.Values.OrderByDescending(x => x.UsageCount);

                Debug.Log("─────────────────────────────────────────────────────────────────");
                Debug.Log($"{logPrefix} 共发现 {shaderInfos.Count} 种不同的Shader:");
                Debug.Log("─────────────────────────────────────────────────────────────────");

                int index = 1;
                foreach (var info in sortedShaders)
                {
                    Debug.Log($"\n[{index}] 🎨 {info.ShaderName}");
                    Debug.Log($"     使用次数: {info.UsageCount}");
                    Debug.Log($"     使用的材质数: {info.MaterialNames.Count}");
                    
                    // 显示前N个使用该shader的材质
                    int matCount = Math.Min(maxMaterialSamples, info.MaterialNames.Count);
                    Debug.Log($"     示例材质 (前{matCount}个):");
                    for (int i = 0; i < matCount; i++)
                    {
                        Debug.Log($"       • {info.MaterialNames[i]}");
                    }
                    if (info.MaterialNames.Count > maxMaterialSamples)
                    {
                        Debug.Log($"       ... 还有 {info.MaterialNames.Count - maxMaterialSamples} 个材质");
                    }

                    index++;
                }

                Debug.Log("\n═════════════════════════════════════════════════════════════════");
                Debug.Log($"{logPrefix} 💡 建议:");
                Debug.Log($"{logPrefix}    - 最常用的Shader通常是游戏的标准着色器");
                Debug.Log($"{logPrefix}    - Universal Render Pipeline/Lit 是常见的URP标准shader");
                Debug.Log($"{logPrefix}    - Standard 是Unity内置渲染管线的标准shader");
                Debug.Log("═════════════════════════════════════════════════════════════════");
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 扫描Shader时出错: {e.Message}");
                Debug.LogError($"{logPrefix} 堆栈: {e.StackTrace}");
            }

            return shaderInfos;
        }

        /// <summary>
        /// 扫描指定位置附近的物体材质
        /// </summary>
        /// <param name="centerPosition">中心位置</param>
        /// <param name="radius">扫描半径</param>
        /// <param name="maxObjects">最大显示物体数</param>
        /// <param name="logPrefix">日志前缀</param>
        public static List<ObjectMaterialInfo> ScanNearbyMaterials(Vector3 centerPosition, float radius = 20f, int maxObjects = 15, string logPrefix = "[ShaderScanner]")
        {
            Debug.Log("╔═══════════════════════════════════════════════════════════════════╗");
            Debug.Log("║                   附近物体材质扫描                                ║");
            Debug.Log("╚═══════════════════════════════════════════════════════════════════╝");

            List<ObjectMaterialInfo> results = new List<ObjectMaterialInfo>();

            try
            {
                Debug.Log($"{logPrefix} 扫描位置: ({centerPosition.x:F1}, {centerPosition.y:F1}, {centerPosition.z:F1})");
                Debug.Log($"{logPrefix} 扫描半径: {radius}米");
                Debug.Log("─────────────────────────────────────────────────────────────────");

                // 获取附近的所有Collider
                Collider[] nearbyObjects = Physics.OverlapSphere(centerPosition, radius);
                Debug.Log($"{logPrefix} 找到 {nearbyObjects.Length} 个物体\n");

                int objectIndex = 1;
                foreach (var collider in nearbyObjects)
                {
                    if (collider == null) continue;

                    GameObject obj = collider.gameObject;
                    float distance = Vector3.Distance(centerPosition, obj.transform.position);

                    // 获取Renderer组件
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer == null)
                    {
                        renderer = obj.GetComponentInChildren<Renderer>();
                    }

                    if (renderer != null && renderer.sharedMaterials != null && renderer.sharedMaterials.Length > 0)
                    {
                        ObjectMaterialInfo info = new ObjectMaterialInfo
                        {
                            ObjectName = obj.name,
                            Distance = distance,
                            Position = obj.transform.position,
                            Layer = LayerMask.LayerToName(obj.layer),
                            Materials = new List<MaterialInfo>()
                        };

                        Debug.Log($"[{objectIndex}] 📦 {obj.name}");
                        Debug.Log($"     距离: {distance:F1}米");
                        Debug.Log($"     位置: ({obj.transform.position.x:F1}, {obj.transform.position.y:F1}, {obj.transform.position.z:F1})");
                        Debug.Log($"     Layer: {info.Layer}");
                        Debug.Log($"     材质数量: {renderer.sharedMaterials.Length}");

                        for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                        {
                            var material = renderer.sharedMaterials[i];
                            if (material != null && material.shader != null)
                            {
                                MaterialInfo matInfo = new MaterialInfo
                                {
                                    MaterialName = material.name,
                                    ShaderName = material.shader.name
                                };

                                // 尝试获取主纹理
                                if (material.HasProperty("_MainTex"))
                                {
                                    var mainTex = material.GetTexture("_MainTex");
                                    if (mainTex != null)
                                    {
                                        matInfo.MainTextureName = mainTex.name;
                                    }
                                }

                                // 尝试获取颜色
                                if (material.HasProperty("_Color"))
                                {
                                    matInfo.Color = material.GetColor("_Color");
                                }

                                info.Materials.Add(matInfo);

                                Debug.Log($"     材质 [{i + 1}]:");
                                Debug.Log($"       名称: {matInfo.MaterialName}");
                                Debug.Log($"       Shader: {matInfo.ShaderName}");
                                
                                if (!string.IsNullOrEmpty(matInfo.MainTextureName))
                                {
                                    Debug.Log($"       主纹理: {matInfo.MainTextureName}");
                                }

                                if (matInfo.Color.HasValue)
                                {
                                    Color c = matInfo.Color.Value;
                                    Debug.Log($"       颜色: RGBA({c.r:F2}, {c.g:F2}, {c.b:F2}, {c.a:F2})");
                                }
                            }
                        }
                        Debug.Log("");

                        results.Add(info);
                        objectIndex++;

                        // 限制输出数量
                        if (objectIndex > maxObjects)
                        {
                            Debug.Log($"{logPrefix} ... 还有更多物体，已显示前{maxObjects}个");
                            break;
                        }
                    }
                }

                Debug.Log("═════════════════════════════════════════════════════════════════");
                Debug.Log($"{logPrefix} ✓ 扫描完成，共分析了 {results.Count} 个有材质的物体");
                Debug.Log("═════════════════════════════════════════════════════════════════");
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 扫描附近材质时出错: {e.Message}");
                Debug.LogError($"{logPrefix} 堆栈: {e.StackTrace}");
            }

            return results;
        }

        /// <summary>
        /// Shader信息类
        /// </summary>
        public class ShaderInfo
        {
            public string ShaderName { get; set; }
            public int UsageCount { get; set; }
            public List<string> MaterialNames { get; set; }
        }

        /// <summary>
        /// 物体材质信息类
        /// </summary>
        public class ObjectMaterialInfo
        {
            public string ObjectName { get; set; }
            public float Distance { get; set; }
            public Vector3 Position { get; set; }
            public string Layer { get; set; }
            public List<MaterialInfo> Materials { get; set; }
        }

        /// <summary>
        /// 材质信息类
        /// </summary>
        public class MaterialInfo
        {
            public string MaterialName { get; set; }
            public string ShaderName { get; set; }
            public string MainTextureName { get; set; }
            public Color? Color { get; set; }
        }
    }
}

