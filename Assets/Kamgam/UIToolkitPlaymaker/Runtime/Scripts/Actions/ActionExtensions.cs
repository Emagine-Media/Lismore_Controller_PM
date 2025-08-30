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
    public static class ActionExtensions
    {
        /// <summary>
        /// Used to set the inner values of a custom type wrapper.
        /// </summary>
        /// <param name="Result"></param>
        /// <param name="element"></param>
        /// <param name="reuseResultVariable"></param>
        public static void SetResultElement(this FsmObject Result, VisualElement element, bool reuseResultVariable)
        {
            if (reuseResultVariable && Result != null && Result.Value != null)
            {
                VisualElementObject wrapper = Result.Value as VisualElementObject;
                if (wrapper != null)
                {
                    wrapper.VisualElement = element;
                    return;
                }
            }

            Result.Value = VisualElementObject.CreateInstance(element);
        }

        public static void SetResultEvent(this FsmObject Result, EventBase evt, bool reuseResultVariable)
        {
            if (reuseResultVariable && Result != null && Result.Value != null)
            {
                EventObject wrapper = Result.Value as EventObject;
                if (wrapper != null)
                {
                    wrapper.Event = evt;
                    return;
                }
            }

            Result.Value = GenericObject.CreateInstance(evt);
        }

        public static void SetResultGeneric(this FsmObject Result, object data, bool reuseResultVariable)
        {
            if (reuseResultVariable && Result != null && Result.Value != null)
            {
                GenericObject wrapper = Result.Value as GenericObject;
                if (wrapper != null)
                {
                    wrapper.Data = data;
                    return;
                }
            }

            Result.Value = GenericObject.CreateInstance(data);
        }
    }
}
#endif
