#if UNITY_EDITOR
#define CHEAT_ENABLED
#endif
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using PostProcess = UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapPlayer : MonoBehaviour
{
#if CHEAT_ENABLED
    public static MapPlayer Instance;
    public Cheat Cheat;
#endif
    public PostProcess.PostProcessVolume postProcessVolume;
	public Texture CursorImage;
	public Texture Grid;
	public Texture NoGrid;
	public GameObject NotesParent;
	public GameObject TrailsParent;
	public GameObject ScoreUI;
	public GameObject TimeUI;
	public GameObject NotePrefab;
	public GameObject GameCursor;
	public static float RealTime = 0f;
	public static float SongTime = 0f;
	private static Map CurrentMap;
	private static Map.Difficulty CurrentDiff;
	private float AT = 0f;
	private float AD = 0f;
	private RawImage ImageObj;
	private LuaState lua;
	public static class Score
	{
		public static int Points = 0;
		public static int PointsNM = 0;
		public static int Hits = 0;
		public static int Total = 0;
		public static int Combo = 0;
		public static int HighestCombo = 0;
		public static int Multiplier = 0;
		public static int Miniplier = 0;
		public static float Health = 0;
	}
	public static class Mods
	{
		public static bool NoFail = false;
		public static float Speed = 1f;
		public static bool Ghost = false;
		public static bool VerticalFlip = false;
		public static bool HorizontalFlip = false;
		public static bool _360Notes = false;
	}
	public static bool SongFailedToLoad = false;
	private static bool Played;
	public static void Reset()
	{
		Score.Hits = 0;
		Score.Total = 0;
		Score.Points = 0;
		Score.PointsNM = 0;
		Score.Combo = 0;
		Score.HighestCombo = 0;
		Score.Multiplier = 1;
		Score.Miniplier = 0;
		Score.Health = 1;
		SongTime = 0f;
		Played = false;
		SongFailedToLoad = false;
		SoundPlayer.Instance.Reset(false);
		ParticleManager.ResetManager();
	}
	public static void RequestMap(Map map, int index)
	{
		CurrentMap = map;
		CurrentDiff = map.Difficulties[index];
	}
	private GameObject p1;
	private GameObject p2;
	private Pool<GameObject> NotePool;
	public Dictionary<Map.Note, GameObject> Notes;
	private int LastNote;
	private Pool<GameObject> Trails;
	private Color[] colors;
	private float RestartTimer;
	private float PauseTime;
	private float UnpauseTimer;
	private bool Unpausing;
	public bool Paused;
	public bool PauseEnabled = true;
	private bool CanSkip;
	public bool SkipEnabled = true;
	public void Awake()
	{
#if UNITY_EDITOR
        Instance = this;
#endif
		if (!DiscordHandler.Initialized)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Startup", UnityEngine.SceneManagement.LoadSceneMode.Single);
			return;
		}
		if (CurrentMap == null || CurrentDiff == null)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
			return;
		}
		ImageObj = GameCursor.transform.Find("Image").Find("Cursor").GetComponent<RawImage>();
		if (ResourceLoader.Instance.Note != null)
			NotePrefab = ResourceLoader.Instance.Note;
		if (ResourceLoader.Instance.HitSound != null)
			SoundPlayer.Instance.hit = ResourceLoader.Instance.HitSound;
		else
			SoundPlayer.Instance.hit = SoundPlayer.Instance.default_hit;
		if (ResourceLoader.Instance.MissSound != null)
			SoundPlayer.Instance.miss = ResourceLoader.Instance.MissSound;
		else
			SoundPlayer.Instance.death = SoundPlayer.Instance.default_miss;
		if (ResourceLoader.Instance.DeathSound != null)
			SoundPlayer.Instance.death = ResourceLoader.Instance.DeathSound;
		else
			SoundPlayer.Instance.death = SoundPlayer.Instance.default_death;
		if (ResourceLoader.Instance.Cursor != null)
			ImageObj.texture = ResourceLoader.Instance.Cursor;
		ImageObj.color = GameSetting.CursorColor.ToRGBA();

		PostProcess.Bloom bloom = postProcessVolume.profile.GetSetting<PostProcess.Bloom>();

		if (bloom != null)
		{
			Enums.BloomType BloomType = (Enums.BloomType)GameSetting.Bloom;
			switch (BloomType)
			{
				case Enums.BloomType.On:
					bloom.enabled.Override(true);
					bloom.fastMode.Override(false);
					break;
				case Enums.BloomType.Fast:
					bloom.enabled.Override(true);
					bloom.fastMode.Override(true);
					break;
				case Enums.BloomType.Off:
					bloom.enabled.Override(false);
					break;
				default:
					break;
			}
		}

		ParticleManager.Instance.StartManager();
		// AT = CurrentDiff.ApproachTime;
		// AD = CurrentDiff.ApproachDistance;
		long start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		DiscordHandler.SetActivity((res) => { }, CurrentDiff.Name + " - Playing", CurrentMap.Artist + " - " + CurrentMap.Title, start);
		Reset();
		p1 = ScoreUI.transform.Find("LeftPanel").gameObject;
		p2 = ScoreUI.transform.Find("RightPanel").gameObject;
		Texture currentGrid;
		if (GameSetting.GridVisible)
		{
			currentGrid = Grid;
		}
		else
		{
			currentGrid = NoGrid;
		}

		var HealthUI = ScoreUI.transform.Find("Health");

		if (Mods.NoFail)
			HealthUI.gameObject.SetActive(false);
		else
			HealthUI.gameObject.SetActive(true);

		ScoreUI.transform.Find("Grid").GetComponent<RawImage>().texture = currentGrid;
		Notes = new Dictionary<Map.Note, GameObject>();
		NotePool = new Pool<GameObject>((int index) =>
		{
			var obj = Instantiate(NotePrefab, NotesParent.transform);
			obj.name = index.ToString();
			obj.SetActive(false);
			return obj;
		},
		(obj) =>
		{
			Destroy(obj);
		}, (obj) =>
		{
			return Notes.ContainsValue(obj);
		}, 24);
		Trails = new Pool<GameObject>((int index) =>
		{
			var obj = Instantiate(GameCursor, TrailsParent.transform);
			obj.name = index.ToString();
			obj.SetActive(false);
			return obj;
		}, (obj) =>
		{
			Destroy(obj);
		}, (obj) =>
		{
			return obj.activeInHierarchy;
		}, 256);
		LastNote = 0;
		RestartTimer = 0;
		PauseTime = 0;
		UnpauseTimer = 0;
		Paused = false;
		Unpausing = false;
		CanSkip = false;

		//if (AT > 0)
		//	AT = GameSetting.ApproachTime;

		// if (GameSetting.ApproachDistance != 0)
		// {
		// var oldAD = AD;

		if (GameSetting.ApproachDistance != 0)
			AD = (float)GameSetting.ApproachDistance;
		else
			AD = CurrentDiff.ApproachDistance;

		if (GameSetting.ApproachTime <= 0f)
			AT = CurrentDiff.ApproachTime;
		else
			AT = (float)GameSetting.ApproachTime;

		AT *= Mods.Speed;
		SongTime = Mathf.Min(0, CurrentDiff.Notes.First<Map.Note>().Time - AT) - (1f / Mods.Speed);
		SoundPlayer.Instance.Load(CurrentMap.AudioPath, false, (float)GameSetting.MusicVolume, Mods.Speed);

		//GameSetting.ApproachTime*SoundPlayer.Instance.music.Pitch;
		//AT /= (oldAD / AD);
		// }

		float fogAD = (float)AD; // We dont need this actually Lol but just incase we do later on

		RenderSettings.fogEndDistance = 7.5f + (fogAD * 1.1f);
		RenderSettings.fogStartDistance = 7.5f + (fogAD * 0.4f);

		if (GameSetting.NoteColors.Length > 0) // || GameSetting.OverrideColors // Might need this for later formats???
		{
			colors = new Color[GameSetting.NoteColors.Length];

			for (int i = 0; i < GameSetting.NoteColors.Length; i++)
			{
				colors[i] = GameSetting.NoteColors[i].ToRGBA();
			}
		}
		else
		{
			colors = new Color[]{
				new Color(0,1,1,0),
				new Color(1,0,0,0)
			};
		}
		if (CurrentDiff.LuaPath != null)
		{
			Debug.Log("Map is a modchart!");
			if (!postProcessVolume.profile.HasSettings<PostProcess.Vignette>())
				postProcessVolume.profile.AddSettings<PostProcess.Vignette>();
			if (!postProcessVolume.profile.HasSettings<PostProcess.ChromaticAberration>())
				postProcessVolume.profile.AddSettings<PostProcess.ChromaticAberration>();
			if (!postProcessVolume.profile.HasSettings<PostProcess.LensDistortion>())
				postProcessVolume.profile.AddSettings<PostProcess.LensDistortion>();
			lua = new LuaState(this, File.ReadAllText(CurrentDiff.LuaPath));
			lua.Run();
		}
#if CHEAT_ENABLED
        // CHEAT
        Cheat.ResetCheat();
		Cheat.GameCursor = GameCursor;
		Cheat.NotesParent = NotesParent;
		Cheat.Notes = CurrentDiff.Notes;
		// CHEAT
#endif
	}
	internal async Task GetCursorImage(string path, RawImage img)
	{
		Texture texture = null;
		using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path))
		{
			uwr.SendWebRequest();

			// wrap tasks in try/catch, otherwise it'll fail silently
			try
			{
				while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

				if (uwr.result != UnityWebRequest.Result.Success) Debug.Log($"{uwr.error}");
				else
				{
					texture = DownloadHandlerTexture.GetContent(uwr);
				}
			}
			catch (Exception err)
			{
				Debug.Log($"{err.Message}, {err.StackTrace}");
			}
		}

		if (texture == null) texture = CursorImage;

		img.texture = texture;
	}
	void Update()
	{
		if (SongFailedToLoad)
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
		}

		if (SoundPlayer.Instance.music.Source.clip == null) { /*Debug.Log("Clip is null");*/ return; }
		try
		{
			var timeLine = TimeUI.transform.Find("TimeLine").GetComponent<TMPro.TextMeshProUGUI>();
			var progress = TimeUI.transform.Find("Bar").Find("Progress");

			TimeSpan timeSpan1 = TimeSpan.FromSeconds(SongTime / SoundPlayer.Instance.music.Pitch);
			TimeSpan timeSpan2 = TimeSpan.FromSeconds(SoundPlayer.Instance.music.Source.clip.length / SoundPlayer.Instance.music.Pitch);

			string timeText1 = string.Format("{0:D}:{1:D2}", timeSpan1.Minutes, timeSpan1.Seconds);
			string timeText2 = string.Format("{0:D}:{1:D2}", timeSpan2.Minutes, timeSpan2.Seconds);

			timeLine.text = timeText1.ToString() + " / " + timeText2.ToString();
			progress.localScale = new Vector3(SongTime / SoundPlayer.Instance.music.Source.clip.length, 1, 1);
		}
		catch (Exception e) { Debug.LogException(e); }

		ParticleManager.gamePaused = Paused;
		ParticleManager.Update();

		var paused = ScoreUI.transform.Find("Paused").gameObject;
		paused.SetActive(Paused);
		if (Paused)
		{
			paused.transform.Find("Progress").transform.localScale = new Vector3(UnpauseTimer / 0.5f, 1, 1);
		}
		var restart = ScoreUI.transform.Find("Restart").gameObject;
		restart.SetActive(RestartTimer > 0);
		if (RestartTimer > 0)
		{
			restart.transform.Find("Progress").transform.localScale = new Vector3(RestartTimer / 0.5f, 1, 1);
		}

		ScoreUI.transform.Find("Health").Find("Progress").transform.localScale = new Vector3(Score.Health, 1, 1);
		ScoreUI.transform.Find("CanSkip").gameObject.SetActive(CanSkip);
		p1.transform.Find("Combo").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Score.Combo}/{Score.HighestCombo}";
		p1.transform.Find("Hits").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Score.Hits}";
		p1.transform.Find("Miss").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Score.Total - Score.Hits}";
		p1.transform.Find("Score").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Score.Points}";
		if (Score.Total > 0) p2.transform.Find("Accuracy").GetComponent<TMPro.TextMeshProUGUI>().text = $"{Math.Floor((Score.PointsNM * 10000.0) / (Score.Total * 25.0)) / 100.0}%";
		p2.transform.Find("Multiplier").Find("Multiple").GetComponent<TMPro.TextMeshProUGUI>().text = $"x{Score.Multiplier}";
		p2.transform.Find("Multiplier").Find("Progress").transform.localScale = new Vector3(Score.Miniplier / 7f, 1, 1);

		if (!Played)
		{
			SongTime += Time.deltaTime * SoundPlayer.Instance.music.Pitch;
			if (SongTime > 0)
			{
				Played = true;
				SoundPlayer.Instance.music.Play();
				SoundPlayer.Instance.music.Time = SongTime;
			}
		}
		else if (!Paused)
		{
			SongTime = SoundPlayer.Instance.music.Time;
		}
		RealTime = SongTime + (float)GameSetting.Offset;

		for (int i = LastNote; i < CurrentDiff.Notes.Count; i++)
		{
			Map.Note self = CurrentDiff.Notes[i];
			if (!Notes.ContainsKey(self) && (self.Time - RealTime) < AT)
			{
				LastNote++;
				var newNote = NotePool.FirstUsable();
				newNote.transform.localRotation = Quaternion.identity;
				newNote.transform.Find("NoteCube").GetComponent<MeshRenderer>().material.color = colors[self.Index % colors.Length];
				Notes.Add(self, newNote);
				newNote.SetActive(true);
			}
		}
		foreach (Map.Note note in Notes.Keys)
		{
			var obj = Notes[note];
			var diff = note.Time - RealTime;

			float x = note.X;
			float y = note.Y;
			float z = AD * (diff / AT);

			z += 1.75f / 2f;

			if (Mods.VerticalFlip)
				y = -y;

			if (Mods.HorizontalFlip)
				x = -x;

			if (Mods.Ghost)
			{
				Color color = obj.transform.Find("NoteCube").GetComponent<Renderer>().material.color;
				var oColor = colors[note.Index % colors.Length];
				color.a = oColor.a * Mathf.Clamp((diff - 0.2f) / (0.1f * Mods.Speed), 0f, 1f);
				obj.transform.Find("NoteCube").GetComponent<Renderer>().material.color = color;
			}

			obj.transform.localPosition = new Vector3(x * 2f, y * 2f, z);
		}

		if (Mods._360Notes)
		{
			NotesParent.transform.localRotation = Quaternion.Euler(0, 0, RealTime * 36f);
		}
		if (lua != null)
		{
			lua.Update();
		}
		if (GameSetting.CursorTrail)
			UpdateTrail();
	}
	void FixedUpdate()
	{
		if (SoundPlayer.Instance.music.Source.clip == null) { /*Debug.Log("Clip is null");*/ return; }

		if (Score.Health < 0f && !Mods.NoFail)
		{
			End(true);
			return;
		}
		if (!Paused)
		{
			foreach (Map.Note note in CurrentDiff.Notes)
			{
				if (!Notes.ContainsKey(note)) continue;
				var obj = Notes[note];
				var diff = note.Time - RealTime;
				bool didMiss = false;
				bool didHit = false;
				var pos1 = GameCursor.transform.position;
				var pos2 = obj.transform.position;
				var posDiff = pos1 - pos2;
				var area = (1.75 + 0.525) / 2;
				bool touching = Mathf.Abs(posDiff.x) <= area && Mathf.Abs(posDiff.y) <= area;
				int baseScore = 25;

				switch ((Enums.ScoreType)GameSetting.ScoreType)
				{
					case Enums.ScoreType.Default:
						{
							bool canHit = diff <= 0;
							bool canMiss = diff < -(1.75 / 30);
							if (canMiss && !touching)
							{
								didMiss = true;
								break;
							}
							didHit = canHit && touching;
							break;
						}
					case Enums.ScoreType.Classic:
						{
							bool canHit = diff <= 0;
							bool canMiss = diff < -0.055;
							if (canMiss)
							{
								didMiss = true;
								break;
							}
							didHit = canHit && touching;
							break;
						}
					case Enums.ScoreType.HitError:
						{
							bool canHit = diff <= 0;
							bool canMiss = diff < -(1.75 / 30);
							if (canMiss && !touching)
							{
								didMiss = true;
								break;
							}
							didHit = canHit && touching;
							if (didHit)
							{
								baseScore -= (int)Math.Round((diff / -(1.75 / 30)) * 20);
							}
							break;
						}
					case Enums.ScoreType.HalfError:
						{
							bool canHit = diff <= 0;
							bool canMiss = diff < -(1.75 / 30);
							if (canMiss && !touching)
							{
								didMiss = true;
								break;
							}
							didHit = canHit && touching;
							if (didHit && diff < -(1.75 / 60))
							{
								baseScore -= (int)Math.Round((diff / -(1.75 / 30)) * 20);
							}
							break;
						}
					case Enums.ScoreType.Precise:
						{
							bool canHit = diff <= 0;
							if (canHit && !touching)
							{
								didMiss = true;
								break;
							}
							didHit = canHit && touching;
							break;
						}
					default:
						break;
				}
				if (didHit)
				{
					Score.Hits += 1;
					Score.Combo += 1;
					if (Score.Combo > Score.HighestCombo) Score.HighestCombo = Score.Combo;
					if (Score.Miniplier < 8 && Score.Multiplier < 8) Score.Miniplier += 1;
					if (Score.Miniplier == 8 && Score.Multiplier < 8)
					{
						Score.Miniplier = 0;
						Score.Multiplier = Mathf.Min(Score.Multiplier + 1, 8);
					}
					Score.Points += baseScore * Score.Multiplier;
					Score.PointsNM += baseScore;
					Score.Health = Mathf.Min(Score.Health + 1f / 8f, 1);
					NoteHit.Invoke(obj, baseScore * Score.Multiplier);
				}
				if (didMiss)
				{
					Score.Combo = 0;
					Score.Multiplier = Mathf.Max(1, Score.Multiplier - 1);
					Score.Miniplier = 0;
					Score.Health -= 1f / 5f;
					NoteMiss.Invoke(obj, 0);
				}
				if (didHit || didMiss)
				{
					obj.SetActive(false);
					Notes.Remove(note);
					Score.Total += 1;
#if CHEAT_ENABLED
                    if (Cheat.Enabled)
						Cheat.newNote();
#endif
				}
			}
			if (Played && !SoundPlayer.Instance.music.Source.isPlaying)
			{
				End(false);
				return;
			}
		}
	}
	private void LateUpdate()
	{
		if (!Paused && Input.GetKey(KeyCode.R))
		{
			RestartTimer += Time.deltaTime;
			if (RestartTimer >= 0.5f)
			{
				End(true);
			}
		}
		else
		{
			RestartTimer = 0;
		}

		if (SoundPlayer.Instance.music.Source.clip == null) { /*Debug.Log("Clip is null");*/ return; }

		bool canSkip = Notes.Count == 0;
		Map.Note skipTo = null;
		bool shouldSkip = SkipEnabled && RestartTimer == 0 && !Paused;
		if (canSkip && shouldSkip)
		{
			foreach (Map.Note note in CurrentDiff.Notes)
			{
				if (note.Time > RealTime)
				{
					if ((note.Time - RealTime) < (AT + 4f))
					{
						shouldSkip = false;
						break;
					}
					skipTo = note;
					break;
				}
			}
			shouldSkip = shouldSkip && (SongTime + 2f < SoundPlayer.Instance.music.Source.clip.length);
			if (shouldSkip && Input.GetKeyDown(KeyCode.Space))
			{
				if (skipTo != null)
				{
					SongTime = skipTo.Time - 1.5f;
					SoundPlayer.Instance.music.Time = skipTo.Time - 1.5f;
				}
				else
				{
					SongTime = SoundPlayer.Instance.music.Source.clip.length - 2f;
					SoundPlayer.Instance.music.Time = SoundPlayer.Instance.music.Source.clip.length - 2f;
				}
			}
		}
		CanSkip = canSkip && shouldSkip;
		bool canPause = PauseEnabled && RestartTimer == 0 && !Paused && !CanSkip && SongTime > 1f && (SongTime + 2f < SoundPlayer.Instance.music.Source.clip.length);
		if (canPause && Input.GetKeyDown(KeyCode.Space))
		{
			SoundPlayer.Instance.music.Pause();
			Paused = true;
			PauseTime = SongTime;
			UnpauseTimer = 0f;
			Unpausing = false;
		}
		else if (Paused && Input.GetKeyDown(KeyCode.Space))
		{
			Unpausing = true;
		}
		if (Paused && Unpausing)
		{
			Unpausing = Input.GetKey(KeyCode.Space);
			UnpauseTimer += Time.deltaTime;
			SongTime = PauseTime - ((0.5f - UnpauseTimer) * Mods.Speed);
		}
		else if (Paused)
		{
			UnpauseTimer = 0f;
		}
		if (Paused && UnpauseTimer > 0.5f)
		{
			Unpausing = false;
			UnpauseTimer = 0f;
			Paused = false;
			SoundPlayer.Instance.music.Time = PauseTime;
			SoundPlayer.Instance.music.Unpause();
		}
	}
	private Vector3 lastPos = Vector3.zero;
	private void UpdateTrail()
	{
		int toBeAdded = Mathf.FloorToInt((lastPos - GameCursor.transform.position).magnitude / 0.15f);
		for (int i = 0; i < toBeAdded; i++)
		{
			var diff = (float)i / (float)toBeAdded;
			var dt = Time.deltaTime * diff * 4f;
			var trail = Trails.FirstUsable();
			trail.transform.localPosition = Vector3.zero;
			trail.transform.position = Vector3.Lerp(GameCursor.transform.position, lastPos, diff);
			var img = trail.transform.Find("Image").Find("Cursor").GetComponent<RawImage>();
			img.color = new Color(img.color.r, img.color.g, img.color.b);
			img.color = img.color - new Color(0, 0, 0, dt * 2f);
			img.transform.localScale = Vector3.one;
			img.transform.localScale -= Vector3.one * dt;
			trail.transform.localPosition += new Vector3(0, 0, dt / 2f);
			trail.SetActive(true);
		}
		if (toBeAdded > 0)
			lastPos = GameCursor.transform.position;
		foreach (GameObject trail in Trails.Used())
		{
			var dt = Time.deltaTime * 4f;
			var img = trail.transform.Find("Image").Find("Cursor").GetComponent<RawImage>();
			img.color = img.color - new Color(0, 0, 0, dt * 2f);
			img.transform.localScale -= Vector3.one * dt;
			trail.transform.localPosition += new Vector3(0, 0, dt / 2f);
			if (trail.transform.localPosition.z > 0.25)
			{
				trail.SetActive(false);
			}
		}
	}

	private static void OnNoteHit(GameObject note, int score)
	{
		if (GameSetting.HitEnabled)
			SoundPlayer.Instance.Play("hit", (float)GameSetting.HitVolume, UnityEngine.Random.Range(0.95f, 1.05f));
		/**/
		if (GameSetting.ParticlesEnabled)
		{
			Renderer noteRenderer = note.transform.Find("NoteCube").GetComponent<Renderer>();
			var color = noteRenderer.material.color;
			Gradient gradient;
			GradientColorKey[] colorKey;
			GradientAlphaKey[] alphaKey;

			gradient = new Gradient();

			// Populate the color keys at the relative time 0 and 1 (0 and 100%)
			colorKey = new GradientColorKey[2];
			colorKey[0].color = color;
			colorKey[0].time = 0.0f;
			colorKey[1].color = color;
			colorKey[1].time = 1.0f;

			// Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
			alphaKey = new GradientAlphaKey[2];
			alphaKey[0].alpha = 1.0f;
			alphaKey[0].time = 0.0f;
			alphaKey[1].alpha = 0.0f;
			alphaKey[1].time = 1.0f;

			gradient.SetKeys(colorKey, alphaKey);
			ParticleManager.particleEvent.Invoke("hit", note.transform.position, gradient);
		}
		/**/
	}
	private static void OnNoteMiss(GameObject note, int score)
	{
		if (GameSetting.MissEnabled)
			SoundPlayer.Instance.Play("miss", (float)GameSetting.MissVolume, UnityEngine.Random.Range(0.95f, 1.05f));
	}

	public delegate void NoteEvent(GameObject note, int score);
	public NoteEvent NoteHit = new NoteEvent(OnNoteHit);
	public NoteEvent NoteMiss = new NoteEvent(OnNoteMiss);

    private void End(bool failed)
	{
		if (lua != null)
			lua.Kill();
		foreach (GameObject note in Notes.Values)
		{
			Destroy(note);
		}
		Notes.Clear();
		Reset();
		SoundPlayer.Instance.Reset(true);
		if (failed) SoundPlayer.Instance.Play("death", (float)GameSetting.DeathVolume);
		UnityEngine.SceneManagement.SceneManager.LoadScene("Menu", UnityEngine.SceneManagement.LoadSceneMode.Single);
	}
}