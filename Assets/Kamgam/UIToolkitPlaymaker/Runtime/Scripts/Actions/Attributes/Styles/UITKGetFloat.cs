#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    public enum StyleAttributeFloat
    {
        Width,
        Height,

        MinWidth,
        MinHeight,
        MaxWidth,
        MaxHeight,

        Left,
        Top,
        Right,
        Bottom,

        Opacity,
        FlexGrow,
        FlexShrink,
        FontSize,
        TextOutlineWidth,

        BorderWidth,
        BorderLeftWidth,
        BorderTopWidth,
        BorderRightWidth,
        BorderBottomWidth,

        BorderRadius,
        BorderTopLeftRadius,
        BorderTopRightRadius,
        BorderBottomLeftRadius,
        BorderBottomRightRadius,

        Margin,
        MarginLeft,
        MarginTop,
        MarginRight,
        MarginBottom,

        Padding,
        PaddingLeft,
        PaddingTop,
        PaddingRight,
        PaddingBottom,

        Rotation,

        Scale,
        ScaleX,
        ScaleY
    }

    [ActionCategory("UI Toolkit")]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public class UITKGetFloat : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [ObjectType(typeof(StyleAttributeFloat))]
        [Tooltip("The style to fetch.")]
        public FsmEnum Style;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmFloat StoreResult;

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
            float result = float.NaN;

            if (VisualElement.TryGetVisualElement(out var element))
            {
                var style = (StyleAttributeFloat) Style.Value;
                switch (style)
                {
                    case StyleAttributeFloat.Width:
                        result = element.GetWidth(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Height:
                        result = element.GetHeight(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MinWidth:
                        result = element.GetMinWidth(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MinHeight:
                        result = element.GetMinHeight(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MaxWidth:
                        result = element.GetMaxWidth(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MaxHeight:
                        result = element.GetMaxHeight(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Opacity:
                        result = element.GetOpacity(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Left:
                        result = element.GetPositionLeft(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Top:
                        result = element.GetPositionTop(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Right:
                        result = element.GetPositionRight(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Bottom:
                        result = element.GetPositionBottom(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.FlexGrow:
                        result = element.GetFlexGrow(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.FlexShrink:
                        result = element.GetFlexShrink(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.FontSize:
                        result = element.GetFontSize(ResolvedStyle);
                        break;
                    
                    case StyleAttributeFloat.TextOutlineWidth:
                        result = element.GetTextOutlineWidth(ResolvedStyle);
                        break;


                    case StyleAttributeFloat.BorderWidth:
                        result = element.GetBorderWidth(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderLeftWidth:
                        result = element.GetBorderLeftWidth(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderTopWidth:
                        result = element.GetBorderTopWidth(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderRightWidth:
                        result = element.GetBorderRightWidth(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderBottomWidth:
                        result = element.GetBorderBottomWidth(ResolvedStyle);
                        break;


                    case StyleAttributeFloat.BorderRadius:
                        result = element.GetBorderRadius(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderTopLeftRadius:
                        result = element.GetBorderTopLeftRadius(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderTopRightRadius:
                        result = element.GetBorderTopRightRadius(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderBottomLeftRadius:
                        result = element.GetBorderBottomLeftRadius(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.BorderBottomRightRadius:
                        result = element.GetBorderBottomRightRadius(ResolvedStyle);
                        break;


                    case StyleAttributeFloat.Margin:
                        result = element.GetMargin(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MarginLeft:
                        result = element.GetMarginLeft(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MarginTop:
                        result = element.GetMarginTop(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MarginRight:
                        result = element.GetMarginRight(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.MarginBottom:
                        result = element.GetMarginBottom(ResolvedStyle);
                        break;


                    case StyleAttributeFloat.Padding:
                        result = element.GetPadding(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.PaddingLeft:
                        result = element.GetPaddingLeft(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.PaddingTop:
                        result = element.GetPaddingTop(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.PaddingRight:
                        result = element.GetPaddingRight(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.PaddingBottom:
                        result = element.GetPaddingBottom(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Rotation:
                        result = element.GetRotation(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.Scale:
                        result = element.GetUniformScale(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.ScaleX:
                        result = element.GetScaleX(ResolvedStyle);
                        break;

                    case StyleAttributeFloat.ScaleY:
                        result = element.GetScaleY(ResolvedStyle);
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