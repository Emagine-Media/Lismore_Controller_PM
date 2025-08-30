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
    public class UITKGetBackgroundImage : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [UIHint(UIHint.Variable)]
        [ObjectType(typeof(BackgroundObject))]
        [Tooltip("Store the result in a variable.")]
        public FsmObject StoreResult;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        [Tooltip("Get the attribute value from the resolved style?")]
        public bool ResolvedStyle = true;

        [Tooltip("If enabled then the Store variable is reused to preserve memory.\n" +
            "Only enable this if you use this every frame or you see a need for it in the Profiler.")]
        public bool ReuseStoreVariable = false;

        public override void Reset()
        {
            VisualElement = null;
            StoreResult = null;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            getAttribute();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            getAttribute();
        }

        void getAttribute()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                StoreResult.SetResultStyleBackground(element.GetBackgroundImage(ResolvedStyle), ReuseStoreVariable);
            }

            StoreResult = null;
        }
    }
}

#endif