using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Robust image-target content spawner for AR Foundation 6.x.
/// Creates the FBX-backed content prefab when the configured reference image
/// is added/updated, keeps it visible while tracking is Tracking or Limited,
/// and exposes a diagnostic state for the on-screen status overlay.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(ARTrackedImageManager))]
public class ARImagePrefabSpawner : MonoBehaviour
{
    [Header("Tracking")]
    [SerializeField] private ARTrackedImageManager imageManager;
    [SerializeField] private string targetReferenceName = "Marker_PinkBotanical";
    [SerializeField] private bool hideWhenTrackingIsLimited = false;
    [SerializeField] private bool writeTrackingLogs = true;

    [Header("AR Content Prefab")]
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private Vector3 localPosition = new Vector3(0f, 0.015f, 0f);
    [SerializeField] private Vector3 localEulerAngles = Vector3.zero;
    [SerializeField, Range(0.02f, 0.20f)] private float uniformScale = 0.07f;

    private readonly Dictionary<TrackableId, GameObject> spawnedContent =
        new Dictionary<TrackableId, GameObject>();
    private bool subscribed;
    private string diagnosticStatus = "WAITING FOR MARKER";
    private string lastReferenceName = "-";

    public string DiagnosticStatus { get { return diagnosticStatus; } }
    public int SpawnedObjectCount { get { return spawnedContent.Count; } }

    private void Reset()
    {
        imageManager = GetComponent<ARTrackedImageManager>();
    }

    private void Awake()
    {
        ResolveImageManager();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        // Start retries subscription in case component enable order changed.
        TrySubscribe();
        if (contentPrefab == null)
            SetDiagnostic("ERROR: CONTENT PREFAB IS EMPTY");
        else if (imageManager == null)
            SetDiagnostic("ERROR: IMAGE MANAGER IS MISSING");
        else
            SetDiagnostic("READY - SEARCHING FOR " + targetReferenceName);
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void OnValidate()
    {
        uniformScale = Mathf.Clamp(uniformScale, 0.02f, 0.20f);
        if (string.IsNullOrWhiteSpace(targetReferenceName))
            targetReferenceName = "Marker_PinkBotanical";
    }

    private void ResolveImageManager()
    {
        if (imageManager == null)
            imageManager = GetComponent<ARTrackedImageManager>();
    }

    private void TrySubscribe()
    {
        ResolveImageManager();
        if (subscribed)
            return;
        if (imageManager == null)
        {
            SetDiagnostic("ERROR: AR TRACKED IMAGE MANAGER NOT FOUND");
            Debug.LogError("[AR Kelompok 4] ARTrackedImageManager tidak ditemukan pada XR Origin.");
            return;
        }

        imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        subscribed = true;
        if (writeTrackingLogs)
            Debug.Log("[AR Kelompok 4] Spawner subscribed to ARTrackedImageManager. Target=" + targetReferenceName);
    }

    private void Unsubscribe()
    {
        if (!subscribed || imageManager == null)
            return;
        imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        subscribed = false;
    }

    private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> changes)
    {
        foreach (ARTrackedImage image in changes.added)
            UpdateTrackedImage(image, "ADDED");

        foreach (ARTrackedImage image in changes.updated)
            UpdateTrackedImage(image, "UPDATED");

        foreach (var removed in changes.removed)
        {
            TrackableId id = removed.Key;
            if (spawnedContent.TryGetValue(id, out GameObject content))
            {
                if (content != null)
                    Destroy(content);
                spawnedContent.Remove(id);
                SetDiagnostic("TARGET REMOVED - SEARCHING AGAIN");
                if (writeTrackingLogs)
                    Debug.Log("[AR Kelompok 4] Target removed; spawned content destroyed. Remaining=" + spawnedContent.Count);
            }
        }
    }

    private void UpdateTrackedImage(ARTrackedImage image, string eventType)
    {
        if (image == null)
            return;

        lastReferenceName = image.referenceImage.name;

        if (!string.Equals(lastReferenceName, targetReferenceName, System.StringComparison.Ordinal))
        {
            SetDiagnostic("IMAGE FOUND, NAME MISMATCH: " + lastReferenceName);
            if (writeTrackingLogs && eventType == "ADDED")
                Debug.LogWarning("[AR Kelompok 4] Image terdeteksi tetapi namanya tidak cocok. Detected='" + lastReferenceName + "', expected='" + targetReferenceName + "'.");
            return;
        }

        if (contentPrefab == null)
        {
            SetDiagnostic("ERROR: CONTENT PREFAB IS EMPTY");
            Debug.LogError("[AR Kelompok 4] Marker cocok, tetapi Content Prefab belum diisi.");
            return;
        }

        if (!spawnedContent.TryGetValue(image.trackableId, out GameObject content) || content == null)
        {
            content = Instantiate(contentPrefab, image.transform, false);
            content.name = "AR_PinkBotanicalGarden_Instance";
            content.transform.localPosition = localPosition;
            content.transform.localRotation = Quaternion.Euler(localEulerAngles);
            content.transform.localScale = Vector3.one * uniformScale;
            content.SetActive(true);

            Renderer[] renderers = content.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                SetDiagnostic("ERROR: PREFAB HAS NO RENDERERS");
                Debug.LogError("[AR Kelompok 4] Prefab berhasil di-instantiate tetapi tidak memiliki Renderer.");
            }
            else
            {
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                    renderer.gameObject.SetActive(true);
                }
                SetDiagnostic("PREFAB CREATED - " + renderers.Length + " RENDERERS");
                if (writeTrackingLogs)
                    Debug.Log("[AR Kelompok 4] FBX prefab instantiated. Target=" + lastReferenceName +
                              ", Renderers=" + renderers.Length + ", TrackableId=" + image.trackableId);
            }

            spawnedContent[image.trackableId] = content;
        }

        bool isVisible = image.trackingState == TrackingState.Tracking ||
                         (!hideWhenTrackingIsLimited && image.trackingState == TrackingState.Limited);
        content.SetActive(isVisible);

        if (isVisible)
            SetDiagnostic("3D CONTENT VISIBLE - " + image.trackingState);
        else
            SetDiagnostic("TARGET MATCHED - WAITING FOR TRACKING");

        if (writeTrackingLogs && eventType == "ADDED")
            Debug.Log("[AR Kelompok 4] Target matched. Name=" + lastReferenceName +
                      ", TrackingState=" + image.trackingState + ", Visible=" + isVisible);
    }

    private void SetDiagnostic(string value)
    {
        diagnosticStatus = value;
    }

    public void SetUniformScale(float newScale)
    {
        uniformScale = Mathf.Clamp(newScale, 0.02f, 0.20f);
        foreach (GameObject content in spawnedContent.Values)
        {
            if (content != null)
                content.transform.localScale = Vector3.one * uniformScale;
        }
    }
}
