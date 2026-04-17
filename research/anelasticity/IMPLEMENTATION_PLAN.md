# 実装計画: 非弾性補正機能の拡張（TDD版）

## 方針

- **TDD厳守**: RED（失敗テスト） → GREEN（最小実装） → REFACTOR の順序を全ステップで遵守
- **各Phase完了時にビルド＋全テスト通過を確認**
- **最終Phase（Phase 7）で文献値との比較E2Eテストを実施**
- **後方互換性**: 既存 `AnelasticityCalculatorTests` 全7テストが常にパス

---

## Phase 1: 基盤 — モデル抽象化とパラメータDB

### 目的
既存コードを壊さず Strategy パターンの骨格と鉱物別パラメータDBを構築。

### TDD Step 1.1: AnelasticityParams モデル
```
RED:  AnelasticityParamsTests — デフォルト値検証、record immutability、with式テスト
GREEN: AnelasticityParams record 実装
```
**新規ファイル:**
- `Core/Models/AnelasticityParams.cs`
- `Tests/AnelasticityParamsTests.cs`

**フィールド:**
| プロパティ | 型 | デフォルト | 単位 |
|-----------|-----|-----------|------|
| GrainSize_m | double | 0.01 | m (= 1cm) |
| ActivationEnergy | double | 424_000 | J/mol |
| ActivationVolume | double | 10e-6 | m³/mol |
| FrequencyExponent | double | 0.27 | - |
| GrainSizeExponent | double | 3.0 | - |
| PreFactor | double | 4.35e-4 | - |
| WaterContent_ppm | double | 0 | ppm H/Si |
| MeltFraction | double | 0 | - |
| QK | double | 1000 | - |
| RefGrainSize_m | double | 13.4e-6 | m |
| MineralPhase | string | "olivine" | - |

### TDD Step 1.2: IAnelasticityModel インタフェース + 既存クラス適合
```
RED:  既存テスト7本がIAnelasticityModel経由で同一結果を返すことを検証
GREEN: IAnelasticityModel定義、AnelasticityCalculator に実装
```
**新規ファイル:**
- `Core/Calculations/IAnelasticityModel.cs`

**変更ファイル:**
- `Core/Calculations/AnelasticityCalculator.cs` — interface 追加実装（既存メソッドはそのまま維持）

**インタフェース:**
```csharp
public interface IAnelasticityModel
{
    double ComputeQS(double T, double P, double frequency, AnelasticityParams? prms = null);
    AnelasticResult ApplyCorrection(double Vp, double Vs, double KS, double GS,
                                     double T, double P, double frequency,
                                     AnelasticityParams? prms = null);
}
```

### TDD Step 1.3: AnelasticityDatabase 鉱物別パラメータ
```
RED:  GetParamsForMineral("fo") が olivine パラメータを返す、
      GetParamsForMineral("mpv") が bridgmanite パラメータを返す、
      全SLB2011鉱物に対して null でないことを検証
GREEN: AnelasticityDatabase 実装
```
**新規ファイル:**
- `Core/Database/AnelasticityDatabase.cs`
- `Tests/AnelasticityDatabaseTests.cs`

**パラメータセット (研究調査結果に基づく):**
| 鉱物相 | E* (kJ/mol) | V* (cm³/mol) | α | m | ソース |
|--------|-------------|--------------|-----|---|--------|
| Olivine (fo,fa) | 424 | 10 | 0.27 | 3.0 | Jackson & Faul (2010) |
| Wadsleyite (mw,fw) | 370 | 8 | 0.27 | 3.0 | 推定 (olivine類似) |
| Ringwoodite (mrw,frw) | 370 | 8 | 0.27 | 3.0 | 推定 |
| Bridgmanite (mpv,fpv,apv) | 500 | 3 | 0.20 | 2.5 | 推定 (高圧相) |
| Ferropericlase (pe,wu) | 300 | 5 | 0.25 | 3.0 | 推定 |
| その他 | 424 | 10 | 0.27 | 3.0 | olivine デフォルト |

### Phase 1 完了条件
- [ ] 既存 `AnelasticityCalculatorTests` 全7テスト PASS
- [ ] 新規テスト ~10件 PASS
- [ ] `dotnet build` 成功

---

## Phase 2: Tier 2 — パラメトリック Q(T,P,f,d) モデル

### 目的
粒径・圧力依存性を含む現実的なQモデルを実装。

### TDD Step 2.1: ParametricQCalculator コア
```
RED:  (1) QS が粒径増大で増加（粗粒→低減衰）
      (2) QS が温度上昇で減少
      (3) QS が圧力増加で増加
      (4) 粒径13.4μm, 1200°C, 200MPa で QS ~ 20-60 (Jackson & Faul 2010 実験値)
      (5) 粒径1cm, 1400K, 5GPa で QS ~ 80-300 (地震学的に妥当)
      (6) 速度補正が Tier 1 と同オーダー（同条件で差20%以内）
GREEN: ParametricQCalculator 実装
REFACTOR: 共通ロジック抽出
```
**新規ファイル:**
- `Core/Calculations/ParametricQCalculator.cs`
- `Tests/ParametricQCalculatorTests.cs`

**コア式:**
```
Q_S^{-1} = A * (d/d_ref)^{-m·α} * (f/f_ref)^{-α} * exp(-α·(E* + P·V*) / (R·T))
```

premelting 補正は既存 `GetSolidusTemperature()` を再利用。

### Phase 2 完了条件
- [ ] 全テスト PASS（既存7 + Phase 1 + Phase 2 新規 ~6件）
- [ ] 粒径依存性の定性的挙動が正しい

---

## Phase 3: 水効果・メルト効果

### TDD Step 3.1: WaterQCorrector
```
RED:  (1) WaterContent=0 で基底Q値と同一
      (2) WaterContent=100ppm で QS 低下（30-70%に）
      (3) WaterContent=1000ppm でさらに低下
      (4) IAnelasticityModel インタフェースを満たす
GREEN: WaterQCorrector 実装（デコレータパターン）
```
**新規ファイル:**
- `Core/Calculations/WaterQCorrector.cs`
- `Tests/WaterQCorrectorTests.cs`

**式:**
```
Q^{-1}_wet = Q^{-1}_dry * (C_OH / C_ref)^{r·α}
```
r = 1.0（デフォルト、UI調整可能）、C_ref = 50 ppm H/Si

### TDD Step 3.2: MeltQCorrector
```
RED:  (1) MeltFraction=0 で基底値と同一
      (2) MeltFraction=0.01 で Vs 1-3% 追加低下
      (3) MeltFraction=0.05 で Vs 3-8% 追加低下
      (4) 弾性効果（contiguity低下）+ 非弾性効果の両方が反映
GREEN: MeltQCorrector 実装
```
**新規ファイル:**
- `Core/Calculations/MeltQCorrector.cs`
- `Tests/MeltQCorrectorTests.cs`

**式 (Yamauchi & Takei 2016):**
- 弾性: `G_eff = G_0 * (1 - A·φ^{1/2})^2`
- 非弾性: solidus近傍で指数的Q低下

### Phase 3 完了条件
- [ ] 全テスト PASS
- [ ] 水量0/メルト0でデコレータが恒等写像

---

## Phase 4: Tier 3 — Extended Burgers / Andrade 粘弾性モデル

### TDD Step 4.1: AndradeCalculator（解析式、先に実装）
```
RED:  (1) 高周波極限 ω→∞ で J*→J_U（非緩和コンプライアンス）
      (2) QS の周波数依存性が power-law Q ~ f^α に近い
      (3) Tier 2 と同条件で QS が同オーダー（差 50% 以内）
      (4) 速度 V(ω) が周波数低下で単調減少
GREEN: AndradeCalculator 実装
```
**新規ファイル:**
- `Core/Calculations/AndradeCalculator.cs`
- `Core/Models/ViscoelasticResult.cs`
- `Tests/AndradeCalculatorTests.cs`

**式（解析的、数値積分不要）:**
```
J*(ω) = J_U + β·Γ(1+n)·(iω)^{-n} + Δ_P/(1+iω·τ_P) - i/(ω·η_M)
```
n ≈ 1/3, Γ(4/3) = 0.8929... を定数化。

### TDD Step 4.2: ExtendedBurgersCalculator（数値積分）
```
RED:  (1) 高周波極限チェック
      (2) Andrade と地震帯域 (0.01-1Hz) で結果 30% 以内
      (3) Jackson & Faul (2010) Table 4: d=13.4μm, 1200°C, period=100s で QS ~ 30-80
      (4) Kramers-Kronig 近似チェック: ΔV/V ≈ -cot(πα/2)/(2Q)
GREEN: ExtendedBurgersCalculator 実装
REFACTOR: 数値積分の精度/速度最適化
```
**新規ファイル:**
- `Core/Calculations/ExtendedBurgersCalculator.cs`
- `Tests/ExtendedBurgersCalculatorTests.cs`

**式（数値積分）:**
```
J*(ω) = J_U + ∫[τ_L→τ_H] D(τ)/(1+iωτ) dτ - i/(ω·η_M)
D(τ) = Δ·τ^{-α}·exp(-|ln(τ/τ_M)|/(σ)) / ∫normalization
```
対数等間隔200点の台形積分。

### Phase 4 完了条件
- [ ] Andrade と Burgers の交差検証（地震帯域で30%以内）
- [ ] 全テスト PASS

---

## Phase 5: 深度依存 Q プロファイル + PREM Q データ

### TDD Step 5.1: PREM Q データ追加
```
RED:  GetQAtDepth(150) が QS=80（LVZ）を返す
      GetQAtDepth(400) が QS=143（遷移帯）を返す
      GetQAtDepth(1000) が QS=312（下部マントル）を返す
GREEN: PREMModel に GetQAtDepth() メソッド追加
```
**変更ファイル:**
- `Core/Models/PREMModel.cs`
- `Tests/PREMModelTests.cs`（追加）

### TDD Step 5.2: QProfileBuilder
```
RED:  (1) 断熱Tp=1600K で Q(z) が LVZ で最小値
      (2) Q(z) が各深度で PREM Q と1桁以内
      (3) Tp=1700K でQ(z) が全体的に低下
      (4) Vs補正がアセノスフェアで2-5%、下部マントルで<1%
GREEN: QProfileBuilder 実装
```
**新規ファイル:**
- `Core/Models/QProfilePoint.cs`
- `Core/Calculations/QProfileBuilder.cs`
- `Tests/QProfileBuilderTests.cs`

### Phase 5 完了条件
- [ ] PREM Q 値が論文値と一致
- [ ] Q プロファイルの定性的挙動が地球物理学的に妥当

---

## Phase 6: UI拡張

### Step 6.1: AnelasticityViewModel 拡張
既存UIにモデル選択・粒径・水量・メルト入力を追加。

**変更ファイル:**
- `Desktop/ViewModels/AnelasticityViewModel.cs` — モデル選択、粒径、水量、メルト分率、詳細パラメータ追加
- `Desktop/Views/AnelasticityView.axaml` — UI更新

**追加プロパティ:**
- `SelectedModelType` (ComboBox: Simple / ParametricQ / Andrade / ExtendedBurgers)
- `GrainSize_mm` (NumericUpDown, 0.001-100mm)
- `WaterContent_ppm` (NumericUpDown, 0-3000)
- `MeltFraction` (NumericUpDown, 0-0.20)
- `ShowAdvanced` (Expander toggle: E*, V*, α, m)

### Step 6.2: QProfileView 新規
**新規ファイル:**
- `Desktop/ViewModels/QProfileViewModel.cs`
- `Desktop/Views/QProfileView.axaml`
- `Desktop/Views/QProfileView.axaml.cs`

入力: ポテンシャル温度、粒径、周波数、モデル選択
結果: DataGrid（QProfilePoint一覧）+ ΔVs(z)表示

### Step 6.3: ナビゲーション登録
**変更ファイル:**
- `Desktop/ViewModels/MainWindowViewModel.cs` — QProfile ViewModel追加
- `Desktop/Views/MainWindow.axaml` — ボタン＋DataTemplate追加

### Phase 6 完了条件
- [ ] 各モデル選択で Calculate が正常動作
- [ ] QProfile が DataGrid 表示
- [ ] 既存ナビゲーションテスト PASS

---

## Phase 7: 文献値比較 E2E テスト + 統合

### 最重要フェーズ: 文献値との定量的比較

### TDD Step 7.1: 文献値比較E2Eテスト
```
RED: 以下の文献値検証テストを全て作成してから実装修正

(1) Jackson & Faul (2010) Table 4 再現:
    d=13.4μm, T=1200°C, period=100s → QS ~ 30-80
    ParametricQ, Andrade, ExtendedBurgers の3モデルで比較

(2) Cammarano et al. (2003) Figure 7 再現:
    Pyrolite上部マントル、Tp=1300°C → ΔVs ~ 1-3% at 200km
    Q プロファイルが PREM Q と概ね一致

(3) Faul & Jackson (2005) grain-size scaling:
    d=3μm vs d=50μm で QS の比が (d1/d2)^{m·α} ≈ (3/50)^{0.81} ≈ 0.088
    実測比との一致を確認

(4) Goes et al. (2000) / Cammarano et al. (2003) 温度推定バイアス:
    非弾性補正あり/なしで上部マントル温度推定差が 50-200K

(5) Karato (1993) Vs補正公式の直接検証:
    Q=100 で ΔVs/Vs ≈ -cot(πα/2)/(2·100) ≈ -0.66%
    Q=50 で ΔVs/Vs ≈ -1.32%

(6) 水効果: Aizawa et al. (2008) / Cline et al. (2018)
    含水 olivine で QS が dry の 30-70% に低下

(7) メルト効果: McCarthy & Takei (2011)
    1% melt で Vs 1-3% 追加低下

GREEN: 各テストがパスするようにモデルパラメータ/実装を調整
```

**新規ファイル:**
- `Tests/AnelasticityLiteratureE2ETests.cs`

### TDD Step 7.2: 既存計算器との統合テスト
```
RED: (1) RockCalculator + 非弾性補正 → Pyrolite の非弾性 Vs が弾性 Vs より 1-5% 低い
     (2) CompositionInverter + 非弾性補正 → Mg# 推定値が補正なしと 1-3% 異なる
GREEN: RockCalculator, CompositionInverter に optional 非弾性パイプライン追加
```

**変更ファイル:**
- `Core/Calculations/RockCalculator.cs` — `AnelasticModel` オプショナルプロパティ
- `Core/Calculations/CompositionInverter.cs` — misfit計算に非弾性補正組込み
- `Tests/AnelasticityIntegrationTests.cs`

### TDD Step 7.3: Desktop E2E テスト
```
RED: (1) ナビゲーション: ShowQProfile → QProfileViewModel
     (2) AnelasticityViewModel: 各モデル選択 → Calculate → 結果妥当性
GREEN: E2E テストパス確認
```

**変更ファイル:**
- `Desktop.E2E/ViewModelE2ETests.cs` — ナビゲーション追加
- `Desktop.E2E/FullStackE2ETests.cs` — App28 非弾性補正E2Eテスト

### Phase 7 完了条件
- [ ] 文献値比較7項目全て PASS
- [ ] 統合テスト全て PASS
- [ ] 全テストスイート PASS（既存 + 新規）
- [ ] `dotnet build` 成功

---

## ファイル一覧

### 新規ファイル（16件）
| ファイル | Phase | 内容 |
|---------|-------|------|
| `Core/Models/AnelasticityParams.cs` | 1 | パラメータ record |
| `Core/Models/ViscoelasticResult.cs` | 4 | J*(ω) 結果 |
| `Core/Models/QProfilePoint.cs` | 5 | 深度別Q点 |
| `Core/Calculations/IAnelasticityModel.cs` | 1 | Strategy インタフェース |
| `Core/Database/AnelasticityDatabase.cs` | 1 | 鉱物別Qパラメータ |
| `Core/Calculations/ParametricQCalculator.cs` | 2 | Tier 2 モデル |
| `Core/Calculations/WaterQCorrector.cs` | 3 | 水効果デコレータ |
| `Core/Calculations/MeltQCorrector.cs` | 3 | メルト効果デコレータ |
| `Core/Calculations/AndradeCalculator.cs` | 4 | Tier 3b（解析式） |
| `Core/Calculations/ExtendedBurgersCalculator.cs` | 4 | Tier 3a（数値積分） |
| `Core/Calculations/QProfileBuilder.cs` | 5 | Q(z) プロファイル |
| `Desktop/ViewModels/QProfileViewModel.cs` | 6 | Q プロファイル VM |
| `Desktop/Views/QProfileView.axaml` | 6 | Q プロファイル UI |
| `Desktop/Views/QProfileView.axaml.cs` | 6 | code-behind |
| `Tests/AnelasticityLiteratureE2ETests.cs` | 7 | 文献値比較 |
| `Tests/AnelasticityIntegrationTests.cs` | 7 | 統合テスト |

### 変更ファイル（8件）
| ファイル | Phase | 変更内容 |
|---------|-------|---------|
| `Core/Calculations/AnelasticityCalculator.cs` | 1 | IAnelasticityModel 実装 |
| `Core/Models/PREMModel.cs` | 5 | GetQAtDepth() 追加 |
| `Core/Calculations/RockCalculator.cs` | 7 | 非弾性パイプライン |
| `Core/Calculations/CompositionInverter.cs` | 7 | 非弾性補正組込み |
| `Desktop/ViewModels/AnelasticityViewModel.cs` | 6 | モデル選択等 |
| `Desktop/Views/AnelasticityView.axaml` | 6 | UI拡張 |
| `Desktop/ViewModels/MainWindowViewModel.cs` | 6 | QProfile 登録 |
| `Desktop/Views/MainWindow.axaml` | 6 | ボタン追加 |

### テストファイル（8件新規 + 2件変更）
| ファイル | Phase |
|---------|-------|
| `Tests/AnelasticityParamsTests.cs` | 1 |
| `Tests/AnelasticityDatabaseTests.cs` | 1 |
| `Tests/ParametricQCalculatorTests.cs` | 2 |
| `Tests/WaterQCorrectorTests.cs` | 3 |
| `Tests/MeltQCorrectorTests.cs` | 3 |
| `Tests/AndradeCalculatorTests.cs` | 4 |
| `Tests/ExtendedBurgersCalculatorTests.cs` | 4 |
| `Tests/QProfileBuilderTests.cs` | 5 |
| `Tests/AnelasticityLiteratureE2ETests.cs` | 7 |
| `Tests/AnelasticityIntegrationTests.cs` | 7 |

---

## 文献値比較テスト一覧（Phase 7）

| # | 検証項目 | 文献ソース | 期待値 |
|---|---------|-----------|--------|
| 1 | QS(d=13.4μm, 1473K, 10mHz) | Jackson & Faul (2010) Tab.4 | 30-80 |
| 2 | ΔVs at 200km depth, Tp=1573K | Cammarano et al. (2003) Fig.7 | 1-3% |
| 3 | QS grain-size ratio d=3/50μm | Faul & Jackson (2005) | ~0.09 |
| 4 | T推定バイアス（補正あり/なし） | Goes/Cammarano (2000/2003) | 50-200K |
| 5 | ΔVs/Vs at Q=100 (Karato式直接) | Karato (1993) | -0.66% |
| 6 | QS_wet / QS_dry (100ppm H/Si) | Aizawa/Cline (2008/2018) | 0.3-0.7 |
| 7 | ΔVs from 1% melt | McCarthy & Takei (2011) | 1-3% |

---

## リスクと対策

| リスク | 重大度 | 対策 |
|--------|--------|------|
| V* 実験的制約不足 | HIGH | UI調整可能 + 感度解析 |
| 粒径外挿 3桁 | HIGH | プリセット提供 + PREM Q比較 |
| Burgers 数値積分精度 | MEDIUM | Andrade(解析式)と交差検証 |
| CompositionInverter 結果変化 | HIGH | オプショナル(デフォルトoff) |
| Gamma関数(.NETに未実装) | LOW | n=1/3 なら Γ(4/3)=0.8929 定数化 |
