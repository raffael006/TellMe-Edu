using System;
using UnityEngine;

namespace TellMe.Core
{
    public enum AppTheme
    {
        SunnyOrange = 0,
        FreshMint = 1,
        BerryPink = 2,
        OceanBlue = 3,
        SoftLavender = 4,
        ObsidianDark = 4 // Alias agar kompatibel dengan referensi lama
    }

    /// <summary>
    /// Token sistem desain TellMe: Bright Sunny Orange & Warm Butter Yellow (Bawaan)
    /// Dilengkapi fitur Ganti Tema Warna (AppTheme) sesuai preferensi siswa/siswi SD & orang tua.
    /// </summary>
    public static class ThemeTokens
    {
        public static AppTheme CurrentTheme = AppTheme.SunnyOrange;
        public static event Action<AppTheme> OnThemeChanged;

        // Background Primary: Warm Sunny Cream & Butterscotch
        public static readonly Color BackgroundPrimary = HexToColor("#FFF9F2");
        public static readonly Color BackgroundSecondary = HexToColor("#FFF4E5");
        public static readonly Color SurfaceCard = HexToColor("#FFFFFF");

        // Primary Accents: Bright Vibrant Orange & Sunny Yellow
        public static readonly Color AccentOrange = HexToColor("#FF7A00");
        public static readonly Color AccentAmber = HexToColor("#FFA000");
        public static readonly Color AccentSunnyYellow = HexToColor("#FFB703");

        // Secondary Accents: Fresh Mint Green (Success & Plant Growth)
        public static readonly Color AccentGreen = HexToColor("#10B981");
        public static readonly Color AccentGreenLight = HexToColor("#34D399");

        // Text & Contrast
        public static readonly Color TextPrimary = HexToColor("#291804");
        public static readonly Color TextSecondary = HexToColor("#744E23");

        // Warm Glass Surface: rgba(255, 255, 255, 0.88) & Warm Amber Border
        public static readonly Color GlassSurfaceWarm = new Color(1f, 1f, 1f, 0.88f);
        public static readonly Color GlassBorderWarm = new Color(255f / 255f, 160f / 255f, 0f / 255f, 0.25f);

        // Subject Pill Tag Colors
        public static readonly Color MathPillBg = HexToColor("#FFE4E6");
        public static readonly Color MathPillText = HexToColor("#9F1239");

        public static readonly Color IndoPillBg = HexToColor("#D1FAE5");
        public static readonly Color IndoPillText = HexToColor("#065F46");

        public static readonly Color EngPillBg = HexToColor("#FEF3C7");
        public static readonly Color EngPillText = HexToColor("#92400E");

        public static void SetTheme(AppTheme newTheme)
        {
            CurrentTheme = newTheme;
            TriggerHapticFeedback();
            OnThemeChanged?.Invoke(newTheme);
        }

        public static string GetThemeDisplayName(AppTheme theme)
        {
            switch (theme)
            {
                case AppTheme.SunnyOrange: return "Sunny";
                case AppTheme.FreshMint: return "Mint";
                case AppTheme.BerryPink: return "Berry";
                case AppTheme.OceanBlue: return "Ocean";
                case AppTheme.SoftLavender: return "Lavender";
                default: return "Sunny";
            }
        }

        public static void GetSubjectPillColors(SubjectType subject, out Color bgColor, out Color textColor)
        {
            switch (subject)
            {
                case SubjectType.Matematika:
                    bgColor = MathPillBg;
                    textColor = MathPillText;
                    break;
                case SubjectType.BahasaIndonesia:
                    bgColor = IndoPillBg;
                    textColor = IndoPillText;
                    break;
                case SubjectType.BahasaInggris:
                default:
                    bgColor = EngPillBg;
                    textColor = EngPillText;
                    break;
            }
        }

        public static string GetSubjectDisplayName(SubjectType subject)
        {
            switch (subject)
            {
                case SubjectType.Matematika:
                    return "Matematika";
                case SubjectType.BahasaIndonesia:
                    return "B. Indonesia";
                case SubjectType.BahasaInggris:
                    return "B. Inggris";
                default:
                    return subject.ToString();
            }
        }

        /// <summary>
        /// Zero-Audio Dependency: getaran (haptic) untuk mendampingi animasi BerryBears.
        /// </summary>
        public static void TriggerHapticFeedback()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
    }
}
