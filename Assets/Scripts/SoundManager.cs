using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("BGM")]
    public AudioClip clipMainBGM;

    [Header("Sound Effect")]
    public AudioClip clipBlockPlace;
    public AudioClip clipGameOver;

    [Header("UI")]
    public AudioClip clipClick;

    [Header("Audio Source")]
    [SerializeField] private AudioSource _asBGM; // 배경은 전용 (loop = true)
    [SerializeField] private AudioSource _asSFX; // 효과음 전용
    [SerializeField] private AudioSource _asUI; // UI 전용

    public event Action<bool> OnMuteChanged;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (clipMainBGM != null)
            PlayBGM(clipMainBGM);
    }

    public bool IsMuted { get; private set; } = false;

    public void ToggleMute()
    {
        IsMuted = !IsMuted;

        _asBGM.mute = IsMuted;
        _asSFX.mute = IsMuted;
        OnMuteChanged.Invoke(IsMuted);
    }

    public void PlayBGM(AudioClip clip)
    {
        _asBGM.clip = clip;
        _asBGM.loop = true;
        _asBGM.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        _asSFX.PlayOneShot(clip);
    }

}
