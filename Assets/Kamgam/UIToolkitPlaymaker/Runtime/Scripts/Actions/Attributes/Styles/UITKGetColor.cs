#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    public enum StyleAttributeColor
    {
        Color,
        TextColor,
        BackgroundColor,
        TextOutlineColor,
        BackgroundImageTint,
        BorderLeftColor,
        BorderTopColor,
        BorderRightColor,
        BorderBottomColor,
    }

    [ActionCategory("UI Toolkit")]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public class UITKGetColor : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [ObjectType(typeof(StyleAttributeColor))]
        [Tooltip("The style to fetch.")]
        public FsmEnum Style;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmColor StoreResult;

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
            Color result = default;

            if (VisualElement.TryGetVisualElement(out var element))
            {
                var style = (StyleAttributeColor) Style.Value;
                switch (style)
                {
                    case StyleAttributeColor.Color:
                    case StyleAttributeColor.TextColor:
                        result = element.GetColor(ResolvedStyle);
                        break;

                    case StyleAttributeColor.BackgroundColor:
                        result = element.GetBackgroundColor(ResolvedStyle);
                        break;

                    case StyleAttributeColor.TextOutlineColor:
                        result = element.GetTextOutlineColor(ResolvedStyle);
                        break;

                    case StyleAttributeColor.BackgroundImageTint:
                        result = element.GetBackgroundImageTint(ResolvedStyle);
                        break;

                    case StyleAttributeColor.BorderLeftColor:
                        result = element.GetBorderLeftColor(ResolvedStyle);
                        break;

                    case StyleAttributeColor.BorderTopColor:
                        result = element.GetBorderTopColor(ResolvedStyle);
                        break;

                    case StyleAttributeColor.BorderRightColor:
                        result = element.GetBorderRightColor(ResolvedStyle);
                        break;

                    case StyleAttributeColor.BorderBottomColor:
                        result = element.GetBorderBottomColor(ResolvedStyle);
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