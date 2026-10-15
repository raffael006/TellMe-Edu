using System;
using UnityEngine;

namespace KawanDengarEdu.Core
{
    public class ParentSecurityManager : MonoBehaviour
    {
        public static ParentSecurityManager Instance { get; private set; }

        private const string PIN_KEY = "PARENT_SECURITY_PIN";
        public const string DEFAULT_PIN = "1234";

        public event Action OnPinVerifiedSuccess;
        public event Action OnPinVerifiedFailed;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public string GetSavedPin()
        {
            return PlayerPrefs.GetString(PIN_KEY, DEFAULT_PIN);
        }

        public bool VerifyPin(string inputPin)
        {
            string savedPin = GetSavedPin();
            bool isCorrect = (inputPin == savedPin);

            if (isCorrect)
            {
                OnPinVerifiedSuccess?.Invoke();
            }
            else
            {
                OnPinVerifiedFailed?.Invoke();
            }

            return isCorrect;
        }

        public void SetNewPin(string newPin)
        {
            if (newPin.Length == 4)
            {
                PlayerPrefs.SetString(PIN_KEY, newPin);
                PlayerPrefs.Save();
            }
        }

        public void ResetToDefault()
        {
            PlayerPrefs.SetString(PIN_KEY, DEFAULT_PIN);
            PlayerPrefs.Save();
        }
    }
}
