using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EduTunarungu.Core;

namespace EduTunarungu.UI
{
    /// <summary>
    /// ISYARAT CILIK - Template Aplikasi Edukasi Inklusif Anak Disabilitas Pendengaran (Tuna Rungu / Tuli).
    /// Berbasis Unity Android: Sepenuhnya mengutamakan Umpan Balik Visual & Taktil (Haptic Vibration).
    /// 
    /// Didesain tanpa ketergantungan aset eksternal (Zero-Dependency Procedural UI).
    /// Cukup pasang script ini ke sebuah GameObject kosong dalam scene lalu tekan Play!
    /// </summary>
    [DisallowMultipleComponent]
    public class EduTunarungu : MonoBehaviour
    {
        [Header("Pengaturan Tampilan")]
        [Tooltip("Opsional: Pasang font TTF khusus anak (jika kosong, menggunakan font bawaan Unity)")]
        public Font customFont;

        // ---------- PALET WARNA INKLUSIF & CERIA ----------
        private static Color H(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }

        // Mode Terang (Default)
        private static readonly Color ColorBgLight = H("#F4F7FB");
        private static readonly Color ColorCardLight = Color.white;
        private static readonly Color ColorTextDark = H("#1C2733");
        private static readonly Color ColorTextMuted = H("#617285");
        private static readonly Color ColorPrimary = H("#2870ED");       // Biru edukatif ramah anak
        private static readonly Color ColorSecondary = H("#FF9800");     // Oranye ceria
        private static readonly Color ColorSuccess = H("#10B981");       // Hijau capaian / benar
        private static readonly Color ColorRetry = H("#F59E0B");         // Kuning hangat coba lagi
        private static readonly Color ColorAccentPink = H("#EC4899");    // Pink vokal / artikulasi
        private static readonly Color ColorLine = H("#E2E8F0");
        private static readonly Color ColorChipBg = H("#EBF2FD");

        // Mode Kontras Tinggi (High Contrast Mode untuk anak dengan hambatan penglihatan ganda)
        private static readonly Color ColorBgDark = H("#0B0F19");
        private static readonly Color ColorCardDark = H("#182030");
        private static readonly Color ColorTextLight = H("#FFFFFF");
        private static readonly Color ColorTextDarkMuted = H("#A2B4C8");
        private static readonly Color ColorNeonYellow = H("#FFEB3B");
        private static readonly Color ColorNeonCyan = H("#00E5FF");

        // ---------- STRUKTUR DATA PEMBELAJARAN INTERNAL ----------
        public class WordEntry
        {
            public string Word;
            public string Category;
            public string[] Syllables;
            public string GestureSteps;
            public string MouthGuide;
            public Color SyllableColor;
            public string Emoji;
            public string GestureIllustrationDesc;

            public WordEntry(string word, string category, string[] syllables, string gesture, string mouth, Color col, string emoji, string gestureDesc)
            {
                Word = word;
                Category = category;
                Syllables = syllables;
                GestureSteps = gesture;
                MouthGuide = mouth;
                SyllableColor = col;
                Emoji = emoji;
                GestureIllustrationDesc = gestureDesc;
            }
        }

        private readonly List<WordEntry> vocabularies = new List<WordEntry>();
        private int currentWordIndex = 0;
        private string activeCategoryFilter = "Semua";

        // ---------- STATE AKSESIBILITAS ----------
        private bool isHighContrast = false;
        private bool isHapticEnabled = true;
        private float animationSpeedMultiplier = 1.0f; // 1.0f = normal, 0.5f = lambat untuk anak menyimak
        private bool isShowingMouthView = false;

        // ---------- REFERENSI ELEMEN UI ----------
        private Font activeFont;
        private Canvas mainCanvas;
        private RectTransform safeAreaRoot;
        private Image appBackground;
        private readonly List<GameObject> tabViews = new List<GameObject>();
        private readonly Text[] navLabels = new Text[4];
        private readonly Image[] navBars = new Image[4];

        // Elemen Tab Belajar
        private Text wordTitleText;
        private Text categoryBadgeText;
        private Text gestureDescText;
        private Text mouthGuideText;
        private Transform syllableContainer;
        private Image gestureVisualCanvas;
        private Text gestureVisualText;
        private Text gestureSubText;
        private Button toggleMouthButton;
        private Text toggleMouthButtonText;
        private Transform categoryFilterRow;

        // Elemen Tab Kuis
        private Text quizQuestionPrompt;
        private Text quizVisualPrompt;
        private Text quizCategoryText;
        private Image quizTimerFill;
        private Transform quizOptionContainer;
        private Text quizScoreText;
        private Text quizFeedbackBannerText;
        private Image quizFeedbackBannerImage;
        private int quizCurrentQuestion = 0;
        private int quizScore = 0;
        private bool quizAnswerLocked = false;
        private Coroutine quizTimerCoroutine;

        // Elemen Tab Taktil / Ritme
        private Text hapticMonitorLog;
        private RectTransform rhythmRipple;

        // Elemen Notifikasi Visual (Pengganti Bunyi Beep)
        private GameObject visualToastBar;
        private Text visualToastText;
        private Image visualToastBg;
        private int toastIdCounter = 0;

        private void Awake()
        {
            SetupVocabularies();
        }

        private void Start()
        {
            EnsureEventSystem();
            activeFont = customFont ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildCanvasAndRoot();
            BuildNotificationToast();

            // Bangun 4 Layar Utama
            BuildFlashcardTab();     // Tab 0: Belajar Isyarat & Kata
            BuildQuizTab();          // Tab 1: Kuis Tebak Isyarat
            BuildRhythmLabTab();     // Tab 2: Lab Ritme & Getaran Taktil
            BuildAccessibilityTab();  // Tab 3: Pengaturan Aksesibilitas
            BuildBottomNav();        // Navigasi Bawah

            SwitchTab(0);
            DisplayCurrentWord();
        }

        // =========================================================================
        // DATA INISIALISASI PEMBELAJARAN
        // =========================================================================
        private void SetupVocabularies()
        {
            vocabularies.Clear();
            vocabularies.Add(new WordEntry(
                "TERIMA KASIH", "Sapaan",
                new[] { "TE", "RI", "MA", "KA", "SIH" },
                "Sentuhkan jari tangan kanan ke dagu/bibir, lalu gerakkan tangan maju ke depan secara terbuka ke arah lawan bicara sambil tersenyum tulus.",
                "Bibir rileks (Te), lidah bergetar tipis (Ri), bibir mengatup lembut (Ma), lalu tersenyum di akhir (Sih).",
                ColorPrimary, "🤝", "Tangan dari Dagu -> Maju ke Depan"));

            vocabularies.Add(new WordEntry(
                "HALO / HAI", "Sapaan",
                new[] { "HA", "LO" },
                "Angkat telapak tangan terbuka di samping pelipis, lalu lambaikan santai ke kanan dan ke kiri sebanyak dua kali.",
                "Buka mulut santai (Ha), lalu bulatkan bibir penuh ke depan (Lo).",
                ColorSecondary, "👋", "Lambaikan Tangan Terbuka di Samping"));

            vocabularies.Add(new WordEntry(
                "TOLONG", "Sapaan",
                new[] { "TO", "LONG" },
                "Tangkupkan kedua telapak tangan bersamaan di depan dada (seperti memohon santun) lalu gerakkan sedikit ke bawah.",
                "Bibir membulat sedikit (To), lalu buka mulut dan posisikan lidah di langit-langit (Long).",
                ColorAccentPink, "🙏", "Dua Tangan Menangkup di Depan Dada"));

            vocabularies.Add(new WordEntry(
                "KUCING", "Hewan",
                new[] { "KU", "CING" },
                "Letakkan ibu jari dan telunjuk di dekat kedua pipi, lalu tarik ke samping luar membentuk gerakan mengelus kumis kucing.",
                "Bibir mengerucut membulat kecil (Ku), kemudian tersenyum lebar dengan gigi tampak merapat (Cing).",
                ColorSecondary, "🐱", "Tarik Jari dari Pipi ke Luar (Kumis)"));

            vocabularies.Add(new WordEntry(
                "IKAN", "Hewan",
                new[] { "I", "KAN" },
                "Rapatkan jemari satu tangan menghadap samping, lalu gerakkan meliuk maju ke depan seperti ikan yang sedang berenang.",
                "Tersenyum pipih melebar (I), lalu buka mulut tegas dengan lidah menyentuh bagian belakang (Kan).",
                ColorPrimary, "🐟", "Tangan Meliuk Berenang ke Depan"));

            vocabularies.Add(new WordEntry(
                "BURUNG", "Hewan",
                new[] { "BU", "RUNG" },
                "Gunakan telunjuk dan ibu jari di depan bibir untuk membuka dan menutup seperti paruh burung, lalu rentangkan kedua tangan seperti sayap.",
                "Bibir menutup lalu membuka membulat (Bu), lalu bibir sedikit maju (Rung).",
                ColorSuccess, "🐦", "Buka-Tutup Paruh di Depan Bibir"));

            vocabularies.Add(new WordEntry(
                "APEL", "Buah",
                new[] { "A", "PEL" },
                "Bentuk jari seperti menggenggam buah bundar di dekat pipi, lalu putar pergelangan tangan sedikit dua kali.",
                "Buka mulut lebar ke bawah (A), lalu katupkan bibir atas dan bibir bawah dengan rapat (Pel).",
                H("#EF4444"), "🍎", "Putar Genggaman Buah di Dekat Pipi"));

            vocabularies.Add(new WordEntry(
                "SENANG", "Emosi",
                new[] { "SE", "NANG" },
                "Buka kedua telapak tangan menghadap dada, lalu gerakkan mengusap ke atas berulang kali sambil menunjukkan ekspresi tersenyum riang.",
                "Bibir rileks terbuka tipis (Se), lalu mulut terbuka riang lebar (Nang).",
                ColorSuccess, "😄", "Usap Dada ke Atas dengan Senyuman"));
        }

        // =========================================================================
        // DASAR UI & PROCEDURAL GRAPHICS
        // =========================================================================
        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        private void BuildCanvasAndRoot()
        {
            var canvasObj = new GameObject("EduTunarunguCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            mainCanvas = canvasObj.GetComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            // Background Penuh
            appBackground = CreateImage(canvasObj.transform, isHighContrast ? ColorBgDark : ColorBgLight);
            Stretch(appBackground.rectTransform);

            // Safe Area Layar Android (Notch & Punch Hole)
            var safeObj = new GameObject("SafeAreaRoot", typeof(RectTransform));
            safeObj.transform.SetParent(canvasObj.transform, false);
            safeAreaRoot = (RectTransform)safeObj.transform;
            Stretch(safeAreaRoot);

            var sa = Screen.safeArea;
            var sz = new Vector2(Screen.width, Screen.height);
            if (Screen.width > 0 && Screen.height > 0)
            {
                safeAreaRoot.anchorMin = sa.position / sz;
                safeAreaRoot.anchorMax = (sa.position + sa.size) / sz;
            }
        }

        private static Sprite cachedRoundedRect;
        private static Sprite GetRoundedSprite(int size = 128, int radius = 36)
        {
            if (cachedRoundedRect != null) return cachedRoundedRect;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(radius - (x + 0.5f), (x + 0.5f) - (size - radius), 0);
                    float dy = Mathf.Max(radius - (y + 0.5f), (y + 0.5f) - (size - radius), 0);
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(0.5f - (dist - radius));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            cachedRoundedRect = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            return cachedRoundedRect;
        }

        private static RectTransform CreateRectTransform(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform r, float left = 0, float bottom = 0, float right = 0, float top = 0)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(left, bottom);
            r.offsetMax = new Vector2(-right, -top);
        }

        private Image CreateImage(Transform parent, Color color, float cornerRadius = 0)
        {
            var r = CreateRectTransform("Image", parent);
            var img = r.gameObject.AddComponent<Image>();
            img.color = color;
            if (cornerRadius > 0)
            {
                img.sprite = GetRoundedSprite();
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 36f / cornerRadius;
            }
            return img;
        }

        private Text CreateText(Transform parent, string content, int size, Color color, FontStyle style = FontStyle.Normal, TextAnchor align = TextAnchor.UpperLeft)
        {
            var r = CreateRectTransform("Text", parent);
            var txt = r.gameObject.AddComponent<Text>();
            txt.font = activeFont;
            txt.text = content;
            txt.fontSize = size;
            txt.color = color;
            txt.fontStyle = style;
            txt.alignment = align;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;
            txt.lineSpacing = 1.15f;
            txt.raycastTarget = false;
            return txt;
        }

        private Button CreateButton(Transform parent, string label, Color bg, Color textCol, Action onClick, int fontSize = 38, float height = 110, float radius = 28)
        {
            var img = CreateImage(parent, bg, radius);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() =>
            {
                TriggerTapHaptic();
                onClick?.Invoke();
            });

            var labelTxt = CreateText(img.transform, label, fontSize, textCol, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(labelTxt.rectTransform);

            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = height;
            return btn;
        }

        private RectTransform CreateTabPanel()
        {
            var tab = CreateRectTransform("TabPanel", safeAreaRoot);
            Stretch(tab, 0, 160, 0, 0); // Sisakan ruang 160px untuk navigasi bawah
            tabViews.Add(tab.gameObject);
            return tab;
        }

        private static VerticalLayoutGroup AddVerticalLayout(RectTransform r, float spacing, int padding)
        {
            var v = r.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(padding, padding, padding, padding);
            v.childControlWidth = true;
            v.childControlHeight = false;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        private static HorizontalLayoutGroup AddHorizontalLayout(RectTransform r, float spacing, int padding, bool expand = false)
        {
            var h = r.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = new RectOffset(padding, padding, padding, padding);
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = expand;
            h.childForceExpandHeight = expand;
            return h;
        }

        // =========================================================================
        // BANNER NOTIFIKASI VISUAL (Pengganti Bunyi untuk Anak Tuli)
        // =========================================================================
        private void BuildNotificationToast()
        {
            var container = CreateImage(safeAreaRoot, ColorPrimary, 30);
            visualToastBar = container.gameObject;
            visualToastBg = container;

            var r = container.rectTransform;
            r.anchorMin = new Vector2(0.05f, 1f);
            r.anchorMax = new Vector2(0.95f, 1f);
            r.pivot = new Vector2(0.5f, 1f);
            r.sizeDelta = new Vector2(0, 120);
            r.anchoredPosition = new Vector2(0, -20);

            visualToastText = CreateText(container.transform, "", 38, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(visualToastText.rectTransform, 20, 10, 20, 10);
            visualToastBar.SetActive(false);
        }

        public void ShowVisualToast(string message, Color color, float duration = 2.5f)
        {
            visualToastText.text = message;
            visualToastBg.color = color;
            visualToastBar.SetActive(true);
            int currentId = ++toastIdCounter;
            StartCoroutine(HideToastRoutine(currentId, duration));
        }

        private IEnumerator HideToastRoutine(int id, float duration)
        {
            yield return new WaitForSeconds(duration);
            if (id == toastIdCounter && visualToastBar != null)
            {
                visualToastBar.SetActive(false);
            }
        }

        // =========================================================================
        // TAB 0: MODUL BELAJAR ISYARAT & ARTIKULASI (Visual Flashcard)
        // =========================================================================
        private void BuildFlashcardTab()
        {
            var tab = CreateTabPanel();
            AddVerticalLayout(tab, 20, 36);

            // Header Aplikasi
            var headerBox = CreateRectTransform("Header", tab);
            var headerH = AddHorizontalLayout(headerBox, 16, 0);
            var titleTxt = CreateText(headerBox, "✨ Isyarat Cilik", 50, isHighContrast ? ColorNeonCyan : ColorPrimary, FontStyle.Bold);
            titleTxt.rectTransform.sizeDelta = new Vector2(600, 70);

            var infoTxt = CreateText(tab, "Pembelajaran Isyarat BISINDO & Bentuk Artikulasi Mulut", 32, isHighContrast ? ColorTextDarkMuted : ColorTextMuted);

            // Filter Kategori (Chips)
            var catScroll = CreateRectTransform("CatScroll", tab);
            catScroll.sizeDelta = new Vector2(0, 80);
            var catH = AddHorizontalLayout(catScroll, 16, 0);
            categoryFilterRow = catScroll;

            string[] categories = { "Semua", "Sapaan", "Hewan", "Buah", "Emosi" };
            foreach (var cat in categories)
            {
                string cName = cat;
                var chip = CreateButton(catScroll, cName, isHighContrast ? ColorCardDark : ColorChipBg,
                    isHighContrast ? ColorNeonYellow : ColorPrimary, () =>
                    {
                        activeCategoryFilter = cName;
                        FilterVocabularies(cName);
                    }, 30, 70, 35);
                chip.GetComponent<LayoutElement>().preferredWidth = 180;
            }

            // KARTU UTAMA PEMBELAJARAN
            var card = CreateImage(tab, isHighContrast ? ColorCardDark : ColorCardLight, 36);
            card.rectTransform.sizeDelta = new Vector2(0, 1050);
            var cardLayout = AddVerticalLayout(card.rectTransform, 18, 40);

            // Badge Kategori & Indikator Halaman
            var topMetaRow = CreateRectTransform("MetaRow", card.transform);
            topMetaRow.sizeDelta = new Vector2(0, 50);
            var metaH = AddHorizontalLayout(topMetaRow, 10, 0);
            categoryBadgeText = CreateText(topMetaRow, "KATEGORI: SAPAAN", 28, ColorSecondary, FontStyle.Bold, TextAnchor.MiddleLeft);

            // Kata Utama (Besar & Kontras)
            wordTitleText = CreateText(card.transform, "TERIMA KASIH", 58, isHighContrast ? ColorNeonYellow : ColorTextDark, FontStyle.Bold, TextAnchor.MiddleCenter);

            // Wadah Tombol Interaktif Suku Kata
            var sylHeader = CreateText(card.transform, "Ketuk suku kata untuk merasakan getaran ritme & artikulasi:", 28, isHighContrast ? ColorTextDarkMuted : ColorTextMuted, FontStyle.Italic, TextAnchor.MiddleCenter);
            var sylBox = CreateRectTransform("SyllableContainer", card.transform);
            sylBox.sizeDelta = new Vector2(0, 100);
            AddHorizontalLayout(sylBox, 16, 0);
            syllableContainer = sylBox;

            // Area Visual Utama (Toggle Isyarat Tangan vs Bentuk Mulut)
            gestureVisualCanvas = CreateImage(card.transform, isHighContrast ? H("#243044") : H("#E6F0FF"), 30);
            gestureVisualCanvas.rectTransform.sizeDelta = new Vector2(0, 420);
            var visualLayout = AddVerticalLayout(gestureVisualCanvas.rectTransform, 10, 24);

            gestureVisualText = CreateText(gestureVisualCanvas.transform, "🤝", 120, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);
            gestureSubText = CreateText(gestureVisualCanvas.transform, "Gerakan Tangan Maju ke Depan", 36, isHighContrast ? ColorTextLight : ColorPrimary, FontStyle.Bold, TextAnchor.MiddleCenter);

            // Panduan Langkah Gerakan / Artikulasi
            gestureDescText = CreateText(card.transform, "Deskripsi Gerakan...", 32, isHighContrast ? ColorTextLight : ColorTextDark, FontStyle.Normal, TextAnchor.MiddleLeft);
            gestureDescText.rectTransform.sizeDelta = new Vector2(0, 110);

            mouthGuideText = CreateText(card.transform, "Panduan Mulut...", 30, ColorAccentPink, FontStyle.Bold, TextAnchor.MiddleLeft);
            mouthGuideText.rectTransform.sizeDelta = new Vector2(0, 80);

            // Tombol Toggle Tampilan (Isyarat / Bentuk Mulut)
            toggleMouthButton = CreateButton(card.transform, "👄 Lihat Bentuk Mulut (Lip Reading)", isHighContrast ? H("#3E2448") : H("#FCE7F3"), ColorAccentPink, () =>
            {
                isShowingMouthView = !isShowingMouthView;
                UpdateVisualDisplayMode();
            }, 32, 90, 25);

            // Bar Navigasi Kata (Sebelumnya - Putar Animasi - Selanjutnya)
            var navRow = CreateRectTransform("WordNavRow", tab);
            navRow.sizeDelta = new Vector2(0, 120);
            AddHorizontalLayout(navRow, 20, 0, true);

            CreateButton(navRow, "◀ Sebelumnya", isHighContrast ? ColorCardDark : ColorCardLight, isHighContrast ? ColorNeonCyan : ColorPrimary, () =>
            {
                NavigateWord(-1);
            }, 34, 110, 30);

            CreateButton(navRow, "▶ Peragakan", ColorSuccess, Color.white, () =>
            {
                StartCoroutine(PlayGestureAnimationRoutine());
            }, 36, 110, 30);

            CreateButton(navRow, "Selanjutnya ▶", isHighContrast ? ColorCardDark : ColorCardLight, isHighContrast ? ColorNeonCyan : ColorPrimary, () =>
            {
                NavigateWord(1);
            }, 34, 110, 30);
        }

        private void FilterVocabularies(string cat)
        {
            if (cat == "Semua")
            {
                currentWordIndex = 0;
            }
            else
            {
                int idx = vocabularies.FindIndex(v => v.Category.Equals(cat, StringComparison.OrdinalIgnoreCase));
                if (idx >= 0) currentWordIndex = idx;
            }
            DisplayCurrentWord();
            ShowVisualToast("Kategori: " + cat, ColorPrimary, 1.5f);
        }

        private void NavigateWord(int delta)
        {
            currentWordIndex = (currentWordIndex + delta + vocabularies.Count) % vocabularies.Count;
            DisplayCurrentWord();
        }

        private void DisplayCurrentWord()
        {
            if (vocabularies.Count == 0) return;
            var item = vocabularies[currentWordIndex];

            wordTitleText.text = item.Word;
            categoryBadgeText.text = "KATEGORI: " + item.Category.ToUpper();
            gestureDescText.text = "🖐️ <b>Gerakan Isyarat:</b> " + item.GestureSteps;
            mouthGuideText.text = "👄 <b>Bentuk Bibir:</b> " + item.MouthGuide;

            UpdateVisualDisplayMode();

            // Refresh Balok Suku Kata
            foreach (Transform child in syllableContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < item.Syllables.Length; i++)
            {
                string syl = item.Syllables[i];
                int sylIndex = i;
                Color chipColor = (i % 2 == 0) ? ColorPrimary : ColorSecondary;

                var sylBtn = CreateButton(syllableContainer, syl, chipColor, Color.white, () =>
                {
                    OnSyllableTapped(syl, sylIndex);
                }, 38, 90, 24);

                sylBtn.GetComponent<LayoutElement>().preferredWidth = 150;
            }
        }

        private void UpdateVisualDisplayMode()
        {
            var item = vocabularies[currentWordIndex];
            if (!isShowingMouthView)
            {
                gestureVisualText.text = item.Emoji;
                gestureSubText.text = item.GestureIllustrationDesc;
                toggleMouthButtonText = toggleMouthButton.GetComponentInChildren<Text>();
                if (toggleMouthButtonText != null) toggleMouthButtonText.text = "👄 Lihat Bentuk Mulut (Lip Reading)";
            }
            else
            {
                gestureVisualText.text = "👄";
                gestureSubText.text = "Artikulasi: " + item.MouthGuide;
                toggleMouthButtonText = toggleMouthButton.GetComponentInChildren<Text>();
                if (toggleMouthButtonText != null) toggleMouthButtonText.text = "🖐️ Kembali ke Bahasa Isyarat";
            }
        }

        private void OnSyllableTapped(string syllable, int index)
        {
            // Memicu getaran haptic berirama di Android
            AndroidHapticHelper.MediumPulse();

            // Umpan balik visual suku kata aktif
            ShowVisualToast("Suku Kata: [ " + syllable + " ] - Rasakan denyut artikulasi!", ColorPrimary, 1.2f);
        }

        private IEnumerator PlayGestureAnimationRoutine()
        {
            var item = vocabularies[currentWordIndex];
            ShowVisualToast("Memeragakan: " + item.Word, ColorSuccess, 2.5f);

            // Simulasi animasi visual gerakan tangan dalam 2 tahap
            gestureVisualCanvas.color = ColorChipBg;
            AndroidHapticHelper.LightTap();
            gestureSubText.text = "Langkah 1: Siapkan posisi tangan...";
            yield return new WaitForSeconds(0.8f * animationSpeedMultiplier);

            gestureVisualCanvas.color = H("#FEF3C7");
            AndroidHapticHelper.MediumPulse();
            gestureSubText.text = "Langkah 2: " + item.GestureIllustrationDesc;
            yield return new WaitForSeconds(1.2f * animationSpeedMultiplier);

            gestureVisualCanvas.color = isHighContrast ? H("#243044") : H("#E6F0FF");
            gestureSubText.text = item.GestureIllustrationDesc;
            AndroidHapticHelper.SuccessPattern();
        }

        // =========================================================================
        // TAB 1: MODUL KUIS TEBAK ISYARAT (Gamifikasi Visual & Haptik)
        // =========================================================================
        private void BuildQuizTab()
        {
            var tab = CreateTabPanel();
            AddVerticalLayout(tab, 20, 36);

            // Bar Skor & Status
            var scoreRow = CreateRectTransform("ScoreRow", tab);
            scoreRow.sizeDelta = new Vector2(0, 80);
            AddHorizontalLayout(scoreRow, 16, 0);

            var quizTitle = CreateText(scoreRow, "🎯 Kuis Tebak Isyarat", 44, isHighContrast ? ColorNeonCyan : ColorPrimary, FontStyle.Bold);
            quizTitle.rectTransform.sizeDelta = new Vector2(550, 70);

            quizScoreText = CreateText(scoreRow, "⭐ 0 Bintang", 38, ColorSecondary, FontStyle.Bold, TextAnchor.MiddleRight);
            quizScoreText.rectTransform.sizeDelta = new Vector2(400, 70);

            // Bar Timer Visual (Menyusut tenang tanpa bunyi detak yang membuat panik)
            var timerBg = CreateImage(tab, ColorLine, 12);
            timerBg.rectTransform.sizeDelta = new Vector2(0, 24);
            quizTimerFill = CreateImage(timerBg.transform, ColorSuccess, 12);
            Stretch(quizTimerFill.rectTransform);

            // Kartu Pertanyaan Visual
            var questionCard = CreateImage(tab, isHighContrast ? ColorCardDark : ColorCardLight, 36);
            questionCard.rectTransform.sizeDelta = new Vector2(0, 680);
            var qLayout = AddVerticalLayout(questionCard.rectTransform, 16, 36);

            quizCategoryText = CreateText(questionCard.transform, "SOAL 1 DARI 5", 28, ColorSecondary, FontStyle.Bold, TextAnchor.MiddleCenter);
            quizQuestionPrompt = CreateText(questionCard.transform, "Manakah kata yang sesuai dengan gerakan isyarat ini?", 36, isHighContrast ? ColorTextLight : ColorTextDark, FontStyle.Bold, TextAnchor.MiddleCenter);

            // Display Isyarat Soal
            var promptVisualBox = CreateImage(questionCard.transform, isHighContrast ? H("#243044") : H("#EFF6FF"), 28);
            promptVisualBox.rectTransform.sizeDelta = new Vector2(0, 320);
            var pvLayout = AddVerticalLayout(promptVisualBox.rectTransform, 8, 20);

            quizVisualPrompt = CreateText(promptVisualBox.transform, "🐱", 130, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);
            var hintSub = CreateText(promptVisualBox.transform, "Petunjuk: Gerakan mengusap kumis di kedua pipi", 30, isHighContrast ? ColorTextDarkMuted : ColorTextMuted, FontStyle.Italic, TextAnchor.MiddleCenter);

            // Banner Evaluasi Hasil Jawaban
            var banner = CreateImage(tab, ColorSuccess, 24);
            banner.rectTransform.sizeDelta = new Vector2(0, 100);
            quizFeedbackBannerImage = banner;
            quizFeedbackBannerText = CreateText(banner.transform, "Pilih salah satu jawaban di bawah!", 34, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(quizFeedbackBannerText.rectTransform);
            quizFeedbackBannerImage.gameObject.SetActive(false);

            // Wadah Pilihan Jawaban
            var optBox = CreateRectTransform("OptionBox", tab);
            optBox.sizeDelta = new Vector2(0, 480);
            var optLayout = AddVerticalLayout(optBox, 16, 0);
            quizOptionContainer = optBox;

            LoadCurrentQuizQuestion();
        }

        private void LoadCurrentQuizQuestion()
        {
            quizAnswerLocked = false;
            quizFeedbackBannerImage.gameObject.SetActive(false);

            int total = vocabularies.Count;
            var target = vocabularies[quizCurrentQuestion % total];

            quizCategoryText.text = $"PERTANYAAN {(quizCurrentQuestion % 5) + 1} DARI 5";
            quizVisualPrompt.text = target.Emoji;
            quizQuestionPrompt.text = "Manakah kata yang tepat untuk gerakan isyarat:\n<i>\"" + target.GestureIllustrationDesc + "\"</i> ?";

            // Buat 4 pilihan jawaban acak
            var options = new List<WordEntry> { target };
            var pool = new List<WordEntry>(vocabularies);
            pool.Remove(target);

            while (options.Count < 4 && pool.Count > 0)
            {
                int rIdx = UnityEngine.Random.Range(0, pool.Count);
                options.Add(pool[rIdx]);
                pool.RemoveAt(rIdx);
            }

            // Acak urutan pilihan
            for (int i = 0; i < options.Count; i++)
            {
                var temp = options[i];
                int rand = UnityEngine.Random.Range(i, options.Count);
                options[i] = options[rand];
                options[rand] = temp;
            }

            // Refresh UI Tombol Pilihan
            foreach (Transform ch in quizOptionContainer) Destroy(ch.gameObject);

            foreach (var opt in options)
            {
                var captured = opt;
                bool isCorrect = (captured == target);

                var btn = CreateButton(quizOptionContainer, $"{captured.Emoji}  {captured.Word}",
                    isHighContrast ? ColorCardDark : ColorCardLight,
                    isHighContrast ? ColorNeonYellow : ColorTextDark, () =>
                    {
                        OnQuizOptionSelected(isCorrect, captured.Word, target.Word);
                    }, 36, 110, 26);
            }

            if (quizTimerCoroutine != null) StopCoroutine(quizTimerCoroutine);
            quizTimerCoroutine = StartCoroutine(QuizTimerRoutine(18f));
        }

        private IEnumerator QuizTimerRoutine(float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = 1f - (elapsed / duration);
                quizTimerFill.rectTransform.anchorMax = new Vector2(progress, 1f);

                if (progress > 0.5f) quizTimerFill.color = ColorSuccess;
                else if (progress > 0.25f) quizTimerFill.color = ColorRetry;
                else quizTimerFill.color = H("#EF4444");

                yield return null;
            }

            if (!quizAnswerLocked)
            {
                OnQuizOptionSelected(false, "Waktu Habis", vocabularies[quizCurrentQuestion % vocabularies.Count].Word);
            }
        }

        private void OnQuizOptionSelected(bool isCorrect, string chosen, string correctWord)
        {
            if (quizAnswerLocked) return;
            quizAnswerLocked = true;
            if (quizTimerCoroutine != null) StopCoroutine(quizTimerCoroutine);

            quizFeedbackBannerImage.gameObject.SetActive(true);

            if (isCorrect)
            {
                quizScore += 10;
                quizScoreText.text = $"⭐ {quizScore} Bintang";
                quizFeedbackBannerImage.color = ColorSuccess;
                quizFeedbackBannerText.text = $"🎉 HEBAT! Jawaban Benar: {correctWord} ⭐";

                // Getaran Android Sukses Ceria
                AndroidHapticHelper.SuccessPattern();
            }
            else
            {
                quizFeedbackBannerImage.color = ColorRetry;
                quizFeedbackBannerText.text = $"Yuk coba lagi! Jawaban yang tepat: {correctWord}";

                // Getaran Android Lembut
                AndroidHapticHelper.RetryPattern();
            }

            StartCoroutine(NextQuizQuestionDelay(2.5f));
        }

        private IEnumerator NextQuizQuestionDelay(float sec)
        {
            yield return new WaitForSeconds(sec);
            quizCurrentQuestion++;
            LoadCurrentQuizQuestion();
        }

        // =========================================================================
        // TAB 2: LAB RITME & GETARAN TAKTIL (Haptic Vibration Laboratory)
        // =========================================================================
        private void BuildRhythmLabTab()
        {
            var tab = CreateTabPanel();
            AddVerticalLayout(tab, 24, 36);

            var titleTxt = CreateText(tab, "🥁 Lab Ritme & Getaran Taktil", 48, isHighContrast ? ColorNeonCyan : ColorPrimary, FontStyle.Bold);
            var descTxt = CreateText(tab, "Latihan merasakan irama suku kata & nada melalui sensasi getaran di ponsel Android.", 32, isHighContrast ? ColorTextDarkMuted : ColorTextMuted);

            // Layar Monitor Umpan Balik Haptik
            var monitorBox = CreateImage(tab, isHighContrast ? ColorCardDark : H("#1E293B"), 28);
            monitorBox.rectTransform.sizeDelta = new Vector2(0, 260);
            var mLayout = AddVerticalLayout(monitorBox.rectTransform, 10, 24);

            CreateText(monitorBox.transform, "HAPTIC FEEDBACK MONITOR (Android Vibrator)", 26, ColorNeonCyan, FontStyle.Bold);
            hapticMonitorLog = CreateText(monitorBox.transform, "Status: Siap menguji pola getaran taktil.\nKetuk salah satu pad di bawah ini untuk merasakan getaran di tangan Anda.", 30, Color.white);

            // Pad Ritme Interaktif
            var padGrid = CreateRectTransform("PadGrid", tab);
            padGrid.sizeDelta = new Vector2(0, 850);
            AddVerticalLayout(padGrid, 20, 0);

            CreateRhythmButton(padGrid, "1️⃣ Ketukan Tunggal (1 Suku Kata)", "Contoh: AIR, CAT, BAN (Durasi: 80ms)", ColorPrimary, () =>
            {
                AndroidHapticHelper.LightTap();
                UpdateHapticLog("Pola: Ketukan Tunggal | 80 milidetik | Amplitudo Ringan");
            });

            CreateRhythmButton(padGrid, "2️⃣ Ketukan Ganda (2 Suku Kata)", "Contoh: KU - CING, A - PEL (Durasi: 60ms - jeda - 80ms)", ColorSecondary, () =>
            {
                AndroidHapticHelper.MediumPulse();
                UpdateHapticLog("Pola: Suku Kata Ganda | 60ms, Jeda 50ms, 80ms | Denyut Berirama");
            });

            CreateRhythmButton(padGrid, "3️⃣ Ketukan Tiga (3 Suku Kata)", "Contoh: TE - RI - MA, SE - PE - DA (3 Pulsa Getar)", ColorAccentPink, () =>
            {
                AndroidHapticHelper.CustomRhythm(new long[] { 0, 50, 40, 50, 40, 90 }, new int[] { 0, 120, 0, 120, 0, 200 });
                UpdateHapticLog("Pola: Tiga Suku Kata | Pola 3 Ketukan Beruntun");
            });

            CreateRhythmButton(padGrid, "⭐ Pola Ceria (Perayaan / Reward)", "Getaran Melompat Tanda Berhasil (Sukses Kuis)", ColorSuccess, () =>
            {
                AndroidHapticHelper.SuccessPattern();
                UpdateHapticLog("Pola: Kemenangan | 70ms, Jeda 60ms, 140ms Ceria");
            });
        }

        private void CreateRhythmButton(Transform parent, string title, string subtitle, Color color, Action onClick)
        {
            var card = CreateImage(parent, isHighContrast ? ColorCardDark : ColorCardLight, 30);
            card.rectTransform.sizeDelta = new Vector2(0, 180);
            var h = AddHorizontalLayout(card.rectTransform, 20, 24);

            var iconBox = CreateImage(card.transform, color, 24);
            iconBox.rectTransform.sizeDelta = new Vector2(120, 120);
            var icTxt = CreateText(iconBox.transform, "📳", 50, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);
            Stretch(icTxt.rectTransform);

            var textBox = CreateRectTransform("TextHolder", card.transform);
            textBox.sizeDelta = new Vector2(650, 120);
            var v = AddVerticalLayout(textBox, 6, 0);

            CreateText(textBox, title, 36, isHighContrast ? ColorNeonYellow : ColorTextDark, FontStyle.Bold);
            CreateText(textBox, subtitle, 28, isHighContrast ? ColorTextDarkMuted : ColorTextMuted);

            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = card;
            btn.onClick.AddListener(() =>
            {
                onClick?.Invoke();
                ShowVisualToast("Memancarkan Getaran: " + title, color, 1.2f);
            });
        }

        private void UpdateHapticLog(string log)
        {
            if (hapticMonitorLog != null)
            {
                hapticMonitorLog.text = $"<b>[GETAR AKTIF]</b>\n{log}\n<i>Getaran aktif pada perangkat fisik Android Anda.</i>";
            }
        }

        // =========================================================================
        // TAB 3: RUANG AKSESIBILITAS (Inklusi & Personalisasi)
        // =========================================================================
        private void BuildAccessibilityTab()
        {
            var tab = CreateTabPanel();
            AddVerticalLayout(tab, 24, 36);

            CreateText(tab, "⚙️ Pengaturan Aksesibilitas", 48, isHighContrast ? ColorNeonCyan : ColorPrimary, FontStyle.Bold);
            CreateText(tab, "Sesuaikan pengalaman belajar sesuai kebutuhan spesifik anak.", 32, isHighContrast ? ColorTextDarkMuted : ColorTextMuted);

            // Toggle Haptic Getaran
            CreateSettingToggle(tab, "Umpan Balik Getaran (Haptic)",
                "Ponsel bergetar saat menyentuh huruf, suku kata, atau menjawab kuis.", isHapticEnabled, (val) =>
                {
                    isHapticEnabled = val;
                    AndroidHapticHelper.IsHapticEnabled = val;
                    if (val) AndroidHapticHelper.SuccessPattern();
                    ShowVisualToast(val ? "Getaran Diaktifkan" : "Getaran Dinonaktifkan", ColorPrimary);
                });

            // Toggle Kontras Tinggi
            CreateSettingToggle(tab, "Mode Kontras Tinggi",
                "Tampilan latar belakang gelap dengan teks kuning cerah untuk mempermudah daya lihat anak.", isHighContrast, (val) =>
                {
                    isHighContrast = val;
                    ApplyContrastTheme();
                    ShowVisualToast(val ? "Mode Kontras Tinggi Aktif" : "Mode Standar Aktif", ColorSecondary);
                });

            // Toggle Kecepatan Gerakan Isyarat
            CreateSettingToggle(tab, "Perlambat Gerakan Isyarat (0.5x)",
                "Membuat animasi isyarat tangan lebih lambat agar mudah dipelajari langkah demi langkah.", (animationSpeedMultiplier > 1.2f), (val) =>
                {
                    animationSpeedMultiplier = val ? 1.8f : 1.0f;
                    ShowVisualToast(val ? "Animasi Diperlambat" : "Kecepatan Animasi Normal", ColorSuccess);
                });

            // Informasi Edukasi Inklusif
            var infoBox = CreateImage(tab, isHighContrast ? ColorCardDark : ColorCardLight, 32);
            infoBox.rectTransform.sizeDelta = new Vector2(0, 320);
            var ibLayout = AddVerticalLayout(infoBox.rectTransform, 12, 32);

            CreateText(infoBox.transform, "💡 Mengapa Umpan Balik Visual & Taktil?", 34, ColorSecondary, FontStyle.Bold);
            CreateText(infoBox.transform,
                "Anak disabilitas pendengaran memproses informasi secara maksimal melalui visual referent (gambar), gestur motorik (bahasa isyarat), dan getaran haptik (pengganti suara). " +
                "Aplikasi ini dibuat untuk mendukung kesetaraan akses pendidikan inklusif bagi Teman Tuli.", 30,
                isHighContrast ? ColorTextLight : ColorTextMuted);
        }

        private void CreateSettingToggle(Transform parent, string title, string description, bool initialVal, Action<bool> onToggle)
        {
            var card = CreateImage(parent, isHighContrast ? ColorCardDark : ColorCardLight, 28);
            card.rectTransform.sizeDelta = new Vector2(0, 180);
            var h = AddHorizontalLayout(card.rectTransform, 20, 28);

            var textCol = CreateRectTransform("Texts", card.transform);
            textCol.sizeDelta = new Vector2(680, 140);
            var v = AddVerticalLayout(textCol, 6, 0);

            CreateText(textCol, title, 36, isHighContrast ? ColorNeonYellow : ColorTextDark, FontStyle.Bold);
            CreateText(textCol, description, 28, isHighContrast ? ColorTextDarkMuted : ColorTextMuted);

            bool state = initialVal;
            var toggleBtn = CreateButton(card.transform, state ? "AKTIF" : "MATI", state ? ColorSuccess : ColorLine,
                state ? Color.white : ColorTextMuted, null, 30, 90, 45);
            toggleBtn.GetComponent<LayoutElement>().preferredWidth = 180;

            toggleBtn.onClick.AddListener(() =>
            {
                state = !state;
                toggleBtn.GetComponent<Image>().color = state ? ColorSuccess : ColorLine;
                var txt = toggleBtn.GetComponentInChildren<Text>();
                txt.text = state ? "AKTIF" : "MATI";
                txt.color = state ? Color.white : ColorTextMuted;
                onToggle?.Invoke(state);
            });
        }

        private void ApplyContrastTheme()
        {
            appBackground.color = isHighContrast ? ColorBgDark : ColorBgLight;
            DisplayCurrentWord();
        }

        // =========================================================================
        // NAVIGASI BAWAH (Bottom Bar)
        // =========================================================================
        private void BuildBottomNav()
        {
            var navBarContainer = CreateImage(safeAreaRoot, isHighContrast ? ColorCardDark : Color.white);
            var r = navBarContainer.rectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = new Vector2(1f, 0);
            r.pivot = new Vector2(0.5f, 0);
            r.sizeDelta = new Vector2(0, 160);

            // Garis Pembatas
            var line = CreateImage(navBarContainer.transform, isHighContrast ? H("#2D3748") : ColorLine);
            line.rectTransform.anchorMin = new Vector2(0, 1);
            line.rectTransform.anchorMax = Vector2.one;
            line.rectTransform.pivot = new Vector2(0.5f, 1);
            line.rectTransform.sizeDelta = new Vector2(0, 2);

            var row = CreateRectTransform("NavRow", navBarContainer.transform);
            Stretch(row);
            AddHorizontalLayout(row, 0, 0, true);

            string[] tabNames = { "Belajar", "Kuis", "Ritme Taktil", "Aksesibilitas" };
            string[] icons = { "📖", "🎯", "🥁", "⚙️" };

            for (int i = 0; i < 4; i++)
            {
                int tabIdx = i;
                var btnImg = CreateImage(row, Color.clear);
                var btn = btnImg.gameObject.AddComponent<Button>();
                btn.targetGraphic = btnImg;
                btn.onClick.AddListener(() => SwitchTab(tabIdx));

                var label = CreateText(btnImg.transform, $"{icons[i]}\n{tabNames[i]}", 28, ColorTextMuted, FontStyle.Bold, TextAnchor.MiddleCenter);
                Stretch(label.rectTransform, 0, 10, 0, 15);
                navLabels[i] = label;

                // Indikator Garis Aktif
                var indicator = CreateImage(btnImg.transform, ColorPrimary, 4);
                var ir = indicator.rectTransform;
                ir.anchorMin = new Vector2(0.2f, 1);
                ir.anchorMax = new Vector2(0.8f, 1);
                ir.pivot = new Vector2(0.5f, 1);
                ir.sizeDelta = new Vector2(0, 8);
                navBars[i] = indicator;
            }
        }

        public void SwitchTab(int index)
        {
            for (int i = 0; i < tabViews.Count; i++)
            {
                bool active = (i == index);
                tabViews[i].SetActive(active);

                if (navLabels[i] != null)
                {
                    navLabels[i].color = active ? (isHighContrast ? ColorNeonCyan : ColorPrimary) : (isHighContrast ? ColorTextDarkMuted : ColorTextMuted);
                }
                if (navBars[i] != null)
                {
                    navBars[i].enabled = active;
                    navBars[i].color = isHighContrast ? ColorNeonCyan : ColorPrimary;
                }
            }

            AndroidHapticHelper.LightTap();
        }

        private void TriggerTapHaptic()
        {
            if (isHapticEnabled)
            {
                AndroidHapticHelper.LightTap();
            }
        }
    }
}
