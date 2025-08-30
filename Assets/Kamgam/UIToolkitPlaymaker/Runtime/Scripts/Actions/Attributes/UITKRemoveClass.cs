#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    [ActionCategory("UI Toolkit")]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public class UITKRemoveClass : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [ArrayEditor(VariableType.String, "Class", 0, 1)]
        [Tooltip("Class names to remove.")]
        [HideIf("_removeAll")]
        public FsmArray ClassNames;

        [Tooltip("If enabled then all classes are removed.")]
        public FsmBool RemoveAll;
        public bool _removeAll() => RemoveAll.Value;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                if (RemoveAll.Value)
                {
                    element.ClearClasses();
                }
                else
                {
                    foreach (var className in ClassNames.stringValues)
                    {
                        element.RemoveClass(className);
                    }
                }
            }

            Finish();
        }
    }
}
#endif
