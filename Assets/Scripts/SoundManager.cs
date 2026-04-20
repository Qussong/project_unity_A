using System;
using DG.Tweening;
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
        // WebGL 자동재생 정책: 사용자 인터랙션 후 재생해야 함
        // StartGame() 또는 첫 터치 시 PlayBGM() 호출할 것
    }

    public bool IsMuted { get; private set; } = false;

    public void ToggleMute()
    {
        IsMuted = !IsMuted;

        _asBGM.mute = IsMuted;
        _asSFX.mute = IsMuted;
        _asUI.mute = IsMuted;
        OnMuteChanged?.Invoke(IsMuted);
    }

    public void FadeBGMVolume(float target, float duration)
    {
        _asBGM.DOKill();
        _asBGM.DOFade(target, duration);
    }

    public void PlayBGM(AudioClip clip)
    {
        _asBGM.clip = clip;
        _asBGM.loop = true;
        _asBGM.volume = 0.5f;
        _asBGM.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        _asSFX.PlayOneShot(clip);
        _asSFX.volume = 0.3f;
    }

    

}
