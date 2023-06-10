using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace AddressablesImageSystem
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public class AddressablesImage : MonoBehaviour
    {
        public string SpriteKey;
        public bool IsLoadAsync = false;
        private string _spriteHandleKey;
        private AsyncOperationHandle<Sprite>? _spriteHandle;

        private void Update()
        {
            UpdateImmediately();
        }

        public void UpdateImmediately()
        {
            if (string.IsNullOrWhiteSpace(SpriteKey))
            {
                ReleaseResource();
            }
            else
            {
                if (_spriteHandleKey == SpriteKey)
                {
                    //do nothing
                }
                else
                {
                    ReleaseResource();
                    _spriteHandleKey = SpriteKey;
                    _spriteHandle = Addressables.LoadAssetAsync<Sprite>(SpriteKey);
                    if (!Application.isPlaying || !IsLoadAsync)
                    {
                        _spriteHandle.Value.WaitForCompletion();
                        OnTexLoaded(_spriteHandle.Value);
                    }
                    else
                    {
                        _spriteHandle.Value.Completed += OnTexLoaded;
                    }
                }
            }
        }

        private void OnTexLoaded(AsyncOperationHandle<Sprite> handle)
        {
            if (handle.OperationException != null) return;
            var img = GetComponent<Image>();
            if (img)
            {
                img.sprite = handle.Result;
            }
        }

        private void ReleaseResource()
        {
            if (_spriteHandle == null || !_spriteHandle.Value.IsValid()) return;
            var sp = _spriteHandle.Value.Result;
            var img = GetComponent<Image>();
            if (img && img.sprite == sp)
            {
                img.sprite = null;
            }

            Addressables.Release(_spriteHandle.Value);
            _spriteHandle = null;
            _spriteHandleKey = null;
        }

        private void OnDestroy()
        {
            if (_spriteHandle != null && _spriteHandle.Value.IsValid())
            {
                ReleaseResource();
            }
        }
    }
}