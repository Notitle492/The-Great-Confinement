using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;
    
    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void PlaySound3D(AudioClip clip, Vector3 pos)
    {
        if(clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, pos);
        }
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        PlaySound3D(sfxLibrary.GetClipFromName(soundName), pos);
    }

    public void PlaySound2D(string soundName)
    {


        if (sfxLibrary == null)
        {
            Debug.LogWarning("SoundManager: sfxLibrary 尚未指派");
            return;
        }

        if (sfx2DSource == null)
        {
            Debug.LogWarning("SoundManager: sfx2DSource 尚未指派");
            return;
        }

        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip == null)
        {
            Debug.LogWarning("SoundManager: 找不到音效 " + soundName);
            return;
        }

        

        /* sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName)); */
        sfx2DSource.PlayOneShot(clip);
    }

}
