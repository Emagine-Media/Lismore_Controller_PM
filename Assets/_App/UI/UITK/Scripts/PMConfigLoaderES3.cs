using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HutongGames.PlayMaker;

[Serializable]
public class PMConfig
{
    public Dictionary<string, string> Strings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, int> Ints = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, float> Floats = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, bool> Bools = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

    // NEW: explicit list of available languages
    public List<string> Languages = new List<string>();
}

public class PMConfigLoaderES3 : MonoBehaviour
{
    [Header("File")]
    [UnityEngine.Tooltip("File name saved to Application.persistentDataPath")]
    public string fileName = "pm_config.json";

    [Header("Behavior")]
    [UnityEngine.Tooltip("Create config file from current PlayMaker Globals if it doesn't exist.")]
    public bool createIfMissingFromGlobals = true;

    [UnityEngine.Tooltip("Load file on Start and apply values to PlayMaker Globals.")]
    public bool applyOnStart = true;

    [UnityEngine.Tooltip("Log actions to the Console.")]
    public bool logActions = true;

    string FullPath => Path.Combine(Application.persistentDataPath, fileName);

    void Start()
    {
        try
        {
            // 1) Create file from current Globals if missing
            if (!File.Exists(FullPath))
            {
                if (createIfMissingFromGlobals)
                {
                    var cfg = BuildFromGlobals();
                    SaveConfig(cfg);
                    if (logActions) Debug.Log($"[PMConfigLoaderES3] Created default config at:\n{FullPath}");
                }
                else
                {
                    if (logActions) Debug.LogWarning($"[PMConfigLoaderES3] Config not found at {FullPath} and creation is disabled.");
                }
            }

            // 2) Apply file to Globals
            if (applyOnStart && File.Exists(FullPath))
            {
                var loaded = LoadConfig();
                ApplyToGlobals(loaded);
                if (logActions) Debug.Log($"[PMConfigLoaderES3] Applied config from:\n{FullPath}");
            }

            if (logActions) Debug.Log($"[PMConfigLoaderES3] Config folder:\n{Application.persistentDataPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PMConfigLoaderES3] Error: {ex.Message}\n{ex}");
        }
    }

    PMConfig BuildFromGlobals()
    {
        var cfg = new PMConfig();
        var vars = PlayMakerGlobals.Instance.Variables;

        foreach (var v in vars.StringVariables) if (v != null) cfg.Strings[v.Name] = v.Value ?? "";
        foreach (var v in vars.IntVariables)    if (v != null) cfg.Ints[v.Name]    = v.Value;
        foreach (var v in vars.FloatVariables)  if (v != null) cfg.Floats[v.Name]  = v.Value;
        foreach (var v in vars.BoolVariables)   if (v != null) cfg.Bools[v.Name]   = v.Value;

        // Set default available languages
        cfg.Languages.AddRange(new[] { "EN", "FR", "ES", "IL" });

        return cfg;
    }

    void ApplyToGlobals(PMConfig cfg)
    {
        var vars = PlayMakerGlobals.Instance.Variables;

        foreach (var kv in cfg.Strings)
        {
            var v = vars.GetFsmString(kv.Key) ?? FindFsmStringIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else if (logActions) Debug.LogWarning($"[PMConfigLoaderES3] String global '{kv.Key}' not found.");
        }
        foreach (var kv in cfg.Ints)
        {
            var v = vars.GetFsmInt(kv.Key) ?? FindFsmIntIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else if (logActions) Debug.LogWarning($"[PMConfigLoaderES3] Int global '{kv.Key}' not found.");
        }
        foreach (var kv in cfg.Floats)
        {
            var v = vars.GetFsmFloat(kv.Key) ?? FindFsmFloatIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else if (logActions) Debug.LogWarning($"[PMConfigLoaderES3] Float global '{kv.Key}' not found.");
        }
        foreach (var kv in cfg.Bools)
        {
            var v = vars.GetFsmBool(kv.Key) ?? FindFsmBoolIgnoreCase(kv.Key);
            if (v != null) v.Value = kv.Value;
            else if (logActions) Debug.LogWarning($"[PMConfigLoaderES3] Bool global '{kv.Key}' not found.");
        }

        // Optional: expose Languages list to a Global String so Playmaker FSMs can use it
        var langsCsv = string.Join(",", cfg.Languages);
        var langsGlobal = vars.GetFsmString("AvailableLanguages");
        if (langsGlobal != null) langsGlobal.Value = langsCsv;
    }

    void SaveConfig(PMConfig cfg)
    {
        // ES3 writes JSON by default; key name "config" keeps it tidy.
        ES3.Save<PMConfig>("config", cfg, FullPath);
    }

    PMConfig LoadConfig()
    {
        if (!File.Exists(FullPath))
            throw new FileNotFoundException("Config file not found", FullPath);

        if (!ES3.KeyExists("config", FullPath))
            throw new Exception($"Key 'config' not found in {FullPath}");

        return ES3.Load<PMConfig>("config", FullPath);
    }

    // ---- case-insensitive helpers ----
    HutongGames.PlayMaker.FsmString FindFsmStringIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.StringVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
    HutongGames.PlayMaker.FsmInt FindFsmIntIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.IntVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
    HutongGames.PlayMaker.FsmFloat FindFsmFloatIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.FloatVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
    HutongGames.PlayMaker.FsmBool FindFsmBoolIgnoreCase(string name)
    {
        foreach (var v in PlayMakerGlobals.Instance.Variables.BoolVariables)
            if (string.Equals(v.Name, name, StringComparison.OrdinalIgnoreCase)) return v;
        return null;
    }
}