using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class UIToolkitBridge : MonoBehaviour
{
    [Header("References")] public UIDocument uiDocument;

    private VisualElement _root;

    // click buffer for buttons we bind at runtime
    private readonly HashSet<string> _buttonClicks = new HashSet<string>(StringComparer.Ordinal);

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
        // local capture name
        var id = elementName;
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
}
