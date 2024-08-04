using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceLoader : MonoBehaviour
{
    private AsyncOperationHandle<GameObject> noteHandle;
    private static ResourceLoader _instance;
    public static ResourceLoader Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<ResourceLoader>();
            }
            return _instance;
        }
    }

    public GameObject default_Note;
    public Texture default_Cursor;

    public Material NoteMat;

    public GameObject Note;
    public Texture Cursor;
    public AudioClip HitSound;
    public AudioClip MissSound;
    public AudioClip DeathSound;

    private string mainPath;
    private string notePath;
    private string cursorPath;
    private string soundPath;
    //private string hitSoundPath;
    //private string missSoundPath;
    //private string deathSoundPath;

    private string[] _Notes;
    private string[] _Cursors;
    private string[] _Sounds;
    //private string[] _HitSounds;
    //private string[] _MissSounds;
    //private string[] _DeathSounds;

    public List<GameObject> Notes;
    public List<Texture> Cursors;
    public List<AudioClip> Sounds;
    //public List<AudioClip> HitSounds;
    //public List<AudioClip> MissSounds;
    //public List<AudioClip> DeathSounds;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError("Multiple ResourceLoaders have been created. Something is very wrong!");
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        Reload();
    }
    async void Reload()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_OSX || UNITY_ANDROID || UNITY_IOS
        mainPath = Path.Combine(Application.persistentDataPath, "assets");
#else
		mainPath = Path.Combine(Application.dataPath, "..", "assets");
#endif
        notePath = Path.Combine(mainPath, "Notes");
        cursorPath = Path.Combine(mainPath, "Cursors");
        soundPath = Path.Combine(mainPath, "Sounds");
        //hitSoundPath = Path.Combine(soundPath, "Hit");
        //missSoundPath = Path.Combine(soundPath, "Miss");
        //deathSoundPath = Path.Combine(soundPath, "Death");

        if (!Directory.Exists(mainPath))
            Directory.CreateDirectory(mainPath);
        if (!Directory.Exists(notePath))
            Directory.CreateDirectory(notePath);
        if (!Directory.Exists(cursorPath))
            Directory.CreateDirectory(cursorPath);
        if (!Directory.Exists(soundPath))
            Directory.CreateDirectory(soundPath);
        //if (!Directory.Exists(hitSoundPath))
        //    Directory.CreateDirectory(hitSoundPath);
        //if (!Directory.Exists(missSoundPath))
        //    Directory.CreateDirectory(missSoundPath);
        //if (!Directory.Exists(deathSoundPath))
        //    Directory.CreateDirectory(deathSoundPath);

        _Notes = Directory.GetFiles(notePath, "*.note");
        _Cursors = Directory.GetFiles(cursorPath, "*");
        _Sounds = Directory.GetFiles(soundPath, "*");
        //_HitSounds = Directory.GetFiles(hitSoundPath, "*");
        //_MissSounds = Directory.GetFiles(missSoundPath, "*");
        //_DeathSounds = Directory.GetFiles(deathSoundPath, "*");

        #region Generate Notes/Cursors/Sounds
        var cursorList = Cursors;
        var noteList = Notes;
        var soundList = Sounds;
        //var hitList = HitSounds;
        //var missList = MissSounds;
        //var deathList = DeathSounds;

        default_Cursor.name = "Default";
        default_Note.name = "Default";
        SoundPlayer.Instance.default_hit.name = "Default Hit";
        SoundPlayer.Instance.default_miss.name = "Default Miss";
        SoundPlayer.Instance.default_death.name = "Default Death";

        cursorList.Add(default_Cursor);
        noteList.Add(default_Note);
        soundList.Add(SoundPlayer.Instance.default_hit);
        soundList.Add(SoundPlayer.Instance.default_miss);
        soundList.Add(SoundPlayer.Instance.default_death);
        //hitList.Add(SoundPlayer.Instance.default_hit);
        //missList.Add(SoundPlayer.Instance.default_miss);
        //deathList.Add(SoundPlayer.Instance.default_death);

        #region Removed (Broken?)
        /*
        if (_Notes.Length > 0)
        {
            foreach (string path in _Notes)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var name = Path.GetFileName(path);
                        var note = AssetBundle.LoadFromFile(path);
                        if (note != null)
                        {
                            var Asset = note.LoadAsset<GameObject>("assets/_customnote.prefab");
                            var Note = Asset.transform.Find("Note").gameObject;
                            var Cube = Note.transform.Find("NoteCube").gameObject;
                            if (Note != null && Cube != null)
                            {
                                Cube.GetComponent<MeshRenderer>().materials = new Material[1] { NoteMat };

                                Note.name = name;
                                noteList.Add(Note);
                            }
                            else
                            {
                                throw new Exception("Invaild note format");
                            }
                        }
                        else
                        {
                            throw new Exception("Note isn't an assetbundle");
                        }
                    }
                }
                catch (Exception err) {
                    Debug.Log($"Note Exception: {name} {err.Message}, {err.StackTrace}");
                }
            }
        }
        */
        #endregion

        if (_Cursors.Length > 0)
        {
            foreach (string path in _Cursors)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var name = Path.GetFileName(path);
                        Texture texture = null;
                        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + path))
                        {
                            uwr.SendWebRequest();

                            // wrap tasks in try/catch, otherwise it'll fail silently
                            try
                            {
                                while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

                                if (uwr.result != UnityWebRequest.Result.Success) throw new Exception(uwr.error);
                                else
                                {
                                    texture = DownloadHandlerTexture.GetContent(uwr);
                                }
                            }
                            catch (Exception err)
                            {
                                Debug.Log($"Cursor Exception: {name} {err.Message}, {err.StackTrace}");
                            }
                        }

                        if (texture != null)
                        {
                            texture.name = name;
                            cursorList.Add(texture);
                        }
                    }
                }
                catch { }
            }
        }

        if (_Sounds.Length > 0)
        {
            foreach (string path in _Sounds)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        var name = Path.GetFileName(path);
                        AudioClip clip = null;
                        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                        {
                            uwr.SendWebRequest();

                            // wrap tasks in try/catch, otherwise it'll fail silently
                            try
                            {
                                while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

                                if (uwr.result != UnityWebRequest.Result.Success) throw new Exception(uwr.error);
                                else
                                {
                                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                                }
                            }
                            catch (Exception err)
                            {
                                Debug.Log($"Sound Exception: {name} {err.Message}, {err.StackTrace}");
                            }
                        }

                        if (clip != null)
                        {
                            clip.name = name;
                            soundList.Add(clip);
                        }
                    }
                }
                catch { }
            }
        }

        #region Removed
        /*
        if (_HitSounds.Length > 0)
        {
            foreach (string path in _HitSounds)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        AudioClip clip = null;
                        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                        {
                            uwr.SendWebRequest();

                            // wrap tasks in try/catch, otherwise it'll fail silently
                            try
                            {
                                while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

                                if (uwr.result != UnityWebRequest.Result.Success) Debug.Log($"{uwr.error}");
                                else
                                {
                                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                                }
                            }
                            catch (Exception err)
                            {
                                Debug.Log($"{err.Message}, {err.StackTrace}");
                            }
                        }

                        if (clip != null)
                        {
                            var name = Path.GetFileName(path);

                            clip.name = name;
                            hitList.Add(clip);
                        }
                    }
                }
                catch { }
            }
        }

        if (_MissSounds.Length > 0)
        {            
            foreach (string path in _MissSounds)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        AudioClip clip = null;
                        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                        {
                            uwr.SendWebRequest();

                            // wrap tasks in try/catch, otherwise it'll fail silently
                            try
                            {
                                while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

                                if (uwr.result != UnityWebRequest.Result.Success) Debug.Log($"{uwr.error}");
                                else
                                {
                                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                                }
                            }
                            catch (Exception err)
                            {
                                Debug.Log($"{err.Message}, {err.StackTrace}");
                            }
                        }

                        if (clip != null)
                        {
                            var name = Path.GetFileName(path);

                            clip.name = name;
                            missList.Add(clip);
                        }
                    }
                }
                catch { }
            }
        }

        if (_DeathSounds.Length > 0)
        {
            foreach (string path in _DeathSounds)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        AudioClip clip = null;
                        using (UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
                        {
                            uwr.SendWebRequest();

                            // wrap tasks in try/catch, otherwise it'll fail silently
                            try
                            {
                                while (uwr.result == UnityWebRequest.Result.InProgress) await Task.Delay(50);

                                if (uwr.result != UnityWebRequest.Result.Success) Debug.Log($"{uwr.error}");
                                else
                                {
                                    clip = DownloadHandlerAudioClip.GetContent(uwr);
                                }
                            }
                            catch (Exception err)
                            {
                                Debug.Log($"{err.Message}, {err.StackTrace}");
                            }
                        }

                        if (clip != null)
                        {
                            var name = Path.GetFileName(path);

                            clip.name = name;
                            deathList.Add(clip);
                        }
                    }
                }
                catch { }
            }
        }
        */
        #endregion

        Cursors = cursorList;
        Notes = noteList;
        Sounds = soundList;
        //HitSounds = hitList;
        //MissSounds = missList;
        //DeathSounds = deathList;
        #endregion
    }

    public string LoadMesh(string path, string type)
    {
        if (type.ToLower() == "note")
        {
            var note = AssetBundle.LoadFromFile(path);

            if (note != null)
            {
                var Asset = note.LoadAsset<GameObject>("assets/_customnote.prefab");
                try
                {
                    Note = Asset.transform.Find("Note").gameObject;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return e.Message;
                }
                return null;
            }
            else
                return "Note not found";
        }
        else
            return "Invaild Mesh";
    }

    /* Old Note Mesh stuff
    private Action<AsyncOperationHandle<GameObject>> Note_Completed(string path, AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Failed)
        {
            var note = AssetBundle.LoadFromFile(path);

            if (note != null)
            {
                var bsAsset = note.LoadAsset<GameObject>("assets/_customnote.prefab");
                NoteObj = bsAsset.transform.Find("NoteLeft").gameObject;
            }
        }
        else
            NoteObj = obj.Result;

        return delegate { };
    }
    */
}
