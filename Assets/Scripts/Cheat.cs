using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Cheat : MonoBehaviour
{
	public GameObject GameCursor;
	public bool Enabled = false;
	public Vector3 lastNotePos = Vector3.zero;
	public Map.Note currentNote = null;
	public Vector3 startPos = Vector3.zero;
	public Vector3 lastDest = Vector3.zero;
	public Vector3 dest = Vector3.zero;
	public float startTime = 0f;
	public List<Map.Note> Notes;
	public float Crosses = 1;
	public float Radius = 1;
	public bool Direction = false;
	public bool Alternate = false;
	public bool Bounce = false;
	public void ResetCheat()
	{
		GameCursor = null;
		Enabled = false;
		lastNotePos = Vector3.zero;
		currentNote = null;
		startPos = Vector3.zero;
		dest = Vector3.zero;
		lastDest = Vector3.zero;
		startTime = 0f;
		Notes = null;
	}

	public void newNote()
	{
#if UNITY_EDITOR
		if (currentNote != null)
			lastNotePos = new Vector3(currentNote.X * 2f, currentNote.Y * 2f);
		currentNote = null;
		setNote();
#endif
	}

	public void setNote()
	{
#if UNITY_EDITOR
		if (Notes.Count <= 0) return;
		foreach (var note in Notes)
		{
			if (currentNote == null && note.Time > MapPlayer.RealTime)
			{
				currentNote = note;
			}
			else if (currentNote?.Time > note.Time && note.Time > MapPlayer.RealTime)
			{
				currentNote = note;
			}
		}
		lastDest = dest;
		startPos = GameCursor.transform.localPosition;
		if (currentNote == null) return;

		Vector2 notePos = new Vector2(currentNote.X * 2f, currentNote.Y * 2f);
		Vector2 pos = Vector2.zero;
		Vector2 maxX = new Vector2(-0.5f, 0.5f);
		Vector2 maxY = new Vector2(-0.5f, 0.5f);
		dest = Vector3.zero;

		dest.Set(notePos.x, notePos.y, 0);
		startTime = MapPlayer.RealTime;
		if (Alternate)
			Direction = !Direction;
#endif
	}
	private static Vector3 BounceCheck(Vector3 position)
	{
		float clamp = 3f - (0.525f / 2);
		while (Mathf.Abs(position.x) > clamp || Mathf.Abs(position.y) > clamp)
		{
			if (position.x > clamp)
			{
				position.x = clamp - (position.x - clamp);
			}
			else if (position.x < -clamp)
			{
				position.x = -clamp + (-position.x - clamp);
			}
			if (position.y > clamp)
			{
				position.y = clamp - (position.y - clamp);
			}
			else if (position.y < -clamp)
			{
				position.y = -clamp + (-position.y - clamp);
			}
		}
		return position;
	}
	public void Update()
	{
#if UNITY_EDITOR
		if (Input.GetKeyDown(KeyCode.C))
			Enabled = !Enabled;
		if (!Enabled) return;
		if (Input.GetKeyDown(KeyCode.UpArrow))
			Crosses += 0.5f;
		if (Input.GetKeyDown(KeyCode.DownArrow))
			Crosses -= 0.5f;
		if (Input.GetKeyDown(KeyCode.RightArrow))
			Radius += 0.25f;
		if (Input.GetKeyDown(KeyCode.LeftArrow))
			Radius -= 0.25f;
		if (Input.GetKeyDown(KeyCode.X))
			Alternate = !Alternate;
		if (Input.GetKeyDown(KeyCode.V))
			Direction = !Direction;
		if (Input.GetKeyDown(KeyCode.B))
			Bounce = !Bounce;
		if (currentNote == null)
		{
			setNote();
			if (currentNote == null) return;
		}
		var diff = Mathf.Clamp((MapPlayer.RealTime - startTime) / (currentNote.Time - startTime), 0, 1);
		var cursor = GameCursor.transform;
		var look = dest - lastDest;
		var mid = (lastDest + dest) / 2f;
		var up = (new Vector3(-look.y, look.x, 0) / 2) * Radius;
		if (Direction)
			up = -up;
		var p = mid + look * -Mathf.Cos(diff * Mathf.PI) / 2f;
		if (Crosses > 0)
			p += up * Mathf.Sin(diff * Mathf.PI * Crosses) / Crosses;
		if (Bounce)
			p = BounceCheck(p);
		cursor.localPosition = p;
#endif
	}
}
// CHEAT END