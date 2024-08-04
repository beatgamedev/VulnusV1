using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
public class LuaTime
{
	private Dictionary<float, LuaSignal> atSignals;
	public LuaSignal Step;
	public float MapTime
	{
		get
		{
			return MapPlayer.RealTime;
		}
	}
	public float SongTime
	{
		get
		{
			return MapPlayer.SongTime;
		}
	}
	private MapPlayer mapPlayer;
	[MoonSharpHidden]
	public LuaTime(MapPlayer mapPlayer)
	{
		this.mapPlayer = mapPlayer;
		this.Step = new LuaSignal();
		this.atSignals = new Dictionary<float, LuaSignal>();
	}
	[MoonSharpHidden]
	public void Update()
	{
		this.Step.Fire(DynValue.NewNumber(Time.deltaTime));
		var time = this.MapTime;
		var remove = new List<float>();
		foreach (KeyValuePair<float, LuaSignal> pair in atSignals)
		{
			if (time >= pair.Key)
			{
				Debug.Log($"At {time}");
				pair.Value.Fire();
				remove.Add(pair.Key);
			}
		}
		foreach (float key in remove)
		{
			atSignals.Remove(key);
		}
	}
	public LuaSignal At(float time)
	{
		if (atSignals.ContainsKey(time))
			return atSignals[time];
		var sig = new LuaSignal();
		atSignals.Add(time, sig);
		return sig;
	}
}
