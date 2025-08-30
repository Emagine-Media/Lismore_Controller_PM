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
    public class UITKSetColor : FsmStateAction
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
        public FsmColor Value;

        [Tooltip("Reset using 'StyleKeyword.Null'?")]
        public FsmBool ResetAttribute = false;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

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
            if (VisualElement.TryGetVisualElement(out var element))
            {
                var style = (StyleAttributeColor)Style.Value;
                if (ResetAttribute.Value)
                {
                    switch (style)
                    {
                        case StyleAttributeColor.Color:
                        case StyleAttributeColor.TextColor:
                            element.ResetColor();
                            break;

                        case StyleAttributeColor.BackgroundColor:
                            element.ResetBackgroundColor();
                            break;

                        case StyleAttributeColor.TextOutlineColor:
                            element.ResetTextOutlineColor();
                            break;

                        case StyleAttributeColor.BackgroundImageTint:
                            element.ResetTextOutlineColor();
                            break;

                        case StyleAttributeColor.BorderLeftColor:
                            element.ResetBorderLeftColor();
                            break;

                        case StyleAttributeColor.BorderTopColor:
                            element.ResetBorderTopColor();
                            break;

                        case StyleAttributeColor.BorderRightColor:
                            element.ResetBorderRightColor();
                            break;

                        case StyleAttributeColor.BorderBottomColor:
                            element.ResetBorderBottomColor();
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    switch (style)
                    {
                        case StyleAttributeColor.Color:
                        case StyleAttributeColor.TextColor:
                            element.SetColor(Value.Value);
                            break;

                        case StyleAttributeColor.BackgroundColor:
                            element.SetBackgroundColor(Value.Value);
                            break;

                        case StyleAttributeColor.TextOutlineColor:
                            element.SetTextOutlineColor(Value.Value);
                            break;

                        case StyleAttributeColor.BackgroundImageTint:
                            element.SetBackgroundImageTint(Value.Value);
                            break;

                        case StyleAttributeColor.BorderLeftColor:
                            element.SetBorderLeftColor(Value.Value);
                            break;

                        case StyleAttributeColor.BorderTopColor:
                            element.SetBorderTopColor(Value.Value);
                            break;

                        case StyleAttributeColor.BorderRightColor:
                            element.SetBorderRightColor(Value.Value);
                            break;

                        case StyleAttributeColor.BorderBottomColor:
                            element.SetBorderBottomColor(Value.Value);
                            break;

                        default:
                            break;
                    }
                }
            }
        }
    }
}

#endif