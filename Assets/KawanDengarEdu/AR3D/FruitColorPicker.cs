using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.Models;

namespace KawanDengarEdu.AR3D
{
    public class FruitColorPicker : MonoBehaviour
    {
        [Header("Target Fruit References")]
        public FruitDecayTimer decayTimer;

        [Header("UI Feedback")]
        public TextMeshProUGUI funFactText;

        private List<FruitColorVariant> variants = new List<FruitColorVariant>();
        private int currentVariantIndex = 0;

        private void Awake()
        {
            variants.Add(new FruitColorVariant("red", "Merah Fuji", "Fuji Red", new Color(0.94f, 0.27f, 0.27f), "Apel merah kaya antioksidan dan rasanya manis renyah!", "Red apples are rich in antioxidants and taste sweet & crisp!"));
            variants.Add(new FruitColorVariant("green", "Hijau Malang", "Green Apple", new Color(0.06f, 0.73f, 0.51f), "Apel hijau rasanya segar dan sangat tinggi vitamin C!", "Green apples taste refreshing and are extra high in Vitamin C!"));
            variants.Add(new FruitColorVariant("yellow", "Kuning Tropis", "Golden Yellow", new Color(0.96f, 0.62f, 0.04f), "Apel kuning aromanya sangat harum dan lembut!", "Yellow apples have a wonderfully sweet fragrance and soft texture!"));
            variants.Add(new FruitColorVariant("purple", "Ungu Fantasi", "Magic Purple", new Color(0.55f, 0.36f, 0.96f), "Warna imajinasi kreatif khusus karya siswa ceria!", "Creative fantasy color designed for joyful children!"));
        }

        private void Start()
        {
            SelectColorVariant(0);
        }

        public void SelectColorVariant(int index)
        {
            if (index >= 0 && index < variants.Count)
            {
                currentVariantIndex = index;
                var selected = variants[index];

                if (decayTimer != null)
                {
                    decayTimer.SetFreshColor(selected.primaryColor);
                }

                UpdateFactUI();
            }
        }

        public void SelectColorByHex(string hexColor)
        {
            if (ColorUtility.TryParseHtmlString(hexColor, out Color col))
            {
                if (decayTimer != null)
                {
                    decayTimer.SetFreshColor(col);
                }
            }
        }

        public void UpdateFactUI()
        {
            if (funFactText != null && variants.Count > currentVariantIndex)
            {
                bool isIndo = BilingualManager.Instance != null && BilingualManager.Instance.IsIndonesian();
                funFactText.text = variants[currentVariantIndex].GetLocalizedFact(isIndo);
            }
        }
    }
}
