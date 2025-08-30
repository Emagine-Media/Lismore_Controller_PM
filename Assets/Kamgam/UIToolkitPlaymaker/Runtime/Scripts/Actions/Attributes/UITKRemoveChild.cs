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
    public class UITKRemoveChild : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Paretn element.")]
        public FsmObject Parent;

        [UIHint(UIHint.Variable)]
        [Tooltip("OPTIONAL: Child element that should be removed from the parent.\n" +
            "Either a Child or an Index needs to be specified. If both are specified then the child will be used.")]
        public FsmObject Child;
        public bool _childIsSet()
        {
            return !Child.IsNone;
        }

        [Tooltip("OPTIONAL: At what index the child should be removed.\n" +
            "Either a Child or an Index needs to be specified. If both are specified then the child will be used.")]
        [HideIf("_childIsSet")]
        public FsmInt Index = -1;

        public override void OnEnter()
        {
            if (Parent.TryGetVisualElement(out var parent))
            {
                if (Child.TryGetVisualElement(out var child))
                {
                    parent.RemoveChild(child);
                }
                else if(Index.Value >= 0)
                {
                    parent.RemoveChildAt(Index.Value);
                }
            }

            Finish();
        }
    }
}
#endif
