[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | **Norsk** | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.2 er her!** Sacred God Mode-overstyringer, Lock-on Auto-Rotate-bryter og alle feilrettinger. Last ned fra **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** eller **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Frittstaaende kameraverktoy for Crimson Desert. Fullt GUI, kameraforhondsvisning i sanntid, tre redigeringsnivaaer, filbaserte presets, **JSON-eksport for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** og **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), og ultrawide-HUD-stotte.

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

> Trenger du hjelp? Sjekk **[wikien](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** for oppsettguider, forklaring av kamerainnstillinger, presethondtering, feilsoking og utviklerdokumentasjon.

---

<details>
<summary><strong>Skjermbilder (v3.x)</strong> -- klikk for aa utvide</summary>
<br>

**UCM Quick** -- avstand, hoyde, forskyvning, FoV, lock-on-zoom, steadycam, forhondsvisning i sanntid
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- kuratert dypjustering med sokbare innrammede kort
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** -- fullstendig XML-redigerer med vanilla-sammenligning
![God Mode](../../screenshots/v3.x/godmode.png)

**JSON-eksport** -- eksport for JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**Import** -- importer fra .ucmpreset, XML, PAZ eller Mod Manager-pakker
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## Grenoversikt

| Gren | Status | Hva det er |
|------|--------|------------|
| **`main`** | v3.2 Release | Frittstaaende kameraverktoy med tre-trinns redigerer (UCM Quick / Fine Tune / God Mode), filbaserte presets, fellesskapskatalog, eksport i flere formater og direkte PAZ-installasjon |
| **`development`** | Utvikling | Utviklingsgren for neste versjon |

v3 inkluderer alle kamerafunksjoner fra v2 pluss et redesignet grensesnitt, filbaserte presets, en tre-trinns redigerer og eksport i flere formater. Direkte PAZ-installasjon er fortsatt tilgjengelig i v3 som et sekundaert alternativ.

---

## Funksjoner

### Kamerakontroller

| Funksjon | Detaljer |
|----------|---------|
| **8 innebygde presets** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival -- med forhondsvisning i sanntid |
| **Tilpasset kamera** | Glidere for avstand (1,5-12), hoyde (-1,6-1,5) og horisontal forskyvning (-3-3). Proporsjonal skalering holder karakteren paa samme skjermposisjon ved alle zoomnivaaer |
| **Synsfelt** | Vanilla 40 grader opp til 80 grader. Universell FoV-konsistens paa tvers av guard, aim, mount, glide og cinematic-tilstander |
| **Sentrert kamera** | Karakteren sentrert paa tvers av 150+ kameratilstander, eliminerer den venstreforskjovne skulder-kameraet |
| **Lock-on-zoom** | Glider fra -60% (zoom inn paa maalet) til +60% (trekk ut bredere). Pavirker alle lock-on-, guard- og rush-tilstander. Fungerer uavhengig av Steadycam |
| **Lock-on Auto-Rotate** | Deaktiver kamerasnapping mot maalet ved lock-on. Forhindrer at kameraet snurrer rundt for aa fokusere fiender bak deg. Takk til [@sillib1980](https://github.com/sillib1980) |
| **Ridedyr-kamerasynk** | Ridedyr-kameraer matcher din valgte spillerkamerahoyde |
| **Horisontal forskyvning paa alle ridedyr** | Hest, elefant, wyvern, kano, krigsmaskin og kost respekterer alle din forskyvningsinnstilling med proporsjonal skalering |
| **Siktekonsistens for ferdigheter** | Lantern, Blinding Flash, Bow og alle siktings-/zoom-/interaksjonsferdigheter respekterer horisontal forskyvning. Ingen kamerasnapping naar du aktiverer ferdigheter |
| **Steadycam-utjevning** | Normaliserte blandingstider og hastighetssving paa tvers av 30+ kameratilstander: idle, walk, run, sprint, combat, guard, rush/charge, freefall, super jump, rope pull/swing, knockback, alle lock-on-varianter, mount lock-on, revive lock-on, aggro/wanted, warmachine og alle mount-tilstander. Hver verdi kan justeres av fellesskapet via Fine Tune-redigereren |
| **Sacred God Mode** | Verdier du redigerer i God Mode er permanent beskyttet fra UCM Quick/Fine Tune-gjenoppbygginger. Gronne indikatorer viser hvilke verdier som er sacred. Lagring per preset |

> **v3-designfilosofi: kun verdiredigeringer, ingen strukturell injeksjon.**
>
> Tidligere versjoner injiserte nye XML-linjer i kamerafilen (ekstra zoomnivaaer, hestens forstepersons-modus, hestens kameraomarbeiding med ytterligere zoomnivaaer). v3 fjerner disse funksjonene med vilje. Aa injisere struktur har mye hoyere risiko for aa gaa i stykker etter spilloppdateringer, og personlige preferanser for nisjekameramodi betjenes bedre av dedikerte modder distribuert via modhontterere. UCM endrer naa bare eksisterende verdier -- samme antall linjer, samme elementstruktur, samme attributter. Dette gjor presets tryggere aa dele og mer motstandsdyktige mot spillpatcher.

### Tre-trinns redigerer (v3)

v3 organiserer redigering i tre faner saa du kan gaa saa dypt du vil:

| Nivaa | Fane | Hva den gjor |
|-------|------|--------------|
| 1 | **UCM Quick** | Det raske laget -- glidere for avstand/hoyde/forskyvning, FoV, sentrert kamera, lock-on-zoom (-60% til +60%), lock-on Auto-Rotate, ridedyrsynk, steadycam, forhondsvisning av kamera og FoV i sanntid |
| 2 | **Fine Tune** | Kuratert dypjustering. Sokbare seksjoner for fotgjenger-zoomnivaaer, hest-/ridedyrzoom, globalt FoV, spesialridedyr og forflytning, kamp og lock-on, kamerautjevning og sikting og traadkorsposisjon. Bygger oppaa UCM Quick |
| 3 | **God Mode** | Fullstendig XML-redigerer -- hver parameter i et sokbart, filtrerbart DataGrid gruppert etter kameratilstand. Sammenligningskolonne mot vanilla. Sacred-overstyringer (gronne) beskyttet fra gjenoppbygginger. "Sacred only"-filter. 54 attributt-tooltips |

### Filbasert presetsystem (v3)

- **`.ucmpreset`-filformat** -- dedikert delbart format for UCM-kamerapresets. Slipp inn i hvilken som helst presetmappe og det fungerer med en gang
- **Sidepanelhondterer** med sammenleggbare grupperte seksjoner: Game Default, UCM Presets, Community Presets, My Presets, Imported
- **Ny / Dupliser / Gi nytt navn / Slett** fra sidepanelet
- **Laas** presets for aa forhindre utilsiktede endringer -- UCM-presets er permanent laast; brukerpresets kan veksles via henglaas-ikonet
- **Ekte Vanilla-preset** -- raa dekodet `playercamerapreset` fra spillbackupen din uten noen endringer. Hurtigglidere er synkronisert med spillets faktiske grunnlinjenverdier
- **Importer** fra `.ucmpreset`, raa XML, PAZ-arkiver eller Mod Manager-pakker. `.ucmpreset`-importer faar full UCM-gliderkontroll; raa XML-/PAZ-/Mod Manager-importer er frittstaaende presets (kun God Mode-redigering, ingen UCM-regler brukt) for aa bevare den opprinnelige modforfatterens verdier
- **Autolagring** -- endringer i ulaaste presets skrives automatisk tilbake til presetfilen (forsinket)
- Automatisk migrering fra eldre `.json`-presets til `.ucmpreset` ved forste oppstart

### Presetkataloger (v3)

Bla gjennom og last ned presets direkte fra UCM. Ett-klikks nedlasting, ingen kontoer nodvendig.

- **UCM Presets** -- 7 offisielle kamerastiler (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definisjoner lagret paa GitHub, sesjons-XML bygges lokalt fra spillfilene dine + gjeldende kameraregler. Gjenoppbygges automatisk naar kameraregler oppdateres
- **[Fellesskapspresets](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- fellesskapets bidragspresets i hovedrepoet, katalog autogenerert av GitHub Actions
- **Bla-knapp** paa hvert sidepanelgruppehode aapner katalogleseren
- Hvert preset viser navn, forfatter, beskrivelse, tagger og en lenke til skaperens Nexus-side
- **Oppdateringsdeteksjon** -- pulserende oppdateringsikon naar en nyere versjon er tilgjengelig i katalogen. Klikk for aa laste ned oppdateringen med valgfri sikkerhetskopiering til My Presets
- Nedlastede presets vises i sidepanelet (laast som standard -- dupliser for aa redigere)
- **2 MB filstorrelsesgrense** og JSON-validering for sikkerhet

**Vil du dele presetet ditt med fellesskapet?** Eksporter som `.ucmpreset` fra UCM, deretter enten:
- Send en [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) og legg til presetet ditt i `community_presets/`-mappen
- Eller send `.ucmpreset`-filen din til 0xFitz paa Discord/Nexus saa legger vi den til for deg

### Eksport i flere formater (v3)

Dialogen **Eksporter for deling** gir ut sesjonen din paa fire maater:

| Format | Bruksomraade |
|--------|--------------|
| **JSON** (modhondterere) | Byte-patcher + `modinfo` for **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) eller **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Eksporter i UCM, importer i hondtereren du bruker; mottakere trenger ikke UCM. **Prepare** tilbys bare naar den aktive `playercamerapreset`-oppforingen fortsatt matcher UCM sin vanilla-backup (verifiser spillfiler hvis du allerede har brukt kameramodder). |
| **XML** | Raa `playercamerapreset.xml` for andre verktoy eller manuell redigering |
| **0.paz** | Patchet arkiv klart til aa slippes i spillets `0010`-mappe |
| **.ucmpreset** | Fullt UCM-preset for andre UCM-brukere |

Inkluderer tittel, versjon, forfatter, Nexus-URL og beskrivelsesfelt for JSON/XML. Viser antall patchregioner og endrede bytes for lagring av `.json`.

### Livskvalitetsfunksjoner

- **Automatisk spilldetektering** -- Steam, Epic Games, Xbox / Game Pass
- **Automatisk sikkerhetskopi** -- vanilla-backup for eventuelle endringer; gjenoppretting med ett klikk. Versjonsbevisst med automatisk opprydding ved oppgradering
- **Installasjonskonfigurasjonsbanner** -- viser din fullstendige aktive konfigurasjon (FoV, avstand, hoyde, forskyvning, innstillinger)
- **Spilloppdateringsbevissthet** -- lagrer installasjonsmetadata etter bruk; advarer naar spillet kan ha blitt oppdatert saa du kan eksportere paa nytt
- **Kamera- og FoV-forhondsvisning i sanntid** -- avstandsbevisst ovenfra-visning med horisontal forskyvning og synsfeltskjegle
- **Oppdateringsvarsler** -- sjekker GitHub-utgivelser ved oppstart
- **Spillmappesnarvei** -- aapner spillkatalogen din fra toppteksten
- **Windows oppgavelinje-identitet** -- korrekt ikongruppering og tittellinje-ikon via shell property store
- **Innstillingsbevaring** -- alle valg huskes mellom sesjoner
- **Justerbart vindu** -- storrelse bevares mellom sesjoner
- **Portabel** -- en enkelt `.exe`, ingen installasjon nodvendig

### Filosofi

> **Ingen har perfeksjonert Crimson Deserts kamera ennaa -- og det er nettopp poenget.**
>
> Vanilla-spillet har over 150 kameratilstander, hver med dusinvis av parametere. Ingen enkelt utvikler kan finjustere alt dette for enhver spillestil og skjerm. Det er derfor UCM eksisterer -- ikke for aa fortelle deg hva det perfekte kameraet er, men for aa gi deg verktoyene til aa finne det selv og dele det med andre.
>
> Hver innstilling du justerer kan eksporteres og deles. Lock-on Auto-Rotate-fiksen som eliminerte kamerasnapping under kamp ble oppdaget av et enkelt fellesskapsmedlem som eksperimenterte i God Mode. Den typen fellesskapsdrevet finjustering er akkurat det dette verktooyet er laget for.

### Presetdeling

Eksporter kameraoppsettet ditt som en `.ucmpreset`-fil og del den med andre. Importer presets fra fellesskapskatalogen, Nexus Mods eller andre spillere. UCM eksporterer ogsaa til JSON (for [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) og [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), raa XML og direkte PAZ-installasjon.

---

## Hvordan det fungerer

1. Finner spillets PAZ-arkiv som inneholder `playercamerapreset.xml`
2. Oppretter en sikkerhetskopi av originalfilen (bare en gang -- overskriver aldri en ren sikkerhetskopi)
3. Dekrypterer arkivoppforingen (ChaCha20 + Jenkins hash-nokkelavledning)
4. Dekomprimerer via LZ4
5. Analyserer og endrer XML-kameraparametrene basert paa dine valg
6. Rekomprimerer, rekrypterer og skriver den endrede oppforingen tilbake til arkivet

Ingen DLL-injeksjon, ingen minnesmanipulering, ingen internettforbindelse nodvendig -- ren datafilmodifisering.

---

## Bygge fra kildekode

Krever [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (eller nyere). Windows x64.

### v3 (anbefalt)

Lukk eventuelle kjorende instanser for du bygger -- exe-kopieringstrinnet feiler hvis filen er laast.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Avhengigheter (NuGet -- gjenopprettes automatisk)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) -- LZ4 blokk-komprimering/dekomprimering

---

## Prosjektstruktur

```
src/UltimateCameraMod/              Delt bibliotek + v2.x WPF-app
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, datamodeller
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x-grensesnitt
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 eksport-forst WPF-app (refererer delt kode ovenfor)
├── Controls/                       CameraPreview, FovPreview (v3-varianter)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Innebygde fellesskapspresets distribuert ved forste oppstart
├── MainWindow.xaml                 Topanelsskall: sidepanel + faneredigerer
├── ExportJsonDialog.xaml           Eksportveiviser for flere formater (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Importer fra .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Presetmetadata-inndata (navn, forfatter, beskrivelse, URL)
├── CommunityBrowserDialog.xaml     Bla gjennom og last ned fellesskapspresets fra GitHub
├── NewPresetDialog.xaml            Opprett / navngi nye presets
├── ShellTaskbarPropertyStore.cs    Windows oppgavelinje-ikon via shell property store
├── ApplicationIdentity.cs          Delt App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  Fellesskapets bidragsbaserte kamerapresets
ucm_presets/                        Offisielle UCM-stilpresetdefinisjoner
```

---

## Kompatibilitet

- **Plattformer:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **Skjerm:** Alle sideforhold -- 16:9, 21:9, 32:9

---

## Vanlige sporsmaal

**Kan jeg bli utestengt for dette?**
UCM endrer kun offline-datafiler. Den rorer ikke spillminnet, injiserer ingen kode og samhandler ikke med kjorende prosesser. Bruk etter eget skjonn i nettbaserte/flerspillermodi.

**Spillet ble oppdatert og kameraet mitt er tilbake til vanilla.**
Normalt -- spilloppdateringer overskriver moddede filer. Aapne UCM igjen og klikk paa Installer (eller eksporter JSON paa nytt for JSON Mod Manager / CDUMM). Innstillingene dine lagres automatisk.

**Antivirusprogrammet mitt flagget exe-filen.**
Kjent falsk positiv med frittstaaende .NET-apper. VirusTotal-skanningen er ren: [v3.2](https://www.virustotal.com/gui/file-analysis/ZWMzZGM4MGM3ZWFlZTY5MTFmZDYwYzNkODFlZGM4Mjg6MTc3NTkxMzY4Mg==). Fullstendig kildekode er tilgjengelig her for gjennomgang og egen kompilering.

**Hva betyr horisontal forskyvning 0?**
0 = vanilla kameraposisjon (karakteren litt til venstre). 0,5 = karakteren sentrert paa skjermen. Negative verdier flytter lenger til venstre, positive verdier flytter lenger til hoyre.

**Oppgraderer fra en tidligere versjon?**
v3.x-brukere: bare bytt ut exe-filen, alle presets og innstillinger bevares. v2.x-brukere: slett den gamle UCM-mappen, verifiser spillfiler paa Steam, og kjor deretter v3.1 fra en ny mappe. Se [utgivelsesnotatene](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) for detaljerte instruksjoner.

---

## Versjonshistorikk

- **v3.2** -- Fiks for sacred-verdier som manglet fra Install/eksporter paa God Mode-fanen. Se [utgivelsesnotater](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.2).
- **v3.1.1** -- Fiks for falsk-positiv deteksjon av skadet sikkerhetskopi paa rene spillfiler.
- **v3.1** -- Sacred God Mode-overstyringer (brukerredigeringer permanent beskyttet fra gjenoppbygginger). Lock-on Auto-Rotate-bryter (takk til [sillib1980](https://github.com/sillib1980)). Gronne sacred-indikatorer. Fiks for Full Manual Control-installasjon. Versjonsbevisst oppgraderingsoverlegg. Se [utgivelsesnotater](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** -- Alle dialoger konvertert til overleggssystem i appen. God Mode-overstyringer bevares paa tvers av fanebytter. Valg av presettype (UCM Managed vs Full Manual Control). Fellesskapets presetkatalog flyttet til hovedrepoet. 54 God Mode-attributt-tooltips. Spillkrasjfikser. Vanilla-validering oppdatert for spillpatchen juni 2026. 21-siders Wiki.
- **v3.0.1** -- Eksport-forst-redesign. Tre-trinns redigerer (UCM Quick / Fine Tune / God Mode). `.ucmpreset`-filformat. Filbasert presetsystem. UCM- og fellesskapspresetkataloger. Eksport i flere formater. Steadycam utvidet til 30+ kameratilstander. Lock-on-zoom-glider.
- **v2.5** -- Siste v2.x-utgivelse.
- **v2.4** -- Proporsjonal horisontal forskyvning, forskyvning paa alle ridedyr og sikteferdigheter, hestens kameraomarbeiding, versjonsbevisste sikkerhetskopier, FoV-forhondsvisning, justerbart vindu.
- **v2.3** -- Horisontal forskyvningsfiks for 16:9, deltabasert glider, fullstendig installasjonskonfigurasjonsbanner.
- **v2.2** -- Steadycam, ekstra zoomnivaaer, hestens forstepersonsmodus, horisontal forskyvning, universelt FoV, siktekonsistens for ferdigheter, XML-import, presetdeling, oppdateringsvarsler.
- **v2.1** -- Fiks for tilpassede presetglidere som ikke skrev til alle zoomnivaaer.
- **v2.0** -- Fullstendig omskriving fra Python til C# / .NET 6 / WPF. Avansert XML-redigerer, presethondtering, automatisk spilldetektering.
- **v1.5** -- Python-versjon med customtkinter-GUI.

---

## Kreditering og takk

- **0xFitz** -- UCM-utvikling, kamerajustering, avansert redigerer
- **[@sillib1980](https://github.com/sillib1980)** -- Oppdaget Lock-on Auto-Rotate-kamerafelt

### C#-omskriving (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** -- CrimsonDesertTools -- C# PAZ/PAMT-parser, ChaCha20-kryptering, LZ4-komprimering, PaChecksum, arkivompakker (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** -- Ren C# ChaCha20-stromchifferimplementering (BSD)
- **[MrIkso paa Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** -- PAZ-ompakkingsguide: 16-byte-justering, PAMT-sjekksum, PAPGT-rotindekspatching

### Opprinnelig Python-versjon (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** -- crimson-desert-unpacker -- PAZ-arkivverktoy, dekrypteringsforskning
- **Maszradine** -- CDCamera -- Kameraregler, steadycam-system, stilpresets
- **manymanecki** -- CrimsonCamera -- Dynamisk PAZ-modifikasjonsarkitektur

## Stotte

Hvis du synes dette er nyttig, vurder aa stotte utviklingen:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Lisens

MIT
