using System;
using System.Collections;
using UnityEngine;
using FMODUnity;

// 使用别名解决 Debug 类名冲突  
using Debug = UnityEngine.Debug;

namespace SoundBeacon
{
    /// <summary>
    /// 声音信标对象 - 使用FMOD发出随距离衰减的3D音效
    /// Sound Beacon Object - Emits 3D audio with distance-based attenuation using FMOD
    /// </summary>
    public class SoundBeaconObject : MonoBehaviour
    {
        // FMOD音频配置
        private FMOD.Sound fmodSound;
        private FMOD.Channel channel;
        private bool hasSound = false;
        
        // 播放参数
        private float minInterval = 3f;    // 最小间隔时间(秒)
        private float maxInterval = 10f;   // 最大间隔时间(秒)
        
        // 3D音效参数
        private float minDistance = 5f;    // 最小听到距离
        private float maxDistance = 50f;   // 最大听到距离
        private float volume = 0.8f;       // 音量
        
        // 可视化对象
        private GameObject? visualCube;
        
        // 协程引用
        private Coroutine? playRoutine;
        
        /// <summary>
        /// 初始化声音信标
        /// </summary>
        public void Initialize(FMOD.Sound sound, float minInt, float maxInt, float minDist, float maxDist, float vol)
        {
            fmodSound = sound;
            hasSound = sound.hasHandle();
            minInterval = minInt;
            maxInterval = maxInt;
            minDistance = minDist;
            maxDistance = maxDist;
            volume = vol;
            
            SetupVisualRepresentation();
            
            if (hasSound)
            {
                StartPlayback();
                Debug.Log($"[SoundBeacon] 信标已初始化于位置: {transform.position}");
            }
            else
            {
                Debug.LogWarning($"[SoundBeacon] 信标初始化时没有有效的音频");
            }
        }
        
        /// <summary>
        /// 设置可视化表现 - 使用Cube代替
        /// </summary>
        private void SetupVisualRepresentation()
        {
            // 创建一个Cube作为占位符
            visualCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualCube.transform.SetParent(transform);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.transform.localScale = Vector3.one * 3f; // 3米大小,超大超显眼!
            
            // 设置到Default层,确保可见
            visualCube.layer = 0;  // Default layer
            
            // 尝试使用自发光着色器(不受光照影响,永远可见)
            Renderer renderer = visualCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                // 优先尝试自发光着色器
                Shader unlitShader = Shader.Find("Unlit/Color");
                if (unlitShader == null)
                {
                    unlitShader = Shader.Find("Sprites/Default");
                }
                if (unlitShader == null)
                {
                    unlitShader = Shader.Find("UI/Default");
                }
                
                Material material;
                if (unlitShader != null)
                {
                    // 使用自发光着色器
                    material = new Material(unlitShader);
                    material.color = new Color(1f, 0.5f, 0f, 1f); // 亮橙色
                    Debug.Log($"[SoundBeacon] ✓ 使用自发光着色器: {unlitShader.name}");
                }
                else
                {
                    // 使用默认材质但设置为自发光
                    material = renderer.material;
                    material.color = new Color(1f, 0.5f, 0f, 1f);
                    
                    // 尝试启用自发光
                    if (material.HasProperty("_EmissionColor"))
                    {
                        material.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f, 1f) * 2f);
                        material.EnableKeyword("_EMISSION");
                    }
                    Debug.LogWarning("[SoundBeacon] ⚠ 使用默认材质+自发光");
                }
                
                renderer.material = material;
                
                // 禁用阴影投射和接收,让它更像发光体
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            
            // 添加一个点光源,让它真的发光!
            Light pointLight = visualCube.AddComponent<Light>();
            if (pointLight != null)
            {
                pointLight.type = LightType.Point;
                pointLight.color = new Color(1f, 0.5f, 0f);
                pointLight.intensity = 5f;
                pointLight.range = 15f;
                pointLight.shadows = LightShadows.None;
                Debug.Log("[SoundBeacon] ✓ 已添加点光源");
            }
            
            Debug.Log("[SoundBeacon] 视觉表现已设置(发光Cube + 光源)");
        }
        
        /// <summary>
        /// 获取FMOD Core System
        /// </summary>
        private FMOD.System GetFMODSystem()
        {
            FMOD.Studio.System studioSystem = FMODUnity.RuntimeManager.StudioSystem;
            FMOD.System coreSystem;
            studioSystem.getCoreSystem(out coreSystem);
            return coreSystem;
        }
        
        /// <summary>
        /// 开始播放音效
        /// </summary>
        private void StartPlayback()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }
            playRoutine = StartCoroutine(PlaySoundRoutine());
        }
        
        /// <summary>
        /// 音效播放协程
        /// </summary>
        private IEnumerator PlaySoundRoutine()
        {
            while (true)
            {
                // 随机等待时间
                float waitTime = UnityEngine.Random.Range(minInterval, maxInterval);
                yield return new WaitForSeconds(waitTime);
                
                // 使用FMOD播放音效
                if (hasSound)
                {
                    PlaySoundWithFMOD();
                }
            }
        }

        /// <summary>
        /// 使用FMOD播放3D音效
        /// </summary>
        private void PlaySoundWithFMOD()
        {
            try
            {
                FMOD.System coreSystem = GetFMODSystem();
                
                // 播放音频
                FMOD.ChannelGroup nullGroup = new FMOD.ChannelGroup();
                FMOD.RESULT result = coreSystem.playSound(fmodSound, nullGroup, false, out channel);
                
                if (result != FMOD.RESULT.OK)
                {
                    Debug.LogError($"[SoundBeacon] FMOD播放失败: {result}");
                    return;
                }

                // 设置3D位置
                Vector3 pos = transform.position;
                FMOD.VECTOR fmodPos = new FMOD.VECTOR 
                { 
                    x = pos.x, 
                    y = pos.y, 
                    z = pos.z 
                };
                
                FMOD.VECTOR fmodVel = new FMOD.VECTOR { x = 0, y = 0, z = 0 };
                
                channel.set3DAttributes(ref fmodPos, ref fmodVel);
                
                // 设置音量
                channel.setVolume(volume);
                
                // 设置3D距离衰减
                channel.set3DMinMaxDistance(minDistance, maxDistance);
                
                Debug.Log($"[SoundBeacon] 播放音效于位置: ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SoundBeacon] 播放音效异常: {e.Message}");
            }
        }
        
        /// <summary>
        /// 更新音效参数(运行时可调用)
        /// </summary>
        public void UpdateAudioSettings(float minDist, float maxDist, float vol)
        {
            minDistance = minDist;
            maxDistance = maxDist;
            volume = vol;
            
            // 如果当前有播放中的频道,更新其参数
            if (channel.hasHandle())
            {
                channel.setVolume(volume);
                channel.set3DMinMaxDistance(minDistance, maxDistance);
            }
        }
        
        /// <summary>
        /// 停止播放
        /// </summary>
        public void StopPlayback()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }
            
            // 停止FMOD频道
            if (channel.hasHandle())
            {
                bool isPlaying = false;
                channel.isPlaying(out isPlaying);
                if (isPlaying)
                {
                    channel.stop();
                }
            }
        }
        
        /// <summary>
        /// 替换可视化模型
        /// </summary>
        public void ReplaceVisualModel(GameObject newModel)
        {
            if (visualCube != null)
            {
                Destroy(visualCube);
            }
            
            visualCube = Instantiate(newModel, transform);
            visualCube.transform.localPosition = Vector3.zero;
            visualCube.transform.localRotation = Quaternion.identity;
            
            Debug.Log("[SoundBeacon] 视觉模型已替换");
        }
        
        void OnDestroy()
        {
            StopPlayback();
            
            // 注意: FMOD.Sound 由 ModBehaviour 统一管理,这里不释放
            
            Debug.Log("[SoundBeacon] 信标已销毁");
        }
        
        // 在Scene视图中显示音效范围
        void OnDrawGizmosSelected()
        {
            // 绘制最小距离
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, minDistance);
            
            // 绘制最大距离
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, maxDistance);
        }
    }
}

