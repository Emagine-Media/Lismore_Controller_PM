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
    public class UITKRegisterEvents : FsmStateAction
    {
        [ActionSection("Elements Source")]

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElements on which the events will the registered.")]
        public FsmArray VisualElements;


        [ActionSection("Event")]

        [RequiredField]
        [Tooltip("The event type that should be registered.")]
        public EventType EventType = EventType.Click;

        [Tooltip("The event that should be triggered.")]
        public FsmEvent SendEvent;

        [UIHint(UIHint.Variable)]
        [Tooltip("Contains the data of the last event that was triggered.")]
        public FsmObject StoreEventData;

        [Tooltip("If enabled then event objects are reused the next time the event is triggered.\n" +
                "Enable only if you see demand for it in the profiler.")]
        public bool PoolEventObjects = false;

        protected VisualElementEventHandler _handler = new VisualElementEventHandler();

        public override void OnEnter()
        {
            if (VisualElements == null || VisualElements.Values == null || VisualElements.Values.Length == 0)
                return;

            _handler.Configure(Fsm, SendEvent, StoreEventData, PoolEventObjects);

            foreach (var eleObj in VisualElements.Values)
            {
                var element = eleObj as VisualElementObject;
                if (!element.HasValue())
                    continue;

                _handler.Register(element.VisualElement, EventType);
            }
        }
    }
}
#endif
