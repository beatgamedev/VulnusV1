using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class MapLoader
{
	private static string isAlphaNumeric(string strToCheck)
	{
		Regex rg = new Regex(@"[^\w\s\\/\.:]+");
		bool isMatch = rg.IsMatch(strToCheck);
		if (isMatch)
			return rg.Replace(strToCheck, "_");
		else
			return string.Empty;
	}

	public static bool HasStarted { get; private set; } = false; // this might cause issues but it is so i can test without startup Lol
	public static List<Map> Maps = new List<Map>();
	public static string MapDirectory { get; private set; } = "";
	public static Map LoadMap(string path)
	{
		if (!HasStarted) HasStarted = true;

		//Debug.Log("Loading map!");

		string newPath = isAlphaNumeric(path);

		//Debug.Log(path);
		//Debug.Log(newPath);

		if (newPath != string.Empty)
		{
			Directory.Move(path, newPath);
			Debug.LogWarning(path + " -> " + newPath);
			path = newPath;
		}

		string json = File.ReadAllText(Path.Combine(path, "meta.json"));
		Map map = JsonUtility.FromJson<Map>(json);
		if (map.version > Map.SupportedFormat)
		{
			map.CanBePlayed = false;
			return map;
		}
		if (map.difficulties.Count < 1)
		{
			map.CanBePlayed = false;
			return map;
		}
		map.AudioPath = Path.Combine(path, map.music);

		if (!File.Exists(map.AudioPath))
		{
			map.CanBePlayed = false;
			return map;
		}
		else
		{
			foreach (string file in Directory.GetFiles(path))
			{
				if (Path.GetFileNameWithoutExtension(file) == "cover")
				{
					map.CoverPath = file;
					break;
				}
			}
			LoadCover(map);

			string newMusicPath = isAlphaNumeric(map.music);

			//Debug.Log(map.music);
			//Debug.Log(newMusicPath);

			if (newMusicPath != string.Empty)
			{
				Debug.LogWarning(map.music + " -> " + newMusicPath);
				string Audio = Path.Combine(path, newMusicPath);
				File.Move(map.AudioPath, Audio);
				map._music = newMusicPath;
				map.AudioPath = Audio;
				File.WriteAllText(Path.Combine(path, "meta.json"), JsonUtility.ToJson(map, true));
			}
		}
		foreach (string file in map.difficulties)
		{
			string diffJson = File.ReadAllText(Path.Combine(path, file));
			Map.Difficulty diff = JsonUtility.FromJson<Map.Difficulty>(diffJson);
			int i = -1;
			foreach (Map.Note note in diff.Notes)
			{
				i++;
				note.Index = i;
			}
			if (map.version >= 2)
			{
				var luaPath = Path.Combine(path, $"{Path.GetFileNameWithoutExtension(file)}.lua");
				if (File.Exists(luaPath))
					diff.LuaPath = luaPath;
			}
			map.Difficulties.Add(diff);
		}
		map.CanBePlayed = true;
		return map;
	}

	private static async void LoadCover(Map map)
	{
		if (map.CoverPath == null) return;
		Texture texture = null;
		using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + map.CoverPath))
		{
			uwr.SendWebRequest();

			// wrap tasks in try/catch, otherwise it'll fail silently
			try
			{
				while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

				if (uwr.result != UnityWebRequest.Result.Success) throw new Exception(uwr.error);
				else
				{
					texture = DownloadHandlerTexture.GetContent(uwr);
				}
			}
			catch (Exception err)
			{
				Debug.Log($"CoverArt Exception: {map.CoverPath} {err.Message}, {err.StackTrace}");
			}
		}

		if (texture != null)
		{
			map.Cover = texture;
		}
	}
	public static void ExtractMap(string path, string directory)
	{
		if (!File.Exists(path) || !Path.GetExtension(path).Equals(".zip"))
		{
			return;
		}
		ZipFile.ExtractToDirectory(path, Path.Combine(directory, Path.GetFileNameWithoutExtension(path)));
		File.Delete(path);
	}
	private static void LoadMaps()
	{
		foreach (string file in Directory.GetFiles(MapDirectory))
		{
			try { ExtractMap(file, MapDirectory); } catch { }

		}
		foreach (string dir in Directory.GetDirectories(MapDirectory))
		{
			try { Maps.Add(LoadMap(dir)); } catch { }
		}
	}
	public static void SetMapsDir(string directory)
	{
		MapDirectory = directory;
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		Task.Run(LoadMaps);
	}
}