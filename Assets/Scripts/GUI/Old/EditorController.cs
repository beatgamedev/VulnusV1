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

public class EditorController : MonoBehaviour
{
    public GameObject Current;
	public GameObject MainMenu;
    public void Start()
    {
        if (!DiscordHandler.Initialized)
        {
            SceneManager.LoadScene("Startup");
            return;
        }
        long start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DiscordHandler.SetActivity((res) => { }, "In Editor", "Idle", start);

        if (SoundPlayer.Instance != null)
            SoundPlayer.Instance.Reset(false);
    }
    public void HoverEnter(Button btn)
    {
        if (btn == null) return;
        btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.26f);
    }
    public void HoverExit(Button btn)
    {
        if (btn == null) return;
        btn.gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.16f);
    }
    public void Exit()
    {
        SceneManager.LoadScene("Menu");
    }
}
