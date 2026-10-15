using System;
using System.Collections.Generic;
using UnityEngine;

namespace KawanDengarEdu.Models
{
    public enum FruitType
    {
        Apple,
        Orange,
        Banana
    }

    [Serializable]
    public class FruitColorVariant
    {
        public string id;
        public string nameId;
        public string nameEn;
        public Color primaryColor;
        public string funFactId;
        public string funFactEn;

        public FruitColorVariant(string id, string nameId, string nameEn, Color color, string factId, string factEn)
        {
            this.id = id;
            this.nameId = nameId;
            this.nameEn = nameEn;
            this.primaryColor = color;
            this.funFactId = factId;
            this.funFactEn = factEn;
        }

        public string GetLocalizedName(bool isIndo) => isIndo ? nameId : nameEn;
        public string GetLocalizedFact(bool isIndo) => isIndo ? funFactId : funFactEn;
    }

    [Serializable]
    public class FruitAnatomyPart
    {
        public string partNameId;
        public string partNameEn;
        public string scientificName;
        public string descriptionId;
        public string descriptionEn;
        public string iconEmoji;

        public FruitAnatomyPart(string nameId, string nameEn, string sciName, string descId, string descEn, string emoji)
        {
            this.partNameId = nameId;
            this.partNameEn = nameEn;
            this.scientificName = sciName;
            this.descriptionId = descId;
            this.descriptionEn = descEn;
            this.iconEmoji = emoji;
        }

        public string GetLocalizedName(bool isIndo) => isIndo ? partNameId : partNameEn;
        public string GetLocalizedDesc(bool isIndo) => isIndo ? descriptionId : descriptionEn;
    }
}
