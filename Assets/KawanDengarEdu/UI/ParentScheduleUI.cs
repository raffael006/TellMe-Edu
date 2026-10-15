using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.Models;

namespace KawanDengarEdu.UI
{
    public class ParentScheduleUI : MonoBehaviour
    {
        [Header("State Panels")]
        public GameObject pinGatePanel;
        public GameObject scheduleEditorPanel;

        [Header("PIN Inputs")]
        public TMP_InputField pinInputField;
        public TextMeshProUGUI pinErrorMessage;
        public Button verifyPinButton;

        [Header("Schedule Form Inputs")]
        public TMP_Dropdown subjectDropdown;
        public TMP_InputField teacherNameField;
        public TMP_Dropdown dayDropdown;
        public TMP_InputField startTimeField;
        public TMP_InputField endTimeField;
        public Button saveScheduleButton;
        public Button closeButton;

        [Header("Screen Manager")]
        public ScreenManager screenManager;

        private void OnEnable()
        {
            ResetToPinGate();
        }

        private void Start()
        {
            if (verifyPinButton != null) verifyPinButton.onClick.AddListener(OnVerifyPinClicked);
            if (saveScheduleButton != null) saveScheduleButton.onClick.AddListener(OnSaveScheduleClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void ResetToPinGate()
        {
            if (pinGatePanel != null) pinGatePanel.SetActive(true);
            if (scheduleEditorPanel != null) scheduleEditorPanel.SetActive(false);
            if (pinInputField != null) pinInputField.text = "";
            if (pinErrorMessage != null) pinErrorMessage.text = "";
        }

        public void OnVerifyPinClicked()
        {
            string enteredPin = pinInputField != null ? pinInputField.text.Trim() : "";

            if (ParentSecurityManager.Instance != null && ParentSecurityManager.Instance.VerifyPin(enteredPin))
            {
                // PIN Benar! Buka panel form jadwal orang tua
                if (pinGatePanel != null) pinGatePanel.SetActive(false);
                if (scheduleEditorPanel != null) scheduleEditorPanel.SetActive(true);
                if (pinErrorMessage != null) pinErrorMessage.text = "";
            }
            else
            {
                // PIN Salah
                if (pinErrorMessage != null)
                {
                    bool isIndo = BilingualManager.Instance == null || BilingualManager.Instance.IsIndonesian();
                    pinErrorMessage.text = isIndo ? "❌ PIN Salah! (Default Demo: 1234)" : "❌ Wrong PIN! (Default Demo: 1234)";
                }
            }
        }

        public void OnSaveScheduleClicked()
        {
            SubjectType selectedSubject = (SubjectType)(subjectDropdown != null ? subjectDropdown.value : 0);
            string teacher = teacherNameField != null && !string.IsNullOrEmpty(teacherNameField.text) ? teacherNameField.text : "Guru Pengajar";
            string day = dayDropdown != null ? dayDropdown.options[dayDropdown.value].text : "Senin";
            string start = startTimeField != null && !string.IsNullOrEmpty(startTimeField.text) ? startTimeField.text : "08:00";
            string end = endTimeField != null && !string.IsNullOrEmpty(endTimeField.text) ? endTimeField.text : "09:30";

            string titleId = selectedSubject == SubjectType.Science ? "Sains - Materi Baru" : (selectedSubject == SubjectType.Language ? "Bahasa - Kosakata" : "Matematika - Logika");
            string titleEn = selectedSubject == SubjectType.Science ? "Science - New Topic" : (selectedSubject == SubjectType.Language ? "Language - Vocabulary" : "Math - Logic");
            Color color = selectedSubject == SubjectType.Science ? new Color(0.23f, 0.51f, 0.96f) : (selectedSubject == SubjectType.Language ? new Color(0.96f, 0.62f, 0.04f) : new Color(0.55f, 0.36f, 0.96f));

            ScheduleItem newSchedule = new ScheduleItem(
                "sch_" + System.Guid.NewGuid().ToString().Substring(0, 5),
                selectedSubject, titleId, titleEn, teacher, day, start, end, "Ruang Kelas", color
            );

            if (AppManager.Instance != null)
            {
                AppManager.Instance.AddSchedule(newSchedule);
            }

            OnCloseClicked();
        }

        public void OnCloseClicked()
        {
            if (screenManager != null)
            {
                screenManager.CloseParentScheduleModal();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
