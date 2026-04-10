[English](README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | **Dansk** | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 er her!** Sacred God Mode-tilsidesaettelser, Lock-on Auto-Rotate-skift og alle fejlrettelser. Download fra **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** eller **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Selvstaendigt kameravaerktoj til Crimson Desert. Fuldt GUI, kameraforhaandsvisning i realtid, tre redigeringsniveauer, filbaserede presets, **JSON-eksport til [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** og **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), samt ultrawide-HUD-understottelse.

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

> Brug for hjaelp? Tjek **[wikien](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** for opsaetningsguider, forklaring af kameraindstillinger, presethondtering, fejlfinding og udviklerdokumentation.

---

<details>
<summary><strong>Skaermbilleder (v3.x)</strong> -- klik for at udvide</summary>
<br>

**UCM Quick** -- afstand, hojde, forskydning, FoV, lock-on-zoom, steadycam, forhaandsvisning i realtid
![UCM Quick](screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- kureret dybdejustering med sogbare indrammede kort
![Fine Tune](screenshots/v3.x/finetune.png)

**God Mode** -- fuldstaendig XML-editor med vanilla-sammenligning
![God Mode](screenshots/v3.x/godmode.png)

**JSON-eksport** -- eksport til JSON Mod Manager / CDUMM
![Export JSON](screenshots/v3.x/exportjson_menu.png)

**Import** -- importer fra .ucmpreset, XML, PAZ eller Mod Manager-pakker
![Import](screenshots/v3.x/import_screen.png)

</details>

---

## Grenoversigt

| Gren | Status | Hvad det er |
|------|--------|-------------|
| **`main`** | v3.1.2 Release | Selvstaendigt kameravaerktoj med tre-trins editor (UCM Quick / Fine Tune / God Mode), filbaserede presets, faellesskabskatalog, eksport i flere formater og direkte PAZ-installation |
| **`development`** | Udvikling | Udviklingsgren til naeste version |

v3 inkluderer alle kamerafunktioner fra v2 plus et redesignet interface, filbaserede presets, en tre-trins editor og eksport i flere formater. Direkte PAZ-installation er stadig tilgaengelig i v3 som et sekundaert alternativ.

---

## Funktioner

### Kamerakontroller

| Funktion | Detaljer |
|----------|---------|
| **8 indbyggede presets** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival -- med forhaandsvisning i realtid |
| **Brugerdefineret kamera** | Skydere for afstand (1,5-12), hojde (-1,6-1,5) og horisontal forskydning (-3-3). Proportional skalering holder karakteren paa samme skaermposition ved alle zoomniveauer |
| **Synsfelt** | Vanilla 40 grader op til 80 grader. Universelt FoV-konsistens paa tvaers af guard, aim, mount, glide og cinematic-tilstande |
| **Centreret kamera** | Karakteren centreret paa tvaers af 150+ kameratilstande, eliminerer det venstreforskudte skulderkamera |
| **Lock-on-zoom** | Skyder fra -60% (zoom ind paa maalet) til +60% (traek ud bredere). Paavirker alle lock-on-, guard- og rush-tilstande. Fungerer uafhaengigt af Steadycam |
| **Lock-on Auto-Rotate** | Deaktiver kamerasnapping mod maalet ved lock-on. Forhindrer kameraet i at snurre rundt for at fokusere fjender bag dig. Tak til [@sillib1980](https://github.com/sillib1980) |
| **Ridedyr-kamerasynk** | Ridedyr-kameraer matcher din valgte spillerkamerahojde |
| **Horisontal forskydning paa alle ridedyr** | Hest, elefant, wyvern, kano, krigsmaskine og kost respekterer alle din forskydningsindstilling med proportional skalering |
| **Sigtekonsistens for faerdigheder** | Lantern, Blinding Flash, Bow og alle sigte-/zoom-/interaktionsfaerdigheder respekterer horisontal forskydning. Ingen kamerasnapping naar du aktiverer faerdigheder |
| **Steadycam-udjsevning** | Normaliserede blandingstider og hastighedsudsving paa tvaers af 30+ kameratilstande: idle, walk, run, sprint, combat, guard, rush/charge, freefall, super jump, rope pull/swing, knockback, alle lock-on-varianter, mount lock-on, revive lock-on, aggro/wanted, warmachine og alle mount-tilstande. Hver vaerdi kan justeres af faellesskabet via Fine Tune-editoren |
| **Sacred God Mode** | Vaerdier du redigerer i God Mode er permanent beskyttet mod UCM Quick/Fine Tune-genopbygninger. Gronne indikatorer viser hvilke vaerdier der er sacred. Lagring per preset |

> **v3-designfilosofi: kun vaerdiredigeringer, ingen strukturel injektion.**
>
> Tidligere versioner injicerede nye XML-linjer i kamerafilen (ekstra zoomniveauer, hestens forstepersons-tilstand, hestens kameraomarbejdning med yderligere zoomniveauer). v3 fjerner disse funktioner bevidst. At injicere struktur har meget hojere risiko for at gaa i stykker efter spilopdateringer, og personlige praeferencer for nichekameratilstande betjenes bedre af dedikerede mods distribueret via modhondterere. UCM aendrer nu kun eksisterende vaerdier -- samme antal linjer, samme elementstruktur, samme attributter. Dette gor presets sikrere at dele og mere modstandsdygtige over for spilpatches.

### Tre-trins editor (v3)

v3 organiserer redigering i tre faner saa du kan gaa saa dybt du vil:

| Niveau | Fane | Hvad den gor |
|--------|------|--------------|
| 1 | **UCM Quick** | Det hurtige lag -- skydere for afstand/hojde/forskydning, FoV, centreret kamera, lock-on-zoom (-60% til +60%), lock-on Auto-Rotate, ridedyrsynk, steadycam, forhaandsvisning af kamera og FoV i realtid |
| 2 | **Fine Tune** | Kureret dybdejustering. Sogbare sektioner for fodgaenger-zoomniveauer, hest-/ridedyrzoom, globalt FoV, specialridedyr og traversering, kamp og lock-on, kameraudjsevning og sigte og traadkorsposition. Bygger oven paa UCM Quick |
| 3 | **God Mode** | Fuldstaendig XML-editor -- hver parameter i et sogbart, filtrerbart DataGrid grupperet efter kameratilstand. Sammenligningskolonne mod vanilla. Sacred-tilsidesaettelser (gronne) beskyttet mod genopbygninger. "Sacred only"-filter. 54 attribut-tooltips |

### Filbaseret presetsystem (v3)

- **`.ucmpreset`-filformat** -- dedikeret delbart format til UCM-kamerapresets. Smid ind i en vilkaarlig presetmappe og det virker med det samme
- **Sidepanelhondterer** med sammenklappelige grupperede sektioner: Game Default, UCM Presets, Community Presets, My Presets, Imported
- **Ny / Dupliker / Omdob / Slet** fra sidepanelet
- **Laas** presets for at forhindre utilsigtede aendringer -- UCM-presets er permanent laast; brugerpresets kan skiftes via haengelaas-ikonet
- **Aedt Vanilla-preset** -- raa afkodet `playercamerapreset` fra din spilbackup uden nogen aendringer. Hurtigskydere er synkroniseret med spillets faktiske basisvaerdier
- **Importer** fra `.ucmpreset`, raa XML, PAZ-arkiver eller Mod Manager-pakker. `.ucmpreset`-importer faar fuld UCM-skyderkontrol; raa XML-/PAZ-/Mod Manager-importer er selvstaendige presets (kun God Mode-redigering, ingen UCM-regler anvendt) for at bevare den originale modforfatters vaerdier
- **Automatisk gemning** -- aendringer i ulaaste presets skrives automatisk tilbage til presetfilen (forsinket)
- Automatisk migrering fra aeldre `.json`-presets til `.ucmpreset` ved forste opstart

### Presetkataloger (v3)

Gennemse og download presets direkte fra UCM. Et-klik download, ingen konti nodvendige.

- **UCM Presets** -- 7 officielle kamerastilarter (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definitioner lagret paa GitHub, sessions-XML bygges lokalt fra dine spilfiler + gaeldende kameraregler. Genopbygges automatisk naar kameraregler opdateres
- **[Faellesskabspresets](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- faellesskabets bidragede presets i hovedrepoet, katalog autogenereret af GitHub Actions
- **Gennemse-knap** paa hvert sidepanelgruppehoved aabner katalogbrowseren
- Hvert preset viser navn, forfatter, beskrivelse, tags og et link til skaberens Nexus-side
- **Opdateringsdetektering** -- pulserende opdateringsikon naar en nyere version er tilgaengelig i katalogen. Klik for at downloade opdateringen med valgfri backup til My Presets
- Downloadede presets vises i sidepanelet (laast som standard -- dupliker for at redigere)
- **2 MB filstorrelsesgraense** og JSON-validering for sikkerhed

**Vil du dele dit preset med faellesskabet?** Eksporter som `.ucmpreset` fra UCM, derefter enten:
- Indsend en [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) og tilfoj dit preset til `community_presets/`-mappen
- Eller send din `.ucmpreset`-fil til 0xFitz paa Discord/Nexus saa tilfoojer vi den for dig

### Eksport i flere formater (v3)

Dialogen **Eksporter til deling** udsender din session paa fire maader:

| Format | Anvendelsesomraade |
|--------|--------------------|
| **JSON** (modhondterere) | Byte-patches + `modinfo` til **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) eller **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Eksporter i UCM, importer i den hondterer du bruger; modtagere behoover ikke UCM. **Prepare** tilbydes kun naar den aktive `playercamerapreset`-post stadig matcher UCM's vanilla-backup (verificer spilfiler hvis du allerede har anvendt kameramods). |
| **XML** | Raa `playercamerapreset.xml` til andre vaerktojer eller manuel redigering |
| **0.paz** | Patchet arkiv klar til at smide i spillets `0010`-mappe |
| **.ucmpreset** | Fuldt UCM-preset til andre UCM-brugere |

Inkluderer titel, version, forfatter, Nexus-URL og beskrivelsesfelt for JSON/XML. Viser antal patchregioner og aendrede bytes for gemning af `.json`.

### Livskvalitetsfunktioner

- **Automatisk spildetektering** -- Steam, Epic Games, Xbox / Game Pass
- **Automatisk sikkerhedskopi** -- vanilla-backup for eventuelle aendringer; gendannelse med et klik. Versionsbevidst med automatisk oprydning ved opgradering
- **Installationskonfigurationsbanner** -- viser din fulde aktive konfiguration (FoV, afstand, hojde, forskydning, indstillinger)
- **Spilopdateringsbevidsthed** -- lagrer installationsmetadata efter anvendelse; advarer naar spillet maaske er blevet opdateret saa du kan geneksportere
- **Kamera- og FoV-forhaandsvisning i realtid** -- afstandsbevidst ovenfra-visning med horisontal forskydning og synsfeltskegle
- **Opdateringsbeskeder** -- tjekker GitHub-udgivelser ved opstart
- **Spilmappegenvej** -- aabner din spilkatalog fra hovedet
- **Windows proceslinje-identitet** -- korrekt ikongruppering og titellinjeikon via shell property store
- **Indstillingspersistens** -- alle valg huskes mellem sessioner
- **Storrelsesaenderbart vindue** -- storrelse bevares mellem sessioner
- **Portabel** -- en enkelt `.exe`, ingen installation nodvendig

### Filosofi

> **Ingen har perfektioneret Crimson Deserts kamera endnu -- og det er netop pointen.**
>
> Vanilla-spillet har over 150 kameratilstande, hver med snesevis af parametre. Ingen enkelt udvikler kan finjustere alt det for enhver spillestil og skaerm. Det er derfor UCM eksisterer -- ikke for at fortaelle dig hvad det perfekte kamera er, men for at give dig vaerktoojerne til at finde det selv og dele det med andre.
>
> Enhver indstilling du justerer kan eksporteres og deles. Lock-on Auto-Rotate-fiksen der eliminerede kamerasnapping under kamp blev opdaget af et enkelt faellesskabsmedlem der eksperimenterede i God Mode. Den slags faellesskabsdrevet finjustering er praecis hvad dette vaerktoj er beregnet til.

### Presetdeling

Eksporter dit kameraopsaetning som en `.ucmpreset`-fil og del den med andre. Importer presets fra faellesskabskataloget, Nexus Mods eller andre spillere. UCM eksporterer ogsaa til JSON (til [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) og [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), raa XML og direkte PAZ-installation.

---

## Hvordan det virker

1. Finder spillets PAZ-arkiv der indeholder `playercamerapreset.xml`
2. Opretter en sikkerhedskopi af originalfilen (kun en gang -- overskriver aldrig en ren sikkerhedskopi)
3. Dekrypterer arkivposten (ChaCha20 + Jenkins hash-nogleafledning)
4. Dekomprimerer via LZ4
5. Analyserer og aendrer XML-kameraparametrene baseret paa dine valg
6. Genkomprimerer, genkrypterer og skriver den aendrede post tilbage til arkivet

Ingen DLL-injektion, ingen hukommelsesmanipulering, ingen internetforbindelse nodvendig -- ren datafilmodificering.

---

## Bygge fra kildekode

Kraever [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (eller nyere). Windows x64.

### v3 (anbefalet)

Luk eventuelle korende instanser for du bygger -- exe-kopieringstrinnet fejler hvis filen er laast.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Afhaengigheder (NuGet -- gendannes automatisk)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) -- LZ4 blok-komprimering/dekomprimering

---

## Projektstruktur

```
src/UltimateCameraMod/              Delt bibliotek + v2.x WPF-app
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, datamodeller
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x-interface
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 eksport-forst WPF-app (refererer delt kode ovenfor)
├── Controls/                       CameraPreview, FovPreview (v3-varianter)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Indlejrede faellesskabspresets distribueret ved forste opstart
├── MainWindow.xaml                 Topanelskal: sidepanel + faneredigerer
├── ExportJsonDialog.xaml           Eksportguide til flere formater (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Importer fra .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Presetmetadata-input (navn, forfatter, beskrivelse, URL)
├── CommunityBrowserDialog.xaml     Gennemse og download faellesskabspresets fra GitHub
├── NewPresetDialog.xaml            Opret / navngiv nye presets
├── ShellTaskbarPropertyStore.cs    Windows proceslinje-ikon via shell property store
├── ApplicationIdentity.cs          Delt App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  Faellesskabets bidragede kamerapresets
ucm_presets/                        Officielle UCM-stilpresetdefinitioner
```

---

## Kompatibilitet

- **Platforme:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Skaerm:** Alle billedformater -- 16:9, 21:9, 32:9

---

## Ofte stillede sporgsmal

**Kan jeg blive udelukket for dette?**
UCM aendrer kun offline-datafiler. Den rorer ikke spilhukommelsen, injicerer ingen kode og interagerer ikke med korende processer. Brug efter eget skon i online-/multiplayer-tilstande.

**Spillet blev opdateret og mit kamera er tilbage til vanilla.**
Normalt -- spilopdateringer overskriver moddede filer. Aabn UCM igen og klik paa Installer (eller geneksporter JSON til JSON Mod Manager / CDUMM). Dine indstillinger gemmes automatisk.

**Mit antivirusprogram markerede exe-filen.**
Kendt falsk positiv med selvstaendige .NET-apps. VirusTotal-scanningen er ren: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). Fuld kildekode er tilgaengelig her til gennemgang og egen kompilering.

**Hvad betyder horisontal forskydning 0?**
0 = vanilla kameraposition (karakteren lidt til venstre). 0,5 = karakteren centreret paa skaermen. Negative vaerdier flytter laengere til venstre, positive vaerdier flytter laengere til hojre.

**Opgraderer fra en tidligere version?**
v3.x-brugere: udskift bare exe-filen, alle presets og indstillinger bevares. v2.x-brugere: slet den gamle UCM-mappe, verificer spilfiler paa Steam, og kor derefter v3.1 fra en ny mappe. Se [udgivelsesnoterne](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) for detaljerede instruktioner.

---

## Versionshistorik

- **v3.1.2** -- Rettelse for sacred-vaerdier der manglede fra Install/eksporter paa God Mode-fanen. Se [udgivelsesnoter](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2).
- **v3.1.1** -- Rettelse for falsk-positiv detektering af beskadiget sikkerhedskopi paa rene spilfiler.
- **v3.1** -- Sacred God Mode-tilsidesaettelser (brugerredigeringer permanent beskyttet mod genopbygninger). Lock-on Auto-Rotate-skift (tak til [sillib1980](https://github.com/sillib1980)). Gronne sacred-indikatorer. Rettelse for Full Manual Control-installation. Versionsbevidst opgraderingsoverlay. Se [udgivelsesnoter](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** -- Alle dialoger konverteret til overlay-system i appen. God Mode-tilsidesaettelser bevares paa tvaers af faneskift. Valg af presettype (UCM Managed vs Full Manual Control). Faellesskabets presetkatalog flyttet til hovedrepoet. 54 God Mode-attribut-tooltips. Spilnedbrudsfixes. Vanilla-validering opdateret til spilpatchen juni 2026. 21-siders Wiki.
- **v3.0.1** -- Eksport-forst-redesign. Tre-trins editor (UCM Quick / Fine Tune / God Mode). `.ucmpreset`-filformat. Filbaseret presetsystem. UCM- og faellesskabspresetkataloger. Eksport i flere formater. Steadycam udvidet til 30+ kameratilstande. Lock-on-zoom-skyder.
- **v2.5** -- Sidste v2.x-udgivelse.
- **v2.4** -- Proportional horisontal forskydning, forskydning paa alle ridedyr og sigtefaerdigheder, hestens kameraomarbejdning, versionsbevidste sikkerhedskopier, FoV-forhaandsvisning, stoorrelsesaenderbart vindue.
- **v2.3** -- Horisontal forskydningsrettelse for 16:9, deltabaseret skyder, fuldstaendigt installationskonfigurationsbanner.
- **v2.2** -- Steadycam, ekstra zoomniveauer, hestens forstepersonstilstand, horisontal forskydning, universelt FoV, sigtekonsistens for faerdigheder, XML-import, presetdeling, opdateringsbeskeder.
- **v2.1** -- Rettelse for brugerdefinerede presetskydere der ikke skrev til alle zoomniveauer.
- **v2.0** -- Fuldstaendig omskrivning fra Python til C# / .NET 6 / WPF. Avanceret XML-editor, presethondtering, automatisk spildetektering.
- **v1.5** -- Python-version med customtkinter-GUI.

---

## Kreditering og tak

- **0xFitz** -- UCM-udvikling, kamerajustering, avanceret editor
- **[@sillib1980](https://github.com/sillib1980)** -- Opdagede Lock-on Auto-Rotate-kamerafelter

### C#-omskrivning (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** -- CrimsonDesertTools -- C# PAZ/PAMT-parser, ChaCha20-kryptering, LZ4-komprimering, PaChecksum, arkivompakker (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** -- Ren C# ChaCha20-stromchifferimplementering (BSD)
- **[MrIkso paa Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** -- PAZ-ompakningsguide: 16-byte-justering, PAMT-tjeksum, PAPGT-rodindekspatching

### Oprindelig Python-version (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** -- crimson-desert-unpacker -- PAZ-arkivvaerktoj, dekrypteringsforskning
- **Maszradine** -- CDCamera -- Kameraregler, steadycam-system, stilpresets
- **manymanecki** -- CrimsonCamera -- Dynamisk PAZ-modificeringsarkitektur

## Stotte

Hvis du finder dette nyttigt, overveej at stotte udviklingen:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Licens

MIT
