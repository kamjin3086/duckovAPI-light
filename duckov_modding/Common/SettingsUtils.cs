using System;
using Duckov.Options;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// 设置保存/加载的通用工具类
    /// 封装游戏的OptionsManager,提供类型安全的API
    /// </summary>
    public static class SettingsUtils
    {
        /// <summary>
        /// 保存设置(int类型)
        /// </summary>
        /// <param name="key">设置键名</param>
        /// <param name="value">值</param>
        /// <param name="logPrefix">日志前缀</param>
        /// <returns>是否保存成功</returns>
        public static bool SaveInt(string key, int value, string logPrefix = "[ModUtils]")
        {
            try
            {
                OptionsManager.Save<int>(key, value);
                Debug.Log($"{logPrefix} 已保存设置 [{key}] = {value}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 保存设置失败 [{key}]: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载设置(int类型)
        /// </summary>
        /// <param name="key">设置键名</param>
        /// <param name="defaultValue">默认值</param>
        /// <param name="logPrefix">日志前缀</param>
        /// <returns>设置值</returns>
        public static int LoadInt(string key, int defaultValue = 0, string logPrefix = "[ModUtils]")
        {
            try
            {
                int value = OptionsManager.Load<int>(key, defaultValue);
                Debug.Log($"{logPrefix} 已加载设置 [{key}] = {value}");
                return value;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 加载设置失败 [{key}]: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 保存设置(float类型)
        /// </summary>
        public static bool SaveFloat(string key, float value, string logPrefix = "[ModUtils]")
        {
            try
            {
                OptionsManager.Save<float>(key, value);
                Debug.Log($"{logPrefix} 已保存设置 [{key}] = {value}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 保存设置失败 [{key}]: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载设置(float类型)
        /// </summary>
        public static float LoadFloat(string key, float defaultValue = 0f, string logPrefix = "[ModUtils]")
        {
            try
            {
                float value = OptionsManager.Load<float>(key, defaultValue);
                Debug.Log($"{logPrefix} 已加载设置 [{key}] = {value}");
                return value;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 加载设置失败 [{key}]: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 保存设置(bool类型)
        /// </summary>
        public static bool SaveBool(string key, bool value, string logPrefix = "[ModUtils]")
        {
            try
            {
                OptionsManager.Save<bool>(key, value);
                Debug.Log($"{logPrefix} 已保存设置 [{key}] = {value}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 保存设置失败 [{key}]: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载设置(bool类型)
        /// </summary>
        public static bool LoadBool(string key, bool defaultValue = false, string logPrefix = "[ModUtils]")
        {
            try
            {
                bool value = OptionsManager.Load<bool>(key, defaultValue);
                Debug.Log($"{logPrefix} 已加载设置 [{key}] = {value}");
                return value;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 加载设置失败 [{key}]: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 保存设置(string类型)
        /// </summary>
        public static bool SaveString(string key, string value, string logPrefix = "[ModUtils]")
        {
            try
            {
                OptionsManager.Save<string>(key, value);
                Debug.Log($"{logPrefix} 已保存设置 [{key}] = {value}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 保存设置失败 [{key}]: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载设置(string类型)
        /// </summary>
        public static string LoadString(string key, string defaultValue = "", string logPrefix = "[ModUtils]")
        {
            try
            {
                string value = OptionsManager.Load<string>(key, defaultValue);
                Debug.Log($"{logPrefix} 已加载设置 [{key}] = {value}");
                return value;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 加载设置失败 [{key}]: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 保存设置(泛型,用于枚举等类型)
        /// </summary>
        public static bool Save<T>(string key, T value, string logPrefix = "[ModUtils]")
        {
            try
            {
                OptionsManager.Save<T>(key, value);
                Debug.Log($"{logPrefix} 已保存设置 [{key}] = {value}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 保存设置失败 [{key}]: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 加载设置(泛型,用于枚举等类型)
        /// </summary>
        public static T Load<T>(string key, T defaultValue, string logPrefix = "[ModUtils]")
        {
            try
            {
                T value = OptionsManager.Load<T>(key, defaultValue);
                Debug.Log($"{logPrefix} 已加载设置 [{key}] = {value}");
                return value;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 加载设置失败 [{key}]: {e.Message}");
                return defaultValue;
            }
        }

        /// <summary>
        /// 检查设置是否存在
        /// </summary>
        public static bool HasKey(string key)
        {
            try
            {
                // 尝试加载,如果不存在会返回默认值
                // 这里使用一个特殊的默认值来判断
                int testValue = OptionsManager.Load<int>(key, int.MinValue);
                return testValue != int.MinValue;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 删除设置
        /// </summary>
        public static bool DeleteKey(string key, string logPrefix = "[ModUtils]")
        {
            try
            {
                // OptionsManager可能没有提供Delete方法
                // 可以通过保存一个特殊值来标记删除
                OptionsManager.Save<string>(key, null);
                Debug.Log($"{logPrefix} 已删除设置 [{key}]");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 删除设置失败 [{key}]: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 生成Mod专用的设置键名(避免与其他Mod冲突)
        /// </summary>
        /// <param name="modName">Mod名称</param>
        /// <param name="settingName">设置名称</param>
        /// <returns>完整的键名</returns>
        public static string GetModSettingKey(string modName, string settingName)
        {
            return $"{modName}_{settingName}";
        }

        /// <summary>
        /// Mod设置管理器
        /// 为单个Mod提供方便的设置管理
        /// </summary>
        public class ModSettingsManager
        {
            private readonly string modName;
            private readonly string logPrefix;

            public ModSettingsManager(string modName, string logPrefix = null)
            {
                this.modName = modName;
                this.logPrefix = logPrefix ?? $"[{modName}]";
            }

            private string GetKey(string settingName)
            {
                return GetModSettingKey(modName, settingName);
            }

            public bool SaveInt(string settingName, int value)
            {
                return SettingsUtils.SaveInt(GetKey(settingName), value, logPrefix);
            }

            public int LoadInt(string settingName, int defaultValue = 0)
            {
                return SettingsUtils.LoadInt(GetKey(settingName), defaultValue, logPrefix);
            }

            public bool SaveFloat(string settingName, float value)
            {
                return SettingsUtils.SaveFloat(GetKey(settingName), value, logPrefix);
            }

            public float LoadFloat(string settingName, float defaultValue = 0f)
            {
                return SettingsUtils.LoadFloat(GetKey(settingName), defaultValue, logPrefix);
            }

            public bool SaveBool(string settingName, bool value)
            {
                return SettingsUtils.SaveBool(GetKey(settingName), value, logPrefix);
            }

            public bool LoadBool(string settingName, bool defaultValue = false)
            {
                return SettingsUtils.LoadBool(GetKey(settingName), defaultValue, logPrefix);
            }

            public bool SaveString(string settingName, string value)
            {
                return SettingsUtils.SaveString(GetKey(settingName), value, logPrefix);
            }

            public string LoadString(string settingName, string defaultValue = "")
            {
                return SettingsUtils.LoadString(GetKey(settingName), defaultValue, logPrefix);
            }

            public bool Save<T>(string settingName, T value)
            {
                return SettingsUtils.Save<T>(GetKey(settingName), value, logPrefix);
            }

            public T Load<T>(string settingName, T defaultValue)
            {
                return SettingsUtils.Load<T>(GetKey(settingName), defaultValue, logPrefix);
            }
        }
    }
}

