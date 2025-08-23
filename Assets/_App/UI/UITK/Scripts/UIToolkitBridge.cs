using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class UIToolkitBridge : MonoBehaviour
{
    [Header("References")] public UIDocument uiDocument;

    private VisualElement _root;

    // Click buffer for any UI Toolkit Button we wire up
    private readonly HashSet<string> _buttonClicks = new HashSet<string>(StringComparer.Ordinal);

    // Field change buffer (TextField, DropdownField)
    private readonly Dictionary<string, string> _fieldChanges = new Dictionary<string, string>(StringComparer.Ordinal);

    void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
        _root = uiDocument != null ? uiDocument.rootVisualElement : null;
    }

    // ---------- Root & element access ----------
    public VisualElement GetRoot() => _root;
    public VisualElement GetElement(string name) => _root?.Q<VisualElement>(name);
    public TextElement GetTextElement(string name) => _root?.Q<TextElement>(name);
    public Label GetLabel(string name) => _root?.Q<Label>(name);
    public TextField GetTextField(string name) => _root?.Q<TextField>(name);
    public DropdownField GetDropdown(string name) => _root?.Q<DropdownField>(name);
    public Button GetButton(string name) => _root?.Q<Button>(name);

    // ---------- Generic helpers ----------
    public void SetText(string elementName, string text)
    {
        var el = GetTextElement(elementName);
        if (el != null) el.text = text ?? string.Empty;
    }

    public void AddClass(string elementName, string className)
    {
        var el = GetElement(elementName);
        el?.AddToClassList(className);
    }

    public void RemoveClass(string elementName, string className)
    {
        var el = GetElement(elementName);
        el?.RemoveFromClassList(className);
    }

    public void ToggleClass(string elementName, string className, bool enabled)
    {
        var el = GetElement(elementName);
        if (el == null) return;
        if (enabled) el.AddToClassList(className); else el.RemoveFromClassList(className);
    }

    public void SetVisible(string elementName, bool visible)
    {
        var el = GetElement(elementName);
        if (el == null) return;
        el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ---------- Button binding & polling ----------
    public void BindButton(string elementName)
    {
        var btn = GetButton(elementName);
        if (btn == null) return;
        var id = elementName; // capture
        btn.clicked += () => _buttonClicks.Add(id);
    }

    public bool ConsumeButtonClicked(string elementName)
    {
        if (_buttonClicks.Contains(elementName))
        {
            _buttonClicks.Remove(elementName);
            return true;
        }
        return false;
    }

    // ---------- Dropdown helpers ----------
    public void SetDropdownChoices(string elementName, string[] options, int selectIndex = 0)
    {
        var dd = GetDropdown(elementName);
        if (dd == null) return;
        dd.choices = options?.ToList() ?? new List<string>();
        if (dd.choices.Count == 0) { dd.value = string.Empty; return; }
        if (selectIndex >= 0 && selectIndex < dd.choices.Count) dd.value = dd.choices[selectIndex];
        else if (!dd.choices.Contains(dd.value)) dd.value = dd.choices[0];
    }

    public void SetDropdownChoicesCSV(string elementName, string csv, int selectIndex = 0)
    {
        var list = (csv ?? string.Empty)
            .Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
        SetDropdownChoices(elementName, list, selectIndex);
    }

    public void EnsureDropdownValue(string elementName, string desiredValue)
    {
        var dd = GetDropdown(elementName);
        if (dd == null) return;
        if (dd.choices == null || dd.choices.Count == 0) return;
        if (dd.choices.Contains(desiredValue)) dd.value = desiredValue;
        else if (!dd.choices.Contains(dd.value)) dd.value = dd.choices[0];
    }

    public void BindTextField(string elementName)
    {
        var tf = GetTextField(elementName);
        if (tf == null) return;
        tf.RegisterValueChangedCallback(evt => _fieldChanges[elementName] = evt.newValue);
    }

    public void BindDropdown(string elementName)
    {
        var dd = GetDropdown(elementName);
        if (dd == null) return;
        dd.RegisterValueChangedCallback(evt => _fieldChanges[elementName] = evt.newValue);
    }

    public string ConsumeFieldChange(string elementName)
    {
        if (_fieldChanges.TryGetValue(elementName, out var val))
        {
            _fieldChanges.Remove(elementName);
            return val;
        }
        return null;
    }

    // ---------- Language buttons (global list) ----------
    public void RebuildLanguageButtonsFromCSV(string containerName, string csv, string baseClasses = "qbtn lang-btn")
    {
        var root = GetElement(containerName);
        if (root == null) return;
        root.Clear();
        var tokens = (csv ?? "").Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in tokens)
        {
            var code = raw.Trim();
            if (string.IsNullOrEmpty(code)) continue;
            var btn = new Button();
            btn.name = "btnLang" + code;  // e.g., btnLangEN
            btn.text = code;
            foreach (var cls in (baseClasses ?? string.Empty).Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries))
                btn.AddToClassList(cls);
            var id = btn.name; btn.clicked += () => _buttonClicks.Add(id);
            root.Add(btn);
        }
    }

    public void SelectLanguageButton(string containerName, string code, string selectedClass = "selected")
    {
        var root = GetElement(containerName);
        if (root == null) return;
        foreach (var child in root.Children())
        {
            if (child is Button b)
            {
                bool isMatch = b.name.Equals("btnLang" + code, StringComparison.OrdinalIgnoreCase);
                if (isMatch) b.AddToClassList(selectedClass);
                else b.RemoveFromClassList(selectedClass);
            }
        }
    }

    // ---------- Client Cards by UUID ----------
    public void EnsureClientCards(string containerName, int targetCount)
    {
        var root = GetElement(containerName);
        if (root == null) return;
        while (root.childCount > targetCount) root.RemoveAt(root.childCount - 1);
        while (root.childCount < targetCount) AddClientCard(containerName, (root.childCount + 1).ToString());
    }

    // Add a numbered card (legacy helper)
    public void AddClientCard(string containerName, string displayIndex)
    {
        AddOrUpdateClientCard(containerName, displayIndex, $"Headset {displayIndex}", null, null);
    }

    public void AddOrUpdateClientCard(string containerName, string uuid, string displayName, string languagesCsv, string defaultLang)
    {
        var list = GetElement(containerName);
        if (list == null) return;

        var cardName = $"clientCard_{uuid}";
        var card = list.Q<VisualElement>(cardName);
        if (card == null)
        {
            card = new VisualElement { name = cardName };
            card.AddToClassList("issue-row");
            card.AddToClassList("is-active");
            card.AddToClassList("mt-12");

            // LEFT
            var hstack = new VisualElement(); hstack.AddToClassList("hstack");
            var icon = new VisualElement();
            icon.name = $"issueIcon_{uuid}";                 // named so FSMs can target it
            icon.AddToClassList("issue-icon");
            icon.AddToClassList("active");
            var copy = new VisualElement(); copy.AddToClassList("issue-copy"); copy.AddToClassList("ml-12");
            var lblTitle = new Label("") { name = $"lblClientTitle_{uuid}" }; lblTitle.AddToClassList("row-title");
            var lblSub   = new Label("Connected") { name = $"lblClientSub_{uuid}" }; lblSub.AddToClassList("row-sub"); lblSub.AddToClassList("active"); lblSub.AddToClassList("mt-2");
            copy.Add(lblTitle); copy.Add(lblSub);
            hstack.Add(icon); hstack.Add(copy);

            // SPACER
            var spacer = new VisualElement(); spacer.AddToClassList("flex-spacer");

            // RIGHT
            var right = new VisualElement(); right.AddToClassList("hstack"); right.AddToClassList("buttons-right");
            var btnGroup = new VisualElement(); btnGroup.AddToClassList("btn-group");
            var btnPlay  = new Button(){ name = $"btnPlay_{uuid}"  }; btnPlay.AddToClassList("icon-btn"); btnPlay.AddToClassList("play"); btnPlay.AddToClassList("selected"); btnPlay.AddToClassList("ml-0");
            var btnReset = new Button(){ name = $"btnReset_{uuid}" }; btnReset.AddToClassList("icon-btn"); btnReset.AddToClassList("reset");
            var btnStop  = new Button(){ name = $"btnStop_{uuid}"  }; btnStop.AddToClassList("icon-btn"); btnStop.AddToClassList("stop");
            var idPlay = btnPlay.name;  btnPlay.clicked  += () => _buttonClicks.Add(idPlay);
            var idReset= btnReset.name; btnReset.clicked += () => _buttonClicks.Add(idReset);
            var idStop = btnStop.name;  btnStop.clicked  += () => _buttonClicks.Add(idStop);
            btnGroup.Add(btnPlay); btnGroup.Add(btnReset); btnGroup.Add(btnStop);

            // Language group container for this card
            var langGroup = new VisualElement { name = $"langGroup_{uuid}" }; langGroup.AddToClassList("btn-group");

            // Battery label
            var battery = new Label("85%") { name = $"lblBattery_{uuid}" }; battery.AddToClassList("battery"); battery.AddToClassList("ml-12");

            right.Add(btnGroup);
            right.Add(new VisualElement(){ name=$"groupSpacer_{uuid}", pickingMode = PickingMode.Ignore });
            right.Add(langGroup);
            right.Add(battery);

            // Assemble
            card.Add(hstack);
            card.Add(spacer);
            card.Add(right);
            list.Add(card);
        }

        // Update display name
        var title = card.Q<Label>($"lblClientTitle_{uuid}");
        if (title != null) title.text = displayName ?? uuid;

        // Build language buttons if CSV provided
        if (!string.IsNullOrEmpty(languagesCsv))
        {
            BuildLanguageButtonsForCard(uuid, languagesCsv, defaultLang);
        }
    }

    public void BuildLanguageButtonsForCard(string uuid, string languagesCsv, string defaultLang)
    {
        var langGroup = _root?.Q<VisualElement>($"langGroup_{uuid}");
        if (langGroup == null) return;
        langGroup.Clear();

        var tokens = (languagesCsv ?? "").Split(new[]{','}, StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
        foreach (var code in tokens)
        {
            var btn = new Button { name = $"btnLang_{uuid}_{code}", text = code };
            btn.AddToClassList("lang-btn");
            var id = btn.name; btn.clicked += () => _buttonClicks.Add(id);
            langGroup.Add(btn);
        }

        // Select default
        SelectClientLanguage("clientsList", uuid, defaultLang);
    }

    public void SelectClientLanguage(string containerName, string uuid, string code, string selectedClass = "selected")
    {
        var langGroup = _root?.Q<VisualElement>($"langGroup_{uuid}");
        if (langGroup == null) return;
        foreach (var child in langGroup.Children())
        {
            if (child is Button b)
            {
                bool match = b.name.Equals($"btnLang_{uuid}_{code}", StringComparison.OrdinalIgnoreCase);
                if (match) b.AddToClassList(selectedClass); else b.RemoveFromClassList(selectedClass);
            }
        }
    }

    public void RemoveClientCardByUUID(string containerName, string uuid)
    {
        var list = GetElement(containerName);
        if (list == null) return;
        var card = list.Q<VisualElement>($"clientCard_{uuid}");
        if (card != null) list.Remove(card);
    }

    public int GetChildCount(string containerName)
    {
        var root = GetElement(containerName);
        return root != null ? root.childCount : 0;
    }

    // ---------- Status helpers (row+icon+sub in one call) ----------
    public void SetClientStatusRed(string uuid)
    {
        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        var icon = _root?.Q<VisualElement>($"issueIcon_{uuid}");
        var sub  = _root?.Q<Label>($"lblClientSub_{uuid}");

        if (card != null){ card.RemoveFromClassList("is-active"); card.AddToClassList("is-error"); }
        if (icon != null){ icon.RemoveFromClassList("active");    icon.AddToClassList("error");    }
        if (sub  != null){ sub.RemoveFromClassList("active");     sub.AddToClassList("error");     }
    }

    public void SetClientStatusGreen(string uuid)
    {
        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        var icon = _root?.Q<VisualElement>($"issueIcon_{uuid}");
        var sub  = _root?.Q<Label>($"lblClientSub_{uuid}");

        if (card != null){ card.RemoveFromClassList("is-error"); card.AddToClassList("is-active"); }
        if (icon != null){ icon.RemoveFromClassList("error");    icon.AddToClassList("active");    }
        if (sub  != null){ sub.RemoveFromClassList("error");     sub.AddToClassList("active");     }
    }

    // Select a language inside ONE card by matching button.text (ignores case/whitespace)
    public void SelectClientLanguageByText(string uuid, string code, string selectedClass = "selected")
    {
        if (string.IsNullOrWhiteSpace(uuid) || string.IsNullOrWhiteSpace(code)) return;

        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        if (card == null) return;

        var norm = code.Trim().ToUpperInvariant();
        // Find all language buttons inside this card, regardless of container/naming
        var buttons = card.Query<Button>(className: "lang-btn").ToList();

        foreach (var b in buttons)
        {
            var txt = (b.text ?? "").Trim().ToUpperInvariant();
            if (txt == norm) b.AddToClassList(selectedClass);
            else             b.RemoveFromClassList(selectedClass);
        }
    }

    // Apply the same selection to ALL cards by text
    public void SetAllClientLanguagesByText(string containerName, string code, string selectedClass = "selected")
    {
        var list = GetElement(containerName);
        if (list == null || string.IsNullOrWhiteSpace(code)) return;

        foreach (var card in list.Children())
        {
            var name = card.name ?? "";
            const string prefix = "clientCard_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var uuid = name.Substring(prefix.Length);
            SelectClientLanguageByText(uuid, code, selectedClass);
        }
    }

    public void SetAllClientLanguages(string containerName, string code, string selectedClass = "selected")
    {
        var list = GetElement(containerName);
        if (list == null || string.IsNullOrEmpty(code)) return;

        foreach (var card in list.Children())
        {
            // card names are clientCard_<uuid>
            var name = card.name ?? "";
            const string prefix = "clientCard_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;

            var uuid = name.Substring(prefix.Length);
            SelectClientLanguage(containerName, uuid, code, selectedClass);
        }
    }

    // Convenience: GUID string you can call from Playmaker
    public string NewGuid(string prefix = "") => (prefix ?? "") + System.Guid.NewGuid().ToString("N");
}
