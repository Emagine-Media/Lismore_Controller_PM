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
    public class UITKSetBackgroundImage : FsmStateAction
    {
        public enum ImageSource { Definition, Sprite, Texture, RenderTexture, VectorImage }

        [RequiredField]
        [UIHint(UIHint.Variable)]
        [Tooltip("Source of the VisualElement.")]
        public FsmObject VisualElement;

        [ActionSection("Value")]

        [Tooltip("There are several possible sources. Choose one.")]
        public ImageSource Source = ImageSource.Sprite;

        [Tooltip("Source of the VisualElement.")]
        [HideIf("_sourceIsNotDefinition")]
        [ObjectType(typeof(BackgroundObject))]
        public FsmObject BackgroundDefinition;
        public bool _sourceIsNotDefinition() => Source != ImageSource.Definition;

        [HideIf("_sourceIsNotSprite")]
        [ObjectType(typeof(Sprite))]
        public FsmObject Sprite;
        public bool _sourceIsNotSprite() => Source != ImageSource.Sprite;

        [HideIf("_sourceIsNotTexture")]
        public FsmTexture Texture;
        public bool _sourceIsNotTexture() => Source != ImageSource.Texture;

        [HideIf("_sourceIsNotRenderTexture")]
        [ObjectType(typeof(RenderTexture))]
        public FsmObject RenderTexture;
        public bool _sourceIsNotRenderTexture() => Source != ImageSource.RenderTexture;

        [HideIf("_sourceIsNotVectorImage")]
        [ObjectType(typeof(VectorImage))]
        public FsmObject VectorImage;
        public bool _sourceIsNotVectorImage() => Source != ImageSource.VectorImage;

        [ActionSection("Options")]

        [Tooltip("Reset using 'StyleKeyword.Null'?")]
        public FsmBool ResetAttribute = false;

        [Tooltip("Repeat every frame.")]
        public bool everyFrame;

        public override void Reset()
        {
            VisualElement = null;
            BackgroundDefinition = default;
            everyFrame = false;
        }

        public override void OnEnter()
        {
            setAttribute();

            if (!everyFrame)
            {
                Finish();
            }
        }

        public override void OnUpdate()
        {
            setAttribute();
        }

        void setAttribute()
        {
            if (VisualElement.TryGetVisualElement(out var element))
            {
                if (ResetAttribute.Value)
                {
                    element.ResetFontDefinition();
                }
                else
                {
                    switch (Source)
                    {
                        case ImageSource.Definition:
                            if (BackgroundDefinition.TryGetWrapper<BackgroundObject>(out var wrapper))
                            {
                                element.SetBackgroundImage(wrapper.Data);
                            }
                            break;
                        case ImageSource.Sprite:
                            element.SetBackgroundImage(Sprite.Value as Sprite);
                            break;
                        case ImageSource.Texture:
                            element.SetBackgroundImage(Texture.Value as Texture2D);
                            break;
                        case ImageSource.RenderTexture:
                            element.SetBackgroundImage(RenderTexture.Value as RenderTexture);
                            break;
                        case ImageSource.VectorImage:
                            element.SetBackgroundImage(VectorImage.Value as VectorImage);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}

#endif