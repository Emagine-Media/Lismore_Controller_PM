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
    public class UITKSetAlign : FsmStateAction
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
        public FsmEnum Value;

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
                var style = (StyleAttributeAlign)Style.Value;
                if (ResetAttribute.Value)
                {
                    switch (style)
                    {
                        case StyleAttributeAlign.AlignContent:
                            element.ResetAlignContent();
                            break;

                        case StyleAttributeAlign.AlignItems:
                            element.ResetAlignItems();
                            break;

                        case StyleAttributeAlign.AlignSelf:
                            element.ResetAlignSelf();
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    switch (style)
                    {
                        case StyleAttributeAlign.AlignContent:
                            element.SetAlignContent((Align)Value.Value);
                            break;

                        case StyleAttributeAlign.AlignItems:
                            element.SetAlignItems((Align)Value.Value);
                            break;

                        case StyleAttributeAlign.AlignSelf:
                            element.SetAlignSelf((Align)Value.Value);
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