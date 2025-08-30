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
    public class UITKGetChildren : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Contains the queried VisualElementObjs.\n" +
        "NOTICE: Actually it contains an Array of FsmObject > VisualElementObj > Value, where Value = VisualElement")]
        public FsmArray StoreChildren;

        protected List<VisualElement> _tmpChildren = new List<VisualElement>();

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                _tmpChildren.Clear();
                foreach (var ele in element.Children())
                {
                    _tmpChildren.Add(ele);
                }

                var list = new VisualElementObject[_tmpChildren.Count];
                for (int i = 0; i < list.Length; i++)
                {
                    list[i] = VisualElementObject.CreateInstance(_tmpChildren[i]);
                }
                _tmpChildren.Clear();

                StoreChildren.Values = list;
            }
                

            Finish();
        }
    }
}
#endif
