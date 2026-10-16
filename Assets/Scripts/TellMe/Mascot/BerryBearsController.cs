using System;
using System.Collections;
using TellMe.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TellMe.Mascot
{
    /// <summary>
    /// Modul 5: Maskot Interaktif "BerryBears" (Karakter 3D Clay We Bare Bears).
    /// Fitur upgrade:
    /// 1. Berkedip otomatis (Eye Blinking secara organik setiap 3-5 detik).
    /// 2. Bola mata bergerak melihat segala arah (Eye Look-around: atas, bawah, kiri, kanan, tengah).
    /// 3. Berpindah/bergoyang dinamis ke kiri dan ke kanan kapan pun waktunya (Horizontal Sway / Wander).
    /// 4. Respon klik instan (On-Tap Response) & balon teks informatif ramah anak SD.
    /// </summary>
    public class BerryBearsController : MonoBehaviour, IPointerClickHandler
    {
        public static BerryBearsController Instance { get; private set; }

        [Header("Komponen Visual & Animasi 3D BerryBears")]
        [SerializeField] private Animator mascotAnimator;
        [SerializeField] private Transform mascotModelTransform;
        [SerializeField] private Transform leftPupilTransform;
        [SerializeField] private Transform rightPupilTransform;
        [SerializeField] private GameObject eyelidLidObj;
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private ParticleSystem tapSparkleParticles;

        [Header("Komponen Balon Teks (Speech Bubble)")]
        [SerializeField] private GameObject speechBubbleContainer;
        [SerializeField] private Text speechBubbleText;
        [SerializeField] private Image speechBubbleBackground;

        [Header("State Saat Ini")]
        [SerializeField] private BerryBearsState currentState = BerryBearsState.STATE_IDLE;

        [Header("Parameter Animasi Hidup")]
        [SerializeField] private bool enableEyeTracking = true;
        [SerializeField] private bool enableHorizontalWander = true;
        [SerializeField] private float wanderRangeX = 24f;

        private readonly string[] onTapGreetings = new string[]
        {
            "Halo Raka! Siap belajar bareng BerryBears?",
            "Kamu hebat! Ketuk aku kapan saja kalau butuh bantuan ya!",
            "Isyarat tangan hari ini seru banget! Ayo semangat!",
            "Waktunya Belajar! Tekan tombol kuning untuk mulai ya!"
        };

        private int tapGreetingIndex = 0;
        private Coroutine bounceRoutine;
        private Coroutine blinkRoutine;
        private Coroutine eyeLookRoutine;
        private Coroutine wanderRoutine;
        private Vector3 initialPosition;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void SetupComponents(
            Transform mascotTransform, Transform leftPupil, Transform rightPupil, GameObject eyelid,
            GameObject bubbleContainer, Text bubbleText, Image bubbleBg)
        {
            mascotModelTransform = mascotTransform;
            leftPupilTransform = leftPupil;
            rightPupilTransform = rightPupil;
            eyelidLidObj = eyelid;
            speechBubbleContainer = bubbleContainer;
            speechBubbleText = bubbleText;
            speechBubbleBackground = bubbleBg;
            if (mascotModelTransform != null) initialPosition = mascotModelTransform.localPosition;
            StartLivingMascotBehaviors();
        }

        private void Start()
        {
            if (mascotModelTransform != null)
            {
                initialPosition = mascotModelTransform.localPosition;
            }

            SetMascotState(BerryBearsState.STATE_IDLE, "Halo! Siap belajar bareng BerryBears?");

            // Mulai loop animasi mata dan pergerakan hidup
            StartLivingMascotBehaviors();
        }

        private void OnDisable()
        {
            StopLivingMascotBehaviors();
        }

        private void StartLivingMascotBehaviors()
        {
            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            blinkRoutine = StartCoroutine(OrganicBlinkLoop());

            if (eyeLookRoutine != null) StopCoroutine(eyeLookRoutine);
            eyeLookRoutine = StartCoroutine(EyeLookAroundLoop());

            if (wanderRoutine != null) StopCoroutine(wanderRoutine);
            wanderRoutine = StartCoroutine(HorizontalWanderLoop());
        }

        private void StopLivingMascotBehaviors()
        {
            if (blinkRoutine != null) StopCoroutine(blinkRoutine);
            if (eyeLookRoutine != null) StopCoroutine(eyeLookRoutine);
            if (wanderRoutine != null) StopCoroutine(wanderRoutine);
        }

        /// <summary>
        /// Fitur: Berkedip secara berkala (organik setiap 2.5 - 5 detik).
        /// </summary>
        private IEnumerator OrganicBlinkLoop()
        {
            while (true)
            {
                float waitTime = UnityEngine.Random.Range(2.5f, 5.0f);
                yield return new WaitForSeconds(waitTime);

                // Tutup mata sekejap
                if (eyelidLidObj != null) eyelidLidObj.SetActive(true);
                yield return new WaitForSeconds(0.12f);
                if (eyelidLidObj != null) eyelidLidObj.SetActive(false);

                // Kadang berkedip dua kali (double blink) seperti karakter kartun
                if (UnityEngine.Random.value < 0.28f)
                {
                    yield return new WaitForSeconds(0.08f);
                    if (eyelidLidObj != null) eyelidLidObj.SetActive(true);
                    yield return new WaitForSeconds(0.11f);
                    if (eyelidLidObj != null) eyelidLidObj.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Fitur: Bola mata melihat ke segala arah (atas, bawah, kiri, kanan, tengah).
        /// </summary>
        private IEnumerator EyeLookAroundLoop()
        {
            Vector2[] directions = new Vector2[]
            {
                Vector2.zero,               // Tengah
                new Vector2(-1.8f, 0f),     // Kiri
                new Vector2(1.8f, 0f),      // Kanan
                new Vector2(0f, 1.6f),      // Atas
                new Vector2(0f, -1.6f),     // Bawah
                new Vector2(-1.2f, 1.2f),   // Kiri Atas
                new Vector2(1.2f, 1.2f),    // Kanan Atas
                Vector2.zero                // Kembali ke Tengah
            };

            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(1.8f, 3.8f));
                if (!enableEyeTracking) continue;

                Vector2 targetDir = directions[UnityEngine.Random.Range(0, directions.Length)];

                if (leftPupilTransform != null)
                {
                    leftPupilTransform.localPosition = new Vector3(targetDir.x, targetDir.y, 0f);
                }
                if (rightPupilTransform != null)
                {
                    rightPupilTransform.localPosition = new Vector3(targetDir.x, targetDir.y, 0f);
                }
            }
        }

        /// <summary>
        /// Fitur: Bergerak / bergoyang ke kiri dan ke kanan kapan pun waktunya.
        /// </summary>
        private IEnumerator HorizontalWanderLoop()
        {
            if (mascotModelTransform == null) yield break;

            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(2.0f, 4.5f));
                if (!enableHorizontalWander) continue;

                float targetOffsetX = UnityEngine.Random.Range(-wanderRangeX, wanderRangeX);
                Vector3 startPos = mascotModelTransform.localPosition;
                Vector3 endPos = new Vector3(initialPosition.x + targetOffsetX, initialPosition.y, initialPosition.z);

                float duration = UnityEngine.Random.Range(0.8f, 1.4f);
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                    // Tambahkan sedikit goyangan vertikal saat melangkah ke samping
                    float hop = Mathf.Sin(t * Mathf.PI) * 4f;
                    mascotModelTransform.localPosition = Vector3.Lerp(startPos, endPos, t) + new Vector3(0, hop, 0);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Aturan: Interaksi Klik Langsung (On-Tap Response).
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerOnTapReaction();
        }

        public void TriggerOnTapReaction(string customMessage = null)
        {
            string message = string.IsNullOrEmpty(customMessage)
                ? onTapGreetings[tapGreetingIndex % onTapGreetings.Length]
                : customMessage;

            tapGreetingIndex++;
            SetMascotState(BerryBearsState.STATE_ON_TAP, message);
        }

        public void SetMascotState(BerryBearsState newState, string speechText = null)
        {
            currentState = newState;
            ThemeTokens.TriggerHapticFeedback();

            if (string.IsNullOrEmpty(speechText))
            {
                speechText = GetDefaultSpeechForState(newState);
            }

            UpdateSpeechBubble(speechText);
            TriggerAnimatorState(newState);
            PlayVisualEffectsForState(newState);
        }

        private string GetDefaultSpeechForState(BerryBearsState state)
        {
            switch (state)
            {
                case BerryBearsState.STATE_IDLE:
                    return "Waktunya Belajar! Ayo mulai bersama BerryBears!";
                case BerryBearsState.STATE_ON_TAP:
                    return "Yey! Aku selalu di sini menemanimu belajar!";
                case BerryBearsState.STATE_RECORDING_STT:
                    return "Aku sedang menyimak penjelasan gurumu... Perhatikan teks di bawah ya!";
                case BerryBearsState.STATE_QUIZ_THINKING:
                    return "Hmm... Baca soalnya pelan-pelan, pilih jawaban yang paling tepat ya!";
                case BerryBearsState.STATE_QUIZ_SUCCESS:
                    return "Hebat sekali! Nilai kamu bagus! Kamu dapat Kartu Digital AR Buah baru!";
                case BerryBearsState.STATE_QUIZ_RETRY:
                    return "Tidak apa-apa! Ayo kita baca ulang materinya sebentar lalu coba lagi!";
                default:
                    return "Halo! Ayo belajar bersama!";
            }
        }

        private void UpdateSpeechBubble(string message)
        {
            if (speechBubbleContainer != null) speechBubbleContainer.SetActive(true);
            if (speechBubbleText != null) speechBubbleText.text = message;
        }

        private void TriggerAnimatorState(BerryBearsState state)
        {
            if (mascotAnimator != null)
            {
                mascotAnimator.SetInteger("MascotState", (int)state);
                mascotAnimator.SetTrigger("StateChanged");
            }

            if (mascotModelTransform != null)
            {
                if (bounceRoutine != null) StopCoroutine(bounceRoutine);
                bounceRoutine = StartCoroutine(AnimateClayBounceRoutine());
            }
        }

        private void PlayVisualEffectsForState(BerryBearsState state)
        {
            if (state == BerryBearsState.STATE_QUIZ_SUCCESS && confettiParticles != null)
                confettiParticles.Play();
            else if (state == BerryBearsState.STATE_ON_TAP && tapSparkleParticles != null)
                tapSparkleParticles.Play();
        }

        private IEnumerator AnimateClayBounceRoutine()
        {
            Vector3 baseScale = Vector3.one;
            float duration = 0.28f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float s = Mathf.Sin(t * Mathf.PI);
                mascotModelTransform.localScale = new Vector3(
                    baseScale.x * (1f - 0.08f * s),
                    baseScale.y * (1f + 0.14f * s),
                    baseScale.z * (1f - 0.08f * s)
                );
                yield return null;
            }

            mascotModelTransform.localScale = baseScale;
        }
    }
}
