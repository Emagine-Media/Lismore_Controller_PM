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
    public class UITKSetUserData : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        public FsmObject UserData;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                var obj = UserData.Value as GenericObject;
                if (obj != null)
                    element.SetUserData(obj.Data);
            }

            Finish();
        }
    }
}
#endif
