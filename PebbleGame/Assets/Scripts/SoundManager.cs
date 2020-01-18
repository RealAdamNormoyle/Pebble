using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public List<AudioClip> sounds = new List<AudioClip>();
    public Transform soundsContainer;
    public List<GameObject> spawnedSounds = new List<GameObject>();
    public float T;
    public AudioMixer mixer;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    public void PlaySound(SOUNDS sound)
    {
        var obj = new GameObject(sound.ToString());
        obj.transform.SetParent(soundsContainer);
        obj.transform.position = Vector3.zero;
        var audio = obj.AddComponent<AudioSource>();
        audio.loop = false;
        audio.outputAudioMixerGroup = mixer.FindMatchingGroups("Master")[0];
        audio.clip = sounds[(int)sound];
        audio.Play();
        spawnedSounds.Add(obj);
    }

    public void Update()
    {
        mixer.SetFloat("Volume", (PlayerPrefs.GetInt("SOUND") == 0) ? -80 : 0);

        T += Time.deltaTime;
        if (T < 1f)
            return;

        T = 0;

        List<GameObject> cleanup = new List<GameObject>();
        foreach (var item in spawnedSounds)
        {
            if (item.GetComponent<AudioSource>().isPlaying)
            {
                cleanup.Add(item);
            }
        }

        for (int i = 0; i < cleanup.Count; i++)
        {
            spawnedSounds.Remove(cleanup[0]);
            Destroy(cleanup[0]);
        }

    }

}

public enum SOUNDS
{
    Button,
    SplashSmall,
    SplashMed,
    SplashBig,
    Popup,

}
