# Panduan Build Android — Kelompok 4 AR

## Prasyarat
- Unity Editor 6000.5.10f1.
- Android Build Support.
- Android SDK & NDK Tools.
- OpenJDK.
- Perangkat Android yang mendukung ARCore.
- USB data cable untuk instalasi melalui komputer (opsional).

## Jalankan builder
1. Buka project.
2. Tunggu semua package selesai.
3. Menu `Kelompok 4 AR > 0 - BUILD COMPLETE P09-P11 PROJECT`.
4. Menu `Kelompok 4 AR > 5 - VALIDATE GENERATED ASSETS`.
5. Menu `Kelompok 4 AR > 6 - CONFIGURE ANDROID + ARCORE`.

## Periksa XR Plug-in Management
Buka Edit > Project Settings > XR Plug-in Management > Android. Pastikan Google ARCore aktif. Jika tidak aktif, centang Google ARCore. Jika tab Android belum muncul, pastikan Android Build Support telah dipasang dan platform Android sudah tersedia.

## Periksa Project Validation
Di XR Plug-in Management > Project Validation > Android:
- Baca semua pesan merah.
- Klik Fix pada pemeriksaan yang menyediakan tombol Fix.
- Jangan mengabaikan error yang berhubungan dengan API Level, graphics API, atau ARCore.

## Build satu pertemuan
Jalankan satu menu:
- Build APK P09
- Build APK P10
- Build APK P11

Jika diminta beralih platform, tunggu Unity melakukan reimport, lalu klik menu build yang sama lagi. Hasil APK tersimpan di `Builds/Android`.

## Instal dan tes
- Transfer APK ke HP.
- Aktifkan izin instalasi dari sumber yang digunakan bila Android meminta.
- Install APK.
- Buka aplikasi dan berikan izin kamera.
- Uji di area terang, marker datar, tidak mengkilap, tidak tertutup.
- Rekam hasil tes asli.

## Jika gagal
- `Android Build Support not installed`: Unity Hub > Installs > gear > Add modules.
- `SDK/NDK/JDK missing`: Preferences > External Tools > aktifkan toolchain yang diinstal bersama Unity.
- `ARCore loader missing`: Project Settings > XR Plug-in Management > Android > Google ARCore.
- `Device not supported`: gunakan HP yang masuk daftar perangkat ARCore supported.
- `Marker not detected`: ukur ulang marker, cetak 15 cm tanpa scaling, pastikan fokus dan cahaya cukup.
