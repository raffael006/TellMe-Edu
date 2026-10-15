using UnityEngine;

namespace KawanDengarEdu.AR3D
{
    /// <summary>
    /// Controller rotasi 3D trackball segala arah (All-Direction Rotation)
    /// Mendukung sentuhan jari Android (Touch) dan klik-geser mouse di Unity Editor.
    /// Memungkinkan siswa memutar buah 360 derajat ke segala sudut (atas, bawah, samping, diagonal).
    /// </summary>
    public class TrackballFruitRotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Sensitivitas perputaran saat digeser")]
        public float sensitivity = 0.35f;

        [Tooltip("Efek inersia kelembaman (0 = berhenti instan, 0.95 = licin berputar halus)")]
        [Range(0f, 0.98f)]
        public float inertiaDamping = 0.90f;

        [Header("Auto Idle Spin (Optional)")]
        public bool autoIdleSpin = true;
        public float idleSpinSpeed = 12f;

        private Vector2 lastPointerPos;
        private Vector2 angularVelocity;
        private bool isDragging = false;
        private Quaternion initialRotation;

        private void Start()
        {
            initialRotation = transform.localRotation;
        }

        private void Update()
        {
            HandleInput();
            ApplyRotation();
        }

        private void HandleInput()
        {
            // 1. Dukungan Sentuhan Android (Touch)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    lastPointerPos = touch.position;
                    angularVelocity = Vector2.zero;
                }
                else if (touch.phase == TouchPhase.Moved && isDragging)
                {
                    Vector2 delta = touch.position - lastPointerPos;
                    angularVelocity = delta * sensitivity;
                    lastPointerPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isDragging = false;
                }
                return;
            }

            // 2. Dukungan Mouse Drag di Unity Editor / Desktop
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastPointerPos = Input.mousePosition;
                angularVelocity = Vector2.zero;
            }
            else if (Input.GetMouseButton(0) && isDragging)
            {
                Vector2 currentPos = (Vector2)Input.mousePosition;
                Vector2 delta = currentPos - lastPointerPos;
                angularVelocity = delta * sensitivity;
                lastPointerPos = currentPos;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }
        }

        private void ApplyRotation()
        {
            Camera cam = Camera.main;
            Transform camTransform = cam != null ? cam.transform : null;

            if (angularVelocity.sqrMagnitude > 0.001f)
            {
                // Hitung arah putar relatif terhadap pandangan kamera (Trackball Sphere)
                Vector3 rotAxisUp = camTransform != null ? camTransform.up : Vector3.up;
                Vector3 rotAxisRight = camTransform != null ? camTransform.right : Vector3.right;

                transform.Rotate(rotAxisUp, -angularVelocity.x, Space.World);
                transform.Rotate(rotAxisRight, angularVelocity.y, Space.World);

                if (!isDragging)
                {
                    // Efek inersia melambat halus
                    angularVelocity *= inertiaDamping;
                }
            }
            else if (autoIdleSpin && !isDragging)
            {
                // Putar pelan saat diam agar objek terasa hidup dan 3D
                transform.Rotate(Vector3.up, idleSpinSpeed * Time.deltaTime, Space.World);
            }
        }

        public void ResetRotation()
        {
            angularVelocity = Vector2.zero;
            transform.localRotation = initialRotation;
        }
    }
}
