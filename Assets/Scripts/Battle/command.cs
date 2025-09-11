using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Simple command menu for monsters. Attach this to a monster GameObject (SpriteRenderer + Collider2D required).
[RequireComponent(typeof(Collider2D))]
public class Command : MonoBehaviour
{
    // Offset in world units from the sprite top-right where the menu will appear
    public Vector2 menuOffset = new Vector2(0.2f, 0.2f);

    // singleton reference to the currently open menu so only one shows at a time
    private static GameObject currentMenu;

    private void OnMouseDown()
    {
        // Toggle menu on click
        if (currentMenu != null)
        {
            Destroy(currentMenu);
            currentMenu = null;
            return;
        }

        CreateCommandMenu();
    }

    private void CreateCommandMenu()
    {
        // Ensure EventSystem exists so Buttons will respond
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
            Debug.Log("Command: Created temporary EventSystem");
        }

        // Compute a world-space position at the sprite's top-right
        Vector3 worldPos = transform.position;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            worldPos += new Vector3(sr.bounds.extents.x, sr.bounds.extents.y, 0f);
        }
        worldPos += new Vector3(menuOffset.x, menuOffset.y, 0f);

        // Create a world-space Canvas
        GameObject canvasGO = new GameObject("CommandMenu_Canvas", typeof(Canvas));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.transform.position = worldPos;
        // scale down so UI fits nicely in world space
        canvasGO.transform.localScale = Vector3.one * 0.01f;

        // Parent the canvas to a root so it isn't lost on scene reloads
        var root = GameObject.Find("UIRoot");
        if (root == null)
        {
            root = new GameObject("UIRoot");
        }
        canvasGO.transform.SetParent(root.transform, true);

        // Create a simple panel to hold buttons
        GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRT = panel.GetComponent<RectTransform>();
        panelRT.sizeDelta = new Vector2(180, 60);
        var panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.6f);

        // Button layout: 3 buttons horizontal
        float btnW = 56f, btnH = 48f;
        CreateButton(panel.transform, "移動", new Vector2(-62, 0), btnW, btnH, OnMoveClicked);
        CreateButton(panel.transform, "スキル", new Vector2(0, 0), btnW, btnH, OnSkillClicked);
        CreateButton(panel.transform, "キャンセル", new Vector2(62, 0), btnW, btnH, OnCancelClicked);

        currentMenu = canvasGO;
    }

    private void CreateButton(Transform parent, string label, Vector2 anchoredPos, float width, float height, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnGO = new GameObject("Btn_" + label, typeof(RectTransform), typeof(Image), typeof(Button));
        btnGO.transform.SetParent(parent, false);
        var rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = anchoredPos;

        var img = btnGO.GetComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.9f);

        var btn = btnGO.GetComponent<Button>();
        btn.onClick.AddListener(onClick);

        // label
        GameObject txtGO = new GameObject("Text", typeof(RectTransform));
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;

        var text = txtGO.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.black;
        // Use the legacy built-in runtime font; Arial.ttf is no longer valid in newer Unity versions.
        try
        {
            var builtin = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (builtin != null)
            {
                text.font = builtin;
            }
            else
            {
                // Fallback: try to create a dynamic font from OS (best-effort)
                Font osf = null;
                try { osf = Font.CreateDynamicFontFromOSFont("Arial", 24); } catch { osf = null; }
                text.font = osf;
            }
        }
        catch
        {
            // Last resort: null (Unity will handle with default font)
            text.font = null;
        }
        text.fontSize = 24;
    }

    private void OnMoveClicked()
    {
        Debug.Log($"Command: Move selected for '{gameObject.name}'");
        CloseMenu();
        // TODO: integrate with your movement system
    }

    private void OnSkillClicked()
    {
        Debug.Log($"Command: Skill selected for '{gameObject.name}'");
        CloseMenu();
        // TODO: open skill selection UI
    }

    private void OnCancelClicked()
    {
        Debug.Log($"Command: Cancel selected for '{gameObject.name}'");
        CloseMenu();
    }

    private void CloseMenu()
    {
        if (currentMenu != null)
        {
            Destroy(currentMenu);
            currentMenu = null;
        }
    }
}
