#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    public static class ObjExtensions
    {
        public static bool HasValue(this FsmObject obj)
        {
            return obj != null && obj.Value != null;
        }

        public static bool HasValue<T>(this FsmObject obj) where T : class
        {
            if (obj == null || obj.Value == null)
                return false;

            var typedValue = obj as T;
            if (typedValue != null)
                return true;

            return false;
        }

        public static bool HasValue(this VisualElementObject obj)
        {
            return obj != null && obj.VisualElement != null;
        }

        public static T GetWrapper<T>(this FsmObject obj) where T : class
        {
            if (obj == null || obj.Value == null)
                return null;

            var typedValue = obj as T;
            if (typedValue != null)
                return typedValue;

            return null;
        }

        public static bool TryGetWrapper<T>(this FsmObject obj, out T value) where T : class
        {
            if (obj == null || obj.Value == null)
            {
                value = null;
                return false;
            }

            var typedValue = obj as T;
            if (typedValue != null)
            {
                value = typedValue;
                return true;
            }

            value = null;
            return false;
        }

        public static bool TryGetVisualElement(this FsmObject fsmObj, out VisualElement element)
        {
            if (!fsmObj.HasValue())
            {
                element = null;
                return false;
            }

            var wrapper = fsmObj.Value as VisualElementObject;
            if (!wrapper.HasValue())
            {
                element = null;
                return false;
            }

            element = wrapper.VisualElement;
            return true;
        }
        
        public static bool TryGetVisualTreeAsset(this FsmObject fsmObj, out VisualTreeAsset asset)
        {
            if (!fsmObj.HasValue())
            {
                asset = null;
                return false;
            }

            asset = fsmObj.Value as VisualTreeAsset;

            return asset != null;
        }
    }
}
#endif
