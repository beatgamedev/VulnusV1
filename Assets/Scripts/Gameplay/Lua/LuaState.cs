using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using PostProcessing = UnityEngine.Rendering.PostProcessing;
using MoonSharp.Interpreter;
public class LuaSignal
{
	public class Connection
	{
		[MoonSharpHidden]
		public DynValue Function;
		[MoonSharpHidden]
		public bool Connected;
		[MoonSharpHidden]
		public Connection(DynValue function)
		{
			this.Function = function;
			this.Connected = true;
		}
		public void Disconnect()
		{
			this.Connected = false;
		}
	}
	private List<Connection> connections = new List<Connection>();
	[MoonSharpHidden]
	public void Fire(params DynValue[] args)
	{
		foreach (Connection connection in connections.ToArray())
		{
			if (!connection.Connected) continue;
			Task.Run(() => connection.Function.Function.Call(args));
		}
	}
	public Connection Connect(DynValue function)
	{
		if (function.Type != DataType.Function)
			return null;
		var connection = new Connection(function);
		this.connections.Add(connection);
		return connection;
	}
}
public class LuaState
{
	public static CoreModules Modules =
		CoreModules.TableIterators |
		CoreModules.Metatables |
		CoreModules.String |
		CoreModules.Table |
		CoreModules.Basic |
		CoreModules.ErrorHandling |
		CoreModules.Math |
		CoreModules.OS_Time |
		CoreModules.Json;
	private static bool initialized = false;
	public static void InitLua()
	{
		if (initialized)
			return;
		initialized = true;
		UserData.RegisterProxyType<LuaProxies.PostProcessVolume, PostProcessing.PostProcessVolume>(obj => new LuaProxies.PostProcessVolume(obj));
		UserData.RegisterType<LuaMath.Vector3>();
		UserData.RegisterType<LuaMath.Transform>();
		UserData.RegisterType<LuaSignal>();
		UserData.RegisterType<LuaSignal.Connection>();
		UserData.RegisterType<LuaTime>();
		UserData.RegisterType<LuaMapPlayer>();
	}
	private MapPlayer mapPlayer;
	private Script script;
	private Thread thread;
	private DynValue loadedScript;
	public LuaState(MapPlayer mapPlayer, string script)
	{
		this.mapPlayer = mapPlayer;
		this.script = new Script(Modules);
		this.InitGlobals();
		this.loadedScript = this.script.LoadString(script);
		this.thread = new Thread(new ThreadStart(this.Start));
	}
	public void Run()
	{
		this.thread.Start();
	}
	public void Update()
	{
		LuaTime time = (LuaTime)this.script.Globals["Time"];
		time.Update();
		LuaMapPlayer mapPlayer = (LuaMapPlayer)this.script.Globals["Game"];
		mapPlayer.Update();
	}
	private void Start()
	{
		Debug.Log("Started Lua thread");
		this.script.Call(this.loadedScript);
	}
	public void Kill()
	{
		if (thread.IsAlive)
			thread.Abort();
		Debug.Log("Killed Lua thread");
	}
	private void InitGlobals()
	{
		this.script.Options.DebugPrint = s => { Debug.Log("[LUA] " + s); };
		this.script.Globals["Vector3"] = UserData.CreateStatic<LuaMath.Vector3>();
		this.script.Globals["Time"] = new LuaTime(mapPlayer);
		this.script.Globals["Game"] = new LuaMapPlayer(mapPlayer);
		this.script.Globals["GFX"] = mapPlayer.postProcessVolume;
	}
}