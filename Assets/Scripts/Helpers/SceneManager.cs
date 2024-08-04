using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SceneManager : MonoBehaviour
{
	private static int fading = 0;
	private static string nextScene = "";
	public GameObject Transition;
	private static CanvasGroup Fade;
	public void Start()
	{
		DontDestroyOnLoad(this.gameObject);
        Fade = Transition.GetComponent<CanvasGroup>();
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, loadSceneMode) =>
		{
			if (scene.name != nextScene)
				return;
			fading = 2;
			Fade.alpha = 1.2f;
		};
	}
	public void Update()
	{
		switch (fading)
		{
			case 1:
				{
					Fade.alpha += Time.deltaTime / 0.4f;
					if (Fade.alpha >= 1)
					{
                        Transition.SetActive(true);
                        fading = 0;
						Load();
					}
					break;
				}
			case 2:
				{
					Fade.alpha -= Time.deltaTime / 0.4f;
					if (Fade.alpha <= 0)
					{
                        Transition.SetActive(false);
                        fading = 0;
						Fade.alpha = 0;
					}
					break;
				}
			default:
				break;
		}
	}
	private static void Load()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(nextScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
	}
	public static void LoadScene(string scene)
	{
		nextScene = scene;
		fading = 1;
	}
}
