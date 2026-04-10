[English](README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | **Türkçe** | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 yayinlandi!** Sacred God Mode gecersiz kilma, Lock-on Auto-Rotate acma/kapama ve tum hata duzeltmeleri. **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** veya **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)** uzerinden indirin.

# Ultimate Camera Mod - Crimson Desert

Crimson Desert icin bagimsiz kamera araci. Tam GUI, canli kamera onizleme, uc katmanli duzenleme, dosya tabanli preset'ler, **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** ve **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) icin **JSON disa aktarma** ve genis ekran HUD destegi.

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banner" width="100%" />
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

> Yardima mi ihtiyaciniz var? Kurulum kilavuzlari, kamera ayarlarinin aciklamalari, preset yonetimi, sorun giderme ve gelistirici belgeleri icin **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** sayfasina goz atin.

---

<details>
<summary><strong>Ekran goruntuleri (v3.x)</strong> -- genisletmek icin tiklayin</summary>
<br>

**UCM Quick** -- mesafe, yukseklik, kaydirma, FoV, lock-on zoom, steadycam, canli onizlemeler
![UCM Quick](screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- aranabilir cerceveli kartlarla secilmis derin ayar
![Fine Tune](screenshots/v3.x/finetune.png)

**God Mode** -- vanilla karsilastirmali tam ham XML duzenleyici
![God Mode](screenshots/v3.x/godmode.png)

**JSON Disa Aktarma** -- JSON Mod Manager / CDUMM icin disa aktarma
![Export JSON](screenshots/v3.x/exportjson_menu.png)

**Ice Aktarma** -- .ucmpreset, XML, PAZ veya Mod Manager paketlerinden ice aktarma
![Import](screenshots/v3.x/import_screen.png)

</details>

---

## Dal genel bakisi

| Dal | Durum | Aciklama |
|-----|-------|----------|
| **`main`** | v3.1.2 Surum | Uc katmanli duzenleyici (UCM Quick / Fine Tune / God Mode), dosya tabanli preset'ler, topluluk katalogu, coklu format disa aktarma ve dogrudan PAZ kurulumu iceren bagimsiz kamera araci |
| **`development`** | Gelistirme | Sonraki surum gelistirme dali |

v3, v2'deki tum kamera ozelliklerini yeniden tasarlanmis bir arayuz, dosya tabanli preset'ler, uc katmanli duzenleyici ve coklu format disa aktarma ile birlikte icermektedir. Dogrudan PAZ kurulumu, v3'te ikincil bir secenek olarak hala mevcuttur.

---

## Ozellikler

### Kamera kontrolleri

| Ozellik | Detaylar |
|---------|----------|
| **8 yerlesik preset** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival - canli onizleme ile |
| **Ozel kamera** | Mesafe (1.5-12), yukseklik (-1.6-1.5) ve yatay kaydirma (-3-3) icin kaydiriciler. Orantisal olcekleme, tum yakinlastirma seviyelerinde karakteri ayni ekran konumunda tutar |
| **Gorus alani** | Vanilla 40 dereceden 80 dereceye kadar. Savunma, nisan, binek, suzulme ve sinematik durumlarda evrensel FoV tutarliligi |
| **Merkezlenmis kamera** | 150'den fazla kamera durumunda karakteri tam ortaya alarak sol ofsetli omuz kamerasini ortadan kaldirir |
| **Lock-on zoom** | -%60 (hedefe yakinlastirma) ile +%60 (geri cekilme) arasinda kaydirici. Tum lock-on, savunma ve hamle durumlarini etkiler. Steadycam'den bagimsiz calisir |
| **Lock-on auto-rotate** | Kilitleme sirasinda kameranin hedefe yapismasini devre disi birakin. Arkanizda kalan dusmanlara kameranin hizla donmesini onler. [@sillib1980](https://github.com/sillib1980)'e tesekkurler |
| **Binek kamera senkronizasyonu** | Binek kameralari sectiginiz oyuncu kamera yuksekligine uyum saglar |
| **Tum bineklerde yatay kaydirma** | At, fil, ejderha, kano, savas makinesi ve supurge, orantisal olcekleme ile kaydirma ayariniza uyar |
| **Yetenek nisan tutarliligi** | Fener, Kor Etme Isigi, Yay ve tum nisan/yakinlastirma/etkilesim yetenekleri yatay kaydirmayi uygular. Yetenekleri etkinlestirirken kamera atlama yapmaz |
| **Steadycam yumustama** | 30'dan fazla kamera durumunda normallestirilmis karistirma zamanlama ve hiz sallanimi: bosta, yurume, kosma, sprint, savas, savunma, hamle/sarj, serbest dusus, super ziplama, ip cekme/sallanma, geri tepme, tum lock-on varyantlari, binek lock-on, diriltme lock-on, tahrik/aranma, savas makinesi ve tum binek durumlari. Her deger, Fine Tune duzenleyicisi araciligiyla topluluk tarafindan ayarlanabilir |
| **Sacred God Mode** | God Mode'da duzenlediginiz degerler, Quick/Fine Tune yeniden olusturmalarina karsi kalici olarak korunur. Yesil gostergeler hangi degerlerin korunmus oldugunu gosterir. Preset bazinda depolama |

> **v3 tasarim felsefesi: sadece deger duzenleme, yapisal enjeksiyon yok.**
>
> Onceki surumler kamera dosyasina yeni XML satirlari enjekte ediyordu (ekstra yakinlastirma seviyeleri, at birinci kisi modu, ek yakinlastirma katmanlariyla at kamerasi revizyonu). v3 bu ozellikleri bilerek kaldirir. Yapi enjeksiyonunun oyun guncellemelerinden sonra bozulma olasiligi cok daha yuksektir ve nis kamera modlari icin kisisel tercihler, mod yoneticileri araciligiyla dagitilan ozel modlarla daha iyi karsilanir. UCM artik yalnizca mevcut degerleri degistirir -- ayni satir sayisi, ayni eleman yapisi, ayni ozellikler. Bu, preset'lerin paylasimini daha guvenli ve oyun yamalari arasinda daha dayanikli kilar.

### Uc katmanli duzenleyici (v3)

v3, istediginiz kadar derine inebilmeniz icin duzenlemeyi uc sekmeye ayirir:

| Katman | Sekme | Ne yapar |
|--------|-------|----------|
| 1 | **UCM Quick** | Hizli katman - mesafe/yukseklik/kaydirma kaydiricilari, FoV, merkezlenmis kamera, lock-on zoom (-%60 ile +%60), lock-on auto-rotate, binek senkronizasyonu, steadycam, canli kamera + FoV onizlemeleri |
| 2 | **Fine Tune** | Secilmis derin ayar. Yaya yakinlastirma seviyeleri, at/binek yakinlastirma, genel FoV, ozel binekler ve gezinti, savas ve lock-on, kamera yumustama ve nisan ve nisangah pozisyonu icin aranabilir bolumler. UCM Quick uzerine insa eder |
| 3 | **God Mode** | Tam ham XML duzenleyici - kamera durumuna gore gruplandilmis, aranabilir, filtrelenebilir DataGrid'de her parametre. Vanilla karsilastirma sutunu. Yeniden olusturmalara karsi korunan Sacred gecersiz kilmalar (yesil). "Yalnizca Sacred" filtresi. 54 ozellik ipucu |

### Dosya tabanli preset sistemi (v3)

- **`.ucmpreset` dosya formati** - UCM kamera preset'leri icin paylasilabilir ozel format. Herhangi bir preset klasorune birakin, calisir
- **Yan cubuk yoneticisi** - daraltilabilir gruplandirmali bolumlerle: Oyun Varsayilani, UCM Preset'leri, Topluluk Preset'leri, Preset'lerim, Ice Aktarilmis
- Yan cubuktan **Yeni / Kopyala / Yeniden Adlandir / Sil**
- Kazara duzenlemeyi onlemek icin preset'leri **kilitleyin** - UCM preset'leri kalici olarak kilitlidir; kullanici preset'leri asma kilidi simgesiyle acilip kapatilabilir
- **Gercek Vanilla preset'i** - oyun yedeginizdeki ham cozulmus `playercamerapreset`, hicbir degisiklik uygulanmamis. Hizli kaydiriclar gercek oyun temel degerlerine senkronize edilir
- **Ice aktarma** - `.ucmpreset`, ham XML, PAZ arsivleri veya Mod Manager paketlerinden. `.ucmpreset` ice aktarmalari tam UCM kaydirici kontrolu saglar; ham XML/PAZ/mod manager ice aktarmalari bagimsiz preset'lerdir (yalnizca God Mode duzenleme, UCM kurallari uygulanmaz) - orijinal mod yazarinin degerlerini korumak icin
- **Otomatik kayit** - kilitlenmemis preset'lerdeki degisiklikler otomatik olarak preset dosyasina yazilir (geciktirilmis)
- Ilk baslangicta eski `.json` preset'lerden `.ucmpreset`'e otomatik gec

### Preset kataloglari (v3)

Preset'leri dogrudan UCM icerisinden gozatip indirin. Tek tikla indirme, hesap gerekli degil.

- **UCM Preset'leri** - 7 resmi kamera stili (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Tanimlar GitHub'da barindiriliyor, oturum XML'i oyun dosyalariniz + guncel kamera kurallarinizla yerel olarak olusturuluyor. Kamera kurallari guncellendiginde otomatik yeniden olusturur
- **[Topluluk preset'leri](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** - ana depodaki topluluk tarafindan katki saglanmis preset'ler, katalog GitHub Actions tarafindan otomatik olusturulur
- Her yan cubuk grup basliginda **Gozat butonu** katalog tarayicisini acar
- Her preset ad, yazar, aciklama, etiketler ve yazarin Nexus sayfasina baglanti gosterir
- **Guncelleme algilama** - katalogda daha yeni bir surum mevcut oldugunda yanip sonen guncelleme simgesi. Guncellemeyi indirmek icin tiklayin, istege bagli olarak Preset'lerim'e yedekleme
- Indirilen preset'ler yan cubukta gorunur (varsayilan olarak kilitli - duzenlemek icin kopyalayin)
- Guvenlik icin **2MB dosya boyutu siniri** ve JSON dogrulamasi

**Preset'inizi toplulukla paylasmak mi istiyorsunuz?** UCM'den `.ucmpreset` olarak disa aktarin, ardindan:
- `community_presets/` klasorune preset'inizi ekleyen bir [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) gonderin
- Veya `.ucmpreset` dosyanizi Discord/Nexus uzerinden 0xFitz'e gonderin, sizin icin ekleyelim

### Coklu format disa aktarma (v3)

**Paylasim icin disa aktar** diyalogu oturumunuzu dort sekilde cikarir:

| Format | Kullanim alani |
|--------|----------------|
| **JSON** (mod yoneticileri) | **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) veya **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) icin byte yamalari + `modinfo`. UCM'de disa aktarin, kullandiginiz yoneticiye ice aktarin; alicilarin UCM'ye ihtiyaci yoktur. **Hazirla** yalnizca canli `playercamerapreset` girisi UCM'nin vanilla yedegi ile hala eslesiyorsa sunulur (zaten kamera modlari uyguladiysaniz oyun dosyalarini dogrulayin). |
| **XML** | Diger araclar veya manuel duzenleme icin ham `playercamerapreset.xml` |
| **0.paz** | Oyunun `0010` klasorune birakilmaya hazir yamalanmis arsiv |
| **.ucmpreset** | Diger UCM kullanicilari icin tam UCM preset'i |

JSON/XML icin baslik, surum, yazar, Nexus URL ve aciklama alanlari icermektedir. `.json` kaydetmeden once yama bolgesi sayisini ve degisen byte'lari gosterir.

### Yasam kalitesi

- **Otomatik oyun algilama** - Steam, Epic Games, Xbox / Game Pass
- **Otomatik yedekleme** - herhangi bir degisiklikten once vanilla yedegi; tek tikla geri yukleme. Surum farkindaligi ile yukseltmede otomatik temizleme
- **Kurulum yapilandirma banneri** - tam aktif yapilandirmanizi gosterir (FoV, mesafe, yukseklik, kaydirma, ayarlar)
- **Oyun yamasi farkindaligi** - uygulamadan sonra kurulum meta verilerini takip eder; oyun guncellenmis olabilecegi durumlarda uyarir, boylece yeniden disa aktarabilirsiniz
- **Canli kamera + FoV onizlemesi** - yatay kaydirma ve gorus alani konisi ile mesafe duyarli kusbakisi gorunum
- **Guncelleme bildirimleri** - baslatmada GitHub surumlerini kontrol eder
- **Oyun klasoru kisayolu** - basliktan oyun dizininizi acar
- **Windows gorev cubugu kimligi** - shell ozellik deposu araciligiyla uygun simge gruplama ve baslik cubugu simgesi
- **Ayar kaliciligi** - tum secimler oturumlar arasinda hatirlanir
- **Yeniden boyutlanabilir pencere** - boyut oturumlar arasinda korunur
- **Tasinabilir** - tek `.exe`, yükleyici gerektirmez

### Felsefe

> **Henuz kimse Crimson Desert'in kamerasini mukemmellestirmedi -- ve asil mesele de bu.**
>
> Vanilla oyunda her biri duzenlerle parametreye sahip 150'den fazla kamera durumu var. Hicbir gelistirici tek basina bunlarin tumunu her oyun tarzi ve ekran icin ayarlayamaz. UCM bunun icin var -- size mukemmel kameranin ne oldugunu soylelemek icin degil, kendiniz bulmaniz ve baskalariyla paylasmaniz icin araclari vermek icin.
>
> Ayarladiginiz her deger disa aktarilip paylasilabilir. Savas sirasinda kamera atlamasini ortadan kaldiran lock-on auto-rotate duzeltmesi, God Mode'da deney yapan tek bir topluluk uyesi tarafindan kesfedildi. Bu tur topluluk odakli ince ayar tam olarak bu aracin varlik amaci.

### Preset paylasimi

Kamera ayarinizi `.ucmpreset` dosyasi olarak disa aktarip baskalariyla paylasin. Topluluk katalogundan, Nexus Mods'tan veya diger oyunculardan preset'leri ice aktarin. UCM ayrica JSON ([JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) ve [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207) icin), ham XML ve dogrudan PAZ kurulumuna da disa aktarma yapar.

---

## Nasil calisir

1. `playercamerapreset.xml` iceren oyunun PAZ arsivini bulur
2. Orijinal dosyanin bir yedegini olusturur (yalnizca bir kez -- temiz bir yedegin uzerine asla yazmaz)
3. Arsiv girisinin sifresini cozer (ChaCha20 + Jenkins hash anahtar turetimi)
4. LZ4 ile acilir
5. Secimlerinize gore XML kamera parametrelerini ayristirip degistirir
6. Yeniden sikistirir, yeniden sifreler ve degistirilmis girisi arsive geri yazar

DLL enjeksiyonu yok, bellek mudahalesi yok, internet baglantisi gerekli degil -- saf veri dosyasi degisikligi.

---

## Kaynaktan derleme

[.NET 6 SDK](https://dotnet.microsoft.com/download/net/6.0) (veya ustunu) gerektirir. Windows x64.

### v3 (onerilen)

Derlemeden once calisan herhangi bir ornegi kapatin -- exe kopyalama adimi dosya kilitliyse basarisiz olur.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Bagimliliklar (NuGet - otomatik olarak geri yuklenir)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - LZ4 blok sikistirma/acma

---

## Proje yapisi

```
src/UltimateCameraMod/              Paylasilan kutuphane + v2.x WPF uygulamasi
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, veri modelleri
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x Arayuz
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 disa aktarma oncelikli WPF uygulamasi (yukardaki paylasilan koda referans verir)
├── Controls/                       CameraPreview, FovPreview (v3 varyantlari)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Ilk baslatmada dagitilan gomulu topluluk preset'leri
├── MainWindow.xaml                 Iki panelli kabuk: yan cubuk + sekmeli duzenleyici
├── ExportJsonDialog.xaml           Coklu format disa aktarma sihirbazi (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         .ucmpreset / XML / PAZ'dan ice aktarma
├── ImportMetadataDialog.xaml       Preset meta veri girisi (ad, yazar, aciklama, URL)
├── CommunityBrowserDialog.xaml     GitHub'dan topluluk preset'lerini gozat ve indir
├── NewPresetDialog.xaml            Yeni preset olustur / adlandir
├── ShellTaskbarPropertyStore.cs    Shell ozellik deposu araciligiyla Windows gorev cubugu simgesi
├── ApplicationIdentity.cs          Paylasilan App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  Topluluk tarafindan katki saglanmis kamera preset'leri
ucm_presets/                        Resmi UCM stil preset tanimlari
```

---

## Uyumluluk

- **Platformlar:** Steam, Epic Games, Xbox / Game Pass
- **Isletim Sistemi:** Windows 10/11 (x64)
- **Ekran:** Herhangi bir en-boy orani - 16:9, 21:9, 32:9

---

## SSS

**Bu ban yemememe neden olur mu?**
UCM yalnizca cevrimdisi veri dosyalarini degistirir. Oyun bellegine dokunmaz, kod enjekte etmez veya calisan islemlerle etkilesime girmez. Cevrimici/cok oyunculu modlarda kendi takdirinize bagli olarak kullanin.

**Oyun guncellendi ve kameram vanillaya dondu.**
Normal -- oyun guncellemeleri modlanmis dosyalarin uzerine yazar. UCM'yi yeniden acin ve Kur'a tiklayin (veya JSON Mod Manager / CDUMM icin JSON'u yeniden disa aktarin). Ayarlariniz otomatik olarak kaydedilir.

**Antivirusum exe'yi isaretledi.**
Bagimsiz .NET uygulamalarinda bilinen yanlis pozitif. VirusTotal taramasi temiz: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). Tam kaynak kodu burada incelenip kendiniz derlenebilir.

**Yatay kaydirma 0 ne anlama geliyor?**
0 = vanilla kamera konumu (karakter hafifce solda). 0.5 = karakter ekranda ortalanmis. Negatif degerler daha sola, pozitif degerler daha saga tasir.

**Onceki surumden yukseltme mi yapiyorsunuz?**
v3.x kullancilari: sadece exe'yi degistirin, tum preset'ler ve ayarlar korunur. v2.x kullanicilari: eski UCM klasorunu silin, Steam'de oyun dosyalarini dogrulayin, ardindan v3.1'i yeni bir klasorden calistirin. Detayli talimatlar icin [surum notlarina](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) bakin.

---

## Surum gecmisi

- **v3.1.2** - God Mode sekmesinde Install/disa aktarmalarda eksik sacred degerleri duzeltildi. [Surum notlarina](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2) bakin.
- **v3.1.1** - Temiz oyun dosyalarinda yanlis-pozitif bozulmus yedek algilama duzeltildi.
- **v3.1** - Sacred God Mode gecersiz kilmalari (kullanici duzenlemeleri yeniden olusturmalara karsi kalici olarak korunur). Lock-on Auto-Rotate acma/kapama ([sillib1980](https://github.com/sillib1980)'e tesekkurler). Yesil sacred gostergeleri. Tam Manuel Kontrol kurulum duzeltmesi. Surum farkindaligi yukseltme kaplama. [Surum notlarina](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) bakin.
- **v3.0.2** - Tum diyaloglar uygulama ici kaplama sistemine donusturuldu. God Mode gecersiz kilmalari sekme gecisleri arasinda devam eder. Preset tur secimi (UCM Yonetimli veya Tam Manuel Kontrol). Topluluk preset katalogu ana depoya tasindi. 54 God Mode ozellik ipucu. Oyun cokme duzeltmeleri. Haziran 2026 oyun yamasi icin vanilla dogrulama guncellendi. 21 sayfalik Wiki.
- **v3.0.1** - Disa aktarma oncelikli yeniden tasarim. Uc katmanli duzenleyici (UCM Quick / Fine Tune / God Mode). `.ucmpreset` dosya formati. Dosya tabanli preset sistemi. UCM ve topluluk preset kataloglari. Coklu format disa aktarma. Steadycam 30+ kamera durumuna genisletildi. Lock-on zoom kaydiricisi.
- **v2.5** - Son v2.x surumu.
- **v2.4** - Orantisal yatay kaydirma, tum binekler ve nisan yeteneklerinde kaydirma, at kamerasi revizyonu, surum farkindalikli yedekler, FoV onizlemesi, yeniden boyutlanabilir pencere.
- **v2.3** - 16:9 icin yatay kaydirma duzeltmesi, delta tabanli kaydirici, tam kurulum yapilandirma banneri.
- **v2.2** - Steadycam, ekstra yakinlastirma seviyeleri, at birinci kisi, yatay kaydirma, evrensel FoV, yetenek nisan tutarliligi, XML Ice Aktarma, preset paylasimi, guncelleme bildirimleri.
- **v2.1** - Ozel preset kaydiriclarinin tum yakinlastirma seviyelerine yazmamasini duzeltildi.
- **v2.0** - Python'dan C# / .NET 6 / WPF'ye tam yeniden yazim. Gelismis XML duzenleyici, preset yonetimi, otomatik oyun algilama.
- **v1.5** - customtkinter GUI ile Python surumu.

---

## Katkida bulunanlar ve tesekkurler

- **0xFitz** - UCM gelistirme, kamera ayarlama, gelismis duzenleyici
- **[@sillib1980](https://github.com/sillib1980)** - Lock-on Auto-Rotate kamera alanlarini kesfetti

### C# yeniden yazimi (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - C# PAZ/PAMT ayristirici, ChaCha20 sifreleme, LZ4 sikistirma, PaChecksum, arsiv yeniden paketleyici (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - Saf C# ChaCha20 akis sifresi uygulamasi (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - PAZ yeniden paketleme kilavuzu: 16-byte hizalama, PAMT sagi toplami, PAPGT kok indeks yamalama

### Orijinal Python surumu (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - PAZ arsiv araclari, sifre cozme arastirmasi
- **Maszradine** - CDCamera - Kamera kurallari, steadycam sistemi, stil preset'leri
- **manymanecki** - CrimsonCamera - Dinamik PAZ degisiklik mimarisi

## Destek

Bunu faydali buluyorsaniz, gelistirmeyi desteklemeyi dusunun:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Lisans

MIT
