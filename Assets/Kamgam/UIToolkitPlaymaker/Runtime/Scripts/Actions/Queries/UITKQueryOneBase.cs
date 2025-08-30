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
    /// <summary>
    /// Base for all actions that are querying the visual tree for one single element.
    /// </summary>
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public abstract class UITKQueryOneBase : UITKDocumentActionBase
    {
        [ActionSection("Query")]

        [ObjectType(typeof(UIElementType))]
        [Tooltip("Limit the query result to a specific type of ui element")]
        public FsmEnum ElementType;

        [Tooltip("Query by name (see 'Name' attribute in the UI Builder).")]
        public FsmString ElementName;

        [Tooltip("Query by class (see 'Style Class List' attribute in the UI Builder).")]
        public FsmString ElementClass;

        [Tooltip("Should the query result be cached and reused on subsequent STATE ENTER occurrences?\n" +
            "Enable this is you repeatedly enter the state and you do not expect the query result to change.")]
        public FsmBool CacheQueryResult = false;
        public bool _notCaching() => !CacheQueryResult.Value; // Sadly PlayMaker only discovers this if PUBLIC. Thus the weird name.

        [HideIf("_notCaching")]
        [Tooltip("Should the cache result be evaluated every time?\n" +
            "If disabled then it is only evaluated whenever the cache is renewed (i.e.: on the first STATE ENTER).")]
        public FsmBool EvaluateCacheHits = false;

        protected VisualElement _cachedVisualElement;
        protected bool _didCacheVisualElement;

        public void ClearCache()
        {
            _didCacheVisualElement = false;
            _cachedVisualElement = null;
        }

        public override void OnEnterWithDocument(UIDocument document)
        {
            if (CacheQueryResult.Value && _didCacheVisualElement)
            {
                if (EvaluateCacheHits.Value)
                {
                    OnElementQueried(_cachedVisualElement);
                }
                Finish();
                return;
            }

            var type = (UIElementType)ElementType.Value;
            var name = string.IsNullOrEmpty(ElementName.Value) ? null : ElementName.Value;
            var className = string.IsNullOrEmpty(ElementClass.Value) ? null : ElementClass.Value;

            var ele = document.rootVisualElement.QueryType(type, name, className);
            
            if (CacheQueryResult.Value)
            {
                _cachedVisualElement = ele;
                _didCacheVisualElement = true;
            }

            OnElementQueried(ele);

            if (!Finished)
                Finish();
        }

        public abstract void OnElementQueried(VisualElement element);

#if UNITY_EDITOR
        public override string AutoName()
        {
            string elementQueryHint = ElementType.Value.ToString();
            if (!string.IsNullOrEmpty(ElementName.Value))
            {
                elementQueryHint = ElementName.Value;
            }
            else if (!string.IsNullOrEmpty(ElementClass.Value))
            {
                elementQueryHint += ElementClass.Value;
            }

            return "UITK Query One: " + elementQueryHint;
        }
#endif
    }
}
#endif
