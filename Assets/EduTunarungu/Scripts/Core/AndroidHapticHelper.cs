using System;
using UnityEngine;

namespace EduTunarungu.Core
{
    /// <summary>
    /// Pengendali getaran (Haptic/Vibration) untuk Android Native dan fallback Unity.
    /// Sangat krusial untuk anak disabilitas pendengaran sebagai pengganti efek suara.
    /// </summary>
    public static class AndroidHapticHelper
    {
        public static bool IsHapticEnabled = true;
        public static float IntensityMultiplier = 1.0f; // 0.5 = lembut, 1.0 = normal, 1.5 = kuat

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaObject vibrator;
        private static AndroidJavaClass vibrationEffectClass;
        private static bool isInitialized;
        private static int apiLevel = 0;

        private static void InitAndroid()
        {
            if (isInitialized) return;
            try
            {
                using (var versionClass = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    apiLevel = versionClass.GetStatic<int>("SDK_INT");
                }

                using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                }

                if (apiLevel >= 26)
                {
                    vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                }
                isInitialized = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[AndroidHapticHelper] Gagal inisialisasi Android Vibrator: " + e.Message);
            }
        }
#endif

        /// <summary>
        /// Getaran sentuhan ringan (untuk klik tombol UI) - 20ms
        /// </summary>
        public static void LightTap()
        {
            if (!IsHapticEnabled) return;
            Vibrate(30, 80);
        }

        /// <summary>
        /// Getaran sedang (untuk interaksi suku kata atau pilihan) - 60ms
        /// </summary>
        public static void MediumPulse()
        {
            if (!IsHapticEnabled) return;
            Vibrate(60, 160);
        }

        /// <summary>
        /// Getaran sukses / jawaban benar (pola ganda ceria: getar-jeda-getar)
        /// </summary>
        public static void SuccessPattern()
        {
            if (!IsHapticEnabled) return;
            long[] timings = { 0, 70, 60, 140 }; // jeda, getar 1, jeda, getar 2
            int[] amplitudes = { 0, 150, 0, 255 };
            VibratePattern(timings, amplitudes);
        }

        /// <summary>
        /// Getaran salah / coba lagi (getar panjang lembut agar tidak mengagetkan anak)
        /// </summary>
        public static void RetryPattern()
        {
            if (!IsHapticEnabled) return;
            long[] timings = { 0, 180 };
            int[] amplitudes = { 0, 120 };
            VibratePattern(timings, amplitudes);
        }

        /// <summary>
        /// Getaran ritme khusus (untuk modul drum/taktil edukasi irama)
        /// </summary>
        public static void CustomRhythm(long[] timings, int[] amplitudes)
        {
            if (!IsHapticEnabled) return;
            VibratePattern(timings, amplitudes);
        }

        private static void Vibrate(long milliseconds, int amplitude)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            InitAndroid();
            if (vibrator != null)
            {
                try
                {
                    if (apiLevel >= 26 && vibrationEffectClass != null)
                    {
                        int adjustedAmp = Mathf.Clamp((int)(amplitude * IntensityMultiplier), 1, 255);
                        var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, adjustedAmp);
                        vibrator.Call("vibrate", effect);
                        return;
                    }
                    else
                    {
                        vibrator.Call("vibrate", milliseconds);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[AndroidHapticHelper] Error vibrate: " + ex.Message);
                }
            }
            Handheld.Vibrate();
#else
            // Di Unity Editor (Windows/Mac) kita gunakan log konsol untuk simulasi visual getaran
            // Debug.Log($"[Haptic Sim] Vibrate: {milliseconds}ms (Amp: {amplitude})");
#endif
        }

        private static void VibratePattern(long[] timings, int[] amplitudes)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            InitAndroid();
            if (vibrator != null)
            {
                try
                {
                    if (apiLevel >= 26 && vibrationEffectClass != null && amplitudes != null && amplitudes.Length == timings.Length)
                    {
                        int[] adjustedAmps = new int[amplitudes.Length];
                        for (int i = 0; i < amplitudes.Length; i++)
                        {
                            adjustedAmps[i] = Mathf.Clamp((int)(amplitudes[i] * IntensityMultiplier), 0, 255);
                        }
                        var effect = vibrationEffectClass.CallStatic<AndroidJavaObject>("createWaveform", timings, adjustedAmps, -1);
                        vibrator.Call("vibrate", effect);
                        return;
                    }
                    else
                    {
                        vibrator.Call("vibrate", timings, -1);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[AndroidHapticHelper] Error waveform: " + ex.Message);
                }
            }
            Handheld.Vibrate();
#else
            // Debug.Log($"[Haptic Sim Pattern] {timings.Length} langkah");
#endif
        }
    }
}
