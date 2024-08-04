using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewHandler : MonoBehaviour
{
	public ViewController StartView;
	private ViewController currentView;
	void Start()
	{
		SetCurrentView(StartView);
	}
	public void SetCurrentView(ViewController view)
	{
		var lastView = this.currentView;
		this.currentView = view;
		lastView?.Finish();
		lastView?.gameObject.SetActive(false);
		view.Init();
		view.gameObject.SetActive(true);
	}
}
