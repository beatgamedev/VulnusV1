using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ViewController : MonoBehaviour
{
	public ViewHandler Handler;
	void Awake()
	{
		gameObject.SetActive(false);
	}
	public virtual void Init()
	{
	}
	public virtual void Finish()
	{
	}
}
