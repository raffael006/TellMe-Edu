using System.Collections.Generic;
using UnityEngine;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.Models;

namespace KawanDengarEdu.AR3D
{
    public class FruitAnatomyController : MonoBehaviour
    {
        [Header("UI Dialog")]
        public GameObject anatomyModalPanel;
        public TextMeshProUGUI titleText;
        public Transform anatomyListContent;

        private List<FruitAnatomyPart> anatomyParts = new List<FruitAnatomyPart>();

        private void Awake()
        {
            anatomyParts.Add(new FruitAnatomyPart("Eksokarp (Kulit Luar)", "Exocarp (Skin)", "Epicarp", "Lapisan pelindung luar yang mengandung pigmen warna dan serat alami.", "Outer protective layer containing color pigments and dietary fiber.", "🍎"));
            anatomyParts.Add(new FruitAnatomyPart("Mesokarp (Daging Buah)", "Mesocarp (Flesh)", "Mesocarpium", "Bagian paling lezat dan kaya akan air, gula alami, serta Vitamin C.", "Delicious juicy edible part packed with water, natural sugar, and Vitamin C.", "🥩"));
            anatomyParts.Add(new FruitAnatomyPart("Endokarp (Kantung Biji)", "Endocarp (Core)", "Endocarpium", "Lapisan berserat keras yang melindungi biji di bagian tengah buah.", "Fibrous inner core structure that safeguards seeds inside.", "⭕"));
            anatomyParts.Add(new FruitAnatomyPart("Biji Apel", "Apple Seeds", "Semen", "Mengandung embrio bakal calon pohon apel baru jika ditanam.", "Contains embryo that can sprout into a whole new apple tree when planted.", "🌱"));
            anatomyParts.Add(new FruitAnatomyPart("Tangkai & Daun", "Stem & Leaf", "Pedicel", "Penghubung buah dengan ranting pohon untuk menyalurkan nutrisi.", "Connects the fruit to the tree branch to deliver water and vital nutrients.", "🌿"));
        }

        public void OpenAnatomyModal()
        {
            if (anatomyModalPanel != null)
            {
                anatomyModalPanel.SetActive(true);
            }
        }

        public void CloseAnatomyModal()
        {
            if (anatomyModalPanel != null)
            {
                anatomyModalPanel.SetActive(false);
            }
        }

        public void ToggleAnatomyModal()
        {
            if (anatomyModalPanel != null)
            {
                anatomyModalPanel.SetActive(!anatomyModalPanel.activeSelf);
            }
        }
    }
}
