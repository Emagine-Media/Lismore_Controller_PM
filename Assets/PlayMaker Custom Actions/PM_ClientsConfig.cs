using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class PM_ClientsConfig
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Public API (what you call from PlayMaker via Call Method)
    // ─────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Mark a headset as connected: ensures UUID + Name are present in the master list,
    /// adds the UUID to Active (does NOT touch other actives).
    /// </summary>
    public static void MarkConnected(string fileName, string uuid, string name = null, string famId = null)
        => UpsertHeadset(fileName, uuid, name, famId, isConnected: true);

    /// <summary>
    /// Mark a headset as disconnected: removes the UUID from Active, but keeps it in UUIDs/Names.
    /// </summary>
    public static void MarkDisconnected(string fileName, string uuid)
        => UpsertHeadset(fileName, uuid, name: null, famId: null, isConnected: false);

    // ─────────────────────────────────────────────────────────────────────────────
    // Implementation
    // ─────────────────────────────────────────────────────────────────────────────

    [Serializable] private class Entry { public string Key; public List<string> Values = new(); }
    [Serializable] private class Root { public List<Entry> Entries = new(); }

    private static void UpsertHeadset(string fileName, string uuid, string name, string famId, bool isConnected)
    {
        uuid = (uuid ?? "").Trim();
        if (string.IsNullOrEmpty(uuid))
        {
            Debug.LogWarning("PM_ClientsConfig.UpsertHeadset: empty uuid");
            return;
        }

        var path = Path.Combine(Application.persistentDataPath, SafeFileName(string.IsNullOrWhiteSpace(fileName) ? "headsets.config" : fileName));
        var root = Load(path) ?? new Root { Entries = new List<Entry>() };

        // get/create required entries
        var eUUIDs  = GetOrAddEntry(root, "UUIDs");
        var eNames  = GetOrAddEntry(root, "Names");
        var eFamIds = GetOrAddEntry(root, "FamIds");
        var eActive = GetOrAddEntry(root, "Active");

        // normalize nulls
        eUUIDs.Values  ??= new List<string>();
        eNames.Values  ??= new List<string>();
        eFamIds.Values ??= new List<string>();
        eActive.Values ??= new List<string>();

        // ensure UUID exists in master list; align Names/FamIds to same length
        int idx = eUUIDs.Values.FindIndex(s => string.Equals(s ?? "", uuid, StringComparison.Ordinal));
        if (idx < 0)
        {
            eUUIDs.Values.Add(uuid);
            eNames.Values.Add(string.IsNullOrWhiteSpace(name) ? uuid : name.Trim());
            eFamIds.Values.Add(famId?.Trim() ?? "");
            idx = eUUIDs.Values.Count - 1;
        }
        else
        {
            // update name if provided and changed
            if (!string.IsNullOrWhiteSpace(name))
            {
                if (idx < eNames.Values.Count) eNames.Values[idx] = name.Trim();
                else EnsureLength(eNames.Values, eUUIDs.Values.Count, "");
            }
            // update famId if provided
            if (!string.IsNullOrWhiteSpace(famId))
            {
                if (idx < eFamIds.Values.Count) eFamIds.Values[idx] = famId.Trim();
                else EnsureLength(eFamIds.Values, eUUIDs.Values.Count, "");
            }
        }

        // keep arrays aligned to UUID count (pad with empty if needed)
        EnsureLength(eNames.Values,  eUUIDs.Values.Count, "");
        EnsureLength(eFamIds.Values, eUUIDs.Values.Count, "");

        // toggle Active set for THIS uuid only
        if (isConnected)
        {
            if (!eActive.Values.Contains(uuid, StringComparer.Ordinal))
                eActive.Values.Add(uuid);
        }
        else
        {
            // remove all occurrences of uuid from Active
            for (int i = eActive.Values.Count - 1; i >= 0; i--)
            {
                if (string.Equals(eActive.Values[i] ?? "", uuid, StringComparison.Ordinal))
                    eActive.Values.RemoveAt(i);
            }
        }

        // de-duplicate Active while preserving order, and ensure Active ⊆ UUIDs
        var known = new HashSet<string>(eUUIDs.Values.Where(s => !string.IsNullOrEmpty(s)), StringComparer.Ordinal);
        eActive.Values = DistinctInOrder(eActive.Values).Where(known.Contains).ToList();

        Save(path, root);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // JSON helpers (Unity JsonUtility)
    // ─────────────────────────────────────────────────────────────────────────────

    private static Root Load(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return null;
            var root = JsonUtility.FromJson<Root>(json);
            return root ?? new Root { Entries = new List<Entry>() };
        }
        catch (Exception e)
        {
            Debug.LogError($"PM_ClientsConfig.Load error: {e}");
            return null;
        }
    }

    private static void Save(string path, Root root)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonUtility.ToJson(root);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"PM_ClientsConfig.Save error: {e}");
        }
    }

    private static Entry GetOrAddEntry(Root root, string key)
    {
        var e = root.Entries.FirstOrDefault(x => string.Equals(x?.Key, key, StringComparison.Ordinal));
        if (e != null) return e;
        e = new Entry { Key = key, Values = new List<string>() };
        root.Entries.Add(e);
        return e;
    }

    private static void EnsureLength(List<string> list, int len, string padValue)
    {
        while (list.Count < len) list.Add(padValue);
        if (list.Count > len) list.RemoveRange(len, list.Count - len);
    }

    private static List<string> DistinctInOrder(List<string> src)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var outList = new List<string>(src?.Count ?? 0);
        if (src == null) return outList;
        foreach (var s in src)
        {
            var v = s ?? "";
            if (seen.Add(v)) outList.Add(v);
        }
        return outList;
    }

    private static string SafeFileName(string input)
    {
        input ??= "headsets.config";
        foreach (var c in Path.GetInvalidFileNameChars()) input = input.Replace(c, '_');
        return input;
    }
}