[English](README.md) | **한국어** | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | [Português (BR)](README.pt-BR.md) | [Русский](README.ru.md)

---

> **v3.1.2 출시!** Sacred God Mode 오버라이드, Lock-on Auto-Rotate 토글, 그리고 모든 버그 수정이 포함되었습니다. **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** 또는 **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**에서 다운로드하세요.

# Ultimate Camera Mod - Crimson Desert

Crimson Desert용 독립형 카메라 툴킷입니다. 완전한 GUI, 실시간 카메라 미리보기, 3단계 편집 시스템, 파일 기반 프리셋, **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** 및 **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM)용 **JSON 내보내기**, 울트라와이드 HUD 지원을 제공합니다.

<p align="center">
  <img src="screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert 배너" width="100%" />
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

> 도움이 필요하신가요? **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)**에서 설치 가이드, 카메라 설정 설명, 프리셋 관리, 문제 해결, 개발자 문서를 확인하세요.

---

<details>
<summary><strong>스크린샷 (v3.x)</strong> -- 클릭하여 펼치기</summary>
<br>

**UCM Quick** -- 거리, 높이, 시프트, FoV, 락온 줌, 스테디캠, 실시간 미리보기
![UCM Quick](screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- 검색 가능한 테두리 카드로 구성된 정밀 조정
![Fine Tune](screenshots/v3.x/finetune.png)

**God Mode** -- 바닐라 비교 기능이 있는 전체 원본 XML 편집기
![God Mode](screenshots/v3.x/godmode.png)

**JSON 내보내기** -- JSON Mod Manager / CDUMM용 내보내기
![Export JSON](screenshots/v3.x/exportjson_menu.png)

**가져오기** -- .ucmpreset, XML, PAZ 또는 모드 매니저 패키지에서 가져오기
![Import](screenshots/v3.x/import_screen.png)

</details>

---

## 브랜치 개요

| 브랜치 | 상태 | 설명 |
|--------|------|------|
| **`main`** | v3.1.2 릴리스 | 3단계 편집기(UCM Quick / Fine Tune / God Mode), 파일 기반 프리셋, 커뮤니티 카탈로그, 다중 형식 내보내기, 직접 PAZ 설치를 갖춘 독립형 카메라 툴킷 |
| **`development`** | 개발 중 | 다음 버전 개발 브랜치 |

v3에는 v2의 모든 카메라 기능이 포함되어 있으며, 재설계된 UI, 파일 기반 프리셋, 3단계 편집기, 다중 형식 내보내기가 추가되었습니다. 직접 PAZ 설치는 v3에서도 보조 옵션으로 제공됩니다.

---

## 기능

### 카메라 조작

| 기능 | 세부 사항 |
|------|----------|
| **8가지 기본 프리셋** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival -- 실시간 미리보기 지원 |
| **커스텀 카메라** | 거리(1.5-12), 높이(-1.6-1.5), 수평 시프트(-3-3)용 슬라이더. 비례 스케일링으로 모든 줌 레벨에서 캐릭터가 동일한 화면 위치를 유지합니다 |
| **시야각** | 바닐라 40도에서 최대 80도까지. 가드, 조준, 탑승, 활공, 시네마틱 상태에서 일관된 FoV |
| **중앙 카메라** | 150개 이상의 카메라 상태에서 캐릭터를 정중앙에 배치하여 좌측 오프셋 숄더 캠을 제거합니다 |
| **락온 줌** | -60%(대상에 줌인)에서 +60%(멀리 당기기)까지 슬라이더. 모든 락온, 가드, 돌진 상태에 적용됩니다. 스테디캠과 독립적으로 작동 |
| **락온 자동 회전** | 락온 시 카메라가 대상을 자동으로 따라가는 기능을 비활성화합니다. 뒤에 있는 적을 향해 카메라가 급격히 회전하는 것을 방지합니다. [@sillib1980](https://github.com/sillib1980) 제공 |
| **탑승물 카메라 동기화** | 탑승물 카메라가 플레이어 카메라 높이 설정에 맞춰집니다 |
| **모든 탑승물에 수평 시프트 적용** | 말, 코끼리, 와이번, 카누, 전쟁 무기, 빗자루 모두 비례 스케일링으로 시프트 설정을 따릅니다 |
| **스킬 에이밍 일관성** | 랜턴, 섬광탄, 활, 모든 조준/줌/상호작용 스킬이 수평 시프트를 따릅니다. 능력 활성화 시 카메라 끊김 없음 |
| **스테디캠 스무딩** | 30개 이상의 카메라 상태에 대한 정규화된 블렌드 타이밍 및 속도 스웨이: 대기, 걷기, 달리기, 전력 질주, 전투, 가드, 돌진/차지, 자유 낙하, 슈퍼 점프, 로프 당기기/스윙, 넉백, 모든 락온 변형, 탑승물 락온, 부활 락온, 어그로/현상수배, 전쟁 무기, 모든 탑승물 상태. Fine Tune 편집기를 통해 모든 값을 커뮤니티에서 조정 가능 |
| **Sacred God Mode** | God Mode에서 편집한 값은 UCM Quick/Fine Tune 재빌드로부터 영구적으로 보호됩니다. 녹색 인디케이터로 Sacred 값을 표시합니다. 프리셋별 저장 |

> **v3 설계 철학: 값 편집만, 구조 주입 없음.**
>
> 이전 버전은 카메라 파일에 새로운 XML 라인을 주입했습니다(추가 줌 레벨, 말 1인칭 모드, 추가 줌 단계가 포함된 말 카메라 개편). v3에서는 이러한 기능을 의도적으로 제거했습니다. 구조 주입은 게임 업데이트 후 깨질 가능성이 훨씬 높으며, 틈새 카메라 모드에 대한 개인적 선호는 모드 매니저를 통해 배포되는 전용 모드가 더 적합합니다. UCM은 이제 기존 값만 수정합니다 -- 동일한 라인 수, 동일한 요소 구조, 동일한 속성. 이를 통해 프리셋을 더 안전하게 공유하고 게임 패치에 더 탄력적으로 대응할 수 있습니다.

### 3단계 편집기 (v3)

v3는 원하는 만큼 깊이 편집할 수 있도록 세 개의 탭으로 구성됩니다:

| 단계 | 탭 | 기능 |
|------|-----|------|
| 1 | **UCM Quick** | 빠른 조작 레이어 -- 거리/높이/시프트 슬라이더, FoV, 중앙 카메라, 락온 줌(-60%~+60%), 락온 자동 회전, 탑승물 동기화, 스테디캠, 실시간 카메라 + FoV 미리보기 |
| 2 | **Fine Tune** | 정밀 조정. 도보 줌 레벨, 말/탑승물 줌, 글로벌 FoV, 특수 탑승물 및 이동, 전투 및 락온, 카메라 스무딩, 에이밍 및 크로스헤어 위치에 대한 검색 가능한 섹션. UCM Quick 위에 구축 |
| 3 | **God Mode** | 전체 원본 XML 편집기 -- 카메라 상태별로 그룹화된 검색 및 필터 가능한 DataGrid의 모든 파라미터. 바닐라 비교 컬럼. 재빌드로부터 보호되는 Sacred 오버라이드(녹색). "Sacred only" 필터. 54개 속성 툴팁 |

### 파일 기반 프리셋 시스템 (v3)

- **`.ucmpreset` 파일 형식** -- UCM 카메라 프리셋을 위한 전용 공유 형식. 프리셋 폴더에 넣기만 하면 바로 작동
- **사이드바 관리자** -- 접을 수 있는 그룹 섹션: Game Default, UCM Presets, Community Presets, My Presets, Imported
- 사이드바에서 **새로 만들기 / 복제 / 이름 변경 / 삭제**
- 실수로 편집되는 것을 방지하기 위한 프리셋 **잠금** -- UCM 프리셋은 영구 잠금, 사용자 프리셋은 자물쇠 아이콘으로 전환 가능
- **순정 바닐라 프리셋** -- 수정 없이 게임 백업에서 디코딩한 원본 `playercamerapreset`. UCM Quick 슬라이더가 실제 게임 기준 값에 동기화됩니다
- **.ucmpreset**, 원본 XML, PAZ 아카이브, 모드 매니저 패키지에서 **가져오기**. `.ucmpreset` 가져오기는 전체 UCM 슬라이더 제어가 가능하며, 원본 XML/PAZ/모드 매니저 가져오기는 독립형 프리셋(God Mode 편집만 가능, UCM 규칙 미적용)으로 원본 모드 작성자의 값을 보존합니다
- **자동 저장** -- 잠금 해제된 프리셋의 변경 사항이 프리셋 파일에 자동으로 기록됩니다(디바운스 적용)
- 첫 실행 시 레거시 `.json` 프리셋에서 `.ucmpreset`으로 자동 마이그레이션

### 프리셋 카탈로그 (v3)

UCM에서 직접 프리셋을 탐색하고 다운로드하세요. 원클릭 다운로드, 계정 불필요.

- **UCM Presets** -- 7가지 공식 카메라 스타일 (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). GitHub에서 정의를 호스팅하며, 사용자의 게임 파일과 현재 카메라 규칙에서 세션 XML을 로컬로 생성합니다. 카메라 규칙 업데이트 시 자동 재생성
- **[커뮤니티 프리셋](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** -- 메인 저장소에 기여된 커뮤니티 프리셋, GitHub Actions로 카탈로그 자동 생성
- 각 사이드바 그룹 헤더의 **탐색 버튼**으로 카탈로그 브라우저 열기
- 각 프리셋에 이름, 작성자, 설명, 태그, 제작자의 Nexus 페이지 링크 표시
- **업데이트 감지** -- 카탈로그에 새 버전이 있을 때 깜빡이는 업데이트 아이콘. 클릭하면 My Presets에 선택적 백업과 함께 업데이트 다운로드
- 다운로드한 프리셋은 사이드바에 표시됩니다(기본 잠금 -- 편집하려면 복제)
- 안전을 위한 **2MB 파일 크기 제한** 및 JSON 유효성 검사

**커뮤니티와 프리셋을 공유하고 싶으신가요?** UCM에서 `.ucmpreset`으로 내보낸 후 다음 중 하나를 선택하세요:
- `community_presets/` 폴더에 프리셋을 추가하는 [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) 제출
- 또는 Discord/Nexus에서 0xFitz에게 `.ucmpreset` 파일을 보내면 추가해드립니다

### 다중 형식 내보내기 (v3)

**공유용 내보내기** 대화 상자에서 세션을 네 가지 방식으로 출력합니다:

| 형식 | 용도 |
|------|------|
| **JSON** (모드 매니저) | **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) 또는 **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM)용 바이트 패치 + `modinfo`. UCM에서 내보내고 사용하는 매니저에서 가져오세요. 받는 사람은 UCM이 필요하지 않습니다. **준비**는 라이브 `playercamerapreset` 항목이 UCM의 바닐라 백업과 일치할 때만 제공됩니다(이미 카메라 모드를 적용한 경우 게임 파일을 확인하세요). |
| **XML** | 다른 도구나 수동 편집을 위한 원본 `playercamerapreset.xml` |
| **0.paz** | 게임의 `0010` 폴더에 바로 넣을 수 있는 패치된 아카이브 |
| **.ucmpreset** | 다른 UCM 사용자를 위한 전체 UCM 프리셋 |

JSON/XML에 제목, 버전, 작성자, Nexus URL, 설명 필드가 포함됩니다. `.json` 저장 전 패치 영역 수와 변경된 바이트를 표시합니다.

### 편의 기능

- **자동 게임 감지** -- Steam, Epic Games, Xbox / Game Pass
- **자동 백업** -- 수정 전 바닐라 백업, 원클릭 복원. 업그레이드 시 자동 정리가 포함된 버전 인식
- **설치 구성 배너** -- 전체 활성 구성 표시(FoV, 거리, 높이, 시프트, 설정)
- **게임 패치 인식** -- 적용 후 설치 메타데이터를 추적하며, 게임이 업데이트되었을 수 있을 때 경고하여 재내보내기 가능
- **실시간 카메라 + FoV 미리보기** -- 수평 시프트와 시야각 원뿔이 포함된 거리 인식 탑다운 뷰
- **업데이트 알림** -- 실행 시 GitHub 릴리스를 확인
- **게임 폴더 바로가기** -- 헤더에서 게임 디렉토리 열기
- **Windows 작업 표시줄 식별** -- 셸 속성 저장소를 통한 적절한 아이콘 그룹화 및 제목 표시줄 아이콘
- **설정 유지** -- 세션 간 모든 선택 사항 기억
- **크기 조절 가능 창** -- 세션 간 크기 유지
- **포터블** -- 단일 `.exe`, 설치 프로그램 불필요

### 철학

> **아직 아무도 Crimson Desert의 카메라를 완벽하게 만들지 못했습니다 -- 그것이 바로 핵심입니다.**
>
> 바닐라 게임에는 150개 이상의 카메라 상태가 있으며, 각각 수십 개의 파라미터를 가지고 있습니다. 모든 플레이스타일과 디스플레이에 맞게 이 모든 것을 조정할 수 있는 개발자는 없습니다. UCM이 존재하는 이유가 바로 이것입니다 -- 완벽한 카메라가 무엇인지 알려주는 것이 아니라, 스스로 찾고 다른 사람과 공유할 수 있는 도구를 제공하는 것입니다.
>
> 여러분이 조정하는 모든 설정은 내보내고 공유할 수 있습니다. 전투 중 카메라 끊김을 제거한 락온 자동 회전 수정은 God Mode에서 실험하던 한 커뮤니티 멤버가 발견한 것입니다. 이런 커뮤니티 주도의 정밀 조정이 바로 이 도구의 존재 이유입니다.

### 프리셋 공유

카메라 설정을 `.ucmpreset` 파일로 내보내 다른 사람과 공유하세요. 커뮤니티 카탈로그, Nexus Mods 또는 다른 플레이어로부터 프리셋을 가져올 수 있습니다. UCM은 또한 JSON([JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) 및 [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)용), 원본 XML, 직접 PAZ 설치로도 내보냅니다.

---

## 작동 원리

1. `playercamerapreset.xml`이 포함된 게임의 PAZ 아카이브를 찾습니다
2. 원본 파일의 백업을 생성합니다(최초 1회만 -- 깨끗한 백업을 덮어쓰지 않음)
3. 아카이브 항목을 복호화합니다(ChaCha20 + Jenkins 해시 키 파생)
4. LZ4로 압축 해제합니다
5. 사용자의 선택에 따라 XML 카메라 파라미터를 파싱하고 수정합니다
6. 재압축, 재암호화한 후 수정된 항목을 아카이브에 다시 기록합니다

DLL 주입 없음, 메모리 해킹 없음, 인터넷 연결 불필요 -- 순수한 데이터 파일 수정입니다.

---

## 소스에서 빌드하기

[.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)(또는 이후 버전)가 필요합니다. Windows x64.

### v3 (권장)

빌드 전 실행 중인 인스턴스를 닫으세요 -- 파일이 잠겨 있으면 exe 복사 단계가 실패합니다.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### 종속성 (NuGet - 자동 복원)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - LZ4 블록 압축/해제

---

## 프로젝트 구조

```
src/UltimateCameraMod/              공유 라이브러리 + v2.x WPF 앱
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, 데이터 모델
├── Paz/                            ArchiveWriter, CompressionUtils, PAZ I/O
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 v2.x UI
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           v3 내보내기 우선 WPF 앱 (위의 공유 코드 참조)
├── Controls/                       CameraPreview, FovPreview (v3 변형)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  첫 실행 시 배포되는 임베디드 커뮤니티 프리셋
├── MainWindow.xaml                 두 패널 셸: 사이드바 + 탭 편집기
├── ExportJsonDialog.xaml           다중 형식 내보내기 마법사 (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         .ucmpreset / XML / PAZ에서 가져오기
├── ImportMetadataDialog.xaml       프리셋 메타데이터 입력 (이름, 작성자, 설명, URL)
├── CommunityBrowserDialog.xaml     GitHub에서 커뮤니티 프리셋 탐색 및 다운로드
├── NewPresetDialog.xaml            프리셋 생성 / 이름 지정
├── ShellTaskbarPropertyStore.cs    셸 속성 저장소를 통한 Windows 작업 표시줄 아이콘
├── ApplicationIdentity.cs          공유 App User Model ID
└── UltimateCameraMod.V3.csproj

community_presets/                  커뮤니티 기여 카메라 프리셋
ucm_presets/                        공식 UCM 스타일 프리셋 정의
```

---

## 호환성

- **플랫폼:** Steam, Epic Games, Xbox / Game Pass
- **OS:** Windows 10/11 (x64)
- **디스플레이:** 모든 화면 비율 -- 16:9, 21:9, 32:9

---

## FAQ

**밴 당할 수 있나요?**
UCM은 오프라인 데이터 파일만 수정합니다. 게임 메모리를 건드리거나, 코드를 주입하거나, 실행 중인 프로세스와 상호작용하지 않습니다. 온라인/멀티플레이어 모드에서는 자기 판단에 따라 사용하세요.

**게임이 업데이트되니 카메라가 바닐라로 돌아갔습니다.**
정상입니다 -- 게임 업데이트가 모딩된 파일을 덮어씁니다. UCM을 다시 열고 설치를 클릭하세요(또는 JSON Mod Manager / CDUMM용으로 JSON을 재내보내기하세요). 설정은 자동으로 저장됩니다.

**백신이 exe를 차단했습니다.**
자체 포함 .NET 앱에서 발생하는 알려진 오탐지입니다. VirusTotal 검사 결과는 깨끗합니다: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). 전체 소스 코드가 여기에 공개되어 있으므로 직접 검토하고 빌드할 수 있습니다.

**수평 시프트 0은 무엇을 의미하나요?**
0 = 바닐라 카메라 위치(캐릭터가 약간 왼쪽). 0.5 = 캐릭터가 화면 중앙. 음수 값은 더 왼쪽으로, 양수 값은 더 오른쪽으로 이동합니다.

**이전 버전에서 업그레이드하려면?**
v3.x 사용자: exe만 교체하면 됩니다. 모든 프리셋과 설정이 유지됩니다. v2.x 사용자: 이전 UCM 폴더를 삭제하고, Steam에서 게임 파일을 확인한 후, 새 폴더에서 v3.1을 실행하세요. 자세한 안내는 [릴리스 노트](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1)를 참조하세요.

---

## 버전 히스토리

- **v3.1.2** - God Mode 탭에서 Install/내보내기 시 Sacred 값이 누락되는 문제 수정. [릴리스 노트](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2) 참조.
- **v3.1.1** - 깨끗한 게임 파일에서 발생하던 잘못된 오염 백업 감지 수정.
- **v3.1** - Sacred God Mode 오버라이드(사용자 편집이 재빌드로부터 영구 보호). 락온 자동 회전 토글([sillib1980](https://github.com/sillib1980) 제공). 녹색 Sacred 인디케이터. Full Manual Control 설치 수정. 버전 인식 업그레이드 오버레이. [릴리스 노트](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) 참조.
- **v3.0.2** - 모든 대화 상자를 인앱 오버레이 시스템으로 변환. 탭 전환 시에도 God Mode 오버라이드 유지. 프리셋 유형 선택(UCM Managed vs Full Manual Control). 커뮤니티 프리셋 카탈로그를 메인 저장소로 이전. 54개 God Mode 속성 툴팁. 게임 크래시 수정. 2026년 6월 게임 패치에 맞춘 바닐라 검증 업데이트. 21페이지 Wiki.
- **v3.0.1** - 내보내기 우선 재설계. 3단계 편집기(UCM Quick / Fine Tune / God Mode). `.ucmpreset` 파일 형식. 파일 기반 프리셋 시스템. UCM 및 커뮤니티 프리셋 카탈로그. 다중 형식 내보내기. 스테디캠 30개 이상 카메라 상태로 확장. 락온 줌 슬라이더.
- **v2.5** - 마지막 v2.x 릴리스.
- **v2.4** - 비례 수평 시프트, 모든 탑승물 및 조준 능력에 시프트 적용, 말 카메라 개편, 버전 인식 백업, FoV 미리보기, 크기 조절 가능 창.
- **v2.3** - 16:9 수평 시프트 수정, 델타 기반 슬라이더, 전체 설치 구성 배너.
- **v2.2** - 스테디캠, 추가 줌 레벨, 말 1인칭, 수평 시프트, 유니버설 FoV, 스킬 에이밍 일관성, XML 가져오기, 프리셋 공유, 업데이트 알림.
- **v2.1** - 커스텀 프리셋 슬라이더가 모든 줌 레벨에 기록되지 않는 문제 수정.
- **v2.0** - Python에서 C# / .NET 6 / WPF로 완전 재작성. 고급 XML 편집기, 프리셋 관리, 자동 게임 감지.
- **v1.5** - customtkinter GUI를 사용한 Python 버전.

---

## 크레딧 및 감사

- **0xFitz** - UCM 개발, 카메라 튜닝, 고급 편집기
- **[@sillib1980](https://github.com/sillib1980)** - 락온 자동 회전 카메라 필드 발견

### C# 재작성 (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - C# PAZ/PAMT 파서, ChaCha20 암호화, LZ4 압축, PaChecksum, 아카이브 리패커 (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - 순수 C# ChaCha20 스트림 암호 구현 (BSD)
- **[MrIkso on Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - PAZ 리패킹 가이드: 16바이트 정렬, PAMT 체크섬, PAPGT 루트 인덱스 패칭

### 오리지널 Python 버전 (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - PAZ 아카이브 도구, 복호화 연구
- **Maszradine** - CDCamera - 카메라 규칙, 스테디캠 시스템, 스타일 프리셋
- **manymanecki** - CrimsonCamera - 동적 PAZ 수정 아키텍처

## 지원

유용하다고 생각하시면 개발 지원을 고려해 주세요:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## 라이선스

MIT
