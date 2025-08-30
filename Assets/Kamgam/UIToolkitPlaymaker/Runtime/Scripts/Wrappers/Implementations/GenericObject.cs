#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    /// <summary>
    /// A wrapper for a generic object<br />
    /// <br />
    /// Since Playmaker variables can not store arbitrary types we have to wrap the content
    /// in a UnityEngie.Object, see: https://forum.unity.com/threads/playmaker-visual-scripting-for-unity.72349/page-70#post-9271821
    /// </summary>
    public class GenericObject : ScriptableObject, IEquatable<GenericObject>
    {
        public object Data;

        public static GenericObject CreateInstance(object data)
        {
            var obj = ScriptableObject.CreateInstance<GenericObject>();
            obj.Data = data;
            return obj;
        }

        public override bool Equals(object obj) => Equals(obj as GenericObject);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Data.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(GenericObject other)
        {
            return Data.Equals(other.Data);
        }
    }
}
#endif
