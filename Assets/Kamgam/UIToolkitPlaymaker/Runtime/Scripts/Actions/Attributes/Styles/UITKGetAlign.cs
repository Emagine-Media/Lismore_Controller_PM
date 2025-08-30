#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    public enum StyleAttributeAlign
    {
        AlignContent,
        AlignItems,
        AlignSelf
    }

    [ActionCategory("UI Toolkit")]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public class UITKGetAlign : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [ObjectType(typeof(StyleAttributeAlign))]
        [Tooltip("The style to fetch.")]
        public FsmEnum Style;

        [RequiredField]
        [ObjectType(typeof(Align))]
        [UIHint(UIHint.Variable)]
        public FsmEnum StoreResult;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        [Tooltip("Get the attribute value from the resolved style?")]
        public bool ResolvedStyle = true;

        public override void Reset()
        {
            VisualElement = null;
            Style = null;
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
            Align result = default;

            if (VisualElement.TryGetVisualElement(out var element))
            {
                var style = (StyleAttributeAlign) Style.Value;
                switch (style)
                {
                    case StyleAttributeAlign.AlignContent:
                        result = element.GetAlignContent(ResolvedStyle);
                        break;

                    case StyleAttributeAlign.AlignItems:
                        result = element.GetAlignItems(ResolvedStyle);
                        break;

                    case StyleAttributeAlign.AlignSelf:
                        result = element.GetAlignSelf(ResolvedStyle);
                        break;

                    default:
                        break;
                }
            }

            StoreResult.Value = result;
        }
    }
}

#endif