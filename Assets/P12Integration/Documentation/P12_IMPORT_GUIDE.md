# Paket Integrasi P12 — Project Lama AR Kelompok 4

Paket ini menambahkan fitur praktikum Pertemuan 12 ke project Unity P09–P11 yang sudah ada. Paket ini **bukan project Unity baru**.

## Yang ditambahkan
- `ARObjectController.cs`: empat aksi publik untuk tombol Rotate Left, Rotate Right, Scale Up, dan Scale Down.
- `ARUISafeArea.cs`: menjaga UI tetap berada di area aman layar Android.
- `ui_click.wav`: feedback audio singkat setelah aksi berhasil.
- `ARP12IntegrationBuilder.cs`: menu Editor untuk menyalin scene P11 menjadi scene P12 dan membuat UI otomatis.

## Yang sengaja tidak diganti
- `ProjectSettings` dan `Packages`.
- Scene `P11_ObjectOnMarker.unity`.
- `ARTrackedImageManager` dan Reference Image Library.
- `ARImagePrefabSpawner.cs` dan pengaturan marker/prefab/skala yang sudah ada.
- Prefab Pink Botanical Garden dan aset FBX yang sudah berhasil dipasang.

## Cara memasang
Pilih salah satu metode berikut, jangan gunakan keduanya sekaligus.

### Metode A — Import Custom Package (disarankan)
1. Buka project lama di Unity.
2. Pilih `Assets > Import Package > Custom Package...`.
3. Pilih `AR_Kelompok4_P12_Integration.unitypackage`.
4. Pastikan seluruh item paket tercentang, lalu klik `Import`.
5. Tunggu Unity selesai compile. Jangan lanjut jika Console menunjukkan error merah.

### Metode B — Salin folder Assets
1. Extract `AR_Kelompok4_P12_Integration_Files.zip` ke folder root project lama, yaitu folder yang berisi `Assets`, `Packages`, dan `ProjectSettings`.
2. Saat Windows menanyakan penggabungan folder `Assets`, pilih merge/yes. Paket ini memakai nama folder `Assets/P12Integration` dan satu file baru di `Assets/Editor`, tidak mengganti script P11.
3. Kembali ke Unity dan tunggu import/compile selesai.

## Membuat scene P12
1. Pastikan scene P11 bernama `Assets/Scenes/P11_ObjectOnMarker.unity`.
2. Pastikan marker sudah bisa memunculkan prefab Pink Botanical Garden pada scene P11.
3. Pilih `Kelompok 4 AR > P12 > 1 - BUAT SCENE P12 DARI P11`.
4. Jika scene P12 sebelumnya sudah ada, pilih `Buat Ulang P12` hanya jika ingin menggantinya dengan salinan P11 terbaru.
5. Unity membuat `Assets/Scenes/Pertemuan12_ARInteraction.unity` dan membukanya.

Builder menyalin file scene P11 terlebih dahulu, lalu mengubah salinannya. File P11 asli tidak diedit oleh builder. Nilai skala, nama marker, prefab, dan Reference Image Library pada spawner disalin apa adanya.

## Build APK P12
1. Pastikan platform aktif adalah Android di `File > Build Profiles`.
2. Pilih `Kelompok 4 AR > P12 > 2 - BUILD APK P12`.
3. APK disimpan di `Builds/P12/AR_P12_Kelompok4.apk`.

## Kontrol aplikasi
- `ROTATE LEFT`: memutar model 15 derajat ke kiri.
- `ROTATE RIGHT`: memutar model 15 derajat ke kanan.
- `SCALE UP`: memperbesar model secara proporsional.
- `SCALE DOWN`: memperkecil model secara proporsional.
- Audio klik diputar setelah aksi berhasil menemukan objek AR.

Batas skala dihitung relatif terhadap ukuran awal instance yang dibuat oleh spawner P11, sehingga tombol Scale tidak memaksa skala awal menjadi angka tertentu.

## Pemeriksaan setelah build
Uji pada HP Android: scan marker, pastikan objek tampil, tekan keempat tombol, cek informasi, dan dengarkan feedback audio. APK belum dapat dinyatakan lulus uji perangkat sampai benar-benar dijalankan pada HP.
