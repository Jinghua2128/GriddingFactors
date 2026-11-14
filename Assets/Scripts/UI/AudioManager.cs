using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Sfx
{
    Click,
    Draw,
    Place,
    Card,
    CardDraw,
    Bingo,
    Buy,
    Win,
    Lose
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private readonly Dictionary<Sfx, AudioClip> clips = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (instance != null)
            return;

        GameObject go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        instance = this;

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = 0.28f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.85f;

        clips[Sfx.Click] = Load("click");
        clips[Sfx.Draw] = Load("draw");
        clips[Sfx.Place] = Load("place");
        clips[Sfx.Card] = Load("card");
        clips[Sfx.CardDraw] = Load("card_draw");
        clips[Sfx.Bingo] = Load("win");
        clips[Sfx.Buy] = Load("buy");
        clips[Sfx.Win] = Load("win");
        clips[Sfx.Lose] = Load("lose");

        PlayBgm();
    }

    private void Start()
    {
        HookSceneButtons();
    }

    public static void Play(Sfx sfx, float volume = 1f)
    {
        if (instance == null)
            return;

        if (instance.clips.TryGetValue(sfx, out AudioClip clip) &&
            clip != null)
        {
            instance.sfxSource.PlayOneShot(clip, volume);
        }
    }

    public static void PlayClick()
    {
        Play(Sfx.Click, 0.6f);

        if (instance != null && !instance.bgmSource.isPlaying)
            instance.PlayBgm();
    }

    public static void AddClick(Button button)
    {
        if (button != null)
            button.onClick.AddListener(PlayClick);
    }

    public static void HookSceneButtons()
    {
        foreach (Button button in
                 FindObjectsByType<Button>(
                     FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            AddClick(button);
            ButtonFrame.Apply(button);
        }
    }

    private void PlayBgm()
    {
        if (bgmSource.clip == null)
            bgmSource.clip = Load("bgm");

        if (bgmSource.clip != null)
            bgmSource.Play();
    }

    private static AudioClip Load(string name)
    {
        return Resources.Load<AudioClip>("Audio/" + name);
    }
}
