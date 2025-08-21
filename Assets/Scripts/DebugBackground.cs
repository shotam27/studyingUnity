using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DebugBackground : MonoBehaviour
{
    [Header("Debug Background")]
    public Color color = new Color(1f, 0f, 0f, 0.25f);
    public Vector2 padding = new Vector2(4f, 4f);
    public bool autoCreate = true;

    private const string BgName = "DEBUG_BG";
    private RectTransform _rect;
    private GameObject _bgObj;
    private Image _bgImage;
    private RectTransform _bgRect;

    void OnEnable()
    {
        _rect = GetComponent<RectTransform>();
        if (autoCreate) CreateOrUpdateBackground();
    }

    void OnValidate()
    {
        _rect = GetComponent<RectTransform>();
        if (autoCreate) CreateOrUpdateBackground();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && autoCreate)
        {
            CreateOrUpdateBackground();
        }
#endif
    }

    public void CreateOrUpdateBackground()
    {
        if (_rect == null) _rect = GetComponent<RectTransform>();
        if (_rect == null) return;

        // find existing bg child
        var bgTransform = transform.Find(BgName);
        if (bgTransform == null)
        {
            _bgObj = new GameObject(BgName);
            _bgObj.transform.SetParent(transform, false);
            _bgObj.transform.SetSiblingIndex(0);
            _bgRect = _bgObj.AddComponent<RectTransform>();
            _bgImage = _bgObj.AddComponent<Image>();
            _bgImage.raycastTarget = false;
        }
        else
        {
            _bgObj = bgTransform.gameObject;
            _bgRect = _bgObj.GetComponent<RectTransform>();
            _bgImage = _bgObj.GetComponent<Image>();
            if (_bgRect == null) _bgRect = _bgObj.AddComponent<RectTransform>();
            if (_bgImage == null) _bgImage = _bgObj.AddComponent<Image>();
        }

        // stretch to parent with padding
        _bgRect.anchorMin = Vector2.zero;
        _bgRect.anchorMax = Vector2.one;
        _bgRect.pivot = _rect.pivot;
        _bgRect.anchoredPosition = Vector2.zero;

        _bgRect.offsetMin = new Vector2(-padding.x, -padding.y);
        _bgRect.offsetMax = new Vector2(padding.x, padding.y);

        _bgImage.color = color;
    }

    public void RemoveBackground()
    {
        var bgTransform = transform.Find(BgName);
        if (bgTransform != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) UnityEngine.Object.DestroyImmediate(bgTransform.gameObject);
            else
#endif
            Destroy(bgTransform.gameObject);
        }
    }
}
