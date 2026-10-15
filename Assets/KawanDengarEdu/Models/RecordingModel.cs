using System;
using UnityEngine;

namespace KawanDengarEdu.Models
{
    [Serializable]
    public class RecordingItem
    {
        public string id;
        public SubjectType subject;
        public string customTitle;
        public string teacherName;
        public string dateString;
        public int durationSeconds;
        public string transcriptSnippet;
        public string fullTranscript;
        public bool hasAISummary;
        public bool isQuizCompleted;

        public RecordingItem(string id, SubjectType subject, string title, string teacher, string date, int duration, string snippet, string fullTranscript)
        {
            this.id = id;
            this.subject = subject;
            this.customTitle = title;
            this.teacherName = teacher;
            this.dateString = date;
            this.durationSeconds = duration;
            this.transcriptSnippet = snippet;
            this.fullTranscript = fullTranscript;
            this.hasAISummary = true;
            this.isQuizCompleted = false;
        }

        public string GetFormattedDuration()
        {
            int minutes = durationSeconds / 60;
            int seconds = durationSeconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
