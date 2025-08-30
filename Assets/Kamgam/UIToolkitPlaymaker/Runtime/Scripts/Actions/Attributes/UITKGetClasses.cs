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
    public class UITKGetClasses : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(string))]
        [Tooltip("Variable to store the class names into.")]
        public FsmArray StoreClassNames;

        public override void OnEnter()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                var classNameList = element.GetAllClassesAsTemporaryList();

                var classNameArray = new string[classNameList.Count];
                for (int i = 0; i < classNameArray.Length; i++)
                {
                    classNameArray[i] = classNameList[i];
                }

                StoreClassNames.stringValues = classNameArray;
            }

            Finish();
        }
    }
}
#endif
