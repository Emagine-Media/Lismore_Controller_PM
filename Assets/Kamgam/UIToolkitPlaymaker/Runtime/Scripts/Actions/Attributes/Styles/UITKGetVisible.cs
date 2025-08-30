#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    [ActionCategory("UI Toolkit")]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public class UITKGetVisible : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [UIHint(UIHint.Variable)]
        [Tooltip("Store the result in a variable.")]
        public FsmBool StoreResult;

        [Tooltip("Event to send if it is visible.")]
        public FsmEvent VisibleEvent;

        [Tooltip("Event to send if it is not visible.")]
        public FsmEvent NotVisibleEvent;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        [Tooltip("Get the attribute value from the resolved style?")]
        public bool ResolvedStyle = true;

        public override void Reset()
        {
            VisualElement = null;
            StoreResult = null;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            getAttribute();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            getAttribute();
        }

        void getAttribute()
        {
            bool result = false;
            if (VisualElement.TryGetVisualElement(out var element))
            {
                result = element.GetVisible(ResolvedStyle);
            }

            StoreResult.Value = result;

            Fsm.Event(result ? VisibleEvent : NotVisibleEvent);
        }
    }
}

#endif