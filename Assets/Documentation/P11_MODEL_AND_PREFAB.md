# Spesifikasi Model dan Prefab P11

## Identitas
- Nama prefab: `Prefab_PinkBotanicalGarden`
- Script pengendali: `ProceduralFlowerGarden`
- Material: 9 material permanen pada `Assets/Materials`
- Marker target: `Marker_PinkBotanical`
- Diameter diorama setelah scale: sekitar 15 cm.

## Komponen visual
1. Alas bawah berwarna pearl.
2. Lapisan tanah rose-quartz.
3. Permukaan taman sage green.
4. Ornamen stud emas di sekeliling alas.
5. Sembilan bunga dengan batang dan daun.
6. Kelopak tiga variasi warna pink, blush, dan magenta.
7. Pusat bunga berwarna emas.
8. Jalur batu dekoratif.
9. Arch dekoratif di bagian belakang taman.

## Transform
Prefab disimpan dengan skala dasar 1,1,1. Spawner menerapkan uniform scale 0.07. Diameter alas hasil akhir sekitar 0.1505 meter. Physical size marker 0.15 meter.

## Pivot
Root prefab menjadi titik origin (0,0,0). Alas berada sedikit di bawah bidang marker; spawner menambahkan offset Y 0.015 meter untuk menghindari z-fighting dan membuat objek tampak berada di atas permukaan.

## Pemeriksaan prefab di Unity
- Buka `Assets/Prefabs/AR/Prefab_PinkBotanicalGarden.prefab`.
- Pastikan prefab memiliki ProceduralFlowerGarden.
- Pastikan child geometry tampak di Prefab Mode.
- Periksa material setiap renderer.
- Pastikan tidak ada collider yang tidak dibutuhkan.
- Pastikan child objects tidak memiliki script yang memunculkan duplikasi.
