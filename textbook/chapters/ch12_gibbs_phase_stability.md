# 第12章 Gibbs 自由エネルギーと相安定性

## 12.1 概要

Gibbs 自由エネルギー $G$ は、所与の圧力 $P$ と温度 $T$ において安定な相を決定する中心的な熱力学ポテンシャルである。地球物理学では圧力と温度が自然な独立変数であるため、$G(P, T)$ が相安定性を支配する適切なポテンシャルとなる。

Stixrude-Lithgow-Bertelloni（SLB）枠組みでは、すべての熱力学量は体積と温度の関数としての Helmholtz 自由エネルギー $F(V, T)$ から出発して計算される。$F$ は冷圧縮エネルギー、Debye 熱エネルギー、Landau 転移エネルギー、磁気的寄与の和として分解される：

$$F = F_0 + F_{\text{cold}} + F_{\text{thermal}} + F_{\text{Landau}} + F_{\text{mag}}$$

Gibbs 自由エネルギーは Legendre 変換によって得られる：

$$\boxed{G = F + PV}$$

コードベースの `ThermoMineralParams` クラスでは `HelmholtzF` として $F_0 + F_{\text{cold}} + F_{\text{thermal}} + F_{\text{Landau}} + F_{\text{mag}}$ を計算し、`GibbsG = HelmholtzF + Pressure * Volume` として $G$ を得る。ここで `MieGruneisenEOSOptimizer` が対象圧力での平衡体積（有限歪み）を事前に求めている。

エントロピーは Helmholtz 自由エネルギーの温度微分から数値的に計算される：

$$S = -\left(\frac{\partial F}{\partial T}\right)_V \approx -\frac{F(T + \delta T) - F(T - \delta T)}{2\,\delta T}$$

相安定性は基本原理に従う：所与の $P$, $T$ で最小の $G$ を持つ相が安定である。2つの相の $G$ が等しくなる条件 $G_1(P, T) = G_2(P, T)$ は相境界を定義し、Clapeyron 方程式がその $P$-$T$ 空間における傾きを支配する：

$$\frac{dP}{dT} = \frac{\Delta S}{\Delta V}$$

この枠組みにより、マントルの鉱物学を深さの関数として計算し、相境界における地震波不連続面を予測し、地震学的観測と比較するための密度・速度プロファイルの計算が可能となる。

---

## 12.2 前提知識

本章を理解するために、以下の前提知識が必要である。

1. **Helmholtz 自由エネルギーの分解**: $F(V, T)$ の冷圧縮項、熱的項、補正項への分解（第5章）。

2. **Mie-Gruneisen 状態方程式とソルバー**: 対象圧力における平衡体積を求める EOS オプティマイザ（第6章）。

3. **Debye モデル**: 熱エネルギー、比熱、熱的自由エネルギーの計算（第2章）。

4. **Euler 有限歪み理論と Birch-Murnaghan 圧縮エネルギー**: 冷圧縮エネルギー $F_{\text{cold}}$ の導出（第1章）。

5. **熱力学ポテンシャル間の Legendre 変換**: $F$ から $G$ への $PV$ 項の付加。

6. **Landau 理論**: 変位型相転移の自由エネルギーへの寄与（第8章）。

7. **数値微分法**: 中心差分近似による微分の計算。

8. **基本的な最適化手法**: 二分法、勾配降下法による最小化。

---

## 12.3 理論と方程式

### 12.3.1 Helmholtz 自由エネルギーから Gibbs 自由エネルギーへ

SLB 枠組みでは、Debye モデルと有限歪み表現が体積の関数として自然に定式化されるため、すべての計算は $F(V, T)$ から始まる。しかし地質学的問題では圧力と温度が指定されるため、Legendre 変換が計算枠組み（体積基盤の EOS）と物理的に関連する独立変数（$P$, $T$）を橋渡しする。

$$\boxed{G = F + PV = F_0 + F_{\text{cold}} + F_{\text{thermal}} + F_{\text{Landau}} + F_{\text{mag}} + PV}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $G$ | Gibbs 自由エネルギー | kJ/mol |
| $F$ | 全 Helmholtz 自由エネルギー | kJ/mol |
| $P$ | 圧力 | GPa |
| $V$ | モル体積 | cm$^3$/mol |
| $F_0$ | 標準状態における参照 Helmholtz エネルギー | kJ/mol |
| $F_{\text{cold}}$ | Birch-Murnaghan EOS からの冷圧縮エネルギー | kJ/mol |
| $F_{\text{thermal}}$ | Debye 熱的自由エネルギー | kJ/mol |
| $F_{\text{Landau}}$ | Landau 相転移自由エネルギー | kJ/mol |
| $F_{\text{mag}}$ | 磁気的自由エネルギー寄与 | kJ/mol |

ここで重要な単位の恒等式がある：$1 \text{ GPa} \cdot \text{cm}^3/\text{mol} = 1 \text{ kJ/mol}$ であるため、$PV$ の積は直接 kJ/mol 単位で得られる。

### 12.3.2 冷圧縮エネルギー

冷圧縮エネルギーは3次 Birch-Murnaghan EOS の体積積分として得られる：

$$\boxed{F_{\text{cold}} = 9 K_0 V_0 \left(\frac{f^2}{2} + \frac{K'_0 - 4}{6} f^3\right)}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $K_0$ | 参照等温体積弾性率 | GPa |
| $V_0$ | 参照モル体積 | cm$^3$/mol |
| $f$ | Euler 有限歪み | 無次元 |
| $K'_0$ | ゼロ圧力での体積弾性率の圧力微分 | 無次元 |

これは等温圧縮時に蓄積される弾性歪みエネルギーであり、`ThermoMineralParams.FCold` で実装されている。高圧において全 Helmholtz 自由エネルギーへの主要な寄与の一つである。

### 12.3.3 Debye 熱的自由エネルギー

格子振動からの熱的寄与は Debye モデルに基づく：

$$\boxed{F_{\text{thermal}} = n\left[k_B T\left(3\ln(1 - e^{-\theta/T}) - D_3(\theta/T)\right) - k_B T_{\text{ref}}\left(3\ln(1 - e^{-\theta_0/T_{\text{ref}}}) - D_3(\theta_0/T_{\text{ref}})\right)\right]\frac{N_A}{1000}}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $n$ | 組成式あたりの原子数 | 無次元 |
| $k_B$ | Boltzmann 定数 | J/K |
| $T$ | 温度 | K |
| $T_{\text{ref}}$ | 参照温度（300 K） | K |
| $\theta$ | 現在の有限歪みにおける Debye 温度 | K |
| $\theta_0$ | 参照 Debye 温度 | K |
| $D_3$ | 3次 Debye 関数 | 無次元 |
| $N_A$ | Avogadro 数 | mol$^{-1}$ |

`ThermoMineralParams.FThermal` で計算される。参照状態の減算により、標準条件（$f = 0$, $T = T_{\text{ref}}$）で $F_{\text{thermal}} = 0$ となることが保証される。

### 12.3.4 エントロピーの数値微分

エントロピーは Helmholtz 自由エネルギーの温度に関する負の微分として定義される：

$$\boxed{S = -\left(\frac{\partial F}{\partial T}\right)_V \approx -\frac{F(T + \delta T) - F(T - \delta T)}{2\,\delta T}}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $S$ | エントロピー | J/(mol$\cdot$K) |
| $F$ | Helmholtz 自由エネルギー | kJ/mol（J 単位に変換するため1000を乗ずる） |
| $T$ | 温度 | K |
| $\delta T$ | 数値微分のステップ幅（コード内で0.5 K） | K |

`ThermoMineralParams.Entropy` プロパティは、同一の有限歪みで $T + 0.5$ K と $T - 0.5$ K の2つの `ThermoMineralParams` オブジェクトを生成し、それぞれの `HelmholtzF` を計算して中心差分をとる。この方法により、Debye 振動項、Landau 過剰エントロピー、磁気的エントロピーを含むすべてのエントロピー寄与が自動的に捕捉される。

中心差分のステップ幅 $\delta T = 0.5$ K は、滑らかな Debye 関数に対して打切り誤差が $O(\delta T^2)$ であるため、0.1% 以下の精度が得られる。これは実験的不確定性の範囲内で十分な精度である。

> **注意**: 一定有限歪みでのエントロピー（定積微分）と一定圧力でのエントロピー（定圧微分）には差がある。これは温度変化時に一定圧力下で体積がわずかに変化するためであり、テスト `GibbsFreeEnergyTests.Entropy_NumericalDerivativeCheck` が両方のアプローチ間で5%以内の整合性を検証している。

### 12.3.5 相平衡条件

2つの相が共存する熱力学的条件は、それらの Gibbs 自由エネルギーが等しいことである：

$$\boxed{G_1(P, T) = G_2(P, T)}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $G_1$ | 相1の Gibbs 自由エネルギー | kJ/mol |
| $G_2$ | 相2の Gibbs 自由エネルギー | kJ/mol |
| $P$ | 平衡圧力 | GPa |
| $T$ | 平衡温度 | K |

`PhaseDiagramCalculator.FindPhaseBoundary` は二分法を用いて $G_1 - G_2$ の符号が変わる圧力を探索し、$|\Delta G| < 0.01$ kJ/mol の許容誤差内で平衡境界に収束する。`GibbsMinimizer` はこれを多相系に一般化する。

### 12.3.6 Clapeyron 方程式

相境界上では $dG_1 = dG_2$ が成立する。各相について $dG = VdP - SdT$ であるから：

$$(V_2 - V_1)dP = (S_2 - S_1)dT$$

$$\boxed{\frac{dP}{dT} = \frac{\Delta S}{\Delta V}}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $dP/dT$ | $P$-$T$ 空間における相境界の傾き | GPa/K |
| $\Delta S$ | 相間のエントロピー差 $S_2 - S_1$ | J/(mol$\cdot$K) |
| $\Delta V$ | 相間の体積差 $V_2 - V_1$ | cm$^3$/mol |

`PhaseDiagramCalculator.ComputeClapeyronSlope` は境界点で両相の EOS を解き、$dP/dT = \Delta S / \Delta V \times 0.001$ [GPa/K] を返す。$0.001$ の因子は J/(cm$^3$) から GPa への単位変換に対応する（$1$ J/cm$^3$ $= 0.001$ GPa）。逆形式 $dT/dP = \Delta V / \Delta S \times 1000$ [K/GPa] も `GibbsMinimizer.ClapeyronSlope` で実装されている。

Clapeyron 傾斜の符号と大きさは重要な地球物理学的情報を含む：

- **正の傾斜**: 高圧相のエントロピーが低い（マントル遷移のほとんどに典型的）
- **負の傾斜**: 高圧相のエントロピーが高い（660 km 不連続面のポストスピネル転移）
- **大きさ**: 境界圧力がマントルの地温勾配に沿ってどの程度シフトするかを制御する

---

## 12.4 物理概念

### 12.4.1 Gibbs 自由エネルギーと相安定性判定基準

一定圧力・温度において、最小のモル Gibbs 自由エネルギーを持つ相が熱力学的に安定である：

$$G = F + PV = H - TS$$

ここで $H = F + PV + TS$ はエンタルピーである。$G$ はエンタルピー（高圧で密に結合した相を有利にする）とエントロピー（高温で無秩序な膨張した相を有利にする）の**競合**を表す。

- **高圧下**: $PV$ 項が大きな体積にペナルティを課し、より高密度な多形体が安定化する
- **高温下**: $-TS$ 項が高エントロピーを報酬し、より大きな振動自由度を持つ相が安定化する
- **相転移**: $P$ または $T$ の増加がこのバランスを傾けたときに生じる

> **よくある誤解**:
> - $F$ と $G$ の安定性基準の混同：$F$ は定積・定温で安定性を決定し、$G$ は定圧・定温で安定性を決定する。地質学的過程は近似的に定圧・定温で起こるため、$G$ が適切なポテンシャルである。
> - 最低エネルギー（$U$ や $H$）の相が常に安定であるという仮定。特に高温ではエントロピー寄与がエンタルピー安定性を覆しうる。
> - $G$ はモルあたり（または整合的な規格化での組成式あたり）で比較する必要がある。

### 12.4.2 Helmholtz から Gibbs への Legendre 変換

Gibbs 自由エネルギー $G(P, T)$ は Helmholtz 自由エネルギー $F(V, T)$ から Legendre 変換によって得られる：

$$G = F + PV$$

この変換は自然変数を体積 $V$ からその共役変数である圧力 $P$ に置き換える。

SLB 定式化は Debye モデルと有限歪み表現が体積の関数として自然であるため、すべてを $F(V, T)$ から計算する。しかし実験的・地球物理学的には圧力を制御する。コード内では EOS オプティマイザがまず対象 $P$ を与える体積 $V$ を求め、次に $PV$ を $F$ に加える。

> **重要**: $F$ と $G$ は異なる物理的情報を含むわけではない。同一系の等価な記述であり、変換は独立変数を変更するだけである。

### 12.4.3 数値微分によるエントロピー

各自由エネルギー成分（Debye 熱的、Landau、磁気的）の温度微分の解析的表現を導出・実装する代わりに、コードは実用的な数値的アプローチをとる。温度を $\pm 0.5$ K だけ摂動させることで、中心差分は $O(\delta T^2)$ の打切り誤差で正確な近似を与える。

このアプローチの利点：
- すべてのエントロピー寄与を自動的に含む（Landau 転移や磁気的無秩序を含む）
- 各項の別個の解析微分コードの維持が不要
- 滑らかな Debye 関数に対して $0.5$ K のステップ幅で $0.1\%$ 以下の精度

> **数値微分の精度**: 数値微分が解析的微分より常に精度が低いわけではない。ここでは滑らかな Debye 関数に対する $0.5$ K ステップの中心差分が実験的不確定性を十分に下回る精度を達成する。

### 12.4.4 Clapeyron 傾斜と相境界の幾何学

Clapeyron 方程式の導出を再確認する。相境界上で2つの相の Gibbs 自由エネルギーは等しい。境界に沿った微小変化では：

$$dG_1 = dG_2$$

各相について $dG = VdP - SdT$ であるから：

$$V_1 dP - S_1 dT = V_2 dP - S_2 dT$$

整理すると：

$$(V_2 - V_1)dP = (S_2 - S_1)dT$$

$$\frac{dP}{dT} = \frac{S_2 - S_1}{V_2 - V_1} = \frac{\Delta S}{\Delta V}$$

**olivine-wadsleyite 転移の場合**: 高圧相（wadsleyite）はより密（$\Delta V < 0$）でより秩序的（$\Delta S < 0$）であるため、正の Clapeyron 傾斜を与える（境界圧力は温度とともに増加）。

**ポストスピネル転移の場合**: 660 km 深度で負の Clapeyron 傾斜を持ち、沈み込むスラブの停滞など深遠な地球力学的影響を及ぼす。

> **注意**: Clapeyron 傾斜は境界全体で一定ではない。$\Delta S$ と $\Delta V$ はともに $P$ と $T$ に依存するため、傾斜は局所的に評価する必要がある。

### 12.4.5 参照状態と $F_0$ の慣例

参照 Helmholtz エネルギー $F_0$ は自由エネルギー計算の絶対スケールを設定し、異なる鉱物相間での $G$ の有意味な比較を可能にする。

- **$F_0$ なし**: 単一相内の自由エネルギー差のみが意味を持つ
- **$F_0$ あり**: forsterite と wadsleyite の $G$ を比較して相安定性を決定できる
- **$F_0$ の起源**: SLB2011 データベースの値は実験的相平衡データと熱量測定から決定される

コード内では $F_0$ は計算された $F_{\text{cold}} + F_{\text{thermal}}$ に単純に加算される。$F_0 = 0$（デフォルト）の場合でも弾性特性や地震波速度は正しく得られるが、相安定性は決定できない。

> **重要**: $F_0$ は弾性率や地震波速度に影響しない。$K$, $G$, $V$ は $F$ の体積や歪みに関する微分であり、定数 $F_0$ は消去される。$F_0$ は相間の $G$ 比較にのみ関わる。

---

## 12.5 計算例

### 12.5.1 計算例1: Forsterite の Gibbs 自由エネルギー

**問題**: $P = 10$ GPa、$T = 1500$ K における forsterite（$\text{Mg}_2\text{SiO}_4$）の Gibbs 自由エネルギーを計算し、$G = F + PV$ を検証せよ。

**解法**:

1. **EOS ソルバー**: `MieGruneisenEOSOptimizer` を用いて $P = 10$ GPa、$T = 1500$ K での平衡体積（有限歪み $f$）を求める。

2. **冷圧縮エネルギー**:
$$F_{\text{cold}} = 9 \times 128.0 \times 43.6 \times \left(\frac{f^2}{2} + \frac{4.2 - 4}{6} f^3\right) \text{ kJ/mol}$$

3. **熱的自由エネルギー**: Debye 関数を用いて $\theta(f)$ と $T = 1500$ K で $F_{\text{thermal}}$ を計算。

4. **全 Helmholtz エネルギー**:
$$F = F_0 + F_{\text{cold}} + F_{\text{thermal}} = -2055.403 + F_{\text{cold}} + F_{\text{thermal}} \text{ kJ/mol}$$

5. **Gibbs エネルギー**:
$$G = F + PV = F + 10 \times V \text{ kJ/mol}$$

**パラメータ値**:

| パラメータ | 値 | 出典 |
|-----------|-----|------|
| $F_0$ | $-2055.403$ kJ/mol | SLB2011 データベース |
| $V_0$ | $43.6$ cm$^3$/mol | SLB2011 データベース |
| $K_0$ | $128.0$ GPa | SLB2011 データベース |
| $K'_0$ | $4.2$ | SLB2011 データベース |

**検証**: テスト `GibbsFreeEnergyTests.GibbsG_EqualsF_Plus_PV` がこの恒等式を小数点以下6桁まで確認している。

**物理的解釈**: マントル遷移帯条件での Gibbs 自由エネルギーは、$PV$ 寄与（$\sim 400$ kJ/mol @ 10 GPa）により周囲条件の $F_0$ から大きくずれる。この $PV$ 項こそが深部でのより高密度な多形体の安定化を駆動する。

### 12.5.2 計算例2: エントロピーの数値微分

**問題**: $P = 5$ GPa、$T = 1000$ K における forsterite のエントロピーを Helmholtz 自由エネルギーの数値微分により計算せよ。

**解法**:

`ThermoMineralParams.Entropy` の実装に従い：

1. 所与の有限歪みで $F(T + 0.5)$ と $F(T - 0.5)$ を計算する。
2. 中心差分でエントロピーを求める：
$$S = -\frac{F(1000.5) - F(999.5)}{2 \times 0.5} \times 1000 \text{ J/(mol} \cdot \text{K)}$$

**数値パラメータ**:

| パラメータ | 値 |
|-----------|-----|
| $P$ | 5 GPa |
| $T$ | 1000 K |
| 内部 $\delta T$ | 0.5 K（`ThermoMineralParams.Entropy` 内） |
| 検証 $\delta T$ | 1.0 K（`GibbsFreeEnergyTests` 内） |

**期待される性質**:
- $T > 0$ K で $S > 0$（熱力学第三法則と整合）
- 内部数値微分と外部数値微分が $5\%$ 以内で一致（有限歪み差を考慮）

**物理的解釈**: マントル条件でのエントロピーは Debye 振動寄与が支配的である。有限歪みを固定した場合（定積微分）と各温度で再最適化した場合（定圧微分）で整合する結果が得られ、実装の熱力学的整合性が検証される。

### 12.5.3 計算例3: Olivine-Wadsleyite 相境界と Clapeyron 傾斜

**問題**: $T = 1600$ K における olivine-wadsleyite 相境界を求め、Clapeyron 傾斜 $dP/dT$ を計算せよ。

**解法**:

1. `PhaseDiagramCalculator.FindPhaseBoundary` を用いて、forsterite と wadsleyite を2相として $P = 10$-$20$ GPa の範囲で探索する。

2. 境界圧力で `ComputeClapeyronSlope` を呼び出し、$\Delta S / \Delta V$ を評価する。

**パラメータ値**:

| パラメータ | Forsterite | Wadsleyite |
|-----------|-----------|------------|
| $V_0$ | 43.6 cm$^3$/mol | 40.52 cm$^3$/mol |
| $F_0$ | $-2055.403$ kJ/mol | $-2028.226$ kJ/mol（不確定） |

**予想される結果**:

| 物理量 | 予想値 |
|--------|--------|
| 境界圧力 | $\sim 13$-$14$ GPa（410 km 深度相当） |
| Clapeyron 傾斜 | $\sim 3$-$4$ MPa/K（正、実験データと整合） |

**計算過程**:
$$\frac{dP}{dT} = \frac{\Delta S \text{ [J/(mol} \cdot \text{K)]}}{\Delta V \text{ [cm}^3\text{/mol]}} \times 0.001 \text{ [GPa} \cdot \text{cm}^3\text{/J]}$$

**物理的解釈**: 正の Clapeyron 傾斜は遷移圧力が温度とともに増加することを意味する。マントルの地温勾配（$\sim 0.3$ K/km）に沿って、これは 410 km 地震波不連続面に対応する。Clapeyron 傾斜は横方向の温度変動に伴う不連続面深度の変化を制約し、これは地震学的研究で観測可能である。

---

## 12.6 計算手法

### 12.6.1 Helmholtz と Gibbs の計算フロー

`MieGruneisenEOSOptimizer` が対象 $(P, T)$ での有限歪み $f$ に収束した後、`ThermoMineralParams` のコンストラクタが一回のパスですべての状態変数を計算する：

1. **$F_{\text{cold}}$**: 解析的 Birch-Murnaghan 歪みエネルギー公式を使用
$$F_{\text{cold}} = 9 K_0 V_0 \left(\frac{f^2}{2} + \frac{K'_0 - 4}{6} f^3\right)$$

2. **$F_{\text{thermal}}$**: `DebyeFunctionCalculator.GetThermalFreeEnergyPerAtom` を呼び出し、$3\ln(1 - e^{-x}) - D_3(x)$ を評価する。$D_3$ は複合 Simpson 則による数値積分で計算。

3. **$F$**: $F_0 + F_{\text{cold}} + F_{\text{thermal}} + \text{Landau} + \text{磁気的寄与}$ を合計。

4. **$G$**: $F + PV$ を加算。

### 12.6.2 数値微分によるエントロピー

`Entropy` プロパティは $\delta T = 0.5$ K の中心差分を使用する。同一有限歪みで $(f, T + 0.5)$ と $(f, T - 0.5)$ の2つの `ThermoMineralParams` オブジェクトを生成し（**EOS を再解せず**有限歪みを固定）、以下を計算する：

$$S = -\frac{F_{+} - F_{-}}{1.0} \times 1000 \text{ J/(mol} \cdot \text{K)}$$

ガード条件 $T < \delta T + 1$ の場合は極低温での除算問題を回避するため $0$ を返す。

### 12.6.3 相境界の二分法探索

`PhaseDiagramCalculator.FindPhaseBoundary` は圧力（または温度）にわたる二分法を用いて $G_1 - G_2 = 0$ の点を探索する：

1. 端点 $P_{\min}$ と $P_{\max}$ で $\Delta G = G_1 - G_2$ を評価
2. 符号変化がなければ `NaN` を返す（範囲内に境界なし）
3. 最大50回の反復で区間を半分に分割
4. $|\Delta G| < 0.01$ kJ/mol で収束と判定

`FindMultiPhaseBoundary` は化学量論係数を持つ多相反応に一般化する。

### 12.6.4 Gibbs 最小化

多相集合体に対して `GibbsMinimizer` は射影勾配降下法を使用する：

1. 各相の化学ポテンシャル $\mu_i = G_i$（固溶体の混合寄与を含む）を計算
2. 平均を減算して方向ベクトルを形成
3. 直線探索で相の量を更新
4. 量が $\epsilon = 10^{-10}$ 以下の相を除去
5. $|\sum(\text{方向} \times \mu)| < 10^{-8}$ で収束と判定

純粋な端成分の2相比較の場合は、単純な $G$ 値の直接比較が使用される。

### 12.6.5 性能特性

| 操作 | EOS 評価回数 |
|------|------------|
| 1回の $G$ 評価 | 1回（オプティマイザ: $\sim 10$-$30$ 反復） |
| 相境界探索 | $\sim 15$-$20$ 回の $G$ 評価（二分法） |
| 数値エントロピー | 追加2回の `ThermoMineralParams` 構築（EOS 再解なし） |
| 相図グリッド（$50 \times 50$） | $2500 \times n_{\text{phases}}$ 回の EOS 評価 |

---

## 12.7 コード対応

本章で解説した概念は、ThermoElasticCalculator の以下のクラスおよびメソッドに対応する。

| 概念 | クラス / メソッド |
|------|------------------|
| Helmholtz 自由エネルギー | `ThermoMineralParams.HelmholtzF` |
| Gibbs 自由エネルギー | `ThermoMineralParams.GibbsG` |
| 冷圧縮エネルギー | `ThermoMineralParams.FCold` |
| 熱的自由エネルギー | `ThermoMineralParams.FThermal` |
| エントロピー（数値微分） | `ThermoMineralParams.Entropy` |
| 相平衡（2相） | `PhaseDiagramCalculator.FindPhaseBoundary` |
| Clapeyron 傾斜（$dP/dT$） | `PhaseDiagramCalculator.ComputeClapeyronSlope` |
| Clapeyron 傾斜（$dT/dP$） | `GibbsMinimizer.ClapeyronSlope` |
| 多相最小化 | `GibbsMinimizer.Minimize` |
| 参照エネルギー $F_0$ | `SLB2011Endmembers`（各端成分のパラメータ） |

**ワークフロー**: 相平衡計算の全体的な流れは以下の通りである：

1. 候補鉱物ごとに、`MieGruneisenEOSOptimizer` で対象 $(P, T)$ での平衡体積を求める
2. $F = F_0 + F_{\text{cold}} + F_{\text{thermal}} + \text{補正}$ を計算
3. $G = F + PV$ を計算
4. 候補相にわたって $G$ を比較し、安定な集合体を同定

**テスト**: `GibbsFreeEnergyTests` が以下の恒等式と整合性を検証する：
- `GibbsG_EqualsF_Plus_PV`: $G = F + PV$ の恒等式（小数点以下6桁）
- `Entropy_NumericalDerivativeCheck`: 定積・定圧エントロピーの整合性（$5\%$ 以内）

---

## 12.8 歴史

### 12.8.1 Gibbs の遺産

Gibbs 自由エネルギーによる相安定性解析の起源は、J. Willard Gibbs の画期的論文 *"On the Equilibrium of Heterogeneous Substances"*（1876-1878）に遡る。この論文は、所与の $P$ と $T$ で最小の化学ポテンシャル（モル Gibbs エネルギー）を持つ相が安定であることを確立した。

### 12.8.2 Clapeyron と Clausius

Clapeyron 方程式はさらに古く、Benoit Paul Emile Clapeyron が1834年に水の気液平衡を記述するために導出し、後に Rudolf Clausius によって一般化された。この方程式は200年近くにわたって相境界の記述に不可欠な道具であり続けている。

### 12.8.3 鉱物物理学への応用

鉱物物理学において Gibbs 自由エネルギーからマントル相集合体を予測する手法は、1960年代から1970年代の Ringwood、秋本らによる先駆的な高圧実験とともに本格化した。計算的アプローチは Ita and Stixrude（1992）によって大きく前進し、自己無撞着な熱力学定式化からマントル鉱物学と地震波速度を予測できることが実証された。

### 12.8.4 SLB 枠組みの成熟

SLB2005/2011 の論文は、$F_0$ 参照エネルギーの包括的データベースと $F(V, T)$ から $G$ を計算する厳密な枠組みを提供し、この手法を成熟させた。

### 12.8.5 数値微分の実用哲学

エントロピーの数値微分アプローチは一見素朴に見えるが、計算鉱物物理学における実用的な哲学を反映している。自由エネルギー関数が複数の複雑な寄与（Debye 熱的、Landau、磁気的、固溶体混合）を含む場合、数値微分はしばしば各項の別個の解析微分コードを維持するよりも信頼性が高い。このアプローチは BurnMan、Perple_X、HeFESTo を含む現代の熱力学ソフトウェアパッケージで広く使用されている。

---

## 12.9 未解決課題

1. **数値微分 vs 解析微分の精度**: 数値微分によるエントロピー値は、特に $F$ にキンクが生じる Landau 転移温度近傍で、完全に解析的な $F$ の微分とどの程度比較されるか。

2. **二分法の精度**: 二分法に基づく相境界探索は、Clapeyron 傾斜をヤコビアンとして用いる Newton-Raphson 法などのより洗練された手法と比較してどの程度の精度か。

3. **多成分固溶体の取り扱い**: 化学ポテンシャルが組成に依存する多成分固溶体について、`GibbsMinimizer` は相の量と組成の同時最適化をどのように扱うべきか。

4. **$F_0$ の第一原理改善**: 密度汎関数理論計算を用いて $F_0$ 参照エネルギーを系統的に改善し、実験的相平衡データへの依存を低減することは可能か。

5. **非調和補正の影響**: 準調和 Debye モデルを超える非調和補正は、極高温（$> 3000$ K）での相境界位置にどのような定量的効果を持つか。

6. **速度論的障壁の組み込み**: $G$ 最小化からの熱力学的平衡予測と並行して、特に沈み込むスラブ中の準安定相について、相変態への速度論的障壁をどのように組み込むべきか。

---

## 12.10 演習問題

### 演習 12.1: $G = F + PV$ の検証

`ThermoMineralParams` を用いて、$P = 25$ GPa、$T = 2000$ K における3種の鉱物（forsterite, wadsleyite, perovskite）について $G = F + PV$ の恒等式を検証せよ。高圧で $PV$ 項が支配的となる理由を説明せよ。

### 演習 12.2: エントロピーの温度依存性

$P = 0.0001$ GPa（常圧近似）で $T = 300, 500, 1000, 1500, 2000$ K における forsterite のエントロピーを計算せよ。$S$ vs $T$ をプロットし、Debye モデルによる比熱 $C_V$ の予測と形状を比較せよ。$S = \int (C_V / T) dT$ の関係を説明せよ。

### 演習 12.3: 相境界のトレース

`PhaseDiagramCalculator` を用いて、$T = 1000$ K から $2500$ K まで20点で forsterite-wadsleyite 相境界をトレースせよ。$P$-$T$ 空間で境界をプロットし、Clapeyron 傾斜が一定か温度とともに変化するか判定せよ。

### 演習 12.4: Clapeyron 傾斜の計算と比較

$T = 1500$ K における olivine-wadsleyite 転移の Clapeyron 傾斜 $dP/dT$ を `ComputeClapeyronSlope` で計算せよ。MPa/K に変換し、Katsura et al.（2004）の実験値（$\sim 3.6$ MPa/K）と比較せよ。不一致の原因を議論せよ。

### 演習 12.5: $F_0$ の影響の調査

Forsterite と wadsleyite の両方で $F_0 = 0$ に設定し、相境界を求めることを試みよ。次に正しい $F_0$ 値を使用して繰り返せ。$F_0$ が相平衡に不可欠であるが弾性特性には無関係である理由を説明せよ。

### 演習 12.6: SiO$_2$ 系の相転移

$T = 300$ K、$P = 40$-$80$ GPa にわたって stishovite と CaCl$_2$ 型 SiO$_2$ 相の $G$ を計算せよ。転移圧力を同定し、Clapeyron 傾斜を計算せよ。Stishovite の $G$ に対する Landau 寄与が境界位置にどう影響するか議論せよ。

### 演習 12.7: 数値微分のステップ幅依存性

`ThermoMineralParams.Entropy`（$\delta T = 0.5$ K）と手動数値微分（$\delta T = 0.1, 1.0, 5.0$ K）を比較する数値実験を行え。誤差 vs ステップ幅をプロットし、中心差分法の2次収束を検証せよ。

### 演習 12.8: 地温勾配に沿った相転移

マントルの地温勾配 $T(P) = 1600 + 10 P$ （K, GPa）の各点で forsterite と wadsleyite の $G$ を計算し、転移が起こる箇所を求めよ。この「地温勾配ベース」の転移深度と固定 $T$ での `FindPhaseBoundary` の結果を比較せよ。

---

## 12.11 図表

### 図 12.1: Gibbs 自由エネルギーの交差

**内容**: $T = 1600$ K における forsterite と wadsleyite の $G$ vs $P$

- **横軸**: 圧力 [GPa]（範囲: 5-25）
- **縦軸**: Gibbs 自由エネルギー $G$ [kJ/mol]
- **主要な特徴**: 相境界（$\sim 13$-$14$ GPa）で交差する2本の曲線。交差の低圧側を「olivine 安定」、高圧側を「wadsleyite 安定」と表示。$\Delta G = 0$ の交差点をマーク。各相が低い $G$ を持つ領域をシェーディング。

### 図 12.2: Olivine-Wadsleyite 相境界

**内容**: Olivine-wadsleyite 転移の相境界（$P$ vs $T$）

- **横軸**: 温度 [K]（範囲: 1000-2500）
- **縦軸**: 圧力 [GPa]（範囲: 10-18）
- **主要な特徴**: 正の傾斜を持つほぼ直線的な相境界。低圧側を「olivine 安定」、高圧側を「wadsleyite 安定」と表示。Clapeyron 傾斜 $dP/dT$ を MPa/K で注記。代表的なマントル地温勾配を重ねて交差深度を表示。

### 図 12.3: Helmholtz 自由エネルギーの成分

**内容**: $T = 1500$ K における forsterite の $F_{\text{cold}}$、$F_{\text{thermal}}$、$F_0$、$PV$ 項の圧力依存性

- **横軸**: 圧力 [GPa]（範囲: 0-30）
- **縦軸**: エネルギー [kJ/mol]
- **主要な特徴**: 各寄与の相対的な大きさを示す重ね合わせ曲線。$F_{\text{cold}}$ は圧縮とともに二次的に増大。$F_{\text{thermal}}$ は比較的小さい。$PV$ は線形に増大。全体の $G = F_0 + F_{\text{cold}} + F_{\text{thermal}} + PV$ を太線で表示。

### 図 12.4: エントロピーの温度・圧力依存性

**内容**: $P = 0$ GPa と $P = 10$ GPa における forsterite のエントロピー $S$ vs 温度

- **横軸**: 温度 [K]（範囲: 100-3000）
- **縦軸**: エントロピー [J/(mol$\cdot$K)]
- **主要な特徴**: 両曲線が $T = 0$ で $S = 0$ から上昇し、高温で Dulong-Petit 極限（$3nR$）に近づく。高圧曲線は圧縮された Debye 温度が高いためより低い $S$ にシフト。各圧力での Debye 温度を表示。

### 図 12.5: 多相 $P$-$T$ 相図

**内容**: MgSiO$_3$ 系の安定領域を示す $P$-$T$ 相図

- **横軸**: 温度 [K]（範囲: 1000-3000）
- **縦軸**: 圧力 [GPa]（範囲: 0-130）
- **主要な特徴**: enstatite、wadsleyite/ringwoodite、perovskite、post-perovskite の色分けされた安定領域。各境界の Clapeyron 傾斜の符号を表示（410 km で正、660 km で負）。マントル地温勾配を重ねて表示。

### 図 12.6: 数値エントロピーの収束性

**内容**: 中心差分と前方差分の数値エントロピーのステップ幅依存性

- **横軸**: ステップ幅 $\delta T$ [K]（対数スケール、範囲: 0.01-10）
- **縦軸**: エントロピーの相対誤差 [%]
- **主要な特徴**: 中心差分の誤差は $\delta T^2$ に比例して減少（対数プロットで傾き $-2$）。前方差分の誤差は $\delta T$ に比例（傾き $-1$）。コードで使用される $\delta T = 0.5$ K をマークし、対応する誤差水準を示す。

---

## 12.12 参考文献

1. **Stixrude, L. and Lithgow-Bertelloni, C.** (2011). "Thermodynamics of mantle minerals - II. Phase equilibria," *Geophysical Journal International*, 184(3), 1180-1213.
   - SLB 枠組みによる Gibbs 自由エネルギーの計算と相平衡方法論を定義。$F_0$ 参照エネルギーを含む完全なデータベースを提供。

2. **Stixrude, L. and Lithgow-Bertelloni, C.** (2005). "Thermodynamics of mantle minerals - I. Physical properties," *Geophysical Journal International*, 162(2), 610-632.
   - Gibbs エネルギー計算の基盤となる $F(V, T)$ の Helmholtz 自由エネルギー定式化を確立。冷圧縮、熱的、非調和寄与への分解を定義。

3. **Anderson, O. L.** (1995). *Equations of State of Solids for Geophysics and Ceramic Science*, Oxford University Press.
   - 高圧下の固体の熱力学ポテンシャル（$F$, $G$, $H$）の包括的な取り扱い。Legendre 変換の関係と各ポテンシャルの役割を解説。

4. **Ita, J. and Stixrude, L.** (1992). "Petrology, elasticity, and composition of the mantle transition zone," *Journal of Geophysical Research*, 97(B5), 6849-6866.
   - 地温勾配に沿ったマントル鉱物学計算への Gibbs 自由エネルギー最小化の初期応用。$G$ から計算される相平衡と地震学的に観測可能な速度不連続面の関係を実証。

5. **Katsura, T. et al.** (2004). "Olivine-wadsleyite transition in the system (Mg,Fe)$_2$SiO$_4$," *Journal of Geophysical Research*, 109(B2), B02209.
   - Olivine-wadsleyite Clapeyron 傾斜（$\sim 3.6$ MPa/K）の実験的制約を提供。コードベースでの Gibbs 自由エネルギー計算の検証対象。

---

## 12.13 他章関連

### 第1章: 高圧下の固体の熱力学（前提）

Birch-Murnaghan EOS と有限歪み理論が Helmholtz 自由エネルギーへの $F_{\text{cold}}$ 寄与を提供する。冷圧縮エネルギーは高圧における $G$ への主要な寄与である。

### 第2章: Debye モデルと熱的性質（前提）

Debye モデルが $F$ ひいては $G$ への $F_{\text{thermal}}$ 寄与を提供する。Debye 関数 $D_3(\theta/T)$ とその微分が、相安定性の温度依存性を支配する熱的自由エネルギーとエントロピーを決定する。

### 第3章: Gruneisen パラメータと非調和性（前提）

Gruneisen パラメータは Debye 温度の圧縮変化を制御し、熱圧力と熱的自由エネルギーの両方に影響する。$F_{\text{thermal}}$ への影響を通じてエントロピー計算に直接入る。

### 第5章: Stixrude-Lithgow-Bertelloni 定式化（前提）

$F_{\text{cold}}$ と $F_{\text{thermal}}$ への分解および有限歪み依存の Gruneisen パラメータを含む完全な $F(V, T)$ 定式化を提供する。本章はこの $F$ を入力として使用する。

### 第6章: Mie-Gruneisen 状態方程式ソルバー（前提）

$G$ の計算に不可欠な、対象 $(P, T)$ での体積を求める EOS オプティマイザ。収束した体積がなければ $PV$ も体積依存の自由エネルギー項も正しく評価できない。

### 第7章: SLB2011 鉱物データベース（応用）

相間の $G$ 比較に必要な $F_0$ 参照エネルギーを提供する。$F_0$ なしでは相内の特性計算のみが可能。データベースは各端成分の $11$ 以上のパラメータも提供する。

### 第8章: Landau 相転移（前提）

変位型相転移を受ける鉱物（例：石英の $\alpha$-$\beta$ 転移）の $G$ を修正する Landau 自由エネルギー、エントロピー、体積補正。$F$ と $V$ への加算的寄与として含まれる。

### 第4章: 弾性率と地震波速度（発展）

$G$ 最小化で相安定性が決定されると、安定な集合体の弾性率と地震波速度が計算可能になる。相転移は速度プロファイルに不連続面を生じ、これが主要な地震学的観測量である。

### 第13章: 相図と相境界（発展）

本章で確立した $G$ の計算枠組みを基盤として、$P$-$T$ 空間にわたる系統的な相境界のトレース、多相反応の平衡計算、グリッドベースの相図計算に発展する。
