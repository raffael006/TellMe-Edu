using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;
using UnityEngine.UI;

namespace TellMe.Modules
{
    /// <summary>
    /// Layar 0: Pemilihan Mode Pengguna (Mode Orang Tua vs Mode Siswa).
    /// Mengadopsi dua kartu besar vertikal bertekstur frosted glass (seperti referensi ToyCAD).
    /// </summary>
    public class RoleSelectionController : MonoBehaviour
    {
        [Header("Kartu Mode Pengguna (Frosted Glass Cards)")]
        [SerializeField] private Button parentModeCardButton;
        [SerializeField] private Button studentModeCardButton;

        [Header("Modal PIN Mode Orang Tua (Opsional)")]
        [SerializeField] private GameObject pinModalPanel;
        [SerializeField] private InputField pinInputField;
        [SerializeField] private Text pinErrorText;

        public void SetupComponents(
            Button parentBtn, Button studentBtn, GameObject pinModal, InputField pinInput, Text pinErr)
        {
            parentModeCardButton = parentBtn;
            studentModeCardButton = studentBtn;
            pinModalPanel = pinModal;
            pinInputField = pinInput;
            pinErrorText = pinErr;

            if (parentModeCardButton != null)
            {
                parentModeCardButton.onClick.RemoveAllListeners();
                parentModeCardButton.onClick.AddListener(OnParentCardClicked);
            }

            if (studentModeCardButton != null)
            {
                studentModeCardButton.onClick.RemoveAllListeners();
                studentModeCardButton.onClick.AddListener(OnStudentCardClicked);
            }
        }

        private void Awake()
        {
            if (parentModeCardButton != null)
            {
                parentModeCardButton.onClick.AddListener(OnParentCardClicked);
            }

            if (studentModeCardButton != null)
            {
                studentModeCardButton.onClick.AddListener(OnStudentCardClicked);
            }
        }

        public void OnParentCardClicked()
        {
            ThemeTokens.TriggerHapticFeedback();
            if (pinModalPanel != null)
            {
                pinModalPanel.SetActive(true);
                if (pinErrorText != null) pinErrorText.text = "";
            }
            else
            {
                EnterParentMode();
            }
        }

        public void VerifyParentPinAndEnter()
        {
            string expectedPin = LocalDataManager.Instance != null
                ? LocalDataManager.Instance.Data.pinOrangTua
                : "1234";

            if (pinInputField == null || string.IsNullOrEmpty(pinInputField.text) || pinInputField.text == expectedPin)
            {
                if (pinModalPanel != null) pinModalPanel.SetActive(false);
                EnterParentMode();
            }
            else
            {
                ThemeTokens.TriggerHapticFeedback();
                if (pinErrorText != null)
                {
                    pinErrorText.text = "PIN belum sesuai (Bawaan: 1234)";
                }
            }
        }

        public void EnterParentMode()
        {
            AppNavigationManager.Instance?.NavigateTo(TellMeScreen.Module1_JadwalOrangTua);
        }

        public void OnStudentCardClicked()
        {
            ThemeTokens.TriggerHapticFeedback();
            BerryBearsController.Instance?.SetMascotState(
                BerryBearsState.STATE_ON_TAP,
                "Hore! Ayo masuk ke kelas bersama BerryBears!");

            AppNavigationManager.Instance?.NavigateTo(TellMeScreen.Module5_BerandaBerryBears);
        }
    }
}
