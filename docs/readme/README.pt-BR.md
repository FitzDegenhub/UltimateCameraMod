[English](../../README.md) | [한국어](README.ko.md) | [日本語](README.ja.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-Hant.md) | [ไทย](README.th.md) | [Bahasa Indonesia](README.id.md) | [Türkçe](README.tr.md) | [Polski](README.pl.md) | [Italiano](README.it.md) | [Svenska](README.sv.md) | [Norsk](README.nb.md) | [Dansk](README.da.md) | [Suomi](README.fi.md) | [Deutsch](README.de.md) | [Français](README.fr.md) | [Español](README.es.md) | **Português (BR)** | [Русский](README.ru.md)

---

> **v3.1.2 ja esta disponivel!** Sobrescritas do Sacred God Mode, opcao de desativar a rotacao automatica do lock-on e todas as correcoes de bugs. Baixe em **[GitHub Releases](https://github.com/FitzDegenhub/UltimateCameraMod/releases/latest)** ou **[Nexus Mods](https://www.nexusmods.com/crimsondesert/mods/438)**.

# Ultimate Camera Mod - Crimson Desert

Kit de ferramentas de camera independente para Crimson Desert. Interface grafica completa, pre-visualizacao ao vivo, tres niveis de edicao, presets baseados em arquivos, **exportacao JSON para [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** e **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM), e suporte de HUD para ultrawide.

<p align="center">
  <img src="../../screenshots/banner.png" alt="Ultimate Camera Mod - Crimson Desert banner" width="100%" />
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

> Precisa de ajuda? Consulte a **[Wiki](https://github.com/FitzDegenhub/UltimateCameraMod/wiki)** para guias de configuracao, explicacao dos ajustes de camera, gerenciamento de presets, solucao de problemas e documentacao para desenvolvedores.

---

<details>
<summary><strong>Capturas de tela (v3.x)</strong> -- clique para expandir</summary>
<br>

**UCM Quick** -- distancia, altura, deslocamento, FoV, zoom de lock-on, steadycam, pre-visualizacoes ao vivo
![UCM Quick](../../screenshots/v3.x/ucm_quick.png)

**Fine Tune** -- ajuste fino com cartoes com bordas e busca
![Fine Tune](../../screenshots/v3.x/finetune.png)

**God Mode** -- editor XML completo com comparacao vanilla
![God Mode](../../screenshots/v3.x/godmode.png)

**Exportar JSON** -- exportacao para JSON Mod Manager / CDUMM
![Export JSON](../../screenshots/v3.x/exportjson_menu.png)

**Importar** -- importar de .ucmpreset, XML, PAZ ou pacotes de gerenciadores de mods
![Import](../../screenshots/v3.x/import_screen.png)

</details>

---

## Visao geral das branches

| Branch | Status | O que e |
|--------|--------|---------|
| **`main`** | v3.1.2 Release | Kit de ferramentas de camera independente com editor de tres niveis (UCM Quick / Fine Tune / God Mode), presets baseados em arquivos, catalogo comunitario, exportacao multiformato e instalacao direta de PAZ |
| **`development`** | Desenvolvimento | Branch de desenvolvimento da proxima versao |

v3 inclui todos os recursos de camera do v2 mais uma interface redesenhada, presets baseados em arquivos, um editor de tres niveis e exportacao multiformato. A instalacao direta de PAZ continua disponivel no v3 como opcao secundaria.

---

## Recursos

### Controles de camera

| Recurso | Detalhes |
|---------|----------|
| **8 presets integrados** | Panoramic, Heroic, Vanilla, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival - com pre-visualizacao ao vivo |
| **Camera personalizada** | Controles deslizantes para distancia (1.5-12), altura (-1.6-1.5) e deslocamento horizontal (-3-3). O escalonamento proporcional mantem o personagem na mesma posicao na tela em todos os niveis de zoom |
| **Campo de visao** | Vanilla 40 graus ate 80 graus. Consistencia universal do FoV nos estados de defesa, mira, montaria, planar e cinematicas |
| **Camera centralizada** | Personagem centralizado em mais de 150 estados de camera, eliminando o deslocamento do ombro para a esquerda |
| **Zoom de lock-on** | Controle deslizante de -60% (aproximar do alvo) a +60% (afastar). Afeta todos os estados de lock-on, defesa e investida. Funciona independentemente do Steadycam |
| **Rotacao automatica de lock-on** | Desativa o giro automatico da camera para o alvo ao fixar. Impede que a camera gire bruscamente para inimigos atras de voce. Creditos para [@sillib1980](https://github.com/sillib1980) |
| **Sincronizacao de camera na montaria** | As cameras de montaria correspondem a altura de camera escolhida para o jogador |
| **Deslocamento horizontal em todas as montarias** | Cavalo, elefante, wyvern, canoa, maquina de guerra e vassoura respeitam sua configuracao de deslocamento com escalonamento proporcional |
| **Consistencia na mira de habilidades** | Lanterna, Flash Cegante, Arco e todas as habilidades de mira/zoom/interacao respeitam o deslocamento horizontal. Sem saltos de camera ao ativar habilidades |
| **Suavizacao Steadycam** | Tempos de mesclagem e oscilacao de velocidade normalizados em mais de 30 estados de camera: parado, andando, correndo, sprint, combate, defesa, investida, queda livre, super pulo, puxar/balanco de corda, recuo, todas as variantes de lock-on, lock-on em montaria, lock-on de reanimacao, agressao/procurado, maquina de guerra e todos os estados de montaria. Cada valor e ajustavel pela comunidade atraves do editor Fine Tune |
| **Sacred God Mode** | Os valores que voce edita no God Mode ficam permanentemente protegidos das reconstrucoes do Quick/Fine Tune. Indicadores verdes mostram quais valores sao sagrados. Armazenamento por preset |

> **Filosofia de design do v3: apenas edicao de valores, sem injecao estrutural.**
>
> As versoes anteriores injetavam novas linhas XML no arquivo de camera (niveis de zoom extras, modo primeira pessoa no cavalo, reformulacao da camera do cavalo com niveis de zoom adicionais). O v3 remove esses recursos intencionalmente. A injecao de estrutura tem uma probabilidade muito maior de quebrar apos atualizacoes do jogo, e as preferencias pessoais para modos de camera especializados sao melhor atendidas por mods dedicados distribuidos atraves de gerenciadores de mods. O UCM agora modifica apenas valores existentes - a mesma contagem de linhas, a mesma estrutura de elementos, os mesmos atributos. Isso torna os presets mais seguros para compartilhar e mais resistentes a patches do jogo.

### Editor de tres niveis (v3)

O v3 organiza a edicao em tres abas para que voce possa aprofundar o quanto quiser:

| Nivel | Aba | O que faz |
|-------|-----|-----------|
| 1 | **UCM Quick** | A camada rapida: controles de distancia/altura/deslocamento, FoV, camera centralizada, zoom de lock-on (-60% a +60%), rotacao automatica de lock-on, sincronizacao de montaria, steadycam, pre-visualizacoes de camera e FoV ao vivo |
| 2 | **Fine Tune** | Ajuste fino. Secoes com busca para niveis de zoom a pe, zoom de cavalo/montaria, FoV global, montarias especiais e travessia, combate e lock-on, suavizacao de camera, e posicao de mira e reticulado. Construido sobre o UCM Quick |
| 3 | **God Mode** | Editor XML completo: cada parametro em um DataGrid com busca e filtros, agrupado por estado de camera. Coluna de comparacao vanilla. Sobrescritas sagradas (verde) protegidas de reconstrucoes. Filtro "Apenas sagrados". 54 tooltips de atributos |

### Sistema de presets baseado em arquivos (v3)

- **Formato de arquivo `.ucmpreset`** - formato compartilhavel dedicado para presets de camera do UCM. Arraste para qualquer pasta de presets e funciona diretamente
- **Gerenciador na barra lateral** com secoes agrupadas e recolhiveis: Game Default, UCM Presets, Community Presets, My Presets, Imported
- **Novo / Duplicar / Renomear / Excluir** pela barra lateral
- **Bloqueio** de presets para evitar edicoes acidentais: os presets do UCM sao permanentemente bloqueados; os presets do usuario podem ser alternados pelo icone de cadeado
- **Preset True Vanilla** - `playercamerapreset` decodificado diretamente do seu backup do jogo sem modificacoes aplicadas. Os controles do Quick sao sincronizados com os valores reais de referencia do jogo
- **Importar** de `.ucmpreset`, XML, arquivos PAZ ou pacotes de gerenciadores de mods. As importacoes de `.ucmpreset` obtem controle total dos controles deslizantes do UCM; as importacoes de XML/PAZ/gerenciador de mods sao presets independentes (apenas edicao no God Mode, sem regras do UCM aplicadas) para preservar os valores do autor original do mod
- **Salvamento automatico** - as alteracoes em presets desbloqueados sao gravadas automaticamente no arquivo do preset (com atraso)
- Migracao automatica de presets legados `.json` para `.ucmpreset` na primeira execucao

### Catalogos de presets (v3)

Explore e baixe presets diretamente do UCM. Download com um clique, sem necessidade de contas.

- **UCM Presets** - 7 estilos de camera oficiais (Heroic, Panoramic, Close-Up, Low Rider, Knee Cam, Dirt Cam, Survival). Definicoes hospedadas no GitHub, XML de sessao gerado localmente a partir dos seus arquivos do jogo e das regras de camera atuais. Regenerado automaticamente quando as regras de camera sao atualizadas
- **[Community presets](https://github.com/FitzDegenhub/UltimateCameraMod/tree/main/community_presets)** - presets contribuidos pela comunidade no repositorio principal, catalogo gerado automaticamente pelo GitHub Actions
- **Botao Explorar** em cada cabecalho de grupo da barra lateral abre o navegador do catalogo
- Cada preset mostra nome, autor, descricao, tags e um link para a pagina do criador no Nexus
- **Deteccao de atualizacoes** - icone de atualizacao pulsante quando uma versao mais recente esta disponivel no catalogo. Clique para baixar a atualizacao com backup opcional em My Presets
- Os presets baixados aparecem na barra lateral (bloqueados por padrao - duplique para editar)
- **Limite de tamanho de 2MB** e validacao JSON por seguranca

**Quer compartilhar seu preset com a comunidade?** Exporte como `.ucmpreset` do UCM e entao:
- Envie um [Pull Request](https://github.com/FitzDegenhub/UltimateCameraMod/pulls) adicionando seu preset a pasta `community_presets/`
- Ou envie seu arquivo `.ucmpreset` para 0xFitz no Discord/Nexus e nos o adicionaremos para voce

### Exportacao multiformato (v3)

O dialogo **Exportar para compartilhar** gera sua sessao em quatro formatos:

| Formato | Caso de uso |
|---------|-------------|
| **JSON** (gerenciadores de mods) | Patches de bytes + `modinfo` para **[JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113)** (PhorgeForge) ou **[Crimson Desert Ultimate Mods Manager](https://www.nexusmods.com/crimsondesert/mods/207)** (CDUMM). Exporte no UCM, importe no gerenciador que voce usa; os destinatarios nao precisam do UCM. **Preparar** so e oferecido quando a entrada `playercamerapreset` ao vivo ainda corresponde ao backup vanilla do UCM (verifique os arquivos do jogo se voce ja aplicou mods de camera). |
| **XML** | `playercamerapreset.xml` bruto para outras ferramentas ou edicao manual |
| **0.paz** | Arquivo patcheado pronto para colocar na pasta `0010` do jogo |
| **.ucmpreset** | Preset completo do UCM para outros usuarios do UCM |

Inclui campos de titulo, versao, autor, URL do Nexus e descricao para JSON/XML. Mostra a contagem de regioes patcheadas e bytes alterados antes de salvar o `.json`.

### Qualidade de vida

- **Deteccao automatica do jogo** - Steam, Epic Games, Xbox / Game Pass
- **Backup automatico** - backup vanilla antes de qualquer modificacao; restauracao com um clique. Compativel com versoes e limpeza automatica ao atualizar
- **Banner de configuracao de instalacao** - mostra sua configuracao ativa completa (FoV, distancia, altura, deslocamento, ajustes)
- **Deteccao de patches do jogo** - rastreia os metadados de instalacao apos aplicar; avisa quando o jogo pode ter sido atualizado para que voce possa re-exportar
- **Pre-visualizacao ao vivo de camera e FoV** - vista superior com reconhecimento de distancia, deslocamento horizontal e cone de campo de visao
- **Notificacoes de atualizacao** - verifica as versoes do GitHub ao iniciar
- **Atalho para a pasta do jogo** - abre o diretorio do jogo pelo cabecalho
- **Identidade na barra de tarefas do Windows** - agrupamento correto de icones e icone na barra de titulo via shell property store
- **Persistencia de configuracao** - todas as selecoes sao lembradas entre sessoes
- **Janela redimensionavel** - o tamanho persiste entre sessoes
- **Portatil** - um unico `.exe`, sem necessidade de instalador

### Filosofia

> **Ninguem aperfeicoou a camera do Crimson Desert ainda -- e esse e o ponto.**
>
> O jogo vanilla tem mais de 150 estados de camera, cada um com dezenas de parametros. Nenhum desenvolvedor individual consegue ajustar tudo isso para cada estilo de jogo e tela. E por isso que o UCM existe -- nao para dizer qual e a camera perfeita, mas para dar a voce as ferramentas para encontra-la por conta propria e compartilha-la com outros.
>
> Cada ajuste que voce modifica pode ser exportado e compartilhado. A correcao de rotacao automatica do lock-on que eliminou os saltos de camera durante o combate foi descoberta por um unico membro da comunidade experimentando no God Mode. Esse tipo de ajuste fino impulsionado pela comunidade e exatamente para isso que esta ferramenta foi criada.

### Compartilhar presets

Exporte sua configuracao de camera como arquivo `.ucmpreset` e compartilhe com outros. Importe presets do catalogo comunitario, Nexus Mods ou outros jogadores. O UCM tambem exporta para JSON (para [JSON Mod Manager](https://www.nexusmods.com/crimsondesert/mods/113) e [CDUMM](https://www.nexusmods.com/crimsondesert/mods/207)), XML bruto e instalacao direta de PAZ.

---

## Como funciona

1. Localiza o arquivo PAZ do jogo que contem `playercamerapreset.xml`
2. Cria um backup do arquivo original (apenas uma vez - nunca sobrescreve um backup limpo)
3. Descriptografa a entrada do arquivo (ChaCha20 + derivacao de chave Jenkins hash)
4. Descomprime via LZ4
5. Analisa e modifica os parametros XML da camera com base nas suas selecoes
6. Re-comprime, re-criptografa e grava a entrada modificada de volta no arquivo

Sem injecao de DLL, sem modificacao de memoria, sem conexao com a internet necessaria -- modificacao pura de arquivos de dados.

---

## Compilar a partir do codigo-fonte

Requer [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) (ou posterior). Windows x64.

### v3 (recomendado)

Feche qualquer instancia em execucao antes de compilar - a etapa de copia do exe falha se o arquivo estiver bloqueado.

```powershell
Stop-Process -Name "UltimateCameraMod.V3" -Force -ErrorAction SilentlyContinue
dotnet build "src/UltimateCameraMod.V3/UltimateCameraMod.V3.csproj" -c Release
Start-Process "src/UltimateCameraMod.V3/bin/Release/net6.0-windows/UltimateCameraMod.V3.exe"
```

### Dependencias (NuGet - restauradas automaticamente)

- [K4os.Compression.LZ4](https://www.nuget.org/packages/K4os.Compression.LZ4/) - Compressao/descompressao de blocos LZ4

---

## Estrutura do projeto

```
src/UltimateCameraMod/              Biblioteca compartilhada + aplicacao WPF v2.x
├── Controls/                       CameraPreview, FovPreview
├── Models/                         PresetCodec, modelos de dados
├── Paz/                            ArchiveWriter, CompressionUtils, E/S de PAZ
├── Services/                       CameraMod, GameDetector, JsonModExporter, GameInstallBaselineTracker
├── MainWindow.xaml                 Interface v2.x
└── UltimateCameraMod.csproj

src/UltimateCameraMod.V3/           Aplicacao WPF v3 com prioridade em exportacao (referencia o codigo compartilhado)
├── Controls/                       CameraPreview, FovPreview (variantes v3)
├── Models/                         PresetManagerItem, ImportedPreset
├── Assets/                         ucm.ico, ucm-app-icon.png
├── ShippedPresets/                  Presets comunitarios integrados implantados na primeira execucao
├── MainWindow.xaml                 Shell de dois paineis: barra lateral + editor com abas
├── ExportJsonDialog.xaml           Assistente de exportacao multiformato (JSON, XML, 0.paz, .ucmpreset)
├── ImportPresetDialog.xaml         Importar de .ucmpreset / XML / PAZ
├── ImportMetadataDialog.xaml       Entrada de metadados do preset (nome, autor, descricao, URL)
├── CommunityBrowserDialog.xaml     Explorar e baixar presets comunitarios do GitHub
├── NewPresetDialog.xaml            Criar / nomear novos presets
├── ShellTaskbarPropertyStore.cs    Icone de barra de tarefas do Windows via shell property store
├── ApplicationIdentity.cs          App User Model ID compartilhado
└── UltimateCameraMod.V3.csproj

community_presets/                  Presets de camera contribuidos pela comunidade
ucm_presets/                        Definicoes oficiais de presets de estilo UCM
```

---

## Compatibilidade

- **Plataformas:** Steam, Epic Games, Xbox / Game Pass
- **Sistema operacional:** Windows 10/11 (x64)
- **Tela:** Qualquer proporcao de aspecto - 16:9, 21:9, 32:9

---

## Perguntas frequentes

**Posso ser banido por isso?**
O UCM modifica apenas arquivos de dados offline. Nao toca na memoria do jogo, nao injeta codigo nem interage com processos em execucao. Use por sua propria conta e risco em modos online/multijogador.

**O jogo atualizou e minha camera voltou ao vanilla.**
Normal - atualizacoes do jogo sobrescrevem os arquivos modificados. Reabra o UCM e clique em Instalar (ou re-exporte JSON para JSON Mod Manager / CDUMM). Suas configuracoes sao salvas automaticamente.

**Meu antivirus sinalizou o exe.**
Falso positivo conhecido com aplicacoes .NET autocontidas. A verificacao do VirusTotal esta limpa: [v3.1.2](https://www.virustotal.com/gui/file/7c5ddbfce28cabecb799a00b87ad4c4641c30c9db65cd2560c6a91d578852021). O codigo-fonte completo esta disponivel aqui para revisar e compilar voce mesmo.

**O que significa deslocamento horizontal 0?**
0 = posicao de camera vanilla (personagem ligeiramente a esquerda). 0.5 = personagem centralizado na tela. Valores negativos movem mais para a esquerda, valores positivos movem mais para a direita.

**Atualizando de uma versao anterior?**
Usuarios do v3.x: simplesmente substitua o exe, todos os presets e configuracoes sao preservados. Usuarios do v2.x: exclua a pasta antiga do UCM, verifique os arquivos do jogo no Steam e entao execute o v3.1 de uma pasta nova. Consulte as [notas da versao](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1) para instrucoes detalhadas.

---

## Historico de versoes

- **v3.1.2** - Correcao de valores sagrados ausentes em Instalar/exportacoes na aba God Mode. Veja as [notas da versao](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1.2).
- **v3.1.1** - Correcao de deteccao falso-positiva de backup contaminado em arquivos de jogo limpos.
- **v3.1** - Sobrescritas do Sacred God Mode (edicoes do usuario protegidas permanentemente de reconstrucoes). Opcao de rotacao automatica de lock-on (creditos para [sillib1980](https://github.com/sillib1980)). Indicadores verdes de valores sagrados. Correcao de instalacao do Full Manual Control. Overlay de atualizacao compativel com versoes. Veja as [notas da versao](https://github.com/FitzDegenhub/UltimateCameraMod/releases/tag/v3.1).
- **v3.0.2** - Todos os dialogos convertidos para sistema de overlay dentro do aplicativo. Sobrescritas do God Mode persistem entre trocas de aba. Selecao de tipo de preset (UCM Managed vs Full Manual Control). Catalogo de presets comunitarios movido para o repositorio principal. 54 tooltips de atributos do God Mode. Correcoes de crashes do jogo. Validacao vanilla atualizada para o patch do jogo de junho de 2026. Wiki de 21 paginas.
- **v3.0.1** - Redesign com prioridade em exportacao. Editor de tres niveis (UCM Quick / Fine Tune / God Mode). Formato de arquivo `.ucmpreset`. Sistema de presets baseado em arquivos. Catalogos de presets UCM e comunitarios. Exportacao multiformato. Steadycam expandido para mais de 30 estados de camera. Controle deslizante de zoom de lock-on.
- **v2.5** - Ultima versao do v2.x.
- **v2.4** - Deslocamento horizontal proporcional, deslocamento em todas as montarias e habilidades de mira, reformulacao da camera do cavalo, backups com reconhecimento de versao, pre-visualizacao de FoV, janela redimensionavel.
- **v2.3** - Correcao do deslocamento horizontal para 16:9, controle deslizante baseado em delta, banner completo de configuracao de instalacao.
- **v2.2** - Steadycam, niveis de zoom extras, primeira pessoa no cavalo, deslocamento horizontal, FoV universal, consistencia na mira de habilidades, importar XML, compartilhar presets, notificacoes de atualizacao.
- **v2.1** - Correcao dos controles deslizantes de presets personalizados que nao gravavam em todos os niveis de zoom.
- **v2.0** - Reescrita completa de Python para C# / .NET 6 / WPF. Editor XML avancado, gerenciamento de presets, deteccao automatica do jogo.
- **v1.5** - Versao Python com interface customtkinter.

---

## Creditos e agradecimentos

- **0xFitz** - Desenvolvimento do UCM, ajuste de camera, editor avancado
- **[@sillib1980](https://github.com/sillib1980)** - Descobriu os campos de camera de rotacao automatica de lock-on

### Reescrita em C# (v2.0)
- **[MrIkso](https://github.com/MrIkso/CrimsonDesertTools)** - CrimsonDesertTools - Analisador C# de PAZ/PAMT, criptografia ChaCha20, compressao LZ4, PaChecksum, reempacotador de arquivos (.NET 8, MIT)
- **[mcraiha](https://github.com/mcraiha/CSharp-ChaCha20-NetStandard)** - Implementacao pura em C# do cifrador de fluxo ChaCha20 (BSD)
- **[MrIkso no Reshax](https://reshax.com/topic/18908-need-help-extracting-paz-pamt-files-from-crimson-desert-blackspace-engine/page/2/?&_rid=3118#findComment-103796)** - Guia de reempacotamento PAZ: alinhamento de 16 bytes, checksum PAMT, patching do indice raiz PAPGT

### Versao original em Python (v1.5)
- **[lazorr410](https://github.com/lazorr410/crimson-desert-unpacker)** - crimson-desert-unpacker - Ferramentas de arquivos PAZ, pesquisa de descriptografia
- **Maszradine** - CDCamera - Regras de camera, sistema steadycam, presets de estilo
- **manymanecki** - CrimsonCamera - Arquitetura de modificacao dinamica de PAZ

## Apoio

Se voce achou isso util, considere apoiar o desenvolvimento:

[![Buy me a coffee](https://img.shields.io/badge/Buy_me_a_coffee-FF5E5B?style=for-the-badge&logo=kofi&logoColor=white)](https://ko-fi.com/0xfitz)

## Licenca

MIT
