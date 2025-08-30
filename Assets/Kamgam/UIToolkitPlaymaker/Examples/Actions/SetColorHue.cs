#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    //[ActionCategory("UI Toolkit")]
    public class SetColorHue : FsmStateAction
    {
        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmColor Color;

        [Tooltip("Hue in units per second.")]
        public FsmFloat Hue;

        public override void OnEnter()
        {
            UnityEngine.Color.RGBToHSV(Color.Value, out var _, out var s, out var v);
            Color.Value = UnityEngine.Color.HSVToRGB(Hue.Value % 1f, s, v);

            Finish();
        }
    }
}
#endif
