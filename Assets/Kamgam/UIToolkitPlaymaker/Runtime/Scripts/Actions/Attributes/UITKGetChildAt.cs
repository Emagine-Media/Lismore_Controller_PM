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
    public class UITKGetChildAt : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        public FsmInt Index;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(VisualElementObject))]
        [Tooltip("Target variable where the result will be stored.\n" +
            "NOTICE: The value is wrapped by a VisualElementObj.")]
        public FsmObject StoreChild;

        [Tooltip("If enabled then the Store variable is reused to preserve memory.\n" +
            "Only enable this if you see a need for it in the Profiler.")]
        public bool ReuseStoreVariable = false;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                StoreChild.SetResultElement(element.ChildAt(Index.Value), ReuseStoreVariable);
            }

            Finish();
        }
    }
}
#endif
