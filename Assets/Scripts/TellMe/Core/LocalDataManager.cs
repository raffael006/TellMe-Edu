using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TellMe.Core
{
    /// <summary>
    /// Mengelola penyimpanan data 100% offline di perangkat (tanpa jaringan/server).
    /// Digunakan bersama pada satu perangkat oleh Orang Tua (Bu Rina), Siswa/Siswi SD (Raka / Rani), dan Guru (Bu Sari).
    /// </summary>
    public class LocalDataManager : MonoBehaviour
    {
        public static LocalDataManager Instance { get; private set; }

        private const string FileName = "tellme_offline_db.json";
        private string SaveFilePath => Path.Combine(Application.persistentDataPath, FileName);

        public TellMeDatabase Data { get; private set; }

        public event Action OnDatabaseUpdated;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDatabase();
        }

        public void LoadDatabase()
        {
            if (File.Exists(SaveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(SaveFilePath);
                    Data = JsonUtility.FromJson<TellMeDatabase>(json);
                    EnsureDataIntegrity();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[TellMe] Gagal membaca database lokal, membuat data awal: {e.Message}");
                    SeedDefaultData();
                }
            }
            else
            {
                SeedDefaultData();
            }
        }

        private void EnsureDataIntegrity()
        {
            if (Data == null)
            {
                SeedDefaultData();
                return;
            }

            if (string.IsNullOrEmpty(Data.peranSiswa))
                Data.peranSiswa = Data.isSiswiPerempuan ? "Siswi SD" : "Siswa SD";
            if (Data.levelKebun < 1) Data.levelKebun = 2;
            if (Data.xpKebun < 0) Data.xpKebun = 120;
            if (Data.streakHari < 1) Data.streakHari = 3;
            if (Data.riwayatAktivitas == null) Data.riwayatAktivitas = new List<AktivitasAnak>();

            if (Data.daftarMateri != null)
            {
                foreach (var m in Data.daftarMateri)
                {
                    if (string.IsNullOrEmpty(m.ringkasan))
                        m.ringkasan = GenerateRingkasanGuru(m.mataPelajaran, m.teks);
                    if (string.IsNullOrEmpty(m.rumusVisual))
                        m.rumusVisual = GetRumusVisual(m.mataPelajaran);
                }
            }

            if (Data.riwayatAktivitas.Count == 0)
            {
                Data.riwayatAktivitas.Add(new AktivitasAnak
                {
                    id = "ACT-001",
                    waktu = "Hari ini, 08:25",
                    kategori = "Kuis",
                    ikon = "🏆",
                    deskripsi = "Menyelesaikan Kuis Matematika (Skor 85%) & Membuka Kartu 3D Apel Merah"
                });
                Data.riwayatAktivitas.Add(new AktivitasAnak
                {
                    id = "ACT-002",
                    waktu = "Hari ini, 08:15",
                    kategori = "STT Guru",
                    ikon = "🎙️",
                    deskripsi = "Merekam & Meringkas Penjelasan Guru: Penjumlahan & Pengurangan 1-20"
                });
                Data.riwayatAktivitas.Add(new AktivitasAnak
                {
                    id = "ACT-003",
                    waktu = "Kemarin, 16:10",
                    kategori = "Kebun 3D",
                    ikon = "🌱",
                    deskripsi = "Menyiram Kebun 3D hingga naik ke Level 2 (Tunas Muda)"
                });
            }
        }

        public void SaveDatabase()
        {
            try
            {
                string json = JsonUtility.ToJson(Data, true);
                File.WriteAllText(SaveFilePath, json);
                OnDatabaseUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[TellMe] Gagal menyimpan database lokal: {e.Message}");
            }
        }

        /// <summary>
        /// Mengganti profil antara Siswa SD (Laki-laki) dan Siswi SD (Perempuan).
        /// </summary>
        public void GantiProfilSiswa(bool siswiPerempuan)
        {
            Data.isSiswiPerempuan = siswiPerempuan;
            if (siswiPerempuan)
            {
                Data.namaSiswa = "Rani (Siswi Kelas 3 SD/SLB)";
                Data.peranSiswa = "Siswi SD";
            }
            else
            {
                Data.namaSiswa = "Raka (Siswa Kelas 3 SD/SLB)";
                Data.peranSiswa = "Siswa SD";
            }
            CatatAktivitas(siswiPerempuan ? "👧" : "👦", "Profil", $"Beralih ke profil aktif: {Data.namaSiswa}");
            SaveDatabase();
        }

        /// <summary>
        /// Mencatat aktivitas anak untuk Dashboard Pemantauan Aktivitas Anak (Orang Tua & Guru).
        /// </summary>
        public void CatatAktivitas(string ikon, string kategori, string deskripsi)
        {
            if (Data.riwayatAktivitas == null)
                Data.riwayatAktivitas = new List<AktivitasAnak>();

            Data.riwayatAktivitas.Insert(0, new AktivitasAnak
            {
                id = "ACT-" + Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper(),
                waktu = DateTime.Now.ToString("HH:mm") + " WIB",
                kategori = kategori,
                ikon = ikon,
                deskripsi = deskripsi
            });

            if (Data.riwayatAktivitas.Count > 25)
                Data.riwayatAktivitas.RemoveAt(Data.riwayatAktivitas.Count - 1);
        }

        /// <summary>
        /// Menambahkan XP pada Kebun 3D dan menaikkan level pohon secara otomatis (Level 1 s/d 7).
        /// </summary>
        public bool TambahXpKebun(int xpTambahan, string aktivitasDeskripsi = null)
        {
            int levelLama = Data.levelKebun;
            Data.xpKebun += xpTambahan;
            Data.levelKebun = Mathf.Clamp(1 + (Data.xpKebun / 100), 1, 7);
            bool naikLevel = Data.levelKebun > levelLama;

            if (!string.IsNullOrEmpty(aktivitasDeskripsi))
            {
                CatatAktivitas("🌱", "Kebun 3D", aktivitasDeskripsi + (naikLevel ? $" (Naik ke Level {Data.levelKebun}!)" : ""));
            }

            SaveDatabase();
            return naikLevel;
        }

        /// <summary>
        /// Menambahkan jadwal belajar baru dari Mode Orang Tua.
        /// </summary>
        public Jadwal TambahJadwal(string hari, int dayIndex, string jam, SubjectType mapel, string judul, string namaGuru)
        {
            var jadwalBaru = new Jadwal
            {
                id = "JDW-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                hari = hari,
                dayIndex = dayIndex,
                jam = jam,
                mataPelajaran = mapel,
                judul = judul,
                namaGuru = namaGuru,
                pengingatAktif = true
            };

            Data.daftarJadwal.Add(jadwalBaru);
            CatatAktivitas("📅", "Jadwal", $"Orang Tua menambahkan jadwal {mapel}: {judul} ({jam})");
            SaveDatabase();
            return jadwalBaru;
        }

        public void HapusJadwal(string jadwalId)
        {
            Data.daftarJadwal.RemoveAll(j => j.id == jadwalId);
            SaveDatabase();
        }

        /// <summary>
        /// Membuat ringkasan pintar secara otomatis dari hasil Speech-to-Text penjelasan guru.
        /// Dirancang khusus agar siswa/siswi Tuli SD langsung memahami poin inti pelajaran secara visual.
        /// </summary>
        public static string GenerateRingkasanGuru(SubjectType mapel, string teks)
        {
            if (mapel == SubjectType.Matematika)
            {
                return "• Poin 1: Raka/Rani memiliki 4 buah apel merah segar di awal.\n" +
                       "• Poin 2: Ditambah lagi 3 buah apel merah dari Ibu Guru.\n" +
                       "• Kesimpulan: Jika digabungkan semuanya menjadi 7 buah apel merah (4 + 3 = 7).";
            }
            if (mapel == SubjectType.BahasaIndonesia)
            {
                return "• Poin 1: Buah jeruk manis berwarna oranye cerah.\n" +
                       "• Poin 2: Jeruk mengandung banyak Vitamin C agar tubuh tidak mudah sakit.\n" +
                       "• Kesimpulan: Rajin makan buah segar membuat tubuh sehat dan kuat.";
            }
            if (mapel == SubjectType.BahasaInggris)
            {
                return "• Poin 1: Bahasa Inggris Apel Merah adalah 'Red Apple'.\n" +
                       "• Poin 2: Bahasa Inggris Jeruk Manis adalah 'Sweet Orange' & Mangga adalah 'Fragrant Mango'.\n" +
                       "• Kesimpulan: Hafalkan warna dan nama buah dalam Bahasa Inggris beserta isyarat tangannya.";
            }

            // Fallback otomatis dari kalimat teks
            if (!string.IsNullOrEmpty(teks))
            {
                string[] kalimat = teks.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                List<string> poin = new List<string>();
                for (int i = 0; i < Mathf.Min(kalimat.Length, 3); i++)
                {
                    string k = kalimat[i].Trim();
                    if (!string.IsNullOrEmpty(k))
                        poin.Add($"• Poin {i + 1}: {k}.");
                }
                if (poin.Count > 0) return string.Join("\n", poin);
            }
            return "• Poin Inti: Perhatikan kata kunci dan rumus visual dari penjelasan guru di atas.";
        }

        public static string GetRumusVisual(SubjectType mapel)
        {
            switch (mapel)
            {
                case SubjectType.Matematika:
                    return "🍎🍎🍎🍎 (4) + 🍎🍎🍎 (3) = 7 Apel Merah";
                case SubjectType.BahasaIndonesia:
                    return "🍊 Jeruk Manis (Oranye) ➔ Kaya Vitamin C ➔ Tubuh Sehat 💪";
                case SubjectType.BahasaInggris:
                    return "🍎 = Red Apple  |  🍊 = Sweet Orange  |  🥭 = Fragrant Mango";
                default:
                    return "💡 Pahami konsep visual dan isyarat tangan SIBI/BISINDO";
            }
        }

        /// <summary>
        /// Menyimpan hasil transkripsi Speech-to-Text guru beserta Ringkasan Pintar Guru sebagai Materi Belajar.
        /// </summary>
        public Materi SimpanMateriRekaman(Jadwal jadwalAktif, string teksTranskripsi)
        {
            SubjectType mapel = jadwalAktif != null ? jadwalAktif.mataPelajaran : SubjectType.Matematika;
            string judul = jadwalAktif != null ? jadwalAktif.judul : "Penjumlahan Buah di Keranjang";
            string guru = jadwalAktif != null ? jadwalAktif.namaGuru : "Bu Sari";

            var materiBaru = new Materi
            {
                id = "MTR-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                jadwalId = jadwalAktif != null ? jadwalAktif.id : "MANUAL",
                mataPelajaran = mapel,
                judul = judul,
                namaGuru = guru,
                teks = teksTranskripsi,
                ringkasan = GenerateRingkasanGuru(mapel, teksTranskripsi),
                rumusVisual = GetRumusVisual(mapel),
                tanggal = DateTime.Now.ToString("dd MMM yyyy HH:mm")
            };

            Data.daftarMateri.Insert(0, materiBaru);
            CatatAktivitas("🎙️", "STT & Ringkasan Guru", $"Merekam & meringkas suara {guru} pada mapel {mapel} ({judul})");
            SaveDatabase();
            return materiBaru;
        }

        /// <summary>
        /// Menyimpan nilai kuis per mata pelajaran dan membuka Kartu Model 3D AR yang sesuai dengan mata pelajaran kuis tersebut.
        /// </summary>
        public bool SimpanHasilKuis(SubjectType mapel, string materiId, int skor, out Kartu kartuTerbuka)
        {
            bool lulus = skor >= Data.ambangNilaiBagus;
            var hasil = new HasilKuis
            {
                id = "KZS-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                materiId = materiId,
                mataPelajaran = mapel,
                skor = skor,
                lulusAmbang = lulus,
                tanggal = DateTime.Now.ToString("dd MMM yyyy HH:mm")
            };

            Data.daftarHasilKuis.Add(hasil);
            kartuTerbuka = null;

            if (lulus)
            {
                // Prioritaskan kartu dari mata pelajaran kuis yang baru saja diselesaikan anak!
                kartuTerbuka = Data.daftarKartu.FirstOrDefault(k => k.mataPelajaran == mapel && !k.terbuka)
                               ?? Data.daftarKartu.FirstOrDefault(k => k.mataPelajaran == mapel)
                               ?? Data.daftarKartu.FirstOrDefault(k => !k.terbuka);

                if (kartuTerbuka != null)
                {
                    kartuTerbuka.terbuka = true;
                    kartuTerbuka.labelBaru = true;
                    kartuTerbuka.tanggalDidapat = DateTime.Now.ToString("dd MMM yyyy");
                }

                // Tambah XP Kebun 3D setiap lulus kuis
                Data.xpKebun += 50;
                Data.levelKebun = Mathf.Clamp(1 + (Data.xpKebun / 100), 1, 7);

                string infoKartu = kartuTerbuka != null ? $" & Unlock Kartu 3D {kartuTerbuka.namaBuahId}" : "";
                CatatAktivitas("🏆", "Kuis & Unlock AR", $"Lulus Kuis {mapel} (Skor {skor}%){infoKartu} (+50 XP Kebun)");
            }
            else
            {
                CatatAktivitas("📝", "Kuis", $"Mengerjakan Kuis {mapel} (Skor {skor}% - Belum mencapai target 75%)");
            }

            SaveDatabase();
            return lulus;
        }

        /// <summary>
        /// Menghitung rata-rata nilai kuis terbaru untuk suatu mata pelajaran (0-100%).
        /// </summary>
        public int GetPersentaseNilaiMapel(SubjectType mapel)
        {
            var list = Data.daftarHasilKuis.Where(h => h.mataPelajaran == mapel).ToList();
            if (list.Count == 0) return 0;
            return Mathf.RoundToInt((float)list.Average(h => h.skor));
        }

        private void SeedDefaultData()
        {
            Data = new TellMeDatabase
            {
                namaSiswa = "Raka (Siswa Kelas 3 SD/SLB)",
                peranSiswa = "Siswa SD",
                isSiswiPerempuan = false,
                kelasSiswa = "Kelas 3 SD / SLB-B",
                pinOrangTua = "1234",
                ambangNilaiBagus = 75,
                levelKebun = 2,
                xpKebun = 120,
                streakHari = 3
            };

            // 1. Seed Jadwal Belajar (Diatur oleh Bu Rina)
            Data.daftarJadwal.Add(new Jadwal
            {
                id = "JDW-001",
                hari = "M",
                dayIndex = 1,
                jam = "08:00",
                mataPelajaran = SubjectType.Matematika,
                judul = "Penjumlahan & Pengurangan 1-20",
                namaGuru = "Bu Sari",
                pengingatAktif = true
            });
            Data.daftarJadwal.Add(new Jadwal
            {
                id = "JDW-002",
                hari = "T",
                dayIndex = 2,
                jam = "09:30",
                mataPelajaran = SubjectType.BahasaIndonesia,
                judul = "Membaca Cerita Kebun Buah",
                namaGuru = "Bu Sari",
                pengingatAktif = true
            });
            Data.daftarJadwal.Add(new Jadwal
            {
                id = "JDW-003",
                hari = "W",
                dayIndex = 3,
                jam = "10:30",
                mataPelajaran = SubjectType.BahasaInggris,
                judul = "Fruits & Colors Around Us",
                namaGuru = "Pak Budi",
                pengingatAktif = true
            });

            // 2. Seed Materi Belajar hasil Speech-to-Text + Ringkasan Guru
            Data.daftarMateri.Add(new Materi
            {
                id = "MTR-001",
                jadwalId = "JDW-001",
                mataPelajaran = SubjectType.Matematika,
                judul = "Penjumlahan & Pengurangan 1-20",
                namaGuru = "Bu Sari",
                teks = "Selamat pagi anak-anak. Hari ini kita belajar menjumlahkan buah apel. Jika Raka punya 4 apel merah, lalu Ibu memberi 3 apel lagi, maka semuanya ada 7 apel merah.",
                ringkasan = GenerateRingkasanGuru(SubjectType.Matematika, ""),
                rumusVisual = GetRumusVisual(SubjectType.Matematika),
                tanggal = "Hari ini, 08:15"
            });

            // 3. Seed Koleksi Kartu Digital (AR Buah 3D)
            // KRT-APEL terbuka dari Kuis Matematika awal, sedangkan Jeruk, Mangga, dan Pisang terbuka saat anak menyelesaikan kuis mapel terkait!
            Data.daftarKartu.Add(new Kartu
            {
                id = "KRT-APEL",
                namaBuahId = "Apel Merah",
                namaBuahEn = "Red Apple",
                deskripsiIsyarat = "Isyarat SIBI/BISINDO: Kepalkan tangan kanan, tempelkan buku jari di pipi lalu putar perlahan.",
                mataPelajaran = SubjectType.Matematika,
                terbuka = true,
                labelBaru = true,
                tanggalDidapat = "Hari ini",
                prefabArKey = "Fruit_Apple_3D"
            });
            Data.daftarKartu.Add(new Kartu
            {
                id = "KRT-JERUK",
                namaBuahId = "Jeruk Manis",
                namaBuahEn = "Sweet Orange",
                deskripsiIsyarat = "Isyarat SIBI/BISINDO: Buka-tutup genggaman tangan di depan dagu seperti memeras jeruk.",
                mataPelajaran = SubjectType.BahasaIndonesia,
                terbuka = false,
                labelBaru = false,
                tanggalDidapat = "-",
                prefabArKey = "Fruit_Orange_3D"
            });
            Data.daftarKartu.Add(new Kartu
            {
                id = "KRT-MANGGA",
                namaBuahId = "Mangga Harum",
                namaBuahEn = "Fragrant Mango",
                deskripsiIsyarat = "Isyarat SIBI/BISINDO: Bentuk tangan melengkung seperti buah mangga lalu dekatkan ke hidung.",
                mataPelajaran = SubjectType.BahasaInggris,
                terbuka = false,
                labelBaru = false,
                tanggalDidapat = "-",
                prefabArKey = "Fruit_Mango_3D"
            });
            Data.daftarKartu.Add(new Kartu
            {
                id = "KRT-PISANG",
                namaBuahId = "Pisang Emas",
                namaBuahEn = "Golden Banana",
                deskripsiIsyarat = "Isyarat SIBI/BISINDO: Acungkan jari telunjuk kiri, lalu jari kanan memperagakan mengupas kulit pisang.",
                mataPelajaran = SubjectType.Matematika,
                terbuka = false,
                labelBaru = false,
                tanggalDidapat = "-",
                prefabArKey = "Fruit_Banana_3D"
            });

            // 4. Seed Hasil Kuis awal
            Data.daftarHasilKuis.Add(new HasilKuis
            {
                id = "KZS-001",
                materiId = "MTR-001",
                mataPelajaran = SubjectType.Matematika,
                skor = 85,
                lulusAmbang = true,
                tanggal = "Hari ini"
            });
            Data.daftarHasilKuis.Add(new HasilKuis
            {
                id = "KZS-002",
                materiId = "MTR-002",
                mataPelajaran = SubjectType.BahasaIndonesia,
                skor = 70,
                lulusAmbang = false,
                tanggal = "Kemarin"
            });
            Data.daftarHasilKuis.Add(new HasilKuis
            {
                id = "KZS-003",
                materiId = "MTR-003",
                mataPelajaran = SubjectType.BahasaInggris,
                skor = 65,
                lulusAmbang = false,
                tanggal = "Kemarin"
            });

            // 5. Seed Riwayat Aktivitas Anak untuk Dashboard Pemantauan Orang Tua
            Data.riwayatAktivitas.Add(new AktivitasAnak
            {
                id = "ACT-001",
                waktu = "08:25 WIB",
                kategori = "Kuis & Unlock AR",
                ikon = "🏆",
                deskripsi = "Lulus Kuis Matematika (Skor 85%) & Unlock Kartu 3D Apel Merah"
            });
            Data.riwayatAktivitas.Add(new AktivitasAnak
            {
                id = "ACT-002",
                waktu = "08:15 WIB",
                kategori = "STT & Ringkasan Guru",
                ikon = "🎙️",
                deskripsi = "Merekam & meringkas suara Bu Sari: Penjumlahan & Pengurangan 1-20"
            });
            Data.riwayatAktivitas.Add(new AktivitasAnak
            {
                id = "ACT-003",
                waktu = "Kemarin",
                kategori = "Kebun 3D",
                ikon = "🌱",
                deskripsi = "Menyiram Kebun 3D hingga naik ke Level 2 (Tunas Muda)"
            });

            SaveDatabase();
        }
    }
}
