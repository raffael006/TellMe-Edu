using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;

public class Simak : MonoBehaviour
{
    public Font font;                       // opsional: tarik font .ttf (mis. Atkinson Hyperlegible)
    const string NamaSiswa = "Nadia";
 
    // Naskah contoh. Awalan "_" menandai kata kunci. Baris berawalan "!" memicu peringatan visual + getar.
    static readonly string[][] Naskah = {
        new[]{"Bu Sari","Selamat pagi, anak-anak. Hari ini kita belajar tentang _rantai _makanan."},
        new[]{"Bu Sari","Setiap rantai makanan dimulai dari _produsen, yaitu makhluk hidup yang membuat makanannya sendiri."},
        new[]{"!","Bu Sari menyebut nama Anda"},
        new[]{"Bu Sari","Nadia, coba sebutkan satu contoh produsen di sekitar sekolah."},
        new[]{"Raka","Pohon mangga di depan kelas, Bu."},
        new[]{"Bu Sari","Benar. Belalang memakan rumput, jadi belalang disebut _konsumen tingkat satu."},
        new[]{"Bu Sari","Lalu jamur dan bakteri yang menguraikan sisa makhluk hidup disebut _pengurai."},
        new[]{"!","Bel istirahat berbunyi"},
        new[]{"Bu Sari","Catat tiga kata ini ya: produsen, konsumen, dan pengurai."}
    };
 
    static Color H(string s) { ColorUtility.TryParseHtmlString(s, out var c); return c; }
    static readonly Color Ink = H("#14202B"), Paper = H("#EEF1F4"), Blue = H("#1F5FBF"), Night = H("#101B27"),
        Bar = H("#18283A"), Key = H("#223850"), Gold = H("#FFD84D"), Red = H("#D93F45"),
        Muted = H("#5E6B78"), Line = H("#D5DCE3"), Dim = H("#8FA0B0"), Off = H("#9AA7B4");
 
    Font F; RectTransform root, capContent, noteContent; ScrollRect capScroll;
    readonly List<GameObject> tabs = new(); readonly List<Text> capTexts = new();
    readonly List<(string t, string s)> notes = new(); readonly HashSet<string> kw = new();
    Image[] navBar = new Image[4]; Text[] navTxt = new Text[4];
    Text kwText, homeCount, alertTxt; Image alertImg; GameObject alertBar;
    int capSize = 46; bool vibrate = true, contrast, paused, started; string cur = "";
 
    void Start()
    {
        F = font ? font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem(); BuildCanvas();
        BuildHome(); BuildKelas(); BuildCatatan(); BuildAtur(); BuildNav(); Show(0);
    }
 
    // ---------- dasar UI ----------
    void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>()) return;
        var e = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
        e.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
        e.AddComponent<StandaloneInputModule>();
#endif
    }
 
    void BuildCanvas()
    {
        var c = new GameObject("SimakCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        c.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var cs = c.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920); cs.matchWidthOrHeight = 0.5f;
        Stretch(Img(c.transform, Paper).rectTransform);
        root = Rt("Safe", c.transform); Stretch(root);
        var sa = Screen.safeArea; var sz = new Vector2(Screen.width, Screen.height);
        root.anchorMin = sa.position / sz; root.anchorMax = (sa.position + sa.size) / sz;
    }
 
    static RectTransform Rt(string n, Transform p)
    { var g = new GameObject(n, typeof(RectTransform)); g.transform.SetParent(p, false); return (RectTransform)g.transform; }
 
    static void Stretch(RectTransform r, float l = 0, float b = 0, float rt = 0, float t = 0)
    { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = new Vector2(l, b); r.offsetMax = new Vector2(-rt, -t); }
 
    static Sprite rr;
    static Sprite RR()   // sprite kotak membulat dibuat lewat kode (9-slice)
    {
        if (rr) return rr;
        int s = 128, r = 48; var t = new Texture2D(s, s, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
        for (int y = 0; y < s; y++) for (int x = 0; x < s; x++)
        {
            float dx = Mathf.Max(r - (x + .5f), (x + .5f) - (s - r), 0), dy = Mathf.Max(r - (y + .5f), (y + .5f) - (s - r), 0);
            t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(.5f - (Mathf.Sqrt(dx * dx + dy * dy) - r))));
        }
        t.Apply();
        return rr = Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
    }
 
    static Image Img(Transform p, Color c, float radius = 0)
    {
        var i = Rt("img", p).gameObject.AddComponent<Image>(); i.color = c;
        if (radius > 0) { i.sprite = RR(); i.type = Image.Type.Sliced; i.pixelsPerUnitMultiplier = 48f / radius; }
        return i;
    }
 
    Text Txt(Transform p, string s, int size, Color c, FontStyle st = FontStyle.Normal, TextAnchor a = TextAnchor.UpperLeft)
    {
        var t = Rt("txt", p).gameObject.AddComponent<Text>();
        t.font = F; t.text = s; t.fontSize = size; t.color = c; t.fontStyle = st; t.alignment = a;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.lineSpacing = 1.1f; t.raycastTarget = false; return t;
    }
 
    static VerticalLayoutGroup VL(RectTransform r, float gap, int pad)
    {
        var v = r.gameObject.AddComponent<VerticalLayoutGroup>(); v.spacing = gap; v.padding = new RectOffset(pad, pad, pad, pad);
        v.childControlWidth = v.childControlHeight = true; v.childForceExpandWidth = true; v.childForceExpandHeight = false; return v;
    }
    static HorizontalLayoutGroup HL(RectTransform r, float gap, int pad, bool expand = false)
    {
        var h = r.gameObject.AddComponent<HorizontalLayoutGroup>(); h.spacing = gap; h.padding = new RectOffset(pad, pad, pad, pad);
        h.childAlignment = TextAnchor.MiddleLeft; h.childControlWidth = h.childControlHeight = true;
        h.childForceExpandWidth = expand; h.childForceExpandHeight = expand; return h;
    }
    static void Size(RectTransform r, float h = -1, float w = -1, float flex = -1)
    { var e = r.gameObject.GetComponent<LayoutElement>() ?? r.gameObject.AddComponent<LayoutElement>(); e.preferredHeight = h; e.preferredWidth = w; e.flexibleWidth = flex; }
 
    Button Btn(Transform p, string label, Color bg, Color fg, Action on, int size = 40, float h = 120, float radius = 28)
    {
        var i = Img(p, bg, radius); var b = i.gameObject.AddComponent<Button>(); b.targetGraphic = i; b.onClick.AddListener(() => on());
        Stretch(Txt(i.transform, label, size, fg, FontStyle.Bold, TextAnchor.MiddleCenter).rectTransform); Size(i.rectTransform, h); return b;
    }
 
    RectTransform Scroll(Transform p, out ScrollRect sr, float gap, int pad)
    {
        var v = Rt("scroll", p); Stretch(v); Img(v, Color.clear).transform.SetParent(v, false);
        var vp = Rt("vp", v); Stretch(vp); vp.gameObject.AddComponent<RectMask2D>();
        var c = Rt("content", vp); c.anchorMin = new Vector2(0, 1); c.anchorMax = Vector2.one; c.pivot = new Vector2(.5f, 1); c.sizeDelta = Vector2.zero;
        VL(c, gap, pad); c.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr = v.gameObject.AddComponent<ScrollRect>(); sr.viewport = vp; sr.content = c; sr.horizontal = false; sr.scrollSensitivity = 40; return c;
    }
 
    RectTransform NewTab() { var r = Rt("Tab", root); Stretch(r, 0, 150, 0, 0); tabs.Add(r.gameObject); return r; }
 
    // ---------- navigasi ----------
    void BuildNav()
    {
        var n = Img(root, Color.white); var r = n.rectTransform;
        r.anchorMin = Vector2.zero; r.anchorMax = new Vector2(1, 0); r.pivot = new Vector2(.5f, 0); r.sizeDelta = new Vector2(0, 150);
        var ln = Img(r, Line).rectTransform; ln.anchorMin = new Vector2(0, 1); ln.anchorMax = Vector2.one; ln.pivot = new Vector2(.5f, 1); ln.sizeDelta = new Vector2(0, 2);
        var row = Rt("row", r); Stretch(row); var h = HL(row, 0, 0, true);
        string[] nama = { "Beranda", "Kelas", "Catatan", "Atur" };
        for (int i = 0; i < 4; i++)
        {
            int k = i; var im = Img(row, Color.clear); var b = im.gameObject.AddComponent<Button>(); b.targetGraphic = im; b.onClick.AddListener(() => Show(k));
            navTxt[i] = Txt(im.transform, nama[i], 34, Muted, FontStyle.Bold, TextAnchor.MiddleCenter); Stretch(navTxt[i].rectTransform);
            navBar[i] = Img(im.transform, Blue, 4); var br = navBar[i].rectTransform;
            br.anchorMin = new Vector2(.3f, 1); br.anchorMax = new Vector2(.7f, 1); br.pivot = new Vector2(.5f, 1); br.sizeDelta = new Vector2(0, 8);
        }
    }
 
    void Show(int k)
    {
        for (int i = 0; i < 4; i++) { tabs[i].SetActive(i == k); navTxt[i].color = i == k ? Blue : Muted; navBar[i].enabled = i == k; }
        if (k == 0) homeCount.text = notes.Count == 0 ? "Belum ada bagian yang ditandai" : notes.Count + " bagian ditandai";
        if (k == 1 && !started) { started = true; StartCoroutine(Kelas()); }
        if (k == 2) RefreshNotes();
    }
 
    // ---------- beranda ----------
    void BuildHome()
    {
        var c = NewTab(); VL(c, 28, 56);
        Txt(c, "Selamat pagi, " + NamaSiswa, 66, Ink, FontStyle.Bold);
        Txt(c, "Apa yang diucapkan guru dan teman tampil sebagai teks di layar Anda.", 38, Muted);
        var k = Img(c, Color.white, 36); VL(k.rectTransform, 16, 48);
        Txt(k.transform, "Kelas berikutnya", 32, Muted, FontStyle.Bold);
        Txt(k.transform, "IPA: Rantai Makanan", 54, Ink, FontStyle.Bold);
        Txt(k.transform, "Pengajar: Bu Sari\nPukul 09.30 di Ruang 4B", 36, Muted);
        Btn(k.transform, "Masuk kelas", Blue, Color.white, () => Show(1));
        var n = Img(c, Color.white, 36); VL(n.rectTransform, 12, 48);
        Txt(n.transform, "Catatan saya", 32, Muted, FontStyle.Bold);
        homeCount = Txt(n.transform, "", 44, Ink, FontStyle.Bold);
        Btn(n.transform, "Buka catatan", Key, Color.white, () => Show(2), 36, 108);
        Txt(c, "Prototipe. Teks kelas pada demo ini adalah contoh, bukan hasil pengenalan suara.", 30, Muted);
    }
 
    // ---------- kelas (teks langsung) ----------
    void BuildKelas()
    {
        var s = NewTab(); Stretch(Img(s, Night).rectTransform);
        var hd = Rt("hd", s); hd.anchorMin = new Vector2(0, 1); hd.anchorMax = Vector2.one; hd.pivot = new Vector2(.5f, 1); hd.sizeDelta = new Vector2(0, 190);
        VL(hd, 4, 48); Txt(hd, "IPA: Rantai Makanan", 46, Color.white, FontStyle.Bold); Txt(hd, "Menerima suara kelas", 32, Dim);
 
        var area = Rt("cap", s); Stretch(area, 0, 200, 0, 190);
        capContent = Scroll(area, out capScroll, 44, 48);
 
        alertImg = Img(s, Red, 28); alertBar = alertImg.gameObject; var ar = alertImg.rectTransform;
        ar.anchorMin = new Vector2(0, 1); ar.anchorMax = Vector2.one; ar.pivot = new Vector2(.5f, 1); ar.sizeDelta = new Vector2(-96, 130); ar.anchoredPosition = new Vector2(0, -190);
        alertTxt = Txt(alertBar.transform, "", 40, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter); Stretch(alertTxt.rectTransform); alertBar.SetActive(false);
 
        var bar = Img(s, Bar); var br = bar.rectTransform; br.anchorMin = Vector2.zero; br.anchorMax = new Vector2(1, 0); br.pivot = new Vector2(.5f, 0); br.sizeDelta = new Vector2(0, 200);
        var row = Rt("row", br); Stretch(row); HL(row, 20, 36, true);
        Btn(row, "A-", Key, Color.white, () => { capSize = Mathf.Max(32, capSize - 6); ApplyCaps(); });
        Btn(row, "A+", Key, Color.white, () => { capSize = Mathf.Min(86, capSize + 6); ApplyCaps(); });
        Button jeda = null; jeda = Btn(row, "Jeda", Key, Color.white, () => { paused = !paused; jeda.GetComponentInChildren<Text>().text = paused ? "Lanjut" : "Jeda"; });
        var tandai = Btn(row, "Tandai", Gold, Ink, () =>
        {
            if (cur.Length == 0) return;
            notes.Add((DateTime.Now.ToString("HH.mm"), cur)); Notify("Bagian ini ditandai", Blue);
        });
        Size(tandai.GetComponent<RectTransform>(), 120, -1, 1.6f);
    }
 
    Text AddLine(string who)
    {
        var blk = Rt("line", capContent); VL(blk, 8, 0);
        Txt(blk, who, 32, Dim, FontStyle.Bold);
        var t = Txt(blk, "", capSize, Color.white); capTexts.Add(t); ApplyCaps(); return t;
    }
 
    void ApplyCaps()
    {
        for (int i = 0; i < capTexts.Count; i++)
        {
            var t = capTexts[i]; t.fontSize = capSize; t.fontStyle = contrast ? FontStyle.Bold : FontStyle.Normal;
            t.color = i == capTexts.Count - 1 ? Color.white : contrast ? new Color(.88f, .9f, .93f) : H("#7F90A1");
        }
    }
 
    IEnumerator Kelas()
    {
        foreach (var l in Naskah)
        {
            while (paused) yield return null;
            if (l[0] == "!") { Notify(l[1], Red); if (vibrate) Handheld.Vibrate(); yield return new WaitForSeconds(3.8f); continue; }
            var t = AddLine(l[0]); var sb = new StringBuilder(); var plain = new StringBuilder();
            foreach (var w in l[1].Split(' '))
            {
                while (paused) yield return null;
                if (w.StartsWith("_"))
                {
                    var core = w.Substring(1).TrimEnd('.', ',', '?', '!'); kw.Add(core.ToLower());
                    sb.Append("<b><color=#FFD84D>").Append(core).Append("</color></b>").Append(w.Substring(1 + core.Length)).Append(' ');
                    plain.Append(w.Substring(1)).Append(' ');
                }
                else { sb.Append(w).Append(' '); plain.Append(w).Append(' '); }
                t.text = sb.ToString(); cur = l[0] + ": " + plain.ToString().Trim();
                Canvas.ForceUpdateCanvases(); capScroll.verticalNormalizedPosition = 0;
                yield return new WaitForSeconds(.2f);
            }
            yield return new WaitForSeconds(.9f);
        }
        Txt(capContent, "Pelajaran selesai. Bagian yang Anda tandai ada di tab Catatan.", 34, Dim);
    }
 
    int toast;
    void Notify(string m, Color c) { alertTxt.text = m; alertImg.color = c; alertBar.SetActive(true); StartCoroutine(HideAlert(++toast)); }
    IEnumerator HideAlert(int id) { yield return new WaitForSeconds(3.5f); if (id == toast) alertBar.SetActive(false); }
 
    // ---------- catatan ----------
    void BuildCatatan()
    {
        var s = NewTab();
        var top = Rt("top", s); top.anchorMin = new Vector2(0, 1); top.anchorMax = Vector2.one; top.pivot = new Vector2(.5f, 1); top.sizeDelta = new Vector2(0, 360);
        VL(top, 14, 56); Txt(top, "Catatan", 66, Ink, FontStyle.Bold); kwText = Txt(top, "", 36, Muted);
        var area = Rt("list", s); Stretch(area, 0, 0, 0, 360);
        noteContent = Scroll(area, out _, 28, 48);
    }
 
    void RefreshNotes()
    {
        foreach (Transform ch in noteContent) { ch.gameObject.SetActive(false); Destroy(ch.gameObject); }
        kwText.text = kw.Count == 0 ? "Kata kunci muncul saat kelas berjalan." : "Kata kunci hari ini: " + string.Join(", ", kw);
        if (notes.Count == 0) { Txt(noteContent, "Belum ada catatan. Ketuk Tandai di layar kelas saat guru menjelaskan hal penting.", 38, Muted); return; }
        foreach (var n in notes)
        {
            var c = Img(noteContent, Color.white, 32); VL(c.rectTransform, 10, 40);
            Txt(c.transform, "Pukul " + n.t, 30, Blue, FontStyle.Bold); Txt(c.transform, n.s, 40, Ink);
        }
    }
 
    // ---------- atur ----------
    void BuildAtur()
    {
        var c = NewTab(); VL(c, 28, 56);
        Txt(c, "Aksesibilitas", 66, Ink, FontStyle.Bold);
        Toggle(c, "Getar saat ada peringatan", "Ponsel bergetar dan banner merah muncul saat nama Anda dipanggil atau bel berbunyi.", vibrate, v => vibrate = v);
        Toggle(c, "Kontras tinggi", "Teks kelas lebih terang dan lebih tebal.", contrast, v => { contrast = v; ApplyCaps(); });
        Txt(c, "Ukuran teks kelas diatur dengan tombol A- dan A+ di layar kelas.", 34, Muted);
    }
 
    void Toggle(Transform p, string judul, string ket, bool on, Action<bool> set)
    {
        var card = Img(p, Color.white, 32); HL(card.rectTransform, 24, 40);
        var l = Rt("l", card.transform); VL(l, 6, 0); Size(l, -1, -1, 1);
        Txt(l, judul, 40, Ink, FontStyle.Bold); Txt(l, ket, 32, Muted);
        Button b = null; b = Btn(card.transform, "", Blue, Color.white, () => { on = !on; set(on); Paint(b, on); }, 36, 100, 50);
        Size(b.GetComponent<RectTransform>(), 100, 190); Paint(b, on);
    }
    static void Paint(Button b, bool on) { b.targetGraphic.color = on ? Blue : Off; b.GetComponentInChildren<Text>().text = on ? "Aktif" : "Mati"; }
}