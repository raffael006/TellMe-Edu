using System;
using UnityEngine;

namespace KawanDengarEdu.Core
{
    public enum AppLanguage
    {
        Indonesian,
        English
    }

    public class BilingualManager : MonoBehaviour
    {
        public static BilingualManager Instance { get; private set; }

        [SerializeField] private AppLanguage currentLanguage = AppLanguage.Indonesian;
        public AppLanguage CurrentLanguage => currentLanguage;

        public event Action<AppLanguage> OnLanguageChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                int savedLang = PlayerPrefs.GetInt("APP_LANG", 0);
                currentLanguage = (AppLanguage)savedLang;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetLanguage(AppLanguage language)
        {
            currentLanguage = language;
            PlayerPrefs.SetInt("APP_LANG", (int)language);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke(currentLanguage);
        }

        public void ToggleLanguage()
        {
            SetLanguage(currentLanguage == AppLanguage.Indonesian ? AppLanguage.English : AppLanguage.Indonesian);
        }

        public bool IsIndonesian() => currentLanguage == AppLanguage.Indonesian;

        public string GetLocalized(string idText, string enText)
        {
            return IsIndonesian() ? idText : enText;
        }
    }
}
