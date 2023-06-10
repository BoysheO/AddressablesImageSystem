using System;
using UnityEditor;
using UnityEngine;

namespace AddressablesImageSystem
{
    public sealed class SpriteSelector : EditorWindow, IObservable<Sprite>, IDisposable
    {
        private static SpriteSelector Instance;
        private IObserver<Sprite> _observer;

        private Sprite _sprite;
        private bool _isDestroy;

        public static SpriteSelector ShowSelector()
        {
            if (Instance)
            {
                Instance.Close();
            }

            Instance = GetWindow<SpriteSelector>("select sprite");
            Instance.maxSize = Instance.minSize = new Vector2(70, 80);
            Instance.ResetState();
            Instance.Show();
            return Instance;
        }

        private void ResetState()
        {
            _sprite = null;
            _observer?.OnCompleted();
            _observer = null;
        }

        private void OnGUI()
        {
            _sprite = (Sprite)EditorGUILayout.ObjectField("", _sprite, typeof(Sprite), false);
            if (GUILayout.Button("ok"))
            {
                if (_observer != null)
                {
                    _observer.OnNext(_sprite);
                }

                if (!_isDestroy) Close();
            }
        }

        public IDisposable Subscribe(IObserver<Sprite> observer)
        {
            if (_observer != null) throw new Exception("don't subscribe twice");
            _observer = observer;
            return this;
        }

        void IDisposable.Dispose()
        {
            if (this == null) return;
            _observer = null;
            if (!_isDestroy) Close();
        }

        private void OnDestroy()
        {
            _isDestroy = true;
            _observer?.OnCompleted();
            _observer = null;
            _sprite = null;
        }
    }
}