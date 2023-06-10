using System;
using System.Linq;
using BoysheO.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;

namespace AddressablesImageSystem
{
    [CustomEditor(typeof(AddressablesImage))]
    public class AddressablesImageEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SpriteGUI();
        }

        private void SpriteGUI()
        {
            if (GUILayout.Button("SelectSprite"))
            {
                var selector = AddressablesImageSystem.SpriteSelector.ShowSelector();
                selector.Subscribe(new OnSpriteSelected((AddressablesImage)target));
            }
        }

        private sealed class OnSpriteSelected : IObserver<Sprite>
        {
            private readonly AddressablesImage _addressablesImage;

            public OnSpriteSelected(AddressablesImage addressablesImage)
            {
                _addressablesImage = addressablesImage;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(Sprite v)
            {
                if (v != null)
                {
                    (SpriteAtlas atlas, string spriteName) = GetAtlasAndSpriteNameBySprite(v);
                    if (atlas == null)
                    {
                        Debug.Log($"没有找到{v.name}的对应图集");
                        return;
                    }
                    else
                    {
                        _addressablesImage.SpriteKey = $"{atlas.name}[{spriteName}]";
                        _addressablesImage.UpdateImmediately();
                    }
                }
                else
                {
                    _addressablesImage.SpriteKey = null;
                }
            }
        }

        private static (SpriteAtlas atlas, string spriteName) GetAtlasAndSpriteNameBySprite(Sprite sprite)
        {
            var spriteName = sprite.name;
            if (spriteName.EndsWith("(Clone)")) spriteName = spriteName.RemoveAmountFromEnd("(Clone)");

            var atlas = AssetDatabase.FindAssets("t:SpriteAtlas")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<SpriteAtlas>)
                    .Select(v =>
                    {
                        if (v.CanBindTo(sprite)) return (v, order: 2);
                        var sp = v.GetSprite(spriteName);
                        bool hasSp = sp != null;
                        DestroyImmediate(sp);
                        return (v, order: hasSp ? 1 : 0);
                    })
                    .Where(v => v.order > 0)
                    .OrderByDescending(v => v.order)
                    .FirstOrDefault()
                    .v
                ;
            return (atlas, spriteName);
        }
    }
}