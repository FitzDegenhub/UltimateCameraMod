[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | **Français** | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.2 est disponible !** Remplacements Sacred God Mode, option Lock-on Auto-Rotate et toutes les corrections de bugs. Telechargez depuis **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** ou **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Boite a outils camera autonome pour Crimson Desert. Interface graphique complete, apercu camera en temps reel, trois niveaux d'edition, systeme de presets base sur fichiers, **export JSON pour [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** et **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), et prise en charge du HUD ultrawide.

<p align="center">
  <img src="../../screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banniere" width="100%" />
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

> Besoin d'aide ? Consultez le **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** pour les guides d'installation, les explications des parametres camera, la gestion des presets, le depannage et la documentation developpeur.

---

<details>
<summary><strong>Captures d'ecran (v3.x)</strong> -- cliquez pour developper</summary>
<br>

**UCM Quick** -- distance, hauteur, decalage, FoV, zoom de verrouillage, steadycam, apercus en temps reel
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- reglage fin organise avec des cartes encadrees et recherche
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** -- editeur XML brut complet avec comparaison aux valeurs d'origine
![God Mode](../../screenshots/v3.x/godmode.png)

**Export JSON** -- export pour JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**Import** -- import depuis .ucmpreset, XML, PAZ ou paquets Mod Manager
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## Apercu des branches

| Branche | Statut | Description |
|---------|--------|-------------|
| **`main`** | v3.2 Release | Boite a outils camera autonome avec editeur a trois niveaux (UCM Quick / Fine Tune / God Mode), presets bases sur fichiers, catalogue communautaire, export multi-format et installation PAZ directe |
| **`development`** | Developpement | Branche de developpement pour la prochaine version |

v3 inclut toutes les fonctionnalites camera de v2 plus une interface repensee, des presets bases sur fichiers, un editeur a trois niveaux et l'export multi-format. L'installation PAZ directe est toujours disponible dans v3 comme option secondaire.

---

## Fonctionnalites

### Controles camera

| Fonctionnalite | Details |
|----------------|---------|
| **8 presets integres** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival -- avec apercu en temps reel |
| **Camera personnalisee** | Curseurs pour la distance (1.5-12), la hauteur (-1.6-1.5) et le decalage horizontal (-3-3). La mise a l'echelle proportionnelle maintient le personnage a la meme position a l'ecran sur tous les niveaux de zoom |
| **Champ de vision** | De 40° (valeur d'origine) jusqu'a 80°. Coherence universelle du FoV en modes garde, visee, monture, planeur et cinematique |
| **Camera centree** | Personnage parfaitement centre sur plus de 150 etats camera, eliminant la camera d'epaule decalee a gauche |
| **Zoom de verrouillage** | Curseur de -60 % (zoom avant sur la cible) a +60 % (recul elargi). Affecte tous les etats de verrouillage, garde et charge. Fonctionne independamment du Steadycam |
| **Auto-rotation du verrouillage** | Desactive l'accrochage de la camera sur la cible lors du verrouillage. Empeche la camera de pivoter brusquement pour faire face aux ennemis derriere vous. Credits a [@sillib1980](https://github.com/sillib1980) |
| **Synchronisation camera de monture** | Les cameras de monture s'adaptent a la hauteur camera choisie pour le joueur |
| **Decalage horizontal sur toutes les montures** | Cheval, elephant, wyverne, canoe, machine de guerre et balai respectent votre reglage de decalage avec mise a l'echelle proportionnelle |
| **Coherence de visee des competences** | Lanterne, Blinding Flash, arc et toutes les competences de visee/zoom/interaction respectent le decalage horizontal. Pas de saut de camera lors de l'activation des capacites |
| **Lissage Steadycam** | Timing de melange et variation de vitesse normalises sur plus de 30 etats camera : repos, marche, course, sprint, combat, garde, charge/assaut, chute libre, super saut, traction/balancement a la corde, recul, toutes les variantes de verrouillage, verrouillage en monture, verrouillage de reanimation, aggro/recherche, machine de guerre et tous les etats de monture. Chaque valeur est ajustable par la communaute via l'editeur Fine Tune |
| **Sacred God Mode** | Les valeurs que vous editez dans God Mode sont definitivement protegees des reconstructions Quick/Fine Tune. Des indicateurs verts montrent quelles valeurs sont protegees. Stockage par preset |

> **Philosophie de conception de v3 : modification des valeurs uniquement, pas d'injection structurelle.**
>
> Les versions precedentes injectaient de nouvelles lignes XML dans le fichier camera (niveaux de zoom supplementaires, mode premiere personne a cheval, refonte de la camera equestre avec des paliers de zoom additionnels). v3 supprime ces fonctionnalites volontairement. L'injection de structure a un risque bien plus eleve de dysfonctionnement apres les mises a jour du jeu, et les preferences personnelles pour des modes camera de niche sont mieux servies par des mods dedies distribues via les gestionnaires de mods. UCM ne modifie desormais que les valeurs existantes -- meme nombre de lignes, meme structure d'elements, memes attributs. Cela rend les presets plus surs a partager et plus resilients face aux patchs du jeu.

### Editeur a trois niveaux (v3)

v3 organise l'edition en trois onglets pour aller aussi loin que vous le souhaitez :

| Niveau | Onglet | Description |
|--------|--------|-------------|
| 1 | **UCM Quick** | La couche rapide -- curseurs distance/hauteur/decalage, FoV, camera centree, zoom de verrouillage (-60 % a +60 %), auto-rotation du verrouillage, synchro monture, steadycam, apercus camera et FoV en temps reel |
| 2 | **Fine Tune** | Reglage fin organise. Sections avec recherche pour les niveaux de zoom a pied, zoom cheval/monture, FoV global, montures speciales et traversee, combat et verrouillage, lissage de camera, et visee et position du reticule. S'appuie sur UCM Quick |
| 3 | **God Mode** | Editeur XML brut complet -- chaque parametre dans un DataGrid recherchable et filtrable, groupe par etat camera. Colonne de comparaison avec les valeurs d'origine. Remplacements Sacred (verts) proteges des reconstructions. Filtre "Sacred uniquement". 54 infobulles d'attributs |

### Systeme de presets base sur fichiers (v3)

- **Format de fichier `.ucmpreset`** -- format dedie et partageable pour les presets camera UCM. Deposez dans n'importe quel dossier de presets et ca fonctionne immediatement
- **Gestionnaire en barre laterale** avec sections groupees repliables : Defaut du jeu, Presets UCM, Presets communautaires, Mes presets, Importes
- **Nouveau / Dupliquer / Renommer / Supprimer** depuis la barre laterale
- **Verrouiller** les presets pour empecher les modifications accidentelles -- les presets UCM sont verrouilles en permanence ; les presets utilisateur sont commutables via l'icone de cadenas
- **Vrai preset Vanilla** -- `playercamerapreset` brut decode depuis votre sauvegarde du jeu sans modification. Les curseurs Quick sont synchronises sur les valeurs de base reelles du jeu
- **Import** depuis `.ucmpreset`, XML brut, archives PAZ ou paquets Mod Manager. Les imports `.ucmpreset` obtiennent le controle complet des curseurs UCM ; les imports XML brut/PAZ/Mod Manager sont des presets autonomes (edition God Mode uniquement, aucune regle UCM appliquee) pour preserver les valeurs de l'auteur du mod d'origine
- **Sauvegarde automatique** -- les modifications des presets deverrouilles sont ecrites automatiquement dans le fichier preset (avec temporisation)
- Migration automatique des anciens presets `.json` vers `.ucmpreset` au premier lancement

### Catalogues de presets (v3)

Parcourez et telechargez des presets directement depuis UCM. Telechargement en un clic, aucun compte necessaire.

- **Presets UCM** -- 7 styles camera officiels (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definitions hebergees sur GitHub, le XML de session est genere localement a partir de vos fichiers de jeu et des regles camera actuelles. Regeneration automatique lors de la mise a jour des regles camera
- **[Presets communautaires](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- presets contribues par la communaute dans le depot principal, catalogue genere automatiquement par GitHub Actions
- **Bouton Parcourir** sur chaque en-tete de groupe de la barre laterale pour ouvrir le navigateur du catalogue
- Chaque preset affiche le nom, l'auteur, la description, les tags et un lien vers la page Nexus du createur
- **Detection des mises a jour** -- icone de mise a jour pulsante quand une version plus recente est disponible dans le catalogue. Cliquez pour telecharger la mise a jour avec sauvegarde optionnelle dans Mes presets
- Les presets telecharges apparaissent dans la barre laterale (verrouilles par defaut -- dupliquer pour editer)
- **Limite de taille de fichier de 2 Mo** et validation JSON pour la securite

**Vous voulez partager votre preset avec la communaute ?** Exportez en `.ucmpreset` depuis UCM, puis soit :
- Soumettez une [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) ajoutant votre preset au dossier `community_presets/`
- Ou envoyez votre fichier `.ucmpreset` a 0xFitz sur Discord/Nexus et nous l'ajouterons pour vous

### Export multi-format (v3)

La boite de dialogue **Exporter pour partage** produit votre session en quatre formats :

| Format | Utilisation |
|--------|-------------|
| **JSON** (gestionnaires de mods) | Correctifs d'octets + `modinfo` pour **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) ou **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Exportez dans UCM, importez dans le gestionnaire que vous utilisez ; les destinataires n'ont pas besoin d'UCM. **Preparer** n'est propose que lorsque l'entree `playercamerapreset` active correspond encore a la sauvegarde vanilla d'UCM (verifiez les fichiers du jeu si vous avez deja applique des mods camera). |
| **XML** | `playercamerapreset.xml` brut pour d'autres outils ou l'edition manuelle |
| **0.paz** | Archive patchee a deposer directement dans le dossier `0010` du jeu |
| **.ucmpreset** | Preset UCM complet pour d'autres utilisateurs UCM |

Inclut les champs titre, version, auteur, URL Nexus et description pour l'export JSON/XML. Affiche le nombre de regions patchees et les octets modifies avant l'enregistrement du `.json`.

### Confort d'utilisation

- **Detection automatique du jeu** -- Steam, Epic Games, Xbox / Game Pass
- **Sauvegarde automatique** -- sauvegarde des fichiers d'origine avant toute modification ; restauration en un clic. Gestion des versions avec nettoyage automatique lors des mises a niveau
- **Banniere de configuration d'installation** -- affiche votre configuration active complete (FoV, distance, hauteur, decalage, parametres)
- **Detection des patchs du jeu** -- suit les metadonnees d'installation apres l'application ; avertit quand le jeu a pu etre mis a jour pour que vous puissiez re-exporter
- **Apercu camera et FoV en temps reel** -- vue de dessus tenant compte de la distance avec le decalage horizontal et le cone de champ de vision
- **Notifications de mise a jour** -- verifie les releases GitHub au demarrage
- **Raccourci dossier du jeu** -- ouvre le repertoire du jeu depuis l'en-tete
- **Identite dans la barre des taches Windows** -- regroupement d'icones et icone de barre de titre via le shell property store
- **Persistance des parametres** -- toutes les selections sont memorisees entre les sessions
- **Fenetre redimensionnable** -- la taille est conservee entre les sessions
- **Portable** -- un seul `.exe`, aucun installateur necessaire

### Philosophie

> **Personne n'a encore perfectionne la camera de Crimson Desert -- et c'est justement l'idee.**
>
> Le jeu d'origine compte plus de 150 etats camera, chacun avec des dizaines de parametres. Aucun developpeur ne peut tout regler pour chaque style de jeu et chaque ecran. C'est pourquoi UCM existe -- non pas pour vous dire quelle est la camera parfaite, mais pour vous donner les outils de la trouver vous-meme et de la partager avec les autres.
>
> Chaque reglage que vous effectuez peut etre exporte et partage. Le correctif Lock-on Auto-Rotate qui a elimine les sauts de camera en combat a ete decouvert par un seul membre de la communaute experimentant dans God Mode. C'est exactement ce type de reglage fin collaboratif que cet outil est concu pour faciliter.

### Partage de presets

Exportez votre configuration camera en fichier `.ucmpreset` et partagez-la avec d'autres. Importez des presets depuis le catalogue communautaire, Nexus Mods ou d'autres joueurs. UCM exporte egalement en JSON (pour [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) et [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), en XML brut et en installation PAZ directe.

---

## Fonctionnement

1. Localise l'archive PAZ du jeu contenant `playercamerapreset.xml`
2. Cree une sauvegarde du fichier original (une seule fois -- n'ecrase jamais une sauvegarde propre)
3. Dechiffre l'entree de l'archive (ChaCha20 + derivation de cle par hachage Jenkins)
4. Decompresse via LZ4
5. Analyse et modifie les parametres camera XML selon vos selections
6. Recompresse, rechiffre et ecrit l'entree modifiee dans l'archive

Pas d'injection de DLL, pas de manipulation de memoire, pas de connexion internet requise -- pure modification de fichiers de donnees.

---

## Compilation depuis les sources

Necessite le [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (ou superieur). Windows x64.

### v3 (recommande)

Fermez toute instance en cours avant la compilation -- l'etape de copie de l'exe echoue si le fichier est verrouille.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Dependances (NuGet -- restaurees automatiquement)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) -- Compression/decompression de blocs LZ4

---

## Structure du projet

```
src/UltimateCameraMod/              Bibliotheque partagee + application WPF v2.x
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, modeles de donnees
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 Interface v2.x
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           Application WPF v3 export-first (reference le code partage ci-dessus)
├── Controls/                       CameraPreview, FovPreview (variantes v3)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Presets communautaires integres deployes au premier lancement
├── MainWindow.xaml                 Shell a deux panneaux : barre laterale + editeur a onglets
├── ExportJsonDialog.xaml           Assistant d'export multi-format (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Import depuis .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Saisie des metadonnees du preset (nom, auteur, description, URL)
├── CommunityBrowserDialog.xaml     Parcourir et telecharger des presets communautaires depuis GitHub
├── NewPresetDialog.xaml            Creer / nommer de nouveaux presets
├── ShellTaskbarPropertyStore.cs    Icone de barre des taches Windows via shell property store
├── ApplicationIdentity.cs          App User Model ID partage
└── UltimateCameraMod.V3.csproj

community_presets/                  Presets camera contribues par la communaute
ucm_presets/                        Definitions des presets de style UCM officiels
```

---

## Compatibilite

- **Plateformes :** Steam, Epic Games, Xbox / Game Pass
- **Systeme d'exploitation :** Windows 10/11 (x64)
- **Affichage :** Tout rapport d'aspect -- 16:9, 21:9, 32:9

---

## FAQ

**Est-ce que je risque un bannissement ?**
UCM modifie uniquement des fichiers de donnees hors ligne. Il ne touche pas la memoire du jeu, n'injecte pas de code et n'interagit pas avec les processus en cours. Utilisez-le a votre discretion en modes en ligne/multijoueur.

**Le jeu a ete mis a jour et ma camera est revenue aux valeurs d'origine.**
Normal -- les mises a jour du jeu ecrasent les fichiers modifies. Rouvrez UCM et cliquez sur Installer (ou re-exportez le JSON pour JSON Mod Manager / CDUMM). Vos parametres sont sauvegardes automatiquement.

**Mon antivirus a signale l'exe.**
Faux positif connu avec les applications .NET autonomes. L'analyse VirusTotal est propre : [v3.2](https://www.virustotal.com/gui/file-analysis/ZWMzZGM4MGM3ZWFlZTY5MTFmZDYwYzNkODFlZGM4Mjg6MTc3NTkxMzY4Mg==). Le code source complet est disponible ici pour examen et compilation par vos soins.

**Que signifie un decalage horizontal de 0 ?**
0 = position camera d'origine (personnage legerement a gauche). 0.5 = personnage centre a l'ecran. Les valeurs negatives decalent davantage a gauche, les valeurs positives davantage a droite.

**Mise a niveau depuis une version precedente ?**
Utilisateurs v3.x : remplacez simplement l'exe, tous les presets et parametres sont preserves. Utilisateurs v2.x : supprimez l'ancien dossier UCM, verifiez les fichiers du jeu sur Steam, puis lancez v3.1 depuis un nouveau dossier. Consultez les [notes de version](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) pour les instructions detaillees.

---

## Historique des versions

- **v3.2** -- Correction des valeurs Sacred manquantes dans l'installation/les exports depuis l'onglet God Mode. Voir les [notes de version](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.2).
- **v3.1.1** -- Correction d'un faux positif de detection de sauvegarde corrompue sur des fichiers de jeu propres.
- **v3.1** -- Remplacements Sacred God Mode (modifications utilisateur definitivement protegees des reconstructions). Option Lock-on Auto-Rotate (credits a [sillib1980](https://github.com/sillib1980)). Indicateurs Sacred verts. Correction de l'installation Full Manual Control. Overlay de mise a niveau tenant compte des versions. Voir les [notes de version](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** -- Toutes les boites de dialogue converties en systeme d'overlay integre. Les remplacements God Mode persistent entre les changements d'onglets. Selection du type de preset (UCM Managed vs Full Manual Control). Catalogue des presets communautaires deplace vers le depot principal. 54 infobulles d'attributs God Mode. Corrections de plantages du jeu. Validation vanilla mise a jour pour le patch de juin 2026. Wiki de 21 pages.
- **v3.0.1** -- Refonte export-first. Editeur a trois niveaux (UCM Quick / Fine Tune / God Mode). Format de fichier `.ucmpreset`. Systeme de presets base sur fichiers. Catalogues de presets UCM et communautaires. Export multi-format. Steadycam etendu a plus de 30 etats camera. Curseur de zoom de verrouillage.
- **v2.5** -- Derniere version v2.x.
- **v2.4** -- Decalage horizontal proportionnel, decalage sur toutes les montures et competences de visee, refonte de la camera equestre, sauvegardes avec gestion de versions, apercu FoV, fenetre redimensionnable.
- **v2.3** -- Correction du decalage horizontal pour le 16:9, curseur base sur le delta, banniere complete de configuration d'installation.
- **v2.2** -- Steadycam, niveaux de zoom supplementaires, premiere personne a cheval, decalage horizontal, FoV universel, coherence de visee des competences, import XML, partage de presets, notifications de mise a jour.
- **v2.1** -- Correction des curseurs de presets personnalises qui n'ecrivaient pas sur tous les niveaux de zoom.
- **v2.0** -- Reecriture complete de Python vers C# / .NET 6 / WPF. Editeur XML avance, gestion des presets, detection automatique du jeu.
- **v1.5** -- Version Python avec interface customtkinter.

---

## Credits et remerciements

- **0xFitz** -- Developpement UCM, reglage camera, editeur avance
- **[@sillib1980](https://github.com/sillib1980)** -- Decouverte des champs camera Lock-on Auto-Rotate

### Reecriture en C# (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** -- CrimsonDesertTools -- Analyseur PAZ/PAMT en C#, chiffrement ChaCha20, compression LZ4, PaChecksum, re-empaqueteur d'archives (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** -- Implementation pure C# du chiffrement par flux ChaCha20 (BSD)
- **[MrIkso sur Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** -- Guide de re-empaquetage PAZ : alignement 16 octets, somme de controle PAMT, correctif de l'index racine PAPGT

### Version Python originale (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** -- crimson-desert-unpacker -- Outillage d'archives PAZ, recherche sur le dechiffrement
- **Maszradine** -- CDCamera -- Regles camera, systeme steadycam, presets de styles
- **manymanecki** -- CrimsonCamera -- Architecture de modification PAZ dynamique

## Soutien

Si vous trouvez cet outil utile, pensez a soutenir le developpement :

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Licence

MIT
