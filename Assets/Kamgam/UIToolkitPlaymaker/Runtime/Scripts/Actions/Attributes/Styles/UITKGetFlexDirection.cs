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
    public class UITKGetFlexDirection : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(FlexDirection))]
        [Tooltip("Store the result in a variable.")]
        public FsmEnum StoreResult;

        [Tooltip("Get the attribute value from the resolved style?")]
        public bool ResolvedStyle = true;

        public override void Reset()
        {
            VisualElement = null;
            StoreResult = null;
        }

        public override void OnEnter()
        {
            getAttribute();
        }

        public override void OnUpdate()
        {
            getAttribute();
        }

        void getAttribute()
        {
            FlexDirection result = default;
            if (VisualElement.TryGetVisualElement(out var element))
            {
                result = element.GetFlexDirection(ResolvedStyle);
            }

            StoreResult.Value = result;
        }
    }
}

#endif