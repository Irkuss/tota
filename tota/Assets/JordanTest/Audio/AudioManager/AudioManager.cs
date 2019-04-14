using UnityEngine.Audio;
using System;
using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{

	public static AudioManager instance;

	public AudioMixerGroup mixerGroup;

	public Sound[] sounds;

	void Awake()
	{
		if (instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (Sound s in sounds)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			s.source.outputAudioMixerGroup = s.mixerGroup;
		}
	}

    public void Play(string sound)
	{
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null)
		{
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

		s.source.Play();
	}

    public IEnumerator EndMusic(string son)
    {
        Sound sound = Array.Find(sounds, item => item.name == son);
        AudioSource oldSound = sound.source;
        while (oldSound.volume > 0)
        {
            oldSound.volume -= 0.01f;
            yield return new WaitForSeconds(.05f);
        }
        Debug.Log("Stoped");
        oldSound.Stop();
    }

    public IEnumerator StartMusic(string son)
    {
        Sound sound = Array.Find(sounds, item => item.name == son);
        sound.source.Play();
        sound.source.volume = 0f;
        while (sound.source.volume < sound.volume)
        {
            sound.source.volume += 0.01f;
            yield return new WaitForSeconds(.1f);
        }
        Debug.Log("FINISH");

    }
}
