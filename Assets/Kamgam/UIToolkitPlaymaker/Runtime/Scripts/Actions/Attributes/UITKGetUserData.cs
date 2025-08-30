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
    public class UITKGetUserData : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(VisualElementObject))]
        [Tooltip("Target variable of the object. NOTICE: The value stored is wrapped by a GenericObj.")]
        public FsmObject StoreUserData;

        [Tooltip("If enabled then the 'StoreUserData' is reused to preserve memory. Only enable this if you see a need for it in the Profiler.")]
        public bool ReuseStoreVariable = false;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                StoreUserData.SetResultGeneric(element.GetUserData(), ReuseStoreVariable);
            }

            Finish();
        }
    }
}
#endif
