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
    public class UITKGetPosition : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [ObjectType(typeof(Position))]
        [Tooltip("Target variable where the result will be stored.")]
        public FsmEnum StorePosition;

        [Tooltip("Get the color set in the style or the resolved style?")]
        public bool ResolvedStyle = true;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                StorePosition.Value = element.GetPosition(ResolvedStyle);
            }

            Finish();
        }
    }
}
#endif
