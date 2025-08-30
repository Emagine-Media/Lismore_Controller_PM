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
    public class UITKQueryManySetVariable : UITKQueryManyBase
    {
        [ActionSection("Set Variable")]

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(VisualElementObject))]
        [Tooltip("Contains the queried VisualElementObjs.\n" +
                "Actually it contains an Array of FsmObject > VisualElementObj > Value, where Value = VisualElement")]
        public FsmArray StoreVisualElements;

        public override void OnElementsQueried(List<VisualElement> elements)
        {
            if (elements == null || elements.Count == 0)
                return;

            var list = new VisualElementObject[elements.Count];
            for (int i = 0; i < list.Length; i++)
            {
                list[i] = VisualElementObject.CreateInstance(elements[i]);
            }

            StoreVisualElements.Values = list;
        }

#if UNITY_EDITOR
        public override string AutoName()
        {
            return base.AutoName() + " > " + StoreVisualElements.Name;
        }
#endif
    }
}
#endif
