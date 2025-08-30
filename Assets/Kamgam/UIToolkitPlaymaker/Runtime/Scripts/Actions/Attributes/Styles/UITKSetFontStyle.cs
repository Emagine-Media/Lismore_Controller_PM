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
    public class UITKSetFontStyle : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [ObjectType(typeof(FontStyle))]
        public FsmEnum FontStyle;

        [Tooltip("Reset using 'StyleKeyword.Null'?")]
        public FsmBool ResetAttribute = false;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        public override void Reset()
        {
            VisualElement = null;
            FontStyle = default;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            setAttribute();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            setAttribute();
        }

        void setAttribute()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                if (ResetAttribute.Value)
                {
                    element.ResetFontStyle();
                }
                else
                {
                    element.SetFontStyle((FontStyle)FontStyle.Value);
                }
            }
        }
    }
}

#endif