using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.Models;

namespace KawanDengarEdu.UI
{
    public class FillWordQuizController : MonoBehaviour
    {
        [Header("UI Prompts")]
        public TextMeshProUGUI subjectBadgeText;
        public TextMeshProUGUI questionPromptText;
        public TextMeshProUGUI hintText;
        public TextMeshProUGUI scoreText;

        [Header("Word Tile Slots")]
        public Transform wordSlotsContainer;
        public GameObject wordTilePrefab; // Prefab tile huruf

        [Header("Keyboard Choices")]
        public Transform keyboardContainer;
        public GameObject letterButtonPrefab; // Prefab tombol huruf

        [Header("Visual Feedback & Confetti")]
        public ParticleSystem confettiParticle;
        public GameObject correctNotificationBanner;

        [Header("Screen Manager")]
        public ScreenManager screenManager;

        private QuizQuestion currentQuestion;
        private List<TextMeshProUGUI> spawnedSlotTexts = new List<TextMeshProUGUI>();
        private int missingIndex = 0;
        private char expectedChar = 'E';
        private bool isAnswered = false;

        private void OnEnable()
        {
            SetupQuiz();
        }

        public void SetupQuiz()
        {
            isAnswered = false;
            if (correctNotificationBanner != null) correctNotificationBanner.SetActive(false);

            var app = AppManager.Instance;
            if (app == null) return;

            bool isIndo = BilingualManager.Instance == null || BilingualManager.Instance.IsIndonesian();
            currentQuestion = app.GetQuizForCurrentSubject();

            string targetWord = currentQuestion.GetTargetWord(isIndo);
            missingIndex = currentQuestion.GetMissingIndex(isIndo);
            expectedChar = char.ToUpper(targetWord[missingIndex]);

            // Update UI Teks
            if (subjectBadgeText != null)
                subjectBadgeText.text = $"Kuis {currentQuestion.subject}";

            if (questionPromptText != null)
                questionPromptText.text = currentQuestion.GetPrompt(isIndo);

            if (hintText != null)
                hintText.text = currentQuestion.GetHint(isIndo);

            if (scoreText != null)
                scoreText.text = "⭐ 100 Poin";

            BuildWordSlots(targetWord, missingIndex);
            BuildKeyboard(currentQuestion.GetChoices(isIndo));
        }

        private void BuildWordSlots(string targetWord, int missIdx)
        {
            // Bersihkan slot lama
            foreach (Transform child in wordSlotsContainer)
            {
                Destroy(child.gameObject);
            }
            spawnedSlotTexts.Clear();

            for (int i = 0; i < targetWord.Length; i++)
            {
                GameObject slot = wordTilePrefab != null
                    ? Instantiate(wordTilePrefab, wordSlotsContainer)
                    : CreateFallbackSlot(wordSlotsContainer);

                var txt = slot.GetComponentInChildren<TextMeshProUGUI>();
                if (i == missIdx)
                {
                    txt.text = "_";
                    txt.color = new Color(0.96f, 0.62f, 0.04f); // Amber kedip
                }
                else
                {
                    txt.text = targetWord[i].ToString();
                    txt.color = new Color(0.06f, 0.09f, 0.16f);
                }
                spawnedSlotTexts.Add(txt);
            }
        }

        private void BuildKeyboard(List<string> choices)
        {
            foreach (Transform child in keyboardContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var ch in choices)
            {
                GameObject btnObj = letterButtonPrefab != null
                    ? Instantiate(letterButtonPrefab, keyboardContainer)
                    : CreateFallbackButton(keyboardContainer, ch);

                var btn = btnObj.GetComponent<Button>();
                var txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (txt != null) txt.text = ch;

                string capturedChar = ch;
                btn.onClick.AddListener(() => OnLetterSelected(capturedChar));
            }
        }

        public void OnLetterSelected(string letter)
        {
            if (isAnswered) return;

            char selectedChar = char.ToUpper(letter[0]);

            if (selectedChar == expectedChar)
            {
                // Benar!
                isAnswered = true;
                if (missingIndex < spawnedSlotTexts.Count)
                {
                    spawnedSlotTexts[missingIndex].text = selectedChar.ToString();
                    spawnedSlotTexts[missingIndex].color = new Color(0.06f, 0.73f, 0.51f); // Hijau sukses
                }

                if (confettiParticle != null) confettiParticle.Play();
                if (correctNotificationBanner != null) correctNotificationBanner.SetActive(true);

                // Tambahkan poin bintang di AppManager
                if (AppManager.Instance != null)
                {
                    AppManager.Instance.starPoints += 50;
                }

                // Transisi ke Layar Hadiah Kartu 3D (Layar 07)
                Invoke(nameof(GoToRewardCardScreen), 1.5f);
            }
            else
            {
                // Salah - getar atau animasi feedback
                if (hintText != null)
                {
                    hintText.text = "❌ Belum tepat! " + currentQuestion.GetHint(BilingualManager.Instance == null || BilingualManager.Instance.IsIndonesian());
                }
            }
        }

        private void GoToRewardCardScreen()
        {
            if (screenManager != null)
            {
                screenManager.ShowRewardCardScreen();
            }
        }

        private GameObject CreateFallbackSlot(Transform parent)
        {
            GameObject slot = new GameObject("SlotTile", typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(parent, false);
            slot.GetComponent<RectTransform>().sizeDelta = new Vector2(56, 56);
            slot.GetComponent<Image>().color = new Color(0.9f, 0.95f, 1f);

            GameObject txtObj = new GameObject("LetterText", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(slot.transform, false);
            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 28;
            return slot;
        }

        private GameObject CreateFallbackButton(Transform parent, string letter)
        {
            GameObject btn = new GameObject("BtnLetter_" + letter, typeof(RectTransform), typeof(Image), typeof(Button));
            btn.transform.SetParent(parent, false);
            btn.GetComponent<RectTransform>().sizeDelta = new Vector2(48, 48);
            btn.GetComponent<Image>().color = Color.white;

            GameObject txtObj = new GameObject("BtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(btn.transform, false);
            var tmp = txtObj.GetComponent<TextMeshProUGUI>();
            tmp.text = letter;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 22;
            return btn;
        }
    }
}
