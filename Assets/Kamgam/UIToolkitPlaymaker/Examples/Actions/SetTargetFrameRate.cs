#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    //[ActionCategory("UI Toolkit")]
    public class SetTargetFrameRate : FsmStateAction
    {
        public int FrameRate = 60;

        public override void OnEnter()
        {
            Application.targetFrameRate = FrameRate;
            Finish();
        }
    }
}
#endif
