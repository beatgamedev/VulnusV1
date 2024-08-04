using System;
using System.Collections.Generic;
using UnityEngine;
using PostProcessing = UnityEngine.Rendering.PostProcessing;
using MoonSharp.Interpreter;

public static class LuaProxies
{
	public class PostProcessVolume
	{
		PostProcessing.PostProcessVolume target;
		[MoonSharpHidden]
		public PostProcessVolume(PostProcessing.PostProcessVolume obj)
		{
			this.target = obj;
		}
		public float Vignette
		{
			get
			{
				return target.profile.GetSetting<PostProcessing.Vignette>().intensity;
			}
			set
			{
				if (value <= 0.05f)
				{
					target.profile.GetSetting<PostProcessing.Vignette>().intensity.overrideState = false;
					target.profile.GetSetting<PostProcessing.Vignette>().enabled.value = false;
				}
				else
				{
					target.profile.GetSetting<PostProcessing.Vignette>().intensity.overrideState = true;
					target.profile.GetSetting<PostProcessing.Vignette>().intensity.value = value;
					target.profile.GetSetting<PostProcessing.Vignette>().enabled.value = true;
				}
			}
		}
		public float Aberration
		{
			get
			{
				return target.profile.GetSetting<PostProcessing.ChromaticAberration>().intensity;
			}
			set
			{
				if (value <= 0.05f)
				{
					target.profile.GetSetting<PostProcessing.ChromaticAberration>().intensity.overrideState = false;
					target.profile.GetSetting<PostProcessing.ChromaticAberration>().enabled.value = false;
				}
				else
				{
					target.profile.GetSetting<PostProcessing.ChromaticAberration>().intensity.overrideState = true;
					target.profile.GetSetting<PostProcessing.ChromaticAberration>().intensity.value = value;
					target.profile.GetSetting<PostProcessing.ChromaticAberration>().enabled.value = true;
				}
			}
		}
		public float Distortion
		{
			get
			{
				return target.profile.GetSetting<PostProcessing.LensDistortion>().intensity;
			}
			set
			{
				if (value <= 0.05f)
				{
					target.profile.GetSetting<PostProcessing.LensDistortion>().intensity.overrideState = false;
					target.profile.GetSetting<PostProcessing.LensDistortion>().enabled.value = false;
				}
				else
				{
					target.profile.GetSetting<PostProcessing.LensDistortion>().intensity.overrideState = true;
					target.profile.GetSetting<PostProcessing.LensDistortion>().intensity.value = value;
					target.profile.GetSetting<PostProcessing.LensDistortion>().enabled.value = true;
				}
			}
		}
	}
}