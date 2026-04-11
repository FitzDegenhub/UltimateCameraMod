[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | **Bahasa Indonesia** | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 telah hadir!** Sacred God Mode override, toggle Lock-on Auto-Rotate, dan semua perbaikan bug. Unduh dari **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** atau **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Perangkat kamera mandiri untuk Crimson Desert. GUI lengkap, pratinjau kamera langsung, tiga tingkat editor, preset berbasis file, **ekspor JSON untuk [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** dan **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), serta dukungan HUD ultrawide.

<p align="center">
  <img src="../../screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banner" width="100%" />
</p>

<p align="center">

[![Download v3.1.2](https://img.shields.io/badge/Download-v3.1.2-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)
[![Wiki](https://img.shields.io/badge/Wiki-Documentation-8B5CF6?style=for-the-badge&logo=bookstack&logoColor=white)](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)

[![VirusTotal v3.1.2](https://img.shields.io/badge/VirusTotal_v3.1.2-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021)
[![Reddit Discussion](https://img.shields.io/badge/Reddit-Discussion-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/CrimsonDesert/comments/1sfou61/ucm_ultimate_camera_mod_v3_crimson_desert_full/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)
[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

</p>

> Butuh bantuan? Lihat **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** untuk panduan pengaturan, penjelasan pengaturan kamera, manajemen preset, pemecahan masalah, dan dokumentasi pengembang.

---

<details>
<summary><strong>Tangkapan Layar (v3.x)</strong> — klik untuk membuka</summary>
<br>

**UCM Quick** — jarak, ketinggian, pergeseran, FoV, zoom lock-on, steadycam, pratinjau langsung
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** — penyetelan mendalam terkurasi dengan kartu berbingkai yang dapat dicari
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** — editor XML mentah lengkap dengan perbandingan vanilla
![God Mode](../../screenshots/v3.x/godmode.png)

**Ekspor JSON** — ekspor untuk JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**Impor** — impor dari .ucmpreset, XML, PAZ, atau paket Mod Manager
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## Ringkasan branch

| Branch | Status | Keterangan |
|--------|--------|------------|
| **`main`** | v3.1.2 Rilis | Perangkat kamera mandiri dengan editor tiga tingkat (UCM Quick / Fine Tune / God Mode), preset berbasis file, katalog komunitas, ekspor multi-format, dan instalasi PAZ langsung |
| **`development`** | Pengembangan | Branch pengembangan versi selanjutnya |

v3 mencakup semua fitur kamera dari v2 ditambah UI yang dirancang ulang, preset berbasis file, editor tiga tingkat, dan ekspor multi-format. Instalasi PAZ langsung tetap tersedia di v3 sebagai opsi sekunder.

---

## Fitur

### Kontrol kamera

| Fitur | Detail |
|-------|--------|
| **8 preset bawaan** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival — dengan pratinjau langsung |
| **Kamera kustom** | Slider untuk jarak (1.5-12), ketinggian (-1.6-1.5), dan pergeseran horizontal (-3-3). Penskalaan proporsional menjaga karakter di posisi layar yang sama di semua tingkat zoom |
| **Bidang pandang** | Vanilla 40° hingga 80°. Konsistensi FoV universal di seluruh status guard, aim, mount, glide, dan sinematik |
| **Kamera terpusat** | Karakter tepat di tengah di lebih dari 150 status kamera, menghilangkan kamera bahu yang offset ke kiri |
| **Zoom lock-on** | Slider dari -60% (zoom ke target) hingga +60% (tarik mundur lebar). Mempengaruhi semua status lock-on, guard, dan rush. Bekerja independen dari Steadycam |
| **Lock-on auto-rotate** | Nonaktifkan snap kamera ke target saat lock-on. Mencegah kamera berputar cepat menghadap musuh di belakang Anda. Kredit kepada [@sillib1980](https://github.com/sillib1980) |
| **Sinkronisasi kamera tunggangan** | Kamera tunggangan menyesuaikan ketinggian kamera pemain yang Anda pilih |
| **Pergeseran horizontal di semua tunggangan** | Kuda, gajah, wyvern, kano, mesin perang, dan sapu semuanya mengikuti pengaturan pergeseran Anda dengan penskalaan proporsional |
| **Konsistensi bidik skill** | Lentera, Blinding Flash, Busur, dan semua skill aim/zoom/interaksi mengikuti pergeseran horizontal. Tidak ada snap kamera saat mengaktifkan kemampuan |
| **Pemulusan steadycam** | Waktu blend dan ayunan kecepatan yang dinormalisasi di lebih dari 30 status kamera: idle, jalan, lari, sprint, tempur, guard, rush/charge, jatuh bebas, lompatan super, tarik/ayunan tali, knockback, semua varian lock-on, lock-on tunggangan, lock-on revive, aggro/wanted, mesin perang, dan semua status tunggangan. Setiap nilai dapat disetel oleh komunitas melalui editor Fine Tune |
| **Sacred God Mode** | Nilai yang Anda edit di God Mode dilindungi secara permanen dari rebuild Quick/Fine Tune. Indikator hijau menunjukkan nilai mana yang sacred. Penyimpanan per-preset |

> **Filosofi desain v3: hanya mengedit nilai, tanpa injeksi struktur.**
>
> Versi sebelumnya menyuntikkan baris XML baru ke dalam file kamera (level zoom tambahan, mode orang pertama kuda, perombakan kamera kuda dengan tingkat zoom tambahan). v3 menghapus fitur-fitur ini secara sengaja. Menyuntikkan struktur memiliki peluang lebih tinggi untuk rusak setelah pembaruan game, dan preferensi pribadi untuk mode kamera khusus lebih baik dilayani oleh mod khusus yang didistribusikan melalui mod manager. UCM sekarang hanya memodifikasi nilai yang ada — jumlah baris yang sama, struktur elemen yang sama, atribut yang sama. Ini membuat preset lebih aman untuk dibagikan dan lebih tahan terhadap patch game.

### Editor tiga tingkat (v3)

v3 mengorganisir pengeditan ke dalam tiga tab sehingga Anda bisa masuk sedalam yang Anda inginkan:

| Tingkat | Tab | Fungsi |
|---------|-----|--------|
| 1 | **UCM Quick** | Layer cepat — slider jarak/ketinggian/pergeseran, FoV, kamera terpusat, zoom lock-on (-60% hingga +60%), lock-on auto-rotate, sinkronisasi tunggangan, steadycam, pratinjau kamera + FoV langsung |
| 2 | **Fine Tune** | Penyetelan mendalam terkurasi. Bagian yang dapat dicari untuk level zoom jalan kaki, zoom kuda/tunggangan, FoV global, tunggangan khusus & traversal, tempur & lock-on, pemulusan kamera, serta bidik & posisi crosshair. Dibangun di atas UCM Quick |
| 3 | **God Mode** | Editor XML mentah lengkap — setiap parameter dalam DataGrid yang dapat dicari dan difilter, dikelompokkan berdasarkan status kamera. Kolom perbandingan vanilla. Override sacred (hijau) yang dilindungi dari rebuild. Filter "sacred only". 54 tooltip atribut |

### Sistem preset berbasis file (v3)

- **Format file `.ucmpreset`** — format khusus yang dapat dibagikan untuk preset kamera UCM. Taruh ke folder preset mana saja dan langsung berfungsi
- **Manajer sidebar** dengan bagian grup yang dapat dilipat: Default Game, Preset UCM, Preset Komunitas, Preset Saya, Diimpor
- **Buat / Duplikat / Ubah Nama / Hapus** dari sidebar
- **Kunci** preset untuk mencegah pengeditan tidak sengaja — preset UCM terkunci secara permanen; preset pengguna dapat di-toggle melalui ikon gembok
- **Preset Vanilla Asli** — `playercamerapreset` mentah yang didekodekan dari backup game Anda tanpa modifikasi apa pun. Slider Quick disinkronkan ke nilai baseline game yang sebenarnya
- **Impor** dari `.ucmpreset`, XML mentah, arsip PAZ, atau paket Mod Manager. Impor `.ucmpreset` mendapatkan kontrol slider UCM penuh; impor XML/PAZ/Mod Manager mentah adalah preset mandiri (hanya pengeditan God Mode, tanpa aturan UCM) untuk mempertahankan nilai asli pembuat mod
- **Simpan otomatis** — perubahan pada preset yang tidak terkunci ditulis kembali ke file preset secara otomatis (dengan debounce)
- Migrasi otomatis dari preset `.json` lama ke `.ucmpreset` saat peluncuran pertama

### Katalog preset (v3)

Jelajahi dan unduh preset langsung dari UCM. Unduh satu klik, tidak perlu akun.

- **Preset UCM** — 7 gaya kamera resmi (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definisi di-host di GitHub, XML sesi di-bake secara lokal dari file game Anda + aturan kamera saat ini. Otomatis di-bake ulang saat aturan kamera diperbarui
- **[Preset komunitas](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** — preset kontribusi komunitas di repo utama, katalog dibuat otomatis oleh GitHub Actions
- **Tombol Jelajahi** di header setiap grup sidebar membuka browser katalog
- Setiap preset menampilkan nama, pembuat, deskripsi, tag, dan tautan ke halaman Nexus pembuat
- **Deteksi pembaruan** — ikon pembaruan berkedip saat versi lebih baru tersedia di katalog. Klik untuk mengunduh pembaruan dengan opsi backup ke Preset Saya
- Preset yang diunduh muncul di sidebar (terkunci secara default — duplikat untuk mengedit)
- **Batas ukuran file 2MB** dan validasi JSON untuk keamanan

**Ingin berbagi preset Anda dengan komunitas?** Ekspor sebagai `.ucmpreset` dari UCM, lalu:
- Kirim [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) yang menambahkan preset Anda ke folder `community_presets/`
- Atau kirim file `.ucmpreset` Anda ke 0xFitz di Discord/Nexus dan kami akan menambahkannya untuk Anda

### Ekspor multi-format (v3)

Dialog **Ekspor untuk berbagi** menghasilkan sesi Anda dalam empat cara:

| Format | Kegunaan |
|--------|----------|
| **JSON** (mod manager) | Patch byte + `modinfo` untuk **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) atau **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Ekspor di UCM, lalu impor di manager yang Anda gunakan; penerima tidak perlu UCM. **Siapkan** hanya ditawarkan saat entri `playercamerapreset` langsung masih cocok dengan backup vanilla UCM (verifikasi file game jika Anda sudah menerapkan mod kamera). |
| **XML** | `playercamerapreset.xml` mentah untuk alat lain atau pengeditan manual |
| **0.paz** | Arsip yang sudah di-patch, siap taruh ke folder `0010` game |
| **.ucmpreset** | Preset UCM lengkap untuk pengguna UCM lainnya |

Termasuk kolom judul, versi, pembuat, URL Nexus, dan deskripsi untuk JSON/XML. Menampilkan jumlah region patch dan byte yang diubah sebelum menyimpan `.json`.

### Kualitas hidup

- **Deteksi game otomatis** — Steam, Epic Games, Xbox / Game Pass
- **Backup otomatis** — backup vanilla sebelum modifikasi apa pun; pulihkan satu klik. Sadar versi dengan pembersihan otomatis saat upgrade
- **Banner konfigurasi terpasang** — menampilkan konfigurasi aktif lengkap Anda (FoV, jarak, ketinggian, pergeseran, pengaturan)
- **Kesadaran patch game** — melacak metadata instalasi setelah penerapan; memperingatkan saat game mungkin telah diperbarui agar Anda bisa mengekspor ulang
- **Pratinjau kamera + FoV langsung** — tampilan atas ke bawah yang sadar jarak dengan pergeseran horizontal dan kerucut bidang pandang
- **Notifikasi pembaruan** — memeriksa GitHub releases saat peluncuran
- **Pintasan folder game** — buka direktori game Anda dari header
- **Identitas Windows taskbar** — pengelompokan ikon dan ikon title bar yang benar melalui shell property store
- **Pengaturan persisten** — semua pilihan diingat antar sesi
- **Jendela dapat diubah ukurannya** — ukuran bertahan antar sesi
- **Portabel** — satu file `.exe`, tidak perlu installer

### Filosofi

> **Belum ada yang menyempurnakan kamera Crimson Desert -- dan itulah intinya.**
>
> Game vanilla memiliki lebih dari 150 status kamera, masing-masing dengan puluhan parameter. Tidak ada pengembang tunggal yang bisa menyetel semua itu untuk setiap gaya bermain dan layar. Itulah mengapa UCM ada -- bukan untuk memberi tahu Anda apa kamera yang sempurna, tetapi untuk memberi Anda alat untuk menemukannya sendiri dan membagikannya dengan orang lain.
>
> Setiap pengaturan yang Anda sesuaikan bisa diekspor dan dibagikan. Perbaikan Lock-on Auto-Rotate yang menghilangkan snap kamera saat pertempuran ditemukan oleh satu anggota komunitas yang bereksperimen di God Mode. Penyetelan halus yang didorong komunitas seperti inilah yang menjadi tujuan alat ini.

### Berbagi preset

Ekspor pengaturan kamera Anda sebagai file `.ucmpreset` dan bagikan dengan orang lain. Impor preset dari katalog komunitas, Nexus Mods, atau pemain lain. UCM juga mengekspor ke JSON (untuk [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) dan [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), XML mentah, dan instalasi PAZ langsung.

---

## Cara kerjanya

1. Menemukan arsip PAZ game yang berisi `playercamerapreset.xml`
2. Membuat backup file asli (hanya sekali — tidak pernah menimpa backup bersih)
3. Mendekripsi entri arsip (ChaCha20 + Jenkins hash key derivation)
4. Melakukan dekompresi melalui LZ4
5. Mem-parse dan memodifikasi parameter kamera XML berdasarkan pilihan Anda
6. Mengompresi ulang, mengenkripsi ulang, dan menulis entri yang dimodifikasi kembali ke arsip

Tanpa injeksi DLL, tanpa peretasan memori, tanpa koneksi internet — murni modifikasi file data.

---

## Membangun dari kode sumber

Memerlukan [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (atau lebih baru). Windows x64.

### v3 (direkomendasikan)

Tutup semua instansi yang berjalan sebelum membangun — langkah penyalinan exe akan gagal jika file terkunci.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Dependensi (NuGet — dipulihkan secara otomatis)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) — kompresi/dekompresi blok LZ4

---

## Struktur proyek

```
src/UltimateCameraMod/              Library bersama + aplikasi WPF v2.x
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, model data
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x UI
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           Aplikasi WPF v3 berorientasi ekspor (mereferensikan kode bersama di atas)
├── Controls/                       CameraPreview, FovPreview (varian v3)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Preset komunitas bawaan yang di-deploy saat peluncuran pertama
├── MainWindow.xaml                 Shell dua panel: sidebar + editor bertab
├── ExportJsonDialog.xaml           Wizard ekspor multi-format (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Impor dari .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Input metadata preset (nama, pembuat, deskripsi, URL)
├── CommunityBrowserDialog.xaml     Jelajahi & unduh preset komunitas dari GitHub
├── NewPresetDialog.xaml            Buat / beri nama preset baru
├── ShellTaskbarPropertyStore.cs    Ikon Windows taskbar melalui shell property store
├── ApplicationIdentity.cs          App User Model ID bersama
└── UltimateCameraMod.V3.csproj

community_presets/                  Preset kamera kontribusi komunitas
ucm_presets/                        Definisi preset gaya UCM resmi
```

---

## Kompatibilitas

- **Platform:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Layar:** Rasio aspek apa saja — 16:9, 21:9, 32:9

---

## FAQ

**Apakah ini akan membuat saya di-ban?**
UCM hanya memodifikasi file data offline. Tidak menyentuh memori game, menyuntikkan kode, atau berinteraksi dengan proses yang berjalan. Gunakan dengan kebijaksanaan Anda sendiri dalam mode online/multiplayer.

**Game diperbarui dan kamera saya kembali ke vanilla.**
Normal — pembaruan game menimpa file yang dimodifikasi. Buka kembali UCM dan klik Instal (atau ekspor ulang JSON untuk JSON Mod Manager / CDUMM). Pengaturan Anda tersimpan secara otomatis.

**Antivirus saya menandai exe ini.**
False positive yang umum terjadi pada aplikasi .NET self-contained. Pemindaian VirusTotal bersih: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). Kode sumber lengkap tersedia di sini untuk ditinjau dan dibangun sendiri.

**Apa arti pergeseran horizontal 0?**
0 = posisi kamera vanilla (karakter sedikit ke kiri). 0.5 = karakter terpusat di layar. Nilai negatif bergeser lebih ke kiri, nilai positif bergeser lebih ke kanan.

**Upgrade dari versi sebelumnya?**
Pengguna v3.x: cukup ganti exe, semua preset dan pengaturan dipertahankan. Pengguna v2.x: hapus folder UCM lama, verifikasi file game di Steam, lalu jalankan v3.1 dari folder baru. Lihat [catatan rilis](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) untuk petunjuk detail.

---

## Riwayat versi

- **v3.1.2** — Perbaikan nilai sacred yang hilang dari Instal/ekspor di tab God Mode. Lihat [catatan rilis](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2).
- **v3.1.1** — Perbaikan deteksi backup tercemar false-positive pada file game bersih.
- **v3.1** — Sacred God Mode override (edit pengguna dilindungi secara permanen dari rebuild). Toggle Lock-on Auto-Rotate (kredit kepada [sillib1980](https://github.com/sillib1980)). Indikator sacred hijau. Perbaikan instalasi Full Manual Control. Overlay upgrade sadar versi. Lihat [catatan rilis](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** — Semua dialog dikonversi ke sistem overlay dalam aplikasi. Override God Mode bertahan saat pergantian tab. Pemilihan tipe preset (UCM Managed vs Full Manual Control). Katalog preset komunitas dipindah ke repo utama. 54 tooltip atribut God Mode. Perbaikan crash game. Validasi vanilla diperbarui untuk patch game Juni 2026. Wiki 21 halaman.
- **v3.0.1** — Desain ulang berorientasi ekspor. Editor tiga tingkat (UCM Quick / Fine Tune / God Mode). Format file `.ucmpreset`. Sistem preset berbasis file. Katalog preset UCM dan komunitas. Ekspor multi-format. Steadycam diperluas ke 30+ status kamera. Slider zoom lock-on.
- **v2.5** — Rilis v2.x terakhir.
- **v2.4** — Pergeseran horizontal proporsional, pergeseran di semua tunggangan dan kemampuan bidik, perombakan kamera kuda, backup sadar versi, pratinjau FoV, jendela dapat diubah ukurannya.
- **v2.3** — Perbaikan pergeseran horizontal untuk 16:9, slider berbasis delta, banner konfigurasi terpasang lengkap.
- **v2.2** — Steadycam, level zoom tambahan, orang pertama kuda, pergeseran horizontal, FoV universal, konsistensi bidik skill, Impor XML, berbagi preset, notifikasi pembaruan.
- **v2.1** — Perbaikan slider preset kustom tidak menulis ke semua level zoom.
- **v2.0** — Penulisan ulang total dari Python ke C# / .NET 6 / WPF. Editor XML lanjutan, manajemen preset, deteksi game otomatis.
- **v1.5** — Versi Python dengan GUI customtkinter.

---

## Kredit & ucapan terima kasih

- **0xFitz** — Pengembangan UCM, penyetelan kamera, editor lanjutan
- **[@sillib1980](https://github.com/sillib1980)** — Menemukan field kamera Lock-on Auto-Rotate

### Penulisan ulang C# (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** — CrimsonDesertTools — Parser PAZ/PAMT C#, enkripsi ChaCha20, kompresi LZ4, PaChecksum, repacker arsip (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** — Implementasi ChaCha20 stream cipher murni C# (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** — Panduan repacking PAZ: alignment 16-byte, PAMT checksum, patching PAPGT root index

### Versi Python asli (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** — crimson-desert-unpacker — Perangkat arsip PAZ, riset dekripsi
- **Maszradine** — CDCamera — Aturan kamera, sistem steadycam, preset gaya
- **manymanecki** — CrimsonCamera — Arsitektur modifikasi PAZ dinamis

## Dukungan

Jika Anda merasa ini berguna, pertimbangkan untuk mendukung pengembangan:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Lisensi

MIT
