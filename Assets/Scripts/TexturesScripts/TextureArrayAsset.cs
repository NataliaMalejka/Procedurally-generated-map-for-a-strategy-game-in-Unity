using UnityEngine;
using System.Collections.Generic;

// ScriptableObject that stores a list of textures
[CreateAssetMenu(fileName = "NewTextureArrayAsset", menuName = "Texture/TextureArrayAsset")]
public class TextureArrayAsset : ScriptableObject
{
    // Source textures used to build the Texture2DArray
    public List<Texture2D> textures;
    // Generated Texture2DArray asset
    public Texture2DArray array;
}