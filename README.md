# ThermoElasticCalculator

高温高圧条件下におけるマントル鉱物の弾性的性質と、多鉱物混合体（岩石）の物性を計算するクロスプラットフォーム デスクトップアプリケーションです。

## 概要

Stixrude & Lithgow-Bertelloni (2011) "Thermodynamics of mantle minerals - II. Phase equilibria" に基づく熱弾性物理学の式を実装し、以下の計算を行います。

- 任意の温度・圧力条件における単一鉱物の弾性波速度、密度、弾性率等の物性計算
- P-T プロファイルに沿った物性変化の計算
- 複数鉱物の混合モデル（Voigt / Reuss / Hill / Hashin-Shtrikman）による岩石物性の計算

## 主な機能

| 機能 | 説明 |
|------|------|
| Mineral Library | 鉱物パラメータの作成・編集・保存（JSON / CSV） |
| P-T Profile Calculator | P-T プロファイルに沿った単一鉱物の物性計算 |
| Mixture Calculator | 2鉱物の体積比混合による組成特性計算 |
| Rock Calculator | 任意数の鉱物を組み合わせた岩石物性の計算 |

## 動作環境

- **Windows** 10 / 11
- **macOS** 12 (Monterey) 以降
- **Linux** (Ubuntu 22.04+, Fedora 38+, その他主要ディストリビューション)
- .NET 8.0 ランタイム

## プロジェクト構成

```
ThermoElasticCalculator/
├── ThermoElasticCalculator.sln
├── README.md
├── docs/                              # 設計ドキュメント
│   ├── requirements.md                # 要件定義書
│   ├── basic-design.md                # 基本設計書
│   └── detailed-design.md             # 詳細設計書
├── references/                        # 参考文献
│   └── Stixrude and Lithgow 2011.pdf
├── src/
│   ├── ThermoElastic.Core/            # 計算エンジン（クラスライブラリ）
│   │   ├── Models/                    # データモデル
│   │   ├── Calculations/             # 計算ロジック
│   │   ├── IO/                       # ファイル入出力
│   │   └── ThermoElastic.Core.csproj
│   └── ThermoElastic.Desktop/        # デスクトップUI（Avalonia）
│       ├── Views/                     # 画面定義（AXAML）
│       ├── ViewModels/               # ViewModel
│       └── ThermoElastic.Desktop.csproj
└── tests/
    └── ThermoElastic.Core.Tests/     # ユニットテスト
        └── ThermoElastic.Core.Tests.csproj
```

## ビルド方法

### 前提条件

- .NET 8.0 SDK

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
MineralName,PaperName,NumAtoms,MolarVolume,MolarWeight,KZero,K1Prime,K2Prime,GZero,G1Prime,G2Prime,DebyeTempZero,GammaZero,QZero,EhtaZero,RefTemp
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

### 混合モデル

- **Voigt** (等歪み上界): M_v = Σ fᵢMᵢ
- **Reuss** (等応力下界): 1/M_r = Σ fᵢ/Mᵢ
- **Hill** (VRH平均): M_h = (M_v + M_r) / 2
- **Hashin-Shtrikman** (変分上下界)

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
│        (.NET 8 クラスライブラリ)            │
│   計算エンジン・データモデル・ファイルI/O     │
└──────────────────────────────────────────┘
```

Core ライブラリはUI非依存のため、将来的に Web UI やCLIツールなど異なるフロントエンドからも利用可能です。

## 参考文献

- Stixrude, L. & Lithgow-Bertelloni, C. (2011). Thermodynamics of mantle minerals - II. Phase equilibria. *Geophysical Journal International*, 184, 1180-1213.
- Stixrude, L. & Lithgow-Bertelloni, C. (2005a). Thermodynamics of mantle minerals - I. Physical properties. *Geophysical Journal International*, 162, 610-632.

## ライセンス

Private repository.
