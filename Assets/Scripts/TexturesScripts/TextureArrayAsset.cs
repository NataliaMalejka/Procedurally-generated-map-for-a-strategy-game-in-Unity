using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewTextureArrayAsset", menuName = "Texture/TextureArrayAsset")]
public class TextureArrayAsset : ScriptableObject
{
    public List<Texture2D> textures;
    public Texture2DArray array;
}