using System;
using System.Reflection;
using System.Threading;
using System.IO;

public class DLLModManager
{
    public static bool DebugEnabled = GameSetting.DebugEnabled;
    public static AppDomain domain = AppDomain.CurrentDomain;
    private static bool AlreadyStarted = false;
    public static void Start()
    {
        if (AlreadyStarted) return;
        domain = AppDomain.CurrentDomain;
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
        string modsDir = Path.Combine(UnityEngine.Application.persistentDataPath, "mods");
#else
		string modsDir = Path.Combine(UnityEngine.Application.dataPath, "..", "mods");
#endif
        if (!Directory.Exists(modsDir))
        {
            Directory.CreateDirectory(modsDir);
        }

        foreach (string file in Directory.GetFiles(modsDir))
        {
            if (!File.Exists(file)) return;

            if (Path.GetExtension(file) == ".dll")
            {
                try
                {
                    //Assembly mod = domain.Load(File.ReadAllBytes(file));
                    Assembly mod = Assembly.LoadFile(file);

                    string init = "Init";

                    if (DebugEnabled)
                    {
                        init = "Init_D";
                    }

                    Type t = mod.GetType("Loader");
                    MethodInfo m = t.GetMethod(init);
                    m.Invoke(mod, null);

                    //mod.
                    //mod.GetModule("Loader").GetMethod(DebugEnabled ? "Init_D" : "Init").Invoke(mod, null);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }
        }
    }
}
