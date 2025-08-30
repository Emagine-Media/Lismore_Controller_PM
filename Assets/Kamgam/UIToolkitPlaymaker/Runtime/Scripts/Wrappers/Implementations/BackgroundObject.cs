#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Kamgam.UIToolkitPlaymaker
{
    /// <summary>
    /// A wrapper for a generic struct<br />
    /// <br />
    /// Since Playmaker variables can not store arbitrary types we have to wrap the content
    /// in a UnityEngie.Object, see: https://forum.unity.com/threads/playmaker-visual-scripting-for-unity.72349/page-70#post-9271821
    /// And: https://hutonggames.com/playmakerforum/index.php?topic=3996.0
    /// </summary>
    public class BackgroundObject : GenericStructObject<Background>
    {
        public static BackgroundObject CreateInstance(Background data)
        {
            var obj = ScriptableObject.CreateInstance<BackgroundObject>();
            obj.Data = data;
            return obj;
        }
    }

    public static class StyleBackgroundExtensions
    {
        public static void SetResultStyleBackground(this FsmObject Result, Background data, bool reuseResultVariable)
        {
            if (reuseResultVariable && Result != null && Result.Value != null)
            {
                var wrapper = Result.Value as BackgroundObject;
                if (wrapper != null)
                {
                    wrapper.Data = data;
                    return;
                }
            }

            Result.Value = BackgroundObject.CreateInstance(data);
        }
    }
}
#endif
