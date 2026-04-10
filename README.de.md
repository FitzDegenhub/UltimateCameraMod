[English](README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | **Deutsch** | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 ist da!** Sacred God Mode-Uberschreibungen, Lock-on Auto-Rotate-Schalter und alle Fehlerbehebungen. Download uber **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** oder **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Eigenstandiges Kamera-Toolkit fur Crimson Desert. Vollstandige grafische Oberflache, Live-Kameravorschau, drei Bearbeitungsstufen, dateibasiertes Preset-System, **JSON-Export fur [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** und **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) sowie Ultrawide-HUD-Unterstutzung.

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert Banner" width="100%" />
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

> Brauchst du Hilfe? Schau im **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** nach -- dort findest du Einrichtungsanleitungen, Erklarungen der Kameraeinstellungen, Preset-Verwaltung, Fehlerbehebung und Entwicklerdokumentation.

---

<details>
<summary><strong>Screenshots (v3.x)</strong> -- zum Ausklappen anklicken</summary>
<br>

**UCM Quick** -- Entfernung, Hohe, Verschiebung, FoV, Lock-on-Zoom, Steadycam, Live-Vorschauen
![UCM Quick](screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- kuratierte Feinabstimmung mit durchsuchbaren umrandeten Karten
![Fine Tune](screenshots/v3.x/finetune.png)

**God Mode** -- vollstandiger Roh-XML-Editor mit Vergleich zu Originalwerten
![God Mode](screenshots/v3.x/godmode.png)

**JSON-Export** -- Export fur JSON Mod Manager / CDUMM
![Export JSON](screenshots/v3.x/exportjson_menu.png)

**Import** -- Import aus .ucmpreset, XML, PAZ oder Mod-Manager-Paketen
![Import](screenshots/v3.x/import_screen.png)

</details>

---

## Branch-Ubersicht

| Branch | Status | Beschreibung |
|--------|--------|--------------|
| **`main`** | v3.1.2 Release | Eigenstandiges Kamera-Toolkit mit dreistufigem Editor (UCM Quick / Fine Tune / God Mode), dateibasierte Presets, Community-Katalog, Multi-Format-Export und direkter PAZ-Installation |
| **`development`** | Entwicklung | Entwicklungsbranch fur die nachste Version |

v3 enthalt alle Kamerafunktionen von v2 sowie eine uberarbeitete Oberflache, dateibasierte Presets, einen dreistufigen Editor und Multi-Format-Export. Die direkte PAZ-Installation ist in v3 weiterhin als sekundare Option verfugbar.

---

## Funktionen

### Kamerasteuerung

| Funktion | Details |
|----------|---------|
| **8 integrierte Presets** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival -- mit Live-Vorschau |
| **Benutzerdefinierte Kamera** | Schieberegler fur Entfernung (1.5-12), Hohe (-1.6-1.5) und horizontale Verschiebung (-3-3). Proportionale Skalierung halt den Charakter an derselben Bildschirmposition uber alle Zoomstufen |
| **Sichtfeld** | Standard 40° bis zu 80°. Einheitliches FoV in Schutz-, Ziel-, Reit-, Gleit- und Filmzustanden |
| **Zentrierte Kamera** | Charakter mittig uber 150+ Kamerazustande, eliminiert die nach links versetzte Schulterkamera |
| **Lock-on-Zoom** | Schieberegler von -60 % (auf Ziel heranzoomen) bis +60 % (weiter herausziehen). Beeinflusst alle Lock-on-, Schutz- und Ansturmzustande. Funktioniert unabhangig von Steadycam |
| **Lock-on Auto-Rotate** | Deaktiviert das Einrasten der Kamera auf das Ziel beim Fixieren. Verhindert, dass die Kamera herumwirbelt, um Feinde hinter dir anzuvisieren. Dank an [@sillib1980](https://github.com/sillib1980) |
| **Reitkamera-Synchronisation** | Reitkameras passen sich der gewahlten Spielerkamerahohe an |
| **Horizontale Verschiebung auf allen Reittieren** | Pferd, Elefant, Wyvern, Kanu, Kriegsmaschine und Besen respektieren deine Verschiebungseinstellung mit proportionaler Skalierung |
| **Fahigkeiten-Zielkonsistenz** | Laterne, Blinding Flash, Bogen und alle Ziel-/Zoom-/Interaktionsfahigkeiten respektieren die horizontale Verschiebung. Kein Kamerasprung beim Aktivieren von Fahigkeiten |
| **Steadycam-Glattung** | Normalisiertes Uberblendungstiming und Geschwindigkeitsschwankung uber 30+ Kamerazustande: Ruhe, Gehen, Laufen, Sprinten, Kampf, Schutz, Ansturm/Angriff, freier Fall, Supersprung, Seilzug/-schwung, Ruckstoss, alle Lock-on-Varianten, Reittier-Lock-on, Wiederbelebungs-Lock-on, Aggro/Gesucht, Kriegsmaschine und alle Reittierstande. Jeder Wert ist von der Community uber den Fine Tune-Editor anpassbar |
| **Sacred God Mode** | Werte, die du im God Mode bearbeitest, sind dauerhaft vor Quick/Fine Tune-Neuaufbauten geschutzt. Grune Indikatoren zeigen, welche Werte geschutzt sind. Preset-individuelle Speicherung |

> **v3 Designphilosophie: Nur Wertanderungen, keine strukturelle Injektion.**
>
> Fruhere Versionen fugten neue XML-Zeilen in die Kameradatei ein (zusatzliche Zoomstufen, Pferd-Ego-Modus, Pferdekamera-Uberarbeitung mit zusatzlichen Zoomstufen). v3 entfernt diese Funktionen absichtlich. Strukturinjektion hat ein deutlich hoheres Risiko, nach Spielupdates nicht mehr zu funktionieren, und personliche Vorlieben fur Nischen-Kameramodi werden besser durch spezielle Mods bedient, die uber Mod-Manager verteilt werden. UCM andert jetzt nur noch bestehende Werte -- gleiche Zeilenanzahl, gleiche Elementstruktur, gleiche Attribute. Das macht Presets sicherer zu teilen und widerstandsfahiger gegenuber Spielpatches.

### Dreistufiger Editor (v3)

v3 organisiert die Bearbeitung in drei Tabs, sodass du so tief einsteigen kannst, wie du mochtest:

| Stufe | Tab | Was es tut |
|-------|-----|------------|
| 1 | **UCM Quick** | Die schnelle Ebene -- Entfernungs-/Hohen-/Verschiebungsregler, FoV, zentrierte Kamera, Lock-on-Zoom (-60 % bis +60 %), Lock-on Auto-Rotate, Reitkamera-Sync, Steadycam, Live-Kamera- und FoV-Vorschauen |
| 2 | **Fine Tune** | Kuratierte Feinabstimmung. Durchsuchbare Abschnitte fur Fussganger-Zoomstufen, Pferd/Reittier-Zoom, globales FoV, Spezialreittiere und Fortbewegung, Kampf und Lock-on, Kameraglattung sowie Zielen und Fadenkreuzposition. Baut auf UCM Quick auf |
| 3 | **God Mode** | Vollstandiger Roh-XML-Editor -- jeder Parameter in einem durchsuchbaren, filterbaren DataGrid, gruppiert nach Kamerazustand. Vergleichsspalte mit Originalwerten. Sacred-Uberschreibungen (grun) vor Neuaufbauten geschutzt. "Nur Sacred"-Filter. 54 Attribut-Tooltips |

### Dateibasiertes Preset-System (v3)

- **`.ucmpreset`-Dateiformat** -- dediziertes teilbares Format fur UCM-Kamerapresets. Einfach in einen Preset-Ordner legen und es funktioniert sofort
- **Seitenleisten-Manager** mit einklappbaren gruppierten Abschnitten: Spielstandard, UCM-Presets, Community-Presets, Meine Presets, Importiert
- **Neu / Duplizieren / Umbenennen / Loschen** aus der Seitenleiste
- **Presets sperren** um versehentliche Anderungen zu verhindern -- UCM-Presets sind dauerhaft gesperrt; Benutzerpresets umschaltbar uber das Schlosssymbol
- **Echtes Vanilla-Preset** -- roh dekodiertes `playercamerapreset` aus deinem Spiel-Backup ohne Anderungen. Quick-Schieberegler sind auf die tatsachlichen Spiel-Basiswerte synchronisiert
- **Import** aus `.ucmpreset`, rohem XML, PAZ-Archiven oder Mod-Manager-Paketen. `.ucmpreset`-Importe erhalten volle UCM-Schiebereglersteuerung; rohe XML-/PAZ-/Mod-Manager-Importe sind eigenstandige Presets (nur God Mode-Bearbeitung, keine UCM-Regeln angewendet), um die Werte des ursprunglichen Mod-Autors zu bewahren
- **Automatisches Speichern** -- Anderungen an entsperrten Presets werden automatisch in die Preset-Datei zuruckgeschrieben (verzogert)
- Automatische Migration von alten `.json`-Presets zu `.ucmpreset` beim ersten Start

### Preset-Kataloge (v3)

Durchsuche und lade Presets direkt aus UCM. Ein-Klick-Download, keine Konten erforderlich.

- **UCM-Presets** -- 7 offizielle Kamerastile (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definitionen auf GitHub gehostet, Sitzungs-XML wird lokal aus deinen Spieldateien und aktuellen Kameraregeln erstellt. Automatischer Neuaufbau bei Aktualisierung der Kameraregeln
- **[Community-Presets](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- von der Community beigetragene Presets im Hauptrepository, Katalog wird automatisch von GitHub Actions generiert
- **Durchsuchen-Button** an jeder Seitenleisten-Gruppenuberschrift offnet den Katalog-Browser
- Jedes Preset zeigt Name, Autor, Beschreibung, Tags und einen Link zur Nexus-Seite des Erstellers
- **Update-Erkennung** -- pulsierendes Update-Symbol, wenn eine neuere Version im Katalog verfugbar ist. Klicken zum Herunterladen des Updates mit optionalem Backup in Meine Presets
- Heruntergeladene Presets erscheinen in der Seitenleiste (standardmassig gesperrt -- duplizieren zum Bearbeiten)
- **2 MB Dateigrossen-Limit** und JSON-Validierung fur die Sicherheit

**Mochtest du dein Preset mit der Community teilen?** Exportiere als `.ucmpreset` aus UCM und dann entweder:
- Sende einen [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls), der dein Preset zum `community_presets/`-Ordner hinzufugt
- Oder sende deine `.ucmpreset`-Datei an 0xFitz auf Discord/Nexus und wir fugen es fur dich hinzu

### Multi-Format-Export (v3)

Der **Fur Weitergabe exportieren**-Dialog gibt deine Sitzung in vier Formaten aus:

| Format | Verwendungszweck |
|--------|------------------|
| **JSON** (Mod-Manager) | Byte-Patches + `modinfo` fur **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) oder **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). In UCM exportieren, im verwendeten Manager importieren; Empfanger benotigen kein UCM. **Vorbereiten** wird nur angeboten, wenn der aktive `playercamerapreset`-Eintrag noch mit UCMs Vanilla-Backup ubereinstimmt (Spieldateien uberprufen, falls bereits Kameramods angewendet wurden). |
| **XML** | Rohe `playercamerapreset.xml` fur andere Werkzeuge oder manuelle Bearbeitung |
| **0.paz** | Gepatchtes Archiv, das direkt in den `0010`-Ordner des Spiels gelegt wird |
| **.ucmpreset** | Vollstandiges UCM-Preset fur andere UCM-Benutzer |

Beinhaltet Titel-, Versions-, Autor-, Nexus-URL- und Beschreibungsfelder fur JSON-/XML-Export. Zeigt Patch-Regionenzahl und geanderte Bytes vor dem Speichern der `.json` an.

### Komfortfunktionen

- **Automatische Spielerkennung** -- Steam, Epic Games, Xbox / Game Pass
- **Automatisches Backup** -- Vanilla-Backup vor jeder Anderung; Wiederherstellung mit einem Klick. Versionsfahig mit automatischer Bereinigung bei Upgrades
- **Installationskonfigurationsbanner** -- zeigt deine vollstandige aktive Konfiguration (FoV, Entfernung, Hohe, Verschiebung, Einstellungen)
- **Spielpatch-Erkennung** -- verfolgt Installationsmetadaten nach dem Anwenden; warnt wenn das Spiel moglicherweise aktualisiert wurde, damit du erneut exportieren kannst
- **Live-Kamera- und FoV-Vorschau** -- entfernungsbewusste Draufsicht mit horizontaler Verschiebung und Sichtfeldkegel
- **Update-Benachrichtigungen** -- pruft GitHub-Releases beim Start
- **Spielordner-Verknupfung** -- offnet dein Spielverzeichnis aus der Kopfzeile
- **Windows-Taskleistenidentitat** -- korrekte Symbol-Gruppierung und Titelleistensymbol uber Shell-Property-Store
- **Einstellungsspeicherung** -- alle Auswahlen werden zwischen Sitzungen gespeichert
- **Grossenveranderliches Fenster** -- Grosse bleibt zwischen Sitzungen erhalten
- **Portabel** -- einzelne `.exe`, kein Installationsprogramm erforderlich

### Philosophie

> **Niemand hat die Kamera von Crimson Desert bisher perfektioniert -- und genau darum geht es.**
>
> Das Originalspiel hat uber 150 Kamerazustande, jeder mit Dutzenden von Parametern. Kein einzelner Entwickler kann all das fur jeden Spielstil und jedes Display abstimmen. Deshalb existiert UCM -- nicht um dir zu sagen, was die perfekte Kamera ist, sondern um dir die Werkzeuge zu geben, sie selbst zu finden und mit anderen zu teilen.
>
> Jede Einstellung, die du anpasst, kann exportiert und geteilt werden. Die Lock-on Auto-Rotate-Korrektur, die das Kameraspringen im Kampf beseitigte, wurde von einem einzelnen Community-Mitglied entdeckt, das im God Mode experimentierte. Genau diese Art der community-getriebenen Feinabstimmung ist der Zweck dieses Werkzeugs.

### Preset-Weitergabe

Exportiere dein Kamera-Setup als `.ucmpreset`-Datei und teile es mit anderen. Importiere Presets aus dem Community-Katalog, von Nexus Mods oder von anderen Spielern. UCM exportiert auch nach JSON (fur [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) und [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), als rohes XML und als direkte PAZ-Installation.

---

## So funktioniert es

1. Findet das PAZ-Archiv des Spiels, das `playercamerapreset.xml` enthalt
2. Erstellt ein Backup der Originaldatei (nur einmal -- uberschreibt niemals ein sauberes Backup)
3. Entschlusselt den Archiveintrag (ChaCha20 + Jenkins-Hash-Schlusselableitung)
4. Dekomprimiert uber LZ4
5. Parst und modifiziert die XML-Kameraparameter basierend auf deinen Auswahlen
6. Rekomprimiert, verschlusselt erneut und schreibt den modifizierten Eintrag zuruck ins Archiv

Keine DLL-Injektion, kein Memory-Hacking, keine Internetverbindung erforderlich -- reine Datendatei-Modifikation.

---

## Aus Quellcode kompilieren

Erfordert [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (oder neuer). Windows x64.

### v3 (empfohlen)

Schliesse alle laufenden Instanzen vor dem Kompilieren -- der exe-Kopierschritt schlagt fehl, wenn die Datei gesperrt ist.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Abhangigkeiten (NuGet -- werden automatisch wiederhergestellt)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) -- LZ4-Blockkompression/-dekompression

---

## Projektstruktur

```
src/UltimateCameraMod/              Gemeinsame Bibliothek + v2.x WPF-App
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, Datenmodelle
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x Oberflache
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 Export-first WPF-App (referenziert gemeinsamen Code oben)
├── Controls/                       CameraPreview, FovPreview (v3-Varianten)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Eingebettete Community-Presets, die beim ersten Start bereitgestellt werden
├── MainWindow.xaml                 Zwei-Panel-Shell: Seitenleiste + Tab-Editor
├── ExportJsonDialog.xaml           Multi-Format-Export-Assistent (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Import aus .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Preset-Metadateneingabe (Name, Autor, Beschreibung, URL)
├── CommunityBrowserDialog.xaml     Community-Presets von GitHub durchsuchen und herunterladen
├── NewPresetDialog.xaml            Neue Presets erstellen / benennen
├── ShellTaskbarPropertyStore.cs    Windows-Taskleistensymbol uber Shell-Property-Store
├── ApplicationIdentity.cs          Gemeinsame App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  Von der Community beigetragene Kamerapresets
ucm_presets/                        Offizielle UCM-Stil-Preset-Definitionen
```

---

## Kompatibilitat

- **Plattformen:** Steam, Epic Games, Xbox / Game Pass
- **Betriebssystem:** Windows 10/11 (x64)
- **Display:** Jedes Seitenverhaltnis -- 16:9, 21:9, 32:9

---

## FAQ

**Kann ich dafur gebannt werden?**
UCM modifiziert nur Offline-Datendateien. Es greift nicht auf den Spielspeicher zu, injiziert keinen Code und interagiert nicht mit laufenden Prozessen. Verwende es nach eigenem Ermessen in Online-/Mehrspieler-Modi.

**Das Spiel hat aktualisiert und meine Kamera ist wieder auf Standard.**
Normal -- Spielupdates uberschreiben modifizierte Dateien. Offne UCM erneut und klicke auf Installieren (oder exportiere JSON erneut fur JSON Mod Manager / CDUMM). Deine Einstellungen werden automatisch gespeichert.

**Mein Antivirenprogramm hat die exe markiert.**
Bekannter Fehlalarm bei eigenstandigen .NET-Apps. Der VirusTotal-Scan ist sauber: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). Der vollstandige Quellcode ist hier verfugbar zum Uberprufen und Selbstkompilieren.

**Was bedeutet horizontale Verschiebung 0?**
0 = Standard-Kameraposition (Charakter leicht links). 0.5 = Charakter mittig auf dem Bildschirm. Negative Werte verschieben weiter nach links, positive Werte weiter nach rechts.

**Upgrade von einer fruheren Version?**
v3.x-Benutzer: Einfach die exe ersetzen, alle Presets und Einstellungen bleiben erhalten. v2.x-Benutzer: Alten UCM-Ordner loschen, Spieldateien auf Steam uberprufen, dann v3.1 aus einem neuen Ordner starten. Siehe die [Release-Notes](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) fur detaillierte Anweisungen.

---

## Versionsgeschichte

- **v3.1.2** -- Sacred-Werte fehlten bei Installation/Exporten im God Mode-Tab behoben. Siehe [Release-Notes](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2).
- **v3.1.1** -- Falscher Fehlalarm bei der Erkennung manipulierter Backups bei sauberen Spieldateien behoben.
- **v3.1** -- Sacred God Mode-Uberschreibungen (Benutzeranderungen dauerhaft vor Neuaufbauten geschutzt). Lock-on Auto-Rotate-Schalter (Dank an [sillib1980](https://github.com/sillib1980)). Grune Sacred-Indikatoren. Full Manual Control-Installationsfix. Versionsfahiges Upgrade-Overlay. Siehe [Release-Notes](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** -- Alle Dialoge in das In-App-Overlay-System umgewandelt. God Mode-Uberschreibungen bleiben uber Tab-Wechsel erhalten. Preset-Typ-Auswahl (UCM Managed vs Full Manual Control). Community-Preset-Katalog ins Hauptrepository verschoben. 54 God Mode-Attribut-Tooltips. Spielabsturz-Fixes. Vanilla-Validierung fur den Spielpatch vom Juni 2026 aktualisiert. 21-seitiges Wiki.
- **v3.0.1** -- Export-first-Neugestaltung. Dreistufiger Editor (UCM Quick / Fine Tune / God Mode). `.ucmpreset`-Dateiformat. Dateibasiertes Preset-System. UCM- und Community-Preset-Kataloge. Multi-Format-Export. Steadycam auf 30+ Kamerazustande erweitert. Lock-on-Zoom-Schieberegler.
- **v2.5** -- Letztes v2.x-Release.
- **v2.4** -- Proportionale horizontale Verschiebung, Verschiebung auf allen Reittieren und Zielfahigkeiten, Pferdekamera-Uberarbeitung, versionsfahige Backups, FoV-Vorschau, grossenveranderliches Fenster.
- **v2.3** -- Horizontale Verschiebungs-Fix fur 16:9, delta-basierter Schieberegler, vollstandiges Installationskonfigurationsbanner.
- **v2.2** -- Steadycam, zusatzliche Zoomstufen, Pferd-Ego-Perspektive, horizontale Verschiebung, universelles FoV, Fahigkeiten-Zielkonsistenz, XML-Import, Preset-Weitergabe, Update-Benachrichtigungen.
- **v2.1** -- Benutzerdefinierte Preset-Schieberegler schrieben nicht auf alle Zoomstufen -- behoben.
- **v2.0** -- Komplette Neuentwicklung von Python zu C# / .NET 6 / WPF. Erweiterter XML-Editor, Preset-Verwaltung, automatische Spielerkennung.
- **v1.5** -- Python-Version mit customtkinter-Oberflache.

---

## Mitwirkende und Danksagungen

- **0xFitz** -- UCM-Entwicklung, Kameraabstimmung, erweiterter Editor
- **[@sillib1980](https://github.com/sillib1980)** -- Entdeckte die Lock-on Auto-Rotate-Kamerafelder

### C#-Neuentwicklung (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** -- CrimsonDesertTools -- C# PAZ/PAMT-Parser, ChaCha20-Verschlusselung, LZ4-Kompression, PaChecksum, Archiv-Repacker (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** -- Reine C# ChaCha20-Stromchiffre-Implementierung (BSD)
- **[MrIkso auf Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** -- PAZ-Repack-Anleitung: 16-Byte-Ausrichtung, PAMT-Prufsumme, PAPGT-Root-Index-Patching

### Originale Python-Version (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** -- crimson-desert-unpacker -- PAZ-Archiv-Werkzeuge, Entschlusselungsforschung
- **Maszradine** -- CDCamera -- Kameraregeln, Steadycam-System, Stil-Presets
- **manymanecki** -- CrimsonCamera -- Dynamische PAZ-Modifikationsarchitektur

## Unterstutzung

Wenn du dies nutzlich findest, erwage die Entwicklung zu unterstutzen:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Lizenz

MIT
