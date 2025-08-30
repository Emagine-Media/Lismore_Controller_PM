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
    public class UITKRegisterEvent : FsmStateAction
    {
        [ActionSection("Element Source")]

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement on which the event will the registered.")]
        public FsmObject VisualElement;


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
            if (VisualElement.TryGetVisualElement(out var ve))
            {
                _handler.Configure(Fsm, SendEvent, StoreEventData, PoolEventObjects);
                _handler.Register(ve, EventType);
            }
        }
    }
}
#endif
