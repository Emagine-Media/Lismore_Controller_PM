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

    // Click buffer for buttons we bind/create at runtime
    private readonly HashSet<string> _buttonClicks = new HashSet<string>(StringComparer.Ordinal);

    // Field change buffer (TextField / DropdownField)
    private readonly Dictionary<string, string> _fieldChanges = new Dictionary<string, string>(StringComparer.Ordinal);

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();
        _root = uiDocument != null ? uiDocument.rootVisualElement : null;
    }

    // ---------- Root & element access ----------
    public VisualElement GetRoot() => _root;

    public VisualElement GetElement(string name) => _root?.Q<VisualElement>(name);
    public Label GetLabel(string name) => _root?.Q<Label>(name);
    public TextElement GetTextElement(string name) => _root?.Q<TextElement>(name);
    public TextField GetTextField(string name) => _root?.Q<TextField>(name);
    public DropdownField GetDropdown(string name) => _root?.Q<DropdownField>(name);
    public Button GetButton(string name) => _root?.Q<Button>(name);

    // ---------- Text helpers (Label/TextElement/TextField) ----------
    public void SetText(string elementName, string text)
    {
        var el = GetTextElement(elementName);
        if (el != null) el.text = text ?? string.Empty;
    }

    public void SetTextField(string elementName, string value)
    {
        var tf = GetTextField(elementName);
        if (tf != null) tf.value = value ?? string.Empty;
    }

    public string GetTextFieldValue(string elementName)
    {
        var tf = GetTextField(elementName);
        return tf != null ? tf.value : string.Empty;
    }

    public void SetDropdownValue(string elementName, string value)
    {
        var dd = GetDropdown(elementName);
        if (dd != null) dd.value = value ?? string.Empty;
    }

    public string GetDropdownValue(string elementName)
    {
        var dd = GetDropdown(elementName);
        return dd != null ? dd.value : string.Empty;
    }

    // ---------- Class helpers ----------
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

    public bool HasClass(string elementName, string className)
    {
        var el = GetElement(elementName);
        return el != null && el.ClassListContains(className);
    }

    // ---------- Visibility ----------
    public void SetVisible(string elementName, bool visible)
    {
        var el = GetElement(elementName);
        if (el == null) return;
        el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // ---------- Button click binding & polling ----------
    public void BindButton(string elementName)
    {
        var btn = GetButton(elementName);
        if (btn == null) return;
        var id = elementName; // capture
        btn.clicked += () => _buttonClicks.Add(id);
    }

    // Returns true once per actual click (consumes the flag)
    public bool ConsumeButtonClicked(string elementName)
    {
        if (_buttonClicks.Contains(elementName))
        {
            _buttonClicks.Remove(elementName);
            return true;
        }
        return false;
    }

    // ---------- Dropdown helpers (choices & change binding) ----------
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

    // ---------- Dynamic language buttons ----------
    // Create buttons under a container from CSV like "EN,FR,ES,IL"
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
            btn.text = code;               // e.g., EN

            foreach (var cls in (baseClasses ?? string.Empty).Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries))
                btn.AddToClassList(cls);

            var id = btn.name; // capture
            btn.clicked += () => _buttonClicks.Add(id);

            root.Add(btn);
        }
    }

    // Add 'selected' class to the matching language button and remove from others
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
}
