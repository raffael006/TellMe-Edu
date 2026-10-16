using System;
using System.Collections.Generic;
using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;
using UnityEngine.UI;

namespace TellMe.Modules
{
    /// <summary>
    /// Modul 4: Kuis Bahasa Isyarat (Pengguna: Siswa / Raka).
    /// Menampilkan isyarat SIBI/BISINDO dalam bentuk animasi gambar di panggung glassmorphism,
    /// lalu siswa memilih kartu kata & ilustrasi objek yang sesuai.
    /// </summary>
    public class SignLanguageQuizController : MonoBehaviour
    {
        [Header("Panggung Animasi Isyarat (Glassmorphism Stage)")]
        [SerializeField] private Image signAnimationStageImage;
        [SerializeField] private Text signGestureDescriptionText;
        [SerializeField] private Text signSystemBadgeText;

        [Header("Pilihan Jawaban Visual")]
        [SerializeField] private Text[] visualOptionLabels;

        private List<SoalIsyarat> daftarSoal = new List<SoalIsyarat>();
        private int currentIndex = 0;

        public event Action<bool, int> OnAnswerEvaluated;
        public int CurrentIndex => currentIndex;
        public List<SoalIsyarat> Questions => daftarSoal;

        public void SetupComponents(
            Image stageImg, Text descText, Text badgeText, Text[] optLabels, Button[] optButtons)
        {
            signAnimationStageImage = stageImg;
            signGestureDescriptionText = descText;
            signSystemBadgeText = badgeText;
            visualOptionLabels = optLabels;

            if (optButtons != null)
            {
                for (int i = 0; i < optButtons.Length; i++)
                {
                    int idx = i;
                    optButtons[i].onClick.RemoveAllListeners();
                    optButtons[i].onClick.AddListener(() => SelectSignAnswer(idx));
                }
            }

            InitializeSignQuestions();
            RenderCurrentSignQuestion();
        }

        private void OnEnable()
        {
            InitializeSignQuestions();
            RenderCurrentSignQuestion();
        }

        private void InitializeSignQuestions()
        {
            if (daftarSoal.Count > 0) return;

            daftarSoal.Add(new SoalIsyarat
            {
                id = "SIGN-01",
                kataTarget = "APEL",
                sistemIsyarat = "SIBI / BISINDO",
                deskripsiGerakan = "Kepalkan tangan kanan, tempelkan buku jari di pipi lalu putar perlahan.",
                pilihanKata = new[] { "Apel", "Buku", "Rumah", "Air" },
                indeksBenar = 0
            });

            daftarSoal.Add(new SoalIsyarat
            {
                id = "SIGN-02",
                kataTarget = "BUKU",
                sistemIsyarat = "SIBI / BISINDO",
                deskripsiGerakan = "Rapatkan kedua telapak tangan di depan dada, lalu buka seperti membuka halaman buku.",
                pilihanKata = new[] { "Pohon", "Buku", "Jeruk", "Tiga" },
                indeksBenar = 1
            });

            daftarSoal.Add(new SoalIsyarat
            {
                id = "SIGN-03",
                kataTarget = "BELAJAR",
                sistemIsyarat = "SIBI / BISINDO",
                deskripsiGerakan = "Ambil ilmu dari telapak tangan kiri lalu arahkan jari-jari tangan kanan ke dahi.",
                pilihanKata = new[] { "Tidur", "Makan", "Belajar", "Lari" },
                indeksBenar = 2
            });
        }

        public void SelectSignAnswer(int selectedOptionIndex)
        {
            if (daftarSoal.Count == 0) return;
            ThemeTokens.TriggerHapticFeedback();

            SoalIsyarat soal = daftarSoal[currentIndex];
            if (selectedOptionIndex == soal.indeksBenar)
            {
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_SUCCESS,
                    $"Hebat, benar! Itu adalah isyarat tangan untuk kata \"{soal.kataTarget}\"!");
            }
            else
            {
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_RETRY,
                    "Tidak apa-apa, coba perhatikan lagi gerakan tangannya ya!");
            }

            OnAnswerEvaluated?.Invoke(selectedOptionIndex == soal.indeksBenar, selectedOptionIndex);
        }

        public void NextSignQuestion()
        {
            if (daftarSoal.Count == 0) return;
            currentIndex = (currentIndex + 1) % daftarSoal.Count;
            RenderCurrentSignQuestion();
        }

        private void RenderCurrentSignQuestion()
        {
            if (daftarSoal.Count == 0) return;
            SoalIsyarat soal = daftarSoal[currentIndex];

            if (signSystemBadgeText != null) signSystemBadgeText.text = soal.sistemIsyarat;
            if (signGestureDescriptionText != null) signGestureDescriptionText.text = soal.deskripsiGerakan;

            if (visualOptionLabels != null)
            {
                for (int i = 0; i < visualOptionLabels.Length && i < soal.pilihanKata.Length; i++)
                {
                    visualOptionLabels[i].text = soal.pilihanKata[i];
                }
            }
        }
    }
}
