using UnityEngine;
using UnityEngine.UIElements;
using HutongGames.PlayMaker;

namespace PlayMaker.UIDoc
{
    [ActionCategory("UI Toolkit")]
    [HutongGames.PlayMaker.Tooltip("Populate UI Toolkit elements from PlayMaker Global variables, and optionally toggle selected classes.")]
    public class UITKPopulateFromGlobals : FsmStateAction
    {
        [RequiredField]
        [HutongGames.PlayMaker.Tooltip("GameObject that has the UIDocument component. If None, uses Owner.")]
        public FsmOwnerDefault uiDocumentObject;

        [HutongGames.PlayMaker.Tooltip("Element names in the UIDocument to set text on (Labels, Buttons, or any TextElement).")]
        public FsmString[] elementNames;

        [HutongGames.PlayMaker.Tooltip("Global string variable names to read from (must match length/order of Element Names).")]
        public FsmString[] globalStringVarNames;

        [HutongGames.PlayMaker.Tooltip("Update a label that shows the default language code (optional).")]
        public FsmString defaultLanguageLabelName;

        [HutongGames.PlayMaker.Tooltip("Name of the Global String that contains the default language code, e.g., 'DefaultLanguage'.")]
        public FsmString defaultLanguageGlobalName;

        [HutongGames.PlayMaker.Tooltip("Language codes to highlight, e.g., EN,FR,IL,ES (will look for buttons named 'btnLang' + code).")]
        public FsmString[] languageCodes;

        [HutongGames.PlayMaker.Tooltip("Prefix for language buttons' names.")]
        public FsmString languageButtonNamePrefix; // e.g., "btnLang"

        [HutongGames.PlayMaker.Tooltip("USS class to toggle for selected visuals.")]
        public FsmString selectedClass; // e.g., "selected"

        [HutongGames.PlayMaker.Tooltip("Optionally mark a play button as selected (yellow).")]
        public FsmBool setPlaySelected;

        [HutongGames.PlayMaker.Tooltip("Name of the play button element (if Set Play Selected is true).")]
        public FsmString playButtonName; // e.g., "btnPlay"

        [HutongGames.PlayMaker.Tooltip("Run every frame (usually false).")]
        public FsmBool everyFrame;

        private VisualElement _root;

        public override void Reset()
        {
            uiDocumentObject = null;
            elementNames = new FsmString[0];
            globalStringVarNames = new FsmString[0];
            defaultLanguageLabelName = "";
            defaultLanguageGlobalName = "DefaultLanguage";
            languageCodes = new FsmString[0];
            languageButtonNamePrefix = "btnLang";
            selectedClass = "selected";
            setPlaySelected = false;
            playButtonName = "btnPlay";
            everyFrame = false;
        }

        public override void OnEnter()
        {
            if (!ResolveRoot())
            {
                Finish();
                return;
            }

            PopulateOnce();

            if (!everyFrame.Value)
                Finish();
        }

        public override void OnUpdate()
        {
            PopulateOnce();
        }

        private bool ResolveRoot()
        {
            GameObject go = Fsm.GetOwnerDefaultTarget(uiDocumentObject);
            if (go == null)
            {
                LogWarning("UIDocument target is null. Assign a GameObject with UIDocument.");
                return false;
            }

            var doc = go.GetComponent<UIDocument>();
            if (doc == null)
            {
                LogWarning("No UIDocument component found on target GameObject.");
                return false;
            }

            _root = doc.rootVisualElement;
            if (_root == null)
            {
                LogWarning("UIDocument.rootVisualElement is null.");
                return false;
            }
            return true;
        }

        private void PopulateOnce()
        {
            // 1) Map globals → UI text
            int count = Mathf.Min(elementNames != null ? elementNames.Length : 0,
                                  globalStringVarNames != null ? globalStringVarNames.Length : 0);

            for (int i = 0; i < count; i++)
            {
                string elemName = elementNames[i].Value;
                string globalName = globalStringVarNames[i].Value;

                if (string.IsNullOrEmpty(elemName) || string.IsNullOrEmpty(globalName))
                    continue;

                var g = FsmVariables.GlobalVariables.FindFsmString(globalName);
                if (g == null)
                {
                    LogWarning($"Global String '{globalName}' not found.");
                    continue;
                }

                var el = _root.Q<VisualElement>(elemName);
                if (el == null)
                {
                    LogWarning($"UI element '{elemName}' not found in UIDocument.");
                    continue;
                }

                // TextElement covers Label, Button, etc.
                if (el is TextElement te)
                {
                    te.text = g.Value ?? string.Empty;
                }
                else
                {
                    LogWarning($"Element '{elemName}' is not a TextElement. Skipping text set.");
                }
            }

            // 2) Language selected logic (optional)
            string selectedClassName = string.IsNullOrEmpty(selectedClass.Value) ? "selected" : selectedClass.Value;
            if (!string.IsNullOrEmpty(defaultLanguageGlobalName.Value) && languageCodes != null && languageCodes.Length > 0)
            {
                var gLang = FsmVariables.GlobalVariables.FindFsmString(defaultLanguageGlobalName.Value);
                string lang = gLang != null ? gLang.Value : "";

                // Update a small default language label if provided
                if (!string.IsNullOrEmpty(defaultLanguageLabelName.Value))
                {
                    var label = _root.Q<TextElement>(defaultLanguageLabelName.Value);
                    if (label != null) label.text = lang;
                }

                string prefix = string.IsNullOrEmpty(languageButtonNamePrefix.Value) ? "btnLang" : languageButtonNamePrefix.Value;

                foreach (var codeFsm in languageCodes)
                {
                    string code = codeFsm.Value;
                    if (string.IsNullOrEmpty(code)) continue;

                    var btn = _root.Q<VisualElement>(prefix + code);
                    if (btn == null) continue;

                    bool isSelected = !string.IsNullOrEmpty(lang) && string.Equals(lang, code, System.StringComparison.OrdinalIgnoreCase);
                    btn.EnableInClassList(selectedClassName, isSelected);
                }
            }

            // 3) Play button selected (optional)
            if (setPlaySelected.Value && !string.IsNullOrEmpty(playButtonName.Value))
            {
                var playBtn = _root.Q<VisualElement>(playButtonName.Value);
                if (playBtn != null)
                {
                    playBtn.EnableInClassList(selectedClassName, true);
                }
            }
        }
    }
}
