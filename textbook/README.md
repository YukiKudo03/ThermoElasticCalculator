# 計算鉱物物理学 — 熱力学理論から地球内部構造の解明へ

## Computational Mineral Physics: From Thermodynamic Theory to Earth's Interior

**A Comprehensive Graduate-Level Textbook**

---

## 概要

本教科書は、ThermoElasticCalculator プロジェクトの全機能を体系化した、大学院レベルの計算鉱物物理学の包括的テキストです。高温高圧条件下におけるマントル鉱物の熱弾性的性質の理論的基礎から、地球深部構造の地震学的解釈に至るまでを、31章・8部構成で網羅しています。

### 対象読者

- 固体地球物理学・鉱物物理学の大学院生
- 地震学的観測と鉱物物理学的制約を統合する研究者
- 惑星科学における内部構造モデリングに携わる研究者
- 計算地球科学のツール開発者

### 本書の特徴

1. **理論と計算の直結**: 全ての方程式が ThermoElasticCalculator のコードと直接対応
2. **段階的な導出**: Birch-Murnaghan EOS から SLB2011 完全定式化まで体系的に展開
3. **豊富な計算例**: 実際の鉱物パラメータを用いた数値例を各章に3題以上収録
4. **演習問題**: 基礎的な導出問題から研究レベルの課題まで、各章8-10題
5. **最新のトピック**: 非弾性補正、スピン転移、ベイズ逆解析など最先端の手法を収録

---

## 目次

### 第I部: 基礎理論 (Foundations)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第1章 | [高圧下における固体の熱力学](chapters/ch01_thermodynamics_high_pressure.md) | ch01 |
| 第2章 | [デバイモデルと熱的性質](chapters/ch02_debye_model.md) | ch02 |
| 第3章 | [グリュナイゼンパラメータと非調和性](chapters/ch03_gruneisen_parameter.md) | ch03 |
| 第4章 | [弾性率と地震波速度](chapters/ch04_elastic_moduli_velocities.md) | ch04 |

### 第II部: SLB2011 フレームワーク (The SLB2011 Framework)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第5章 | [Stixrude-Lithgow-Bertelloni 定式化](chapters/ch05_slb2011_formulation.md) | ch05 |
| 第6章 | [Mie-Grüneisen 状態方程式ソルバー](chapters/ch06_eos_solver.md) | ch06 |
| 第7章 | [SLB2011 鉱物データベース](chapters/ch07_mineral_database.md) | ch07 |
| 第8章 | [Landau 相転移](chapters/ch08_landau_transitions.md) | ch08 |
| 第9章 | [磁気的寄与](chapters/ch09_magnetic_contributions.md) | ch09 |

### 第III部: 混合体と岩石の物性 (Mixtures and Rock Properties)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第10章 | [弾性混合モデル](chapters/ch10_elastic_mixing.md) | ch10 |
| 第11章 | [岩石組成と鉱物集合体](chapters/ch11_rock_compositions.md) | ch11 |

### 第IV部: 相平衡 (Phase Equilibria)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第12章 | [ギブズ自由エネルギーと相安定性](chapters/ch12_gibbs_phase_stability.md) | ch12 |
| 第13章 | [相図と相境界](chapters/ch13_phase_diagrams.md) | ch13 |
| 第14章 | [ポストペロブスカイト相転移](chapters/ch14_post_perovskite.md) | ch14 |

### 第V部: 地震学的解釈 (Seismic Interpretation)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第15章 | [PREM と一次元地球モデル](chapters/ch15_prem.md) | ch15 |
| 第16章 | [非弾性と Q](chapters/ch16_anelasticity.md) | ch16 |
| 第17章 | [感度カーネル](chapters/ch17_sensitivity_kernels.md) | ch17 |
| 第18章 | [組成逆問題](chapters/ch18_composition_inversion.md) | ch18 |

### 第VI部: 深部地球構造 (Deep Earth Structures)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第19章 | [沈み込むスラブ](chapters/ch19_subducting_slabs.md) | ch19 |
| 第20章 | [LLSVP と ULVZ](chapters/ch20_llsvp_ulvz.md) | ch20 |
| 第21章 | [惑星内部構造](chapters/ch21_planetary_interiors.md) | ch21 |

### 第VII部: 物質の輸送特性と物理 (Transport and Material Properties)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第22章 | [鉄のスピン転移](chapters/ch22_spin_crossover.md) | ch22 |
| 第23章 | [鉄の分配](chapters/ch23_iron_partitioning.md) | ch23 |
| 第24章 | [マントル中の水](chapters/ch24_water_in_mantle.md) | ch24 |
| 第25章 | [熱伝導率と電気伝導率](chapters/ch25_conductivity.md) | ch25 |
| 第26章 | [弾性異方性](chapters/ch26_elastic_anisotropy.md) | ch26 |

### 第VIII部: 発展的手法 (Advanced Methods)

| 章 | タイトル | ファイル |
|----|---------|---------|
| 第27章 | [衝撃圧縮とユゴニオ](chapters/ch27_shock_hugoniot.md) | ch27 |
| 第28章 | [非線形最小二乗法とパラメータフィッティング](chapters/ch28_parameter_fitting.md) | ch28 |
| 第29章 | [ベイズ推定と MCMC](chapters/ch29_bayesian_mcmc.md) | ch29 |
| 第30章 | [地質圧力計と地質温度計](chapters/ch30_geobarometry.md) | ch30 |
| 第31章 | [酸素フガシティ](chapters/ch31_oxygen_fugacity.md) | ch31 |

---

## 統計情報

| 項目 | 値 |
|------|-----|
| 総章数 | 31 |
| 総行数 | ~20,300 |
| 総文字数 | ~1,500,000 |
| 方程式数 | ~200 |
| 計算例数 | ~90 |
| 演習問題数 | ~260 |
| 推奨図表数 | ~160 |
| 参考文献数 | ~200 |

## ThermoElasticCalculator との対応

各章の「コード対応」セクションで、理論的概念と ThermoElasticCalculator の具体的なクラス・メソッドとの対応関係を明示しています。読者は理論を学びながら、実際のコードで計算を実行し、結果を確認することができます。

## ライセンス

本教科書は ThermoElasticCalculator プロジェクトの一部として作成されています。
