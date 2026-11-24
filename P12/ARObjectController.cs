using UnityEngine;
using UnityEngine.XR.ARFoundation;

/// <summary>
/// P12 controller for the Pink Botanical Garden spawned by the existing P11
/// ARImagePrefabSpawner. It discovers the runtime child under the tracked image,
/// so it does not require changing the existing spawner script.
/// </summary>
[DisallowMultipleComponent]
public class ARObjectController : MonoBehaviour
{
    [Header("Existing P11 Image Tracking")]
    public ARTrackedImageManager imageManager;
    public string targetReferenceName = "Marker_PinkBotanical";
    public string spawnedObjectName = "AR_PinkBotanicalGarden_Instance";
    public Transform targetObject;

    [Header("Rotation")]
    [Min(1f)] public float rotateStep = 15f;

    [Header("Scale")]
    [Range(1.01f, 1.5f)] public float scaleFactor = 1.15f;
    [Min(0.1f)] public float minScaleMultiplier = 0.5f;
    [Min(1f)] public float maxScaleMultiplier = 3f;

    [Header("Feedback")]
    public AudioSource audioSource;

    private Transform cachedTarget;
    private Vector3 originalScale = Vector3.one;
    private bool hasOriginalScale;
    private bool warningShown;

    private void Awake()
    {
        if (imageManager == null)
            imageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    /// <summary>Finds the live instance created by the P11 image spawner.</summary>
    public void FindTargetIfNeeded()
    {
        if (targetObject != null && targetObject.gameObject.activeInHierarchy)
        {
            CacheOriginalScale(targetObject);
            return;
        }

        targetObject = null;

        if (imageManager == null)
            imageManager = FindObjectOfType<ARTrackedImageManager>();

        if (imageManager != null)
        {
            foreach (ARTrackedImage trackedImage in imageManager.trackables)
            {
                if (trackedImage == null)
                    continue;

                if (trackedImage.referenceImage.name != targetReferenceName)
                    continue;

                Transform candidate = FindSpawnedChild(trackedImage.transform);
                if (candidate != null && candidate.gameObject.activeInHierarchy)
                {
                    targetObject = candidate;
                    CacheOriginalScale(targetObject);
                    return;
                }
            }
        }

        // Fallback for scenes where the tracked image is already active but the
        // trackable collection has not been refreshed in this frame.
        GameObject fallback = GameObject.Find(spawnedObjectName);
        if (fallback != null && fallback.activeInHierarchy)
        {
            targetObject = fallback.transform;
            CacheOriginalScale(targetObject);
        }
    }

    private Transform FindSpawnedChild(Transform trackedImageTransform)
    {
        Transform exact = trackedImageTransform.Find(spawnedObjectName);
        if (exact != null)
            return exact;

        // Compatibility fallback: find the first direct child containing a
        // renderer. The P11 spawner's current instance is named above.
        for (int i = 0; i < trackedImageTransform.childCount; i++)
        {
            Transform child = trackedImageTransform.GetChild(i);
            if (child.GetComponentInChildren<Renderer>(true) != null)
                return child;
        }

        return null;
    }

    private void CacheOriginalScale(Transform candidate)
    {
        if (candidate == null || cachedTarget == candidate)
            return;

        cachedTarget = candidate;
        originalScale = candidate.localScale;
        hasOriginalScale = true;
    }

    private bool TargetReady()
    {
        FindTargetIfNeeded();

        if (targetObject == null || !targetObject.gameObject.activeInHierarchy)
        {
            if (!warningShown)
            {
                Debug.LogWarning("[P12 AR] Objek belum ditemukan. Arahkan kamera ke marker Pink Botanical terlebih dahulu.");
                warningShown = true;
            }
            return false;
        }

        warningShown = false;
        return true;
    }

    public void RotateLeft()
    {
        if (!TargetReady()) return;
        targetObject.Rotate(0f, -rotateStep, 0f, Space.Self);
        PlayFeedback();
    }

    public void RotateRight()
    {
        if (!TargetReady()) return;
        targetObject.Rotate(0f, rotateStep, 0f, Space.Self);
        PlayFeedback();
    }

    public void ScaleUp()
    {
        if (!TargetReady()) return;
        ApplyScale(scaleFactor);
        PlayFeedback();
    }

    public void ScaleDown()
    {
        if (!TargetReady()) return;
        ApplyScale(1f / scaleFactor);
        PlayFeedback();
    }

    private void ApplyScale(float factor)
    {
        if (!hasOriginalScale)
            CacheOriginalScale(targetObject);

        Vector3 current = targetObject.localScale;
        float currentX = Mathf.Max(Mathf.Abs(current.x), 0.0001f);
        float baseX = Mathf.Max(Mathf.Abs(originalScale.x), 0.0001f);
        float minimum = baseX * minScaleMultiplier;
        float maximum = baseX * maxScaleMultiplier;
        float nextX = Mathf.Clamp(currentX * factor, minimum, maximum);
        float ratio = nextX / currentX;
        targetObject.localScale = current * ratio;
    }

    private void PlayFeedback()
    {
        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);
    }
}
