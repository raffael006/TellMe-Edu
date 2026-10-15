using System;
using UnityEngine;

namespace KawanDengarEdu.AR3D
{
    /// <summary>
    /// Simulasi pembusukan alami buah selama 5 menit (300 detik).
    /// Mengubah warna buah perlahan menjadi kecokelatan busuk dan memunculkan partikel lalat kartun.
    /// Dilengkapi metode FastForward untuk keperluan demonstrasi ke juri hackathon.
    /// </summary>
    public class FruitDecayTimer : MonoBehaviour
    {
        [Header("Decay Configuration")]
        [Tooltip("Durasi pembusukan penuh dalam detik (default 300s = 5 menit)")]
        public float fullDecayDurationSeconds = 300f;

        [Tooltip("Warna segar dasar buah")]
        public Color freshColor = new Color(0.94f, 0.27f, 0.27f); // Merah Fuji

        [Tooltip("Warna pembusukan akhir (cokelat tua busuk)")]
        public Color rottenColor = new Color(0.32f, 0.18f, 0.08f);

        [Header("Visual References")]
        [Tooltip("Renderer buah yang material warnanya akan diubah")]
        public Renderer[] fruitRenderers;

        [Tooltip("Efek partikel lalat kartun yang muncul saat membusuk")]
        public ParticleSystem fliesParticleEffect;

        [Header("Current State (Read Only)")]
        [SerializeField] private float timer = 0f;
        [SerializeField] private float decayProgress = 0f; // 0.0 s/d 1.0
        [SerializeField] private bool isDecaying = true;

        public float DecayProgress => decayProgress;
        public float RemainingTime => Mathf.Max(0f, fullDecayDurationSeconds - timer);

        public event Action<float> OnDecayProgressUpdated;
        public event Action OnFruitRotten;

        private MaterialPropertyBlock propertyBlock;
        private static readonly int ColorPropId = Shader.PropertyToID("_Color");
        private static readonly int DecayPropId = Shader.PropertyToID("_DecayProgress");

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Start()
        {
            ApplyVisualColors();
        }

        private void Update()
        {
            if (!isDecaying) return;

            timer += Time.deltaTime;
            decayProgress = Mathf.Clamp01(timer / fullDecayDurationSeconds);

            ApplyVisualColors();
            OnDecayProgressUpdated?.Invoke(decayProgress);

            // Munculkan lalat jika pembusukan di atas 60%
            if (fliesParticleEffect != null)
            {
                if (decayProgress >= 0.6f && !fliesParticleEffect.isPlaying)
                {
                    fliesParticleEffect.Play();
                }
                else if (decayProgress < 0.6f && fliesParticleEffect.isPlaying)
                {
                    fliesParticleEffect.Stop();
                }
            }

            if (decayProgress >= 1f && timer - Time.deltaTime < fullDecayDurationSeconds)
            {
                OnFruitRotten?.Invoke();
            }
        }

        public void ApplyVisualColors()
        {
            Color currentColor = Color.Lerp(freshColor, rottenColor, decayProgress);

            if (fruitRenderers != null)
            {
                foreach (var rend in fruitRenderers)
                {
                    if (rend != null)
                    {
                        rend.GetPropertyBlock(propertyBlock);
                        propertyBlock.SetColor(ColorPropId, currentColor);
                        propertyBlock.SetFloat(DecayPropId, decayProgress);
                        rend.SetPropertyBlock(propertyBlock);
                    }
                }
            }
        }

        public void SetFreshColor(Color newFreshColor)
        {
            freshColor = newFreshColor;
            ApplyVisualColors();
        }

        /// <summary>
        /// Pengujian cepat untuk demo juri: melompati timer ke kondisi membusuk 100%
        /// </summary>
        public void FastForwardDecay()
        {
            timer = fullDecayDurationSeconds;
            decayProgress = 1f;
            ApplyVisualColors();
            if (fliesParticleEffect != null && !fliesParticleEffect.isPlaying)
            {
                fliesParticleEffect.Play();
            }
            OnDecayProgressUpdated?.Invoke(decayProgress);
            OnFruitRotten?.Invoke();
        }

        /// <summary>
        /// Mengembalikan buah ke kondisi segar 100%
        /// </summary>
        public void ResetDecay()
        {
            timer = 0f;
            decayProgress = 0f;
            if (fliesParticleEffect != null)
            {
                fliesParticleEffect.Stop();
                fliesParticleEffect.Clear();
            }
            ApplyVisualColors();
            OnDecayProgressUpdated?.Invoke(decayProgress);
        }

        public void SetPauseDecay(bool pause)
        {
            isDecaying = !pause;
        }

        public string GetFormattedTimerString()
        {
            int rem = Mathf.CeilToInt(RemainingTime);
            int m = rem / 60;
            int s = rem % 60;
            return $"{m:D2}:{s:D2}";
        }
    }
}
