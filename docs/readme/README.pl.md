[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | **Polski** | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.2 jest juz dostepna!** Nadpisywanie Sacred God Mode, przelacznik Lock-on Auto-Rotate i wszystkie poprawki bledow. Pobierz z **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** lub **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Samodzielny zestaw narzedzi kamery dla Crimson Desert. Pelny GUI, podglad kamery na zywo, trzy poziomy edycji, system presetow oparty na plikach, **eksport JSON dla [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** i **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) oraz wsparcie HUD dla szerokich ekranow.

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

> Potrzebujesz pomocy? Sprawdz **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)**, gdzie znajdziesz poradniki konfiguracji, wyjasnienia ustawien kamery, zarzadzanie presetami, rozwiazywanie problemow i dokumentacje deweloperska.

---

<details>
<summary><strong>Zrzuty ekranu (v3.x)</strong> -- kliknij, aby rozwinac</summary>
<br>

**UCM Quick** -- dystans, wysokosc, przesuniecie, FoV, lock-on zoom, steadycam, podglad na zywo
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- wysekcjonowane glebokie strojenie z przeszukiwalnymi kartami w ramkach
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** -- pelny surowy edytor XML z porownaniem do vanilla
![God Mode](../../screenshots/v3.x/godmode.png)

**Eksport JSON** -- eksport dla JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**Import** -- import z .ucmpreset, XML, PAZ lub pakietow Mod Manager
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## Przeglad galezi

| Galaz | Status | Opis |
|-------|--------|------|
| **`main`** | Wydanie v3.2 | Samodzielny zestaw narzedzi kamery z trzypoziomowym edytorem (UCM Quick / Fine Tune / God Mode), presetami opartymi na plikach, katalogiem spolecznosci, eksportem wieloformatowym i bezposrednia instalacja PAZ |
| **`development`** | Rozwoj | Galaz rozwojowa nastepnej wersji |

v3 zawiera wszystkie funkcje kamery z v2 oraz przeprojektowany interfejs, presety oparte na plikach, trzypoziomowy edytor i eksport wieloformatowy. Bezposrednia instalacja PAZ jest nadal dostepna w v3 jako opcja drugorzedna.

---

## Funkcje

### Sterowanie kamera

| Funkcja | Szczegoly |
|---------|-----------|
| **8 wbudowanych presetow** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival - z podgladem na zywo |
| **Niestandardowa kamera** | Suwaki dystansu (1.5-12), wysokosci (-1.6-1.5) i przesuniecia poziomego (-3-3). Proporcjonalne skalowanie utrzymuje postac w tej samej pozycji na ekranie na wszystkich poziomach zblizenia |
| **Pole widzenia** | Vanilla 40 stopni do 80 stopni. Uniwersalna spojnosc FoV w trybach obrony, celowania, jazdy konnej, szybowania i kinematograficznym |
| **Wycentrowana kamera** | Postac dokladnie na srodku ekranu w ponad 150 stanach kamery, eliminujac kamera na ramie z przesunieniem w lewo |
| **Lock-on zoom** | Suwak od -60% (zblizenie na cel) do +60% (oddalenie). Wplywa na wszystkie stany lock-on, obrony i szarzy. Dziala niezaleznie od Steadycam |
| **Lock-on auto-rotate** | Wylacz automatyczne obracanie kamery w strone celu podczas blokowania. Zapobiega gwaltownemu obracaniu kamery w strone wrogow za toba. Dzieki [@sillib1980](https://github.com/sillib1980) |
| **Synchronizacja kamery wierzchowca** | Kamery wierzchowcow dopasowuja sie do wybranej wysokosci kamery gracza |
| **Przesuniecie poziome na wszystkich wierzchowcach** | Kon, slon, wiwerna, kajak, machina wojenna i miotla uwzgledniaja ustawienie przesuniecia z proporcjonalnym skalowaniem |
| **Spojnosc celowania umiejetnosci** | Latarnia, Oslepiajacy Blask, Luk i wszystkie umiejetnosci celowania/zblizenia/interakcji uwzgledniaja przesuniecie poziome. Brak skoku kamery podczas aktywacji umiejetnosci |
| **Wygladzanie Steadycam** | Znormalizowany czas mieszania i wahania predkosci w ponad 30 stanach kamery: bezczynnosc, chod, bieg, sprint, walka, obrona, szarza/ladowanie, swobodny spadek, super skok, ciagniecie/hustawka na linie, odrzucenie, wszystkie warianty lock-on, lock-on na wierzchowcu, lock-on przy wskrzeszaniu, agresja/poszukiwanie, machina wojenna i wszystkie stany wierzchowcow. Kazda wartosc jest dosrajana przez spolecznosc za pomoca edytora Fine Tune |
| **Sacred God Mode** | Wartosci edytowane w God Mode sa trwale chronione przed przebudowa Quick/Fine Tune. Zielone wskazniki pokazuja, ktore wartosci sa chronione. Przechowywanie per-preset |

> **Filozofia projektowa v3: tylko edycja wartosci, zadnego wstrzykiwania struktury.**
>
> Wczesniejsze wersje wstrzykiwaly nowe linie XML do pliku kamery (dodatkowe poziomy zblizenia, tryb pierwszej osoby na koniu, przebudowa kamery konnej z dodatkowymi poziomami zblizenia). v3 celowo usuwa te funkcje. Wstrzykiwanie struktury ma znacznie wieksze ryzyko awarii po aktualizacjach gry, a osobiste preferencje dotyczace niszowych trybow kamery sa lepiej obslugiwane przez dedykowane mody dystrybuowane przez menedzery modow. UCM teraz modyfikuje tylko istniejace wartosci -- ta sama liczba linii, ta sama struktura elementow, te same atrybuty. Dzieki temu presety sa bezpieczniejsze do udostepniania i bardziej odporne na latki gry.

### Trzypoziomowy edytor (v3)

v3 organizuje edycje w trzy zakladki, abys mogl zaglebic sie tak daleko, jak chcesz:

| Poziom | Zakladka | Co robi |
|--------|----------|---------|
| 1 | **UCM Quick** | Szybka warstwa -- suwaki dystansu/wysokosci/przesuniecia, FoV, wycentrowana kamera, lock-on zoom (-60% do +60%), lock-on auto-rotate, synchronizacja wierzchowcow, steadycam, podglad kamery + FoV na zywo |
| 2 | **Fine Tune** | Wysekcjonowane glebokie strojenie. Przeszukiwalne sekcje dla poziomow zblizenia pieszego, zblizenia konnego/wierzchowca, globalnego FoV, specjalnych wierzchowcow i przemieszczania, walki i lock-on, wygladzania kamery oraz celowania i pozycji celownika. Buduje na bazie UCM Quick |
| 3 | **God Mode** | Pelny surowy edytor XML -- kazdy parametr w przeszukiwalnym, filtrowalnym DataGrid pogrupowany wedlug stanu kamery. Kolumna porownania z vanilla. Chronione nadpisania Sacred (zielone) przed przebudowa. Filtr "Tylko Sacred". 54 podpowiedzi atrybutow |

### System presetow oparty na plikach (v3)

- **Format pliku `.ucmpreset`** -- dedykowany format do udostepniania presetow kamery UCM. Wrzuc do dowolnego folderu presetow i po prostu dziala
- **Menedzer paska bocznego** ze zwijanymi, pogrupowanymi sekcjami: Domyslne gry, Presety UCM, Presety spolecznosci, Moje presety, Zaimportowane
- **Nowy / Duplikuj / Zmien nazwe / Usun** z paska bocznego
- **Zablokuj** presety, aby zapobiec przypadkowym zmianom -- presety UCM sa trwale zablokowane; presety uzytkownika przelaczalne ikona klodki
- **Prawdziwy preset Vanilla** -- surowe zdekodowane `playercamerapreset` z kopii zapasowej gry bez zadnych zmian. Szybkie suwaki sa zsynchronizowane z rzeczywistymi wartosciami bazowymi gry
- **Import** z `.ucmpreset`, surowego XML, archiwow PAZ lub pakietow Mod Manager. Importy `.ucmpreset` zapewniaja pelna kontrole suwakami UCM; importy surowego XML/PAZ/mod managera to samodzielne presety (tylko edycja God Mode, bez zasad UCM) -- aby zachowac oryginalne wartosci autora moda
- **Automatyczny zapis** -- zmiany w odblokowanych presetach sa automatycznie zapisywane do pliku presetu (z opoznieniem)
- Automatyczna migracja ze starszych presetow `.json` do `.ucmpreset` przy pierwszym uruchomieniu

### Katalogi presetow (v3)

Przegladaj i pobieraj presety bezposrednio z UCM. Pobieranie jednym kliknieciem, bez koniecznosci zakladania konta.

- **Presety UCM** -- 7 oficjalnych stylow kamery (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definicje hostowane na GitHubie, XML sesji generowany lokalnie z plikow gry + aktualnych zasad kamery. Automatyczne ponowne generowanie po aktualizacji zasad kamery
- **[Presety spolecznosci](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- presety stworzone przez spolecznosc w glownym repozytorium, katalog generowany automatycznie przez GitHub Actions
- **Przycisk Przegladaj** w naglowku kazdej grupy na pasku bocznym otwiera przegladarke katalogu
- Kazdy preset pokazuje nazwe, autora, opis, tagi i link do strony twory na Nexusie
- **Wykrywanie aktualizacji** -- pulsujaca ikona aktualizacji gdy nowsza wersja jest dostepna w katalogu. Kliknij, aby pobrac aktualizacje z opcjonalna kopia zapasowa do Moich presetow
- Pobrane presety pojawiaja sie na pasku bocznym (domyslnie zablokowane -- duplikuj, aby edytowac)
- **Limit rozmiaru pliku 2MB** i walidacja JSON dla bezpieczenstwa

**Chcesz udostepnic swoj preset spolecznosci?** Eksportuj jako `.ucmpreset` z UCM, a nastepnie:
- Zglosz [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) dodajacy Twoj preset do folderu `community_presets/`
- Lub wyslij plik `.ucmpreset` do 0xFitz na Discordzie/Nexusie, a dodamy go za Ciebie

### Eksport wieloformatowy (v3)

Okno **Eksportuj do udostepniania** eksportuje Twoja sesje na cztery sposoby:

| Format | Zastosowanie |
|--------|--------------|
| **JSON** (menedzery modow) | Latki bajtowe + `modinfo` dla **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) lub **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Eksportuj w UCM, importuj w menedzerze, ktorego uzywasz; odbiorcy nie potrzebuja UCM. **Przygotuj** jest oferowane tylko gdy aktualny wpis `playercamerapreset` nadal odpowiada kopii zapasowej vanilla UCM (zweryfikuj pliki gry, jesli juz zastosowalas/es mody kamery). |
| **XML** | Surowy `playercamerapreset.xml` dla innych narzedzi lub recznej edycji |
| **0.paz** | Poprawione archiwum gotowe do umieszczenia w folderze `0010` gry |
| **.ucmpreset** | Pelny preset UCM dla innych uzytkownikow UCM |

Zawiera pola tytulu, wersji, autora, URL Nexusa i opisu dla JSON/XML. Pokazuje liczbe regionow latek i zmienione bajty przed zapisaniem `.json`.

### Jakosc zycia

- **Automatyczne wykrywanie gry** -- Steam, Epic Games, Xbox / Game Pass
- **Automatyczna kopia zapasowa** -- kopia zapasowa vanilla przed jakakolwiek modyfikacja; przywracanie jednym kliknieciem. Swiadomosc wersji z automatycznym czyszczeniem przy aktualizacji
- **Banner konfiguracji instalacji** -- pokazuje pelna aktywna konfiguracje (FoV, dystans, wysokosc, przesuniecie, ustawienia)
- **Swiadomosc latek gry** -- sledzi metadane instalacji po zastosowaniu; ostrzega gdy gra mogla zostac zaktualizowana, abys mogl ponownie eksportowac
- **Podglad kamery + FoV na zywo** -- widok z gory z uwzglednieniem dystansu, z przesunieciem poziomym i stozkiem pola widzenia
- **Powiadomienia o aktualizacjach** -- sprawdza wydania na GitHubie przy uruchomieniu
- **Skrot do folderu gry** -- otwiera katalog gry z naglowka
- **Tozsamosc paska zadan Windows** -- prawidlowe grupowanie ikon i ikona paska tytulu przez shell property store
- **Trwalosc ustawien** -- wszystkie wybory zapamietywane miedzy sesjami
- **Okno o zmiennym rozmiarze** -- rozmiar zachowany miedzy sesjami
- **Przenosny** -- pojedynczy `.exe`, nie wymaga instalatora

### Filozofia

> **Nikt jeszcze nie udoskonalil kamery Crimson Desert -- i o to wlasnie chodzi.**
>
> Gra w wersji vanilla ma ponad 150 stanow kamery, kazdy z dziesiatkami parametrow. Zaden pojedynczy deweloper nie jest w stanie dostroic tego wszystkiego dla kazdego stylu gry i wyswietlacza. Dlatego istnieje UCM -- nie po to, aby mowic Ci, jaka jest idealna kamera, ale aby dac Ci narzedzia do znalezienia jej samodzielnie i podzielenia sie nia z innymi.
>
> Kazde ustawienie, ktore dostoisz, moze byc wyeksportowane i udostepnione. Poprawka lock-on auto-rotate, ktora wyeliminowala skok kamery podczas walki, zostala odkryta przez pojedynczego czlonka spolecznosci eksperymentujacego w God Mode. Tego rodzaju strojenie napedzane przez spolecznosc jest dokladnie tym, do czego to narzedzie sluzy.

### Udostepnianie presetow

Eksportuj ustawienia kamery jako plik `.ucmpreset` i udostepnij innym. Importuj presety z katalogu spolecznosci, Nexus Mods lub od innych graczy. UCM eksportuje rowniez do JSON (dla [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) i [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), surowego XML i bezposredniej instalacji PAZ.

---

## Jak to dziala

1. Lokalizuje archiwum PAZ gry zawierajace `playercamerapreset.xml`
2. Tworzy kopie zapasowa oryginalnego pliku (tylko raz -- nigdy nie nadpisuje czystej kopii zapasowej)
3. Odszyfrowuje wpis archiwum (ChaCha20 + Jenkins hash key derivation)
4. Dekompresuje przez LZ4
5. Parsuje i modyfikuje parametry kamery XML na podstawie Twoich wyborow
6. Ponownie kompresuje, ponownie szyfruje i zapisuje zmodyfikowany wpis z powrotem do archiwum

Bez wstrzykiwania DLL, bez hackowania pamieci, bez wymaganego polaczenia internetowego -- czysta modyfikacja plikow danych.

---

## Budowanie ze zrodla

Wymaga [.NET 6 SDK](https://dotnet.microsoft.com/download/net/6.0) (lub nowszego). Windows x64.

### v3 (zalecane)

Zamknij wszystkie uruchomione instancje przed budowaniem -- krok kopiowania exe konczy sie niepowodzeniem jesli plik jest zablokowany.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Zaleznosci (NuGet -- przywracane automatycznie)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) -- kompresja/dekompresja blokow LZ4

---

## Struktura projektu

```
src/UltimateCameraMod/              Wspoldzielona biblioteka + aplikacja WPF v2.x
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, modele danych
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 Interfejs v2.x
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           Aplikacja WPF v3 z priorytetem eksportu (odwoluje sie do wspoldzielonego kodu powyzej)
├── Controls/                       CameraPreview, FovPreview (warianty v3)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Wbudowane presety spolecznosci wdrazane przy pierwszym uruchomieniu
├── MainWindow.xaml                 Dwupanelowa powloka: pasek boczny + edytor z zakladkami
├── ExportJsonDialog.xaml           Kreator eksportu wieloformatowego (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Import z .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Wprowadzanie metadanych presetu (nazwa, autor, opis, URL)
├── CommunityBrowserDialog.xaml     Przegladanie i pobieranie presetow spolecznosci z GitHuba
├── NewPresetDialog.xaml            Tworzenie / nazywanie nowych presetow
├── ShellTaskbarPropertyStore.cs    Ikona paska zadan Windows przez shell property store
├── ApplicationIdentity.cs          Wspoldzielone App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  Presety kamery stworzone przez spolecznosc
ucm_presets/                        Oficjalne definicje stylow presetow UCM
```

---

## Kompatybilnosc

- **Platformy:** Steam, Epic Games, Xbox / Game Pass
- **System operacyjny:** Windows 10/11 (x64)
- **Wyswietlacz:** Dowolny stosunek proporcji -- 16:9, 21:9, 32:9

---

## FAQ

**Czy to moze spowodowac bana?**
UCM modyfikuje tylko pliki danych offline. Nie dotyka pamieci gry, nie wstrzykuje kodu ani nie wchodzi w interakcje z uruchomionymi procesami. Uzywaj wedlug wlasnego uznania w trybach online/wieloosobowych.

**Gra sie zaktualizowala i moja kamera wrocila do vanilla.**
To normalne -- aktualizacje gry nadpisuja zmodyfikowane pliki. Ponownie otworz UCM i kliknij Zainstaluj (lub ponownie eksportuj JSON dla JSON Mod Manager / CDUMM). Twoje ustawienia sa zapisywane automatycznie.

**Moj antywirus oznaczy exe.**
Znany falszywy alarm z samodzielnymi aplikacjami .NET. Skan VirusTotal jest czysty: [v3.2](https://www.virustotal.com/gui/file-analysis/ZWMzZGM4MGM3ZWFlZTY5MTFmZDYwYzNkODFlZGM4Mjg6MTc3NTkxMzY4Mg==). Pelny kod zrodlowy jest dostepny tutaj do przegladniecia i samodzielnego zbudowania.

**Co oznacza przesuniecie poziome 0?**
0 = pozycja kamery vanilla (postac lekko po lewej). 0.5 = postac wycentrowana na ekranie. Wartosci ujemne przesuwaja dalej w lewo, wartosci dodatnie dalej w prawo.

**Aktualizujesz z poprzedniej wersji?**
Uzytkownicy v3.x: po prostu zastap exe, wszystkie presety i ustawienia sa zachowane. Uzytkownicy v2.x: usun stary folder UCM, zweryfikuj pliki gry na Steamie, a nastepnie uruchom v3.1 z nowego folderu. Zobacz [notatki wydania](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) po szczegolowe instrukcje.

---

## Historia wersji

- **v3.2** -- Naprawiono brakujace wartosci sacred w Instalacji/eksportach na zakladce God Mode. Zobacz [notatki wydania](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.2).
- **v3.1.1** -- Naprawiono falszywo-pozytywne wykrywanie uszkodzonej kopii zapasowej na czystych plikach gry.
- **v3.1** -- Nadpisywanie Sacred God Mode (edycje uzytkownika trwale chronione przed przebudowa). Przelacznik Lock-on Auto-Rotate (dzieki [sillib1980](https://github.com/sillib1980)). Zielone wskazniki sacred. Poprawka instalacji Pelnej Kontroli Recznej. Nakladka swiadomosci wersji przy aktualizacji. Zobacz [notatki wydania](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** -- Wszystkie okna dialogowe przekonwertowane na system nakladek w aplikacji. Nadpisania God Mode zachowywane miedzy przelaczaniem zakladek. Wybor typu presetu (Zarzadzany przez UCM vs Pelna Kontrola Reczna). Katalog presetow spolecznosci przeniesiony do glownego repozytorium. 54 podpowiedzi atrybutow God Mode. Poprawki awarii gry. Walidacja vanilla zaktualizowana dla latki gry z czerwca 2026. 21-stronicowe Wiki.
- **v3.0.1** -- Przeprojektowanie z priorytetem eksportu. Trzypoziomowy edytor (UCM Quick / Fine Tune / God Mode). Format pliku `.ucmpreset`. System presetow oparty na plikach. Katalogi presetow UCM i spolecznosci. Eksport wieloformatowy. Steadycam rozszerzony na ponad 30 stanow kamery. Suwak Lock-on zoom.
- **v2.5** -- Ostatnie wydanie v2.x.
- **v2.4** -- Proporcjonalne przesuniecie poziome, przesuniecie na wszystkich wierzchowcach i umiejetnosciach celowania, przebudowa kamery konnej, kopie zapasowe swiadome wersji, podglad FoV, okno o zmiennym rozmiarze.
- **v2.3** -- Poprawka przesuniecia poziomego dla 16:9, suwak oparty na delcie, pelny banner konfiguracji instalacji.
- **v2.2** -- Steadycam, dodatkowe poziomy zblizenia, kon pierwsza osoba, przesuniecie poziome, uniwersalny FoV, spojnosc celowania umiejetnosci, Import XML, udostepnianie presetow, powiadomienia o aktualizacjach.
- **v2.1** -- Naprawiono suwaki niestandardowych presetow nie zapisujace do wszystkich poziomow zblizenia.
- **v2.0** -- Kompletne przepisanie z Pythona na C# / .NET 6 / WPF. Zaawansowany edytor XML, zarzadzanie presetami, automatyczne wykrywanie gry.
- **v1.5** -- Wersja Python z interfejsem customtkinter.

---

## Autorzy i podziekowania

- **0xFitz** -- Rozwoj UCM, strojenie kamery, zaawansowany edytor
- **[@sillib1980](https://github.com/sillib1980)** -- Odkryl pola kamery Lock-on Auto-Rotate

### Przepisanie na C# (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** -- CrimsonDesertTools -- parser PAZ/PAMT w C#, szyfrowanie ChaCha20, kompresja LZ4, PaChecksum, repacker archiwow (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** -- Czysta implementacja szyfru strumieniowego ChaCha20 w C# (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** -- Przewodnik ponownego pakowania PAZ: wyrownanie 16-bajtowe, suma kontrolna PAMT, latanie indeksu glownego PAPGT

### Oryginalna wersja Python (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** -- crimson-desert-unpacker -- Narzedzia archiwow PAZ, badania nad odszyfrowywaniem
- **Maszradine** -- CDCamera -- Zasady kamery, system steadycam, presety stylow
- **manymanecki** -- CrimsonCamera -- Architektura dynamicznej modyfikacji PAZ

## Wsparcie

Jesli uważasz to za przydatne, rozważ wsparcie rozwoju:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Licencja

MIT
