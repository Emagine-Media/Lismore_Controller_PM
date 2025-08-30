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
    public class UITKSetPosition : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [ObjectType(typeof(Position))]
        public FsmEnum Position = UnityEngine.UIElements.Position.Relative;

        [Tooltip("Reset using 'StyleKeyword.Null'?")]
        public FsmBool ResetAttribute = false;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        public override void Reset()
        {
            VisualElement = null;
            Position = UnityEngine.UIElements.Position.Relative;
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
                    element.ResetPosition();
                }
                else
                {
                    element.SetPosition((UnityEngine.UIElements.Position)Position.Value);
                }
            }
        }
    }
}

#endif