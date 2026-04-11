[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.2 現已推出！** Sacred God Mode 覆寫、Lock-on Auto-Rotate 切換，以及所有錯誤修正。請從 **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** 或 **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)** 下載。

# Ultimate Camera Mod - Crimson Desert

Crimson Desert 的獨立鏡頭工具包。完整 GUI、即時鏡頭預覽、三層編輯系統、檔案式預設、**JSON 匯出功能（適用於 [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)）**和 **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)**（CDUMM），以及超寬螢幕 HUD 支援。

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

> 需要幫助？請查閱 **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)**，內含安裝指南、鏡頭設定說明、預設管理、疑難排解與開發者文件。

---

<details>
<summary><strong>截圖 (v3.x)</strong> — 點擊展開</summary>
<br>

**UCM Quick** — 距離、高度、偏移、FoV、鎖定目標縮放、穩定鏡頭、即時預覽
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** — 精選深度調校，搭配可搜尋的框線卡片
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** — 完整原始 XML 編輯器，附原版對比功能
![God Mode](../../screenshots/v3.x/godmode.png)

**JSON 匯出** — 匯出至 JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**匯入** — 從 .ucmpreset、XML、PAZ 或 Mod Manager 套件匯入
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## 分支概覽

| 分支 | 狀態 | 說明 |
|------|------|------|
| **`main`** | v3.2 正式版 | 獨立鏡頭工具包，三層編輯器（UCM Quick / Fine Tune / God Mode）、檔案式預設、社群目錄、多格式匯出、直接 PAZ 安裝 |
| **`development`** | 開發中 | 下一版本開發分支 |

v3 包含 v2 的所有鏡頭功能，並加入重新設計的 UI、檔案式預設、三層編輯器與多格式匯出。直接 PAZ 安裝在 v3 中仍作為備選方案提供。

---

## 功能特色

### 鏡頭控制

| 功能 | 詳情 |
|------|------|
| **8 組內建預設** | Panoramic、Heroic、Vanilla、Close-Up、Low Rider、Knee Cam、Dirt Cam、Survival — 附即時預覽 |
| **自訂鏡頭** | 距離（1.5-12）、高度（-1.6-1.5）、水平偏移（-3-3）的滑桿。等比縮放確保角色在所有縮放等級中保持相同螢幕位置 |
| **視野角度** | 原版 40° 最高可達 80°。全場景 FoV 一致性，涵蓋防禦、瞄準、騎乘、滑翔及過場狀態 |
| **置中鏡頭** | 角色在超過 150 種鏡頭狀態下保持正中央，消除左偏的肩上鏡頭 |
| **鎖定目標縮放** | 滑桿範圍 -60%（拉近目標）到 +60%（拉遠廣角）。影響所有鎖定、防禦和衝鋒狀態。獨立於穩定鏡頭運作 |
| **鎖定目標自動旋轉** | 停用鎖定目標時的鏡頭自動對準功能。防止鏡頭在鎖定身後敵人時劇烈旋轉。感謝 [@sillib1980](https://github.com/sillib1980) |
| **騎乘鏡頭同步** | 騎乘鏡頭會匹配您選擇的玩家鏡頭高度 |
| **所有坐騎水平偏移** | 馬匹、大象、飛龍、獨木舟、戰爭機器和掃帚全部遵循您的偏移設定，並進行等比縮放 |
| **技能瞄準一致性** | 提燈、致盲閃光、弓箭及所有瞄準/縮放/互動技能都遵循水平偏移。啟用技能時不會發生鏡頭跳動 |
| **穩定鏡頭平滑** | 在 30 種以上鏡頭狀態中統一混合時間和速度搖擺：待機、步行、奔跑、衝刺、戰鬥、防禦、衝鋒、自由落體、超級跳躍、繩索拉拽/擺盪、擊退、所有鎖定變體、騎乘鎖定、復活鎖定、仇恨/通緝、戰爭機器及所有騎乘狀態。每個數值都可透過 Fine Tune 編輯器由社群自行調整 |
| **Sacred God Mode** | 您在 God Mode 中編輯的數值將永久受到保護，不受 Quick/Fine Tune 重建影響。綠色指示器顯示哪些數值為神聖值。各預設獨立儲存 |

> **v3 設計理念：僅修改數值，不注入結構。**
>
> 早期版本會在鏡頭檔案中注入新的 XML 行（額外縮放等級、馬匹第一人稱模式、附加縮放層級的馬匹鏡頭大改）。v3 刻意移除了這些功能。注入結構在遊戲更新後更容易導致問題，而且針對特殊鏡頭模式的個人偏好，最好由專門的模組透過模組管理器發布。UCM 現在僅修改既有數值 — 相同的行數、相同的元素結構、相同的屬性。這使得預設更安全地分享，並在遊戲更新中更具韌性。

### 三層編輯器 (v3)

v3 將編輯分為三個分頁，讓您可以依需求深入調整：

| 層級 | 分頁 | 功能說明 |
|------|------|----------|
| 1 | **UCM Quick** | 快速層 — 距離/高度/偏移滑桿、FoV、置中鏡頭、鎖定目標縮放（-60% 至 +60%）、鎖定目標自動旋轉、騎乘同步、穩定鏡頭、即時鏡頭 + FoV 預覽 |
| 2 | **Fine Tune** | 精選深度調校。可搜尋的分區，涵蓋步行縮放等級、馬匹/騎乘縮放、全域 FoV、特殊坐騎與移動、戰鬥與鎖定、鏡頭平滑、瞄準與準星位置。建構於 UCM Quick 之上 |
| 3 | **God Mode** | 完整原始 XML 編輯器 — 所有參數在可搜尋、可篩選的 DataGrid 中，按鏡頭狀態分組。原版對比欄。受保護的神聖覆寫（綠色），不受重建影響。「僅神聖值」篩選器。54 個屬性工具提示 |

### 檔案式預設系統 (v3)

- **`.ucmpreset` 檔案格式** — 專屬的可分享 UCM 鏡頭預設格式。放入任何預設資料夾即可直接使用
- **側邊欄管理器**，附可摺疊的分組區塊：遊戲預設、UCM 預設、社群預設、我的預設、匯入的預設
- 側邊欄中可**新增 / 複製 / 重新命名 / 刪除**
- **鎖定**預設以防意外編輯 — UCM 預設永久鎖定；使用者預設可透過鎖頭圖示切換
- **真實原版預設** — 從您的遊戲備份解碼的原始 `playercamerapreset`，未套用任何修改。Quick 滑桿同步至實際遊戲基準值
- **匯入**來源支援 `.ucmpreset`、原始 XML、PAZ 檔案或 Mod Manager 套件。`.ucmpreset` 匯入可獲得完整 UCM 滑桿控制；原始 XML/PAZ/Mod Manager 匯入為獨立預設（僅限 God Mode 編輯，不套用 UCM 規則），以保留原始模組作者的數值
- **自動儲存** — 解鎖預設的變更會自動寫回預設檔案（含防抖動機制）
- 首次啟動時自動從舊版 `.json` 預設遷移至 `.ucmpreset`

### 預設目錄 (v3)

直接在 UCM 中瀏覽和下載預設。一鍵下載，無需帳號。

- **UCM 預設** — 7 種官方鏡頭風格（Heroic、Panoramic、Close-Up、Low Rider、Knee Cam、Dirt Cam、Survival）。定義託管於 GitHub，工作階段 XML 從您的遊戲檔案 + 目前鏡頭規則在本機烘焙。鏡頭規則更新時自動重新烘焙
- **[社群預設](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** — 社群貢獻的預設位於主倉庫中，目錄由 GitHub Actions 自動產生
- 每個側邊欄群組標題上的**瀏覽按鈕**可開啟目錄瀏覽器
- 每個預設顯示名稱、作者、描述、標籤，以及創作者的 Nexus 頁面連結
- **更新偵測** — 目錄中有新版本時會顯示脈動的更新圖示。點擊即可下載更新，並可選擇備份至「我的預設」
- 下載的預設會出現在側邊欄（預設為鎖定 — 複製後即可編輯）
- **2MB 檔案大小限制**及 JSON 驗證以確保安全

**想與社群分享您的預設嗎？** 從 UCM 匯出為 `.ucmpreset`，然後：
- 提交 [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) 將您的預設加入 `community_presets/` 資料夾
- 或將您的 `.ucmpreset` 檔案傳送給 Discord/Nexus 上的 0xFitz，我們會為您加入

### 多格式匯出 (v3)

**匯出分享**對話框以四種方式輸出您的工作階段：

| 格式 | 用途 |
|------|------|
| **JSON**（模組管理器） | 位元組修補 + `modinfo`，適用於 **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)**（PhorgeForge）或 **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)**（CDUMM）。在 UCM 中匯出，再於您使用的管理器中匯入；接收者不需要 UCM。**準備**功能僅在即時 `playercamerapreset` 條目仍與 UCM 的原版備份一致時提供（如果您已套用鏡頭模組，請先驗證遊戲檔案）。 |
| **XML** | 原始 `playercamerapreset.xml`，適用於其他工具或手動編輯 |
| **0.paz** | 已修補的檔案，可直接放入遊戲的 `0010` 資料夾 |
| **.ucmpreset** | 完整 UCM 預設，供其他 UCM 使用者使用 |

包含標題、版本、作者、Nexus 網址及描述欄位（適用於 JSON/XML）。儲存 `.json` 前會顯示修補區域數量和已變更的位元組。

### 便利功能

- **自動偵測遊戲** — Steam、Epic Games、Xbox / Game Pass
- **自動備份** — 任何修改前先備份原版；一鍵還原。具版本感知功能，升級時自動清理
- **安裝設定橫幅** — 顯示您完整的啟用設定（FoV、距離、高度、偏移、設定值）
- **遊戲更新感知** — 套用後追蹤安裝中繼資料；當遊戲可能已更新時發出警告，方便您重新匯出
- **即時鏡頭 + FoV 預覽** — 具距離感知的俯視圖，附水平偏移和視野角度錐形
- **更新通知** — 啟動時檢查 GitHub releases
- **遊戲資料夾捷徑** — 從標題列開啟您的遊戲目錄
- **Windows 工作列識別** — 透過 shell 屬性存放區實現正確的圖示分組和標題列圖示
- **設定持續保存** — 所有選擇在工作階段間保留
- **可調整視窗大小** — 大小在工作階段間保留
- **可攜式** — 單一 `.exe`，無需安裝程式

### 設計理念

> **沒有人完美地調校過 Crimson Desert 的鏡頭 -- 而這正是重點。**
>
> 原版遊戲有超過 150 種鏡頭狀態，每種都有數十個參數。沒有單一開發者能為每種遊玩風格和顯示器調校所有設定。這就是 UCM 存在的原因 -- 不是告訴您完美的鏡頭是什麼，而是給您工具讓您自己找到它，並與他人分享。
>
> 您調整的每個設定都可以匯出和分享。消除戰鬥中鏡頭跳動的鎖定目標自動旋轉修正，就是由一位社群成員在 God Mode 中實驗時發現的。這種社群驅動的微調正是這個工具的存在意義。

### 預設分享

將您的鏡頭設定匯出為 `.ucmpreset` 檔案，與他人分享。從社群目錄、Nexus Mods 或其他玩家匯入預設。UCM 也支援匯出為 JSON（適用於 [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) 和 [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)）、原始 XML 及直接 PAZ 安裝。

---

## 運作原理

1. 定位包含 `playercamerapreset.xml` 的遊戲 PAZ 檔案
2. 建立原始檔案的備份（僅一次 — 絕不覆寫乾淨的備份）
3. 解密檔案條目（ChaCha20 + Jenkins 雜湊金鑰衍生）
4. 透過 LZ4 解壓縮
5. 根據您的選擇解析並修改 XML 鏡頭參數
6. 重新壓縮、重新加密，並將修改後的條目寫回檔案

無 DLL 注入、無記憶體修改、無需網路連線 -- 純粹的資料檔案修改。

---

## 從原始碼建置

需要 [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)（或更新版本）。Windows x64。

### v3（建議使用）

建置前請關閉所有執行中的實例 — 如果檔案被鎖定，exe 複製步驟會失敗。

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### 依賴套件（NuGet — 自動還原）

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) — LZ4 區塊壓縮/解壓縮

---

## 專案結構

```
src/UltimateCameraMod/              共用函式庫 + v2.x WPF 應用程式
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, 資料模型
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x UI
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 匯出優先的 WPF 應用程式（引用上方共用程式碼）
├── Controls/                       CameraPreview, FovPreview (v3 版本)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  首次啟動時部署的內建社群預設
├── MainWindow.xaml                 雙面板外殼：側邊欄 + 分頁編輯器
├── ExportJsonDialog.xaml           多格式匯出精靈（JSON, XML, 0.paz, .ucmpreset）
├── ImportPresetDialog.xaml         從 .ucmpreset / XML / PAZ 匯入
├── ImportMetadataDialog.xaml       預設中繼資料輸入（名稱、作者、描述、網址）
├── CommunityBrowserDialog.xaml     從 GitHub 瀏覽及下載社群預設
├── NewPresetDialog.xaml            建立 / 命名新預設
├── ShellTaskbarPropertyStore.cs    透過 shell 屬性存放區設定 Windows 工作列圖示
├── ApplicationIdentity.cs          共用 App User Model ID
└── UltimateCameraMod.V3.csproj


community_presets/                  社群貢獻的鏡頭預設
ucm_presets/                        官方 UCM 風格預設定義
```

---

## 相容性

- **平台：** Steam、Epic Games、Xbox / Game Pass
- **作業系統：** Windows 10/11 (x64)
- **顯示器：** 任何螢幕比例 — 16:9、21:9、32:9

---

## 常見問題

**會被封號嗎？**
UCM 僅修改離線資料檔案。不會接觸遊戲記憶體、注入程式碼或與執行中的程序互動。在線上/多人模式中請自行斟酌使用。

**遊戲更新後鏡頭恢復為原版了。**
這是正常現象 — 遊戲更新會覆蓋被修改的檔案。重新開啟 UCM 並點擊安裝（或重新匯出 JSON 以供 JSON Mod Manager / CDUMM 使用）。您的設定會自動儲存。

**防毒軟體標記了 exe。**
這是自包含 .NET 應用程式的已知誤報。VirusTotal 掃描結果為乾淨：[v3.2](https://www.virustotal.com/gui/file-analysis/ZWMzZGM4MGM3ZWFlZTY5MTFmZDYwYzNkODFlZGM4Mjg6MTc3NTkxMzY4Mg==)。完整原始碼可在此查閱並自行建置。

**水平偏移 0 是什麼意思？**
0 = 原版鏡頭位置（角色略偏左方）。0.5 = 角色置中於螢幕。負值繼續向左偏移，正值向右偏移。

**從舊版本升級？**
v3.x 使用者：只需替換 exe，所有預設和設定都會保留。v2.x 使用者：刪除舊的 UCM 資料夾，在 Steam 中驗證遊戲檔案，然後從新資料夾執行 v3.1。詳細說明請參閱[版本說明](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1)。

---

## 版本歷史

- **v3.2** — 修正 Sacred 值在 God Mode 分頁中從安裝/匯出遺失的問題。參閱[版本說明](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.2)。
- **v3.1.1** — 修正在乾淨遊戲檔案上的備份污染誤判。
- **v3.1** — Sacred God Mode 覆寫（使用者編輯永久受保護，不受重建影響）。Lock-on Auto-Rotate 切換（感謝 [sillib1980](https://github.com/sillib1980)）。綠色神聖值指示器。Full Manual Control 安裝修正。具版本感知的升級覆蓋層。參閱[版本說明](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1)。
- **v3.0.2** — 所有對話框轉換為應用程式內覆蓋系統。God Mode 覆寫在分頁切換間持續保存。預設類型選擇（UCM 託管 vs Full Manual Control）。社群預設目錄移至主倉庫。54 個 God Mode 屬性工具提示。遊戲當機修正。原版驗證更新以適應 2026 年 6 月遊戲更新。21 頁 Wiki。
- **v3.0.1** — 匯出優先重新設計。三層編輯器（UCM Quick / Fine Tune / God Mode）。`.ucmpreset` 檔案格式。檔案式預設系統。UCM 及社群預設目錄。多格式匯出。穩定鏡頭擴展至 30 種以上鏡頭狀態。鎖定目標縮放滑桿。
- **v2.5** — 最後一個 v2.x 版本。
- **v2.4** — 等比水平偏移、所有坐騎和瞄準技能的偏移、馬匹鏡頭大改、具版本感知的備份、FoV 預覽、可調整視窗大小。
- **v2.3** — 16:9 水平偏移修正、增量式滑桿、完整安裝設定橫幅。
- **v2.2** — 穩定鏡頭、額外縮放等級、馬匹第一人稱、水平偏移、通用 FoV、技能瞄準一致性、匯入 XML、預設分享、更新通知。
- **v2.1** — 修正自訂預設滑桿未寫入所有縮放等級的問題。
- **v2.0** — 從 Python 完全重寫為 C# / .NET 6 / WPF。進階 XML 編輯器、預設管理、自動偵測遊戲。
- **v1.5** — 使用 customtkinter GUI 的 Python 版本。

---

## 致謝

- **0xFitz** — UCM 開發、鏡頭調校、進階編輯器
- **[@sillib1980](https://github.com/sillib1980)** — 發現 Lock-on Auto-Rotate 鏡頭欄位

### C# 重寫 (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** — CrimsonDesertTools — C# PAZ/PAMT 解析器、ChaCha20 加密、LZ4 壓縮、PaChecksum、檔案重新打包（.NET 8, MIT）
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** — 純 C# ChaCha20 串流加密實作（BSD）
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** — PAZ 重新打包指南：16 位元組對齊、PAMT 校驗和、PAPGT 根索引修補

### 原始 Python 版本 (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** — crimson-desert-unpacker — PAZ 檔案工具、解密研究
- **Maszradine** — CDCamera — 鏡頭規則、穩定鏡頭系統、風格預設
- **manymanecki** — CrimsonCamera — 動態 PAZ 修改架構

## 支持

如果您覺得有幫助，歡迎支持開發：

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## 授權條款

MIT
