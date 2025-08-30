#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    public class VisualElementEventHandler
    {
        protected Dictionary<IEventHandler, Dictionary<Type, EventObject>> _eventPool = null;

        protected Fsm _fsm;
        protected FsmEvent _fsmEvent;
        protected FsmObject _storeEventData;
        protected bool _poolEventObjects;

        public void Configure(Fsm fsm, FsmEvent fsmEvent, FsmObject storeEventData, bool poolEventObjects)
        {
            _fsm = fsm;
            _fsmEvent = fsmEvent;
            _storeEventData = storeEventData;
            _poolEventObjects = poolEventObjects;
        }

        public void Register(VisualElement element, EventType eventType)
        {
            switch (eventType)
            {
                case EventType.AttachToPanel:
                    reregisterEvent<AttachToPanelEvent>(element, sendEvent);
                    break;
                case EventType.Blur:
                    reregisterEvent<BlurEvent>(element, sendEvent);
                    break;
                case EventType.ChangeFloat:
                    reregisterEvent<ChangeEvent<float>>(element, sendEvent);
                    break;
                case EventType.Click:
                    reregisterEvent<ClickEvent>(element, sendEvent);
                    break;
                case EventType.ContextClick:
                    reregisterEvent<ContextClickEvent>(element, sendEvent);
                    break;
                case EventType.ContextualMenuPopulate:
                    reregisterEvent<ContextualMenuPopulateEvent>(element, sendEvent);
                    break;
                case EventType.CustomStyleResolved:
                    reregisterEvent<CustomStyleResolvedEvent>(element, sendEvent);
                    break;
                case EventType.DetachFromPanel:
                    reregisterEvent<DetachFromPanelEvent>(element, sendEvent);
                    break;
                case EventType.Focus:
                    reregisterEvent<FocusEvent>(element, sendEvent);
                    break;
                case EventType.FocusIn:
                    reregisterEvent<FocusInEvent>(element, sendEvent);
                    break;
                case EventType.FocusOut:
                    reregisterEvent<FocusOutEvent>(element, sendEvent);
                    break;
                case EventType.GeometryChanged:
                    reregisterEvent<GeometryChangedEvent>(element, sendEvent);
                    break;
                case EventType.IMGUI:
                    reregisterEvent<IMGUIEvent>(element, sendEvent);
                    break;
                case EventType.Input:
                    reregisterEvent<InputEvent>(element, sendEvent);
                    break;
                case EventType.KeyDown:
                    reregisterEvent<KeyDownEvent>(element, sendEvent);
                    break;
                case EventType.KeyUp:
                    reregisterEvent<KeyUpEvent>(element, sendEvent);
                    break;
                case EventType.MouseCapture:
                    reregisterEvent<MouseCaptureEvent>(element, sendEvent);
                    break;
                case EventType.MouseCaptureOut:
                    reregisterEvent<MouseCaptureOutEvent>(element, sendEvent);
                    break;
                case EventType.MouseDown:
                    reregisterEvent<MouseDownEvent>(element, sendEvent);
                    break;
                case EventType.MouseEnter:
                    reregisterEvent<MouseEnterEvent>(element, sendEvent);
                    break;
                case EventType.MouseEnterWindow:
                    reregisterEvent<MouseEnterWindowEvent>(element, sendEvent);
                    break;
                case EventType.MouseLeave:
                    reregisterEvent<MouseLeaveEvent>(element, sendEvent);
                    break;
                case EventType.MouseLeaveWindow:
                    reregisterEvent<MouseLeaveWindowEvent>(element, sendEvent);
                    break;
                case EventType.MouseMove:
                    reregisterEvent<MouseMoveEvent>(element, sendEvent);
                    break;
                case EventType.MouseOut:
                    reregisterEvent<MouseOutEvent>(element, sendEvent);
                    break;
                case EventType.MouseOver:
                    reregisterEvent<MouseOverEvent>(element, sendEvent);
                    break;
                case EventType.MouseUp:
                    reregisterEvent<MouseUpEvent>(element, sendEvent);
                    break;
                case EventType.NavigationCancel:
                    reregisterEvent<NavigationCancelEvent>(element, sendEvent);
                    break;
                case EventType.NavigationMove:
                    reregisterEvent<NavigationMoveEvent>(element, sendEvent);
                    break;
                case EventType.NavigationSubmit:
                    reregisterEvent<NavigationSubmitEvent>(element, sendEvent);
                    break;
                case EventType.PointerCancel:
                    reregisterEvent<PointerCancelEvent>(element, sendEvent);
                    break;
                case EventType.PointerCapture:
                    reregisterEvent<PointerCaptureEvent>(element, sendEvent);
                    break;
                case EventType.PointerCaptureOut:
                    reregisterEvent<PointerCaptureOutEvent>(element, sendEvent);
                    break;
                case EventType.PointerDown:
                    reregisterEvent<PointerDownEvent>(element, sendEvent);
                    break;
                case EventType.PointerEnter:
                    reregisterEvent<PointerEnterEvent>(element, sendEvent);
                    break;
                case EventType.PointerLeave:
                    reregisterEvent<PointerLeaveEvent>(element, sendEvent);
                    break;
                case EventType.PointerMove:
                    reregisterEvent<PointerMoveEvent>(element, sendEvent);
                    break;
                case EventType.PointerOut:
                    reregisterEvent<PointerOutEvent>(element, sendEvent);
                    break;
                case EventType.PointerOver:
                    reregisterEvent<PointerOverEvent>(element, sendEvent);
                    break;
#if !UNITY_6000_0_OR_NEWER
                case EventType.PointerStationary:
                    reregisterEvent<PointerStationaryEvent>(element, sendEvent);
                    break;
#endif
                case EventType.PointerUp:
                    reregisterEvent<PointerUpEvent>(element, sendEvent);
                    break;
                case EventType.Tooltip:
                    reregisterEvent<TooltipEvent>(element, sendEvent);
                    break;
                case EventType.TransitionCancel:
                    reregisterEvent<TransitionCancelEvent>(element, sendEvent);
                    break;
                case EventType.TransitionEnd:
                    reregisterEvent<TransitionEndEvent>(element, sendEvent);
                    break;
                case EventType.TransitionRun:
                    reregisterEvent<TransitionRunEvent>(element, sendEvent);
                    break;
                case EventType.TransitionStart:
                    reregisterEvent<TransitionStartEvent>(element, sendEvent);
                    break;
                case EventType.Wheel:
                    reregisterEvent<WheelEvent>(element, sendEvent);
                    break;
                default:
                    break;
            }
        }

        protected void reregisterEvent<TEventType>(VisualElement element, EventCallback<TEventType> callback, TrickleDown useTrickleDown = TrickleDown.NoTrickleDown) where TEventType : EventBase<TEventType>, new()
        {
            element.UnregisterCallback<TEventType>(callback);
            element.RegisterCallback<TEventType>(callback);
        }

        protected void sendEvent(EventBase evt)
        {
            // Create or fetch from pool
            EventObject eventObj;
            var type = evt.GetType();

            // Reuse from pool?
            if (_poolEventObjects && _eventPool.ContainsKey(evt.target) && _eventPool[evt.target].ContainsKey(type))
            {
                // Reuse
                eventObj = _eventPool[evt.target][type];
                eventObj.Event = evt;
            }
            else
            {
                eventObj = EventObject.CreateInstance(evt);

                // Store in pool
                if (_poolEventObjects)
                {
                    if (_eventPool == null)
                    {
                        _eventPool = new Dictionary<IEventHandler, Dictionary<Type, EventObject>>();
                    }

                    if (!_eventPool.ContainsKey(evt.target))
                    {
                        var types = new Dictionary<Type, EventObject>();
                        types.Add(type, eventObj);
                        _eventPool.Add(evt.target, types);
                    }
                    else if (!_eventPool[evt.target].ContainsKey(type))
                    {
                        _eventPool[evt.target].Add(type, eventObj);
                    }
                }
            }

            // Store in variable
            if (_storeEventData != null)
                _storeEventData.Value = eventObj;

            // Trigger event
            _fsm.Event(_fsmEvent);
        }

    }

}
#endif
