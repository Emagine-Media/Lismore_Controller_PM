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
    public class UITKGetSibling : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [Tooltip("The sibling index delta. negative = up, positive = down")]
        public FsmInt IndexDelta = 1;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(VisualElementObject))]
        [Tooltip("Target variable where the result will be stored.\n" +
            "NOTICE: The value is wrapped by a VisualElementObj.")]
        public FsmObject StoreSibling;

        [Tooltip("If enabled then the Store variable is reused to preserve memory.\n" +
            "Only enable this if you see a need for it in the Profiler.")]
        public bool ReuseStoreVariable = false;


        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                int index = element.GetIndex() + IndexDelta.Value;

                if(index >= 0 && index < element.childCount)
                    StoreSibling.SetResultElement(element.ChildAt(index), ReuseStoreVariable);
                else
                    StoreSibling.SetResultElement(null, ReuseStoreVariable);

            }

            Finish();
        }
    }
}
#endif
