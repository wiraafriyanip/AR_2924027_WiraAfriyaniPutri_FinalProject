# Panduan Final Unity — Kelompok 4 (P09, P10, P11)

## 1. Identitas dan versi
| Komponen | Versi / nilai |
|---|---|
| Unity Editor | 6000.5.10f1 |
| AR Foundation | 6.5.1 |
| Google ARCore XR Plugin | 6.5.1 |
| XR Plug-in Management | 4.6.1 |
| Nama aplikasi | Pink Botanical Garden AR |
| Package Identifier | com.kelompok4.pinkbotanicalar |
| Marker final | Marker_PinkBotanical |
| Ukuran cetak rencana | 15 × 15 cm |
| Lebar fisik dalam library | 0.15 meter |

## 2. Persiapan laptop
1. Install Unity Hub.
2. Pada Unity Hub > Installs, pastikan Unity Editor 6000.5.10f1 tersedia.
3. Pada Add Modules, pastikan Android Build Support, Android SDK & NDK Tools, dan OpenJDK tersedia.
4. Extract ZIP ke folder lokal yang mudah ditemukan, misalnya `C:/UnityProjects/`.
5. Di Unity Hub > Projects > Add > Add project from disk, pilih folder project yang berisi folder `Assets`, `Packages`, dan `ProjectSettings`.
6. Pilih Editor 6000.5.10f1 saat diminta.
7. Buka project dan tunggu Package Manager menyelesaikan proses resolve. Jangan tutup Unity saat import berlangsung.

## 3. Builder otomatis
Setelah project terbuka dan Console tidak memiliki error merah:
1. Klik menu `Kelompok 4 AR` di menu bar Unity.
2. Pilih `0 - BUILD COMPLETE P09-P11 PROJECT`.
3. Tunggu proses pembuatan asset dan scene.
4. Klik OK pada dialog selesai.
5. Builder membuka scene `P11_ObjectOnMarker`.

Builder akan membuat Reference Image Library dari file marker yang sudah disertakan, membuat prefab diorama, membuat scene P09/P10/P11, mengisi library pada P10/P11, menghubungkan prefab pada spawner P11, dan memasukkan ketiga scene ke daftar build.

## 4. Hasil struktur Assets
```text
Assets
├── Documentation
├── Editor
├── Markers
│   ├── Marker_PinkBotanical.png
│   ├── Marker_Candidate_Text.png
│   └── Marker_Candidate_Blur.png
├── Prefabs
│   └── AR
│       └── Prefab_PinkBotanicalGarden.prefab
├── ReferenceImages
│   └── RefImageLibrary_FinalProject.asset
├── Scenes
│   ├── P09_ARFoundationSetup.unity
│   ├── P10_ImageTracking.unity
│   └── P11_ObjectOnMarker.unity
└── Scripts
    ├── AR
    └── Visuals
```

## 5. P09 — AR Foundation setup
Scene P09 terdiri dari:
- AR Session: mengelola lifecycle sesi AR.
- XR Origin: transform koordinat ruang AR.
- AR Camera: kamera perangkat yang menerima feed kamera.
- AR Camera Manager: menyediakan konfigurasi dan frame kamera.
- AR Camera Background: menampilkan feed kamera sebagai latar aplikasi.
- Directional Light: pencahayaan untuk konten virtual.

Pemeriksaan:
1. Buka `Assets/Scenes/P09_ARFoundationSetup.unity`.
2. Pastikan Hierarchy memuat AR Session, XR Origin, dan Directional Light.
3. Expand XR Origin dan pastikan AR Camera menjadi child.
4. Pastikan AR Camera memiliki Camera, Audio Listener, AR Camera Manager, dan AR Camera Background.
5. Simpan scene.

## 6. P10 — Marker-based image tracking
Reference Image Library:
- Asset: `RefImageLibrary_FinalProject`
- Gambar: `Marker_PinkBotanical.png`
- Reference Image Name: `Marker_PinkBotanical`
- Specify Size: aktif
- Physical Size: 0.15 × 0.15 meter

Pemeriksaan:
1. Buka `Assets/Scenes/P10_ImageTracking.unity`.
2. Pilih XR Origin.
3. Pastikan AR Tracked Image Manager terpasang.
4. Pastikan Serialized Library menunjuk ke `RefImageLibrary_FinalProject`.
5. Requested Max Number Of Moving Images: 1.
6. Pastikan AR Status Overlay juga terpasang.
7. Simpan scene.

## 7. P11 — 3D object on marker
Scene P11 menggunakan:
- AR Session.
- XR Origin dan AR Camera.
- AR Tracked Image Manager.
- AR Status Overlay.
- AR Image Prefab Spawner.
- Reference Image Library yang sama dengan P10.
- Prefab `Prefab_PinkBotanicalGarden`.

Nilai awal spawner:
| Parameter | Nilai |
|---|---|
| Target Reference Name | Marker_PinkBotanical |
| Local Position | (0, 0.015, 0) |
| Local Euler Angles | (0, 0, 0) |
| Uniform Scale | 0.07 |
| Content Prefab | Prefab_PinkBotanicalGarden |

Prefab memakai `ProceduralFlowerGarden` untuk membangun diorama saat aplikasi berjalan. Diorama terdiri dari alas mutiara, lapisan tanah rose-quartz, rumput sage, bunga pink/blush/magenta, batang, daun, pusat bunga emas, jalur batu, dan lengkungan dekoratif. Model dibangun dari primitive Unity sehingga tidak membutuhkan paket model eksternal.

Pemeriksaan:
1. Buka scene P11.
2. Pilih XR Origin.
3. Pastikan Serialized Library terisi.
4. Pastikan AR Image Prefab Spawner menunjuk ke prefab.
5. Pastikan targetReferenceName sama persis dengan nama image di library.
6. Simpan scene.

## 8. Konfigurasi Android
1. Edit > Project Settings > XR Plug-in Management.
2. Pilih tab Android.
3. Aktifkan Google ARCore.
4. Buka Project Validation dan perbaiki error yang ditandai.
5. Player > Android > Other Settings:
   - Package Name: `com.kelompok4.pinkbotanicalar`
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64
6. Pada Edit > Preferences > External Tools, gunakan SDK, NDK, dan JDK yang disediakan Unity Hub.
7. Ikuti rekomendasi Project Validation untuk Graphics APIs.
8. Gunakan smartphone Android yang tercantum sebagai perangkat ARCore supported.

## 9. Build APK
1. File > Build Profiles.
2. Pilih Android.
3. Switch Platform jika belum aktif.
4. Pastikan P09, P10, dan P11 ada pada Scene List.
5. Untuk pengujian terpisah, aktifkan scene yang sedang diuji sebagai scene awal atau gunakan daftar scene sesuai kebutuhan.
6. Klik Build.
7. Simpan APK di `Assets/Builds/Android/`.
8. Install APK pada perangkat Android.
9. Izinkan akses kamera saat diminta.

Catatan: APK tidak disertakan karena harus dibangun oleh Unity pada laptop yang memiliki Android SDK/NDK/JDK. Build tidak boleh dianggap berhasil sebelum Unity selesai tanpa error.

## 10. Prosedur pengujian
### P09
- Jalankan aplikasi.
- Berikan izin kamera.
- Pastikan feed kamera tampil dan tidak hitam.

### P10
- Cetak marker pada ukuran 15 × 15 cm.
- Letakkan marker pada permukaan datar dan terang.
- Arahkan kamera dari jarak awal sekitar 20–40 cm.
- Pastikan HUD berubah dari SEARCHING FOR MARKER menjadi TRACKING ACTIVE.
- Uji ulang dengan sudut dan jarak berbeda.

### P11
- Jalankan APK P11.
- Arahkan kamera ke marker final.
- Pastikan diorama muncul di atas marker.
- Gerakkan marker perlahan; objek harus mengikuti posisi dan orientasi marker.
- Tutup sebagian marker atau jauhkan kamera; periksa perilaku saat tracking terbatas/hilang.
- Uji jarak, sudut, fokus, dan pencahayaan.

## 11. Troubleshooting
| Gejala | Pemeriksaan |
|---|---|
| Menu Kelompok 4 AR tidak muncul | Periksa compile error dan lokasi script di Assets/Editor |
| Package resolve gagal | Periksa internet, manifest, versi Editor, dan Console |
| Library kosong | Jalankan builder lengkap; pastikan file marker ada |
| Marker tidak terdeteksi | Periksa nama marker, ukuran cetak, fokus, kontras, glare, dan pencahayaan |
| Marker terdeteksi tetapi objek tidak muncul | Periksa Content Prefab, Target Reference Name, serta Console |
| Kamera hitam | Periksa izin kamera, ARCore, perangkat kompatibel, dan Project Validation |
| APK gagal build | Periksa Android Build Support, SDK/NDK/JDK, IL2CPP, ARM64, dan Build Profiles |

## 12. Daftar bukti dokumentasi
Simpan screenshot asli dari Unity/HP, jangan membuat ulang tampilan bukti:
1. Unity Hub menampilkan Editor 6000.5.10f1.
2. Package Manager menampilkan AR Foundation dan ARCore.
3. XR Plug-in Management Android dengan Google ARCore aktif.
4. Hierarchy dan Inspector P09.
5. Feed kamera P09 pada HP.
6. Marker kandidat dan tabel evaluasi P10.
7. Inspector Reference Image Library dengan nama dan ukuran marker.
8. Inspector AR Tracked Image Manager.
9. Hierarchy dan Inspector P11 yang memperlihatkan AR Image Prefab Spawner.
10. Prefab taman bunga di Project/Inspector.
11. Foto/video objek 3D tampil pada marker di HP.
12. Bukti APK terpasang dan video demonstrasi 20–60 detik.

## 13. Batas bukti
Screenshot konfigurasi hanya membuktikan pengaturan project. Keberhasilan deteksi marker, tracking stabil, dan kemunculan objek 3D hanya dapat dibuktikan melalui pengujian langsung pada perangkat Android yang kompatibel dengan ARCore.
