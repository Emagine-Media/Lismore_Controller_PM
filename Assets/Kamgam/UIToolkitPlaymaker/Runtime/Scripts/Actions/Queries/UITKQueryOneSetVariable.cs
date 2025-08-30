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
    public class UITKQueryOneSetVariable : UITKQueryOneBase
    {
        [ActionSection("Set Variable")]

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(VisualElementObject))]
        [Tooltip("Contains the queried VisualElementObj.\n" +
            "Actually it contains a FsmObject > VisualElementObj > Value, where Value = VisualElement")]
        public FsmObject StoreVisualElement;

        [Tooltip("If enabled then the Store variable is reused to preserve memory.\n" +
            "Only enable this if you see a need for it in the Profiler.")]
        public bool ReuseStoreVariable = false;

        public override void OnElementQueried(VisualElement element)
        {
            StoreVisualElement.SetResultElement(element, ReuseStoreVariable);
        }

#if UNITY_EDITOR
        public override string AutoName()
        {
            return base.AutoName() + " > " + StoreVisualElement.Name;
        }
#endif
    }
}
#endif
