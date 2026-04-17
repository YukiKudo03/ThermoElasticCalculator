# 非弾性・周波数依存性による地震波速度補正に関するリサーチレポート

## 背景

実験室で測定された鉱物の弾性波速度（MHz帯）と地震学的に観測される速度（Hz帯以下）を直接比較する際、
非弾性効果（内部摩擦・エネルギー散逸）による速度減衰と周波数分散が無視できない系統誤差を生じる。
本リサーチでは、この問題に関する近年の研究を12項目にわたり調査した。

## リサーチ完了サマリー

| # | アイテム | カテゴリ | 関連度 | 不確実項目数 |
|---|---------|---------|--------|------------|
| 1 | Extended Burgers Model | Rheological Models | 5 | 2 |
| 2 | Andrade Model | Rheological Models | 5 | 4 |
| 3 | Grain-size sensitivity | Experimental | 5 | 2 |
| 4 | Melt effects on attenuation | Experimental | 5 | 2 |
| 5 | Water effects on attenuation | Experimental | 5 | 4 |
| 6 | High-P attenuation measurements | Experimental | 4 | 13 |
| 7 | Anelastic velocity correction methods | Correction Methods | 5 | 5 |
| 8 | Frequency-dependent dispersion | Correction Methods | 5 | 4 |
| 9 | Upper mantle anelasticity profiles | Applications | 5 | 7 |
| 10 | Transition zone & lower mantle Q | Applications | 4 | 12 |
| 11 | Spin crossover and anelasticity | Emerging | 3 | 14 |
| 12 | Ab initio anelasticity | Emerging | 2 | 9 |

**完了: 12/12 | 平均関連度: 4.3/5**

## 主要な知見

### 1. 速度補正の規模

| 深さ領域 | Vs補正 (%) | Vp補正 (%) | 主要因 |
|---------|-----------|-----------|--------|
| リソスフェア (0-80 km) | < 0.3 | < 0.1 | 低温のため無視可能 |
| アセノスフェア (80-220 km) | 2-5 | 0.5-2 | 高温 + 部分溶融の可能性 |
| 上部マントル (220-410 km) | 1-3 | 0.3-1 | 温度依存の粘弾性緩和 |
| 遷移帯 (410-660 km) | 0.5-2 | 0.2-0.5 | 含水鉱物の効果が不確実 |
| 下部マントル (660-2700 km) | 0.3-1 | 0.1-0.5 | Q高く補正小、ただしD"層では増大 |
| D"層 | 1-3 | 0.5-1.5 | 高温 + 部分溶融 + 化学的不均質 |

### 2. 推奨される補正手法（3段階）

**Tier 1: 簡易Q補正（Karato 1993型）**
```
V_seis / V_anh = 1 - cot(πα/2) / (2Q)
```
- α ≈ 0.27, Q = Q(T, P, f) をパラメトリックモデルから計算
- 本アプリの AnelasticityCalculator に既に実装済み

**Tier 2: パラメトリックQ(T,P,f,d)モデル（Cammarano/Goes型）**
```
Q⁻¹ = A · d⁻ᵐ · f⁻ᵅ · exp(-(E* + PV*) / RT)
```
- 粒径 d、活性化エネルギー E*、活性化体積 V* を含む完全なパラメータ化
- 上部マントルに最適（olivine系での実験データが豊富）

**Tier 3: 完全粘弾性モデル（Extended Burgers / Andrade）**
- 複素コンプライアンス J*(ω) から V(ω) を数値的に計算
- Kramers-Kronig関係で整合性保証
- 最も精度が高いが計算コストも最大

### 3. 最大の不確実性源

1. **活性化体積 V*** — 高圧実験データが乏しく、6-12 cm³/mol の範囲で不確実
2. **粒径 d** — 実験室（μm）とマントル（mm-cm）の外挿に3桁以上のギャップ
3. **含水量の効果** — 水の量と Q への影響の定量関係が未確立（指数 r = 0.5-2.0）
4. **下部マントル鉱物の Q** — bridgmanite, ferropericlase の直接測定がほぼ皆無
5. **バルク弾性率の減衰 Q_K** — 多くのモデルで Q_K → ∞ と仮定されるが検証不十分

### 4. ThermoElasticCalculator への実装推奨

既存の `AnelasticityCalculator` を拡張し、以下の機能追加を推奨：

1. **Q(T,P,f,d) パラメトリックモデル** — olivine, wadsleyite, ringwoodite, bridgmanite, ferropericlase 各相のパラメータセット
2. **粒径パラメータ** — ユーザー設定可能なデフォルト値（上部マントル: d=10mm, 下部マントル: d=5mm）
3. **含水量効果** — optional な水含有量パラメータ（0-1000 ppm wt H₂O）
4. **周波数選択** — デフォルト1Hz（地震波）、ユーザー変更可能
5. **depth-dependent Q profile** — PREM Q モデルとの比較表示
6. **感度解析** — Q パラメータの不確実性が組成推定に及ぼす影響の可視化

## 出力ディレクトリ

```
research/anelasticity/
├── outline.yaml          # リサーチ定義
├── fields.yaml           # フィールド定義
├── REPORT.md             # 本レポート
└── results/              # 12件のJSON結果
    ├── Extended_Burgers_Model.json
    ├── Andrade_Model.json
    ├── Grain-size_sensitivity.json
    ├── Melt_effects_on_attenuation.json
    ├── Water_effects_on_attenuation.json
    ├── High-pressure_attenuation_measurements.json
    ├── Anelastic_velocity_correction_methods.json
    ├── Frequency-dependent_velocity_dispersion.json
    ├── Upper_mantle_anelasticity_profiles.json
    ├── Transition_zone_and_lower_mantle_Q.json
    ├── Spin_crossover_and_anelasticity.json
    └── Ab_initio_anelasticity.json
```

## 主要文献（横断的に頻出）

- Jackson & Faul (2010) — Extended Burgers model parameterization for olivine
- Faul & Jackson (2015) — Updated grain-size and temperature scaling
- Yamauchi & Takei (2016) — Near-solidus parameterization including melt effects
- Cammarano et al. (2003) — Radial Earth model with anelastic corrections
- Stixrude & Lithgow-Bertelloni (2005) — Thermodynamic framework with anelasticity
- Karato (1993) — Foundational Q-based velocity correction formulation
- McCarthy & Takei (2011) — Melt squirt mechanism and poroelastic model
- Lau & Faul (2019) — Updated absorption band model for tidal/seismic frequencies
