# 教科書計画: Computational Mineral Physics — From Thermodynamic Theory to Earth's Interior

## 概要

本プロジェクト ThermoElasticCalculator の全機能を体系化した、大学院レベルの教科書構成案。
31章・8部構成で、鉱物物理学の熱力学的基礎から地球深部構造の解釈までを網羅する。

## リサーチ完了サマリー

- **全31章のリサーチ完了**: 31/31 (100%)
- **失敗**: 0件
- **不確実フィールド合計**: 68件（主に数値例の近似値と文献詳細）
- **出力ディレクトリ**: `research/textbook/results/`

## 教科書構成

### Part I: Foundations（基礎理論）— 4章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch01 | Thermodynamics of Solids at High Pressure | MineralParams, BM3 | 0 |
| Ch02 | Debye Model and Thermal Properties | DebyeFunctionCalculator | 1 |
| Ch03 | Gruneisen Parameter and Anharmonicity | ThermoMineralParams | 0 |
| Ch04 | Elastic Moduli and Seismic Velocities | Vp, Vs, KS, GS | 4 |

### Part II: The SLB2011 Framework — 5章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch05 | The Stixrude-Lithgow-Bertelloni Formulation | 全SLB2011パイプライン | 4 |
| Ch06 | Mie-Gruneisen Equation of State Solver | MieGruneisenEOSOptimizer | 0 |
| Ch07 | SLB2011 Mineral Database | SLB2011Endmembers (46鉱物) | 1 |
| Ch08 | Landau Phase Transitions | LandauCalculator | 1 |
| Ch09 | Magnetic Contributions | SpinQuantumNumber, F_mag | 1 |

### Part III: Mixtures and Rock Properties — 2章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch10 | Elastic Mixing Models | MixtureCalculator, VRH, HS | 9 |
| Ch11 | Rock Compositions and Assemblages | RockCalculator, PredefinedRocks | 6 |

### Part IV: Phase Equilibria — 3章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch12 | Gibbs Free Energy and Phase Stability | HelmholtzF, GibbsG, Entropy | 1 |
| Ch13 | Phase Diagrams and Boundaries | PhaseDiagramCalculator | 6 |
| Ch14 | Post-Perovskite Transition | PostPerovskiteCalculator | 3 |

### Part V: Seismic Interpretation — 4章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch15 | PREM and Radial Earth Models | PREMModel | 2 |
| Ch16 | Anelasticity and Q | 全Anelasticityモデル | 3 |
| Ch17 | Sensitivity Kernels | SensitivityKernelCalculator | 2 |
| Ch18 | Composition Inversion | CompositionInverter | 2 |

### Part VI: Deep Earth Structures — 3章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch19 | Subducting Slabs | SlabThermalModel | 6 |
| Ch20 | LLSVPs and ULVZs | LLSVPCalculator, ULVZCalculator | 1 |
| Ch21 | Planetary Interiors | PlanetaryInteriorSolver, Mars | 1 |

### Part VII: Transport and Material Properties — 5章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch22 | Iron Spin Crossover | SpinCrossoverCalculator | 2 |
| Ch23 | Iron Partitioning | IronPartitioningSolver | 1 |
| Ch24 | Water in the Mantle | WaterContentEstimator | 2 |
| Ch25 | Thermal and Electrical Conductivity | Thermal/ElectricalConductivity | 3 |
| Ch26 | Elastic Anisotropy | ElasticTensorCalculator | 2 |

### Part VIII: Advanced Methods — 5章

| 章 | タイトル | コード対応 | 不確実項目 |
|----|---------|-----------|-----------|
| Ch27 | Shock Compression and Hugoniot | HugoniotCalculator | 1 |
| Ch28 | Nonlinear Least Squares and Parameter Fitting | LM, EOSFitter, ThermoElasticFitter | 0 |
| Ch29 | Bayesian Inversion and MCMC | MCMCSampler | 1 |
| Ch30 | Geobarometry and Geothermometry | ClassicalGeobarometer | 1 |
| Ch31 | Oxygen Fugacity | OxygenFugacityCalculator | 1 |

## 各章の内容構成（全章共通）

各章JSONには以下のフィールドが含まれている：

1. **chapter_summary** — 3-5段落の概要（大学院レベル）
2. **prerequisite_knowledge** — 前提知識リスト
3. **key_equations** — LaTeX形式の主要方程式（変数定義・物理的解釈付き）
4. **physical_concepts** — コアとなる物理概念（定義・直感・よくある誤解）
5. **key_references** — 必読文献リスト
6. **worked_examples** — 2-3の計算例（問題文・解法・数値・解釈）
7. **connections_to_other_chapters** — 他章との関連
8. **computational_methods** — 数値計算手法の説明
9. **open_questions** — 未解決の研究課題
10. **exercise_suggestions** — 5-8の演習問題（基礎～研究レベル）
11. **figure_suggestions** — 推奨図表
12. **historical_context** — 歴史的背景

## 教科書化への次のステップ

### Phase 1: 原稿執筆
- 各章JSONの内容を基に、日本語（または英語）の章原稿を執筆
- 各章 20-30 ページ、全体 700-900 ページを想定
- LaTeX/Markdown で管理

### Phase 2: 図表作成
- figure_suggestions に基づき、ThermoElasticCalculator で計算結果を生成
- matplotlib/ScottPlot で出版品質のグラフを作成

### Phase 3: 演習問題の充実
- exercise_suggestions を基に、解答付き演習問題を整備
- ThermoElasticCalculator を使った実習課題

### Phase 4: コードとの連携
- 各章に対応する ThermoElasticCalculator の使用チュートリアルを追加
- Jupyter Notebook 形式の実習教材

### Phase 5: レビューと出版
- 専門家によるピアレビュー
- 出版社との協議（Cambridge UP, Elsevier, Springer 等）

## 類似教科書との差別化

| 既存書籍 | 本書の差別化ポイント |
|---------|-------------------|
| Poirier (2000) "Introduction to the Physics of the Earth's Interior" | 計算実装との直結、最新のSLB2011フレームワーク |
| Anderson (1995) "Equations of State" | より広範な応用（惑星科学、逆問題、非弾性） |
| Stacey & Davis (2008) "Physics of the Earth" | 定量的計算例の充実、ソフトウェアとの連携 |
| SLB (2011) 原論文 | 教科書としての体系化、段階的な導出 |

## ファイル構成

```
research/textbook/
├── outline.yaml          # 全31章のアウトライン定義
├── fields.yaml           # 各章のフィールド定義
├── TEXTBOOK_PLAN.md      # 本計画書
└── results/              # 31章の調査結果JSON
    ├── Ch01_Thermodynamics_of_Solids_at_High_Pressure.json
    ├── Ch02_Debye_Model_and_Thermal_Properties.json
    ├── ... (29 more chapters)
    └── Ch31_Oxygen_Fugacity.json
```
