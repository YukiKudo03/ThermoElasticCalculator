# ThermoElasticCalculator ユーザーガイド

## 概要

ThermoElasticCalculator は、任意の圧力-温度 (P-T) 条件下におけるマントル鉱物および岩石組成の熱弾性物性を計算するクロスプラットフォーム・デスクトップアプリケーションです。Stixrude & Lithgow-Bertelloni (2005, 2011) の熱力学モデルを実装しており、以下の理論に基づいています：

- **Birch-Murnaghan 3次状態方程式** — 圧力-体積関係の計算
- **Mie-Gruneisen 熱モデル** — 温度効果の計算
- **Debye モデル** — 内部エネルギーと熱容量の計算
- **混合理論** (Voigt, Reuss, Hill, Hashin-Shtrikman) — 複合岩石物性の計算

.NET 8 と Avalonia UI で構築されており、Windows、macOS、Linux で動作します。

---

## インストール

### 必要環境

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### ソースからビルド

```bash
git clone <repository-url>
cd ThermoElasticCalculator
dotnet build ThermoElasticCalculator.sln --configuration Release
```

### 実行

```bash
dotnet run --project src/ThermoElastic.Desktop
```

---

## アプリケーション構成

サイドバーナビゲーション方式で、5つのメインビューで構成されています：

![メインウィンドウ](images/01_main_window.png)

左サイドバーのナビゲーションボタン：

| ボタン | ビュー | 説明 |
|--------|--------|------|
| **Mineral Library** | 鉱物パラメータエディタ | 鉱物データの読込・編集・保存 |
| **PT Profile** | P-Tプロファイル計算 | P-T経路に沿った物性計算 |
| **Mixture** | 2鉱物混合計算 | 二成分鉱物混合物の物性計算 |
| **Rock Calculator** | 多鉱物岩石計算 | 複合岩石の物性計算 |
| **Results** | 結果表示 | 計算結果の表示とエクスポート |

---

## Mineral Library（鉱物ライブラリ）

鉱物パラメータの管理を行うメインインターフェースです。鉱物データの読込、編集、保存、テスト計算が可能です。

### 鉱物パラメータ一覧

各鉱物は以下のパラメータで定義されます：

| パラメータ | 記号 | 単位 | 説明 |
|-----------|------|------|------|
| Mineral Name | - | - | 鉱物名（例：「Forsterite」） |
| Paper Name | - | - | 略称（例：「fo」） |
| NumAtoms (n) | n | - | 単位格子あたりの原子数 |
| Molar Volume (V0) | V₀ | cm³/mol | 基準状態でのモル体積 |
| Molar Weight (Mw) | Mw | g/mol | モル質量 |
| K0 | K₀ | GPa | 基準状態での等温体積弾性率 |
| K0' | K'₀ | - | Kの圧力1次微分 |
| K0'' | K''₀ | GPa⁻¹ | Kの圧力2次微分 |
| G0 | G₀ | GPa | 基準状態でのせん断弾性率 |
| G0' | G'₀ | - | Gの圧力1次微分 |
| G0'' | G''₀ | GPa⁻¹ | Gの圧力2次微分 |
| Debye Temp (theta0) | θ₀ | K | 基準状態でのDebye温度 |
| Gamma0 | γ₀ | - | Gruneisen パラメータ |
| q0 | q₀ | - | γの体積対数微分 |
| etaS0 | ηS₀ | - | γのせん断ひずみ微分 |
| Ref Temp | Tref | K | 基準温度（通常300 K） |

### ファイル操作

- **Load .mine** — 鉱物パラメータファイル（JSON形式）を読込
- **Save .mine** — 現在のパラメータを `.mine` ファイルとして保存
- **Import CSV** — CSVファイルから鉱物データをインポート
- **Export CSV** — 鉱物データをCSVとしてエクスポート

### テスト計算

圧力（GPa）と温度（K）を入力し、**Calculate** をクリックすると、その条件での熱弾性物性が計算されます。パラメータの検証に便利です。

---

## P-T Profile（P-Tプロファイル計算）

鉱物の物性をP-Tプロファイル（地温勾配、断熱経路など）に沿って計算します。

### 操作手順

1. **Load Mineral...** をクリックして `.mine` ファイルを選択
2. 以下のいずれか：
   - **Load PT Profile...** をクリックして `.ptpf` ファイルを読込
   - **Add P-T Point** をクリックして左側データグリッドに手動でP-Tデータを追加
3. **Calculate** をクリックして各P-T点での物性を計算
4. 結果が右側のデータグリッドに表示

### P-Tプロファイル形式

P-Tプロファイルは `.ptpf` ファイル（JSON）として保存されます：

```json
{
  "Name": "マントル地温勾配",
  "Profile": [
    { "Pressure": 1.0, "Temperature": 500.0 },
    { "Pressure": 5.0, "Temperature": 1000.0 },
    { "Pressure": 10.0, "Temperature": 1500.0 }
  ]
}
```

### 出力物性

各P-T点について以下の物性が計算されます：

| 物性 | 記号 | 単位 | 説明 |
|------|------|------|------|
| P | P | GPa | 圧力 |
| T | T | K | 温度 |
| Vp | VP | m/s | P波速度 |
| Vs | VS | m/s | S波速度 |
| Vb | VΦ | m/s | バルク音速 |
| Density | ρ | g/cm³ | 密度 |
| Volume | V | cm³/mol | モル体積 |
| KS | KS | GPa | 断熱体積弾性率 |
| KT | KT | GPa | 等温体積弾性率 |
| GS | G | GPa | せん断弾性率 |
| Alpha | α | K⁻¹ | 熱膨張係数 |
| Debye Temp | θ | K | Debye温度 |
| Gamma | γ | - | Gruneisen パラメータ |
| etaS | ηS | - | せん断ひずみ微分 |
| q | q | - | γの体積微分 |

### エクスポート

**Export CSV...** をクリックしてCSVファイルとして保存できます。

---

## Mixture（2鉱物混合計算）

二成分（2鉱物）混合物の熱弾性物性を体積分率の関数として計算します。

### 操作手順

1. **Load Mineral 1...** と **Load Mineral 2...** をクリックして2つの鉱物を選択
2. **Pressure**（GPa）と **Temperature**（K）の条件を設定
3. 組成範囲を定義：
   - **Ratio Start**、**Ratio End**、**Ratio Step** を設定
   - **Generate** をクリックして体積比リストを生成
   - Ratioは鉱物1の体積分率（0.0 = 純粋な鉱物2、1.0 = 純粋な鉱物1）
4. 混合 **Method** を選択（Voigt, Reuss, Hill, HS）
5. **Calculate** をクリック

### 混合手法

| 手法 | 説明 |
|------|------|
| **Voigt** | 等ひずみ（上界）: M = Σ fi · Mi |
| **Reuss** | 等応力（下界）: 1/M = Σ fi / Mi |
| **Hill** | VoigtとReussの算術平均 |
| **HS** | Hashin-Shtrikman 変分限界 |

### VProfile 保存/読込

- **Save VProfile...** — 現在の鉱物ペアと体積比リストを `.vpf` ファイルとして保存
- **Load VProfile...** — 以前に保存したVProfile設定を復元

### エクスポート

**Export CSV...** をクリックして結果をエクスポートできます。

---

## Rock Calculator（岩石計算）

任意の数の鉱物相と体積分率からなる多鉱物岩石組成の物性を計算します。

### 操作手順

1. **Rock Name** を入力
2. **Add Mineral...** をクリックして鉱物相を追加（`.mine` ファイルから読込）
3. 鉱物リストの **Volume Ratio** 列を編集して体積分率を設定（合計が1.0になるように）
4. **Pressure**（GPa）と **Temperature**（K）を設定
5. 混合 **Method** を選択
6. **Calculate** をクリック

### 岩石組成ファイル

岩石組成は `.rock` ファイル（JSON）として保存/読込が可能です：

- **Save Rock...** — 現在の鉱物リストと体積比を保存
- **Load Rock...** — 以前に定義した岩石組成を読込

### 出力

結果には以下が含まれます：
- 指定P-T条件での各鉱物の個別物性
- 選択した平均化手法で計算された混合（複合）岩石物性

**Export CSV...** をクリックして全結果を保存できます。

---

## Results（結果表示）

計算結果をデータグリッド形式で表示・エクスポートするための画面です。

**Export CSV...** をクリックして表示中の結果をエクスポートできます。

---

## ファイル形式

| 拡張子 | 内容 | 形式 |
|--------|------|------|
| `.mine` | 単一鉱物パラメータ | JSON |
| `.ptpf` | 圧力-温度プロファイル | JSON |
| `.vpf` | 体積プロファイル（2鉱物混合設定） | JSON |
| `.rock` | 岩石組成（N鉱物＋体積比） | JSON |
| `.csv` | 鉱物データまたは計算結果 | CSV |

すべてのJSONファイルは人間が読める形式であり、テキストエディタで編集可能です。

---

## CSV形式

### 鉱物CSV（インポート/エクスポート）

```
MineralName,PaperName,NumAtoms,MolarVolume,MolarWeight,KZero,K1Prime,K2Prime,GZero,G1Prime,G2Prime,DebyeTempZero,GammaZero,QZero,EhtaZero,RefTemp
Forsterite,fo,7,43.6,140.69,128.0,4.2,0,82.0,1.5,0,809.0,0.99,2.1,2.3,300.0
```

### 結果CSV

```
P[GPa], T[K], Vp[m/s], Vs[m/s], Vb[m/s], ρ[g/cm3], V[cm3/mol], KS[GPa], KT[GPa], GS[GPa], α[K-1], θd[K], γ, ηs, q
```

---

## 使用例：Forsterite の物性計算

1. **Mineral Library** ビューを開く
2. 以下のパラメータを入力：
   - Mineral Name: `Forsterite`
   - Paper Name: `fo`
   - NumAtoms: `7`
   - Molar Volume: `43.6`
   - Molar Weight: `140.69`
   - K0: `128.0`, K0': `4.2`, K0'': `0`
   - G0: `82.0`, G0': `1.5`, G0'': `0`
   - Debye Temp: `809.0`
   - Gamma0: `0.99`, q0: `2.1`, etaS0: `2.3`
   - Ref Temp: `300`
3. **Test Calculation** で P = `5` GPa、T = `1000` K を設定
4. **Calculate** をクリックして計算結果を確認
5. **Save .mine** をクリックして鉱物データを保存

---

## 参考文献

- Stixrude, L. & Lithgow-Bertelloni, C. (2005). Thermodynamics of mantle minerals - I. Physical properties. *Geophysical Journal International*, 162(2), 610-632.
- Stixrude, L. & Lithgow-Bertelloni, C. (2011). Thermodynamics of mantle minerals - II. Phase equilibria. *Geophysical Journal International*, 184(3), 1180-1213.
