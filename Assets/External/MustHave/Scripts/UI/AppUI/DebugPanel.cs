using UnityEngine;
using UnityEngine.UI;

namespace MustHave.UI
{
    public class DebugPanel : UIScript
    {
        [SerializeField] private Text _fpsText = default;
        [SerializeField] private Text _debugText = default;

        private float _fps = default;

        public string DebugText { set { _debugText.text = value; } }

        public void SetDebugText(string text)
        {
            _debugText.text = text;
        }

        private void Update()
        {
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            if (unscaledDeltaTime > 0f)
            {
                _fps = Mathf.Lerp(_fps, 1f / unscaledDeltaTime, 2f * unscaledDeltaTime);
                _fpsText.text = "FPS: " + _fps.ToString("F2");
            }
        }
    }
}
