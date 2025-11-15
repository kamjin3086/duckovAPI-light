using System;
using UnityEngine;
using Duckov;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// 调试相关的通用工具类
    /// </summary>
    public static class DebugUtils
    {
        /// <summary>
        /// 打印玩家当前位置
        /// </summary>
        /// <param name="logPrefix">日志前缀</param>
        public static void PrintPlayerPosition(string logPrefix = "[ModUtils]")
        {
            if (CharacterMainControl.Main == null)
            {
                Debug.Log($"{logPrefix} ⚠ 玩家角色未找到");
                return;
            }

            Vector3 pos = CharacterMainControl.Main.transform.position;
            Debug.Log("=".PadRight(60, '='));
            Debug.Log($"{logPrefix} 📍 玩家当前位置:");
            Debug.Log($"{logPrefix}    X: {pos.x:F2}");
            Debug.Log($"{logPrefix}    Y: {pos.y:F2}");
            Debug.Log($"{logPrefix}    Z: {pos.z:F2}");
            Debug.Log($"{logPrefix} 复制代码: new Vector3({pos.x:F1}f, {pos.y:F1}f, {pos.z:F1}f)");
            Debug.Log("=".PadRight(60, '='));
        }

        /// <summary>
        /// 打印带标题和边框的日志
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="content">内容</param>
        /// <param name="logPrefix">日志前缀</param>
        public static void PrintBoxedLog(string title, string content, string logPrefix = "[ModUtils]")
        {
            Debug.Log("╔═══════════════════════════════════════════════════════════════════╗");
            Debug.Log($"║  {title.PadRight(64)}║");
            Debug.Log("╚═══════════════════════════════════════════════════════════════════╝");
            Debug.Log($"{logPrefix} {content}");
            Debug.Log("═════════════════════════════════════════════════════════════════════");
        }

        /// <summary>
        /// 打印分隔线
        /// </summary>
        /// <param name="style">样式: "=" 或 "-" 或 "━"</param>
        /// <param name="length">长度</param>
        public static void PrintSeparator(string style = "━", int length = 60)
        {
            Debug.Log(style.PadRight(length, style[0]));
        }

        /// <summary>
        /// 打印物体层级结构
        /// </summary>
        /// <param name="obj">要打印的物体</param>
        /// <param name="maxDepth">最大深度</param>
        /// <param name="logPrefix">日志前缀</param>
        public static void PrintObjectHierarchy(GameObject obj, int maxDepth = 3, string logPrefix = "[ModUtils]")
        {
            if (obj == null)
            {
                Debug.LogWarning($"{logPrefix} 物体为空");
                return;
            }

            Debug.Log($"{logPrefix} 物体层级结构: {obj.name}");
            PrintHierarchyRecursive(obj.transform, 0, maxDepth, logPrefix);
        }

        private static void PrintHierarchyRecursive(Transform trans, int depth, int maxDepth, string logPrefix)
        {
            if (depth > maxDepth) return;

            string indent = new string(' ', depth * 2);
            string prefix = depth == 0 ? "└─" : "├─";
            
            Debug.Log($"{logPrefix} {indent}{prefix} {trans.name}");
            Debug.Log($"{logPrefix} {indent}   Pos: ({trans.position.x:F1}, {trans.position.y:F1}, {trans.position.z:F1})");
            
            var components = trans.GetComponents<Component>();
            if (components.Length > 1) // 排除Transform本身
            {
                Debug.Log($"{logPrefix} {indent}   Components: {string.Join(", ", System.Array.ConvertAll(components, c => c.GetType().Name))}");
            }

            for (int i = 0; i < trans.childCount; i++)
            {
                PrintHierarchyRecursive(trans.GetChild(i), depth + 1, maxDepth, logPrefix);
            }
        }

        /// <summary>
        /// 打印Vector3信息
        /// </summary>
        public static string FormatVector3(Vector3 v, int decimals = 2)
        {
            string format = $"F{decimals}";
            return $"({v.x.ToString(format)}, {v.y.ToString(format)}, {v.z.ToString(format)})";
        }

        /// <summary>
        /// 打印颜色信息
        /// </summary>
        public static string FormatColor(Color c, int decimals = 2)
        {
            string format = $"F{decimals}";
            return $"RGBA({c.r.ToString(format)}, {c.g.ToString(format)}, {c.b.ToString(format)}, {c.a.ToString(format)})";
        }

        /// <summary>
        /// 在场景中绘制调试球体
        /// </summary>
        public static void DrawDebugSphere(Vector3 position, float radius, Color color, float duration = 5f)
        {
            // 绘制三个正交的圆
            DrawDebugCircle(position, radius, Vector3.up, color, duration);
            DrawDebugCircle(position, radius, Vector3.right, color, duration);
            DrawDebugCircle(position, radius, Vector3.forward, color, duration);
        }

        /// <summary>
        /// 在场景中绘制调试圆
        /// </summary>
        public static void DrawDebugCircle(Vector3 center, float radius, Vector3 normal, Color color, float duration = 5f)
        {
            Vector3 prevPoint = center + GetPerpendicularVector(normal) * radius;
            for (int i = 1; i <= 32; i++)
            {
                float angle = i * 360f / 32f * Mathf.Deg2Rad;
                Vector3 newPoint = center + (Mathf.Cos(angle) * GetPerpendicularVector(normal) + 
                                            Mathf.Sin(angle) * Vector3.Cross(normal, GetPerpendicularVector(normal))) * radius;
                UnityEngine.Debug.DrawLine(prevPoint, newPoint, color, duration);
                prevPoint = newPoint;
            }
        }

        private static Vector3 GetPerpendicularVector(Vector3 v)
        {
            if (Mathf.Abs(v.x) > 0.1f)
                return new Vector3(-v.y, v.x, 0).normalized;
            else
                return new Vector3(0, -v.z, v.y).normalized;
        }
    }
}

