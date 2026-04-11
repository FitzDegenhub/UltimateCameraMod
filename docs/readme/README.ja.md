[English](../../README.md) | [한국어](README.ko.md) | **日本語** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 リリース!** Sacred God Mode オーバーライド、Lock-on Auto-Rotate トグル、全バグ修正を含みます。**[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** または **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)** からダウンロードしてください。

# Ultimate Camera Mod - Crimson Desert

Crimson Desert 用スタンドアロンカメラツールキットです。フル GUI、リアルタイムカメラプレビュー、3段階編集システム、ファイルベースのプリセット、**[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** および **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) 向け **JSON エクスポート**、ウルトラワイド HUD サポートを提供します。

<p align="center">
  <img src="../../screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert バナー" width="100%" />
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

> お困りですか? **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** でセットアップガイド、カメラ設定の解説、プリセット管理、トラブルシューティング、開発者ドキュメントをご確認ください。

---

<details>
<summary><strong>スクリーンショット (v3.x)</strong> -- クリックして展開</summary>
<br>

**UCM Quick** -- 距離、高さ、シフト、FoV、ロックオンズーム、ステディカム、リアルタイムプレビュー
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- 検索可能なボーダーカードによる精密チューニング
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** -- バニラ比較付きフル生 XML エディタ
![God Mode](../../screenshots/v3.x/godmode.png)

**JSON エクスポート** -- JSON Mod Manager / CDUMM 向けエクスポート
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**インポート** -- .ucmpreset、XML、PAZ、またはモッドマネージャーパッケージからインポート
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## ブランチ概要

| ブランチ | ステータス | 内容 |
|----------|-----------|------|
| **`main`** | v3.1.2 リリース | 3段階エディタ(UCM Quick / Fine Tune / God Mode)、ファイルベースプリセット、コミュニティカタログ、マルチフォーマットエクスポート、直接 PAZ インストールを備えたスタンドアロンカメラツールキット |
| **`development`** | 開発中 | 次バージョン開発ブランチ |

v3 には v2 の全カメラ機能に加え、再設計された UI、ファイルベースプリセット、3段階エディタ、マルチフォーマットエクスポートが含まれています。直接 PAZ インストールは v3 でもサブオプションとして利用可能です。

---

## 機能

### カメラ操作

| 機能 | 詳細 |
|------|------|
| **8 種類の組み込みプリセット** | Panoramic、Heroic、Vanilla、Close-Up、Low Rider、Knee Cam、Dirt Cam、Survival -- リアルタイムプレビュー付き |
| **カスタムカメラ** | 距離(1.5-12)、高さ(-1.6-1.5)、水平シフト(-3-3)のスライダー。比例スケーリングにより、すべてのズームレベルでキャラクターが同じ画面位置を維持します |
| **視野角** | バニラ 40 度から最大 80 度まで。ガード、エイム、騎乗、滑空、シネマティックの全状態で一貫した FoV |
| **センターカメラ** | 150 以上のカメラステートでキャラクターを画面中央に配置し、左オフセットのショルダーカムを排除 |
| **ロックオンズーム** | -60%(ターゲットにズームイン)から +60%(ワイドに引く)までのスライダー。全ロックオン、ガード、ラッシュステートに適用。ステディカムとは独立して動作 |
| **ロックオン自動回転** | ロックオン時のカメラのターゲット自動追従を無効化。背後の敵に向かってカメラが急回転するのを防ぎます。[@sillib1980](https://github.com/sillib1980) 提供 |
| **騎乗カメラ同期** | 騎乗カメラがプレイヤーカメラの高さ設定に合わせます |
| **全騎乗物での水平シフト** | 馬、象、ワイバーン、カヌー、戦争兵器、ほうき全てが比例スケーリングでシフト設定を反映 |
| **スキルエイミング一貫性** | ランタン、ブラインディングフラッシュ、弓、全エイム/ズーム/インタラクションスキルが水平シフトを反映。アビリティ発動時のカメラスナップなし |
| **ステディカムスムージング** | 30 以上のカメラステートに対する正規化ブレンドタイミングと速度スウェイ: 待機、歩行、走行、スプリント、戦闘、ガード、ラッシュ/チャージ、自由落下、スーパージャンプ、ロープ引き/スイング、ノックバック、全ロックオンバリアント、騎乗ロックオン、蘇生ロックオン、アグロ/指名手配、戦争兵器、全騎乗ステート。Fine Tune エディタで全値をコミュニティ調整可能 |
| **Sacred God Mode** | God Mode で編集した値は UCM Quick/Fine Tune リビルドから永続的に保護されます。グリーンインジケーターで Sacred 値を表示。プリセットごとに保存 |

> **v3 設計思想: 値の編集のみ、構造の注入なし。**
>
> 以前のバージョンではカメラファイルに新しい XML 行を注入していました(追加ズームレベル、馬の一人称モード、追加ズーム段階を含む馬カメラの大幅改修)。v3 ではこれらの機能を意図的に削除しました。構造の注入はゲームアップデート後に壊れる可能性がはるかに高く、ニッチなカメラモードの個人的な好みはモッドマネージャーを通じて配布される専用モッドの方が適しています。UCM は既存の値のみを変更します -- 同じ行数、同じ要素構造、同じ属性。これによりプリセットの共有がより安全になり、ゲームパッチへの耐性が高まります。

### 3段階エディタ (v3)

v3 では好きなだけ深く編集できるよう、3 つのタブで構成されています:

| 段階 | タブ | 機能 |
|------|------|------|
| 1 | **UCM Quick** | 高速レイヤー -- 距離/高さ/シフトスライダー、FoV、センターカメラ、ロックオンズーム(-60%~+60%)、ロックオン自動回転、騎乗同期、ステディカム、リアルタイムカメラ + FoV プレビュー |
| 2 | **Fine Tune** | 精密チューニング。徒歩ズームレベル、馬/騎乗ズーム、グローバル FoV、特殊騎乗物 & 移動、戦闘 & ロックオン、カメラスムージング、エイミング & クロスヘア位置の検索可能なセクション。UCM Quick の上に構築 |
| 3 | **God Mode** | フル生 XML エディタ -- カメラステート別にグループ化された検索・フィルタ可能な DataGrid で全パラメータを表示。バニラ比較カラム。リビルドから保護される Sacred オーバーライド(グリーン)。「Sacred only」フィルタ。54 属性ツールチップ |

### ファイルベースプリセットシステム (v3)

- **`.ucmpreset` ファイルフォーマット** -- UCM カメラプリセット専用の共有フォーマット。プリセットフォルダにドロップするだけで動作
- **サイドバーマネージャー** -- 折りたたみ可能なグループセクション: Game Default、UCM Presets、Community Presets、My Presets、Imported
- サイドバーから**新規作成 / 複製 / 名前変更 / 削除**
- 誤編集を防ぐプリセット**ロック** -- UCM プリセットは永久ロック、ユーザープリセットは南京錠アイコンで切り替え可能
- **トゥルーバニラプリセット** -- ゲームバックアップから変更なしでデコードした生の `playercamerapreset`。UCM Quick スライダーが実際のゲーム基準値に同期
- **.ucmpreset**、生 XML、PAZ アーカイブ、モッドマネージャーパッケージからの**インポート**。`.ucmpreset` インポートは完全な UCM スライダー制御が可能、生 XML/PAZ/モッドマネージャーインポートはスタンドアロンプリセット(God Mode 編集のみ、UCM ルール非適用)としてオリジナルモッド作者の値を保持
- **自動保存** -- ロック解除されたプリセットの変更はプリセットファイルに自動的に書き込まれます(デバウンス適用)
- 初回起動時にレガシー `.json` プリセットから `.ucmpreset` への自動マイグレーション

### プリセットカタログ (v3)

UCM から直接プリセットを閲覧・ダウンロードできます。ワンクリックダウンロード、アカウント不要。

- **UCM Presets** -- 7 種類の公式カメラスタイル(Heroic、Panoramic、Close-Up、Low Rider、Knee Cam、Dirt Cam、Survival)。定義は GitHub でホスティング、ゲームファイルと現在のカメラルールからセッション XML をローカル生成。カメラルール更新時に自動再生成
- **[コミュニティプリセット](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- メインリポジトリへのコミュニティ貢献プリセット、GitHub Actions でカタログ自動生成
- 各サイドバーグループヘッダーの**閲覧ボタン**でカタログブラウザを開く
- 各プリセットに名前、作者、説明、タグ、制作者の Nexus ページへのリンクを表示
- **アップデート検出** -- カタログに新バージョンがある場合、点滅するアップデートアイコン。クリックで My Presets への任意バックアップ付きアップデートをダウンロード
- ダウンロードしたプリセットはサイドバーに表示(デフォルトでロック -- 編集するには複製)
- 安全のための **2MB ファイルサイズ制限**と JSON バリデーション

**コミュニティとプリセットを共有しませんか?** UCM から `.ucmpreset` としてエクスポートし、以下のいずれかを選択:
- `community_presets/` フォルダにプリセットを追加する [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) を提出
- または Discord/Nexus で 0xFitz に `.ucmpreset` ファイルを送信すれば追加します

### マルチフォーマットエクスポート (v3)

**共有用エクスポート**ダイアログでセッションを 4 つの方法で出力します:

| フォーマット | 用途 |
|-------------|------|
| **JSON** (モッドマネージャー) | **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) または **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) 向けバイトパッチ + `modinfo`。UCM でエクスポートし、使用するマネージャーでインポート。受取側は UCM 不要。**準備**はライブの `playercamerapreset` エントリが UCM のバニラバックアップと一致する場合にのみ提供されます(既にカメラモッドを適用済みの場合はゲームファイルを検証してください)。 |
| **XML** | 他のツールや手動編集用の生 `playercamerapreset.xml` |
| **0.paz** | ゲームの `0010` フォルダに直接配置できるパッチ済みアーカイブ |
| **.ucmpreset** | 他の UCM ユーザー向けフル UCM プリセット |

JSON/XML にタイトル、バージョン、作者、Nexus URL、説明フィールドを含みます。`.json` 保存前にパッチリージョン数と変更バイト数を表示します。

### 使い勝手の向上

- **自動ゲーム検出** -- Steam、Epic Games、Xbox / Game Pass
- **自動バックアップ** -- 変更前にバニラバックアップ、ワンクリック復元。アップグレード時の自動クリーンアップ付きバージョン管理
- **インストール設定バナー** -- 完全なアクティブ設定を表示(FoV、距離、高さ、シフト、設定)
- **ゲームパッチ検知** -- 適用後のインストールメタデータを追跡、ゲームが更新された可能性がある場合に警告して再エクスポートを促進
- **リアルタイムカメラ + FoV プレビュー** -- 水平シフトと視野角コーン付き距離対応トップダウンビュー
- **アップデート通知** -- 起動時に GitHub リリースを確認
- **ゲームフォルダショートカット** -- ヘッダーからゲームディレクトリを開く
- **Windows タスクバー識別** -- シェルプロパティストアによる適切なアイコングループ化とタイトルバーアイコン
- **設定の永続化** -- セッション間で全選択内容を記憶
- **リサイズ可能なウィンドウ** -- セッション間でサイズを保持
- **ポータブル** -- 単一 `.exe`、インストーラー不要

### 設計思想

> **まだ誰も Crimson Desert のカメラを完璧にはできていません -- それこそがポイントです。**
>
> バニラゲームには 150 以上のカメラステートがあり、それぞれに数十のパラメータがあります。あらゆるプレイスタイルとディスプレイに対してこれら全てをチューニングできる開発者は一人もいません。UCM が存在する理由はまさにこれです -- 完璧なカメラが何かを教えるのではなく、自分で見つけて他の人と共有するためのツールを提供すること。
>
> あなたが調整するすべての設定はエクスポートして共有できます。戦闘中のカメラスナップを解消したロックオン自動回転修正は、God Mode で実験していた一人のコミュニティメンバーが発見したものです。このようなコミュニティ主導の精密チューニングこそが、このツールの存在意義です。

### プリセット共有

カメラ設定を `.ucmpreset` ファイルとしてエクスポートし、他のユーザーと共有しましょう。コミュニティカタログ、Nexus Mods、他のプレイヤーからプリセットをインポートできます。UCM は JSON([JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) および [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207) 向け)、生 XML、直接 PAZ インストールへのエクスポートにも対応しています。

---

## 仕組み

1. `playercamerapreset.xml` を含むゲームの PAZ アーカイブを検索
2. オリジナルファイルのバックアップを作成(初回のみ -- クリーンバックアップは上書きしません)
3. アーカイブエントリを復号(ChaCha20 + Jenkins ハッシュ鍵導出)
4. LZ4 で解凍
5. ユーザーの選択に基づいて XML カメラパラメータを解析・変更
6. 再圧縮、再暗号化し、変更されたエントリをアーカイブに書き戻す

DLL インジェクションなし、メモリハッキングなし、インターネット接続不要 -- 純粋なデータファイル変更です。

---

## ソースからビルド

[.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)(またはそれ以降)が必要です。Windows x64。

### v3 (推奨)

ビルド前に実行中のインスタンスを終了してください -- ファイルがロックされていると exe のコピーステップが失敗します。

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### 依存関係 (NuGet - 自動復元)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - LZ4 ブロック圧縮/解凍

---

## プロジェクト構造

```
src/UltimateCameraMod/              共有ライブラリ + v2.x WPF アプリ
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec、データモデル
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x UI
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 エクスポート優先 WPF アプリ (上記の共有コードを参照)
├── Controls/                       CameraPreview, FovPreview (v3 バリアント)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  初回起動時にデプロイされる組み込みコミュニティプリセット
├── MainWindow.xaml                 2パネルシェル: サイドバー + タブエディタ
├── ExportJsonDialog.xaml           マルチフォーマットエクスポートウィザード (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         .ucmpreset / XML / PAZ からのインポート
├── ImportMetadataDialog.xaml       プリセットメタデータ入力 (名前、作者、説明、URL)
├── CommunityBrowserDialog.xaml     GitHub からコミュニティプリセットを閲覧 & ダウンロード
├── NewPresetDialog.xaml            プリセット作成 / 命名
├── ShellTaskbarPropertyStore.cs    シェルプロパティストアによる Windows タスクバーアイコン
├── ApplicationIdentity.cs          共有 App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  コミュニティ貢献カメラプリセット
ucm_presets/                        公式 UCM スタイルプリセット定義
```

---

## 互換性

- **プラットフォーム:** Steam、Epic Games、Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **ディスプレイ:** 全アスペクト比対応 -- 16:9、21:9、32:9

---

## FAQ

**BAN されますか?**
UCM はオフラインデータファイルのみを変更します。ゲームメモリへのアクセス、コードインジェクション、実行中プロセスとのやり取りは一切行いません。オンライン/マルチプレイヤーモードでの使用は自己判断でお願いします。

**ゲームがアップデートされてカメラがバニラに戻りました。**
正常な動作です -- ゲームアップデートによりモッドファイルが上書きされます。UCM を再度開いてインストールをクリックしてください(または JSON Mod Manager / CDUMM 向けに JSON を再エクスポート)。設定は自動保存されています。

**アンチウイルスが exe を検出しました。**
自己完結型 .NET アプリで発生する既知の誤検知です。VirusTotal スキャンはクリーンです: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021)。完全なソースコードがここで公開されていますので、ご自身で確認してビルドできます。

**水平シフト 0 は何を意味しますか?**
0 = バニラカメラ位置(キャラクターがわずかに左寄り)。0.5 = キャラクターが画面中央。負の値はさらに左へ、正の値はさらに右へ移動します。

**以前のバージョンからアップグレードするには?**
v3.x ユーザー: exe を置き換えるだけで、全プリセットと設定が保持されます。v2.x ユーザー: 旧 UCM フォルダを削除し、Steam でゲームファイルを検証してから、新しいフォルダで v3.1 を実行してください。詳細は[リリースノート](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1)をご覧ください。

---

## バージョン履歴

- **v3.1.2** - God Mode タブでの Install/エクスポート時に Sacred 値が欠落する問題を修正。[リリースノート](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2)参照。
- **v3.1.1** - クリーンなゲームファイルでの誤ったバックアップ汚染検出を修正。
- **v3.1** - Sacred God Mode オーバーライド(ユーザー編集がリビルドから永続的に保護)。ロックオン自動回転トグル([sillib1980](https://github.com/sillib1980) 提供)。グリーン Sacred インジケーター。Full Manual Control インストール修正。バージョン対応アップグレードオーバーレイ。[リリースノート](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1)参照。
- **v3.0.2** - 全ダイアログをアプリ内オーバーレイシステムに変換。タブ切り替え時も God Mode オーバーライドが保持。プリセットタイプ選択(UCM Managed vs Full Manual Control)。コミュニティプリセットカタログをメインリポジトリに移行。54 の God Mode 属性ツールチップ。ゲームクラッシュ修正。2026年6月ゲームパッチに対応したバニラ検証更新。21ページ Wiki。
- **v3.0.1** - エクスポート優先再設計。3段階エディタ(UCM Quick / Fine Tune / God Mode)。`.ucmpreset` ファイルフォーマット。ファイルベースプリセットシステム。UCM およびコミュニティプリセットカタログ。マルチフォーマットエクスポート。ステディカム 30 以上のカメラステートに拡張。ロックオンズームスライダー。
- **v2.5** - 最後の v2.x リリース。
- **v2.4** - 比例水平シフト、全騎乗物とエイムアビリティへのシフト適用、馬カメラ改修、バージョン対応バックアップ、FoV プレビュー、リサイズ可能ウィンドウ。
- **v2.3** - 16:9 水平シフト修正、デルタベーススライダー、フルインストール設定バナー。
- **v2.2** - ステディカム、追加ズームレベル、馬一人称、水平シフト、ユニバーサル FoV、スキルエイミング一貫性、XML インポート、プリセット共有、アップデート通知。
- **v2.1** - カスタムプリセットスライダーが全ズームレベルに書き込まれない問題を修正。
- **v2.0** - Python から C# / .NET 6 / WPF への完全書き直し。高度な XML エディタ、プリセット管理、自動ゲーム検出。
- **v1.5** - customtkinter GUI の Python バージョン。

---

## クレジット & 謝辞

- **0xFitz** - UCM 開発、カメラチューニング、高度なエディタ
- **[@sillib1980](https://github.com/sillib1980)** - ロックオン自動回転カメラフィールドの発見

### C# 書き直し (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - C# PAZ/PAMT パーサー、ChaCha20 暗号化、LZ4 圧縮、PaChecksum、アーカイブリパッカー (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - 純粋な C# ChaCha20 ストリーム暗号実装 (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - PAZ リパッキングガイド: 16 バイトアラインメント、PAMT チェックサム、PAPGT ルートインデックスパッチング

### オリジナル Python バージョン (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - PAZ アーカイブツール、復号研究
- **Maszradine** - CDCamera - カメラルール、ステディカムシステム、スタイルプリセット
- **manymanecki** - CrimsonCamera - 動的 PAZ 変更アーキテクチャ

## サポート

このツールが役に立ったら、開発のサポートをご検討ください:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## ライセンス

MIT
