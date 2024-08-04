using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class GameCamera : MonoBehaviour
{
	public GameObject GameCursor;
	private float Pitch = 0f;
	private float Yaw = 0f;
	void Update()
	{
		CameraMode cameraMode = (CameraMode)GameSetting.CameraMode;

		if (UnityEngine.XR.XRSettings.enabled)
		{
			if (cameraMode == CameraMode.Spin)
			{
				cameraMode = CameraMode.Full;
			}
		}

		float sens = (float)GameSetting.Sensitivity;

		if (sens == 0)
		{
			sens = 0.1f;
		}

		Cursor.visible = false;
		if (GameSetting.Absolute)
		{
			Cursor.lockState = CursorLockMode.Confined;
			var x = Input.mousePosition.x - Screen.width / 2f;
			var y = Input.mousePosition.y - Screen.height / 2f;
			Pitch = 8f * (y / Screen.height);
			Yaw = 8f * (x / Screen.height);
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			var x = Input.GetAxisRaw("Mouse X") * 2f * sens;
			var y = Input.GetAxisRaw("Mouse Y") * 2f * sens;
			if (cameraMode != CameraMode.Spin)
			{
				x /= 8f;
				y /= -8f;
				if (GameSetting.DriftEnabled)
				{
					Pitch = GameCursor.transform.localPosition.y;
					Yaw = GameCursor.transform.localPosition.x;
				}
			}
			Yaw += x;
			Pitch -= y;
		}

		float clamp = 3f - (0.525f / 2);

		var p = GameCursor.transform.localPosition / 4f;

		switch (cameraMode)
		{
			case CameraMode.Spin:
				gameObject.transform.localRotation = Quaternion.Euler(Pitch, Yaw, 0);
				var fwd = gameObject.transform.forward;
				gameObject.transform.localPosition = new Vector3(p.x, p.y, -7f) - fwd / 2f;
				var cursorPos1 = transform.localPosition + new Vector3(fwd.x, fwd.y, -Mathf.Abs(fwd.z)) * Mathf.Abs(Mathf.Abs(transform.localPosition.z) / fwd.z);
				GameCursor.transform.localPosition = new Vector3(Mathf.Clamp(cursorPos1.x, -clamp, clamp), Mathf.Clamp(cursorPos1.y, -clamp, clamp), 0);
				break;
			case CameraMode.Parallax:
				gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
				var cursorPos2 = new Vector3(Yaw, Pitch, 0);
				GameCursor.transform.localPosition = new Vector3(Mathf.Clamp(cursorPos2.x, -clamp, clamp), Mathf.Clamp(cursorPos2.y, -clamp, clamp), 0);
				gameObject.transform.localPosition = new Vector3(p.x, p.y, -7.5f);
				break;
			default:
				gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
				var cursorPos3 = new Vector3(Yaw, Pitch, 0);
				GameCursor.transform.localPosition = new Vector3(Mathf.Clamp(cursorPos3.x, -clamp, clamp), Mathf.Clamp(cursorPos3.y, -clamp, clamp), 0);
				gameObject.transform.localPosition = new Vector3(0, 0, -7.5f);
				break;
		}
	}
}
