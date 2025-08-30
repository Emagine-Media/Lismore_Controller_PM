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
    public class UITKSetFloat : FsmStateAction
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
        public FsmFloat Value;

        [Tooltip("If enabled then you can change the unit too (if the value is a length and supports a unit change).")]
        public bool SetLengthUnit = false;

        [ObjectType(typeof(LengthUnit))]
        [Tooltip("The unti of the length.")]
        [HideIf("_dontSetLengthUnity")]
        public FsmEnum LengthUnit;
        public bool _dontSetLengthUnity() => !SetLengthUnit;

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
                var lengthUnit = (LengthUnit) LengthUnit.Value;

                var style = (StyleAttributeFloat)Style.Value;
                if (ResetAttribute.Value)
                {
                    switch (style)
                    {
                        case StyleAttributeFloat.Width:
                            element.ResetWidth();
                            break;

                        case StyleAttributeFloat.Height:
                            element.ResetHeight();
                            break;

                        case StyleAttributeFloat.MinWidth:
                            element.ResetMinWidth();
                            break;

                        case StyleAttributeFloat.MinHeight:
                            element.ResetMinHeight();
                            break;

                        case StyleAttributeFloat.MaxWidth:
                            element.ResetMaxWidth();
                            break;

                        case StyleAttributeFloat.MaxHeight:
                            element.ResetMaxHeight();
                            break;

                        case StyleAttributeFloat.Opacity:
                            element.ResetOpacity();
                            break;

                        case StyleAttributeFloat.Left:
                            element.ResetPositionLeft();
                            break;

                        case StyleAttributeFloat.Top:
                            element.ResetPositionTop();
                            break;

                        case StyleAttributeFloat.Right:
                            element.ResetPositionRight();
                            break;

                        case StyleAttributeFloat.Bottom:
                            element.ResetPositionBottom();
                            break;

                        case StyleAttributeFloat.FlexGrow:
                            element.ResetFlexGrow();
                            break;

                        case StyleAttributeFloat.FlexShrink:
                            element.ResetFlexShrink();
                            break;

                        case StyleAttributeFloat.FontSize:
                            element.ResetFontSize();
                            break;

                        case StyleAttributeFloat.TextOutlineWidth:
                            element.ResetTextOutlineWidth();
                            break;


                        case StyleAttributeFloat.BorderLeftWidth:
                            element.ResetBorderLeftWidth();
                            break;

                        case StyleAttributeFloat.BorderTopWidth:
                            element.ResetBorderTopWidth();
                            break;

                        case StyleAttributeFloat.BorderRightWidth:
                            element.ResetBorderRightWidth();
                            break;

                        case StyleAttributeFloat.BorderBottomWidth:
                            element.ResetBorderBottomWidth();
                            break;


                        case StyleAttributeFloat.BorderRadius:
                            element.ResetBorderRadius();
                            break;

                        case StyleAttributeFloat.BorderTopLeftRadius:
                            element.ResetBorderTopLeftRadius();
                            break;

                        case StyleAttributeFloat.BorderTopRightRadius:
                            element.ResetBorderTopRightRadius();
                            break;

                        case StyleAttributeFloat.BorderBottomLeftRadius:
                            element.ResetBorderBottomLeftRadius();
                            break;

                        case StyleAttributeFloat.BorderBottomRightRadius:
                            element.ResetBorderBottomRightRadius();
                            break;


                        case StyleAttributeFloat.Margin:
                            element.ResetMargin();
                            break;

                        case StyleAttributeFloat.MarginLeft:
                            element.ResetMarginLeft();
                            break;

                        case StyleAttributeFloat.MarginTop:
                            element.ResetMarginTop();
                            break;

                        case StyleAttributeFloat.MarginRight:
                            element.ResetMarginRight();
                            break;

                        case StyleAttributeFloat.MarginBottom:
                            element.ResetMarginBottom();
                            break;


                        case StyleAttributeFloat.Padding:
                            element.ResetPadding();
                            break;

                        case StyleAttributeFloat.PaddingLeft:
                            element.ResetPaddingLeft();
                            break;

                        case StyleAttributeFloat.PaddingTop:
                            element.ResetPaddingTop();
                            break;

                        case StyleAttributeFloat.PaddingRight:
                            element.ResetPaddingRight();
                            break;

                        case StyleAttributeFloat.PaddingBottom:
                            element.ResetPaddingBottom();
                            break;

                        case StyleAttributeFloat.Rotation:
                            element.ResetRotation();
                            break;

                        case StyleAttributeFloat.Scale:
                            element.ResetScale();
                            break;

                        case StyleAttributeFloat.ScaleX:
                            element.ResetScale();
                            break;

                        case StyleAttributeFloat.ScaleY:
                            element.ResetScale();
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    switch (style)
                    {
                        case StyleAttributeFloat.Width:
                            if(SetLengthUnit)
                                element.SetWidth(Value.Value, lengthUnit);
                            else
                                element.SetWidth(Value.Value);
                            break;

                        case StyleAttributeFloat.Height:
                            if (SetLengthUnit)
                                element.SetHeight(Value.Value, lengthUnit);
                            else
                                element.SetHeight(Value.Value);
                            break;

                        case StyleAttributeFloat.MinWidth:
                            if (SetLengthUnit)
                                element.SetMinWidth(Value.Value, lengthUnit);
                            else
                                element.SetMinWidth(Value.Value);
                            break;

                        case StyleAttributeFloat.MinHeight:
                            if (SetLengthUnit)
                                element.SetMinHeight(Value.Value, lengthUnit);
                            else
                                element.SetMinHeight(Value.Value);
                            break;

                        case StyleAttributeFloat.MaxWidth:
                            if (SetLengthUnit)
                                element.SetMaxWidth(Value.Value, lengthUnit);
                            else
                                element.SetMaxWidth(Value.Value);
                            break;

                        case StyleAttributeFloat.MaxHeight:
                            if (SetLengthUnit)
                                element.SetMaxHeight(Value.Value, lengthUnit);
                            else
                                element.SetMaxHeight(Value.Value);
                            break;

                        case StyleAttributeFloat.Opacity:
                            element.SetOpacity(Value.Value);
                            break;

                        case StyleAttributeFloat.Left:
                            if (SetLengthUnit)
                                element.SetPositionLeft(Value.Value, lengthUnit);
                            else
                                element.SetPositionLeft(Value.Value);
                            break;

                        case StyleAttributeFloat.Top:
                            if (SetLengthUnit)
                                element.SetPositionTop(Value.Value, lengthUnit);
                            else
                                element.SetPositionTop(Value.Value);
                            break;

                        case StyleAttributeFloat.Right:
                            if (SetLengthUnit)
                                element.SetPositionRight(Value.Value, lengthUnit);
                            else
                                element.SetPositionRight(Value.Value);
                            break;

                        case StyleAttributeFloat.Bottom:
                            if (SetLengthUnit)
                                element.SetPositionBottom(Value.Value, lengthUnit);
                            else
                                element.SetPositionBottom(Value.Value);
                            break;

                        case StyleAttributeFloat.FlexGrow:
                            element.SetFlexGrow(Value.Value);
                            break;

                        case StyleAttributeFloat.FlexShrink:
                            element.SetFlexShrink(Value.Value);
                            break;

                        case StyleAttributeFloat.FontSize:
                            element.SetFontSize(Value.Value);
                            break;

                        case StyleAttributeFloat.TextOutlineWidth:
                            element.SetTextOutlineWidth(Value.Value);
                            break;


                        case StyleAttributeFloat.BorderWidth:
                            element.SetBorderWidth(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderLeftWidth:
                            element.SetBorderLeftWidth(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderTopWidth:
                            element.SetBorderTopWidth(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderRightWidth:
                            element.SetBorderRightWidth(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderBottomWidth:
                            element.SetBorderBottomWidth(Value.Value);
                            break;


                        case StyleAttributeFloat.BorderRadius:
                            element.SetBorderRadius(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderTopLeftRadius:
                            element.SetBorderTopLeftRadius(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderTopRightRadius:
                            element.SetBorderTopRightRadius(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderBottomLeftRadius:
                            element.SetBorderBottomLeftRadius(Value.Value);
                            break;

                        case StyleAttributeFloat.BorderBottomRightRadius:
                            element.SetBorderBottomRightRadius(Value.Value);
                            break;


                        case StyleAttributeFloat.Margin:
                            if (SetLengthUnit)
                                element.SetMargin(Value.Value, lengthUnit);
                            else
                                element.SetMargin(Value.Value);
                            break;

                        case StyleAttributeFloat.MarginLeft:
                            if (SetLengthUnit)
                                element.SetMarginLeft(Value.Value, lengthUnit);
                            else
                                element.SetMarginLeft(Value.Value);
                            break;

                        case StyleAttributeFloat.MarginTop:
                            if (SetLengthUnit)
                                element.SetMarginTop(Value.Value, lengthUnit);
                            else
                                element.SetMarginTop(Value.Value);
                            break;

                        case StyleAttributeFloat.MarginRight:
                            if (SetLengthUnit)
                                element.SetMarginRight(Value.Value, lengthUnit);
                            else
                                element.SetMarginRight(Value.Value);
                            break;

                        case StyleAttributeFloat.MarginBottom:
                            if (SetLengthUnit)
                                element.SetMarginBottom(Value.Value, lengthUnit);
                            else
                                element.SetMarginBottom(Value.Value);
                            break;


                        case StyleAttributeFloat.Padding:
                            if (SetLengthUnit)
                                element.SetPadding(Value.Value, lengthUnit);
                            else
                                element.SetPadding(Value.Value);
                            break;

                        case StyleAttributeFloat.PaddingLeft:
                            if (SetLengthUnit)
                                element.SetPaddingLeft(Value.Value, lengthUnit);
                            else
                                element.SetPaddingLeft(Value.Value);
                            break;

                        case StyleAttributeFloat.PaddingTop:
                            if (SetLengthUnit)
                                element.SetPaddingTop(Value.Value, lengthUnit);
                            else
                                element.SetPaddingTop(Value.Value);
                            break;

                        case StyleAttributeFloat.PaddingRight:
                            if (SetLengthUnit)
                                element.SetPaddingRight(Value.Value, lengthUnit);
                            else
                                element.SetPaddingRight(Value.Value);
                            break;

                        case StyleAttributeFloat.PaddingBottom:
                            if (SetLengthUnit)
                                element.SetPaddingBottom(Value.Value, lengthUnit);
                            else
                                element.SetPaddingBottom(Value.Value);
                            break;

                        case StyleAttributeFloat.Rotation:
                            element.SetRotation(Value.Value);
                            break;

                        case StyleAttributeFloat.Scale:
                            element.SetUniformScale(Value.Value);
                            break;

                        case StyleAttributeFloat.ScaleX:
                            element.SetScaleX(Value.Value);
                            break;

                        case StyleAttributeFloat.ScaleY:
                            element.SetScaleY(Value.Value);
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