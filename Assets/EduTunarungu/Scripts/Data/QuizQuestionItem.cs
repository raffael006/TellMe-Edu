using UnityEngine;

namespace EduTunarungu.Data
{
    public enum QuizType
    {
        SignToPicture,     // Soal berupa gambar isyarat -> Tebak gambar objek
        PictureToSign,     // Soal berupa gambar objek -> Tebak gerakan isyarat
        SyllableAssembly   // Menyusun balok suku kata
    }

    [System.Serializable]
    public struct QuizOption
    {
        public string optionText;       // Teks pilihan
        public Sprite optionSprite;     // Gambar pilihan
        public bool isCorrect;          // Apakah jawaban benar
    }

    /// <summary>
    /// Model data soal kuis interaktif berbasis visual untuk evaluasi belajar anak tuna rungu.
    /// </summary>
    [CreateAssetMenu(fileName = "NewQuizQuestion", menuName = "EduTunarungu/Quiz Question")]
    public class QuizQuestionItem : ScriptableObject
    {
        [Header("Konfigurasi Soal")]
        public QuizType quizType = QuizType.SignToPicture;
        public string questionPrompt = "Manakah benda yang dimaksud oleh isyarat ini?";

        [Header("Petunjuk Visual Utama")]
        public Sprite promptSprite;     // Gambar isyarat atau objek utama
        public string promptHint = "Perhatikan gerakan tangan yang menyerupai kumis!";

        [Header("Pilihan Jawaban (3-4 Pilihan)")]
        public QuizOption[] options;

        [Header("Umpan Balik Positif")]
        public string successPraise = "Hebat! Kamu berhasil mengenali isyarat Kucing! ⭐";
        public string retryHint = "Hampir benar! Coba perhatikan lagi gerakan jarinya ya.";
    }
}
