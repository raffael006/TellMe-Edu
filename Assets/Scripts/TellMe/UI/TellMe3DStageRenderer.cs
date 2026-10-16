using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TellMe.UI
{
    /// <summary>
    /// Renderer 3D Prosedural Real-Time untuk:
    /// 1) AR Buah 3D Interaktif dengan 5 Fase Pertumbuhan & Anatomi saat diklik.
    /// 2) Kebun 3D (3D Garden Diorama) yang bertumbuh secara fisik seiring naiknya Level (Level 1 s/d Level 7).
    /// Menggunakan kamera 3D terpisah dan RenderTexture agar tampil tajam di atas kamera AR maupun di kartu UI.
    /// </summary>
    public class TellMe3DStageRenderer : MonoBehaviour
    {
        public RenderTexture OutputTexture { get; private set; }

        private Camera stageCamera;
        private Light mainLight;
        private Light fillLight;
        private Transform rootAnchor;
        private Transform turntableAnchor;
        private Transform waterDropsAnchor;

        private bool autoRotate = true;
        private float rotationSpeed = 28f;
        private float currentScale = 1f;
        private Vector3 stageOffset;
        private Coroutine popCoroutine;
        private Coroutine waterCoroutine;

        // Metadata 5 Fase Buah 3D
        public static readonly string[] FruitPhaseTitles = new string[]
        {
            "Fase 1/5: 🌱 Bibit & Tunas Kecil 3D",
            "Fase 2/5: 🌸 Bunga & Bakal Buah Muda 3D",
            "Fase 3/5: 🍎 Buah Matang Segar Utuh 3D",
            "Fase 4/5: 🔪 Buah Terbelah (Anatomi Dalam & Biji 3D)",
            "Fase 5/5: 🍂 Fase Layu / Busuk (Edukasi Kesegaran)"
        };

        public static readonly string[] FruitPhaseDescriptions = new string[]
        {
            "Ketuk buah 3D! Ini fase awal: biji ditanam di tanah subur lalu tumbuh tunas daun hijau kecil.",
            "Ketuk lagi! Bunga mekar lalu kelopaknya berubah menjadi bakal buah muda yang masih kecil dan hijau.",
            "Buah sudah matang sempurna! Warnanya cerah, kulitnya segar, kaya vitamin, dan siap dimakan.",
            "Lihat bagian dalam buah 3D! Ada daging buah yang segar dan biji di tengahnya yang bisa ditanam kembali.",
            "Jika dibiarkan terlalu lama, buah menjadi layu dan kecokelatan. Pilih buah yang segar di Fase 3 ya!"
        };

        public void Initialize(Vector3 worldOffset, int resolution = 512)
        {
            stageOffset = worldOffset;
            transform.position = stageOffset;

            OutputTexture = new RenderTexture(resolution, resolution, 24, RenderTextureFormat.ARGB32);
            OutputTexture.antiAliasing = 2;
            OutputTexture.Create();

            // Buat Kamera 3D khusus
            GameObject camObj = new GameObject("Stage3DCamera");
            camObj.transform.SetParent(transform, false);
            camObj.transform.localPosition = new Vector3(0f, 1.15f, -3.6f);
            camObj.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);

            stageCamera = camObj.AddComponent<Camera>();
            stageCamera.clearFlags = CameraClearFlags.SolidColor;
            stageCamera.backgroundColor = new Color(0f, 0f, 0f, 0f); // Transparan penuh untuk overlay AR
            stageCamera.targetTexture = OutputTexture;
            stageCamera.fieldOfView = 38f;
            stageCamera.nearClipPlane = 0.1f;
            stageCamera.farClipPlane = 20f;

            // Pencahayaan Utama (Key Light)
            GameObject lightObj = new GameObject("KeyLight3D");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localRotation = Quaternion.Euler(42f, -35f, 0f);
            mainLight = lightObj.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.intensity = 1.15f;
            mainLight.color = new Color(1f, 0.98f, 0.92f);

            // Pencahayaan Tambahan (Fill Light)
            GameObject fillObj = new GameObject("FillLight3D");
            fillObj.transform.SetParent(transform, false);
            fillObj.transform.localRotation = Quaternion.Euler(-20f, 145f, 0f);
            fillLight = fillObj.AddComponent<Light>();
            fillLight.type = LightType.Directional;
            fillLight.intensity = 0.5f;
            fillLight.color = new Color(0.85f, 0.92f, 1f);

            // Anchor Putar 3D
            GameObject rootObj = new GameObject("Root3DAnchor");
            rootObj.transform.SetParent(transform, false);
            rootObj.transform.localPosition = Vector3.zero;
            rootAnchor = rootObj.transform;

            GameObject turnObj = new GameObject("Turntable3D");
            turnObj.transform.SetParent(rootAnchor, false);
            turnObj.transform.localPosition = Vector3.zero;
            turntableAnchor = turnObj.transform;
        }

        private void Update()
        {
            if (turntableAnchor != null && autoRotate)
            {
                turntableAnchor.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
            }
        }

        public void RotateManual(float deltaY)
        {
            if (turntableAnchor != null)
            {
                turntableAnchor.Rotate(0f, deltaY, 0f, Space.Self);
            }
        }

        public void SetZoomScale(float scale)
        {
            currentScale = Mathf.Clamp(scale, 0.65f, 1.5f);
            if (rootAnchor != null)
            {
                rootAnchor.localScale = Vector3.one * currentScale;
            }
        }

        public float GetZoomScale() => currentScale;

        public void TriggerBouncePop()
        {
            if (popCoroutine != null) StopCoroutine(popCoroutine);
            if (gameObject.activeInHierarchy)
            {
                popCoroutine = StartCoroutine(BouncePopRoutine());
            }
        }

        private IEnumerator BouncePopRoutine()
        {
            float duration = 0.28f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float bump = 1f + Mathf.Sin(t * Mathf.PI) * 0.22f;
                if (rootAnchor != null)
                    rootAnchor.localScale = Vector3.one * (currentScale * bump);
                yield return null;
            }
            if (rootAnchor != null)
                rootAnchor.localScale = Vector3.one * currentScale;
        }

        private void ClearTurntable()
        {
            if (turntableAnchor == null) return;
            for (int i = turntableAnchor.childCount - 1; i >= 0; i--)
            {
                Destroy(turntableAnchor.GetChild(i).gameObject);
            }
        }

        // =========================================================================
        // 1. RENDER 3D BUAH DENGAN 5 FASE INTERAKTIF (SAAT DIKLIK DI AR)
        // =========================================================================
        public void RenderFruitPhase(string cardId, int phaseIndex)
        {
            ClearTurntable();
            if (stageCamera != null)
            {
                stageCamera.transform.localPosition = new Vector3(0f, 0.65f, -3.2f);
                stageCamera.transform.localRotation = Quaternion.Euler(11f, 0f, 0f);
            }

            phaseIndex = Mathf.Clamp(phaseIndex, 0, 4);
            Color mainFruitColor = GetFruitMainColor(cardId);
            Color innerFleshColor = GetFruitFleshColor(cardId);

            // Piringan Alas AR Hologram Tipis
            CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                new Vector3(0f, -0.78f, 0f), Vector3.zero, new Vector3(1.55f, 0.04f, 1.55f),
                new Color(0.2f, 0.9f, 0.65f, 0.45f));

            switch (phaseIndex)
            {
                case 0: // Fase 1: Bibit & Tunas 3D
                    BuildPhase1SeedSprout(mainFruitColor);
                    break;
                case 1: // Fase 2: Bunga & Bakal Buah Muda Hijau 3D
                    BuildPhase2BlossomAndYoungFruit();
                    break;
                case 2: // Fase 3: Buah Matang Segar Utuh 3D
                    BuildPhase3RipeWholeFruit(cardId, mainFruitColor, false);
                    break;
                case 3: // Fase 4: Buah Terbelah / Anatomi Dalam & Biji 3D
                    BuildPhase4SlicedFruitAnatomy(cardId, mainFruitColor, innerFleshColor);
                    break;
                case 4: // Fase 5: Fase Layu / Busuk 3D
                    BuildPhase3RipeWholeFruit(cardId, new Color(0.48f, 0.34f, 0.18f), true);
                    break;
            }

            TriggerBouncePop();
        }

        private void BuildPhase1SeedSprout(Color seedAccent)
        {
            // Gundukan tanah 3D
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                new Vector3(0f, -0.58f, 0f), Vector3.zero, new Vector3(1.25f, 0.45f, 1.25f),
                new Color(0.42f, 0.26f, 0.14f));

            // Biji 3D setengah terlihat
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                new Vector3(0f, -0.35f, 0f), new Vector3(0f, 0f, 18f), new Vector3(0.38f, 0.28f, 0.28f),
                Color.Lerp(seedAccent, new Color(0.55f, 0.35f, 0.15f), 0.5f));

            // Batang Tunas Hijau Muda
            CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                new Vector3(0f, 0.02f, 0f), Vector3.zero, new Vector3(0.11f, 0.42f, 0.11f),
                new Color(0.35f, 0.82f, 0.28f));

            // 2 Daun Tunas Kiri & Kanan
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                new Vector3(-0.26f, 0.38f, 0f), new Vector3(0f, 0f, 32f), new Vector3(0.48f, 0.14f, 0.26f),
                new Color(0.18f, 0.78f, 0.32f));
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                new Vector3(0.26f, 0.42f, 0f), new Vector3(0f, 0f, -32f), new Vector3(0.48f, 0.14f, 0.26f),
                new Color(0.22f, 0.85f, 0.36f));
        }

        private void BuildPhase2BlossomAndYoungFruit()
        {
            // Ranting kayu 3D
            CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                new Vector3(0f, 0.45f, 0f), new Vector3(0f, 0f, 75f), new Vector3(0.12f, 0.75f, 0.12f),
                new Color(0.45f, 0.28f, 0.14f));

            // Bunga Mekar 5 Kelopak di kiri
            Vector3 flowerCenter = new Vector3(-0.38f, 0.38f, 0.12f);
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                flowerCenter, Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f),
                new Color(1f, 0.82f, 0.15f)); // Putik sari kuning

            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f * Mathf.Deg2Rad;
                Vector3 petalOffset = new Vector3(Mathf.Cos(angle) * 0.22f, Mathf.Sin(angle) * 0.22f, 0.04f);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    flowerCenter + petalOffset, Vector3.zero, new Vector3(0.22f, 0.22f, 0.1f),
                    new Color(1f, 0.88f, 0.92f));
            }

            // Buah Muda Hijau Kecil di kanan
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                new Vector3(0.25f, -0.02f, 0f), Vector3.zero, new Vector3(0.68f, 0.72f, 0.68f),
                new Color(0.45f, 0.85f, 0.32f));

            // Daun pelindung
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                new Vector3(0.45f, 0.42f, 0f), new Vector3(0f, 0f, -28f), new Vector3(0.46f, 0.14f, 0.24f),
                new Color(0.16f, 0.68f, 0.28f));
        }

        private void BuildPhase3RipeWholeFruit(string cardId, Color bodyColor, bool isWilted)
        {
            float scaleMod = isWilted ? 0.86f : 1f;
            Vector3 tilt = isWilted ? new Vector3(12f, 0f, -14f) : Vector3.zero;

            GameObject fruitGroup = new GameObject("WholeFruit3D");
            fruitGroup.transform.SetParent(turntableAnchor, false);
            fruitGroup.transform.localPosition = Vector3.zero;
            fruitGroup.transform.localRotation = Quaternion.Euler(tilt);
            fruitGroup.transform.localScale = Vector3.one * scaleMod;

            if (cardId == "KRT-PISANG")
            {
                // Pisang 3D Melengkung (tersusun dari 4 segmen kapsul)
                Color bananaColor = isWilted ? new Color(0.45f, 0.35f, 0.15f) : new Color(1f, 0.86f, 0.12f);
                CreatePart(PrimitiveType.Capsule, fruitGroup.transform,
                    new Vector3(-0.18f, -0.25f, 0f), new Vector3(0f, 0f, -32f), new Vector3(0.36f, 0.42f, 0.36f), bananaColor);
                CreatePart(PrimitiveType.Capsule, fruitGroup.transform,
                    new Vector3(0.02f, 0.05f, 0f), new Vector3(0f, 0f, -8f), new Vector3(0.38f, 0.45f, 0.38f), bananaColor);
                CreatePart(PrimitiveType.Capsule, fruitGroup.transform,
                    new Vector3(0.08f, 0.36f, 0f), new Vector3(0f, 0f, 18f), new Vector3(0.34f, 0.40f, 0.34f), bananaColor);
                // Ujung tangkai hijau/cokelat
                CreatePart(PrimitiveType.Cylinder, fruitGroup.transform,
                    new Vector3(-0.02f, 0.68f, 0f), new Vector3(0f, 0f, 28f), new Vector3(0.12f, 0.14f, 0.12f),
                    new Color(0.38f, 0.55f, 0.18f));
            }
            else if (cardId == "KRT-MANGGA")
            {
                // Mangga Harum 3D (Bentuk lonjong asimetris)
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(0f, 0.04f, 0f), new Vector3(0f, 0f, -14f), new Vector3(1.05f, 1.32f, 0.92f), bodyColor);
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(0.16f, -0.26f, 0f), new Vector3(0f, 0f, 15f), new Vector3(0.68f, 0.75f, 0.65f),
                    Color.Lerp(bodyColor, new Color(1f, 0.85f, 0.15f), 0.45f));
            }
            else if (cardId == "KRT-JERUK")
            {
                // Jeruk Manis 3D Bulat dengan tekstur pori lembut
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(0f, 0f, 0f), Vector3.zero, new Vector3(1.22f, 1.14f, 1.22f), bodyColor);
                // Kelopak kecil hijau di atas jeruk
                CreatePart(PrimitiveType.Cylinder, fruitGroup.transform,
                    new Vector3(0f, 0.56f, 0f), Vector3.zero, new Vector3(0.24f, 0.04f, 0.24f),
                    new Color(0.18f, 0.58f, 0.22f));
            }
            else
            {
                // Apel Merah 3D (2 belahan bulat membentuk lekukan khas apel di atas & bawah)
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(-0.14f, 0f, 0f), new Vector3(0f, 0f, 8f), new Vector3(1.02f, 1.12f, 1.08f), bodyColor);
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(0.14f, 0f, 0f), new Vector3(0f, 0f, -8f), new Vector3(1.02f, 1.12f, 1.08f), bodyColor);
            }

            // Tangkai & Daun di bagian atas (untuk Apel, Jeruk, Mangga)
            if (cardId != "KRT-PISANG")
            {
                CreatePart(PrimitiveType.Cylinder, fruitGroup.transform,
                    new Vector3(0f, 0.64f, 0f), new Vector3(0f, 0f, 10f), new Vector3(0.09f, 0.18f, 0.09f),
                    new Color(0.42f, 0.25f, 0.12f));

                Color leafColor = isWilted ? new Color(0.55f, 0.42f, 0.18f) : new Color(0.16f, 0.76f, 0.28f);
                Vector3 leafRot = isWilted ? new Vector3(25f, 0f, -45f) : new Vector3(0f, 0f, -24f);
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(0.26f, 0.68f, 0f), leafRot, new Vector3(0.48f, 0.12f, 0.24f), leafColor);
            }

            // Bintik gelap jika fase layu/busuk (Fase 5)
            if (isWilted)
            {
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(0.22f, 0.08f, -0.48f), Vector3.zero, new Vector3(0.28f, 0.24f, 0.12f),
                    new Color(0.25f, 0.16f, 0.08f));
                CreatePart(PrimitiveType.Sphere, fruitGroup.transform,
                    new Vector3(-0.24f, -0.18f, -0.46f), Vector3.zero, new Vector3(0.22f, 0.20f, 0.12f),
                    new Color(0.22f, 0.14f, 0.06f));
            }
        }

        private void BuildPhase4SlicedFruitAnatomy(string cardId, Color skinColor, Color fleshColor)
        {
            // Belahan Kiri (Kulit Luar + Daging Dalam)
            GameObject leftHalf = new GameObject("LeftHalf3D");
            leftHalf.transform.SetParent(turntableAnchor, false);
            leftHalf.transform.localPosition = new Vector3(-0.42f, -0.02f, 0f);
            leftHalf.transform.localRotation = Quaternion.Euler(0f, 18f, 8f);

            CreatePart(PrimitiveType.Sphere, leftHalf.transform,
                Vector3.zero, Vector3.zero, new Vector3(0.85f, 1.05f, 0.85f), skinColor);
            CreatePart(PrimitiveType.Cylinder, leftHalf.transform,
                new Vector3(0.08f, 0f, -0.12f), new Vector3(90f, 0f, 0f), new Vector3(0.76f, 0.08f, 0.94f), fleshColor);

            // Belahan Kanan (Menampilkan Biji / Inti Buah 3D)
            GameObject rightHalf = new GameObject("RightHalf3D");
            rightHalf.transform.SetParent(turntableAnchor, false);
            rightHalf.transform.localPosition = new Vector3(0.42f, -0.02f, 0f);
            rightHalf.transform.localRotation = Quaternion.Euler(0f, -18f, -8f);

            CreatePart(PrimitiveType.Sphere, rightHalf.transform,
                Vector3.zero, Vector3.zero, new Vector3(0.85f, 1.05f, 0.85f), skinColor);
            CreatePart(PrimitiveType.Cylinder, rightHalf.transform,
                new Vector3(-0.08f, 0f, -0.12f), new Vector3(90f, 0f, 0f), new Vector3(0.76f, 0.08f, 0.94f), fleshColor);

            // Biji 3D di tengah daging buah
            Color seedColor = cardId == "KRT-MANGGA"
                ? new Color(0.82f, 0.72f, 0.48f)
                : new Color(0.28f, 0.15f, 0.06f);
            Vector3 seedScale = cardId == "KRT-MANGGA"
                ? new Vector3(0.34f, 0.55f, 0.14f)
                : new Vector3(0.12f, 0.18f, 0.10f);

            CreatePart(PrimitiveType.Sphere, leftHalf.transform,
                new Vector3(0.08f, 0.05f, -0.21f), Vector3.zero, seedScale, seedColor);
            CreatePart(PrimitiveType.Sphere, rightHalf.transform,
                new Vector3(-0.08f, -0.05f, -0.21f), Vector3.zero, seedScale, seedColor);
        }

        // =========================================================================
        // 2. RENDER 3D KEBUN YANG BERTUMBUH SEIRING LEVEL (LEVEL 1 S/D LEVEL 7)
        // =========================================================================
        public void RenderGardenLevel(string fruitName, int level, bool isWilted)
        {
            ClearTurntable();
            if (stageCamera != null)
            {
                stageCamera.transform.localPosition = new Vector3(0f, 1.45f, -4.1f);
                stageCamera.transform.localRotation = Quaternion.Euler(16f, 0f, 0f);
            }

            level = Mathf.Clamp(level, 1, 7);

            // 1. Pulau Rumput 3D (Diorama Base)
            Color grassColor = isWilted ? new Color(0.68f, 0.62f, 0.28f) : new Color(0.26f, 0.78f, 0.36f);
            CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                new Vector3(0f, -0.95f, 0f), Vector3.zero, new Vector3(2.55f, 0.16f, 2.55f), grassColor);

            // Pinggiran Tanah Bawah Pulau
            CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                new Vector3(0f, -1.14f, 0f), Vector3.zero, new Vector3(2.35f, 0.12f, 2.35f),
                new Color(0.45f, 0.29f, 0.16f));

            // Gundukan Tanah Kebun di Tengah
            Color soilColor = isWilted ? new Color(0.62f, 0.48f, 0.32f) : new Color(0.36f, 0.22f, 0.11f);
            CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                new Vector3(0f, -0.78f, 0f), Vector3.zero, new Vector3(1.45f, 0.08f, 1.45f), soilColor);

            // 2. Pagar Kayu 3D di Belakang Kebun
            BuildWoodenFence3D();

            // 3. Pohon / Tanaman 3D yang Bertumbuh sesuai Level (1..7)
            Color foliageColor = isWilted ? new Color(0.65f, 0.56f, 0.22f) : new Color(0.15f, 0.72f, 0.30f);
            Color foliageLight = isWilted ? new Color(0.72f, 0.62f, 0.28f) : new Color(0.24f, 0.84f, 0.38f);
            Color fruitColor = GetFruitColorByName(fruitName);

            if (level == 1)
            {
                // Level 1: Biji di Tanah + Tunas Mini
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0f, -0.64f, 0f), Vector3.zero, new Vector3(0.34f, 0.24f, 0.34f),
                    new Color(0.55f, 0.36f, 0.16f));
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0f, -0.48f, 0f), Vector3.zero, new Vector3(0.16f, 0.22f, 0.16f), foliageLight);
            }
            else if (level == 2)
            {
                // Level 2: Berkecambah (Batang pendek + 2 daun)
                CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                    new Vector3(0f, -0.48f, 0f), Vector3.zero, new Vector3(0.10f, 0.28f, 0.10f), foliageLight);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(-0.20f, -0.25f, 0f), new Vector3(0f, 0f, 28f), new Vector3(0.36f, 0.10f, 0.20f), foliageColor);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0.20f, -0.25f, 0f), new Vector3(0f, 0f, -28f), new Vector3(0.36f, 0.10f, 0.20f), foliageLight);
            }
            else if (level == 3)
            {
                // Level 3: Tunas Muda (Batang sedang + 4 daun rimbun)
                CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                    new Vector3(0f, -0.32f, 0f), Vector3.zero, new Vector3(0.14f, 0.46f, 0.14f),
                    new Color(0.45f, 0.29f, 0.14f));
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0f, 0.15f, 0f), Vector3.zero, new Vector3(0.75f, 0.55f, 0.75f), foliageColor);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0f, 0.42f, 0f), Vector3.zero, new Vector3(0.52f, 0.42f, 0.52f), foliageLight);
            }
            else
            {
                // Level 4, 5, 6, 7: Pohon Kayu Bertumbuh Besar & Rimbun
                float growthFactor = 0.78f + (level - 4) * 0.11f; // Makin tinggi level makin besar pohonnya!
                CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                    new Vector3(0f, -0.18f, 0f), Vector3.zero,
                    new Vector3(0.24f * growthFactor, 0.62f * growthFactor, 0.24f * growthFactor),
                    new Color(0.44f, 0.27f, 0.13f));

                // Mahkota Daun 3D Berlapis
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0f, 0.48f * growthFactor, 0f), Vector3.zero,
                    new Vector3(1.35f * growthFactor, 1.05f * growthFactor, 1.35f * growthFactor), foliageColor);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(-0.42f * growthFactor, 0.36f * growthFactor, 0.18f), Vector3.zero,
                    new Vector3(0.88f * growthFactor, 0.78f * growthFactor, 0.88f * growthFactor), foliageLight);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0.42f * growthFactor, 0.38f * growthFactor, -0.15f), Vector3.zero,
                    new Vector3(0.88f * growthFactor, 0.78f * growthFactor, 0.88f * growthFactor), foliageLight);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    new Vector3(0f, 0.92f * growthFactor, 0f), Vector3.zero,
                    new Vector3(0.95f * growthFactor, 0.75f * growthFactor, 0.95f * growthFactor), foliageLight);

                // Level 5: Muncul Bunga-Bunga Mekar di Pohon 3D
                if (level == 5)
                {
                    SpawnCanopyOrnaments(6, growthFactor, new Color(1f, 0.86f, 0.92f), 0.18f);
                }
                // Level 6: Muncul Buah-Buah Muda Hijau di Pohon 3D
                else if (level == 6)
                {
                    SpawnCanopyOrnaments(6, growthFactor, new Color(0.52f, 0.90f, 0.32f), 0.22f);
                }
                // Level 7: Pohon Panen Raya! Buah Matang Merona + Keranjang Panen 3D di Bawah Pohon
                else if (level >= 7)
                {
                    SpawnCanopyOrnaments(8, growthFactor, fruitColor, 0.26f);
                    BuildHarvestBasket3D(fruitColor);
                }
            }

            TriggerBouncePop();
        }

        private void BuildWoodenFence3D()
        {
            Color woodColor = new Color(0.68f, 0.46f, 0.24f);
            for (int i = -2; i <= 2; i++)
            {
                float angle = (i * 22f + 180f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * 1.05f, -0.58f, -Mathf.Cos(angle) * 1.05f);
                CreatePart(PrimitiveType.Cube, turntableAnchor,
                    pos, new Vector3(0f, i * 22f, 0f), new Vector3(0.10f, 0.48f, 0.06f), woodColor);
            }
        }

        private void SpawnCanopyOrnaments(int count, float growthFactor, Color ornamentColor, float size)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = (i * (360f / count)) * Mathf.Deg2Rad;
                float radius = 0.58f * growthFactor;
                float yPos = (0.36f + (i % 2) * 0.28f) * growthFactor;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, yPos, Mathf.Sin(angle) * radius);
                CreatePart(PrimitiveType.Sphere, turntableAnchor,
                    pos, Vector3.zero, Vector3.one * size, ornamentColor);
            }
        }

        private void BuildHarvestBasket3D(Color fruitColor)
        {
            // Keranjang Panen di samping pohon
            Vector3 basketPos = new Vector3(0.68f, -0.66f, -0.35f);
            CreatePart(PrimitiveType.Cylinder, turntableAnchor,
                basketPos, Vector3.zero, new Vector3(0.46f, 0.14f, 0.46f),
                new Color(0.62f, 0.40f, 0.18f));

            // Tumpukan buah 3D di dalam keranjang
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                basketPos + new Vector3(-0.08f, 0.14f, 0f), Vector3.zero, Vector3.one * 0.18f, fruitColor);
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                basketPos + new Vector3(0.08f, 0.14f, 0.05f), Vector3.zero, Vector3.one * 0.18f, fruitColor);
            CreatePart(PrimitiveType.Sphere, turntableAnchor,
                basketPos + new Vector3(0f, 0.22f, -0.04f), Vector3.zero, Vector3.one * 0.18f, fruitColor);
        }

        /// <summary>
        /// Memainkan animasi tetesan air hujan 3D saat anak menekan tombol Siram Tanaman.
        /// </summary>
        public void PlayWateringEffect()
        {
            if (waterCoroutine != null) StopCoroutine(waterCoroutine);
            if (gameObject.activeInHierarchy)
            {
                waterCoroutine = StartCoroutine(WaterDropsRoutine());
            }
        }

        private IEnumerator WaterDropsRoutine()
        {
            if (waterDropsAnchor != null) Destroy(waterDropsAnchor.gameObject);

            GameObject dropGroup = new GameObject("WaterDrops3D");
            dropGroup.transform.SetParent(rootAnchor, false);
            waterDropsAnchor = dropGroup.transform;

            List<Transform> drops = new List<Transform>();
            Color waterColor = new Color(0.22f, 0.72f, 0.98f);
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = new Vector3(
                    UnityEngine.Random.Range(-0.45f, 0.45f),
                    UnityEngine.Random.Range(0.8f, 1.45f),
                    UnityEngine.Random.Range(-0.45f, 0.45f));
                GameObject drop = CreatePart(PrimitiveType.Sphere, waterDropsAnchor,
                    pos, Vector3.zero, new Vector3(0.09f, 0.15f, 0.09f), waterColor);
                drops.Add(drop.transform);
            }

            float elapsed = 0f;
            while (elapsed < 0.75f)
            {
                elapsed += Time.deltaTime;
                foreach (var d in drops)
                {
                    if (d != null)
                        d.localPosition += Vector3.down * (1.8f * Time.deltaTime);
                }
                yield return null;
            }

            if (waterDropsAnchor != null) Destroy(waterDropsAnchor.gameObject);
        }

        private GameObject CreatePart(PrimitiveType type, Transform parent, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Color color)
        {
            GameObject obj = GameObject.CreatePrimitive(type);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localRotation = Quaternion.Euler(localEuler);
            obj.transform.localScale = localScale;

            Collider col = obj.GetComponent<Collider>();
            if (col != null) Destroy(col);

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                                ?? Shader.Find("Standard")
                                ?? Shader.Find("Mobile/Diffuse")
                                ?? Shader.Find("Sprites/Default");
                Material mat = new Material(shader);
                mat.color = color;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", 0.65f);
                if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", 0.65f);
                rend.material = mat;
            }

            return obj;
        }

        public static Color GetFruitMainColor(string cardId)
        {
            switch (cardId)
            {
                case "KRT-JERUK": return new Color(1f, 0.52f, 0.05f);   // Oranye Cerah
                case "KRT-MANGGA": return new Color(0.98f, 0.72f, 0.08f); // Kuning Mangga Harum
                case "KRT-PISANG": return new Color(1f, 0.86f, 0.14f);  // Kuning Emas
                case "KRT-APEL":
                default: return new Color(0.92f, 0.16f, 0.22f);         // Merah Apel Segar
            }
        }

        public static Color GetFruitFleshColor(string cardId)
        {
            switch (cardId)
            {
                case "KRT-JERUK": return new Color(1f, 0.74f, 0.25f);
                case "KRT-MANGGA": return new Color(1f, 0.82f, 0.18f);
                case "KRT-PISANG": return new Color(1f, 0.96f, 0.78f);
                case "KRT-APEL":
                default: return new Color(1f, 0.96f, 0.86f);
            }
        }

        public static Color GetFruitColorByName(string fruitName)
        {
            if (string.IsNullOrEmpty(fruitName)) return new Color(0.92f, 0.16f, 0.22f);
            string lower = fruitName.ToLowerInvariant();
            if (lower.Contains("jeruk")) return new Color(1f, 0.52f, 0.05f);
            if (lower.Contains("mangga")) return new Color(0.98f, 0.72f, 0.08f);
            if (lower.Contains("pisang")) return new Color(1f, 0.86f, 0.14f);
            return new Color(0.92f, 0.16f, 0.22f);
        }

        private void OnDestroy()
        {
            if (OutputTexture != null)
            {
                OutputTexture.Release();
                Destroy(OutputTexture);
            }
        }
    }
}
