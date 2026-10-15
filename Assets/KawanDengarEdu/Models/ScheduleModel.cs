using System;
using UnityEngine;

namespace KawanDengarEdu.Models
{
    public enum SubjectType
    {
        Science,    // Sains / IPA
        Language,   // Bahasa Indonesia / English
        Math        // Matematika
    }

    [Serializable]
    public class ScheduleItem
    {
        public string id;
        public SubjectType subject;
        public string subjectTitleId;
        public string subjectTitleEn;
        public string teacherName;
        public string dayOfWeek;
        public string startTime;
        public string endTime;
        public string room;
        public Color themeColor;

        public ScheduleItem(string id, SubjectType subject, string titleId, string titleEn, string teacher, string day, string start, string end, string room, Color color)
        {
            this.id = id;
            this.subject = subject;
            this.subjectTitleId = titleId;
            this.subjectTitleEn = titleEn;
            this.teacherName = teacher;
            this.dayOfWeek = day;
            this.startTime = start;
            this.endTime = end;
            this.room = room;
            this.themeColor = color;
        }

        public string GetLocalizedTitle(bool isIndonesian)
        {
            return isIndonesian ? subjectTitleId : subjectTitleEn;
        }
    }
}
