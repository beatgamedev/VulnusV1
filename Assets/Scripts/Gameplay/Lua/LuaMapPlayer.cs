using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public class LuaMapPlayer
{
	public LuaSignal OnHit;
	public LuaSignal OnMiss;
	public LuaMath.Transform GameOffset;
	private Transform noteParent;
	public LuaMath.Transform CameraOffset;
	private Transform cameraOffset;
	public bool Paused => mapPlayer.Paused;
	public bool PauseEnabled
	{
		get { return mapPlayer.PauseEnabled; }
		set { mapPlayer.PauseEnabled = value; }
	}
	public bool SkipEnabled
	{
		get { return mapPlayer.SkipEnabled; }
		set { mapPlayer.SkipEnabled = value; }
	}
	private MapPlayer mapPlayer;
	[MoonSharpHidden]
	public LuaMapPlayer(MapPlayer mapPlayer)
	{
		this.mapPlayer = mapPlayer;
		this.OnHit = new LuaSignal();
		mapPlayer.NoteHit += (GameObject note, int score) =>
		{
			this.OnHit.Fire(UserData.Create(note), DynValue.NewNumber(score));
		};
		this.OnMiss = new LuaSignal();
		mapPlayer.NoteMiss += (GameObject note, int score) =>
		{
			this.OnMiss.Fire(UserData.Create(note));
		};
		this.CameraOffset = new LuaMath.Transform();
		this.cameraOffset = new GameObject("CameraParent").transform;
		Component.FindObjectOfType<GameCamera>().transform.SetParent(this.cameraOffset);
		this.GameOffset = new LuaMath.Transform();
		this.noteParent = this.mapPlayer.NotesParent.transform;
	}
	[MoonSharpHidden]
	public void Update()
	{
		this.CameraOffset.ApplyTo(this.cameraOffset);
		this.GameOffset.ApplyTo(this.noteParent);
	}
}
