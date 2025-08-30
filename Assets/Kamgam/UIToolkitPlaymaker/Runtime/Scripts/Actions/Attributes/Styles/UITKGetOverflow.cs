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
    public class UITKGetOverflow : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(Overflow))]
        [Tooltip("Store the result in a variable.")]
        public FsmEnum StoreResult;

        // Seems like element.resolvedStyle.overflow; is not a thing. TODO: Investigate.
        // [Tooltip("Get the attribute value from the resolved style?")]
        // public bool ResolvedStyle = true;

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
            Overflow result = default;
            if (VisualElement.TryGetVisualElement(out var element))
            {
                // Seems like element.resolvedStyle.overflow is not a thing. TODO: Investigate.
                // result = element.GetOverflow(ResolvedStyle);

                result = element.GetOverflow();
            }

            StoreResult.Value = result;
        }
    }
}

#endif