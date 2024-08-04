using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCamera : MonoBehaviour
{
	void Update()
	{
		if (UnityEngine.XR.XRSettings.enabled) return;
		Cursor.visible = true;
		Cursor.lockState = CursorLockMode.None;
#if !(UNITY_ANDROID || UNITY_IOS)
		var x = Input.mousePosition.x - Screen.width / 2f;
		var y = Input.mousePosition.y - Screen.height / 2f;
		var newPos = new Vector3((x / Screen.height) / 4f, 2 + (y / Screen.height) / 4f, 4);
		this.gameObject.transform.localPosition = new Vector3(0, 2, 0);
		this.gameObject.transform.LookAt(newPos);
#endif
	}
}
