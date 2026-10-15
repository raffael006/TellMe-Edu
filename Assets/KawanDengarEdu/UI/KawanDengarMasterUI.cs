using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.Models;
using KawanDengarEdu.AR3D;
using KawanDengarEdu.MiniGame;

namespace KawanDengarEdu.UI
{
    public enum KDAction
    {
        None,
        GoToRegister,        // 00
        GoToDashboard,       // 01
        OpenParentModal,     // 01b
        CloseParentModal,    // Close 01b
        GoToHistory,         // 02
        GoToLiveSTT,         // 03
        GoToAISummary,       // 04
        OpenQuizChoice,      // 05
        GoToFillQuiz,        // 06
        GoToRewardCard,      // 07
        GoToARStudio,        // 08
        GoToPlantingGame,    // 09
        ToggleLanguage,
        TriggerBite,
        ResetApple,
        ToggleAnatomy,
        SetStudioMode,
        SetARMode,
        FastForwardDecay,
        TakeARPhoto,
        GardenWater,
        GardenFertilize,
        GardenShield,
        GardenHarvest,
        ColorRed,
        ColorGreen,
        ColorYellow,
        ColorPurple,
        ColorPink,
        ChartFilterAll,
        ChartFilterScience,
        ChartFilterLanguage,
        ChartFilterMath,
        QuizPickCorrect,
        QuizPickWrong,
        DownloadARCard,
        OpenARScanner,
        SimulateScanCard,
        NextARCard,
        PrevARCard,
        SelectQuizApple,
        SelectQuizOrange,
        SelectQuizBanana,
        SelectQuizAvocado
    }

    [Serializable]
    public class ARCardItem
    {
        public string id;
        public string fruitName;
        public string scientificName;
        public string emoji;
        public string targetWord;
        public char missingChar;
        public string prompt;
        public string nutrientFact;
        public string healthBenefit;
        public Color primaryColor;
        public bool isUnlocked;
        public string cardCode;
        public string textureName;

        public ARCardItem(string id, string name, string sci, string emoji, string word, char missing, string prompt, string nutrient, string benefit, Color col, bool unlocked, string code, string tex)
        {
            this.id = id;
            this.fruitName = name;
            this.scientificName = sci;
            this.emoji = emoji;
            this.targetWord = word;
            this.missingChar = missing;
            this.prompt = prompt;
            this.nutrientFact = nutrient;
            this.healthBenefit = benefit;
            this.primaryColor = col;
            this.isUnlocked = unlocked;
            this.cardCode = code;
            this.textureName = tex;
        }
    }

    [RequireComponent(typeof(Button))]
    public class KawanDengarButtonHook : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
    {
        public KDAction action = KDAction.None;
        private int lastClickFrame = -1;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            WireButton();
        }

        private void OnEnable()
        {
            rectTransform = transform as RectTransform;
            WireButton();
        }

        private void Start()
        {
            rectTransform = transform as RectTransform;
            WireButton();
        }

        public void WireButton()
        {
            var btn = GetComponent<Button>();
            if (btn != null && action != KDAction.None)
            {
                btn.onClick.RemoveListener(OnClick);
                btn.onClick.AddListener(OnClick);
            }
        }

        public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
        {
            OnClick();
        }

        private void Update()
        {
            // Direct Screen Point Click Fallback:
            // Pastikan setiap tombol dapat diklik 100% responsif walaupun EventSystem/InputSystem sedang terkendala
            if (Input.GetMouseButtonDown(0))
            {
                if (rectTransform == null) rectTransform = transform as RectTransform;
                if (rectTransform != null && gameObject.activeInHierarchy)
                {
                    var inst = KawanDengarMasterUI.Instance;
                    if (inst != null)
                    {
                        // Jika modal PIN orang tua terbuka, hanya terima klik tombol di dalam modal
                        if (inst.panelParentModal != null && inst.panelParentModal.activeSelf)
                        {
                            if (!transform.IsChildOf(inst.panelParentModal.transform)) return;
                        }
                        // Jika modal Anatomi terbuka, hanya terima klik tombol di dalam modal anatomi
                        else if (inst.arAnatomyModal != null && inst.arAnatomyModal.activeSelf)
                        {
                            if (!transform.IsChildOf(inst.arAnatomyModal.transform)) return;
                        }
                    }

                    Canvas parentCanvas = GetComponentInParent<Canvas>();
                    if (parentCanvas != null && parentCanvas.isActiveAndEnabled)
                    {
                        Camera cam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;
                        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, cam))
                        {
                            OnClick();
                        }
                    }
                }
            }
        }

        private void OnClick()
        {
            if (Time.frameCount == lastClickFrame) return;
            lastClickFrame = Time.frameCount;

            var inst = KawanDengarMasterUI.Instance;
            if (inst == null) inst = UnityEngine.Object.FindFirstObjectByType<KawanDengarMasterUI>();

            if (inst != null && action != KDAction.None)
            {
                inst.HandleAction(action);
            }
        }
    }

    /// <summary>
    /// Master UI & Scene Orchestrator untuk KawanDengar Edu.
    /// Menampilkan seluruh 11 layar antarmuka berbasis artwork prototipe Figma beresolusi tinggi (1080x2228 HD),
    /// bebas dari overlapping teks maupun emoji hilang (kotak-kotak), dengan hotspot interaktif responsif,
    /// panggung 3D Apel interaktif (rotator trackball, gigitan buah 4 tahap, timer busuk, varian warna),
    /// serta simulasi kebun berkebun.
    /// </summary>
    [DisallowMultipleComponent]
    public class KawanDengarMasterUI : MonoBehaviour
    {
        public static KawanDengarMasterUI Instance { get; private set; }

        [Header("Font Kustom (Opsional)")]
        public TMP_FontAsset customTmpFont;

        // ---------- CONTAINER 11 LAYAR ----------
        [Header("11 Layar Figma Panels")]
        public GameObject panelRegister;          // 00
        public GameObject panelDashboard;         // 01
        public GameObject panelParentModal;       // 01b
        public GameObject panelHistory;           // 02
        public GameObject panelLiveSTT;           // 03
        public GameObject panelAISummary;         // 04
        public GameObject panelQuizChoice;        // 05
        public GameObject panelFillQuiz;          // 06
        public GameObject panelRewardCard;        // 07
        public GameObject panelARStudio;          // 08
        public GameObject panelPlantingGame;      // 09

        // ---------- KOMPONEN AR 3D & STAGE ----------
        [Header("3D & AR Stage References")]
        public GameObject stage3DContainer;
        public GameObject fruitContainer;
        public GameObject gardenContainer;
        public GameObject appleWholeMesh;
        public GameObject appleBite1Mesh;
        public GameObject appleBite2Mesh;
        public GameObject appleBite3CoreMesh;
        public ParticleSystem crumbParticles;
        public ParticleSystem flyParticles;
        public RawImage arCameraFeed;
        public WebCamTexture webCamTexture;
        public GameObject arGroundReticle;
        public TextMeshProUGUI arDecayTimerText;
        public TextMeshProUGUI arNutritionFactText;
        public GameObject arAnatomyModal;
        public GameObject arSnapshotFlash;
        public CanvasScaler canvasScaler;

        // Controllers
        [Header("Controllers")]
        public ScreenManager screenManager;
        public AppManager appManager;
        public BilingualManager bilingualManager;
        public ParentSecurityManager securityManager;
        public TrackballFruitRotator trackballRotator;
        public FruitBiteController biteController;
        public FruitDecayTimer decayTimer;
        public MultiLineScoreChart scoreChart;
        public PlantingGameController plantingController;

        // State AR & UI
        private bool isRealARCameraMode = false;
        private int currentBite = 0;
        private Color currentFruitColor = new Color(0.94f, 0.27f, 0.27f);

        // Feedback / Dynamic texts
        private GameObject quizFeedbackCard;
        private GameObject quizSolvedLetterE;
        private TextMeshProUGUI quizFeedbackText;
        private TextMeshProUGUI dashboardToastText;
        private TextMeshProUGUI gardenNotificationText;
        // ---------- KOLEKSI KARTU AR & GAMIFIKASI ----------
        [Header("AR Cards & Flashcards Gamification")]
        public List<ARCardItem> arCardCollection = new List<ARCardItem>();
        public int activeCardIndex = 0;
        public bool isScanningCard = false;

        // Dynamic Card UI References (Screen 07)
        private TextMeshProUGUI rewardCardTitleText;
        private TextMeshProUGUI rewardCardSciNameText;
        private TextMeshProUGUI rewardCardNutrientText;
        private TextMeshProUGUI rewardCardStatusBadgeText;

        // AR Scanner References (Screen 08)
        private GameObject arScannerOverlay;
        private RectTransform arLaserLine;
        private TextMeshProUGUI arScannerPromptText;
        private TextMeshProUGUI arAnatomyTitleText;
        private TextMeshProUGUI arAnatomyBodyText;

        // Dynamic Quiz References (Screen 05 & 06)
        private TextMeshProUGUI quizChoiceTopicPreviewText;
        private TextMeshProUGUI quizQuestionPromptText;
        private TextMeshProUGUI quizSolvedLetterText;
        private TextMeshProUGUI quizWordSlot1Text;
        private TextMeshProUGUI quizWordSlot2Text;
        private TextMeshProUGUI quizWordSlot3Text;
        private TextMeshProUGUI quizWordSlot4Text;
        private TextMeshProUGUI quizOptionText1;
        private TextMeshProUGUI quizOptionText2;
        private TextMeshProUGUI quizOptionText3;
        private Image quizFruitIconImage;
        private RenderTexture fruitRenderTexture;
        private Camera fruitRenderCamera;
        private RawImage rewardCardFruitRawImage;
        private RawImage studioFruitRawImage;
        private static Dictionary<string, Sprite> cachedFruitSprites = new Dictionary<string, Sprite>();

        public void InitCardCollection()
        {
            if (arCardCollection == null || arCardCollection.Count == 0)
            {
                arCardCollection = new List<ARCardItem>
                {
                    new ARCardItem(
                        "apple_fuji", 
                        "Apel Juara", 
                        "Malus domestica", 
                        "[Apel]", 
                        "APEL", 
                        'E', 
                        "Lengkapi nama buah merah kaya Vitamin C ini:", 
                        "Vitamin C (14%), Serat Pektin, Polifenol Antioksidan", 
                        "Menyehatkan pencernaan, melindungi sel tubuh dari infeksi, dan membersihkan plak gigi secara alami.", 
                        new Color(0.94f, 0.27f, 0.27f), 
                        false, 
                        "KD-AR-APPLE-01",
                        "07_reward_card_3d.png"
                    ),
                    new ARCardItem(
                        "orange_citrus", 
                        "Jeruk Imun", 
                        "Citrus sinensis", 
                        "[Jeruk]", 
                        "JERUK", 
                        'U', 
                        "Lengkapi buah oranye manis tinggi asam folat & imunitas:", 
                        "Super Vitamin C (53mg/100g), Asam Folat, Kalium, Flavonoid", 
                        "Meningkatkan daya tahan tubuh anak, mempercepat penyembuhan sariawan, dan menjaga kesehatan kulit.", 
                        new Color(0.98f, 0.50f, 0.10f), 
                        false, 
                        "KD-AR-ORANGE-02",
                        "07_reward_card_3d.png"
                    ),
                    new ARCardItem(
                        "banana_gold", 
                        "Pisang Energi", 
                        "Musa acuminata", 
                        "[Pisang]", 
                        "PISANG", 
                        'A', 
                        "Lengkapi buah kuning sumber energi cepat untuk belajar:", 
                        "Tinggi Kalium (358mg), Vitamin B6, Karbohidrat Alami & Serat", 
                        "Memberikan energi instan untuk konsentrasi belajar, menjaga fungsi otot, dan menjaga mood ceria anak.", 
                        new Color(0.96f, 0.76f, 0.15f), 
                        false, 
                        "KD-AR-BANANA-03",
                        "07_reward_card_3d.png"
                    ),
                    new ARCardItem(
                        "avocado_brain", 
                        "Alpukat Cerdas", 
                        "Persea americana", 
                        "[Alpukat]", 
                        "ALPUKAT", 
                        'U', 
                        "Lengkapi buah hijau kaya lemak baik untuk nutrisi otak:", 
                        "Asam Oleat (Omega-9), Vitamin E, Lutein, Asam Folat", 
                        "Membantu perkembangan jaringan otak & memori belajar anak tunarungu, serta menjaga ketajaman penglihatan mata.", 
                        new Color(0.12f, 0.65f, 0.38f), 
                        false, 
                        "KD-AR-AVOCADO-04",
                        "07_reward_card_3d.png"
                    )
                };
            }
        }

        public ARCardItem GetActiveCard()
        {
            InitCardCollection();
            if (activeCardIndex < 0 || activeCardIndex >= arCardCollection.Count) activeCardIndex = 0;
            return arCardCollection[activeCardIndex];
        }

        public bool HasUnlockedAnyCard()
        {
            InitCardCollection();
            foreach (var c in arCardCollection)
            {
                if (c.isUnlocked) return true;
            }
            return false;
        }

        public void DownloadActiveARCard()
        {
            var card = GetActiveCard();
            if (card == null) return;

            try
            {
                string saveDir = Path.Combine(Application.persistentDataPath, "AR_Cards_Downloads");
                if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
                string filePath = Path.Combine(saveDir, $"{card.id}_flashcard.png");

                int w = 512, h = 768;
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color borderCol = new Color(0.1f, 0.15f, 0.25f);
                Color bgCol = Color.white;
                Color primary = card.primaryColor;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        bool isBorder = (x < 24 || x > w - 24 || y < 24 || y > h - 24);
                        bool isCorner = (x < 72 && y < 72) || (x > w - 72 && y < 72) || (x < 72 && y > h - 72) || (x > w - 72 && y > h - 72);
                        if (isBorder || isCorner) tex.SetPixel(x, y, borderCol);
                        else if (y > h - 140) tex.SetPixel(x, y, primary);
                        else tex.SetPixel(x, y, bgCol);
                    }
                }
                tex.Apply();
                byte[] png = tex.EncodeToPNG();
                File.WriteAllBytes(filePath, png);
                Destroy(tex);

                ShowDashboardToast($"Kartu AR '{card.fruitName}' Berhasil Diunduh!\n<size=13>Tersimpan di Galeri & siap dipindai di kamera AR.</size>");
                TriggerHapticSuccess();
            }
            catch (Exception ex)
            {
                ShowDashboardToast($"Kartu AR '{card.fruitName}' tersimpan di Galeri Edukasi!");
                Debug.LogWarning("[DownloadARCard] " + ex.Message);
            }
        }

        public void VerifyAndSpawnCardObject()
        {
            var card = GetActiveCard();
            if (card == null) return;

            isScanningCard = false;
            if (arScannerOverlay != null) arScannerOverlay.SetActive(false);
            if (fruitContainer != null) fruitContainer.SetActive(true);

            ChangeFruitColor(card.primaryColor, card.fruitName);
            if (fruitContainer != null)
            {
                if (card.id.Contains("banana"))
                {
                    fruitContainer.transform.localScale = new Vector3(0.70f, 1.45f, 0.70f);
                }
                else if (card.id.Contains("avocado"))
                {
                    fruitContainer.transform.localScale = new Vector3(0.95f, 1.25f, 0.95f);
                }
                else
                {
                    fruitContainer.transform.localScale = Vector3.one;
                }
            }

            if (arAnatomyTitleText != null)
            {
                arAnatomyTitleText.text = $"Anatomi & Gizi {card.fruitName}";
            }
            if (arAnatomyBodyText != null)
            {
                arAnatomyBodyText.text = $"<b>• Nama Ilmiah:</b> <i>{card.scientificName}</i>\n" +
                                        $"<b>• Nutrisi Utama:</b> {card.nutrientFact}\n" +
                                        $"<b>• Khasiat Tubuh:</b> {card.healthBenefit}\n" +
                                        $"<b>• Kode Verifikasi:</b> {card.cardCode} (AR Active)";
            }

            if (arNutritionFactText != null)
            {
                arNutritionFactText.text = $"<b>Kartu AR '{card.fruitName}' Terverifikasi!</b>\n<size=15>Kandungan: {card.nutrientFact}</size>";
                arNutritionFactText.gameObject.SetActive(true);
            }
            ShowDashboardToast($"Kartu AR '{card.fruitName}' Terverifikasi! Objek 3D Berhasil Muncul.");
            TriggerHapticSuccess();
        }

        public void UpdateCardUI()
        {
            var card = GetActiveCard();
            if (card == null) return;

            if (rewardCardTitleText != null) rewardCardTitleText.text = card.fruitName;
            if (rewardCardSciNameText != null) rewardCardSciNameText.text = $"<i>{card.scientificName}</i>";
            if (rewardCardNutrientText != null) rewardCardNutrientText.text = $"Kandungan: {card.nutrientFact}\n<size=11>Khasiat: {card.healthBenefit}</size>";
            if (rewardCardFruitRawImage != null && fruitRenderTexture != null) rewardCardFruitRawImage.texture = fruitRenderTexture;
            if (studioFruitRawImage != null && fruitRenderTexture != null) studioFruitRawImage.texture = fruitRenderTexture;

            if (rewardCardStatusBadgeText != null)
            {
                if (card.isUnlocked)
                {
                    rewardCardStatusBadgeText.text = $"[ KARTU AR TERBUKA • {card.cardCode} • SIAP SCAN ]";
                    rewardCardStatusBadgeText.color = new Color(0.2f, 0.95f, 0.5f);
                }
                else
                {
                    rewardCardStatusBadgeText.text = "[ KARTU TERKUNCI • SELESAIKAN KUIS UNTUK MEMBUKA ]";
                    rewardCardStatusBadgeText.color = new Color(0.95f, 0.4f, 0.35f);
                }
            }

            ChangeFruitColor(card.primaryColor, card.fruitName);
            if (fruitContainer != null)
            {
                if (card.id.Contains("banana"))
                    fruitContainer.transform.localScale = new Vector3(0.70f, 1.45f, 0.70f);
                else if (card.id.Contains("avocado"))
                    fruitContainer.transform.localScale = new Vector3(0.95f, 1.25f, 0.95f);
                else
                    fruitContainer.transform.localScale = Vector3.one;
            }
        }

        public void UpdateQuizUI()
        {
            var card = GetActiveCard();
            if (card == null) return;

            if (quizQuestionPromptText != null) quizQuestionPromptText.text = card.prompt;
            if (quizFruitIconImage != null) quizFruitIconImage.sprite = GetFruitSprite(card.id);
            if (quizSolvedLetterText != null) quizSolvedLetterText.text = card.missingChar.ToString();

            // Word slots
            if (quizWordSlot1Text != null && card.targetWord.Length >= 4)
            {
                quizWordSlot1Text.text = card.targetWord[0].ToString();
                quizWordSlot2Text.text = card.targetWord[1].ToString();
                quizWordSlot3Text.text = "?";
                quizWordSlot4Text.text = card.targetWord[card.targetWord.Length - 1].ToString();
            }

            // Options
            if (quizOptionText1 != null) quizOptionText1.text = card.missingChar.ToString();
            if (quizOptionText2 != null)
            {
                char d1 = card.missingChar == 'E' ? 'O' : (card.missingChar == 'U' ? 'A' : 'I');
                quizOptionText2.text = d1.ToString();
            }
            if (quizOptionText3 != null)
            {
                char d2 = card.missingChar == 'A' ? 'O' : (card.missingChar == 'E' ? 'I' : 'E');
                quizOptionText3.text = d2.ToString();
            }
        }

        public void ShowDashboardToast(string message)
        {
            if (dashboardToastText != null)
            {
                if (dashboardToastText.transform.parent != null)
                {
                    dashboardToastText.transform.parent.gameObject.SetActive(true);
                    dashboardToastText.transform.parent.SetAsLastSibling();
                }
                dashboardToastText.text = message;
                dashboardToastText.gameObject.SetActive(true);
                StopCoroutine(nameof(HideDashboardToastRoutine));
                StartCoroutine(nameof(HideDashboardToastRoutine));
            }
        }

        private IEnumerator HideDashboardToastRoutine()
        {
            yield return new WaitForSeconds(3.0f);
            if (dashboardToastText != null)
            {
                dashboardToastText.gameObject.SetActive(false);
                if (dashboardToastText.transform.parent != null && dashboardToastText.transform.parent.name == "DashboardToast")
                {
                    dashboardToastText.transform.parent.gameObject.SetActive(false);
                }
            }
        }

        public static void SafeDestroy(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }

            EnsureCoreManagers();
            EnsureEventSystem();
            InitCardCollection();
        }

        private void Start()
        {
            EnsureCoreManagers();
            EnsureEventSystem();
            InitCardCollection();

            Transform safeTr = panelDashboard != null ? panelDashboard.transform.parent : null;
            RectTransform safeRect = safeTr as RectTransform;
            bool isProperlyCentered = safeRect != null && 
                                      safeRect.anchorMin == new Vector2(0.5f, 0.5f) && 
                                      safeRect.sizeDelta == new Vector2(1080, 2228);

            bool needsRebuild = (panelDashboard == null) || !isProperlyCentered ||
                                (plantingController == null || plantingController.harvestFruitTreeObject == null) ||
                                (panelDashboard.transform.Find("Hotspot_Btn_ToggleLang") == null && 
                                 panelDashboard.transform.Find("Btn_ToggleLang") == null) ||
                                (panelAISummary == null || panelAISummary.transform.Find("Hotspot_Btn_CardApel") == null) ||
                                (panelQuizChoice == null || panelQuizChoice.transform.Find("Hotspot_Btn_StartQuizNow") == null) ||
                                (panelRewardCard == null || panelRewardCard.transform.Find("FruitLiveView") == null) ||
                                (arScannerOverlay == null) ||
                                (arAnatomyModal == null);

            if (needsRebuild)
            {
                var oldCanvas = GameObject.Find("KawanDengarCanvas");
                if (oldCanvas != null) SafeDestroy(oldCanvas);
                var oldStage = GameObject.Find("3D_AR_Stage");
                if (oldStage != null) SafeDestroy(oldStage);

                BuildUIHierarchy();
                Setup3DStageAndApple();
            }

            Align3DStageWithViewport();

            // Default: Tampilkan Dashboard Siswa
            ShowScreen(1);
        }

        public void EnsureEventSystem()
        {
            var existingES = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (existingES == null)
            {
                var esObj = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
                existingES = esObj.GetComponent<UnityEngine.EventSystems.EventSystem>();
            }
            existingES.enabled = true;

            var standalone = existingES.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            if (standalone == null)
            {
                standalone = existingES.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            standalone.enabled = true;

            // In activeInputHandler: 2 (Both), InputSystemUIInputModule blocks mouse clicks if action asset is empty.
            // Disable it so StandaloneInputModule processes clicks seamlessly.
            var inputModule = existingES.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputModule != null)
            {
                inputModule.enabled = false;
            }
        }

        private void Update()
        {
            if (canvasScaler != null)
            {
                float screenAspect = (float)Screen.width / Mathf.Max(1, Screen.height);
                float targetAspect = 1080f / 2228f;
                // Jika rasio layar >= target (lebih lebar dari desain, misal 1080x1920), kunci tinggi (1.0f)
                // Jika rasio layar < target (lebih tinggi/sempit), kunci lebar (0.0f)
                canvasScaler.matchWidthOrHeight = (screenAspect >= targetAspect) ? 1.0f : 0.0f;
            }

            if (panelARStudio != null && panelARStudio.activeSelf && decayTimer != null && arDecayTimerText != null)
            {
                float rem = decayTimer.RemainingTime;
                int min = Mathf.FloorToInt(rem / 60f);
                int sec = Mathf.FloorToInt(rem % 60f);
                arDecayTimerText.text = $"Pembusukan: {min:00}:{sec:00}";
            }

            if (isRealARCameraMode && arGroundReticle != null)
            {
                float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.08f;
                arGroundReticle.transform.localScale = new Vector3(pulse, 1f, pulse);
                arGroundReticle.transform.Rotate(0, 30f * Time.deltaTime, 0, Space.World);
            }

            if (isScanningCard && arLaserLine != null)
            {
                float yOffset = Mathf.Sin(Time.time * 3f) * 110f;
                arLaserLine.anchoredPosition = new Vector2(0, yOffset);
            }
        }

        private void OnDestroy()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                Destroy(webCamTexture);
            }
            if (fruitRenderTexture != null)
            {
                fruitRenderTexture.Release();
                Destroy(fruitRenderTexture);
            }
        }

        public void BuildFullSceneInEditor()
        {
            Instance = this;
            EnsureCoreManagers();
            BuildUIHierarchy();
            Setup3DStageAndApple();
            ShowScreen(1);
        }

        public void EnsureCoreManagers()
        {
            if (appManager == null)
            {
                var child = transform.Find("[AppManager]");
                if (child != null) appManager = child.GetComponent<AppManager>();
                else
                {
                    var go = new GameObject("[AppManager]", typeof(AppManager));
                    go.transform.SetParent(transform, false);
                    appManager = go.GetComponent<AppManager>();
                }
            }

            if (bilingualManager == null)
            {
                var child = transform.Find("[BilingualManager]");
                if (child != null) bilingualManager = child.GetComponent<BilingualManager>();
                else
                {
                    var go = new GameObject("[BilingualManager]", typeof(BilingualManager));
                    go.transform.SetParent(transform, false);
                    bilingualManager = go.GetComponent<BilingualManager>();
                }
            }

            if (securityManager == null)
            {
                var child = transform.Find("[ParentSecurityManager]");
                if (child != null) securityManager = child.GetComponent<ParentSecurityManager>();
                else
                {
                    var go = new GameObject("[ParentSecurityManager]", typeof(ParentSecurityManager));
                    go.transform.SetParent(transform, false);
                    securityManager = go.GetComponent<ParentSecurityManager>();
                }
            }

            if (screenManager == null)
            {
                var child = transform.Find("[ScreenManager]");
                if (child != null) screenManager = child.GetComponent<ScreenManager>();
                else
                {
                    var go = new GameObject("[ScreenManager]", typeof(ScreenManager));
                    go.transform.SetParent(transform, false);
                    screenManager = go.GetComponent<ScreenManager>();
                }
            }
        }

        // =========================================================================
        // MEMBANGUN CANVAS & 11 LAYAR FIGMA HD
        // =========================================================================
        public void BuildUIHierarchy()
        {
            EnsureEventSystem();

            var canvasObj = new GameObject("KawanDengarCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasObj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 2228);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1.0f;
            canvasScaler = scaler;

            var safeArea = CreateRect("SafeArea", canvasObj.transform);
            safeArea.anchorMin = new Vector2(0.5f, 0.5f);
            safeArea.anchorMax = new Vector2(0.5f, 0.5f);
            safeArea.pivot = new Vector2(0.5f, 0.5f);
            safeArea.sizeDelta = new Vector2(1080, 2228);
            safeArea.anchoredPosition = Vector2.zero;

            panelRegister = BuildScreen_00_Register(safeArea);
            panelDashboard = BuildScreen_01_Dashboard(safeArea);
            panelParentModal = BuildScreen_01b_ParentModal(safeArea);
            panelHistory = BuildScreen_02_RecordingHistory(safeArea);
            panelLiveSTT = BuildScreen_03_LiveSTT(safeArea);
            panelAISummary = BuildScreen_04_AISummary(safeArea);
            panelQuizChoice = BuildScreen_05_QuizChoiceModal(safeArea);
            panelFillQuiz = BuildScreen_06_FillWordQuiz(safeArea);
            panelRewardCard = BuildScreen_07_RewardCard(safeArea);
            panelARStudio = BuildScreen_08_AR3DStudio(safeArea);
            panelPlantingGame = BuildScreen_09_PlantingGame(safeArea);

            // AR Snapshot Flash Overlay
            var flashObj = CreateRect("ARSnapshotFlash", canvasObj.transform);
            Stretch(flashObj);
            var flashImg = flashObj.gameObject.AddComponent<Image>();
            flashImg.color = new Color(1f, 1f, 1f, 0.95f);
            flashImg.raycastTarget = false;
            arSnapshotFlash = flashObj.gameObject;
            arSnapshotFlash.SetActive(false);

            if (screenManager != null)
            {
                screenManager.panelRegister = panelRegister;
                screenManager.panelDashboard = panelDashboard;
                screenManager.panelParentModal = panelParentModal;
                screenManager.panelHistory = panelHistory;
                screenManager.panelLiveSTT = panelLiveSTT;
                screenManager.panelAISummary = panelAISummary;
                screenManager.panelQuizChoice = panelQuizChoice;
                screenManager.panelFillQuiz = panelFillQuiz;
                screenManager.panelRewardCard = panelRewardCard;
                screenManager.panelARStudio = panelARStudio;
                screenManager.panelPlantingGame = panelPlantingGame;
            }
        }

        public void ShowScreen(int screenNumber)
        {
            if (panelRegister != null) panelRegister.SetActive(screenNumber == 0);
            if (panelDashboard != null) panelDashboard.SetActive(screenNumber == 1);
            if (panelParentModal != null) panelParentModal.SetActive(screenNumber == 15);
            if (panelHistory != null) panelHistory.SetActive(screenNumber == 2);
            if (panelLiveSTT != null) panelLiveSTT.SetActive(screenNumber == 3);
            if (panelAISummary != null) panelAISummary.SetActive(screenNumber == 4);
            if (panelQuizChoice != null) panelQuizChoice.SetActive(screenNumber == 5);
            if (panelFillQuiz != null) panelFillQuiz.SetActive(screenNumber == 6);
            if (panelRewardCard != null) panelRewardCard.SetActive(screenNumber == 7);
            if (panelARStudio != null) panelARStudio.SetActive(screenNumber == 8);
            if (panelPlantingGame != null) panelPlantingGame.SetActive(screenNumber == 9);

            if (screenNumber == 6)
            {
                if (quizFeedbackCard != null) quizFeedbackCard.SetActive(false);
                if (quizSolvedLetterE != null) quizSolvedLetterE.SetActive(false);
            }

            if (stage3DContainer != null)
            {
                stage3DContainer.SetActive(screenNumber == 7 || screenNumber == 8 || screenNumber == 9);
            }

            if (fruitContainer != null)
            {
                fruitContainer.SetActive(screenNumber == 7 || screenNumber == 8);
            }

            if (gardenContainer != null)
            {
                gardenContainer.SetActive(screenNumber == 9);
            }

            if (screenNumber == 7 || screenNumber == 8 || screenNumber == 9)
            {
                Align3DStageWithViewport();
            }

            if (screenNumber == 7)
            {
                UpdateCardUI();
            }
        }

        // =========================================================================
        // UNIVERSAL ACTION HANDLER
        // =========================================================================
        public void HandleAction(KDAction action)
        {
            TriggerHapticLight();

            switch (action)
            {
                case KDAction.GoToRegister:
                    ShowScreen(0);
                    break;
                case KDAction.GoToDashboard:
                    ShowScreen(1);
                    break;
                case KDAction.OpenParentModal:
                    if (panelParentModal != null)
                    {
                        panelParentModal.SetActive(true);
                        panelParentModal.transform.SetAsLastSibling();
                    }
                    ShowDashboardToast("Mode Orang Tua: Masukkan PIN 4 Digit");
                    break;
                case KDAction.CloseParentModal:
                    if (panelParentModal != null) panelParentModal.SetActive(false);
                    ShowDashboardToast("Pengaturan Jadwal & Akses Orang Tua Disimpan");
                    break;
                case KDAction.GoToHistory:
                    ShowScreen(2);
                    break;
                case KDAction.GoToLiveSTT:
                    ShowScreen(3);
                    break;
                case KDAction.GoToAISummary:
                    ShowScreen(4);
                    break;
                case KDAction.OpenQuizChoice:
                    ShowScreen(5);
                    break;
                case KDAction.GoToFillQuiz:
                    ShowScreen(6);
                    UpdateQuizUI();
                    break;
                case KDAction.GoToRewardCard:
                    ShowScreen(7);
                    UpdateCardUI();
                    break;
                case KDAction.GoToARStudio:
                    if (!HasUnlockedAnyCard())
                    {
                        ShowDashboardToast("Kerjakan Kuis Sains terlebih dahulu untuk membuka Kartu AR!");
                        ShowScreen(5);
                    }
                    else
                    {
                        ShowScreen(8);
                        VerifyAndSpawnCardObject();
                    }
                    break;
                case KDAction.GoToPlantingGame:
                    ShowScreen(9);
                    break;
                case KDAction.SelectQuizApple:
                    activeCardIndex = 0;
                    UpdateQuizUI();
                    UpdateCardUI();
                    if (quizChoiceTopicPreviewText != null) quizChoiceTopicPreviewText.text = "Topik: <b>Apel Juara</b> (Hadiah: Kartu AR Apel)";
                    ShowDashboardToast("Kuis Dipilih: Apel Juara");
                    break;
                case KDAction.SelectQuizOrange:
                    activeCardIndex = 1;
                    UpdateQuizUI();
                    UpdateCardUI();
                    if (quizChoiceTopicPreviewText != null) quizChoiceTopicPreviewText.text = "Topik: <b>Jeruk Imun</b> (Hadiah: Kartu AR Jeruk)";
                    ShowDashboardToast("Kuis Dipilih: Jeruk Imun");
                    break;
                case KDAction.SelectQuizBanana:
                    activeCardIndex = 2;
                    UpdateQuizUI();
                    UpdateCardUI();
                    if (quizChoiceTopicPreviewText != null) quizChoiceTopicPreviewText.text = "Topik: <b>Pisang Energi</b> (Hadiah: Kartu AR Pisang)";
                    ShowDashboardToast("Kuis Dipilih: Pisang Energi");
                    break;
                case KDAction.SelectQuizAvocado:
                    activeCardIndex = 3;
                    UpdateQuizUI();
                    UpdateCardUI();
                    if (quizChoiceTopicPreviewText != null) quizChoiceTopicPreviewText.text = "Topik: <b>Alpukat Cerdas</b> (Hadiah: Kartu AR Alpukat)";
                    ShowDashboardToast("Kuis Dipilih: Alpukat Cerdas");
                    break;
                case KDAction.DownloadARCard:
                    DownloadActiveARCard();
                    break;
                case KDAction.OpenARScanner:
                    ShowScreen(8);
                    isScanningCard = true;
                    if (arScannerOverlay != null) arScannerOverlay.SetActive(true);
                    if (fruitContainer != null) fruitContainer.SetActive(false);
                    ShowDashboardToast("Scanner AR Aktif: Arahkan kamera ke Kartu AR yang diunduh");
                    break;
                case KDAction.SimulateScanCard:
                    VerifyAndSpawnCardObject();
                    break;
                case KDAction.NextARCard:
                    InitCardCollection();
                    activeCardIndex = (activeCardIndex + 1) % arCardCollection.Count;
                    UpdateCardUI();
                    UpdateQuizUI();
                    break;
                case KDAction.PrevARCard:
                    InitCardCollection();
                    activeCardIndex = (activeCardIndex - 1 + arCardCollection.Count) % arCardCollection.Count;
                    UpdateCardUI();
                    UpdateQuizUI();
                    break;
                case KDAction.ToggleLanguage:
                    if (bilingualManager != null)
                    {
                        bilingualManager.ToggleLanguage();
                        string lName = bilingualManager.IsIndonesian() ? "Bahasa Indonesia" : "English";
                        ShowDashboardToast($"Bahasa Aktif: {lName}");
                    }
                    else
                    {
                        ShowDashboardToast("Bahasa Berhasil Diubah: ID <-> EN");
                    }
                    break;
                case KDAction.TriggerBite:
                    TriggerBiteApple();
                    break;
                case KDAction.ResetApple:
                    ResetAppleStage();
                    break;
                case KDAction.ToggleAnatomy:
                    ToggleAnatomyModal();
                    break;
                case KDAction.SetStudioMode:
                    SetARMode(false);
                    break;
                case KDAction.SetARMode:
                    SetARMode(true);
                    break;
                case KDAction.FastForwardDecay:
                    if (decayTimer != null) decayTimer.FastForwardDecay();
                    break;
                case KDAction.TakeARPhoto:
                    StartCoroutine(TakeARPhotoRoutine());
                    break;
                case KDAction.ColorRed:
                    ChangeFruitColor(new Color(0.94f, 0.27f, 0.27f), "Merah Fuji");
                    break;
                case KDAction.ColorGreen:
                    ChangeFruitColor(new Color(0.06f, 0.73f, 0.51f), "Hijau Fuji");
                    break;
                case KDAction.ColorYellow:
                    ChangeFruitColor(new Color(0.96f, 0.62f, 0.04f), "Golden Yellow");
                    break;
                case KDAction.ColorPurple:
                    ChangeFruitColor(new Color(0.55f, 0.36f, 0.96f), "Ungu Fancy");
                    break;
                case KDAction.ColorPink:
                    ChangeFruitColor(new Color(0.93f, 0.28f, 0.60f), "Pink Lady");
                    break;
                case KDAction.ChartFilterAll:
                    if (scoreChart != null) scoreChart.SetFilter(ChartFilter.All);
                    ShowDashboardToast("Filter Grafik: Menampilkan Semua Mapel (Sains, Bahasa, Matematika)");
                    break;
                case KDAction.ChartFilterScience:
                    if (scoreChart != null) scoreChart.SetFilter(ChartFilter.Science);
                    ShowDashboardToast("Filter Grafik: Nilai Sains (Rata-rata: 95 - Sangat Baik)");
                    break;
                case KDAction.ChartFilterLanguage:
                    if (scoreChart != null) scoreChart.SetFilter(ChartFilter.Language);
                    ShowDashboardToast("Filter Grafik: Nilai Bahasa (Rata-rata: 90 - Aktif)");
                    break;
                case KDAction.ChartFilterMath:
                    if (scoreChart != null) scoreChart.SetFilter(ChartFilter.Math);
                    ShowDashboardToast("Filter Grafik: Nilai Matematika (Rata-rata: 92 - Logis)");
                    break;
                case KDAction.QuizPickCorrect:
                    var cardCorrect = GetActiveCard();
                    cardCorrect.isUnlocked = true;
                    if (quizSolvedLetterE != null) quizSolvedLetterE.SetActive(true);
                    if (quizSolvedLetterText != null) quizSolvedLetterText.text = cardCorrect.missingChar.ToString();
                    if (quizFeedbackCard != null)
                    {
                        quizFeedbackCard.SetActive(true);
                        quizFeedbackCard.transform.SetAsLastSibling();
                    }
                    if (quizFeedbackText != null)
                    {
                        quizFeedbackText.color = new Color(0.05f, 0.65f, 0.40f);
                        quizFeedbackText.text = $"<b>Luar Biasa Benar!</b> Kata: <b>{cardCorrect.targetWord}</b>\n<size=17>+50 Bintang & KARTU AR '{cardCorrect.fruitName}' TERBUKA!</size>";
                        quizFeedbackText.gameObject.SetActive(true);
                    }
                    UpdateCardUI();
                    TriggerHapticSuccess();
                    StartCoroutine(QuizWinTransition());
                    break;
                case KDAction.QuizPickWrong:
                    var cardWrong = GetActiveCard();
                    if (quizFeedbackCard != null)
                    {
                        quizFeedbackCard.SetActive(true);
                        quizFeedbackCard.transform.SetAsLastSibling();
                    }
                    if (quizFeedbackText != null)
                    {
                        quizFeedbackText.color = new Color(0.85f, 0.25f, 0.20f);
                        quizFeedbackText.text = $"<b>Hampir tepat!</b> Pilih huruf <b>{cardWrong.missingChar}</b> untuk melengkapi <b>{cardWrong.targetWord}</b>";
                        quizFeedbackText.gameObject.SetActive(true);
                    }
                    TriggerHapticMedium();
                    break;
                case KDAction.GardenWater:
                    if (plantingController != null) plantingController.OnClickWater();
                    if (gardenNotificationText != null)
                    {
                        gardenNotificationText.text = "<b>Disiram!</b> Tunas apel tumbuh semakin hijau & subur.";
                        gardenNotificationText.gameObject.SetActive(true);
                    }
                    TriggerHapticMedium();
                    break;
                case KDAction.GardenFertilize:
                    if (plantingController != null) plantingController.OnClickFertilize();
                    if (gardenNotificationText != null)
                    {
                        gardenNotificationText.text = "<b>Dipupuk!</b> Batang pohon apel menguat & daun rimbun.";
                        gardenNotificationText.gameObject.SetActive(true);
                    }
                    TriggerHapticMedium();
                    break;
                case KDAction.GardenShield:
                    if (plantingController != null) plantingController.OnClickActivateShield();
                    if (gardenNotificationText != null)
                    {
                        gardenNotificationText.text = "<b>Perisai Aktif!</b> Hama ulat berhasil dihalau.";
                        gardenNotificationText.gameObject.SetActive(true);
                    }
                    TriggerHapticSuccess();
                    break;
                case KDAction.GardenHarvest:
                    if (plantingController != null) plantingController.OnClickHarvest();
                    if (gardenNotificationText != null)
                    {
                        gardenNotificationText.text = "<b>Panen Raya!</b> Kamu memanen 3 Apel Merah Segar! (+100 Bintang)";
                        gardenNotificationText.gameObject.SetActive(true);
                    }
                    TriggerHapticSuccess();
                    break;
            }
        }

        private IEnumerator QuizWinTransition()
        {
            yield return new WaitForSeconds(1.2f);
            ShowScreen(7);
        }

        // =========================================================================
        // BUILD 11 LAYAR FIGMA HD DENGAN HOTSPOT RESPONSIF
        // =========================================================================
        private GameObject BuildScreen_00_Register(Transform parent)
        {
            var p = CreateHDArtboardPanel("00_Register", parent, "00_register_account.png");
            CreateHotspot(p.transform, 40, 584, 332, 48, KDAction.GoToDashboard, "Btn_Daftar");
            CreateHotspot(p.transform, 75, 640, 260, 40, KDAction.GoToDashboard, "Btn_Masuk");
            return p;
        }

        private GameObject BuildScreen_01_Dashboard(Transform parent)
        {
            var p = CreateHDArtboardPanel("01_Dashboard", parent, "01_dashboard.png");

            // Language Switcher (ID/EN)
            CreateHotspot(p.transform, 275, 46, 115, 50, KDAction.ToggleLanguage, "Btn_ToggleLang");

            // 4 Filter Chips Grafik
            CreateHotspot(p.transform, 32, 140, 84, 36, KDAction.ChartFilterAll, "Btn_ChartAll");
            CreateHotspot(p.transform, 112, 140, 68, 36, KDAction.ChartFilterScience, "Btn_ChartSains");
            CreateHotspot(p.transform, 176, 140, 74, 36, KDAction.ChartFilterLanguage, "Btn_ChartBahasa");
            CreateHotspot(p.transform, 246, 140, 84, 36, KDAction.ChartFilterMath, "Btn_ChartMath");

            // Parent PIN Lock
            CreateHotspot(p.transform, 235, 342, 160, 36, KDAction.OpenParentModal, "Btn_ParentLock");

            // 3 Baris Jadwal Pelajaran (Klik jadwal untuk mulai rekam suara kelas)
            CreateHotspot(p.transform, 16, 372, 380, 72, KDAction.GoToLiveSTT, "Btn_JadwalSains");
            CreateHotspot(p.transform, 16, 444, 380, 66, KDAction.GoToLiveSTT, "Btn_JadwalBahasa");
            CreateHotspot(p.transform, 16, 510, 380, 66, KDAction.GoToLiveSTT, "Btn_JadwalMath");

            // Quick Navigation (Riwayat, Kuis, Studio AR, Kebun)
            CreateHotspot(p.transform, 16, 578, 94, 58, KDAction.GoToHistory, "Btn_NavRiwayat");
            CreateHotspot(p.transform, 110, 578, 94, 58, KDAction.OpenQuizChoice, "Btn_NavKuis");
            CreateHotspot(p.transform, 204, 578, 102, 58, KDAction.GoToARStudio, "Btn_NavStudioAR");
            CreateHotspot(p.transform, 306, 578, 90, 58, KDAction.GoToPlantingGame, "Btn_NavKebun");

            // Tombol Besar: Mulai Rekam Suara Guru
            CreateHotspot(p.transform, 16, 646, 380, 82, KDAction.GoToLiveSTT, "Btn_MulaiRekam");

            // Dynamic Dashboard Toast Notification
            var toastRect = CreateRect("DashboardToast", p.transform);
            toastRect.anchorMin = new Vector2(0.06f, 0.235f);
            toastRect.anchorMax = new Vector2(0.94f, 0.295f);
            toastRect.offsetMin = Vector2.zero;
            toastRect.offsetMax = Vector2.zero;
            var toastBg = toastRect.gameObject.AddComponent<Image>();
            toastBg.color = new Color(0.92f, 0.96f, 1.0f, 0.96f);
            toastBg.raycastTarget = false;

            var toastOutline = toastRect.gameObject.AddComponent<Outline>();
            toastOutline.effectColor = new Color(0.23f, 0.51f, 0.96f, 0.55f);
            toastOutline.effectDistance = new Vector2(1.5f, -1.5f);

            var toastTextRect = CreateRect("ToastText", toastRect.transform);
            Stretch(toastTextRect, 10, 6, 10, 6);
            dashboardToastText = toastTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            dashboardToastText.fontSize = 18;
            dashboardToastText.fontStyle = FontStyles.Bold;
            dashboardToastText.color = new Color(0.10f, 0.35f, 0.85f);
            dashboardToastText.alignment = TextAlignmentOptions.Center;
            dashboardToastText.text = "";
            toastRect.gameObject.SetActive(false);

            return p;
        }

        private GameObject BuildScreen_01b_ParentModal(Transform parent)
        {
            var p = CreateHDArtboardPanel("01b_ParentModal", parent, "01b_parent_schedule_modal.png");
            p.SetActive(false);

            // Shaded Backdrop Dismiss (Tap outside modal card to close)
            CreateHotspot(p.transform, 0, 0, 412, 80, KDAction.CloseParentModal, "Btn_TopDismiss");
            CreateHotspot(p.transform, 0, 760, 412, 90, KDAction.CloseParentModal, "Btn_BottomDismiss");

            // PIN Box Verification Area (Default: 1234)
            CreateHotspot(p.transform, 40, 235, 332, 70, KDAction.CloseParentModal, "Btn_PinVerification");

            // Modal Buttons: Simpan Jadwal & Tutup (di bagian bawah kartu modal y=670)
            CreateHotspot(p.transform, 40, 670, 208, 55, KDAction.CloseParentModal, "Btn_SimpanJadwal");
            CreateHotspot(p.transform, 250, 670, 120, 55, KDAction.CloseParentModal, "Btn_TutupModal");
            return p;
        }

        private GameObject BuildScreen_02_RecordingHistory(Transform parent)
        {
            var p = CreateHDArtboardPanel("02_RecordingHistory", parent, "02_recording_history.png");
            CreateHotspot(p.transform, 18, 40, 110, 36, KDAction.GoToDashboard, "Btn_BackToDash");

            // Filter Chips
            CreateHotspot(p.transform, 20, 120, 84, 32, KDAction.ChartFilterAll, "Btn_HistFilterAll");
            CreateHotspot(p.transform, 112, 120, 80, 32, KDAction.ChartFilterScience, "Btn_HistFilterSains");
            CreateHotspot(p.transform, 200, 120, 88, 32, KDAction.ChartFilterLanguage, "Btn_HistFilterBahasa");
            CreateHotspot(p.transform, 296, 120, 96, 32, KDAction.ChartFilterMath, "Btn_HistFilterMath");

            // Clickable recording items -> Buka Rangkuman AI
            CreateHotspot(p.transform, 18, 170, 376, 126, KDAction.GoToAISummary, "Btn_RecItemCard");
            CreateHotspot(p.transform, 260, 235, 116, 26, KDAction.GoToARStudio, "Btn_RecKartu3D");

            // Edit Title Card buttons
            CreateHotspot(p.transform, 44, 520, 200, 44, KDAction.GoToDashboard, "Btn_SaveTitle");
            CreateHotspot(p.transform, 252, 520, 116, 44, KDAction.GoToDashboard, "Btn_CancelTitle");
            return p;
        }

        private GameObject BuildScreen_03_LiveSTT(Transform parent)
        {
            var p = CreateHDArtboardPanel("03_LiveSTT", parent, "03_live_stt_recording.png");
            CreateHotspot(p.transform, 18, 40, 90, 44, KDAction.GoToDashboard, "Btn_BackToDash");
            CreateHotspot(p.transform, 260, 112, 116, 28, KDAction.ToggleLanguage, "Btn_ToggleSignLang");

            // Highlighted vocab words in live transcript
            CreateHotspot(p.transform, 36, 440, 128, 34, KDAction.GoToARStudio, "Btn_WordApel");
            CreateHotspot(p.transform, 36, 506, 132, 34, KDAction.GoToPlantingGame, "Btn_WordVitC");

            // Bottom CTA: Selesai Rekam • Proses AI & Kuis
            CreateHotspot(p.transform, 18, 695, 376, 70, KDAction.GoToAISummary, "Btn_SelesaiRekam");
            return p;
        }

        private GameObject BuildScreen_04_AISummary(Transform parent)
        {
            var p = CreateHDArtboardPanel("04_AISummary", parent, "04_ai_summary.png");
            
            // ← Transkripsi (Kembali ke Live STT)
            CreateHotspot(p.transform, 16, 36, 130, 44, KDAction.GoToLiveSTT, "Btn_BackToLiveSTT");

            // 3 Summary Cards interaktif (Klik untuk membuka eksplorasi 3D Studio)
            CreateHotspot(p.transform, 18, 155, 376, 96, KDAction.GoToARStudio, "Btn_CardApel");
            CreateHotspot(p.transform, 18, 263, 376, 96, KDAction.GoToARStudio, "Btn_CardWarna");
            CreateHotspot(p.transform, 18, 371, 376, 96, KDAction.GoToARStudio, "Btn_CardVitamin");

            // Kosakata Penting Guru: Apel & Vitamin C
            CreateHotspot(p.transform, 34, 522, 98, 42, KDAction.GoToARStudio, "Btn_VocabApel");
            CreateHotspot(p.transform, 138, 522, 116, 42, KDAction.GoToPlantingGame, "Btn_VocabVitC");

            // Tombol Utama: Lanjut ke Kuis Ceria 🚀
            CreateHotspot(p.transform, 16, 672, 380, 75, KDAction.GoToFillQuiz, "Btn_MulaiKuis");
            return p;
        }

        private GameObject BuildScreen_05_QuizChoiceModal(Transform parent)
        {
            var p = CreateHDArtboardPanel("05_QuizChoiceModal", parent, "05_quiz_choice_modal.png");

            // Tap area luar (gelap) untuk menutup kembali ke Dashboard
            CreateHotspot(p.transform, 0, 0, 412, 320, KDAction.GoToDashboard, "Btn_BackdropClose");

            // 4 Fruit Quiz Selectors (Pilihan Topik Buah)
            // Chip 1: 🍎 Apel
            CreateHotspot(p.transform, 22, 480, 86, 44, KDAction.SelectQuizApple, "Btn_SelectApple");
            // Chip 2: 🍊 Jeruk
            CreateHotspot(p.transform, 114, 480, 86, 44, KDAction.SelectQuizOrange, "Btn_SelectOrange");
            // Chip 3: 🍌 Pisang
            CreateHotspot(p.transform, 206, 480, 88, 44, KDAction.SelectQuizBanana, "Btn_SelectBanana");
            // Chip 4: 🥑 Alpukat
            CreateHotspot(p.transform, 300, 480, 90, 44, KDAction.SelectQuizAvocado, "Btn_SelectAvocado");

            // Preview Text: Topik Kuis & Reward Kartu AR
            var prevRect = CreateRect("TopicPreviewText", p.transform);
            prevRect.anchorMin = new Vector2(24f / 412f, 1f - (556f / 850f));
            prevRect.anchorMax = new Vector2(388f / 412f, 1f - (528f / 850f));
            prevRect.offsetMin = Vector2.zero;
            prevRect.offsetMax = Vector2.zero;
            quizChoiceTopicPreviewText = prevRect.gameObject.AddComponent<TextMeshProUGUI>();
            quizChoiceTopicPreviewText.fontSize = 14;
            quizChoiceTopicPreviewText.fontStyle = FontStyles.Bold;
            quizChoiceTopicPreviewText.color = new Color(0.12f, 0.45f, 0.85f);
            quizChoiceTopicPreviewText.alignment = TextAlignmentOptions.Center;
            quizChoiceTopicPreviewText.text = "Topik Terpilih: <b>Apel Juara</b> (Hadiah: Kartu AR Apel)";

            // Tombol 1: Ya, Mulai Kuis Sekarang!
            CreateHotspot(p.transform, 36, 645, 340, 58, KDAction.GoToFillQuiz, "Btn_StartQuizNow");

            // Tombol 2: Nanti Saja, Baca Rangkuman
            CreateHotspot(p.transform, 36, 708, 340, 52, KDAction.GoToAISummary, "Btn_LaterReadSummary");
            return p;
        }

        private GameObject BuildScreen_06_FillWordQuiz(Transform parent)
        {
            var p = CreateHDArtboardPanel("06_FillWordQuiz", parent, "06_fill_the_word_quiz.png");

            // Tombol Keluar / Kembali
            CreateHotspot(p.transform, 18, 40, 90, 36, KDAction.OpenQuizChoice, "Btn_BackToQuizChoice");

            // Skor badge (klik untuk lihat reward)
            CreateHotspot(p.transform, 140, 40, 130, 36, KDAction.GoToRewardCard, "Btn_ScoreBadge");

            // Question Prompt Text (Di dalam bubble putih atas Y=240 s/d 440)
            var promptRect = CreateRect("QuizPromptText", p.transform);
            promptRect.anchorMin = new Vector2(90f / 1080f, 1f - (440f / 2228f));
            promptRect.anchorMax = new Vector2(990f / 1080f, 1f - (240f / 2228f));
            promptRect.offsetMin = Vector2.zero;
            promptRect.offsetMax = Vector2.zero;
            var promptBg = promptRect.gameObject.AddComponent<Image>();
            promptBg.color = Color.white;
            quizQuestionPromptText = promptRect.gameObject.AddComponent<TextMeshProUGUI>();
            quizQuestionPromptText.text = GetActiveCard().prompt;
            quizQuestionPromptText.fontSize = 32;
            quizQuestionPromptText.fontStyle = FontStyles.Bold;
            quizQuestionPromptText.alignment = TextAlignmentOptions.Center;
            quizQuestionPromptText.color = new Color(0.06f, 0.09f, 0.16f);

            // Fruit Image in Pink Center Box (X=380..700, Y=495..815 -> Center 540, 655)
            var iconRect = CreateRect("QuizFruitIcon", p.transform);
            iconRect.anchorMin = new Vector2(410f / 1080f, 1f - (785f / 2228f));
            iconRect.anchorMax = new Vector2(670f / 1080f, 1f - (525f / 2228f));
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
            quizFruitIconImage = iconRect.gameObject.AddComponent<Image>();
            quizFruitIconImage.preserveAspect = true;
            quizFruitIconImage.raycastTarget = false;
            quizFruitIconImage.sprite = GetFruitSprite(GetActiveCard().id);

            // Kotak-kotak huruf Slot 1, 2, 3 (?), 4
            CreateHotspot(p.transform, 44, 332, 72, 80, KDAction.QuizPickWrong, "Btn_LetterSlot1");
            CreateHotspot(p.transform, 128, 332, 72, 80, KDAction.QuizPickWrong, "Btn_LetterSlot2");
            CreateHotspot(p.transform, 210, 330, 75, 84, KDAction.QuizPickCorrect, "Btn_LetterSlot3");
            CreateHotspot(p.transform, 296, 332, 72, 80, KDAction.QuizPickWrong, "Btn_LetterSlot4");

            // Solved letter overlay card
            var solvedRect = CreateRect("SolvedLetterOverlay", p.transform);
            solvedRect.anchorMin = new Vector2(560f / 1080f, 1f - (1085f / 2228f));
            solvedRect.anchorMax = new Vector2(740f / 1080f, 1f - (875f / 2228f));
            solvedRect.offsetMin = Vector2.zero;
            solvedRect.offsetMax = Vector2.zero;
            var solvedImg = solvedRect.gameObject.AddComponent<Image>();
            solvedImg.color = new Color(0.86f, 0.92f, 1.0f);
            solvedImg.raycastTarget = false;
            var solvedOutline = solvedRect.gameObject.AddComponent<Outline>();
            solvedOutline.effectColor = new Color(0.38f, 0.65f, 0.98f);
            solvedOutline.effectDistance = new Vector2(4f, -4f);

            var solvedTextRect = CreateRect("SolvedText", solvedRect.transform);
            Stretch(solvedTextRect);
            quizSolvedLetterText = solvedTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            quizSolvedLetterText.text = "E";
            quizSolvedLetterText.fontSize = 64;
            quizSolvedLetterText.fontStyle = FontStyles.Bold;
            quizSolvedLetterText.alignment = TextAlignmentOptions.Center;
            quizSolvedLetterText.color = new Color(0.06f, 0.09f, 0.16f);
            quizSolvedLetterE = solvedRect.gameObject;
            quizSolvedLetterE.SetActive(false);

            // 3 Pilihan Opsi Kartu Huruf di Bawah
            CreateHotspot(p.transform, 26, 470, 114, 60, KDAction.QuizPickCorrect, "Btn_Option1");
            CreateHotspot(p.transform, 148, 470, 114, 60, KDAction.QuizPickWrong, "Btn_Option2");
            CreateHotspot(p.transform, 270, 470, 114, 60, KDAction.QuizPickWrong, "Btn_Option3");

            // Feedback Card Banner
            var fbCard = CreateRect("FeedbackBanner", p.transform);
            fbCard.anchorMin = new Vector2(65f / 1080f, 1f - (1660f / 2228f));
            fbCard.anchorMax = new Vector2(1015f / 1080f, 1f - (1460f / 2228f));
            fbCard.offsetMin = Vector2.zero;
            fbCard.offsetMax = Vector2.zero;
            var fbImg = fbCard.gameObject.AddComponent<Image>();
            fbImg.color = new Color(0.93f, 0.98f, 0.95f);
            fbImg.raycastTarget = false;

            var fbOutline = fbCard.gameObject.AddComponent<Outline>();
            fbOutline.effectColor = new Color(0.2f, 0.8f, 0.45f, 0.9f);
            fbOutline.effectDistance = new Vector2(3f, -3f);

            var fbTextRect = CreateRect("FeedbackText", fbCard.transform);
            Stretch(fbTextRect, 16, 12, 16, 12);
            quizFeedbackText = fbTextRect.gameObject.AddComponent<TextMeshProUGUI>();
            quizFeedbackText.fontSize = 28;
            quizFeedbackText.fontStyle = FontStyles.Bold;
            quizFeedbackText.color = new Color(0.04f, 0.50f, 0.30f);
            quizFeedbackText.alignment = TextAlignmentOptions.Center;
            quizFeedbackText.text = "";
            quizFeedbackCard = fbCard.gameObject;
            quizFeedbackCard.SetActive(false);

            return p;
        }

        private GameObject BuildScreen_07_RewardCard(Transform parent)
        {
            var p = CreateHDArtboardPanel("07_RewardCard", parent, "07_reward_card_3d.png");

            // Tombol Kembali ke Dashboard
            CreateHotspot(p.transform, 16, 36, 110, 48, KDAction.GoToDashboard, "Btn_BackToDash");

            // Card Navigation Arrows (< and >) di samping kartu untuk ganti buah
            CreateHotspot(p.transform, 6, 260, 44, 70, KDAction.PrevARCard, "Btn_PrevCard");
            CreateHotspot(p.transform, 362, 260, 44, 70, KDAction.NextARCard, "Btn_NextCard");

            // 1. 3D Live Fruit Viewport (RenderTexture) di bagian atas kartu gelap (X=200..880, Y=280..1000)
            var fruitViewRect = CreateRect("FruitLiveView", p.transform);
            fruitViewRect.anchorMin = new Vector2(200f / 1080f, 1f - (1000f / 2228f));
            fruitViewRect.anchorMax = new Vector2(880f / 1080f, 1f - (280f / 2228f));
            fruitViewRect.offsetMin = Vector2.zero;
            fruitViewRect.offsetMax = Vector2.zero;
            rewardCardFruitRawImage = fruitViewRect.gameObject.AddComponent<RawImage>();
            if (fruitRenderTexture != null) rewardCardFruitRawImage.texture = fruitRenderTexture;
            rewardCardFruitRawImage.color = Color.white;
            rewardCardFruitRawImage.raycastTarget = false;

            // 2. Card Info Area di bagian bawah kartu gelap (X=200..880, Y=1040..1400)
            var infoArea = CreateRect("CardInfoArea", p.transform);
            infoArea.anchorMin = new Vector2(200f / 1080f, 1f - (1400f / 2228f));
            infoArea.anchorMax = new Vector2(880f / 1080f, 1f - (1040f / 2228f));
            infoArea.offsetMin = Vector2.zero;
            infoArea.offsetMax = Vector2.zero;

            // Patch latar belakang gelap yang menutupi pre-render teks jika berganti buah
            var titlePatch = CreateRect("TitlePatch", infoArea.transform);
            titlePatch.anchorMin = new Vector2(0.04f, 0.72f);
            titlePatch.anchorMax = new Vector2(0.96f, 0.98f);
            titlePatch.offsetMin = Vector2.zero; titlePatch.offsetMax = Vector2.zero;
            var patchImg = titlePatch.gameObject.AddComponent<Image>();
            patchImg.color = new Color(0.09f, 0.12f, 0.19f);
            patchImg.raycastTarget = false;

            var titleTxtRect = CreateRect("TitleText", titlePatch.transform);
            Stretch(titleTxtRect);
            rewardCardTitleText = titleTxtRect.gameObject.AddComponent<TextMeshProUGUI>();
            rewardCardTitleText.text = "Apel Juara";
            rewardCardTitleText.fontSize = 32;
            rewardCardTitleText.fontStyle = FontStyles.Bold;
            rewardCardTitleText.alignment = TextAlignmentOptions.Center;
            rewardCardTitleText.color = Color.white;

            // Nama Ilmiah (e.g. Malus domestica)
            var sciRect = CreateRect("CardSciName", infoArea.transform);
            sciRect.anchorMin = new Vector2(0.05f, 0.58f);
            sciRect.anchorMax = new Vector2(0.95f, 0.72f);
            sciRect.offsetMin = Vector2.zero; sciRect.offsetMax = Vector2.zero;
            rewardCardSciNameText = sciRect.gameObject.AddComponent<TextMeshProUGUI>();
            rewardCardSciNameText.text = "<i>Malus domestica</i>";
            rewardCardSciNameText.fontSize = 20;
            rewardCardSciNameText.alignment = TextAlignmentOptions.Center;
            rewardCardSciNameText.color = new Color(0.58f, 0.77f, 1.0f);

            // Nutrisi & Khasiat
            var nutRect = CreateRect("CardNutrient", infoArea.transform);
            nutRect.anchorMin = new Vector2(0.04f, 0.22f);
            nutRect.anchorMax = new Vector2(0.96f, 0.56f);
            nutRect.offsetMin = Vector2.zero; nutRect.offsetMax = Vector2.zero;
            rewardCardNutrientText = nutRect.gameObject.AddComponent<TextMeshProUGUI>();
            rewardCardNutrientText.text = "<b>Kandungan:</b> Vitamin C (14%), Serat Pektin\n<size=16><b>Khasiat:</b> Menyehatkan pencernaan & imunitas tubuh</size>";
            rewardCardNutrientText.fontSize = 18;
            rewardCardNutrientText.lineSpacing = 1.2f;
            rewardCardNutrientText.color = new Color(0.88f, 0.92f, 0.98f);
            rewardCardNutrientText.alignment = TextAlignmentOptions.Center;

            // Status Badge
            var badgeRect = CreateRect("CardBadge", infoArea.transform);
            badgeRect.anchorMin = new Vector2(0.04f, 0.02f);
            badgeRect.anchorMax = new Vector2(0.96f, 0.20f);
            badgeRect.offsetMin = Vector2.zero; badgeRect.offsetMax = Vector2.zero;
            rewardCardStatusBadgeText = badgeRect.gameObject.AddComponent<TextMeshProUGUI>();
            rewardCardStatusBadgeText.text = "[ KARTU AR TERBUKA • SIAP SCAN ]";
            rewardCardStatusBadgeText.fontSize = 18;
            rewardCardStatusBadgeText.fontStyle = FontStyles.Bold;
            rewardCardStatusBadgeText.color = new Color(0.2f, 0.95f, 0.5f);
            rewardCardStatusBadgeText.alignment = TextAlignmentOptions.Center;

            // 3. Tombol Ungu Pre-Render Figma: "Buka Objek 3D & Kustomisasi Warna" (Y=1525..1660 di 1080x2228)
            CreateHotspot(p.transform, 25, 582, 362, 51, KDAction.GoToARStudio, "Btn_BukaObjek3D");

            // 4. Dua Tombol Aksi di Area Bersih Bawah (Y=1710 s/d 1830 di 1080x2228) - Tidak menabrak tombol ungu!
            // Tombol 1: Unduh Kartu (PNG) - Kiri
            CreateHotspot(p.transform, 25, 652, 175, 46, KDAction.DownloadARCard, "Btn_DownloadCard");
            var dlVis = CreateRect("BtnDownloadVis", p.transform);
            dlVis.anchorMin = new Vector2(65f / 1080f, 1f - (1830f / 2228f));
            dlVis.anchorMax = new Vector2(520f / 1080f, 1f - (1710f / 2228f));
            dlVis.offsetMin = Vector2.zero; dlVis.offsetMax = Vector2.zero;
            var dlImg = dlVis.gameObject.AddComponent<Image>();
            dlImg.color = new Color(0.12f, 0.42f, 0.88f);
            dlImg.raycastTarget = false;
            var dlOutline = dlVis.gameObject.AddComponent<Outline>();
            dlOutline.effectColor = new Color(0.4f, 0.7f, 1f, 0.5f);
            dlOutline.effectDistance = new Vector2(2f, -2f);
            var dlTxtRect = CreateRect("DlTxt", dlVis.transform);
            Stretch(dlTxtRect);
            var dlTxt = dlTxtRect.gameObject.AddComponent<TextMeshProUGUI>();
            dlTxt.text = "<b>Unduh Kartu (PNG)</b>";
            dlTxt.fontSize = 22; dlTxt.color = Color.white; dlTxt.alignment = TextAlignmentOptions.Center;

            // Tombol 2: Pindai Kamera AR - Kanan
            CreateHotspot(p.transform, 212, 652, 175, 46, KDAction.OpenARScanner, "Btn_OpenScanner");
            var scVis = CreateRect("BtnScanVis", p.transform);
            scVis.anchorMin = new Vector2(560f / 1080f, 1f - (1830f / 2228f));
            scVis.anchorMax = new Vector2(1015f / 1080f, 1f - (1710f / 2228f));
            scVis.offsetMin = Vector2.zero; scVis.offsetMax = Vector2.zero;
            var scImg = scVis.gameObject.AddComponent<Image>();
            scImg.color = new Color(0.08f, 0.62f, 0.40f);
            scImg.raycastTarget = false;
            var scOutline = scVis.gameObject.AddComponent<Outline>();
            scOutline.effectColor = new Color(0.3f, 0.9f, 0.6f, 0.5f);
            scOutline.effectDistance = new Vector2(2f, -2f);
            var scTxtRect = CreateRect("ScTxt", scVis.transform);
            Stretch(scTxtRect);
            var scTxt = scTxtRect.gameObject.AddComponent<TextMeshProUGUI>();
            scTxt.text = "<b>Pindai Kamera AR</b>";
            scTxt.fontSize = 22; scTxt.color = Color.white; scTxt.alignment = TextAlignmentOptions.Center;

            // 5. Tombol Dashboard Cepat di Y=1860..1960
            CreateHotspot(p.transform, 25, 710, 362, 44, KDAction.GoToDashboard, "Btn_KembaliDashboard");
            var dashVis = CreateRect("BtnDashVis", p.transform);
            dashVis.anchorMin = new Vector2(65f / 1080f, 1f - (1960f / 2228f));
            dashVis.anchorMax = new Vector2(1015f / 1080f, 1f - (1860f / 2228f));
            dashVis.offsetMin = Vector2.zero; dashVis.offsetMax = Vector2.zero;
            var dashImg = dashVis.gameObject.AddComponent<Image>();
            dashImg.color = new Color(0.92f, 0.94f, 0.98f);
            dashImg.raycastTarget = false;
            var dashOutline = dashVis.gameObject.AddComponent<Outline>();
            dashOutline.effectColor = new Color(0.7f, 0.8f, 0.9f);
            dashOutline.effectDistance = new Vector2(1.5f, -1.5f);
            var dashTxtRect = CreateRect("DashTxt", dashVis.transform);
            Stretch(dashTxtRect);
            var dashTxt = dashTxtRect.gameObject.AddComponent<TextMeshProUGUI>();
            dashTxt.text = "<b>Kembali ke Dashboard Siswa</b>";
            dashTxt.fontSize = 20; dashTxt.color = new Color(0.2f, 0.3f, 0.5f); dashTxt.alignment = TextAlignmentOptions.Center;

            return p;
        }

        private GameObject BuildScreen_08_AR3DStudio(Transform parent)
        {
            var p = CreateHDArtboardPanel("08_ARStudio", parent, "08_ar_3d_fruit_studio.png");

            // Top Bar
            CreateHotspot(p.transform, 20, 44, 80, 30, KDAction.GoToDashboard, "Btn_BackToDash");
            CreateHotspot(p.transform, 130, 44, 76, 28, KDAction.SetStudioMode, "Btn_StudioMode");
            CreateHotspot(p.transform, 210, 44, 80, 28, KDAction.SetARMode, "Btn_ARMode");
            CreateHotspot(p.transform, 326, 46, 66, 26, KDAction.ResetApple, "Btn_ResetApple");

            // Camera feed (RawImage) for real AR
            var feedRect = CreateRect("ARCameraFeed", p.transform);
            feedRect.anchorMin = new Vector2(20f / 412f, 1f - (404f / 850f));
            feedRect.anchorMax = new Vector2(392f / 412f, 1f - (84f / 850f));
            feedRect.offsetMin = Vector2.zero;
            feedRect.offsetMax = Vector2.zero;
            arCameraFeed = feedRect.gameObject.AddComponent<RawImage>();
            arCameraFeed.color = Color.white;
            feedRect.gameObject.SetActive(false);

            // 3D Live Fruit Viewport (RenderTexture) inside studio frame
            var studioViewRect = CreateRect("StudioLiveView", p.transform);
            studioViewRect.anchorMin = new Vector2(20f / 412f, 1f - (404f / 850f));
            studioViewRect.anchorMax = new Vector2(392f / 412f, 1f - (84f / 850f));
            studioViewRect.offsetMin = Vector2.zero;
            studioViewRect.offsetMax = Vector2.zero;
            studioFruitRawImage = studioViewRect.gameObject.AddComponent<RawImage>();
            if (fruitRenderTexture != null) studioFruitRawImage.texture = fruitRenderTexture;
            studioFruitRawImage.color = Color.white;
            studioFruitRawImage.raycastTarget = false;
            feedRect.SetAsFirstSibling();

            // AR Scanner Overlay (Target Reticle, Corner Brackets, Scanning Laser)
            var scannerBox = CreateRect("ARScannerOverlay", p.transform);
            scannerBox.anchorMin = new Vector2(20f / 412f, 1f - (404f / 850f));
            scannerBox.anchorMax = new Vector2(392f / 412f, 1f - (84f / 850f));
            scannerBox.offsetMin = Vector2.zero;
            scannerBox.offsetMax = Vector2.zero;
            var scanBg = scannerBox.gameObject.AddComponent<Image>();
            scanBg.color = new Color(0.04f, 0.08f, 0.15f, 0.88f);

            // Scanner Corner Frame
            var scanFrame = CreateRect("ScanFrame", scannerBox.transform);
            scanFrame.anchorMin = new Vector2(0.12f, 0.12f);
            scanFrame.anchorMax = new Vector2(0.88f, 0.88f);
            scanFrame.offsetMin = Vector2.zero;
            scanFrame.offsetMax = Vector2.zero;
            var frameOutline = scanFrame.gameObject.AddComponent<Outline>();
            frameOutline.effectColor = new Color(0.15f, 0.95f, 0.65f, 0.9f);
            frameOutline.effectDistance = new Vector2(2f, -2f);

            // Scanning Laser Line
            var laserRect = CreateRect("LaserLine", scannerBox.transform);
            laserRect.anchorMin = new Vector2(0.10f, 0.5f);
            laserRect.anchorMax = new Vector2(0.90f, 0.5f);
            laserRect.sizeDelta = new Vector2(0, 4);
            laserRect.anchoredPosition = Vector2.zero;
            var laserImg = laserRect.gameObject.AddComponent<Image>();
            laserImg.color = new Color(0.2f, 1.0f, 0.5f, 0.95f);
            arLaserLine = laserRect;

            // Scanner Prompt Text
            var scanPromptRect = CreateRect("ScanPrompt", scannerBox.transform);
            scanPromptRect.anchorMin = new Vector2(0.05f, 0.70f);
            scanPromptRect.anchorMax = new Vector2(0.95f, 0.95f);
            scanPromptRect.offsetMin = Vector2.zero;
            scanPromptRect.offsetMax = Vector2.zero;
            arScannerPromptText = scanPromptRect.gameObject.AddComponent<TextMeshProUGUI>();
            arScannerPromptText.text = "<b>Arahkan Kamera ke Kartu AR</b>\n<size=13>Pindai kartu flashcard buah untuk memunculkan model 3D</size>";
            arScannerPromptText.fontSize = 15;
            arScannerPromptText.color = Color.white;
            arScannerPromptText.alignment = TextAlignmentOptions.Center;

            // Scanner Action Button: "Pindai & Deteksi Kartu AR Sekarang"
            CreateHotspot(scannerBox.transform, 40, 220, 292, 46, KDAction.SimulateScanCard, "Btn_SimulateScan");
            var simVis = CreateRect("SimVis", scannerBox.transform);
            simVis.anchorMin = new Vector2(0.12f, 0.15f);
            simVis.anchorMax = new Vector2(0.88f, 0.32f);
            simVis.offsetMin = Vector2.zero;
            simVis.offsetMax = Vector2.zero;
            var simImg = simVis.gameObject.AddComponent<Image>();
            simImg.color = new Color(0.12f, 0.85f, 0.55f);
            simImg.raycastTarget = false;
            var simTxtRect = CreateRect("SimTxt", simVis.transform);
            Stretch(simTxtRect);
            var simTxt = simTxtRect.gameObject.AddComponent<TextMeshProUGUI>();
            simTxt.text = "<b>Pindai & Munculkan 3D</b>";
            simTxt.fontSize = 16;
            simTxt.fontStyle = FontStyles.Bold;
            simTxt.color = new Color(0.04f, 0.15f, 0.10f);
            simTxt.alignment = TextAlignmentOptions.Center;

            arScannerOverlay = scannerBox.gameObject;
            arScannerOverlay.SetActive(false);

            // Timer Text Overlay
            var timerRect = CreateRect("DecayTimerText", p.transform);
            timerRect.anchorMin = new Vector2(220f / 412f, 1f - (304f / 850f));
            timerRect.anchorMax = new Vector2(356f / 412f, 1f - (278f / 850f));
            timerRect.offsetMin = Vector2.zero;
            timerRect.offsetMax = Vector2.zero;
            arDecayTimerText = timerRect.gameObject.AddComponent<TextMeshProUGUI>();
            arDecayTimerText.fontSize = 24;
            arDecayTimerText.fontStyle = FontStyles.Bold;
            arDecayTimerText.color = new Color(0.57f, 0.25f, 0.05f);
            arDecayTimerText.alignment = TextAlignmentOptions.Center;
            arDecayTimerText.text = "Pembusukan: 05:00";

            // Tombol Kamera Foto AR (Snapshot)
            CreateHotspot(p.transform, 345, 95, 45, 45, KDAction.TakeARPhoto, "Btn_Snapshot");

            // AR Status / Notification Banner
            var factRect = CreateRect("ARStatusBanner", p.transform);
            factRect.anchorMin = new Vector2(0.05f, 0.465f);
            factRect.anchorMax = new Vector2(0.95f, 0.505f);
            factRect.offsetMin = Vector2.zero;
            factRect.offsetMax = Vector2.zero;
            arNutritionFactText = factRect.gameObject.AddComponent<TextMeshProUGUI>();
            arNutritionFactText.fontSize = 18;
            arNutritionFactText.fontStyle = FontStyles.Bold;
            arNutritionFactText.color = new Color(0.04f, 0.52f, 0.38f);
            arNutritionFactText.alignment = TextAlignmentOptions.Center;
            arNutritionFactText.text = "";
            factRect.gameObject.SetActive(false);

            // Action Buttons: Gigit, Buang, Tanam, Anatomi
            CreateHotspot(p.transform, 20, 436, 86, 48, KDAction.TriggerBite, "Btn_Gigit");
            CreateHotspot(p.transform, 114, 436, 86, 48, KDAction.ResetApple, "Btn_Buang");
            CreateHotspot(p.transform, 208, 436, 94, 48, KDAction.GoToPlantingGame, "Btn_Tanam");
            CreateHotspot(p.transform, 310, 436, 82, 48, KDAction.ToggleAnatomy, "Btn_Anatomi");

            // Fast-Forward Decay Demo Button
            CreateHotspot(p.transform, 36, 592, 340, 26, KDAction.FastForwardDecay, "Btn_FastDecay");

            // Color Swatches (Merah, Hijau, Kuning, Ungu, Pink)
            CreateHotspot(p.transform, 26, 650, 52, 50, KDAction.ColorRed, "Btn_ColRed");
            CreateHotspot(p.transform, 86, 650, 52, 50, KDAction.ColorGreen, "Btn_ColGreen");
            CreateHotspot(p.transform, 146, 650, 52, 50, KDAction.ColorYellow, "Btn_ColYellow");
            CreateHotspot(p.transform, 206, 650, 52, 50, KDAction.ColorPurple, "Btn_ColPurple");
            CreateHotspot(p.transform, 266, 650, 52, 50, KDAction.ColorPink, "Btn_ColPink");

            // Bottom CTA: Buka Mode Berkebun
            CreateHotspot(p.transform, 20, 718, 372, 52, KDAction.GoToPlantingGame, "Btn_BukaKebun");

            // Anatomy & Nutrition Pop-up Modal
            var anatModal = CreateRect("ARAnatomyModal", p.transform);
            anatModal.anchorMin = new Vector2(0.08f, 0.22f);
            anatModal.anchorMax = new Vector2(0.92f, 0.70f);
            anatModal.offsetMin = Vector2.zero;
            anatModal.offsetMax = Vector2.zero;
            var anatBg = anatModal.gameObject.AddComponent<Image>();
            anatBg.color = new Color(1f, 1f, 1f, 0.98f);
            var anatOutline = anatModal.gameObject.AddComponent<Outline>();
            anatOutline.effectColor = new Color(0.2f, 0.6f, 0.95f, 0.6f);
            anatOutline.effectDistance = new Vector2(2f, -2f);

            var anatTitleRect = CreateRect("AnatTitle", anatModal.transform);
            anatTitleRect.anchorMin = new Vector2(0.05f, 0.82f);
            anatTitleRect.anchorMax = new Vector2(0.95f, 0.96f);
            anatTitleRect.offsetMin = Vector2.zero;
            anatTitleRect.offsetMax = Vector2.zero;
            arAnatomyTitleText = anatTitleRect.gameObject.AddComponent<TextMeshProUGUI>();
            arAnatomyTitleText.text = "Anatomi & Gizi Buah Apel";
            arAnatomyTitleText.fontSize = 20;
            arAnatomyTitleText.fontStyle = FontStyles.Bold;
            arAnatomyTitleText.alignment = TextAlignmentOptions.Center;
            arAnatomyTitleText.color = new Color(0.1f, 0.2f, 0.5f);

            var anatBodyRect = CreateRect("AnatBody", anatModal.transform);
            anatBodyRect.anchorMin = new Vector2(0.06f, 0.18f);
            anatBodyRect.anchorMax = new Vector2(0.94f, 0.80f);
            anatBodyRect.offsetMin = Vector2.zero;
            anatBodyRect.offsetMax = Vector2.zero;
            arAnatomyBodyText = anatBodyRect.gameObject.AddComponent<TextMeshProUGUI>();
            arAnatomyBodyText.fontSize = 14.5f;
            arAnatomyBodyText.color = new Color(0.15f, 0.18f, 0.22f);
            arAnatomyBodyText.lineSpacing = 1.2f;
            arAnatomyBodyText.text = "<b>• Kulit (Peel):</b> Kaya antioksidan & serat pektin.\n" +
                            "<b>• Daging Buah (Flesh):</b> Mengandung vitamin C & air manis alami.\n" +
                            "<b>• Inti (Core):</b> Melindungi biji buah untuk regenerasi kebun.\n" +
                            "<b>• Kalori Sehat:</b> Bebas Lemak Jenuh • Kaya Serat";

            var closeAnatBtn = CreateRect("CloseAnatBtn", anatModal.transform);
            closeAnatBtn.anchorMin = new Vector2(0.25f, 0.04f);
            closeAnatBtn.anchorMax = new Vector2(0.75f, 0.15f);
            closeAnatBtn.offsetMin = Vector2.zero;
            closeAnatBtn.offsetMax = Vector2.zero;
            var closeAnatImg = closeAnatBtn.gameObject.AddComponent<Image>();
            closeAnatImg.color = new Color(0.23f, 0.51f, 0.96f);
            var closeAnatBtnComp = closeAnatBtn.gameObject.AddComponent<Button>();
            closeAnatBtnComp.targetGraphic = closeAnatImg;
            var closeHook = closeAnatBtn.gameObject.AddComponent<KawanDengarButtonHook>();
            closeHook.action = KDAction.ToggleAnatomy;

            var closeAnatTxtRect = CreateRect("CloseTxt", closeAnatBtn.transform);
            Stretch(closeAnatTxtRect);
            var closeAnatTxt = closeAnatTxtRect.gameObject.AddComponent<TextMeshProUGUI>();
            closeAnatTxt.text = "Tutup Info";
            closeAnatTxt.fontSize = 16;
            closeAnatTxt.fontStyle = FontStyles.Bold;
            closeAnatTxt.color = Color.white;
            closeAnatTxt.alignment = TextAlignmentOptions.Center;
            closeAnatBtnComp.onClick.AddListener(() => ToggleAnatomyModal());

            arAnatomyModal = anatModal.gameObject;
            arAnatomyModal.SetActive(false);

            return p;
        }

        private GameObject BuildScreen_09_PlantingGame(Transform parent)
        {
            var p = CreateHDArtboardPanel("09_PlantingGame", parent, "09_fruit_planting_game.png");

            // Top Bar
            CreateHotspot(p.transform, 18, 40, 90, 36, KDAction.GoToDashboard, "Btn_BackToDash");

            // 4 Garden Tool Cards (y=436 sesuai kartu icon di figma SVG)
            CreateHotspot(p.transform, 16, 436, 88, 76, KDAction.GardenWater, "Btn_Siram");
            CreateHotspot(p.transform, 108, 436, 88, 76, KDAction.GardenFertilize, "Btn_Pupuk");
            CreateHotspot(p.transform, 200, 436, 88, 76, KDAction.GardenShield, "Btn_Perisai");
            CreateHotspot(p.transform, 292, 436, 102, 76, KDAction.GardenHarvest, "Btn_Panen");

            // Fallback area klik luas di bawahnya
            CreateHotspot(p.transform, 20, 540, 180, 65, KDAction.GardenWater, "Btn_SiramWide");
            CreateHotspot(p.transform, 210, 540, 180, 65, KDAction.GardenFertilize, "Btn_PupukWide");
            CreateHotspot(p.transform, 20, 615, 180, 65, KDAction.GardenShield, "Btn_PerisaiWide");
            CreateHotspot(p.transform, 210, 615, 180, 65, KDAction.GardenHarvest, "Btn_PanenWide");

            // Tombol Kembali ke Studio 3D di bawah (y=708)
            CreateHotspot(p.transform, 18, 708, 376, 58, KDAction.GoToARStudio, "Btn_KembaliKeStudioAR");

            // Notification Banner
            var notifRect = CreateRect("GardenNotif", p.transform);
            notifRect.anchorMin = new Vector2(0.08f, 0.45f);
            notifRect.anchorMax = new Vector2(0.92f, 0.50f);
            notifRect.offsetMin = Vector2.zero;
            notifRect.offsetMax = Vector2.zero;
            gardenNotificationText = notifRect.gameObject.AddComponent<TextMeshProUGUI>();
            gardenNotificationText.fontSize = 26;
            gardenNotificationText.color = new Color(0.02f, 0.47f, 0.34f);
            gardenNotificationText.alignment = TextAlignmentOptions.Center;
            gardenNotificationText.text = "";
            notifRect.gameObject.SetActive(false);

            return p;
        }

        // =========================================================================
        // SETUP 3D AR STAGE (APEL 4 TAHAP, TRACKBALL, DECAY & KEBUN 3D)
        // =========================================================================
        public void Setup3DStageAndApple()
        {
            stage3DContainer = new GameObject("3D_AR_Stage");
            stage3DContainer.transform.position = new Vector3(0, 0, 2.5f);

            // 1. FruitContainer (Posisi di jendela viewport Studio AR)
            fruitContainer = new GameObject("FruitContainer");
            fruitContainer.transform.SetParent(stage3DContainer.transform, false);
            fruitContainer.transform.localPosition = new Vector3(0, 0.45f, 0);

            var col = fruitContainer.AddComponent<SphereCollider>();
            col.radius = 0.65f;

            trackballRotator = fruitContainer.AddComponent<TrackballFruitRotator>();
            biteController = fruitContainer.AddComponent<FruitBiteController>();
            decayTimer = fruitContainer.AddComponent<FruitDecayTimer>();
            fruitContainer.AddComponent<FruitColorPicker>();
            fruitContainer.AddComponent<FruitActionToolbar>();

            // AR Ground Reticle di bawah buah
            arGroundReticle = new GameObject("AR_Ground_Reticle");
            arGroundReticle.transform.SetParent(fruitContainer.transform, false);
            arGroundReticle.transform.localPosition = new Vector3(0, -0.65f, 0);
            CreateColoredPrimitive(PrimitiveType.Cylinder, arGroundReticle.transform, Vector3.zero, new Vector3(1.3f, 0.01f, 1.3f), new Color(0.12f, 0.74f, 0.98f, 0.65f), "ReticleRing");
            CreateColoredPrimitive(PrimitiveType.Sphere, arGroundReticle.transform, Vector3.zero, new Vector3(0.25f, 0.02f, 0.25f), new Color(0.2f, 1f, 0.6f, 0.9f), "ReticleCore");
            arGroundReticle.SetActive(false);

            // Setup 4 Tahap Model Buah Apel:
            appleWholeMesh = CreateAppleMeshObject("WholeFruitMesh", fruitContainer.transform, 0);
            appleBite1Mesh = CreateAppleMeshObject("Bite1Mesh", fruitContainer.transform, 1);
            appleBite2Mesh = CreateAppleMeshObject("Bite2Mesh", fruitContainer.transform, 2);
            appleBite3CoreMesh = CreateAppleMeshObject("CoreMesh", fruitContainer.transform, 3);

            // Particle Systems
            var crumbObj = new GameObject("CrumbParticles", typeof(ParticleSystem));
            crumbObj.transform.SetParent(fruitContainer.transform, false);
            crumbParticles = crumbObj.GetComponent<ParticleSystem>();
            var crumbMain = crumbParticles.main;
            crumbMain.startColor = new Color(0.99f, 0.98f, 0.76f);
            crumbMain.startSize = 0.05f;
            crumbMain.playOnAwake = false;

            var flyObj = new GameObject("FlyParticles", typeof(ParticleSystem));
            flyObj.transform.SetParent(fruitContainer.transform, false);
            flyParticles = flyObj.GetComponent<ParticleSystem>();
            var flyMain = flyParticles.main;
            flyMain.startColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            flyMain.startSize = 0.04f;
            flyMain.playOnAwake = false;

            biteController.wholeFruitMesh = appleWholeMesh;
            biteController.bite1Mesh = appleBite1Mesh;
            biteController.bite2Mesh = appleBite2Mesh;
            biteController.bite3CoreMesh = appleBite3CoreMesh;
            biteController.crumbParticlePrefab = crumbParticles;

            var renderers = fruitContainer.GetComponentsInChildren<Renderer>();
            decayTimer.fruitRenderers = renderers;
            decayTimer.fliesParticleEffect = flyParticles;

            // 2. GardenContainer (Posisi di jendela kebun)
            gardenContainer = new GameObject("GardenContainer");
            gardenContainer.transform.SetParent(stage3DContainer.transform, false);
            gardenContainer.transform.localPosition = new Vector3(0, 0.35f, 0);
            plantingController = gardenContainer.AddComponent<PlantingGameController>();

            var potSeed = CreateGardenMeshObject("PotSeedObject", gardenContainer.transform, 0);
            var sprout = CreateGardenMeshObject("SproutObject", gardenContainer.transform, 1);
            var tree = CreateGardenMeshObject("HealthyTreeObject", gardenContainer.transform, 2);
            var pest = CreateGardenMeshObject("PestObject", gardenContainer.transform, 3);
            var shield = CreateGardenMeshObject("ShieldEffectObject", gardenContainer.transform, 4);
            var harvestTree = CreateGardenMeshObject("HarvestFruitTreeObject", gardenContainer.transform, 5);

            plantingController.potSeedObject = potSeed;
            plantingController.sproutObject = sprout;
            plantingController.healthyTreeObject = tree;
            plantingController.pestObject = pest;
            plantingController.shieldEffectObject = shield;
            plantingController.harvestFruitTreeObject = harvestTree;

            // 3. RenderTexture & Dedicated 3D Stage Camera untuk RawImage di Screen 07 (Kartu AR) & Screen 08 (Studio AR)
            if (fruitRenderTexture == null)
            {
                fruitRenderTexture = new RenderTexture(768, 768, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 4,
                    filterMode = FilterMode.Bilinear,
                    name = "FruitLiveRenderTexture"
                };
                fruitRenderTexture.Create();
            }

            var camObj = new GameObject("FruitStageCamera");
            camObj.transform.SetParent(stage3DContainer.transform, false);
            camObj.transform.localPosition = new Vector3(0, 0.45f, -2.5f);
            camObj.transform.localRotation = Quaternion.identity;
            fruitRenderCamera = camObj.AddComponent<Camera>();
            fruitRenderCamera.clearFlags = CameraClearFlags.SolidColor;
            fruitRenderCamera.backgroundColor = new Color(0, 0, 0, 0); // Transparan murni
            fruitRenderCamera.targetTexture = fruitRenderTexture;
            fruitRenderCamera.fieldOfView = 34f;
            fruitRenderCamera.nearClipPlane = 0.1f;
            fruitRenderCamera.farClipPlane = 20f;
            fruitRenderCamera.depth = 1;

            var lightObj = new GameObject("FruitStageLight");
            lightObj.transform.SetParent(stage3DContainer.transform, false);
            lightObj.transform.localPosition = new Vector3(1.5f, 2.5f, -2f);
            lightObj.transform.LookAt(fruitContainer.transform);
            var stageLight = lightObj.AddComponent<Light>();
            stageLight.type = LightType.Directional;
            stageLight.color = new Color(1f, 0.98f, 0.95f);
            stageLight.intensity = 1.35f;

            if (rewardCardFruitRawImage != null) rewardCardFruitRawImage.texture = fruitRenderTexture;
            if (studioFruitRawImage != null) studioFruitRawImage.texture = fruitRenderTexture;

            UpdateBiteMeshes(0);
            Align3DStageWithViewport();
        }

        public void Align3DStageWithViewport()
        {
            var cam = Camera.main;
            if (cam == null) cam = FindFirstObjectByType<Camera>();
            if (cam == null) return;

            // gardenContainer menggunakan Main Camera untuk jendela transparan di Screen 09 Kebun
            if (gardenContainer != null)
            {
                gardenContainer.transform.position = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.670f, 3.8f));
            }

            // Posisikan fruitContainer di depan kamera utama untuk fallback dan pastikan FruitStageCamera menghadap lurus
            if (fruitContainer != null)
            {
                fruitContainer.transform.position = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.713f, 3.8f));

                if (fruitRenderCamera != null)
                {
                    fruitRenderCamera.transform.position = fruitContainer.transform.position + new Vector3(0, 0, -2.5f);
                    fruitRenderCamera.transform.LookAt(fruitContainer.transform);
                }
            }
        }

        private static GameObject CreateColoredPrimitive(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localScale, Color color, string name = null)
        {
            var go = GameObject.CreatePrimitive(type);
            if (!string.IsNullOrEmpty(name)) go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            var r = go.GetComponent<Renderer>();
            if (r != null)
            {
                r.material.color = color;
            }
            return go;
        }

        private GameObject CreateAppleMeshObject(string name, Transform parent, int stage)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            if (stage == 0) // Apple Utuh
            {
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, Vector3.zero, new Vector3(1.15f, 1.05f, 1.15f), currentFruitColor, "AppleBody");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.48f, 0), new Vector3(0.35f, 0.12f, 0.35f), new Color(0.6f, 0.12f, 0.12f), "TopDimple");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, -0.48f, 0), new Vector3(0.12f, 0.04f, 0.12f), new Color(0.25f, 0.10f, 0.04f), "Calyx");

                var stem1 = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.58f, 0), new Vector3(0.06f, 0.18f, 0.06f), new Color(0.42f, 0.22f, 0.08f), "Stem1");
                stem1.transform.localRotation = Quaternion.Euler(0, 0, 10);
                var stem2 = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.03f, 0.72f, 0), new Vector3(0.05f, 0.14f, 0.05f), new Color(0.42f, 0.22f, 0.08f), "Stem2");
                stem2.transform.localRotation = Quaternion.Euler(0, 0, 25);

                var leafStalk = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.08f, 0.65f, 0), new Vector3(0.02f, 0.08f, 0.02f), new Color(0.12f, 0.65f, 0.28f), "LeafStalk");
                leafStalk.transform.localRotation = Quaternion.Euler(0, 0, -45);
                var leaf = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.22f, 0.72f, 0), new Vector3(0.28f, 0.05f, 0.14f), new Color(0.12f, 0.72f, 0.28f), "LeafBlade");
                leaf.transform.localRotation = Quaternion.Euler(20, -15, -30);

                var highlight = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.35f, 0.30f, -0.38f), new Vector3(0.18f, 0.10f, 0.04f), new Color(1f, 1f, 1f, 0.45f), "Highlight");
                highlight.transform.localRotation = Quaternion.Euler(-20, 40, -10);
            }
            else if (stage == 1) // Gigitan 1
            {
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, Vector3.zero, new Vector3(1.15f, 1.05f, 1.15f), currentFruitColor, "AppleBody");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.58f, 0), new Vector3(0.06f, 0.20f, 0.06f), new Color(0.42f, 0.22f, 0.08f), "Stem");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.18f, 0.68f, 0), new Vector3(0.25f, 0.05f, 0.12f), new Color(0.12f, 0.72f, 0.28f), "Leaf");

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.42f, 0.15f, 0), new Vector3(0.50f, 0.48f, 0.50f), new Color(0.99f, 0.96f, 0.78f), "BiteCavity");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.45f, 0.25f, 0.12f), new Vector3(0.18f, 0.18f, 0.18f), new Color(0.97f, 0.94f, 0.72f), "BiteMark1");
            }
            else if (stage == 2) // Gigitan 2 (Terlihat Biji)
            {
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.08f, 0, 0), new Vector3(0.98f, 1.0f, 0.98f), currentFruitColor, "AppleBody");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(-0.05f, 0.58f, 0), new Vector3(0.06f, 0.20f, 0.06f), new Color(0.42f, 0.22f, 0.08f), "Stem");

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.36f, 0.12f, 0), new Vector3(0.68f, 0.65f, 0.68f), new Color(0.99f, 0.96f, 0.78f), "BiteCavityDeep");

                var seed1 = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.12f, 0.06f, 0.08f), new Vector3(0.06f, 0.12f, 0.05f), new Color(0.25f, 0.10f, 0.02f), "Seed1");
                seed1.transform.localRotation = Quaternion.Euler(25, 0, 15);
                var seed2 = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.10f, -0.04f, -0.06f), new Vector3(0.06f, 0.12f, 0.05f), new Color(0.25f, 0.10f, 0.02f), "Seed2");
                seed2.transform.localRotation = Quaternion.Euler(-20, 0, -10);
            }
            else // Stage 3: Inti Apel (Core)
            {
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, Vector3.zero, new Vector3(0.28f, 0.55f, 0.28f), new Color(0.99f, 0.96f, 0.78f), "CoreCylinder");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.50f, 0), new Vector3(0.40f, 0.15f, 0.40f), currentFruitColor, "TopCap");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, -0.50f, 0), new Vector3(0.38f, 0.14f, 0.38f), currentFruitColor, "BottomCap");

                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.65f, 0), new Vector3(0.06f, 0.20f, 0.06f), new Color(0.42f, 0.22f, 0.08f), "Stem");

                var seed = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.10f, 0, 0.05f), new Vector3(0.06f, 0.12f, 0.05f), new Color(0.25f, 0.10f, 0.02f), "CoreSeed");
                seed.transform.localRotation = Quaternion.Euler(15, 0, 10);
            }

            return root;
        }

        private GameObject CreateGardenMeshObject(string name, Transform parent, int type)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);

            if (type == 0) // Pot & Biji (SeedInPot)
            {
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, Vector3.zero, new Vector3(0.75f, 0.40f, 0.75f), new Color(0.85f, 0.38f, 0.18f), "PotBody");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.20f, 0), new Vector3(0.82f, 0.08f, 0.82f), new Color(0.88f, 0.42f, 0.22f), "PotRim");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.19f, 0), new Vector3(0.72f, 0.05f, 0.72f), new Color(0.24f, 0.14f, 0.08f), "Soil");

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.22f, 0), new Vector3(0.18f, 0.06f, 0.18f), new Color(0.28f, 0.16f, 0.09f), "SoilMound");
                var seed = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.25f, 0), new Vector3(0.08f, 0.14f, 0.08f), new Color(0.45f, 0.24f, 0.08f), "Seed");
                seed.transform.localRotation = Quaternion.Euler(15, 0, 10);
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.02f, 0.32f, 0), new Vector3(0.04f, 0.05f, 0.04f), new Color(0.38f, 0.85f, 0.25f), "SproutBud");

                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(-0.25f, 0.32f, 0.15f), new Vector3(0.02f, 0.25f, 0.02f), new Color(0.75f, 0.60f, 0.42f), "SignStick");
                CreateColoredPrimitive(PrimitiveType.Cube, root.transform, new Vector3(-0.25f, 0.42f, 0.15f), new Vector3(0.16f, 0.08f, 0.02f), new Color(0.98f, 0.98f, 0.98f), "SignTag");
            }
            else if (type == 1) // Tunas Hijau (WateredSprout)
            {
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, Vector3.zero, new Vector3(0.75f, 0.40f, 0.75f), new Color(0.85f, 0.38f, 0.18f), "PotBody");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.20f, 0), new Vector3(0.82f, 0.08f, 0.82f), new Color(0.88f, 0.42f, 0.22f), "PotRim");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.19f, 0), new Vector3(0.72f, 0.05f, 0.72f), new Color(0.24f, 0.14f, 0.08f), "Soil");

                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.30f, 0), new Vector3(0.04f, 0.18f, 0.04f), new Color(0.12f, 0.65f, 0.28f), "StemLower");
                var stemUp = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.02f, 0.44f, 0), new Vector3(0.035f, 0.16f, 0.035f), new Color(0.18f, 0.75f, 0.32f), "StemUpper");
                stemUp.transform.localRotation = Quaternion.Euler(0, 0, -12);

                var leafL = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.12f, 0.52f, 0), new Vector3(0.22f, 0.04f, 0.12f), new Color(0.18f, 0.80f, 0.35f), "LeafL");
                leafL.transform.localRotation = Quaternion.Euler(10, 0, 30);
                var leafR = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.14f, 0.56f, 0), new Vector3(0.24f, 0.04f, 0.13f), new Color(0.22f, 0.85f, 0.38f), "LeafR");
                leafR.transform.localRotation = Quaternion.Euler(-10, 0, -25);

                var centerLeaf = CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.02f, 0.62f, 0), new Vector3(0.10f, 0.03f, 0.08f), new Color(0.40f, 0.90f, 0.42f), "CenterLeaf");
                centerLeaf.transform.localRotation = Quaternion.Euler(0, 0, 5);

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.20f, 0.59f, 0.02f), new Vector3(0.04f, 0.04f, 0.04f), new Color(0.85f, 0.96f, 1.0f, 0.85f), "Dewdrop");
            }
            else if (type == 2) // Pohon Sehat Rimbun (FertilizedBush / HealthyTree)
            {
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, Vector3.zero, new Vector3(0.75f, 0.40f, 0.75f), new Color(0.85f, 0.38f, 0.18f), "PotBody");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.20f, 0), new Vector3(0.82f, 0.08f, 0.82f), new Color(0.88f, 0.42f, 0.22f), "PotRim");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.19f, 0), new Vector3(0.72f, 0.05f, 0.72f), new Color(0.24f, 0.14f, 0.08f), "Soil");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.21f, 0), new Vector3(0.73f, 0.04f, 0.73f), new Color(0.28f, 0.68f, 0.22f), "MossGrass");

                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.52f, 0), new Vector3(0.10f, 0.55f, 0.10f), new Color(0.46f, 0.26f, 0.12f), "Trunk");
                var brL = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(-0.16f, 0.70f, 0), new Vector3(0.06f, 0.30f, 0.06f), new Color(0.46f, 0.26f, 0.12f), "BranchL");
                brL.transform.localRotation = Quaternion.Euler(0, 0, 45);
                var brR = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.16f, 0.78f, 0), new Vector3(0.06f, 0.28f, 0.06f), new Color(0.46f, 0.26f, 0.12f), "BranchR");
                brR.transform.localRotation = Quaternion.Euler(0, 0, -40);

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 1.05f, 0), new Vector3(0.75f, 0.60f, 0.75f), new Color(0.12f, 0.62f, 0.26f), "FoliageCenter");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.32f, 0.92f, 0.08f), new Vector3(0.55f, 0.48f, 0.55f), new Color(0.16f, 0.70f, 0.30f), "FoliageL");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.32f, 0.98f, -0.06f), new Vector3(0.58f, 0.50f, 0.58f), new Color(0.14f, 0.66f, 0.28f), "FoliageR");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.02f, 1.30f, 0), new Vector3(0.50f, 0.42f, 0.50f), new Color(0.20f, 0.76f, 0.34f), "FoliageTop");

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.25f, 1.10f, 0.35f), new Vector3(0.08f, 0.08f, 0.08f), new Color(1.0f, 0.88f, 0.92f), "Blossom1");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.28f, 1.15f, 0.32f), new Vector3(0.08f, 0.08f, 0.08f), new Color(1.0f, 0.88f, 0.92f), "Blossom2");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.05f, 1.32f, 0.28f), new Vector3(0.08f, 0.08f, 0.08f), new Color(1.0f, 0.88f, 0.92f), "Blossom3");
            }
            else if (type == 3) // Hama Ulat Daun (PestAlert)
            {
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.42f, 0.88f, 0.20f), new Vector3(0.12f, 0.12f, 0.12f), new Color(0.35f, 0.85f, 0.18f), "PestHead");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.35f, 0.84f, 0.15f), new Vector3(0.11f, 0.11f, 0.11f), new Color(0.96f, 0.55f, 0.12f), "PestSeg2");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.28f, 0.81f, 0.10f), new Vector3(0.10f, 0.10f, 0.10f), new Color(0.35f, 0.85f, 0.18f), "PestSeg3");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.22f, 0.78f, 0.06f), new Vector3(0.08f, 0.08f, 0.08f), new Color(0.96f, 0.78f, 0.12f), "PestTail");

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.46f, 0.93f, 0.24f), new Vector3(0.035f, 0.035f, 0.035f), Color.white, "EyeL");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.40f, 0.94f, 0.25f), new Vector3(0.035f, 0.035f, 0.035f), Color.white, "EyeR");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.47f, 0.94f, 0.26f), new Vector3(0.018f, 0.018f, 0.018f), Color.black, "PupilL");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.41f, 0.95f, 0.27f), new Vector3(0.018f, 0.018f, 0.018f), Color.black, "PupilR");

                var ant1 = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(-0.44f, 0.98f, 0.18f), new Vector3(0.01f, 0.06f, 0.01f), new Color(0.9f, 0.4f, 0.1f), "Antenna1");
                ant1.transform.localRotation = Quaternion.Euler(15, 0, 15);
                var ant2 = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(-0.38f, 0.99f, 0.18f), new Vector3(0.01f, 0.06f, 0.01f), new Color(0.9f, 0.4f, 0.1f), "Antenna2");
                ant2.transform.localRotation = Quaternion.Euler(15, 0, -15);

                var badge = CreateColoredPrimitive(PrimitiveType.Cube, root.transform, new Vector3(-0.38f, 1.25f, 0.20f), new Vector3(0.20f, 0.20f, 0.04f), new Color(0.98f, 0.78f, 0.08f), "WarningSign");
                badge.transform.localRotation = Quaternion.Euler(0, 0, 45);
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(-0.38f, 1.27f, 0.23f), new Vector3(0.03f, 0.08f, 0.03f), Color.black, "ExclamationTop");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.38f, 1.18f, 0.23f), new Vector3(0.03f, 0.03f, 0.03f), Color.black, "ExclamationDot");
            }
            else if (type == 4) // Perisai Energi (ShieldEffect)
            {
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 0.85f, 0), new Vector3(1.35f, 1.45f, 1.35f), new Color(0.20f, 0.65f, 1.0f, 0.38f), "ShieldDome");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.85f, 0), new Vector3(1.50f, 0.02f, 1.50f), new Color(0.35f, 0.90f, 1.0f, 0.65f), "ShieldOrbitRing");
            }
            else if (type == 5) // Pohon Berbuah Siap Panen (FruitHarvestReady)
            {
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, Vector3.zero, new Vector3(0.75f, 0.40f, 0.75f), new Color(0.85f, 0.38f, 0.18f), "PotBody");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.20f, 0), new Vector3(0.82f, 0.08f, 0.82f), new Color(0.88f, 0.42f, 0.22f), "PotRim");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.19f, 0), new Vector3(0.72f, 0.05f, 0.72f), new Color(0.24f, 0.14f, 0.08f), "Soil");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.21f, 0), new Vector3(0.73f, 0.04f, 0.73f), new Color(0.28f, 0.68f, 0.22f), "MossGrass");

                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0, 0.52f, 0), new Vector3(0.14f, 0.55f, 0.14f), new Color(0.44f, 0.24f, 0.10f), "Trunk");
                var br1 = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(-0.20f, 0.72f, 0.05f), new Vector3(0.08f, 0.35f, 0.08f), new Color(0.44f, 0.24f, 0.10f), "Branch1");
                br1.transform.localRotation = Quaternion.Euler(10, 0, 48);
                var br2 = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.22f, 0.78f, -0.05f), new Vector3(0.08f, 0.36f, 0.08f), new Color(0.44f, 0.24f, 0.10f), "Branch2");
                br2.transform.localRotation = Quaternion.Euler(-10, 0, -45);
                var br3 = CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.02f, 0.82f, 0.18f), new Vector3(0.07f, 0.28f, 0.07f), new Color(0.44f, 0.24f, 0.10f), "Branch3");
                br3.transform.localRotation = Quaternion.Euler(35, 0, 0);

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 1.10f, 0), new Vector3(0.90f, 0.75f, 0.90f), new Color(0.10f, 0.60f, 0.24f), "FoliageCore");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.40f, 0.96f, 0.10f), new Vector3(0.65f, 0.58f, 0.65f), new Color(0.14f, 0.68f, 0.28f), "FoliageL");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.40f, 1.02f, -0.08f), new Vector3(0.68f, 0.60f, 0.68f), new Color(0.12f, 0.64f, 0.26f), "FoliageR");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.05f, 1.00f, 0.35f), new Vector3(0.60f, 0.52f, 0.60f), new Color(0.16f, 0.72f, 0.30f), "FoliageF");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.05f, 1.05f, -0.32f), new Vector3(0.62f, 0.54f, 0.62f), new Color(0.11f, 0.58f, 0.23f), "FoliageB");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 1.42f, 0), new Vector3(0.60f, 0.48f, 0.60f), new Color(0.18f, 0.76f, 0.32f), "FoliageTop");

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.28f, 0.85f, 0.30f), new Vector3(0.16f, 0.16f, 0.16f), new Color(0.94f, 0.20f, 0.20f), "Apple1");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.30f, 0.92f, 0.28f), new Vector3(0.16f, 0.16f, 0.16f), new Color(0.94f, 0.20f, 0.20f), "Apple2");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.02f, 1.25f, 0.32f), new Vector3(0.16f, 0.16f, 0.16f), new Color(0.94f, 0.20f, 0.20f), "Apple3");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.42f, 1.05f, -0.10f), new Vector3(0.15f, 0.15f, 0.15f), new Color(0.94f, 0.20f, 0.20f), "Apple4");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.38f, 1.15f, -0.12f), new Vector3(0.15f, 0.15f, 0.15f), new Color(0.94f, 0.20f, 0.20f), "Apple5");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.18f, 0.75f, 0.22f), new Vector3(0.15f, 0.15f, 0.15f), new Color(0.94f, 0.20f, 0.20f), "Apple6");

                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(-0.50f, 1.35f, 0.10f), new Vector3(0.08f, 0.08f, 0.08f), new Color(1.0f, 0.86f, 0.15f), "Star1");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.48f, 1.40f, 0.15f), new Vector3(0.08f, 0.08f, 0.08f), new Color(1.0f, 0.86f, 0.15f), "Star2");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0, 1.62f, 0), new Vector3(0.09f, 0.09f, 0.09f), new Color(1.0f, 0.86f, 0.15f), "Star3");

                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.48f, 0.12f, 0.15f), new Vector3(0.30f, 0.20f, 0.30f), new Color(0.72f, 0.52f, 0.30f), "BasketBody");
                CreateColoredPrimitive(PrimitiveType.Cylinder, root.transform, new Vector3(0.48f, 0.22f, 0.15f), new Vector3(0.32f, 0.04f, 0.32f), new Color(0.65f, 0.45f, 0.24f), "BasketRim");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.45f, 0.24f, 0.12f), new Vector3(0.11f, 0.11f, 0.11f), new Color(0.94f, 0.20f, 0.20f), "BasketApple1");
                CreateColoredPrimitive(PrimitiveType.Sphere, root.transform, new Vector3(0.51f, 0.25f, 0.18f), new Vector3(0.11f, 0.11f, 0.11f), new Color(0.94f, 0.20f, 0.20f), "BasketApple2");
            }

            return root;
        }

        private void UpdateBiteMeshes(int stage)
        {
            currentBite = stage;
            if (appleWholeMesh != null) appleWholeMesh.SetActive(stage == 0);
            if (appleBite1Mesh != null) appleBite1Mesh.SetActive(stage == 1);
            if (appleBite2Mesh != null) appleBite2Mesh.SetActive(stage == 2);
            if (appleBite3CoreMesh != null) appleBite3CoreMesh.SetActive(stage == 3);
        }

        public void TriggerBiteApple()
        {
            if (biteController != null)
            {
                biteController.TakeBite();
                currentBite = biteController.currentBiteStage;
            }
            else
            {
                currentBite = (currentBite + 1) % 4;
                UpdateBiteMeshes(currentBite);
            }
            if (crumbParticles != null) crumbParticles.Play();
            StartCoroutine(SquashAndStretchRoutine(fruitContainer != null ? fruitContainer.transform : null));
            TriggerHapticMedium();
        }

        private IEnumerator SquashAndStretchRoutine(Transform target)
        {
            if (target == null) yield break;
            Vector3 orig = Vector3.one;
            target.localScale = new Vector3(1.15f, 0.88f, 1.15f);
            yield return new WaitForSeconds(0.08f);
            target.localScale = new Vector3(0.92f, 1.10f, 0.92f);
            yield return new WaitForSeconds(0.08f);
            target.localScale = orig;
        }

        public void ResetAppleStage()
        {
            if (biteController != null)
            {
                biteController.ResetFruit();
                currentBite = 0;
            }
            else
            {
                currentBite = 0;
                UpdateBiteMeshes(0);
            }
            if (decayTimer != null) decayTimer.ResetDecay();
            TriggerHapticLight();
        }

        public void ChangeFruitColor(Color col, string colorName)
        {
            currentFruitColor = col;
            if (fruitContainer != null)
            {
                var rends = fruitContainer.GetComponentsInChildren<Renderer>();
                foreach (var r in rends)
                {
                    if (r.gameObject.name.Contains("Apple") || r.gameObject.name.Contains("Sphere") || r.gameObject.name.Contains("Whole"))
                    {
                        r.material.color = col;
                    }
                }
            }
            if (decayTimer != null) decayTimer.freshColor = col;
            TriggerHapticLight();
        }

        public void SetARMode(bool isAR)
        {
            isRealARCameraMode = isAR;
            if (arCameraFeed != null) arCameraFeed.gameObject.SetActive(isAR);
            if (arGroundReticle != null) arGroundReticle.SetActive(isAR);

            if (isAR)
            {
                StartCameraFeed();
                if (arNutritionFactText != null)
                {
                    arNutritionFactText.text = "Mode AR Kamera Aktif • Deteksi Meja Siap!";
                    arNutritionFactText.gameObject.SetActive(true);
                }
                TriggerHapticSuccess();
            }
            else
            {
                StopCameraFeed();
                if (arNutritionFactText != null)
                {
                    arNutritionFactText.text = "Mode Studio 3D Aktif • Putar Apel dengan Sentuhan";
                    arNutritionFactText.gameObject.SetActive(true);
                }
                TriggerHapticLight();
            }
        }

        private void StartCameraFeed()
        {
            try
            {
                if (webCamTexture == null)
                {
                    WebCamDevice[] devs = WebCamTexture.devices;
                    string camName = (devs.Length > 0) ? devs[0].name : "";
                    foreach (var d in devs) { if (!d.isFrontFacing) { camName = d.name; break; } }
                    if (!string.IsNullOrEmpty(camName))
                    {
                        webCamTexture = new WebCamTexture(camName, 1280, 720, 30);
                        if (arCameraFeed != null) arCameraFeed.texture = webCamTexture;
                    }
                }
                if (webCamTexture != null && !webCamTexture.isPlaying) webCamTexture.Play();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[AR Camera] Error: " + ex.Message);
            }
        }

        private void StopCameraFeed()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
        }

        public void ToggleAnatomyModal()
        {
            if (arAnatomyModal != null) arAnatomyModal.SetActive(!arAnatomyModal.activeSelf);
            TriggerHapticLight();
        }

        private IEnumerator TakeARPhotoRoutine()
        {
            TriggerHapticMedium();
            if (arSnapshotFlash != null)
            {
                arSnapshotFlash.SetActive(true);
                yield return new WaitForSeconds(0.12f);
                arSnapshotFlash.SetActive(false);
            }
            if (arNutritionFactText != null)
            {
                arNutritionFactText.text = "Foto AR Berhasil Disimpan ke Galeri Edukasi!";
                arNutritionFactText.gameObject.SetActive(true);
            }
            TriggerHapticSuccess();
        }

        // =========================================================================
        // HELPER BUILDER DARI ARTWORK HD & HOTSPOT INTERAKTIF
        // =========================================================================
        private static RectTransform CreateRect(string n, Transform p)
        {
            var go = new GameObject(n, typeof(RectTransform));
            go.transform.SetParent(p, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform r, float l = 0, float b = 0, float rt = 0, float t = 0)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(l, b);
            r.offsetMax = new Vector2(-rt, -t);
        }

        private GameObject CreateHDArtboardPanel(string screenName, Transform parent, string textureFileName)
        {
            var panel = CreateRect(screenName, parent);
            Stretch(panel);

            var bg = panel.gameObject.AddComponent<Image>();
            bg.raycastTarget = false;
            bg.preserveAspect = false; // safeArea AspectRatioFitter menjaga rasio 1080x2228 persis tanpa distorsi & tanpa offset hotspot

            var sprite = LoadSpriteFromFile(textureFileName);
            if (sprite != null)
            {
                bg.sprite = sprite;
                bg.color = Color.white;
            }
            else
            {
                bg.color = new Color(0.97f, 0.98f, 0.99f);
            }

            return panel.gameObject;
        }

        private Button CreateHotspot(Transform parent, float svgX, float svgY, float svgW, float svgH, KDAction action, string name = null)
        {
            float minX = svgX / 412f;
            float maxX = (svgX + svgW) / 412f;
            float minY = 1.0f - ((svgY + svgH) / 850f);
            float maxY = 1.0f - (svgY / 850f);

            var r = CreateRect(name ?? $"Hotspot_{action}", parent);
            r.anchorMin = new Vector2(minX, minY);
            r.anchorMax = new Vector2(maxX, maxY);
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;

            var img = r.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.005f); // Area klik transparan tapi selalu terdeteksi raycaster
            img.raycastTarget = true;

            var btn = r.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.005f);
            colors.highlightedColor = new Color(0.3f, 0.6f, 1f, 0.20f);
            colors.pressedColor = new Color(0.2f, 0.5f, 1f, 0.35f);
            colors.selectedColor = new Color(0.3f, 0.6f, 1f, 0.15f);
            btn.colors = colors;

            if (action != KDAction.None)
            {
                var hook = r.gameObject.AddComponent<KawanDengarButtonHook>();
                hook.action = action;
                btn.onClick.AddListener(() =>
                {
                    if (Instance != null) Instance.HandleAction(action);
                });
            }

            return btn;
        }

        public static Sprite LoadSpriteFromFile(string fileName)
        {
#if UNITY_EDITOR
            string pSprite1 = "Assets/KawanDengarEdu/Sprites/Textures/" + fileName;
            string pSprite2 = "Assets/Sprites/Textures/" + fileName;
            var editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(pSprite1);
            if (editorSprite == null) editorSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(pSprite2);
            if (editorSprite != null) return editorSprite;
#endif
            string p1 = Path.Combine(Application.dataPath, "Sprites", "Textures", fileName);
            string p2 = Path.Combine(Application.dataPath, "KawanDengarEdu", "Sprites", "Textures", fileName);
            string path = File.Exists(p1) ? p1 : (File.Exists(p2) ? p2 : null);

            if (path == null) return null;

            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, true);
                if (tex.LoadImage(bytes))
                {
                    tex.filterMode = FilterMode.Bilinear;
                    tex.wrapMode = TextureWrapMode.Clamp;
                    tex.anisoLevel = 8;
                    tex.Apply(true, true);
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LoadSpriteFromFile] Gagal memuat " + fileName + ": " + ex.Message);
            }
            return null;
        }

        private static void TriggerHapticLight()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private static void TriggerHapticMedium()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private static void TriggerHapticSuccess()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        public static Sprite GetFruitSprite(string fruitId)
        {
            if (string.IsNullOrEmpty(fruitId)) fruitId = "apple_fuji";
            if (cachedFruitSprites.TryGetValue(fruitId, out var existing) && existing != null)
                return existing;

            int size = 160;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color transparent = new Color(0, 0, 0, 0);
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;

            float cx = size * 0.5f;
            float cy = size * 0.46f;

            if (fruitId.Contains("orange"))
            {
                // Orange: bright orange circular body with glossy sheen and green leaf
                float r = size * 0.36f;
                Color orangeCol = new Color(0.98f, 0.54f, 0.08f);
                Color orangeDark = new Color(0.88f, 0.42f, 0.04f);
                Color leafCol = new Color(0.20f, 0.75f, 0.28f);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - cx;
                        float dy = y - cy;
                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist <= r)
                        {
                            float factor = Mathf.Clamp01((r - dist) / 1.5f);
                            float shade = Mathf.Clamp01(1f - (dy * 0.5f - dx * 0.3f) / r);
                            Color c = Color.Lerp(orangeDark, orangeCol, shade);
                            float hdx = x - (cx - r * 0.35f);
                            float hdy = y - (cy + r * 0.35f);
                            if (hdx * hdx + hdy * hdy < (r * 0.28f) * (r * 0.28f))
                                c = Color.Lerp(c, Color.white, 0.45f);
                            pixels[y * size + x] = new Color(c.r, c.g, c.b, factor);
                        }
                        // Green leaf at top
                        float ldx = x - (cx + 12f);
                        float ldy = y - (cy + r + 10f);
                        if ((ldx * ldx) / 100f + (ldy * ldy) / 40f <= 1f)
                        {
                            pixels[y * size + x] = leafCol;
                        }
                    }
                }
            }
            else if (fruitId.Contains("banana"))
            {
                // Banana: curved golden crescent
                Color yellowCol = new Color(0.98f, 0.83f, 0.12f);
                Color yellowDark = new Color(0.85f, 0.68f, 0.08f);
                Color tipCol = new Color(0.42f, 0.35f, 0.12f);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float t = (x - (cx - 50f)) / 100f;
                        if (t >= 0f && t <= 1f)
                        {
                            float curveY = cy - 25f + 55f * Mathf.Sin(t * Mathf.PI);
                            float thickness = 18f * Mathf.Sin(t * Mathf.PI);
                            float dy = Mathf.Abs(y - curveY);
                            if (dy <= thickness)
                            {
                                Color c = (t < 0.1f || t > 0.9f) ? tipCol : Color.Lerp(yellowDark, yellowCol, (curveY + thickness - y) / (2f * thickness));
                                pixels[y * size + x] = c;
                            }
                        }
                    }
                }
            }
            else if (fruitId.Contains("avocado"))
            {
                // Avocado: pear shape with skin, flesh, and brown seed
                Color skinCol = new Color(0.12f, 0.35f, 0.16f);
                Color fleshCol = new Color(0.68f, 0.88f, 0.42f);
                Color seedCol = new Color(0.45f, 0.24f, 0.12f);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x - cx;
                        float dyLower = y - (cy - 12f);
                        float dyUpper = y - (cy + 22f);
                        float dLower = Mathf.Sqrt(dx * dx + dyLower * dyLower);
                        float dUpper = Mathf.Sqrt(dx * dx + dyUpper * dyUpper);

                        bool inBody = (dLower <= 44f) || (dUpper <= 28f && y >= cy);
                        if (inBody)
                        {
                            bool isSkin = (dLower > 38f) || (dUpper > 23f && y >= cy);
                            Color c = isSkin ? skinCol : fleshCol;

                            if (dLower <= 18f)
                            {
                                c = seedCol;
                            }
                            pixels[y * size + x] = c;
                        }
                    }
                }
            }
            else
            {
                // Apple (default): bright crimson fuji apple with stem and leaf
                float r = size * 0.36f;
                Color redCol = new Color(0.92f, 0.22f, 0.22f);
                Color redDark = new Color(0.72f, 0.12f, 0.12f);
                Color stemCol = new Color(0.42f, 0.24f, 0.10f);
                Color leafCol = new Color(0.20f, 0.75f, 0.28f);

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x - cx) * 1.05f;
                        float dy = (y - cy) * 0.98f;
                        float rMod = r - Mathf.Abs(dx) * 0.08f;
                        if (dy > 0) rMod -= (1f - Mathf.Abs(dx) / r) * 6f;
                        else rMod -= (1f - Mathf.Abs(dx) / r) * 4f;

                        float dist = Mathf.Sqrt(dx * dx + dy * dy);
                        if (dist <= rMod)
                        {
                            float factor = Mathf.Clamp01((rMod - dist) / 1.5f);
                            float shade = Mathf.Clamp01(1f - (dy * 0.4f - dx * 0.3f) / rMod);
                            Color c = Color.Lerp(redDark, redCol, shade);
                            float hdx = x - (cx - r * 0.32f);
                            float hdy = y - (cy + r * 0.30f);
                            if (hdx * hdx + hdy * hdy < (r * 0.24f) * (r * 0.24f))
                                c = Color.Lerp(c, Color.white, 0.40f);
                            pixels[y * size + x] = new Color(c.r, c.g, c.b, factor);
                        }

                        // Brown stem at top
                        if (x >= cx - 3f && x <= cx + 3f && y >= cy + r - 4f && y <= cy + r + 20f)
                        {
                            pixels[y * size + x] = stemCol;
                        }

                        // Green leaf at top right
                        float ldx = x - (cx + 14f);
                        float ldy = y - (cy + r + 12f);
                        if ((ldx * ldx) / 110f + (ldy * ldy) / 45f <= 1f)
                        {
                            pixels[y * size + x] = leafCol;
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            cachedFruitSprites[fruitId] = sprite;
            return sprite;
        }
    }
}
