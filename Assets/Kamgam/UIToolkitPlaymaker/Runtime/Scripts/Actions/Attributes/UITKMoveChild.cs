#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    [ActionCategory("UI Toolkit")]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public class UITKMoveChild : FsmStateAction
    {
        public enum MoveToAction
        {
            Front,
            Back,
            /// <summary>
            /// Alias for Back
            /// </summary>
            First,
            /// <summary>
            /// Alias for Front
            /// </summary>
            Last,
            Up,
            Down
        }

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Child element that should be added to the parent.")]
        public FsmObject Child;

        [Tooltip("At what index the child should be added. If < 0 then the child will be appended.")]
        [ObjectType(typeof(MoveToAction))]
        public FsmEnum MoveTo = MoveToAction.Front;

        public override void OnEnter()
        {
            if (Child.TryGetVisualElement(out var child))
            {
                var moveTo = (MoveToAction) MoveTo.Value;
                switch (moveTo)
                {
                    case MoveToAction.Front:
                    case MoveToAction.Last:
                        child.MoveToFront();
                        break;
                    case MoveToAction.Back:
                    case MoveToAction.First:
                        child.MoveToBack();
                        break;
                    case MoveToAction.Up:
                        child.MoveUp();
                        break;
                    case MoveToAction.Down:
                        child.MoveDown();
                        break;
                    default:
                        break;
                }
            }

            Finish();
        }
    }
}
#endif
