using System;
using UnityEngine;

namespace EduTunarungu.Data
{
    public enum WordCategory
    {
        Sapaan,         // Kata sapaan & sopan santun (Halo, Terima kasih, Tolong, Maaf)
        Hewan,          // Nama-nama hewan (Kucing, Burung, Ikan, Kelinci)
        MakananBuah,    // Makanan & buah-buahan (Apel, Jeruk, Pisang, Air)
        Keluarga,       // Anggota keluarga (Ayah, Ibu, Kakak, Adik)
        Emosi,          // Perasaan (Senang, Sedih, Semangat, Takut)
        Alfabet         // Abjad Isyarat A-Z
    }

    [System.Serializable]
    public struct SyllableInfo
    {
        public string text;           // Contoh: "KU"
        public string mouthShape;      // Petunjuk artikulasi bibir: "Bibir membulat (vokal U)"
        public string lipReadingDesc;  // Deskripsi visual gerak bibir
    }

    /// <summary>
    /// Model data kosakata pembelajaran khusus anak disabilitas pendengaran (tuna rungu).
    /// Mengintegrasikan visual referent, isyarat tangan (BISINDO), dan artikulasi bentuk bibir (lip reading).
    /// </summary>
    [CreateAssetMenu(fileName = "NewVocabularyItem", menuName = "EduTunarungu/Vocabulary Item")]
    public class VocabularyItem : ScriptableObject
    {
        [Header("Informasi Dasar")]
        public string word = "Kucing";
        public WordCategory category = WordCategory.Hewan;
        public string meaningShort = "Hewan berbulu dengan kumis yang suka mendengkur";

        [Header("Pemenggalan Suku Kata")]
        [Tooltip("Potongan suku kata untuk latihan artikulasi (misal: ['KU', 'CING'])")]
        public SyllableInfo[] syllables;

        [Header("Aset Visual")]
        public Sprite realObjectSprite;     // Gambar ilustrasi objek
        public Sprite signLanguageSprite;   // Gambar gerakan isyarat utama
        public Sprite[] signAnimationFrames; // Rangkaian frame animasi tangan (opsional)
        public Sprite mouthShapeSprite;     // Gambar bentuk bibir saat pengucapan

        [Header("Deskripsi Gerakan Isyarat (BISINDO)")]
        [TextArea(2, 4)]
        public string gestureDescription = "Buka kedua tangan di dekat pipi lalu gerakkan menjauh seperti menarik kumis kucing.";

        [Header("Panduan Artikulasi (Bentuk Mulut)")]
        [TextArea(2, 3)]
        public string articulationGuide = "Tutup bibir sedikit membulat untuk 'KU', lalu buka tersenyum lebar dengan gigi merapat untuk 'CING'.";

        [Header("Irama Taktil (Haptic Pattern)")]
        [Tooltip("Durasi getaran per suku kata dalam milidetik")]
        public long[] syllableVibrations = new long[] { 60, 90 };
    }
}
