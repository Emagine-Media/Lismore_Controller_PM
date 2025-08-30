#if PLAYMAKER
using Unity.VisualScripting;

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    public static class UIToolkitEventExtensions
    {
        public static VisualElement GetTarget(this EventBase evt)
        {
            return evt.target as VisualElement;
        }
    }
}
#endif