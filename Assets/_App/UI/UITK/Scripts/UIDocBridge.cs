using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UIDocBridge : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement root;

    // If you change your language set, update this array.
    public string[] langCodes = new[] { "EN", "FR", "IL", "ES" };

    void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        root = uiDocument != null ? uiDocument.rootVisualElement : null;
    }

    // Generic: set the text of a Label by its name
    public void SetText(string elementName, string value)
    {
        if (root == null) return;
        var label = root.Q<Label>(elementName);
        if (label != null) label.text = value ?? string.Empty;
    }

    // Generic: toggle a USS class on any element by name
    public void SetSelected(string elementName, bool selected)
    {
        if (root == null) return;
        var el = root.Q<VisualElement>(elementName);
        if (el != null) el.EnableInClassList("selected", selected);
    }

    // Convenience: highlight the correct language button and update label
    public void SelectLanguage(string langCode)
    {
        if (root == null) return;

        // Update the small label that shows default language (optional)
        var dl = root.Q<Label>("defaultLangLabel");
        if (dl != null && !string.IsNullOrEmpty(langCode)) dl.text = langCode;

        foreach (var code in langCodes)
        {
            var btn = root.Q<VisualElement>("btnLang" + code);
            if (btn != null) btn.EnableInClassList("selected",
                string.Equals(code, langCode, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
