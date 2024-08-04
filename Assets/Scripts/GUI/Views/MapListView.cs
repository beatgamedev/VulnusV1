using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapListView : ViewController
{
	public GameObject MapButton;
	public GameObject MapList;
	private static int currentPage;
	private static List<Map> mapList;
	private Dictionary<GameObject, Map> maps = new Dictionary<GameObject, Map>();
	private Pool<GameObject> mapButtons;
	void Start()
	{
		if (mapList == null)
			mapList = new List<Map>(MapLoader.Maps);
		MapButton.SetActive(false);
		mapButtons = new Pool<GameObject>((int index) =>
		{
			var obj = Instantiate(MapButton);
			obj.transform.SetParent(MapList.transform);
			obj.GetComponent<Button>().onClick.AddListener(() =>
			{
				if (maps.ContainsKey(obj))
					mapSelected(maps[obj]);
			});
			return obj;
		}, (GameObject obj) =>
		{
			Destroy(obj);
		}, (GameObject obj) =>
		{
			return obj.activeSelf;
		}, 16);
	}
	void Update()
	{

	}
	public void SearchUpdated()
	{

	}
	private void mapSelected(Map map)
	{

	}
}
