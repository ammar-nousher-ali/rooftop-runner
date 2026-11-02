using UnityEngine;
using UnityEditor;
using RooftopRunner;
public static class AtmosphereSetup
{
    public static void AddToScene(GameController game)
    {
        var atmosphere = game.GetComponent<CityAtmosphere>();
        if (!atmosphere) atmosphere = game.gameObject.AddComponent<CityAtmosphere>();
        atmosphere.game = game; game.atmosphere = atmosphere;
        var city = Object.FindFirstObjectByType<CityBackdrop>();
        var art = city.art;
        art.nightSkyMaterial = Material("SkyAtmosphere", 1, 0);
        art.nightCityMaterial = Material("CityAtmosphere", 0, 1);
        art.nightPropMaterial = Material("PropAtmosphere", 0, 0);
        EditorUtility.SetDirty(art);
    }
    static Material Material(string name, float sky, float windows)
    {
        string path = "Assets/RooftopRunner/Shaders/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material) { material = new Material(Shader.Find("RooftopRunner/CityAtmosphere")); AssetDatabase.CreateAsset(material, path); }
        material.SetFloat("_Sky", sky); material.SetFloat("_Windows", windows);
        EditorUtility.SetDirty(material); return material;
    }
}
