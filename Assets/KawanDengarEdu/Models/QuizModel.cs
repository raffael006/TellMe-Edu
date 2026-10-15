using System;
using System.Collections.Generic;
using UnityEngine;

namespace KawanDengarEdu.Models
{
    [Serializable]
    public class QuizQuestion
    {
        public string id;
        public SubjectType subject;
        public string targetWordId;         // Misal: "APEL"
        public string targetWordEn;         // Misal: "APPLE"
        public int missingIndexId;          // Indeks huruf rumpang di Indo (contoh: 2 -> 'E')
        public int missingIndexEn;          // Indeks huruf rumpang di En (contoh: 4 -> 'E')
        public string questionPromptId;     // "Lengkapi nama buah merah manis kaya Vitamin C:"
        public string questionPromptEn;     // "Complete the name of this sweet red fruit rich in Vitamin C:"
        public string hintTextId;           // "💡 Huruf vokal setelah P"
        public string hintTextEn;           // "💡 Vowel letter after P"
        public List<string> keyboardChoicesId; // [ "E", "O", "I", "U", "K", "M" ]
        public List<string> keyboardChoicesEn; // [ "E", "O", "A", "U", "P", "L" ]

        public QuizQuestion(string id, SubjectType subject, string wordId, string wordEn, int missId, int missEn, string promptId, string promptEn, string hintId, string hintEn, List<string> choicesId, List<string> choicesEn)
        {
            this.id = id;
            this.subject = subject;
            this.targetWordId = wordId;
            this.targetWordEn = wordEn;
            this.missingIndexId = missId;
            this.missingIndexEn = missEn;
            this.questionPromptId = promptId;
            this.questionPromptEn = promptEn;
            this.hintTextId = hintId;
            this.hintTextEn = hintEn;
            this.keyboardChoicesId = choicesId;
            this.keyboardChoicesEn = choicesEn;
        }

        public string GetTargetWord(bool isIndonesian) => isIndonesian ? targetWordId : targetWordEn;
        public int GetMissingIndex(bool isIndonesian) => isIndonesian ? missingIndexId : missingIndexEn;
        public string GetPrompt(bool isIndonesian) => isIndonesian ? questionPromptId : questionPromptEn;
        public string GetHint(bool isIndonesian) => isIndonesian ? hintTextId : hintTextEn;
        public List<string> GetChoices(bool isIndonesian) => isIndonesian ? keyboardChoicesId : keyboardChoicesEn;
        public char GetExpectedChar(bool isIndonesian)
        {
            string word = GetTargetWord(isIndonesian);
            int idx = GetMissingIndex(isIndonesian);
            return word[idx];
        }
    }
}
