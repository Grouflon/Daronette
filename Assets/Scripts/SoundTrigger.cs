using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    public bool justOnce = true;

    // Start is called before the first frame update
    void Start()
    {
        m_audio = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!m_played)
        {
            if (justOnce)
            {
                m_played = true;
            }

            m_audio.Play();
        }
    }

    AudioSource m_audio;
    bool m_played = false;
}
