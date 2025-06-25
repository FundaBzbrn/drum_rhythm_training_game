using UnityEngine; //unity uzantısı dahil edildi
using System.Collections; 

[RequireComponent(typeof(AudioSource))] //özniteliktir
public class DrumHitDetector : MonoBehaviour //DrumHitDetector Sınfı MonoBehaviour sınıfından türetilmiştir.MonoBehaviour,Unity'deki tüm script bileşenlerinin temel sınıfıdır.
{                                            //Bu sayede  Awake, Start, Update, OnMouseDown gibi Unity'nin özel mesaj fonksiyonlarını kullanabilir.
    [Header("Drum Kimliği ve Ses")] // Inspector'da başlık. Attribute'dir

    [Tooltip("Bu davulun benzersiz adı. GameManager'daki sekans isimleriyle EŞLEŞMELİDİR!")]
    public string drumName = "Bilinmeyen Davul"; // Defaultta bilinmeyen, inspectordan ayarlanmalı.

    [Tooltip("Bu davula vurulduğunda çalınacak ses dosyası.")]
    public AudioClip drumSoundClip;

    [Header("Görsel Efekt Ayarları")] // Inspector'da başlık. Attribute'dir

    [Tooltip("İsteğe bağlı: Vurulduğunda aktifleşecek ayrı bir parıltı/efekt objesi.")]
    public GameObject glowEffectObject; // Parıltı efekti için ayrı bir obje 

    [Tooltip("Vurulduğunda davulun ne kadar büyüyeceği (1 = değişmez, 1.1 = %10 büyür).")]
    public float scaleAmount = 1.1f;

    [Tooltip("Görsel efektin süresi (saniye).")]
    public float effectDuration = 0.1f;

    private AudioSource audioSource;
    private Vector3 originalScale;
    private bool isEffectActive = false; // Efektin zaten aktif olup olmadığını kontrol etmek için

    void Awake() //MonoBehaviour'dan gelen özel bir Unity mesaj fonksiyonudur. Start() metodundan önce, oyunun ilk karesinden önce *bir kez* çağrılır.
    {

        audioSource = GetComponent<AudioSource>(); //GameObject'e bağlı AudioSource bileşenini alır.

        // Başlangıçtaki orijinal boyutu sakla
        originalScale = transform.localScale;

        // Parıltı efekti objesi varsa, başlangıçta kapalı olduğundan emin ol
        if (glowEffectObject != null)
        {
            glowEffectObject.SetActive(false);
        }

        // Hata Kontrolleri
        if (string.IsNullOrEmpty(drumName) || drumName == "Bilinmeyen Davul")
        {
            // Eğer davul ismi atanmamışsa veya varsayılan değerdeyse uyar.
            // Bu, GameManager'ın davulu tanıyamamasına neden olabilir.
            //Yani -> Eğer drumName boşsa veya varsayılan değerdeyse uyarı ver.
            Debug.LogWarning($"'{gameObject.name}' isimli objenin 'Drum Name' alanı atanmamış veya varsayılan değerde. Lütfen Inspector'dan benzersiz bir isim atayın.", this.gameObject);
        }
        if (audioSource != null && drumSoundClip == null)
        {
            //// Drum'a ses atanmadıysa uyarı verir.
            Debug.LogWarning($"'{gameObject.name}' (Drum: '{drumName}') için 'Drum Sound Clip' atanmamış. Ses çalınmayacak.", this.gameObject);
        }
    }

     // Kullanıcı bu objeye tıkladığında tetiklenir (Collider varsa çalışır).
    void OnMouseDown()
    {
        // Oyunun durumunu kontrol eden merkezi GameManager'a haber ver.
        if (GameManager.Instance != null)
        {
            // GameManager'daki DrumHit metodunu çağır ve bu davulun adını parametre olarak gönder.
            GameManager.Instance.DrumHit(this.drumName);
        }
        else
        {
            // Eğer GameManager bulunamazsa, bu kritik bir hata
            Debug.LogError("GameManager.Instance bulunamadı! Sahnenizde bir GameManager objesi olduğundan ve script'inin çalıştığından emin olun.");
        }

        // Atanmış bir ses klibi varsa çal.
        if (audioSource != null && drumSoundClip != null)
        {
            audioSource.PlayOneShot(drumSoundClip);
        }

        // Eğer zaten bir efekt aktif değilse, görsel efekti uygula.
        if (!isEffectActive)
        {
            ApplyHitEffect();
        }
    }

    // Görsel efektleri başlatan metod.
    void ApplyHitEffect()
    {
        isEffectActive = true; //Efekt aktif olarak işaretlendi.

        transform.localScale = originalScale * scaleAmount; //// Drum belirlenen miktarda büyütülür.

        if (glowEffectObject != null)
        {
            glowEffectObject.SetActive(true);
        }

        // Belirlenen süre sonunda efektleri geri alacak metot
        Invoke("ResetEffect", effectDuration);
    }

    // Görsel efektleri geri alan metot
    void ResetEffect()
    {
        // Boyutu orijinal haline getir
        transform.localScale = originalScale;

        // Parıltı objesi varsa kapat
        if (glowEffectObject != null)
        {
            glowEffectObject.SetActive(false);
        }

        isEffectActive = false; 
    }
}