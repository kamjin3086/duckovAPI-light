using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// Harmony补丁相关的通用工具类
    /// </summary>
    public static class HarmonyUtils
    {
        /// <summary>
        /// 初始化并应用Harmony补丁
        /// </summary>
        /// <param name="harmonyId">Harmony ID(建议格式: com.modauthor.modname)</param>
        /// <param name="assembly">要应用补丁的程序集(通常是 typeof(ModBehaviour).Assembly)</param>
        /// <param name="logPrefix">日志前缀</param>
        /// <returns>Harmony实例</returns>
        public static Harmony InitializeAndPatch(string harmonyId, System.Reflection.Assembly assembly, string logPrefix = "[ModUtils]")
        {
            try
            {
                Debug.Log($"{logPrefix} 准备应用Harmony补丁...");
                Debug.Log($"{logPrefix} Harmony ID: {harmonyId}");
                Debug.Log($"{logPrefix} Assembly: {assembly.FullName}");

                Harmony harmony = new Harmony(harmonyId);
                harmony.PatchAll(assembly);
                
                Debug.Log($"{logPrefix} PatchAll 调用完成");

                // 验证补丁
                var patchedMethods = harmony.GetPatchedMethods();
                int patchCount = 0;
                foreach (var method in patchedMethods)
                {
                    patchCount++;
                    Debug.Log($"{logPrefix}   ✓ 已补丁: {method.DeclaringType?.Name}.{method.Name}");
                }

                if (patchCount == 0)
                {
                    Debug.LogWarning($"{logPrefix} ⚠ 没有任何方法被补丁");
                }
                else
                {
                    Debug.Log($"{logPrefix} ✓ Harmony补丁成功，共 {patchCount} 个方法");
                }

                return harmony;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{logPrefix} ❌ Harmony补丁失败: {ex.Message}");
                Debug.LogException(ex);
                throw;
            }
        }

        /// <summary>
        /// 移除Harmony补丁
        /// </summary>
        /// <param name="harmony">Harmony实例</param>
        /// <param name="harmonyId">Harmony ID</param>
        /// <param name="logPrefix">日志前缀</param>
        public static void UnpatchAll(Harmony harmony, string harmonyId, string logPrefix = "[ModUtils]")
        {
            try
            {
                if (harmony != null)
                {
                    harmony.UnpatchAll(harmonyId);
                    Debug.Log($"{logPrefix} Harmony补丁已移除");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 移除Harmony补丁时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 获取所有已打补丁的方法信息
        /// </summary>
        /// <param name="harmony">Harmony实例</param>
        /// <returns>补丁方法列表</returns>
        public static List<PatchedMethodInfo> GetPatchedMethodsInfo(Harmony harmony)
        {
            List<PatchedMethodInfo> result = new List<PatchedMethodInfo>();

            try
            {
                var patchedMethods = harmony.GetPatchedMethods();
                foreach (var method in patchedMethods)
                {
                    var patches = Harmony.GetPatchInfo(method);
                    if (patches != null)
                    {
                        result.Add(new PatchedMethodInfo
                        {
                            DeclaringType = method.DeclaringType?.Name ?? "Unknown",
                            MethodName = method.Name,
                            PrefixCount = patches.Prefixes.Count,
                            PostfixCount = patches.Postfixes.Count,
                            TranspilerCount = patches.Transpilers.Count,
                            FinalizerCount = patches.Finalizers.Count
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HarmonyUtils] 获取补丁信息失败: {e.Message}");
            }

            return result;
        }

        /// <summary>
        /// 打印所有已打补丁的方法信息
        /// </summary>
        public static void PrintPatchedMethods(Harmony harmony, string logPrefix = "[ModUtils]")
        {
            var methods = GetPatchedMethodsInfo(harmony);
            
            Debug.Log($"{logPrefix} ========== Harmony补丁信息 ==========");
            Debug.Log($"{logPrefix} 共有 {methods.Count} 个方法被补丁");
            
            foreach (var method in methods)
            {
                Debug.Log($"{logPrefix} - {method.DeclaringType}.{method.MethodName}");
                Debug.Log($"{logPrefix}     Prefix: {method.PrefixCount}, Postfix: {method.PostfixCount}, " +
                         $"Transpiler: {method.TranspilerCount}, Finalizer: {method.FinalizerCount}");
            }
            
            Debug.Log($"{logPrefix} =====================================");
        }

        /// <summary>
        /// 补丁方法信息
        /// </summary>
        public class PatchedMethodInfo
        {
            public string DeclaringType { get; set; }
            public string MethodName { get; set; }
            public int PrefixCount { get; set; }
            public int PostfixCount { get; set; }
            public int TranspilerCount { get; set; }
            public int FinalizerCount { get; set; }
        }
    }
}

