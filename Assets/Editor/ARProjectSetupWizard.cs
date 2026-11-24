#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
using UnityEditor.Android;
using UnityEditor.XR.ARSubsystems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Unity.XR.CoreUtils;

/// <summary>
/// One-click builder for the Kelompok 4 P09-P11 AR project.
/// Generates the reference image library, garden prefab and all three scenes.
/// Run from: Kelompok 4 AR > 0 - BUILD COMPLETE P09-P11 PROJECT.
/// </summary>
public static class ARProjectSetupWizard
{
    private const string MarkerPath = "Assets/Markers/Marker_PinkBotanical.png";
    private const string LibraryPath = "Assets/ReferenceImages/RefImageLibrary_FinalProject.asset";
    private const string PrefabPath = "Assets/Prefabs/AR/Prefab_PinkBotanicalGarden.prefab";
    private const string P09Path = "Assets/Scenes/P09_ARFoundationSetup.unity";
    private const string P10Path = "Assets/Scenes/P10_ImageTracking.unity";
    private const string P11Path = "Assets/Scenes/P11_ObjectOnMarker.unity";
    private const string MarkerName = "Marker_PinkBotanical";
    private const string FBXPath = "Assets/Models/PinkBotanicalGarden.fbx";

    // Always resolve the asset again after AssetDatabase import/save operations.
    // This avoids retaining a stale UnityEngine.Object reference if an importer reloads.
    private static XRReferenceImageLibrary LoadReferenceImageLibrary()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        XRReferenceImageLibrary library =
            AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(LibraryPath);

        if (library == null)
            library = AssetDatabase.LoadMainAssetAtPath(LibraryPath) as XRReferenceImageLibrary;

        if (library == null)
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:XRReferenceImageLibrary", new[] { "Assets/ReferenceImages" });

            foreach (string guid in guids)
            {
                string candidatePath = AssetDatabase.GUIDToAssetPath(guid);
                library = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(candidatePath);
                if (library != null)
                {
                    Debug.Log("[AR Kelompok 4] Reference Image Library ditemukan melalui GUID: " + candidatePath);
                    break;
                }
            }
        }

        if (library == null)
        {
            throw new InvalidOperationException(
                "Asset Reference Image Library tidak dapat dimuat. Path yang dicari: " +
                LibraryPath + ". Periksa Console untuk error import worker, lalu restart Unity.");
        }

        if (library.count < 1)
            throw new InvalidOperationException(
                "Reference Image Library ditemukan tetapi belum berisi marker. Jalankan menu Create/Refresh Reference Library.");

        return library;
    }

    [MenuItem("Kelompok 4 AR/0 - BUILD COMPLETE P09-P11 PROJECT", false, 0)]
    public static void BuildCompleteProject()
    {
        try
        {
            EnsureFolders();
            AssetDatabase.Refresh();

            Texture2D marker = PrepareMarkerTexture();
            CreateOrRefreshReferenceLibrary(marker);
            XRReferenceImageLibrary library;
            Material[] gardenMaterials = CreateOrRefreshGardenMaterials();
            GameObject prefab = File.Exists(FBXPath)
                ? PinkBotanicalFBXInstaller.BuildPrefabFromFBX()
                : CreateOrRefreshGardenPrefab(gardenMaterials);

            // Prefab/material creation can trigger AssetDatabase imports.
            // Re-resolve the library after those imports rather than using a stale object reference.
            AssetDatabase.SaveAssets();
            library = LoadReferenceImageLibrary();
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
                throw new InvalidOperationException("Prefab taman bunga tidak dapat dimuat setelah dibuat: " + PrefabPath);

            ConfigureAndroidPlayerSettings();
            ConfigureARCoreLoader();

            CreateScene(P09Path, false, null, null);
            CreateScene(P10Path, true, library, null);
            CreateScene(P11Path, true, library, prefab);

            ConfigureBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorSceneManager.OpenScene(P11Path, OpenSceneMode.Single);
            EditorUtility.DisplayDialog(
                "Kelompok 4 AR — Build Complete",
                "P09, P10, dan P11 berhasil dibuat.\n\n" +
                "Reference Image Library: " + LibraryPath + "\n" +
                "Marker: " + MarkerName + " (physical size estimated by ARCore)\n" +
                "Prefab: " + PrefabPath + "\n\n" +
                "Scene P11 sudah dibuka. Periksa Inspector XR Origin sebelum Build Android.",
                "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Kelompok 4 AR — Build gagal",
                "Terjadi kesalahan saat membuat project. Periksa Console.\n\n" + ex.Message, "OK");
        }
    }

    [MenuItem("Kelompok 4 AR/Legacy/1 - Create Procedural Garden Prefab (not for FBX final)", false, 20)]
    public static void CreateLegacyProceduralGardenPrefabMenu()
    {
        EnsureFolders();
        Material[] gardenMaterials = CreateOrRefreshGardenMaterials();
        GameObject prefab = CreateOrRefreshGardenPrefab(gardenMaterials);
        Selection.activeObject = prefab;
        EditorUtility.DisplayDialog("Legacy Procedural Prefab",
            "Prefab prosedural lama dibuat. Untuk project final FBX, gunakan menu INSTALL FBX MODEL + REBUILD P11 PREFAB.", "OK");
    }

    [MenuItem("Kelompok 4 AR/2 - Create P09 AR Foundation Scene", false, 2)]
    public static void CreateP09Menu()
    {
        EnsureFolders();
        CreateScene(P09Path, false, null, null);
        EditorSceneManager.OpenScene(P09Path, OpenSceneMode.Single);
    }

    [MenuItem("Kelompok 4 AR/3 - Create P10 Image Tracking Scene", false, 3)]
    public static void CreateP10Menu()
    {
        EnsureFolders();
        CreateOrRefreshReferenceLibrary(PrepareMarkerTexture());
        var library = LoadReferenceImageLibrary();
        CreateScene(P10Path, true, library, null);
        EditorSceneManager.OpenScene(P10Path, OpenSceneMode.Single);
    }

    [MenuItem("Kelompok 4 AR/4 - Create P11 Object On Marker Scene", false, 4)]
    public static void CreateP11Menu()
    {
        EnsureFolders();
        CreateOrRefreshReferenceLibrary(PrepareMarkerTexture());
        var library = LoadReferenceImageLibrary();
        GameObject prefab = File.Exists(FBXPath)
            ? PinkBotanicalFBXInstaller.BuildPrefabFromFBX()
            : CreateOrRefreshGardenPrefab(CreateOrRefreshGardenMaterials());
        library = LoadReferenceImageLibrary();
        CreateScene(P11Path, true, library, prefab);
        EditorSceneManager.OpenScene(P11Path, OpenSceneMode.Single);
    }

    private static Texture2D PrepareMarkerTexture()
    {
        if (!File.Exists(MarkerPath))
            throw new FileNotFoundException("Marker tidak ditemukan: " + MarkerPath);

        var importer = AssetImporter.GetAtPath(MarkerPath) as TextureImporter;
        if (importer != null)
        {
            bool changed = false;
            if (importer.textureType != TextureImporterType.Default) { importer.textureType = TextureImporterType.Default; changed = true; }
            if (!importer.sRGBTexture) { importer.sRGBTexture = true; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
            if (importer.npotScale != TextureImporterNPOTScale.None) { importer.npotScale = TextureImporterNPOTScale.None; changed = true; }
            if (importer.maxTextureSize != 2048) { importer.maxTextureSize = 2048; changed = true; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (importer.wrapMode != TextureWrapMode.Clamp) { importer.wrapMode = TextureWrapMode.Clamp; changed = true; }
            if (importer.filterMode != FilterMode.Bilinear) { importer.filterMode = FilterMode.Bilinear; changed = true; }
            if (changed)
                importer.SaveAndReimport();
        }

        AssetDatabase.ImportAsset(MarkerPath, ImportAssetOptions.ForceSynchronousImport);

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(MarkerPath);
        if (texture == null)
            throw new InvalidOperationException("Unity tidak dapat memuat marker sebagai Texture2D.");
        return texture;
    }

    private static XRReferenceImageLibrary CreateOrRefreshReferenceLibrary(Texture2D marker)
    {
        if (marker == null)
            throw new InvalidOperationException("Texture marker kosong. Periksa: " + MarkerPath);

        XRReferenceImageLibrary library =
            AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(LibraryPath);

        if (library == null)
            library = AssetDatabase.LoadMainAssetAtPath(LibraryPath) as XRReferenceImageLibrary;

        if (library == null)
        {
            // If a broken/incorrect asset exists at the target path, remove it before creating
            // the correctly typed XRReferenceImageLibrary asset.
            UnityEngine.Object existing = AssetDatabase.LoadMainAssetAtPath(LibraryPath);
            if (existing != null)
                AssetDatabase.DeleteAsset(LibraryPath);

            library = ScriptableObject.CreateInstance<XRReferenceImageLibrary>();
            library.name = "RefImageLibrary_FinalProject";
            AssetDatabase.CreateAsset(library, LibraryPath);
        }

        while (library.count > 0)
            library.RemoveAt(library.count - 1);

        library.Add();
        library.SetTexture(0, marker, false);
        library.SetName(0, MarkerName);
        library.SetSpecifySize(0, false);
        library.SetSize(0, new Vector2(0.15f, 0.15f));

        EditorUtility.SetDirty(library);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(LibraryPath, ImportAssetOptions.ForceSynchronousImport);

        XRReferenceImageLibrary reloaded =
            AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(LibraryPath);
        if (reloaded == null)
            reloaded = AssetDatabase.LoadMainAssetAtPath(LibraryPath) as XRReferenceImageLibrary;

        if (reloaded == null)
            throw new InvalidOperationException(
                "Reference Image Library gagal dimuat ulang setelah dibuat. Periksa status import asset: " + LibraryPath);

        if (reloaded.count < 1)
            throw new InvalidOperationException(
                "Reference Image Library berhasil dibuat tetapi marker tidak tersimpan.");

        return reloaded;
    }

    private static Material[] CreateOrRefreshGardenMaterials()
    {
        EnsureFolders();
        return new[]
        {
            CreateOrRefreshMaterial("MAT_RoseQuartz_Soil", new Color(0.48f, 0.20f, 0.34f), 0f),
            CreateOrRefreshMaterial("MAT_Sage_Garden", new Color(0.22f, 0.48f, 0.34f), 0f),
            CreateOrRefreshMaterial("MAT_Botanical_Stem", new Color(0.12f, 0.34f, 0.20f), 0f),
            CreateOrRefreshMaterial("MAT_Botanical_Leaf", new Color(0.28f, 0.62f, 0.37f), 0f),
            CreateOrRefreshMaterial("MAT_Petal_Pink", new Color(0.95f, 0.35f, 0.60f), 0f),
            CreateOrRefreshMaterial("MAT_Petal_Blush", new Color(1.00f, 0.68f, 0.78f), 0f),
            CreateOrRefreshMaterial("MAT_Petal_Magenta", new Color(0.66f, 0.12f, 0.38f), 0f),
            CreateOrRefreshMaterial("MAT_Pollen_Gold", new Color(1.00f, 0.72f, 0.18f), 0.20f),
            CreateOrRefreshMaterial("MAT_Pearl_Stone", new Color(0.94f, 0.78f, 0.85f), 0.12f)
        };
    }

    private static Material CreateOrRefreshMaterial(string materialName, Color color, float metallic)
    {
        string path = "Assets/Materials/" + materialName + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                shader = Shader.Find("Standard");
            if (shader == null)
                throw new InvalidOperationException("Shader Lit/Standard tidak ditemukan untuk material " + materialName);

            material = new Material(shader);
            material.name = materialName;
            AssetDatabase.CreateAsset(material, path);
        }

        material.color = color;
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static GameObject CreateOrRefreshGardenPrefab(Material[] materials)
    {
        EnsureFolders();

        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(PrefabPath);

        var root = new GameObject("Prefab_PinkBotanicalGarden");
        var garden = root.AddComponent<ProceduralFlowerGarden>();
        var serializedGarden = new SerializedObject(garden);
        string[] materialProperties =
        {
            "soil", "grass", "stem", "leaf", "petalPink",
            "petalBlush", "petalMagenta", "gold", "pearl"
        };

        if (materials == null || materials.Length != materialProperties.Length)
            throw new InvalidOperationException("Daftar material taman bunga tidak lengkap.");

        for (int i = 0; i < materialProperties.Length; i++)
        {
            SerializedProperty property = serializedGarden.FindProperty(materialProperties[i]);
            if (property == null)
                throw new InvalidOperationException("Field material tidak ditemukan: " + materialProperties[i]);
            property.objectReferenceValue = materials[i];
        }
        serializedGarden.ApplyModifiedPropertiesWithoutUndo();

        // Bake all meshes and renderers into the prefab so it is visible in
        // Project/Inspector and does not rely on runtime-only generated geometry.
        garden.BuildGarden();

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        UnityEngine.Object.DestroyImmediate(root);

        if (prefab == null)
            throw new InvalidOperationException("Prefab taman bunga gagal dibuat.");

        AssetDatabase.SaveAssets();
        return prefab;
    }

    private static void CreateScene(string scenePath, bool addImageTracking,
        XRReferenceImageLibrary library, GameObject contentPrefab)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var sessionObject = new GameObject("AR Session");
        sessionObject.AddComponent<ARSession>();

        var originObject = new GameObject("XR Origin");
        var xrOrigin = originObject.AddComponent<XROrigin>();

        var cameraObject = new GameObject("AR Camera");
        cameraObject.transform.SetParent(originObject.transform, false);
        cameraObject.tag = "MainCamera";

        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.nearClipPlane = 0.01f;
        camera.farClipPlane = 100f;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<ARCameraManager>();
        cameraObject.AddComponent<ARCameraBackground>();
        xrOrigin.Camera = camera;

        if (addImageTracking)
        {
            if (library == null)
                library = LoadReferenceImageLibrary();

            if (library.count < 1)
                throw new InvalidOperationException("Reference Image Library kosong. Marker belum tersimpan.");

            var manager = originObject.AddComponent<ARTrackedImageManager>();
            // Assign the non-null library before enabling the manager.
            manager.enabled = false;
            manager.referenceLibrary = library;
            manager.requestedMaxNumberOfMovingImages = 1;
            manager.enabled = true;

            var overlay = originObject.AddComponent<ARStatusOverlay>();
            var overlaySO = new SerializedObject(overlay);
            overlaySO.FindProperty("imageManager").objectReferenceValue = manager;
            overlaySO.ApplyModifiedPropertiesWithoutUndo();

            if (contentPrefab != null)
            {
                var spawner = originObject.AddComponent<ARImagePrefabSpawner>();
                var spawnerSO = new SerializedObject(spawner);
                spawnerSO.FindProperty("imageManager").objectReferenceValue = manager;
                spawnerSO.FindProperty("contentPrefab").objectReferenceValue = contentPrefab;
                spawnerSO.FindProperty("targetReferenceName").stringValue = MarkerName;
                spawnerSO.FindProperty("hideWhenTrackingIsLimited").boolValue = false;
                spawnerSO.FindProperty("writeTrackingLogs").boolValue = true;
                spawnerSO.FindProperty("localPosition").vector3Value = new Vector3(0f, 0.015f, 0f);
                spawnerSO.FindProperty("localEulerAngles").vector3Value = Vector3.zero;
                spawnerSO.FindProperty("uniformScale").floatValue = 0.07f;
                spawnerSO.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.0f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        EditorSceneManager.SaveScene(scene, scenePath);
    }

    [MenuItem("Kelompok 4 AR/5 - VALIDATE GENERATED ASSETS", false, 5)]
    public static void ValidateGeneratedAssetsMenu()
    {
        var issues = new System.Collections.Generic.List<string>();

        var library = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(LibraryPath);
        if (library == null)
            issues.Add("Reference Image Library belum dibuat.");
        else if (library.count < 1)
            issues.Add("Reference Image Library belum memiliki marker.");
        else if (library[0].name != MarkerName)
            issues.Add("Nama marker di library tidak sesuai: " + library[0].name);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(FBXPath);
        if (fbxModel == null)
            issues.Add("Model FBX belum diimpor: " + FBXPath);
        if (prefab == null)
            issues.Add("Prefab taman bunga belum dibuat.");
        else
        {
            MeshFilter[] meshFilters = prefab.GetComponentsInChildren<MeshFilter>(true);
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            if (meshFilters.Length == 0 || renderers.Length == 0)
                issues.Add("Prefab FBX belum memiliki MeshFilter dan Renderer yang aktif.");
            if (prefab.GetComponent<ProceduralFlowerGarden>() != null)
                issues.Add("Prefab masih memakai ProceduralFlowerGarden lama. Jalankan INSTALL FBX MODEL + REBUILD P11 PREFAB.");
        }

        foreach (string scenePath in new[] { P09Path, P10Path, P11Path })
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                issues.Add("Scene belum ditemukan: " + scenePath);
        }

        if (issues.Count == 0)
        {
            EditorUtility.DisplayDialog("Validasi Kelompok 4 AR",
                "LULUS pemeriksaan asset dasar.\n\nLibrary: " + library.count + " marker\n" +
                "Prefab: " + prefab.transform.childCount + " child objects\n" +
                "Scene: P09, P10, P11 ditemukan.\n\n" +
                "Catatan: compile, ARCore, build APK, dan tracking fisik tetap harus diuji.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Validasi belum lengkap",
                string.Join("\n• ", issues.ToArray()), "OK");
        }
    }

    [MenuItem("Kelompok 4 AR/6 - CONFIGURE ANDROID + ARCORE", false, 6)]
    public static void ConfigureAndroidMenu()
    {
        ConfigureAndroidPlayerSettings();
        bool loaderConfigured = ConfigureARCoreLoader();
        EditorUtility.DisplayDialog("Android Configuration",
            "Player Settings Android sudah disiapkan.\n" +
            (loaderConfigured
                ? "Google ARCore loader sudah terpasang pada XR Plug-in Management."
                : "Periksa Edit > Project Settings > XR Plug-in Management > Android dan aktifkan Google ARCore secara manual.") +
            "\n\nPastikan Android Build Support, SDK & NDK Tools, dan OpenJDK terpasang di Unity Hub.",
            "OK");
    }

    private static void ConfigureAndroidPlayerSettings()
    {
        PlayerSettings.productName = "Pink Botanical Garden AR";
        PlayerSettings.companyName = "Kelompok 4";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,
            "com.kelompok4.pinkbotanicalar");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,
            ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.Android.forceInternetPermission = true;
        AssetDatabase.SaveAssets();
    }

    private static bool ConfigureARCoreLoader()
    {
        try
        {
            XRGeneralSettings generalSettings =
                XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);

            if (generalSettings == null || generalSettings.AssignedSettings == null)
            {
                Debug.LogWarning("[AR Kelompok 4] XR General Settings Android belum tersedia. Aktifkan ARCore dari Project Settings.");
                return false;
            }

            XRManagerSettings manager = generalSettings.AssignedSettings;
            bool alreadyAssigned = false;
            foreach (var loader in manager.activeLoaders)
            {
                if (loader != null && loader.GetType().FullName == "UnityEngine.XR.ARCore.ARCoreLoader")
                {
                    alreadyAssigned = true;
                    break;
                }
            }

            if (!alreadyAssigned)
            {
                bool assigned = XRPackageMetadataStore.AssignLoader(
                    manager, "UnityEngine.XR.ARCore.ARCoreLoader", BuildTargetGroup.Android);
                if (!assigned)
                {
                    Debug.LogWarning("[AR Kelompok 4] ARCore loader belum dapat ditambahkan otomatis. Aktifkan melalui Project Settings.");
                    return false;
                }
            }

            generalSettings.InitManagerOnStart = true;
            EditorUtility.SetDirty(generalSettings);
            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[AR Kelompok 4] Konfigurasi ARCore otomatis tidak berhasil: " + ex.Message);
            return false;
        }
    }

    [MenuItem("Kelompok 4 AR/7 - BUILD APK P09 (Android)", false, 7)]
    public static void BuildP09AndroidMenu() { BuildAndroidApk(P09Path, "AR_Kelompok4_P09.apk"); }

    [MenuItem("Kelompok 4 AR/8 - BUILD APK P10 (Android)", false, 8)]
    public static void BuildP10AndroidMenu() { BuildAndroidApk(P10Path, "AR_Kelompok4_P10.apk"); }

    [MenuItem("Kelompok 4 AR/9 - BUILD APK P11 (Android)", false, 9)]
    public static void BuildP11AndroidMenu()
    {
        GameObject prefab = PinkBotanicalFBXInstaller.BuildPrefabFromFBX();
        PinkBotanicalFBXInstaller.WireP11Scene(prefab);
        BuildAndroidApk(P11Path, "AR_Kelompok4_P11_FBX.apk");
    }

    private static void BuildAndroidApk(string scenePath, string fileName)
    {
        if (!File.Exists(scenePath))
        {
            EditorUtility.DisplayDialog("Build APK gagal",
                "Scene belum dibuat. Jalankan menu BUILD COMPLETE P09-P11 PROJECT terlebih dahulu.", "OK");
            return;
        }

        ConfigureAndroidPlayerSettings();

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android, BuildTarget.Android);
            EditorUtility.DisplayDialog("Platform Android",
                switched
                    ? "Platform sudah dialihkan ke Android. Tunggu proses reimport selesai, lalu klik menu Build APK yang sama sekali lagi."
                    : "Unity belum dapat beralih ke Android. Pastikan Android Build Support terpasang melalui Unity Hub.",
                "OK");
            return;
        }

        Directory.CreateDirectory("Builds/Android");
        string outputPath = Path.Combine("Builds/Android", fileName);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { scenePath },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("Build berhasil",
                "APK berhasil dibuat:\n" + outputPath + "\n\nInstall pada perangkat Android yang mendukung ARCore untuk pengujian.",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Build belum berhasil",
                "Unity Build Result: " + report.summary.result +
                "\nPeriksa Console dan Project Validation. Pastikan SDK, NDK, OpenJDK, dan ARCore tersedia.",
                "OK");
        }
    }

    private static void ConfigureBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(P09Path, true),
            new EditorBuildSettingsScene(P10Path, true),
            new EditorBuildSettingsScene(P11Path, true)
        };
    }

    private static void EnsureFolders()
    {
        string[] folders =
        {
            "Assets/Documentation",
            "Assets/Editor",
            "Assets/Markers",
            "Assets/Materials",
            "Assets/Models",
            "Assets/Prefabs",
            "Assets/Prefabs/AR",
            "Assets/ReferenceImages",
            "Assets/Scenes",
            "Assets/Scripts",
            "Assets/Scripts/AR",
            "Assets/Scripts/Visuals",
            "Assets/XR",
            "Assets/Builds",
            "Assets/Builds/Android"
        };

        foreach (string folder in folders)
        {
            if (AssetDatabase.IsValidFolder(folder))
                continue;

            string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
            string name = Path.GetFileName(folder);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolderPath(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void EnsureFolderPath(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
            return;
        string parent = Path.GetDirectoryName(folder).Replace("\\", "/");
        string name = Path.GetFileName(folder);
        EnsureFolderPath(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
