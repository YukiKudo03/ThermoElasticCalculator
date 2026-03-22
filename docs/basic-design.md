# 基本設計書

> **NOTE (2026-03-23):** このドキュメントは v0.4.0 (2026-03-10) に基づいており、プロジェクトは現在 v1.0.0 (2026-03-23) に更新されています。v1.0.0 で追加された26個の新しい計算エンジンと5つの新しいUI画面については、詳細は README.md の「v1.0.0 新機能」セクションと `docs/CODEMAPS/` を参照してください。

## ThermoElasticCalculator

| 項目 | 内容 |
|------|------|
| 文書名 | ThermoElasticCalculator 基本設計書 |
| ドキュメント バージョン | 0.4.0 |
| プロジェクト バージョン | 1.0.0 |
| 作成日 | 2026-03-10 |
| 最終更新日 | 2026-03-23 |

---

## 1. システム構成

### 1.1 アーキテクチャ概要

計算エンジン（Core）とUI（Desktop）を分離した2層構成とする。

```
┌──────────────────────────────────────────────────────┐
│              ThermoElastic.Desktop                     │
│              (Avalonia UI / MVVM)                      │
│                                                        │
│  ┌─────────────────────────────────────────────────┐  │
│  │  Views (AXAML)                                   │  │
│  │  MainWindow │ MineralEditor │ PTProfile │ ...   │  │
│  ├─────────────────────────────────────────────────┤  │
│  │  ViewModels                                      │  │
│  │  MainWindowVM │ MineralEditorVM │ PTProfileVM   │  │
│  └─────────────────────────────────────────────────┘  │
├──────────────────────────────────────────────────────┤
│              ThermoElastic.Core                        │
│              (.NET 8 クラスライブラリ)                    │
│                                                        │
│  ┌──────────────┐┌──────────────┐┌─────────┐┌──────┐│
│  │   Models     ││ Calculations ││ Database ││  IO  ││
│  │ MineralParams││ EOSOptimizer ││ SLB2011  ││JsonIO││
│  │ ThermoMineral││ DebyeFunction││Endmembers││CsvIO ││
│  │ SolidSolution││ LandauCalc   ││Solutions ││      ││
│  │ PhaseAssem.  ││ GibbsMinim.  ││MineralDB ││      ││
│  │ ResultSummary││ SolutionCalc ││          ││      ││
│  │ RockComposit.││ MixtureCalc  ││          ││      ││
│  └──────────────┘└──────────────┘└─────────┘└──────┘│
└──────────────────────────────────────────────────────┘
```

### 1.2 技術スタック

| 要素 | 技術 |
|------|------|
| 言語 | C# 12 |
| ランタイム | .NET 8.0 |
| UI フレームワーク | Avalonia UI 11 |
| UIパターン | MVVM（CommunityToolkit.Mvvm） |
| シリアライゼーション | System.Text.Json |
| ユニットテスト | xUnit |
| 対応OS | Windows, macOS, Linux |

### 1.3 プロジェクト構成

```
ThermoElasticCalculator.sln
│
├── src/ThermoElastic.Core/              # 計算エンジン
│   ├── ThermoElastic.Core.csproj        # net8.0 クラスライブラリ
│   ├── Models/
│   │   ├── MineralParams.cs             # 鉱物パラメータ（BM3 EOS含む）
│   │   ├── ThermoMineralParams.cs       # 計算済み熱弾性特性（F, G, S含む）
│   │   ├── ResultSummary.cs             # 結果集約
│   │   ├── PTData.cs                    # P-T データ点
│   │   ├── PTProfile.cs                 # P-T プロファイル
│   │   ├── RockComposition.cs           # 岩石組成
│   │   ├── PhaseAssemblage.cs           # 相集合体（Gibbs最小化用）
│   │   ├── PhaseEntry.cs                # 単一相エントリ
│   │   ├── SolidSolution.cs             # 固溶体定義
│   │   ├── SolutionSite.cs              # 結晶学サイト
│   │   └── InteractionParam.cs          # van Laar 相互作用パラメータ
│   ├── Calculations/
│   │   ├── MieGruneisenEOSOptimizer.cs  # EOS最適化
│   │   ├── DebyeFunctionCalculator.cs   # Debye関数（解析的Simpson積分）
│   │   ├── LandauCalculator.cs          # Landau 変位型相転移
│   │   ├── SolutionCalculator.cs        # 固溶体熱力学
│   │   ├── GibbsMinimizer.cs            # Gibbs自由エネルギー最小化
│   │   ├── EquilibriumAggregateCalculator.cs  # 平衡相+混合計算
│   │   ├── PhaseDiagramCalculator.cs    # 相境界計算
│   │   ├── MixtureCalculator.cs         # 機械的混合計算
│   │   ├── VProfileCalculator.cs        # 組成プロファイル
│   │   ├── PTProfileCalculator.cs       # P-Tプロファイル計算
│   │   ├── RockCalculator.cs            # 岩石物性計算
│   │   └── Optimizer.cs                 # 数値最適化
│   ├── Database/
│   │   ├── SLB2011Endmembers.cs         # 42端成分（BurnMan検証済み）
│   │   ├── SLB2011Solutions.cs          # 固溶体モデル定義
│   │   └── MineralDatabase.cs           # 静的アクセサ
│   ├── IO/
│   │   ├── MineralCsvIO.cs              # CSV入出力
│   │   └── JsonFileIO.cs               # JSON入出力ヘルパー
│   └── PhysicConstants.cs               # 物理定数
│
├── src/ThermoElastic.Desktop/           # デスクトップUI
│   ├── ThermoElastic.Desktop.csproj     # Avalonia アプリ
│   ├── App.axaml / App.axaml.cs         # アプリケーション定義
│   ├── Program.cs                       # エントリポイント
│   ├── Views/
│   │   ├── MainWindow.axaml             # メインウィンドウ
│   │   ├── MineralEditorView.axaml      # 鉱物パラメータ編集
│   │   ├── MineralDatabaseView.axaml    # SLB2011 鉱物データベース閲覧
│   │   ├── PTProfileView.axaml          # P-Tプロファイル計算
│   │   ├── MixtureView.axaml            # 2鉱物混合
│   │   ├── RockCalculatorView.axaml     # 岩石計算
│   │   ├── ResultsView.axaml            # 結果表示
│   │   └── ChartView.axaml              # グラフ可視化
│   └── ViewModels/
│       ├── MainWindowViewModel.cs
│       ├── MineralEditorViewModel.cs
│       ├── MineralDatabaseViewModel.cs
│       ├── PTProfileViewModel.cs
│       ├── MixtureViewModel.cs
│       ├── RockCalculatorViewModel.cs
│       ├── ResultsViewModel.cs
│       └── ChartViewModel.cs
│
├── tests/ThermoElastic.Core.Tests/      # ユニットテスト（24クラス）
│   ├── ThermoElastic.Core.Tests.csproj
│   ├── MineralParamsTests.cs            # BM3 EOS テスト
│   ├── EOSOptimizerTests.cs             # EOS最適化テスト
│   ├── DebyeFunctionCalculatorTests.cs  # Debye関数精度テスト
│   ├── LandauCalculatorTests.cs         # Landau相転移テスト
│   ├── MagneticContributionTests.cs     # 磁気寄与テスト
│   ├── SolutionCalculatorTests.cs       # 固溶体テスト
│   ├── GibbsFreeEnergyTests.cs          # 自由エネルギーテスト
│   ├── GibbsMinimizerTests.cs           # 相平衡テスト
│   ├── MixtureCalculatorTests.cs        # 混合モデルテスト
│   ├── BurnManEndmemberVerificationTests.cs  # BurnManクロス検証
│   ├── ThermodynamicIdentityTests.cs    # 熱力学恒等式検証
│   ├── LandauSolutionVerificationTests.cs    # Landau・固溶体検証
│   ├── MixingModelVerificationTests.cs  # 混合モデル・PREM比較
│   ├── BurnManReferenceHelper.cs        # BurnMan CSV読込ヘルパー
│   ├── TestData/                        # BurnManリファレンスCSV
│   └── ... (他テストファイル)
│
└── thermo-dynamics/                     # 旧WinForms版（非推奨）
```

---

## 2. モジュール構成

### 2.1 ThermoElastic.Core モジュール

UI非依存な計算エンジン。全計算ロジック・データモデル・ファイルI/Oを含む。

| パッケージ | 責務 |
|-----------|------|
| Models | データモデル定義（鉱物、固溶体、岩石、相集合体、プロファイル、結果） |
| Calculations | EOS計算、Debye関数、Landau相転移、固溶体熱力学、Gibbs最小化、混合計算、最適化 |
| Database | SLB2011 鉱物パラメータデータベース（42端成分 + 固溶体モデル） |
| IO | JSON / CSV ファイル入出力 |

#### 依存関係

```
Models ← Calculations ← IO
  │          │
  └──────────┘
Database → Models (パラメータ生成)
```

### 2.2 ThermoElastic.Desktop モジュール

Avalonia UI ベースのクロスプラットフォーム デスクトップアプリケーション。

| パッケージ | 責務 |
|-----------|------|
| Views | AXAML によるUI定義 |
| ViewModels | UIロジック、Core呼び出し、データバインディング |

#### 依存関係

```
Desktop → Core (一方向依存)
```

Core は Desktop に一切依存しない。

### 2.3 モジュール間の依存方向

```
ThermoElastic.Core.Tests ──→ ThermoElastic.Core
ThermoElastic.Desktop ──────→ ThermoElastic.Core
```

---

## 3. データフロー

### 3.1 MVVM データバインディング

```
┌──────────┐    DataBinding    ┌──────────────┐    メソッド呼出    ┌──────────┐
│   View   │ ←──────────────→ │  ViewModel   │ ──────────────→ │   Core   │
│  (AXAML) │   INotifyProperty │              │                  │ (計算)   │
│          │   Changed         │  Commands    │ ←──────────────  │          │
│          │                   │  Properties  │   計算結果        │          │
└──────────┘                   └──────────────┘                  └──────────┘
```

### 3.2 単一鉱物計算フロー

```
[View]                    [ViewModel]                    [Core]
ユーザー入力 ──→ バインドプロパティ ──→ MieGruneisenEOSOptimizer
  P, T                     │                              │
                           │ CalculateCommand              │
                           ├──────────────────────────────→│
                           │                              │
                           │←── ThermoMineralParams ──────│
                           │                              │
結果表示 ←───── ResultSummary                              │
```

### 3.3 岩石物性計算フロー

```
[View]                    [ViewModel]                    [Core]
鉱物追加 ──→ MineralEntries (ObservableCollection)
比率入力 ──→ VolumeRatio
P,T入力  ──→ Pressure, Temperature
方法選択 ──→ MixtureMethod
                           │
                           │ CalculateCommand
                           ├──────────────────────────────→ RockCalculator
                           │                                    │
                           │←── (mixed, individuals) ──────────│
                           │
結果表示 ←───── ResultsViewModel
```

### 3.4 相平衡計算フロー（v0.4.0 追加）

```
[入力]                                     [Core]
相集合体定義 ──→ EquilibriumAggregateCalculator
  鉱物A (Fo)                                  │
  鉱物B (Wa)          ┌──────────────────────┤
  P-T条件              │                      │
                       ▼                      │
                  GibbsMinimizer              │
                    │ 各端成分の G(P,T) 計算    │
                    │ (MieGruneisenEOSOptimizer)│
                    │                         │
                    ▼                         │
                  安定相比率の決定              │
                    │ (SVDベース最適化)         │
                    │                         │
                    ▼                         ▼
                  PhaseAssemblage ──→ MixtureCalculator
                    (安定相+比率)       (Hill/Voigt/Reuss)
                                          │
                                          ▼
                                    ResultSummary
                                    (岩石バルク物性)
```

### 3.5 ファイル I/O フロー

```
┌─────────────────────┐
│  プラットフォーム     │    Avalonia の
│  ファイルダイアログ    │    IStorageProvider
└─────────┬───────────┘
          │ パス
          ▼
┌─────────────────────┐
│  Core.IO             │
│  JsonFileIO          │    System.Text.Json
│  MineralCsvIO        │    StreamReader/Writer
└─────────┬───────────┘
          │ オブジェクト
          ▼
┌─────────────────────┐
│  Core.Models         │
│  MineralParams       │
│  RockComposition     │
│  PTProfile           │
└─────────────────────┘
```

---

## 4. 画面設計

### 4.1 画面遷移

メインウィンドウから各機能画面への遷移。Avalonia ではモーダルダイアログまたはタブ切替で実現。

```
┌──────────────────────────────────────┐
│           MainWindow                  │
│                                      │
│  [Mineral Library] [Mineral Database] │
│  [PT Profile]      [Mixture]         │
│  [Rock Calculator] [Chart]           │
└──┬──────────┬──────────┬────────────┬┘
   │          │          │            │
   ▼          ▼          ▼            ▼
┌────────┐ ┌────────┐ ┌────────┐ ┌──────────┐
│Mineral │ │  PT    │ │Mixture │ │  Rock    │
│Editor  │ │Profile │ │        │ │Calculator│
└────────┘ └───┬────┘ └───┬────┘ └────┬─────┘
               │          │           │
               ▼          ▼           ▼
            ┌──────────────────────────┐
            │       ResultsView        │
            │  (共通結果表示・CSV出力)    │
            └──────────────────────────┘
```

### 4.2 各画面の概要

#### MainWindow（メインウィンドウ）
- 4つの機能ボタンを配置
- アプリ情報（バージョン等）表示
- プラットフォーム非依存のデータディレクトリ初期化

#### MineralEditorView（鉱物パラメータ編集）
- 全16パラメータの入力フィールド
- JSON（.mine）読込・保存
- CSV一括インポート・エクスポート
- テスト計算機能（指定P-Tでの速度・密度確認）

#### PTProfileView（P-Tプロファイル計算）
- 鉱物ファイル読込
- P-Tデータの手動入力またはファイル読込
- DataGrid による結果表示

#### MixtureView（2鉱物混合計算）
- 2鉱物ファイル読込
- 体積比リスト入力
- 混合モデル選択

#### RockCalculatorView（岩石物性計算）
- 任意数の鉱物追加・削除
- 各鉱物の体積比率入力
- 岩石組成の保存・読込（.rock）

#### ResultsView（結果表示）
- DataGrid によるテーブル表示
- CSVエクスポート機能
- 表示モード切替（プロファイル / 組成 / 岩石）

---

## 5. データ設計

### 5.1 ファイルフォーマット

全ファイル形式は旧WinForms版と互換性を維持する。

#### 鉱物パラメータ（.mine）

```json
{
  "MineralName": "Forsterite",
  "PaperName": "fo",
  "NumAtoms": 7,
  "MolarVolume": 43.603,
  "MolarWeight": 140.69,
  "KZero": 127.96,
  "K1Prime": 4.218,
  "K2Prime": 0,
  "GZero": 81.60,
  "G1Prime": 1.4626,
  "G2Prime": 0,
  "DebyeTempZero": 809.17,
  "GammaZero": 0.99282,
  "QZero": 2.10672,
  "EhtaZero": 2.2997,
  "RefTemp": 300.0,
  "F0": -2055.403,
  "Tc0": 0,
  "VD": 0,
  "SD": 0,
  "SpinQuantumNumber": 0,
  "MagneticAtomCount": 0
}
```

#### 岩石組成（.rock）

```json
{
  "Name": "SimplePeridotite",
  "Minerals": [
    {
      "Mineral": { ... MineralParams ... },
      "VolumeRatio": 0.6
    },
    {
      "Mineral": { ... MineralParams ... },
      "VolumeRatio": 0.4
    }
  ]
}
```

### 5.2 データディレクトリ

プラットフォーム非依存なデータ保存先を使用する。

```csharp
// 各OSのアプリケーションデータディレクトリを使用
var basePath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "ThermoElasticCalculator");
```

| サブディレクトリ | 内容 |
|----------------|------|
| `Minerals/` | .mine ファイル |
| `PTProfiles/` | P-T プロファイルファイル |
| `VProfiles/` | 組成プロファイルファイル |
| `Results/` | 出力 CSV |

---

## 6. エラーハンドリング方針

| 種別 | 方針 |
|------|------|
| ファイル読込失敗 | ダイアログでユーザーに通知、処理を中断 |
| 計算エラー（収束失敗等） | ダイアログでエラー内容を表示 |
| 不正入力（比率合計≠1等） | 自動正規化して計算を実行 |
| JSON デシリアライズ失敗 | 「読み込みに失敗しました」メッセージ |

Core 層では例外をスローし、ViewModel 層で catch してユーザーに通知する。
