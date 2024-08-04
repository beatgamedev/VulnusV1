using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class SkinButton : MonoBehaviour
{
    public bool Selected = false;
    public SkinType Type;
    public string Name;
    public Texture CursorAsset;
    public GameObject MeshAsset;
    public AudioClip SoundAsset;
    //public (Texture, GameObject, AudioClip) Asset;
    public SkinButton(SkinType Type, string Name, Texture Cursor)
    {
        this.Type = Type;
        this.Name = Name;
        this.CursorAsset = Cursor;
        //this.Asset = new(item1: Cursor, item2: null, item3: null); ;
    }
    public SkinButton(SkinType Type, string Name, GameObject Mesh)
    {
        this.Type = Type;
        this.Name = Name;
        this.MeshAsset = Mesh;
        //this.Asset = new(item1: null, item2: Mesh, item3: null);
    }
    public SkinButton(SkinType Type, string Name, AudioClip Sound)
    {
        this.Type = Type;
        this.Name = Name;
        this.SoundAsset = Sound;
        //this.Asset = new(item1: null, item2: null, item3: Sound);
    }
}
