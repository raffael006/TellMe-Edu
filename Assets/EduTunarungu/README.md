# 🌟 TellMe AR - Template Aplikasi Edukasi Inklusif Wicara, Isyarat & AR (Unity Android)
> **Referensi Proyek:** `TellMe.apk` (Latihan Artikulasi Wicara, Isyarat BISINDO & Augmented Reality)  
> **Platform Target:** Unity (Unity 6 / 2022+) untuk Android  
> **Kategori:** Edukasi Inklusif, Speech Training, AR Camera, Taktil Haptik

---

## 🎯 Konsep Berdasarkan Referensi TellMe.apk
Aplikasi ini dikembangkan langsung dengan mereferensikan modul latihan wicara dan kosa kata dari **TellMe**:
1. **Latihan Melengkapi Kalimat Rumpang:**
   - *"Kita membaca ___ di sekolah."* ➔ **Buku**
   - *"Apel ini berwarna ___."* ➔ **Merah**
   - *"Burung bisa ___ di langit."* ➔ **Terbang**
   - *"Ikan hidup di dalam ___."* ➔ **Air**
   - *"Kucing suka minum ___."* ➔ **Susu**
   - *"Saya menyikat ___ setiap pagi."* ➔ **Gigi**
2. **Dua Mode Interaksi Anak:**
   - **Mode Suara (Voice Input):** *"Tekan, lalu ucapkan jawabanmu!"*
   - **Mode Sentuh (Pilihan Visual):** *"Atau pilih jawabannya:"*
3. **🌟 FITUR BARU: Mode Augmented Reality (AR):**
   - Menghadirkan objek materi pembelajaran (Buku, Apel, Burung, Ikan, Kucing, Gigi) sebagai **objek 3D interaktif di dunia nyata** melalui kamera HP!
   - Di atas objek 3D melayang balon dialog AR berisi kalimat tanya, isyarat BISINDO, dan panduan artikulasi bibir.
   - Anak dapat menyentuh objek 3D di ruangan (bergetar haptik + memantul riang) serta mengambil foto kenang-kenangan (*AR Snapshot*).

---

## 📱 Fitur Utama dalam Template

### 1. 📖 Modul Belajar Isyarat & Kata (Visual Flashcard)
- Kategori kosakata: **Sapaan**, **Hewan**, **Buah/Makanan**, dan **Emosi**.
- Kartu interaktif besar dengan pemenggalan suku kata berwarna cerah.
- Tombol **"Lihat Bentuk Mulut (Lip Reading)"** untuk latihan artikulasi wicara.
- Tombol **"Peragakan"** untuk menjalankan simulasi gerakan tangan berurutan.

### 2. 🎯 Kuis Tebak Isyarat (Interactive Visual Quiz)
- Pertanyaan berbasis gambar isyarat tangan.
- 4 Pilihan kartu gambar & teks kontras.
- Sistem reward **Bintang Visual (⭐)** dan perayaan layar hijau saat benar.
- Timer bar visual yang menyusut tanpa suara panik.

### 3. 🥁 Lab Ritme & Getaran Taktil (Haptic Lab)
- Melatih kepekaan sentuhan dan persepsi ritme anak terhadap suku kata:
  - **1 Ketukan:** Kosakata 1 suku kata (AIR, CAT, BAN).
  - **2 Ketukan:** Kosakata 2 suku kata (KU-CING, A-PEL).
  - **3 Ketukan:** Kosakata 3 suku kata (TE-RI-MA, SE-PE-DA).
  - **Pola Ceria:** Ritme tepuk tangan perayaan.
- Dilengkapi **Haptic Monitor** untuk memantau status getaran Android secara langsung.

### 4. ⚙️ Ruang Aksesibilitas (Customizer)
- **Getaran Haptic:** Toggle aktif/mati getaran pada ponsel.
- **Mode Kontras Tinggi (High Contrast):** Tema gelap OLED dengan aksen kuning neon & cyan untuk anak dengan hambatan penglihatan ganda (*low vision*).
- **Kecepatan Gerak Isyarat:** Opsi kecepatan 1.0x (Normal) atau 0.5x (Lambat).

### 5. 🛡️ Dukungan Android Notch / Safe Area
- UI secara otomatis menyesuaikan diri dengan *punch hole* kamera, poni layar (*notch*), dan *navigation bar* Android agar konten tidak terpotong.

---

## 🚀 Cara Menjalankan & Menguji di Unity

### Langkah Cepat (Langsung Coba):
1. Buka **Unity Hub**, pilih project `My project (2)` atau tambahkan folder `Joints`.
2. Buka scene baru atau `SampleScene`.
3. Di jendela **Hierarchy**, klik kanan -> **Create Empty**, beri nama `EduApp`.
4. Di jendela **Inspector**, klik **Add Component** -> cari dan pilih **`EduTunarungu`**.
5. Tekan tombol **Play (▶️)** di Unity Editor!
   - Seluruh antarmuka (Canvas, Tombol, Kartu, Kuis, Animasi) akan otomatis ter-generate secara prosedural tanpa perlu konfigurasi prefab manual!

### Pengujian di Perangkat Android Fisik (Rekomendasi untuk Uji Getar):
1. Sambungkan ponsel Android melalui kabel USB (aktifkan *USB Debugging*).
2. Di Unity Editor: **File** -> **Build Profiles** (atau **Build Settings**).
3. Pilih platform **Android** -> klik **Switch Platform**.
4. Di **Player Settings**:
   - **Orientation:** Portrait
   - **Minimum API Level:** Android 8.0 (API 26) atau lebih tinggi untuk getaran canggih (*VibrationEffect*).
5. Klik **Build and Run**.
6. Rasakan getaran taktil saat mengetuk suku kata, menjawab kuis, dan bermain di Lab Ritme!

---

## 📂 Struktur File Template

```
Joints/ (atau My project (2)/Assets/EduTunarungu/)
├── Assets/
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── AndroidHapticHelper.cs   # Driver getaran Android Native JNI
│   │   │   └── SafeAreaFitter.cs        # Penyesuai notch & poni Android
│   │   ├── Data/
│   │   │   ├── VocabularyItem.cs        # ScriptableObject data kosakata
│   │   │   └── QuizQuestionItem.cs      # ScriptableObject data kuis
│   │   └── UI/
│   │       └── EduTunarungu.cs          # Controller aplikasi & UI prosedural
│   └── README.md
```

---

## 🛠️ Cara Menambahkan Kosakata Baru

### Lewat Script (`EduTunarungu.cs`):
Buka file `EduTunarungu.cs` pada fungsi `SetupVocabularies()`, lalu tambahkan baris berikut:
```csharp
vocabularies.Add(new WordEntry(
    "MOBIL",                                    // Kata
    "Kendaraan",                                // Kategori
    new[] { "MO", "BIL" },                      // Pemenggalan suku kata
    "Pegang setir imajiner dengan kedua tangan lalu putar kiri-kanan.", // Isyarat
    "Bibir membulat (Mo), lalu bibir menutup rapat dan tersenyum (Bil).", // Bentuk Mulut
    ColorPrimary,                               // Warna aksen
    "🚗",                                       // Ikon / Ilustrasi
    "Gerakan Mengemudi Setir Mobil"             // Ringkasan langkah
));
```

### Lewat ScriptableObject (`VocabularyItem`):
1. Di Unity Editor: Klik kanan di Project Window -> **Create** -> **EduTunarungu** -> **Vocabulary Item**.
2. Masukkan kata, gambar isyarat (Sprite), gambar bentuk mulut, dan deskripsi artikulasi.
