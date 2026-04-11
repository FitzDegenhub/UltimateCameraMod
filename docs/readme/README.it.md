[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | **Italiano** | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.2 e disponibile!** Override Sacred God Mode, interruttore Lock-on Auto-Rotate e tutte le correzioni di bug. Scarica da **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** o **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Kit di strumenti per la telecamera standalone per Crimson Desert. GUI completa, anteprima telecamera in tempo reale, tre livelli di modifica, preset basati su file, **esportazione JSON per [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** e **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), e supporto HUD ultrawide.

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

> Hai bisogno di aiuto? Consulta il **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** per guide all'installazione, spiegazione delle impostazioni della telecamera, gestione dei preset, risoluzione dei problemi e documentazione per sviluppatori.

---

<details>
<summary><strong>Screenshot (v3.x)</strong> -- clicca per espandere</summary>
<br>

**UCM Quick** -- distanza, altezza, spostamento, FoV, lock-on zoom, steadycam, anteprime in tempo reale
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- regolazione approfondita con schede bordate ricercabili
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** -- editor XML grezzo completo con confronto vanilla
![God Mode](../../screenshots/v3.x/godmode.png)

**Esportazione JSON** -- esportazione per JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**Importazione** -- importazione da .ucmpreset, XML, PAZ o pacchetti Mod Manager
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## Panoramica dei branch

| Branch | Stato | Descrizione |
|--------|-------|-------------|
| **`main`** | Release v3.2 | Kit di strumenti per telecamera standalone con editor a tre livelli (UCM Quick / Fine Tune / God Mode), preset basati su file, catalogo della comunita, esportazione multi-formato e installazione diretta PAZ |
| **`development`** | Sviluppo | Branch di sviluppo per la prossima versione |

v3 include tutte le funzionalita della telecamera di v2 piu un'interfaccia ridisegnata, preset basati su file, un editor a tre livelli e l'esportazione multi-formato. L'installazione diretta PAZ e ancora disponibile in v3 come opzione secondaria.

---

## Funzionalita

### Controlli della telecamera

| Funzionalita | Dettagli |
|-------------|----------|
| **8 preset integrati** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival - con anteprima in tempo reale |
| **Telecamera personalizzata** | Cursori per distanza (1.5-12), altezza (-1.6-1.5) e spostamento orizzontale (-3-3). Il ridimensionamento proporzionale mantiene il personaggio nella stessa posizione sullo schermo a tutti i livelli di zoom |
| **Campo visivo** | Da 40 gradi vanilla fino a 80 gradi. Coerenza universale del FoV in stati di guardia, mira, cavalcatura, planata e cinematiche |
| **Telecamera centrata** | Personaggio perfettamente centrato in oltre 150 stati della telecamera, eliminando la telecamera a spalla con offset a sinistra |
| **Lock-on zoom** | Cursore da -60% (zoom sul bersaglio) a +60% (allargamento). Influisce su tutti gli stati di lock-on, guardia e carica. Funziona indipendentemente dalla Steadycam |
| **Lock-on auto-rotate** | Disattiva lo scatto automatico della telecamera verso il bersaglio durante il blocco. Impedisce alla telecamera di ruotare bruscamente verso i nemici dietro di te. Merito a [@sillib1980](https://github.com/sillib1980) |
| **Sincronizzazione telecamera cavalcatura** | Le telecamere delle cavalcature si adattano all'altezza della telecamera del giocatore scelta |
| **Spostamento orizzontale su tutte le cavalcature** | Cavallo, elefante, viverna, canoa, macchina da guerra e scopa rispettano tutti l'impostazione di spostamento con ridimensionamento proporzionale |
| **Coerenza mira delle abilita** | Lanterna, Lampo Accecante, Arco e tutte le abilita di mira/zoom/interazione rispettano lo spostamento orizzontale. Nessuno scatto della telecamera all'attivazione delle abilita |
| **Stabilizzazione Steadycam** | Tempi di fusione normalizzati e oscillazione della velocita in oltre 30 stati della telecamera: inattivo, camminata, corsa, sprint, combattimento, guardia, carica, caduta libera, super salto, trazione/oscillazione su corda, contraccolpo, tutte le varianti lock-on, lock-on su cavalcatura, lock-on rianimazione, aggro/ricercato, macchina da guerra e tutti gli stati di cavalcatura. Ogni valore e regolabile dalla comunita tramite l'editor Fine Tune |
| **Sacred God Mode** | I valori che modifichi in God Mode sono permanentemente protetti dalle ricostruzioni Quick/Fine Tune. Indicatori verdi mostrano quali valori sono protetti. Memorizzazione per preset |

> **Filosofia di progettazione v3: solo modifiche ai valori, nessuna iniezione strutturale.**
>
> Le versioni precedenti iniettavano nuove righe XML nel file della telecamera (livelli di zoom extra, modalita prima persona a cavallo, revisione della telecamera a cavallo con livelli di zoom aggiuntivi). v3 rimuove queste funzionalita intenzionalmente. L'iniezione di struttura ha una probabilita molto piu alta di rompersi dopo gli aggiornamenti del gioco, e le preferenze personali per modalita di telecamera di nicchia sono meglio servite da mod dedicati distribuiti tramite gestori di mod. UCM ora modifica solo i valori esistenti -- stesso numero di righe, stessa struttura degli elementi, stessi attributi. Questo rende i preset piu sicuri da condividere e piu resilienti tra le patch del gioco.

### Editor a tre livelli (v3)

v3 organizza la modifica in tre schede cosi puoi approfondire quanto vuoi:

| Livello | Scheda | Cosa fa |
|---------|--------|---------|
| 1 | **UCM Quick** | Il livello veloce -- cursori distanza/altezza/spostamento, FoV, telecamera centrata, lock-on zoom (da -60% a +60%), lock-on auto-rotate, sincronizzazione cavalcatura, steadycam, anteprime telecamera + FoV in tempo reale |
| 2 | **Fine Tune** | Regolazione approfondita curata. Sezioni ricercabili per livelli di zoom a piedi, zoom cavallo/cavalcatura, FoV globale, cavalcature speciali e attraversamento, combattimento e lock-on, stabilizzazione telecamera e mira e posizione del mirino. Si basa su UCM Quick |
| 3 | **God Mode** | Editor XML grezzo completo -- ogni parametro in un DataGrid ricercabile e filtrabile raggruppato per stato della telecamera. Colonna di confronto vanilla. Override Sacred (verdi) protetti dalle ricostruzioni. Filtro "Solo Sacred". 54 tooltip degli attributi |

### Sistema di preset basato su file (v3)

- **Formato file `.ucmpreset`** -- formato dedicato e condivisibile per i preset della telecamera UCM. Rilascialo in qualsiasi cartella preset e funziona
- **Gestore della barra laterale** con sezioni raggruppate comprimibili: Predefinito del gioco, Preset UCM, Preset della comunita, I miei preset, Importati
- **Nuovo / Duplica / Rinomina / Elimina** dalla barra laterale
- **Blocca** i preset per prevenire modifiche accidentali -- i preset UCM sono permanentemente bloccati; i preset dell'utente attivabili tramite icona del lucchetto
- **Preset Vanilla autentico** -- `playercamerapreset` grezzo decodificato dal backup del gioco senza modifiche applicate. I cursori rapidi sono sincronizzati con i valori base effettivi del gioco
- **Importazione** da `.ucmpreset`, XML grezzo, archivi PAZ o pacchetti Mod Manager. Le importazioni `.ucmpreset` ottengono il pieno controllo dei cursori UCM; le importazioni XML grezzo/PAZ/mod manager sono preset autonomi (solo modifica God Mode, nessuna regola UCM applicata) per preservare i valori dell'autore originale del mod
- **Salvataggio automatico** -- le modifiche ai preset sbloccati vengono scritte automaticamente nel file del preset (con ritardo)
- Migrazione automatica dai vecchi preset `.json` a `.ucmpreset` al primo avvio

### Cataloghi dei preset (v3)

Sfoglia e scarica preset direttamente da UCM. Download con un clic, nessun account necessario.

- **Preset UCM** -- 7 stili di telecamera ufficiali (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definizioni ospitate su GitHub, XML di sessione generato localmente dai file di gioco + regole della telecamera correnti. Rigenerazione automatica quando le regole della telecamera vengono aggiornate
- **[Preset della comunita](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- preset contribuiti dalla comunita nel repository principale, catalogo generato automaticamente da GitHub Actions
- **Pulsante Sfoglia** nell'intestazione di ogni gruppo della barra laterale apre il browser del catalogo
- Ogni preset mostra nome, autore, descrizione, tag e un link alla pagina Nexus del creatore
- **Rilevamento aggiornamenti** -- icona di aggiornamento pulsante quando una versione piu recente e disponibile nel catalogo. Clicca per scaricare l'aggiornamento con backup opzionale nei Miei preset
- I preset scaricati appaiono nella barra laterale (bloccati per impostazione predefinita -- duplica per modificare)
- **Limite di 2MB** per la dimensione del file e validazione JSON per la sicurezza

**Vuoi condividere il tuo preset con la comunita?** Esporta come `.ucmpreset` da UCM, poi:
- Invia una [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) aggiungendo il tuo preset alla cartella `community_presets/`
- Oppure invia il tuo file `.ucmpreset` a 0xFitz su Discord/Nexus e lo aggiungeremo per te

### Esportazione multi-formato (v3)

La finestra **Esporta per condivisione** produce la tua sessione in quattro modi:

| Formato | Caso d'uso |
|---------|------------|
| **JSON** (gestori di mod) | Patch di byte + `modinfo` per **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) o **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Esporta in UCM, importa nel gestore che usi; i destinatari non hanno bisogno di UCM. **Prepara** viene offerto solo quando la voce `playercamerapreset` attiva corrisponde ancora al backup vanilla di UCM (verifica i file di gioco se hai gia applicato mod della telecamera). |
| **XML** | `playercamerapreset.xml` grezzo per altri strumenti o modifica manuale |
| **0.paz** | Archivio patchato pronto da inserire nella cartella `0010` del gioco |
| **.ucmpreset** | Preset UCM completo per altri utenti UCM |

Include campi per titolo, versione, autore, URL Nexus e descrizione per JSON/XML. Mostra il conteggio delle regioni di patch e i byte modificati prima di salvare `.json`.

### Qualita della vita

- **Rilevamento automatico del gioco** -- Steam, Epic Games, Xbox / Game Pass
- **Backup automatico** -- backup vanilla prima di qualsiasi modifica; ripristino con un clic. Consapevolezza della versione con pulizia automatica all'aggiornamento
- **Banner configurazione installazione** -- mostra la tua configurazione attiva completa (FoV, distanza, altezza, spostamento, impostazioni)
- **Consapevolezza delle patch di gioco** -- traccia i metadati dell'installazione dopo l'applicazione; avvisa quando il gioco potrebbe essersi aggiornato cosi puoi riesportare
- **Anteprima telecamera + FoV in tempo reale** -- vista dall'alto sensibile alla distanza con spostamento orizzontale e cono del campo visivo
- **Notifiche di aggiornamento** -- controlla le release su GitHub all'avvio
- **Scorciatoia alla cartella del gioco** -- apre la directory del gioco dall'intestazione
- **Identita barra delle applicazioni Windows** -- raggruppamento icone corretto e icona nella barra del titolo tramite shell property store
- **Persistenza delle impostazioni** -- tutte le selezioni ricordate tra le sessioni
- **Finestra ridimensionabile** -- le dimensioni persistono tra le sessioni
- **Portatile** -- singolo `.exe`, nessun installer richiesto

### Filosofia

> **Nessuno ha ancora perfezionato la telecamera di Crimson Desert -- e questo e il punto.**
>
> Il gioco vanilla ha oltre 150 stati della telecamera, ciascuno con decine di parametri. Nessun singolo sviluppatore puo regolare tutto questo per ogni stile di gioco e display. Ecco perche esiste UCM -- non per dirti qual e la telecamera perfetta, ma per darti gli strumenti per trovarla da solo e condividerla con gli altri.
>
> Ogni impostazione che regoli puo essere esportata e condivisa. La correzione del lock-on auto-rotate che ha eliminato lo scatto della telecamera durante il combattimento e stata scoperta da un singolo membro della comunita che sperimentava in God Mode. Questo tipo di regolazione fine guidata dalla comunita e esattamente lo scopo di questo strumento.

### Condivisione dei preset

Esporta la tua configurazione della telecamera come file `.ucmpreset` e condividila con gli altri. Importa preset dal catalogo della comunita, da Nexus Mods o da altri giocatori. UCM esporta anche in JSON (per [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) e [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), XML grezzo e installazione diretta PAZ.

---

## Come funziona

1. Localizza l'archivio PAZ del gioco contenente `playercamerapreset.xml`
2. Crea un backup del file originale (solo una volta -- non sovrascrive mai un backup pulito)
3. Decifra la voce dell'archivio (ChaCha20 + Jenkins hash key derivation)
4. Decomprime tramite LZ4
5. Analizza e modifica i parametri XML della telecamera in base alle tue selezioni
6. Ricomprime, ricifra e scrive la voce modificata nell'archivio

Nessuna iniezione DLL, nessun hacking della memoria, nessuna connessione internet richiesta -- pura modifica dei file di dati.

---

## Compilazione dal sorgente

Richiede [.NET 6 SDK](https://dotnet.microsoft.com/download/net/6.0) (o successivo). Windows x64.

### v3 (consigliato)

Chiudi qualsiasi istanza in esecuzione prima di compilare -- il passaggio di copia dell'exe fallisce se il file e bloccato.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Dipendenze (NuGet -- ripristinate automaticamente)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) -- Compressione/decompressione a blocchi LZ4

---

## Struttura del progetto

```
src/UltimateCameraMod/              Libreria condivisa + app WPF v2.x
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, modelli dati
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 Interfaccia v2.x
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           App WPF v3 con priorita all'esportazione (fa riferimento al codice condiviso sopra)
├── Controls/                       CameraPreview, FovPreview (varianti v3)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Preset della comunita integrati distribuiti al primo avvio
├── MainWindow.xaml                 Shell a due pannelli: barra laterale + editor a schede
├── ExportJsonDialog.xaml           Procedura guidata esportazione multi-formato (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Importazione da .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Inserimento metadati preset (nome, autore, descrizione, URL)
├── CommunityBrowserDialog.xaml     Sfoglia e scarica preset della comunita da GitHub
├── NewPresetDialog.xaml            Crea / nomina nuovi preset
├── ShellTaskbarPropertyStore.cs    Icona barra delle applicazioni Windows tramite shell property store
├── ApplicationIdentity.cs          App User Model ID condiviso
└── UltimateCameraMod.V3.csproj

community_presets/                  Preset della telecamera contribuiti dalla comunita
ucm_presets/                        Definizioni ufficiali degli stili dei preset UCM
```

---

## Compatibilita

- **Piattaforme:** Steam, Epic Games, Xbox / Game Pass
- **Sistema operativo:** Windows 10/11 (x64)
- **Display:** Qualsiasi rapporto d'aspetto -- 16:9, 21:9, 32:9

---

## FAQ

**Questo puo causare un ban?**
UCM modifica solo file di dati offline. Non tocca la memoria del gioco, non inietta codice e non interagisce con i processi in esecuzione. Usa a tua discrezione nelle modalita online/multiplayer.

**Il gioco si e aggiornato e la mia telecamera e tornata a vanilla.**
Normale -- gli aggiornamenti del gioco sovrascrivono i file modificati. Riapri UCM e clicca Installa (o riesporta il JSON per JSON Mod Manager / CDUMM). Le tue impostazioni vengono salvate automaticamente.

**Il mio antivirus ha segnalato l'exe.**
Falso positivo noto con le app .NET standalone. La scansione VirusTotal e pulita: [v3.2](https://www.virustotal.com/gui/file-analysis/ZWMzZGM4MGM3ZWFlZTY5MTFmZDYwYzNkODFlZGM4Mjg6MTc3NTkxMzY4Mg==). Il codice sorgente completo e disponibile qui per la revisione e la compilazione autonoma.

**Cosa significa spostamento orizzontale 0?**
0 = posizione telecamera vanilla (personaggio leggermente a sinistra). 0.5 = personaggio centrato sullo schermo. I valori negativi spostano ulteriormente a sinistra, i valori positivi ulteriormente a destra.

**Aggiornamento da una versione precedente?**
Utenti v3.x: sostituite semplicemente l'exe, tutti i preset e le impostazioni vengono preservati. Utenti v2.x: eliminate la vecchia cartella UCM, verificate i file di gioco su Steam, poi eseguite v3.1 da una nuova cartella. Consultate le [note di rilascio](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) per istruzioni dettagliate.

---

## Cronologia delle versioni

- **v3.2** -- Corretti i valori sacred mancanti da Installa/esportazioni nella scheda God Mode. Vedi [note di rilascio](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.2).
- **v3.1.1** -- Corretto il falso-positivo di rilevamento backup corrotto su file di gioco puliti.
- **v3.1** -- Override Sacred God Mode (le modifiche dell'utente permanentemente protette dalle ricostruzioni). Interruttore Lock-on Auto-Rotate (merito a [sillib1980](https://github.com/sillib1980)). Indicatori sacred verdi. Correzione installazione Controllo Manuale Completo. Overlay di consapevolezza versione all'aggiornamento. Vedi [note di rilascio](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** -- Tutte le finestre di dialogo convertite al sistema di overlay in-app. Gli override di God Mode persistono tra i cambi di scheda. Selezione tipo preset (Gestito UCM vs Controllo Manuale Completo). Catalogo preset della comunita spostato nel repository principale. 54 tooltip attributi God Mode. Correzioni crash del gioco. Validazione vanilla aggiornata per la patch di gioco di giugno 2026. Wiki di 21 pagine.
- **v3.0.1** -- Riprogettazione con priorita all'esportazione. Editor a tre livelli (UCM Quick / Fine Tune / God Mode). Formato file `.ucmpreset`. Sistema di preset basato su file. Cataloghi preset UCM e della comunita. Esportazione multi-formato. Steadycam estesa a oltre 30 stati della telecamera. Cursore Lock-on zoom.
- **v2.5** -- Ultimo rilascio v2.x.
- **v2.4** -- Spostamento orizzontale proporzionale, spostamento su tutte le cavalcature e abilita di mira, revisione telecamera a cavallo, backup con consapevolezza della versione, anteprima FoV, finestra ridimensionabile.
- **v2.3** -- Correzione spostamento orizzontale per 16:9, cursore basato su delta, banner completo configurazione installazione.
- **v2.2** -- Steadycam, livelli di zoom extra, prima persona a cavallo, spostamento orizzontale, FoV universale, coerenza mira delle abilita, Importazione XML, condivisione preset, notifiche di aggiornamento.
- **v2.1** -- Corretti i cursori dei preset personalizzati che non scrivevano su tutti i livelli di zoom.
- **v2.0** -- Riscrittura completa da Python a C# / .NET 6 / WPF. Editor XML avanzato, gestione preset, rilevamento automatico del gioco.
- **v1.5** -- Versione Python con GUI customtkinter.

---

## Crediti e ringraziamenti

- **0xFitz** -- Sviluppo UCM, regolazione telecamera, editor avanzato
- **[@sillib1980](https://github.com/sillib1980)** -- Ha scoperto i campi della telecamera Lock-on Auto-Rotate

### Riscrittura in C# (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** -- CrimsonDesertTools -- Parser PAZ/PAMT in C#, crittografia ChaCha20, compressione LZ4, PaChecksum, repacker archivi (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** -- Implementazione pura in C# del cifrario a flusso ChaCha20 (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** -- Guida al repacking PAZ: allineamento a 16 byte, checksum PAMT, patching dell'indice root PAPGT

### Versione originale Python (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** -- crimson-desert-unpacker -- Strumenti per archivi PAZ, ricerca sulla decrittazione
- **Maszradine** -- CDCamera -- Regole della telecamera, sistema steadycam, preset degli stili
- **manymanecki** -- CrimsonCamera -- Architettura di modifica dinamica PAZ

## Supporto

Se trovi utile questo progetto, considera di supportare lo sviluppo:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Licenza

MIT
