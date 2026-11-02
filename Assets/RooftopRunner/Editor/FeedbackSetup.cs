using UnityEngine;
using UnityEditor;
using RooftopRunner;
public static class FeedbackSetup
{
    public static void AddToScene(GameController game)
    {
        var feedback = game.GetComponent<RunFeedback>();
        if (!feedback) feedback = game.gameObject.AddComponent<RunFeedback>();
        game.feedback = feedback; feedback.game = game;
        feedback.running = Clip("Running", ".mp3"); feedback.jumpGrunt = Clip("JumpGrunt", ".mp3");
        feedback.landing = Clip("Landing"); feedback.charge = Clip("Charge");
    }
    static AudioClip Clip(string name, string extension = ".wav")
    {
        string path = "Assets/RooftopRunner/Audio/" + name + extension;
        AssetDatabase.ImportAsset(path);
        var importer = (AudioImporter)AssetImporter.GetAtPath(path);
        var settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        importer.defaultSampleSettings = settings; importer.forceToMono = true;
        importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
}
