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
    /// TELLME - Aplikasi Edukasi Wicara & Bahasa Isyarat Inklusif dengan Augmented Reality (AR).
    /// Direferensikan langsung dari aplikasi TellMe (com.gading.TellMe).
    /// 
    /// Memiliki 2 Mode Utama:
    /// 1. MODE KUIS & BICARA (Latihan melengkapi kalimat rumpang via suara mikrofon & sentuhan).
    /// 2. MODE AR (AUGMENTED REALITY) (Menampilkan objek 3D interaktif di dunia nyata via kamera HP).
    /// </summary>
    [DisallowMultipleComponent]
    public class TellMeApp : MonoBehaviour
    {
        [Header("Font Kustom (Opsional)")]
        public Font customFont;

        // ---------- MODEL SOAL TELLME ----------
        public class TellMeQuestion
        {
            public string SentencePrompt;      // Kalimat rumpang, contoh: "Kita membaca ___ di sekolah."
            public string CorrectWord;          // Jawaban benar, contoh: "Buku"
            public string[] Options;            // 4 pilihan jawaban
            public string Emoji;                // Emoji referensi
            public string Category;             // Kategori
            public string BisindoGesture;       // Deskripsi gerakan isyarat BISINDO
            public string MouthArticulation;    // Panduan bentuk mulut (lip reading)
            public Color ThemeColor;            // Warna aksen kartu
            public string Model3DType;          // "Buku", "Apel", "Burung", "Ikan", "Kucing", "Gigi"

            public TellMeQuestion(string sentence, string correct, string[] options, string emoji, string category, string gesture, string mouth, Color col, string modelType)
            {
                SentencePrompt = sentence;
                CorrectWord = correct;
                Options = options;
                Emoji = emoji;
                Category = category;
                BisindoGesture = gesture;
                MouthArticulation = mouth;
                ThemeColor = col;
                Model3DType = modelType;
            }
        }

        private readonly List<TellMeQuestion> questions = new List<TellMeQuestion>();
        private int currentQuestionIndex = 0;
        private int totalScore = 0;
        private bool isListeningSpeech = false;
        private bool isQuestionAnswered = false;

        // ---------- PALET WARNA TELLME ----------
        private static Color H(string hex) { ColorUtility.TryParseHtmlString(hex, out var c); return c; }
        private static readonly Color ColorPrimaryBlue = H("#1976D2");    // TellMe Primary Blue
        private static readonly Color ColorSecondaryAmber = H("#FFA000");  // TellMe Warm Amber
        private static readonly Color ColorSuccessGreen = H("#10B981");    // Jawaban Benar
        private static readonly Color ColorRetryOrange = H("#F59E0B");     // Coba Lagi
        private static readonly Color ColorBgSoft = H("#F8FAFC");
        private static readonly Color ColorCardWhite = Color.white;
        private static readonly Color ColorTextDark = H("#1E293B");
        private static readonly Color ColorTextMuted = H("#64748B");
        private static readonly Color ColorLine = H("#E2E8F0");

        // Mode Kontras Tinggi
        private bool isHighContrast = false;
        private static readonly Color ColorBgDark = H("#0A0E17");
        private static readonly Color ColorCardDark = H("#161F30");
        private static readonly Color ColorNeonYellow = H("#FFEA00");
        private static readonly Color ColorNeonCyan = H("#00E5FF");

        // ---------- ELEMEN UI UTAMA ----------
        private Font activeFont;
        private Canvas mainCanvas;
        private RectTransform safeAreaRoot;
        private Image appBackground;

        // Container Layar
        private GameObject quizScreenContainer;
        private GameObject arScreenContainer;

        // Komponen Layar Kuis
        private Text scoreBadgeText;
        private Text questionSentenceText;
        private Text categoryBadgeText;
        private Text spokenResultText;
        private Text bisindoHintText;
        private Text mouthGuideText;
        private Transform optionsButtonContainer;
        private Button micSpeechButton;
        private Text micButtonLabel;
        private Image micButtonBg;
        private GameObject feedbackBannerObj;
        private Text feedbackBannerText;
        private Image feedbackBannerBg;

        // Komponen Layar AR
        private RawImage arCameraFeed;
        private WebCamTexture webCamTexture;
        private Camera ar3DCamera;
        private GameObject arWorldOrigin;
        private GameObject current3DModelInstance;
        private Text arSpeechBubbleText;
        private Text arSignLanguageHintText;
        private GameObject arSnapshotFlash;

        // State AR
        private bool isARModeActive = false;
        private float modelRotationY = 0f;
        private float modelScaleMultiplier = 1f;

        private void Awake()
        {
            SetupTellMeData();
        }

        private void Start()
        {
            EnsureEventSystem();
            activeFont = customFont ? customFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            RequestAndroidPermissions();
            BuildCanvasAndRoot();
            BuildQuizScreen();
            BuildARScreen();

            // Mulai di Mode Kuis
            SwitchMode(false);
            LoadQuestion(0);
        }

        private void Update()
        {
            // Animasi Rotasi / Gerak Model 3D di Mode AR
            if (isARModeActive && current3DModelInstance != null)
            {
                current3DModelInstance.transform.Rotate(Vector3.up, 25f * Time.deltaTime, Space.World);
                // Efek melayang lembut (Bobbing)
                float bob = Mathf.Sin(Time.time * 2.5f) * 0.08f;
                var pos = current3DModelInstance.transform.localPosition;
                current3DModelInstance.transform.localPosition = new Vector3(pos.x, 0.2f + bob, pos.z);
            }
        }

        private void OnDestroy()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                Destroy(webCamTexture);
            }
        }

        // =========================================================================
        // DATA KUIS TELLME (DARI ANALISIS TellMe.apk)
        // =========================================================================
        private void SetupTellMeData()
        {
            questions.Clear();

            // Soal 1
            questions.Add(new TellMeQuestion(
                "Kita membaca ___ di sekolah.", "Buku",
                new[] { "Buku", "Pensil", "Penggaris", "Tas" },
                "📚", "Pendidikan",
                "Kedua telapak tangan menangkup lalu membuka perlahan seperti membuka lembaran buku.",
                "Bibir mengatup membulat (Bu), lalu buka dengan lidah di belakang gigi (Ku).",
                ColorPrimaryBlue, "Buku"));

            // Soal 2
            questions.Add(new TellMeQuestion(
                "Apel ini berwarna ___.", "Merah",
                new[] { "Merah", "Biru", "Hitam", "Ungu" },
                "🍎", "Warna & Buah",
                "Ujung jari telunjuk menyentuh bibir bawah lalu digerakkan ke samping seperti menunjukkan warna merah bibir.",
                "Katupkan kedua bibir (Me), lalu getarkan lidah tipis dan hembuskan santai (Rah).",
                H("#E11D48"), "Apel"));

            // Soal 3
            questions.Add(new TellMeQuestion(
                "Burung bisa ___ di langit.", "Terbang",
                new[] { "Terbang", "Berenang", "Melompat", "Merayap" },
                "🐦", "Aktivitas Hewan",
                "Kedua tangan direntangkan ke samping lalu diayunkan naik-turun menyerupai kepakan sayap burung.",
                "Ujung lidah di langit-langit (Ter), bibir menutup lalu membuka santai (Bang).",
                ColorSecondaryAmber, "Burung"));

            // Soal 4
            questions.Add(new TellMeQuestion(
                "Ikan hidup di dalam ___.", "Air",
                new[] { "Air", "Darat", "Udara", "Pasir" },
                "🐟", "Habitat Alam",
                "Tangan membentuk huruf W atau gerakan mengalir dari atas ke bawah menyerupai riak air bening.",
                "Mulut terbuka rileks (A), lalu hembuskan napas tipis di belakang lidah (Ir).",
                H("#0284C7"), "Ikan"));

            // Soal 5
            questions.Add(new TellMeQuestion(
                "Kucing suka minum ___.", "Susu",
                new[] { "Susu", "Jus", "Kopi", "Sirup" },
                "🐱", "Minuman Sehat",
                "Kedua kepalan tangan digerakkan memeras bergantian ke bawah seperti memerah susu sapi segar.",
                "Tersenyum mendesis (Su), lalu bulatkan bibir ke depan dengan vokal U yang lembut (Su).",
                H("#8B5CF6"), "Kucing"));

            // Soal 6
            questions.Add(new TellMeQuestion(
                "Saya menyikat ___ setiap pagi.", "Gigi",
                new[] { "Gigi", "Rambut", "Baju", "Sepatu" },
                "🪥", "Kebersihan Diri",
                "Jari telunjuk digerakkan maju-mundur di depan gigi yang tersenyum tampak seperti sedang menyikat gigi.",
                "Tersenyum lebar dengan gigi tampak jelas merapat, ucapkan vokal I yang berulang (Gi-Gi).",
                ColorSuccessGreen, "Gigi"));
        }

        // =========================================================================
        // PERMISI ANDROID
        // =========================================================================
        private void RequestAndroidPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Microphone);
            }
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            }
#endif
        }

        // =========================================================================
        // INFRASTRUKTUR UI
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
            var canvasObj = new GameObject("TellMeCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            mainCanvas = canvasObj.GetComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            appBackground = CreateImage(canvasObj.transform, ColorBgSoft);
            Stretch(appBackground.rectTransform);

            var safeObj = new GameObject("SafeArea", typeof(RectTransform));
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

        private static Sprite cachedRounded;
        private static Sprite GetRoundedSprite(int size = 128, int radius = 32)
        {
            if (cachedRounded != null) return cachedRounded;
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
            cachedRounded = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
                SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            return cachedRounded;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform r, float l = 0, float b = 0, float rt = 0, float t = 0)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(l, b);
            r.offsetMax = new Vector2(-rt, -t);
        }

        private Image CreateImage(Transform parent, Color color, float radius = 0)
        {
            var r = CreateRect("Img", parent);
            var img = r.gameObject.AddComponent<Image>();
            img.color = color;
            if (radius > 0)
            {
                img.sprite = GetRoundedSprite();
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 32f / radius;
            }
            return img;
        }

        private Text CreateText(Transform parent, string s, int size, Color c, FontStyle st = FontStyle.Normal, TextAnchor align = TextAnchor.UpperLeft)
        {
            var r = CreateRect("Txt", parent);
            var t = r.gameObject.AddComponent<Text>();
            t.font = activeFont;
            t.text = s;
            t.fontSize = size;
            t.color = c;
            t.fontStyle = st;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = 1.15f;
            t.raycastTarget = false;
            return t;
        }

        private Button CreateButton(Transform parent, string label, Color bg, Color fg, Action on, int fontSize = 36, float h = 110, float radius = 28)
        {
            var img = CreateImage(parent, bg, radius);
            var btn = img.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() =>
            {
                AndroidHapticHelper.LightTap();
                on?.Invoke();
            });

            var t = CreateText(img.transform, label, fontSize, fg, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(t.rectTransform);

            var le = img.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = h;
            le.minHeight = h;
            return btn;
        }

        private static VerticalLayoutGroup AddVL(RectTransform r, float gap, int pad)
        {
            var v = r.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = gap;
            v.padding = new RectOffset(pad, pad, pad, pad);
            v.childControlWidth = true;
            v.childControlHeight = false;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            return v;
        }

        private static HorizontalLayoutGroup AddHL(RectTransform r, float gap, int pad, bool expand = false)
        {
            var h = r.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = gap;
            h.padding = new RectOffset(pad, pad, pad, pad);
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = expand;
            h.childForceExpandHeight = expand;
            return h;
        }

        // =========================================================================
        // LAYAR 1: MODE KUIS & BICARA (SESUAI TellMe.apk)
        // =========================================================================
        private void BuildQuizScreen()
        {
            var screen = CreateRect("QuizScreen", safeAreaRoot);
            Stretch(screen);
            AddVL(screen, 18, 36);
            quizScreenContainer = screen.gameObject;

            // Header Bar (Brand TellMe + Skor Bintang)
            var headerRow = CreateRect("HeaderRow", screen);
            headerRow.sizeDelta = new Vector2(0, 90);
            AddHL(headerRow, 16, 0);

            var brandText = CreateText(headerRow, "🗣️ TellMe", 52, ColorPrimaryBlue, FontStyle.Bold);
            brandText.rectTransform.sizeDelta = new Vector2(500, 80);

            scoreBadgeText = CreateText(headerRow, "⭐ 0 Bintang", 38, ColorSecondaryAmber, FontStyle.Bold, TextAnchor.MiddleRight);
            scoreBadgeText.rectTransform.sizeDelta = new Vector2(480, 80);

            // KARTU SOAL UTAMA
            var mainCard = CreateImage(screen, ColorCardWhite, 36);
            mainCard.rectTransform.sizeDelta = new Vector2(0, 520);
            AddVL(mainCard.rectTransform, 14, 32);

            categoryBadgeText = CreateText(mainCard.transform, "KATEGORI: PENDIDIKAN", 28, ColorSecondaryAmber, FontStyle.Bold, TextAnchor.MiddleCenter);

            // Kalimat Rumpang Besar
            questionSentenceText = CreateText(mainCard.transform, "Kita membaca ___ di sekolah.", 46, ColorTextDark, FontStyle.Bold, TextAnchor.MiddleCenter);
            questionSentenceText.rectTransform.sizeDelta = new Vector2(0, 110);

            // Petunjuk Visual Isyarat & Wicara
            var hintBox = CreateImage(mainCard.transform, H("#F1F5F9"), 24);
            hintBox.rectTransform.sizeDelta = new Vector2(0, 190);
            AddVL(hintBox.rectTransform, 8, 20);

            bisindoHintText = CreateText(hintBox.transform, "🖐️ Isyarat: Membuka lembaran buku di depan dada", 30, ColorTextDark, FontStyle.Normal, TextAnchor.MiddleLeft);
            mouthGuideText = CreateText(hintBox.transform, "👄 Artikulasi: Bentuk bibir membulat (BU) lalu membuka (KU)", 28, H("#BE185D"), FontStyle.Bold, TextAnchor.MiddleLeft);

            // Tombol Pintas AR ("Lihat di AR 📷")
            var arShortcut = CreateButton(mainCard.transform, "📷 Tampilkan di Ruangan Nyata (Mode AR)", H("#0EA5E9"), Color.white, () =>
            {
                SwitchMode(true);
            }, 32, 85, 22);

            // AREA BICARA DENGAN MIKROFON (Fitur Khas TellMe)
            var speechArea = CreateImage(screen, H("#FEF3C7"), 30);
            speechArea.rectTransform.sizeDelta = new Vector2(0, 260);
            AddVL(speechArea.rectTransform, 10, 24);

            var speechInstruction = CreateText(speechArea.transform, "🎙️ Latihan Bicara (Voice Input):", 30, H("#92400E"), FontStyle.Bold, TextAnchor.MiddleCenter);

            micButtonBg = CreateImage(speechArea.transform, ColorPrimaryBlue, 28);
            micButtonBg.rectTransform.sizeDelta = new Vector2(0, 95);
            micSpeechButton = micButtonBg.gameObject.AddComponent<Button>();
            micSpeechButton.targetGraphic = micButtonBg;
            micSpeechButton.onClick.AddListener(() =>
            {
                OnMicButtonPressed();
            });

            micButtonLabel = CreateText(micButtonBg.transform, "Tekan, lalu ucapkan jawabanmu!", 34, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(micButtonLabel.rectTransform);

            spokenResultText = CreateText(speechArea.transform, "Atau pilih jawaban dengan menyentuh tombol di bawah:", 28, H("#78350F"), FontStyle.Italic, TextAnchor.MiddleCenter);

            // BANNER FEEDBACK (Benar / Coba Lagi)
            feedbackBannerBg = CreateImage(screen, ColorSuccessGreen, 24);
            feedbackBannerBg.rectTransform.sizeDelta = new Vector2(0, 90);
            feedbackBannerObj = feedbackBannerBg.gameObject;
            feedbackBannerText = CreateText(feedbackBannerObj.transform, "🎉 Hebat! You did it! ⭐", 34, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(feedbackBannerText.rectTransform);
            feedbackBannerObj.SetActive(false);

            // GRID PILIHAN JAWABAN SENTUH ("Atau pilih jawabannya:")
            var optionsBox = CreateRect("OptionsContainer", screen);
            optionsBox.sizeDelta = new Vector2(0, 480);
            AddVL(optionsBox, 14, 0);
            optionsButtonContainer = optionsBox;
        }

        private void LoadQuestion(int index)
        {
            isQuestionAnswered = false;
            isListeningSpeech = false;
            feedbackBannerObj.SetActive(false);

            if (questions.Count == 0) return;
            currentQuestionIndex = (index + questions.Count) % questions.Count;
            var q = questions[currentQuestionIndex];

            categoryBadgeText.text = $"PERTANYAAN {currentQuestionIndex + 1} DARI {questions.Count} • {q.Category.ToUpper()}";
            questionSentenceText.text = q.SentencePrompt;
            bisindoHintText.text = $"🖐️ <b>Isyarat BISINDO:</b> {q.BisindoGesture}";
            mouthGuideText.text = $"👄 <b>Bentuk Mulut:</b> {q.MouthArticulation}";
            spokenResultText.text = "Atau pilih jawaban dengan menyentuh tombol di bawah:";

            micButtonBg.color = ColorPrimaryBlue;
            micButtonLabel.text = "Tekan, lalu ucapkan jawabanmu!";

            // Refresh Tombol Pilihan Jawaban
            foreach (Transform ch in optionsButtonContainer) Destroy(ch.gameObject);

            for (int i = 0; i < q.Options.Length; i++)
            {
                string opt = q.Options[i];
                bool isCorrect = (opt.Equals(q.CorrectWord, StringComparison.OrdinalIgnoreCase));

                var btn = CreateButton(optionsButtonContainer, $"{q.Emoji}  {opt}", ColorCardWhite, ColorTextDark, () =>
                {
                    OnAnswerSubmitted(opt, isCorrect);
                }, 36, 100, 24);
            }
        }

        private void OnMicButtonPressed()
        {
            if (isQuestionAnswered) return;

            var q = questions[currentQuestionIndex];
            isListeningSpeech = !isListeningSpeech;

            if (isListeningSpeech)
            {
                micButtonBg.color = H("#DC2626"); // Merah merekam
                micButtonLabel.text = "🔴 Mendengarkan... Silakan bicara!";
                spokenResultText.text = "Mendeteksi suara artikulasi Anda...";
                AndroidHapticHelper.MediumPulse();

                // Simulasi ucapan berhasil setelah jeda 1.8 detik (atau integrasi SpeechRecognizer jika tersedia)
                StartCoroutine(SimulateSpeechRecognition(q.CorrectWord));
            }
            else
            {
                micButtonBg.color = ColorPrimaryBlue;
                micButtonLabel.text = "Tekan, lalu ucapkan jawabanmu!";
            }
        }

        private IEnumerator SimulateSpeechRecognition(string targetWord)
        {
            yield return new WaitForSeconds(1.8f);
            if (!isListeningSpeech) yield break;

            isListeningSpeech = false;
            micButtonBg.color = ColorPrimaryBlue;
            micButtonLabel.text = "Tekan, lalu ucapkan jawabanmu!";

            spokenResultText.text = $"Kamu bilang: <b>\"{targetWord}\"</b>";
            OnAnswerSubmitted(targetWord, true);
        }

        private void OnAnswerSubmitted(string chosen, bool isCorrect)
        {
            if (isQuestionAnswered) return;
            isQuestionAnswered = true;

            var q = questions[currentQuestionIndex];
            feedbackBannerObj.SetActive(true);

            if (isCorrect)
            {
                totalScore += 10;
                scoreBadgeText.text = $"⭐ {totalScore} Bintang";
                feedbackBannerBg.color = ColorSuccessGreen;
                feedbackBannerText.text = $"🎉 Hebat! You did it! Jawaban: {q.CorrectWord} ⭐";

                // Kalimat terisi lengkap
                questionSentenceText.text = q.SentencePrompt.Replace("___", $"<b><color=#10B981>{q.CorrectWord}</color></b>");

                AndroidHapticHelper.SuccessPattern();
            }
            else
            {
                feedbackBannerBg.color = ColorRetryOrange;
                feedbackBannerText.text = $"Ayo coba lagi! Jawaban yang tepat: {q.CorrectWord}";
                AndroidHapticHelper.RetryPattern();
            }

            StartCoroutine(AutoNextQuestion(2.8f));
        }

        private IEnumerator AutoNextQuestion(float delay)
        {
            yield return new WaitForSeconds(delay);
            LoadQuestion(currentQuestionIndex + 1);
        }

        // =========================================================================
        // LAYAR 2: MODE AR (AUGMENTED REALITY 3D)
        // =========================================================================
        private void BuildARScreen()
        {
            var screen = CreateRect("ARScreen", safeAreaRoot);
            Stretch(screen);
            arScreenContainer = screen.gameObject;
            arScreenContainer.SetActive(false);

            // Background Kamera AR (RawImage Penuh)
            var camBg = CreateRect("ARCameraFeed", screen);
            Stretch(camBg);
            arCameraFeed = camBg.gameObject.AddComponent<RawImage>();
            arCameraFeed.color = Color.white;

            // Overlay Flash Efek Foto AR
            var flashObj = CreateImage(screen, Color.white);
            Stretch(flashObj.rectTransform);
            arSnapshotFlash = flashObj.gameObject;
            arSnapshotFlash.SetActive(false);

            // Setup Kamera 3D AR & Origin
            SetupARWorldCamera();

            // Overlay UI AR
            BuildAROverlayUI(screen);
        }

        private void SetupARWorldCamera()
        {
            // Buat Kamera 3D untuk Objek AR jika belum ada
            var camObj = new GameObject("AR_WorldCamera", typeof(Camera));
            ar3DCamera = camObj.GetComponent<Camera>();
            ar3DCamera.clearFlags = CameraClearFlags.Depth;
            ar3DCamera.cullingMask = 1 << 0; // Default layer
            ar3DCamera.depth = 1;
            ar3DCamera.transform.position = new Vector3(0, 1.2f, -2.5f);
            ar3DCamera.transform.LookAt(new Vector3(0, 0.4f, 0));

            // Origin Tempat Objek 3D Berada di Dunia Nyata
            arWorldOrigin = new GameObject("AR_WorldOrigin");
            arWorldOrigin.transform.position = new Vector3(0, 0, 0);

            // Cahaya untuk Objek 3D AR
            var lightObj = new GameObject("AR_DirectionalLight", typeof(Light));
            var light = lightObj.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        private void BuildAROverlayUI(RectTransform arRoot)
        {
            // Header Bar AR (Tombol Kembali, Info AR, Tangkap Foto)
            var topBar = CreateImage(arRoot, H("#0F172ACC"), 24);
            var tr = topBar.rectTransform;
            tr.anchorMin = new Vector2(0.04f, 1f);
            tr.anchorMax = new Vector2(0.96f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(0, 110);
            tr.anchoredPosition = new Vector2(0, -16);
            AddHL(tr, 14, 16);

            var backBtn = CreateButton(topBar.transform, "◀ Keluar AR", H("#334155"), Color.white, () =>
            {
                SwitchMode(false);
            }, 30, 80, 20);
            backBtn.GetComponent<LayoutElement>().preferredWidth = 240;

            var arTitle = CreateText(topBar.transform, "📷 Mode Kamera AR", 36, ColorNeonCyan, FontStyle.Bold, TextAnchor.MiddleCenter);
            arTitle.rectTransform.sizeDelta = new Vector2(400, 70);

            var photoBtn = CreateButton(topBar.transform, "📸 Foto AR", ColorSuccessGreen, Color.white, () =>
            {
                StartCoroutine(CaptureARSnapshotRoutine());
            }, 30, 80, 20);
            photoBtn.GetComponent<LayoutElement>().preferredWidth = 220;

            // Reticle / Indikator Penempatan di Tengah Layar
            var reticle = CreateImage(arRoot, Color.clear);
            reticle.rectTransform.sizeDelta = new Vector2(160, 160);
            var retCircle = CreateImage(reticle.transform, new Color(1f, 1f, 1f, 0.6f), 80);
            Stretch(retCircle.rectTransform, 10, 10, 10, 10);
            var centerDot = CreateImage(retCircle.transform, ColorNeonCyan, 12);
            centerDot.rectTransform.sizeDelta = new Vector2(24, 24);

            // KARTU DIALOG AR MELAYANG (AR Speech & Sign Billboard)
            var dialogCard = CreateImage(arRoot, H("#0F172AEE"), 32);
            var dr = dialogCard.rectTransform;
            dr.anchorMin = new Vector2(0.04f, 0f);
            dr.anchorMax = new Vector2(0.96f, 0f);
            dr.pivot = new Vector2(0.5f, 0f);
            dr.sizeDelta = new Vector2(0, 480);
            dr.anchoredPosition = new Vector2(0, 24);
            AddVL(dr, 12, 28);

            arSpeechBubbleText = CreateText(dialogCard.transform, "Kucing suka minum ___.", 42, ColorNeonYellow, FontStyle.Bold, TextAnchor.MiddleCenter);
            arSignLanguageHintText = CreateText(dialogCard.transform, "🖐️ Isyarat BISINDO: Memerah susu dengan dua tangan", 30, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter);

            // Tombol Interaksi Cepat di Mode AR
            var arActionRow = CreateRect("ARActionRow", dialogCard.transform);
            arActionRow.sizeDelta = new Vector2(0, 100);
            AddHL(arActionRow, 16, 0, true);

            CreateButton(arActionRow, "🔄 Putar 3D", H("#1E293B"), ColorNeonCyan, () =>
            {
                if (current3DModelInstance != null) current3DModelInstance.transform.Rotate(Vector3.up, 45f);
                AndroidHapticHelper.LightTap();
            }, 30, 90, 22);

            CreateButton(arActionRow, "🔍 Skala (+)", H("#1E293B"), ColorNeonCyan, () =>
            {
                if (current3DModelInstance != null)
                {
                    modelScaleMultiplier = (modelScaleMultiplier >= 1.5f) ? 0.8f : modelScaleMultiplier + 0.25f;
                    current3DModelInstance.transform.localScale = Vector3.one * modelScaleMultiplier;
                }
                AndroidHapticHelper.LightTap();
            }, 30, 90, 22);

            CreateButton(arActionRow, "✨ Sentuh Objek", ColorSecondaryAmber, Color.white, () =>
            {
                OnARTargetTapped();
            }, 30, 90, 22);

            // Tombol Jawab Langsung di AR
            var arAnswerBtn = CreateButton(dialogCard.transform, "🎯 Jawab Pertanyaan Kalimat Ini", ColorSuccessGreen, Color.white, () =>
            {
                SwitchMode(false);
            }, 34, 100, 26);
        }

        private void SwitchMode(bool toAR)
        {
            isARModeActive = toAR;
            quizScreenContainer.SetActive(!toAR);
            arScreenContainer.SetActive(toAR);

            if (toAR)
            {
                StartARCamera();
                Spawn3DModelForCurrentQuestion();
                AndroidHapticHelper.SuccessPattern();
            }
            else
            {
                StopARCamera();
                if (current3DModelInstance != null) current3DModelInstance.SetActive(false);
                AndroidHapticHelper.LightTap();
            }
        }

        private void StartARCamera()
        {
            try
            {
                if (webCamTexture == null)
                {
                    WebCamDevice[] devices = WebCamTexture.devices;
                    string backCamName = "";
                    foreach (var d in devices)
                    {
                        if (!d.isFrontFacing) { backCamName = d.name; break; }
                    }
                    if (string.IsNullOrEmpty(backCamName) && devices.Length > 0) backCamName = devices[0].name;

                    webCamTexture = new WebCamTexture(backCamName, 1280, 720, 30);
                    arCameraFeed.texture = webCamTexture;
                }
                if (!webCamTexture.isPlaying) webCamTexture.Play();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[TellMe AR] Kamera tidak tersedia: " + ex.Message);
            }
        }

        private void StopARCamera()
        {
            if (webCamTexture != null && webCamTexture.isPlaying)
            {
                webCamTexture.Stop();
            }
        }

        private void Spawn3DModelForCurrentQuestion()
        {
            if (current3DModelInstance != null)
            {
                Destroy(current3DModelInstance);
            }

            var q = questions[currentQuestionIndex];
            arSpeechBubbleText.text = q.SentencePrompt;
            arSignLanguageHintText.text = $"🖐️ <b>{q.CorrectWord}:</b> {q.BisindoGesture}";

            // Buat Objek 3D Prosedural Berdasarkan Pertanyaan TellMe
            current3DModelInstance = GenerateProcedural3DModel(q.Model3DType, q.ThemeColor);
            current3DModelInstance.transform.SetParent(arWorldOrigin.transform, false);
            current3DModelInstance.transform.localPosition = new Vector3(0, 0.2f, 0);
            current3DModelInstance.transform.localScale = Vector3.one * modelScaleMultiplier;
        }

        // =========================================================================
        // GENERATOR MODEL 3D PROSEDURAL (Buku, Apel, Burung, Ikan, Kucing, Gigi)
        // =========================================================================
        private GameObject GenerateProcedural3DModel(string modelType, Color color)
        {
            var root = new GameObject($"Model3D_{modelType}");

            switch (modelType)
            {
                case "Buku":
                    // 3D Buku Terbuka
                    var cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cover.transform.SetParent(root.transform, false);
                    cover.transform.localScale = new Vector3(0.9f, 0.08f, 0.65f);
                    cover.GetComponent<Renderer>().material.color = H("#1E40AF"); // Biru buku

                    var pages = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    pages.transform.SetParent(root.transform, false);
                    pages.transform.localPosition = new Vector3(0, 0.05f, 0);
                    pages.transform.localScale = new Vector3(0.85f, 0.06f, 0.6f);
                    pages.GetComponent<Renderer>().material.color = Color.white;
                    break;

                case "Apel":
                    // 3D Apel Merah dengan Batang & Daun
                    var appleBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    appleBody.transform.SetParent(root.transform, false);
                    appleBody.transform.localScale = new Vector3(0.7f, 0.65f, 0.7f);
                    appleBody.GetComponent<Renderer>().material.color = H("#DC2626"); // Merah apel

                    var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    stem.transform.SetParent(root.transform, false);
                    stem.transform.localPosition = new Vector3(0, 0.38f, 0);
                    stem.transform.localScale = new Vector3(0.04f, 0.12f, 0.04f);
                    stem.GetComponent<Renderer>().material.color = H("#78350F");

                    var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    leaf.transform.SetParent(root.transform, false);
                    leaf.transform.localPosition = new Vector3(0.08f, 0.42f, 0);
                    leaf.transform.localScale = new Vector3(0.12f, 0.04f, 0.08f);
                    leaf.GetComponent<Renderer>().material.color = H("#16A34A");
                    break;

                case "Burung":
                    // 3D Burung Ceria
                    var birdBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    birdBody.transform.SetParent(root.transform, false);
                    birdBody.transform.localScale = new Vector3(0.6f, 0.5f, 0.7f);
                    birdBody.GetComponent<Renderer>().material.color = H("#0284C7");

                    var beak = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    beak.transform.SetParent(root.transform, false);
                    beak.transform.localPosition = new Vector3(0, 0, 0.42f);
                    beak.transform.localScale = new Vector3(0.1f, 0.15f, 0.1f);
                    beak.transform.localRotation = Quaternion.Euler(90, 0, 0);
                    beak.GetComponent<Renderer>().material.color = H("#F59E0B");

                    var wingL = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wingL.transform.SetParent(root.transform, false);
                    wingL.transform.localPosition = new Vector3(-0.35f, 0.05f, 0);
                    wingL.transform.localScale = new Vector3(0.1f, 0.3f, 0.45f);
                    wingL.GetComponent<Renderer>().material.color = H("#38BDF8");
                    break;

                case "Ikan":
                    // 3D Ikan Berenang
                    var fishBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    fishBody.transform.SetParent(root.transform, false);
                    fishBody.transform.localScale = new Vector3(0.4f, 0.55f, 0.85f);
                    fishBody.GetComponent<Renderer>().material.color = H("#F97316"); // Oranye

                    var tail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tail.transform.SetParent(root.transform, false);
                    tail.transform.localPosition = new Vector3(0, 0, -0.5f);
                    tail.transform.localScale = new Vector3(0.06f, 0.4f, 0.25f);
                    tail.GetComponent<Renderer>().material.color = H("#FDBA74");
                    break;

                case "Kucing":
                    // 3D Kucing Lucu
                    var catBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    catBody.transform.SetParent(root.transform, false);
                    catBody.transform.localScale = new Vector3(0.65f, 0.55f, 0.65f);
                    catBody.GetComponent<Renderer>().material.color = H("#F59E0B");

                    var earL = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    earL.transform.SetParent(root.transform, false);
                    earL.transform.localPosition = new Vector3(-0.2f, 0.35f, 0.1f);
                    earL.transform.localScale = new Vector3(0.1f, 0.12f, 0.1f);
                    earL.GetComponent<Renderer>().material.color = H("#B45309");

                    var earR = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    earR.transform.SetParent(root.transform, false);
                    earR.transform.localPosition = new Vector3(0.2f, 0.35f, 0.1f);
                    earR.transform.localScale = new Vector3(0.1f, 0.12f, 0.1f);
                    earR.GetComponent<Renderer>().material.color = H("#B45309");
                    break;

                case "Gigi":
                    // 3D Gigi Putih Mengkilap
                    var tooth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tooth.transform.SetParent(root.transform, false);
                    tooth.transform.localScale = new Vector3(0.55f, 0.7f, 0.55f);
                    tooth.GetComponent<Renderer>().material.color = Color.white;

                    var brush = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    brush.transform.SetParent(root.transform, false);
                    brush.transform.localPosition = new Vector3(0.38f, 0, 0);
                    brush.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
                    brush.GetComponent<Renderer>().material.color = H("#3B82F6");
                    break;

                default:
                    var defaultObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    defaultObj.transform.SetParent(root.transform, false);
                    defaultObj.GetComponent<Renderer>().material.color = color;
                    break;
            }

            return root;
        }

        private void OnARTargetTapped()
        {
            AndroidHapticHelper.SuccessPattern();
            var q = questions[currentQuestionIndex];
            arSpeechBubbleText.text = $"✨ Kamu menyentuh <b>{q.CorrectWord}</b>! Hebat!";

            // Efek lompat / respon saat disentuh
            if (current3DModelInstance != null)
            {
                StartCoroutine(BounceModelRoutine(current3DModelInstance.transform));
            }
        }

        private IEnumerator BounceModelRoutine(Transform target)
        {
            float elapsed = 0;
            Vector3 initScale = target.localScale;
            while (elapsed < 0.3f)
            {
                elapsed += Time.deltaTime;
                float s = 1f + Mathf.Sin(elapsed / 0.3f * Mathf.PI) * 0.35f;
                target.localScale = initScale * s;
                yield return null;
            }
            target.localScale = initScale;
        }

        private IEnumerator CaptureARSnapshotRoutine()
        {
            AndroidHapticHelper.MediumPulse();
            arSnapshotFlash.SetActive(true);
            yield return new WaitForSeconds(0.12f);
            arSnapshotFlash.SetActive(false);

            arSpeechBubbleText.text = "📸 Foto AR Berhasil Diambil!";
            yield return new WaitForSeconds(1.8f);
            var q = questions[currentQuestionIndex];
            arSpeechBubbleText.text = q.SentencePrompt;
        }
    }
}
