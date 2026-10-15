using System;
using System.Collections.Generic;
using UnityEngine;
using KawanDengarEdu.Models;

namespace KawanDengarEdu.Core
{
    public class AppManager : MonoBehaviour
    {
        public static AppManager Instance { get; private set; }

        [Header("Student Profile")]
        public string studentName = "Budi Wicaksono";
        public string studentClass = "Kelas 3 SD";
        public string schoolName = "SD Inklusi Harapan Kasih";
        public int starPoints = 350;

        [Header("Current Subject State")]
        public SubjectType currentSubject = SubjectType.Science;
        public string currentTeacher = "Ibu Guru Ratna";

        [Header("Schedules")]
        public List<ScheduleItem> schedules = new List<ScheduleItem>();

        [Header("Recordings")]
        public List<RecordingItem> recordings = new List<RecordingItem>();

        [Header("Quiz Score Data (Multi-Line Chart)")]
        // Scores for weeks 1..4
        public List<int> scienceScores = new List<int> { 80, 88, 92, 95 };
        public List<int> languageScores = new List<int> { 75, 82, 85, 88 };
        public List<int> mathScores = new List<int> { 85, 90, 89, 92 };

        [Header("Quiz Bank")]
        public List<QuizQuestion> quizQuestions = new List<QuizQuestion>();

        public event Action OnDataUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeDefaultData()
        {
            // Schedules matching the Figma prototype
            schedules.Clear();
            schedules.Add(new ScheduleItem("sch_1", SubjectType.Science, "Sains (IPA) - Mengenal Buah Sehat", "Science - Healthy Fruits", "Ibu Ratna", "Senin", "09:00", "10:30", "Ruang Inklusi A", new Color(0.23f, 0.51f, 0.96f)));
            schedules.Add(new ScheduleItem("sch_2", SubjectType.Language, "Bahasa Indonesia - Kosakata Baru", "Language - New Vocabulary", "Pak Doni", "Selasa", "08:00", "09:30", "Ruang Inklusi B", new Color(0.96f, 0.62f, 0.04f)));
            schedules.Add(new ScheduleItem("sch_3", SubjectType.Math, "Matematika - Bangun Ruang Ceria", "Math - Geometric Shapes", "Ibu Maya", "Rabu", "10:00", "11:30", "Lab Sains C", new Color(0.55f, 0.36f, 0.96f)));

            // Recordings matching the Figma prototype
            recordings.Clear();
            recordings.Add(new RecordingItem("rec_1", SubjectType.Science, "Apel dan Kandungan Vitamin C", "Ibu Ratna", "14 Okt 2026", 185, "Apel kaya vitamin C, kulit merah, hijau...", "Anak-anak hebat, hari ini kita belajar tentang Buah Apel. Apel memiliki banyak Vitamin C yang membuat tubuh kita kuat dan tidak mudah sakit. Kulit apel ada yang berwarna Merah, Hijau, dan Kuning..."));
            recordings.Add(new RecordingItem("rec_2", SubjectType.Language, "Belajar Mengenal Kata Benda", "Pak Doni", "13 Okt 2026", 240, "Mengenal buku, pensil, dan meja...", "Hari ini kita membaca cerita bergambar tentang petualangan membaca buku bersama teman-teman..."));
            recordings.Add(new RecordingItem("rec_3", SubjectType.Math, "Mengenal Bentuk Bola & Kubus", "Ibu Maya", "12 Okt 2026", 150, "Bentuk bulat bola dan sisi kubus...", "Perhatikan benda di sekitar kita, bola berbentuk lingkaran bulat tiga dimensi..."));

            // Contextual Quizzes matching the subjects
            quizQuestions.Clear();
            // Science Quiz: APEL / APPLE
            quizQuestions.Add(new QuizQuestion(
                "q_sci", SubjectType.Science,
                "APEL", "APPLE",
                2, 4,
                "Lengkapi nama buah merah manis kaya Vitamin C:",
                "Complete the name of this sweet red fruit rich in Vitamin C:",
                "💡 Huruf vokal setelah P", "💡 Vowel letter after P",
                new List<string> { "E", "O", "I", "U", "K", "M" },
                new List<string> { "E", "O", "A", "U", "P", "L" }
            ));
            // Language Quiz: BUKU / BOOK
            quizQuestions.Add(new QuizQuestion(
                "q_lang", SubjectType.Language,
                "BUKU", "BOOK",
                1, 2,
                "Benda berpustaka jendela ilmu dunia:",
                "An object full of knowledge and pages to read:",
                "💡 Huruf vokal bulat seperti donat", "💡 Round vowel like a donut",
                new List<string> { "U", "A", "I", "E", "O", "S" },
                new List<string> { "O", "A", "E", "U", "K", "T" }
            ));
            // Math Quiz: BOLA / CUBE
            quizQuestions.Add(new QuizQuestion(
                "q_math", SubjectType.Math,
                "BOLA", "CUBE",
                1, 1,
                "Bangun ruang bundar yang bisa memantul:",
                "A 3D geometric shape with six square faces:",
                "💡 Huruf vokal setelah B", "💡 Vowel letter after C",
                new List<string> { "O", "A", "I", "E", "U", "R" },
                new List<string> { "U", "A", "O", "I", "E", "B" }
            ));
        }

        public QuizQuestion GetQuizForCurrentSubject()
        {
            var found = quizQuestions.Find(q => q.subject == currentSubject);
            return found ?? quizQuestions[0];
        }

        public void AddSchedule(ScheduleItem item)
        {
            schedules.Add(item);
            OnDataUpdated?.Invoke();
        }

        public void RenameRecording(string recordingId, string newTitle)
        {
            var item = recordings.Find(r => r.id == recordingId);
            if (item != null)
            {
                item.customTitle = newTitle;
                OnDataUpdated?.Invoke();
            }
        }

        public void AddNewRecording(RecordingItem item)
        {
            recordings.Insert(0, item);
            OnDataUpdated?.Invoke();
        }
    }
}
