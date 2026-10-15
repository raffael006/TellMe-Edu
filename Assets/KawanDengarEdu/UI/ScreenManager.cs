using UnityEngine;

namespace KawanDengarEdu.UI
{
    public enum ScreenType
    {
        Register,
        Dashboard,
        ParentScheduleModal,
        RecordingHistory,
        LiveSTT,
        AISummary,
        QuizChoiceModal,
        FillWordQuiz,
        RewardCard,
        ARStudio,
        PlantingGame
    }

    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        [Header("Screen Canvas Panels")]
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

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Default: Tampilkan Dashboard jika sudah punya akun, atau Register jika pertama kali
            ShowDashboardScreen();
        }

        public void HideAllScreens()
        {
            if (panelRegister != null) panelRegister.SetActive(false);
            if (panelDashboard != null) panelDashboard.SetActive(false);
            if (panelParentModal != null) panelParentModal.SetActive(false);
            if (panelHistory != null) panelHistory.SetActive(false);
            if (panelLiveSTT != null) panelLiveSTT.SetActive(false);
            if (panelAISummary != null) panelAISummary.SetActive(false);
            if (panelQuizChoice != null) panelQuizChoice.SetActive(false);
            if (panelFillQuiz != null) panelFillQuiz.SetActive(false);
            if (panelRewardCard != null) panelRewardCard.SetActive(false);
            if (panelARStudio != null) panelARStudio.SetActive(false);
            if (panelPlantingGame != null) panelPlantingGame.SetActive(false);
        }

        public void ShowRegisterScreen()
        {
            HideAllScreens();
            if (panelRegister != null) panelRegister.SetActive(true);
        }

        public void ShowDashboardScreen()
        {
            HideAllScreens();
            if (panelDashboard != null) panelDashboard.SetActive(true);
        }

        public void OpenParentScheduleModal()
        {
            if (panelParentModal != null) panelParentModal.SetActive(true);
        }

        public void CloseParentScheduleModal()
        {
            if (panelParentModal != null) panelParentModal.SetActive(false);
        }

        public void ShowHistoryScreen()
        {
            HideAllScreens();
            if (panelHistory != null) panelHistory.SetActive(true);
        }

        public void ShowLiveSTTScreen()
        {
            HideAllScreens();
            if (panelLiveSTT != null) panelLiveSTT.SetActive(true);
        }

        public void ShowAISummaryScreen()
        {
            HideAllScreens();
            if (panelAISummary != null) panelAISummary.SetActive(true);
        }

        public void OpenQuizChoiceModal()
        {
            if (panelQuizChoice != null) panelQuizChoice.SetActive(true);
        }

        public void CloseQuizChoiceModal()
        {
            if (panelQuizChoice != null) panelQuizChoice.SetActive(false);
        }

        public void ShowFillQuizScreen()
        {
            HideAllScreens();
            if (panelFillQuiz != null) panelFillQuiz.SetActive(true);
        }

        public void ShowRewardCardScreen()
        {
            HideAllScreens();
            if (panelRewardCard != null) panelRewardCard.SetActive(true);
        }

        public void ShowARStudioScreen()
        {
            HideAllScreens();
            if (panelARStudio != null) panelARStudio.SetActive(true);
        }

        public void ShowPlantingGameScreen()
        {
            HideAllScreens();
            if (panelPlantingGame != null) panelPlantingGame.SetActive(true);
        }
    }
}
