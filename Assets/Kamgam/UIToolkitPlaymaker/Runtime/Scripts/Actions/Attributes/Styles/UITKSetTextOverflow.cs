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
    public class UITKSetTextOverflow : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [ObjectType(typeof(TextOverflow))]
        public FsmEnum TextOverflow;

        [Tooltip("Reset using 'StyleKeyword.Null'?")]
        public FsmBool ResetAttribute = false;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        public override void Reset()
        {
            VisualElement = null;
            TextOverflow = default;
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
                    element.ResetOverflow();
                }
                else
                {
                    element.SetTextOverflow((TextOverflow)TextOverflow.Value);
                }
            }
        }
    }
}

#endif