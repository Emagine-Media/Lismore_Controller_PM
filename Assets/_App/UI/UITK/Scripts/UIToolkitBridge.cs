using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class UIToolkitBridge : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ─────────────────────────────────────────────────────────────────────────────
    [Header("References")]
    public UIDocument uiDocument;

    [Header("UI Names")]
    [Tooltip("Q() name of the container that holds the client cards.")]
    public string clientsListContainerName = "clientsList";
    [Tooltip("Q() name of the label that displays the number of ACTIVE clients.")]
    public string activeCountLabelName = "lblClientCount";

    [Header("Icons")]
    public Sprite iconPlay;
    public Sprite iconPause;
    public Sprite iconStop;

    [Header("Global Languages (optional)")]
    [Tooltip("Read from Application.persistentDataPath. Example: pm_config.json")]
    public string pmConfigFileName = "pm_config.json";
    [Tooltip("Key inside pm_config.json (kept for clarity; loader reads config.value.Languages).")]
    public string pmConfigLanguagesKey = "languages";

    [Header("Config")]
    [Tooltip("Default config filename in Application.persistentDataPath.")]
    public string defaultConfigFileName = "headsets.config";

    // ─────────────────────────────────────────────────────────────────────────────
    // Private state
    // ─────────────────────────────────────────────────────────────────────────────
    private VisualElement _root;

    private readonly HashSet<string> _buttonClicks = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _fieldChanges = new(StringComparer.Ordinal);

    // Per-UUID language cache (if you ever decide to store per-device langs)
    private readonly Dictionary<string, string> _langsCsvByUuid = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _defaultLangByUuid = new(StringComparer.OrdinalIgnoreCase);

    // Track current active UUIDs to update the active count label
    private readonly HashSet<string> _connectedUuids = new(StringComparer.Ordinal);

    // Global fallback languages read from pm_config.json
    private string _globalLanguagesCsv = "";

    // ─────────────────────────────────────────────────────────────────────────────
    // Unity
    // ─────────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        _root = uiDocument != null ? uiDocument.rootVisualElement : null;

        LoadGlobalLanguagesFromPmConfig();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // PUBLIC API — ALPHABETICAL (A → Z)
    // ─────────────────────────────────────────────────────────────────────────────

    // A
    public void AddClass(string elementName, string className) => GetElement(elementName)?.AddToClassList(className);

    public void AddClientCard(string containerName, string displayIndex)
        => AddOrUpdateClientCard(containerName, displayIndex, $"Headset {displayIndex}", null, null);

    /// <summary>
    /// Creates the visual row if missing and wires up controls (neutral by default).
    /// Use MarkClientConnected/MarkClientDisconnected to set state.
    /// </summary>
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
            card.AddToClassList("mt-12");

            // LEFT
            var hstack = new VisualElement(); hstack.AddToClassList("hstack");
            var icon = new VisualElement { name = $"issueIcon_{uuid}" };
            icon.AddToClassList("issue-icon");

            var copy = new VisualElement(); copy.AddToClassList("issue-copy"); copy.AddToClassList("ml-12");
            var lblTitle = new Label("") { name = $"lblClientTitle_{uuid}" }; lblTitle.AddToClassList("row-title");
            var lblSub = new Label("") { name = $"lblClientSub_{uuid}" }; lblSub.AddToClassList("row-sub"); lblSub.AddToClassList("mt-2");
            copy.Add(lblTitle); copy.Add(lblSub);
            hstack.Add(icon); hstack.Add(copy);

            // SPACER
            var spacer = new VisualElement(); spacer.AddToClassList("flex-spacer");

            // RIGHT
            var right = new VisualElement(); right.AddToClassList("hstack"); right.AddToClassList("buttons-right");
            var btnGroup = new VisualElement { name = $"btnGroup_{uuid}" }; btnGroup.AddToClassList("btn-group");

            var btnPlay = new Button() { name = $"btnPlay_{uuid}" }; btnPlay.AddToClassList("icon-btn"); btnPlay.AddToClassList("play");
            var btnStop = new Button() { name = $"btnStop_{uuid}" }; btnStop.AddToClassList("icon-btn"); btnStop.AddToClassList("stop");

            ApplyIcon(btnPlay, "play");
            ApplyIcon(btnStop, "stop");

            var idPlay = btnPlay.name; btnPlay.clicked += () => _buttonClicks.Add(idPlay);
            var idStop = btnStop.name; btnStop.clicked += () => _buttonClicks.Add(idStop);

            btnGroup.Add(btnPlay); btnGroup.Add(btnStop);

            var langGroup = new VisualElement { name = $"langGroup_{uuid}" }; langGroup.AddToClassList("btn-group");

            var battery = new Label("85%") { name = $"lblBattery_{uuid}" }; battery.AddToClassList("battery"); battery.AddToClassList("ml-12");

            right.Add(btnGroup);
            right.Add(new VisualElement() { name = $"groupSpacer_{uuid}", pickingMode = PickingMode.Ignore });
            right.Add(langGroup);
            right.Add(battery);

            // Assemble
            card.Add(hstack);
            card.Add(spacer);
            card.Add(right);
            list.Add(card);

            // Default: controls hidden/disabled until state is set
            SetClientButtonsEnabled(uuid, false);
            SetClientCardEnabled(uuid, false);
            SetVisible($"btnGroup_{uuid}", false);
            SetVisible($"langGroup_{uuid}", false);
        }

        var title = card.Q<Label>($"lblClientTitle_{uuid}");
        if (title != null) title.text = string.IsNullOrEmpty(displayName) ? uuid : displayName;

        if (!string.IsNullOrEmpty(languagesCsv))
            BuildLanguageButtonsForCard(uuid, languagesCsv, defaultLang);
    }

    /// <summary>
    /// Clamps UI strictly to the provided *live* UUIDs (does NOT write config).
    /// </summary>
    public void ApplyLiveConnections(IEnumerable<string> liveUuids, string containerName = null, string configFileName = "headsets.config")
    {
        containerName = string.IsNullOrEmpty(containerName) ? clientsListContainerName : containerName;
        var list = GetElement(containerName);
        if (list == null) return;

        var live = new HashSet<string>(StringComparer.Ordinal);
        if (liveUuids != null)
        {
            foreach (var u in liveUuids)
            {
                var t = (u ?? "").Trim();
                if (!string.IsNullOrEmpty(t)) live.Add(t);
            }
        }

        var data = LoadConfig(configFileName);
        if (data != null)
        {
            var nameByUuid = new Dictionary<string, string>(StringComparer.Ordinal);
            int pairCount = Mathf.Min(data.UUIDs.Count, data.Names.Count);
            for (int i = 0; i < pairCount; i++)
            {
                var u = (data.UUIDs[i] ?? "").Trim();
                var n = (data.Names[i] ?? "").Trim();
                if (!string.IsNullOrEmpty(u) && !nameByUuid.ContainsKey(u)) nameByUuid.Add(u, n);
            }

            foreach (var raw in data.UUIDs)
            {
                var uuid = (raw ?? "").Trim();
                if (string.IsNullOrEmpty(uuid)) continue;
                var displayName = nameByUuid.TryGetValue(uuid, out var nm) && !string.IsNullOrEmpty(nm) ? nm : uuid;
                AddOrUpdateClientCard(containerName, uuid, displayName, null, null);
            }
        }

        foreach (var child in list.Children())
        {
            var n = child.name ?? "";
            const string prefix = "clientCard_";
            if (!n.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var uuid = n.Substring(prefix.Length);
            if (!live.Contains(uuid))
            {
                var title = _root?.Q<Label>($"lblClientTitle_{uuid}");
                var nameText = title != null ? title.text : uuid;
                MarkClientDisconnected(uuid, nameText);
            }
        }

        foreach (var uuid in live)
        {
            var title = _root?.Q<Label>($"lblClientTitle_{uuid}");
            var nameText = title != null ? title.text : uuid;
            MarkClientConnected(uuid, nameText);
        }

        _connectedUuids.Clear();
        foreach (var id in live) _connectedUuids.Add(id);
        UpdateActiveCountLabelFromSet();
    }

    // B
    public void BindButton(string elementName)
    {
        var btn = GetButton(elementName);
        if (btn == null) return;
        var id = elementName;
        btn.clicked += () => _buttonClicks.Add(id);
    }

    public void BindDropdown(string elementName)
    {
        var dd = GetDropdown(elementName);
        if (dd == null) return;
        dd.RegisterValueChangedCallback(evt => _fieldChanges[elementName] = evt.newValue);
    }

    public void BindTextField(string elementName)
    {
        var tf = GetTextField(elementName);
        if (tf == null) return;
        tf.RegisterValueChangedCallback(evt => _fieldChanges[elementName] = evt.newValue);
    }

    public void BuildLanguageButtonsForCard(string uuid, string languagesCsv, string defaultLang)
    {
        _langsCsvByUuid[uuid] = languagesCsv ?? "";
        _defaultLangByUuid[uuid] = defaultLang ?? "";

        var langGroup = _root?.Q<VisualElement>($"langGroup_{uuid}");
        if (langGroup == null) return;
        langGroup.Clear();

        var tokens = (languagesCsv ?? "")
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s));
        foreach (var code in tokens)
        {
            var btn = new Button { name = $"btnLang_{uuid}_{code}", text = code };
            btn.AddToClassList("lang-btn");
            var id = btn.name; btn.clicked += () => _buttonClicks.Add(id);
            langGroup.Add(btn);
        }

        SelectClientLanguage(clientsListContainerName, uuid, defaultLang);
    }

    // C
    public void ClearBackground(string elementName)
    {
        var btn = GetButton(elementName);
        if (btn != null) btn.style.backgroundImage = new StyleBackground();
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

    public string ConsumeFieldChange(string elementName)
    {
        if (_fieldChanges.TryGetValue(elementName, out var val))
        {
            _fieldChanges.Remove(elementName);
            return val;
        }
        return null;
    }

    // E
    public void EnsureClientCards(string containerName, int targetCount)
    {
        var root = GetElement(containerName);
        if (root == null) return;
        while (root.childCount > targetCount) root.RemoveAt(root.childCount - 1);
        while (root.childCount < targetCount) AddClientCard(containerName, (root.childCount + 1).ToString());
    }

    public void EnsureDropdownValue(string elementName, string desiredValue)
    {
        var dd = GetDropdown(elementName);
        if (dd == null) return;
        if (dd.choices == null || dd.choices.Count == 0) return;
        if (dd.choices.Contains(desiredValue)) dd.value = desiredValue;
        else if (!dd.choices.Contains(dd.value)) dd.value = dd.choices[0];
    }

    // G
    public string GetAllClientUUIDsCsv(string containerName)
    {
        var list = GetElement(containerName);
        if (list == null) return string.Empty;

        var uuids = new List<string>();
        foreach (var child in list.Children())
        {
            var name = child.name ?? "";
            const string prefix = "clientCard_";
            if (name.StartsWith(prefix, StringComparison.Ordinal))
                uuids.Add(name.Substring(prefix.Length));
        }
        return string.Join(",", uuids);
    }

    public Button GetButton(string name) => _root?.Q<Button>(name);
    public int GetChildCount(string containerName) => GetElement(containerName)?.childCount ?? 0;
    public DropdownField GetDropdown(string name) => _root?.Q<DropdownField>(name);
    public VisualElement GetElement(string name) => _root?.Q<VisualElement>(name);
    public Label GetLabel(string name) => _root?.Q<Label>(name);
    public VisualElement GetRoot() => _root;
    public TextElement GetTextElement(string name) => _root?.Q<TextElement>(name);
    public TextField GetTextField(string name) => _root?.Q<TextField>(name);

    private void GuardLanguageContainers()
{
    void Stop<T>(VisualElement ve) where T : EventBase<T>, new()
    {
        ve?.RegisterCallback<T>(evt => evt.StopPropagation());
    }

    // Global bar
    var global = GetElement("languagesBar");
    if (global != null)
    {
        Stop<PointerDownEvent>(global);
        Stop<PointerUpEvent>(global);
        Stop<ClickEvent>(global);
    }

    // Per-card groups: guard dynamically when created
    // Call this right after you create langGroup_{uuid}
    var q = _root.Query<VisualElement>(className: "btn-group"); // or by name prefix if you prefer
}

    // I
    public void InitTopIcons()
    {
        ApplyIcon(GetButton("btnPlayAll"), "play");
        ApplyIcon(GetButton("btnPauseAll"), "pause");
        ApplyIcon(GetButton("btnStopAll"), "stop");
    }

    // M
    /// <summary>
    /// Connect ONLY this UUID in UI and append it to Active in the config (leave others untouched).
    /// </summary>
    public void MarkClientConnected(string uuid, string displayName = null, float batteryPercent = -1f)
        => MarkClientConnected(uuid, displayName, null, batteryPercent);

    /// <summary>
    /// FamId-aware version (writes FamIds when provided).
    /// </summary>
    public void MarkClientConnected(string uuid, string displayName, string famId, float batteryPercent = -1f)
    {
        uuid = (uuid ?? "").Trim();
        if (string.IsNullOrEmpty(uuid)) return;

        // Ensure card exists
        AddOrUpdateClientCard(clientsListContainerName, uuid, displayName ?? uuid, null, null);

        // Visual state
        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        card?.RemoveFromClassList("is-disconnected");
        card?.AddToClassList("is-active");

        var icon = _root?.Q<VisualElement>($"issueIcon_{uuid}");
        icon?.RemoveFromClassList("disconnected");
        icon?.AddToClassList("active");

        var sub = _root?.Q<Label>($"lblClientSub_{uuid}");
        if (sub != null) { sub.text = "Connected"; sub.RemoveFromClassList("disconnected"); sub.AddToClassList("active"); }

        var titleSelf = _root?.Q<Label>($"lblClientTitle_{uuid}");
        if (titleSelf != null && !string.IsNullOrEmpty(displayName)) titleSelf.text = displayName;

        var bat = _root?.Q<Label>($"lblBattery_{uuid}");
        if (bat != null && batteryPercent >= 0f) bat.text = $"{Mathf.RoundToInt(batteryPercent)}%";

        SetClientCardEnabled(uuid, true);
        SetClientButtonsEnabled(uuid, true);
        SetVisible($"btnGroup_{uuid}", true);

        // Language bar (global pm_config.json preferred)
        if (string.IsNullOrWhiteSpace(_globalLanguagesCsv))
            LoadGlobalLanguagesFromPmConfig();

        bool built = false;
        if (!string.IsNullOrWhiteSpace(_globalLanguagesCsv))
        {
            RebuildLanguageButtonsFromCSV($"langGroup_{uuid}", _globalLanguagesCsv);
            built = true;
        }
        else if (_langsCsvByUuid.TryGetValue(uuid, out var cachedCsv) && !string.IsNullOrWhiteSpace(cachedCsv))
        {
            RebuildLanguageButtonsFromCSV($"langGroup_{uuid}", cachedCsv);
            built = true;
        }
        SetVisible($"langGroup_{uuid}", built);

        // FILE: append/merge UUID, Name, FamId, Active
        var raw = LoadRaw(defaultConfigFileName);
        var eUUIDs  = GetOrAdd(raw, "UUIDs");
        var eNames  = GetOrAdd(raw, "Names");
        var eFamIds = GetOrAdd(raw, "FamIds");
        var eActive = GetOrAdd(raw, "Active");

        eUUIDs.Values  ??= new List<string>();
        eNames.Values  ??= new List<string>();
        eFamIds.Values ??= new List<string>();
        eActive.Values ??= new List<string>();

        int idx = eUUIDs.Values.FindIndex(s => string.Equals(s ?? "", uuid, StringComparison.Ordinal));
        if (idx < 0)
        {
            eUUIDs.Values.Add(uuid);
            eNames.Values.Add(string.IsNullOrWhiteSpace(displayName) ? uuid : displayName.Trim());
            eFamIds.Values.Add(string.IsNullOrWhiteSpace(famId) ? "" : famId.Trim());
            idx = eUUIDs.Values.Count - 1;
        }
        else
        {
            EnsureLen(eNames.Values,  eUUIDs.Values.Count, "");
            EnsureLen(eFamIds.Values, eUUIDs.Values.Count, "");
            if (!string.IsNullOrWhiteSpace(displayName)) eNames.Values[idx] = displayName.Trim();
            if (!string.IsNullOrWhiteSpace(famId))       eFamIds.Values[idx] = famId.Trim();
        }

        // Active: add this uuid, keep unique and subset of UUIDs
        if (!eActive.Values.Contains(uuid, StringComparer.Ordinal))
            eActive.Values.Add(uuid);

        var known = new HashSet<string>(eUUIDs.Values.Where(s => !string.IsNullOrEmpty(s)), StringComparer.Ordinal);
        var seen  = new HashSet<string>(StringComparer.Ordinal);
        var compact = new List<string>();
        foreach (var v in eActive.Values)
        {
            var t = v ?? "";
            if (known.Contains(t) && seen.Add(t)) compact.Add(t);
        }
        eActive.Values = compact;

        SaveRaw(defaultConfigFileName, raw);

        _connectedUuids.Add(uuid);
        UpdateActiveCountLabelFromSet();
        SortClientCardsByStatusAndFamId(clientsListContainerName, defaultConfigFileName);
    }

    /// <summary>
    /// Same as MarkClientConnected, but also persists a FamId for this UUID.
    /// Use this when you have the familiar ID from GraphQL.
    /// </summary>
    public void MarkClientConnectedWithFamId(string uuid, string displayName, string famId, float batteryPercent = -1f)
    {
        MarkClientConnected(uuid, displayName, famId, batteryPercent);
        // (already writes FamId inside MarkClientConnected)
    }

    /// <summary>
    /// Disconnect ONLY this UUID in UI and remove it from Active in the config (keep in history lists).
    /// </summary>
    public void MarkClientDisconnected(string uuid, string displayName = null)
    {
        uuid = (uuid ?? "").Trim();
        if (string.IsNullOrEmpty(uuid)) return;

        AddOrUpdateClientCard(clientsListContainerName, uuid, displayName ?? uuid, null, null);

        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        card?.RemoveFromClassList("is-active");
        card?.AddToClassList("is-disconnected");

        var icon = _root?.Q<VisualElement>($"issueIcon_{uuid}");
        icon?.RemoveFromClassList("active");
        icon?.AddToClassList("disconnected");

        var sub = _root?.Q<Label>($"lblClientSub_{uuid}");
        if (sub != null) { sub.text = "Disconnected"; sub.RemoveFromClassList("active"); sub.AddToClassList("disconnected"); }

        var title = _root?.Q<Label>($"lblClientTitle_{uuid}");
        if (title != null && !string.IsNullOrEmpty(displayName)) title.text = displayName;

        var bat = _root?.Q<Label>($"lblBattery_{uuid}");
        if (bat != null) bat.text = "0%";

        SetClientButtonsEnabled(uuid, false);
        SetClientCardEnabled(uuid, false);

        // Hide & clear language buttons on disconnect
        var langGroup = _root?.Q<VisualElement>($"langGroup_{uuid}");
        if (langGroup != null) langGroup.Clear();
        SetVisible($"langGroup_{uuid}", false);
        SetVisible($"btnGroup_{uuid}", false);

        _connectedUuids.Remove(uuid);
        UpdateActiveCountLabelFromSet();

        // FILE: remove uuid from Active, keep it in UUIDs/Names/FamIds
        var raw = LoadRaw(defaultConfigFileName);
        var eUUIDs  = GetOrAdd(raw, "UUIDs");
        var eNames  = GetOrAdd(raw, "Names");
        var eFamIds = GetOrAdd(raw, "FamIds");
        var eActive = GetOrAdd(raw, "Active");

        eUUIDs.Values  ??= new List<string>();
        eNames.Values  ??= new List<string>();
        eFamIds.Values ??= new List<string>();
        eActive.Values ??= new List<string>();

        if (!eUUIDs.Values.Contains(uuid, StringComparer.Ordinal))
        {
            eUUIDs.Values.Add(uuid);
            eNames.Values.Add(displayName ?? uuid);
            eFamIds.Values.Add("");
        }
        EnsureLen(eNames.Values,  eUUIDs.Values.Count, "");
        EnsureLen(eFamIds.Values, eUUIDs.Values.Count, "");

        for (int i = eActive.Values.Count - 1; i >= 0; i--)
            if (string.Equals(eActive.Values[i] ?? "", uuid, StringComparison.Ordinal))
                eActive.Values.RemoveAt(i);

        SaveRaw(defaultConfigFileName, raw);

        SortClientCardsByStatusAndFamId(clientsListContainerName, defaultConfigFileName);
    }

    // N
    public string NewGuid(string prefix = "") => (prefix ?? "") + Guid.NewGuid().ToString("N");

    // R
  public void RebuildLanguageButtonsFromCSV(string containerName, string csv, string baseClasses = "qbtn lang-btn")
{
    var root = GetElement(containerName);
    if (root == null) return;
    root.Clear();

    // Detect card vs. global bar
    bool isCardGroup = containerName.StartsWith("langGroup_", StringComparison.Ordinal);
    string uuid = isCardGroup ? containerName.Substring("langGroup_".Length) : null;

    var tokens = (csv ?? string.Empty)
        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        .Select(s => s.Trim())
        .Where(s => !string.IsNullOrEmpty(s));

    foreach (var code in tokens)
    {
        var btn = new Button
        {
            name = isCardGroup ? $"btnLang_{uuid}_{code}" : $"btnLang{code}",
            text = code
        };
        foreach (var cls in (baseClasses ?? string.Empty).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            btn.AddToClassList(cls);

        var id = btn.name;
        btn.clicked += () => _buttonClicks.Add(id);
        root.Add(btn);
    }
}

public void RefreshGlobalLanguageBar()
{
    RebuildLanguageButtonsFromCSV("languagesBar", _globalLanguagesCsv);
    var defaultCode = (_globalLanguagesCsv ?? "").Split(',').FirstOrDefault()?.Trim();
    if (!string.IsNullOrEmpty(defaultCode))
        SelectLanguageButton("languagesBar", defaultCode);
}

    public void RemoveClass(string elementName, string className) => GetElement(elementName)?.RemoveFromClassList(className);

    public void RemoveClientCardByUUID(string containerName, string uuid)
    {
        var list = GetElement(containerName);
        if (list == null) return;
        var card = list.Q<VisualElement>($"clientCard_{uuid}");
        if (card != null) list.Remove(card);
    }

    // S
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

    public void SelectClientLanguageByText(string uuid, string code, string selectedClass = "selected")
    {
        if (string.IsNullOrWhiteSpace(uuid) || string.IsNullOrWhiteSpace(code)) return;

        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        if (card == null) return;

        var norm = code.Trim().ToUpperInvariant();
        var buttons = card.Query<Button>(className: "lang-btn").ToList();

        foreach (var b in buttons)
        {
            var txt = (b.text ?? "").Trim().ToUpperInvariant();
            if (txt == norm) b.AddToClassList(selectedClass);
            else b.RemoveFromClassList(selectedClass);
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

    public void SetAllClientCardsEnabled(string containerName, bool enabled)
    {
        var list = GetElement(containerName);
        if (list == null) return;
        foreach (var card in list.Children())
        {
            var name = card.name ?? "";
            const string prefix = "clientCard_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;
            card.SetEnabled(enabled);
        }
    }

    public void SetAllClientLanguages(string containerName, string code, string selectedClass = "selected")
    {
        var list = GetElement(containerName);
        if (list == null || string.IsNullOrEmpty(code)) return;

        foreach (var card in list.Children())
        {
            var name = card.name ?? "";
            const string prefix = "clientCard_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;

            var uuid = name.Substring(prefix.Length);
            SelectClientLanguage(containerName, uuid, code, selectedClass);
        }
    }

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

    public void SetAllClientPlayIconMode(string containerName, string mode /* play | pause | stop */)
    {
        var list = GetElement(containerName);
        if (list == null) return;

        foreach (var card in list.Children())
        {
            var name = card.name ?? "";
            const string prefix = "clientCard_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var uuid = name.Substring(prefix.Length);
            SetClientPlayIconMode(uuid, mode);
        }
    }

    public void SetAllClientPlaySelected(string containerName, bool on, string selectedClass = "selected")
    {
        var list = GetElement(containerName);
        if (list == null) return;

        foreach (var card in list.Children())
        {
            var name = card.name ?? "";
            const string prefix = "clientCard_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var uuid = name.Substring(prefix.Length);
            SetClientPlaySelected(uuid, on, selectedClass);
        }
    }

    public void SetClientButtonsEnabled(string uuid, bool enabled)
    {
        var playGroup = _root?.Q<VisualElement>($"btnGroup_{uuid}");
        var langGroup = _root?.Q<VisualElement>($"langGroup_{uuid}");
        playGroup?.SetEnabled(enabled);
        langGroup?.SetEnabled(enabled);
    }

    public void SetClientCardEnabled(string uuid, bool enabled)
    {
        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        if (card == null) return;
        card.SetEnabled(enabled);
    }

    public void SetClientLanguages(string uuid, string languagesCsv, string defaultLang)
    {
        if (string.IsNullOrWhiteSpace(uuid)) return;
        _langsCsvByUuid[uuid] = languagesCsv ?? "";
        _defaultLangByUuid[uuid] = defaultLang ?? "";

        BuildLanguageButtonsForCard(uuid, languagesCsv, defaultLang);
        SetVisible($"langGroup_{uuid}", true);
    }

    public void SetClientPlayIconMode(string uuid, string mode /* play | pause | stop */)
    {
        var btn = _root?.Q<Button>($"btnPlay_{uuid}");
        ApplyIcon(btn, mode);
    }

    public void SetClientPlaySelected(string uuid, bool on, string selectedClass = "selected")
    {
        var btn = _root?.Q<Button>($"btnPlay_{uuid}");
        if (btn == null) return;
        if (on) btn.AddToClassList(selectedClass); else btn.RemoveFromClassList(selectedClass);
    }

    public void SetClientStopSelected(string uuid, bool on, string selectedClass = "selected")
    {
        var btn = _root?.Q<Button>($"btnSrop_{uuid}"); // kept as-is to avoid breaking existing FSM refs
        if (btn == null) return;
        if (on) btn.AddToClassList(selectedClass); else btn.RemoveFromClassList(selectedClass);
    }

    public void SetClientStatusGreen(string uuid)
    {
        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        var icon = _root?.Q<VisualElement>($"issueIcon_{uuid}");
        var sub = _root?.Q<Label>($"lblClientSub_{uuid}");

        if (card != null) { card.RemoveFromClassList("is-error"); card.AddToClassList("is-active"); }
        if (icon != null) { icon.RemoveFromClassList("error"); icon.AddToClassList("active"); }
        if (sub != null) { sub.RemoveFromClassList("error"); sub.AddToClassList("active"); }
    }

    public void SetClientStatusRed(string uuid)
    {
        var card = _root?.Q<VisualElement>($"clientCard_{uuid}");
        var icon = _root?.Q<VisualElement>($"issueIcon_{uuid}");
        var sub = _root?.Q<Label>($"lblClientSub_{uuid}");

        if (card != null) { card.RemoveFromClassList("is-active"); card.AddToClassList("is-error"); }
        if (icon != null) { icon.RemoveFromClassList("active"); icon.AddToClassList("error"); }
        if (sub != null) { sub.RemoveFromClassList("active"); sub.AddToClassList("error"); }
    }

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
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
        SetDropdownChoices(elementName, list, selectIndex);
    }

    public void SetEnabled(string elementName, bool enabled) => GetElement(elementName)?.SetEnabled(enabled);

    public void SetText(string elementName, string text)
    {
        var el = GetTextElement(elementName);
        if (el != null) el.text = text ?? string.Empty;
    }

    public void SetVisible(string elementName, bool visible)
    {
        var el = GetElement(elementName);
        if (el == null) return;
        el.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void SortClientCardsByStatusAndFamId(string containerName = null, string configFileName = "headsets.config")
    {
        containerName = string.IsNullOrEmpty(containerName) ? clientsListContainerName : containerName;
        var list = GetElement(containerName);
        if (list == null) return;

        // Build UUID -> FamId map from config (aligned by index)
        var cfg = LoadConfig(configFileName) ?? new ConfigData();
        var famByUuid = new Dictionary<string, string>(StringComparer.Ordinal);
        int n = Mathf.Min(cfg.UUIDs.Count, cfg.FamIds.Count);
        for (int i = 0; i < n; i++)
        {
            var u = (cfg.UUIDs[i] ?? "").Trim();
            if (string.IsNullOrEmpty(u)) continue;
            var f = (cfg.FamIds[i] ?? "").Trim();
            if (!famByUuid.ContainsKey(u)) famByUuid.Add(u, f);
        }

        // Collect current cards
        var cards = new List<(VisualElement ve, string uuid, bool isConnected, string famId, int famSort)>();
        foreach (var child in list.Children())
        {
            var name = child.name ?? "";
            const string prefix = "clientCard_";
            if (!name.StartsWith(prefix, StringComparison.Ordinal)) continue;
            var uuid = name.Substring(prefix.Length);

            bool isConnected = _connectedUuids.Contains(uuid);

            famByUuid.TryGetValue(uuid, out string famId);
            int famSort = int.MaxValue;
            if (!string.IsNullOrWhiteSpace(famId))
            {
                if (!int.TryParse(famId, out famSort))
                {
                    var digits = new string(famId.Where(char.IsDigit).ToArray());
                    if (!int.TryParse(digits, out famSort)) famSort = int.MaxValue;
                }
            }

            cards.Add((child, uuid, isConnected, famId ?? "", famSort));
        }

        // Sort: connected first, then famId asc (unknowns last), then uuid as tiebreaker
        cards.Sort((a, b) =>
        {
            int byConn = b.isConnected.CompareTo(a.isConnected); // true before false
            if (byConn != 0) return byConn;
            int byFam = a.famSort.CompareTo(b.famSort);
            if (byFam != 0) return byFam;
            return string.Compare(a.uuid, b.uuid, StringComparison.Ordinal);
        });

        // Rebuild order
        foreach (var item in cards)
            list.Remove(item.ve);
        foreach (var item in cards)
            list.Add(item.ve);
    }

    public void SyncClientsFromConfig(string containerName = null, string configFileName = "headsets.config")
    {
        containerName = string.IsNullOrEmpty(containerName) ? clientsListContainerName : containerName;

        var list = GetElement(containerName);
        if (list == null) return;

        // Purge existing rows
        var toRemove = new List<VisualElement>();
        foreach (var child in list.Children())
        {
            var n = child.name ?? "";
            if (n.StartsWith("clientCard_", StringComparison.Ordinal))
                toRemove.Add(child);
        }
        foreach (var ve in toRemove) list.Remove(ve);

        // Load config
        var data = LoadConfig(configFileName);
        if (data == null) return;

        var activeSet = new HashSet<string>(data.Active ?? new List<string>(0), StringComparer.Ordinal);
        _connectedUuids.Clear();
        foreach (var id in activeSet) _connectedUuids.Add(id);

        // name by uuid
        var nameByUuid = new Dictionary<string, string>(StringComparer.Ordinal);
        int pairCount = Mathf.Min(data.UUIDs.Count, data.Names.Count);
        for (int i = 0; i < pairCount; i++)
        {
            var u = (data.UUIDs[i] ?? "").Trim();
            var n = (data.Names[i] ?? "").Trim();
            if (!string.IsNullOrEmpty(u) && !nameByUuid.ContainsKey(u)) nameByUuid.Add(u, n);
        }

        // Ensure global langs loaded
        if (string.IsNullOrWhiteSpace(_globalLanguagesCsv))
            LoadGlobalLanguagesFromPmConfig();

        // Build rows
        foreach (var raw in data.UUIDs)
        {
            var uuid = (raw ?? "").Trim();
            if (string.IsNullOrEmpty(uuid)) continue;

            var displayName = nameByUuid.TryGetValue(uuid, out var nm) && !string.IsNullOrEmpty(nm) ? nm : uuid;

            AddOrUpdateClientCard(containerName, uuid, displayName, null, null);

            if (activeSet.Contains(uuid))
                MarkClientConnected(uuid, displayName);
            else
                MarkClientDisconnected(uuid, displayName);

            // Language bar: always from global pm_config.json here
            if (!string.IsNullOrWhiteSpace(_globalLanguagesCsv))
            {
                RebuildLanguageButtonsFromCSV($"langGroup_{uuid}", _globalLanguagesCsv);
                SetVisible($"langGroup_{uuid}", activeSet.Contains(uuid)); // show only if connected
            }
        }

        UpdateActiveCountLabelFromSet();
        SortClientCardsByStatusAndFamId(clientsListContainerName, defaultConfigFileName);
    }

    // T
    public void ToggleClass(string elementName, bool enabled, string className) // legacy-safe overload
        => ToggleClass(elementName, className, enabled);

    public void ToggleClass(string elementName, string className, bool enabled)
    {
        var el = GetElement(elementName);
        if (el == null) return;
        if (enabled) el.AddToClassList(className); else el.RemoveFromClassList(className);
    }

    // U
    public void UpdateStatesFromConfig(string containerName = null, string configFileName = "headsets.config")
    {
        containerName = string.IsNullOrEmpty(containerName) ? clientsListContainerName : containerName;

        var list = GetElement(containerName);
        if (list == null) return;

        var data = LoadConfig(configFileName);
        if (data == null) return;

        var activeSet = new HashSet<string>(data.Active ?? new List<string>(0), StringComparer.Ordinal);

        // Build a quick name map
        var nameByUuid = new Dictionary<string, string>(StringComparer.Ordinal);
        int pairCount = Mathf.Min(data.UUIDs.Count, data.Names.Count);
        for (int i = 0; i < pairCount; i++)
        {
            var u = (data.UUIDs[i] ?? "").Trim();
            var n = (data.Names[i] ?? "").Trim();
            if (!string.IsNullOrEmpty(u) && !nameByUuid.ContainsKey(u)) nameByUuid.Add(u, n);
        }

        // Ensure global langs loaded
        if (string.IsNullOrWhiteSpace(_globalLanguagesCsv))
            LoadGlobalLanguagesFromPmConfig();

        // Ensure rows exist, then set state strictly from Active
        foreach (var raw in data.UUIDs)
        {
            var uuid = (raw ?? "").Trim();
            if (string.IsNullOrEmpty(uuid)) continue;

            var displayName = nameByUuid.TryGetValue(uuid, out var nm) && !string.IsNullOrEmpty(nm) ? nm : uuid;
            AddOrUpdateClientCard(containerName, uuid, displayName, null, null);

            if (activeSet.Contains(uuid))
                MarkClientConnected(uuid, displayName);
            else
                MarkClientDisconnected(uuid, displayName);

            // Language bar: always from global pm_config.json here
            if (!string.IsNullOrWhiteSpace(_globalLanguagesCsv))
            {
                RebuildLanguageButtonsFromCSV($"langGroup_{uuid}", _globalLanguagesCsv);
                SetVisible($"langGroup_{uuid}", activeSet.Contains(uuid)); // show only if connected
            }
        }

        // Force any existing UI rows not in Active to disconnected
        foreach (var child in list.Children())
        {
            var n = child.name ?? "";
            const string prefix = "clientCard_";
            if (!n.StartsWith(prefix, StringComparison.Ordinal)) continue;

            var uuid = n.Substring(prefix.Length);
            if (!activeSet.Contains(uuid))
            {
                var title = _root?.Q<Label>($"lblClientTitle_{uuid}");
                var nameText = title != null ? title.text : uuid;
                MarkClientDisconnected(uuid, nameText);
                SetVisible($"langGroup_{uuid}", false);
            }
        }

        _connectedUuids.Clear();
        foreach (var id in activeSet) _connectedUuids.Add(id);
        UpdateActiveCountLabelFromSet();
        SortClientCardsByStatusAndFamId(clientsListContainerName, defaultConfigFileName);
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────────────────────
    // (kept in case you later store per-device languages in headsets.config)
    private bool TryGetPerDeviceLangFromConfig(string uuid, out string csv, out string def, string fileName = null)
    {
        csv = ""; def = "";
        uuid = (uuid ?? "").Trim();
        if (string.IsNullOrEmpty(uuid)) return false;

        var data = LoadConfig(string.IsNullOrEmpty(fileName) ? defaultConfigFileName : fileName);
        if (data == null) return false;

        // build index by uuid
        var indexByUuid = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < data.UUIDs.Count; i++)
        {
            var u = (data.UUIDs[i] ?? "").Trim();
            if (!string.IsNullOrEmpty(u) && !indexByUuid.ContainsKey(u)) indexByUuid.Add(u, i);
        }

        if (!indexByUuid.TryGetValue(uuid, out var idx)) return false;

        csv = (idx >= 0 && idx < data.Languages.Count) ? (data.Languages[idx] ?? "").Trim() : "";
        def = (idx >= 0 && idx < data.DefaultLangs.Count) ? (data.DefaultLangs[idx] ?? "").Trim() : "";
        return !string.IsNullOrWhiteSpace(csv);
    }

    private void ApplyIcon(Button b, string mode)
    {
        if (b == null) return;
        b.RemoveFromClassList("play");
        b.RemoveFromClassList("pause");
        b.RemoveFromClassList("stop");

        Sprite s = iconPlay;
        var m = (mode ?? "play").ToLowerInvariant();
        if (m == "pause") s = iconPause;
        else if (m == "stop") s = iconStop;
        else m = "play";

        b.AddToClassList(m);
        b.style.backgroundImage = s != null ? new StyleBackground(s) : new StyleBackground();
    }

    private void UpdateActiveCountLabelFromSet()
    {
        var lbl = GetLabel(activeCountLabelName);
        if (lbl != null) lbl.text = _connectedUuids.Count.ToString();
    }

    private void RestoreLanguageButtonsIfMissing(string uuid)
    {
        var langGroup = _root?.Q<VisualElement>($"langGroup_{uuid}");
        if (langGroup == null) return;

        if (langGroup.childCount == 0 && _langsCsvByUuid.TryGetValue(uuid, out var csv))
        {
            _defaultLangByUuid.TryGetValue(uuid, out var def);
            if (!string.IsNullOrWhiteSpace(csv))
                BuildLanguageButtonsForCard(uuid, csv, def);
        }
    }

    private static string SafeFileName(string input)
    {
        foreach (var c in Path.GetInvalidFileNameChars()) input = input.Replace(c, '_');
        return input;
    }

    // pm_config.json → config.value.Languages
    [Serializable] private class PMCfgRoot   { public PMCfgContainer config; }
    [Serializable] private class PMCfgContainer { public string __type; public PMCfgValue value; }
    [Serializable] private class PMCfgValue { public List<string> Languages; }

    private void LoadGlobalLanguagesFromPmConfig()
    {
        _globalLanguagesCsv = "";
        try
        {
            var safe = string.IsNullOrWhiteSpace(pmConfigFileName) ? "pm_config.json" : pmConfigFileName;
            var path = Path.Combine(Application.persistentDataPath, SafeFileName(safe));
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return;

            var root = JsonUtility.FromJson<PMCfgRoot>(json);
            var langs = root?.config?.value?.Languages;
            if (langs != null && langs.Count > 0)
            {
                _globalLanguagesCsv = string.Join(",", langs.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()));
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UIToolkitBridge.LoadGlobalLanguagesFromPmConfig: {e}");
        }
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // CONFIG TYPES + NORMALIZATION (reader for Sync/UpdateStates)
    // ─────────────────────────────────────────────────────────────────────────────
    [Serializable] private class Entry { public string Key; public List<string> Values = new(); }
    [Serializable] private class GenericConfig { public List<Entry> Entries = new(); }
    [Serializable] private class FlatConfig
    {
        public List<string> UUIDs = new();
        public List<string> Names = new();
        public List<string> FamIds = new();
        public List<string> Active = new();
        public List<string> Languages = new();     // CSV
        public List<string> DefaultLangs = new();  // default code
    }

    private class ConfigData
    {
        public List<string> UUIDs = new();
        public List<string> Names = new();
        public List<string> FamIds = new();
        public List<string> Active = new();
        public List<string> Languages = new();
        public List<string> DefaultLangs = new();
    }

    private ConfigData LoadConfig(string fileName)
    {
        try
        {
            var safe = string.IsNullOrWhiteSpace(fileName) ? defaultConfigFileName : fileName;
            var path = Path.Combine(Application.persistentDataPath, SafeFileName(safe));
            if (!File.Exists(path)) return new ConfigData();

            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return new ConfigData();

            // Try generic Entries model first
            var gc = JsonUtility.FromJson<GenericConfig>(json);
            if (gc != null && gc.Entries != null && gc.Entries.Count > 0)
                return Normalize(gc);

            // Fallback: flat model
            var flat = JsonUtility.FromJson<FlatConfig>(json);
            if (flat != null) return Normalize(flat);
        }
        catch (Exception e)
        {
            Debug.LogError($"UIToolkitBridge.LoadConfig: {e}");
        }
        return new ConfigData();
    }

    private static ConfigData Normalize(GenericConfig gc)
    {
        var d = new ConfigData();
        foreach (var e in gc.Entries)
        {
            if (e == null) continue;
            var key = e.Key ?? "";
            if (key == "UUIDs") d.UUIDs = Copy(e.Values);
            else if (key == "Names") d.Names = Copy(e.Values);
            else if (key == "FamIds") d.FamIds = Copy(e.Values);
            else if (key == "Active") d.Active = Copy(e.Values);
            else if (key == "Languages") d.Languages = Copy(e.Values);
            else if (key == "DefaultLangs") d.DefaultLangs = Copy(e.Values);
        }
        DedupAndClamp(d);
        return d;
    }

    private static ConfigData Normalize(FlatConfig f)
    {
        var d = new ConfigData
        {
            UUIDs        = Copy(f.UUIDs),
            Names        = Copy(f.Names),
            FamIds       = Copy(f.FamIds),
            Active       = Copy(f.Active),
            Languages    = Copy(f.Languages),
            DefaultLangs = Copy(f.DefaultLangs)
        };
        DedupAndClamp(d);
        return d;
    }

    private static List<string> Copy(List<string> src)
    {
        var outList = new List<string>(src?.Count ?? 0);
        if (src == null) return outList;
        foreach (var s in src) outList.Add(s ?? "");
        return outList;
    }

    private static void DedupAndClamp(ConfigData d)
    {
        d.UUIDs = DistinctInOrder(d.UUIDs);
        var known = new HashSet<string>(d.UUIDs);
        d.Active = IntersectDistinct(d.Active, known); // Active ⊆ UUIDs

        // Keep aux arrays aligned to UUID count
        void Truncate(ref List<string> arr, int len)
        {
            if (arr == null) arr = new List<string>();
            if (arr.Count > len) arr = arr.GetRange(0, len);
        }
        int L = d.UUIDs.Count;
        Truncate(ref d.Names, L);
        Truncate(ref d.FamIds, L);
        Truncate(ref d.Languages, L);
        Truncate(ref d.DefaultLangs, L);
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

    private static List<string> IntersectDistinct(List<string> src, HashSet<string> allow)
    {
        var seen = new HashSet<string>();
        var outList = new List<string>(src?.Count ?? 0);
        if (src == null) return outList;
        foreach (var s in src)
        {
            var t = s ?? "";
            if (allow.Contains(t) && seen.Add(t)) outList.Add(t);
        }
        return outList;
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // RAW ENTRIES[] READ/WRITE (used by MarkClientConnected/Disconnected)
    // ─────────────────────────────────────────────────────────────────────────────
    [Serializable] private class RawEntry { public string Key; public List<string> Values = new(); }
    [Serializable] private class RawRoot  { public List<RawEntry> Entries = new(); }

    private RawRoot LoadRaw(string fileName)
    {
        try
        {
            var path = Path.Combine(Application.persistentDataPath, SafeFileName(
                string.IsNullOrWhiteSpace(fileName) ? defaultConfigFileName : fileName));
            if (!File.Exists(path)) return new RawRoot();
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return new RawRoot();
            var root = JsonUtility.FromJson<RawRoot>(json);
            return root ?? new RawRoot();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"UIToolkitBridge.LoadRaw: {e}");
            return new RawRoot();
        }
    }

    private void SaveRaw(string fileName, RawRoot root)
    {
        try
        {
            var path = Path.Combine(Application.persistentDataPath, SafeFileName(
                string.IsNullOrWhiteSpace(fileName) ? defaultConfigFileName : fileName));
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonUtility.ToJson(root, true)); // pretty print
        }
        catch (Exception e)
        {
            Debug.LogError($"UIToolkitBridge.SaveRaw: {e}");
        }
    }

    private static RawEntry GetOrAdd(RawRoot root, string key)
    {
        foreach (var e in root.Entries)
            if (string.Equals(e?.Key, key, StringComparison.Ordinal)) return e;
        var ne = new RawEntry { Key = key, Values = new List<string>() };
        root.Entries.Add(ne);
        return ne;
    }

    private static void EnsureLen(List<string> list, int len, string pad = "")
    {
        while (list.Count < len) list.Add(pad);
        if (list.Count > len) list.RemoveRange(len, list.Count - len);
    }
}