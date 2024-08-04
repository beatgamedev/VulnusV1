using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

public class SoundPlayer : MonoBehaviour
{
	public AudioClip default_hit;
	public AudioClip default_miss;
	public AudioClip default_click;
	public AudioClip default_songclick;
	public AudioClip default_death;

	public GameObject Sounds;
	public bool bgMusic;
	public Transform _soundprefab;
	public AudioClip bgMusicClip;
	public AudioClip hit;
	public AudioClip miss;
	public AudioClip click;
	public AudioClip songclick;
	public AudioClip death;
	public SyncAudio music;
	public AudioMixerGroup Menu;
	public AudioMixerGroup Game;
	public TimeSpan CurrentTime
	{
		get
		{
			return TimeSpan.FromSeconds(music.Time);
		}
		set
		{
			music.Time = (float)value.TotalSeconds;
		}
	}
	private static SoundPlayer _instance;
	public static SoundPlayer Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<SoundPlayer>();
			}
			return _instance;
		}
	}
	private void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("Multiple SoundPlayers have been created. Something is very wrong!");
			Destroy(this.gameObject);
			return;
		}
		this.music = new SyncAudio(gameObject);
		this.music.Source.volume = 0.5f;
		_instance = this;
		DontDestroyOnLoad(this.gameObject);
	}

	public void Start()
	{
		bgMusic = GameSetting.MenuMusic;
		if (bgMusic)
        {
			music.Source.clip = bgMusicClip;
			music.Source.volume = (float)GameSetting.MenuVolume;
		}
	}

	private IEnumerator waitForSound(Transform source)
	{
		AudioSource audio = source.GetComponent<AudioSource>();

		yield return new WaitWhile(() => audio.isPlaying);

		Destroy(source.gameObject);
	}

    [System.Diagnostics.CodeAnalysis.SuppressMessage("TypeSafety", "UNT0006:Incorrect message signature", Justification = "Not built-in function")]
    public void Reset(bool fullReset = false)
	{
		AudioClip currentClip = null;

		music.Stop();
		music.Time = 0f;
		music.Pitch = 1f;
		music.Source.volume = 0.5f; // Reset to default (50%)
		bgMusic = GameSetting.MenuMusic;
		if (fullReset && bgMusic)
		{
			music.Source.loop = true;
			music.Source.clip = bgMusicClip;
			music.Source.volume = (float)GameSetting.MenuVolume;
			music.Play();
		}
		else
		{
			music.Source.clip = currentClip;
			music.Source.loop = false;
		}
	}

	public async void Load(string file, bool autoplay = true, float volume = 0.5f, float pitch = 1f)
	{
		AudioClip clip = await LoadClip("file:///" + WebUtility.UrlEncode(file));

		if (clip == null)
		{
			Debug.LogError($"AudioClip Failed to load!\n{file}");
			return;
		}
		else
		{
			int Timeout = 0;
			clip.LoadAudioData();
			while (clip.loadState == AudioDataLoadState.Loading && Timeout < 100)
			{
				Timeout += 1;
				await Task.Delay(100);
			}

			if (!(clip.loadState == AudioDataLoadState.Loaded))
			{
				MapPlayer.SongFailedToLoad = true;
				Debug.LogError($"AudioClip Failed to load!\n{file}");
				Reset(false);
				return;
			}

			clip.name = file;
			Debug.Log(clip.name + ":" + clip.length);
		}

		Reset(false);
		if (music.Source.isPlaying && !autoplay)
		{
			music.Stop();
		}
		playSong(clip, volume, pitch);
	}

	async Task<AudioClip> LoadClip(string path)
	{
		AudioClip clip = null;
		using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
		{
			uwr.SendWebRequest();
			try
			{
				while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

				if (uwr.result != UnityWebRequest.Result.Success) Debug.Log($"{uwr.error}");
				else
				{
					clip = DownloadHandlerAudioClip.GetContent(uwr);
				}
			}
			catch (Exception err)
			{
				Debug.Log($"{err.Message}, {err.StackTrace}");
				return null;
			}
		}

		return clip;
	}
	internal void playSong(AudioClip song, float volume = 0.5f, float pitch = 1f)
	{
		music.Source.clip = song;
		music.Source.volume = volume;
		music.Pitch = pitch;
	}

	internal void Play(string name, float volume = 0.5f, float pitch = 1f, int priority = 128)
	{
		priority = Math.Min(priority, 256);
		priority = Math.Max(priority, 0);

		Transform sound = Instantiate(_soundprefab, new Vector3(1, 1, -4), Quaternion.identity);
		sound.parent = Sounds.transform;
		AudioSource audio = sound.GetComponent<AudioSource>();

		audio.name = name;
		audio.volume = volume / 2f;
		audio.pitch = pitch;
		audio.priority = priority;

		if (name == "hit")
		{
			audio.clip = hit;
		}
		else if (name == "miss")
		{
			audio.clip = miss;
		}
		else if (name == "songclick")
		{
			audio.clip = songclick;
		}
		else if (name == "click")
		{
			audio.clip = click;
		}
		else if (name == "death")
		{
			audio.clip = death;
		}

		audio.Play();

		StartCoroutine(waitForSound(sound));
	}
}
