using System;
using System.Collections;
using System.Collections.Generic;
using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;
using UnityEngine.UI;

namespace TellMe.Modules
{
    /// <summary>
    /// Modul 8: Kebun 3D & Tumbuh Kembang Tanaman yang Bertumbuh Seiring Bertambahnya Level (Level 1 s/d Level 7).
    /// - Siswa memilih & menanam biji buah (Apel, Jeruk, Mangga, Pisang).
    /// - Harus disiram secara teratur; tanah memiliki tingkat kelembapan.
    /// - Kebun 3D bertumbuh secara fisik seiring naiknya Level Kebun & XP dari kuis maupun siraman air.
    /// - Jika tidak disiram selama 2 hari, muncul peringatan & notifikasi lokal offline.
    /// </summary>
    public class PlantGrowthModuleController : MonoBehaviour
    {
        public enum GrowthStage
        {
            BelumTanam = 0,
            BijiDiTanah = 1,
            Berkecambah = 2,
            TunasMuda = 3,
            TanamanRimbun = 4,
            PohonBerbunga = 5,
            BuahMuda = 6,
            BuahMatang = 7
        }

        [Header("State Tanaman & Level Kebun 3D")]
        [SerializeField] private string jenisBuah = "Apel Merah";
        [SerializeField] private GrowthStage tahapanSekarang = GrowthStage.Berkecambah;
        [SerializeField] private float kelembapanTanah = 85f; // 0 - 100%
        [SerializeField] private float hariSejakDisiram = 0f; // Jika >= 2.0 hari -> Tanaman layu & kirim alert
        [SerializeField] private bool tanamanLayu = false;

        [Header("Model 3D per Fase")]
        [SerializeField] private GameObject[] stage3DModels;
        [SerializeField] private ParticleSystem waterDropParticles;

        [Header("UI Komponen")]
        [SerializeField] private Text stageTitleText;
        [SerializeField] private Text soilMoistureText;
        [SerializeField] private Slider soilMoistureSlider;
        [SerializeField] private Text lastWateredText;
        [SerializeField] private GameObject wateringAlertBadge;
        [SerializeField] private Button waterButton;

        public event Action<GrowthStage> OnStageChanged;
        public event Action OnNeglectAlertTriggered;
        public event Action OnWateredEffectRequested;

        public GrowthStage CurrentStage => tahapanSekarang;
        public float SoilMoisture => kelembapanTanah;
        public bool IsNeglected => tanamanLayu;
        public string CurrentFruit => jenisBuah;

        public int CurrentLevel => Mathf.Clamp((int)tahapanSekarang, 1, 7);

        public void SetupComponents(
            Text stageTitle, Text moistureText, Slider moistureSlider, Text lastWatered, GameObject alertBadge, Button waterBtn)
        {
            stageTitleText = stageTitle;
            soilMoistureText = moistureText;
            soilMoistureSlider = moistureSlider;
            lastWateredText = lastWatered;
            wateringAlertBadge = alertBadge;
            waterButton = waterBtn;

            SyncLevelFromDatabase();

            if (waterButton != null)
            {
                waterButton.onClick.RemoveAllListeners();
                waterButton.onClick.AddListener(SiramTanaman);
            }

            UpdateUI();
        }

        public void SyncLevelFromDatabase()
        {
            if (LocalDataManager.Instance != null && LocalDataManager.Instance.Data != null)
            {
                int dbLevel = Mathf.Clamp(LocalDataManager.Instance.Data.levelKebun, 1, 7);
                tahapanSekarang = (GrowthStage)dbLevel;
            }
        }

        public void ToggleNeglectSimulation()
        {
            tanamanLayu = !tanamanLayu;
            if (tanamanLayu)
            {
                kelembapanTanah = 15f;
                hariSejakDisiram = 2.5f;
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_RETRY,
                    "⚠️ Tanamanmu haus! Sudah 2 hari belum disiram, ayo siram sekarang!");
                OnNeglectAlertTriggered?.Invoke();
            }
            else
            {
                kelembapanTanah = 85f;
                hariSejakDisiram = 0.2f;
            }
            UpdateUI();
        }

        private void OnEnable()
        {
            SyncLevelFromDatabase();
            UpdateUI();
        }

        /// <summary>
        /// Aksi 1: Mengganti / Menanam Pohon Buah di Kebun 3D
        /// </summary>
        public void TanamBiji(string namaBuah)
        {
            jenisBuah = namaBuah;
            kelembapanTanah = 100f;
            hariSejakDisiram = 0f;
            tanamanLayu = false;

            ThemeTokens.TriggerHapticFeedback();
            LocalDataManager.Instance?.CatatAktivitas("🌱", "Kebun 3D", $"Memilih tanaman Kebun 3D: Pohon {jenisBuah}");
            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                $"Kebun 3D sekarang menanam Pohon {jenisBuah} (Level {CurrentLevel})! Ayo siram agar levelnya makin tinggi!");

            UpdateUI();
        }

        /// <summary>
        /// Aksi 2: Siram Tanaman & Naikkan XP/Level Kebun 3D
        /// </summary>
        public void SiramTanaman()
        {
            ThemeTokens.TriggerHapticFeedback();
            kelembapanTanah = 100f;
            hariSejakDisiram = 0f;
            tanamanLayu = false;

            if (waterDropParticles != null)
            {
                waterDropParticles.Play();
            }

            OnWateredEffectRequested?.Invoke();

            if (tahapanSekarang < GrowthStage.BuahMatang)
            {
                tahapanSekarang++;
            }

            if (LocalDataManager.Instance != null)
            {
                LocalDataManager.Instance.Data.levelKebun = (int)tahapanSekarang;
                LocalDataManager.Instance.Data.xpKebun = Mathf.Max(LocalDataManager.Instance.Data.xpKebun + 35, ((int)tahapanSekarang - 1) * 100 + 20);
                LocalDataManager.Instance.CatatAktivitas("💧", "Kebun 3D", $"Menyiram Kebun 3D {jenisBuah} hingga tumbuh ke Level {(int)tahapanSekarang}");
                LocalDataManager.Instance.SaveDatabase();
            }

            string speech = tahapanSekarang == GrowthStage.BuahMatang
                ? $"Segar sekali! Kebun 3D {jenisBuah} sudah mencapai Level 7 (Panen Buah Matang)! Hore!"
                : $"Tanah sudah basah & segar! Kebun 3D {jenisBuah} bertumbuh naik ke Level {(int)tahapanSekarang}!";

            BerryBearsController.Instance?.SetMascotState(
                tahapanSekarang == GrowthStage.BuahMatang ? BerryBearsState.STATE_QUIZ_SUCCESS : BerryBearsState.STATE_ON_TAP,
                speech);

            UpdateUI();
        }

        public void ResetGardenLevelTo1()
        {
            tahapanSekarang = GrowthStage.BijiDiTanah;
            kelembapanTanah = 85f;
            tanamanLayu = false;
            if (LocalDataManager.Instance != null)
            {
                LocalDataManager.Instance.Data.levelKebun = 1;
                LocalDataManager.Instance.Data.xpKebun = 20;
                LocalDataManager.Instance.SaveDatabase();
            }
            UpdateUI();
        }

        public void SimulasiWaktuBerlalu(float penambahanHari)
        {
            hariSejakDisiram += penambahanHari;
            kelembapanTanah = Mathf.Max(0f, kelembapanTanah - (penambahanHari * 40f));

            if (hariSejakDisiram >= 2f && !tanamanLayu)
            {
                tanamanLayu = true;
                TriggerNeglectAlert();
            }

            UpdateUI();
        }

        private void TriggerNeglectAlert()
        {
            ThemeTokens.TriggerHapticFeedback();
            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_QUIZ_RETRY,
                "⚠️ Tanamanmu haus! Sudah 2 hari belum disiram, ayo siram sekarang!");

            OnNeglectAlertTriggered?.Invoke();
        }

        private void UpdateUI()
        {
            if (stageTitleText != null)
                stageTitleText.text = GetStageTitle(tahapanSekarang);

            if (soilMoistureSlider != null)
                soilMoistureSlider.value = kelembapanTanah / 100f;

            if (soilMoistureText != null)
                soilMoistureText.text = $"Kelembapan: {Mathf.RoundToInt(kelembapanTanah)}%";

            if (lastWateredText != null)
                lastWateredText.text = tanamanLayu
                    ? "⚠️ Kering! (2 hari belum disiram)"
                    : $"Terakhir disiram: {Mathf.RoundToInt(hariSejakDisiram * 24f)} jam lalu";

            if (wateringAlertBadge != null)
                wateringAlertBadge.SetActive(tanamanLayu);

            OnStageChanged?.Invoke(tahapanSekarang);
        }

        private string GetStageTitle(GrowthStage stage)
        {
            switch (stage)
            {
                case GrowthStage.BelumTanam: return "Level 1: Pilih Biji untuk Ditanam";
                case GrowthStage.BijiDiTanah: return "Level 1: Biji di Tanah (Baru Ditanam)";
                case GrowthStage.Berkecambah: return "Level 2: Berkecambah (Akar & Tunas Kecil)";
                case GrowthStage.TunasMuda: return "Level 3: Tunas Muda (Muncul Daun Segar)";
                case GrowthStage.TanamanRimbun: return "Level 4: Pohon Muda Bertumbuh Rimbun";
                case GrowthStage.PohonBerbunga: return "Level 5: Pohon Mulai Berbunga Mekar";
                case GrowthStage.BuahMuda: return "Level 6: Muncul Buah Muda Hijau";
                case GrowthStage.BuahMatang: return "Level 7 (MAX): Panen Buah Matang!";
                default: return "Tahap Pertumbuhan Kebun 3D";
            }
        }
    }
}
