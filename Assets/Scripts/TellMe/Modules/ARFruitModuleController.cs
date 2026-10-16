using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;
using UnityEngine.UI;

namespace TellMe.Modules
{
    /// <summary>
    /// Modul 7: AR Buah (Pengguna: Siswa / Raka).
    /// Menampilkan layar kamera AR dengan indikator permukaan tempat siswa memunculkan objek 3D Buah,
    /// dilengkapi panel glassmorphism di bagian bawah berisi nama buah dalam Bahasa Indonesia,
    /// Bahasa Inggris, dan animasi isyarat SIBI/BISINDO didampingi BerryBears.
    /// </summary>
    public class ARFruitModuleController : MonoBehaviour
    {
        [Header("Panel Glassmorphism Bawah (Info Buah & Isyarat)")]
        [SerializeField] private Text fruitNameIndonesianText;
        [SerializeField] private Text fruitNameEnglishText;
        [SerializeField] private Text signLanguageInstructionText;

        [Header("Objek 3D Buah pada Kamera AR")]
        [SerializeField] private Transform arSpawnAnchor;
        [SerializeField] private GameObject currentSpawnedFruitObject;

        private Kartu currentCard;

        public Kartu CurrentCard => currentCard;

        public void SetupComponents(
            Text nameId, Text nameEn, Text signInstruction, Transform anchor, GameObject fruitObj)
        {
            fruitNameIndonesianText = nameId;
            fruitNameEnglishText = nameEn;
            signLanguageInstructionText = signInstruction;
            arSpawnAnchor = anchor;
            currentSpawnedFruitObject = fruitObj;

            BindFruitInfoPanel();
        }

        private void OnEnable()
        {
            currentCard = AppNavigationManager.Instance?.ActiveCardForAR;
            if (currentCard == null && LocalDataManager.Instance != null && LocalDataManager.Instance.Data.daftarKartu.Count > 0)
            {
                currentCard = LocalDataManager.Instance.Data.daftarKartu[0];
            }

            BindFruitInfoPanel();
        }

        private void BindFruitInfoPanel()
        {
            if (currentCard == null) return;

            if (fruitNameIndonesianText != null)
                fruitNameIndonesianText.text = $"ID: {currentCard.namaBuahId}";
            if (fruitNameEnglishText != null)
                fruitNameEnglishText.text = $"EN: {currentCard.namaBuahEn}";
            if (signLanguageInstructionText != null)
                signLanguageInstructionText.text = currentCard.deskripsiIsyarat;

            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                $"Ini objek 3D {currentCard.namaBuahId} ({currentCard.namaBuahEn})! Ayo peragakan juga isyarat tangannya!");
        }

        /// <summary>
        /// Dipanggil saat siswa mengetuk indikator permukaan AR untuk memunculkan atau memutar buah 3D.
        /// </summary>
        public void OnArSurfaceTapped()
        {
            ThemeTokens.TriggerHapticFeedback();

            if (currentSpawnedFruitObject != null)
            {
                currentSpawnedFruitObject.SetActive(true);
                currentSpawnedFruitObject.transform.Rotate(0f, 45f, 0f);
            }

            string fruitName = currentCard != null ? currentCard.namaBuahId : "Apel Merah";
            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                $"Wah, {fruitName} 3D muncul di depanmu! Kamu juga bisa lanjut melihat Tumbuh Kembang Tanaman!");
        }

        public void OpenPlantGrowthFromAR()
        {
            AppNavigationManager.Instance?.NavigateTo(TellMeScreen.Module8_TumbuhKembangTanaman);
        }
    }
}
