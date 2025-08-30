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
    public class FontDefinitionObject : GenericStructObject<FontDefinition>
    {
        public static FontDefinitionObject CreateInstance(FontDefinition data)
        {
            var obj = ScriptableObject.CreateInstance<FontDefinitionObject>();
            obj.Data = data;
            return obj;
        }
    }

    public static class FontDefinitionExtensions
    {
        public static void SetResultFontDefinition(this FsmObject Result, FontDefinition data, bool reuseResultVariable)
        {
            if (reuseResultVariable && Result != null && Result.Value != null)
            {
                var wrapper = Result.Value as FontDefinitionObject;
                if (wrapper != null)
                {
                    wrapper.Data = data;
                    return;
                }
            }

            Result.Value = FontDefinitionObject.CreateInstance(data);
        }
    }
}
#endif
