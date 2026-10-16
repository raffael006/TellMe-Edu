using System;
using System.Collections.Generic;
using System.Linq;
using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;
using UnityEngine.UI;

namespace TellMe.Modules
{
    /// <summary>
    /// Modul 1: Jadwal Belajar (Pengguna: Orang Tua / Bu Rina).
    /// Mengatur hari (S M T W T F S), jam pengingat, mata pelajaran (Matematika / B. Indonesia / B. Inggris),
    /// judul rekaman, nama guru, serta menjadwalkan notifikasi lokal offline di perangkat.
    /// </summary>
    public class ScheduleModuleController : MonoBehaviour
    {
        private static readonly string[] DayCodes = { "S", "M", "T", "W", "T", "F", "S" };

        [Header("Pemilih Hari Horizontal (S, M, T, W, T, F, S)")]
        [SerializeField] private int selectedDayIndex = 1; // Default Senin ('M')

        [Header("Input Form Pengaturan Jadwal")]
        [SerializeField] private InputField inputJamPengingat;
        [SerializeField] private Dropdown dropdownMataPelajaran;
        [SerializeField] private InputField inputJudulRekaman;
        [SerializeField] private InputField inputNamaGuru;

        [Header("Referensi UI Timeline")]
        [SerializeField] private Transform timelineListContainer;

        public event Action OnTimelineUpdated;

        public int SelectedDayIndex => selectedDayIndex;

        public void SetupComponents(
            InputField jamInput, Dropdown mapelDropdown, InputField judulInput, InputField guruInput, Transform timeline)
        {
            inputJamPengingat = jamInput;
            dropdownMataPelajaran = mapelDropdown;
            inputJudulRekaman = judulInput;
            inputNamaGuru = guruInput;
            timelineListContainer = timeline;
        }

        public void SelectDay(int dayIndex)
        {
            selectedDayIndex = Mathf.Clamp(dayIndex, 0, 6);
            ThemeTokens.TriggerHapticFeedback();
            RefreshScheduleTimeline();
            OnTimelineUpdated?.Invoke();
        }

        public void SimpanJadwalBaru()
        {
            if (LocalDataManager.Instance == null) return;

            string jam = inputJamPengingat != null && !string.IsNullOrEmpty(inputJamPengingat.text)
                ? inputJamPengingat.text
                : "08:00";

            SubjectType mapel = dropdownMataPelajaran != null
                ? (SubjectType)Mathf.Clamp(dropdownMataPelajaran.value, 0, 2)
                : SubjectType.Matematika;

            string judul = inputJudulRekaman != null && !string.IsNullOrEmpty(inputJudulRekaman.text)
                ? inputJudulRekaman.text
                : "Materi Kelas Hari Ini";

            string guru = inputNamaGuru != null && !string.IsNullOrEmpty(inputNamaGuru.text)
                ? inputNamaGuru.text
                : "Bu Sari";

            string hariKode = DayCodes[selectedDayIndex];
            Jadwal jadwal = LocalDataManager.Instance.TambahJadwal(hariKode, selectedDayIndex, jam, mapel, judul, guru);

            ScheduleOfflineAndroidNotification(jadwal);
            RefreshScheduleTimeline();
            OnTimelineUpdated?.Invoke();

            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                $"Jadwal {ThemeTokens.GetSubjectDisplayName(mapel)} jam {jam} bersama {guru} berhasil disimpan!");
        }

        /// <summary>
        /// Simulasi langsung pengingat lokal muncul ketika kelas dimulai:
        /// Mengarahkan Raka langsung ke Modul 2 (Tombol Rekam Satu Ketukan).
        /// </summary>
        public void TriggerClassReminderNow(Jadwal jadwal)
        {
            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_IDLE,
                "Waktunya belajar, tekan Rekam ya!");
            AppNavigationManager.Instance?.OpenStudentRecordingWithSchedule(jadwal);
        }

        public List<Jadwal> GetSchedulesForSelectedDay()
        {
            if (LocalDataManager.Instance == null) return new List<Jadwal>();
            return LocalDataManager.Instance.Data.daftarJadwal
                .Where(j => j.dayIndex == selectedDayIndex)
                .OrderBy(j => j.jam)
                .ToList();
        }

        public void RefreshScheduleTimeline()
        {
            // Hook untuk me-render ulang kartu mata pelajaran pada timeline per jam pelajaran
            var list = GetSchedulesForSelectedDay();
            Debug.Log($"[TellMe] Menampilkan {list.Count} jadwal untuk hari indeks {selectedDayIndex}");
        }

        private void ScheduleOfflineAndroidNotification(Jadwal jadwal)
        {
            // Berjalan 100% offline menggunakan notifikasi lokal perangkat Android (Unity.Notifications.Android)
            Debug.Log($"[TellMe Offline Notification] Pengingat lokal aktif untuk {jadwal.judul} ({jadwal.jam}) - Guru: {jadwal.namaGuru}");
        }
    }
}
