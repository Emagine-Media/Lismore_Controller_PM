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
    public class UITKGetChildIndex : FsmStateAction
    {
        [ActionSection("Element Source")]

        [UIHint(UIHint.Variable)]
        [Tooltip("OPTIONAL: The parent to search through. If no parent is specified then the parent of the child is used.")]
        public FsmObject Parent;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject Child;

        [UIHint(UIHint.Variable)]
        [Tooltip("Variable where the index shoud be stored.\n" +
            "Will be -1 if the child was not found in the parent.\n" +
            "Will fall back to 0 if the child has no parent.")]
        public FsmInt StoreIndex;

        public override void OnEnter()
        {
            if (Child.TryGetVisualElement(out var element))
            {
                if (Parent.TryGetVisualElement(out var parent))
                {
                    StoreIndex.Value = parent.IndexOf(element);
                }
                else
                {
                    StoreIndex.Value = element.GetIndex();
                }
            }

            Finish();
        }
    }
}
#endif
