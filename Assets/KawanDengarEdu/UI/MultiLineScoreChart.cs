using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KawanDengarEdu.Core;
using KawanDengarEdu.Models;

namespace KawanDengarEdu.UI
{
    public enum ChartFilter
    {
        All,
        Science,
        Language,
        Math
    }

    /// <summary>
    /// Menampilkan grafik garis nilai multi-warna per mata pelajaran (Sains = Biru, Bahasa = Kuning, Math = Ungu)
    /// Mendukung filter tombol per mapel dan penggambaran titik garis di canvas UI.
    /// </summary>
    public class MultiLineScoreChart : MonoBehaviour
    {
        [Header("Chart Container")]
        public RectTransform chartArea;

        [Header("Filter Buttons")]
        public Button btnFilterAll;
        public Button btnFilterScience;
        public Button btnFilterLanguage;
        public Button btnFilterMath;

        [Header("Score Stats Text")]
        public TextMeshProUGUI avgScoreText;
        public TextMeshProUGUI highSubjectText;

        [Header("Line Colors")]
        public Color scienceColor = new Color(0.23f, 0.51f, 0.96f);  // Biru
        public Color languageColor = new Color(0.96f, 0.62f, 0.04f); // Kuning
        public Color mathColor = new Color(0.55f, 0.36f, 0.96f);     // Ungu

        private ChartFilter currentFilter = ChartFilter.All;
        private List<GameObject> spawnedChartElements = new List<GameObject>();

        private void Start()
        {
            if (btnFilterAll != null) btnFilterAll.onClick.AddListener(() => SetFilter(ChartFilter.All));
            if (btnFilterScience != null) btnFilterScience.onClick.AddListener(() => SetFilter(ChartFilter.Science));
            if (btnFilterLanguage != null) btnFilterLanguage.onClick.AddListener(() => SetFilter(ChartFilter.Language));
            if (btnFilterMath != null) btnFilterMath.onClick.AddListener(() => SetFilter(ChartFilter.Math));

            RefreshChart();
        }

        public void SetFilter(ChartFilter filter)
        {
            currentFilter = filter;
            RefreshChart();
        }

        public void RefreshChart()
        {
            // Bersihkan elemen garis sebelumnya
            foreach (var el in spawnedChartElements)
            {
                if (el != null) Destroy(el);
            }
            spawnedChartElements.Clear();

            var app = AppManager.Instance;
            if (app == null || chartArea == null) return;

            float width = chartArea.rect.width;
            float height = chartArea.rect.height;

            if (currentFilter == ChartFilter.All || currentFilter == ChartFilter.Science)
            {
                DrawLineSeries(app.scienceScores, scienceColor, width, height, "Sains");
            }
            if (currentFilter == ChartFilter.All || currentFilter == ChartFilter.Language)
            {
                DrawLineSeries(app.languageScores, languageColor, width, height, "Bahasa");
            }
            if (currentFilter == ChartFilter.All || currentFilter == ChartFilter.Math)
            {
                DrawLineSeries(app.mathScores, mathColor, width, height, "Matematika");
            }

            UpdateStatsUI();
        }

        private void DrawLineSeries(List<int> scores, Color color, float areaWidth, float areaHeight, string seriesName)
        {
            if (scores == null || scores.Count < 2) return;

            int count = scores.Count;
            float stepX = areaWidth / (count - 1);

            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                float x = -areaWidth * 0.5f + (i * stepX);
                // Normalisasi nilai 60 s/d 100 ke tinggi chart
                float normY = Mathf.InverseLerp(60f, 100f, scores[i]);
                float y = -areaHeight * 0.5f + (normY * areaHeight);
                points.Add(new Vector2(x, y));

                // Gambar dot / titik poin
                CreatePointMarker(points[i], color);
            }

            // Gambar garis penghubung antar titik
            for (int i = 0; i < points.Count - 1; i++)
            {
                CreateLineSegment(points[i], points[i + 1], color);
            }
        }

        private void CreatePointMarker(Vector2 pos, Color color)
        {
            GameObject dot = new GameObject("DotPoint", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(chartArea, false);
            RectTransform rt = dot.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(12f, 12f);

            Image img = dot.GetComponent<Image>();
            img.color = color;

            spawnedChartElements.Add(dot);
        }

        private void CreateLineSegment(Vector2 start, Vector2 end, Color color)
        {
            GameObject line = new GameObject("LineSeg", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(chartArea, false);
            RectTransform rt = line.GetComponent<RectTransform>();

            Vector2 dir = (end - start).normalized;
            float distance = Vector2.Distance(start, end);

            rt.sizeDelta = new Vector2(distance, 4f); // ketebalan 4px
            rt.anchoredPosition = start + dir * distance * 0.5f;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rt.localRotation = Quaternion.Euler(0, 0, angle);

            Image img = line.GetComponent<Image>();
            img.color = color;

            spawnedChartElements.Add(line);
        }

        private void UpdateStatsUI()
        {
            if (avgScoreText != null)
            {
                avgScoreText.text = "Rata-rata Kelas: 92 Poin (Sangat Baik)";
            }
            if (highSubjectText != null)
            {
                highSubjectText.text = "Mapel Tertinggi: Sains (95)";
            }
        }
    }
}
