using System;
using System.Collections.Generic;
public class Pool<ObjectType>
{
	public ObjectType[] Objects { get; private set; }
	public Func<ObjectType, bool> InUse { get; private set; }
	private Func<int, ObjectType> Init;
	private Action<ObjectType> Destroyer;
	public Pool(Func<int, ObjectType> initialiser, Action<ObjectType> destroyer, Func<ObjectType, bool> usedCheck, int defaultSize = 0)
	{
		this.InUse = usedCheck;
		this.Objects = new ObjectType[defaultSize];
		this.Init = initialiser;
		this.Destroyer = destroyer;
		if (defaultSize > 0)
		{
			for (int i = 0; i < defaultSize; i++)
			{
				this.Objects[i] = this.Init(i);
			}
		}
	}
	public ObjectType[] Used()
	{
		List<ObjectType> objs = new List<ObjectType>();
		foreach (ObjectType obj in Objects)
		{
			if (InUse(obj))
			{
				objs.Add(obj);
			}
		}
		return objs.ToArray();
	}
	public ObjectType[] Usable()
	{
		List<ObjectType> objs = new List<ObjectType>();
		foreach (ObjectType obj in Objects)
		{
			if (!InUse(obj))
			{
				objs.Add(obj);
			}
		}
		return objs.ToArray();
	}
	public ObjectType FirstUsable()
	{
		foreach (ObjectType obj in Objects)
		{
			if (!InUse(obj))
			{
				return obj;
			}
		}
		return CreateNewObject();
	}
	private ObjectType CreateNewObject()
	{
		var obj = Init(Objects.Length);
		var newArray = new ObjectType[Objects.Length + 1];
		newArray[0] = obj;
		var oldArray = Objects;
		oldArray.CopyTo(newArray, 1);
		this.Objects = newArray;
		return obj;
	}
	~Pool()
	{
		foreach (ObjectType obj in Objects)
		{
			Destroyer(obj);
		}
	}
}