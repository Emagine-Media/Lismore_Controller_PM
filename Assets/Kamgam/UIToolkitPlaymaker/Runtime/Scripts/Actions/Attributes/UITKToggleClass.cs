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
    public class UITKToggleClass : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [ArrayEditor(VariableType.String, "Class", 0, 1)]
        [Tooltip("Class names to toggle.")]
        public FsmArray ClassNames;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                foreach (var className in ClassNames.stringValues)
                {
                    element.ToggleClass(className);
                }
            }

            Finish();
        }
    }
}
#endif
