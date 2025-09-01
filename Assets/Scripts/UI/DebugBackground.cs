using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Simple helper that creates a semi-transparent Image behind a UI element for visual debugging of bounds.
    /// Used by ListItemHelper / MyMonsterUI for development-only background visuals.
    /// </summary>
    [DisallowMultipleComponent]
    public class DebugBackground : MonoBehaviour
    {
        public Color color = new Color(0f, 0.6f, 0f, 0.18f);
        public Vector2 padding = new Vector2(4f, 4f);
        public bool autoCreate = true;

        private const string BG_NAME = "__DEBUG_BG";
        private GameObject bgInstance;

        void Start()
        {
            if (autoCreate)
                CreateOrUpdateBackground();
        }

        void OnValidate()
        {
            if (autoCreate)
                CreateOrUpdateBackground();
        }

        /// <summary>
        /// Creates or updates a child Image used as a debug background.
        /// Safe to call in Edit and Play modes.
        /// </summary>
        public void CreateOrUpdateBackground()
        {
            // try to find existing
            if (bgInstance == null)
            {
                var t = transform.Find(BG_NAME);
                if (t != null) bgInstance = t.gameObject;
            }

            if (bgInstance == null)
            {
                bgInstance = new GameObject(BG_NAME, typeof(RectTransform), typeof(Image));
                bgInstance.transform.SetParent(transform, false);
                // place behind other children
                bgInstance.transform.SetAsFirstSibling();
            }

            var rt = bgInstance.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = new Vector2(-padding.x, -padding.y);
            rt.offsetMax = new Vector2(padding.x, padding.y);

            var img = bgInstance.GetComponent<Image>();
            img.raycastTarget = false;
            img.color = color;
        }
    }
}
