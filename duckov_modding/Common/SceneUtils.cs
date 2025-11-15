using System;
using System.Collections.Generic;
using UnityEngine;
using Duckov;
using Duckov.Scenes;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// 场景相关的通用工具类
    /// </summary>
    public static class SceneUtils
    {
        /// <summary>
        /// 打印当前场景详细信息
        /// </summary>
        /// <param name="eventName">事件名称(用于标识调用来源)</param>
        /// <param name="logPrefix">日志前缀(默认"[ModUtils]")</param>
        public static void LogCurrentSceneInfo(string eventName, string logPrefix = "[ModUtils]")
        {
            try
            {
                Debug.Log($"{logPrefix} === 当前场景信息 ({eventName}) ===");
                
                // 主场景信息
                var mainScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                Debug.Log($"{logPrefix} 主场景: {mainScene.name} (路径: {mainScene.path})");
                
                // LevelManager信息
                if (LevelManager.Instance != null)
                {
                    var levelInfo = LevelManager.GetCurrentLevelInfo();
                    Debug.Log($"{logPrefix} LevelInfo:");
                    Debug.Log($"{logPrefix}    - 是否基础关卡: {levelInfo.isBaseLevel}");
                    Debug.Log($"{logPrefix}    - 场景名称: {levelInfo.sceneName}");
                    Debug.Log($"{logPrefix}    - 活动子场景ID: {levelInfo.activeSubSceneID}");
                }
                else
                {
                    Debug.Log($"{logPrefix} LevelManager.Instance == null");
                }
                
                // MultiSceneCore信息
                if (MultiSceneCore.Instance != null)
                {
                    Debug.Log($"{logPrefix} MultiSceneCore:");
                    Debug.Log($"{logPrefix}    - DisplayName: {MultiSceneCore.Instance.DisplayName}");
                    Debug.Log($"{logPrefix}    - DisplayNameRaw: {MultiSceneCore.Instance.DisplaynameRaw}");
                    
                    var mainSceneInfo = MultiSceneCore.MainScene;
                    if (mainSceneInfo.HasValue)
                    {
                        Debug.Log($"{logPrefix}    - 主场景: {mainSceneInfo.Value.name}");
                    }
                    
                    var activeSubScene = MultiSceneCore.ActiveSubScene;
                    if (activeSubScene.HasValue)
                    {
                        Debug.Log($"{logPrefix}    - 活动子场景: {activeSubScene.Value.name}");
                    }
                    
                    string activeSubSceneID = MultiSceneCore.ActiveSubSceneID;
                    if (!string.IsNullOrEmpty(activeSubSceneID))
                    {
                        Debug.Log($"{logPrefix}    - 活动子场景ID: {activeSubSceneID}");
                    }
                }
                else
                {
                    Debug.Log($"{logPrefix} MultiSceneCore.Instance == null");
                }
                
                // 玩家信息
                if (CharacterMainControl.Main != null)
                {
                    Debug.Log($"{logPrefix} 玩家存在: {CharacterMainControl.Main.transform.position}");
                }
                else
                {
                    Debug.Log($"{logPrefix} CharacterMainControl.Main == null");
                }
                
                Debug.Log($"{logPrefix} ================================");
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 打印场景信息时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 判断是否应该在当前场景执行某些操作(排除大厅、菜单等场景)
        /// </summary>
        /// <param name="excludedSceneKeywords">要排除的场景关键词</param>
        /// <param name="excludeBaseLevel">是否排除基础关卡(通常是大厅/基地)</param>
        /// <param name="requireActiveSubScene">是否要求有活动子场景</param>
        /// <param name="logPrefix">日志前缀</param>
        /// <returns>是否应该执行操作</returns>
        public static bool ShouldOperateInCurrentScene(
            string[] excludedSceneKeywords = null, 
            bool excludeBaseLevel = true,
            bool requireActiveSubScene = true,
            string logPrefix = "[ModUtils]")
        {
            try
            {
                // 必须有LevelManager
                if (LevelManager.Instance == null)
                {
                    Debug.Log($"{logPrefix} ❌ LevelManager不存在");
                    return false;
                }
                
                var levelInfo = LevelManager.GetCurrentLevelInfo();
                
                // 检查场景名称
                string sceneName = levelInfo.sceneName?.ToLower() ?? "";
                Debug.Log($"{logPrefix} 🔍 检查场景: {sceneName}");
                
                // 默认排除的场景
                if (excludedSceneKeywords == null)
                {
                    excludedSceneKeywords = new string[]
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
                }
                
                // 排除不应该操作的场景
                foreach (string excluded in excludedSceneKeywords)
                {
                    if (sceneName.Contains(excluded))
                    {
                        Debug.Log($"{logPrefix} ❌ 场景包含排除关键词: {excluded}");
                        return false;
                    }
                }
                
                // 如果是基础关卡(base level),可能是大厅/基地,根据参数决定是否跳过
                if (excludeBaseLevel && levelInfo.isBaseLevel)
                {
                    Debug.Log($"{logPrefix} ❌ 这是基础关卡(可能是大厅/基地)");
                    return false;
                }
                
                // 根据参数决定是否要求有活动的子场景
                if (requireActiveSubScene && string.IsNullOrEmpty(levelInfo.activeSubSceneID))
                {
                    Debug.Log($"{logPrefix} ❌ 没有活动子场景");
                    return false;
                }
                
                Debug.Log($"{logPrefix} ✅ 当前场景通过检查!");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 检查场景时出错: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取当前场景的显示名称
        /// </summary>
        public static string GetCurrentSceneDisplayName()
        {
            if (MultiSceneCore.Instance != null)
            {
                return MultiSceneCore.Instance.DisplayName;
            }
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        }

        /// <summary>
        /// 检查玩家是否存在
        /// </summary>
        public static bool IsPlayerPresent()
        {
            return CharacterMainControl.Main != null;
        }

        /// <summary>
        /// 获取玩家位置(如果玩家存在)
        /// </summary>
        public static Vector3? GetPlayerPosition()
        {
            if (CharacterMainControl.Main != null)
            {
                return CharacterMainControl.Main.transform.position;
            }
            return null;
        }
    }
}

