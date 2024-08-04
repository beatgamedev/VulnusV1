using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Discord;
public class NetworkManager : MonoBehaviour
{
	public static bool InLobby = false;
	public static bool Joining = false;
	public static long LobbyId;
	public static string LobbySecret;
	public static Lobby CurrentLobby;
	public static LobbyType LobbyType;
	private static LobbyManager LobbyManager => DiscordHandler.LobbyManager;
	void Start()
	{
		// testing
		ManualAwake();
	}
	void ManualAwake()
	{
		Debug.Log("Networking init");
		if (!Joining)
		{
			LobbyType = LobbyType.Private;
			var txn = LobbyManager.GetLobbyCreateTransaction();
			txn.SetCapacity(12);
			txn.SetType(LobbyType);
			LobbyManager.CreateLobby(txn, (Result res, ref Lobby lobby) =>
			{
				InLobby = true;
				CurrentLobby = lobby;
				LobbyId = lobby.Id;
				LobbySecret = lobby.Secret;

				Debug.Log($"Created lobby {LobbyId}");
				DiscordHandler.SetActivity(res => { }, "Testing Multiplayer");
			});
		}
		else
		{
			LobbyManager.ConnectLobby(LobbyId, LobbySecret, (Result res, ref Lobby lobby) =>
			{
				InLobby = true;
				CurrentLobby = lobby;
				Debug.Log($"Joined lobby {LobbyId}");
				DiscordHandler.SetActivity(res => { }, "Testing Multiplayer");
			});
		}
	}
	void OnDestroy()
	{
		if (!InLobby) return;
		if (CurrentLobby.OwnerId == DiscordHandler.Client.GetUserManager().GetCurrentUser().Id)
		{
			var users = LobbyManager.GetMemberUsers(CurrentLobby.Id);
			if (!Joining)
			{
				LobbyManager.DeleteLobby(CurrentLobby.Id, (Result res) => { });
			}
			else
			{
				LobbyManager.DisconnectLobby(CurrentLobby.Id, (Result res) => { });
			}
		}
	}
}
