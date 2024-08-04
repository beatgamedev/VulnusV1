using System;
using UnityEngine;
using Discord;
using System.IO;

public class DiscordHandler : MonoBehaviour
{
	public static bool Initialized
	{
		get; private set;
	} = false;
	public static bool DiscordRunning
	{
		get; private set;
	} = false;
	private static Discord.User? currentUser;
	public static Discord.User? User => currentUser;
	private static Discord.Discord discord;
	public static Discord.Discord Client => discord;
	private static ActivityManager activityManager;
	public static ActivityManager ActivityManager => activityManager;
	private static LobbyManager lobbyManager;
	public static LobbyManager LobbyManager => lobbyManager;
	private static UserManager userManager;
	public static UserManager UserManager => userManager;
	private static readonly long clientId = 954573999088730154;
	private static DiscordHandler _instance;
	public static DiscordHandler Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<DiscordHandler>();
			}
			return _instance;
		}
	}
	private void Awake()
	{
		if (_instance != null)
		{
			Debug.LogError("Multiple DiscordHandlers have been created. Something is very wrong!");
			Destroy(this.gameObject);
			return;
		}
		_instance = this;
		DontDestroyOnLoad(this.gameObject);
	}
	private void Start()
	{
		try
		{
			discord = new Discord.Discord(clientId, (ulong)Discord.CreateFlags.NoRequireDiscord);
			try { discord.RunCallbacks(); DiscordRunning = true; } catch (Exception e) { throw e; }
			userManager = discord.GetUserManager();
			userManager.OnCurrentUserUpdate += () =>
			{
				currentUser = userManager.GetCurrentUser();
			};
			lobbyManager = discord.GetLobbyManager();
			activityManager = discord.GetActivityManager();
			activityManager.RegisterCommand(Path.Combine(Application.dataPath, "../Vulnus.exe"));
			SetActivity((res) =>
			{
				Debug.Log(res);
				if (res != Discord.Result.Ok)
				{
					Debug.Log("Discord isn't working right now");
					DiscordRunning = false;
				}
				else
				{
					Debug.Log("Discord is working");
				}
			});
		}
		catch (Exception e)
		{
			Debug.LogError(e.Message);
			DiscordRunning = false;
		}
		Initialized = true;
	}
	private void FixedUpdate()
	{
		if (DiscordRunning)
		{
			try
			{
				discord.RunCallbacks();
			}
			catch
			{
				DiscordRunning = false;
			}
		}
	}
	private void OnDestroy()
	{
		DiscordRunning = false; // just in case it loads before fully disposed
		Debug.Log("Disposing Discord object");
		discord.Dispose();
	}
	public static void SetActivity(ActivityManager.UpdateActivityHandler callback, string state = "Loading", string details = "", long? startTimestamp = null, long? endTimestamp = null)
	{
		try
		{
			if (DiscordRunning)
			{
				var activity = new Activity();
				activity.Assets.LargeImage = "vulnus";
				activity.Assets.LargeText = "Vulnus";
				activity.Assets.SmallImage = "beatgamedev";
				if (startTimestamp != null) activity.Timestamps.Start = (long)startTimestamp;
				if (endTimestamp != null) activity.Timestamps.End = (long)endTimestamp;
				activity.State = state;
				activity.Details = details;

				if (NetworkManager.InLobby)
                {
					ActivityPartyPrivacy Privacy;
					if (NetworkManager.LobbyType == LobbyType.Private)
						Privacy = ActivityPartyPrivacy.Private;
					else
						Privacy = ActivityPartyPrivacy.Public;

					activity.Party.Id = NetworkManager.LobbyId.ToString();
					activity.Party.Privacy = Privacy;
					activity.Party.Size.MaxSize = (int)NetworkManager.CurrentLobby.Capacity;
					activity.Party.Size.CurrentSize = lobbyManager.MemberCount(NetworkManager.LobbyId);
					activity.Secrets.Join = NetworkManager.LobbySecret;

					print(activity.Party.Privacy);
				}

				activityManager.UpdateActivity(activity, callback);
			}
		}
		catch { }
	}
}