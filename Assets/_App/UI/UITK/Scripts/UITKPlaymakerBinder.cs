using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using HutongGames.PlayMaker;


[RequireComponent(typeof(UIDocument))]
public class UITKPlaymakerBinder : MonoBehaviour
{
    [Serializable]
    public enum VarType { String, Int, Float, Bool }

    [Serializable]
    public class Binding
    {
        [UnityEngine.Tooltip("Name of the UI Toolkit element in your UXML (e.g., movieNameLabel)")]
        public string elementName;

        [UnityEngine.Tooltip("Name of the PlayMaker Global Variable (e.g., MovieName)")]
        public string globalVariable;

        [UnityEngine.Tooltip("Type of the PlayMaker Global Variable")]
        public VarType type = VarType.String;

        [UnityEngine.Tooltip("Optional numeric format, e.g. '0.0' or '#,0'")]
        public string numberFormat = "";
    }

    [Header("Explicit bindings (element ↔ global)")]
    public List<Binding> bindings = new List<Binding>();

    [Header("Convention binding")]
    [UnityEngine.Tooltip("When true, auto-binds any element ending with 'Label' to a global of the same name minus 'Label' (case-insensitive).")]
    public bool enableConventionBinding = true;

    [UnityEngine.Tooltip("Polling interval in seconds")]
    public float pollSeconds = 0.25f;

    [Header("Behavior")]
    [UnityEngine.Tooltip("If true, empty or null global values will NOT overwrite existing label text.")]
    public bool ignoreEmptyUpdates = true;

    [UnityEngine.Tooltip("If true, log each applied update once per change.")]
    public bool logUpdates = true;

    UIDocument _doc;
    VisualElement _root;

    // cache last shown strings to avoid spamming UI updates
    Dictionary<string, string> _lastValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    HashSet<string> _loggedMissingElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    HashSet<string> _loggedMissingGlobals = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    void Awake()
    {
        _doc = GetComponent<UIDocument>();
        _root = _doc.rootVisualElement;
    }

    void OnEnable()
    {
        CancelInvoke(nameof(Poll));
        InvokeRepeating(nameof(Poll), 0f, Mathf.Max(0.05f, pollSeconds));
    }

    void OnDisable()
    {
        CancelInvoke(nameof(Poll));
    }

    void Poll()
    {
        if (_root == null) return;

        // 1) explicit bindings
        foreach (var b in bindings)
        {
            if (string.IsNullOrWhiteSpace(b.elementName) || string.IsNullOrWhiteSpace(b.globalVariable))
                continue;

            TryUpdateElementFromGlobal(b.elementName, b.globalVariable, b.type, b.numberFormat);
        }

        // 2) convention bindings
        if (enableConventionBinding)
        {
            foreach (var el in _root.Children())
            {
                TryConventionBindRecursive(el);
            }
        }
    }

    void TryConventionBindRecursive(VisualElement el)
    {
        var name = el.name;
        if (!string.IsNullOrEmpty(name) && name.EndsWith("Label", StringComparison.Ordinal))
        {
            var guess = name.Substring(0, name.Length - "Label".Length);
            // Allow case-insensitive match to PlayMaker global
            TryUpdateElementFromGlobal(name, guess, VarType.String, "");
        }

        foreach (var child in el.Children())
            TryConventionBindRecursive(child);
    }

    void TryUpdateElementFromGlobal(string elementName, string globalName, VarType type, string numberFormat)
    {
        // Find UI element (Label/TextElement)
        Label label = _root.Q<Label>(elementName);
        TextElement textEl = null;
        if (label == null)
            textEl = _root.Q<TextElement>(elementName);
        TextField textField = null;
        if (label == null && textEl == null)
            textField = _root.Q<TextField>(elementName);

        if (label == null && textEl == null && textField == null)
        {
            LogOnce(_loggedMissingElements, elementName, $"[UITKPlaymakerBinder] UI element '{elementName}' not found in UXML.");
            return;
        }

        // Find Global variable (case-insensitive)
        string valueStr;
        if (!TryGetGlobalAsString(globalName, type, numberFormat, out valueStr))
        {
            LogOnce(_loggedMissingGlobals, globalName, $"[UITKPlaymakerBinder] PlayMaker Global '{globalName}' not found.");
            return;
        }

        // don't wipe out label if the global is empty
        if (ignoreEmptyUpdates && string.IsNullOrEmpty(valueStr))
            return;

        // Avoid unnecessary assignments
        string cacheKey = $"{elementName}__{globalName}";
        if (_lastValues.TryGetValue(cacheKey, out var last) && last == valueStr)
            return;

        _lastValues[cacheKey] = valueStr;

        if (label != null) label.text = valueStr;
        else textEl.text = valueStr;

        if (label != null) label.text = valueStr;
        else if (textEl != null) textEl.text = valueStr;
        else textField.value = valueStr;

        if (logUpdates)
            Debug.Log($"[UITKPlaymakerBinder] {elementName} \u2190 {globalName} = '{valueStr}'");
            }

    bool TryGetGlobalAsString(string globalName, VarType type, string fmt, out string result)
    {
        // Try exact, then case-insensitive fallback
        switch (type)
        {
            case VarType.String:
            {
                var v = PlayMakerGlobals.Instance.Variables.GetFsmString(globalName)
                        ?? FindFsmStringIgnoreCase(globalName);
                if (v == null) { result = null; return false; }
                result = v.Value ?? "";
                return true;
            }
            case VarType.Int:
            {
                var v = PlayMakerGlobals.Instance.Variables.GetFsmInt(globalName)
                        ?? FindFsmIntIgnoreCase(globalName);
                if (v == null) { result = null; return false; }
                result = string.IsNullOrEmpty(fmt) ? v.Value.ToString() : v.Value.ToString(fmt);
                return true;
            }
            case VarType.Float:
            {
                var v = PlayMakerGlobals.Instance.Variables.GetFsmFloat(globalName)
                        ?? FindFsmFloatIgnoreCase(globalName);
                if (v == null) { result = null; return false; }
                result = string.IsNullOrEmpty(fmt) ? v.Value.ToString() : v.Value.ToString(fmt);
                return true;
            }
            case VarType.Bool:
            {
                var v = PlayMakerGlobals.Instance.Variables.GetFsmBool(globalName)
                        ?? FindFsmBoolIgnoreCase(globalName);
                if (v == null) { result = null; return false; }
                result = v.Value ? "True" : "False";
                return true;
            }
            default:
                result = null;
                return false;
        }
    }

    // Case-insensitive helpers (PlayMaker Globals don’t expose a direct ignore-case getter)
    HutongGames.PlayMaker.FsmString FindFsmStringIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.StringVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))
                return v;
        return null;
    }
    HutongGames.PlayMaker.FsmInt FindFsmIntIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.IntVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))
                return v;
        return null;
    }
    HutongGames.PlayMaker.FsmFloat FindFsmFloatIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.FloatVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))
                return v;
        return null;
    }
    HutongGames.PlayMaker.FsmBool FindFsmBoolIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.BoolVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase))
                return v;
        return null;
    }

    void LogOnce(HashSet<string> set, string key, string msg)
    {
        if (set.Add(key))
            Debug.LogWarning(msg);
    }
}