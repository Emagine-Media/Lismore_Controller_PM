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
    public class UITKSetPickingMode : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [ObjectType(typeof(PickingMode))]
        public FsmEnum PickingMode;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                element.SetPickingMode((PickingMode)PickingMode.Value);
            }

            Finish();
        }
    }
}
#endif
