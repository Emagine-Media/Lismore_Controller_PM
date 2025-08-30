#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    [ActionCategory(ActionCategory.Logic)]
    [Tooltip("Checks if the element has the class. If yes then the 'Has' event is triggered. If not then the 'HasNot' event.")]
    public class UITKHasClass : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [RequiredField]
        [Tooltip("The class name to check for.")]
        public FsmString ClassName;

        [Tooltip("Event to send if it has the class.")]
        public FsmEvent HasEvent;

        [Tooltip("Event to send if it does not have the class.")]
        public FsmEvent HasNotEvent;

        [UIHint(UIHint.Variable)]
        [Tooltip("Store the result in a variable.")]
        public FsmBool StoreResult;


        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        public override void Reset()
        {
            VisualElement = null;
            StoreResult = null;
            HasEvent = null;
            HasNotEvent = null;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            checkClass();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            checkClass();
        }

        void checkClass()
        {
            bool result = false; ;
            if (VisualElement.TryGetVisualElement(out var element))
            {
                result = element.HasClass(ClassName.Value);
            }

            StoreResult.Value = result;

            Fsm.Event(result ? HasEvent : HasNotEvent);
        }
    }
}

#endif