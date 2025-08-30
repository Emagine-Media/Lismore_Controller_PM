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
    public class UITKGetDisplay : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(DisplayStyle))]
        [Tooltip("Store the result in a variable.")]
        public FsmEnum StoreResult;

        [Tooltip("Event to send if it is displayed.")]
        public FsmEvent DisplayedEvent;

        [Tooltip("Event to send if it is not displayed.")]
        public FsmEvent NotDisplayedEvent;

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
            DisplayStyle result = DisplayStyle.Flex;
            if (VisualElement.TryGetVisualElement(out var element))
            {
                result = element.GetDisplay(ResolvedStyle);
            }

            StoreResult.Value = result;

            Fsm.Event(result == DisplayStyle.None ? NotDisplayedEvent : DisplayedEvent);
        }
    }
}

#endif