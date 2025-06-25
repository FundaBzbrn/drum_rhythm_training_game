using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

[System.Serializable]
public class LevelData
{
    public string levelName = "Level X";
    public int sequenceLength = 10;
    public float delayBetweenBeats = 1.0f;
    public float perfectWindow = 0.15f;
    public float goodWindow = 0.3f;
    [Range(0f, 1f)]
    public float chanceForMultiHit = 0.0f;
    public bool allowMultiHits = false;
}

public class GameManager : MonoBehaviour
{
    public enum HitAccuracy { Perfect, Good, OK, Miss }
    public static GameManager Instance { get; private set; }
    public static int startLevelIndex = 0; // Ana menüden bu ayarlanabilir

    [Header("Oyun Ayarları")]
    // Bu değişkenler artık LoadLevelSettings tarafından doldurulacak,
    // Inspector'daki başlangıç değerleri sadece fallback veya ilk seviye için olabilir.
    public int sequenceLength = 10;
    public float delayBetweenBeats = 1.0f;

    [Header("Vuruş Zamanlama Pencereleri (Saniye)")]
    public float perfectWindow = 0.15f;
    public float goodWindow = 0.3f;

    [Header("Kombo Ayarları")]
    public int comboHitsForBonus = 3;
    public int comboBonusPerSuccessfulHit = 1;

    [Header("Seviye Ayarları")]
    public List<LevelData> levels = new List<LevelData>();
    [Tooltip("Oyunun başlayacağı seviyenin indeksi (0'dan başlar). MainMenuManager tarafından ayarlanabilir.")]
    public int currentLevelIndex = 0;
    private LevelData currentLevelData;

    [Header("UI Referansları")]
    public TextMeshProUGUI scoreText;
    public Image cueImage;
    public TextMeshProUGUI accuracyFeedbackText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI popupScoreText;
    public TextMeshProUGUI levelTextUI; // Seviye adını göstermek için

    [Header("Popup Score Animasyon Ayarları")]
    public float popupAnimDuration = 0.85f;
    public float popupAnimMoveDistance = 60f;
    public Color popupAnimBonusColor = Color.yellow;
    public Color popupAnimNormalColor = Color.white;

    [Header("Davul Verileri")]
    public List<DrumSpriteMapping> drumSprites;

    [Header("Oyun Durumu (Debug İçin Görünür)")]
    [SerializeField] private List<string> drumSequence;
    [SerializeField] private int score = 0;
    [SerializeField] private int currentSequenceIndex = 0;
    [SerializeField] private bool waitingForInput = false;
    [SerializeField] private string expectedDrum = "";
    [SerializeField] private int currentCombo = 0;

    private float expectedHitTime_ideal;
    private bool awaitingSpecificHit;

    [System.Serializable]
    public class DrumSpriteMapping
    {
        public string drumName;
        public Sprite drumSprite;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        currentLevelIndex = startLevelIndex; // Ana menüden gelen başlangıç seviyesini al
        LoadLevelSettings(); // SEVİYE AYARLARINI YÜKLE
        GenerateRandomSequence(); // sequenceLength artık doğru seviyeden geliyor
    }

    void Start()
    {
        // Oyunun ilk başlangıcında ve her yeni seviye başladığında burası çalışacak.
        // ResetAndStartNewLevel fonksiyonu da benzer bir sıfırlama yapacak.
        // Bu yüzden bazı sıfırlamalar ResetAndStartNewLevel'a taşınabilir.
        score = 0; // Her yeni oyun başladığında (sahne yüklendiğinde) skoru sıfırla
        currentSequenceIndex = 0;
        currentCombo = 0;

        // UI Elemanlarının başlangıç durumlarını ayarla
        if (scoreText != null) UpdateScoreUI(); else Debug.LogError("ScoreText atanmamış!");
        if (levelTextUI != null && currentLevelData != null) levelTextUI.text = currentLevelData.levelName; else if (levelTextUI != null) Debug.LogWarning("LevelTextUI var ama currentLevelData null olabilir.");
        if (comboText != null) comboText.enabled = false; else Debug.LogError("ComboText atanmamış!");
        if (popupScoreText != null) popupScoreText.enabled = false; else Debug.LogError("PopupScoreText atanmamış!");
        if (accuracyFeedbackText != null) accuracyFeedbackText.enabled = false; else Debug.LogError("AccuracyFeedbackText atanmamış!");
        if (cueImage != null) cueImage.enabled = false; else Debug.LogError("CueImage atanmamış!");

        if (drumSequence != null && drumSequence.Count > 0)
        {
            StartCoroutine(PlaySequence());
        }
        else
        {
            Debug.LogError("Davul sırası oluşturulamadı, oyun başlatılamıyor!");
            if (scoreText != null) scoreText.text = "Hata: Davul sırası yok!";
        }
        // LoadLevelSettings(); // Awake'de zaten çağrılıyor, burada tekrar gerek yok.
        // GenerateRandomSequence(); // Awake'de zaten çağrılıyor, burada tekrar gerek yok.
    }

    void LoadLevelSettings()
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("Seviye verileri (Levels listesi) tanımlanmamış! Varsayılan ayarlar kullanılıyor.");
            currentLevelData = new LevelData(); // Boş bir LevelData ile fallback
            // Fallback değerlerini burada da ayarla:
            this.delayBetweenBeats = 1.0f; this.perfectWindow = 0.15f; this.goodWindow = 0.3f; this.sequenceLength = 10;
            if (levelTextUI != null) levelTextUI.text = "Level X (Hata)";
            return;
        }

        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count)
        {
            Debug.LogError($"Geçersiz currentLevelIndex: {currentLevelIndex}. 0'a ayarlanıyor.");
            currentLevelIndex = 0; // Güvenli bir değere çek
            if (levels.Count == 0) { /* Tekrar hata durumu */ return; } // Eğer levels hala boşsa
        }

        currentLevelData = levels[currentLevelIndex];

        this.delayBetweenBeats = currentLevelData.delayBetweenBeats;
        this.perfectWindow = currentLevelData.perfectWindow;
        this.goodWindow = currentLevelData.goodWindow;
        this.sequenceLength = currentLevelData.sequenceLength;
        // Çoklu vuruş ayarları Level 3'te eklenecek:
        // this.chanceForMultiHit = currentLevelData.chanceForMultiHit;
        // this.allowMultiHits = currentLevelData.allowMultiHits;

        Debug.Log($"Seviye {currentLevelData.levelName} ayarları yüklendi. Delay: {this.delayBetweenBeats}, Perfect: {this.perfectWindow}, SeqLen: {this.sequenceLength}");

        if (levelTextUI != null)
        {
            levelTextUI.text = currentLevelData.levelName;
        }
    }

    void GenerateRandomSequence()
    {
        drumSequence = new List<string>();
        // sequenceLength artık mevcut seviyeden geliyor
        if (drumSprites == null || drumSprites.Count == 0) { Debug.LogError("Drum Sprites listesi boş!"); return; }
        List<string> availableNames = drumSprites.Where(m => m != null && !string.IsNullOrEmpty(m.drumName) && m.drumSprite != null).Select(m => m.drumName).ToList();
        if (availableNames.Count == 0) { Debug.LogError("Geçerli davul adı bulunamadı!"); return; }

        for (int i = 0; i < this.sequenceLength; i++) // GameManager'ın sequenceLength'ini kullan
        {
            int randomIndex = Random.Range(0, availableNames.Count);
            drumSequence.Add(availableNames[randomIndex]);
        }
        Debug.Log($"Oluşturulan Sıra ({currentLevelData?.levelName}): {string.Join(" -> ", drumSequence)}");
    }

    IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(1.5f); // Oyuna başlamadan önceki genel bekleme

        while (currentSequenceIndex < this.sequenceLength) // GameManager'ın sequenceLength'ini kullan
        {
            if (currentSequenceIndex >= drumSequence.Count)
            {
                Debug.LogWarning("currentSequenceIndex, drumSequence sınırlarını aştı. Döngüden çıkılıyor.");
                break;
            }

            expectedDrum = drumSequence[currentSequenceIndex];
            ShowCue(expectedDrum);

            expectedHitTime_ideal = Time.time;
            awaitingSpecificHit = true;
            waitingForInput = true;

            float maxWaitTimeForHit = this.goodWindow + 0.2f; // GameManager'ın goodWindow'unu kullan
            yield return new WaitForSeconds(maxWaitTimeForHit);

            if (waitingForInput)
            {
                Debug.Log($"Zaman Aşımı! Beklenen: {expectedDrum}. Kombo Sıfırlandı.");
                HideCue();
                awaitingSpecificHit = false;
                waitingForInput = false;
                if (currentCombo > 0) currentCombo = 0;
                ProcessInputResult(false, HitAccuracy.Miss);
            }
            yield return new WaitForSeconds(this.delayBetweenBeats); // GameManager'ın delayBetweenBeats'ini kullan
        }
        // Sekans bittiğinde EndSequence çağır
        EndSequence();
    }

    // --- YENİ EKLENEN SEVİYE GEÇİŞ FONKSİYONLARI ---
    void EndSequence()
    {
        awaitingSpecificHit = false; // Artık vuruş beklenmiyor
        waitingForInput = false;
        Debug.Log($"Seviye {currentLevelData?.levelName} tamamlandı! Skor: {score}");

        currentLevelIndex++; // Bir sonraki seviyeye geç
        GameManager.startLevelIndex = currentLevelIndex; // Bir sonraki oyun için başlangıç seviyesini güncelle (opsiyonel)


        if (currentLevelIndex < levels.Count) // Hala oynanacak seviye var mı?
        {
            string prevLevelName = (currentLevelIndex > 0 && levels.Count > currentLevelIndex - 1) ? levels[currentLevelIndex - 1].levelName : "Bilinmeyen";
            string nextLevelName = levels[currentLevelIndex].levelName;
            if (scoreText != null) scoreText.text = $"{prevLevelName} Bitti! Skor: {score}\nSıradaki: {nextLevelName}";

            StartCoroutine(LoadNextLevelAfterDelay(3.0f)); // 3 saniye sonra sonraki seviye
        }
        else
        {
            // Tüm seviyeler bitti
            if (scoreText != null) scoreText.text = $"OYUN TAMAMLANDI! Toplam Skor: {score}";
            Debug.Log("Tüm seviyeler tamamlandı!");
            // Ana menüye dön veya yeniden başlatma seçeneği sun
            Invoke("ReturnToMainMenuDelayed", 4f); // 4 saniye sonra ana menüye dön
        }
    }

    IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetAndStartNewLevel();
    }

    void ResetAndStartNewLevel()
    {
        // Skoru seviyeler arası tutmak istiyorsanız bu satırı kaldırın.
        // Her seviye için ayrı skor istiyorsanız kalsın. Şimdilik kalsın.
        // score = 0; 
        currentSequenceIndex = 0;
        currentCombo = 0;

        LoadLevelSettings();      // Yeni seviyenin ayarlarını yükle
        GenerateRandomSequence(); // Yeni seviye için sekans oluştur

        // UI'ları yeni seviyeye göre güncelle
        UpdateScoreUI(); // Skor (eğer sıfırlanmadıysa) aynı kalır
        if (levelTextUI != null && currentLevelData != null) levelTextUI.text = currentLevelData.levelName;
        if (comboText != null) comboText.enabled = false;
        if (popupScoreText != null) popupScoreText.enabled = false;
        if (accuracyFeedbackText != null) accuracyFeedbackText.enabled = false;
        if (cueImage != null) cueImage.enabled = false;

        if (drumSequence != null && drumSequence.Count > 0)
        {
            StartCoroutine(PlaySequence()); // Yeni seviyeyi başlat
        }
        else
        {
            Debug.LogError($"Yeni seviye ({currentLevelData?.levelName}) için davul sırası oluşturulamadı!");
        }
    }

    void ReturnToMainMenuDelayed()
    {
        AnaMenuyeDon();
    }
    // --- SEVİYE GEÇİŞ FONKSİYONLARI SONU ---


    public void DrumHit(string hitDrumName)
    {
        if (!awaitingSpecificHit || !waitingForInput)
        {
            // Debug.LogWarning($"Beklenti Dışı Vuruş: {hitDrumName}. awaitingSpecificHit={awaitingSpecificHit}, waitingForInput={waitingForInput}");
            return;
        }

        awaitingSpecificHit = false; // Vuruş geldi, artık bu spesifik anı beklemiyoruz (aynı vuruş için)
        waitingForInput = false;   // Girdi alındı, PlaySequence'daki zaman aşımını durdur
        HideCue();

        int baseScoreToAdd = 0;
        HitAccuracy accuracy;
        bool gainedBonus = false;

        if (hitDrumName == expectedDrum)
        {
            float timeDifference = Mathf.Abs(Time.time - expectedHitTime_ideal);
            if (timeDifference <= this.perfectWindow) { accuracy = HitAccuracy.Perfect; baseScoreToAdd = 3; }
            else if (timeDifference <= this.goodWindow) { accuracy = HitAccuracy.Good; baseScoreToAdd = 1; }
            else { accuracy = HitAccuracy.OK; baseScoreToAdd = 0; }

            if (accuracy == HitAccuracy.Perfect || accuracy == HitAccuracy.Good)
            {
                currentCombo++;
                if (currentCombo >= comboHitsForBonus) { baseScoreToAdd += comboBonusPerSuccessfulHit; gainedBonus = true; }
                Debug.Log($"{accuracy} HIT! Kombo: {currentCombo}");
            }
            else { if (currentCombo > 0) currentCombo = 0; Debug.Log($"OK HIT. Kombo Sıfırlandı."); }
            ProcessInputResult(true, accuracy, baseScoreToAdd, gainedBonus);
        }
        else
        {
            Debug.Log($"YANLIŞ DAVUL! Vurulan: {hitDrumName}, Beklenen: {expectedDrum}. Kombo Sıfırlandı.");
            if (currentCombo > 0) currentCombo = 0;
            ProcessInputResult(false, HitAccuracy.Miss, 0, false);
        }
        UpdateComboUI();
    }

    void ProcessInputResult(bool wasCorrectDrumAndTimingOk, HitAccuracy accuracy, int pointsEarned = 0, bool bonusApplied = false)
    {
        ShowAccuracyFeedback(accuracy);
        if (wasCorrectDrumAndTimingOk) { score += pointsEarned; if (pointsEarned != 0) ShowPopupScore(pointsEarned, bonusApplied); }
        else { score = Mathf.Max(0, score - 1); }
        UpdateScoreUI(); UpdateComboUI(); currentSequenceIndex++;
    }

    void ShowCue(string drumName) { if (cueImage == null) return; Sprite s = GetSpriteForDrum(drumName); if (s != null) { cueImage.sprite = s; cueImage.enabled = true; } else { Debug.LogWarning($"'{drumName}' için sprite bulunamadı!"); cueImage.enabled = false; } }
    void HideCue() { if (cueImage != null) cueImage.enabled = false; }
    Sprite GetSpriteForDrum(string name) { if (drumSprites == null) return null; DrumSpriteMapping m = drumSprites.FirstOrDefault(d => d != null && d.drumName == name); return m?.drumSprite; }
    void UpdateScoreUI() { if (scoreText != null) scoreText.text = "Skor: " + score; }
    void UpdateComboUI() { if (comboText == null) return; if (currentCombo >= 2) { comboText.text = currentCombo + " KOMBO!"; comboText.enabled = true; } else comboText.enabled = false; }
    void ShowAccuracyFeedback(HitAccuracy acc) { if (accuracyFeedbackText == null) return; string msg = ""; Color clr = Color.white; switch (acc) { case HitAccuracy.Perfect: msg = "PERFECT!"; clr = popupAnimBonusColor; break; case HitAccuracy.Good: msg = "GOOD!"; clr = Color.green; break; case HitAccuracy.OK: msg = "OK"; clr = Color.blue; break; case HitAccuracy.Miss: msg = "MISS!"; clr = Color.red; break; } accuracyFeedbackText.text = msg; accuracyFeedbackText.color = clr; accuracyFeedbackText.enabled = true; StopCoroutine("HideAccuracyFeedbackAfterDelayInternal"); StartCoroutine(HideAccuracyFeedbackAfterDelayInternal(0.75f)); }
    IEnumerator HideAccuracyFeedbackAfterDelayInternal(float d) { yield return new WaitForSeconds(d); if (accuracyFeedbackText != null) accuracyFeedbackText.enabled = false; }
    void ShowPopupScore(int amt, bool bonus) { if (popupScoreText != null && amt != 0) { StopCoroutine("AnimatePopupScoreInternal"); StartCoroutine(AnimatePopupScoreInternal(amt, bonus)); } }
    IEnumerator AnimatePopupScoreInternal(int amt, bool bonus) { popupScoreText.text = (amt > 0 ? "+" : "") + amt; popupScoreText.color = bonus ? popupAnimBonusColor : popupAnimNormalColor; popupScoreText.enabled = true; RectTransform rt = popupScoreText.GetComponent<RectTransform>(); Vector2 opos = rt.anchoredPosition; Color scol = popupScoreText.color; float et = 0f; while (et < popupAnimDuration) { et += Time.deltaTime; float prog = Mathf.Clamp01(et / popupAnimDuration); float ep = 1 - Mathf.Pow(1 - prog, 3); rt.anchoredPosition = opos + new Vector2(0, popupAnimMoveDistance * ep); popupScoreText.color = new Color(scol.r, scol.g, scol.b, 1f - prog); yield return null; } popupScoreText.enabled = false; rt.anchoredPosition = opos; popupScoreText.color = scol; }
    public void AnaMenuyeDon() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenuManager"); GameManager.startLevelIndex = 0; /* Ana menüye dönüldüğünde bir sonraki oyunu Level 1'den başlat*/ }
}