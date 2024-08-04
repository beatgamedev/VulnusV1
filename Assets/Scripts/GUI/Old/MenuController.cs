using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Enums;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using dynamicscroll;

public class MapListInfo
{
	public bool isEnabled = true;
	public Map map;
}

public class MapsFrame : DynamicScrollObject<MapListInfo>
{
	public override float CurrentHeight { get; set; }
	public override float CurrentWidth { get; set; }

	private MenuController controller;
	private Button btn;
	private RectTransform btnRect;
	private TextMeshProUGUI Song;
	private TextMeshProUGUI Mappers;
	private RawImage CoverArt;
	private MapButtonInfo info;

	public void Awake()
	{
		controller = GameObject.FindGameObjectWithTag("Canvas").GetComponent<MenuController>();

		CurrentHeight = GetComponent<RectTransform>().rect.height;
		CurrentWidth = GetComponent<RectTransform>().rect.width;

		btn = GetComponent<Button>();
		btnRect = GetComponent<RectTransform>();
		Song = transform.Find("Song").GetComponent<TextMeshProUGUI>();
		Mappers = transform.Find("Mappers").GetComponent<TextMeshProUGUI>();
		CoverArt = transform.Find("SongCoverArt").GetComponent<RawImage>();
		info = GetComponent<MapButtonInfo>();
		CoverArt.texture = controller.DefaultImage;
	}

	public override void UpdateScrollObject(MapListInfo item, int index)
	{
		Song.text = item.map.Artist + " - " + item.map.Title;
		Mappers.text = string.Join(", ", item.map.Mappers.ToArray());
		info.map = item.map; // set the map info

		if (btn == null)
		{
			Debug.LogError("Unable to find button");
			return;
		}

		try
		{
			btn.onClick.RemoveAllListeners();
		} catch { }

		btn.onClick.AddListener(() =>
		{
			controller.SelectMap(item.map);
		});

		if (info.map.Cover != null)
			CoverArt.texture = info.map.Cover; //GetCoverArt
		else
			CoverArt.texture = controller.DefaultImage;

		base.UpdateScrollObject(item, index);
	}
}

public class MenuController : MonoBehaviour
{
	public GameObject Current;
	public GameObject MainMenu;
	public GameObject MapsMenu;
	public GameObject ModsMenu;
	public GameObject SettingsMenu;
	private Transform settings_Main;
	private Transform settings_Colors;
	private Transform settings_Skins;
	public GameObject MapButtonPrefab;
	public GameObject DiffButtonPrefab;
	public GameObject SkinButtonPrefab;
	public Button lastDiffBtn;
	public Texture DefaultImage;
	public Map SelectedMap = null;
	public int SelectedDiff = 0;
	public DynamicScrollRect verticalScroll;
	private MapListInfo[] RealMapList = new MapListInfo[0];
	private MapListInfo[] MapList = new MapListInfo[0];
	private DynamicScroll<MapListInfo, MapsFrame> MapScroll = new DynamicScroll<MapListInfo, MapsFrame>();
	public bool MapInitialized = false;

	public void updateSettings()
    {
		print("UpdatingSettings...");
		if (SettingsMenu != null && settings_Main != null)
		{
			print(GameSetting.settings.ToString());
			// Main
			settings_Main.Find("Sensitivity").GetComponent<TMP_InputField>().text = LoadSetting("Sensitivity");
			settings_Main.Find("ApproachDistance").GetComponent<TMP_InputField>().text = LoadSetting("ApproachDistance");
			settings_Main.Find("ApproachTime").GetComponent<TMP_InputField>().text = LoadSetting("ApproachTime");
			settings_Main.Find("ApproachRate").GetComponent<TextMeshProUGUI>().text = LoadSetting("ApproachRate");
			//settings_Main.Find("ApproachRate").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("ApproachRate");
			settings_Main.Find("HitEnabled").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("HitEnabled");
			settings_Main.Find("MissEnabled").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("MissEnabled");
			settings_Main.Find("CameraMode").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("CameraMode");
			settings_Main.Find("DriftEnabled").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("DriftEnabled");
			settings_Main.Find("CursorTrail").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("CursorTrail");
			settings_Main.Find("GridVisible").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("GridVisible");
			settings_Main.Find("ScoreType").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("ScoreType");
			settings_Main.Find("Bloom").Find("Text").GetComponent<TextMeshProUGUI>().text = LoadSetting("Bloom");
			// Colors
		}
		else
			print("Unknown error updating settings\nContent not found?");
	}
	public void SendToSS()
    {
		Application.OpenURL("https://www.roblox.com/games/2677609345/");
    }

	private GameObject CursorSelected;
	private GameObject NoteSelected;
	private GameObject HitSelected;
	private GameObject MissSelected;
	private GameObject DeathSelected;

	private void SearchMap(string searchStr = null)
	{
		searchStr = searchStr.ToLower();
		bool isNull = string.IsNullOrEmpty(searchStr) || string.IsNullOrWhiteSpace(searchStr);
		print(isNull);
		print(searchStr);
		//List.Where(v=>v.Contains(bruh))
		var backup = RealMapList;
		var list = backup.ToList();
		if (!isNull)
		{
			RealMapList.ToList().ForEach(m =>
			{
				if (m != null && m.map != null)
				{
					string map = "";
					try
					{
						map = m.map.ToString();
					}
					catch (Exception)
					{
						isNull = true;
					}

					if (!map.Contains(searchStr))
					{
						list.Remove(m);
					}
				}
			});

			if (list.Count > 0)
				MapScroll.ChangeList(list.ToArray(), 0, true);
			else
				MapScroll.ChangeList(RealMapList, 0, true);
		}
		else
		{
			MapScroll.ChangeList(MapList, 0, true);
		}
	}

    public void ClickSkin(GameObject obj)
    {
		SkinButton info = obj.GetComponent<SkinButton>();
		if (info == null || info.Selected) return;
		info.Selected = true;

		obj.GetComponent<Image>().color = new Color(0.9019608f, 0.9019608f, 0.9019608f);

		switch (info.Type)
        {
			case SkinType.Cursor:
				if (CursorSelected != null)
				{
					CursorSelected.GetComponent<SkinButton>().Selected = false;
					CursorSelected.GetComponent<Image>().color = new Color(0.7058824f, 0.7058824f, 0.7058824f);
					CursorSelected = obj;
				}
				else
					CursorSelected = obj;

				ResourceLoader.Instance.Cursor = info.CursorAsset;
				GameSetting.CurrentCursor = info.Name;
				break;
			case SkinType.Mesh:
				if (NoteSelected != null)
				{
					NoteSelected.GetComponent<SkinButton>().Selected = false;
					NoteSelected.GetComponent<Image>().color = new Color(0.7058824f, 0.7058824f, 0.7058824f);
					NoteSelected = obj;
				}
				else
					NoteSelected = obj;

				ResourceLoader.Instance.Note = info.MeshAsset;
				GameSetting.CurrentNote = info.Name;
				break;
			case SkinType.HitSound:
				if (HitSelected != null)
				{
					HitSelected.GetComponent<SkinButton>().Selected = false;
					HitSelected.GetComponent<Image>().color = new Color(0.7058824f, 0.7058824f, 0.7058824f);
					HitSelected = obj;
				}
				else
					HitSelected = obj;

				ResourceLoader.Instance.HitSound = info.SoundAsset;
				GameSetting.CurrentHit = info.Name;
				break;
			case SkinType.MissSound:
				if (MissSelected != null)
				{
					MissSelected.GetComponent<SkinButton>().Selected = false;
					MissSelected.GetComponent<Image>().color = new Color(0.7058824f, 0.7058824f, 0.7058824f);
					MissSelected = obj;
				}
				else
					MissSelected = obj;

				ResourceLoader.Instance.MissSound = info.SoundAsset;
				GameSetting.CurrentMiss = info.Name;
				break;
			case SkinType.DeathSound:
				if (DeathSelected != null)
				{
					DeathSelected.GetComponent<SkinButton>().Selected = false;
					DeathSelected.GetComponent<Image>().color = new Color(0.7058824f, 0.7058824f, 0.7058824f);
					DeathSelected = obj;
				}
				else
					DeathSelected = obj;

				ResourceLoader.Instance.DeathSound = info.SoundAsset;
				GameSetting.CurrentDeath = info.Name;
				break;
			default:
				Debug.LogError("Unknown type");
				break;
		}

		GameSetting.SaveSettings();
	}

	public void Start()
	{
		if (!DiscordHandler.Initialized)
		{
			SceneManager.LoadScene("Startup");
			return;
		}
		
		GameSetting.SettingsChanged.AddListener(() => {
			updateSettings(); // Settings file changed
		});
		
		long start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		DiscordHandler.SetActivity((res) => { }, "In Menu", "", start);
		//Discord.Wrapper.SetActivity(MapMetadata.Metadata.Name + " by " + MapMetadata.Metadata.Artist, "Playing", start);
		
		if (MainMenu != null)
		{
			var Settings = MainMenu.transform.Find("Settings").GetComponentInChildren<Button>();
			var Editor = MainMenu.transform.Find("Editor").GetComponentInChildren<Button>();
			var Exit = MainMenu.transform.Find("Exit").GetComponent<Button>();

			Editor.onClick.AddListener(() =>
			{
#if UNITY_EDITOR
				SceneManager.LoadScene("Editor");
#else
				SoundPlayer.Instance.Play("death");
#endif
			});
			Exit.onClick.AddListener(() =>
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.ExitPlaymode();
#else
				Application.Quit();
#endif
			});
			Settings.onClick.AddListener(() =>
			{
				GameSetting.LoadSettings(); // Reload on loading settings just incase
			});
		}

		if (SettingsMenu != null)
		{
			/*
			var settingsContent = SettingsMenu.transform.Find("SettingsPage").Find("Viewport").Find("Content");
			var Menu = SettingsMenu.transform.Find("GoBack");
			var Sensitivity = settingsContent.Find("Sensitivity");
			var HitEnabled = settingsContent.Find("HitEnabled");
			var MissEnabled = settingsContent.Find("MissEnabled");
			var CameraModeBtn = settingsContent.Find("CameraMode");
			var DriftEnabled = settingsContent.Find("DriftEnabled");
			var CursorTrail = settingsContent.Find("CursorTrail");
			var GridVisible = settingsContent.Find("GridVisible");
			var ApproachRate = settingsContent.Find("ApproachRate").Find("Text (TMP)");
			var ApproachDistance = settingsContent.Find("ApproachDistance");
			var ApproachTime = settingsContent.Find("ApproachTime");
			*/
			settings_Main = SettingsMenu.transform.Find("SettingsPage");
			settings_Colors = SettingsMenu.transform.Find("ColorsPage");
			settings_Skins = SettingsMenu.transform.Find("SkinsPage");

			if (settings_Main == null)
				throw new Exception("Unable to find settings");

			var current = settings_Main;
			var Menu = SettingsMenu.transform.Find("GoBack");
			// Settings content
			var Colors = settings_Main.Find("Colors");
			var Skins = settings_Main.Find("Skins");
			var Sensitivity = settings_Main.Find("Sensitivity");
			var HitEnabled = settings_Main.Find("HitEnabled");
			var MissEnabled = settings_Main.Find("MissEnabled");
			var CameraModeBtn = settings_Main.Find("CameraMode");
			var DriftEnabled = settings_Main.Find("DriftEnabled");
			var CursorTrail = settings_Main.Find("CursorTrail");
			var GridVisible = settings_Main.Find("GridVisible");
			var ApproachRate = settings_Main.Find("ApproachRate");
			var ApproachDistance = settings_Main.Find("ApproachDistance");
			var ApproachTime = settings_Main.Find("ApproachTime");
			var ScoreTypeBtn = settings_Main.Find("ScoreType");
			var BloomBtn = settings_Main.Find("Bloom");
			// Colors content
			var ColorsBack = settings_Colors.Find("GoBack");
			// Skins content
			var SkinsBack = settings_Skins.Find("GoBack");
			var CursorMenu = settings_Skins.Find("Cursors").Find("List").Find("Viewport").Find("Content");
			var MeshMenu = settings_Skins.Find("Meshes").Find("List").Find("Viewport").Find("Content");
			var HitMenu = settings_Skins.Find("HitSounds").Find("List").Find("Viewport").Find("Content");
			var MissMenu = settings_Skins.Find("MissSounds").Find("List").Find("Viewport").Find("Content");
			var DeathMenu = settings_Skins.Find("DeathSounds").Find("List").Find("Viewport").Find("Content");

			// I know this is done badly (I think) But we can change it later (I just wanna get it working Lol)
			#region Load Resources
			// This is for checking if it has or hasn't selected stuff
			bool hasSelectedCursor = false;
			bool hasSelectedNote = false;
			bool hasSelectedHit = false;
			bool hasSelectedMiss = false;
			bool hasSelectedDeath = false;

			ResourceLoader.Instance.Cursors.ForEach(m =>
			{
				if (m == null) return;
				GameObject Btn = Instantiate(SkinButtonPrefab, CursorMenu);
				Btn.name = m.name;
				Btn.transform.Find("Cursor").gameObject.SetActive(true);
				Btn.transform.Find("Other").gameObject.SetActive(false);
				SkinButton info = Btn.AddComponent<SkinButton>();
				info.Type = SkinType.Cursor;
				info.Name = m.name;
				info.CursorAsset = m;
				info.Selected = (GameSetting.CurrentCursor == info.Name) || (string.IsNullOrEmpty(GameSetting.CurrentCursor) && info.name == "Default");

				if (info.Selected)
                {
					hasSelectedCursor = true;
					CursorSelected = Btn;
					Btn.GetComponent<Image>().color = new Color(0.9019608f, 0.9019608f, 0.9019608f);
					ResourceLoader.Instance.Cursor = info.CursorAsset;
				}

				Btn.transform.Find("Cursor").Find("Text").GetComponent<TMP_Text>().text = Path.GetFileNameWithoutExtension(info.Name);
				Btn.transform.Find("Cursor").Find("Cursor").GetComponent<RawImage>().texture = info.CursorAsset;
				Btn.GetComponent<Button>().onClick.AddListener(() => { ClickSkin(Btn); });
			});

			ResourceLoader.Instance.Notes.ForEach(m =>
			{
				if (m == null) return;
				GameObject Btn = Instantiate(SkinButtonPrefab, MeshMenu);
				SkinButton info = Btn.AddComponent<SkinButton>();
				Btn.name = m.name;
				info.Type = SkinType.Mesh;
				info.Name = m.name;
				info.MeshAsset = m;
				info.Selected = (GameSetting.CurrentNote == info.Name) || (string.IsNullOrEmpty(GameSetting.CurrentNote) && info.name == "Default");

				if (info.Selected)
				{
					hasSelectedNote = true;
					NoteSelected = Btn;
					Btn.GetComponent<Image>().color = new Color(0.9019608f, 0.9019608f, 0.9019608f);
					ResourceLoader.Instance.Note = info.MeshAsset;
				}

				Btn.transform.Find("Other").Find("Text").GetComponent<TMP_Text>().text = Path.GetFileNameWithoutExtension(info.Name);
				Btn.GetComponent<Button>().onClick.AddListener(() => { ClickSkin(Btn); });
			});

			ResourceLoader.Instance.Sounds.ForEach(m =>
			{
				if (m == null) return;
				#region HitSounds
				GameObject Btn1 = Instantiate(SkinButtonPrefab, HitMenu);
				SkinButton info1 = Btn1.AddComponent<SkinButton>();
				Btn1.name = m.name;
				info1.Type = SkinType.HitSound;
				info1.Name = m.name;
				info1.SoundAsset = m;
				info1.Selected = (GameSetting.CurrentHit == info1.Name) || (string.IsNullOrEmpty(GameSetting.CurrentHit) && info1.name == "Default Hit");

				if (info1.Selected)
				{
					hasSelectedHit = true;
					HitSelected = Btn1;
					Btn1.GetComponent<Image>().color = new Color(0.9019608f, 0.9019608f, 0.9019608f);
					ResourceLoader.Instance.HitSound = info1.SoundAsset;
				}

				Btn1.transform.Find("Other").Find("Text").GetComponent<TMP_Text>().text = Path.GetFileNameWithoutExtension(info1.Name);
				Btn1.GetComponent<Button>().onClick.AddListener(() => { ClickSkin(Btn1); });
				#endregion

				#region MissSounds
				if (m == null) return;
				GameObject Btn2 = Instantiate(SkinButtonPrefab, MissMenu);
				SkinButton info2 = Btn2.AddComponent<SkinButton>();
				Btn2.name = m.name;
				info2.Type = SkinType.MissSound;
				info2.Name = m.name;
				info2.SoundAsset = m;
				info2.Selected = (GameSetting.CurrentMiss == info2.Name) || (string.IsNullOrEmpty(GameSetting.CurrentMiss) && info2.name == "Default Miss");

				if (info2.Selected)
				{
					hasSelectedMiss = true;
					MissSelected = Btn2;
					Btn2.GetComponent<Image>().color = new Color(0.9019608f, 0.9019608f, 0.9019608f);
					ResourceLoader.Instance.MissSound = info2.SoundAsset;
				}

				Btn2.transform.Find("Other").Find("Text").GetComponent<TMP_Text>().text = Path.GetFileNameWithoutExtension(info2.Name);
				Btn2.GetComponent<Button>().onClick.AddListener(() => { ClickSkin(Btn2); });
				#endregion

				#region DeathSounds
				GameObject Btn3 = Instantiate(SkinButtonPrefab, DeathMenu);
				SkinButton info3 = Btn3.AddComponent<SkinButton>();
				Btn3.name = m.name;
				info3.Type = SkinType.DeathSound;
				info3.Name = m.name;
				info3.SoundAsset = m;
				info3.Selected = (GameSetting.CurrentDeath == info3.Name) || (string.IsNullOrEmpty(GameSetting.CurrentDeath) && info3.name == "Default Death");

				if (info3.Selected)
				{
					hasSelectedDeath = true;
					DeathSelected = Btn3;
					Btn3.GetComponent<Image>().color = new Color(0.9019608f, 0.9019608f, 0.9019608f);
					ResourceLoader.Instance.DeathSound = info3.SoundAsset;
				}

				Btn3.transform.Find("Other").Find("Text").GetComponent<TMP_Text>().text = Path.GetFileNameWithoutExtension(info3.Name);
				Btn3.GetComponent<Button>().onClick.AddListener(() => { ClickSkin(Btn3); });
                #endregion
            });

			// This is so we select default if none is selected!
			if (!hasSelectedCursor)
            {
				CursorSelected = CursorMenu.Find("Default").gameObject;
				CursorSelected.GetComponent<SkinButton>().Selected = true;
			}
			if (!hasSelectedNote)
			{
				NoteSelected = MeshMenu.Find("Default").gameObject;
				NoteSelected.GetComponent<SkinButton>().Selected = true;
			}
			if (!hasSelectedHit)
			{
				HitSelected = HitMenu.Find("Default Hit").gameObject;
				HitSelected.GetComponent<SkinButton>().Selected = true;
			}
			if (!hasSelectedMiss)
			{
				MissSelected = MissMenu.Find("Default Miss").gameObject;
				MissSelected.GetComponent<SkinButton>().Selected = true;
			}
			if (!hasSelectedDeath)
			{
				DeathSelected = DeathMenu.Find("Default Death").gameObject;
				DeathSelected.GetComponent<SkinButton>().Selected = true;
			}
			#endregion

			Colors.GetComponent<Button>().onClick.AddListener(() =>
			{
#if UNITY_EDITOR
				current.gameObject.SetActive(false);
				HoverSettingExit(Colors.GetComponent<Button>());
				current = settings_Colors;
				current.gameObject.SetActive(true);
#else
				SoundPlayer.Instance.Play("death");
#endif
			});
			ColorsBack.GetComponent<Button>().onClick.AddListener(() =>
			{
				current.gameObject.SetActive(false);
				HoverSettingExit(ColorsBack.GetComponent<Button>());
				current = settings_Main;
				current.gameObject.SetActive(true);
			});
			Skins.GetComponent<Button>().onClick.AddListener(() =>
			{
				current.gameObject.SetActive(false);
				HoverSettingExit(Skins.GetComponent<Button>());
				current = settings_Skins;
				current.gameObject.SetActive(true);
			});
			SkinsBack.GetComponent<Button>().onClick.AddListener(() =>
			{
				current.gameObject.SetActive(false);
				HoverSettingExit(SkinsBack.GetComponent<Button>());
				current = settings_Main;
				current.gameObject.SetActive(true);
			});
			Menu.GetComponent<Button>().onClick.AddListener(() =>
			{
				GameSetting.SaveSettings(); // Save settings after leaving UI
			});
			DriftEnabled.GetComponent<Button>().onClick.AddListener(() =>
			{
				SetSetting("DriftEnabled", (!GameSetting.DriftEnabled).ToString(), DriftEnabled.Find("Text").GetComponent<TextMeshProUGUI>());
			});
			ApproachDistance.GetComponent<TMP_InputField>().onEndEdit.AddListener((string val) =>
			{
				SetSetting("ApproachDistance", val.ToString(), ApproachDistance.GetComponent<TMP_InputField>());
				ApproachRate.GetComponent<TextMeshProUGUI>().text = LoadSetting("ApproachRate");
			});
			ApproachTime.GetComponent<TMP_InputField>().onEndEdit.AddListener((string val) =>
			{
				SetSetting("ApproachTime", val.ToString(), ApproachTime.GetComponent<TMP_InputField>());
				ApproachRate.GetComponent<TextMeshProUGUI>().text = LoadSetting("ApproachRate");
			});
			ApproachRate.GetComponent<TextMeshProUGUI>().text = LoadSetting("ApproachRate");
			CursorTrail.GetComponent<Button>().onClick.AddListener(() =>
			{
				SetSetting("CursorTrail", (!GameSetting.CursorTrail).ToString(), CursorTrail.Find("Text").GetComponent<TextMeshProUGUI>());
			});
			GridVisible.GetComponent<Button>().onClick.AddListener(() =>
			{
				SetSetting("GridVisible", (!GameSetting.GridVisible).ToString(), GridVisible.Find("Text").GetComponent<TextMeshProUGUI>());
			});
			Sensitivity.GetComponent<TMP_InputField>().onEndEdit.AddListener((string val) =>
			{
				SetSetting("Sensitivity", val.ToString(), Sensitivity.GetComponent<TMP_InputField>());
			});
			CameraModeBtn.GetComponent<Button>().onClick.AddListener(() =>
			{
				int mode = GameSetting.CameraMode + 1;
				if (mode > 2)
				{
					mode = 0;
				}

				SetSetting("CameraMode", mode.ToString(), CameraModeBtn.Find("Text").GetComponent<TextMeshProUGUI>());
			});
			HitEnabled.GetComponent<Button>().onClick.AddListener(() =>
			{
				SetSetting("HitEnabled", (!GameSetting.HitEnabled).ToString(), HitEnabled.Find("Text").GetComponent<TextMeshProUGUI>());
			});
			MissEnabled.GetComponent<Button>().onClick.AddListener(() =>
			{
				SetSetting("MissEnabled", (!GameSetting.MissEnabled).ToString(), MissEnabled.Find("Text").GetComponent<TextMeshProUGUI>());
			});
			ScoreTypeBtn.GetComponent<Button>().onClick.AddListener(() =>
			{
				int mode = GameSetting.ScoreType + 1;
				if (mode > 4)
				{
					mode = 0;
				}

				SetSetting("ScoreType", mode.ToString(), ScoreTypeBtn.Find("Text").GetComponent<TextMeshProUGUI>());
			});
			BloomBtn.GetComponent<Button>().onClick.AddListener(() =>
			{
				int mode = GameSetting.Bloom + 1;
				if (mode > 2)
				{
					mode = 0;
				}

				SetSetting("bloom", mode.ToString(), BloomBtn.Find("Text").GetComponent<TextMeshProUGUI>());
			});
		}

		if (ModsMenu != null)
		{
			var modContent = ModsMenu.transform.Find("Viewport").Find("Content");
			var NoFail = modContent.Find("NoFail");
			var Speed = modContent.Find("Speed");
			var Ghost = modContent.Find("Ghost");
			var VerticalFlip = modContent.Find("VerticalFlip");
			var HorizontalFlip = modContent.Find("HorizontalFlip");
			var _360Notes = modContent.Find("360Notes");

			NoFail.GetComponent<Button>().onClick.AddListener(() =>
			{
				MapPlayer.Mods.NoFail = !MapPlayer.Mods.NoFail;
				NoFail.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "No Fail: " + MapPlayer.Mods.NoFail;
			});
			Speed.Find("InputField (TMP)").GetComponent<TMP_InputField>().onEndEdit.AddListener((string val) =>
			{
				bool success = float.TryParse(val, out float speed);
				if (success)
				{
					speed /= 100f;
					speed = Mathf.Clamp(speed, 0.25f, 3f);
					Speed.Find("Slider").GetComponent<Slider>().value = speed;
					MapPlayer.Mods.Speed = speed;
				}

				Speed.Find("InputField (TMP)").GetComponent<TMP_InputField>().text = (MapPlayer.Mods.Speed * 100).ToString();
			});
			Speed.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener((float val) =>
			{
				MapPlayer.Mods.Speed = Mathf.Floor(val * 1000f) / 1000f;
				Speed.Find("InputField (TMP)").GetComponent<TMP_InputField>().text = (MapPlayer.Mods.Speed * 100).ToString();
			});
			Ghost.GetComponent<Button>().onClick.AddListener(() =>
			{
				MapPlayer.Mods.Ghost = !MapPlayer.Mods.Ghost;
				Ghost.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Ghost: " + MapPlayer.Mods.Ghost;
			});
			VerticalFlip.GetComponent<Button>().onClick.AddListener(() =>
			{
				MapPlayer.Mods.VerticalFlip = !MapPlayer.Mods.VerticalFlip;
				VerticalFlip.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Vertical Flip: " + MapPlayer.Mods.VerticalFlip;
			});
			HorizontalFlip.GetComponent<Button>().onClick.AddListener(() =>
			{
				MapPlayer.Mods.HorizontalFlip = !MapPlayer.Mods.HorizontalFlip;
				HorizontalFlip.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Horizontal Flip: " + MapPlayer.Mods.HorizontalFlip;
			});
			_360Notes.GetComponent<Button>().onClick.AddListener(() =>
			{
				MapPlayer.Mods._360Notes = !MapPlayer.Mods._360Notes;
				_360Notes.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "360 Notes: " + MapPlayer.Mods._360Notes;
			});
		}

		if (MapsMenu != null)
		{
			MapsMenu.transform.localPosition = Vector3.one * 1000;
			MapsMenu.SetActive(true);
			var Search = MapsMenu.transform.Find("Search");

			Search.GetComponent<TMP_InputField>().onValueChanged.AddListener((string searchStr) =>
			{
				SearchMap(searchStr);
			});
		}
		
		if (SoundPlayer.Instance != null)
			SoundPlayer.Instance.Reset(true);

		ReloadMaps(); // Reload on start???
	}
	public void SetSetting(string name, string value, TMP_InputField txt)
	{
		name = name.ToLower();
		value = value.ToLower();

		switch (name)
		{
			case "sensitivity":
				bool s1 = double.TryParse(value, out double sensitivity);

				if (!s1)
				{
					sensitivity = 1f; // Default
				}

				if (txt != null)
					txt.text = sensitivity.ToString();

				GameSetting.Sensitivity = sensitivity;
				break;
			case "approachdistance":
				bool s2 = double.TryParse(value, out double approachdistance);

				if (!s2)
				{
					approachdistance = 0f; // Default
				}

				if (txt != null)
					txt.text = approachdistance.ToString();

				GameSetting.ApproachDistance = approachdistance;
				break;
			case "approachtime":
				bool s3 = double.TryParse(value, out double approachtime);

				if (!s3)
				{
					approachtime = 0f; // Default
				}
				else
				{
					if (approachtime < 0f)
					{
						approachtime = -approachtime;
					}
				}

				if (txt != null)
					txt.text = approachtime.ToString();

				GameSetting.ApproachTime = approachtime;
				break;
			default: // So like this means there isnt a setting for this yet?
				Debug.Log("Unknown setting - " + name + ": " + value);
				break;
		}
	}
	public void SetSetting(string name, string value, TextMeshProUGUI txt)
	{
		name = name.ToLower();
		value = value.ToLower();

		switch (name)
		{
			case "hitenabled":
				bool s2 = bool.TryParse(value, out bool hitenabled);

				if (!s2)
				{
					hitenabled = true; // Default
				}

				if (txt != null)
					txt.text = hitenabled ? "ON" : "OFF";

				GameSetting.HitEnabled = hitenabled;
				break;
			case "missenabled":
				bool s3 = bool.TryParse(value, out bool missenabled);

				if (!s3)
				{
					missenabled = true; // Default
				}

				if (txt != null)
					txt.text = missenabled ? "ON" : "OFF";

				GameSetting.MissEnabled = missenabled;
				break;
			case "cameramode":
				bool s4 = int.TryParse(value, out int cameramode);
				if (!s4)
				{
					cameramode = 0; // Default
				}

				if (txt != null)
					txt.text = ((CameraMode)cameramode).ToString();

				GameSetting.CameraMode = cameramode;
				break;
			case "driftenabled":
				bool s5 = bool.TryParse(value, out bool driftenabled);
				if (!s5)
				{
					driftenabled = true; // Default
				}

				if (txt != null)
					txt.text = driftenabled ? "ON" : "OFF";

				GameSetting.DriftEnabled = driftenabled;
				break;
			case "cursortrail":
				bool s6 = bool.TryParse(value, out bool cursortrail);
				if (!s6)
				{
					cursortrail = true; // Default
				}

				if (txt != null)
					txt.text = cursortrail ? "ON" : "OFF";

				GameSetting.CursorTrail = cursortrail;
				break;
			case "gridvisible":
				bool s7 = bool.TryParse(value, out bool gridvisible);
				if (!s7)
				{
					gridvisible = true; // Default
				}

				if (txt != null)
					txt.text = gridvisible ? "ON" : "OFF";

				GameSetting.GridVisible = gridvisible;
				break;
			case "scoretype":
				bool s8 = int.TryParse(value, out int scoretype);
				if (!s8)
				{
					scoretype = 0; // Default
				}

				if (txt != null)
					txt.text = ((ScoreType)scoretype).ToString();

				GameSetting.ScoreType = scoretype;
				break;
			case "bloom":
				bool s9 = int.TryParse(value, out int bloom);
				if (!s9)
				{
					bloom = 0; // Default
				}

				if (txt != null)
					txt.text = ((BloomType)bloom).ToString();

				GameSetting.Bloom = bloom;
				break;
			default: // So like this means there isnt a setting for this yet?
				Debug.Log("Unknown setting - " + name + ": " + value);
				break;
		}
	}
	public string LoadSetting(string name)
	{
		name = name.ToLower();

		switch (name)
		{
			case "sensitivity":
				return GameSetting.Sensitivity.ToString();
			case "approachtime":
				return GameSetting.ApproachTime.ToString();
			case "approachdistance":
				return GameSetting.ApproachDistance.ToString();
			case "approachrate":
				var AD = GameSetting.ApproachDistance;
				var AT = GameSetting.ApproachTime;
				string rate = "N/A";

				if (AD != 0 && AT != 0)
				{
					rate = (AD / AT).ToString();
				}
				else
				{
					rate = "N/A";
				}

				return "Approach Rate: " + rate;
			case "hitenabled":
				return GameSetting.HitEnabled ? "ON" : "OFF";
			case "missenabled":
				return GameSetting.MissEnabled ? "ON" : "OFF";
			case "cameramode":
				return ((CameraMode)GameSetting.CameraMode).ToString();
			case "driftenabled":
				return GameSetting.DriftEnabled ? "ON" : "OFF";
			case "cursortrail":
				return GameSetting.CursorTrail ? "ON" : "OFF";
			case "gridvisible":
				return GameSetting.GridVisible ? "ON" : "OFF";
			case "scoretype":
				return ((ScoreType)GameSetting.ScoreType).ToString();
			case "bloom":
				return ((BloomType)GameSetting.Bloom).ToString();
			default: // So like this means there isnt a setting for this yet?
				Debug.Log("Unknown setting - " + name);
				return null;
		}
	}
	public void ReloadMaps(bool getNew = false)
	{
		if (MapsMenu == null) return; // MapsMenu is missing???
		if (!MapLoader.HasStarted) getNew = true; // Load maps

		SelectMap(null);

		GameObject Frame = MapsMenu.transform.Find("Maps").gameObject;
		GameObject bar = Frame.transform.Find("Scrollbar Vertical").gameObject;
		GameObject content = Frame.transform.Find("Viewport").Find("Content").gameObject;

		if (getNew)
		{
			try
			{
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
				string mapsDir = Path.Combine(Application.persistentDataPath, "maps");
#else
		string mapsDir = Path.Combine(Application.dataPath, "..", "maps");
#endif
				MapLoader.SetMapsDir(MapLoader.MapDirectory != "" ? MapLoader.MapDirectory : mapsDir);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		/* // This might break it?
		for (int i = 0; i < content.transform.childCount; i++)
		{
			var currentObj = content.transform.GetChild(i).gameObject;
			Destroy(currentObj); // Remove old obj so it doesn't add a billion objects
		}
		*/

		//MapList = new MapListInfo[0];
		var list = MapList.ToList();
		list.RemoveRange(0,list.Count);
		if (MapLoader.Maps.Count > 0)
		{
			MapLoader.Maps.ForEach(map =>
			{
				if (map != null && map.CanBePlayed)
				{
					MapListInfo info = new MapListInfo();
					info.isEnabled = true;
					info.map = map;
					list.Add(info);
				}
			});
            //RectTransform objRect = content.GetComponent<RectTransform>();
            //objRect.sizeDelta = new Vector2(objRect.sizeDelta.x, (MapButtonPrefab.transform.localScale.y + 5) * MapLoader.Maps.Length);
            #region Removed
            /* Removed due to new map list
			for (int i = 0; i < MapLoader.Maps.Count; i++)
			{
				Map map = MapLoader.Maps[i];

				if (map != null && map.CanBePlayed)
				{
					MapListInfo info = new MapListInfo();
					info.isEnabled = true;
					info.map = map;
					list.Add(info);
					/ *
					GameObject newButton = Instantiate(MapButtonPrefab);
					Button btn = newButton.GetComponent<Button>();
					RectTransform btnRect = newButton.GetComponent<RectTransform>();
					TextMeshProUGUI Song = newButton.transform.Find("Song").GetComponent<TextMeshProUGUI>();
					TextMeshProUGUI Mappers = newButton.transform.Find("Mappers").GetComponent<TextMeshProUGUI>();
					RawImage CoverArt = newButton.transform.Find("SongCoverArt").GetComponent<RawImage>();
					CoverArt.texture = DefaultImage;

					btn.onClick.AddListener(() =>
					{
						SelectMap(map);
					});

					if (map.CoverPath != null)
					{
						_ = GetCoverArt(map.CoverPath, CoverArt);
					}

					Song.text = map.Artist + " - " + map.Title;
					Mappers.text = string.Join(", ", map.Mappers.ToArray());
					newButton.GetComponent<MapButtonInfo>().map = info.map; // set the map info
					newButton.transform.SetParent(content.transform);
					btnRect.anchoredPosition3D = Vector3.zero;
					* /
				}
			}
			*/
            #endregion
        }
        else
		{
			Debug.Log("No maps found");
		}

		MapList = list.ToArray();
		RealMapList = MapList;

		MapScroll.spacing = 0f;

		if (!MapInitialized)
		{
			MapInitialized = true;
			MapScroll.Initiate(verticalScroll, MapList, 0, MapButtonPrefab);
		}
		else
        {
			var Search = MapsMenu.transform.Find("Search");
			SearchMap(Search.GetComponent<TMP_InputField>().text);
		}
	}
	/*
	internal async Task GetCoverArt(string path, RawImage img)
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

		if (texture == null) texture = DefaultImage;

		img.texture = texture;
	}
	*/
	public void SelectDifficulty(int diff)
	{
		if (SelectedMap != null && SelectedMap.difficulties.Count >= diff)
		{
			SelectedDiff = diff;
		}
	}
	public void SelectMap(Map map = null)
	{
		SelectedDiff = 0;
		SelectedMap = map;
		// Start setup map
		if (MapsMenu == null) return;

		if (SoundPlayer.Instance != null)
			SoundPlayer.Instance.Play("songclick", (float)GameSetting.ClickVolume);

		GameObject mapInfo = MapsMenu.transform.Find("MapInfo").gameObject;
		GameObject ScrollFrame = mapInfo.transform.Find("DifficultyList").gameObject;
		GameObject bar = ScrollFrame.transform.Find("Scrollbar Horizontal").gameObject;
		GameObject content = ScrollFrame.transform.Find("Viewport").Find("Content").gameObject;

		for (int i = 0; i < content.transform.childCount; i++)
		{
			var currentObj = content.transform.GetChild(i).gameObject;
			Destroy(currentObj); // Remove old obj so it doesn't add a billion objects
		}

		if (SelectedMap == null) // we need to clear the map info
		{
			mapInfo.SetActive(false);
			return;
		}
		else // We should now start on adding and setting up the map objects
		{
			mapInfo.SetActive(true);
			//RectTransform objRect = content.GetComponent<RectTransform>();
			//objRect.sizeDelta = new Vector2((MapButtonPrefab.transform.localScale.x) * SelectedMap.Difficulties.Count, objRect.sizeDelta.y);

			if (SelectedMap.Difficulties.Count > 0)
			{
				TextMeshProUGUI Song = mapInfo.transform.Find("Song").GetComponent<TextMeshProUGUI>();
				TextMeshProUGUI Mappers = mapInfo.transform.Find("Mappers").GetComponent<TextMeshProUGUI>();
				RawImage CoverArt = mapInfo.transform.Find("SongCoverArt").GetComponent<RawImage>();
				CoverArt.texture = DefaultImage;

				Song.text = SelectedMap.Artist + " - " + SelectedMap.Title;
				Mappers.text = string.Join(", ", SelectedMap.Mappers.ToArray());

				/*
				if (map.CoverPath != null)
				{
					_ = GetCoverArt(map.CoverPath, CoverArt);
				}
				*/

				if (map.Cover != null)
                {
					CoverArt.texture = map.Cover;
                }

				lastDiffBtn = null;
				for (int i = 0; i < SelectedMap.Difficulties.Count; i++)
				{
					Map.Difficulty diff = SelectedMap.Difficulties[i];
					if (diff != null && SelectedMap.CanBePlayed)
					{
						GameObject newButton = Instantiate(DiffButtonPrefab);
						Button btn = newButton.GetComponent<Button>();
						RectTransform btnRect = newButton.GetComponent<RectTransform>();
						TextMeshProUGUI diffName = newButton.transform.Find("DifficultyName").GetComponent<TextMeshProUGUI>();
						DifficultyButtonInfo btnInfo = newButton.GetComponent<DifficultyButtonInfo>();

						btnInfo.map = SelectedMap;
						btnInfo.difficulty = i;

						if (i == 0)
						{
							if (lastDiffBtn)
							{
								lastDiffBtn.transform.Find("DifficultyName").gameObject.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Normal | TMPro.FontStyles.UpperCase;
							}
							lastDiffBtn = btn;
							lastDiffBtn.transform.Find("DifficultyName").gameObject.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase;
							SelectDifficulty(i);
						}

						diffName.text = diff.Name;

						btn.onClick.AddListener(() =>
						{
							if (lastDiffBtn)
							{
								lastDiffBtn.transform.Find("DifficultyName").gameObject.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Normal | TMPro.FontStyles.UpperCase;
							}
							lastDiffBtn = btn;
							lastDiffBtn.transform.Find("DifficultyName").gameObject.GetComponent<TextMeshProUGUI>().fontStyle = TMPro.FontStyles.Bold | TMPro.FontStyles.UpperCase;
							SelectDifficulty(btnInfo.difficulty);
						});

						newButton.transform.SetParent(content.transform);
						btnRect.anchoredPosition3D = Vector3.zero;
					}
				}
			}
			else
			{
				Debug.Log("Difficulty list not found??\n" + SelectedMap.Artist + " - " + SelectedMap.Title + "\n" + string.Join(", ", SelectedMap.Mappers.ToArray()));
			}
		}
	}
	public void PlayMap()
	{
		Map map = SelectedMap;
		int Diff = SelectedDiff;

		if (map == null) return; // Invaild map
								 // Play Logic
		MapPlayer.RequestMap(map, Diff);
		SceneManager.LoadScene("Game");
	}
	public void ClickButton()
	{
		if (SoundPlayer.Instance == null) return;
		SoundPlayer.Instance.Play("click", (float)GameSetting.ClickVolume);
	}
	public void SwitchTo(GameObject next)
	{
		if (next.name.ToLower() == "settings")
		{
			updateSettings(); // Updates settings stuff
		}
		else if (next.name.ToLower() == "mapsmenu")
		{
			var modContent = next.transform.Find("Mods").Find("Viewport").Find("Content");
			if (modContent != null)
			{
				var NoFail = modContent.Find("NoFail");
				var Speed = modContent.Find("Speed");
				var Ghost = modContent.Find("Ghost");
				var VerticalFlip = modContent.Find("VerticalFlip");
				var HorizontalFlip = modContent.Find("HorizontalFlip");
				var _360Notes = modContent.Find("360Notes");
				NoFail.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "No Fail: " + MapPlayer.Mods.NoFail;
				Speed.Find("Slider").GetComponent<Slider>().value = MapPlayer.Mods.Speed;
				Speed.Find("InputField (TMP)").GetComponent<TMP_InputField>().text = (MapPlayer.Mods.Speed * 100).ToString();
				Ghost.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Ghost: " + MapPlayer.Mods.Ghost;
				VerticalFlip.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Vertical Flip: " + MapPlayer.Mods.VerticalFlip;
				HorizontalFlip.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "Horizontal Flip: " + MapPlayer.Mods.HorizontalFlip;
				_360Notes.Find("Text (TMP)").GetComponent<TextMeshProUGUI>().text = "360 Notes: " + MapPlayer.Mods._360Notes;
			}
		}
		if (Current.name.ToLower() == "mapsmenu")
		{
			Current.transform.localPosition = Vector3.one * 1000;
		}
		else
			Current.SetActive(false);

		if (next.name.ToLower() == "mapsmenu")
		{
			next.transform.localPosition = Vector3.zero;
		}
		else
			next.SetActive(true);

		this.Current = next;
	}
	public void HoverMapEnter(Button btn)
	{
		if (btn == null) return;
		btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.26f);
	}
	public void HoverSettingEnter(GameObject btn)
	{
		if (btn == null) return;
		btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.16f);
	}
	public void HoverSettingExit(GameObject btn)
	{
		if (btn == null) return;
		btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
	}
	public void HoverSettingEnter(Button btn)
	{
		if (btn == null) return;
		btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.16f);
	}
	public void HoverSettingExit(Button btn)
	{
		if (btn == null) return;
		btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.1f);
	}
	public void HoverEnter(Image img)
	{
		if (img == null) return;
		img.color = new Color(1, 1, 1, 0.26f);
	}
	public void HoverMapExit(Button btn)
	{
		if (btn == null) return;
		btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.16f);
	}
	public void HoverExit(Image img)
	{
		if (img == null) return;
		img.color = new Color(1, 1, 1, 0.16f);
	}
	public void HoverEnter(GameObject btn)
	{
		if (btn == null) return;
		Vector3 Pos = btn.transform.localPosition;
		btn.transform.localPosition = new Vector3(Pos.x, Pos.y, (Vector3.back * 10).z);
	}
	public void HoverEnter(Button btn)
	{
		if (btn == null) return;
		Vector3 Pos = btn.transform.localPosition;
		btn.transform.localPosition = new Vector3(Pos.x, Pos.y, (Vector3.back * 10).z);
	}
	public void HoverExit(GameObject btn)
	{
		if (btn == null) return;
		Vector3 Pos = btn.transform.localPosition;
		btn.transform.localPosition = new Vector3(Pos.x, Pos.y, 0);
	}
	public void HoverExit(Button btn)
	{
		if (btn == null) return;
		Vector3 Pos = btn.transform.localPosition;
		btn.transform.localPosition = new Vector3(Pos.x, Pos.y, 0);
	}
}