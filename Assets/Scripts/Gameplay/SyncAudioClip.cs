using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncAudio
{
	public AudioSource Source;
	public float Pitch
	{
		get
		{
			return this.Source.pitch;
		}
		set
		{
			this.Source.pitch = value;
			this.Sync.Set(this.Source.time);
		}
	}
	public float Time
	{
		get
		{
			return this.Sync.CurrentTime;
		}
		set
		{
			this.Source.time = value;
			this.Sync.Set(Source.time);
		}
	}
	private Sync Sync;
	public SyncAudio(GameObject parent)
	{
		this.Source = parent.AddComponent<AudioSource>();
		this.Sync = parent.AddComponent<Sync>();
		this.Sync.SyncTo(this);
	}
	public void Play()
	{
		if (this.Source.clip.loadState != AudioDataLoadState.Loaded) return;
		this.Source.Play();
		this.Sync.Toggle(true);
		this.Sync.Set(this.Source.time);
	}
	public void Pause()
	{
		this.Sync.Pause();
		this.Source.Pause();
		this.Source.time = this.Sync.CurrentTime;
	}
	public void Unpause()
	{
		this.Sync.Unpause();
		this.Source.UnPause();
		this.Source.time = this.Sync.CurrentTime;
	}
	public void Stop()
	{
		this.Source.Stop();
		this.Sync.Toggle(false);
		this.Source.time = 0;
		this.Sync.Set(0);
	}
}
internal class Sync : MonoBehaviour
{
	public float CurrentTime { get; private set; }
	private SyncAudio syncAudio;
	private bool pause = false;
	private bool started = false;
	private float lastAudioTime = 0;
	void Awake()
	{
		DontDestroyOnLoad(this);
	}
	public void SyncTo(SyncAudio parent)
	{
		this.syncAudio = parent;
	}
	public void Set(float time)
	{
		this.CurrentTime = time;
	}
	public void Pause()
	{
		this.pause = true;
	}
	public void Unpause()
	{
		this.pause = false;
	}
	public void Toggle(bool toggle)
	{
		this.CurrentTime = 0;
		this.started = toggle;
	}
	void Update()
	{
		if (!this.started) return;
		if (this.pause) return;
		this.CurrentTime += Time.deltaTime * syncAudio.Pitch;
		if (this.lastAudioTime != syncAudio.Source.time)
		{
			this.lastAudioTime = syncAudio.Source.time;
			if (Mathf.Abs(this.CurrentTime - this.lastAudioTime) > Time.fixedDeltaTime)
				this.CurrentTime = this.lastAudioTime;
		}
	}

}