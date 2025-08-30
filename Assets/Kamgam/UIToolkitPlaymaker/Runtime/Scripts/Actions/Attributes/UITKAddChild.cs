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
    public class UITKAddChild : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Parent element.")]
        public FsmObject Parent;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Child element that should be added to the parent.")]
        public FsmObject Child;

        [Tooltip("At what index the child should be added. If < 0 then the child will be appended.")]
        public FsmInt Index = -1;

        public override void OnEnter()
        {
            // First assume it's a VisualElement
            Child.TryGetVisualElement(out var visualElementChild);
            
            // Second try VisualTreeAsset
            VisualTreeAsset visualTreeAsset = null;
            if (visualElementChild == null)
            {
                Child.TryGetVisualTreeAsset(out visualTreeAsset);
            }
            
            if (Parent.TryGetVisualElement(out var parent)
                && (visualElementChild != null || visualTreeAsset != null))
            {
                VisualElement child;
                if (visualTreeAsset != null)
                {
                    child = visualTreeAsset.Instantiate();
                }
                else
                {
                    child = visualElementChild;
                }
                
                if (Index.Value >= 0)
                {
                    parent.InsertAt(Index.Value, child);
                }
                else
                {
                    parent.AddChild(child);
                }
            }

            Finish();
        }
    }
}
#endif
