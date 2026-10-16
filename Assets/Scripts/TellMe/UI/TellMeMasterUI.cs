using System;
using System.Collections;
using System.Collections.Generic;
using TellMe.Core;
using TellMe.Mascot;
using TellMe.Modules;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TellMe.UI
{
    /// <summary>
    /// Master UI Orchestrator & Dynamic Runtime Canvas Builder untuk TellMe (DingBuzzer - JOINTS UGM 2026).
    /// Versi Revamp Visual & Layout:
    /// - Procedural 9-Slice Rounded Rectangles, Tactile 3D Buttons, Smooth Circles, dan Soft Glow Blobs.
    /// - Menghilangkan 100% tampilan kotak mentah/kaku (sprite == null).
    /// - Viewport adaptif tanpa dead whitespace: meregang proporsional di seluruh rasio layar mobile.
    /// - BerryBears Companion Avatar dengan mata berkedip organik, lirik 8 arah, highlight glossy, dan pipi merona lembut.
    /// - Layar 0 (Role Selection) seimbang, taktil, dan vertikal penuh (Mode Orang Tua, Mode Siswa, dan Pengingat Cepat).
    /// - 9 Layar/Modul Lengkap berfungsi 100% offline.
    /// </summary>
    public class TellMeMasterUI : MonoBehaviour
    {
        public static TellMeMasterUI Instance { get; private set; }

        [Header("Global Settings")]
        [SerializeField] private bool autoBuildOnStart = true;

        // References to Controllers
        private LocalDataManager dataManager;
        private AppNavigationManager navManager;
        private BerryBearsController mascotController;
        private RoleSelectionController roleController;
        private ScheduleModuleController scheduleController;
        private SpeechToTextModuleController sttController;
        private QuizModuleController quizController;
        private SignLanguageQuizController signController;
        private DigitalCardGalleryController cardController;
        private ARFruitModuleController arController;
        private PlantGrowthModuleController plantController;

        // UI Root & Common Elements
        private Canvas mainCanvas;
        private RectTransform screenViewportRect;
        private Image appBackground;
        private Text statusClockText;
        private Button topBackButton;
        private Text streakBadgeText;
        private Text currentThemeButtonText;
        private Text kebunStatusText;
        private GameObject themePickerModal;
        private GameObject modeSwitcherModal;

        // 3D Stage Renderers (AR Fruit 5-Phase & 3D Leveling Garden)
        private TellMe3DStageRenderer ar3DRenderer;
        private TellMe3DStageRenderer garden3DRenderer;
        private WebCamTexture arWebCamTexture;
        private RawImage arCameraFeedRawImage;

        // BerryBears Dock Elements
        private GameObject mascotDockRoot;
        private RectTransform mascotHeadTransform;
        private RectTransform leftPupilTransform;
        private RectTransform rightPupilTransform;
        private GameObject eyelidObj;
        private Text mascotSpeechText;
        private Text mascotStatePillText;
        private Image mascotSpeechBubbleBg;

        // 9 Screen Panels
        private GameObject panelScreen0Role;
        private GameObject panelMod1Schedule;
        private GameObject panelMod2STT;
        private GameObject panelMod3Quiz;
        private GameObject panelMod4Sign;
        private GameObject panelMod5Home;
        private GameObject panelMod6Cards;
        private GameObject panelMod7AR;
        private GameObject panelMod8Plant;
        private GameObject floatingBottomNav;

        // Bottom Nav Item Buttons
        private Image[] bottomNavIconImages;
        private Text[] bottomNavLabels;

        // Modul 1 Specific Elements (Jadwal & Dashboard Pemantauan Anak)
        private GameObject mod1SubViewSchedule;
        private GameObject mod1SubViewMonitoring;
        private Transform scheduleListContent;
        private Transform parentActivityLogContainer;
        private Text parentMonitorSummaryText;
        private Button[] daySelectorButtons;
        private GameObject addScheduleModal;
        private InputField inputAddJam;
        private Dropdown dropdownAddMapel;
        private InputField inputAddJudul;
        private InputField inputAddGuru;

        // Modul 2 Specific Elements (STT + Ringkasan Guru)
        private Text sttActiveMapelText;
        private Text sttActiveClassTitleText;
        private Text sttActiveTeacherText;
        private Button sttRecordButton;
        private Text sttRecordButtonLabel;
        private Text sttLiveTranscriptText;
        private Text sttTeacherSummaryText;
        private Text sttVisualFormulaText;
        private GameObject sttWavePulseObj;

        // Modul 3 Specific Elements (Kuis & Dashboard Aktivitas)
        private GameObject quizSubViewQuiz;
        private GameObject quizSubViewDashboard;
        private Text quizQuestionProgressText;
        private Slider quizProgressBar;
        private Text quizSubjectBadgeText;
        private Text quizQuestionContentText;
        private Button[] quizOptionButtons;
        private Text[] quizOptionTexts;
        private GameObject quizResultCard;
        private Text quizResultScoreText;
        private Text quizResultDescText;
        private Button quizResultActionBtn;
        private Button quizResultDirectArBtn;
        private Text quizResultActionBtnText;
        private Text bentoTotalMateriText;
        private Text bentoKartUnlockedText;
        private Text bentoAvgScoreText;
        private Transform quizActivityLogContainer;

        // Modul 4 Specific Elements
        private Text signSystemText;
        private Text signDescText;
        private Button[] signOptionButtons;
        private Text[] signOptionTexts;
        private Button signNextButton;

        // Modul 5 Specific Elements
        private Text homeWelcomeTitleText;
        private Text homeNextClassTitle;
        private Text homeNextClassTeacher;

        // Modul 6 Specific Elements
        private Transform cardsGridContainer;

        // Modul 7 Specific Elements (AR Buah 3D dengan 5 Fase Klik)
        private Text arFruitNameId;
        private Text arFruitNameEn;
        private Text arSignInstruction;
        private Text arPhaseTitleText;
        private Text arPhaseDescText;
        private Button arCyclePhaseBtn;
        private RectTransform arFruit3DContainer;
        private RawImage arFruit3DRawImage;
        private int arCurrentPhaseIndex = 2; // Default Fase 3: Buah Matang Utuh 3D
        private float arCurrentRotation = 0f;
        private float arCurrentScale = 1f;

        // Modul 8 Specific Elements (3D Kebun Bertumbuh Level 1-7)
        private RawImage garden3DRawImage;
        private Text plantStageTitleText;
        private Text plantStageDescText;
        private Text plantStageBadgeText;
        private Text plantLevelXpText;
        private Slider plantXpSlider;
        private Text plantEmojiVisualText;
        private Slider plantMoistureSlider;
        private Text plantMoisturePercentText;
        private GameObject plantNeglectAlertBanner;
        private Button plantWaterButton;
        private Button plantNeglectSimButton;

        // Static Procedural Sprites Cache
        private static Sprite cardSprite;
        private static Sprite buttonSprite;
        private static Sprite pillSprite;
        private static Sprite circleSprite;
        private static Sprite softBlobSprite;

        // Theme Palette Definitions
        private struct ThemeColors
        {
            public Color bgApp;
            public Color bgSurface;
            public Color primary;
            public Color accent;
            public Color success;
            public Color textMain;
            public Color textSecondary;
            public Color border;
        }

        private readonly Dictionary<AppTheme, ThemeColors> themePalettes = new Dictionary<AppTheme, ThemeColors>
        {
            {
                AppTheme.SunnyOrange, new ThemeColors
                {
                    bgApp = HexToColor("#FFF9F2"),
                    bgSurface = HexToColor("#FFFFFF"),
                    primary = HexToColor("#FF7A00"),
                    accent = HexToColor("#FFB703"),
                    success = HexToColor("#10B981"),
                    textMain = HexToColor("#291804"),
                    textSecondary = HexToColor("#744E23"),
                    border = HexToColor("#FED7AA")
                }
            },
            {
                AppTheme.FreshMint, new ThemeColors
                {
                    bgApp = HexToColor("#F0FDF4"),
                    bgSurface = HexToColor("#FFFFFF"),
                    primary = HexToColor("#059669"),
                    accent = HexToColor("#10B981"),
                    success = HexToColor("#10B981"),
                    textMain = HexToColor("#064E3B"),
                    textSecondary = HexToColor("#047857"),
                    border = HexToColor("#A7F3D0")
                }
            },
            {
                AppTheme.BerryPink, new ThemeColors
                {
                    bgApp = HexToColor("#FDF2F8"),
                    bgSurface = HexToColor("#FFFFFF"),
                    primary = HexToColor("#DB2777"),
                    accent = HexToColor("#F472B6"),
                    success = HexToColor("#10B981"),
                    textMain = HexToColor("#831843"),
                    textSecondary = HexToColor("#9D174D"),
                    border = HexToColor("#FBCFE8")
                }
            },
            {
                AppTheme.OceanBlue, new ThemeColors
                {
                    bgApp = HexToColor("#F0F9FF"),
                    bgSurface = HexToColor("#FFFFFF"),
                    primary = HexToColor("#0284C7"),
                    accent = HexToColor("#38BDF8"),
                    success = HexToColor("#10B981"),
                    textMain = HexToColor("#0C4A6E"),
                    textSecondary = HexToColor("#0369A1"),
                    border = HexToColor("#BAE6FD")
                }
            },
            {
                AppTheme.SoftLavender, new ThemeColors
                {
                    bgApp = HexToColor("#FAF5FF"),
                    bgSurface = HexToColor("#FFFFFF"),
                    primary = HexToColor("#7C3AED"),
                    accent = HexToColor("#A78BFA"),
                    success = HexToColor("#10B981"),
                    textMain = HexToColor("#3B0764"),
                    textSecondary = HexToColor("#6B21A8"),
                    border = HexToColor("#E9D5FF")
                }
            }
        };

        private List<Graphic> themeBgElements = new List<Graphic>();
        private List<Graphic> themePrimaryElements = new List<Graphic>();
        private List<Graphic> themeTextMainElements = new List<Graphic>();
        private List<Graphic> themeTextSecElements = new List<Graphic>();
        private List<Graphic> themeSurfaceElements = new List<Graphic>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrapTellMeApp()
        {
            if (Instance == null && FindFirstObjectByType<TellMeMasterUI>() == null)
            {
                var bootstrapGo = new GameObject("[TellMe_MasterUI_AutoBootstrap]");
                DontDestroyOnLoad(bootstrapGo);
                bootstrapGo.AddComponent<TellMeMasterUI>();
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Pastikan orientasi portrait saat jalan di smartphone Android
            Screen.orientation = ScreenOrientation.Portrait;

            EnsureProceduralSprites();
            EnsureCoreManagers();
            EnsureEventSystem();

            if (autoBuildOnStart)
            {
                BuildCompleteTellMeUI();
            }
        }

        private void Start()
        {
            ApplyTheme(ThemeTokens.CurrentTheme);
            ThemeTokens.OnThemeChanged += ApplyTheme;

            if (navManager != null)
            {
                navManager.OnScreenChanged += HandleScreenChanged;
                navManager.NavigateTo(TellMeScreen.Screen0_RoleSelection);
            }
        }

        private void OnDestroy()
        {
            ThemeTokens.OnThemeChanged -= ApplyTheme;
            if (navManager != null)
            {
                navManager.OnScreenChanged -= HandleScreenChanged;
            }
        }

        #region Procedural Sprite Generator Engine
        private static void EnsureProceduralSprites()
        {
            if (cardSprite == null)
                cardSprite = GenerateRoundedRectSprite(128, 128, 32, new Vector4(32, 32, 32, 32));

            if (buttonSprite == null)
                buttonSprite = GenerateTactileButtonSprite(128, 128, 26, new Vector4(26, 26, 26, 26));

            if (pillSprite == null)
                pillSprite = GenerateRoundedRectSprite(128, 64, 31, new Vector4(31, 31, 31, 31));

            if (circleSprite == null)
                circleSprite = GenerateCircleSprite(256);

            if (softBlobSprite == null)
                softBlobSprite = GenerateSoftBlobSprite(256);
        }

        private static Sprite GenerateRoundedRectSprite(int width, int height, int radius, Vector4 border)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[width * height];

            float r = radius;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0;
                    float dy = 0;
                    if (x < r) dx = r - x;
                    else if (x >= width - r) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y >= height - r) dy = y - (height - r - 1);

                    float alpha = 1f;
                    if (dx > 0 && dy > 0)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist > r) alpha = 0f;
                        else if (dist > r - 1.2f) alpha = Mathf.Clamp01(r - dist);
                    }

                    byte a = (byte)(alpha * 255f);
                    pixels[y * width + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static Sprite GenerateTactileButtonSprite(int width, int height, int radius, Vector4 border)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[width * height];

            float r = radius;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = 0;
                    float dy = 0;
                    if (x < r) dx = r - x;
                    else if (x >= width - r) dx = x - (width - r - 1);

                    if (y < r) dy = r - y;
                    else if (y >= height - r) dy = y - (height - r - 1);

                    float alpha = 1f;
                    if (dx > 0 && dy > 0)
                    {
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist > r) alpha = 0f;
                        else if (dist > r - 1.2f) alpha = Mathf.Clamp01(r - dist);
                    }

                    // 3D tactile bevel shadow on bottom 8 pixels
                    byte shade = 255;
                    if (y < 8) shade = 215;
                    else if (y >= height - 6) shade = 255; // Specular top edge

                    byte a = (byte)(alpha * 255f);
                    pixels[y * width + x] = new Color32(shade, shade, shade, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static Sprite GenerateCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[size * size];

            float center = (size - 1) * 0.5f;
            float radius = center - 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 1f;
                    if (dist > radius) alpha = 0f;
                    else if (dist > radius - 1.5f) alpha = Mathf.Clamp01(radius - dist);

                    byte a = (byte)(alpha * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        private static Sprite GenerateSoftBlobSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color32[] pixels = new Color32[size * size];

            float center = (size - 1) * 0.5f;
            float radius = center;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center;
                    float dy = y - center;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float norm = Mathf.Clamp01(dist / radius);
                    float alpha = Mathf.Pow(Mathf.Cos(norm * Mathf.PI * 0.5f), 1.8f);

                    byte a = (byte)(alpha * 255f);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }
        #endregion

        #region Core Managers Setup
        private void EnsureCoreManagers()
        {
            dataManager = FindFirstObjectByType<LocalDataManager>();
            if (dataManager == null)
            {
                var go = new GameObject("[TellMe_LocalDataManager]");
                dataManager = go.AddComponent<LocalDataManager>();
            }

            navManager = FindFirstObjectByType<AppNavigationManager>();
            if (navManager == null)
            {
                var go = new GameObject("[TellMe_AppNavigationManager]");
                navManager = go.AddComponent<AppNavigationManager>();
            }

            mascotController = FindFirstObjectByType<BerryBearsController>();
            if (mascotController == null)
            {
                var go = new GameObject("[TellMe_BerryBearsController]");
                mascotController = go.AddComponent<BerryBearsController>();
            }

            // 3D Stage Renderers for AR Fruit 5-Phase & 3D Leveling Garden
            if (ar3DRenderer == null)
            {
                var arStageGO = new GameObject("[TellMe_ARFruit3DStageRenderer]");
                ar3DRenderer = arStageGO.AddComponent<TellMe3DStageRenderer>();
                ar3DRenderer.Initialize(new Vector3(500f, 500f, 500f), 512);
            }

            if (garden3DRenderer == null)
            {
                var gardenStageGO = new GameObject("[TellMe_Garden3DStageRenderer]");
                garden3DRenderer = gardenStageGO.AddComponent<TellMe3DStageRenderer>();
                garden3DRenderer.Initialize(new Vector3(-500f, 500f, 500f), 512);
            }

            // Module Controllers
            roleController = GetOrAddComponent<RoleSelectionController>();
            scheduleController = GetOrAddComponent<ScheduleModuleController>();
            sttController = GetOrAddComponent<SpeechToTextModuleController>();
            quizController = GetOrAddComponent<QuizModuleController>();
            signController = GetOrAddComponent<SignLanguageQuizController>();
            cardController = GetOrAddComponent<DigitalCardGalleryController>();
            arController = GetOrAddComponent<ARFruitModuleController>();
            plantController = GetOrAddComponent<PlantGrowthModuleController>();
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            T comp = FindFirstObjectByType<T>();
            if (comp == null)
            {
                var go = new GameObject($"[TellMe_{typeof(T).Name}]");
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        private void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }
        #endregion

        #region UI Construction Pipeline
        public void BuildCompleteTellMeUI()
        {
            // Remove previous canvas if exists
            var existingCanvas = GameObject.Find("TellMe_Canvas");
            if (existingCanvas != null)
            {
                DestroyImmediate(existingCanvas);
            }

            EnsureProceduralSprites();

            // 1. Create Main Canvas
            var canvasGO = new GameObject("TellMe_Canvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.pixelPerfect = false;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2340);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // 2. Fullscreen Background
            var bgGO = CreateUIObject("AppBackground", canvasGO.transform);
            SetStretchFull(bgGO.GetComponent<RectTransform>());
            appBackground = bgGO.AddComponent<Image>();
            appBackground.color = HexToColor("#FFF9F2");
            themeBgElements.Add(appBackground);

            // Ambient Blobs (Ultra-Soft Clay Glow)
            CreateAmbientBlob(canvasGO.transform, new Vector2(-300, 780), new Vector2(750, 750), HexToColor("#FFA000"), 0.20f);
            CreateAmbientBlob(canvasGO.transform, new Vector2(320, -700), new Vector2(800, 800), HexToColor("#FFD54F"), 0.22f);

            // 3. Safe Area Container
            var safeAreaGO = CreateUIObject("SafeAreaContainer", canvasGO.transform);
            var safeRect = safeAreaGO.GetComponent<RectTransform>();
            SetStretchFull(safeRect);
            safeRect.offsetMin = new Vector2(36, 44);
            safeRect.offsetMax = new Vector2(-36, -44);

            // 4. Android Status Bar (Top)
            BuildAndroidStatusBar(safeAreaGO.transform);

            // 5. Retention Strip (Streak, Theme Switcher, Kebun Status)
            BuildRetentionStrip(safeAreaGO.transform);

            // 6. Persistent BerryBears Companion Dock
            BuildBerryBearsCompanionDock(safeAreaGO.transform);

            // 7. Dynamic Viewport for the 9 Screens
            var viewportGO = CreateUIObject("ScreenViewport", safeAreaGO.transform);
            screenViewportRect = viewportGO.GetComponent<RectTransform>();
            screenViewportRect.anchorMin = new Vector2(0f, 0f);
            screenViewportRect.anchorMax = new Vector2(1f, 1f);
            screenViewportRect.offsetMin = new Vector2(0, 20); // Default without bottom nav
            screenViewportRect.offsetMax = new Vector2(0, -368); // Directly under Mascot Dock

            // Build Individual Screens
            panelScreen0Role = BuildScreen0RoleSelection(viewportGO.transform);
            panelMod1Schedule = BuildModul1Schedule(viewportGO.transform);
            panelMod2STT = BuildModul2STT(viewportGO.transform);
            panelMod3Quiz = BuildModul3Quiz(viewportGO.transform);
            panelMod4Sign = BuildModul4SignQuiz(viewportGO.transform);
            panelMod5Home = BuildModul5Home(viewportGO.transform);
            panelMod6Cards = BuildModul6Cards(viewportGO.transform);
            panelMod7AR = BuildModul7AR(viewportGO.transform);
            panelMod8Plant = BuildModul8Plant(viewportGO.transform);

            // 8. Floating Bottom Navigation Bar
            floatingBottomNav = BuildFloatingBottomNav(safeAreaGO.transform);

            // 9. Bottom Sheet Modals (Theme Picker & Quick Mode Switcher)
            BuildThemeAndModeModals(safeAreaGO.transform);

            // Wire up NavManager
            if (navManager != null)
            {
                navManager.RegisterPanels(
                    panelScreen0Role,
                    panelMod1Schedule,
                    panelMod2STT,
                    panelMod3Quiz,
                    panelMod4Sign,
                    panelMod5Home,
                    panelMod6Cards,
                    panelMod7AR,
                    panelMod8Plant,
                    floatingBottomNav
                );
            }

            RefreshStudentProfileHeader();
            Debug.Log("[TellMe] Revamped UI/UX System Built Successfully with 3D AR & 3D Garden!");
        }

        private void CreateAmbientBlob(Transform parent, Vector2 pos, Vector2 size, Color color, float alpha)
        {
            var blob = CreateUIObject("AmbientBlob", parent);
            var rect = blob.GetComponent<RectTransform>();
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            var img = blob.AddComponent<Image>();
            img.sprite = softBlobSprite;
            color.a = alpha;
            img.color = color;
            img.raycastTarget = false;
        }
        #endregion

        #region Header & Retention Strip
        private void BuildAndroidStatusBar(Transform parent)
        {
            var barGO = CreateUIObject("StatusBar", parent);
            var rect = barGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.sizeDelta = new Vector2(0, 64);

            var hGroup = barGO.AddComponent<HorizontalLayoutGroup>();
            hGroup.childAlignment = TextAnchor.MiddleCenter;
            hGroup.childForceExpandHeight = false;
            hGroup.childForceExpandWidth = false;
            hGroup.spacing = 12;

            // Back Button (←)
            topBackButton = CreateButton(barGO.transform, "←", HexToColor("#FFE8D6"), HexToColor("#D95D00"), 48);
            topBackButton.GetComponent<RectTransform>().sizeDelta = new Vector2(68, 48);
            topBackButton.onClick.AddListener(() => navManager?.GoBack());

            // Student Profile Pill (Tap to toggle Siswa SD Raka <-> Siswi SD Rani)
            var profileBtnGO = CreateUIObject("StudentProfilePill", barGO.transform);
            profileBtnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(460, 48);
            var profBg = profileBtnGO.AddComponent<Image>();
            profBg.sprite = pillSprite;
            profBg.type = Image.Type.Sliced;
            profBg.color = HexToColor("#FFF3E0");
            var profBtn = profileBtnGO.AddComponent<Button>();
            profBtn.targetGraphic = profBg;
            profBtn.onClick.AddListener(ToggleStudentGenderProfile);

            statusClockText = CreateText("ClockText", profileBtnGO.transform, "👦 Siswa SD: Raka (Ketuk Ganti Siswi)", 20, FontStyle.Bold, HexToColor("#744E23"));
            SetStretchFull(statusClockText.GetComponent<RectTransform>());
            statusClockText.alignment = TextAnchor.MiddleCenter;
            themeTextSecElements.Add(statusClockText);

            // Quick Mode Switcher Button (⚙️ Mode)
            var modeBtn = CreateButton(barGO.transform, "⚙️ Mode", HexToColor("#FEF3C7"), HexToColor("#92400E"), 46);
            modeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 46);
            modeBtn.onClick.AddListener(() =>
            {
                if (modeSwitcherModal != null) modeSwitcherModal.SetActive(!modeSwitcherModal.activeSelf);
            });

            // Offline Pill Badge
            var offlinePill = CreateUIObject("OfflinePill", barGO.transform);
            offlinePill.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 46);
            var pillImg = offlinePill.AddComponent<Image>();
            pillImg.sprite = pillSprite;
            pillImg.type = Image.Type.Sliced;
            pillImg.color = HexToColor("#D1FAE5");

            var offlineTxt = CreateText("OfflineText", offlinePill.transform, "● 100% OFFLINE", 19, FontStyle.Bold, HexToColor("#065F46"));
            SetStretchFull(offlineTxt.GetComponent<RectTransform>());
            offlineTxt.alignment = TextAnchor.MiddleCenter;
        }

        public void ToggleStudentGenderProfile()
        {
            if (dataManager == null || dataManager.Data == null) return;
            bool nextIsFemale = !dataManager.Data.isSiswiPerempuan;
            dataManager.GantiProfilSiswa(nextIsFemale);
            RefreshStudentProfileHeader();

            string panggilan = nextIsFemale ? "Rani (Siswi SD)" : "Raka (Siswa SD)";
            mascotController?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                $"Halo {panggilan}! BerryBears siap menemani kamu belajar hari ini!");
        }

        private void RefreshStudentProfileHeader()
        {
            if (dataManager == null || dataManager.Data == null) return;
            bool isFemale = dataManager.Data.isSiswiPerempuan;
            string icon = isFemale ? "👧" : "👦";
            string namaPendek = isFemale ? "Rani • Siswi SD" : "Raka • Siswa SD";

            if (statusClockText != null)
            {
                statusClockText.text = $"{icon} {namaPendek} (Ganti ⇄)";
            }

            if (homeWelcomeTitleText != null)
            {
                homeWelcomeTitleText.text = $"Halo {(isFemale ? "Rani (Siswi SD)" : "Raka (Siswa SD)")}! Selamat Belajar! 🐻";
            }

            if (kebunStatusText != null)
            {
                kebunStatusText.text = $"🌱 Kebun 3D Lv.{dataManager.Data.levelKebun}";
            }
        }

        private void BuildRetentionStrip(Transform parent)
        {
            var stripGO = CreateUIObject("RetentionStrip", parent);
            var rect = stripGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -68);
            rect.sizeDelta = new Vector2(0, 74);

            var bg = stripGO.AddComponent<Image>();
            bg.sprite = cardSprite;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(1f, 1f, 1f, 0.94f);
            themeSurfaceElements.Add(bg);

            var shadow = stripGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.06f);
            shadow.effectDistance = new Vector2(0, -3);

            var hGroup = stripGO.AddComponent<HorizontalLayoutGroup>();
            hGroup.childAlignment = TextAnchor.MiddleCenter;
            hGroup.childForceExpandHeight = false;
            hGroup.childForceExpandWidth = true;
            hGroup.padding = new RectOffset(24, 24, 8, 8);
            hGroup.spacing = 16;

            // Streak Badge
            var streakGO = CreateUIObject("StreakBadge", stripGO.transform);
            streakBadgeText = CreateText("StreakText", streakGO.transform, "🔥 3 Hari Streak", 22, FontStyle.Bold, HexToColor("#FF7A00"));
            streakBadgeText.alignment = TextAnchor.MiddleLeft;
            themePrimaryElements.Add(streakBadgeText);

            // In-App Theme Switcher Button (Opens Theme Picker Modal)
            var themeBtnGO = CreateUIObject("ThemeSwitcherBtn", stripGO.transform);
            themeBtnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 48);
            var themeBtnImg = themeBtnGO.AddComponent<Image>();
            themeBtnImg.sprite = pillSprite;
            themeBtnImg.type = Image.Type.Sliced;
            themeBtnImg.color = HexToColor("#FFF3E0");

            var themeBtn = themeBtnGO.AddComponent<Button>();
            themeBtn.targetGraphic = themeBtnImg;
            themeBtn.onClick.AddListener(() =>
            {
                if (themePickerModal != null) themePickerModal.SetActive(!themePickerModal.activeSelf);
                else CycleTheme();
            });

            currentThemeButtonText = CreateText("ThemeLabel", themeBtnGO.transform, "🎨 Sunny", 20, FontStyle.Bold, HexToColor("#D95D00"));
            SetStretchFull(currentThemeButtonText.GetComponent<RectTransform>());
            currentThemeButtonText.alignment = TextAnchor.MiddleCenter;

            // Garden Status (Click to jump to 3D Garden)
            var gardenGO = CreateUIObject("GardenStatusBadge", stripGO.transform);
            var gardenBtn = gardenGO.AddComponent<Button>();
            gardenBtn.onClick.AddListener(() => navManager?.NavigateTo(TellMeScreen.Module8_TumbuhKembangTanaman));
            kebunStatusText = CreateText("GardenText", gardenGO.transform, "🌱 Kebun 3D Lv.2", 22, FontStyle.Bold, HexToColor("#10B981"));
            kebunStatusText.alignment = TextAnchor.MiddleRight;
        }

        private void BuildThemeAndModeModals(Transform parent)
        {
            // 1. Theme Picker Modal (5 Tema Ramah Anak)
            themePickerModal = CreateUIObject("ThemePickerModal", parent);
            var tRect = themePickerModal.GetComponent<RectTransform>();
            tRect.sizeDelta = new Vector2(880, 560);
            tRect.anchoredPosition = new Vector2(0, 60);
            var tBg = themePickerModal.AddComponent<Image>();
            tBg.sprite = cardSprite;
            tBg.type = Image.Type.Sliced;
            tBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(tBg);

            var tShadow = themePickerModal.AddComponent<Shadow>();
            tShadow.effectColor = new Color(0, 0, 0, 0.25f);
            tShadow.effectDistance = new Vector2(0, -10);

            var tV = themePickerModal.AddComponent<VerticalLayoutGroup>();
            tV.childAlignment = TextAnchor.UpperCenter;
            tV.childForceExpandHeight = false;
            tV.childForceExpandWidth = true;
            tV.padding = new RectOffset(28, 28, 22, 22);
            tV.spacing = 12;

            var tTitle = CreateText("Title", themePickerModal.transform, "🎨 Pilih Tema Warna Aplikasi", 28, FontStyle.Bold, HexToColor("#291804"));
            tTitle.alignment = TextAnchor.MiddleCenter;

            CreateThemeOptionButton(themePickerModal.transform, "☀️ Sunny Orange (Default Hangat)", AppTheme.SunnyOrange, HexToColor("#FFF3E0"), HexToColor("#D95D00"));
            CreateThemeOptionButton(themePickerModal.transform, "🌿 Fresh Mint (Hijau Segar)", AppTheme.FreshMint, HexToColor("#D1FAE5"), HexToColor("#065F46"));
            CreateThemeOptionButton(themePickerModal.transform, "🍓 Sweet Berry (Merah Muda Ceria)", AppTheme.BerryPink, HexToColor("#FCE7F3"), HexToColor("#9D174D"));
            CreateThemeOptionButton(themePickerModal.transform, "🌊 Cool Ocean (Biru Langit)", AppTheme.OceanBlue, HexToColor("#E0F2FE"), HexToColor("#0369A1"));
            CreateThemeOptionButton(themePickerModal.transform, "💜 Soft Lavender (Ungu Lembut)", AppTheme.SoftLavender, HexToColor("#F3E8FF"), HexToColor("#6B21A8"));

            var closeThemeBtn = CreateButton(themePickerModal.transform, "Tutup", HexToColor("#F1F5F9"), HexToColor("#334155"), 52);
            closeThemeBtn.onClick.AddListener(() => themePickerModal.SetActive(false));
            themePickerModal.SetActive(false);

            // 2. Quick Mode Switcher Modal
            modeSwitcherModal = CreateUIObject("ModeSwitcherModal", parent);
            var mRect = modeSwitcherModal.GetComponent<RectTransform>();
            mRect.sizeDelta = new Vector2(880, 520);
            mRect.anchoredPosition = new Vector2(0, 80);
            var mBg = modeSwitcherModal.AddComponent<Image>();
            mBg.sprite = cardSprite;
            mBg.type = Image.Type.Sliced;
            mBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(mBg);

            var mShadow = modeSwitcherModal.AddComponent<Shadow>();
            mShadow.effectColor = new Color(0, 0, 0, 0.25f);
            mShadow.effectDistance = new Vector2(0, -10);

            var mV = modeSwitcherModal.AddComponent<VerticalLayoutGroup>();
            mV.childAlignment = TextAnchor.UpperCenter;
            mV.childForceExpandHeight = false;
            mV.childForceExpandWidth = true;
            mV.padding = new RectOffset(28, 28, 22, 22);
            mV.spacing = 14;

            var mTitle = CreateText("Title", modeSwitcherModal.transform, "⚙️ Ganti Mode & Profil Pengguna", 28, FontStyle.Bold, HexToColor("#291804"));
            mTitle.alignment = TextAnchor.MiddleCenter;

            var btnParent = CreateButton(modeSwitcherModal.transform, "👨‍👩‍👧 Mode Orang Tua (Jadwal & Pantau Aktivitas Anak)", HexToColor("#FFE8D6"), HexToColor("#D95D00"), 64);
            btnParent.onClick.AddListener(() =>
            {
                modeSwitcherModal.SetActive(false);
                navManager?.NavigateTo(TellMeScreen.Module1_JadwalOrangTua);
            });

            var btnSiswa = CreateButton(modeSwitcherModal.transform, "👦 Mode Siswa SD: Raka (Kelas 3 SD/SLB)", HexToColor("#D1FAE5"), HexToColor("#065F46"), 64);
            btnSiswa.onClick.AddListener(() =>
            {
                dataManager?.GantiProfilSiswa(false);
                RefreshStudentProfileHeader();
                modeSwitcherModal.SetActive(false);
                navManager?.NavigateTo(TellMeScreen.Module5_BerandaBerryBears);
            });

            var btnSiswi = CreateButton(modeSwitcherModal.transform, "👧 Mode Siswi SD: Rani (Kelas 3 SD/SLB)", HexToColor("#FCE7F3"), HexToColor("#9D174D"), 64);
            btnSiswi.onClick.AddListener(() =>
            {
                dataManager?.GantiProfilSiswa(true);
                RefreshStudentProfileHeader();
                modeSwitcherModal.SetActive(false);
                navManager?.NavigateTo(TellMeScreen.Module5_BerandaBerryBears);
            });

            var btnScreen0 = CreateButton(modeSwitcherModal.transform, "🏠 Kembali ke Layar 0 (Pemilihan Peran)", HexToColor("#E0F2FE"), HexToColor("#0369A1"), 58);
            btnScreen0.onClick.AddListener(() =>
            {
                modeSwitcherModal.SetActive(false);
                navManager?.NavigateTo(TellMeScreen.Screen0_RoleSelection);
            });

            var closeModeBtn = CreateButton(modeSwitcherModal.transform, "Tutup", HexToColor("#F1F5F9"), HexToColor("#334155"), 52);
            closeModeBtn.onClick.AddListener(() => modeSwitcherModal.SetActive(false));
            modeSwitcherModal.SetActive(false);
        }

        private void CreateThemeOptionButton(Transform parent, string label, AppTheme theme, Color bg, Color txtCol)
        {
            var btn = CreateButton(parent, label, bg, txtCol, 56);
            btn.onClick.AddListener(() =>
            {
                ThemeTokens.SetTheme(theme);
                if (themePickerModal != null) themePickerModal.SetActive(false);
            });
        }
        #endregion

        #region BerryBears Companion Dock
        private void BuildBerryBearsCompanionDock(Transform parent)
        {
            mascotDockRoot = CreateUIObject("BerryBearsCompanionDock", parent);
            var rect = mascotDockRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -150);
            rect.sizeDelta = new Vector2(0, 204);

            var bgImg = mascotDockRoot.AddComponent<Image>();
            bgImg.sprite = cardSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(bgImg);

            var shadow = mascotDockRoot.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.08f);
            shadow.effectDistance = new Vector2(0, -5);

            var dockBtn = mascotDockRoot.AddComponent<Button>();
            dockBtn.targetGraphic = bgImg;
            dockBtn.onClick.AddListener(() =>
            {
                mascotController?.TriggerOnTapReaction();
                StartCoroutine(SquashAndStretchRoutine(mascotHeadTransform));
            });

            var hGroup = mascotDockRoot.AddComponent<HorizontalLayoutGroup>();
            hGroup.childAlignment = TextAnchor.MiddleCenter;
            hGroup.childForceExpandHeight = false;
            hGroup.childForceExpandWidth = false;
            hGroup.padding = new RectOffset(20, 20, 14, 14);
            hGroup.spacing = 20;

            // Avatar Stage Container
            var avatarStageGO = CreateUIObject("AvatarStage", mascotDockRoot.transform);
            avatarStageGO.GetComponent<RectTransform>().sizeDelta = new Vector2(150, 150);
            var avatarBg = avatarStageGO.AddComponent<Image>();
            avatarBg.sprite = cardSprite;
            avatarBg.type = Image.Type.Sliced;
            avatarBg.color = HexToColor("#FFF4E5");

            // Ears (Ditempatkan di belakang kepala)
            CreateBearEar(avatarStageGO.transform, new Vector2(-44, 46));
            CreateBearEar(avatarStageGO.transform, new Vector2(44, 46));

            // Smooth Procedural Bear Head
            var headGO = CreateUIObject("BerryBearHead", avatarStageGO.transform);
            mascotHeadTransform = headGO.GetComponent<RectTransform>();
            mascotHeadTransform.sizeDelta = new Vector2(120, 120);
            mascotHeadTransform.anchoredPosition = new Vector2(0, -4);
            var headImg = headGO.AddComponent<Image>();
            headImg.sprite = circleSprite;
            headImg.color = HexToColor("#D97706"); // Warm Bear Amber

            // Head Highlight (Glossy Reflection)
            var glossGO = CreateUIObject("GlossHighlight", headGO.transform);
            var glossRect = glossGO.GetComponent<RectTransform>();
            glossRect.sizeDelta = new Vector2(34, 18);
            glossRect.anchoredPosition = new Vector2(-24, 28);
            var glossImg = glossGO.AddComponent<Image>();
            glossImg.sprite = pillSprite;
            glossImg.type = Image.Type.Sliced;
            glossImg.color = new Color(1f, 1f, 1f, 0.40f);

            // Blush Cheeks
            CreateBlushCheek(headGO.transform, new Vector2(-36, -10));
            CreateBlushCheek(headGO.transform, new Vector2(36, -10));

            // Muzzle (Snout)
            var muzzleGO = CreateUIObject("Muzzle", headGO.transform);
            var muzzleRect = muzzleGO.GetComponent<RectTransform>();
            muzzleRect.sizeDelta = new Vector2(62, 44);
            muzzleRect.anchoredPosition = new Vector2(0, -14);
            var muzzleImg = muzzleGO.AddComponent<Image>();
            muzzleImg.sprite = pillSprite;
            muzzleImg.type = Image.Type.Sliced;
            muzzleImg.color = HexToColor("#FEF3C7");

            // Nose
            var noseGO = CreateUIObject("Nose", muzzleGO.transform);
            var noseRect = noseGO.GetComponent<RectTransform>();
            noseRect.sizeDelta = new Vector2(20, 14);
            noseRect.anchoredPosition = new Vector2(0, 6);
            var noseImg = noseGO.AddComponent<Image>();
            noseImg.sprite = pillSprite;
            noseImg.type = Image.Type.Sliced;
            noseImg.color = HexToColor("#451A03");

            // Smile Mouth Arc
            var mouthTxt = CreateText("Mouth", muzzleGO.transform, "‿", 22, FontStyle.Bold, HexToColor("#451A03"));
            var mouthRect = mouthTxt.GetComponent<RectTransform>();
            mouthRect.anchoredPosition = new Vector2(0, -8);
            mouthTxt.alignment = TextAnchor.MiddleCenter;

            // Eyes (Sclera + Pupils)
            var leftEyeGO = CreateUIObject("LeftEye", headGO.transform);
            var leftEyeRect = leftEyeGO.GetComponent<RectTransform>();
            leftEyeRect.sizeDelta = new Vector2(26, 26);
            leftEyeRect.anchoredPosition = new Vector2(-22, 16);
            var leftEyeImg = leftEyeGO.AddComponent<Image>();
            leftEyeImg.sprite = circleSprite;
            leftEyeImg.color = Color.white;

            var leftPupilGO = CreateUIObject("LeftPupil", leftEyeGO.transform);
            leftPupilTransform = leftPupilGO.GetComponent<RectTransform>();
            leftPupilTransform.sizeDelta = new Vector2(14, 14);
            var leftPupilImg = leftPupilGO.AddComponent<Image>();
            leftPupilImg.sprite = circleSprite;
            leftPupilImg.color = HexToColor("#291804");

            // Pupil Shiny Dot
            var leftShineGO = CreateUIObject("Shine", leftPupilGO.transform);
            var lsRect = leftShineGO.GetComponent<RectTransform>();
            lsRect.sizeDelta = new Vector2(5, 5);
            lsRect.anchoredPosition = new Vector2(2, 2);
            var lsImg = leftShineGO.AddComponent<Image>();
            lsImg.sprite = circleSprite;
            lsImg.color = Color.white;

            var rightEyeGO = CreateUIObject("RightEye", headGO.transform);
            var rightEyeRect = rightEyeGO.GetComponent<RectTransform>();
            rightEyeRect.sizeDelta = new Vector2(26, 26);
            rightEyeRect.anchoredPosition = new Vector2(22, 16);
            var rightEyeImg = rightEyeGO.AddComponent<Image>();
            rightEyeImg.sprite = circleSprite;
            rightEyeImg.color = Color.white;

            var rightPupilGO = CreateUIObject("RightPupil", rightEyeGO.transform);
            rightPupilTransform = rightPupilGO.GetComponent<RectTransform>();
            rightPupilTransform.sizeDelta = new Vector2(14, 14);
            var rightPupilImg = rightPupilGO.AddComponent<Image>();
            rightPupilImg.sprite = circleSprite;
            rightPupilImg.color = HexToColor("#291804");

            var rightShineGO = CreateUIObject("Shine", rightPupilGO.transform);
            var rsRect = rightShineGO.GetComponent<RectTransform>();
            rsRect.sizeDelta = new Vector2(5, 5);
            rsRect.anchoredPosition = new Vector2(2, 2);
            var rsImg = rightShineGO.AddComponent<Image>();
            rsImg.sprite = circleSprite;
            rsImg.color = Color.white;

            // Eyelids for Organic Blinking
            eyelidObj = CreateUIObject("Eyelids", headGO.transform);
            var eyelidRect = eyelidObj.GetComponent<RectTransform>();
            eyelidRect.sizeDelta = new Vector2(76, 28);
            eyelidRect.anchoredPosition = new Vector2(0, 16);
            var eyelidImg = eyelidObj.AddComponent<Image>();
            eyelidImg.sprite = pillSprite;
            eyelidImg.type = Image.Type.Sliced;
            eyelidImg.color = HexToColor("#D97706");
            eyelidObj.SetActive(false);

            // Speech Bubble Container
            var bubbleGO = CreateUIObject("SpeechBubbleBox", mascotDockRoot.transform);
            var bubbleRect = bubbleGO.GetComponent<RectTransform>();
            bubbleRect.sizeDelta = new Vector2(790, 172);
            mascotSpeechBubbleBg = bubbleGO.AddComponent<Image>();
            mascotSpeechBubbleBg.sprite = cardSprite;
            mascotSpeechBubbleBg.type = Image.Type.Sliced;
            mascotSpeechBubbleBg.color = HexToColor("#FFFDF9");

            var vGroup = bubbleGO.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.MiddleLeft;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(20, 20, 12, 12);
            vGroup.spacing = 6;

            // Top Row (Name + State Pill)
            var topRowGO = CreateUIObject("TopRow", bubbleGO.transform);
            topRowGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 38);
            var hRow = topRowGO.AddComponent<HorizontalLayoutGroup>();
            hRow.childForceExpandWidth = false;
            hRow.spacing = 16;

            var nameTxt = CreateText("BearName", topRowGO.transform, "🐻 BerryBears", 24, FontStyle.Bold, HexToColor("#D95D00"));
            nameTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 38);

            var pillGO = CreateUIObject("StatePill", topRowGO.transform);
            var pillImg = pillGO.AddComponent<Image>();
            pillImg.sprite = pillSprite;
            pillImg.type = Image.Type.Sliced;
            pillImg.color = HexToColor("#FFE8D6");
            pillGO.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 36);

            mascotStatePillText = CreateText("StatePillText", pillGO.transform, "STATE_IDLE", 18, FontStyle.Bold, HexToColor("#FF7A00"));
            SetStretchFull(mascotStatePillText.GetComponent<RectTransform>());
            mascotStatePillText.alignment = TextAnchor.MiddleCenter;

            // Speech Text
            mascotSpeechText = CreateText("SpeechText", bubbleGO.transform, "Halo Raka! Siap belajar bersama BerryBears?", 22, FontStyle.Normal, HexToColor("#291804"));
            mascotSpeechText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            themeTextMainElements.Add(mascotSpeechText);

            // Tap hint chip
            var hintTxt = CreateText("HintChip", bubbleGO.transform, "✨ Ketuk aku kapan saja (Berkedip & Lirik Mata)", 18, FontStyle.Italic, HexToColor("#A37A4C"));
            hintTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);

            // Setup Mascot Controller References
            if (mascotController != null)
            {
                mascotController.SetupComponents(
                    headGO.transform,
                    leftPupilTransform,
                    rightPupilTransform,
                    eyelidObj,
                    bubbleGO,
                    mascotSpeechText,
                    mascotSpeechBubbleBg
                );
            }
        }

        private void CreateBearEar(Transform parent, Vector2 pos)
        {
            var earGO = CreateUIObject("Ear", parent);
            var rect = earGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(44, 44);
            rect.anchoredPosition = pos;
            var img = earGO.AddComponent<Image>();
            img.sprite = circleSprite;
            img.color = HexToColor("#B45309");

            var innerEar = CreateUIObject("InnerEar", earGO.transform);
            var iRect = innerEar.GetComponent<RectTransform>();
            iRect.sizeDelta = new Vector2(22, 22);
            iRect.anchoredPosition = Vector2.zero;
            var iImg = innerEar.AddComponent<Image>();
            iImg.sprite = circleSprite;
            iImg.color = HexToColor("#92400E");
        }

        private void CreateBlushCheek(Transform parent, Vector2 pos)
        {
            var cheekGO = CreateUIObject("BlushCheek", parent);
            var rect = cheekGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(26, 16);
            rect.anchoredPosition = pos;
            var img = cheekGO.AddComponent<Image>();
            img.sprite = pillSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.96f, 0.25f, 0.37f, 0.45f); // Soft Coral Pink
        }
        #endregion

        #region Screen 0: Role Selection (Redesign Seimbang & Taktil)
        private GameObject BuildScreen0RoleSelection(Transform parent)
        {
            var panel = CreateUIObject("Screen0_RoleSelection", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            // Scroll container agar nyaman pada semua rasio aspek
            var scrollRect = panel.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var contentGO = CreateUIObject("RoleContent", panel.transform);
            var contentRect = contentGO.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 1150);
            scrollRect.content = contentRect;

            var vGroup = contentGO.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(20, 20, 16, 24);
            vGroup.spacing = 24;

            // Brand Badge Header
            var headerBox = CreateUIObject("HeaderBox", contentGO.transform);
            headerBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);
            var hBoxV = headerBox.AddComponent<VerticalLayoutGroup>();
            hBoxV.childAlignment = TextAnchor.UpperCenter;
            hBoxV.childForceExpandHeight = false;
            hBoxV.childForceExpandWidth = true;
            hBoxV.spacing = 8;

            var badgePill = CreateUIObject("BadgePill", headerBox.transform);
            badgePill.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 38);
            var bImg = badgePill.AddComponent<Image>();
            bImg.sprite = pillSprite;
            bImg.type = Image.Type.Sliced;
            bImg.color = HexToColor("#FFE8D6");
            var bTxt = CreateText("BadgeTxt", badgePill.transform, "Layar 0 • Satu Perangkat", 18, FontStyle.Bold, HexToColor("#D95D00"));
            SetStretchFull(bTxt.GetComponent<RectTransform>());
            bTxt.alignment = TextAnchor.MiddleCenter;

            var title = CreateText("Title", headerBox.transform, "Pilih Mode Belajar", 36, FontStyle.Bold, HexToColor("#291804"));
            title.alignment = TextAnchor.MiddleCenter;
            title.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 46);
            themeTextMainElements.Add(title);

            var desc = CreateText("Desc", headerBox.transform, "Satu perangkat digunakan bersama oleh Orang Tua di rumah dan Siswa di kelas.", 22, FontStyle.Normal, HexToColor("#744E23"));
            desc.alignment = TextAnchor.MiddleCenter;
            desc.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 36);
            themeTextSecElements.Add(desc);

            // Card 1: Mode Orang Tua
            var parentCard = CreateRoleCardVertical(
                contentGO.transform,
                badgeLabel: "👨‍👩‍👧 MODE ORANG TUA",
                badgeBg: HexToColor("#FFE8D6"),
                badgeTextColor: HexToColor("#D95D00"),
                icon: "🗓️",
                title: "Atur Jadwal & Pengingat",
                desc: "Atur jam pelajaran, mata pelajaran, nama guru, dan pengingat harian (PIN: 1234).",
                btnLabel: "Buka Mode Orang Tua →",
                cardBg: HexToColor("#FFFBF6"),
                btnBg: HexToColor("#FFE8D6"),
                btnTextColor: HexToColor("#D95D00")
            );

            // Card 2: Mode Siswa (Raka)
            var studentCard = CreateRoleCardVertical(
                contentGO.transform,
                badgeLabel: "🎒 MODE SISWA",
                badgeBg: HexToColor("#FF7A00"),
                badgeTextColor: Color.white,
                icon: "🐻✨",
                title: "Mulai Belajar (Raka)",
                desc: "Cukup 1 ketukan rekam suara guru, kuis visual, kartu buah 3D & rawat kebun virtual.",
                btnLabel: "Mulai Belajar Sekarang! 🚀",
                cardBg: HexToColor("#F0FDF4"),
                btnBg: HexToColor("#059669"),
                btnTextColor: Color.white
            );

            // Card 3: Pengingat Kelas Hari Ini (Quick Glance)
            var reminderCard = CreateQuickReminderCard(contentGO.transform);

            // PIN Modal
            var pinModal = CreateUIObject("ParentPinModal", panel.transform);
            var pinRect = pinModal.GetComponent<RectTransform>();
            pinRect.sizeDelta = new Vector2(860, 420);
            var pinBg = pinModal.AddComponent<Image>();
            pinBg.sprite = cardSprite;
            pinBg.type = Image.Type.Sliced;
            pinBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(pinBg);

            var pinShadow = pinModal.AddComponent<Shadow>();
            pinShadow.effectColor = new Color(0, 0, 0, 0.20f);
            pinShadow.effectDistance = new Vector2(0, -8);

            var pinVGroup = pinModal.AddComponent<VerticalLayoutGroup>();
            pinVGroup.childAlignment = TextAnchor.MiddleCenter;
            pinVGroup.childForceExpandHeight = false;
            pinVGroup.childForceExpandWidth = true;
            pinVGroup.padding = new RectOffset(32, 32, 28, 28);
            pinVGroup.spacing = 18;

            var pinTitle = CreateText("PinTitle", pinModal.transform, "🔒 Masukkan PIN Mode Orang Tua", 30, FontStyle.Bold, HexToColor("#291804"));
            pinTitle.alignment = TextAnchor.MiddleCenter;

            var pinSub = CreateText("PinSub", pinModal.transform, "PIN default adalah 1234 untuk menjaga pengaturan jadwal.", 20, FontStyle.Normal, HexToColor("#744E23"));
            pinSub.alignment = TextAnchor.MiddleCenter;

            var pinInputGO = CreateInputField(pinModal.transform, "Ketik PIN (Bawaan: 1234)", 30);
            var pinInput = pinInputGO.GetComponent<InputField>();
            pinInput.contentType = InputField.ContentType.Password;

            var pinErrTxt = CreateText("PinError", pinModal.transform, "", 22, FontStyle.Italic, HexToColor("#EF4444"));
            pinErrTxt.alignment = TextAnchor.MiddleCenter;

            var pinBtnRow = CreateUIObject("PinBtnRow", pinModal.transform);
            pinBtnRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 68);
            var pinHRow = pinBtnRow.AddComponent<HorizontalLayoutGroup>();
            pinHRow.childForceExpandWidth = true;
            pinHRow.spacing = 20;

            var cancelBtn = CreateButton(pinBtnRow.transform, "Batal", HexToColor("#FEE2E2"), HexToColor("#DC2626"));
            cancelBtn.onClick.AddListener(() => pinModal.SetActive(false));

            var verifyBtn = CreateButton(pinBtnRow.transform, "Masuk (1234)", HexToColor("#D1FAE5"), HexToColor("#059669"));

            pinModal.SetActive(false);

            if (roleController != null)
            {
                roleController.SetupComponents(parentCard, studentCard, pinModal, pinInput, pinErrTxt);
                verifyBtn.onClick.AddListener(roleController.VerifyParentPinAndEnter);
            }

            return panel;
        }

        private Button CreateRoleCardVertical(
            Transform parent, string badgeLabel, Color badgeBg, Color badgeTextColor,
            string icon, string title, string desc, string btnLabel, Color cardBg, Color btnBg, Color btnTextColor)
        {
            var cardGO = CreateUIObject("RoleCardVertical", parent);
            cardGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 290);

            var img = cardGO.AddComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = cardBg;

            var shadow = cardGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.08f);
            shadow.effectDistance = new Vector2(0, -5);

            var vGroup = cardGO.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperLeft;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(26, 26, 20, 20);
            vGroup.spacing = 10;

            // Top Row: Badge + Icon
            var topRow = CreateUIObject("TopRow", cardGO.transform);
            topRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 42);
            var hRow = topRow.AddComponent<HorizontalLayoutGroup>();
            hRow.childForceExpandWidth = false;
            hRow.spacing = 16;

            var pill = CreateUIObject("BadgePill", topRow.transform);
            pill.GetComponent<RectTransform>().sizeDelta = new Vector2(260, 38);
            var pImg = pill.AddComponent<Image>();
            pImg.sprite = pillSprite;
            pImg.type = Image.Type.Sliced;
            pImg.color = badgeBg;
            var pTxt = CreateText("BadgeTxt", pill.transform, badgeLabel, 18, FontStyle.Bold, badgeTextColor);
            SetStretchFull(pTxt.GetComponent<RectTransform>());
            pTxt.alignment = TextAnchor.MiddleCenter;

            var iconTxt = CreateText("IconTxt", topRow.transform, icon, 32, FontStyle.Normal, Color.white);
            iconTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(50, 42);
            iconTxt.alignment = TextAnchor.MiddleRight;

            var titleTxt = CreateText("CardTitle", cardGO.transform, title, 28, FontStyle.Bold, HexToColor("#291804"));
            titleTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 38);

            var descTxt = CreateText("CardDesc", cardGO.transform, desc, 20, FontStyle.Normal, HexToColor("#744E23"));
            descTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 56);

            var btn = CreateButton(cardGO.transform, btnLabel, btnBg, btnTextColor, 64);
            return btn;
        }

        private GameObject CreateQuickReminderCard(Transform parent)
        {
            var cardGO = CreateUIObject("QuickReminderCard", parent);
            cardGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 220);

            var img = cardGO.AddComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(img);

            var shadow = cardGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.06f);
            shadow.effectDistance = new Vector2(0, -4);

            var vGroup = cardGO.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperLeft;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(24, 24, 18, 18);
            vGroup.spacing = 10;

            var topRow = CreateUIObject("TopRow", cardGO.transform);
            topRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 36);
            var hRow = topRow.AddComponent<HorizontalLayoutGroup>();
            hRow.childForceExpandWidth = false;
            hRow.spacing = 16;

            var labelTxt = CreateText("Label", topRow.transform, "🔔 PENGINGAT HARI INI", 20, FontStyle.Bold, HexToColor("#FF7A00"));
            labelTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 36);

            var pill = CreateUIObject("SubjectPill", topRow.transform);
            pill.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 32);
            var pImg = pill.AddComponent<Image>();
            pImg.sprite = pillSprite;
            pImg.type = Image.Type.Sliced;
            pImg.color = HexToColor("#FFE4E6");
            var pTxt = CreateText("PillTxt", pill.transform, "Matematika", 18, FontStyle.Bold, HexToColor("#9F1239"));
            SetStretchFull(pTxt.GetComponent<RectTransform>());
            pTxt.alignment = TextAnchor.MiddleCenter;

            var schedTxt = CreateText("SchedDetail", cardGO.transform, "08:00 WIB — Penjumlahan 1–20 (Bu Sari)", 24, FontStyle.Bold, HexToColor("#291804"));
            schedTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 34);

            var startBtn = CreateButton(cardGO.transform, "Mulai Kelas 🔔", HexToColor("#D1FAE5"), HexToColor("#059669"), 56);
            startBtn.onClick.AddListener(() =>
            {
                navManager?.NavigateTo(TellMeScreen.Module2_SpeechToTextBelajar);
            });

            return cardGO;
        }
        #endregion

        #region Modul 1: Jadwal Belajar & Dashboard Pemantauan Anak (Mode Orang Tua)
        private GameObject BuildModul1Schedule(Transform parent)
        {
            var panel = CreateUIObject("Module1_Jadwal", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            var vGroup = panel.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(16, 16, 12, 12);
            vGroup.spacing = 12;

            // Header Back Button & Title
            var headerRow = CreateUIObject("HeaderRow", panel.transform);
            headerRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 56);
            var hRow = headerRow.AddComponent<HorizontalLayoutGroup>();
            hRow.childForceExpandWidth = false;
            hRow.spacing = 16;

            var backBtn = CreateButton(headerRow.transform, "⬅️ Layar 0", HexToColor("#FFE8D6"), HexToColor("#D95D00"), 52);
            backBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 52);
            backBtn.onClick.AddListener(() => navManager?.NavigateTo(TellMeScreen.Screen0_RoleSelection));

            var headerTitle = CreateText("HeaderTitle", headerRow.transform, "👨‍👩‍👧 Mode Orang Tua: Jadwal & Pantau Anak", 24, FontStyle.Bold, HexToColor("#291804"));
            headerTitle.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 52);
            headerTitle.alignment = TextAnchor.MiddleLeft;

            // Sub-Tab Switcher: Atur Jadwal Belajar vs Dashboard Pemantauan Aktivitas Anak
            var subTabRow = CreateUIObject("ParentSubTabRow", panel.transform);
            subTabRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 56);
            var subTabH = subTabRow.AddComponent<HorizontalLayoutGroup>();
            subTabH.childForceExpandWidth = true;
            subTabH.spacing = 14;

            var btnTabJadwal = CreateButton(subTabRow.transform, "📅 Atur Jadwal Belajar", HexToColor("#FF7A00"), Color.white, 52);
            var btnTabMonitor = CreateButton(subTabRow.transform, "📊 Dashboard Aktivitas Anak", HexToColor("#FFF3E0"), HexToColor("#744E23"), 52);

            // =========================================================
            // SUB-VIEW 1: ATUR JADWAL BELAJAR
            // =========================================================
            mod1SubViewSchedule = CreateUIObject("SubView_Jadwal", panel.transform);
            mod1SubViewSchedule.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 820);
            var schedV = mod1SubViewSchedule.AddComponent<VerticalLayoutGroup>();
            schedV.childAlignment = TextAnchor.UpperCenter;
            schedV.childForceExpandHeight = false;
            schedV.childForceExpandWidth = true;
            schedV.spacing = 14;

            // Day Selector (S M T W T F S)
            var daysRow = CreateUIObject("DaysRow", mod1SubViewSchedule.transform);
            daysRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 62);
            var daysLayout = daysRow.AddComponent<HorizontalLayoutGroup>();
            daysLayout.childForceExpandWidth = true;
            daysLayout.spacing = 10;

            string[] dayLabels = { "S", "M", "T", "W", "T", "F", "S" };
            daySelectorButtons = new Button[7];

            for (int i = 0; i < 7; i++)
            {
                int dayIndex = i;
                var btn = CreateButton(daysRow.transform, dayLabels[i], HexToColor("#FFF3E0"), HexToColor("#744E23"), 54);
                daySelectorButtons[i] = btn;
                btn.onClick.AddListener(() =>
                {
                    scheduleController?.SelectDay(dayIndex);
                    HighlightSelectedDay(dayIndex);
                    RefreshScheduleCardsList();
                });
            }

            // Schedule Cards List Container (Scrollable)
            var scrollGO = CreateUIObject("ScheduleScrollView", mod1SubViewSchedule.transform);
            scrollGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 500);
            var scrollImg = scrollGO.AddComponent<Image>();
            scrollImg.sprite = cardSprite;
            scrollImg.type = Image.Type.Sliced;
            scrollImg.color = new Color(1f, 1f, 1f, 0.5f);

            var scrollRect = scrollGO.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var contentGO = CreateUIObject("ScheduleContent", scrollGO.transform);
            SetStretchFull(contentGO.GetComponent<RectTransform>());
            scheduleListContent = contentGO.transform;
            scrollRect.content = contentGO.GetComponent<RectTransform>();

            var contentVGroup = contentGO.AddComponent<VerticalLayoutGroup>();
            contentVGroup.childAlignment = TextAnchor.UpperCenter;
            contentVGroup.childForceExpandHeight = false;
            contentVGroup.childForceExpandWidth = true;
            contentVGroup.padding = new RectOffset(10, 10, 10, 10);
            contentVGroup.spacing = 12;

            // Add Schedule Action Button
            var addBtn = CreateButton(mod1SubViewSchedule.transform, "+ Tambah Jadwal Belajar Baru", HexToColor("#FF7A00"), Color.white, 64);
            addBtn.onClick.AddListener(() =>
            {
                if (addScheduleModal != null) addScheduleModal.SetActive(true);
            });

            // Notification Banner Info
            var banner = CreateUIObject("NotifBanner", mod1SubViewSchedule.transform);
            banner.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 68);
            var bannerImg = banner.AddComponent<Image>();
            bannerImg.sprite = cardSprite;
            bannerImg.type = Image.Type.Sliced;
            bannerImg.color = HexToColor("#D1FAE5");
            var bannerTxt = CreateText("BannerTxt", banner.transform, "🔔 Pengingat Lokal 100% Offline: Anak cukup tekan 1 tombol saat jam belajar tiba.", 19, FontStyle.Bold, HexToColor("#065F46"));
            SetStretchFull(bannerTxt.GetComponent<RectTransform>());
            bannerTxt.alignment = TextAnchor.MiddleCenter;

            // =========================================================
            // SUB-VIEW 2: DASHBOARD PEMANTAUAN AKTIVITAS ANAK
            // =========================================================
            mod1SubViewMonitoring = CreateUIObject("SubView_Monitoring", panel.transform);
            mod1SubViewMonitoring.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 820);
            var monV = mod1SubViewMonitoring.AddComponent<VerticalLayoutGroup>();
            monV.childAlignment = TextAnchor.UpperCenter;
            monV.childForceExpandHeight = false;
            monV.childForceExpandWidth = true;
            monV.spacing = 14;

            // Ringkasan Progres Anak Card
            var summaryCard = CreateUIObject("ParentSummaryCard", mod1SubViewMonitoring.transform);
            summaryCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 210);
            var sumBg = summaryCard.AddComponent<Image>();
            sumBg.sprite = cardSprite;
            sumBg.type = Image.Type.Sliced;
            sumBg.color = HexToColor("#FFF3E0");
            var sumV = summaryCard.AddComponent<VerticalLayoutGroup>();
            sumV.padding = new RectOffset(20, 20, 14, 14);
            sumV.spacing = 6;

            CreateText("SumHeader", summaryCard.transform, "📊 Ringkasan Pemantauan Aktivitas Anak Hari Ini", 22, FontStyle.Bold, HexToColor("#D95D00"));
            parentMonitorSummaryText = CreateText("SumBody", summaryCard.transform, "Memuat data aktivitas anak...", 20, FontStyle.Normal, HexToColor("#291804"));
            parentMonitorSummaryText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 130);

            // Tombol Ganti Profil Siswa / Siswi SD dari Dashboard Orang Tua
            var switchStudentBtn = CreateButton(mod1SubViewMonitoring.transform, "👦/👧 Ganti Profil Anak Aktif (Siswa SD Raka ⇄ Siswi SD Rani)", HexToColor("#E0F2FE"), HexToColor("#0369A1"), 54);
            switchStudentBtn.onClick.AddListener(() =>
            {
                ToggleStudentGenderProfile();
                RefreshParentMonitoringDashboard();
            });

            var logHeader = CreateText("LogHeader", mod1SubViewMonitoring.transform, "🕒 Riwayat Aktivitas Anak (Real-Time Offline Log):", 22, FontStyle.Bold, HexToColor("#291804"));
            logHeader.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 32);

            var logScrollGO = CreateUIObject("ActivityLogScroll", mod1SubViewMonitoring.transform);
            logScrollGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 460);
            var logScrollImg = logScrollGO.AddComponent<Image>();
            logScrollImg.sprite = cardSprite;
            logScrollImg.type = Image.Type.Sliced;
            logScrollImg.color = new Color(1f, 1f, 1f, 0.7f);
            var logScroll = logScrollGO.AddComponent<ScrollRect>();
            logScroll.horizontal = false;
            logScroll.vertical = true;

            var logContentGO = CreateUIObject("ActivityLogContent", logScrollGO.transform);
            SetStretchFull(logContentGO.GetComponent<RectTransform>());
            parentActivityLogContainer = logContentGO.transform;
            logScroll.content = logContentGO.GetComponent<RectTransform>();

            var logV = logContentGO.AddComponent<VerticalLayoutGroup>();
            logV.childAlignment = TextAnchor.UpperCenter;
            logV.childForceExpandHeight = false;
            logV.childForceExpandWidth = true;
            logV.padding = new RectOffset(12, 12, 10, 10);
            logV.spacing = 10;

            mod1SubViewMonitoring.SetActive(false);

            btnTabJadwal.onClick.AddListener(() =>
            {
                mod1SubViewSchedule.SetActive(true);
                mod1SubViewMonitoring.SetActive(false);
                btnTabJadwal.GetComponent<Image>().color = HexToColor("#FF7A00");
                btnTabJadwal.GetComponentInChildren<Text>().color = Color.white;
                btnTabMonitor.GetComponent<Image>().color = HexToColor("#FFF3E0");
                btnTabMonitor.GetComponentInChildren<Text>().color = HexToColor("#744E23");
            });

            btnTabMonitor.onClick.AddListener(() =>
            {
                mod1SubViewSchedule.SetActive(false);
                mod1SubViewMonitoring.SetActive(true);
                RefreshParentMonitoringDashboard();
                btnTabMonitor.GetComponent<Image>().color = HexToColor("#FF7A00");
                btnTabMonitor.GetComponentInChildren<Text>().color = Color.white;
                btnTabJadwal.GetComponent<Image>().color = HexToColor("#FFF3E0");
                btnTabJadwal.GetComponentInChildren<Text>().color = HexToColor("#744E23");
            });

            // Build Add Schedule Modal Dialog
            BuildAddScheduleModal(panel.transform);

            // Setup Controller
            if (scheduleController != null)
            {
                scheduleController.SetupComponents(inputAddJam, dropdownAddMapel, inputAddJudul, inputAddGuru, scheduleListContent);
                scheduleController.OnTimelineUpdated += RefreshScheduleCardsList;
            }

            HighlightSelectedDay(1); // Default Senin
            RefreshScheduleCardsList();
            RefreshParentMonitoringDashboard();

            return panel;
        }

        private void RefreshParentMonitoringDashboard()
        {
            if (dataManager == null || dataManager.Data == null) return;
            var db = dataManager.Data;

            int unlocked = db.daftarKartu.FindAll(k => k.terbuka).Count;
            int skorMat = dataManager.GetPersentaseNilaiMapel(SubjectType.Matematika);
            int skorIndo = dataManager.GetPersentaseNilaiMapel(SubjectType.BahasaIndonesia);
            int skorIng = dataManager.GetPersentaseNilaiMapel(SubjectType.BahasaInggris);

            if (parentMonitorSummaryText != null)
            {
                parentMonitorSummaryText.text =
                    $"• Profil Anak: {db.namaSiswa} ({db.peranSiswa})  |  Streak: 🔥 {db.streakHari} Hari\n" +
                    $"• Rekaman & Ringkasan Guru: {db.daftarMateri.Count} Sesi  |  Kartu 3D AR Terbuka: {unlocked}/{db.daftarKartu.Count} Kartu\n" +
                    $"• Kebun 3D: 🌱 Level {db.levelKebun}/7 ({db.xpKebun} XP)  |  Rata-Rata Kuis: Mat {skorMat}% • B.Ind {skorIndo}% • B.Ing {skorIng}%";
            }

            PopulateActivityLogContainer(parentActivityLogContainer);
            PopulateActivityLogContainer(quizActivityLogContainer);
        }

        private void PopulateActivityLogContainer(Transform container)
        {
            if (container == null || dataManager == null || dataManager.Data == null) return;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }

            var logs = dataManager.Data.riwayatAktivitas;
            if (logs == null || logs.Count == 0)
            {
                var empty = CreateText("EmptyLog", container, "Belum ada riwayat aktivitas tercatat.", 20, FontStyle.Italic, HexToColor("#744E23"));
                empty.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
                return;
            }

            int maxCount = Mathf.Min(logs.Count, 8);
            for (int i = 0; i < maxCount; i++)
            {
                var item = logs[i];
                var row = CreateUIObject("LogItem", container);
                row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 74);
                var rBg = row.AddComponent<Image>();
                rBg.sprite = cardSprite;
                rBg.type = Image.Type.Sliced;
                rBg.color = HexToColor("#FFFFFF");

                var rV = row.AddComponent<VerticalLayoutGroup>();
                rV.padding = new RectOffset(16, 16, 8, 8);
                rV.spacing = 2;

                CreateText("Top", row.transform, $"{item.ikon} [{item.kategori}] • {item.waktu}", 18, FontStyle.Bold, HexToColor("#D95D00"));
                CreateText("Desc", row.transform, item.deskripsi, 19, FontStyle.Normal, HexToColor("#291804"));
            }
        }

        private void HighlightSelectedDay(int selectedIndex)
        {
            if (daySelectorButtons == null) return;
            for (int i = 0; i < daySelectorButtons.Length; i++)
            {
                var img = daySelectorButtons[i].GetComponent<Image>();
                var txt = daySelectorButtons[i].GetComponentInChildren<Text>();
                if (i == selectedIndex)
                {
                    img.color = HexToColor("#FF7A00");
                    txt.color = Color.white;
                }
                else
                {
                    img.color = HexToColor("#FFF3E0");
                    txt.color = HexToColor("#744E23");
                }
            }
        }

        private void RefreshScheduleCardsList()
        {
            if (scheduleListContent == null || scheduleController == null) return;

            foreach (Transform child in scheduleListContent)
            {
                Destroy(child.gameObject);
            }

            var list = scheduleController.GetSchedulesForSelectedDay();
            if (list.Count == 0)
            {
                var emptyTxt = CreateText("EmptyTxt", scheduleListContent, "Belum ada jadwal untuk hari ini. Tekan tombol + Tambah Jadwal Baru.", 22, FontStyle.Italic, HexToColor("#A37A4C"));
                emptyTxt.alignment = TextAnchor.MiddleCenter;
                emptyTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100);
                return;
            }

            foreach (var jadwal in list)
            {
                CreateScheduleCardItem(scheduleListContent, jadwal);
            }
        }

        private void CreateScheduleCardItem(Transform parent, Jadwal jadwal)
        {
            var cardGO = CreateUIObject("ScheduleCardItem", parent);
            cardGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 140);
            var cardBg = cardGO.AddComponent<Image>();
            cardBg.sprite = cardSprite;
            cardBg.type = Image.Type.Sliced;
            cardBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(cardBg);

            var shadow = cardGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.06f);
            shadow.effectDistance = new Vector2(0, -3);

            var hGroup = cardGO.AddComponent<HorizontalLayoutGroup>();
            hGroup.childAlignment = TextAnchor.MiddleCenter;
            hGroup.childForceExpandHeight = false;
            hGroup.childForceExpandWidth = false;
            hGroup.padding = new RectOffset(18, 18, 12, 12);
            hGroup.spacing = 16;

            // Time & Subject Column
            var leftCol = CreateUIObject("LeftCol", cardGO.transform);
            leftCol.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 110);
            var leftV = leftCol.AddComponent<VerticalLayoutGroup>();
            leftV.childForceExpandHeight = false;
            leftV.childForceExpandWidth = true;

            var timeTxt = CreateText("TimeTxt", leftCol.transform, $"⏰ {jadwal.jam} WIB", 22, FontStyle.Bold, HexToColor("#FF7A00"));
            ThemeTokens.GetSubjectPillColors(jadwal.mataPelajaran, out Color pillBg, out Color pillText);
            var mapelTxt = CreateText("MapelTxt", leftCol.transform, ThemeTokens.GetSubjectDisplayName(jadwal.mataPelajaran), 20, FontStyle.Bold, pillText);

            // Title & Teacher Column
            var midCol = CreateUIObject("MidCol", cardGO.transform);
            midCol.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 110);
            var midV = midCol.AddComponent<VerticalLayoutGroup>();
            midV.childForceExpandHeight = false;
            midV.childForceExpandWidth = true;

            var titleTxt = CreateText("TitleTxt", midCol.transform, jadwal.judul, 22, FontStyle.Bold, HexToColor("#291804"));
            var guruTxt = CreateText("GuruTxt", midCol.transform, $"Guru: {jadwal.namaGuru}", 20, FontStyle.Normal, HexToColor("#744E23"));

            // Test Reminder Trigger Button
            var actionCol = CreateUIObject("ActionCol", cardGO.transform);
            actionCol.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 110);
            var actionV = actionCol.AddComponent<VerticalLayoutGroup>();
            actionV.childForceExpandHeight = false;
            actionV.childForceExpandWidth = true;

            var testBtn = CreateButton(actionCol.transform, "▶️ Rekam", HexToColor("#D1FAE5"), HexToColor("#059669"), 48);
            testBtn.onClick.AddListener(() =>
            {
                scheduleController?.TriggerClassReminderNow(jadwal);
            });

            var delBtn = CreateButton(actionCol.transform, "🗑️ Hapus", HexToColor("#FEE2E2"), HexToColor("#DC2626"), 48);
            delBtn.onClick.AddListener(() =>
            {
                dataManager?.HapusJadwal(jadwal.id);
                RefreshScheduleCardsList();
            });
        }

        private void BuildAddScheduleModal(Transform parent)
        {
            addScheduleModal = CreateUIObject("AddScheduleModal", parent);
            var rect = addScheduleModal.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(900, 520);
            var bg = addScheduleModal.AddComponent<Image>();
            bg.sprite = cardSprite;
            bg.type = Image.Type.Sliced;
            bg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(bg);

            var shadow = addScheduleModal.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.20f);
            shadow.effectDistance = new Vector2(0, -8);

            var vGroup = addScheduleModal.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(28, 28, 22, 22);
            vGroup.spacing = 16;

            var title = CreateText("Title", addScheduleModal.transform, "Tambah Jadwal Belajar Baru", 28, FontStyle.Bold, HexToColor("#291804"));

            inputAddJam = CreateInputField(addScheduleModal.transform, "Jam Pengingat (contoh: 08:00)", 22).GetComponent<InputField>();

            // Mapel Dropdown
            var dropGO = CreateUIObject("MapelDropdown", addScheduleModal.transform);
            dropGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            var dropImg = dropGO.AddComponent<Image>();
            dropImg.sprite = cardSprite;
            dropImg.type = Image.Type.Sliced;
            dropImg.color = HexToColor("#FFF3E0");
            dropdownAddMapel = dropGO.AddComponent<Dropdown>();
            dropdownAddMapel.options = new List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("Matematika"),
                new Dropdown.OptionData("Bahasa Indonesia"),
                new Dropdown.OptionData("Bahasa Inggris")
            };

            inputAddJudul = CreateInputField(addScheduleModal.transform, "Judul Pembelajaran (contoh: Penjumlahan Apel)", 22).GetComponent<InputField>();
            inputAddGuru = CreateInputField(addScheduleModal.transform, "Nama Guru (contoh: Bu Sari)", 22).GetComponent<InputField>();

            var btnRow = CreateUIObject("BtnRow", addScheduleModal.transform);
            btnRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 64);
            var hRow = btnRow.AddComponent<HorizontalLayoutGroup>();
            hRow.childForceExpandWidth = true;
            hRow.spacing = 20;

            var cancelBtn = CreateButton(btnRow.transform, "Batal", HexToColor("#FEE2E2"), HexToColor("#DC2626"), 56);
            cancelBtn.onClick.AddListener(() => addScheduleModal.SetActive(false));

            var saveBtn = CreateButton(btnRow.transform, "Simpan Jadwal", HexToColor("#FF7A00"), Color.white, 56);
            saveBtn.onClick.AddListener(() =>
            {
                scheduleController?.SimpanJadwalBaru();
                addScheduleModal.SetActive(false);
            });

            addScheduleModal.SetActive(false);
        }
        #endregion

        #region Modul 2: Belajar & Rekam (STT + Ringkasan Pintar Guru)
        private GameObject BuildModul2STT(Transform parent)
        {
            var panel = CreateUIObject("Module2_STT", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            var vGroup = panel.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(18, 18, 10, 10);
            vGroup.spacing = 12;

            // Subject Switcher Row so Student/Teacher can record & summarize any subject
            var mapelRow = CreateUIObject("SubjectSelectRow", panel.transform);
            mapelRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 50);
            var mapelH = mapelRow.AddComponent<HorizontalLayoutGroup>();
            mapelH.childForceExpandWidth = true;
            mapelH.spacing = 10;

            var btnMat = CreateButton(mapelRow.transform, "📐 Matematika", HexToColor("#FFE4E6"), HexToColor("#9F1239"), 48);
            var btnInd = CreateButton(mapelRow.transform, "📖 B. Indonesia", HexToColor("#D1FAE5"), HexToColor("#065F46"), 48);
            var btnIng = CreateButton(mapelRow.transform, "🌍 B. Inggris", HexToColor("#FEF3C7"), HexToColor("#92400E"), 48);

            btnMat.onClick.AddListener(() => sttController?.SelectSubjectForRecording(SubjectType.Matematika));
            btnInd.onClick.AddListener(() => sttController?.SelectSubjectForRecording(SubjectType.BahasaIndonesia));
            btnIng.onClick.AddListener(() => sttController?.SelectSubjectForRecording(SubjectType.BahasaInggris));

            // Active Class Header Card
            var headerCard = CreateUIObject("ActiveClassCard", panel.transform);
            headerCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 112);
            var cardBg = headerCard.AddComponent<Image>();
            cardBg.sprite = cardSprite;
            cardBg.type = Image.Type.Sliced;
            cardBg.color = HexToColor("#FFF3E0");

            var cardV = headerCard.AddComponent<VerticalLayoutGroup>();
            cardV.childAlignment = TextAnchor.MiddleLeft;
            cardV.childForceExpandHeight = false;
            cardV.childForceExpandWidth = true;
            cardV.padding = new RectOffset(18, 18, 10, 10);
            cardV.spacing = 4;

            sttActiveMapelText = CreateText("MapelPill", headerCard.transform, "Matematika", 19, FontStyle.Bold, HexToColor("#9F1239"));
            sttActiveClassTitleText = CreateText("ClassTitle", headerCard.transform, "Penjumlahan & Pengurangan 1–20", 24, FontStyle.Bold, HexToColor("#291804"));
            sttActiveTeacherText = CreateText("TeacherName", headerCard.transform, "Guru: Bu Sari • 🎙️ STT Offline Siap", 18, FontStyle.Normal, HexToColor("#744E23"));

            // One-Tap Circular Record Button Stage
            var recordStage = CreateUIObject("RecordStage", panel.transform);
            recordStage.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 184);

            var stageV = recordStage.AddComponent<VerticalLayoutGroup>();
            stageV.childAlignment = TextAnchor.MiddleCenter;
            stageV.childForceExpandHeight = false;
            stageV.childForceExpandWidth = false;
            stageV.spacing = 8;

            var roundBtnGO = CreateUIObject("OneTapRecordBtn", recordStage.transform);
            var roundRect = roundBtnGO.GetComponent<RectTransform>();
            roundRect.sizeDelta = new Vector2(132, 132);
            var roundImg = roundBtnGO.AddComponent<Image>();
            roundImg.sprite = circleSprite;
            roundImg.color = HexToColor("#FFB703");

            var btnShadow = roundBtnGO.AddComponent<Shadow>();
            btnShadow.effectColor = new Color(0, 0, 0, 0.16f);
            btnShadow.effectDistance = new Vector2(0, -5);

            sttRecordButton = roundBtnGO.AddComponent<Button>();
            sttRecordButton.targetGraphic = roundImg;

            var micTxt = CreateText("MicIcon", roundBtnGO.transform, "🎙️", 56, FontStyle.Normal, Color.white);
            SetStretchFull(micTxt.GetComponent<RectTransform>());
            micTxt.alignment = TextAnchor.MiddleCenter;

            sttRecordButtonLabel = CreateText("RecordLabel", recordStage.transform, "🎙️ KETUK UNTUK REKAM SUARA GURU", 21, FontStyle.Bold, HexToColor("#FF7A00"));
            sttRecordButtonLabel.alignment = TextAnchor.MiddleCenter;

            // Card 1: Live Speech-to-Text Transcript Card
            var transCard = CreateUIObject("LiveTranscriptCard", panel.transform);
            transCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 175);
            var transBg = transCard.AddComponent<Image>();
            transBg.sprite = cardSprite;
            transBg.type = Image.Type.Sliced;
            transBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(transBg);

            var transV = transCard.AddComponent<VerticalLayoutGroup>();
            transV.childAlignment = TextAnchor.UpperLeft;
            transV.childForceExpandHeight = false;
            transV.childForceExpandWidth = true;
            transV.padding = new RectOffset(18, 18, 12, 12);
            transV.spacing = 6;

            CreateText("TransTitle", transCard.transform, "📝 Transkripsi Suara Guru (Speech-to-Text Offline):", 20, FontStyle.Bold, HexToColor("#FF7A00"));
            sttLiveTranscriptText = CreateText("TransContent", transCard.transform, "Selamat pagi anak-anak. Hari ini kita belajar menjumlahkan buah apel. Jika ada 4 apel merah ditambah 3 apel lagi, semuanya menjadi 7 apel merah.", 20, FontStyle.Normal, HexToColor("#291804"));
            sttLiveTranscriptText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 110);

            // Card 2: Ringkasan Pintar Penjelasan Guru (Khusus Siswa/Siswi Tuli SD)
            var sumCard = CreateUIObject("TeacherSummaryCard", panel.transform);
            sumCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 215);
            var sumBg = sumCard.AddComponent<Image>();
            sumBg.sprite = cardSprite;
            sumBg.type = Image.Type.Sliced;
            sumBg.color = HexToColor("#F0FDF4");

            var sumV = sumCard.AddComponent<VerticalLayoutGroup>();
            sumV.childAlignment = TextAnchor.UpperLeft;
            sumV.childForceExpandHeight = false;
            sumV.childForceExpandWidth = true;
            sumV.padding = new RectOffset(18, 18, 12, 12);
            sumV.spacing = 6;

            CreateText("SumTitle", sumCard.transform, "✨ Ringkasan Pintar Penjelasan Guru (Poin Inti Visual):", 20, FontStyle.Bold, HexToColor("#059669"));
            sttTeacherSummaryText = CreateText("SumContent", sumCard.transform, LocalDataManager.GenerateRingkasanGuru(SubjectType.Matematika, ""), 19, FontStyle.Normal, HexToColor("#064E3B"));
            sttTeacherSummaryText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 108);

            // Visual Formula Box inside Summary Card
            var formulaPill = CreateUIObject("FormulaPill", sumCard.transform);
            formulaPill.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 44);
            var fBg = formulaPill.AddComponent<Image>();
            fBg.sprite = pillSprite;
            fBg.type = Image.Type.Sliced;
            fBg.color = HexToColor("#D1FAE5");

            sttVisualFormulaText = CreateText("FormulaTxt", formulaPill.transform, LocalDataManager.GetRumusVisual(SubjectType.Matematika), 20, FontStyle.Bold, HexToColor("#065F46"));
            SetStretchFull(sttVisualFormulaText.GetComponent<RectTransform>());
            sttVisualFormulaText.alignment = TextAnchor.MiddleCenter;

            // Proceed to Mini Quiz Button
            var quizBtn = CreateButton(panel.transform, "Lanjut Kerjakan Kuis & Unlock Kartu AR ➡️", HexToColor("#10B981"), Color.white, 64);

            if (sttController != null)
            {
                sttController.SetupComponents(
                    sttActiveMapelText,
                    sttActiveClassTitleText,
                    sttActiveTeacherText,
                    sttRecordButton,
                    sttRecordButtonLabel,
                    sttLiveTranscriptText,
                    quizBtn,
                    sttTeacherSummaryText,
                    sttVisualFormulaText
                );
            }

            return panel;
        }
        #endregion

        #region Modul 3: Kuis & Dashboard Pemantauan Aktivitas Anak
        private GameObject BuildModul3Quiz(Transform parent)
        {
            var panel = CreateUIObject("Module3_Quiz", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            var vGroup = panel.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(18, 18, 12, 12);
            vGroup.spacing = 12;

            // Sub Tab Buttons (Kuis Mini vs Dashboard Pemantauan Anak)
            var tabRow = CreateUIObject("TabRow", panel.transform);
            tabRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 56);
            var hTab = tabRow.AddComponent<HorizontalLayoutGroup>();
            hTab.childForceExpandWidth = true;
            hTab.spacing = 14;

            var btnTabQuiz = CreateButton(tabRow.transform, "📝 Kuis (Unlock Kartu AR)", HexToColor("#FF7A00"), Color.white, 54);
            var btnTabBento = CreateButton(tabRow.transform, "📊 Dashboard Aktivitas Anak", HexToColor("#FFF3E0"), HexToColor("#744E23"), 54);

            // View 1: Kuis Mini
            quizSubViewQuiz = CreateUIObject("SubView_Quiz", panel.transform);
            quizSubViewQuiz.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 820);
            var qV = quizSubViewQuiz.AddComponent<VerticalLayoutGroup>();
            qV.childAlignment = TextAnchor.UpperCenter;
            qV.childForceExpandHeight = false;
            qV.childForceExpandWidth = true;
            qV.spacing = 12;

            // Subject Selector Row so Student can unlock each Subject's 3D Fruit Card!
            var subjRow = CreateUIObject("QuizSubjectSelector", quizSubViewQuiz.transform);
            subjRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 52);
            var subjH = subjRow.AddComponent<HorizontalLayoutGroup>();
            subjH.childForceExpandWidth = true;
            subjH.spacing = 10;

            var qMatBtn = CreateButton(subjRow.transform, "📐 Matematika (🍎/🍌)", HexToColor("#FFE4E6"), HexToColor("#9F1239"), 48);
            var qIndBtn = CreateButton(subjRow.transform, "📖 B.Indo (🍊 Jeruk)", HexToColor("#D1FAE5"), HexToColor("#065F46"), 48);
            var qIngBtn = CreateButton(subjRow.transform, "🌍 B.Inggris (🥭 Mangga)", HexToColor("#FEF3C7"), HexToColor("#92400E"), 48);

            qMatBtn.onClick.AddListener(() =>
            {
                quizResultCard.SetActive(false);
                quizController?.StartQuizForSubject(SubjectType.Matematika);
            });
            qIndBtn.onClick.AddListener(() =>
            {
                quizResultCard.SetActive(false);
                quizController?.StartQuizForSubject(SubjectType.BahasaIndonesia);
            });
            qIngBtn.onClick.AddListener(() =>
            {
                quizResultCard.SetActive(false);
                quizController?.StartQuizForSubject(SubjectType.BahasaInggris);
            });

            // Progress Header
            var progressRow = CreateUIObject("ProgressRow", quizSubViewQuiz.transform);
            progressRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 42);
            var progH = progressRow.AddComponent<HorizontalLayoutGroup>();
            progH.childForceExpandWidth = false;
            progH.spacing = 20;

            quizSubjectBadgeText = CreateText("SubjectBadge", progressRow.transform, "Matematika", 22, FontStyle.Bold, HexToColor("#FF7A00"));
            quizSubjectBadgeText.GetComponent<RectTransform>().sizeDelta = new Vector2(340, 42);

            quizQuestionProgressText = CreateText("ProgressTxt", progressRow.transform, "50%", 20, FontStyle.Bold, HexToColor("#744E23"));
            quizQuestionProgressText.GetComponent<RectTransform>().sizeDelta = new Vector2(600, 42);
            quizQuestionProgressText.alignment = TextAnchor.MiddleRight;

            var sliderGO = CreateUIObject("ProgressBar", quizSubViewQuiz.transform);
            sliderGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 20);
            quizProgressBar = sliderGO.AddComponent<Slider>();
            var sliderBg = sliderGO.AddComponent<Image>();
            sliderBg.sprite = pillSprite;
            sliderBg.type = Image.Type.Sliced;
            sliderBg.color = HexToColor("#FFE8D6");

            // Question Card
            var qCard = CreateUIObject("QuestionCard", quizSubViewQuiz.transform);
            qCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 155);
            var qCardBg = qCard.AddComponent<Image>();
            qCardBg.sprite = cardSprite;
            qCardBg.type = Image.Type.Sliced;
            qCardBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(qCardBg);

            var qShadow = qCard.AddComponent<Shadow>();
            qShadow.effectColor = new Color(0, 0, 0, 0.06f);
            qShadow.effectDistance = new Vector2(0, -4);

            quizQuestionContentText = CreateText("QuestionContent", qCard.transform, "Raka punya 4 apel merah, lalu Ibu memberi 3 apel lagi. Berapa total semuanya?", 24, FontStyle.Bold, HexToColor("#291804"));
            SetStretchFull(quizQuestionContentText.GetComponent<RectTransform>());
            quizQuestionContentText.alignment = TextAnchor.MiddleCenter;

            // 3 Option Buttons
            quizOptionButtons = new Button[3];
            quizOptionTexts = new Text[3];
            string[] defaultOptions = { "6 Apel", "7 Apel", "8 Apel" };

            for (int i = 0; i < 3; i++)
            {
                var optBtn = CreateButton(quizSubViewQuiz.transform, defaultOptions[i], HexToColor("#FFFDF9"), HexToColor("#291804"), 68);
                quizOptionButtons[i] = optBtn;
                quizOptionTexts[i] = optBtn.GetComponentInChildren<Text>();
                quizOptionTexts[i].fontSize = 23;
            }

            // End Quiz Result & Card Unlock Box
            quizResultCard = CreateUIObject("QuizResultCard", quizSubViewQuiz.transform);
            quizResultCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 255);
            var resBg = quizResultCard.AddComponent<Image>();
            resBg.sprite = cardSprite;
            resBg.type = Image.Type.Sliced;
            resBg.color = HexToColor("#D1FAE5");

            var resV = quizResultCard.AddComponent<VerticalLayoutGroup>();
            resV.childAlignment = TextAnchor.MiddleCenter;
            resV.childForceExpandHeight = false;
            resV.childForceExpandWidth = true;
            resV.padding = new RectOffset(18, 18, 12, 12);
            resV.spacing = 8;

            quizResultScoreText = CreateText("ResScore", quizResultCard.transform, "🎉 NILAI: 100% (LULUS & UNLOCK KARTU AR!)", 24, FontStyle.Bold, HexToColor("#065F46"));
            quizResultScoreText.alignment = TextAnchor.MiddleCenter;

            quizResultDescText = CreateText("ResDesc", quizResultCard.transform, "Hebat sekali! Kamu membuka Kartu Model 3D Baru untuk AR Buah & +50 XP Kebun 3D!", 20, FontStyle.Normal, HexToColor("#047857"));
            quizResultDescText.alignment = TextAnchor.MiddleCenter;

            var resBtnRow = CreateUIObject("ResBtnRow", quizResultCard.transform);
            resBtnRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 58);
            var resBtnH = resBtnRow.AddComponent<HorizontalLayoutGroup>();
            resBtnH.childForceExpandWidth = true;
            resBtnH.spacing = 12;

            quizResultDirectArBtn = CreateButton(resBtnRow.transform, "🔍 Langsung Buka AR 3D Buah!", HexToColor("#059669"), Color.white, 56);
            quizResultActionBtn = CreateButton(resBtnRow.transform, "🎴 Lihat Galeri Kartu", HexToColor("#FFFFFF"), HexToColor("#065F46"), 56);

            quizResultCard.SetActive(false);

            // View 2: Dashboard Pemantauan Nilai & Aktivitas Anak
            quizSubViewDashboard = CreateUIObject("SubView_Dashboard", panel.transform);
            quizSubViewDashboard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 820);
            var bentoV = quizSubViewDashboard.AddComponent<VerticalLayoutGroup>();
            bentoV.childAlignment = TextAnchor.UpperCenter;
            bentoV.childForceExpandHeight = false;
            bentoV.childForceExpandWidth = true;
            bentoV.spacing = 12;

            var bentoRow1 = CreateUIObject("BentoRow1", quizSubViewDashboard.transform);
            bentoRow1.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);
            var bRow1 = bentoRow1.AddComponent<HorizontalLayoutGroup>();
            bRow1.childForceExpandWidth = true;
            bRow1.spacing = 14;

            var cardMateri = CreateBentoMetricCard(bentoRow1.transform, "📖 Rekaman Guru", "1 Materi", HexToColor("#FFE8D6"));
            var materiTexts = cardMateri.GetComponentsInChildren<Text>();
            bentoTotalMateriText = materiTexts.Length > 1 ? materiTexts[1] : materiTexts[0];

            var cardKartu = CreateBentoMetricCard(bentoRow1.transform, "🎴 Kartu 3D AR", "1 / 4 Terbuka", HexToColor("#D1FAE5"));
            var kartuTexts = cardKartu.GetComponentsInChildren<Text>();
            bentoKartUnlockedText = kartuTexts.Length > 1 ? kartuTexts[1] : kartuTexts[0];

            // Subject Score Bars
            BuildSubjectMeterRow(quizSubViewDashboard.transform, "Matematika (Kartu Apel & Pisang)", 0.85f, HexToColor("#FFE4E6"), HexToColor("#9F1239"));
            BuildSubjectMeterRow(quizSubViewDashboard.transform, "Bahasa Indonesia (Kartu Jeruk)", 0.80f, HexToColor("#D1FAE5"), HexToColor("#065F46"));
            BuildSubjectMeterRow(quizSubViewDashboard.transform, "Bahasa Inggris (Kartu Mangga)", 0.75f, HexToColor("#FEF3C7"), HexToColor("#92400E"));

            // Activity Log inside Dashboard Tab
            CreateText("DashLogTitle", quizSubViewDashboard.transform, "🕒 Log Aktivitas Belajar Anak Terbaru:", 20, FontStyle.Bold, HexToColor("#291804"));
            var dashLogBox = CreateUIObject("DashLogBox", quizSubViewDashboard.transform);
            dashLogBox.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 290);
            quizActivityLogContainer = dashLogBox.transform;
            var dashLogV = dashLogBox.AddComponent<VerticalLayoutGroup>();
            dashLogV.childForceExpandHeight = false;
            dashLogV.childForceExpandWidth = true;
            dashLogV.spacing = 8;

            quizSubViewDashboard.SetActive(false);

            // Switch sub-tabs
            btnTabQuiz.onClick.AddListener(() =>
            {
                quizSubViewQuiz.SetActive(true);
                quizSubViewDashboard.SetActive(false);
                btnTabQuiz.GetComponent<Image>().color = HexToColor("#FF7A00");
                btnTabBento.GetComponent<Image>().color = HexToColor("#FFF3E0");
            });

            btnTabBento.onClick.AddListener(() =>
            {
                quizSubViewQuiz.SetActive(false);
                quizSubViewDashboard.SetActive(true);
                quizController?.RefreshDashboardStats();
                RefreshParentMonitoringDashboard();
                btnTabQuiz.GetComponent<Image>().color = HexToColor("#FFF3E0");
                btnTabBento.GetComponent<Image>().color = HexToColor("#FF7A00");
            });

            // Wire Controller
            if (quizController != null)
            {
                quizController.SetupComponents(
                    bentoTotalMateriText,
                    bentoKartUnlockedText,
                    null, null, null,
                    quizProgressBar,
                    quizQuestionProgressText,
                    quizSubjectBadgeText,
                    quizQuestionContentText,
                    quizOptionButtons,
                    quizOptionTexts
                );

                quizController.OnQuizFinishedEvent += (score, passed, card) =>
                {
                    RefreshStudentProfileHeader();
                    RefreshCardGalleryGrid();
                    RefreshParentMonitoringDashboard();

                    quizResultCard.SetActive(true);
                    quizResultScoreText.text = $"NILAI KUIS: {score}% ({(passed ? "LULUS & UNLOCK KARTU 3D!" : "PERLU ULANG")})";
                    quizResultScoreText.color = passed ? HexToColor("#065F46") : HexToColor("#DC2626");
                    quizResultCard.GetComponent<Image>().color = passed ? HexToColor("#D1FAE5") : HexToColor("#FEE2E2");

                    if (passed)
                    {
                        string cName = card != null ? card.namaBuahId : "Kartu Buah 3D";
                        quizResultDescText.text = $"🎉 Hore! Kartu Model 3D \"{cName}\" telah TERBUKA untuk Kamera AR & Kebun 3D naik +50 XP!";

                        if (quizResultDirectArBtn != null)
                        {
                            quizResultDirectArBtn.gameObject.SetActive(true);
                            quizResultDirectArBtn.GetComponentInChildren<Text>().text = $"🔍 Buka AR {cName}";
                            quizResultDirectArBtn.onClick.RemoveAllListeners();
                            quizResultDirectArBtn.onClick.AddListener(() =>
                            {
                                if (card != null) navManager?.OpenARFruitWithCard(card);
                                else navManager?.NavigateTo(TellMeScreen.Module7_ARBuah);
                            });
                        }

                        quizResultActionBtn.GetComponentInChildren<Text>().text = "🎴 Galeri Kartu ➡️";
                        quizResultActionBtn.onClick.RemoveAllListeners();
                        quizResultActionBtn.onClick.AddListener(() => navManager?.NavigateTo(TellMeScreen.Module6_KartuDigital));
                    }
                    else
                    {
                        if (quizResultDirectArBtn != null) quizResultDirectArBtn.gameObject.SetActive(false);
                        quizResultDescText.text = "Tetap semangat! Nilai belum mencapai 75. Ayo baca ulang Ringkasan Guru lalu coba lagi.";
                        quizResultActionBtn.GetComponentInChildren<Text>().text = "📖 Baca Ringkasan Guru";
                        quizResultActionBtn.onClick.RemoveAllListeners();
                        quizResultActionBtn.onClick.AddListener(() => navManager?.NavigateTo(TellMeScreen.Module2_SpeechToTextBelajar));
                    }
                };
            }

            return panel;
        }

        private GameObject CreateBentoMetricCard(Transform parent, string title, string val, Color bg)
        {
            var card = CreateUIObject("BentoCard", parent);
            var img = card.AddComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = bg;

            var shadow = card.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -3);

            var v = card.AddComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.MiddleCenter;
            v.padding = new RectOffset(16, 16, 12, 12);
            CreateText("Title", card.transform, title, 20, FontStyle.Normal, HexToColor("#744E23"));
            CreateText("Val", card.transform, val, 26, FontStyle.Bold, HexToColor("#291804"));
            return card;
        }

        private void BuildSubjectMeterRow(Transform parent, string mapel, float progress, Color bg, Color fill)
        {
            var row = CreateUIObject("SubjectRow", parent);
            row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 78);
            var rImg = row.AddComponent<Image>();
            rImg.sprite = cardSprite;
            rImg.type = Image.Type.Sliced;
            rImg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(rImg);

            var shadow = row.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.05f);
            shadow.effectDistance = new Vector2(0, -3);

            var v = row.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(18, 18, 10, 10);
            v.spacing = 6;

            var topRow = CreateUIObject("Top", row.transform);
            topRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 28);
            var h = topRow.AddComponent<HorizontalLayoutGroup>();
            h.childForceExpandWidth = false;

            var nameTxt = CreateText("Name", topRow.transform, mapel, 20, FontStyle.Bold, fill);
            nameTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(540, 28);

            var pctTxt = CreateText("Pct", topRow.transform, $"{Mathf.RoundToInt(progress * 100f)}%", 20, FontStyle.Bold, HexToColor("#291804"));
            pctTxt.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 28);
            pctTxt.alignment = TextAnchor.MiddleRight;

            var barGO = CreateUIObject("ProgressBar", row.transform);
            barGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 18);
            var slider = barGO.AddComponent<Slider>();
            slider.value = progress;
            var bImg = barGO.AddComponent<Image>();
            bImg.sprite = pillSprite;
            bImg.type = Image.Type.Sliced;
            bImg.color = bg;
        }
        #endregion

        #region Modul 4: Kuis Bahasa Isyarat
        private GameObject BuildModul4SignQuiz(Transform parent)
        {
            var panel = CreateUIObject("Module4_SignQuiz", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            var vGroup = panel.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(20, 20, 16, 16);
            vGroup.spacing = 18;

            // System Badge Header
            signSystemText = CreateText("SignSystemBadge", panel.transform, "🤟 SIBI / BISINDO (Bahasa Isyarat)", 26, FontStyle.Bold, HexToColor("#FF7A00"));
            signSystemText.alignment = TextAnchor.MiddleCenter;

            // Animated Gesture Stage (Clay Card)
            var stageCard = CreateUIObject("SignStageCard", panel.transform);
            stageCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 280);
            var stageBg = stageCard.AddComponent<Image>();
            stageBg.sprite = cardSprite;
            stageBg.type = Image.Type.Sliced;
            stageBg.color = HexToColor("#FFF3E0");

            var shadow = stageCard.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.06f);
            shadow.effectDistance = new Vector2(0, -4);

            var stageV = stageCard.AddComponent<VerticalLayoutGroup>();
            stageV.childAlignment = TextAnchor.MiddleCenter;
            stageV.childForceExpandHeight = false;
            stageV.childForceExpandWidth = true;
            stageV.padding = new RectOffset(20, 20, 16, 16);
            stageV.spacing = 12;

            var handIcon = CreateText("HandVisual", stageCard.transform, "🖐️ ➡️ ✊ ➡️ 🔄", 54, FontStyle.Normal, HexToColor("#D97706"));
            handIcon.alignment = TextAnchor.MiddleCenter;

            signDescText = CreateText("SignDesc", stageCard.transform, "Kepalkan tangan kanan di pipi, putar perlahan 2x.", 24, FontStyle.Bold, HexToColor("#291804"));
            signDescText.alignment = TextAnchor.MiddleCenter;
            themeTextMainElements.Add(signDescText);

            var hintTxt = CreateText("Hint", stageCard.transform, "Pilih kata dan objek yang tepat sesuai gerakan tangan di atas:", 20, FontStyle.Normal, HexToColor("#744E23"));
            hintTxt.alignment = TextAnchor.MiddleCenter;

            // 4 Multiple Choice Option Cards (Grid 2x2)
            var gridGO = CreateUIObject("OptionsGrid", panel.transform);
            gridGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 240);
            var grid = gridGO.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(480, 100);
            grid.spacing = new Vector2(20, 20);

            signOptionButtons = new Button[4];
            signOptionTexts = new Text[4];
            string[] defaultSignLabels = { "🍎 Apel", "📘 Buku", "🏠 Rumah", "💧 Air" };

            for (int i = 0; i < 4; i++)
            {
                var btn = CreateButton(gridGO.transform, defaultSignLabels[i], HexToColor("#FFFFFF"), HexToColor("#291804"), 100);
                signOptionButtons[i] = btn;
                signOptionTexts[i] = btn.GetComponentInChildren<Text>();
                signOptionTexts[i].fontSize = 24;
                themeSurfaceElements.Add(btn.GetComponent<Image>());
            }

            signNextButton = CreateButton(panel.transform, "Soal Isyarat Berikutnya ➡️", HexToColor("#10B981"), Color.white, 68);
            signNextButton.onClick.AddListener(() =>
            {
                signController?.NextSignQuestion();
                ResetSignButtonStyles();
            });

            if (signController != null)
            {
                signController.SetupComponents(null, signDescText, signSystemText, signOptionTexts, signOptionButtons);
                signController.OnAnswerEvaluated += (correct, index) =>
                {
                    if (index >= 0 && index < signOptionButtons.Length)
                    {
                        signOptionButtons[index].GetComponent<Image>().color = correct ? HexToColor("#D1FAE5") : HexToColor("#FEE2E2");
                    }
                };
            }

            return panel;
        }

        private void ResetSignButtonStyles()
        {
            if (signOptionButtons == null) return;
            foreach (var btn in signOptionButtons)
            {
                btn.GetComponent<Image>().color = HexToColor("#FFFFFF");
            }
        }
        #endregion

        #region Modul 5: Beranda BerryBears
        private GameObject BuildModul5Home(Transform parent)
        {
            var panel = CreateUIObject("Module5_Home", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            var vGroup = panel.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(20, 20, 14, 14);
            vGroup.spacing = 16;

            // Welcome Card
            var welcomeCard = CreateUIObject("WelcomeCard", panel.transform);
            welcomeCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 134);
            var wBg = welcomeCard.AddComponent<Image>();
            wBg.sprite = cardSprite;
            wBg.type = Image.Type.Sliced;
            wBg.color = HexToColor("#FFF3E0");

            var shadow1 = welcomeCard.AddComponent<Shadow>();
            shadow1.effectColor = new Color(0, 0, 0, 0.05f);
            shadow1.effectDistance = new Vector2(0, -3);

            var wV = welcomeCard.AddComponent<VerticalLayoutGroup>();
            wV.padding = new RectOffset(20, 20, 12, 12);
            wV.spacing = 6;

            homeWelcomeTitleText = CreateText("Title", welcomeCard.transform, "Halo Raka (Siswa SD)! Selamat Belajar! 🐻", 26, FontStyle.Bold, HexToColor("#FF7A00"));
            CreateText("Streak", welcomeCard.transform, "🔥 3 Hari Berturut-turut! Selesaikan kuis untuk membuka Kartu Model 3D AR & menaikkan Level Kebun 3D!", 19, FontStyle.Normal, HexToColor("#744E23"));

            // Next Class Action Card
            var nextClassCard = CreateUIObject("NextClassCard", panel.transform);
            nextClassCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 176);
            var nBg = nextClassCard.AddComponent<Image>();
            nBg.sprite = cardSprite;
            nBg.type = Image.Type.Sliced;
            nBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(nBg);

            var shadow2 = nextClassCard.AddComponent<Shadow>();
            shadow2.effectColor = new Color(0, 0, 0, 0.06f);
            shadow2.effectDistance = new Vector2(0, -4);

            var nV = nextClassCard.AddComponent<VerticalLayoutGroup>();
            nV.padding = new RectOffset(20, 20, 12, 12);
            nV.spacing = 6;

            CreateText("Label", nextClassCard.transform, "⏰ Jadwal Belajar Berikutnya:", 19, FontStyle.Bold, HexToColor("#FF7A00"));
            homeNextClassTitle = CreateText("Title", nextClassCard.transform, "Matematika • Penjumlahan 1–20", 24, FontStyle.Bold, HexToColor("#291804"));
            homeNextClassTeacher = CreateText("Teacher", nextClassCard.transform, "Guru: Bu Sari (08:00 WIB) • Speech-to-Text & Ringkasan Guru", 19, FontStyle.Normal, HexToColor("#744E23"));

            var rekamBtn = CreateButton(nextClassCard.transform, "🎙️ REKAM & RINGKAS PENJELASAN GURU", HexToColor("#FF7A00"), Color.white, 58);
            rekamBtn.onClick.AddListener(() => navManager?.NavigateTo(TellMeScreen.Module2_SpeechToTextBelajar));

            // 4 Bento Navigation Tiles
            var bentoGrid = CreateUIObject("HomeBentoGrid", panel.transform);
            bentoGrid.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 216);
            var bLayout = bentoGrid.AddComponent<GridLayoutGroup>();
            bLayout.cellSize = new Vector2(480, 96);
            bLayout.spacing = new Vector2(20, 16);

            CreateHomeBentoBtn(bentoGrid.transform, "📝 Kuis & Unlock AR", HexToColor("#FFE4E6"), HexToColor("#9F1239"), TellMeScreen.Module3_KuisDanNilai);
            CreateHomeBentoBtn(bentoGrid.transform, "🤟 Bahasa Isyarat", HexToColor("#D1FAE5"), HexToColor("#065F46"), TellMeScreen.Module4_KuisBahasaIsyarat);
            CreateHomeBentoBtn(bentoGrid.transform, "🎴 Kartu 3D AR Buah", HexToColor("#FEF3C7"), HexToColor("#92400E"), TellMeScreen.Module6_KartuDigital);
            CreateHomeBentoBtn(bentoGrid.transform, "🌱 Kebun 3D (Lv.1-7)", HexToColor("#E0E7FF"), HexToColor("#3730A3"), TellMeScreen.Module8_TumbuhKembangTanaman);

            // Recent Materials Section with Teacher Summary Preview
            var mTitle = CreateText("MTitle", panel.transform, "📚 Ringkasan Materi Guru Terakhir:", 21, FontStyle.Bold, HexToColor("#291804"));
            mTitle.alignment = TextAnchor.MiddleLeft;

            var mCard = CreateUIObject("RecentMateriCard", panel.transform);
            mCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 130);
            var mBg = mCard.AddComponent<Image>();
            mBg.sprite = cardSprite;
            mBg.type = Image.Type.Sliced;
            mBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(mBg);

            var shadow3 = mCard.AddComponent<Shadow>();
            shadow3.effectColor = new Color(0, 0, 0, 0.05f);
            shadow3.effectDistance = new Vector2(0, -3);

            var mV = mCard.AddComponent<VerticalLayoutGroup>();
            mV.padding = new RectOffset(16, 16, 10, 10);
            mV.spacing = 4;
            CreateText("M1", mCard.transform, "Penjumlahan 1–20 (Matematika • Bu Sari)", 21, FontStyle.Bold, HexToColor("#291804"));
            CreateText("M2", mCard.transform, "✨ Ringkasan: 🍎🍎🍎🍎 (4) + 🍎🍎🍎 (3) = 7 Apel Merah", 19, FontStyle.Bold, HexToColor("#059669"));

            return panel;
        }

        private void CreateHomeBentoBtn(Transform parent, string label, Color bg, Color textCol, TellMeScreen screen)
        {
            var btn = CreateButton(parent, label, bg, textCol, 96);
            btn.onClick.AddListener(() => navManager?.NavigateTo(screen));
        }
        #endregion

        #region Modul 6: Kartu Digital (Unlock dari Kuis Sesuai Mapel)
        private GameObject BuildModul6Cards(Transform parent)
        {
            var panel = CreateUIObject("Module6_Cards", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            var vGroup = panel.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(20, 20, 12, 12);
            vGroup.spacing = 12;

            var title = CreateText("Title", panel.transform, "🎴 Koleksi Kartu Model 3D AR Buah", 26, FontStyle.Bold, HexToColor("#FF7A00"));
            var subtitle = CreateText("Sub", panel.transform, "Model AR Buah 3D hanya bisa dipakai jika kartunya sudah terbuka dari menyelesaikan kuis mapel terkait (Skor >= 75)!", 19, FontStyle.Normal, HexToColor("#744E23"));

            // 2-Column Responsive Card Grid
            var gridGO = CreateUIObject("CardsGrid", panel.transform);
            gridGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 650);
            var grid = gridGO.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(480, 305);
            grid.spacing = new Vector2(20, 20);
            cardsGridContainer = gridGO.transform;

            // Demo Unlock All Button for Jury Testing
            var demoUnlockBtn = CreateButton(panel.transform, "🔓 Buka Semua Kartu 3D (Tombol Demo Pengujian Juri)", HexToColor("#FEF3C7"), HexToColor("#92400E"), 52);
            demoUnlockBtn.onClick.AddListener(() =>
            {
                if (dataManager != null && dataManager.Data != null)
                {
                    foreach (var k in dataManager.Data.daftarKartu)
                    {
                        k.terbuka = true;
                        k.labelBaru = true;
                    }
                    dataManager.CatatAktivitas("🔓", "Kartu 3D AR", "Membuka seluruh Kartu Model 3D Buah untuk pengujian AR");
                    dataManager.SaveDatabase();
                    RefreshCardGalleryGrid();
                    mascotController?.SetMascotState(BerryBearsState.STATE_QUIZ_SUCCESS, "Semua 4 Kartu Model 3D Buah telah dibuka! Silakan pilih kartu mana saja untuk mencoba AR 5 Fase!");
                }
            });

            RefreshCardGalleryGrid();

            return panel;
        }

        private void RefreshCardGalleryGrid()
        {
            if (cardsGridContainer == null || dataManager == null) return;

            foreach (Transform child in cardsGridContainer)
            {
                Destroy(child.gameObject);
            }

            var cards = dataManager.Data.daftarKartu;
            foreach (var kartu in cards)
            {
                CreateFruitCardItem(cardsGridContainer, kartu);
            }
        }

        private void CreateFruitCardItem(Transform parent, Kartu kartu)
        {
            var cardGO = CreateUIObject("FruitCard", parent);
            var cardBg = cardGO.AddComponent<Image>();
            cardBg.sprite = cardSprite;
            cardBg.type = Image.Type.Sliced;
            cardBg.color = kartu.terbuka ? HexToColor("#FFFFFF") : HexToColor("#F1F5F9");
            if (kartu.terbuka) themeSurfaceElements.Add(cardBg);

            var shadow = cardGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.07f);
            shadow.effectDistance = new Vector2(0, -4);

            var v = cardGO.AddComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.MiddleCenter;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            v.padding = new RectOffset(14, 14, 12, 12);
            v.spacing = 6;

            string fruitIcon = kartu.id.Contains("APEL") ? "🍎" : kartu.id.Contains("JERUK") ? "🍊" : kartu.id.Contains("MANGGA") ? "🥭" : "🍌";
            string mapelName = ThemeTokens.GetSubjectDisplayName(kartu.mataPelajaran);

            if (kartu.terbuka)
            {
                var badge = CreateText("NewBadge", cardGO.transform, $"✅ TERBUKA • Kuis {mapelName}", 17, FontStyle.Bold, HexToColor("#059669"));
                badge.alignment = TextAnchor.MiddleCenter;

                var iconTxt = CreateText("Icon", cardGO.transform, fruitIcon, 52, FontStyle.Normal, Color.white);
                iconTxt.alignment = TextAnchor.MiddleCenter;

                var nameIdTxt = CreateText("NameId", cardGO.transform, kartu.namaBuahId, 23, FontStyle.Bold, HexToColor("#291804"));
                nameIdTxt.alignment = TextAnchor.MiddleCenter;

                var nameEnTxt = CreateText("NameEn", cardGO.transform, $"{kartu.namaBuahEn} (3D 5-Fase)", 18, FontStyle.Italic, HexToColor("#744E23"));
                nameEnTxt.alignment = TextAnchor.MiddleCenter;

                var arBtn = CreateButton(cardGO.transform, "🔍 Pakai Model AR 3D", HexToColor("#10B981"), Color.white, 50);
                arBtn.onClick.AddListener(() => cardController?.OnCardClicked(kartu));
            }
            else
            {
                var lockIcon = CreateText("LockIcon", cardGO.transform, $"🔒 {fruitIcon}", 46, FontStyle.Normal, HexToColor("#94A3B8"));
                lockIcon.alignment = TextAnchor.MiddleCenter;

                var lockName = CreateText("LockName", cardGO.transform, $"{kartu.namaBuahId} (Terkunci)", 21, FontStyle.Bold, HexToColor("#64748B"));
                lockName.alignment = TextAnchor.MiddleCenter;

                var lockHint = CreateText("LockHint", cardGO.transform, $"Selesaikan Kuis {mapelName} (Skor >= 75) untuk unlock kartu ini!", 17, FontStyle.Normal, HexToColor("#94A3B8"));
                lockHint.alignment = TextAnchor.MiddleCenter;

                SubjectType targetMapel = kartu.mataPelajaran;
                var quizJumpBtn = CreateButton(cardGO.transform, $"📝 Kuis {mapelName}", HexToColor("#FF7A00"), Color.white, 48);
                quizJumpBtn.onClick.AddListener(() =>
                {
                    navManager?.NavigateTo(TellMeScreen.Module3_KuisDanNilai);
                    quizController?.StartQuizForSubject(targetMapel);
                });
            }
        }
        #endregion

        #region Modul 7: AR Buah 3D dengan 5 Fase Interaktif Saat Diklik
        private GameObject BuildModul7AR(Transform parent)
        {
            var panel = CreateUIObject("Module7_AR", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            // 1. AR Camera Viewport (Supports Live Phone Camera via WebCamTexture + Simulated AR Grid)
            var cameraViewport = CreateUIObject("CameraViewport", panel.transform);
            SetStretchFull(cameraViewport.GetComponent<RectTransform>());
            var camBg = cameraViewport.AddComponent<Image>();
            camBg.color = HexToColor("#0F172A");

            // Live Camera Feed RawImage (activated when user taps Camera Live button on phone)
            var camFeedGO = CreateUIObject("LiveCameraFeed", cameraViewport.transform);
            SetStretchFull(camFeedGO.GetComponent<RectTransform>());
            arCameraFeedRawImage = camFeedGO.AddComponent<RawImage>();
            arCameraFeedRawImage.color = new Color(1f, 1f, 1f, 0f); // Hidden until WebCam started
            arCameraFeedRawImage.raycastTarget = false;

            // AR Ground Plane Grid Line Simulation
            var gridLine = CreateUIObject("ARGridLine", cameraViewport.transform);
            var gRect = gridLine.GetComponent<RectTransform>();
            gRect.sizeDelta = new Vector2(840, 360);
            gRect.anchoredPosition = new Vector2(0, 60);
            var gImg = gridLine.AddComponent<Image>();
            gImg.sprite = cardSprite;
            gImg.type = Image.Type.Sliced;
            gImg.color = new Color(0.16f, 0.85f, 0.62f, 0.16f);
            gImg.raycastTarget = false;

            // 2. Real 3D Fruit Stage (Rendered via TellMe3DStageRenderer onto RawImage)
            var fruitStage = CreateUIObject("Fruit3DStage", cameraViewport.transform);
            arFruit3DContainer = fruitStage.GetComponent<RectTransform>();
            arFruit3DContainer.sizeDelta = new Vector2(460, 460);
            arFruit3DContainer.anchoredPosition = new Vector2(0, 130);

            var raw3DGO = CreateUIObject("Fruit3DRawImage", fruitStage.transform);
            SetStretchFull(raw3DGO.GetComponent<RectTransform>());
            arFruit3DRawImage = raw3DGO.AddComponent<RawImage>();
            if (ar3DRenderer != null)
            {
                arFruit3DRawImage.texture = ar3DRenderer.OutputTexture;
            }

            // Clicking the 3D Fruit in AR advances to the next 3D Phase (Fase 1 -> 2 -> 3 -> 4 -> 5)!
            var touchArea = fruitStage.AddComponent<Button>();
            touchArea.targetGraphic = arFruit3DRawImage;
            touchArea.onClick.AddListener(CycleNextArFruitPhase);

            // Top Bar 1: Back to Cards + Unlocked Fruit Model Selector + Live Camera Toggle
            var topBar = CreateUIObject("ARTopBar", panel.transform);
            var tbRect = topBar.GetComponent<RectTransform>();
            tbRect.anchorMin = new Vector2(0f, 1f);
            tbRect.anchorMax = new Vector2(1f, 1f);
            tbRect.sizeDelta = new Vector2(0, 62);
            tbRect.anchoredPosition = new Vector2(0, -10);
            var tbH = topBar.AddComponent<HorizontalLayoutGroup>();
            tbH.childForceExpandWidth = true;
            tbH.padding = new RectOffset(14, 14, 4, 4);
            tbH.spacing = 10;

            var backBtn = CreateButton(topBar.transform, "⬅️ Kartu", HexToColor("#334155"), Color.white, 52);
            backBtn.onClick.AddListener(() =>
            {
                StopArLiveCamera();
                navManager?.NavigateTo(TellMeScreen.Module6_KartuDigital);
            });

            var apelBtn = CreateButton(topBar.transform, "🍎 Apel", HexToColor("#EF4444"), Color.white, 52);
            apelBtn.onClick.AddListener(() => TrySelectArCardById("KRT-APEL"));

            var jerukBtn = CreateButton(topBar.transform, "🍊 Jeruk", HexToColor("#F97316"), Color.white, 52);
            jerukBtn.onClick.AddListener(() => TrySelectArCardById("KRT-JERUK"));

            var manggaBtn = CreateButton(topBar.transform, "🥭 Mangga", HexToColor("#F59E0B"), Color.white, 52);
            manggaBtn.onClick.AddListener(() => TrySelectArCardById("KRT-MANGGA"));

            var pisangBtn = CreateButton(topBar.transform, "🍌 Pisang", HexToColor("#EAB308"), Color.white, 52);
            pisangBtn.onClick.AddListener(() => TrySelectArCardById("KRT-PISANG"));

            var camBtn = CreateButton(topBar.transform, "📷 Kamera", HexToColor("#059669"), Color.white, 52);
            camBtn.onClick.AddListener(ToggleArLiveCamera);

            // Top Bar 2: 5 Direct Phase Selector Pills (Fase 1..5) + Zoom Controls
            var phaseBar = CreateUIObject("ARPhaseBar", panel.transform);
            var pbRect = phaseBar.GetComponent<RectTransform>();
            pbRect.anchorMin = new Vector2(0f, 1f);
            pbRect.anchorMax = new Vector2(1f, 1f);
            pbRect.sizeDelta = new Vector2(0, 54);
            pbRect.anchoredPosition = new Vector2(0, -78);
            var pbH = phaseBar.AddComponent<HorizontalLayoutGroup>();
            pbH.childForceExpandWidth = true;
            pbH.padding = new RectOffset(14, 14, 2, 2);
            pbH.spacing = 8;

            string[] phaseShortLabels = { "1.🌱Bibit", "2.🌸Bunga", "3.🍎Matang", "4.🔪Belah", "5.🍂Layu" };
            for (int i = 0; i < 5; i++)
            {
                int pIdx = i;
                var pBtn = CreateButton(phaseBar.transform, phaseShortLabels[i], HexToColor("#1E293B"), Color.white, 48);
                pBtn.onClick.AddListener(() => SetArFruitPhase(pIdx));
            }

            var zoomInBtn = CreateButton(phaseBar.transform, "🔍+", HexToColor("#334155"), Color.white, 48);
            zoomInBtn.onClick.AddListener(() =>
            {
                if (ar3DRenderer != null) ar3DRenderer.SetZoomScale(ar3DRenderer.GetZoomScale() + 0.15f);
            });

            // Bottom Glass Panel: 5-Phase Info, Fruit Names & SIBI Sign Language Guidance
            var bottomGlass = CreateUIObject("ARBottomGlass", panel.transform);
            var bgRect = bottomGlass.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0f, 0f);
            bgRect.anchorMax = new Vector2(1f, 0f);
            bgRect.sizeDelta = new Vector2(0, 350);
            bgRect.anchoredPosition = new Vector2(0, 12);
            var glassImg = bottomGlass.AddComponent<Image>();
            glassImg.sprite = cardSprite;
            glassImg.type = Image.Type.Sliced;
            glassImg.color = new Color(1f, 1f, 1f, 0.96f);
            themeSurfaceElements.Add(glassImg);

            var bShadow = bottomGlass.AddComponent<Shadow>();
            bShadow.effectColor = new Color(0, 0, 0, 0.12f);
            bShadow.effectDistance = new Vector2(0, -6);

            var bgV = bottomGlass.AddComponent<VerticalLayoutGroup>();
            bgV.padding = new RectOffset(20, 20, 12, 12);
            bgV.spacing = 6;

            // Interactive Click-to-Change-Phase Button
            arCyclePhaseBtn = CreateButton(bottomGlass.transform, "👆 Ketuk Buah 3D / Tombol Ini: Ganti Fase Pertumbuhan (Fase 3/5)", HexToColor("#FF7A00"), Color.white, 54);
            arCyclePhaseBtn.onClick.AddListener(CycleNextArFruitPhase);

            arPhaseTitleText = CreateText("PhaseTitle", bottomGlass.transform, TellMe3DStageRenderer.FruitPhaseTitles[2], 22, FontStyle.Bold, HexToColor("#059669"));
            arPhaseDescText = CreateText("PhaseDesc", bottomGlass.transform, TellMe3DStageRenderer.FruitPhaseDescriptions[2], 18, FontStyle.Normal, HexToColor("#291804"));
            arPhaseDescText.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 48);

            // Fruit Bilingual Name Row
            var nameRow = CreateUIObject("NameRow", bottomGlass.transform);
            nameRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 34);
            var nameH = nameRow.AddComponent<HorizontalLayoutGroup>();
            nameH.childForceExpandWidth = true;

            arFruitNameId = CreateText("NameId", nameRow.transform, "ID: Apel Merah", 22, FontStyle.Bold, HexToColor("#291804"));
            arFruitNameEn = CreateText("NameEn", nameRow.transform, "EN: Red Apple", 20, FontStyle.Italic, HexToColor("#744E23"));
            arFruitNameEn.alignment = TextAnchor.MiddleRight;

            arSignInstruction = CreateText("SignInstr", bottomGlass.transform, "🖐️ Isyarat SIBI: Kepalkan tangan kanan, tempelkan buku jari di pipi lalu putar perlahan.", 18, FontStyle.Bold, HexToColor("#D95D00"));
            arSignInstruction.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 46);

            var plantBtn = CreateButton(bottomGlass.transform, "🌱 Rawat Pohon Buah Ini di Kebun 3D (Modul 8) ➡️", HexToColor("#10B981"), Color.white, 52);
            plantBtn.onClick.AddListener(() =>
            {
                StopArLiveCamera();
                arController?.OpenPlantGrowthFromAR();
            });

            if (arController != null)
            {
                arController.SetupComponents(arFruitNameId, arFruitNameEn, arSignInstruction, arFruit3DContainer, fruitStage);
            }

            return panel;
        }

        private void TrySelectArCardById(string cardId)
        {
            if (dataManager == null || dataManager.Data == null) return;
            var card = dataManager.Data.daftarKartu.Find(k => k.id == cardId);
            if (card == null) return;

            if (!card.terbuka)
            {
                ThemeTokens.TriggerHapticFeedback();
                string mapelName = ThemeTokens.GetSubjectDisplayName(card.mataPelajaran);
                mascotController?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_THINKING,
                    $"🔒 Model AR 3D {card.namaBuahId} masih terkunci! Yuk selesaikan Kuis {mapelName} dengan nilai >= 75 dulu untuk membuka kartunya!");
                return;
            }

            if (navManager != null)
            {
                navManager.OpenARFruitWithCard(card);
            }
            RefreshAr3DFruitDisplay();
        }

        private void CycleNextArFruitPhase()
        {
            int nextPhase = (arCurrentPhaseIndex + 1) % 5;
            SetArFruitPhase(nextPhase);
        }

        private void SetArFruitPhase(int phaseIndex)
        {
            ThemeTokens.TriggerHapticFeedback();
            arCurrentPhaseIndex = Mathf.Clamp(phaseIndex, 0, 4);
            RefreshAr3DFruitDisplay();

            var card = navManager?.ActiveCardForAR ?? (dataManager?.Data?.daftarKartu?.Count > 0 ? dataManager.Data.daftarKartu[0] : null);
            string fruitName = card != null ? card.namaBuahId : "Apel Merah";
            dataManager?.CatatAktivitas("🔍", "AR Buah 3D", $"Melihat {fruitName} 3D pada {TellMe3DStageRenderer.FruitPhaseTitles[arCurrentPhaseIndex]}");

            mascotController?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                $"{TellMe3DStageRenderer.FruitPhaseTitles[arCurrentPhaseIndex]} — {TellMe3DStageRenderer.FruitPhaseDescriptions[arCurrentPhaseIndex]}");
        }

        private void RefreshAr3DFruitDisplay()
        {
            var card = navManager?.ActiveCardForAR;
            if (card == null && dataManager != null && dataManager.Data.daftarKartu.Count > 0)
            {
                card = dataManager.Data.daftarKartu.Find(k => k.terbuka) ?? dataManager.Data.daftarKartu[0];
            }

            string cardId = card != null ? card.id : "KRT-APEL";
            if (ar3DRenderer != null)
            {
                ar3DRenderer.RenderFruitPhase(cardId, arCurrentPhaseIndex);
            }

            if (arPhaseTitleText != null)
                arPhaseTitleText.text = TellMe3DStageRenderer.FruitPhaseTitles[arCurrentPhaseIndex];
            if (arPhaseDescText != null)
                arPhaseDescText.text = TellMe3DStageRenderer.FruitPhaseDescriptions[arCurrentPhaseIndex];
            if (arCyclePhaseBtn != null)
                arCyclePhaseBtn.GetComponentInChildren<Text>().text = $"👆 Ketuk Buah 3D: Ganti Fase ({arCurrentPhaseIndex + 1}/5) ➡️";

            if (card != null)
            {
                if (arFruitNameId != null) arFruitNameId.text = $"ID: {card.namaBuahId}";
                if (arFruitNameEn != null) arFruitNameEn.text = $"EN: {card.namaBuahEn}";
                if (arSignInstruction != null) arSignInstruction.text = $"🖐️ {card.deskripsiIsyarat}";
            }
        }

        private void ToggleArLiveCamera()
        {
            if (arWebCamTexture != null && arWebCamTexture.isPlaying)
            {
                StopArLiveCamera();
                return;
            }

            try
            {
                if (WebCamTexture.devices != null && WebCamTexture.devices.Length > 0)
                {
                    arWebCamTexture = new WebCamTexture(WebCamTexture.devices[0].name, 720, 1280, 30);
                    arWebCamTexture.Play();
                    if (arCameraFeedRawImage != null)
                    {
                        arCameraFeedRawImage.texture = arWebCamTexture;
                        arCameraFeedRawImage.color = Color.white;
                    }
                    mascotController?.SetMascotState(BerryBearsState.STATE_ON_TAP, "📷 Kamera AR Smartphone aktif! Ketuk buah 3D di layar untuk melihat 5 fase pertumbuhannya!");
                }
                else
                {
                    mascotController?.SetMascotState(BerryBearsState.STATE_ON_TAP, "Mode Simulasi Kamera AR Aktif! Ketuk buah 3D di tengah layar untuk melihat 5 fase pertumbuhannya!");
                }
            }
            catch (Exception)
            {
                mascotController?.SetMascotState(BerryBearsState.STATE_ON_TAP, "Mode Simulasi Kamera AR Aktif!");
            }
        }

        private void StopArLiveCamera()
        {
            if (arWebCamTexture != null && arWebCamTexture.isPlaying)
            {
                arWebCamTexture.Stop();
            }
            if (arCameraFeedRawImage != null)
            {
                arCameraFeedRawImage.color = new Color(1f, 1f, 1f, 0f);
            }
        }
        #endregion

        #region Modul 8: Kebun 3D yang Bertumbuh Seiring Bertambahnya Level (Level 1 s/d Level 7)
        private GameObject BuildModul8Plant(Transform parent)
        {
            var panel = CreateUIObject("Module8_Plant", parent);
            SetStretchFull(panel.GetComponent<RectTransform>());

            var vGroup = panel.AddComponent<VerticalLayoutGroup>();
            vGroup.childAlignment = TextAnchor.UpperCenter;
            vGroup.childForceExpandHeight = false;
            vGroup.childForceExpandWidth = true;
            vGroup.padding = new RectOffset(18, 18, 10, 10);
            vGroup.spacing = 10;

            var title = CreateText("Title", panel.transform, "🌱 Kebun 3D Bertumbuh (Level 1 s/d Level 7)", 25, FontStyle.Bold, HexToColor("#059669"));

            // Level & XP Progress Card
            var xpCard = CreateUIObject("GardenLevelXpCard", panel.transform);
            xpCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 74);
            var xpBg = xpCard.AddComponent<Image>();
            xpBg.sprite = cardSprite;
            xpBg.type = Image.Type.Sliced;
            xpBg.color = HexToColor("#ECFDF5");
            var xpV = xpCard.AddComponent<VerticalLayoutGroup>();
            xpV.padding = new RectOffset(16, 16, 8, 8);
            xpV.spacing = 4;

            plantLevelXpText = CreateText("LevelXpText", xpCard.transform, "🌟 Level Kebun 3D: Level 2 / 7  •  XP: 120 / 700 XP (Selesaikan Kuis & Siram Kebun!)", 18, FontStyle.Bold, HexToColor("#065F46"));
            var xpBarGO = CreateUIObject("XpSlider", xpCard.transform);
            xpBarGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 16);
            plantXpSlider = xpBarGO.AddComponent<Slider>();
            plantXpSlider.value = 0.25f;
            var xpBarImg = xpBarGO.AddComponent<Image>();
            xpBarImg.sprite = pillSprite;
            xpBarImg.type = Image.Type.Sliced;
            xpBarImg.color = HexToColor("#A7F3D0");

            // Fruit Tree Selector Pills
            var fruitPillsRow = CreateUIObject("FruitPills", panel.transform);
            fruitPillsRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 48);
            var fH = fruitPillsRow.AddComponent<HorizontalLayoutGroup>();
            fH.childForceExpandWidth = true;
            fH.spacing = 10;

            var apelBtn = CreateButton(fruitPillsRow.transform, "🍎 Apel 3D", HexToColor("#D1FAE5"), HexToColor("#065F46"), 46);
            var jerukBtn = CreateButton(fruitPillsRow.transform, "🍊 Jeruk 3D", HexToColor("#FFF3E0"), HexToColor("#744E23"), 46);
            var manggaBtn = CreateButton(fruitPillsRow.transform, "🥭 Mangga 3D", HexToColor("#FEF3C7"), HexToColor("#92400E"), 46);
            var resetLvBtn = CreateButton(fruitPillsRow.transform, "🔄 Ulang Lv.1", HexToColor("#F1F5F9"), HexToColor("#334155"), 46);

            apelBtn.onClick.AddListener(() => plantController?.TanamBiji("Apel Merah"));
            jerukBtn.onClick.AddListener(() => plantController?.TanamBiji("Jeruk Manis"));
            manggaBtn.onClick.AddListener(() => plantController?.TanamBiji("Mangga Harum"));
            resetLvBtn.onClick.AddListener(() => plantController?.ResetGardenLevelTo1());

            // 3D Garden Diorama Stage Card
            var stageCard = CreateUIObject("GrowthStageCard", panel.transform);
            stageCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 380);
            var stageBg = stageCard.AddComponent<Image>();
            stageBg.sprite = cardSprite;
            stageBg.type = Image.Type.Sliced;
            stageBg.color = HexToColor("#E0F2FE"); // Sky Blue Diorama Backdrop

            var shadow1 = stageCard.AddComponent<Shadow>();
            shadow1.effectColor = new Color(0, 0, 0, 0.06f);
            shadow1.effectDistance = new Vector2(0, -4);

            var sV = stageCard.AddComponent<VerticalLayoutGroup>();
            sV.childAlignment = TextAnchor.MiddleCenter;
            sV.childForceExpandHeight = false;
            sV.childForceExpandWidth = true;
            sV.padding = new RectOffset(16, 16, 10, 10);
            sV.spacing = 4;

            plantStageBadgeText = CreateText("StageBadge", stageCard.transform, "🌱 DIORAMA KEBUN 3D • LEVEL 2 DARI 7 (Ketuk Kebun untuk Putar 3D)", 18, FontStyle.Bold, HexToColor("#0369A1"));
            plantStageBadgeText.alignment = TextAnchor.MiddleCenter;

            // Real 3D Garden RawImage Viewport
            var gardenViewportGO = CreateUIObject("Garden3DViewport", stageCard.transform);
            gardenViewportGO.GetComponent<RectTransform>().sizeDelta = new Vector2(360, 240);
            garden3DRawImage = gardenViewportGO.AddComponent<RawImage>();
            if (garden3DRenderer != null)
            {
                garden3DRawImage.texture = garden3DRenderer.OutputTexture;
            }
            var gardenTapBtn = gardenViewportGO.AddComponent<Button>();
            gardenTapBtn.targetGraphic = garden3DRawImage;
            gardenTapBtn.onClick.AddListener(() =>
            {
                garden3DRenderer?.RotateManual(45f);
                garden3DRenderer?.TriggerBouncePop();
            });

            plantStageTitleText = CreateText("StageTitle", stageCard.transform, "Level 2: Berkecambah (Tunas Kecil 3D)", 23, FontStyle.Bold, HexToColor("#064E3B"));
            plantStageTitleText.alignment = TextAnchor.MiddleCenter;

            plantStageDescText = CreateText("StageDesc", stageCard.transform, "Pohon 3D bertambah besar setiap naik level! Siram tanaman atau kerjakan kuis untuk naik ke Level berikutnya.", 18, FontStyle.Normal, HexToColor("#047857"));
            plantStageDescText.alignment = TextAnchor.MiddleCenter;

            // Soil Moisture Gauge
            var gaugeCard = CreateUIObject("MoistureCard", panel.transform);
            gaugeCard.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 88);
            var gBg = gaugeCard.AddComponent<Image>();
            gBg.sprite = cardSprite;
            gBg.type = Image.Type.Sliced;
            gBg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(gBg);

            var gV = gaugeCard.AddComponent<VerticalLayoutGroup>();
            gV.padding = new RectOffset(18, 18, 8, 8);
            gV.spacing = 6;

            plantMoisturePercentText = CreateText("MoistureText", gaugeCard.transform, "💧 Kelembapan Tanah: 85% (Tanah Lembap & Subur)", 19, FontStyle.Bold, HexToColor("#0284C7"));

            var sliderGO = CreateUIObject("MoistureSlider", gaugeCard.transform);
            sliderGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 18);
            plantMoistureSlider = sliderGO.AddComponent<Slider>();
            plantMoistureSlider.value = 0.85f;
            var sImg = sliderGO.AddComponent<Image>();
            sImg.sprite = pillSprite;
            sImg.type = Image.Type.Sliced;
            sImg.color = HexToColor("#BAE6FD");

            // 48h Neglect Alert Banner
            plantNeglectAlertBanner = CreateUIObject("NeglectAlertBanner", panel.transform);
            plantNeglectAlertBanner.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 58);
            var nImg = plantNeglectAlertBanner.AddComponent<Image>();
            nImg.sprite = cardSprite;
            nImg.type = Image.Type.Sliced;
            nImg.color = HexToColor("#FEE2E2");

            var nTxt = CreateText("AlertTxt", plantNeglectAlertBanner.transform, "⚠️ Tanamanmu haus! Sudah 2 hari belum disiram, ayo siram sekarang!", 18, FontStyle.Bold, HexToColor("#DC2626"));
            SetStretchFull(nTxt.GetComponent<RectTransform>());
            nTxt.alignment = TextAnchor.MiddleCenter;
            plantNeglectAlertBanner.SetActive(false);

            // Water Button & Neglect Test Toggle
            var actRow = CreateUIObject("ActRow", panel.transform);
            actRow.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 62);
            var aH = actRow.AddComponent<HorizontalLayoutGroup>();
            aH.childForceExpandWidth = true;
            aH.spacing = 14;

            plantWaterButton = CreateButton(actRow.transform, "💧 Siram Kebun 3D (+XP & Naik Level)", HexToColor("#059669"), Color.white, 58);
            plantNeglectSimButton = CreateButton(actRow.transform, "⚠️ Uji 2 Hari Belum Disiram", HexToColor("#FFE8D6"), HexToColor("#D95D00"), 58);

            plantNeglectSimButton.onClick.AddListener(() =>
            {
                plantController?.ToggleNeglectSimulation();
                UpdatePlantView();
            });

            if (plantController != null)
            {
                plantController.SetupComponents(plantStageTitleText, plantMoisturePercentText, plantMoistureSlider, null, plantNeglectAlertBanner, plantWaterButton);
                plantController.OnStageChanged += (stage) => UpdatePlantView();
                plantController.OnWateredEffectRequested += () =>
                {
                    garden3DRenderer?.PlayWateringEffect();
                };
            }

            UpdatePlantView();

            return panel;
        }

        private void UpdatePlantView()
        {
            if (plantController == null) return;

            string[] stageNames =
            {
                "Level 1: Pilih Biji",
                "Level 1: Biji di Tanah 3D",
                "Level 2: Berkecambah (Tunas Kecil 3D)",
                "Level 3: Tunas Muda Rimbun 3D",
                "Level 4: Pohon Kayu Rimbun 3D",
                "Level 5: Pohon Berbunga Mekar 3D",
                "Level 6: Pohon Berbuah Muda Hijau 3D",
                "Level 7 (MAX): Pohon Panen Buah Matang + Keranjang 3D!"
            };

            string[] stageDescs =
            {
                "Pilih biji buah untuk mulai menanam di Kebun 3D.",
                "Biji baru ditanam di pulau kebun 3D. Siram air untuk menumbuhkan kecambah!",
                "Batang hijau kecil dan 2 daun pertamanya tumbuh di tengah kebun 3D.",
                "Tunas muda makin tinggi dengan daun-daun rimbun. Terus rawat dan kerjakan kuis!",
                "Batang pohon kayu mulai kokoh dengan mahkota daun 3D berlapis!",
                "Bunga-bunga mekar bermunculan di seluruh mahkota pohon 3D!",
                "Kelopak bunga berubah menjadi buah-buah muda hijau yang menggantung di pohon 3D!",
                "Panen Raya! Buah matang merona di pohon dan memenuhi keranjang panen 3D di bawah pohon!"
            };

            int level = plantController.CurrentLevel;
            if (level >= 1 && level < stageNames.Length)
            {
                if (plantStageTitleText != null) plantStageTitleText.text = stageNames[level];
                if (plantStageDescText != null) plantStageDescText.text = stageDescs[level];
                if (plantStageBadgeText != null)
                    plantStageBadgeText.text = $"🌱 DIORAMA KEBUN 3D ({plantController.CurrentFruit.ToUpper()}) • LEVEL {level} DARI 7";
            }

            if (garden3DRenderer != null)
            {
                garden3DRenderer.RenderGardenLevel(plantController.CurrentFruit, level, plantController.IsNeglected);
            }

            if (dataManager != null && dataManager.Data != null)
            {
                int xp = dataManager.Data.xpKebun;
                if (plantLevelXpText != null)
                    plantLevelXpText.text = $"🌟 Level Kebun 3D: Level {level} / 7  •  Total XP: {xp} XP (Siram Kebun / Lulus Kuis = +XP)";
                if (plantXpSlider != null)
                    plantXpSlider.value = Mathf.Clamp01(level / 7f);
            }

            RefreshStudentProfileHeader();

            if (plantMoistureSlider != null)
            {
                plantMoistureSlider.value = plantController.SoilMoisture / 100f;
            }

            if (plantMoisturePercentText != null)
            {
                plantMoisturePercentText.text = $"💧 Kelembapan Tanah: {Mathf.RoundToInt(plantController.SoilMoisture)}% ({(plantController.IsNeglected ? "Kering & Haus Air!" : "Tanah Lembap & Subur")})";
            }

            if (plantNeglectAlertBanner != null)
            {
                plantNeglectAlertBanner.SetActive(plantController.IsNeglected);
            }
        }
        #endregion

        #region Floating Bottom Navigation Bar
        private GameObject BuildFloatingBottomNav(Transform parent)
        {
            var navGO = CreateUIObject("FloatingBottomNav", parent);
            var rect = navGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0, 16);
            rect.sizeDelta = new Vector2(0, 130);

            var bgImg = navGO.AddComponent<Image>();
            bgImg.sprite = cardSprite;
            bgImg.type = Image.Type.Sliced;
            bgImg.color = HexToColor("#FFFFFF");
            themeSurfaceElements.Add(bgImg);

            var shadow = navGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.10f);
            shadow.effectDistance = new Vector2(0, -6);

            var hGroup = navGO.AddComponent<HorizontalLayoutGroup>();
            hGroup.childAlignment = TextAnchor.MiddleCenter;
            hGroup.childForceExpandHeight = true;
            hGroup.childForceExpandWidth = true;
            hGroup.padding = new RectOffset(16, 16, 8, 8);
            hGroup.spacing = 16;

            bottomNavIconImages = new Image[5];
            bottomNavLabels = new Text[5];

            // 1. Beranda
            CreateBottomNavItem(navGO.transform, 0, "🏠", "Beranda", TellMeScreen.Module5_BerandaBerryBears);
            // 2. Kuis
            CreateBottomNavItem(navGO.transform, 1, "📝", "Kuis", TellMeScreen.Module3_KuisDanNilai);
            // 3. Rekam (Giant Elevated FAB)
            CreateBottomNavCenterFab(navGO.transform, 2, "🎙️", "Rekam", TellMeScreen.Module2_SpeechToTextBelajar);
            // 4. Isyarat
            CreateBottomNavItem(navGO.transform, 3, "🤟", "Isyarat", TellMeScreen.Module4_KuisBahasaIsyarat);
            // 5. Kebun
            CreateBottomNavItem(navGO.transform, 4, "🌱", "Kebun", TellMeScreen.Module8_TumbuhKembangTanaman);

            return navGO;
        }

        private void CreateBottomNavItem(Transform parent, int index, string icon, string label, TellMeScreen screen)
        {
            var btnGO = CreateUIObject($"NavItem_{label}", parent);
            var btn = btnGO.AddComponent<Button>();
            var v = btnGO.AddComponent<VerticalLayoutGroup>();
            v.childAlignment = TextAnchor.MiddleCenter;
            v.childForceExpandHeight = false;
            v.childForceExpandWidth = true;
            v.spacing = 4;

            var iconTxt = CreateText("Icon", btnGO.transform, icon, 34, FontStyle.Normal, Color.white);
            iconTxt.alignment = TextAnchor.MiddleCenter;

            var labelTxt = CreateText("Label", btnGO.transform, label, 18, FontStyle.Bold, HexToColor("#744E23"));
            labelTxt.alignment = TextAnchor.MiddleCenter;
            bottomNavLabels[index] = labelTxt;

            btn.onClick.AddListener(() =>
            {
                navManager?.NavigateTo(screen);
                HighlightBottomNav(index);
            });
        }

        private void CreateBottomNavCenterFab(Transform parent, int index, string icon, string label, TellMeScreen screen)
        {
            var fabGO = CreateUIObject("CenterRecordFab", parent);
            var fabRect = fabGO.GetComponent<RectTransform>();
            fabRect.sizeDelta = new Vector2(108, 108);
            var fabImg = fabGO.AddComponent<Image>();
            fabImg.sprite = circleSprite;
            fabImg.color = HexToColor("#FF7A00");

            var fabShadow = fabGO.AddComponent<Shadow>();
            fabShadow.effectColor = new Color(0, 0, 0, 0.20f);
            fabShadow.effectDistance = new Vector2(0, -6);

            var btn = fabGO.AddComponent<Button>();
            btn.targetGraphic = fabImg;

            var iconTxt = CreateText("Icon", fabGO.transform, icon, 48, FontStyle.Normal, Color.white);
            SetStretchFull(iconTxt.GetComponent<RectTransform>());
            iconTxt.alignment = TextAnchor.MiddleCenter;

            btn.onClick.AddListener(() =>
            {
                navManager?.NavigateTo(screen);
                HighlightBottomNav(index);
            });
        }

        private void HighlightBottomNav(int activeIndex)
        {
            if (bottomNavLabels == null) return;
            for (int i = 0; i < bottomNavLabels.Length; i++)
            {
                if (bottomNavLabels[i] == null) continue;
                bottomNavLabels[i].color = (i == activeIndex) ? HexToColor("#FF7A00") : HexToColor("#744E23");
            }
        }
        #endregion

        #region Theme Switching Engine
        public void CycleTheme()
        {
            int next = ((int)ThemeTokens.CurrentTheme + 1) % 5;
            ThemeTokens.SetTheme((AppTheme)next);
        }

        private void ApplyTheme(AppTheme theme)
        {
            if (!themePalettes.TryGetValue(theme, out var palette)) return;

            if (appBackground != null) appBackground.color = palette.bgApp;

            foreach (var g in themeBgElements)
            {
                if (g != null) g.color = palette.bgApp;
            }

            foreach (var g in themePrimaryElements)
            {
                if (g != null) g.color = palette.primary;
            }

            foreach (var g in themeTextMainElements)
            {
                if (g != null) g.color = palette.textMain;
            }

            foreach (var g in themeTextSecElements)
            {
                if (g != null) g.color = palette.textSecondary;
            }

            foreach (var g in themeSurfaceElements)
            {
                if (g != null) g.color = palette.bgSurface;
            }

            if (currentThemeButtonText != null)
            {
                currentThemeButtonText.text = $"🎨 {theme}";
            }
        }
        #endregion

        #region Mascot Living Animation Routines
        private IEnumerator SquashAndStretchRoutine(RectTransform target)
        {
            if (target == null) yield break;

            Vector3 origScale = Vector3.one;
            float duration = 0.28f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float s = Mathf.Sin(t * Mathf.PI);
                target.localScale = new Vector3(
                    origScale.x * (1f + 0.15f * s),
                    origScale.y * (1f - 0.15f * s),
                    origScale.z
                );
                yield return null;
            }

            target.localScale = origScale;
        }
        #endregion

        #region Screen Switch Event Handler
        private void HandleScreenChanged(TellMeScreen screen)
        {
            // Dynamically adjust viewport height to avoid dead whitespace
            bool isBottomNavVisible = (screen != TellMeScreen.Screen0_RoleSelection
                                      && screen != TellMeScreen.Module1_JadwalOrangTua
                                      && screen != TellMeScreen.Module7_ARBuah);

            if (topBackButton != null)
            {
                topBackButton.gameObject.SetActive(screen != TellMeScreen.Screen0_RoleSelection);
            }

            if (screenViewportRect != null)
            {
                screenViewportRect.offsetMin = new Vector2(0, isBottomNavVisible ? 154 : 20);
                screenViewportRect.offsetMax = new Vector2(0, -368);
            }

            if (screen != TellMeScreen.Module7_ARBuah)
            {
                StopArLiveCamera();
            }

            switch (screen)
            {
                case TellMeScreen.Module1_JadwalOrangTua:
                    RefreshScheduleCardsList();
                    RefreshParentMonitoringDashboard();
                    break;
                case TellMeScreen.Module3_KuisDanNilai:
                    RefreshParentMonitoringDashboard();
                    break;
                case TellMeScreen.Module5_BerandaBerryBears:
                    RefreshStudentProfileHeader();
                    break;
                case TellMeScreen.Module6_KartuDigital:
                    RefreshCardGalleryGrid();
                    break;
                case TellMeScreen.Module7_ARBuah:
                    arCurrentPhaseIndex = 2; // Mulai dari Fase 3 (Buah Matang Utuh 3D) lalu klik untuk eksplorasi 5 fase
                    RefreshAr3DFruitDisplay();
                    break;
                case TellMeScreen.Module8_TumbuhKembangTanaman:
                    plantController?.SyncLevelFromDatabase();
                    UpdatePlantView();
                    break;
            }

            // Sync Bottom Nav Highlight
            switch (screen)
            {
                case TellMeScreen.Module5_BerandaBerryBears:
                    HighlightBottomNav(0);
                    break;
                case TellMeScreen.Module3_KuisDanNilai:
                    HighlightBottomNav(1);
                    break;
                case TellMeScreen.Module2_SpeechToTextBelajar:
                    HighlightBottomNav(2);
                    break;
                case TellMeScreen.Module4_KuisBahasaIsyarat:
                    HighlightBottomNav(3);
                    break;
                case TellMeScreen.Module8_TumbuhKembangTanaman:
                    HighlightBottomNav(4);
                    break;
            }
        }
        #endregion

        #region UI Helper Utilities
        private static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void SetStretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle style, Color color)
        {
            var go = CreateUIObject(name, parent);
            var text = go.AddComponent<Text>();
            text.text = content;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.supportRichText = true;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Color bgColor, Color textColor, float height = 64)
        {
            var btnGO = CreateUIObject($"Btn_{label}", parent);
            btnGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, height);

            var img = btnGO.AddComponent<Image>();
            img.sprite = buttonSprite;
            img.type = Image.Type.Sliced;
            img.color = bgColor;

            var shadow = btnGO.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.12f);
            shadow.effectDistance = new Vector2(0, -4);

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r * 1.05f, bgColor.g * 1.05f, bgColor.b * 1.05f, 1f);
            colors.pressedColor = new Color(bgColor.r * 0.90f, bgColor.g * 0.90f, bgColor.b * 0.90f, 1f);
            btn.colors = colors;

            var txt = CreateText("Label", btnGO.transform, label, 22, FontStyle.Bold, textColor);
            SetStretchFull(txt.GetComponent<RectTransform>());
            txt.alignment = TextAnchor.MiddleCenter;

            return btn;
        }

        private static GameObject CreateInputField(Transform parent, string placeholderText, int fontSize)
        {
            var inputGO = CreateUIObject("InputField", parent);
            inputGO.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);

            var img = inputGO.AddComponent<Image>();
            img.sprite = cardSprite;
            img.type = Image.Type.Sliced;
            img.color = HexToColor("#FFF3E0");

            var textGO = CreateUIObject("Text", inputGO.transform);
            SetStretchFull(textGO.GetComponent<RectTransform>());
            var text = textGO.AddComponent<Text>();
            text.fontSize = fontSize;
            text.color = HexToColor("#291804");
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var phGO = CreateUIObject("Placeholder", inputGO.transform);
            SetStretchFull(phGO.GetComponent<RectTransform>());
            var ph = phGO.AddComponent<Text>();
            ph.text = placeholderText;
            ph.fontSize = fontSize;
            ph.fontStyle = FontStyle.Italic;
            ph.color = HexToColor("#A37A4C");
            ph.font = text.font;

            var inputField = inputGO.AddComponent<InputField>();
            inputField.textComponent = text;
            inputField.placeholder = ph;

            return inputGO;
        }

        private static Color HexToColor(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }
        #endregion
    }
}
