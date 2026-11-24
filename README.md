# AR KELOMPOK 4 — PINK BOTANICAL GARDEN AR
## Paket project Unity untuk P09, P10, dan P11

**Versi Editor terkunci:** Unity 6000.5.10f1  
**AR Foundation:** 6.5.1  
**Google ARCore XR Plugin:** 6.5.1  
**XR Plug-in Management:** 4.6.1  
**Package ID:** `com.kelompok4.pinkbotanicalar`

Paket ini berisi source project, marker, script, material, dokumentasi, dan satu-click Editor builder. Builder membuat scene P09/P10/P11, XR Reference Image Library, serta prefab diorama taman bunga 3D dengan geometri yang sudah dibake ke dalam prefab (bukan hanya muncul ketika Play).

## 1. Instalasi project
1. Extract ZIP ke folder baru, misalnya `C:/UnityProjects/AR_Kelompok4_Unity6000.5.10f1`.
2. Unity Hub > Projects > Add > Add project from disk.
3. Pilih folder project yang berisi `Assets`, `Packages`, dan `ProjectSettings`.
4. Buka menggunakan Unity `6000.5.10f1`.
5. Tunggu Package Manager selesai resolve. Jangan tutup Unity selama import.
6. Pastikan Console tidak menunjukkan error merah.

## 2. Buat keseluruhan asset dan scene (wajib)
Pada menu bar Unity pilih:
`Kelompok 4 AR > 0 - BUILD COMPLETE P09-P11 PROJECT`

Builder akan:
- Menyiapkan 9 material permanen di `Assets/Materials`.
- Membuat `Assets/ReferenceImages/RefImageLibrary_FinalProject.asset`.
- Memasukkan `Marker_PinkBotanical` sebagai reference image berukuran 0.15 × 0.15 meter.
- Membuat prefab `Assets/Prefabs/AR/Prefab_PinkBotanicalGarden.prefab`.
- Membake geometri diorama (alas, rumput, 9 bunga, batang, daun, kelopak, pusat bunga, jalur batu, ornamen, dan arch) menjadi child GameObjects di prefab.
- Membuat scene `P09_ARFoundationSetup`, `P10_ImageTracking`, dan `P11_ObjectOnMarker`.
- Menghubungkan library ke AR Tracked Image Manager.
- Menghubungkan prefab ke AR Image Prefab Spawner pada P11.
- Menyusun Build Settings untuk ketiga scene.
- Menyiapkan Player Settings Android dan mencoba memasang ARCore loader.

Setelah builder selesai, pilih:
`Kelompok 4 AR > 5 - VALIDATE GENERATED ASSETS`

Validasi dasar harus menyatakan library, prefab, dan tiga scene ditemukan. Validasi ini belum menggantikan pengujian perangkat.

## 3. Hasil struktur Assets
```text
Assets/
├── Documentation/
├── Editor/
│   └── ARProjectSetupWizard.cs
├── Markers/
│   ├── Marker_PinkBotanical.png
│   ├── Marker_Candidate_Text.png
│   └── Marker_Candidate_Blur.png
├── Materials/
│   ├── MAT_RoseQuartz_Soil.mat
│   ├── MAT_Sage_Garden.mat
│   ├── MAT_Botanical_Stem.mat
│   ├── MAT_Botanical_Leaf.mat
│   ├── MAT_Petal_Pink.mat
│   ├── MAT_Petal_Blush.mat
│   ├── MAT_Petal_Magenta.mat
│   ├── MAT_Pollen_Gold.mat
│   └── MAT_Pearl_Stone.mat
├── Prefabs/AR/
│   └── Prefab_PinkBotanicalGarden.prefab
├── ReferenceImages/
│   └── RefImageLibrary_FinalProject.asset
├── Scenes/
│   ├── P09_ARFoundationSetup.unity
│   ├── P10_ImageTracking.unity
│   └── P11_ObjectOnMarker.unity
└── Scripts/
    ├── AR/
    │   ├── ARImagePrefabSpawner.cs
    │   └── ARStatusOverlay.cs
    └── Visuals/
        └── ProceduralFlowerGarden.cs
```

## 4. P09 — AR Foundation setup
Scene P09 berisi AR Session, XR Origin, AR Camera, AR Camera Manager, AR Camera Background, dan Directional Light.
Periksa Hierarchy dan Inspector. Pada Android, feed kamera hanya dapat dibuktikan dengan menjalankan APK di HP yang kompatibel.

## 5. P10 — Marker-based image tracking
- Marker final: `Marker_PinkBotanical`.
- File marker: `Assets/Markers/Marker_PinkBotanical.png`.
- Reference Image Library: `RefImageLibrary_FinalProject`.
- Nama reference image: `Marker_PinkBotanical`.
- Physical size: 0.15 × 0.15 meter.
- `ARTrackedImageManager` berada pada XR Origin.
- `ARStatusOverlay` menampilkan status pencarian/deteksi marker.

Cetak marker dari PDF `Marker_PinkBotanical_Print_15cm_A4.pdf` dengan skala 100% / Actual Size. Jangan gunakan Fit to Page. Ukur hasil cetak untuk memastikan sisi marker tepat 15 cm.

## 6. P11 — 3D object on marker
XR Origin memuat:
- AR Tracked Image Manager
- AR Status Overlay
- AR Image Prefab Spawner

Spawner terhubung ke `Prefab_PinkBotanicalGarden`, menargetkan nama reference image `Marker_PinkBotanical`. Prefab mempunyai geometri 3D yang sudah tersimpan di dalam prefab dan material permanen pada `Assets/Materials`.

Nilai awal spawner:
- Local Position: `(0, 0.015, 0)`
- Local Rotation: `(0, 0, 0)`
- Uniform Scale: `0.07`

Diorama dibuat berdiameter sekitar 15 cm setelah skala prefab 0.07 diterapkan. Kalibrasi akhir tetap dilakukan dengan marker cetak dan kamera perangkat.

## 7. Konfigurasi Android
Builder mencoba mengatur:
- Package Identifier: `com.kelompok4.pinkbotanicalar`
- Scripting Backend: IL2CPP
- Architecture: ARM64
- Minimum API Level: Android 7.0 / API 24
- Orientasi: Portrait
- Google ARCore XR Loader (jika XR Plug-in Management sudah memiliki settings Android)

Periksa secara manual:
1. Edit > Project Settings > XR Plug-in Management > Android.
2. Pastikan Google ARCore dicentang.
3. Buka Project Validation dan selesaikan error yang ditandai.
4. Unity Hub > Installs > Add Modules: Android Build Support, Android SDK & NDK Tools, OpenJDK.
5. Edit > Preferences > External Tools: gunakan toolchain yang disediakan Unity Hub.

Google menyatakan aplikasi AR Required perlu Android API Level 24 atau lebih tinggi dan perangkat yang mendukung ARCore. Referensi: https://developers.google.com/ar/develop/unity-arf/enable-arcore

## 8. Build APK terpisah
Menu Unity:
- `Kelompok 4 AR > 7 - BUILD APK P09 (Android)`
- `Kelompok 4 AR > 8 - BUILD APK P10 (Android)`
- `Kelompok 4 AR > 9 - BUILD APK P11 (Android)`

Jika Unity baru beralih ke platform Android, tunggu reimport lalu klik menu build yang sama sekali lagi. Output:
- `Builds/Android/AR_Kelompok4_P09.apk`
- `Builds/Android/AR_Kelompok4_P10.apk`
- `Builds/Android/AR_Kelompok4_P11.apk`

APK hanya dapat dihasilkan pada komputer yang mempunyai Android Build Support dan SDK/NDK/JDK. APK tidak disertakan dalam ZIP karena harus dibuat oleh Unity pada laptop pengguna.

## 9. Pengujian perangkat
1. Gunakan HP Android yang tercantum sebagai perangkat ARCore supported.
2. Install APK P09, izinkan kamera, pastikan feed kamera tampil.
3. Install APK P10, arahkan kamera ke marker final, tunggu HUD menunjukkan `MARKER DETECTED` / `TRACKING ACTIVE`.
4. Install APK P11, pastikan taman bunga muncul di atas marker.
5. Gerakkan marker perlahan. Objek harus mengikuti pose marker.
6. Uji jarak, sudut, fokus, pencahayaan, dan kehilangan tracking.
7. Catat hasil pengujian nyata. Jangan menulis “berhasil” sebelum tes benar-benar dilakukan.

## 10. Bukti dokumentasi
- P09: Unity version, Package Manager, ARCore Android, Hierarchy/Inspector, video feed kamera.
- P10: dua kandidat marker, evaluasi marker, Reference Image Library + ukuran, AR Tracked Image Manager, video deteksi.
- P11: prefab diorama, material, pivot/scale, Inspector spawner, video objek 3D mengikuti marker, APK terpasang.
Simpan screenshot asli dari Unity dan rekaman HP. Jangan membuat screenshot bukti palsu.

## Batas validasi
Arsip ini menyiapkan source dan otomatisasi project. Unity Editor dan smartphone tidak dijalankan saat arsip dibuat. Oleh karena itu compile di mesin pengguna, konfigurasi XR loader, build APK, izin kamera, dan tracking fisik tetap memerlukan pengujian pada laptop dan perangkat Android.
