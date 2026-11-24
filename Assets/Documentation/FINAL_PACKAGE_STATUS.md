# STATUS PAKET — AR KELOMPOK 4 (REVISI LENGKAP)

## Isi source archive
- ProjectVersion Unity 6000.5.10f1.
- Manifest terkunci: AR Foundation 6.5.1, ARCore 6.5.1, XR Management 4.6.1.
- Tiga marker/candidate images.
- Script tracking, HUD, dan diorama.
- Builder Editor satu klik untuk membuat P09/P10/P11.
- Builder membuat 9 material permanen.
- Builder membake geometri diorama ke prefab sehingga prefab dapat dilihat di Prefab Mode.
- Builder membuat Reference Image Library dengan marker 15 × 15 cm.
- Builder menghubungkan reference library dan prefab ke komponen yang sesuai.
- Menu validasi asset.
- Menu konfigurasi Android dan upaya konfigurasi ARCore loader.
- Menu build APK terpisah untuk P09, P10, P11.
- Panduan instalasi, build, model, dokumentasi, dan PDF marker siap cetak.

## Yang harus dijalankan di laptop Unity
1. Resolve packages.
2. Jalankan menu builder.
3. Jalankan menu validasi.
4. Periksa Project Settings > XR Plug-in Management > Android.
5. Build APK P09/P10/P11.

## Yang harus dibuktikan pada perangkat Android
- Feed kamera.
- Deteksi marker.
- Diorama 3D tampil pada marker.
- Objek mengikuti pose marker.
- Pengujian jarak, sudut, dan cahaya.
- Rekaman video asli.

## Batasan yang tidak dapat dipalsukan
Arsip source tidak menyertakan APK hasil build atau video hasil tes fisik. APK hanya dapat dibuat dengan Unity + Android Build Support pada komputer pengguna, dan tracking hanya dapat disahkan setelah diuji pada HP ARCore-compatible. Paket tidak boleh dinyatakan lulus hardware testing sebelum bukti tersebut tersedia.
