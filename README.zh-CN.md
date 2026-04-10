[English](README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | **简体中文** | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 已发布!** Sacred God Mode 覆盖、Lock-on Auto-Rotate 开关以及所有错误修复。从 **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** 或 **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)** 下载。

# Ultimate Camera Mod - Crimson Desert

Crimson Desert 独立相机工具包。提供完整 GUI、实时相机预览、三级编辑系统、基于文件的预设、面向 **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** 和 **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) 的 **JSON 导出**，以及超宽屏 HUD 支持。

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert 横幅" width="100%" />
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

> 需要帮助？请查看 **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)**，获取安装指南、相机设置说明、预设管理、故障排除和开发者文档。

---

<details>
<summary><strong>截图 (v3.x)</strong> -- 点击展开</summary>
<br>

**UCM Quick** -- 距离、高度、偏移、FoV、锁定缩放、稳定镜头、实时预览
![UCM Quick](screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- 可搜索边框卡片式精密调整
![Fine Tune](screenshots/v3.x/finetune.png)

**God Mode** -- 带原版对比的完整原始 XML 编辑器
![God Mode](screenshots/v3.x/godmode.png)

**JSON 导出** -- 导出至 JSON Mod Manager / CDUMM
![Export JSON](screenshots/v3.x/exportjson_menu.png)

**导入** -- 从 .ucmpreset、XML、PAZ 或模组管理器包导入
![Import](screenshots/v3.x/import_screen.png)

</details>

---

## 分支概览

| 分支 | 状态 | 说明 |
|------|------|------|
| **`main`** | v3.1.2 发布版 | 具有三级编辑器（UCM Quick / Fine Tune / God Mode）、基于文件的预设、社区目录、多格式导出和直接 PAZ 安装的独立相机工具包 |
| **`development`** | 开发中 | 下一版本开发分支 |

v3 包含 v2 的全部相机功能，另外新增了重新设计的 UI、基于文件的预设、三级编辑器和多格式导出。直接 PAZ 安装在 v3 中仍作为备选方案可用。

---

## 功能

### 相机控制

| 功能 | 详情 |
|------|------|
| **8 个内置预设** | Panoramic、Heroic、Vanilla、Close-Up、Low Rider、Knee Cam、Dirt Cam、Survival -- 支持实时预览 |
| **自定义相机** | 距离（1.5-12）、高度（-1.6-1.5）和水平偏移（-3-3）滑块。按比例缩放确保角色在所有缩放级别下保持相同的屏幕位置 |
| **视野角** | 从原版 40 度到最大 80 度。在防御、瞄准、骑乘、滑翔和过场状态下保持一致的 FoV |
| **居中相机** | 在 150 多个相机状态中将角色置于画面正中央，消除左偏肩部视角 |
| **锁定缩放** | 滑块范围从 -60%（拉近目标）到 +60%（拉远广角）。适用于所有锁定、防御和冲刺状态。与稳定镜头独立运作 |
| **锁定自动旋转** | 禁用锁定时相机自动跟随目标的功能。防止相机急速转向身后的敌人。由 [@sillib1980](https://github.com/sillib1980) 提供 |
| **骑乘相机同步** | 骑乘相机匹配你选择的玩家相机高度 |
| **所有坐骑水平偏移** | 马、大象、飞龙、独木舟、战争器械和扫帚全部按比例缩放遵循你的偏移设置 |
| **技能瞄准一致性** | 灯笼、致盲闪光、弓箭以及所有瞄准/缩放/交互技能都遵循水平偏移。激活能力时不会发生相机跳转 |
| **稳定镜头平滑** | 30 多个相机状态的标准化混合时序和速度摇摆：待机、行走、奔跑、冲刺、战斗、防御、突进/蓄力、自由落体、超级跳、绳索拉/摇摆、击退、所有锁定变体、骑乘锁定、复活锁定、仇恨/通缉、战争器械以及所有骑乘状态。所有值均可通过 Fine Tune 编辑器由社区调整 |
| **Sacred God Mode** | 在 God Mode 中编辑的值将被永久保护，不受 UCM Quick/Fine Tune 重建的影响。绿色指示器显示哪些值是 Sacred 值。按预设存储 |

> **v3 设计理念：仅修改值，不注入结构。**
>
> 早期版本会向相机文件注入新的 XML 行（额外缩放级别、马匹第一人称模式、带有额外缩放层级的马匹相机改造）。v3 有意移除了这些功能。结构注入在游戏更新后更容易导致问题，而小众相机模式的个人偏好更适合通过模组管理器分发的专用模组来满足。UCM 现在只修改现有值 -- 相同的行数、相同的元素结构、相同的属性。这使得预设共享更安全，对游戏补丁的适应性更强。

### 三级编辑器 (v3)

v3 将编辑分为三个标签页，让你可以随心所欲地深入调整：

| 层级 | 标签页 | 功能 |
|------|--------|------|
| 1 | **UCM Quick** | 快速操作层 -- 距离/高度/偏移滑块、FoV、居中相机、锁定缩放（-60% 到 +60%）、锁定自动旋转、骑乘同步、稳定镜头、实时相机 + FoV 预览 |
| 2 | **Fine Tune** | 精密调整。可搜索的分区涵盖步行缩放级别、马匹/坐骑缩放、全局 FoV、特殊坐骑和移动、战斗和锁定、相机平滑、瞄准和准星位置。在 UCM Quick 基础上构建 |
| 3 | **God Mode** | 完整原始 XML 编辑器 -- 按相机状态分组的可搜索、可过滤 DataGrid 中显示所有参数。原版对比列。受重建保护的 Sacred 覆盖（绿色）。"Sacred only" 过滤器。54 个属性工具提示 |

### 基于文件的预设系统 (v3)

- **`.ucmpreset` 文件格式** -- UCM 相机预设专用共享格式。放入任意预设文件夹即可使用
- **侧边栏管理器** -- 可折叠分组区域：Game Default、UCM Presets、Community Presets、My Presets、Imported
- 从侧边栏**新建 / 复制 / 重命名 / 删除**
- **锁定**预设以防止意外编辑 -- UCM 预设永久锁定；用户预设可通过锁头图标切换
- **原版基准预设** -- 从游戏备份解码的未经修改的原始 `playercamerapreset`。UCM Quick 滑块与实际游戏基准值同步
- 从 **.ucmpreset**、原始 XML、PAZ 存档或模组管理器包**导入**。`.ucmpreset` 导入可获得完整的 UCM 滑块控制；原始 XML/PAZ/模组管理器导入为独立预设（仅限 God Mode 编辑，不应用 UCM 规则）以保留原始模组作者的值
- **自动保存** -- 对未锁定预设的更改会自动写回预设文件（防抖处理）
- 首次启动时自动从旧版 `.json` 预设迁移到 `.ucmpreset`

### 预设目录 (v3)

直接在 UCM 中浏览和下载预设。一键下载，无需账户。

- **UCM Presets** -- 7 种官方相机风格（Heroic、Panoramic、Close-Up、Low Rider、Knee Cam、Dirt Cam、Survival）。定义托管在 GitHub 上，从你的游戏文件和当前相机规则在本地生成会话 XML。相机规则更新时自动重新生成
- **[社区预设](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- 主仓库中的社区贡献预设，由 GitHub Actions 自动生成目录
- 每个侧边栏分组标题的**浏览按钮**可打开目录浏览器
- 每个预设显示名称、作者、描述、标签以及创作者的 Nexus 页面链接
- **更新检测** -- 目录中有新版本时显示闪烁的更新图标。点击可下载更新，并可选备份到 My Presets
- 下载的预设显示在侧边栏中（默认锁定 -- 复制后方可编辑）
- 为安全起见设有 **2MB 文件大小限制**和 JSON 验证

**想与社区分享你的预设？** 从 UCM 导出为 `.ucmpreset`，然后选择以下方式之一：
- 提交 [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls)，将你的预设添加到 `community_presets/` 文件夹
- 或者在 Discord/Nexus 上将 `.ucmpreset` 文件发送给 0xFitz，我们会帮你添加

### 多格式导出 (v3)

**导出分享**对话框以四种方式输出你的会话：

| 格式 | 用途 |
|------|------|
| **JSON**（模组管理器） | 面向 **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) 或 **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM) 的字节补丁 + `modinfo`。在 UCM 中导出，在你使用的管理器中导入；接收方无需安装 UCM。**准备**仅在实时 `playercamerapreset` 条目与 UCM 的原版备份匹配时提供（如果已应用相机模组，请验证游戏文件）。 |
| **XML** | 用于其他工具或手动编辑的原始 `playercamerapreset.xml` |
| **0.paz** | 可直接放入游戏 `0010` 文件夹的已补丁存档 |
| **.ucmpreset** | 供其他 UCM 用户使用的完整 UCM 预设 |

JSON/XML 包含标题、版本、作者、Nexus URL 和描述字段。保存 `.json` 前显示补丁区域数和更改的字节数。

### 便捷功能

- **自动游戏检测** -- Steam、Epic Games、Xbox / Game Pass
- **自动备份** -- 修改前备份原版文件，一键恢复。升级时自动清理的版本感知功能
- **安装配置横幅** -- 显示完整的当前配置（FoV、距离、高度、偏移、设置）
- **游戏补丁感知** -- 在应用后跟踪安装元数据；当游戏可能已更新时发出警告以便重新导出
- **实时相机 + FoV 预览** -- 带水平偏移和视野角锥体的距离感知俯视图
- **更新通知** -- 启动时检查 GitHub 发布
- **游戏文件夹快捷方式** -- 从标题栏打开游戏目录
- **Windows 任务栏标识** -- 通过 Shell 属性存储实现正确的图标分组和标题栏图标
- **设置持久化** -- 所有选项在会话间保持记忆
- **可调整窗口大小** -- 窗口大小在会话间保持
- **便携式** -- 单个 `.exe`，无需安装程序

### 设计理念

> **还没有人完美调校过 Crimson Desert 的相机 -- 这正是关键所在。**
>
> 原版游戏有 150 多个相机状态，每个状态都有数十个参数。没有任何一个开发者能为所有游玩风格和显示器调校好这一切。这正是 UCM 存在的意义 -- 不是告诉你完美的相机是什么，而是给你工具让你自己去发现，并与他人分享。
>
> 你调整的每一项设置都可以导出和分享。消除战斗中相机跳转的锁定自动旋转修复，就是一位社区成员在 God Mode 中实验时发现的。这种社区驱动的精细调校正是这个工具的存在意义。

### 预设分享

将你的相机设置导出为 `.ucmpreset` 文件并与他人分享。从社区目录、Nexus Mods 或其他玩家导入预设。UCM 还支持导出为 JSON（用于 [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) 和 [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)）、原始 XML 以及直接 PAZ 安装。

---

## 工作原理

1. 定位包含 `playercamerapreset.xml` 的游戏 PAZ 存档
2. 创建原始文件的备份（仅一次 -- 不会覆盖干净的备份）
3. 解密存档条目（ChaCha20 + Jenkins 哈希密钥派生）
4. 通过 LZ4 解压缩
5. 根据用户的选择解析和修改 XML 相机参数
6. 重新压缩、重新加密，并将修改后的条目写回存档

无 DLL 注入、无内存篡改、无需网络连接 -- 纯粹的数据文件修改。

---

## 从源码构建

需要 [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)（或更高版本）。Windows x64。

### v3（推荐）

构建前请关闭所有运行中的实例 -- 文件被锁定时 exe 复制步骤会失败。

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### 依赖项（NuGet - 自动还原）

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - LZ4 块压缩/解压缩

---

## 项目结构

```
src/UltimateCameraMod/              共享库 + v2.x WPF 应用
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec、数据模型
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x UI
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 导出优先 WPF 应用（引用上述共享代码）
├── Controls/                       CameraPreview, FovPreview（v3 变体）
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  首次启动时部署的内嵌社区预设
├── MainWindow.xaml                 双面板界面：侧边栏 + 标签页编辑器
├── ExportJsonDialog.xaml           多格式导出向导（JSON, XML, 0.paz, .ucmpreset）
├── ImportPresetDialog.xaml         从 .ucmpreset / XML / PAZ 导入
├── ImportMetadataDialog.xaml       预设元数据输入（名称、作者、描述、URL）
├── CommunityBrowserDialog.xaml     从 GitHub 浏览和下载社区预设
├── NewPresetDialog.xaml            创建 / 命名预设
├── ShellTaskbarPropertyStore.cs    通过 Shell 属性存储实现 Windows 任务栏图标
├── ApplicationIdentity.cs          共享 App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  社区贡献的相机预设
ucm_presets/                        官方 UCM 风格预设定义
```

---

## 兼容性

- **平台：** Steam、Epic Games、Xbox / Game Pass
- **操作系统：** Windows 10/11 (x64)
- **显示器：** 任意宽高比 -- 16:9、21:9、32:9

---

## 常见问题

**会被封号吗？**
UCM 仅修改离线数据文件。它不会接触游戏内存、注入代码或与运行中的进程交互。在线/多人模式下请自行斟酌使用。

**游戏更新后相机恢复到原版了。**
这是正常现象 -- 游戏更新会覆盖模组文件。重新打开 UCM 并点击安装即可（或为 JSON Mod Manager / CDUMM 重新导出 JSON）。你的设置已自动保存。

**杀毒软件标记了 exe 文件。**
这是自包含 .NET 应用的已知误报。VirusTotal 扫描结果是干净的：[v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021)。完整源代码在此处公开，你可以自行审查和构建。

**水平偏移 0 是什么意思？**
0 = 原版相机位置（角色略微偏左）。0.5 = 角色居中。负值进一步向左移动，正值进一步向右移动。

**从旧版本升级？**
v3.x 用户：只需替换 exe，所有预设和设置都会保留。v2.x 用户：删除旧的 UCM 文件夹，在 Steam 中验证游戏文件，然后在新文件夹中运行 v3.1。详细说明请参阅[发布说明](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1)。

---

## 版本历史

- **v3.1.2** - 修复 God Mode 标签页中 Install/导出时 Sacred 值缺失的问题。参见[发布说明](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2)。
- **v3.1.1** - 修复在干净游戏文件上错误检测到备份污染的问题。
- **v3.1** - Sacred God Mode 覆盖（用户编辑永久受保护，不受重建影响）。锁定自动旋转开关（[sillib1980](https://github.com/sillib1980) 提供）。绿色 Sacred 指示器。Full Manual Control 安装修复。版本感知升级覆盖层。参见[发布说明](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1)。
- **v3.0.2** - 所有对话框转换为应用内覆盖系统。标签页切换时 God Mode 覆盖保持不变。预设类型选择（UCM Managed vs Full Manual Control）。社区预设目录迁移到主仓库。54 个 God Mode 属性工具提示。游戏崩溃修复。针对 2026 年 6 月游戏补丁更新原版验证。21 页 Wiki。
- **v3.0.1** - 导出优先重新设计。三级编辑器（UCM Quick / Fine Tune / God Mode）。`.ucmpreset` 文件格式。基于文件的预设系统。UCM 和社区预设目录。多格式导出。稳定镜头扩展至 30 多个相机状态。锁定缩放滑块。
- **v2.5** - 最后一个 v2.x 版本。
- **v2.4** - 比例水平偏移、所有坐骑和瞄准技能的偏移应用、马匹相机改造、版本感知备份、FoV 预览、可调整窗口大小。
- **v2.3** - 16:9 水平偏移修复、基于增量的滑块、完整安装配置横幅。
- **v2.2** - 稳定镜头、额外缩放级别、马匹第一人称、水平偏移、全局 FoV、技能瞄准一致性、XML 导入、预设共享、更新通知。
- **v2.1** - 修复自定义预设滑块未写入所有缩放级别的问题。
- **v2.0** - 从 Python 完全重写为 C# / .NET 6 / WPF。高级 XML 编辑器、预设管理、自动游戏检测。
- **v1.5** - 使用 customtkinter GUI 的 Python 版本。

---

## 致谢

- **0xFitz** - UCM 开发、相机调校、高级编辑器
- **[@sillib1980](https://github.com/sillib1980)** - 发现锁定自动旋转相机字段

### C# 重写 (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - C# PAZ/PAMT 解析器、ChaCha20 加密、LZ4 压缩、PaChecksum、存档重打包器 (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - 纯 C# ChaCha20 流密码实现 (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - PAZ 重打包指南：16 字节对齐、PAMT 校验和、PAPGT 根索引修补

### 原始 Python 版本 (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - PAZ 存档工具、解密研究
- **Maszradine** - CDCamera - 相机规则、稳定镜头系统、风格预设
- **manymanecki** - CrimsonCamera - 动态 PAZ 修改架构

## 支持

如果你觉得有用，请考虑支持开发：

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## 许可证

MIT
