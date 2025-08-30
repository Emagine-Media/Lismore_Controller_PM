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
    public class UITKQueryManyRegisterEvent : UITKQueryManyBase
    {
        [ActionSection("Event")]

        [RequiredField]
        [Tooltip("The event which this should listen for.")]
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

        public override void OnElementsQueried(List<VisualElement> elements)
        {
            if (elements == null || elements.Count == 0)
                return;

            _handler.Configure(Fsm, SendEvent, StoreEventData, PoolEventObjects);

            foreach (var element in elements)
            {
                _handler.Register(element, EventType);
            }
        }
    }
}
#endif
