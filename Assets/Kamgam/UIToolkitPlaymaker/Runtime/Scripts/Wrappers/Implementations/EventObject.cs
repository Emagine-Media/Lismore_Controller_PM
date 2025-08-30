#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    /// <summary>
    /// A wrapper for an EventBase object.<br />
    /// <br />
    /// Since Playmaker variables can not store arbitrary types we have to wrap event data
    /// in a UnityEngine.Object, see: https://forum.unity.com/threads/playmaker-visual-scripting-for-unity.72349/page-70#post-9271821
    /// </summary>
    public class EventObject : ScriptableObject, IEquatable<EventObject>
    {
        protected EventBase _event;
        public EventBase Event
        {
            get => _event;

            set
            {
                if (_event != value)
                {
                    _event = value;
                    refreshName();
                }
            }
        }

        public static EventObject CreateInstance(EventBase evt)
        {
            var obj = ScriptableObject.CreateInstance<EventObject>();
            obj.Event = evt;
            return obj;
        }

        protected void refreshName()
        {
            if (Event != null)
            {
                var target = Event.target as VisualElement;
                if (target != null && !string.IsNullOrEmpty(target.name))
                {
                    name = target.name + " (" + Event.GetType().Name + ")";
                    return;
                }
            }

            name = null;
        }

        public override bool Equals(object obj) => Equals(obj as EventObject);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Event.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(EventObject other)
        {
            return Event.Equals(other.Event);
        }
        
        // Type Cast Event Getters
        public AttachToPanelEvent AttachToPanelEvent => Event as AttachToPanelEvent;
        public BlurEvent BlurEvent => Event as BlurEvent;
        public IChangeEvent IChangeEvent => Event as IChangeEvent;
        public ClickEvent ClickEvent => Event as ClickEvent;
        public ContextClickEvent ContextClickEvent => Event as ContextClickEvent;
        public ContextualMenuPopulateEvent ContextualMenuPopulateEvent => Event as ContextualMenuPopulateEvent;
        public CustomStyleResolvedEvent CustomStyleResolvedEvent => Event as CustomStyleResolvedEvent;
        public DetachFromPanelEvent DetachFromPanelEvent => Event as DetachFromPanelEvent;
        public FocusEvent FocusEvent => Event as FocusEvent;
        public FocusInEvent FocusInEvent => Event as FocusInEvent;
        public FocusOutEvent FocusOutEvent => Event as FocusOutEvent;
        public GeometryChangedEvent GeometryChangedEvent => Event as GeometryChangedEvent;
        public IMGUIEvent IMGUIEvent => Event as IMGUIEvent;
        public InputEvent InputEvent => Event as InputEvent;
        public KeyDownEvent KeyDownEvent => Event as KeyDownEvent;
        public KeyUpEvent KeyUpEvent => Event as KeyUpEvent;
        public MouseCaptureEvent MouseCaptureEvent => Event as MouseCaptureEvent;
        public MouseCaptureOutEvent MouseCaptureOutEvent => Event as MouseCaptureOutEvent;
        public MouseDownEvent MouseDownEvent => Event as MouseDownEvent;
        public MouseEnterEvent MouseEnterEvent => Event as MouseEnterEvent;
        public MouseEnterWindowEvent MouseEnterWindowEvent => Event as MouseEnterWindowEvent;
        public MouseLeaveEvent MouseLeaveEvent => Event as MouseLeaveEvent;
        public MouseLeaveWindowEvent MouseLeaveWindowEvent => Event as MouseLeaveWindowEvent;
        public MouseMoveEvent MouseMoveEvent => Event as MouseMoveEvent;
        public MouseOutEvent MouseOutEvent => Event as MouseOutEvent;
        public MouseOverEvent MouseOverEvent => Event as MouseOverEvent;
        public MouseUpEvent MouseUpEvent => Event as MouseUpEvent;
        public NavigationCancelEvent NavigationCancelEvent => Event as NavigationCancelEvent;
        public NavigationMoveEvent NavigationMoveEvent => Event as NavigationMoveEvent;
        public NavigationSubmitEvent NavigationSubmitEvent => Event as NavigationSubmitEvent;
        public PointerCancelEvent PointerCancelEvent => Event as PointerCancelEvent;
        public PointerCaptureEvent PointerCaptureEvent => Event as PointerCaptureEvent;
        public PointerCaptureOutEvent PointerCaptureOutEvent => Event as PointerCaptureOutEvent;
        public PointerDownEvent PointerDownEvent => Event as PointerDownEvent;
        public PointerEnterEvent PointerEnterEvent => Event as PointerEnterEvent;
        public PointerLeaveEvent PointerLeaveEvent => Event as PointerLeaveEvent;
        public PointerMoveEvent PointerMoveEvent => Event as PointerMoveEvent;
        public PointerOutEvent PointerOutEvent => Event as PointerOutEvent;
        public PointerOverEvent PointerOverEvent => Event as PointerOverEvent;
        public PointerStationaryEvent PointerStationaryEvent => Event as PointerStationaryEvent;
        public PointerUpEvent PointerUpEvent => Event as PointerUpEvent;
        public TooltipEvent TooltipEvent => Event as TooltipEvent;
        public TransitionCancelEvent TransitionCancelEvent => Event as TransitionCancelEvent;
        public TransitionEndEvent TransitionEndEvent => Event as TransitionEndEvent;
        public TransitionRunEvent TransitionRunEvent => Event as TransitionRunEvent;
        public TransitionStartEvent TransitionStartEvent => Event as TransitionStartEvent;
        public WheelEvent WheelEvent => Event as WheelEvent;
    }
}
#endif
