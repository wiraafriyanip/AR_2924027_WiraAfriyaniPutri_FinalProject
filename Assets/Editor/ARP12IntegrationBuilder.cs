#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// Adds P12 to the existing P11 project. It copies the user's P11 scene file,
/// builds the P12 UI in that copy, and leaves the original P11 scene untouched.
/// It does not replace Packages, ProjectSettings, marker libraries, prefabs,
/// or the existing ARImagePrefabSpawner script/configuration.
/// </summary>
public static class ARP12IntegrationBuilder
{
    private const string P11ScenePath = "Assets/Scenes/P11_ObjectOnMarker.unity";
    private const string P12ScenePath = "Assets/Scenes/Pertemuan12_ARInteraction.unity";
    private const string AudioPath = "Assets/P12Integration/Audio/ui_click.wav";
    private const string ApkPath = "Builds/P12/AR_P12_Kelompok4.apk";
    private const string DefaultMarkerName = "Marker_PinkBotanical";
    private const string SpawnedObjectName = "AR_PinkBotanicalGarden_Instance";

    private static string ToFullPath(string projectRelativePath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, projectRelativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    [MenuItem("Kelompok 4 AR/P12/1 - BUAT SCENE P12 DARI P11")]
    public static void GenerateP12SceneMenu()
    {
        GenerateP12Scene();
    }

    [MenuItem("Kelompok 4 AR/P12/2 - BUILD APK P12")]
    public static void BuildP12ApkMenu()
    {
        if (!File.Exists(ToFullPath(P12ScenePath)))
        {
            if (!GenerateP12Scene())
                return;
        }

        BuildP12Apk();
    }

    [MenuItem("Kelompok 4 AR/P12/3 - BUAT SCENE + BUILD APK")]
    public static void GenerateAndBuildP12Menu()
    {
        if (GenerateP12Scene())
            BuildP12Apk();
    }

    private static bool GenerateP12Scene()
    {
        try
        {
            if (!File.Exists(ToFullPath(P11ScenePath)))
                throw new FileNotFoundException("Scene P11 tidak ditemukan: " + P11ScenePath + ". Pastikan scene P11 bernama P11_ObjectOnMarker.unity.");

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            if (File.Exists(ToFullPath(P12ScenePath)))
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Scene P12 Sudah Ada",
                    "Scene P12 sebelumnya sudah ada. Buat ulang dari P11? Perubahan manual pada scene P12 lama akan diganti. Scene P11 tetap aman.",
                    "Buat Ulang P12", "Batal");
                if (!overwrite)
                    return false;

                AssetDatabase.DeleteAsset(P12ScenePath);
            }

            string p11FullPath = ToFullPath(P11ScenePath);
            string p12FullPath = ToFullPath(P12ScenePath);
            string sceneFolder = Path.GetDirectoryName(p12FullPath);
            if (!Directory.Exists(sceneFolder))
                Directory.CreateDirectory(sceneFolder);

            // Copy the serialized scene file first. P11 is never opened for editing
            // or saved by this builder; all P12 changes happen in the copied scene.
            File.Copy(p11FullPath, p12FullPath, true);
            AssetDatabase.ImportAsset(P12ScenePath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();

            Scene scene = EditorSceneManager.OpenScene(P12ScenePath, OpenSceneMode.Single);
            GameObject xrOrigin = GameObject.Find("XR Origin");
            if (xrOrigin == null)
                throw new InvalidOperationException("GameObject XR Origin tidak ditemukan pada salinan scene P11.");

            ARTrackedImageManager imageManager = xrOrigin.GetComponent<ARTrackedImageManager>();
            if (imageManager == null)
                throw new InvalidOperationException("AR Tracked Image Manager tidak ditemukan pada XR Origin.");

            Component spawner = xrOrigin.GetComponent("ARImagePrefabSpawner");
            if (spawner == null)
                throw new InvalidOperationException("ARImagePrefabSpawner tidak ditemukan pada XR Origin.");

            SerializedObject managerObject = new SerializedObject(imageManager);
            SerializedProperty libraryProperty = managerObject.FindProperty("m_SerializedLibrary");
            if (libraryProperty == null || libraryProperty.objectReferenceValue == null)
                throw new InvalidOperationException("Reference Image Library belum terhubung pada AR Tracked Image Manager.");

            SerializedObject spawnerObject = new SerializedObject(spawner);
            SerializedProperty prefabProperty = spawnerObject.FindProperty("contentPrefab");
            SerializedProperty markerProperty = spawnerObject.FindProperty("targetReferenceName");
            if (prefabProperty == null || prefabProperty.objectReferenceValue == null)
                throw new InvalidOperationException("Content Prefab pada ARImagePrefabSpawner masih kosong. Periksa prefab P11 terlebih dahulu.");

            string markerName = markerProperty != null && !string.IsNullOrWhiteSpace(markerProperty.stringValue)
                ? markerProperty.stringValue
                : DefaultMarkerName;

            // Hide only the diagnostic HUD in the P12 copy; keep P11 unchanged.
            Component diagnostic = xrOrigin.GetComponent("ARStatusOverlay");
            Behaviour diagnosticBehaviour = diagnostic as Behaviour;
            if (diagnosticBehaviour != null)
                diagnosticBehaviour.enabled = false;

            CreateController(imageManager, markerName);
            EnsureEventSystem();
            CreateCanvasAndUI();

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, P12ScenePath))
                throw new IOException("Unity gagal menyimpan scene P12 ke " + P12ScenePath + ".");

            AddP12ToBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeGameObject = GameObject.Find("ARUIController");

            EditorUtility.DisplayDialog(
                "P12 Berhasil Ditambahkan",
                "Scene P12 berhasil dibuat dari salinan P11.\n\n" +
                "P11 asli tidak diubah. Marker, Reference Image Library, prefab, dan skala spawner P11 dipertahankan.\n\n" +
                "Fitur P12: empat tombol Rotate/Scale, panel informasi, Safe Area, dan feedback audio.\n\n" +
                "Scene aktif: " + P12ScenePath + "\n\n" +
                "Berikutnya: Kelompok 4 AR > P12 > 2 - BUILD APK P12",
                "OK");

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("P12 Gagal Dibuat", ex.Message + "\n\nPeriksa error pertama pada Console.", "OK");
            return false;
        }
    }

    private static void CreateController(ARTrackedImageManager imageManager, string markerName)
    {
        GameObject controllerObject = new GameObject("ARUIController");
        ARObjectController controller = controllerObject.AddComponent<ARObjectController>();
        AudioSource audioSource = controllerObject.AddComponent<AudioSource>();

        AudioClip clickClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath);
        if (clickClip == null)
            throw new FileNotFoundException("Audio feedback tidak ditemukan: " + AudioPath);

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.35f;
        audioSource.clip = clickClip;

        controller.imageManager = imageManager;
        controller.targetReferenceName = markerName;
        controller.spawnedObjectName = SpawnedObjectName;
        controller.targetObject = null;
        controller.rotateStep = 15f;
        controller.scaleFactor = 1.15f;
        controller.minScaleMultiplier = 0.5f;
        controller.maxScaleMultiplier = 3f;
        controller.audioSource = audioSource;

        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(audioSource);
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.transform.SetAsLastSibling();
            return;
        }

        if (eventSystem.GetComponent<StandaloneInputModule>() == null &&
            eventSystem.GetComponent("InputSystemUIInputModule") == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }
    }

    private static void CreateCanvasAndUI()
    {
        GameObject canvasObject = new GameObject(
            "Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        scaler.referencePixelsPerUnit = 100f;

        GameObject safeAreaObject = new GameObject("SafeAreaRoot", typeof(RectTransform), typeof(ARUISafeArea));
        safeAreaObject.transform.SetParent(canvasObject.transform, false);
        RectTransform safeRect = safeAreaObject.GetComponent<RectTransform>();
        safeRect.anchorMin = Vector2.zero;
        safeRect.anchorMax = Vector2.one;
        safeRect.offsetMin = Vector2.zero;
        safeRect.offsetMax = Vector2.zero;

        CreateInfoPanel(safeAreaObject.transform);
        CreateControlPanel(safeAreaObject.transform);
    }

    private static void CreateInfoPanel(Transform parent)
    {
        RectTransform panel = CreatePanel(
            parent, "InfoPanel",
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
            new Vector2(1000f, 220f), new Vector2(0f, -38f),
            new Color32(75, 24, 57, 235));

        CreateText(panel, "TxtTitle", "PINK BOTANICAL GARDEN",
            42, new Color32(255, 232, 243, 255), FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-48f, 76f), new Vector2(0f, -22f));

        CreateText(panel, "TxtDescription",
            "Diorama taman bunga dengan kelopak merah muda, daun hijau, batang, dan ornamen dekoratif. Scan marker, lalu gunakan tombol untuk memutar dan mengubah ukuran objek.",
            24, Color.white, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),
            new Vector2(-70f, 100f), new Vector2(0f, -104f));
    }

    private static void CreateControlPanel(Transform parent)
    {
        RectTransform panel = CreatePanel(
            parent, "ControlPanel",
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(1000f, 470f), new Vector2(0f, 34f),
            new Color32(75, 24, 57, 238));

        CreateText(panel, "TxtControlTitle", "KONTROL OBJEK",
            30, new Color32(255, 232, 243, 255), FontStyle.Bold, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(700f, 48f), new Vector2(0f, 184f));

        ARObjectController controller = GameObject.Find("ARUIController").GetComponent<ARObjectController>();
        CreateButton(panel, "BtnRotateLeft", "ROTATE LEFT", new Vector2(-235f, 65f),
            new Color32(216, 70, 142, 255), controller.RotateLeft);
        CreateButton(panel, "BtnRotateRight", "ROTATE RIGHT", new Vector2(235f, 65f),
            new Color32(216, 70, 142, 255), controller.RotateRight);
        CreateButton(panel, "BtnScaleUp", "SCALE UP", new Vector2(-235f, -88f),
            new Color32(135, 54, 104, 255), controller.ScaleUp);
        CreateButton(panel, "BtnScaleDown", "SCALE DOWN", new Vector2(235f, -88f),
            new Color32(135, 54, 104, 255), controller.ScaleDown);

        CreateText(panel, "TxtControlHint", "Scan marker terlebih dahulu agar objek dapat dikendalikan.",
            18, new Color32(255, 225, 238, 255), FontStyle.Normal, TextAnchor.MiddleCenter,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
            new Vector2(900f, 34f), new Vector2(0f, 12f));
    }

    private static RectTransform CreatePanel(
        Transform parent, string objectName,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject panelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(parent, false);

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, anchorMin.y >= 0.99f ? 1f : anchorMin.y);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = panelObject.GetComponent<Image>();
        image.sprite = GetUiSprite();
        image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static void CreateButton(
        Transform parent, string objectName, string label, Vector2 position,
        Color normalColor, UnityAction action)
    {
        GameObject buttonObject = new GameObject(
            objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(440f, 122f);
        rect.anchoredPosition = position;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = GetUiSprite();
        image.type = image.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = normalColor;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = new Color(
            Mathf.Clamp01(normalColor.r + 0.10f),
            Mathf.Clamp01(normalColor.g + 0.10f),
            Mathf.Clamp01(normalColor.b + 0.10f), 1f);
        colors.pressedColor = new Color(
            Mathf.Clamp01(normalColor.r - 0.12f),
            Mathf.Clamp01(normalColor.g - 0.12f),
            Mathf.Clamp01(normalColor.b - 0.12f), 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.35f, 0.35f, 0.35f, 0.65f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        CreateText(rect, "Label", label, 30, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

        UnityEventTools.AddPersistentListener(button.onClick, action);
        EditorUtility.SetDirty(button);
    }

    private static Text CreateText(
        Transform parent, string objectName, string value,
        int fontSize, Color color, FontStyle style, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 sizeDelta, Vector2 anchoredPosition)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;

        Text text = textObject.GetComponent<Text>();
        // Unity 6 no longer exposes Arial.ttf as a built-in font.
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.font = font;
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = Mathf.Max(16, fontSize - 8);
        text.resizeTextMaxSize = fontSize;
        text.raycastTarget = false;
        return text;
    }

    private static Sprite GetUiSprite()
    {
        Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null)
            sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        return sprite;
    }

    private static void AddP12ToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        bool found = false;

        for (int i = 0; i < scenes.Count; i++)
        {
            if (scenes[i].path == P12ScenePath)
            {
                scenes[i] = new EditorBuildSettingsScene(P12ScenePath, true);
                found = true;
                break;
            }
        }

        if (!found)
            scenes.Add(new EditorBuildSettingsScene(P12ScenePath, true));

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void BuildP12Apk()
    {
        try
        {
            if (!File.Exists(ToFullPath(P12ScenePath)))
                throw new FileNotFoundException("Scene P12 belum dibuat: " + P12ScenePath);

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUtility.DisplayDialog(
                    "Platform Belum Android",
                    "Pilih Android pada File > Build Profiles, tunggu proses pergantian platform selesai, lalu jalankan menu Build APK P12 lagi. Project Settings tidak diubah oleh paket ini.",
                    "OK");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            AddP12ToBuildSettings();
            Directory.CreateDirectory(Path.GetDirectoryName(ToFullPath(ApkPath)));

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { P12ScenePath },
                locationPathName = ToFullPath(ApkPath),
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                EditorUtility.DisplayDialog(
                    "Build P12 Berhasil",
                    "APK berhasil dibuat:\n" + ApkPath + "\n\nInstal pada HP Android, izinkan kamera, lalu uji marker dan empat tombol.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Build P12 Belum Berhasil",
                    "Hasil: " + report.summary.result + "\nJumlah error: " + report.summary.totalErrors + "\n\nBuka Console dan periksa error pertama.",
                    "OK");
            }
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorUtility.DisplayDialog("Build P12 Gagal", ex.Message + "\n\nPeriksa Console untuk detail.", "OK");
        }
    }
}
#endif
