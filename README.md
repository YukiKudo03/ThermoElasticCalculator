# ThermoElasticCalculator

高温高圧条件下におけるマントル鉱物の弾性的性質と、多鉱物混合体（岩石）の物性を計算するクロスプラットフォーム デスクトップアプリケーションです。

## 概要

Stixrude & Lithgow-Bertelloni (2011) "Thermodynamics of mantle minerals - II. Phase equilibria" に基づく熱弾性物理学の式を実装し、以下の計算を行います。

- 任意の温度・圧力条件における単一鉱物の弾性波速度、密度、弾性率等の物性計算
- P-T プロファイルに沿った物性変化の計算
- 複数鉱物の混合モデル（Voigt / Reuss / Hill / Hashin-Shtrikman）による岩石物性の計算
- SLB2011 鉱物データベース（46端成分 + 固溶体モデル）
- Gibbs 自由エネルギー最小化による相平衡計算
- Landau 変位型相転移モデル、磁気寄与
- 断熱温度プロファイル（等エントロピー線）の計算
- PREM 参照地球モデルによる深さ-圧力変換
- 定義済み岩石組成（Pyrolite、Harzburgite、MORB、下部マントル）

## 主な機能

| 機能 | 説明 |
|------|------|
| Mineral Library | 鉱物パラメータの作成・編集・保存（JSON / CSV） |
| Mineral Database | SLB2011 端成分46鉱物 + 固溶体モデルの内蔵データベース |
| P-T Profile Calculator | P-T プロファイルに沿った単一鉱物の物性計算 |
| Isentrope Calculator | 断熱温度プロファイル T(P) の計算（S = const） |
| Mixture Calculator | 2鉱物の体積比混合による組成特性計算（HS境界対応） |
| Rock Calculator | 任意数の鉱物を組み合わせた岩石物性の計算 + 定義済み岩石 |
| Phase Equilibrium | Gibbs 自由エネルギー最小化による安定相集合体の計算 |
| Phase Diagram | 相境界の圧力-温度依存性の計算 |
| Thermoelastic Param. Fitter | 実験データ（Vp/Vs, V(T,P)）からSLB2011熱弾性パラメータのフィッティング |
| PREM Reference | 深さ-圧力変換、参照地球モデルとの比較 |

## 教科書 (Textbook)

本プロジェクトの全機能を体系化した大学院レベルの計算鉱物物理学教科書です。

**構成:**
- **章数:** 31章
- **部数:** 8部構成
- **規模:** 約20,300行、約1.5MB
- **場所:** `textbook/` ディレクトリ

詳細な目次と各章の説明は [textbook/README.md](textbook/README.md) をご覧ください。

## v1.0.0 新機能

### 新しい計算アプリケーション（+31新規追加）

v1.0.0では、以下のカテゴリで31の新しい計算エンジンと5つの新しいUI画面を追加しました:

**衝撃圧縮 (Shock Equations)**
- HugoniotCalculator — 衝撃圧縮曲線 (EOS) 計算
- IsomekeCalculator — 等動圧線 (S = const, U = varying)
- RankineHugoniotSolver — ランキン-ユゴニオ関係式

**相図・平衡計算の拡張**
- PhaseDiagramCalculator — 相境界追跡（完全な P-T 図）
- EquilibriumAggregateCalculator — 複数相共存の安定相集合体
- VProfileCalculator — 体積プロファイル P(V) 計算

**惑星内部物理**
- PlanetaryInteriorSolver — 火星、木星型惑星の内部構造計算
- MarsInteriorModel — 火星特化モデル
- MagmaOceanCalculator — 原始岩石圏の融融状態計算
- PostPerovskiteCalculator — pPv 相の深さ依存計算
- ULVZCalculator — 超低速度層 (ULVZ) の物性

**逆問題・最適化**
- LevenbergMarquardtOptimizer — 非線形最小二乗法
- ThermoElasticFitter — 実験データからのSLB2011パラメータフィッティング
- MCMCSampler — Markov Chain Monte Carlo サンプリング
- InversionFramework — Bayesian 逆解析フレームワーク
- MLSurrogateModel — 機械学習サロゲートモデル
- TrainingDataGenerator — 学習データ生成

**逆地化学**
- IronPartitioningSolver — 鉄の分配係数計算
- SpinCrossoverCalculator — スピン転移の圧力依存性
- ElementPartitioningModel — 元素分配モデル

**熱・輸送特性**
- ThermalConductivityCalculator — 熱伝導率 κ(P,T)
- ElasticTensorCalculator — 弾性テンソル C_ijkl の計算
- AnelasticityCalculator — 非弾性 (Q⁻¹) の計算
- ParametricQCalculator — パラメトリック非弾性計算
- AndradeCalculator — Andrade 粘弾性モデル
- ExtendedBurgersCalculator — 拡張 Burgers 粘弾性モデル
- WaterQCorrector — 含水による非弾性補正
- MeltQCorrector — メルト効果による非弾性補正
- QProfileBuilder — Q プロファイル構築

**検証・品質保証**
- ThermodynamicVerifier — SLB2011 と BurnMan の相互検証
- JointLikelihood — 複数データセットの統合尤度
- WaterContentEstimator — 含水量推定

### 新しい UI 画面（+22新規追加、計35ビュー）

**基本計算ツール（3ビュー）**
- **Mineral Editor** — 鉱物パラメータの作成・編集・保存
- **Mineral Database** — SLB2011 端成分46鉱物の検索・閲覧
- **P-T Profile** — 温度-圧力プロファイル沿いの物性計算

**混合・岩石計算（2ビュー）**
- **Mixture** — 2鉱物混合モデル（Voigt/Reuss/Hill/HS）
- **Rock Calculator** — 任意数鉱物の岩石物性計算

**相平衡・相図（2ビュー）**
- **Phase Diagram Explorer** — 相境界を対話的に探索し、複雑な相図を可視化
- **Verification Dashboard** — SLB2011 と BurnMan の相互検証

**衝撃圧縮・状態方程式（3ビュー）**
- **Hugoniot** — 衝撃圧縮 EOS (Hugoniot 曲線) 計算
- **EOS Fitter** — 実験データへの状態方程式フィッティング
- **Thermoelastic Fitter** — 実験データ（Vp/Vs, V(T,P)）からSLB2011パラメータのフィッティング

**惑星内部物理（3ビュー）**
- **Planetary Interior** — 火星・外部惑星の質量-半径 (M-R) 関係計算
- **Magma Ocean** — 原始岩石圏の融融状態計算
- **Post-Perovskite** — pPv 相の深さ依存物性計算

**深部地球物質（3ビュー）**
- **LLSVP** — 大低速度領域 (LLSVP) の物性構造
- **ULVZ** — 超低速度層 (ULVZ) の物性
- **Slab Model** — スラブの冷却・沈み込みモデル

**逆問題・最適化（4ビュー）**
- **Bayesian Inversion** — Bayesian 逆解析フレームワーク
- **Composition Inverter** — 地震波速度から岩石組成を推定
- **ML Data** — 機械学習用の学習データ生成
- **Lookup Table** — 計算結果を高速検索可能なテーブルとして事前計算・保存

**地化学・鉱物学（3ビュー）**
- **Iron Partitioning** — 鉄の分配係数と圧力依存性計算
- **Spin Crossover** — スピン転移の圧力依存性計算
- **Water Content** — 含水量推定と影響評価

**物理特性（4ビュー）**
- **Thermal Conductivity** — 熱伝導率 κ(P,T) 計算
- **Elastic Tensor** — 弾性テンソル C_ijkl の計算
- **Anelasticity** — 非弾性 (Q⁻¹) の圧力温度依存性
- **Q Profile** — Q プロファイルの構築と評価

**ジオバロメトリ・化学（2ビュー）**
- **Classical Geobarometry** — 古典的鉱物平衡バロメトリ計算
- **Geobarometry** — 詳細ジオバロメトリと温度推定

**その他の計算（2ビュー）**
- **Oxygen Fugacity** — 酸素フガシティ計算
- **Electrical Conductivity** — 電気伝導率の圧力温度依存性

**補助ツール（3ビュー）**
- **Sensitivity Kernel** — 地震波 (Vp/Vs) の深さカーネル計算・可視化
- **Chart** — 計算結果の詳細グラフ表示
- **Results** — 計算結果の表示・エクスポート

### 拡張テスト カバレッジ

- **総テスト数:** 556（v0.5.0 の 286 から +270）
  - コアテスト（ThermoElastic.Core.Tests）：479 テスト
  - E2E テスト（ThermoElastic.Desktop.E2E）：77 テスト
- **行カバレッジ:** 95.6%（品質確保）

## 動作環境

- **Windows** 10 / 11
- **macOS** 12 (Monterey) 以降
- **Linux** (Ubuntu 22.04+, Fedora 38+, その他主要ディストリビューション)
- .NET 9.0 ランタイム

## プロジェクト構成

```
ThermoElasticCalculator/
├── ThermoElasticCalculator.sln
├── README.md
├── docs/                              # 設計ドキュメント
│   ├── CODEMAPS/                     # アーキテクチャマップ
│   ├── requirements.md                # 要件定義書
│   ├── basic-design.md                # 基本設計書
│   └── detailed-design.md             # 詳細設計書
├── src/
│   ├── ThermoElastic.Core/            # 計算エンジン（クラスライブラリ）
│   │   ├── Models/                    # データモデル（25ファイル）
│   │   ├── Calculations/             # 計算ロジック（42ファイル）
│   │   ├── Database/                 # SLB2011 鉱物DB + 定義済み岩石（5ファイル）
│   │   ├── IO/                       # ファイル入出力
│   │   └── ThermoElastic.Core.csproj
│   └── ThermoElastic.Desktop/        # デスクトップUI（Avalonia）
│       ├── Views/                     # 画面定義（AXAML、35ビュー）
│       ├── ViewModels/               # ViewModel（35個）
│       └── ThermoElastic.Desktop.csproj
├── tests/
│   ├── ThermoElastic.Core.Tests/     # ユニットテスト（479テスト）
│   │   └── ThermoElastic.Core.Tests.csproj
│   └── ThermoElastic.Desktop.E2E/    # E2Eテスト（77テスト）
│       └── ThermoElastic.Desktop.E2E.csproj
├── textbook/                          # 大学院レベル教科書（31章・8部構成）
│   ├── README.md                      # 教科書の目次と概要
│   └── chapters/                      # 各章のMarkdownファイル
└── research/                          # 研究プロジェクトと応用例
    ├── anelasticity/                  # 非弾性の圧力温度依存性研究
    └── textbook/                      # 教科書作成補助（計算結果・図表）
```

## ビルド方法

### 前提条件

- .NET 9.0 SDK

### ビルド・実行

```bash
# リポジトリクローン
git clone <repository-url>
cd ThermoElasticCalculator

# ビルド
dotnet build

# 実行
dotnet run --project src/ThermoElastic.Desktop

# テスト
dotnet test
```

### プラットフォーム別パブリッシュ

```bash
# Windows
dotnet publish src/ThermoElastic.Desktop -r win-x64 -c Release

# macOS (Apple Silicon)
dotnet publish src/ThermoElastic.Desktop -r osx-arm64 -c Release

# macOS (Intel)
dotnet publish src/ThermoElastic.Desktop -r osx-x64 -c Release

# Linux
dotnet publish src/ThermoElastic.Desktop -r linux-x64 -c Release
```

## 使い方

### 1. 鉱物データの準備

**手動入力:**
Mineral Library 画面で各パラメータを入力し、`.mine` ファイル（JSON）として保存。

**CSV 一括インポート:**
以下のヘッダ形式の CSV ファイルを用意し、Import CSV から読み込み。

```
MineralName,PaperName,NumAtoms,MolarVolume,MolarWeight,KZero,K1Prime,K2Prime,GZero,G1Prime,G2Prime,DebyeTempZero,GammaZero,QZero,EhtaZero,RefTemp,F0,Tc0,VD,SD,SpinQuantumNumber,MagneticAtomCount
```

インポート時、各鉱物は `Minerals/` フォルダに個別 `.mine` ファイルとして自動保存されます。

### 2. 単一鉱物の計算

1. **Mineral Library** で鉱物パラメータを読み込み
2. CalcTest 欄で圧力(GPa)・温度(K) を指定し確認

### 3. P-T プロファイル計算

1. **Calculate w/ PTProfile** を開く
2. 鉱物ファイル（`.mine`）を読み込み
3. P-T プロファイル（圧力・温度の組リスト）を作成または読み込み
4. 計算実行 → 結果テーブルで Vp, Vs, 密度等を確認
5. CSV エクスポート可能

### 4. 2鉱物混合計算

1. **Calculate Mixture** を開く
2. 2つの鉱物ファイルを読み込み
3. 体積比リストを入力
4. 混合モデル（Hill / Voigt / Reuss / HS）を選択
5. P-T 条件を指定して計算

### 5. 岩石物性計算

1. **Rock Calculator** を開く
2. 「Add Mineral...」で任意数の鉱物を追加し、各体積比率を入力
3. 混合モデル・P-T 条件を指定して計算
4. 岩石組成は `.rock` ファイルとして保存・再利用可能

## 計算理論

### 状態方程式

Birch-Murnaghan 3次有限歪み状態方程式:

```
P = 3K₀f(1+2f)^(5/2) [1 + 3(K₀'-4)f/2]
```

ここで `f = [(V₀/V)^(2/3) - 1] / 2` はEulerian有限歪み。

### 熱効果

Mie-Gruneisen モデルによる熱圧力補正:

```
ΔP_th = (γ/V) × ΔE_th
```

Debye モデルに基づく内部エネルギーと比熱の計算。

### 弾性波速度

```
Vp = √[(KS + 4G/3) / ρ]
Vs = √[G / ρ]
```

### Debye モデル

Debye 関数 D₃(x) による内部エネルギーと比熱の解析的計算（Simpson 積分、500分点）:

```
E_th(T) = 3nRT × D₃(θ/T)
Cv(T) = 3nR × [4D₃(x) - 3x/(eˣ-1)]
D₃(x) = (3/x³) ∫₀ˣ t³/(eᵗ-1) dt
```

### 自由エネルギー

```
F = F₀ + F_cold + F_thermal + F_Landau + F_mag  [kJ/mol]
G = F + PV  [kJ/mol]
S = -∂F/∂T  [J/mol/K]
```

### Landau 変位型相転移

三臨界遷移（α-βクオーツ等）:

```
Q(T) = (1 - T/Tc)^(1/4)  (T < Tc)
Tc(P) = Tc₀ + VD × P / SD
G_Landau = SD × [(T-Tc)Q² + Tc×Q⁶/3]
```

### 磁気寄与

```
F_mag = -T × r × R × ln(2S+1)
```

### 混合モデル

- **Voigt** (等歪み上界): M_v = Σ fᵢMᵢ
- **Reuss** (等応力下界): 1/M_r = Σ fᵢ/Mᵢ
- **Hill** (VRH平均): M_h = (M_v + M_r) / 2
- **Hashin-Shtrikman** (変分上下界)

### 固溶体モデル

Van Laar モデルによる非対称混合:

```
S_conf = -R × Σ [mₛ × Σ xⱼ ln(xⱼ)]  (配置エントロピー)
G_excess = Σ φₐφᵦBₐᵦ  (過剰Gibbs自由エネルギー)
```

### 相平衡

Gibbs 自由エネルギー最小化により安定相集合体を決定（SVD ベース最適化）。

## ファイル形式

| 拡張子 | 内容 | 形式 |
|--------|------|------|
| `.mine` | 鉱物パラメータ | JSON |
| `.ptpf` | P-T プロファイル | JSON |
| `.vpf` | 組成プロファイル | JSON |
| `.rock` | 岩石組成（複数鉱物＋比率） | JSON |
| `.csv` | 鉱物データ / 計算結果 | CSV |

## アーキテクチャ

本プロジェクトは計算エンジン（Core）とUI（Desktop）を分離した構成です。

```
┌──────────────────────────────────────────┐
│        ThermoElastic.Desktop             │
│        (Avalonia UI / MVVM)              │
│   Windows / macOS / Linux                │
├──────────────────────────────────────────┤
│        ThermoElastic.Core                │
│        (.NET 9 クラスライブラリ)            │
│   EOS計算 │ Debye熱力学 │ 相平衡        │
│   SLB2011鉱物DB │ 固溶体 │ 混合モデル   │
│   PREM参照 │ 断熱線 │ HS境界         │
│   衝撃圧縮 │ 相図計算 │ 惑星内部解析   │
└──────────────────────────────────────────┘
```

Core ライブラリはUI非依存のため、将来的に Web UI やCLIツールなど異なるフロントエンドからも利用可能です。

## 参考文献

- Stixrude, L. & Lithgow-Bertelloni, C. (2011). Thermodynamics of mantle minerals - II. Phase equilibria. *Geophysical Journal International*, 184, 1180-1213.
- Stixrude, L. & Lithgow-Bertelloni, C. (2005). Thermodynamics of mantle minerals - I. Physical properties. *Geophysical Journal International*, 162, 610-632.
- Cottaar, S., Heister, T., Rose, I. & Unterborn, C. (2014). BurnMan: A lower mantle mineral physics toolkit. *Geochemistry, Geophysics, Geosystems*, 15, 1164-1179.
- Dziewonski, A. M. & Anderson, D. L. (1981). Preliminary reference Earth model. *Physics of the Earth and Planetary Interiors*, 25, 297-356.

## ライセンス

Private repository.
