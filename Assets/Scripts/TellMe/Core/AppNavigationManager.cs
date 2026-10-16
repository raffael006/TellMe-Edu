using System;
using TellMe.Mascot;
using UnityEngine;

namespace TellMe.Core
{
    public enum TellMeScreen
    {
        Screen0_RoleSelection = 0,
        Module1_JadwalOrangTua = 1,
        Module2_SpeechToTextBelajar = 2,
        Module3_KuisDanNilai = 3,
        Module4_KuisBahasaIsyarat = 4,
        Module5_BerandaBerryBears = 5,
        Module6_KartuDigital = 6,
        Module7_ARBuah = 7,
        Module8_TumbuhKembangTanaman = 8
    }

    /// <summary>
    /// Mengatur navigasi antar Layar 0 (Pemilihan Peran) dan 8 Modul Inti TellMe,
    /// termasuk Floating Bottom Navigation Bar dan sinkronisasi state BerryBears.
    /// </summary>
    public class AppNavigationManager : MonoBehaviour
    {
        public static AppNavigationManager Instance { get; private set; }

        [Header("Panel Layar (Layar 0 + 8 Modul Inti)")]
        [SerializeField] private GameObject screen0RoleSelection;
        [SerializeField] private GameObject module1JadwalPanel;
        [SerializeField] private GameObject module2SttPanel;
        [SerializeField] private GameObject module3KuisNilaiPanel;
        [SerializeField] private GameObject module4KuisIsyaratPanel;
        [SerializeField] private GameObject module5HeroMascotPanel;
        [SerializeField] private GameObject module6KartuDigitalPanel;
        [SerializeField] private GameObject module7ArBuahPanel;
        [SerializeField] private GameObject module8TanamanPanel;

        [Header("Komponen Navigasi Global")]
        [SerializeField] private GameObject floatingBottomNavBar;

        public TellMeScreen CurrentScreen { get; private set; } = TellMeScreen.Screen0_RoleSelection;
        public Jadwal ActiveScheduleForSession { get; private set; }
        public Materi ActiveMateriForQuiz { get; private set; }
        public Kartu ActiveCardForAR { get; private set; }

        public event Action<TellMeScreen> OnScreenChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterPanels(
            GameObject s0, GameObject m1, GameObject m2, GameObject m3,
            GameObject m4, GameObject m5, GameObject m6, GameObject m7, GameObject m8,
            GameObject bottomNav)
        {
            screen0RoleSelection = s0;
            module1JadwalPanel = m1;
            module2SttPanel = m2;
            module3KuisNilaiPanel = m3;
            module4KuisIsyaratPanel = m4;
            module5HeroMascotPanel = m5;
            module6KartuDigitalPanel = m6;
            module7ArBuahPanel = m7;
            module8TanamanPanel = m8;
            floatingBottomNavBar = bottomNav;
        }

        private void Start()
        {
            NavigateTo(TellMeScreen.Screen0_RoleSelection);
        }

        private TellMeScreen previousScreen = TellMeScreen.Screen0_RoleSelection;

        public void NavigateTo(TellMeScreen targetScreen)
        {
            if (CurrentScreen != targetScreen)
            {
                previousScreen = CurrentScreen;
            }
            CurrentScreen = targetScreen;
            ThemeTokens.TriggerHapticFeedback();

            SetActiveSafe(screen0RoleSelection, targetScreen == TellMeScreen.Screen0_RoleSelection);
            SetActiveSafe(module1JadwalPanel, targetScreen == TellMeScreen.Module1_JadwalOrangTua);
            SetActiveSafe(module2SttPanel, targetScreen == TellMeScreen.Module2_SpeechToTextBelajar);
            SetActiveSafe(module3KuisNilaiPanel, targetScreen == TellMeScreen.Module3_KuisDanNilai);
            SetActiveSafe(module4KuisIsyaratPanel, targetScreen == TellMeScreen.Module4_KuisBahasaIsyarat);
            SetActiveSafe(module5HeroMascotPanel, targetScreen == TellMeScreen.Module5_BerandaBerryBears);
            SetActiveSafe(module6KartuDigitalPanel, targetScreen == TellMeScreen.Module6_KartuDigital);
            SetActiveSafe(module7ArBuahPanel, targetScreen == TellMeScreen.Module7_ARBuah);
            SetActiveSafe(module8TanamanPanel, targetScreen == TellMeScreen.Module8_TumbuhKembangTanaman);

            // Floating Bottom Nav tampil pada Mode Siswa (selain Layar 0 dan layar kamera AR penuh)
            bool showBottomNav = targetScreen != TellMeScreen.Screen0_RoleSelection
                                 && targetScreen != TellMeScreen.Module1_JadwalOrangTua
                                 && targetScreen != TellMeScreen.Module7_ARBuah;
            SetActiveSafe(floatingBottomNavBar, showBottomNav);

            UpdateMascotForScreen(targetScreen);
            OnScreenChanged?.Invoke(targetScreen);
        }

        public void GoBack()
        {
            if (CurrentScreen == TellMeScreen.Module7_ARBuah)
            {
                NavigateTo(TellMeScreen.Module6_KartuDigital);
            }
            else if (CurrentScreen == TellMeScreen.Module1_JadwalOrangTua || CurrentScreen == TellMeScreen.Module5_BerandaBerryBears)
            {
                NavigateTo(TellMeScreen.Screen0_RoleSelection);
            }
            else if (previousScreen != CurrentScreen)
            {
                NavigateTo(previousScreen);
            }
            else
            {
                NavigateTo(TellMeScreen.Module5_BerandaBerryBears);
            }
        }

        /// <summary>
        /// Alur Langkah 2: Saat pengingat jadwal muncul, siswa langsung diarahkan ke Modul 2 (Rekam STT).
        /// </summary>
        public void OpenStudentRecordingWithSchedule(Jadwal jadwal)
        {
            ActiveScheduleForSession = jadwal;
            NavigateTo(TellMeScreen.Module2_SpeechToTextBelajar);
        }

        /// <summary>
        /// Alur Langkah 4: Siswa mengerjakan Kuis Mini dari materi yang baru direkam.
        /// </summary>
        public void OpenQuizForMateri(Materi materi)
        {
            ActiveMateriForQuiz = materi;
            NavigateTo(TellMeScreen.Module3_KuisDanNilai);
        }

        /// <summary>
        /// Alur Langkah 7: Kartu Digital membuka akses ke modul AR Objek Buah.
        /// </summary>
        public void OpenARFruitWithCard(Kartu kartu)
        {
            ActiveCardForAR = kartu;
            NavigateTo(TellMeScreen.Module7_ARBuah);
        }

        private void UpdateMascotForScreen(TellMeScreen screen)
        {
            if (BerryBearsController.Instance == null) return;

            switch (screen)
            {
                case TellMeScreen.Screen0_RoleSelection:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_IDLE,
                        "Halo! Pilih Mode Orang Tua untuk atur jadwal, atau Mode Siswa untuk mulai belajar!");
                    break;
                case TellMeScreen.Module1_JadwalOrangTua:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_IDLE,
                        "Halo Ayah & Ibu! Atur jadwal belajar sekali saja, nanti BerryBears yang ingatkan Raka!");
                    break;
                case TellMeScreen.Module2_SpeechToTextBelajar:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_IDLE,
                        "Waktunya belajar! Tekan tombol kuning besar REKAM saat guru mulai menjelaskan ya!");
                    break;
                case TellMeScreen.Module3_KuisDanNilai:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_QUIZ_THINKING,
                        "Ayo kerjakan Kuis Mini! Kalau nilaimu bagus, kamu dapat Kartu Digital AR Buah!");
                    break;
                case TellMeScreen.Module4_KuisBahasaIsyarat:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_QUIZ_THINKING,
                        "Perhatikan gerakan tangan SIBI/BISINDO di panggung, lalu pilih kata yang tepat!");
                    break;
                case TellMeScreen.Module5_BerandaBerryBears:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_IDLE,
                        "Waktunya Belajar! Halo Raka, siap belajar bareng BerryBears hari ini?");
                    break;
                case TellMeScreen.Module6_KartuDigital:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_IDLE,
                        "Ini koleksi Kartu Buah 3D milikmu! Tekan tombol hijau untuk melihat buah di AR!");
                    break;
                case TellMeScreen.Module7_ARBuah:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_ON_TAP,
                        "Arahkan kamera ke meja belajarmu, lalu ketuk layar untuk memunculkan Buah 3D!");
                    break;
                case TellMeScreen.Module8_TumbuhKembangTanaman:
                    BerryBearsController.Instance.SetMascotState(
                        BerryBearsState.STATE_IDLE,
                        "Ketuk tahap Bibit, Tunas, Pohon, atau Berbuah untuk melihat tanaman tumbuh!");
                    break;
            }
        }

        private void SetActiveSafe(GameObject obj, bool state)
        {
            if (obj != null && obj.activeSelf != state)
            {
                obj.SetActive(state);
            }
        }
    }
}
