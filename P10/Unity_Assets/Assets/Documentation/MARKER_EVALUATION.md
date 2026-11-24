# Evaluasi Desain Marker — Pertemuan 10

Tiga gambar kandidat disertakan pada `Assets/Markers`:
1. `Marker_Candidate_Text.png` — desain teks sederhana dengan sedikit fitur visual.
2. `Marker_Candidate_Blur.png` — gambar floral dengan detail yang sengaja dibuat blur.
3. `Marker_PinkBotanical.png` — desain floral final dengan detail tepi, kontras lokal, dan fitur yang tersebar.

| Kriteria | Teks polos | Foto blur | Pink Botanical |
|---|---|---|---|
| Jumlah tepi/sudut visual | Rendah | Sedang tetapi tidak tajam | Tinggi |
| Kontras lokal | Rendah | Menurun karena blur | Tinggi |
| Pola visual nonrepetitif | Rendah | Sedang | Tinggi |
| Sebaran fitur | Terbatas | Kurang tegas | Menyebar |
| Ketajaman | Tinggi pada teks, tetapi sederhana | Rendah | Tinggi |
| Keputusan | Tidak dipilih | Tidak dipilih | Marker final |

## Alasan pemilihan
Pink Botanical dipilih karena memiliki elemen visual yang lebih kaya dan tersebar: kelopak berlapis, pusat bunga, ornamen titik, garis bingkai, serta elemen daun di sudut. Kombinasi ini berpotensi menyediakan fitur lokal yang lebih mudah dibedakan dibandingkan teks polos atau gambar blur.

Penilaian pada tabel bersifat kualitatif berdasarkan desain visual. Ini bukan skor kualitas fitur yang dihasilkan ARCore. Keberhasilan tracking tetap harus diuji pada perangkat Android.

## Spesifikasi marker final
- Nama file: `Marker_PinkBotanical.png`
- Nama referensi: `Marker_PinkBotanical`
- Resolusi: 1024 × 1024 piksel
- Rasio: 1:1
- Specify Size di XR Reference Image Library: nonaktif
- Ukuran fisik marker tidak dikunci; ARCore memperkirakan ukuran saat tracking
- Marker dapat dicetak atau ditampilkan pada layar HP kedua
- Pastikan gambar utuh, cukup besar pada bidang kamera, fokus, dan tidak memantulkan cahaya

Jangan crop, meregangkan, memutar, atau mengubah rasio marker setelah didaftarkan pada Reference Image Library. Karena Specify Size nonaktif, ukuran marker tidak perlu dimasukkan secara manual. Ukuran tampilan tetap memengaruhi jarak dan kualitas deteksi: marker yang terlalu kecil, blur, terpotong, atau silau dapat sulit dilacak.
