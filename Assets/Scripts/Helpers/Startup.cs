using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Application = UnityEngine.Application;
using System.Runtime.InteropServices;

public class Startup : MonoBehaviour
{
	[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
	public static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);
	public GameObject LoadingText;
	public string[] GetCommandLineArgs() // just for getting command line args
	{
		//Debug.Log("CommandLine: {0}" + Environment.CommandLine);

		string[] rawArguments = Environment.GetCommandLineArgs(); // idk why but yea add args later ig
		string[] arguments = new string[] { };
		//Debug.Log("GetCommandLineArgs: {0}" + string.Join(", ", rawArguments));

		return rawArguments;
	}

	private string getNextArg(string[] args, int i)
	{
		try
		{
			string nextStr = args[i + 1]; // get next arg

			if (nextStr.StartsWith("-")) // dont allow "-" at the start since thats the next arg
			{
				return "null";
				//throw new Exception("Invaild argument");
			}

			return nextStr;
		}
		catch (Exception)
		{
			//Debug.LogError(e.Message);
			return "null";
		}
	}

	private void doArgFunction(string arg, string val)
	{
		if (val == "null")
		{
			val = null; // set to null
		}

		switch (arg)
		{
			default: // arg is not defined in switch statement
				break;
		}
	}

	IEnumerator Start()
	{
#if PLATFORM_STANDALONE_WIN && !UNITY_EDITOR

		var path = Path.Combine(Application.dataPath, "../..");
		var temp = Path.GetTempPath();
		if (path.IndexOf(temp, StringComparison.OrdinalIgnoreCase) == 0)
		{
			MessageBox(IntPtr.Zero, "Please extract before running the game again.", "ERROR", 0x00000000 | 0x00000010);
			Application.Quit();
			yield break;
		}
#endif
		Application.wantsToQuit += ApplicationQuit;
		QualitySettings.vSyncCount = 0; // Force VSync off
		GameSetting.Start(); // Load the settings on startup
		yield return new WaitUntil(() => DiscordHandler.Initialized); // Wait for discord RPC to load before starting anything else
																	  //yield return new WaitUntil(() => DiscordHandler.currentUser != null);

		Debug.Log("Discord running: " + DiscordHandler.DiscordRunning.ToString());

		string[] CmdArgs = GetCommandLineArgs(); // get cmd line args

		for (int i = 0; i < CmdArgs.Length; i++) // loop for args
		{
			try
			{
				string str = CmdArgs[i]; // current arg
				string nextStr = getNextArg(CmdArgs, i); // get next arg (sometimes this will be used)
				doArgFunction(str, nextStr); // send the arg plus next arg to see if its a command
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message); // log Exception
			}
		}
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
		string assetsDir = Path.Combine(Application.persistentDataPath, "assets");
		string mapsDir = Path.Combine(Application.persistentDataPath, "maps");
#else
		string assetsDir = Path.Combine(Application.dataPath, "..", "assets");
		string mapsDir = Path.Combine(Application.dataPath, "..", "maps");
#endif
		if (!Directory.Exists(assetsDir))
		{
			Directory.CreateDirectory(assetsDir);
			yield return new WaitForSeconds(0.2f);
		}
		LuaState.InitLua();
		bool errored = false;
		try
		{
			MapLoader.SetMapsDir(mapsDir);
		}
		catch (Exception e)
		{
			LoadingText.GetComponent<TextMeshProUGUI>().text = $"An error occured\n{e.Message}";
			errored = true;
		}
		if (errored)
			yield return new WaitForSeconds(2f);
		else
			yield return new WaitForSeconds(1f);
		SceneManager.LoadScene("Menu"); // Load Menu
	}

	private bool ApplicationQuit()
	{
		GameSetting.SaveSettings();
		return true;
	}
}
