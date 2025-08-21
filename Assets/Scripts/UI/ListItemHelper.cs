using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// 再利用可能なリストアイテム作成ヘルパー
    /// </summary>
    public static class ListItemHelper
    {
        /// <summary>
        /// リストアイテムを作成して基本設定を行う
        /// </summary>
        public static GameObject CreateListItem(GameObject prefab, Transform parent, string displayText, System.Action onClick = null)
        {
            if (prefab == null || parent == null)
            {
                Debug.LogError("CreateListItem: prefab or parent is null");
                return null;
            }

            var listItem = Object.Instantiate(prefab, parent);

            // テキスト設定
            var text = listItem.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = displayText;
            }

            // ボタン設定
            SetupButton(listItem, onClick);

            return listItem;
        }

        /// <summary>
        /// リストアイテムにボタン機能を設定
        /// </summary>
        public static void SetupButton(GameObject listItem, System.Action onClick)
        {
            if (listItem == null || onClick == null) return;

            // 既存のButtonを探す
            var button = listItem.GetComponent<Button>() ?? listItem.GetComponentInChildren<Button>();

            // Buttonが見つからない場合は作成
            if (button == null)
            {
                button = CreateClickableButton(listItem);
            }

            // イベント設定
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick.Invoke());
            }
        }

        /// <summary>
        /// クリック可能なButtonを作成（Graphic競合回避）
        /// </summary>
        private static Button CreateClickableButton(GameObject target)
        {
            // 既存のGraphicがあるかチェック
            var existingGraphic = target.GetComponent<Graphic>();
            
            if (existingGraphic == null)
            {
                // ルートに直接追加
                var img = target.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f); // 透明
                img.raycastTarget = true;
                
                var button = target.AddComponent<Button>();
                button.targetGraphic = img;
                return button;
            }
            else
            {
                // 子オブジェクトとしてクリックエリアを作成
                var clickArea = new GameObject("CLICK_AREA", typeof(RectTransform));
                clickArea.transform.SetParent(target.transform, false);
                
                var rt = clickArea.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                var img = clickArea.AddComponent<Image>();
                img.color = new Color(1f, 1f, 1f, 0f);
                img.raycastTarget = true;

                var button = clickArea.AddComponent<Button>();
                button.targetGraphic = img;
                return button;
            }
        }

        /// <summary>
        /// リストアイテムにデバッグ背景を追加
        /// </summary>
        public static void AddDebugBackground(GameObject listItem, Color color = default)
        {
            if (listItem == null) return;

            if (color == default)
                color = new Color(0f, 0.6f, 0f, 0.18f); // デフォルト緑

            try
            {
                var debugBg = listItem.GetComponent<DebugBackground>();
                if (debugBg == null)
                {
                    debugBg = listItem.AddComponent<DebugBackground>();
                    debugBg.color = color;
                    debugBg.padding = new Vector2(4f, 4f);
                    debugBg.autoCreate = true;
                    debugBg.CreateOrUpdateBackground();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to add DebugBackground: {ex.Message}");
            }
        }
    }
}
