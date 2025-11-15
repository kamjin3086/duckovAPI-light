using System;
using System.IO;
using UnityEngine;
using FMODUnity;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// 音频相关的通用工具类(FMOD)
    /// </summary>
    public static class AudioUtils
    {
        /// <summary>
        /// 使用FMOD加载音频文件
        /// </summary>
        /// <param name="audioPath">音频文件路径</param>
        /// <param name="mode">FMOD加载模式(默认: 3D + Loop_Off)</param>
        /// <param name="logPrefix">日志前缀</param>
        /// <returns>加载的FMOD.Sound,如果失败返回空句柄</returns>
        public static FMOD.Sound LoadAudioWithFMOD(string audioPath, FMOD.MODE mode = FMOD.MODE.DEFAULT, string logPrefix = "[AudioUtils]")
        {
            FMOD.Sound sound = default;

            try
            {
                if (!File.Exists(audioPath))
                {
                    Debug.LogError($"{logPrefix} 音频文件不存在: {audioPath}");
                    return sound;
                }

                // 获取FMOD系统
                FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
                if (!studioSystem.hasHandle())
                {
                    Debug.LogError($"{logPrefix} FMOD Studio System 未初始化");
                    return sound;
                }

                FMOD.System coreSystem;
                FMOD.RESULT result = studioSystem.getCoreSystem(out coreSystem);
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"{logPrefix} 获取FMOD Core System失败: {result}");
                    return sound;
                }

                // 默认使用3D和不循环模式
                if (mode == FMOD.MODE.DEFAULT)
                {
                    mode = FMOD.MODE.DEFAULT | FMOD.MODE._3D | FMOD.MODE.LOOP_OFF;
                }

                // 创建Sound
                result = coreSystem.createSound(audioPath, mode, out sound);
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"{logPrefix} FMOD创建Sound失败: {result}");
                    return default;
                }

                // 获取并打印音频信息
                uint length = 0;
                sound.getLength(out length, FMOD.TIMEUNIT.MS);
                
                FMOD.SOUND_TYPE soundType;
                FMOD.SOUND_FORMAT soundFormat;
                int channels = 0;
                int bits = 0;
                sound.getFormat(out soundType, out soundFormat, out channels, out bits);

                Debug.Log($"{logPrefix} ✓ FMOD音频加载成功!");
                Debug.Log($"{logPrefix}    文件: {Path.GetFileName(audioPath)}");
                Debug.Log($"{logPrefix}    时长: {length / 1000f:F2}秒");
                Debug.Log($"{logPrefix}    类型: {soundType}");
                Debug.Log($"{logPrefix}    格式: {soundFormat}");
                Debug.Log($"{logPrefix}    声道: {channels}");
                Debug.Log($"{logPrefix}    位深: {bits}");

                return sound;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 加载音频时出错: {e.Message}");
                Debug.LogError($"{logPrefix} 堆栈: {e.StackTrace}");
                return default;
            }
        }

        /// <summary>
        /// 在指定位置播放3D音频
        /// </summary>
        /// <param name="sound">FMOD Sound</param>
        /// <param name="position">播放位置</param>
        /// <param name="volume">音量(0-1)</param>
        /// <param name="minDistance">最小听到距离</param>
        /// <param name="maxDistance">最大听到距离</param>
        /// <param name="logPrefix">日志前缀</param>
        /// <returns>播放的Channel</returns>
        public static FMOD.Channel Play3DSound(FMOD.Sound sound, Vector3 position, float volume = 1f, 
            float minDistance = 5f, float maxDistance = 50f, string logPrefix = "[AudioUtils]")
        {
            FMOD.Channel channel = default;

            try
            {
                if (!sound.hasHandle())
                {
                    Debug.LogError($"{logPrefix} Sound句柄无效");
                    return channel;
                }

                // 获取FMOD系统
                FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
                if (!studioSystem.hasHandle())
                {
                    Debug.LogError($"{logPrefix} FMOD Studio System 未初始化");
                    return channel;
                }

                FMOD.System coreSystem;
                FMOD.RESULT result = studioSystem.getCoreSystem(out coreSystem);
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"{logPrefix} 获取FMOD Core System失败: {result}");
                    return channel;
                }

                // 播放声音
                FMOD.ChannelGroup nullGroup = new FMOD.ChannelGroup();
                result = coreSystem.playSound(sound, nullGroup, false, out channel);
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"{logPrefix} 播放声音失败: {result}");
                    return channel;
                }

                // 设置3D属性
                FMOD.VECTOR fmodPos = new FMOD.VECTOR { x = position.x, y = position.y, z = position.z };
                FMOD.VECTOR fmodVel = new FMOD.VECTOR { x = 0, y = 0, z = 0 };
                channel.set3DAttributes(ref fmodPos, ref fmodVel);

                // 设置音量
                channel.setVolume(volume);

                // 设置3D距离
                channel.set3DMinMaxDistance(minDistance, maxDistance);

                return channel;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 播放音频时出错: {e.Message}");
                return channel;
            }
        }

        /// <summary>
        /// 释放FMOD Sound资源
        /// </summary>
        public static void ReleaseSound(ref FMOD.Sound sound, string logPrefix = "[AudioUtils]")
        {
            try
            {
                if (sound.hasHandle())
                {
                    sound.release();
                    Debug.Log($"{logPrefix} FMOD Sound已释放");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 释放Sound时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 停止并释放Channel
        /// </summary>
        public static void StopChannel(ref FMOD.Channel channel, string logPrefix = "[AudioUtils]")
        {
            try
            {
                if (channel.hasHandle())
                {
                    channel.stop();
                    Debug.Log($"{logPrefix} FMOD Channel已停止");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} 停止Channel时出错: {e.Message}");
            }
        }

        /// <summary>
        /// 查找Mod目录中的音频文件
        /// </summary>
        /// <param name="modDirectory">Mod目录路径</param>
        /// <param name="extensions">支持的扩展名</param>
        /// <returns>找到的音频文件路径,如果没找到返回null</returns>
        public static string FindAudioFile(string modDirectory, string[] extensions = null)
        {
            if (extensions == null)
            {
                extensions = new string[] { ".wav", ".ogg", ".mp3" };
            }

            foreach (string ext in extensions)
            {
                string[] files = Directory.GetFiles(modDirectory, $"*{ext}", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    return files[0];
                }
            }

            return null;
        }
    }
}

