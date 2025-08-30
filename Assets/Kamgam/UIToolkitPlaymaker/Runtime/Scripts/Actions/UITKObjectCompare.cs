#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    [ActionCategory(ActionCategory.Logic)]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    [Tooltip("Compares two Object Variables using Equals() instead of the '==' operator and sends events based on the result.\n" +
        "Doing it that way we can compare anything that implements the IEquatable<T> interface.")]
    public class UITKObjectCompare : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable), Readonly]
        [Tooltip("The Object Variable to compare.")]
        public FsmObject ObjectVariable;

        [RequiredField]
        [Tooltip("The value to compare it to.")]
        public FsmObject CompareTo;

        [Tooltip("Event to send if the two visual element object values are equal.")]
        public FsmEvent EqualEvent;

        [Tooltip("Event to send if the two visual element object values are not equal.")]
        public FsmEvent NotEqualEvent;

        [UIHint(UIHint.Variable)]
        [Tooltip("Store the result in a variable.")]
        public FsmBool StoreResult;


        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        public override void Reset()
        {
            ObjectVariable = null;
            CompareTo = null;
            StoreResult = null;
            EqualEvent = null;
            NotEqualEvent = null;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            compare();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            compare();
        }

        void compare()
        {
            bool result;
            if (ObjectVariable.Value == null || CompareTo.Value == null)
                result = ObjectVariable.Value == CompareTo.Value;
            else
                result = ObjectVariable.Value.Equals(CompareTo.Value);

            StoreResult.Value = result;

            Fsm.Event(result ? EqualEvent : NotEqualEvent);
        }

#if UNITY_EDITOR
        public override string AutoName()
        {
            return "UITK Object Compare: " + ObjectVariable.Name + " <> " + CompareTo.Name;
        }
#endif
    }
}

#endif