# 詳細設計書

## ThermoElasticCalculator

| 項目 | 内容 |
|------|------|
| 文書名 | ThermoElasticCalculator 詳細設計書 |
| バージョン | 0.3.0 |
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
GT_cold(f) = (1 + 2f)^(5/2) × [G₀ + (1+2f)^(5/2)
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
| Alpha | γ × 3CvT / (nKTV) | 式(18) |
| Density | Mw / V | - |
| Vp | √[(KS + 4G/3) / ρ] | - |
| Vs | √[G / ρ] | - |
| Vb | √[KS / ρ] | - |

**単位注意:** 内部計算は kJ/mol 系。GPa への変換は `/1000` 係数。

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
**責務:** Debyeモデルに基づく内部エネルギーと比熱をテーブル参照で計算。

#### テーブル構成

| テーブル | エントリ数 | キー | 説明 |
|---------|----------|------|------|
| debyeInternalSpecificHeatList | 1001 | θ/T × 1000 | Cv/(3nR) の値 |
| debyeFunctionValueList | 1001 | θ/T × 1000 | 内部エネルギー値 |

**比率レンジ:** θ/T = 0 ～ 1000（刻み: 1）

#### 補間方法

```
index = (int)(θ/T * 1000)
fraction = θ/T * 1000 - index
value = table[index] + fraction × (table[index+1] - table[index])
```

#### メソッド

| メソッド | 引数 | 戻り値 | 説明 |
|---------|------|--------|------|
| GetInternalEnergy(T) | T [K] | double [kJ/mol/atom] | ΔE from ref |
| GetCv(T) | T [K] | double [kJ/mol/atom/K] | 定容比熱 |

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
MineralName,PaperName,NumAtoms,MolarVolume,MolarWeight,KZero,K1Prime,K2Prime,GZero,G1Prime,G2Prime,DebyeTempZero,GammaZero,QZero,EhtaZero,RefTemp
```

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

- [ ] 各プラットフォーム向けパブリッシュ確認
- [ ] GitHub Actions CI 設定（任意）
