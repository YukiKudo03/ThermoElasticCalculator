# 詳細設計書

## ThermoElasticCalculator

| 項目 | 内容 |
|------|------|
| 文書名 | ThermoElasticCalculator 詳細設計書 |
| バージョン | 0.4.0 |
| 作成日 | 2026-03-10 |

---

## 1. ThermoElastic.Core クラス詳細設計

### 1.1 MineralParams

**ファイル:** `Core/Models/MineralParams.cs`
**責務:** 基準状態における鉱物の熱弾性パラメータを保持する。

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|------|------|
| MineralName | string | 鉱物名 |
| PaperName | string | 論文略称 |
| NumAtoms | int | 単位格子あたり原子数 n |
| MolarVolume | double | V₀ [cm³/mol] |
| MolarWeight | double | Mw [g/mol] |
| KZero | double | K₀ [GPa] |
| K1Prime | double | K₀' [-] |
| K2Prime | double | K₀'' [-] |
| GZero | double | G₀ [GPa] |
| G1Prime | double | G₀' [-] |
| G2Prime | double | G₀'' [-] |
| DebyeTempZero | double | θ₀ [K] |
| GammaZero | double | γ₀ [-] |
| QZero | double | q₀ [-] |
| EhtaZero | double | ηS₀ [-] |
| RefTemp | double | T_ref [K] (default: 300) |
| F0 | double | 基準状態 Helmholtz 自由エネルギー [kJ/mol] |
| Tc0 | double | Landau 臨界温度 (P=0) [K] (0=無効) |
| VD | double | Landau 最大過剰体積 [cm³/mol] |
| SD | double | Landau 最大過剰エントロピー [J/(mol·K)] |
| SpinQuantumNumber | double | スピン量子数 S (0=無効) |
| MagneticAtomCount | double | 磁気原子数/格子単位 r (0=無効) |

#### 算出プロパティ

| プロパティ | 計算式 | 論文対応 |
|-----------|--------|---------|
| AveMolarWeight | Mw / n | - |
| Aii | 6γ₀ | 式(25) |
| Aiikk | -12γ₀ + 36γ₀² - 18γ₀q₀ | 式(26) |
| As | -2γ₀ - 2ηS₀ | Paper I |
| ParamSymbol | "{MineralName} ({PaperName})" | 表示用 |

#### メソッド

| メソッド | 引数 | 戻り値 | 説明 |
|---------|------|--------|------|
| GetPressure(f) | finite strain | double [GPa] | BM3圧力計算 |
| BM3Finite(P) | target P [GPa] | double | 圧力から有限歪みの逆算 |
| BM3KT(f) | finite strain | double [GPa] | Cold part の体積弾性率 |
| BM3GT(f) | finite strain | double [GPa] | Cold part の剪断弾性率 |
| FiniteToVolume(f) | finite strain | double [cm³/mol] | 有限歪み→体積 |
| VolumeToFinite(V) | volume | double | 体積→有限歪み |

#### BM3圧力計算

```
P(f) = 3K₀ × f × (1 + 2f)^(5/2) × [1 + 3(K₀' - 4)f / 2]
```

ここで f は Eulerian 有限歪み: `f = [(V₀/V)^(2/3) - 1] / 2`

#### BM3 体積弾性率

```
KT_cold(f) = (1 + 2f)^(5/2) × K₀ × [1 + (3K₀' - 5)f + (27/2)(K₀' - 4)f²]
```

#### BM3 剪断弾性率

```
GT_cold(f) = (1 + 2f)^(5/2) × [G₀
             + (3K₀G₀' - 5G₀)f
             + (6K₀G₀' - 24K₀ - 14G₀ + 9K₀K₀'/2)f²]
```

---

### 1.2 ThermoMineralParams

**ファイル:** `Core/Models/ThermoMineralParams.cs`
**責務:** 与えられた有限歪みと温度における鉱物の全熱弾性特性を計算する。

#### コンストラクタ引数

| 引数 | 型 | 説明 |
|------|------|------|
| targetFinite | double | 有限歪み f |
| targetTemperature | double | 温度 T [K] |
| mineral | MineralParams | 鉱物パラメータ |

#### コンストラクタ内の計算手順

```
1. μ = 1 + a(1)_ii × f + a(2)_iikk × f² / 2
2. γ(f) = (1/μ) × (2f+1) × (a(1)_ii + a(2)_iikk × f) / 6
3. ηs(f) = -γ - (2f+1)² × As / (2μ)
4. θ(f) = √μ × θ₀
5. DebyeFunctionCalculator を θ(f) で初期化
6. ΔP_th = (γ/V) × ΔE / 1000
7. q(f) = [18γ² - 6γ - a(2)_iikk(2f+1)²/(2μ)] / (9γ)
```

#### 主要プロパティの計算式

| プロパティ | 計算式 | 論文対応 |
|-----------|--------|---------|
| Pressure | P_cold(f) + ΔP_th | - |
| DeltaE | [E(T) - E(T_ref)] × n | 式(19)の積分 |
| CvT | Cv(T) × n × T | Debyeモデル |
| KT | KT_cold + (γ+1-q₀)γΔE/V - γ²ΔCvT/V | 式(16)の熱補正 |
| KS | KT + γ²CvT/V | KS = KT(1 + αγT) |
| GS | GT_cold - ηs×ΔE/V | 式の熱補正 |
| Alpha | γ × CvT / (T × KT × V × 1000) | SLB2005 α=γCv/(KTV) |
| Density | Mw / V | - |
| Vp | √[(KS + 4G/3) / ρ] | - |
| Vs | √[G / ρ] | - |
| Vb | √[KS / ρ] | - |

**単位注意:** DeltaE/V は J/cm³ 単位。1 J/cm³ = 10⁻³ GPa のため `/1000` で GPa に変換。自由エネルギー F, G は kJ/mol。1 GPa·cm³/mol = 1 kJ/mol。

#### v0.4.0 追加プロパティ

| プロパティ | 計算式 | 説明 |
|-----------|--------|------|
| FCold | 9K₀V₀(f²/2 + a₁f³/6) | 圧縮自由エネルギー [kJ/mol] |
| FThermal | n·kB·NA·(T·f_th(θ/T) - T_ref·f_th(θ₀/T_ref))/1000 | 熱自由エネルギー [kJ/mol] |
| HelmholtzF | F₀ + F_cold + F_thermal + F_Landau/1000 + F_mag/1000 | Helmholtz自由エネルギー [kJ/mol] |
| GibbsG | F + P×V | Gibbs自由エネルギー [kJ/mol] |
| Entropy | -∂F/∂T（数値中心差分, ΔT=0.5K） | エントロピー [J/(mol·K)] |
| LandauTc | Tc₀ + V_D×P/S_D | 現在圧力でのLandau臨界温度 [K] |
| LandauFreeEnergy | S_D×[(T-Tc)Q² + Tc·Q⁶/3] | Landau自由エネルギー [J/mol] |
| LandauEntropy | -S_D×Q² | Landauエントロピー [J/(mol·K)] |
| LandauVolume | V_D×Q² | Landau体積補正 [cm³/mol] |
| MagneticFreeEnergy | -T×r×R×ln(2S+1) | 磁気自由エネルギー [J/mol] |
| MagneticEntropy | r×R×ln(2S+1) | 磁気エントロピー [J/(mol·K)] |

---

### 1.3 ResultSummary

**ファイル:** `Core/Models/ResultSummary.cs`
**責務:** 計算結果の集約と出力フォーマッティング。

全15出力項目（P, T, Vp, Vs, Vb, ρ, V, KS, KT, GS, α, θd, γ, ηs, q）を保持。Vp, Vs, Vb は KS, GS, ρ から算出プロパティとして動的計算。

---

### 1.4 PTData / PTProfile

**ファイル:** `Core/Models/PTData.cs`, `Core/Models/PTProfile.cs`

| クラス | 責務 |
|--------|------|
| PTData | 圧力・温度の1データ点 |
| PTProfile | 名前付きP-Tデータ点のリスト |

---

### 1.5 RockComposition / RockMineralEntry

**ファイル:** `Core/Models/RockComposition.cs`
**責務:** 岩石組成（複数鉱物＋体積比率）のデータモデル。

```
RockComposition
  ├── Name: string
  └── Minerals: List<RockMineralEntry>
        ├── Mineral: MineralParams
        └── VolumeRatio: double
```

---

### 1.6 MieGruneisenEOSOptimizer

**ファイル:** `Core/Calculations/MieGruneisenEOSOptimizer.cs`
**責務:** 与えられた (P, T) に対して、熱効果を含む自己無撞着な有限歪みを求める。

#### アルゴリズム

```
入力: MineralParams, P_target, T

1. refPressure = P_target
2. LOOP:
   a. finite = BM3Finite(refPressure)        # cold EOS の逆算
   b. th = ThermoMineralParams(finite, T)    # 熱補正込み計算
   c. P_calc = th.Pressure                   # cold + thermal
   d. IF |P_target - P_calc| < 1e-5 → BREAK
   e. refPressure += (P_target - P_calc)     # 圧力残差で補正
3. RETURN ThermoMineralParams(finite, T)
```

通常2-5回で収束する。

---

### 1.7 MixtureCalculator

**ファイル:** `Core/Calculations/MixtureCalculator.cs`
**責務:** N鉱物の体積比混合による弾性率計算。

#### コンストラクタ

```csharp
MixtureCalculator(List<(double elemRatio, ResultSummary elemResult)> results)
```

#### 混合計算の詳細

**密度:**
```
ρ_mix = Σ(fᵢ × Vᵢ × ρᵢ) / Σ(fᵢ × Vᵢ)
```

**Voigt平均（等歪み上界）:**
```
M_V = Σ fᵢ × Mᵢ    (M = KT, KS, GS)
```

**Reuss平均（等応力下界）:**
```
1/M_R = Σ fᵢ / Mᵢ
```

**Hill平均（VRH）:**
```
M_H = (M_V + M_R) / 2
```

#### バリデーション

- 全比率が 0-1 の範囲内
- 比率合計が 0-1.01 の範囲内
- 全鉱物が同一P-T条件で計算済み

---

### 1.8 VProfileCalculator

**ファイル:** `Core/Calculations/VProfileCalculator.cs`
**責務:** 2成分系の体積比スイープ計算。

Hashin-Shtrikman 下界（2成分系 K₁<K₂, G₁<G₂）:
```
K_HS⁻ = K₁ + f₂ / [1/(K₂-K₁) + f₁/(K₁ + 4G₁/3)]
G_HS⁻ = G₁ + f₂ / [1/(G₂-G₁) + 2f₁(K₁+2G₁)/(5G₁(K₁+4G₁/3))]
```

---

### 1.9 PTProfileCalculator

**ファイル:** `Core/Calculations/PTProfileCalculator.cs`
**責務:** P-Tプロファイル全点に対する単一鉱物の物性計算。

```csharp
public List<ResultSummary> DoProfileCalculationAsSummary()
// 各P-T点でMieGruneisenEOSOptimizerを実行し結果を集約
```

---

### 1.10 RockCalculator

**ファイル:** `Core/Calculations/RockCalculator.cs`
**責務:** 岩石組成の全鉱物を指定P-Tで計算し、混合モデルでバルク物性を算出。

#### 処理フロー

```
1. 各鉱物について MieGruneisenEOSOptimizer で P-T 物性を計算
2. 体積比率を正規化（合計 = 1.0）
3. 正規化比率 + 個別 ResultSummary で MixtureCalculator を生成
4. 指定された MixtureMethod で混合計算
5. (混合ResultSummary, 個別結果リスト) を返却
```

---

### 1.11 DebyeFunctionCalculator

**ファイル:** `Core/Calculations/DebyeFunctionCalculator.cs`
**責務:** Debyeモデルに基づく内部エネルギー・比熱・自由エネルギーを解析的数値積分で計算。

#### 計算方式（v0.4.0 で解析的積分に移行）

v0.3.0 まではルックアップテーブル（470KB）を使用していたが、v0.4.0 で composite Simpson's rule（500分点）による D₃(x) の直接積分に置き換え。精度向上とファイルサイズ削減（470KB → 4KB）を実現。

#### 数式

```
D₃(x) = (3/x³) ∫₀ˣ t³/(eᵗ-1) dt    [Simpson 500分点]
E_atom(T) = 3R·T·D₃(θ/T)             [J/(mol_atom)]
Cv_atom(T) = 3R·[4D₃(x) - 3x/(eˣ-1)] [J/(mol_atom·K)]
f_th(T) = 3·ln(1-e⁻ˣ) - D₃(x)       [無次元, kBT単位]
```

#### メソッド

| メソッド | 引数 | 戻り値 | 説明 |
|---------|------|--------|------|
| DebyeFunction3(x) | x = θ/T | double | D₃(x) Debye関数（static） |
| GetInternalEnergy(T) | T [K] | double [J/mol_atom] | 3R·D₃(θ/T)·T |
| GetCv(T) | T [K] | double [J/(mol_atom·K)] | 3R·[4D₃-3x/(eˣ-1)] |
| GetThermalFreeEnergyPerAtom(T) | T [K] | double [無次元] | 3ln(1-e⁻ˣ)-D₃(x) |

#### 数値安定性

- x < 1e-10: D₃ → 1.0（Dulong-Petit極限）
- x > 150: D₃ → π⁴/(5x³)（漸近展開）
- t > 40: t³/(eᵗ-1) → t³·e⁻ᵗ（オーバーフロー防止）

---

### 1.12 Optimizer

**ファイル:** `Core/Calculations/Optimizer.cs`
**責務:** 1変数非線形方程式 f(x) = 0 の求根。

#### 実装クラス

| クラス | アルゴリズム | 特徴 |
|--------|-----------|------|
| ReglaFalsiOptimizer | 偽位法 | デフォルト。安定性が高い |
| SecantOptimizer | 割線法 | 収束が速い。不安定な場合あり |
| BisectionOptimizer | 二分法 | 最も安定だが遅い |

#### 共通パラメータ

| パラメータ | 値 |
|-----------|------|
| threshold | 1×10⁻⁸ |
| lowerBound | 0.0005 |
| upperBound | 0.02 |

---

### 1.13 MineralCsvIO

**ファイル:** `Core/IO/MineralCsvIO.cs`
**責務:** MineralParams の CSV 入出力。

#### CSV形式

```
MineralName,PaperName,NumAtoms,MolarVolume,MolarWeight,KZero,K1Prime,K2Prime,GZero,G1Prime,G2Prime,DebyeTempZero,GammaZero,QZero,EhtaZero,RefTemp,F0,Tc0,VD,SD,SpinQuantumNumber,MagneticAtomCount
```

**後方互換:** F0以降のフィールドは省略可能（デフォルト0）。

#### メソッド

| メソッド | 説明 |
|---------|------|
| ExportCsvRow(MineralParams) | 1鉱物→CSV行 |
| ImportCsvRow(string) | CSV行→MineralParams |
| ImportCsvFile(path) | CSVファイル→List\<MineralParams\> |
| ExportCsvFile(path, list) | List\<MineralParams\>→CSVファイル |

---

### 1.14 JsonFileIO

**ファイル:** `Core/IO/JsonFileIO.cs`
**責務:** JSON ファイルの汎用読み書き。

```csharp
public static class JsonFileIO
{
    public static T Load<T>(string filePath);
    public static void Save<T>(string filePath, T obj);
    public static string Serialize<T>(T obj);
    public static T Deserialize<T>(string json);
}
```

---

### 1.15 PhysicConstants

**ファイル:** `Core/PhysicConstants.cs`

| 定数 | 値 | 単位 |
|------|------|------|
| GasConst (R) | 8.31477 | J/(mol·K) |
| NA | 6.02×10²³ | mol⁻¹ |
| Boltzman (kB) | 1.38×10⁻²³ | J/K |
| Plank (h) | 6.63×10⁻³⁴ | J·s |
| RefTemperature | 300.0 | K |

---

## 2. ThermoElastic.Desktop 詳細設計

### 2.1 MVVM 構成

#### ViewModel 基底

CommunityToolkit.Mvvm を使用:

```csharp
// 基本パターン
public partial class SomeViewModel : ObservableObject
{
    [ObservableProperty]
    private double _pressure;

    [RelayCommand]
    private void Calculate() { ... }
}
```

### 2.2 MainWindowViewModel

| プロパティ/コマンド | 型 | 説明 |
|-------------------|------|------|
| OpenMineralEditorCommand | IRelayCommand | 鉱物エディタを開く |
| OpenPTProfileCommand | IRelayCommand | P-Tプロファイル画面を開く |
| OpenMixtureCommand | IRelayCommand | 混合計算画面を開く |
| OpenRockCalculatorCommand | IRelayCommand | 岩石計算画面を開く |

### 2.3 MineralEditorViewModel

| プロパティ | 型 | 説明 |
|-----------|------|------|
| MineralName | string | 鉱物名 |
| PaperName | string | 論文略称 |
| NumAtoms | int | 原子数 |
| MolarVolume | double | V₀ |
| ... (全16パラメータ) | | |
| TestPressure | double | テスト計算用P |
| TestTemperature | double | テスト計算用T |
| TestResult | string | テスト結果文字列 |

| コマンド | 説明 |
|---------|------|
| LoadFileCommand | .mine ファイル読込 |
| SaveFileCommand | .mine ファイル保存 |
| ImportCsvCommand | CSV一括インポート |
| ExportCsvCommand | CSVエクスポート |
| TestCalcCommand | テスト計算実行 |

### 2.4 RockCalculatorViewModel

| プロパティ | 型 | 説明 |
|-----------|------|------|
| RockName | string | 岩石名 |
| MineralEntries | ObservableCollection\<MineralEntryVM\> | 鉱物リスト |
| Pressure | double | 圧力 [GPa] |
| Temperature | double | 温度 [K] |
| SelectedMethod | MixtureMethod | 混合モデル |

| コマンド | 説明 |
|---------|------|
| AddMineralCommand | 鉱物追加 |
| RemoveMineralCommand | 選択鉱物削除 |
| LoadRockCommand | .rock 読込 |
| SaveRockCommand | .rock 保存 |
| CalculateCommand | 計算実行 |

#### MineralEntryVM（子ViewModel）

| プロパティ | 型 | 説明 |
|-----------|------|------|
| Mineral | MineralParams | 鉱物データ |
| DisplayName | string | 表示名 |
| VolumeRatio | double | 体積比率 |

### 2.5 ResultsViewModel

| プロパティ | 型 | 説明 |
|-----------|------|------|
| Title | string | 結果タイトル |
| Subtitle | string | 条件情報 |
| ResultTable | DataTable | 結果データ |

| コマンド | 説明 |
|---------|------|
| ExportCsvCommand | CSVエクスポート |

---

## 3. Avalonia View 設計

### 3.1 共通方針

- AXAML でUIを宣言的に定義
- DataContext に ViewModel をバインド
- ファイルダイアログは Avalonia の `IStorageProvider` を使用（プラットフォーム非依存）
- DataGrid は Avalonia の `DataGrid` コントロールを使用

### 3.2 RockCalculatorView.axaml 構造例

```xml
<Window>
  <DockPanel>
    <!-- ヘッダー: 岩石名 + Load/Save -->
    <StackPanel DockPanel.Dock="Top" Orientation="Horizontal">
      <TextBox Text="{Binding RockName}" />
      <Button Command="{Binding LoadRockCommand}" Content="Load Rock..." />
      <Button Command="{Binding SaveRockCommand}" Content="Save Rock..." />
    </StackPanel>

    <!-- 鉱物リスト -->
    <DataGrid ItemsSource="{Binding MineralEntries}"
              AutoGenerateColumns="False">
      <DataGrid.Columns>
        <DataGridTextColumn Header="Mineral" Binding="{Binding DisplayName}" IsReadOnly="True" />
        <DataGridTextColumn Header="Vol. Ratio" Binding="{Binding VolumeRatio}" />
      </DataGrid.Columns>
    </DataGrid>

    <!-- 操作パネル -->
    <StackPanel DockPanel.Dock="Bottom">
      <StackPanel Orientation="Horizontal">
        <Button Command="{Binding AddMineralCommand}" Content="Add Mineral..." />
        <Button Command="{Binding RemoveMineralCommand}" Content="Remove" />
      </StackPanel>
      <StackPanel Orientation="Horizontal">
        <TextBlock Text="P[GPa]" />
        <NumericUpDown Value="{Binding Pressure}" />
        <TextBlock Text="T[K]" />
        <NumericUpDown Value="{Binding Temperature}" />
        <ComboBox ItemsSource="{Binding MixtureMethods}"
                  SelectedItem="{Binding SelectedMethod}" />
        <Button Command="{Binding CalculateCommand}" Content="Calculate" />
      </StackPanel>
    </StackPanel>
  </DockPanel>
</Window>
```

---

## 4. ユニットテスト設計

### 4.1 テスト対象

Core ライブラリの全計算ロジックをテスト対象とする。

| テストクラス | 対象 | テスト内容 |
|------------|------|----------|
| MineralParamsTests | MineralParams | BM3圧力計算、有限歪み変換、CSV入出力 |
| EOSOptimizerTests | MieGruneisenEOSOptimizer | 既知鉱物での収束確認、精度検証 |
| MixtureCalculatorTests | MixtureCalculator | Voigt/Reuss/Hill の計算正確性 |
| CsvIOTests | MineralCsvIO | CSV往復変換の一致確認 |
| DebyeFunctionTests | DebyeFunctionCalculator | 既知温度での内部エネルギー・比熱 |

### 4.2 テスト例

```csharp
[Fact]
public void Forsterite_At10GPa_1500K_ShouldReturnExpectedVp()
{
    var fo = new MineralParams
    {
        MineralName = "Forsterite", NumAtoms = 7,
        MolarVolume = 43.60, MolarWeight = 140.69,
        KZero = 128.0, K1Prime = 4.2,
        GZero = 82.0, G1Prime = 1.5,
        DebyeTempZero = 809.0, GammaZero = 0.99,
        QZero = 2.1, EhtaZero = 2.3, RefTemp = 300.0
    };
    var optimizer = new MieGruneisenEOSOptimizer(fo, 10.0, 1500.0);
    var result = optimizer.ExecOptimize();

    Assert.InRange(result.Vp, 8000, 9500);  // 合理的な範囲
    Assert.InRange(result.Vs, 4500, 5500);
    Assert.InRange(result.Density, 3.3, 3.8);
}
```

---

## 5. 移行チェックリスト

### 5.1 Core 分離

- [ ] MineralParams → Core/Models/MineralParams.cs
- [ ] ThermoMineralParams → Core/Models/ThermoMineralParams.cs
- [ ] ResultSummary → Core/Models/ResultSummary.cs
- [ ] PTData, PTProfile → Core/Models/
- [ ] RockComposition, RockMineralEntry → Core/Models/
- [ ] MieGruneisenEOSOptimizer → Core/Calculations/
- [ ] MixtureCalculator → Core/Calculations/
- [ ] VProfileCalculator → Core/Calculations/
- [ ] PTProfileCalculator → Core/Calculations/
- [ ] RockCalculator → Core/Calculations/
- [ ] DebyeFunctionCalculator → Core/Calculations/
- [ ] Optimizer 各種 → Core/Calculations/
- [ ] CSV入出力 → Core/IO/MineralCsvIO.cs
- [ ] JSON入出力 → Core/IO/JsonFileIO.cs
- [ ] PhysicConstants → Core/

### 5.2 Desktop UI 実装

- [ ] ソリューション構成の作成
- [ ] Avalonia プロジェクトテンプレート生成
- [ ] MainWindow 実装
- [ ] MineralEditorView 実装
- [ ] PTProfileView 実装
- [ ] MixtureView 実装
- [ ] RockCalculatorView 実装
- [ ] ResultsView 実装
- [ ] ファイルダイアログ（IStorageProvider）統合

### 5.3 テスト

- [ ] Core ユニットテストプロジェクト作成
- [ ] 既知鉱物での計算精度検証
- [ ] CSV往復変換テスト
- [ ] JSON往復変換テスト

### 5.4 ビルド・配布

- [x] 各プラットフォーム向けパブリッシュ確認
- [x] GitHub Actions CI 設定

---

## 6. v0.4.0 追加クラス詳細設計

### 6.1 LandauCalculator

**ファイル:** `Core/Calculations/LandauCalculator.cs`
**責務:** 変位型相転移（三臨界 Landau モデル）の計算。SLB2011 式(28)-(32)。

#### メソッド（全 static）

| メソッド | 計算式 | 説明 |
|---------|--------|------|
| GetOrderParameter(T, Tc) | Q = (1 - T/Tc)^(1/4) | 秩序パラメータ（T < Tc） |
| GetTc(P, Tc0, VD, SD) | Tc(P) = Tc₀ + V_D×P/S_D | 圧力依存臨界温度 |
| GetFreeEnergy(T, Tc, SD) | G_t = S_D×[(T-Tc)Q² + Tc·Q⁶/3] | Landau自由エネルギー |
| GetEntropy(T, Tc, SD) | S_L = -S_D×Q² | Landauエントロピー |
| GetVolume(T, Tc, VD) | V_L = V_D×Q² | Landau体積補正 |

**適用鉱物:** Quartz (Tc₀=847K), Stishovite (Tc₀=-4250K)

---

### 6.2 SolutionCalculator

**ファイル:** `Core/Calculations/SolutionCalculator.cs`
**責務:** 固溶体の熱力学計算。SLB2011 式(7), (33)-(37)。

#### メソッド（全 static）

| メソッド | 説明 |
|---------|------|
| GetIdealEntropy(x[], sites) | 配置エントロピー S_conf = -R·Σ[m_s·Σ(x_j·ln(x_j))] |
| GetExcessGibbs(x[], interactions) | Van Laar 過剰 Gibbs: G_ex = Σ φ_a·φ_b·B_ab |
| GetActivityCoefficients(x[], interactions, T) | 活動度係数 γ_i（数値微分） |
| GetChemicalPotential(i, x[], interactions, G_i, T) | 化学ポテンシャル μ_i = G_i + RT·ln(x_i·γ_i) |
| GetEffectiveParams(x[], endmembers) | 線形内挿パラメータ V=Σx_i·V_i 等 |
| ValidateComposition(x[]) | 組成の妥当性検証 (Σx_i = 1) |

---

### 6.3 GibbsMinimizer

**ファイル:** `Core/Calculations/GibbsMinimizer.cs`
**責務:** Gibbs自由エネルギー最小化による安定相集合体の決定。

#### アルゴリズム

- **2相系:** 直接比較（G_A vs G_B）
- **多相系:** SVDベース線形計画法（MathNet.Numerics）

#### 入力/出力

| 入力 | 型 |
|------|------|
| 初期相集合体 | PhaseAssemblage |
| 圧力, 温度 | double, double |

| 出力 | 型 |
|------|------|
| 安定相集合体 | PhaseAssemblage |

---

### 6.4 EquilibriumAggregateCalculator

**ファイル:** `Core/Calculations/EquilibriumAggregateCalculator.cs`
**責務:** P-Tパスに沿って相平衡を再計算し、機械的混合でバルク物性を算出。

#### 処理フロー

```
1. 各P-T点で GibbsMinimizer を実行 → 安定相比率
2. 各安定相の物性を MieGruneisenEOSOptimizer で計算
3. MixtureCalculator で混合 → バルク Vp, Vs, ρ
```

---

### 6.5 PhaseDiagramCalculator

**ファイル:** `Core/Calculations/PhaseDiagramCalculator.cs`
**責務:** 指定温度範囲で相境界圧力を探索。

2相間の相境界: G_A(P,T) = G_B(P,T) を解く。Clapeyron勾配 dP/dT の符号も計算。

---

### 6.6 データモデル（v0.4.0 追加）

#### SolidSolution

```
SolidSolution
├── Name: string
├── Endmembers: List<MineralParams>    # 端成分鉱物
├── Sites: List<SolutionSite>          # 結晶学サイト
└── InteractionParams: List<InteractionParam>  # 相互作用パラメータ
```

#### SolutionSite

```
SolutionSite
├── SiteName: string
├── Multiplicity: double               # サイト多重度 m
└── Occupancies: Dictionary<int, double[]>  # 端成分→占有率マッピング
```

#### InteractionParam

```
InteractionParam
├── EndmemberA: int                    # 端成分Aインデックス
├── EndmemberB: int                    # 端成分Bインデックス
├── W: double                          # 相互作用エネルギー [kJ/mol]
├── SizeA: double                      # サイズパラメータ d_A (default 1.0)
└── SizeB: double                      # サイズパラメータ d_B (default 1.0)
```

#### PhaseAssemblage / PhaseEntry

```
PhaseAssemblage
├── BulkComposition: double[]          # バルク組成
├── Phases: List<PhaseEntry>           # 相リスト
└── TotalGibbs: double                 # 合計 Gibbs [kJ/mol]

PhaseEntry
├── PhaseName: string
├── Endmember: MineralParams?          # 単一端成分の場合
├── Solution: SolidSolution?           # 固溶体の場合
├── Composition: double[]              # 固溶体組成
├── Amount: double                     # モル量
└── GibbsG: double                     # G [kJ/mol]
```

---

### 6.7 SLB2011Endmembers

**ファイル:** `Core/Database/SLB2011Endmembers.cs`
**責務:** SLB2011 Table A1 の全42端成分鉱物パラメータ。

パラメータはBurnManプロジェクトと同一の参照状態補正済み値を使用。v0.4.0 でBurnManとの包括的クロス検証を実施済み。

#### 収録鉱物系

| 系 | 鉱物 |
|---|------|
| Olivine | Forsterite (fo), Fayalite (fa) |
| Wadsleyite | Mg-Wadsleyite (mw), Fe-Wadsleyite (fw) |
| Ringwoodite | Mg-Ringwoodite (mrw), Fe-Ringwoodite (frw) |
| Perovskite | Mg/Fe/Al-Perovskite (mpv, fpv, apv) |
| Post-Perovskite | Mg/Fe/Al-PostPerovskite (mppv, fppv, appv) |
| Ferropericlase | Periclase (pe), Wuestite (wu) |
| SiO₂ | Quartz (qtz), Coesite (coe), Stishovite (st), Seifertite (seif) |
| Garnet | Pyrope (py), Almandine (al), Grossular (gr), Majorite (maj) |
| Pyroxene | Diopside (di), Hedenbergite (he), CaTs (cats), Jadeite (jd), Enstatite (en), Ferrosilite (fs), Mg-Tschermak (mgts), HP-Clinoenstatite (hpcen), HP-Clinoferrosilite (hpcfs) |
| Akimotoite | Mg/Fe-Akimotoite (mak, fak), Corundum (cor) |
| Spinel | Spinel (sp), Hercynite (hc) |
| Other | Anorthite (an), Albite (ab), Ca-Perovskite (capv), Nepheline (neph), Kyanite (ky), Mg-Ilmenite (mil) |

---

## 7. ユニットテスト設計（v0.4.0 追加）

### 7.1 BurnMan クロス検証テスト

BurnMan (Python) で生成したリファレンスCSVデータと本ソフトウェアの計算結果を比較。

| テストクラス | 対象 | テスト数 |
|------------|------|---------|
| BurnManEndmemberVerificationTests | 全端成分×P-Tグリッド | 53+ |
| ThermodynamicIdentityTests | D₃(x), KS-KT, G=F+PV, Gibbs | 30+ |
| LandauSolutionVerificationTests | Landau Q(T), F_mag, 固溶体 | 15+ |
| MixingModelVerificationTests | VRH解析解, PREM比較 | 10+ |

#### 検証精度基準

| 条件 | 許容誤差 |
|------|---------|
| 常温常圧 (0.0001 GPa, 300K) | 密度 1%, KS/G 1%, Vp/Vs 1% |
| 高温高圧 (10-100 GPa, 1500-2500K) | 密度 1%, KS/G/Vp/Vs 3% |
| Debye関数 D₃(x) | scipy参照値と 1% |
| Gibbs自由エネルギー | BurnManと 1% |

#### リファレンスデータ

`tests/ThermoElastic.Core.Tests/TestData/` に配置:

| ファイル | 内容 |
|---------|------|
| burnman_endmember_reference.csv | 42端成分 × 7圧力 × 5温度 = 1468レコード |
| burnman_solution_reference.csv | 固溶体 (olivine, ferropericlase) 28レコード |
| burnman_debye_reference.csv | D₃(x) 参照値 12点 |
