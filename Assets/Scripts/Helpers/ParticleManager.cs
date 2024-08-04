using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ParticleEvent : UnityEvent<string, Vector3, Gradient>
{

}

public class ParticleManager : MonoBehaviour
{
	private static ParticleManager _instance;
	public static ParticleManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GameObject.FindObjectOfType<ParticleManager>();
			}
			return _instance;
		}
	}
	public static GameObject Particles { get; set; }
	public static bool enableParticles = true;
	public static bool gamePaused = false;
	public static bool Paused { get; internal set; } = false;
	public static bool changeGridColor { get; internal set; } = true;
	public ParticleSystem HitParticle;
	public static List<ParticleSystem> HitParticles { get; internal set; }
	public static ParticleEvent particleEvent { get; internal set; } = new ParticleEvent();

	public static void ResetManager()
	{
		Color color = new Color
		{
			r = 1f,
			g = 0f,
			b = 0.415686275f,
			a = 1f
		};

		//var outline = GameLogic.Grid.GetComponent<Outline>();
		//outline.OutlineColor = color;

		for (int i = Particles.transform.childCount - 1; i >= 0; i--)
		{
			var child = Particles.transform.GetChild(i).gameObject;
			Destroy(child);
		}
	}

	public void StartManager()
	{
		Particles = GameObject.Find("Particles");
		HitParticles = new List<ParticleSystem>();
		particleEvent.AddListener((type, position, color) =>
		{
			var pos = position;
			pos.z = 0;

			if (type == "hit")
			{
				//var outline = GameLogic.Grid.GetComponent<Outline>();

				if (changeGridColor)
				{
					//outline.OutlineColor = color.colorKeys[0].color;
				}

				if (!enableParticles)
				{
					return;
				}
				var hit = Instantiate(HitParticle, pos, Quaternion.identity);

				var main = hit.main;
				var col = hit.colorOverLifetime;

				hit.transform.parent = Particles.transform;

				main.startColor = color;
				col.color = color;

				hit.Play();

				HitParticles.Add(hit);
				return;
			}
			else if (type == "miss")
			{
				return;
			}
		});
	}

	public static void Update()
	{
		if (!enableParticles)
		{
			return;
		}

		// if (gamePaused && !Paused)
		// {
		//     Paused = true;
		//     for (int i = HitParticles.Count - 1; i >= 0; i--)
		//     {
		//         var part = HitParticles[i];
		//         if (part)
		//         {
		//             part.Pause();
		//         }
		//         else
		//         {
		//             HitParticles.Remove(part);
		//             //part = null;
		//         }
		//     }
		// }
		// else if (!gamePaused && Paused)
		// {
		//     Paused = false;
		//     for (int i = HitParticles.Count - 1; i >= 0; i--)
		//     {
		//         var part = HitParticles[i];
		//         if (part)
		//         {
		//             part.Play();
		//         }
		//         else
		//         {
		//             HitParticles.Remove(part);
		//             //part = null;
		//         }
		//     }
		// }
	}
}