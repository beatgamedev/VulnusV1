#if UNITY_EDITOR
#define CHEAT_ENABLED
#endif
using Enums;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using Random = UnityEngine.Random;

public class Cheat : MonoBehaviour
{
#if CHEAT_ENABLED
    public CameraMode cameraMode { get => (CameraMode)GameSetting.CameraMode; }
    public GameCamera gameCamera;
    public GameObject noteObject;
    public GameObject NotesParent;
	public GameObject GameCursor;
    public bool Enabled = false;
	public Vector3 lastNotePos = Vector3.zero;
	public Map.Note currentNote = null;
	public Vector3 startPos = Vector3.zero;
	public Vector3 lastDest = Vector3.zero;
	public Vector3 dest = Vector3.zero;
	public float startTime = 0f;
	public List<Map.Note> Notes;
	public float cursorClamp = 3f - (0.525f / 2);
	public float Crosses = 1;
	public float Radius = 1;
	public bool Direction = false;
	public bool Alternate = false;
	public bool Bounce = false;
	public bool ShouldClamp = false;

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
		if (currentNote != null && noteObject != null)
			lastNotePos = new Vector3(noteObject.transform.position.x, noteObject.transform.position.y);
		currentNote = null;
		noteObject = null;

        setNote();
	}

	public void setNote()
	{
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
		startPos = GameCursor.transform.position;
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
    public static Quaternion LookAt(Vector3 sourcePoint, Vector3 destPoint)
    {
        Vector3 forwardVector = Vector3.Normalize(destPoint - sourcePoint);

        float dot = Vector3.Dot(Vector3.forward, forwardVector);

        if (Math.Abs(dot - (-1.0f)) < 0.000001f)
        {
            return new Quaternion(Vector3.up.x, Vector3.up.y, Vector3.up.z, 3.1415926535897932f);
        }
        if (Math.Abs(dot - (1.0f)) < 0.000001f)
        {
            return Quaternion.identity;
        }

        float rotAngle = (float)Math.Acos(dot);
        Vector3 rotAxis = Vector3.Cross(Vector3.forward, forwardVector);
        rotAxis = Vector3.Normalize(rotAxis);
        return CreateFromAxisAngle(rotAxis, rotAngle);
    }

    // just in case you need that function also
    public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
    {
        float halfAngle = angle * .5f;
        float s = (float)System.Math.Sin(halfAngle);
        Quaternion q;
        q.x = axis.x * s;
        q.y = axis.y * s;
        q.z = axis.z * s;
        q.w = (float)System.Math.Cos(halfAngle);
        return q;
    }
    public void Update()
	{
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
		if (Input.GetKeyDown(KeyCode.Y))
			ShouldClamp = !ShouldClamp;
		if (currentNote == null)
		{
			setNote();
			if (currentNote == null) return;
		}
		MapPlayer.Instance.Notes.TryGetValue(currentNote, out noteObject);
		if (noteObject == null) return;

        dest = new Vector3(noteObject.transform.position.x, noteObject.transform.position.y, 0);
        var diff = Mathf.Clamp((MapPlayer.RealTime - startTime) / (currentNote.Time - startTime), 0, 1);
		var speed = Time.deltaTime * float.Parse(GameSetting.ApproachDistance.ToString());
		var cursor = GameCursor.transform;
		var look = dest - cursor.position;
		var mid = (cursor.position + dest) / 2f;
		var up = (new Vector3(-look.y, look.x, 0) / 2) * Radius;
		var clampAmount = ShouldClamp ? cursorClamp : 1000f;

        if (Direction)
			up = -up;
		var p = mid + look * -Mathf.Cos(diff * Mathf.PI) / 2f;
		if (Crosses > 0)
			p += up * Mathf.Sin(diff * Mathf.PI * Crosses) / Crosses;
		if (Bounce)
			p = BounceCheck(p);
        p = new Vector3(Mathf.Clamp(p.x, -clampAmount, clampAmount), Mathf.Clamp(p.y, -clampAmount, clampAmount), 0);
		var cursorPos = Vector3.LerpUnclamped(startPos, p, ((MapPlayer.RealTime - startTime) / (currentNote.Time - startTime)) * (1 + speed));
		cursorPos = new Vector3(Mathf.Clamp(cursorPos.x, -clampAmount, clampAmount), Mathf.Clamp(cursorPos.y, -clampAmount, clampAmount));

        cursor.localPosition = cursorPos;

        if (cameraMode == CameraMode.Spin)
        {
			var np = cursorPos / 4f;
            Quaternion rot = LookAt(gameObject.transform.position, cursor.position);
            gameCamera.Pitch = Mathf.Rad2Deg * Mathf.Atan2(2 * rot.x * rot.w - 2 * rot.y * rot.z, 1 - 2 * rot.x * rot.x - 2 * rot.z * rot.z);
            gameCamera.Yaw = Mathf.Rad2Deg * Mathf.Atan2(2 * rot.y * rot.w - 2 * rot.x * rot.z, 1 - 2 * rot.y * rot.y - 2 * rot.z * rot.z);
			
			gameObject.transform.localRotation = Quaternion.Euler(gameCamera.Pitch, gameCamera.Yaw, 0);
            var fwd = gameObject.transform.forward;
            gameObject.transform.localPosition = new Vector3(np.x, np.y, -7f) - fwd / 2f;
        }
    }
#endif
}
// CHEAT END