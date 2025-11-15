using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Debug = UnityEngine.Debug;

namespace DuckovModding.Common
{
    /// <summary>
    /// UI相关的通用工具类
    /// </summary>
    public static class UIUtils
    {
        /// <summary>
        /// 创建一个简单的UI Canvas
        /// </summary>
        /// <param name="canvasName">Canvas名称</param>
        /// <param name="sortingOrder">渲染顺序</param>
        /// <param name="referenceResolution">参考分辨率</param>
        /// <returns>创建的Canvas</returns>
        public static Canvas CreateCanvas(string canvasName, int sortingOrder = 0, Vector2? referenceResolution = null)
        {
            GameObject canvasObj = new GameObject(canvasName);
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution ?? new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            return canvas;
        }

        /// <summary>
        /// 创建TextMeshProUGUI文本组件
        /// </summary>
        /// <param name="parent">父物体</param>
        /// <param name="name">物体名称</param>
        /// <param name="text">文本内容</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="color">文本颜色</param>
        /// <param name="alignment">对齐方式</param>
        /// <returns>创建的TextMeshProUGUI组件</returns>
        public static TextMeshProUGUI CreateText(Transform parent, string name, string text,
            float fontSize = 20f, Color? color = null, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);

            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200f, 50f);

            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = color ?? Color.white;
            textComponent.alignment = alignment;

            return textComponent;
        }

        /// <summary>
        /// 创建一个简单的按钮
        /// </summary>
        /// <param name="parent">父物体</param>
        /// <param name="name">按钮名称</param>
        /// <param name="buttonText">按钮文本</param>
        /// <param name="onClick">点击回调</param>
        /// <param name="size">按钮大小</param>
        /// <returns>创建的Button组件</returns>
        public static Button CreateButton(Transform parent, string name, string buttonText,
            Action onClick, Vector2? size = null)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size ?? new Vector2(160f, 40f);

            Image image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Button button = buttonObj.AddComponent<Button>();
            if (onClick != null)
            {
                button.onClick.AddListener(() => onClick());
            }

            // 添加文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = buttonText;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.fontSize = 16f;

            return button;
        }

        /// <summary>
        /// 检查鼠标是否在UI元素上
        /// </summary>
        /// <returns>是否在UI上</returns>
        public static bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
                return false;

            return EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// 获取鼠标下的所有UI元素
        /// </summary>
        /// <param name="mousePosition">鼠标位置(可选,默认使用当前鼠标位置)</param>
        /// <returns>UI元素列表</returns>
        public static List<RaycastResult> GetUIElementsUnderMouse(Vector2? mousePosition = null)
        {
            List<RaycastResult> results = new List<RaycastResult>();

            if (EventSystem.current == null)
                return results;

            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = mousePosition ?? (Vector2)Input.mousePosition
            };

            EventSystem.current.RaycastAll(pointerData, results);
            return results;
        }

        /// <summary>
        /// 检查鼠标是否在空白区域(不在任何UI上)
        /// </summary>
        /// <param name="excludeObjects">要排除的GameObject(如自己的UI)</param>
        /// <returns>是否在空白区域</returns>
        public static bool IsMouseOverEmptyArea(params GameObject[] excludeObjects)
        {
            try
            {
                var results = GetUIElementsUnderMouse();

                // 移除要排除的对象
                if (excludeObjects != null && excludeObjects.Length > 0)
                {
                    results.RemoveAll(result =>
                    {
                        foreach (var exclude in excludeObjects)
                        {
                            if (exclude == null) continue;
                            if (result.gameObject == exclude ||
                                result.gameObject.transform.IsChildOf(exclude.transform))
                            {
                                return true;
                            }
                        }
                        return false;
                    });
                }

                return results.Count == 0;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIUtils] 检测空白区域失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取GameObject的完整路径
        /// </summary>
        public static string GetGameObjectPath(GameObject obj)
        {
            if (obj == null) return "null";

            string path = obj.name;
            Transform parent = obj.transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        /// <summary>
        /// 查找所有指定类型的UI组件(包括非激活的)
        /// </summary>
        public static T[] FindAllUIComponents<T>(bool includeInactive = true) where T : Component
        {
            return UnityEngine.Object.FindObjectsOfType<T>(includeInactive);
        }

        /// <summary>
        /// 设置RectTransform为全屏拉伸
        /// </summary>
        public static void SetFullScreen(RectTransform rectTransform)
        {
            if (rectTransform == null) return;

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 设置RectTransform居中
        /// </summary>
        public static void SetCenter(RectTransform rectTransform, Vector2 size)
        {
            if (rectTransform == null) return;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// 获取Canvas的缩放因子
        /// </summary>
        public static float GetCanvasScaleFactor(Canvas canvas)
        {
            if (canvas == null) return 1f;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null) return 1f;

            return canvas.scaleFactor;
        }

        /// <summary>
        /// 销毁所有子物体
        /// </summary>
        public static void DestroyAllChildren(Transform parent)
        {
            if (parent == null) return;

            int childCount = parent.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
            }
        }

        /// <summary>
        /// 设置文本颜色渐变
        /// </summary>
        public static void SetTextGradient(TextMeshProUGUI text, Color topColor, Color bottomColor)
        {
            if (text == null) return;

            text.enableVertexGradient = true;
            text.colorGradient = new VertexGradient(topColor, topColor, bottomColor, bottomColor);
        }

        /// <summary>
        /// 创建一个背景面板
        /// </summary>
        public static GameObject CreateBackgroundPanel(Transform parent, Color color, string name = "Background")
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);

            RectTransform rect = panel.AddComponent<RectTransform>();
            SetFullScreen(rect);

            Image image = panel.AddComponent<Image>();
            image.color = color;

            return panel;
        }

        /// <summary>
        /// 使UI可拖动
        /// </summary>
        public static void MakeDraggable(GameObject uiObject)
        {
            if (uiObject.GetComponent<DraggableUI>() == null)
            {
                uiObject.AddComponent<DraggableUI>();
            }
        }

        /// <summary>
        /// 简单的UI拖动组件
        /// </summary>
        private class DraggableUI : MonoBehaviour, IDragHandler, IBeginDragHandler
        {
            private RectTransform rectTransform;
            private Canvas canvas;
            private Vector2 offset;

            void Awake()
            {
                rectTransform = GetComponent<RectTransform>();
                canvas = GetComponentInParent<Canvas>();
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (rectTransform == null) return;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform, eventData.position, eventData.pressEventCamera, out offset);
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (rectTransform == null || canvas == null) return;

                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
                {
                    rectTransform.anchoredPosition = localPoint - offset;
                }
            }
        }
    }
}

