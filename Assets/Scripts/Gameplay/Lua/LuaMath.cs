using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

public static class LuaMath
{
	public class Transform
	{
		public Vector3 Position = new Vector3();
		public Vector3 Rotation = new Vector3();
		public Vector3 Scale = new Vector3(1, 1, 1);
		public Transform Parent;
		[MoonSharpHidden]
		public void ApplyTo(UnityEngine.Transform transform, bool ignoreScale = false)
		{
			transform.localPosition = this.Position.vec3;
			transform.localRotation = Quaternion.Euler(this.Rotation.vec3);
			transform.localScale = this.Scale.vec3;
		}
	}
	public class Vector3
	{
		[MoonSharpHidden]
		public UnityEngine.Vector3 vec3 = new UnityEngine.Vector3();
		public float X { get { return vec3.x; } set { vec3.x = value; } }
		public float Y { get { return vec3.y; } set { vec3.y = value; } }
		public float Z { get { return vec3.y; } set { vec3.y = value; } }
		public Vector3() { }
		public Vector3(float x, float y, float z)
		{
			this.vec3 = new UnityEngine.Vector3(x, y, z);
		}
		public Vector3(UnityEngine.Vector3 vec3)
		{
			this.vec3 = vec3;
		}
		[MoonSharpUserDataMetamethod("__add")]
		public static Vector3 Add(Vector3 a, Vector3 b)
		{
			return new Vector3(a.vec3 + b.vec3);
		}
		[MoonSharpUserDataMetamethod("__sub")]
		public static Vector3 Sub(Vector3 a, Vector3 b)
		{
			return new Vector3(a.vec3 - b.vec3);
		}
		[MoonSharpUserDataMetamethod("__mul")]
		public static Vector3 Mul(Vector3 a, float b)
		{
			return new Vector3(a.vec3 * b);
		}
		[MoonSharpUserDataMetamethod("__div")]
		public static Vector3 Div(Vector3 a, float b)
		{
			return new Vector3(a.vec3 / b);
		}
	}
}
