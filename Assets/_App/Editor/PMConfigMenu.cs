#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using HutongGames.PlayMaker;

// Uses PMConfig type you already defined in PMConfigLoaderES3.cs
// Menus appear under: Tools → PlayMaker Config → ...

public static class PMConfigMenu
{
    const string DefaultFileName = "pm_config.json";

    static string ResolveFileName()
    {
        // If a PMConfigLoaderES3 exists in the scene, use its fileName.
        var loader = Object.FindObjectOfType<PMConfigLoaderES3>();
        if (loader != null && !string.IsNullOrWhiteSpace(loader.fileName))
            return loader.fileName;
        return DefaultFileName;
    }

    static string GetFullPath()
    {
        return Path.Combine(Application.persistentDataPath, ResolveFileName());
    }

    [MenuItem("Tools/PlayMaker Config/Open Config Folder")]
    public static void OpenConfigFolder()
    {
        EnsureFolder();
        var path = GetFullPath();
        if (File.Exists(path))
            EditorUtility.RevealInFinder(path);
        else
            EditorUtility.RevealInFinder(Path.GetDirectoryName(path));
        Debug.Log($"[PMConfigMenu] Persistent path:\n{Application.persistentDataPath}\nFile: {path}");
    }

    [MenuItem("Tools/PlayMaker Config/Edit pm_config.json")]
    public static void EditConfigFile()
    {
        EnsureFileExists();
        var path = GetFullPath();
        EditorUtility.OpenWithDefaultApp(path);
        Debug.Log($"[PMConfigMenu] Editing: {path}");
    }

    [MenuItem("Tools/PlayMaker Config/Save Globals → Config")]
    public static void SaveGlobalsToConfig()
    {
        EnsureFolder();
        var cfg = BuildFromGlobals();
        ES3.Save<PMConfig>("config", cfg, GetFullPath());
        Debug.Log($"[PMConfigMenu] Saved Globals → {GetFullPath()}");
        EditorUtility.RevealInFinder(GetFullPath());
    }

    [MenuItem("Tools/PlayMaker Config/Load Config → Globals")]
    public static void LoadConfigToGlobals()
    {
        var path = GetFullPath();
        if (!File.Exists(path) || !ES3.KeyExists("config", path))
        {
            EditorUtility.DisplayDialog(
                "Config Missing",
                $"Config not found or missing key 'config':\n{path}\n\nUse 'Save Globals → Config' first.",
                "OK");
            return;
        }

        var cfg = ES3.Load<PMConfig>("config", path);
        ApplyToGlobals(cfg);
        Debug.Log($"[PMConfigMenu] Applied {path} → PlayMaker Globals");
    }

    // ---- helpers ----

    static void EnsureFolder()
    {
        var dir = Path.GetDirectoryName(GetFullPath());
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
    }

    static void EnsureFileExists()
    {
        EnsureFolder();
        var path = GetFullPath();
        if (!File.Exists(path))
        {
            var cfg = BuildFromGlobals();
            ES3.Save<PMConfig>("config", cfg, path);
            AssetDatabase.Refresh();
            Debug.Log($"[PMConfigMenu] Created new config: {path}");
        }
    }

    static PMConfig BuildFromGlobals()
    {
        var cfg = new PMConfig();
        var vars = PlayMakerGlobals.Instance.Variables;

        foreach (var v in vars.StringVariables) if (v != null) cfg.Strings[v.Name] = v.Value ?? "";
        foreach (var v in vars.IntVariables)    if (v != null) cfg.Ints[v.Name]    = v.Value;
        foreach (var v in vars.FloatVariables)  if (v != null) cfg.Floats[v.Name]  = v.Value;
        foreach (var v in vars.BoolVariables)   if (v != null) cfg.Bools[v.Name]   = v.Value;

        return cfg;
    }

    static void ApplyToGlobals(PMConfig cfg)
    {
        var vars = PlayMakerGlobals.Instance.Variables;

        foreach (var kv in cfg.Strings)
        {
            var v = vars.GetFsmString(kv.Key) ?? FindFsmStringIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else Debug.LogWarning($"[PMConfigMenu] String global '{kv.Key}' not found.");
        }
        foreach (var kv in cfg.Ints)
        {
            var v = vars.GetFsmInt(kv.Key) ?? FindFsmIntIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else Debug.LogWarning($"[PMConfigMenu] Int global '{kv.Key}' not found.");
        }
        foreach (var kv in cfg.Floats)
        {
            var v = vars.GetFsmFloat(kv.Key) ?? FindFsmFloatIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else Debug.LogWarning($"[PMConfigMenu] Float global '{kv.Key}' not found.");
        }
        foreach (var kv in cfg.Bools)
        {
            var v = vars.GetFsmBool(kv.Key) ?? FindFsmBoolIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else Debug.LogWarning($"[PMConfigMenu] Bool global '{kv.Key}' not found.");
        }
    }

    // case-insensitive lookups
    static HutongGames.PlayMaker.FsmString FindFsmStringIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.StringVariables)
            if (string.Equals(v.Name, name, System.StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
    static HutongGames.PlayMaker.FsmInt FindFsmIntIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.IntVariables)
            if (string.Equals(v.Name, name, System.StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
    static HutongGames.PlayMaker.FsmFloat FindFsmFloatIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.FloatVariables)
            if (string.Equals(v.Name, name, System.StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
    static HutongGames.PlayMaker.FsmBool FindFsmBoolIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.BoolVariables)
            if (string.Equals(v.Name, name, System.StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
}
#endif