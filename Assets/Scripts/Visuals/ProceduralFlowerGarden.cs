using UnityEngine;

/// <summary>
/// Pink Botanical Garden diorama. Geometry is baked into the prefab by the
/// Editor builder; Start() only creates it as a safety fallback if the prefab
/// was not baked correctly.
/// </summary>
[DisallowMultipleComponent]
public class ProceduralFlowerGarden : MonoBehaviour
{
    [Header("Composition")]
    [SerializeField, Range(5, 15)] private int flowerCount = 9;
    [SerializeField] private bool createGardenArch = true;
    [SerializeField] private bool createDecorativePath = true;

    [Header("Persistent materials")]
    [SerializeField] private Material soil;
    [SerializeField] private Material grass;
    [SerializeField] private Material stem;
    [SerializeField] private Material leaf;
    [SerializeField] private Material petalPink;
    [SerializeField] private Material petalBlush;
    [SerializeField] private Material petalMagenta;
    [SerializeField] private Material gold;
    [SerializeField] private Material pearl;

    [SerializeField, HideInInspector] private bool geometryBuilt;

    private void Start()
    {
        // Prefab normally contains baked children. This fallback prevents an
        // empty display if the user skipped the Editor builder.
        if (!geometryBuilt || transform.childCount == 0)
            BuildGarden();
    }

    /// <summary>Called by the Unity Editor builder before saving the prefab.</summary>
    public void BuildGarden()
    {
        if (geometryBuilt && transform.childCount > 0)
            return;

        ClearGeneratedChildren();
        if (AnyMaterialMissing())
            CreateRuntimeMaterials();

        CreateBase();
        CreateFlowers();
        CreateDetails();
        geometryBuilt = true;
    }

    private bool AnyMaterialMissing()
    {
        return soil == null || grass == null || stem == null || leaf == null ||
               petalPink == null || petalBlush == null || petalMagenta == null ||
               gold == null || pearl == null;
    }

    private void ClearGeneratedChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }

    private Material CreateRuntimeMaterial(string materialName, Color color, float metallic = 0f)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;
        if (material.HasProperty("_Metallic"))
            material.SetFloat("_Metallic", metallic);
        return material;
    }

    private void CreateRuntimeMaterials()
    {
        if (soil == null) soil = CreateRuntimeMaterial("MAT_RoseQuartz_Soil", new Color(0.48f, 0.20f, 0.34f));
        if (grass == null) grass = CreateRuntimeMaterial("MAT_Sage_Garden", new Color(0.22f, 0.48f, 0.34f));
        if (stem == null) stem = CreateRuntimeMaterial("MAT_Botanical_Stem", new Color(0.12f, 0.34f, 0.20f));
        if (leaf == null) leaf = CreateRuntimeMaterial("MAT_Botanical_Leaf", new Color(0.28f, 0.62f, 0.37f));
        if (petalPink == null) petalPink = CreateRuntimeMaterial("MAT_Petal_Pink", new Color(0.95f, 0.35f, 0.60f));
        if (petalBlush == null) petalBlush = CreateRuntimeMaterial("MAT_Petal_Blush", new Color(1.00f, 0.68f, 0.78f));
        if (petalMagenta == null) petalMagenta = CreateRuntimeMaterial("MAT_Petal_Magenta", new Color(0.66f, 0.12f, 0.38f));
        if (gold == null) gold = CreateRuntimeMaterial("MAT_Pollen_Gold", new Color(1.00f, 0.72f, 0.18f), 0.20f);
        if (pearl == null) pearl = CreateRuntimeMaterial("MAT_Pearl_Stone", new Color(0.94f, 0.78f, 0.85f), 0.12f);
    }

    private GameObject CreatePrimitive(
        PrimitiveType type, string objectName, Vector3 localPosition,
        Vector3 localScale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = objectName;
        go.transform.SetParent(transform, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = localScale;

        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            renderer.receiveShadows = true;
        }

        Collider collider = go.GetComponent<Collider>();
        if (collider != null)
        {
            if (Application.isPlaying)
                Destroy(collider);
            else
                DestroyImmediate(collider);
        }

        return go;
    }

    private void CreateBase()
    {
        CreatePrimitive(PrimitiveType.Cylinder, "Diorama_Pearl_Base",
            new Vector3(0f, -0.08f, 0f), new Vector3(2.15f, 0.16f, 2.15f), pearl);

        CreatePrimitive(PrimitiveType.Cylinder, "Diorama_RoseQuartz_Soil",
            new Vector3(0f, 0.005f, 0f), new Vector3(1.96f, 0.045f, 1.96f), soil);

        CreatePrimitive(PrimitiveType.Cylinder, "Diorama_Sage_Lawn",
            new Vector3(0f, 0.032f, 0f), new Vector3(1.82f, 0.025f, 1.82f), grass);

        for (int i = 0; i < 16; i++)
        {
            float angle = i * Mathf.PI * 2f / 16f;
            Vector3 position = new Vector3(Mathf.Cos(angle) * 0.94f, 0.06f,
                                           Mathf.Sin(angle) * 0.94f);
            CreatePrimitive(PrimitiveType.Sphere, "Border_Gold_Stud_" + i,
                position, Vector3.one * 0.045f, gold);
        }
    }

    private void CreateFlowers()
    {
        for (int i = 0; i < flowerCount; i++)
        {
            float angle = i * Mathf.PI * 2f / flowerCount;
            float radius = (i % 2 == 0) ? 0.46f : 0.68f;
            float height = 0.36f + (i % 3) * 0.075f;

            Vector3 basePosition = new Vector3(Mathf.Cos(angle) * radius, 0.06f,
                                               Mathf.Sin(angle) * radius);

            GameObject stemObject = CreatePrimitive(PrimitiveType.Cylinder,
                "FlowerStem_" + i, basePosition + Vector3.up * (height * 0.5f),
                new Vector3(0.022f, height, 0.022f), stem);
            stemObject.transform.localRotation = Quaternion.Euler(0f, 0f,
                (i % 2 == 0 ? -1f : 1f) * 7f);

            for (int leafIndex = 0; leafIndex < 2; leafIndex++)
            {
                Vector3 leafPosition = basePosition +
                    Vector3.up * (height * (0.34f + leafIndex * 0.22f)) +
                    new Vector3(leafIndex == 0 ? -0.095f : 0.095f, 0f, 0f);

                GameObject leafObject = CreatePrimitive(PrimitiveType.Sphere,
                    "FlowerLeaf_" + i + "_" + leafIndex, leafPosition,
                    new Vector3(0.17f, 0.032f, 0.07f), leaf);
                leafObject.transform.localRotation =
                    Quaternion.Euler(0f, 0f, leafIndex == 0 ? 24f : -24f);
            }

            Vector3 flowerHead = basePosition + Vector3.up * height;
            Material petalMaterial = (i % 3 == 0) ? petalMagenta :
                                      (i % 3 == 1) ? petalPink : petalBlush;

            for (int petalIndex = 0; petalIndex < 8; petalIndex++)
            {
                float petalAngle = petalIndex * Mathf.PI * 2f / 8f;
                Vector3 offset = new Vector3(Mathf.Cos(petalAngle) * 0.105f,
                    0f, Mathf.Sin(petalAngle) * 0.105f);

                GameObject petal = CreatePrimitive(PrimitiveType.Sphere,
                    "FlowerPetal_" + i + "_" + petalIndex, flowerHead + offset,
                    new Vector3(0.12f, 0.043f, 0.073f), petalMaterial);
                petal.transform.localRotation =
                    Quaternion.Euler(0f, -petalAngle * Mathf.Rad2Deg, 0f);
            }

            CreatePrimitive(PrimitiveType.Sphere, "FlowerPollen_" + i,
                flowerHead + Vector3.up * 0.018f, Vector3.one * 0.085f, gold);
        }
    }

    private void CreateDetails()
    {
        if (createDecorativePath)
        {
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                Vector3 p = Vector3.Lerp(new Vector3(-0.62f, 0.07f, -0.12f),
                                         new Vector3(0.62f, 0.07f, 0.12f), t);
                CreatePrimitive(PrimitiveType.Sphere, "Pearl_Path_Stone_" + i,
                    p, new Vector3(0.12f, 0.035f, 0.085f), pearl);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            float angle = i * Mathf.PI * 2f / 5f;
            Vector3 p = new Vector3(Mathf.Cos(angle) * 0.78f, 0.09f,
                                    Mathf.Sin(angle) * 0.78f);
            CreatePrimitive(PrimitiveType.Sphere, "Golden_Bud_" + i,
                p, Vector3.one * 0.042f, gold);
        }

        if (createGardenArch)
            CreateArch();
    }

    private void CreateArch()
    {
        const int segments = 18;
        float radiusX = 0.25f;
        float radiusY = 0.25f;
        Vector3 center = new Vector3(0f, 0.43f, -0.47f);

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.PI * i / segments;
            Vector3 p = center + new Vector3(Mathf.Cos(angle) * radiusX,
                                              Mathf.Sin(angle) * radiusY, 0f);
            CreatePrimitive(PrimitiveType.Sphere, "RoseArch_Segment_" + i,
                p, new Vector3(0.055f, 0.055f, 0.055f), petalMagenta);
        }
    }
}
