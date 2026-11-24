# Arsitektur Aplikasi AR Kelompok 4

Diagram visual tersedia pada `AR_ARCHITECTURE_DIAGRAM.png`.

```text
Android Camera + Motion Sensors
              |
              v
     Google ARCore XR Plugin
              |
              v
         AR Foundation
              |
      +-------+--------+
      |                |
   AR Session       XR Origin
                       |
                    AR Camera
                       |
              AR Tracked Image Manager
                       |
              XR Reference Image Library
                       |
                Marker_PinkBotanical
                       |
               ARImagePrefabSpawner
                       |
             Tracking State Validation
                       |
          Prefab_PinkBotanicalGarden
                       |
        PinkBotanicalGarden.fbx
                       |
              Smartphone Display
```

## Tanggung jawab komponen
- **Android Camera dan Motion Sensors:** menyediakan data kamera dan sensor perangkat untuk pengalaman AR.
- **Google ARCore XR Plugin:** integrasi subsistem AR pada Android.
- **AR Foundation:** lapisan API Unity untuk mengelola fitur AR.
- **AR Session:** mengelola lifecycle sesi AR.
- **XR Origin:** mendefinisikan transform koordinat ruang AR.
- **AR Camera:** menampilkan feed kamera dan sudut pandang pengguna.
- **AR Tracked Image Manager:** mencari dan memperbarui marker yang terdaftar.
- **XR Reference Image Library:** menyimpan gambar marker, nama referensi, dan ukuran fisik.
- **ARImagePrefabSpawner:** memeriksa nama marker dan status tracking, kemudian membuat prefab ketika marker cocok.
- **Prefab Pink Botanical Garden:** prefab runtime yang memiliki child model FBX dan ditempatkan sebagai child dari ARTrackedImage.
- **PinkBotanicalGarden.fbx:** model diorama statis dengan sembilan kelompok mesh material.

## Alur kerja runtime
1. Aplikasi meminta izin kamera dan memulai AR Session.
2. ARCore menyediakan data kamera dan tracking perangkat.
3. AR Tracked Image Manager menerima Reference Image Library.
4. Kamera memindai lingkungan dan mencari pola marker `Marker_PinkBotanical`.
5. Saat marker dikenali, AR Foundation membuat ARTrackedImage.
6. ARImagePrefabSpawner memeriksa nama referensi dan tracking state.
7. Jika cocok, prefab taman bunga dibuat sebagai child dari ARTrackedImage.
8. Karena menjadi child dari trackable, konten mengikuti posisi dan orientasi marker.
9. Saat tracking tidak valid, prefab disembunyikan sesuai pengaturan script.

## Batas teknis
Diorama berasal dari model FBX statis yang diimpor melalui Unity ModelImporter. Installer Editor membangun prefab dan menghubungkan material project. Tracking fisik hanya dapat divalidasi pada perangkat Android yang mendukung ARCore.
