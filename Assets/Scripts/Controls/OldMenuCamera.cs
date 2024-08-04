using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldMenuCamera : MonoBehaviour
{
	internal float parallax = 0.25f;

	void Update()
	{
		if (UnityEngine.XR.XRSettings.enabled) return;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
#if !(UNITY_ANDROID || UNITY_IOS)
		var height = Screen.height;
		var width = Screen.width;
		var pos = Input.mousePosition;
		var newPos = new Vector3((pos.x / width) - parallax, (pos.y / width) - parallax, 0);
		this.gameObject.transform.localPosition = newPos * 0.75f;
		this.gameObject.transform.LookAt(newPos + Vector3.forward * 4);
#endif
	}
}
