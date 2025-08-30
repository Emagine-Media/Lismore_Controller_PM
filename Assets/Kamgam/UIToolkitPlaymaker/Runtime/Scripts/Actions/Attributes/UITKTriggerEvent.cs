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
    public class UITKTriggerEvent : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [ObjectType(typeof(EventObject))]
        [Tooltip("The event object.")]
        public FsmObject EventObject;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element) && EventObject.TryGetWrapper(out EventObject evt))
            {
                element.TriggerEvent(evt.Event);
            }

            Finish();
        }
    }
}
#endif
