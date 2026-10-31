using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using RooftopRunner;

public static class SunsetArtSetup
{
    public const string Folder = "Assets/RooftopRunner/Art/Sunset/";
    public static SunsetArt Import()
    {
        if (!File.Exists(Folder + "sky.png")) return null;
        var art = AssetDatabase.LoadAssetAtPath<SunsetArt>(Folder + "SunsetArt.asset");
        if (!art) { art = ScriptableObject.CreateInstance<SunsetArt>(); AssetDatabase.CreateAsset(art, Folder + "SunsetArt.asset"); }
        art.sky = ImportSprite("sky"); art.skyline = ImportSprite("city");
        art.facadeSheet = ImportTexture("roofs"); art.propSheet = ImportTexture("props"); art.runnerSheet = ImportTexture("runner-clean");
        // Rectangle modules are intentionally sliced inside the facade edges; no sheet backdrop is rendered.
        float sx = art.facadeSheet.width / 1536f, sy = art.facadeSheet.height / 1024f;
        art.facadeRects = new[] { new Rect(65*sx, 49*sy, 425*sx, 914*sy), new Rect(557*sx, 49*sy, 426*sx, 914*sy), new Rect(1054*sx, 49*sy, 414*sx, 914*sy) };
        // Generated prop silhouettes cross nominal grid lines; use reviewed individual regions.
        float px = art.propSheet.width / 1774f, py = art.propSheet.height / 887f;
        art.propRects = new[] {
            Region(0, 0, 444, 460, px, py, art.propSheet.height), Region(444, 0, 450, 460, px, py, art.propSheet.height),
            Region(894, 0, 450, 460, px, py, art.propSheet.height), Region(1344, 0, 430, 460, px, py, art.propSheet.height),
            Region(0, 465, 500, 422, px, py, art.propSheet.height), Region(500, 465, 438, 422, px, py, art.propSheet.height),
            Region(938, 465, 402, 422, px, py, art.propSheet.height), Region(1340, 465, 434, 422, px, py, art.propSheet.height)
        };
        for (int i = 0; i < art.propRects.Length; i++) art.propRects[i] = Trim(art.propSheet, art.propRects[i]);
        art.runnerRects = FindCells(art.runnerSheet, 4, 2);
        EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
        foreach (var name in new[] { "sky", "city", "roofs", "props", "runner-clean" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(Folder + name + ".png");
            importer.isReadable = false; importer.SaveAndReimport();
        }
        return art;
    }
    static Texture2D ImportTexture(string name)
    {
        string path = Folder + name + ".png";
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100; importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false; importer.isReadable = true; importer.maxTextureSize = 2048;
        importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
    static Sprite ImportSprite(string name) { ImportTexture(name); return AssetDatabase.LoadAssetAtPath<Sprite>(Folder + name + ".png"); }
    static Rect Region(float x, float y, float width, float height, float sx, float sy, float textureHeight)
    { return new Rect(x * sx, textureHeight - (y + height) * sy, width * sx, height * sy); }
    static Rect Trim(Texture2D texture, Rect region)
    {
        var pixels = texture.GetPixels32();
        int minX = (int)region.xMax, minY = (int)region.yMax, maxX = (int)region.x, maxY = (int)region.y;
        for (int y = Mathf.Max(0, (int)region.y); y < Mathf.Min(texture.height, (int)region.yMax); y++)
            for (int x = Mathf.Max(0, (int)region.x); x < Mathf.Min(texture.width, (int)region.xMax); x++)
            {
                if (pixels[y * texture.width + x].a < 40) continue;
                minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y); maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y);
            }
        return new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
    static Rect[] FindCells(Texture2D texture, int columns, int rows)
    {
        if (!texture) throw new InvalidOperationException("Missing generated art sheet.");
        var pixels = texture.GetPixels32();
        var rects = new Rect[columns * rows];
        int cellWidth = texture.width / columns, cellHeight = texture.height / rows;
        for (int row = 0; row < rows; row++) for (int column = 0; column < columns; column++)
        {
            int startX = column * cellWidth, startY = (rows - row - 1) * cellHeight;
            int minX = startX + cellWidth, minY = startY + cellHeight, maxX = startX, maxY = startY;
            for (int y = startY; y < startY + cellHeight; y++) for (int x = startX; x < startX + cellWidth; x++)
            {
                if (pixels[y * texture.width + x].a < 40) continue;
                minX = Mathf.Min(minX, x); minY = Mathf.Min(minY, y); maxX = Mathf.Max(maxX, x); maxY = Mathf.Max(maxY, y);
            }
            if (maxX <= minX || maxY <= minY) throw new InvalidOperationException("Empty sprite cell: " + texture.name + " " + row + "," + column);
            rects[row * columns + column] = new Rect(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
        return rects;
    }
}
