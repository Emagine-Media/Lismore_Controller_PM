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
    [Tooltip("Each time this action is called it gets the next child from the Parent. " +
             "This lets you quickly loop through all the items of a parent to perform actions on them.")]
    public class UITKGetNextChild : FsmStateAction
    {
        [RequiredField]
		[UIHint(UIHint.Variable)]
        [Tooltip("The Parent Variable to use.")]
		public FsmObject Parent;

		[Tooltip("From where to start iteration, leave as 0 to start from the beginning")]
		public FsmInt StartIndex;
		
		[Tooltip("When to end iteration, leave as 0 to iterate until the end")]
		public FsmInt EndIndex;
		
		[Tooltip("Event to send to get the next item.")]
		public FsmEvent LoopEvent;

        [Tooltip("If you want to reset the iteration, raise this flag to true when you enter the state, it will indicate you want to start from the beginning again")]
        [UIHint(UIHint.Variable)]
        public FsmBool ResetFlag;

        [Tooltip("Event to send when there are no more items.")]
		public FsmEvent FinishedEvent;
		

		[ActionSection("Result")]

		[UIHint(UIHint.Variable)]
        [ObjectType(typeof(VisualElementObject))]
        [Tooltip("Store the current item in a variable of the same type.")]
        public FsmObject Result;

		[UIHint(UIHint.Variable)]
        [Tooltip("Store the current index in an int variable.")]
		public FsmInt CurrentIndex;

        [Tooltip("If enabled then the Result variable is reused to preserve memory.\n" +
            "Only enable this if you see a need for it in the Profiler.")]
        public bool ReuseResultVariable = false;

        // increment that index as we loop through item
        private int nextChildIndex = 0;		
		
		public override void Reset()
		{		
			Parent = null;
			StartIndex = null;
			EndIndex = null;
			CurrentIndex = null;
			LoopEvent = null;
			FinishedEvent = null;
            ResetFlag = null;

            Result = null;
		}
		
		public override void OnEnter()
		{
			if (nextChildIndex == 0)
			{
				if (StartIndex.Value > 0)
				{
					nextChildIndex = StartIndex.Value;
				}
			}

            if (ResetFlag.Value)
            {
                nextChildIndex = StartIndex.Value;
                ResetFlag.Value = false;
            }

            DoGetNextItem();
			
			Finish();
		}
		
		
		void DoGetNextItem()
		{
            // Abort if parent is invalid.
            if (!Parent.TryGetVisualElement(out var parent))
            {
				CurrentIndex.Value = -1;
                Fsm.Event(FinishedEvent);
                return;
            }

            // Stop if no more children
            if (nextChildIndex >= parent.childCount)
			{
				nextChildIndex = 0;
				CurrentIndex.Value = parent.childCount - 1;
				Fsm.Event(FinishedEvent);
				return;
			}

            // Next
            Result.SetResultElement(parent.ChildAt(nextChildIndex), ReuseResultVariable);
			
			// Check again to avoid locks or possible infinite loop if the action is called again.
            if (nextChildIndex >= parent.childCount)
			{
				nextChildIndex = 0;
				CurrentIndex.Value = parent.childCount - 1;
				Fsm.Event(FinishedEvent);
				return;
			}
			
			if (EndIndex.Value>0 && nextChildIndex>= EndIndex.Value)
			{
				nextChildIndex = 0;
				CurrentIndex.Value = EndIndex.Value;
				Fsm.Event(FinishedEvent);
				return;
			}

            CurrentIndex.Value = nextChildIndex;
            nextChildIndex++;

			if (LoopEvent != null)
			{
				Fsm.Event(LoopEvent);
			}
		}

    }
}
#endif
