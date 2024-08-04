using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Map
{
	public override string ToString()
	{
		base.ToString();
		return Artist.ToLower() + " - " + Title.ToLower() + " " + string.Join(", ", Mappers.ToArray()).ToLower();
	}
	public static int SupportedFormat { get; } = 2;
	[SerializeField]
	private int _version;
	public int version => _version;
	[SerializeField]
	private List<string> _difficulties;
	public List<string> difficulties => _difficulties;
	[SerializeField]
	private List<string> _mappers;
	public List<string> Mappers => _mappers;
	[SerializeField]
	private string _title;
	public string Title => _title;
	[SerializeField]
	private string _artist;
	public string Artist => _artist;
	[SerializeField]
	internal string _music;
	public string music => _music;
	[NonSerialized]
	public List<Difficulty> Difficulties = new List<Difficulty> { };
	[NonSerialized]
	public string AudioPath;
	[NonSerialized]
	public string CoverPath;
	[NonSerialized]
	public bool CanBePlayed;
	[NonSerialized]
	public Texture Cover;

	[Serializable]
	public class Difficulty
	{
		[SerializeField]
		private string _name;
		public string Name => _name;
		[SerializeField]
		private List<Note> _notes;
		public List<Note> Notes => _notes;
		[SerializeField]
		private float _approachDistance;
		public float ApproachDistance => _approachDistance;
		[SerializeField]
		private float _approachTime;
		public float ApproachTime => _approachTime;
		public string LuaPath;
	}
	[Serializable]
	public class Note
	{
		[SerializeField]
		private float _x;
		public float X => -_x;
		[SerializeField]
		private float _y;
		public float Y => _y;
		[SerializeField]
		private float _time;
		public float Time => _time;
		public int Index = 0;
	}
}