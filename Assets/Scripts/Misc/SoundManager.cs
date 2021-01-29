using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public List<Sound> Sounds;
    private static SoundManager _instance;

    private void Awake()
    {
        InitializeAudioClips();
    }

    public static SoundManager Instance
    {
        get
        {
            if (_instance == null) 
            {
                if (FindObjectOfType<SoundManager>())
                {
                    _instance = _instance = GameObject.FindObjectOfType<SoundManager>();
                }
            }
            return _instance;
        }
    }

    private void Start()
    {
        foreach(Sound sound in Sounds)
        {
            if (sound.ShouldStartOnStart)
            {
                sound.AudioSource.Play();
            }
        }
    }

    public void InitializeAudioClips()
    {
        foreach(Sound sound in Sounds)
        {
            sound.AudioSource = gameObject.AddComponent<AudioSource>();
            sound.AudioSource.clip = sound.AudioClip;
            sound.AudioSource.volume = SetVolume(sound.IsMusicVolume);
            sound.AudioSource.loop = sound.ShouldLoop;
        }
    }

    public void PlaySound(string name)
    {
        Sound sound = Sounds.Find(s => s.Name == name);
        if (sound == null)
            return;
        sound.AudioSource.Play();
    }

    public void StopSound(string name)
    {
        Sound sound = Sounds.Find(s => s.Name == name);
        if (sound == null)
            return;
        sound.AudioSource.Stop();
    }

    private float SetVolume(bool musicVolume)
    {
        PlayerSettings settings = SaveHandler.Instance.LoadDataContainer<PlayerSettings>();
        if(settings != null)
            return musicVolume ? ((float) settings.MusicVolume / 100) : ((float) settings.AudioVolume / 100);
        return 1;
    }
}
