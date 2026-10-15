using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;

namespace KawanDengarEdu.UI
{
    public enum SignLanguageMode
    {
        BISINDO,    // Bahasa Isyarat Indonesia
        ASL         // American Sign Language
    }

    /// <summary>
    /// Controller Speech-to-Text Live dengan kontrol 1-Tombol (Single Toggle Button)
    /// dan Avatar AI Penerjemah Bahasa Isyarat (BISINDO & ASL).
    /// </summary>
    public class LiveSTTController : MonoBehaviour
    {
        [Header("Single Toggle Button")]
        public Button singleToggleRecordButton;
        public TextMeshProUGUI toggleButtonLabel;
        public Image toggleButtonBackground;
        public Color recordColor = new Color(0.96f, 0.25f, 0.37f); // Rose merah
        public Color stopColor = new Color(0.06f, 0.73f, 0.51f);   // Hijau selesai

        [Header("Speech Visualizer")]
        public AudioWaveformVisualizer visualizer;
        public GameObject soundActiveBadge;

        [Header("Transcription View")]
        public TextMeshProUGUI transcriptText;

        [Header("AI Sign Language Avatar")]
        public SignLanguageMode signMode = SignLanguageMode.BISINDO;
        public Button btnBisindo;
        public Button btnASL;
        public TextMeshProUGUI avatarSpeechBubble;
        public Animator avatarAnimator; // Opsional jika menggunakan 3D Avatar Animator

        [Header("Screen Navigation")]
        public ScreenManager screenManager;

        private bool isRecording = false;
        private Coroutine transcriptionSimulationRoutine;

        private void Start()
        {
            if (singleToggleRecordButton != null)
                singleToggleRecordButton.onClick.AddListener(OnToggleRecordingClicked);

            if (btnBisindo != null)
                btnBisindo.onClick.AddListener(() => SetSignMode(SignLanguageMode.BISINDO));

            if (btnASL != null)
                btnASL.onClick.AddListener(() => SetSignMode(SignLanguageMode.ASL));

            UpdateUIState();
        }

        public void SetSignMode(SignLanguageMode mode)
        {
            signMode = mode;
            if (avatarSpeechBubble != null)
            {
                avatarSpeechBubble.text = mode == SignLanguageMode.BISINDO
                    ? "Memperagakan Isyarat BISINDO 🇮🇩"
                    : "Translating to ASL Sign Language 🇺🇸";
            }
        }

        public void OnToggleRecordingClicked()
        {
            isRecording = !isRecording;

            if (isRecording)
            {
                StartRecording();
            }
            else
            {
                StopRecordingAndSummarize();
            }

            UpdateUIState();
        }

        private void StartRecording()
        {
            if (visualizer != null) visualizer.SetAudioActive(true);
            if (soundActiveBadge != null) soundActiveBadge.SetActive(true);

            if (transcriptionSimulationRoutine != null)
                StopCoroutine(transcriptionSimulationRoutine);

            transcriptionSimulationRoutine = StartCoroutine(SimulateLiveSpeechStream());
        }

        private void StopRecordingAndSummarize()
        {
            if (visualizer != null) visualizer.SetAudioActive(false);
            if (soundActiveBadge != null) soundActiveBadge.SetActive(false);

            if (transcriptionSimulationRoutine != null)
                StopCoroutine(transcriptionSimulationRoutine);

            // Beralih ke Layar Rangkuman Cerdas AI (Layar 04)
            if (screenManager != null)
            {
                screenManager.ShowAISummaryScreen();
            }
        }

        private IEnumerator SimulateLiveSpeechStream()
        {
            bool isIndo = BilingualManager.Instance == null || BilingualManager.Instance.IsIndonesian();

            string[] phrasesId = new string[]
            {
                "Anak-anak hebat...",
                "Hari ini kita belajar tentang <color=#3B82F6><b>Buah Apel</b></color>.",
                "Apel memiliki banyak <color=#F59E0B><b>Vitamin C</b></color> yang membuat tubuh kita kuat dan tidak mudah sakit.",
                "Kulit apel ada yang berwarna <color=#EF4444><b>Merah</b></color>, <color=#10B981><b>Hijau</b></color>, dan <color=#F59E0B><b>Kuning</b></color>..."
            };

            string[] phrasesEn = new string[]
            {
                "Wonderful students...",
                "Today we are learning about <color=#3B82F6><b>Apples</b></color>.",
                "Apples are packed with <color=#F59E0B><b>Vitamin C</b></color> to keep our immune system strong.",
                "Apple skins can be <color=#EF4444><b>Red</b></color>, <color=#10B981><b>Green</b></color>, and <color=#F59E0B><b>Yellow</b></color>..."
            };

            string[] stream = isIndo ? phrasesId : phrasesEn;
            string current = "";

            if (transcriptText != null) transcriptText.text = "";

            for (int i = 0; i < stream.Length; i++)
            {
                current += (i > 0 ? " " : "") + stream[i];
                if (transcriptText != null) transcriptText.text = current;

                // Animasi trigger avatar tangan
                if (avatarAnimator != null)
                {
                    avatarAnimator.SetTrigger("SignGesture");
                }

                yield return new WaitForSeconds(2.0f);
            }
        }

        private void UpdateUIState()
        {
            bool isIndo = BilingualManager.Instance == null || BilingualManager.Instance.IsIndonesian();

            if (toggleButtonLabel != null)
            {
                if (isRecording)
                {
                    toggleButtonLabel.text = isIndo ? "⏹️ Selesai & Buat Rangkuman AI" : "⏹️ Finish & Generate AI Summary";
                }
                else
                {
                    toggleButtonLabel.text = isIndo ? "🎙️ Mulai Merekam Suara Guru" : "🎙️ Start Recording Teacher";
                }
            }

            if (toggleButtonBackground != null)
            {
                toggleButtonBackground.color = isRecording ? stopColor : recordColor;
            }
        }
    }
}
