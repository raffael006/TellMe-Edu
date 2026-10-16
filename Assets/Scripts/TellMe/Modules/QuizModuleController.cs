using System;
using System.Collections.Generic;
using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;
using UnityEngine.UI;

namespace TellMe.Modules
{
    /// <summary>
    /// Modul 3: Kuis dan Nilai (Pengguna: Siswa & Guru).
    /// Mencakup Dashboard Nilai (Bento Grid untuk Guru/Orang Tua) dan Layar Kuis Mini per mata pelajaran.
    /// Mengevaluasi nilai:
    /// - Jika Nilai Bagus (Ya, >= 75): BerryBears selebrasi & membuka Kartu Digital -> AR Buah.
    /// - Jika Nilai Belum Bagus (Tidak, < 75): BerryBears menyemangati & mengarahkan ulang materi (Langkah 3).
    /// </summary>
    public class QuizModuleController : MonoBehaviour
    {
        [Header("Komponen Dashboard Nilai (Bento Grid)")]
        [SerializeField] private Text totalMateriCountText;
        [SerializeField] private Text unlockedCardsCountText;
        [SerializeField] private Text mathScorePercentText;
        [SerializeField] private Text indoScorePercentText;
        [SerializeField] private Text engScorePercentText;

        [Header("Komponen Layar Kuis Mini")]
        [SerializeField] private Slider quizProgressSlider;
        [SerializeField] private Text quizProgressPercentText;
        [SerializeField] private Text activeSubjectPillText;
        [SerializeField] private Text questionText;
        [SerializeField] private Button[] optionButtons;
        [SerializeField] private Text[] optionTexts;

        private SubjectType currentSubject = SubjectType.Matematika;
        private List<SoalKuis> activeQuestions = new List<SoalKuis>();
        private int currentQuestionIndex = 0;
        private int correctAnswersCount = 0;

        public event Action<int, bool, Kartu> OnQuizFinishedEvent;

        public void SetupComponents(
            Text totalMateriText, Text cardsText, Text mathScoreText, Text indoScoreText, Text engScoreText,
            Slider progressSlider, Text progressText, Text subjectPill, Text question, Button[] optButtons, Text[] optTexts)
        {
            totalMateriCountText = totalMateriText;
            unlockedCardsCountText = cardsText;
            mathScorePercentText = mathScoreText;
            indoScorePercentText = indoScoreText;
            engScorePercentText = engScoreText;
            quizProgressSlider = progressSlider;
            quizProgressPercentText = progressText;
            activeSubjectPillText = subjectPill;
            questionText = question;
            optionButtons = optButtons;
            optionTexts = optTexts;

            if (optionButtons != null)
            {
                for (int i = 0; i < optionButtons.Length; i++)
                {
                    int idx = i;
                    optionButtons[i].onClick.RemoveAllListeners();
                    optionButtons[i].onClick.AddListener(() => SubmitAnswer(idx));
                }
            }
        }

        private void OnEnable()
        {
            RefreshDashboardStats();
            Materi activeMateri = AppNavigationManager.Instance?.ActiveMateriForQuiz;
            StartQuizForSubject(activeMateri != null ? activeMateri.mataPelajaran : SubjectType.Matematika);
        }

        public void RefreshDashboardStats()
        {
            if (LocalDataManager.Instance == null) return;
            var db = LocalDataManager.Instance.Data;

            if (totalMateriCountText != null)
                totalMateriCountText.text = db.daftarMateri.Count.ToString();

            int unlockedCards = db.daftarKartu.FindAll(k => k.terbuka).Count;
            if (unlockedCardsCountText != null)
                unlockedCardsCountText.text = $"{unlockedCards}/{db.daftarKartu.Count}";

            if (mathScorePercentText != null)
                mathScorePercentText.text = $"{LocalDataManager.Instance.GetPersentaseNilaiMapel(SubjectType.Matematika)}%";
            if (indoScorePercentText != null)
                indoScorePercentText.text = $"{LocalDataManager.Instance.GetPersentaseNilaiMapel(SubjectType.BahasaIndonesia)}%";
            if (engScorePercentText != null)
                engScorePercentText.text = $"{LocalDataManager.Instance.GetPersentaseNilaiMapel(SubjectType.BahasaInggris)}%";
        }

        public void StartQuizForSubject(SubjectType subject)
        {
            currentSubject = subject;
            currentQuestionIndex = 0;
            correctAnswersCount = 0;
            activeQuestions = BuildQuestionsForSubject(subject);

            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_QUIZ_THINKING,
                $"Ayo kerjakan Kuis Mini {ThemeTokens.GetSubjectDisplayName(subject)}! Baca pelan-pelan ya!");

            RenderCurrentQuestion();
        }

        public void SubmitAnswer(int optionIndex)
        {
            if (currentQuestionIndex >= activeQuestions.Count) return;

            ThemeTokens.TriggerHapticFeedback();
            SoalKuis currentQ = activeQuestions[currentQuestionIndex];

            if (optionIndex == currentQ.indeksJawabanBenar)
            {
                correctAnswersCount++;
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_ON_TAP,
                    "Hebat, jawabanmu benar! Ayo lanjut ke soal berikutnya!");
            }
            else
            {
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_THINKING,
                    "Tidak apa-apa, tetap semangat ya!");
            }

            currentQuestionIndex++;
            if (currentQuestionIndex < activeQuestions.Count)
            {
                RenderCurrentQuestion();
            }
            else
            {
                EvaluateFinalQuizScore();
            }
        }

        private void RenderCurrentQuestion()
        {
            if (activeQuestions.Count == 0) return;

            SoalKuis q = activeQuestions[currentQuestionIndex];
            float progress = (float)(currentQuestionIndex + 1) / activeQuestions.Count;

            if (quizProgressSlider != null) quizProgressSlider.value = progress;
            if (quizProgressPercentText != null) quizProgressPercentText.text = $"{Mathf.RoundToInt(progress * 100f)}%";
            if (activeSubjectPillText != null) activeSubjectPillText.text = ThemeTokens.GetSubjectDisplayName(currentSubject);
            if (questionText != null) questionText.text = q.pertanyaan;

            if (optionTexts != null)
            {
                for (int i = 0; i < optionTexts.Length && i < q.pilihanJawaban.Length; i++)
                {
                    optionTexts[i].text = q.pilihanJawaban[i];
                }
            }
        }

        /// <summary>
        /// Langkah 6 Alur Penggunaan: Evaluasi Nilai Kuis.
        /// - Jika Nilai Bagus (>= 75): Siswa mendapat Kartu Digital -> akses AR Buah.
        /// - Jika Nilai Belum Bagus (< 75): BerryBears mengarahkan kembali ke Langkah 3 (ulangi materi).
        /// </summary>
        private void EvaluateFinalQuizScore()
        {
            int finalScore = Mathf.RoundToInt(((float)correctAnswersCount / activeQuestions.Count) * 100f);
            string materiId = AppNavigationManager.Instance?.ActiveMateriForQuiz != null
                ? AppNavigationManager.Instance.ActiveMateriForQuiz.id
                : "MTR-001";

            Kartu unlockedCard = null;
            bool passed = finalScore >= 75;

            if (LocalDataManager.Instance != null)
            {
                passed = LocalDataManager.Instance.SimpanHasilKuis(currentSubject, materiId, finalScore, out unlockedCard);
            }

            RefreshDashboardStats();

            if (passed)
            {
                string cardName = unlockedCard != null ? unlockedCard.namaBuahId : "Kartu Buah 3D";
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_SUCCESS,
                    $"HORE! Nilaimu {finalScore}! Kamu mendapat Kartu Digital \"{cardName}\" untuk membuka AR Buah!");
            }
            else
            {
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_RETRY,
                    $"Nilaimu {finalScore}. Tidak apa-apa! Ayo kita baca ulang Materi Belajar sebentar lalu coba lagi ya!");
            }

            OnQuizFinishedEvent?.Invoke(finalScore, passed, unlockedCard);
        }

        /// <summary>
        /// Dipanggil saat nilai belum cukup (< 75) untuk kembali mengulang materi di Modul 2.
        /// </summary>
        public void RetryReadingMateri()
        {
            AppNavigationManager.Instance?.NavigateTo(TellMeScreen.Module2_SpeechToTextBelajar);
        }

        /// <summary>
        /// Dipanggil saat nilai bagus (>= 75) untuk membuka koleksi Kartu Digital & AR Buah.
        /// </summary>
        public void OpenUnlockedCardGallery()
        {
            AppNavigationManager.Instance?.NavigateTo(TellMeScreen.Module6_KartuDigital);
        }

        private List<SoalKuis> BuildQuestionsForSubject(SubjectType subject)
        {
            var list = new List<SoalKuis>();
            switch (subject)
            {
                case SubjectType.Matematika:
                    list.Add(new SoalKuis
                    {
                        id = "QM-1",
                        mataPelajaran = SubjectType.Matematika,
                        pertanyaan = "Raka punya 4 apel merah, lalu Ibu memberi 3 apel lagi. Berapa jumlah apel Raka sekarang?",
                        pilihanJawaban = new[] { "6 Apel", "7 Apel", "8 Apel" },
                        indeksJawabanBenar = 1
                    });
                    list.Add(new SoalKuis
                    {
                        id = "QM-2",
                        mataPelajaran = SubjectType.Matematika,
                        pertanyaan = "Di meja ada 10 jeruk manis. Dimakan 2 jeruk. Berapa sisa jeruk di meja?",
                        pilihanJawaban = new[] { "8 Jeruk", "7 Jeruk", "9 Jeruk" },
                        indeksJawabanBenar = 0
                    });
                    break;

                case SubjectType.BahasaIndonesia:
                    list.Add(new SoalKuis
                    {
                        id = "QBI-1",
                        mataPelajaran = SubjectType.BahasaIndonesia,
                        pertanyaan = "Apa tahap pertama sebelum pohon apel tumbuh besar di kebun?",
                        pilihanJawaban = new[] { "Bibit Kecil", "Langsung Berbuah", "Daun Kering" },
                        indeksJawabanBenar = 0
                    });
                    list.Add(new SoalKuis
                    {
                        id = "QBI-2",
                        mataPelajaran = SubjectType.BahasaIndonesia,
                        pertanyaan = "Buah apel yang sudah matang rasanya...",
                        pilihanJawaban = new[] { "Asin sekali", "Manis dan segar", "Pahit" },
                        indeksJawabanBenar = 1
                    });
                    break;

                case SubjectType.BahasaInggris:
                default:
                    list.Add(new SoalKuis
                    {
                        id = "QEN-1",
                        mataPelajaran = SubjectType.BahasaInggris,
                        pertanyaan = "What is 'Apel Merah' in English?",
                        pilihanJawaban = new[] { "Red Apple", "Yellow Banana", "Green Mango" },
                        indeksJawabanBenar = 0
                    });
                    list.Add(new SoalKuis
                    {
                        id = "QEN-2",
                        mataPelajaran = SubjectType.BahasaInggris,
                        pertanyaan = "What color is a 'Yellow Banana'?",
                        pilihanJawaban = new[] { "Biru (Blue)", "Kuning (Yellow)", "Ungu (Purple)" },
                        indeksJawabanBenar = 1
                    });
                    break;
            }
            return list;
        }
    }
}
