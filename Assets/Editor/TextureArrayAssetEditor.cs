using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextureArrayAsset))]
public class TextureArrayAssetEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var asset = (TextureArrayAsset)target;

        if (GUILayout.Button("Rebuild Texture2DArray"))
        {
            BuildArray(asset);
        }
    }

    static bool IsSupportedFormat(TextureFormat format)
    {
        return format == TextureFormat.DXT1   
            || format == TextureFormat.DXT5; 
    }

    static void BuildArray(TextureArrayAsset asset)
    {
        if (asset.textures == null || asset.textures.Count == 0)
            return;

        Texture2D first = asset.textures[0];

        int width = first.width;
        int height = first.height;
        TextureFormat format = first.format;

        if (!IsSupportedFormat(format))
        {
            return;
        }

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

        if (asset.array != null)
        {
            AssetDatabase.RemoveObjectFromAsset(asset.array);
            DestroyImmediate(asset.array, true);
        }

        var array = new Texture2DArray(
            width,
            height,
            asset.textures.Count,
            format,
            true 
        );

        array.wrapMode = TextureWrapMode.Repeat;
        array.filterMode = FilterMode.Bilinear;

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

        array.name = asset.name + "_Array";

        AssetDatabase.AddObjectToAsset(array, asset);
        AssetDatabase.SaveAssets();

        asset.array = array;
        EditorUtility.SetDirty(asset);

        Debug.Log($"Texture2DArray rebuilt: {format}, {width}x{height}, layers: {asset.textures.Count}");
    }
}
