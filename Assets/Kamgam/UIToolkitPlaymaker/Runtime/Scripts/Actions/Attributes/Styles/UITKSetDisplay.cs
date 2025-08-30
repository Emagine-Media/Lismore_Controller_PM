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
    [Tooltip("Gets the visible attribute and stores it in a variable.")]
    public class UITKSetDisplay : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [ObjectType(typeof(DisplayStyle))]
        public FsmEnum Display = DisplayStyle.Flex;

        [Tooltip("Reset using 'StyleKeyword.Null'?")]
        public FsmBool ResetAttribute = false;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        public override void Reset()
        {
            VisualElement = null;
            Display = DisplayStyle.Flex;
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
                    element.ResetDisplay();
                }
                else
                {
                    element.SetDisplay((DisplayStyle)Display.Value);
                }
            }
        }
    }
}

#endif