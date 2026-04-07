using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("BGM")]
    public AudioClip clipMainBGM;

    [Header("Sound Effect")]
    public AudioClip clipBlockPlace;

    [Header("Audio Source")]
    [SerializeField] private AudioSource _asBGM; // 배경은 전용 (loop = true)
    [SerializeField] private AudioSource _asSFX; // 효과음 전용

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // BGM 재생
        if (clipMainBGM != null)
        {
            _asBGM.clip = clipMainBGM;
            _asBGM.loop = true;
            _asBGM.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        _asSFX.PlayOneShot(clip);
    }

}
