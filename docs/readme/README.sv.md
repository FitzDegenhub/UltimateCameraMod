[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | **Svenska** | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.2 ar har!** Sacred God Mode-overrides, Lock-on Auto-Rotate-vaxling och alla buggfixar. Ladda ner fran **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** eller **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Fristaende kameraverktyg for Crimson Desert. Fullt GUI, kameraforhandsvisning i realtid, tre redigeringnivaer, filbaserade presets, **JSON-export for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** och **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), samt stod for ultrawide-HUD.

<p align="center">
  <img src="../../screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banner" width="100%" />
</p>

<p align="center">

[![Download v3.2](https://img.shields.io/badge/Download-v3.2-brightgreen?style=for-the-badge&logo=github)](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.2)
[![Nexus Mods](https://img.shields.io/badge/Nexus_Mods-UCM-d98f40?style=for-the-badge&logo=nexusmods&logoColor=white)](https://www.nexusmods.com/crimsondesert/mods/438)
[![Wiki](https://img.shields.io/badge/Wiki-Documentation-8B5CF6?style=for-the-badge&logo=bookstack&logoColor=white)](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)

[![VirusTotal v3.2](https://img.shields.io/badge/VirusTotal_v3.2-Clean-blue?style=for-the-badge&logo=virustotal&logoColor=white)](https://www.virustotal.com/gui/file-analysis/ZWMzZGM4MGM3ZWFlZTY5MTFmZDYwYzNkODFlZGM4Mjg6MTc3NTkxMzY4Mg==)
[![Reddit Discussion](https://img.shields.io/badge/Reddit-Discussion-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/CrimsonDesert/comments/1sfou61/ucm_ultimate_camera_mod_v3_crimson_desert_full/)
[![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)](LICENSE)
[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

</p>

> Behover du hjalp? Kolla **[wikin](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** for installationsguider, forklaringar av kamerainstellningar, presethantering, felsokning och utvecklardokumentation.

---

<details>
<summary><strong>Skarmdumpar (v3.x)</strong> -- klicka for att expandera</summary>
<br>

**UCM Quick** -- avstand, hojd, forskjutning, FoV, lock-on-zoom, steadycam, forhandsvisning i realtid
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- kurerad djupjustering med sokbara inramade kort
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** -- fullstandig XML-redigerare med jamforelse mot vanilla
![God Mode](../../screenshots/v3.x/godmode.png)

**JSON-export** -- export for JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**Import** -- importera fran .ucmpreset, XML, PAZ eller Mod Manager-paket
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## Grenoversikt

| Gren | Status | Vad det ar |
|------|--------|------------|
| **`main`** | v3.2 Release | Fristaende kameraverktyg med tre-stegs redigerare (UCM Quick / Fine Tune / God Mode), filbaserade presets, communityns katalog, export i flera format och direkt PAZ-installation |
| **`development`** | Utveckling | Utvecklingsgren for nasta version |

v3 inkluderar alla kamerafunktioner fran v2 plus ett omdesignat gransnitt, filbaserade presets, en tre-stegs redigerare och export i flera format. Direkt PAZ-installation ar fortfarande tillganglig i v3 som ett sekundart alternativ.

---

## Funktioner

### Kamerakontroller

| Funktion | Detaljer |
|----------|---------|
| **8 inbyggda presets** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival -- med forhandsvisning i realtid |
| **Anpassad kamera** | Reglage for avstand (1,5-12), hojd (-1,6-1,5) och horisontell forskjutning (-3-3). Proportionell skalning haller karaktaren pa samma skarmposition vid alla zoomnivaer |
| **Synfalt** | Vanilla 40 grader upp till 80 grader. Enhetligt synfalt over guard, aim, mount, glide och cinematic-lagen |
| **Centrerad kamera** | Karaktaren centrerad over 150+ kameratillstand, eliminerar den vansterforskjutna axelkameran |
| **Lock-on-zoom** | Reglage fran -60% (zooma in pa malet) till +60% (dra ut bredare). Paverkar alla lock-on-, guard- och rush-tillstand. Fungerar oberoende av Steadycam |
| **Lock-on Auto-Rotate** | Inaktivera kamerasnappning mot malet vid lock-on. Forhindrar att kameran snurrar runt for att fokusera fiender bakom dig. Tack till [@sillib1980](https://github.com/sillib1980) |
| **Riddjurskamerasynk** | Riddjurskameror matchar din valda spelarkamerahojd |
| **Horisontell forskjutning pa alla riddjur** | Hast, elefant, wyvern, kanot, krigsmaskin och kvast respekterar alla din forskjutningsinstallning med proportionell skalning |
| **Siktningskonsistens for fardigheter** | Lantern, Blinding Flash, Bow och alla siktnings-/zoom-/interaktionsfardigheter respekterar horisontell forskjutning. Ingen kamerasnappning nar du aktiverar fardigheter |
| **Steadycam-utjamning** | Normaliserade blandningstider och hastighetssvaj over 30+ kameratillstand: idle, walk, run, sprint, combat, guard, rush/charge, freefall, super jump, rope pull/swing, knockback, alla lock-on-varianter, mount lock-on, revive lock-on, aggro/wanted, warmachine och alla mount-tillstand. Varje varde ar community-justerbart via Fine Tune-redigeraren |
| **Sacred God Mode** | Varden du redigerar i God Mode ar permanent skyddade fran UCM Quick/Fine Tune-ombyggnader. Grona indikatorer visar vilka varden som ar sacred. Lagring per preset |

> **v3-designfilosofi: enbart varderedigeringar, ingen strukturell injektion.**
>
> Tidigare versioner injicerade nya XML-rader i kamerafilen (extra zoomnivaer, hastens forstapersonslage, hastens kameraomarbetning med ytterligare zoomnivaer). v3 tar bort dessa funktioner medvetet. Att injicera struktur har mycket hogre risk att ga sonder efter speluppdateringar, och personliga preferenser for nischade kameralegen betjanas battre av dedikerade moddar distribuerade via moddhanterare. UCM andrar nu bara befintliga varden -- samma antal rader, samma elementstruktur, samma attribut. Detta gor presets sakrare att dela och mer motstandskraftiga mot spelpatchar.

### Tre-stegs redigerare (v3)

v3 organiserar redigering i tre flikar sa att du kan ga sa djupt du vill:

| Niva | Flik | Vad den gor |
|------|------|-------------|
| 1 | **UCM Quick** | Det snabba lagret -- reglage for avstand/hojd/forskjutning, FoV, centrerad kamera, lock-on-zoom (-60% till +60%), lock-on Auto-Rotate, riddjurssynk, steadycam, forhandsvisning av kamera och FoV i realtid |
| 2 | **Fine Tune** | Kurerad djupjustering. Sokbara sektioner for fotgangarzoomnivaer, hast-/riddjurszoom, globalt FoV, specialriddjur och forflyttning, strid och lock-on, kamerautjamning och siktning och harkorsposition. Bygger ovanpa UCM Quick |
| 3 | **God Mode** | Fullstandig XML-redigerare -- varje parameter i ett sokbart, filtrerbart DataGrid grupperat efter kameratillstand. Jamforelsekolumn mot vanilla. Sacred-overrides (grona) skyddade fran ombyggnader. "Sacred only"-filter. 54 attribut-tooltips |

### Filbaserat presetsystem (v3)

- **`.ucmpreset`-filformat** -- dedikerat delbart format for UCM-kamerapresets. Slapp i valfri presetmapp sa fungerar det direkt
- **Sidofalthanterare** med hopfallbara grupperade sektioner: Game Default, UCM Presets, Community Presets, My Presets, Imported
- **Skapa / Duplicera / Byt namn / Ta bort** fran sidofaltet
- **Las** presets for att forhindra oavsiktliga andringar -- UCM-presets ar permanent lasta; anvandarpresets kan vaxlas via hanglasikonen
- **Akt Vanilla-preset** -- ra avkodad `playercamerapreset` fran din spelbackup utan nagra andringar. Snabbreglage ar synkade med spelets faktiska baslinjevarden
- **Importera** fran `.ucmpreset`, ra XML, PAZ-arkiv eller Mod Manager-paket. `.ucmpreset`-importer far full UCM-reglerkontroll; ra XML-/PAZ-/Mod Manager-importer ar fristaende presets (enbart God Mode-redigering, inga UCM-regler tillampas) for att bevara originalmoddarens varden
- **Autosparning** -- andringar i olasta presets skrivs automatiskt tillbaka till presetfilen (fordrojd)
- Automatisk migrering fran aldra `.json`-presets till `.ucmpreset` vid forsta start

### Presetkataloger (v3)

Blaadra och ladda ner presets direkt fran UCM. Ett-klicks nedladdning, inga konton kravs.

- **UCM Presets** -- 7 officiella kamerastilar (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definitioner lagrade pa GitHub, sessions-XML skapas lokalt fran dina spelfiler + aktuella kameraregler. Aterrbygger automatiskt nar kameraregler uppdateras
- **[Community-presets](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- communityns bidragande presets i huvudrepot, katalog autogenererad av GitHub Actions
- **Bladdringsknapp** pa varje sidofaltsgruppshuvud oppnar katalogbladdraren
- Varje preset visar namn, forfattare, beskrivning, taggar och en lank till skaparens Nexus-sida
- **Uppdateringsdetektion** -- pulserande uppdateringsikon nar en nyare version finns tillganglig i katalogen. Klicka for att ladda ner uppdateringen med valfri backup till My Presets
- Nerladdade presets visas i sidofaltet (lasta som standard -- duplicera for att redigera)
- **2 MB filstorleksgrans** och JSON-validering for sakerhet

**Vill du dela din preset med communityn?** Exportera som `.ucmpreset` fran UCM, sedan antingen:
- Skicka en [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) och lagg till din preset i mappen `community_presets/`
- Eller skicka din `.ucmpreset`-fil till 0xFitz pa Discord/Nexus sa lagger vi till den at dig

### Export i flera format (v3)

Dialogen **Exportera for delning** matar ut din session pa fyra satt:

| Format | Anvandningsomrade |
|--------|-------------------|
| **JSON** (moddhanterare) | Byte-patchar + `modinfo` for **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) eller **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Exportera i UCM, importera i den hanterare du anvander; mottagare behover inte UCM. **Prepare** erbjuds bara nar den aktiva `playercamerapreset`-posten fortfarande matchar UCM:s vanilla-backup (verifiera spelfiler om du redan har applicerat kameramoddar). |
| **XML** | Ra `playercamerapreset.xml` for andra verktyg eller manuell redigering |
| **0.paz** | Patchat arkiv redo att slappas i spelets `0010`-mapp |
| **.ucmpreset** | Full UCM-preset for andra UCM-anvandare |

Inkluderar titel, version, forfattare, Nexus-URL och beskrivningsfalt for JSON/XML. Visar antal patchregioner och andrade bytes innan `.json` sparas.

### Livskvalitetsfunktioner

- **Automatisk speldetektering** -- Steam, Epic Games, Xbox / Game Pass
- **Automatisk backup** -- vanilla-backup fore alla andringar; aterstellning med ett klick. Versionsmedveten med automatisk rensning vid uppgradering
- **Installationskonfigurationsbanner** -- visar din fullstandiga aktiva konfiguration (FoV, avstand, hojd, forskjutning, installningar)
- **Speluppdateringsmedvetenhet** -- sparar installationsmetadata efter applicering; varnar nar spelet kan ha uppdaterats sa att du kan omexportera
- **Kamera- och FoV-forhandsvisning i realtid** -- avstandsmedveten oviftsvy med horisontell forskjutning och synfaltskona
- **Uppdateringsnotiser** -- kontrollerar GitHub-releaser vid start
- **Spelmappsgenvag** -- oppnar din spelkatalog fran huvudet
- **Windows aktivitetsfaltidentitet** -- korrekt ikongruppering och titelfaltikon via shell property store
- **Installningsbestandighet** -- alla val komms ihag mellan sessioner
- **Storleksanderbart fonster** -- storlek behalls mellan sessioner
- **Portabel** -- en enda `.exe`, ingen installation kravs

### Filosofi

> **Ingen har perfektionerat Crimson Deserts kamera annu -- och det ar det som ar poangen.**
>
> Vaniljspelet har over 150 kameratillstand, vart och ett med dussintals parametrar. Ingen enskild utvecklare kan finjustera allt det for varje spelstil och skarm. Darfor finns UCM -- inte for att beratta for dig vad den perfekta kameran ar, utan for att ge dig verktygen att hitta den sjalv och dela den med andra.
>
> Varje installning du justerar kan exporteras och delas. Lock-on Auto-Rotate-fixen som eliminerade kamerasnappning under strid upptacktes av en enda communitymedlem som experimenterade i God Mode. Den typen av communitydriven finjustering ar precis vad det har verktyget ar till for.

### Presetdelning

Exportera din kamerauppstallning som en `.ucmpreset`-fil och dela den med andra. Importera presets fran communitykatalogen, Nexus Mods eller andra spelare. UCM exporterar aven till JSON (for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) och [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), ra XML och direkt PAZ-installation.

---

## Hur det fungerar

1. Lokaliserar spelets PAZ-arkiv som innehaller `playercamerapreset.xml`
2. Skapar en backup av originalfilen (bara en gang -- skriver aldrig over en ren backup)
3. Dekrypterar arkivposten (ChaCha20 + Jenkins hash-nyckelharleddning)
4. Dekomprimerar via LZ4
5. Tolkar och modifierar XML-kameraparametrarna baserat pa dina val
6. Aterkomprimerar, aterkrypterar och skriver tillbaka den modifierade posten till arkivet

Ingen DLL-injektion, ingen minnesmanipulering, ingen internetanslutning kravs -- enbart datafilsmodifiering.

---

## Bygga fran kallkod

Kraver [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (eller nyare). Windows x64.

### v3 (rekommenderad)

Stang alla korande instanser innan du bygger -- exe-kopieringssteget misslyckas om filen ar last.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Beroenden (NuGet -- aterstalls automatiskt)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) -- LZ4 block-komprimering/dekomprimering

---

## Projektstruktur

```
src/UltimateCameraMod/              Delat bibliotek + v2.x WPF-app
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, datamodeller
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x-gransnitt
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 export-forst WPF-app (refererar delad kod ovan)
├── Controls/                       CameraPreview, FovPreview (v3-varianter)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Inbaddade communitypresets som distribueras vid forsta start
├── MainWindow.xaml                 Tvapanelsskal: sidofalt + flikredigerare
├── ExportJsonDialog.xaml           Exportguide for flera format (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Importera fran .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Presetmetadata-inmatning (namn, forfattare, beskrivning, URL)
├── CommunityBrowserDialog.xaml     Bladddra och ladda ner communitypresets fran GitHub
├── NewPresetDialog.xaml            Skapa / namnsatt nya presets
├── ShellTaskbarPropertyStore.cs    Windows aktivitetsfaltikon via shell property store
├── ApplicationIdentity.cs          Delat App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  Communityns bidragande kamerapresets
ucm_presets/                        Officiella UCM-stilpresetdefinitioner
```

---

## Kompatibilitet

- **Plattformar:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Skarm:** Alla bildforhallanden -- 16:9, 21:9, 32:9

---

## Vanliga fragor

**Kan jag bli bannad for detta?**
UCM modifierar enbart offline-datafiler. Det ror inte spelminnet, injicerar ingen kod och interagerar inte med korande processer. Anvand efter eget omdome i online-/multiplayerlage.

**Spelet uppdaterades och min kamera ar tillbaka till vanilla.**
Normalt -- speluppdateringar skriver over moddade filer. Oppna UCM igen och klicka pa Installera (eller omexportera JSON for JSON Mod Manager / CDUMM). Dina installningar sparas automatiskt.

**Mitt antivirusprogram flaggade exe-filen.**
Kand falsk positiv med fristaende .NET-appar. VirusTotal-skanningen ar ren: [v3.2](https://www.virustotal.com/gui/file-analysis/ZWMzZGM4MGM3ZWFlZTY5MTFmZDYwYzNkODFlZGM4Mjg6MTc3NTkxMzY4Mg==). Fullstandig kallkod finns tillganglig har for granskning och egen kompilering.

**Vad betyder horisontell forskjutning 0?**
0 = vaniljkameraposition (karaktaren lite till vanster). 0,5 = karaktaren centrerad pa skarmen. Negativa varden flyttar langre at vanster, positiva varden flyttar langre at hoger.

**Uppgraderar fran en tidigare version?**
v3.x-anvandare: byt bara ut exe-filen, alla presets och installningar bevaras. v2.x-anvandare: ta bort den gamla UCM-mappen, verifiera spelfiler pa Steam och kor sedan v3.1 fran en ny mapp. Se [versionsinformationen](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) for detaljerade instruktioner.

---

## Versionshistorik

- **v3.2** -- Fix for sacred-varden som saknades fran Install/exporter pa God Mode-fliken. Se [versionsinformation](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.2).
- **v3.1.1** -- Fix for falsk-positiv detektering av skadad backup pa rena spelfiler.
- **v3.1** -- Sacred God Mode-overrides (anvandarredigeringar permanent skyddade fran ombyggnader). Lock-on Auto-Rotate-vaxling (tack till [sillib1980](https://github.com/sillib1980)). Grona sacred-indikatorer. Fix for Full Manual Control-installation. Versionsmedveten uppgraderingsoverlagring. Se [versionsinformation](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** -- Alla dialoger konverterade till overlagringssystem i appen. God Mode-overrides behalls over flikbyten. Val av presettyp (UCM Managed vs Full Manual Control). Communitypresentkatalog flyttad till huvudrepot. 54 God Mode-attribut-tooltips. Spelkraschfixar. Vanilla-validering uppdaterad for spelpatchen juni 2026. 21-sidig Wiki.
- **v3.0.1** -- Export-forst-omdesign. Tre-stegs redigerare (UCM Quick / Fine Tune / God Mode). `.ucmpreset`-filformat. Filbaserat presetsystem. UCM- och communitypresetkataloger. Export i flera format. Steadycam utokad till 30+ kameratillstand. Lock-on-zoom-reglage.
- **v2.5** -- Sista v2.x-versionen.
- **v2.4** -- Proportionell horisontell forskjutning, forskjutning pa alla riddjur och siktningsfardigheter, hastens kameraomarbetning, versionsmedvetna backuper, FoV-forhandsvisning, storleksanderbart fonster.
- **v2.3** -- Horisontell forskjutningsfix for 16:9, deltabaserat reglage, fullstandigt installationskonfigurationsbanner.
- **v2.2** -- Steadycam, extra zoomnivaer, hastens forstapersonslage, horisontell forskjutning, universellt FoV, siktningskonsistens for fardigheter, XML-import, presetdelning, uppdateringsnotiser.
- **v2.1** -- Fix for anpassade presetreglage som inte skrevs till alla zoomnivaer.
- **v2.0** -- Fullstandig omskrivning fran Python till C# / .NET 6 / WPF. Avancerad XML-redigerare, presethantering, automatisk speldetektering.
- **v1.5** -- Python-version med customtkinter-GUI.

---

## Erkannanden och tack

- **0xFitz** -- UCM-utveckling, kamerajustering, avancerad redigerare
- **[@sillib1980](https://github.com/sillib1980)** -- Upptackte Lock-on Auto-Rotate-kamerafalt

### C#-omskrivning (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** -- CrimsonDesertTools -- C# PAZ/PAMT-parser, ChaCha20-kryptering, LZ4-komprimering, PaChecksum, arkivompackare (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** -- Ren C# ChaCha20-stromchifferimplementering (BSD)
- **[MrIkso pa Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** -- PAZ-ompackningsguide: 16-byte-justering, PAMT-checksumma, PAPGT-rotindexpatchning

### Ursprunglig Python-version (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** -- crimson-desert-unpacker -- PAZ-arkivverktyg, dekrypteringsforskning
- **Maszradine** -- CDCamera -- Kameraregler, steadycam-system, stilpresets
- **manymanecki** -- CrimsonCamera -- Dynamisk PAZ-modifieringsarkitektur

## Stod

Om du tycker att detta ar anvandbart, overv att stodja utvecklingen:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Licens

MIT
