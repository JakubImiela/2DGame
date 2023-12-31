using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class Vocals : MonoBehaviour
{
    private AudioSource source;
    
    public static Vocals instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        source = gameObject.AddComponent<AudioSource>();
    }

    public void Say(AudioObject clip)
    {
        if (source.isPlaying)
            source.Stop();

        source.PlayOneShot(clip.clip);

        UI.instance.SetSubtitle(clip.subtitle, clip.clip.length);
    }

}
