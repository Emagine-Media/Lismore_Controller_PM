#if PLAYMAKER
using HutongGames.PlayMaker;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using Tooltip = HutongGames.PlayMaker.TooltipAttribute;

namespace Kamgam.UIToolkitPlaymaker
{
    [ActionCategory("UI Toolkit")]
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public class UITKWaitForDocumentLayout : UITKDocumentActionBase
    {
        [Tooltip("The event that should be triggered.")]
        public FsmEvent SendEvent;

        protected bool _triggered = false;

        protected UIDocument _doc;

        public override void OnEnterWithDocument(UIDocument document)
        {
            if (_triggered)
                return;

            _doc = document;

            // According to Unity the GeometryChanged event is sent "after layout calculations".
            // Source: https://docs.unity3d.com/ScriptReference/UIElements.GeometryChangedEvent.html
            document.rootVisualElement.RegisterCallback<GeometryChangedEvent>(onGeometryChanged);

            Finish();
        }

        protected void onGeometryChanged(GeometryChangedEvent evt)
        {
            _triggered = true;

            _doc.rootVisualElement.UnregisterCallback<GeometryChangedEvent>(onGeometryChanged);
            _doc = null;

            Fsm.Event(SendEvent);
        }
    }
}
#endif
