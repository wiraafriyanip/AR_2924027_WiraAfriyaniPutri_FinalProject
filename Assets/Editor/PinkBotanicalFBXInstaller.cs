
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[Serializable]
public sealed class PinkBotanicalMeshCollectionData
{
    public PinkBotanicalMeshData[] meshes;
}

[Serializable]
public sealed class PinkBotanicalMeshData
{
    public string name;
    public float[] vertices;
    public float[] normals;
}

/// <summary>
/// Builds the Pink Botanical Garden prefab from validated mesh data.
/// Bypasses Unity FBX ModelImporter.
/// </summary>
public static class PinkBotanicalFBXInstaller
{
    // Compatibility method required by ARProjectSetupWizard.cs.
    public static GameObject BuildPrefabFromFBX()
    {
        return BuildPrefabFromMeshData();
    }

    private const string MeshDataPath =
        "Assets/Models/PinkBotanicalGarden_MeshData.json";

    private const string MeshAssetFolder =
        "Assets/Models/PinkBotanicalGarden_GeneratedMeshes";

    private const string PrefabPath =
        "Assets/Prefabs/AR/Prefab_PinkBotanicalGarden.prefab";

    private const string P11Path =
        "Assets/Scenes/P11_ObjectOnMarker.unity";

    private const string LibraryPath =
        "Assets/ReferenceImages/RefImageLibrary_FinalProject.asset";

    private const string MarkerName = "Marker_PinkBotanical";

    private const float TargetHorizontalDiameter = 2.0f;
    private const float SpawnerScale = 0.07f;

    private static readonly Dictionary<string, string> MaterialPaths =
        new Dictionary<string, string>
    {
        { "MAT_Pearl_Stone", "Assets/Materials/MAT_Pearl_Stone.mat" },
        { "MAT_RoseQuartz_Soil", "Assets/Materials/MAT_RoseQuartz_Soil.mat" },
        { "MAT_Sage_Garden", "Assets/Materials/MAT_Sage_Garden.mat" },
        { "MAT_Botanical_Stem", "Assets/Materials/MAT_Botanical_Stem.mat" },
        { "MAT_Botanical_Leaf", "Assets/Materials/MAT_Botanical_Leaf.mat" },
        { "MAT_Petal_Pink", "Assets/Materials/MAT_Petal_Pink.mat" },
        { "MAT_Petal_Blush", "Assets/Materials/MAT_Petal_Blush.mat" },
        { "MAT_Petal_Magenta", "Assets/Materials/MAT_Petal_Magenta.mat" },
        { "MAT_Pollen_Gold", "Assets/Materials/MAT_Pollen_Gold.mat" }
    };

    [MenuItem(
        "Kelompok 4 AR/1 - BUILD P11 PREFAB FROM REPAIRED MESH DATA",
        false,
        1)]
    public static void InstallAndWireP11Menu()
    {
        try
        {
            GameObject prefab = BuildPrefabFromMeshData();
            WireP11Scene(prefab);

            EditorUtility.DisplayDialog(
                "Pink Botanical Garden Ready",
                "Model berhasil dibuat tanpa FBX ModelImporter.\n\n" +
                "Mesh: 9\n" +
                "Prefab: " + PrefabPath + "\n" +
                "Scene: " + P11Path + "\n\n" +
                "Periksa Prefab dan komponen AR Tracked Image Manager sebelum Build APK.",
                "OK");
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);

            EditorUtility.DisplayDialog(
                "Pink Botanical Build Failed",
                ex.Message + "\n\nPeriksa Console untuk detail.",
                "OK");
        }
    }

    public static GameObject BuildPrefabFromMeshData()
    {
        EnsureFolder("Assets/Models");
        EnsureFolder(MeshAssetFolder);
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/AR");

        AssetDatabase.ImportAsset(
            MeshDataPath,
            ImportAssetOptions.ForceSynchronousImport);

        TextAsset jsonAsset =
            AssetDatabase.LoadAssetAtPath<TextAsset>(MeshDataPath);

        if (jsonAsset == null)
        {
            throw new FileNotFoundException(
                "File data mesh tidak ditemukan. Letakkan " +
                "PinkBotanicalGarden_MeshData.json di Assets/Models/.");
        }

        PinkBotanicalMeshCollectionData collection =
            JsonUtility.FromJson<PinkBotanicalMeshCollectionData>(
                jsonAsset.text);

        if (collection == null ||
            collection.meshes == null ||
            collection.meshes.Length != 9)
        {
            throw new InvalidDataException(
                "Data mesh tidak lengkap. Installer membutuhkan tepat 9 kelompok mesh.");
        }

        Dictionary<string, Material> materials =
            LoadProjectMaterials();

        GameObject root =
            new GameObject("Prefab_PinkBotanicalGarden");

        GameObject modelRoot =
            new GameObject("PinkBotanicalGarden_Model");

        modelRoot.transform.SetParent(root.transform, false);

        try
        {
            int totalVertices = 0;
            int totalTriangles = 0;

            foreach (PinkBotanicalMeshData data in collection.meshes)
            {
                ValidateMeshData(data);

                Material material;

                if (!materials.TryGetValue(data.name, out material))
                {
                    throw new InvalidDataException(
                        "Tidak ada material project yang cocok dengan mesh: " +
                        data.name);
                }

                Mesh mesh = CreateAndSaveMeshAsset(data);

                GameObject meshObject = new GameObject(data.name);

                meshObject.transform.SetParent(
                    modelRoot.transform,
                    false);

                MeshFilter filter =
                    meshObject.AddComponent<MeshFilter>();

                filter.sharedMesh = mesh;

                MeshRenderer renderer =
                    meshObject.AddComponent<MeshRenderer>();

                renderer.sharedMaterial = material;
                renderer.enabled = true;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.receiveShadows = true;

                totalVertices += mesh.vertexCount;
                totalTriangles += mesh.triangles.Length / 3;
            }

            Renderer[] renderers =
                modelRoot.GetComponentsInChildren<Renderer>(true);

            if (renderers.Length != 9)
            {
                throw new InvalidOperationException(
                    "Validasi renderer gagal. Seharusnya 9 renderer, ditemukan " +
                    renderers.Length + ".");
            }

            Bounds originalBounds =
                CalculateCombinedBounds(renderers);

            float horizontalDiameter =
                Mathf.Max(
                    originalBounds.size.x,
                    originalBounds.size.z);

            if (horizontalDiameter <= 0.0001f)
            {
                throw new InvalidOperationException(
                    "Ukuran horizontal model tidak valid: " +
                    horizontalDiameter);
            }

            float correctionScale =
                TargetHorizontalDiameter / horizontalDiameter;

            modelRoot.transform.localScale =
                Vector3.one * correctionScale;

            Bounds scaledBounds =
                CalculateCombinedBounds(renderers);

            modelRoot.transform.localPosition =
                new Vector3(
                    -scaledBounds.center.x,
                    -scaledBounds.min.y,
                    -scaledBounds.center.z);

            Bounds finalBounds =
                CalculateCombinedBounds(renderers);

            if (Mathf.Abs(finalBounds.min.y) > 0.02f)
            {
                throw new InvalidOperationException(
                    "Validasi posisi dasar model gagal. Nilai dasar Y = " +
                    finalBounds.min.y.ToString("F4"));
            }

            bool saved;

            GameObject savedPrefab =
                PrefabUtility.SaveAsPrefabAsset(
                    root,
                    PrefabPath,
                    out saved);

            if (!saved || savedPrefab == null)
            {
                throw new InvalidOperationException(
                    "Prefab gagal disimpan ke " + PrefabPath + ".");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject reloaded =
                AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);

            if (reloaded == null)
            {
                throw new InvalidOperationException(
                    "Prefab sudah dibuat tetapi gagal dimuat ulang dari AssetDatabase.");
            }

            MeshFilter[] savedFilters =
                reloaded.GetComponentsInChildren<MeshFilter>(true);

            Renderer[] savedRenderers =
                reloaded.GetComponentsInChildren<Renderer>(true);

            if (savedFilters.Length != 9 ||
                savedRenderers.Length != 9)
            {
                throw new InvalidOperationException(
                    "Prefab tidak lolos validasi akhir. MeshFilter=" +
                    savedFilters.Length +
                    ", Renderer=" + savedRenderers.Length + ".");
            }

            Debug.Log(
                "[Kelompok 4 AR] Pink Botanical prefab berhasil dibuat. " +
                "Meshes=9, MeshFilters=" + savedFilters.Length +
                ", Renderers=" + savedRenderers.Length +
                ", Vertices=" + totalVertices +
                ", Triangles=" + totalTriangles +
                ", Scale=" + correctionScale.ToString("F6") +
                ", Prefab=" + PrefabPath);

            return reloaded;
        }
        finally
        {
            if (root != null)
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    private static void ValidateMeshData(
        PinkBotanicalMeshData data)
    {
        if (data == null || string.IsNullOrEmpty(data.name))
        {
            throw new InvalidDataException(
                "Ada kelompok mesh tanpa nama.");
        }

        if (data.vertices == null ||
            data.normals == null ||
            data.vertices.Length == 0 ||
            data.vertices.Length % 9 != 0)
        {
            throw new InvalidDataException(
                "Data vertex mesh " + data.name +
                " kosong atau bukan kumpulan segitiga yang valid.");
        }

        if (data.normals.Length != data.vertices.Length)
        {
            throw new InvalidDataException(
                "Jumlah normal tidak sama dengan jumlah vertex pada mesh " +
                data.name + ".");
        }

        for (int i = 0; i < data.vertices.Length; i++)
        {
            if (float.IsNaN(data.vertices[i]) ||
                float.IsInfinity(data.vertices[i]) ||
                float.IsNaN(data.normals[i]) ||
                float.IsInfinity(data.normals[i]))
            {
                throw new InvalidDataException(
                    "Ditemukan nilai NaN/Infinity pada mesh " +
                    data.name + ".");
            }
        }
    }

    private static Mesh CreateAndSaveMeshAsset(
        PinkBotanicalMeshData data)
    {
        int vertexCount = data.vertices.Length / 3;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        int[] triangles = new int[vertexCount];

        for (int i = 0; i < vertexCount; i++)
        {
            int offset = i * 3;

            vertices[i] = new Vector3(
                data.vertices[offset],
                data.vertices[offset + 1],
                data.vertices[offset + 2]);

            Vector3 normal = new Vector3(
                data.normals[offset],
                data.normals[offset + 1],
                data.normals[offset + 2]);

            if (normal.sqrMagnitude < 0.25f)
            {
                throw new InvalidDataException(
                    "Ditemukan normal yang tidak valid pada mesh " +
                    data.name + ".");
            }

            normals[i] = normal.normalized;
            triangles[i] = i;
        }

        Mesh mesh = new Mesh();

        mesh.name = data.name;
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();

        string assetPath =
            MeshAssetFolder + "/" + data.name + ".asset";

        if (AssetDatabase.LoadAssetAtPath<Mesh>(assetPath) != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        AssetDatabase.CreateAsset(mesh, assetPath);

        return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
    }

    private static Dictionary<string, Material> LoadProjectMaterials()
    {
        Dictionary<string, Material> loaded =
            new Dictionary<string, Material>();

        foreach (KeyValuePair<string, string> entry in MaterialPaths)
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(entry.Value);

            if (material == null)
            {
                throw new FileNotFoundException(
                    "Material project tidak ditemukan: " +
                    entry.Value +
                    ". Pastikan folder Assets/Materials tidak berubah.");
            }

            loaded.Add(entry.Key, material);
        }

        return loaded;
    }

    private static Bounds CalculateCombinedBounds(
        Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0)
        {
            throw new InvalidOperationException(
                "Tidak ada Renderer untuk menghitung ukuran model.");
        }

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void EnsureFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent =
            Path.GetDirectoryName(folderPath).Replace("\\", "/");

        string folderName =
            Path.GetFileName(folderPath);

        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, folderName);
    }

    public static void WireP11Scene(GameObject prefab)
    {
        if (prefab == null)
        {
            throw new ArgumentNullException("prefab");
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(P11Path) == null)
        {
            throw new FileNotFoundException(
                "Scene P11 tidak ditemukan: " + P11Path);
        }

        XRReferenceImageLibrary library =
            AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(
                LibraryPath);

        if (library == null || library.count == 0)
        {
            throw new InvalidOperationException(
                "Reference Image Library kosong atau tidak ditemukan: " +
                LibraryPath);
        }

        Scene previousActiveScene =
            SceneManager.GetActiveScene();

        Scene p11Scene =
            SceneManager.GetSceneByPath(P11Path);

        bool openedHere = false;

        try
        {
            if (!p11Scene.IsValid() || !p11Scene.isLoaded)
            {
                p11Scene = EditorSceneManager.OpenScene(
                    P11Path,
                    OpenSceneMode.Additive);

                openedHere = true;
            }

            GameObject origin =
                FindInScene(p11Scene, "XR Origin");

            if (origin == null)
            {
                throw new InvalidOperationException(
                    "XR Origin tidak ditemukan di scene P11. " +
                    "Tidak ada objek yang ditambahkan ke Hierarchy.");
            }

            ARTrackedImageManager manager =
                origin.GetComponent<ARTrackedImageManager>();

            if (manager == null)
            {
                throw new InvalidOperationException(
                    "ARTrackedImageManager tidak ditemukan pada XR Origin.");
            }

            ARImagePrefabSpawner spawner =
                origin.GetComponent<ARImagePrefabSpawner>();

            ARStatusOverlay overlay =
                origin.GetComponent<ARStatusOverlay>();

            if (spawner == null)
            {
                spawner = origin.AddComponent<ARImagePrefabSpawner>();
            }

            spawner.enabled = false;
            manager.enabled = false;

            manager.referenceLibrary = library;
            manager.requestedMaxNumberOfMovingImages = 1;
            manager.enabled = true;

            SerializedObject serializedSpawner =
                new SerializedObject(spawner);

            RequireProperty(
                serializedSpawner,
                "imageManager").objectReferenceValue = manager;

            RequireProperty(
                serializedSpawner,
                "targetReferenceName").stringValue = MarkerName;

            RequireProperty(
                serializedSpawner,
                "hideWhenTrackingIsLimited").boolValue = false;

            RequireProperty(
                serializedSpawner,
                "writeTrackingLogs").boolValue = true;

            RequireProperty(
                serializedSpawner,
                "contentPrefab").objectReferenceValue = prefab;

            RequireProperty(
                serializedSpawner,
                "localPosition").vector3Value =
                new Vector3(0f, 0.015f, 0f);

            RequireProperty(
                serializedSpawner,
                "localEulerAngles").vector3Value = Vector3.zero;

            RequireProperty(
                serializedSpawner,
                "uniformScale").floatValue = SpawnerScale;

            serializedSpawner.ApplyModifiedPropertiesWithoutUndo();

            spawner.enabled = true;

            if (overlay != null)
            {
                overlay.enabled = true;
                EditorUtility.SetDirty(overlay);
            }

            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(spawner);

            EditorSceneManager.MarkSceneDirty(p11Scene);
            EditorSceneManager.SaveScene(p11Scene);

            AssetDatabase.SaveAssets();

            Debug.Log(
                "[Kelompok 4 AR] P11 berhasil dihubungkan ke prefab. " +
                "Marker=" + MarkerName +
                ", Prefab=" + PrefabPath +
                ", XR Origin tetap pada posisi Hierarchy semula.");
        }
        finally
        {
            if (openedHere &&
                p11Scene.IsValid() &&
                p11Scene.isLoaded)
            {
                if (previousActiveScene.IsValid() &&
                    previousActiveScene.isLoaded)
                {
                    SceneManager.SetActiveScene(previousActiveScene);
                }

                EditorSceneManager.CloseScene(p11Scene, true);
            }
        }
    }

    private static GameObject FindInScene(
        Scene scene,
        string objectName)
    {
        foreach (GameObject sceneRoot in scene.GetRootGameObjects())
        {
            Transform[] transforms =
                sceneRoot.GetComponentsInChildren<Transform>(true);

            foreach (Transform item in transforms)
            {
                if (item.name == objectName)
                {
                    return item.gameObject;
                }
            }
        }

        return null;
    }

    private static SerializedProperty RequireProperty(
        SerializedObject serializedObject,
        string propertyName)
    {
        SerializedProperty property =
            serializedObject.FindProperty(propertyName);

        if (property == null)
        {
            throw new InvalidOperationException(
                "Field '" + propertyName +
                "' tidak ditemukan pada ARImagePrefabSpawner. " +
                "Periksa nama field di script ARImagePrefabSpawner.cs.");
        }

        return property;
    }
}
#endif
