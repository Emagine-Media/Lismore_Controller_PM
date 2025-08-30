#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    /// <summary>
    /// A wrapper for a Visual Element<br />
    /// <br />
    /// Since Playmaker variables can not store arbitrary types we have to wrap VisualElements
    /// in a UnityEngine.Object, see: https://forum.unity.com/threads/playmaker-visual-scripting-for-unity.72349/page-70#post-9271821
    /// </summary>
    public class VisualElementObject : ScriptableObject, IEquatable<VisualElementObject>
    {
        protected VisualElement _visualElement;
        public VisualElement VisualElement
        {
            get => _visualElement;

            set
            {
                if (_visualElement != value)
                {
                    _visualElement = value;
                    refreshName();
                }
            }
        }

        public static VisualElementObject CreateInstance(VisualElement visualElement)
        {
            var obj = ScriptableObject.CreateInstance<VisualElementObject>();
            obj.VisualElement = visualElement;
            return obj;
        }

        protected void refreshName()
        {
            if (!string.IsNullOrEmpty(VisualElement.name))
            {
                name = VisualElement.name;
            }
            else
            {
                name = VisualElement.GetType().Name;
            }
        }

        public override bool Equals(object obj) => Equals(obj as VisualElementObject);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = VisualElement.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VisualElementObject other)
        {
            return VisualElement.Equals(other.VisualElement);
        }
    }
}
#endif
