using System.Collections;
using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;
using UnityEngine.UI;

namespace TellMe.Modules
{
    /// <summary>
    /// Modul 2: Belajar / Speech-to-Text + Ringkasan Pintar Guru (Pengguna: Siswa & Siswi SD).
    /// Siswa cukup menekan SATU tombol bulat besar REKAM; suara guru diubah menjadi teks
    /// secara real-time sekaligus diringkas menjadi Poin Inti & Rumus Visual agar ramah anak Tuli SD.
    /// </summary>
    public class SpeechToTextModuleController : MonoBehaviour
    {
        [Header("Header Kelas Aktif")]
        [SerializeField] private Text activeSubjectTagText;
        [SerializeField] private Text activeClassTitleText;
        [SerializeField] private Text activeTeacherNameText;

        [Header("One-Tap Record Button & Live Transcript + Ringkasan Guru")]
        [SerializeField] private Button oneTapRecordButton;
        [SerializeField] private Text recordButtonLabel;
        [SerializeField] private Text liveTranscriptText;
        [SerializeField] private Text teacherSummaryText;
        [SerializeField] private Text visualFormulaText;
        [SerializeField] private Button saveAndStartQuizButton;

        public bool IsRecording { get; private set; }

        private Jadwal currentSchedule;
        private Materi lastSavedMateri;
        private Coroutine simulatedOfflineSttCoroutine;

        public void SetupComponents(
            Text mapelText, Text classTitle, Text teacherText, Button recordBtn, Text recordLabel,
            Text transcriptText, Button quizBtn, Text summaryTxt = null, Text formulaTxt = null)
        {
            activeSubjectTagText = mapelText;
            activeClassTitleText = classTitle;
            activeTeacherNameText = teacherText;
            oneTapRecordButton = recordBtn;
            recordButtonLabel = recordLabel;
            liveTranscriptText = transcriptText;
            saveAndStartQuizButton = quizBtn;
            teacherSummaryText = summaryTxt;
            visualFormulaText = formulaTxt;

            if (oneTapRecordButton != null)
            {
                oneTapRecordButton.onClick.RemoveAllListeners();
                oneTapRecordButton.onClick.AddListener(OnOneTapRecordButtonClicked);
            }
            if (saveAndStartQuizButton != null)
            {
                saveAndStartQuizButton.onClick.RemoveAllListeners();
                saveAndStartQuizButton.onClick.AddListener(ProceedToMiniQuiz);
            }
        }

        private void OnEnable()
        {
            SetupActiveClassHeader();
        }

        public void SelectSubjectForRecording(SubjectType mapel)
        {
            if (LocalDataManager.Instance != null)
            {
                var found = LocalDataManager.Instance.Data.daftarJadwal.Find(j => j.mataPelajaran == mapel);
                if (found != null)
                {
                    currentSchedule = found;
                }
                else
                {
                    currentSchedule = new Jadwal
                    {
                        id = "JDW-TEMP",
                        mataPelajaran = mapel,
                        judul = mapel == SubjectType.Matematika ? "Penjumlahan & Pengurangan 1-20"
                              : mapel == SubjectType.BahasaIndonesia ? "Membaca Cerita Kebun Buah"
                              : "Fruits & Colors Around Us",
                        namaGuru = mapel == SubjectType.BahasaInggris ? "Pak Budi" : "Bu Sari",
                        jam = "08:00"
                    };
                }
            }
            BindScheduleHeader();
        }

        public void SetupActiveClassHeader()
        {
            currentSchedule = AppNavigationManager.Instance?.ActiveScheduleForSession;
            if (currentSchedule == null && LocalDataManager.Instance != null && LocalDataManager.Instance.Data.daftarJadwal.Count > 0)
            {
                currentSchedule = LocalDataManager.Instance.Data.daftarJadwal[0];
            }
            BindScheduleHeader();
        }

        private void BindScheduleHeader()
        {
            if (currentSchedule != null)
            {
                if (activeSubjectTagText != null)
                    activeSubjectTagText.text = ThemeTokens.GetSubjectDisplayName(currentSchedule.mataPelajaran);
                if (activeClassTitleText != null)
                    activeClassTitleText.text = currentSchedule.judul;
                if (activeTeacherNameText != null)
                    activeTeacherNameText.text = $"Guru: {currentSchedule.namaGuru} • 🎙️ STT Offline Siap";

                if (teacherSummaryText != null)
                    teacherSummaryText.text = LocalDataManager.GenerateRingkasanGuru(currentSchedule.mataPelajaran, "");
                if (visualFormulaText != null)
                    visualFormulaText.text = LocalDataManager.GetRumusVisual(currentSchedule.mataPelajaran);
            }
        }

        /// <summary>
        /// Dipanggil ketika siswa menekan tombol bulat besar REKAM.
        /// </summary>
        public void OnOneTapRecordButtonClicked()
        {
            ThemeTokens.TriggerHapticFeedback();

            if (!IsRecording)
            {
                StartOfflineRecording();
            }
            else
            {
                StopAndSaveRecording();
            }
        }

        public void StartOfflineRecording()
        {
            IsRecording = true;
            if (recordButtonLabel != null) recordButtonLabel.text = "⏹️ BERHENTI & RINGKAS";
            if (liveTranscriptText != null) liveTranscriptText.text = "";
            if (teacherSummaryText != null) teacherSummaryText.text = "⏳ Sedang menyimak & membuat ringkasan poin inti penjelasan guru...";

            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_RECORDING_STT,
                "Aku ikut menyimak suara gurumu! Perhatikan transkripsi dan ringkasan pintar di bawah ya!");

            if (simulatedOfflineSttCoroutine != null) StopCoroutine(simulatedOfflineSttCoroutine);
            simulatedOfflineSttCoroutine = StartCoroutine(StreamOfflineSpeechToTextRoutine());
        }

        public void StopAndSaveRecording()
        {
            IsRecording = false;
            if (simulatedOfflineSttCoroutine != null)
            {
                StopCoroutine(simulatedOfflineSttCoroutine);
                simulatedOfflineSttCoroutine = null;
            }

            if (recordButtonLabel != null) recordButtonLabel.text = "🎙️ REKAM SUARA GURU";

            SubjectType mapel = currentSchedule != null ? currentSchedule.mataPelajaran : SubjectType.Matematika;
            string finalTranscript = liveTranscriptText != null && !string.IsNullOrEmpty(liveTranscriptText.text)
                ? liveTranscriptText.text
                : GetSampleTeacherTranscript(mapel);

            if (liveTranscriptText != null)
                liveTranscriptText.text = finalTranscript;

            if (LocalDataManager.Instance != null)
            {
                lastSavedMateri = LocalDataManager.Instance.SimpanMateriRekaman(currentSchedule, finalTranscript);
                if (teacherSummaryText != null)
                    teacherSummaryText.text = lastSavedMateri.ringkasan;
                if (visualFormulaText != null)
                    visualFormulaText.text = lastSavedMateri.rumusVisual;
            }

            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                "Ringkasan penjelasan guru sudah siap! Ayo pahami poin intinya lalu lanjut ke Kuis Mini!");
        }

        public void ProceedToMiniQuiz()
        {
            SubjectType mapel = currentSchedule != null ? currentSchedule.mataPelajaran : SubjectType.Matematika;
            if (lastSavedMateri == null || lastSavedMateri.mataPelajaran != mapel)
            {
                if (LocalDataManager.Instance != null)
                {
                    lastSavedMateri = LocalDataManager.Instance.Data.daftarMateri.Find(m => m.mataPelajaran == mapel);
                    if (lastSavedMateri == null)
                    {
                        lastSavedMateri = LocalDataManager.Instance.SimpanMateriRekaman(currentSchedule, GetSampleTeacherTranscript(mapel));
                    }
                }
            }

            AppNavigationManager.Instance?.OpenQuizForMateri(lastSavedMateri);
        }

        private IEnumerator StreamOfflineSpeechToTextRoutine()
        {
            SubjectType mapel = currentSchedule != null ? currentSchedule.mataPelajaran : SubjectType.Matematika;
            string fullText = GetSampleTeacherTranscript(mapel);
            string[] words = fullText.Split(' ');

            string accumulated = "";
            foreach (string word in words)
            {
                accumulated += (accumulated.Length == 0 ? "" : " ") + word;
                if (liveTranscriptText != null)
                {
                    liveTranscriptText.text = accumulated;
                }
                yield return new WaitForSeconds(0.14f);
            }

            // Otomatis simpan & buat ringkasan setelah penjelasan selesai
            StopAndSaveRecording();
        }

        private string GetSampleTeacherTranscript(SubjectType mapel)
        {
            switch (mapel)
            {
                case SubjectType.Matematika:
                    return "Selamat pagi anak-anak. Hari ini kita belajar penjumlahan dengan buah apel. Di meja ada 4 buah apel merah, lalu Ibu tambahkan 3 buah apel lagi. Jadi 4 ditambah 3 sama dengan 7 buah apel merah.";
                case SubjectType.BahasaIndonesia:
                    return "Hari ini kita membaca cerita tentang Buah Jeruk Manis di kebun. Buah jeruk berwarna oranye cerah dan mengandung banyak Vitamin C supaya tubuh kita sehat dan tidak mudah sakit.";
                case SubjectType.BahasaInggris:
                default:
                    return "Good morning class! Today we learn fruit names in English. Apel merah in English is Red Apple. Jeruk manis is Sweet Orange. Mangga harum is Fragrant Mango.";
            }
        }
    }
}
