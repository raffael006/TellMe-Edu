using System;
using UnityEngine;

namespace KawanDengarEdu.AR3D
{
    /// <summary>
    /// Mengelola animasi gigitan buah saat siswa mengklik/menyentuh objek apel 3D.
    /// Mendukung transisi visual gigitan (1x, 2x, 3x) hingga buah habis dimakan,
    /// serta memancarkan partikel serpihan daging buah segar.
    /// </summary>
    public class FruitBiteController : MonoBehaviour
    {
        [Header("Bite Stages")]
        [Tooltip("Tahap gigitan saat ini (0 = Utuh, 1 = Gigitan 1, 2 = Gigitan 2, 3 = Gigitan 3, 4 = Habis)")]
        [Range(0, 4)]
        public int currentBiteStage = 0;

        [Header("Mesh References (Bite Variations)")]
        [Tooltip("Model buah utuh")]
        public GameObject wholeFruitMesh;
        [Tooltip("Model buah gigitan 1")]
        public GameObject bite1Mesh;
        [Tooltip("Model buah gigitan 2")]
        public GameObject bite2Mesh;
        [Tooltip("Model sisa biji/tangkai (gigitan 3)")]
        public GameObject bite3CoreMesh;

        [Header("Visual Effects")]
        [Tooltip("Sistem partikel remah-remah buah saat digigit")]
        public ParticleSystem crumbParticlePrefab;

        [Header("Bite Animation Shake")]
        public float bitePunchScale = 1.15f;
        public float animSpeed = 12f;

        private Vector3 originalScale;
        private Vector3 targetScale;

        public event Action<int> OnBiteStageChanged;
        public event Action OnFruitFinished;

        private void Start()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
            UpdateBiteVisuals();
        }

        private void Update()
        {
            // Deteksi Klik / Sentuhan langsung pada objek 3D
            DetectFruitClick();

            // Animasi squash & stretch saat digigit
            if (transform.localScale != targetScale)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animSpeed);
                if (Vector3.Distance(transform.localScale, targetScale) < 0.005f)
                {
                    targetScale = originalScale;
                }
            }
        }

        private void DetectFruitClick()
        {
            bool hasClicked = false;
            Vector2 screenPos = Vector2.zero;

            // Touch input Android
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                if (t.phase == TouchPhase.Began)
                {
                    hasClicked = true;
                    screenPos = t.position;
                }
            }
            // Mouse click Editor
            else if (Input.GetMouseButtonDown(0))
            {
                hasClicked = true;
                screenPos = Input.mousePosition;
            }

            if (hasClicked)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Ray ray = cam.ScreenPointToRay(screenPos);
                    if (Physics.Raycast(ray, out RaycastHit hit))
                    {
                        if (hit.transform == transform || hit.transform.IsChildOf(transform))
                        {
                            TakeBite();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mengeksekusi 1x gigitan pada buah
        /// </summary>
        public void TakeBite()
        {
            if (currentBiteStage < 4)
            {
                currentBiteStage++;

                // Efek squash scale (pantulan gigitan renyah)
                transform.localScale = originalScale * bitePunchScale;
                targetScale = originalScale;

                // Pancarkan partikel remah
                if (crumbParticlePrefab != null)
                {
                    crumbParticlePrefab.Play();
                }

                UpdateBiteVisuals();
                OnBiteStageChanged?.Invoke(currentBiteStage);

                if (currentBiteStage == 4)
                {
                    OnFruitFinished?.Invoke();
                }
            }
        }

        public void ResetFruit()
        {
            currentBiteStage = 0;
            transform.localScale = originalScale;
            targetScale = originalScale;
            UpdateBiteVisuals();
            OnBiteStageChanged?.Invoke(currentBiteStage);
        }

        private void UpdateBiteVisuals()
        {
            if (wholeFruitMesh != null) wholeFruitMesh.SetActive(currentBiteStage == 0);
            if (bite1Mesh != null) bite1Mesh.SetActive(currentBiteStage == 1);
            if (bite2Mesh != null) bite2Mesh.SetActive(currentBiteStage == 2);
            if (bite3CoreMesh != null) bite3CoreMesh.SetActive(currentBiteStage == 3);

            // Jika tahap 4 (habis), semua mesh mati sementara
            if (currentBiteStage == 4)
            {
                if (wholeFruitMesh != null) wholeFruitMesh.SetActive(false);
                if (bite1Mesh != null) bite1Mesh.SetActive(false);
                if (bite2Mesh != null) bite2Mesh.SetActive(false);
                if (bite3CoreMesh != null) bite3CoreMesh.SetActive(false);
            }
        }
    }
}
