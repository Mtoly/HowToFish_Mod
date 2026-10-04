using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class HTFMapBundleBuilder
{
    private const string OutputFolder = "HTFMapBuild";
    private const string BundleNameKey = "HTFMapLoader.BundleName";

    [MenuItem("Tools/HTF Map Loader/Build Selected Scene AssetBundle")]
    public static void BuildSelectedSceneAssetBundle()
    {
        UnityEngine.Object selected = Selection.activeObject;
        if (selected == null) {
            EditorUtility.DisplayDialog("HTF Map Loader", "Select the .unity scene asset you want to build in the Project window.", "OK");
            return;
        }

        string scenePath = AssetDatabase.GetAssetPath(selected);
        if (string.IsNullOrWhiteSpace(scenePath) || !scenePath.EndsWith(".unity", StringComparison.OrdinalIgnoreCase)) {
            EditorUtility.DisplayDialog("HTF Map Loader", "The selected asset is not a Unity scene (.unity).", "OK");
            return;
        }

        string defaultName = Path.GetFileNameWithoutExtension(scenePath).ToLowerInvariant().Replace(" ", "_");
        string lastName = EditorPrefs.GetString(BundleNameKey, defaultName);
        string bundleName = AskBundleName(lastName);
        if (string.IsNullOrWhiteSpace(bundleName)) return;

        bundleName = SanitizeBundleName(bundleName);
        if (string.IsNullOrWhiteSpace(bundleName)) return;
        EditorPrefs.SetString(BundleNameKey, bundleName);

        if (!Directory.Exists(OutputFolder)) Directory.CreateDirectory(OutputFolder);
        AssetImporter importer = AssetImporter.GetAtPath(scenePath);
        if (importer == null) return;

        string previousBundle = importer.assetBundleName;
        try {
            importer.assetBundleName = bundleName;
            importer.SaveAndReimport();
            AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(
                OutputFolder,
                BuildAssetBundleOptions.ChunkBasedCompression,
                BuildTarget.StandaloneWindows64
            );
            if (manifest == null) return;
            string bundlePath = Path.Combine(Path.GetFullPath(OutputFolder), bundleName);
            Debug.Log("[HTF Map Loader] AssetBundle built: " + bundlePath);
            EditorUtility.RevealInFinder(bundlePath);
            EditorUtility.DisplayDialog("HTF Map Loader",
                "AssetBundle created successfully.\n\n" + bundlePath +
                "\n\nPut this file beside map.json. The .manifest files are not required.", "OK");
        }
        finally {
            importer.assetBundleName = previousBundle;
            importer.SaveAndReimport();
        }
    }

    private static string AskBundleName(string current)
    {
        BundleNameWindow window = ScriptableObject.CreateInstance<BundleNameWindow>();
        window.titleContent = new GUIContent("HTF Map Bundle");
        window.BundleName = current;
        window.position = new Rect(200, 200, 430, 115);
        window.ShowModal();
        return window.Accepted ? window.BundleName : null;
    }

    private static string SanitizeBundleName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "";
        value = value.Trim().ToLowerInvariant();
        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++) {
            char c = chars[i];
            if (!(char.IsLetterOrDigit(c) || c == '_' || c == '-')) chars[i] = '_';
        }
        return new string(chars);
    }

    private sealed class BundleNameWindow : EditorWindow
    {
        public string BundleName;
        public bool Accepted;
        private void OnGUI()
        {
            GUILayout.Label("AssetBundle filename", EditorStyles.boldLabel);
            BundleName = EditorGUILayout.TextField("Bundle name", BundleName);
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel")) { Accepted = false; Close(); }
            if (GUILayout.Button("Build")) { Accepted = true; Close(); }
            GUILayout.EndHorizontal();
        }
    }
}
