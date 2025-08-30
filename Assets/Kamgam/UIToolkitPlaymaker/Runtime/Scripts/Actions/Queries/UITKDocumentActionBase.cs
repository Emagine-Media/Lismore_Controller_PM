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
    /// This class is the base for all actions that need to access a UI Document. It fetches the document from a game object.
    /// </summary>
#if UNITY_EDITOR
    [HelpUrl(Installer.ManualUrl)]
#endif
    public abstract class UITKDocumentActionBase : FsmStateAction
    {
        [ActionSection("Document")]

        public FsmOwnerDefault UIDocumentSource;

        [HideIf("_hideDocument")]
        [ObjectType(typeof(UIDocument))]
        [Tooltip("Optional: If not specified then the UIDocument will be fetched from the 'Game Object' field via GetComponent().")]
        public FsmObject Document;

        protected UIDocument _cachedDocument;
        protected bool _didCacheDocument = false;
        public bool _hideDocument()
        {
            return (UIDocumentSource != null && UIDocumentSource.OwnerOption == OwnerDefaultOption.UseOwner) || (UIDocumentSource.OwnerOption == OwnerDefaultOption.SpecifyGameObject && UIDocumentSource.GameObject.Value != null);
        }

        public void ClearDocumentCache()
        {
            _didCacheDocument = false;
            _cachedDocument = null;
        }

        public override void OnEnter()
        {
            if(_didCacheDocument)
            {
                OnEnterWithDocument(_cachedDocument);
                return;
            }

            bool goIsNull = false;
            if (UIDocumentSource.OwnerOption == OwnerDefaultOption.SpecifyGameObject && (UIDocumentSource == null || UIDocumentSource.GameObject == null || UIDocumentSource.GameObject.Value == null))
            {
                goIsNull = true;
            }

            UIDocument doc = null;

            bool docIsNull = false;
            if (Document == null || Document.Value == null)
            {
                docIsNull = true;
            }

            if (goIsNull && docIsNull)
            {
                // Both are null then abort.
                Finish();
                return;
            }

            if (docIsNull)
            {
                var go = UIDocumentSource.OwnerOption == OwnerDefaultOption.SpecifyGameObject ? UIDocumentSource.GameObject.Value : Owner;
                doc = go.GetComponent<UIDocument>();
            }
            else
            {
                doc = Document.Value as UIDocument;
            }

            // If doc is still null then abort.
            if (doc == null)
            {
                Finish();
                return;
            }

            _didCacheDocument = true;
            _cachedDocument = doc;

            OnEnterWithDocument(_cachedDocument);

            if(!Finished)
                Finish();
        }

        public abstract void OnEnterWithDocument(UIDocument document);

    }
}
#endif
