using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.UI;

namespace KawanDengarEdu.MiniGame
{
    public enum GardenStage
    {
        SeedInPot,          // Biji dalam pot
        WateredSprout,      // Tunas hijau disiram
        FertilizedBush,     // Pohon kecil diberi pupuk sehat
        PestAlert,          // Serangan hama (perlu perisai!)
        FruitHarvestReady   // Pohon berbuah lebat siap panen
    }

    public class PlantingGameController : MonoBehaviour
    {
        [Header("Current Garden Stage")]
        public GardenStage currentStage = GardenStage.SeedInPot;

        [Header("Visual Tree GameObjects")]
        public GameObject potSeedObject;
        public GameObject sproutObject;
        public GameObject healthyTreeObject;
        public GameObject pestObject;
        public GameObject shieldEffectObject;
        public GameObject harvestFruitTreeObject;

        [Header("UI Action Buttons")]
        public Button waterButton;
        public Button fertilizerButton;
        public Button shieldButton;
        public Button harvestButton;
        public Button backToStudioButton;

        [Header("UI Text & Progress")]
        public TextMeshProUGUI stageInstructionText;
        public Slider progressSlider;

        [Header("Particle Systems")]
        public ParticleSystem waterParticles;
        public ParticleSystem sparkleParticles;

        [Header("Navigation")]
        public ScreenManager screenManager;

        private void Start()
        {
            ResetGarden();
        }

        public void ResetGarden()
        {
            currentStage = GardenStage.SeedInPot;
            UpdateStageVisuals();
        }

        public void OnClickWater()
        {
            if (currentStage == GardenStage.SeedInPot)
            {
                if (waterParticles != null) waterParticles.Play();
                currentStage = GardenStage.WateredSprout;
                UpdateStageVisuals();
            }
        }

        public void OnClickFertilize()
        {
            if (currentStage == GardenStage.WateredSprout)
            {
                if (sparkleParticles != null) sparkleParticles.Play();
                currentStage = GardenStage.FertilizedBush;
                UpdateStageVisuals();

                // Trigger serangan hama otomatis setelah 1 detik
                Invoke(nameof(TriggerPestAttack), 1.5f);
            }
        }

        private void TriggerPestAttack()
        {
            currentStage = GardenStage.PestAlert;
            UpdateStageVisuals();
        }

        public void OnClickActivateShield()
        {
            if (currentStage == GardenStage.PestAlert)
            {
                if (shieldEffectObject != null) shieldEffectObject.SetActive(true);
                if (sparkleParticles != null) sparkleParticles.Play();

                // Hama berhasil dihalau! Pohon berbuah lebat!
                Invoke(nameof(GrowJuicyFruits), 1.0f);
            }
        }

        private void GrowJuicyFruits()
        {
            currentStage = GardenStage.FruitHarvestReady;
            UpdateStageVisuals();
        }

        public void OnClickHarvest()
        {
            if (currentStage == GardenStage.FruitHarvestReady)
            {
                if (sparkleParticles != null) sparkleParticles.Play();

                // Panen selesai, bawa kembali buah segar ke Studio 3D
                Invoke(nameof(ReturnToStudioWithNewFruit), 1.0f);
            }
        }

        private void ReturnToStudioWithNewFruit()
        {
            if (screenManager != null)
            {
                screenManager.ShowARStudioScreen();
            }
        }

        private void UpdateStageVisuals()
        {
            // Toggle visual objek 3D
            if (potSeedObject != null) potSeedObject.SetActive(currentStage == GardenStage.SeedInPot);
            if (sproutObject != null) sproutObject.SetActive(currentStage == GardenStage.WateredSprout);
            if (healthyTreeObject != null) healthyTreeObject.SetActive(currentStage == GardenStage.FertilizedBush || currentStage == GardenStage.PestAlert);
            if (pestObject != null) pestObject.SetActive(currentStage == GardenStage.PestAlert);
            if (shieldEffectObject != null) shieldEffectObject.SetActive(false);
            if (harvestFruitTreeObject != null) harvestFruitTreeObject.SetActive(currentStage == GardenStage.FruitHarvestReady);

            // Toggle tombol aksi sesuai tahapan
            if (waterButton != null) waterButton.gameObject.SetActive(currentStage == GardenStage.SeedInPot);
            if (fertilizerButton != null) fertilizerButton.gameObject.SetActive(currentStage == GardenStage.WateredSprout);
            if (shieldButton != null) shieldButton.gameObject.SetActive(currentStage == GardenStage.PestAlert);
            if (harvestButton != null) harvestButton.gameObject.SetActive(currentStage == GardenStage.FruitHarvestReady);

            // Update Progress Bar & Teks Instruksi
            bool isIndo = BilingualManager.Instance == null || BilingualManager.Instance.IsIndonesian();

            switch (currentStage)
            {
                case GardenStage.SeedInPot:
                    if (progressSlider != null) progressSlider.value = 0.2f;
                    if (stageInstructionText != null) stageInstructionText.text = isIndo ? "🌱 Biji tertanam di pot tanah. Ayo berikan air segar agar bertunas!" : "🌱 Seed is in the pot. Give it fresh water to sprout!";
                    break;
                case GardenStage.WateredSprout:
                    if (progressSlider != null) progressSlider.value = 0.45f;
                    if (stageInstructionText != null) stageInstructionText.text = isIndo ? "🌿 Tunas kecil tumbuh! Berikan pupuk organik sehat agar daunnya rimbun." : "🌿 Little sprout emerged! Add organic fertilizer for rich leaves.";
                    break;
                case GardenStage.FertilizedBush:
                    if (progressSlider != null) progressSlider.value = 0.65f;
                    if (stageInstructionText != null) stageInstructionText.text = isIndo ? "🌳 Pohon tumbuh kuat! Waspada terhadap hama ulat..." : "🌳 Tree is growing strong! Watch out for caterpillars...";
                    break;
                case GardenStage.PestAlert:
                    if (progressSlider != null) progressSlider.value = 0.75f;
                    if (stageInstructionText != null) stageInstructionText.text = isIndo ? "⚠️ Hama ulat daun menyerang! Tekan 'Pasang Perisai' untuk melindungi pohon!" : "⚠️ Pests incoming! Press 'Shield Defense' to protect the tree!";
                    break;
                case GardenStage.FruitHarvestReady:
                    if (progressSlider != null) progressSlider.value = 1.0f;
                    if (stageInstructionText != null) stageInstructionText.text = isIndo ? "🍎 Pohon berhasil dilindungi dan berbuah lebat! Petik buah segar sekarang!" : "🍎 Tree is saved and full of juicy fruits! Harvest fresh fruits now!";
                    break;
            }
        }
    }
}
