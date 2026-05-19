using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;
    public float maxVolume = 1f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // РЕАЛІЗАЦІЯ DDOL (DontDestroyOnLoad)
        if (Instance == null)
        {
            Instance = this;
            // Перевіряємо, чи є об'єкт кореневим (root), бо DDOL працює тільки для них
            if (transform.parent == null) 
            {
                DontDestroyOnLoad(gameObject);
            }
            else 
            {
                // Якщо скрипт на вкладеному об'єкті, DDOL треба робити для всього батька
                DontDestroyOnLoad(transform.root.gameObject);
            }
        }
        else
        {
            // Якщо менеджер вже існує (після переходу сцени) — знищуємо копію
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        // ВАЖЛИВО: не ставимо 0f тут, якщо хочемо, щоб музика грала відразу 
        // або регулюємо через StartDialogue/EventManager
    }

    // Метод, який викликає EventManager
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        // Якщо кліп нульовий — вимикаємо музику плавним затиханням
        if (clip == null)
        {
            fadeCoroutine = StartCoroutine(FadeOut());
            return;
        }

        // КЛЮЧОВИЙ МОМЕНТ: Якщо той самий трек вже грає — просто ігноруємо команду.
        // Оскільки об'єкт не видалявся при зміні сцени, аудіосорс продовжує крутити кліп
        // з того ж місця, де він був на попередній сцені.
        if (audioSource.isPlaying && audioSource.clip == clip)
        {
            Debug.Log("[AudioManager] Той самий трек уже грає, продовжуємо...");
            return;
        }

        // Якщо трек новий — робимо кросфейд
        fadeCoroutine = StartCoroutine(FadeToNewClip(clip));
    }

    private IEnumerator FadeToNewClip(AudioClip newClip)
    {
        if (audioSource.isPlaying)
            yield return StartCoroutine(FadeOut());

        audioSource.clip = newClip;
        audioSource.Play();
        yield return StartCoroutine(FadeIn());
    }

    private IEnumerator FadeOut()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        audioSource.clip = null;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, maxVolume, elapsed / fadeDuration);
            yield return null;
        }

        audioSource.volume = maxVolume;
    }
}