using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.Models;

namespace KawanDengarEdu.UI
{
    public class RecordingHistoryUI : MonoBehaviour
    {
        [Header("Filter Buttons")]
        public Button btnFilterAll;
        public Button btnFilterScience;
        public Button btnFilterLanguage;
        public Button btnFilterMath;

        [Header("Rename Modal Dialog")]
        public GameObject renameModalPanel;
        public TMP_InputField titleInputField;
        public Button saveRenameButton;
        public Button cancelRenameButton;

        [Header("Navigation")]
        public ScreenManager screenManager;

        private SubjectType? currentFilter = null; // null = All
        private string activeRecordingIdToRename = "";

        private void Start()
        {
            if (btnFilterAll != null) btnFilterAll.onClick.AddListener(() => SetFilter(null));
            if (btnFilterScience != null) btnFilterScience.onClick.AddListener(() => SetFilter(SubjectType.Science));
            if (btnFilterLanguage != null) btnFilterLanguage.onClick.AddListener(() => SetFilter(SubjectType.Language));
            if (btnFilterMath != null) btnFilterMath.onClick.AddListener(() => SetFilter(SubjectType.Math));

            if (saveRenameButton != null) saveRenameButton.onClick.AddListener(SaveRename);
            if (cancelRenameButton != null) cancelRenameButton.onClick.AddListener(CloseRenameModal);
        }

        public void SetFilter(SubjectType? filter)
        {
            currentFilter = filter;
            // Di Unity implementasi nyata: refresh list GameObject Prefabs
        }

        public void OpenRenameModal(string recordingId, string currentTitle)
        {
            activeRecordingIdToRename = recordingId;
            if (titleInputField != null) titleInputField.text = currentTitle;
            if (renameModalPanel != null) renameModalPanel.SetActive(true);
        }

        public void SaveRename()
        {
            if (!string.IsNullOrEmpty(activeRecordingIdToRename) && titleInputField != null)
            {
                string newTitle = titleInputField.text.Trim();
                if (!string.IsNullOrEmpty(newTitle) && AppManager.Instance != null)
                {
                    AppManager.Instance.RenameRecording(activeRecordingIdToRename, newTitle);
                }
            }
            CloseRenameModal();
        }

        public void CloseRenameModal()
        {
            if (renameModalPanel != null) renameModalPanel.SetActive(false);
            activeRecordingIdToRename = "";
        }
    }
}
