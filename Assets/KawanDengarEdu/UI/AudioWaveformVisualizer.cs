using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KawanDengarEdu.UI
{
    /// <summary>
    /// Visualizer gelombang suara 12 bar (Audio Waveform)
    /// Memberikan feedback visual yang jelas bagi anak tunarungu bahwa mikrofon mendeteksi suara guru.
    /// </summary>
    public class AudioWaveformVisualizer : MonoBehaviour
    {
        [Header("Waveform Bars")]
        public List<RectTransform> bars = new List<RectTransform>();

        [Header("Animation Settings")]
        public float minHeight = 6f;
        public float maxHeight = 40f;
        public float animationSpeed = 10f;
        public bool isAudioActive = false;

        private float[] targetHeights;

        private void Start()
        {
            if (bars.Count > 0)
            {
                targetHeights = new float[bars.Count];
            }
        }

        private void Update()
        {
            if (bars == null || bars.Count == 0) return;

            for (int i = 0; i < bars.Count; i++)
            {
                if (bars[i] == null) continue;

                if (isAudioActive)
                {
                    // Simulasi gelombang acak suara dinamis
                    float noise = Mathf.PerlinNoise(Time.time * animationSpeed, i * 0.4f);
                    float targetH = Mathf.Lerp(minHeight, maxHeight, noise);
                    Vector2 size = bars[i].sizeDelta;
                    size.y = Mathf.Lerp(size.y, targetH, Time.deltaTime * animationSpeed);
                    bars[i].sizeDelta = size;
                }
                else
                {
                    // Tenang saat tidak ada suara
                    Vector2 size = bars[i].sizeDelta;
                    size.y = Mathf.Lerp(size.y, minHeight, Time.deltaTime * 8f);
                    bars[i].sizeDelta = size;
                }
            }
        }

        public void SetAudioActive(bool active)
        {
            isAudioActive = active;
        }
    }
}
