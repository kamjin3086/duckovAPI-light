using System;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// Unity组件相关的通用工具类
    /// </summary>
    public static class ComponentUtils
    {
        /// <summary>
        /// 创建一个持久化的GameObject(DontDestroyOnLoad)
        /// </summary>
        /// <param name="name">GameObject名称</param>
        /// <returns>创建的GameObject</returns>
        public static GameObject CreatePersistentObject(string name)
        {
            GameObject obj = new GameObject(name);
            UnityEngine.Object.DontDestroyOnLoad(obj);
            return obj;
        }

        /// <summary>
        /// 创建一个持久化的GameObject并添加组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="name">GameObject名称</param>
        /// <returns>添加的组件</returns>
        public static T CreatePersistentObject<T>(string name) where T : Component
        {
            GameObject obj = CreatePersistentObject(name);
            return obj.AddComponent<T>();
        }

        /// <summary>
        /// 安全地获取或添加组件
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="gameObject">目标GameObject</param>
        /// <returns>组件实例</returns>
        public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null)
            {
                Debug.LogError("[ComponentUtils] GetOrAddComponent: GameObject为null");
                return null;
            }

            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// 安全地销毁GameObject
        /// </summary>
        public static void SafeDestroy(GameObject obj, string logPrefix = "[ModUtils]")
        {
            if (obj != null)
            {
                try
                {
                    UnityEngine.Object.Destroy(obj);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{logPrefix} 销毁GameObject失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 安全地销毁Component
        /// </summary>
        public static void SafeDestroy(Component component, string logPrefix = "[ModUtils]")
        {
            if (component != null)
            {
                try
                {
                    UnityEngine.Object.Destroy(component);
                }
                catch (Exception e)
                {
                    Debug.LogError($"{logPrefix} 销毁Component失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 查找组件(包括非激活的)
        /// </summary>
        public static T FindComponent<T>(bool includeInactive = true) where T : Component
        {
            return UnityEngine.Object.FindObjectOfType<T>(includeInactive);
        }

        /// <summary>
        /// 查找所有组件(包括非激活的)
        /// </summary>
        public static T[] FindAllComponents<T>(bool includeInactive = true) where T : Component
        {
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
        }

        /// <summary>
        /// 在父对象及其所有子对象中查找组件
        /// </summary>
        public static T FindComponentInHierarchy<T>(GameObject root, bool includeInactive = true) where T : Component
        {
            if (root == null) return null;

            T component = root.GetComponentInChildren<T>(includeInactive);
            return component;
        }

        /// <summary>
        /// 在父对象及其所有子对象中查找所有组件
        /// </summary>
        public static T[] FindAllComponentsInHierarchy<T>(GameObject root, bool includeInactive = true) where T : Component
        {
            if (root == null) return new T[0];

            T[] components = root.GetComponentsInChildren<T>(includeInactive);
            return components;
        }

        /// <summary>
        /// 设置GameObject及其所有子对象的激活状态
        /// </summary>
        public static void SetActiveRecursive(GameObject obj, bool active)
        {
            if (obj == null) return;

            obj.SetActive(active);
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                SetActiveRecursive(obj.transform.GetChild(i).gameObject, active);
            }
        }

        /// <summary>
        /// 设置GameObject的Layer(包括所有子对象)
        /// </summary>
        public static void SetLayerRecursive(GameObject obj, int layer)
        {
            if (obj == null) return;

            obj.layer = layer;
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                SetLayerRecursive(obj.transform.GetChild(i).gameObject, layer);
            }
        }

        /// <summary>
        /// 克隆GameObject
        /// </summary>
        public static GameObject Clone(GameObject original, Transform parent = null, bool worldPositionStays = false)
        {
            if (original == null) return null;

            GameObject clone = UnityEngine.Object.Instantiate(original, parent, worldPositionStays);
            return clone;
        }

        /// <summary>
        /// 克隆GameObject并返回指定组件
        /// </summary>
        public static T Clone<T>(T original, Transform parent = null, bool worldPositionStays = false) where T : Component
        {
            if (original == null) return null;

            T clone = UnityEngine.Object.Instantiate(original, parent, worldPositionStays);
            return clone;
        }

        /// <summary>
        /// 检查GameObject是否激活(包括父对象)
        /// </summary>
        public static bool IsActiveInHierarchy(GameObject obj)
        {
            if (obj == null) return false;
            return obj.activeInHierarchy;
        }

        /// <summary>
        /// 检查组件是否启用(包括GameObject激活状态)
        /// </summary>
        public static bool IsEnabled(Behaviour behaviour)
        {
            if (behaviour == null) return false;
            return behaviour.enabled && behaviour.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// 延迟调用(使用GameObject的生命周期)
        /// </summary>
        public static void DelayedCall(MonoBehaviour context, float delay, Action callback)
        {
            if (context == null || callback == null) return;

            context.StartCoroutine(DelayedCallCoroutine(delay, callback));
        }

        private static System.Collections.IEnumerator DelayedCallCoroutine(float delay, Action callback)
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }

        /// <summary>
        /// 下一帧调用
        /// </summary>
        public static void NextFrameCall(MonoBehaviour context, Action callback)
        {
            if (context == null || callback == null) return;

            context.StartCoroutine(NextFrameCallCoroutine(callback));
        }

        private static System.Collections.IEnumerator NextFrameCallCoroutine(Action callback)
        {
            yield return null;
            callback?.Invoke();
        }

        /// <summary>
        /// 添加标记组件(用于标识处理过的GameObject)
        /// </summary>
        public static TMarker AddMarker<TMarker>(GameObject obj) where TMarker : Component
        {
            if (obj == null) return null;
            return GetOrAddComponent<TMarker>(obj);
        }

        /// <summary>
        /// 检查是否有标记组件
        /// </summary>
        public static bool HasMarker<TMarker>(GameObject obj) where TMarker : Component
        {
            if (obj == null) return false;
            return obj.GetComponent<TMarker>() != null;
        }

        /// <summary>
        /// 移除标记组件
        /// </summary>
        public static void RemoveMarker<TMarker>(GameObject obj) where TMarker : Component
        {
            if (obj == null) return;

            TMarker marker = obj.GetComponent<TMarker>();
            if (marker != null)
            {
                UnityEngine.Object.Destroy(marker);
            }
        }
    }
}

