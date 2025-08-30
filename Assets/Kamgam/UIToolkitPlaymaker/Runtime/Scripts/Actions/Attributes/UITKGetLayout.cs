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
    public class UITKGetLayout : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Target variable where the result is stored.")]
        public FsmRect StoreLayout;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                StoreLayout.Value = element.GetLayout();
            }

            Finish();
        }
    }
}
#endif
