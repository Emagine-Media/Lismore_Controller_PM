using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HutongGames.PlayMaker;

namespace PlayMakerCustom
{
    [HutongGames.PlayMaker.ActionCategory("Custom/Data")]
    [HutongGames.PlayMaker.Tooltip(
        "Dynamic key → string[] JSON config in Application.persistentDataPath.\n" +
        "Use Pairs (Size) for general sync; set Size=0 for disconnect-only.\n" +
        "Hardening: de-dupes all lists, clamps Active ⊆ UUIDs, and can remove a specific disconnectUuid from Active."
    )]
    public class PM_Clients_Config : FsmStateAction
    {
        [HutongGames.PlayMaker.RequiredField]
        [HutongGames.PlayMaker.Tooltip("Config file name, e.g., headsets.config (saved in Application.persistentDataPath).")]
        public FsmString fileName;

        // Pairs: optional – can be Size=0 when using only disconnectUuid.
        [CompoundArray("Pairs", "Key", "Value")]
        public FsmString[] Keys;

        [ArrayEditor(VariableType.String)]
        public FsmArray[] Values;

        [HutongGames.PlayMaker.Tooltip("If true, write the merged result back to disk.")]
        public FsmBool doSave;

        [HutongGames.PlayMaker.Tooltip("If true, logs details to the Console.")]
        public FsmBool debugLog;

        // Hardening / behavior knobs
        [HutongGames.PlayMaker.Tooltip("Name of the UUIDs list key in JSON.")]
        public FsmString uuidsKeyName;

        [HutongGames.PlayMaker.Tooltip("Name of the Active list key in JSON.")]
        public FsmString activeKeyName;

        [HutongGames.PlayMaker.Tooltip("Optional: when set, this UUID will be removed from Active (but kept in UUIDs).")]
        public FsmString disconnectUuid;

        // ---- JSON model ----
        [Serializable]
        private class Entry
        {
            public string Key;
            public List<string> Values = new List<string>();
        }

        [Serializable]
        private class GenericConfig
        {
            public List<Entry> Entries = new List<Entry>();
        }

        public override void Reset()
        {
            fileName = "headsets.config";
            doSave = true;
            debugLog = false;

            uuidsKeyName = "UUIDs";
            activeKeyName = "Active";
            disconnectUuid = string.Empty;
        }

        public override void OnEnter()
        {
            int n = Keys != null ? Keys.Length : 0;

            // Enforce paired sizes only if n > 0 (Pairs mode)
            if (n > 0 && (Values == null || Values.Length != n))
            {
                Debug.LogError($"PM_Clients_Config: Pairs not configured correctly. Keys.Length={n}, Values.Length={(Values == null ? -1 : Values.Length)}");
                Finish();
                return;
            }

            // Ensure arrays in pairs are string typed
            if (n > 0)
            {
                for (int i = 0; i < n; i++)
                    if (Values[i] != null && Values[i].ElementType != VariableType.String)
                        Values[i].SetType(VariableType.String);
            }

            var path = Path.Combine(Application.persistentDataPath, SafeFileName(fileName.Value));
            var model = LoadOrCreate(path);

            // 1) If we have Pairs, merge them in
            if (n > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    var keyText = ResolveKeyText(Keys[i]);
                    if (string.IsNullOrWhiteSpace(keyText))
                    {
                        if (debugLog.Value) Debug.LogWarning($"PM_Clients_Config: Skipping pair {i} — empty key.");
                        continue;
                    }

                    var arr = Values[i];
                    if (arr == null)
                    {
                        if (debugLog.Value) Debug.LogWarning($"PM_Clients_Config: Skipping key '{keyText}' — Value array at index {i} is not assigned.");
                        continue;
                    }

                    var entry = GetOrCreateEntry(model, keyText.Trim());
                    entry.Values = ToList(arr); // overwrite from FSM
                }
            }

            // 2) Hardening (always runs): de-dupe, clamp Active to UUIDs, apply disconnectUuid removal
            SanitizeModel(model, uuidsKeyName.Value, activeKeyName.Value, disconnectUuid.Value);

            // 3) Save if requested
            if (doSave.Value)
                Save(path, model);

            // 4) Push saved values back to FSM arrays (only if Pairs > 0)
            if (n > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    var keyText = ResolveKeyText(Keys[i]);
                    if (string.IsNullOrWhiteSpace(keyText)) continue;

                    var arr = Values[i];
                    if (arr == null) continue;

                    var entry = FindEntry(model, keyText.Trim());
                    if (entry != null)
                        FromList(arr, entry.Values);
                }
            }

            if (debugLog.Value)
            {
                Debug.Log($"PM_Clients_Config @ {path}");
                foreach (var e in model.Entries)
                    Debug.Log($"{e.Key}: {string.Join(", ", e.Values)}");
            }

            Finish();
        }

        // ---- Helpers ----

        private static string SafeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "headsets.config";
            foreach (var c in Path.GetInvalidFileNameChars()) input = input.Replace(c, '_');
            return input;
        }

        private static GenericConfig LoadOrCreate(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var loaded = JsonUtility.FromJson<GenericConfig>(json);
                        if (loaded != null && loaded.Entries != null) return loaded;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("PM_Clients_Config: Failed to read config. Starting new. " + e);
            }
            return new GenericConfig { Entries = new List<Entry>() };
        }

        private static void Save(string path, GenericConfig model)
        {
            try
            {
                var json = JsonUtility.ToJson(model, true);
                File.WriteAllText(path, json);
            }
            catch (Exception e)
            {
                Debug.LogError("PM_Clients_Config: Failed to write config. " + e);
            }
        }

        private static Entry FindEntry(GenericConfig model, string key)
        {
            return model.Entries.Find(e => string.Equals(e.Key, key, StringComparison.Ordinal));
        }

        private static Entry GetOrCreateEntry(GenericConfig model, string key)
        {
            var e = FindEntry(model, key);
            if (e == null)
            {
                e = new Entry { Key = key };
                model.Entries.Add(e);
            }
            return e;
        }

        private static List<string> ToList(FsmArray arr)
        {
            var list = new List<string>(arr != null ? arr.Length : 0);
            if (arr == null) return list;
            for (int i = 0; i < arr.Length; i++)
                list.Add(arr.Get(i)?.ToString() ?? "");
            return list;
        }

        private static void FromList(FsmArray arr, List<string> src)
        {
            if (arr == null) return;
            if (arr.ElementType != VariableType.String)
                arr.SetType(VariableType.String);

            arr.Resize(src.Count);
            for (int i = 0; i < src.Count; i++)
                arr.Set(i, src[i] ?? "");
            arr.SaveChanges();
        }

        // NOTE: Keep only one ResolveKeyText. If you see CS0111 again, you have another file defining it too.
        private static string ResolveKeyText(FsmString keyField)
        {
            if (keyField == null) return string.Empty;
            return keyField.Value ?? string.Empty; // literal or variable-backed
        }

        // ---- Hardening ----

        private static void SanitizeModel(GenericConfig model, string uuidsKey, string activeKey, string disconnect)
        {
            // De-dupe all lists
            foreach (var e in model.Entries)
                e.Values = DistinctInOrder(e.Values);

            // Clamp Active to UUIDs
            var uuids = FindEntry(model, string.IsNullOrWhiteSpace(uuidsKey) ? "UUIDs" : uuidsKey);
            var active = FindEntry(model, string.IsNullOrWhiteSpace(activeKey) ? "Active" : activeKey);

            if (active != null)
            {
                if (uuids != null)
                {
                    var known = new HashSet<string>(uuids.Values);
                    var clamped = new List<string>(active.Values.Count);
                    foreach (var a in active.Values)
                        if (!string.IsNullOrEmpty(a) && known.Contains(a))
                            clamped.Add(a);
                    active.Values = clamped;
                }

                // Remove disconnect UUID from Active, keep it in UUIDs
                if (!string.IsNullOrWhiteSpace(disconnect))
                {
                    var pruned = new List<string>(active.Values.Count);
                    foreach (var a in active.Values)
                        if (!string.Equals(a, disconnect, StringComparison.Ordinal))
                            pruned.Add(a);
                    active.Values = pruned;
                }
            }
        }

        private static List<string> DistinctInOrder(List<string> src)
        {
            var seen = new HashSet<string>();
            var outList = new List<string>(src?.Count ?? 0);
            if (src == null) return outList;
            foreach (var s in src)
            {
                var t = s ?? "";
                if (seen.Add(t)) outList.Add(t);
            }
            return outList;
        }
    }
}
