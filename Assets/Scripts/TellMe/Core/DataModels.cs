using System;
using System.Collections.Generic;
using UnityEngine;

namespace TellMe.Core
{
    /// <summary>
    /// Tiga mata pelajaran utama pada prototipe TellMe (Kelas 1-4 SD SLB).
    /// </summary>
    public enum SubjectType
    {
        Matematika = 0,
        BahasaIndonesia = 1,
        BahasaInggris = 2
    }

    /// <summary>
    /// State Machine resmi untuk maskot interaktif 3D BerryBears.
    /// </summary>
    public enum BerryBearsState
    {
        STATE_IDLE,
        STATE_ON_TAP,
        STATE_RECORDING_STT,
        STATE_QUIZ_THINKING,
        STATE_QUIZ_SUCCESS,
        STATE_QUIZ_RETRY
    }

    /// <summary>
    /// Tahap pertumbuhan tanaman pada Modul 8 (Tumbuh Kembang Tanaman).
    /// </summary>
    public enum PlantGrowthPhase
    {
        Bibit = 0,
        Tunas = 1,
        Pohon = 2,
        Berbuah = 3
    }

    /// <summary>
    /// Entitas Jadwal: diatur oleh Orang Tua (Bu Rina) pada Mode Orang Tua.
    /// </summary>
    [Serializable]
    public class Jadwal
    {
        public string id;
        public string hari; // "S", "M", "T", "W", "T", "F", "S"
        public int dayIndex; // 0 = Minggu .. 6 = Sabtu
        public string jam; // Format "HH:mm", misal "08:00"
        public SubjectType mataPelajaran;
        public string judul;
        public string namaGuru;
        public bool pengingatAktif = true;
    }

    /// <summary>
    /// Entitas Materi: hasil transkripsi Speech-to-Text offline dari penjelasan guru
    /// beserta Ringkasan Pintar Penjelasan Guru (poin penting & visual).
    /// </summary>
    [Serializable]
    public class Materi
    {
        public string id;
        public string jadwalId;
        public SubjectType mataPelajaran;
        public string judul;
        public string namaGuru;
        [TextArea(3, 10)]
        public string teks;
        [TextArea(2, 6)]
        public string ringkasan;
        public string rumusVisual;
        public string tanggal;
    }

    /// <summary>
    /// Entitas Soal Kuis Mini per Mata Pelajaran.
    /// </summary>
    [Serializable]
    public class SoalKuis
    {
        public string id;
        public string materiId;
        public SubjectType mataPelajaran;
        public string visualPersamaan;
        public string pertanyaan;
        public string[] pilihanJawaban;
        public int indeksJawabanBenar;
    }

    /// <summary>
    /// Entitas HasilKuis: menyimpan nilai siswa per mata pelajaran untuk dipantau Guru & Orang Tua.
    /// </summary>
    [Serializable]
    public class HasilKuis
    {
        public string id;
        public string materiId;
        public SubjectType mataPelajaran;
        public int skor; // 0 - 100
        public bool lulusAmbang; // true jika skor >= 75
        public string tanggal;
    }

    /// <summary>
    /// Entitas Kartu Digital: penghargaan saat nilai kuis bagus, membuka model 3D AR Buah yang sesuai.
    /// </summary>
    [Serializable]
    public class Kartu
    {
        public string id;
        public string namaBuahId;       // Contoh: "Apel Merah"
        public string namaBuahEn;       // Contoh: "Red Apple"
        public string emojiBuah;        // Contoh: "🍎"
        public string deskripsiIsyarat; // Contoh: "Kepalkan tangan di pipi lalu putar pelan (SIBI/BISINDO)"
        public SubjectType mataPelajaran;
        public bool terbuka;
        public bool labelBaru;          // Menampilkan badge "BARU"
        public string tanggalDidapat;
        public string prefabArKey;
    }

    /// <summary>
    /// Entitas Soal Kuis Bahasa Isyarat (SIBI / BISINDO) dengan animasi gambar.
    /// </summary>
    [Serializable]
    public class SoalIsyarat
    {
        public string id;
        public string kataTarget;
        public string sistemIsyarat; // "SIBI / BISINDO"
        public string deskripsiGerakan;
        public string[] frameAnimasiPaths;
        public string[] pilihanKata;
        public int indeksBenar;
    }

    /// <summary>
    /// Informasi visual per fase pada Modul 8 (Tumbuh Kembang Tanaman).
    /// </summary>
    [Serializable]
    public class InfoTahapTanaman
    {
        public PlantGrowthPhase fase;
        public string namaFase;
        [TextArea(2, 4)]
        public string penjelasanBerryBears;
    }

    /// <summary>
    /// Entitas Log Aktivitas Anak untuk Dashboard Pemantauan Orang Tua & Guru.
    /// </summary>
    [Serializable]
    public class AktivitasAnak
    {
        public string id;
        public string waktu;
        public string ikon;
        public string kategori;
        public string deskripsi;
    }

    /// <summary>
    /// Container utama penyimpanan lokal (100% offline JSON di perangkat).
    /// </summary>
    [Serializable]
    public class TellMeDatabase
    {
        public string namaSiswa = "Raka";
        public string peranSiswa = "Siswa SD (Kelas 3)";
        public bool isSiswiPerempuan = false;
        public string kelasSiswa = "Kelas 3 SD SLB";
        public string pinOrangTua = "1234";
        public int ambangNilaiBagus = 75;
        public int streakHari = 3;
        public int levelKebun = 2;
        public int xpKebun = 40;

        public List<Jadwal> daftarJadwal = new List<Jadwal>();
        public List<Materi> daftarMateri = new List<Materi>();
        public List<HasilKuis> daftarHasilKuis = new List<HasilKuis>();
        public List<Kartu> daftarKartu = new List<Kartu>();
        public List<SoalKuis> daftarSoalKuis = new List<SoalKuis>();
        public List<SoalIsyarat> daftarSoalIsyarat = new List<SoalIsyarat>();
        public List<AktivitasAnak> riwayatAktivitas = new List<AktivitasAnak>();
    }
}
