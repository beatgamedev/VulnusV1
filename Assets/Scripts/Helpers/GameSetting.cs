using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using UnityEngine.Events;

[Serializable]
public class Settings
{
	[SerializeField]
	public bool DebugEnabled = false;
	[SerializeField]
	public bool NoLua = false;
	[SerializeField]
	public ScoreType ScoreType = ScoreType.Default;
	[SerializeField]
	public CameraMode CameraMode = CameraMode.Spin;
	[SerializeField]
	public BloomType Bloom = BloomType.On;
	[SerializeField]
	public bool CursorTrail = false;
	[SerializeField]
	public bool DriftEnabled = true;
	[SerializeField]
	public bool Absolute = false;
	[SerializeField]
	public bool GridVisible = true;
	[SerializeField]
	public bool ConstantAR = true;
	[SerializeField]
	public double Offset = 0.028;
	[SerializeField]
	public double Sensitivity = 1.0;
	[SerializeField]
	public double ApproachDistance = 0.0;
	[SerializeField]
	public double ApproachTime = 0.0;
	[SerializeField]
	public bool MenuMusic = true;
	[SerializeField]
	public bool HitEnabled = true;
	[SerializeField]
	public bool MissEnabled = true;
	[SerializeField]
	public bool ParticlesEnabled = false;
	[SerializeField]
	public double MusicVolume = 0.50;
	[SerializeField]
	public double MenuVolume = 0.20;
	[SerializeField]
	public double HitVolume = 0.50;
	[SerializeField]
	public double MissVolume = 0.50;
	[SerializeField]
	public double DeathVolume = 0.50;
	[SerializeField]
	public double ClickVolume = 0.50;
	[SerializeField]
	public string CurrentCursor = "Default";
	[SerializeField]
	public string CurrentNote = "Default";
	[SerializeField]
	public string CurrentHit = "Default";
	[SerializeField]
	public string CurrentMiss = "Default";
	[SerializeField]
	public string CurrentDeath = "Default";
	[SerializeField]
	public ColorFormat CursorColor = new ColorFormat(255, 255, 255, 255);
	[SerializeField]
	public ColorFormat[] NoteColors = new ColorFormat[2] { new ColorFormat(0, 255, 255, 255), new ColorFormat(255, 0, 0, 255) };
	public override string ToString()
    {
		return JsonUtility.ToJson(this, true);
    }
}

[Serializable]
public class ColorFormat
{
	[SerializeField]
	public int R = 255;
	[SerializeField]
	public int G = 255;
	[SerializeField]
	public int B = 255;
	[SerializeField]
	public int A = 255;

	public ColorFormat(int r = 255, int g = 255, int b = 255, int a = 255)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}
	public ColorFormat(float r = 1f, float g = 1f, float b = 1f, float a = 1f)
	{
		R = (int)r * 255;
		G = (int)g * 255;
		B = (int)b * 255;
		A = (int)a * 255;
	}

	public Color ToRGBA()
	{
		return new Color((float)R / 255, (float)G / 255, (float)B / 255, (float)A / 255);
	}
}

public static class GameSetting
{
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
	public static readonly string path = Application.persistentDataPath;
#else
	public static readonly string path = Application.dataPath + "/..";
#endif
	public static bool DebugEnabled = false;
	public static bool NoLua = false;
	public static int ScoreType = 0;
	public static int CameraMode = 0;
	public static int Bloom = 0;
	public static bool CursorTrail = false;
	public static bool DriftEnabled = true;
	public static bool Absolute = false;
	public static bool GridVisible = true;
	public static double Offset = 0.028;
	public static bool ConstantAR = true;
	public static double Sensitivity = 1.0;
	public static double ApproachDistance = 0.0;
	public static double ApproachTime = 0.0;
	public static bool HitEnabled = true;
	public static bool MissEnabled = true;
	public static bool ParticlesEnabled = false;
	public static bool MenuMusic = true;
	public static double MusicVolume = 0.50;
	public static double MenuVolume = 0.20;
	public static double HitVolume = 0.50;
	public static double MissVolume = 0.50;
	public static double DeathVolume = 0.50;
	public static double ClickVolume = 0.50;
	public static string CurrentCursor = "Default";
	public static string CurrentNote = "Default";
	public static string CurrentHit = "Default";
	public static string CurrentMiss = "Default";
	public static string CurrentDeath = "Default";
	public static ColorFormat CursorColor = new ColorFormat(255, 255, 255, 255);
	public static ColorFormat[] NoteColors = new ColorFormat[2] { new ColorFormat(0, 255, 255, 255), new ColorFormat(255, 0, 0, 255) };
	public static Settings settings { get; private set; } = new Settings();

	private static FileSystemWatcher watcher;

	private static bool hasStarted = false;
	private static bool hasChanged = false;

	public static UnityEvent SettingsChanged { get; private set; }

	public static void Start()
    {
		if (hasStarted) return;
		SettingsChanged = new UnityEvent();
		SettingsChanged.AddListener(() => {
			SoundPlayer.Instance.bgMusic = MenuMusic;
		});
		hasStarted = true;
		watcher = new FileSystemWatcher(path, "settings.json");
		watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += Watcher_Changed;
		watcher.EnableRaisingEvents = true;
		LoadSettings();
	}

    private static void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
		OnFileChanged();
	}

    private static void OnFileChanged()
	{
		if (!hasChanged)
		{
			hasChanged = true;
			LoadSettings(true);
		}
		else
			hasChanged = false;
	}

	public static void SaveSettings()
	{
		hasChanged = true;
		settings.DebugEnabled = DebugEnabled;
		settings.NoLua = NoLua;
		settings.ScoreType = (ScoreType)ScoreType;
		settings.CameraMode = (CameraMode)CameraMode;
		settings.Bloom = (BloomType)Bloom;
		settings.DriftEnabled = DriftEnabled;
		settings.Absolute = Absolute;
		settings.CursorTrail = CursorTrail;
		settings.GridVisible = GridVisible;
		settings.Offset = Offset;
		settings.ConstantAR = ConstantAR;
		settings.Sensitivity = Sensitivity;
		settings.ApproachDistance = ApproachDistance;
		settings.ApproachTime = ApproachTime;
		settings.HitEnabled = HitEnabled;
		settings.MissEnabled = MissEnabled;
		settings.ParticlesEnabled = ParticlesEnabled;
		settings.MenuMusic = MenuMusic;
		settings.MusicVolume = MusicVolume;
		settings.MenuVolume = MenuVolume;
		settings.HitVolume = HitVolume;
		settings.MissVolume = MissVolume;
		settings.DeathVolume = DeathVolume;
		settings.ClickVolume = ClickVolume;
		settings.CurrentCursor = CurrentCursor;
		settings.CurrentNote = CurrentNote;
		settings.CurrentHit = CurrentHit;
		settings.CurrentMiss = CurrentMiss;
		settings.CurrentDeath = CurrentDeath;
		settings.CursorColor = CursorColor;
		settings.NoteColors = NoteColors;
		string save = JsonUtility.ToJson(settings, true);
		if (save != null)
		{
			File.WriteAllText(Path.Combine(path, "settings.json"), save);
		}
		else
		{
			Debug.Log("Something went wrong while saving");
		}
	}

	public static void LoadSettings(bool FireEvnt = false)
	{
		if (!hasChanged) FireEvnt = false; else hasChanged = false;
		if (!File.Exists(Path.Combine(path, "settings.json")))
		{
			settings.DebugEnabled = false;
			settings.NoLua = false;
			settings.ScoreType = 0;
			settings.CameraMode = 0;
			settings.Bloom = 0;
			settings.DriftEnabled = true;
			settings.Absolute = false;
			settings.CursorTrail = false;
			settings.GridVisible = true;
			settings.Offset = 0.028;
			settings.ConstantAR = true;
			settings.Sensitivity = 1.0;
			settings.ApproachDistance = 50.0;
			settings.ApproachTime = 1.0;
			settings.MenuMusic = true;
			settings.HitEnabled = true;
			settings.MissEnabled = true;
			settings.ParticlesEnabled = false;
			settings.MusicVolume = 0.50;
			settings.MenuVolume = 0.20;
			settings.HitVolume = 0.50;
			settings.MissVolume = 0.50;
			settings.DeathVolume = 0.50;
			settings.ClickVolume = 0.50;
			settings.CurrentCursor = "Default";
			settings.CurrentNote = "Default";
			settings.CurrentHit = "Default";
			settings.CurrentMiss = "Default";
			settings.CurrentDeath = "Default";
			settings.CursorColor = new ColorFormat(255, 255, 255, 255);
			settings.NoteColors = new ColorFormat[2] { new ColorFormat(0, 255, 255, 255), new ColorFormat(255, 0, 0, 255) };
			string save = JsonUtility.ToJson(settings, true);
			File.WriteAllText(Path.Combine(path, "settings.json"), save);
		}
		else
		{
			settings = JsonUtility.FromJson<Settings>(File.ReadAllText(Path.Combine(path, "settings.json")));
			DebugEnabled = settings.DebugEnabled;
			NoLua = settings.NoLua;
			ScoreType = (int)settings.ScoreType;
			CameraMode = (int)settings.CameraMode;
			Bloom = (int)settings.Bloom;
			CursorTrail = settings.CursorTrail;
			DriftEnabled = settings.DriftEnabled;
			Absolute = settings.Absolute;
			GridVisible = settings.GridVisible;
			Offset = settings.Offset;
			ConstantAR = settings.ConstantAR;
			Sensitivity = settings.Sensitivity;
			ApproachDistance = settings.ApproachDistance;
			ApproachTime = settings.ApproachTime;
			HitEnabled = settings.HitEnabled;
			MissEnabled = settings.MissEnabled;
			ParticlesEnabled = settings.ParticlesEnabled;
			MenuMusic = settings.MenuMusic;
			MusicVolume = settings.MusicVolume;
			MenuVolume = settings.MenuVolume;
			HitVolume = settings.HitVolume;
			MissVolume = settings.MissVolume;
			DeathVolume = settings.DeathVolume;
			ClickVolume = settings.ClickVolume;
			CurrentCursor = settings.CurrentCursor;
			CurrentNote = settings.CurrentNote;
			CurrentHit = settings.CurrentHit;
			CurrentMiss = settings.CurrentMiss;
			CurrentDeath = settings.CurrentDeath;
			CursorColor = settings.CursorColor;
			NoteColors = settings.NoteColors;
		}
		if (FireEvnt == true)
			SettingsChanged.Invoke();
	}

	/* // Old method
    public static int CameraMode = 0;
    public static float Sensitivity = 1f;
    public static bool HitEnabled = true;
    public static bool MissEnabled = true;

    // Load settings
    public static void LoadSettings()
    {
        Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 1f);
        CameraMode = PlayerPrefs.GetInt("CameraMode", 0);

        bool s1 = bool.TryParse(PlayerPrefs.GetString("HitEnabled", "true"), out bool hitRes);
        bool s2 = bool.TryParse(PlayerPrefs.GetString("MissEnabled", "true"), out bool missRes);

        if (!s1)
        {
            hitRes = true;
        }

        if (!s2)
        {
            missRes = true;
        }

        HitEnabled = hitRes;
        MissEnabled = missRes;
    }
    // Save settings
    public static void SaveSettings()
    {
        PlayerPrefs.SetFloat("Sensitivity", Sensitivity);
        PlayerPrefs.SetInt("CameraMode", CameraMode);
        PlayerPrefs.SetString("HitEnabled", HitEnabled.ToString());
        PlayerPrefs.SetString("MissEnabled", MissEnabled.ToString());
    }
    */
}

