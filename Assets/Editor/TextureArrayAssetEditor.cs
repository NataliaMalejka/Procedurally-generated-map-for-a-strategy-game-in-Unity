using UnityEditor;
using UnityEngine;


// Custom inspector for TextureArrayAsset.
// Adds a button that allows rebuilding the Texture2DArray
[CustomEditor(typeof(TextureArrayAsset))]
public class TextureArrayAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default inspector fields (textures list, array reference)
        DrawDefaultInspector();

        var asset = (TextureArrayAsset)target;

        // Button that triggers rebuilding the Texture2DArray
        if (GUILayout.Button("Rebuild Texture2DArray"))
        {
            BuildArray(asset);
        }
    }

    // Checks if the texture format is supported by Texture2DArray
    static bool IsSupportedFormat(TextureFormat format)
    {
        return format == TextureFormat.DXT1   
            || format == TextureFormat.DXT5; 
    }


    // Builds a Texture2DArray from the list of textures stored in the asset
    static void BuildArray(TextureArrayAsset asset)
    {
        // Validate texture list
        if (asset.textures == null || asset.textures.Count == 0)
            return;

        // Use the first texture as a reference
        Texture2D first = asset.textures[0];

        int width = first.width;
        int height = first.height;
        TextureFormat format = first.format;

        // Return if texture format is not supported
        if (!IsSupportedFormat(format))
        {
            return;
        }

        // Validate all textures for size, format and mipmaps
        foreach (var tex in asset.textures)
        {
            if (tex == null)
            {
                return;
            }

            if (tex.width != width || tex.height != height)
            {
                return;
            }

            if (tex.format != format)
            {
                return;
            }

            if (tex.mipmapCount <= 1)
            {
                return;
            }
        }

        // Remove previously generated array (if exists)
        if (asset.array != null)
        {
            AssetDatabase.RemoveObjectFromAsset(asset.array);
            DestroyImmediate(asset.array, true);
        }

        // Create new Texture2DArray
        var array = new Texture2DArray(
            width,
            height,
            asset.textures.Count,
            format,
            true 
        );

        // Set texture sampling settings
        array.wrapMode = TextureWrapMode.Repeat;
        array.filterMode = FilterMode.Bilinear;

        // Copy all mip levels of each texture into the array
        for (int i = 0; i < asset.textures.Count; i++)
        {
            for (int mip = 0; mip < asset.textures[i].mipmapCount; mip++)
            {
                Graphics.CopyTexture(
                    asset.textures[i], 0, mip,
                    array, i, mip
                );
            }
        }

        // Name the generated array
        array.name = asset.name + "_Array";

        // Save the array as a sub-asset
        AssetDatabase.AddObjectToAsset(array, asset);
        AssetDatabase.SaveAssets();

        asset.array = array;
        EditorUtility.SetDirty(asset);

        Debug.Log($"Texture2DArray rebuilt: {format}, {width}x{height}, layers: {asset.textures.Count}");
    }
}
