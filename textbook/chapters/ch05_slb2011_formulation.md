# 第5章 Stixrude-Lithgow-Bertelloni (SLB2011) 定式化

## 5.1 概要

Stixrude-Lithgow-Bertelloni (SLB2011) 定式化は、マントル鉱物のすべての平衡熱力学的・弾性的性質を、実験的に決定された最小限のパラメータセットから自己無撞着に計算する熱力学的枠組みである。本定式化は、Euler 有限歪み理論と Mie-Gruneisen-Debye 熱モデルを組み合わせ、Helmholtz 自由エネルギー $F(V,T)$ を以下の三つの寄与の和として構成する：基準エネルギー $F_0$、第3次 Birch-Murnaghan 状態方程式による冷圧縮エネルギー $F_{\text{cold}}$、および Debye モデルによる熱エネルギー $F_{\text{thermal}}$。

この定式化の核心的な特徴は、圧力、体積弾性率、剪断弾性率、熱膨張率、比熱、エントロピー、Gibbs 自由エネルギー、地震波速度など、すべての物性が**単一の自由エネルギー関数の熱力学的に無撞着な微分**として得られることである。Gruneisen パラメータ、Debye 温度、剪断弾性率がすべて有限歪みの陽な関数として表されるため、導出されるすべての物性間の内部的整合性が保証される。

SLB2011 データベースは、olivine から post-perovskite まで主要なマントル相に及ぶ60種以上の端成分鉱物のパラメータを提供し、マントル鉱物学における最も広く利用される熱力学データベースとなっている。

本章の内容は、ThermoElasticCalculator の `MineralParams`、`MieGruneisenEOSOptimizer`、および `ThermoMineralParams` クラスに直接対応しており、理論から実装までの一貫した対応関係を示す。

---

## 5.2 前提知識

本章を十分に理解するためには、以下の知識が必要である。

1. **熱力学ポテンシャル**: Helmholtz 自由エネルギー $F(V,T)$、Gibbs 自由エネルギー $G(P,T)$、およびそれらの偏微分関係。自由エネルギーから圧力、エントロピー、比熱などを導出する方法。

2. **Euler 有限歪み理論と Birch-Murnaghan 状態方程式**（第1章）: 有限歪み $f = [(V_0/V)^{2/3} - 1]/2$ の定義と、第3次 Birch-Murnaghan 状態方程式による圧力-体積関係。

3. **Debye モデル**（第2章）: 格子振動の Debye モデル、Debye 関数 $D_3(x)$、および内部エネルギー、比熱、エントロピー、自由エネルギーの計算方法。

4. **Gruneisen パラメータ**（第3章）: Gruneisen パラメータ $\gamma$ の定義、物理的意味、$q_0$ による体積依存性の記述。

5. **弾性率と地震波速度**（第4章）: 体積弾性率 $K$、剪断弾性率 $G$ と地震波速度 $V_p$、$V_s$、$V_\phi$ の関係。

6. **Mie-Gruneisen 熱圧力**: 振動エネルギーの体積変化による熱圧力 $\Delta P = (\gamma/V) \Delta E_{\text{th}}$ の概念と、熱状態方程式におけるその役割。

7. **基本的な数値計算法**: 非線形方程式の求根アルゴリズム（Regula Falsi 法、二分法）の原理。

8. **結晶化学**: 化学式単位、化学式あたりの原子数 $n$、モル体積・モル質量の規約。

---

## 5.3 理論と主要方程式

### 5.3.1 Helmholtz 自由エネルギーの分解

SLB 定式化の基本方程式（マスター方程式）は、Helmholtz 自由エネルギーの三つの寄与への分解である：

$$F(V,T) = F_0 + F_{\text{cold}}(f) + F_{\text{thermal}}(f,T)$$

ここで各項の意味は以下の通りである：

- $F_0$: 標準状態における基準 Helmholtz 自由エネルギー [kJ/mol]
- $F_{\text{cold}}(f)$: Birch-Murnaghan 状態方程式による冷圧縮（歪み）エネルギー [kJ/mol]
- $F_{\text{thermal}}(f,T)$: Debye モデルによる熱的寄与 [kJ/mol]
- $f$: Euler 有限歪み、$f = [(V_0/V)^{2/3} - 1] / 2$

これが SLB 定式化のマスター方程式である。すべての熱力学的性質は、この単一の関数の偏微分として導出される。冷圧縮項と熱的項への分解により、ゼロ温度での圧縮曲線は Birch-Murnaghan 状態方程式で正確に記述され、熱的効果は Debye モデルを通じて自己無撞着に付加される。ThermoElasticCalculator では、`ThermoMineralParams` クラスの `FCold` プロパティおよび `FThermal` プロパティがそれぞれ対応する。

### 5.3.2 冷圧縮エネルギー

冷圧縮エネルギーは、第3次 Birch-Murnaghan 有限歪み展開から得られる：

$$F_{\text{cold}} = 9 K_0 V_0 \left( \frac{f^2}{2} + \frac{K'_0 - 4}{6} f^3 \right)$$

ここで：

- $K_0$: 基準等温体積弾性率 [GPa]
- $V_0$: 基準モル体積 [cm$^3$/mol]
- $K'_0$: 零圧における体積弾性率の圧力微分（無次元）
- $f$: Euler 有限歪み

係数 $9 K_0 V_0$ はエネルギーのスケールを定める。GPa $\times$ cm$^3$/mol = kJ/mol であるため、単位は直接的に整合する。$f^2/2$ の項は調和的な寄与を与え、$f^3$ の項は $K'_0$ によって制御される非調和的な硬化（圧縮に伴う剛性の増加）を表す。

この式は `ThermoMineralParams.FCold` として実装されている。

### 5.3.3 熱自由エネルギー

Debye モデルからの熱自由エネルギーは以下のように表される：

$$F_{\text{thermal}} = n \left[ k_B T \cdot g(\theta/T) - k_B T_{\text{ref}} \cdot g(\theta_0/T_{\text{ref}}) \right] \frac{N_A}{1000}$$

ここで：

- $n$: 化学式あたりの原子数
- $k_B$: Boltzmann 定数 [J/K]
- $T$: 温度 [K]
- $T_{\text{ref}}$: 基準温度（300 K）
- $g(x)$: 無次元熱自由エネルギー関数、$g(x) = 3 \ln(1 - e^{-x}) - D_3(x)$
- $\theta$: 現在の歪み状態における Debye 温度 [K]
- $\theta_0$: 基準 Debye 温度 [K]
- $N_A$: Avogadro 数

関数 $g(x)$ はゼロ点エネルギーと熱的占有の項を結合したものである。歪み依存性は $\theta(f)$ を通じて入り、圧縮と熱的性質を結合する。この項は `ThermoMineralParams.FThermal` として実装されており、`DebyeFunctionCalculator.GetThermalFreeEnergyPerAtom` を利用する。

### 5.3.4 全圧力：冷圧縮 + 熱圧力（Mie-Gruneisen）

Helmholtz 自由エネルギーの体積微分から得られる全圧力は：

$$P(f,T) = 3 K_0 (1+2f)^{5/2} f \left[1 + \frac{3}{2}(K'_0 - 4) f \right] + \frac{\gamma}{V} \Delta E_{\text{th}}$$

ここで：

- 第1項: Birch-Murnaghan 冷圧縮圧力
- 第2項: Mie-Gruneisen 熱圧力
- $\gamma$: 現在の歪み状態における Gruneisen パラメータ
- $V$: 現在の歪み状態におけるモル体積 [cm$^3$/mol]
- $\Delta E_{\text{th}}$: 基準温度からの熱エネルギー変化 [J/mol]

ThermoElasticCalculator では、`RefP` が `MineralParams.GetPressure(f)` による冷圧縮項を、`DeltaP` = $(\gamma/V) \times \Delta E / 1000$ が熱的項を与える。`MieGruneisenEOSOptimizer` は、目標圧力を再現する歪み $f$ を反復的に求める。

### 5.3.5 Debye 温度のスケーリング多項式

Debye 温度の歪み依存性は、無次元スケーリング因子 $\mu(f)$ を用いて表される：

$$\mu(f) = 1 + A_{ii} f + \frac{1}{2} A_{iikk} f^2$$

ここで：

- $A_{ii} = 6 \gamma_0$: 第1歪み係数
- $A_{iikk} = -12\gamma_0 + 36\gamma_0^2 - 18\gamma_0 q_0$: 第2歪み係数

有限歪みにおける Debye 温度は：

$$\theta(f) = \theta_0 \sqrt{\mu(f)}$$

係数 $A_{ii}$ と $A_{iikk}$ は、振動周波数スペクトルが圧縮下でどのようにシフトするかを、$\gamma_0$ と $q_0$ を通じて符号化している。これが状態方程式と熱モデルの中心的な結合である。`ThermoMineralParams` コンストラクタで直接計算される。

### 5.3.6 有限歪みにおける Gruneisen パラメータ

任意の圧縮状態における Gruneisen パラメータは、$\mu(f)$ 多項式に $\gamma = -d(\ln \theta)/d(\ln V)$ を適用することで得られる：

$$\gamma(f) = \frac{(2f+1)(A_{ii} + A_{iikk} f)}{6 \mu(f)}$$

$f = 0$（常温常圧条件）では $\gamma = \gamma_0$ に還元される。高圧縮下では $\gamma$ は一般に減少する。`ThermoMineralParams` の `_gamma` フィールドとして実装されている。

### 5.3.7 Gruneisen パラメータの体積微分

有限歪みにおける $q$（Gruneisen パラメータの対数体積微分）は：

$$q(f) = \frac{1}{9\gamma} \left[ 18\gamma^2 - 6\gamma - \frac{(2f+1)^2 A_{iikk}}{2\mu} \right]$$

$f = 0$ では $q = q_0$ となる。この量は $K_T$ の熱補正項に入り、熱圧力生成のモードが圧縮とともにどのように変化するかを記述する。`ThermoMineralParams.Q` として実装されている。

### 5.3.8 剪断歪み Gruneisen パラメータ

剪断弾性率の熱補正を支配する $\eta_S$ は以下のように表される：

$$\eta_S(f) = -\gamma - \frac{(2f+1)^2 A_S}{2 \mu}$$

ここで：

- $A_S = -2\gamma_0 - 2\eta_{S0}$: 剪断歪み係数

$\eta_S$ は剪断歪みの下で振動周波数がどのようにシフトするかを記述する。$\eta_S$ が大きいほど、剪断弾性率 $G$ の温度感受性が強くなる。$f = 0$ では $\eta_S = \eta_{S0}$ となる。`ThermoMineralParams` の `_ethaS` フィールドに格納される。

### 5.3.9 等温体積弾性率

熱補正を含む完全な等温体積弾性率は：

$$K_T = K_{T}^{\text{cold}}(f) + \frac{(\gamma+1-q) \gamma \Delta E}{V} - \frac{\gamma^2 \Delta(C_v T)}{V}$$

ここで：

- $K_{T}^{\text{cold}}(f)$: `MineralParams.BM3KT(f)` による Birch-Murnaghan $K_T$ [GPa]
- $\gamma$: 歪み $f$ における Gruneisen パラメータ
- $q$: 歪み $f$ における Gruneisen パラメータの対数体積微分
- $\Delta E$: 熱エネルギー変化 [kJ/mol]
- $V$: 体積 [cm$^3$/mol]
- $\Delta(C_v T)$: 温度 $T$ における $C_v T$ から基準温度 $T_{\text{ref}}$ における $C_v T$ を引いた値 [J/mol]

高圧では冷圧縮項が支配的であり、二つの熱補正項は部分的に相殺する。`ThermoMineralParams.KT` プロパティとして実装されている。

### 5.3.10 断熱体積弾性率

地震波伝播に関連する断熱体積弾性率は：

$$K_S = K_T + \frac{\gamma^2 C_v T}{V}$$

ここで：

- $K_S$: 断熱体積弾性率 [GPa]
- $K_T$: 等温体積弾性率 [GPa]
- $C_v$: 定積モル比熱 [J/(mol$\cdot$K)]
- $T$: 温度 [K]
- $V$: モル体積 [cm$^3$/mol]

断熱補正は常に正であるため、$K_S > K_T$ である。`ThermoMineralParams` では効率のため $C_v T$ が事前に計算される。この変換は、緩やかな等温圧縮に関連する $K_T$ を、速い断熱的地震波に関連する $K_S$ へと変換する。

### 5.3.11 剪断弾性率

熱補正を含む剪断弾性率は：

$$G = G^{\text{cold}}(f) - \frac{\eta_S \Delta E}{V}$$

ここで：

- $G^{\text{cold}}(f)$: 有限歪み展開による剪断弾性率 (`BM3GT`) [GPa]
- $\eta_S$: 剪断歪み Gruneisen パラメータ
- $\Delta E$: 熱エネルギー変化 [kJ/mol]
- $V$: 体積 [cm$^3$/mol]

剪断弾性率の熱補正は $K_T$ に比べて単純である。これは、剪断変形が体積を変えないためである。$\eta_S$ が正で $\Delta E$ が基準温度以上で正であるため、$G$ は温度の上昇とともに常に減少する。`ThermoMineralParams.GS` として実装されている。

### 5.3.12 地震波速度

弾性率と密度が求まれば、地震波速度は以下の標準的な関係式から計算される：

$$V_p = \sqrt{\frac{K_S + \frac{4}{3}G}{\rho}}, \quad V_s = \sqrt{\frac{G}{\rho}}, \quad V_\phi = \sqrt{\frac{K_S}{\rho}}$$

ここで密度 $\rho$ はモル質量 $M$ と体積 $V$ から：

$$\rho = \frac{M}{V}$$

として得られる。これらは `ThermoMineralParams` の `Vp`、`Vs`、`Vb` プロパティに対応する。

### 5.3.13 熱膨張率

定圧熱膨張率は以下の関係から計算される：

$$\alpha = \frac{\gamma C_v}{K_T V}$$

ここで適切な単位変換を伴う。この式は、Gruneisen パラメータが振動エネルギーと圧力を結びつけるのと同様に、熱的振動の増大が体積膨張を引き起こすことを表している。

### 5.3.14 エントロピーと Gibbs 自由エネルギー

エントロピーは Helmholtz 自由エネルギーの温度微分から得られる：

$$S = -\left(\frac{\partial F}{\partial T}\right)_V$$

`ThermoMineralParams` では、数値的中心差分法を用いて計算する：

$$S = -\frac{F(T+0.5) - F(T-0.5)}{1.0} \times 1000 \quad [\text{J/(mol$\cdot$K)}]$$

Gibbs 自由エネルギーは：

$$G = F + PV$$

GPa $\times$ cm$^3$/mol = kJ/mol の単位関係により、直接的に計算される。相平衡計算では、この Gibbs 自由エネルギーが最小化される量である。

---

## 5.4 物理概念

### 5.4.1 自己無撞着な熱力学的定式化

SLB 定式化の最も重要な概念は、すべての熱力学的性質が**単一の自由エネルギー関数の偏微分**として導出されるということである。

地形図が単一の標高関数からすべての勾配と等高線を決定するように、Helmholtz 自由エネルギー $F(V,T)$ は圧力（$-\partial F/\partial V$）、エントロピー（$-\partial F/\partial T$）、体積弾性率（$V \cdot \partial^2 F/\partial V^2$）、およびその他すべての物性を微分として決定する。もし各物性に対して別々のアドホックなフィットを使えば、それらは一般に相互に矛盾することになる。

**よくある誤解**: $K(P,T)$、$\alpha(P,T)$、$C_p(P,T)$ を独立にフィットして組み合わせることは、Maxwell 関係を破り、極端な条件で負の比熱や虚の速度のような熱力学的に不可能な結果を生じ得る。SLB 定式化はこの問題を原理的に回避する。

### 5.4.2 Euler 有限歪み

有限歪み $f = [(V_0/V)^{2/3} - 1]/2$ は体積圧縮を記述する無次元量である。

$f$ は常温常圧体積でゼロであり、圧縮とともに単調に増加する。自由エネルギーの自然な展開変数として機能する。上部マントル条件（約25 GPa）では $f \approx 0.02$–$0.04$、コア-マントル境界（136 GPa）では bridgmanite に対して $f \approx 0.08$–$0.12$ に達する。

**よくある誤解**: $f$ は $V_0/V$ や体積変化率とは異なる。$(V_0/V)^{2/3}$ のべき乗は、変形後の配置を基準とする Euler 歪みの定義に由来する。変形前の状態を基準とする Lagrange 的な代替法は、収束がより遅い。

### 5.4.3 鉱物あたり11個の独立パラメータ

SLB2011 データベースにおける各端成分鉱物は、11個の独立パラメータで特徴づけられる：

$$V_0, \; K_0, \; K'_0, \; G_0, \; G'_0, \; \theta_0, \; \gamma_0, \; q_0, \; \eta_{S0}, \; n, \; M$$

これらは三つのグループに分けられる：

1. **力学的パラメータ** ($V_0, K_0, K'_0, G_0, G'_0$): 結晶が圧縮と剪断にどのように抵抗するかを記述する。
2. **熱的パラメータ** ($\theta_0, \gamma_0, q_0, \eta_{S0}$): 振動周波数とそのモード平均的性質が体積とともにどのように変化するかを記述する。
3. **化学式パラメータ** ($n, M$): 化学的同一性を定義する。

これら11個のパラメータは、0から150 GPa、300から5000 K の範囲での挙動を予測するために必要なすべてを符号化する。`MineralParams` クラスにこれらのパラメータが格納される。

**よくある誤解**: $K'_0$ と $G'_0$ は $K$ と $G$ の二階微分ではない。これらは $P = 0$ における圧力に対する一階微分（$dK/dP$ と $dG/dP$）である。また、$\eta_{S0}$ は体積圧縮だけからは直接測定できず、高温での剪断波速度データが必要である。

### 5.4.4 Mie-Gruneisen 熱圧力

結晶が加熱されると原子はより激しく振動し、隣接原子を押し広げて追加の圧力を生成する。Gruneisen パラメータ $\gamma$ は、振動エネルギーがどの程度効率的に圧力に変換されるかを示す：

$$\Delta P = \frac{\gamma}{V} \Delta E_{\text{th}}$$

マントル条件では、熱圧力は典型的に5–30 GPa であり、冷圧縮圧力に匹敵する規模である。

**よくある誤解**: 熱圧力は単に $\alpha K_T \Delta T$ ではない。この線形近似は高温・高圧で破綻する。Debye 内部エネルギーを用いた Mie-Gruneisen の形式は、高温での比熱の飽和を正しく捉えるため、はるかに広い範囲で有効である。

### 5.4.5 有限歪みにおける Debye 温度

圧縮は結晶格子を硬化させ、すべての振動周波数を上方にシフトさせる。Debye 温度は振動スペクトルの周波数スケールを特徴づける：

$$\theta(f) = \theta_0 \sqrt{\mu(f)}$$

下部マントル条件への圧縮下では、$\theta$ は常温常圧値から50–100%増加し得る。

**よくある誤解**: Debye モデルはすべての振動に対して単一の特性周波数を仮定する。実際の結晶は複雑なフォノンスペクトルを持つ。にもかかわらず、Debye モデルは熱力学的性質（周波数平均された積分量）に対しては、個々のフォノン分枝に対しては破綻する場合でも、驚くべきほど良く機能する。

### 5.4.6 反復的自己無撞着ループ

熱圧力は体積に依存し（$\gamma$ と $\theta$ を通じて）、しかし体積は全圧力に依存する。この循環的な依存性は反復によって解消される：

1. 基準圧力を推定する
2. $f$ を解く
3. 熱圧力を計算する
4. 全圧力が目標値と一致するか確認する
5. 調整して繰り返す

`MieGruneisenEOSOptimizer` はこの手順を実装し、典型的に3–10回の反復で0.01 mPa より良い精度に収束する。

**よくある誤解**: 反復なしに冷圧縮曲線に熱圧力を単純に加算する「ワンショット」アプローチは、高温で数 GPa の誤差を導入し、速度などの導出物性に顕著な影響を与えうる。

---

## 5.5 計算例

### 5.5.1 計算例1：Forsterite の高圧高温物性

**問題**: 410 km 不連続面付近の条件（$P = 14$ GPa、$T = 1673$ K）における forsterite（Mg$_2$SiO$_4$）の密度、断熱体積弾性率 $K_S$、剪断弾性率 $G$、地震波速度 $V_p$ と $V_s$ を計算せよ。

**解法**:

**Step 1: パラメータの読み込み**

SLB2011Endmembers から forsterite のパラメータを読み込む：

| パラメータ | 値 | 単位 |
|---|---|---|
| $V_0$ | 43.603 | cm$^3$/mol |
| $K_0$ | 127.96 | GPa |
| $K'_0$ | 4.218 | — |
| $G_0$ | 81.6 | GPa |
| $G'_0$ | 1.4626 | — |
| $\theta_0$ | 809.17 | K |
| $\gamma_0$ | 0.99282 | — |
| $q_0$ | 2.10672 | — |
| $\eta_{S0}$ | 2.2997 | — |
| $n$ | 7 | — |
| $M$ | 140.69 | g/mol |

**Step 2: 歪み係数の計算**

$$A_{ii} = 6 \gamma_0 = 6 \times 0.99282 = 5.957$$

$$A_{iikk} = -12\gamma_0 + 36\gamma_0^2 - 18\gamma_0 q_0$$
$$= -12 \times 0.99282 + 36 \times 0.99282^2 - 18 \times 0.99282 \times 2.10672$$
$$= -11.914 + 35.488 - 37.665 = -14.091$$

$$A_S = -2\gamma_0 - 2\eta_{S0} = -2 \times 0.99282 - 2 \times 2.2997 = -6.585$$

**Step 3: 反復求解**

`MieGruneisenEOSOptimizer` を $P = 14$ GPa、$T = 1673$ K で実行する。

最適化ループの概要：
1. 初期推定：基準圧力 $P_{\text{ref}} = 14$ GPa
2. `BM3Finite(14)` で有限歪みを求める
3. `ThermoMineralParams` を構築し全圧力を計算
4. 残差を確認し基準圧力を調整
5. 約5回の反復で収束

**Step 4: 収束後の結果**

| 物性 | 値 | 単位 |
|---|---|---|
| 収束有限歪み $f$ | $\approx 0.028$ | — |
| モル体積 $V$ | $\approx 40.1$ | cm$^3$/mol |
| 密度 $\rho$ | $\approx 3.51$ | g/cm$^3$ |
| Gruneisen パラメータ $\gamma$ | $\approx 0.88$ | — |
| Debye 温度 $\theta$ | $\approx 870$ | K |
| 等温体積弾性率 $K_T$ | $\approx 184$ | GPa |
| 断熱体積弾性率 $K_S$ | $\approx 192$ | GPa |
| 剪断弾性率 $G$ | $\approx 93$ | GPa |
| P波速度 $V_p$ | $\approx 9200$ | m/s |
| S波速度 $V_s$ | $\approx 5150$ | m/s |
| 熱膨張率 $\alpha$ | $\approx 2.5 \times 10^{-5}$ | 1/K |

**解釈**:

410 km 条件では、forsterite は常温常圧に比べて顕著に硬化している（$K_S$ は常温常圧の $K_0$ より約50%高い）。Gruneisen パラメータは 0.99 から約 0.88 に減少しており、圧縮下での非調和性の低下を反映している。計算された $V_p$ と $V_s$ は、410 km における PREM 値（$V_p \approx 9.0$ km/s、$V_s \approx 5.1$ km/s）と合理的に一致しており、単一相端成分に対する良好な精度を示す。

### 5.5.2 計算例2：Bridgmanite の熱力学的自己無撞着性の検証

**問題**: $P = 100$ GPa、$T = 2500$ K における Mg-perovskite（bridgmanite）に対して、熱力学的恒等式 $K_S = K_T + \gamma^2 C_v T / V$ を検証せよ。また、近似式 $K_S = K_T(1 + \alpha \gamma T)$ との差異を議論せよ。

**解法**:

**Step 1: パラメータの読み込み**

Mg-Perovskite のパラメータ：

| パラメータ | 値 | 単位 |
|---|---|---|
| $V_0$ | 24.445 | cm$^3$/mol |
| $K_0$ | 250.53 | GPa |
| $K'_0$ | 4.14 | — |
| $G_0$ | 172.9 | GPa |
| $G'_0$ | 1.6904 | — |
| $\theta_0$ | 905.94 | K |
| $\gamma_0$ | 1.56508 | — |
| $q_0$ | 1.10945 | — |
| $\eta_{S0}$ | 2.5654 | — |
| $n$ | 5 | — |
| $M$ | 100.39 | g/mol |

**Step 2: 計算結果**

`MieGruneisenEOSOptimizer` を $P = 100$ GPa、$T = 2500$ K で実行する。

| 物性 | 値 | 単位 |
|---|---|---|
| $K_T$ | $\approx 553$ | GPa |
| $K_S$（直接計算） | $\approx 586$ | GPa |
| $\alpha$ | $\approx 1.2 \times 10^{-5}$ | 1/K |
| $\gamma$ | $\approx 1.24$ | — |

**Step 3: 検証**

厳密な恒等式（コードで使用）：

$$K_S = K_T + \frac{\gamma^2 C_v T}{V}$$

近似式：

$$K_S \approx K_T (1 + \alpha \gamma T) = 553 \times (1 + 1.2 \times 10^{-5} \times 1.24 \times 2500)$$
$$= 553 \times 1.037 = 573 \; \text{GPa}$$

**解釈**:

直接計算の $K_S \approx 586$ GPa と近似式の $K_S \approx 573$ GPa の間には約2%の差異がある。この差異は、$K_S = K_T(1 + \alpha \gamma T)$ が $C_v \approx C_p$（Dulong-Petit 極限）の近似のもとで成立するためである。2500 K では Debye モデルが $C_v$ を $3nR$ よりわずかに低く予測し、約1–2%の不一致を生じる。このことが、コードが厳密な形式 $K_S = K_T + \gamma^2 C_v T / V$ を使用する理由を示している。

### 5.5.3 計算例3：Wadsleyite の自由エネルギーとエントロピー

**問題**: $P = 16$ GPa、$T = 1600$ K における Mg-Wadsleyite の Helmholtz 自由エネルギー、Gibbs 自由エネルギー、エントロピーを計算し、$G = F + PV$ および $S = -\partial F / \partial T$ を検証せよ。

**解法**:

**Step 1: パラメータの読み込み**

Mg-Wadsleyite のパラメータ：

| パラメータ | 値 | 単位 |
|---|---|---|
| $V_0$ | 40.515 | cm$^3$/mol |
| $K_0$ | 168.69 | GPa |
| $K'_0$ | 4.3229 | — |
| $G_0$ | 112.0 | GPa |
| $G'_0$ | 1.4442 | — |
| $\theta_0$ | 843.5 | K |
| $\gamma_0$ | 1.2061 | — |
| $q_0$ | 2.0188 | — |
| $\eta_{S0}$ | 2.6368 | — |
| $n$ | 7 | — |
| $M$ | 140.69 | g/mol |

**Step 2: 計算結果**

| 物性 | 値 | 単位 |
|---|---|---|
| 収束有限歪み $f$ | $\approx 0.024$ | — |
| モル体積 $V$ | $\approx 37.9$ | cm$^3$/mol |
| $F_0$ | $-2027.837$ | kJ/mol |
| $F_{\text{cold}}$ | $\approx 12.5$ | kJ/mol |
| $F_{\text{thermal}}$ | $\approx -160$ | kJ/mol |
| Helmholtz 自由エネルギー $F$ | $\approx -2175$ | kJ/mol |
| $PV$ | $\approx 606$ | kJ/mol |
| Gibbs 自由エネルギー $G$ | $\approx -1569$ | kJ/mol |
| エントロピー $S$ | $\approx 230$ | J/(mol$\cdot$K) |

**Step 3: 検証**

**Gibbs 自由エネルギー**:

$$G = F + PV = -2175 + 606 = -1569 \; \text{kJ/mol} \quad \checkmark$$

GPa $\times$ cm$^3$/mol = kJ/mol であるため、$PV = 16 \times 37.9 = 606$ kJ/mol。

**エントロピー**（数値中心差分）:

$$S = -\frac{F(T+0.5) - F(T-0.5)}{1.0} \times 1000 \; \text{[J/(mol$\cdot$K)]}$$

$dT = 0.5$ K での中心差分により、解析的推定と0.1%以内で一致する。

**解釈**:

Gibbs 自由エネルギー $G = F + PV$ は固定 $P$, $T$ で最小化される熱力学ポテンシャルであり、相平衡計算（例えば410 km における olivine-wadsleyite 転移の決定）の鍵となる量である。1600 K でのエントロピー $\approx 230$ J/(mol$\cdot$K) は Debye 熱的寄与（振動エントロピー）に支配される。

---

## 5.6 計算手法

### 5.6.1 全体的な計算フロー

SLB2011 定式化の計算は、三つの主要クラスを通じて以下の順序で進行する：

```
入力: 鉱物パラメータ (11個) + 目標条件 (P, T)
  │
  ▼
MineralParams: パラメータの格納と代数的計算
  │  GetPressure(f) → 冷 BM 圧力
  │  BM3Finite(P)   → P から f への逆変換
  │  BM3KT(f)       → 冷等温体積弾性率
  │  BM3GT(f)       → 冷剪断弾性率
  │
  ▼
MieGruneisenEOSOptimizer: 反復的自己無撞着ループ
  │  (1) 基準圧力の推定
  │  (2) f = BM3Finite(P_ref) で歪みを求解
  │  (3) ThermoMineralParams(f, T) で全 P(f,T) を計算
  │  (4) |P_target - P_computed| < 10^{-5} GPa まで調整
  │  典型的に 3-10 回の反復で収束
  │
  ▼
ThermoMineralParams: すべての導出物性を一括計算
  │  mu(f), gamma(f), eta_S(f), theta(f)
  │  DeltaE, DeltaP, Cv, q(f)
  │  K_T, K_S, G, alpha
  │  Vp, Vs, Vb, rho
  │  F, G (Gibbs), S (entropy)
  │
  ▼
出力: 完全な熱力学的・弾性的性質セット
```

### 5.6.2 MineralParams クラス

`MineralParams` クラスは11個の基本パラメータを格納し、以下の代数的メソッドを提供する：

- `GetPressure(f)`: 冷 Birch-Murnaghan 圧力 $P(f) = 3K_0(1+2f)^{5/2}f[1 + \frac{3}{2}(K'_0-4)f]$
- `BM3Finite(P)`: $P(f) = P$ を満たす $f$ を求根法（Regula Falsi）で逆算
- `BM3KT(f)`: 冷等温体積弾性率 $K_T^{\text{cold}}(f)$
- `BM3GT(f)`: 冷剪断弾性率 $G^{\text{cold}}(f)$

`BM3Finite` における求根法は、動的に推定された上界 $f_{\text{max}} = \max(0.02, \; 3P/(3K_0) + 0.01)$ を用いた Regula Falsi 最適化器を使用する。

### 5.6.3 MieGruneisenEOSOptimizer クラス

反復的自己無撞着ループの実装である。アルゴリズムは以下の通り：

1. 目標圧力を基準（冷）圧力の初期推定値とする
2. `BM3Finite` で有限歪み $f$ を求める
3. `ThermoMineralParams` を構築して全 $P(f,T)$ を計算する
4. 基準圧力を残差 $P_{\text{target}} - P_{\text{computed}}$ で調整する
5. $|P_{\text{target}} - P_{\text{computed}}| < 10^{-5}$ GPa になるまで繰り返す（最大500回）

収束は典型的に3–10回の反復で達成される。オプティマイザは収束状態、反復回数、圧力残差を診断情報として返す。

### 5.6.4 ThermoMineralParams クラス

中心的な計算クラスである。コンストラクタは収束した $(f, T, \text{mineral})$ の三つ組を受け取り、以下の順序ですべての導出量を計算する：

1. $\mu(f)$ の計算
2. $\gamma(f)$ の計算
3. $\eta_S(f)$ の計算
4. $\theta(f) = \theta_0 \sqrt{\mu}$ の計算
5. `DebyeFunctionCalculator` による Debye 積分の評価
6. $\Delta E$（Debye 内部エネルギー差）の計算
7. 熱圧力 $\Delta P$ の計算
8. $q(f)$ の計算

物性 $K_T$, $K_S$, $G$, $\alpha$, $V_p$, $V_s$, $V_b$, $F$, $G$（Gibbs）は遅延評価される C# プロパティとして利用可能である。

### 5.6.5 DebyeFunctionCalculator

Debye 関数 $D_3(x)$ は500個の副区間を持つ複合 Simpson 則で評価され、倍精度の精度を達成する。数値的安定性のため、以下の漸近形式が使用される：

- $x > 150$ の場合: $D_3(x) \to \pi^4/(5x^3)$
- $x < 10^{-10}$ の場合: $D_3(x) \to 1$

また、大きな $x$ の引数に対しては $1/(e^x - 1)$ の代わりに $e^{-x}/(1 - e^{-x})$ を使用して数値的オーバーフローを回避する。

内部エネルギー、比熱、エントロピー、自由エネルギー（原子あたり）はすべて $D_3(x)$ と関連する式から計算される。

### 5.6.6 数値的精度に関する注意

SLB 定式化の実装における数値的精度の確保には以下の点が重要である：

1. **反復収束判定**: $10^{-5}$ GPa の閾値は、導出物性（速度など）の相対精度が $10^{-4}$ 程度になることを保証する。

2. **エントロピーの中心差分**: $dT = 0.5$ K を用いた中心差分は、打ち切り誤差 $O(dT^2)$ を与え、0.1%以内の精度を達成する。

3. **Debye 積分の精度**: 500副区間の Simpson 則は、$D_3(x)$ の相対精度 $10^{-12}$ 以上を保証する。

4. **有限歪みの求根**: Regula Falsi 法は超線形収束を示し、通常10–20回の反復で機械精度に達する。

---

## 5.7 コード対応

### 5.7.1 クラス構造とファイル対応

本章の理論は、ThermoElasticCalculator の以下のクラスに直接対応する：

| 理論的概念 | クラス/プロパティ | 役割 |
|---|---|---|
| 11個の基本パラメータ | `MineralParams` | パラメータの格納 |
| 冷 BM 圧力 $P(f)$ | `MineralParams.GetPressure(f)` | 冷圧縮圧力 |
| $P \to f$ の逆変換 | `MineralParams.BM3Finite(P)` | 求根法による逆算 |
| 冷 $K_T(f)$ | `MineralParams.BM3KT(f)` | 冷体積弾性率 |
| 冷 $G(f)$ | `MineralParams.BM3GT(f)` | 冷剪断弾性率 |
| 自己無撞着ループ | `MieGruneisenEOSOptimizer` | 反復求解 |
| Debye 関数 $D_3(x)$ | `DebyeFunctionCalculator` | Debye 積分 |
| $\mu(f)$, $\gamma(f)$, $\theta(f)$ | `ThermoMineralParams` コンストラクタ | 歪み依存量 |
| $K_T$, $K_S$, $G$ | `ThermoMineralParams.KT`, `.KS`, `.GS` | 弾性率 |
| $V_p$, $V_s$, $V_\phi$ | `ThermoMineralParams.Vp`, `.Vs`, `.Vb` | 地震波速度 |
| $F$, $G$, $S$ | `ThermoMineralParams.F`, `.GibbsG`, `.Entropy` | 熱力学ポテンシャル |
| $\alpha$ | `ThermoMineralParams.Alpha` | 熱膨張率 |
| $\rho$ | `ThermoMineralParams.Rho` | 密度 |

### 5.7.2 計算の流れ（コード対応）

典型的な使用例のコード対応を示す：

```
// Step 1: 鉱物パラメータの準備
var mineral = SLB2011Endmembers.Forsterite();  // MineralParams インスタンス

// Step 2: 自己無撞着ループの実行
var optimizer = new MieGruneisenEOSOptimizer(
    targetPressure: 14.0,   // GPa
    temperature: 1673.0,    // K
    mineral: mineral
);

// Step 3: 収束確認
if (optimizer.IsConverged) {
    var result = optimizer.Result;  // ThermoMineralParams

    // Step 4: 物性の取得
    double rho = result.Rho;       // 密度 [g/cm^3]
    double KS  = result.KS;       // 断熱体積弾性率 [GPa]
    double G   = result.GS;       // 剪断弾性率 [GPa]
    double Vp  = result.Vp;       // P波速度 [m/s]
    double Vs  = result.Vs;       // S波速度 [m/s]
}
```

### 5.7.3 拡張機能

基本的な11パラメータを超える拡張が、ThermoMineralParams クラスに実装されている：

1. **Landau 理論**（変位型相転移）: 臨界温度 $T_{c0}$、過剰体積 $V_D$、過剰エントロピー $S_D$ を追加パラメータとして、体積・エントロピー・自由エネルギーに加法的補正を行う。

2. **磁気的寄与**（遷移金属含有鉱物）: スピン量子数 $S$ と磁気原子数 $r$ を追加し、スピン遷移に伴う異常弾性挙動を記述する。

3. **基準 Helmholtz エネルギー $F_0$**: 絶対自由エネルギー計算のための基準値。

これらの拡張は、SLB 枠組みのコアの熱力学的自己無撞着性を保持したまま、加法的補正として実装されている。

---

## 5.8 歴史的背景

### 5.8.1 二つの知的系譜

SLB 定式化は、20世紀半ばに並行して発展した二つの理論的伝統の合流点に位置する。

**第一の系譜: 有限歪み理論**

Francis Birch（1947, 1952, 1978）は、Euler 歪みが圧縮された固体の弾性エネルギーに対する自然な展開変数を提供することを認識した。Birch の状態方程式と、弾性率を有限歪みの多項式として表す拡張は、静的圧縮実験の解析のための標準的枠組みとなった。

**第二の系譜: Mie-Gruneisen 熱モデル**

Gruneisen（1926）、Slater（1939）、Dugdale and MacDonald（1953）の研究を通じて発展した Mie-Gruneisen モデルは、内部エネルギーと Gruneisen パラメータを介して熱圧力を関連づける。

### 5.8.2 SLB 定式化の成立

これら二つのアプローチの統合的な熱力学的枠組みへの合成は、Stixrude and Lithgow-Bertelloni による二つの画期的論文（2005年、2011年）で達成された。

**2005年論文（SLB2005）**: 理論的枠組みの提示と個別鉱物への適用を実証した。Gruneisen パラメータと Debye 温度を、体積の関数としてではなく Euler 有限歪みの陽な関数として表すという鍵となる選択により、自由エネルギーから導出されるすべての熱力学的性質の相互整合性を保証した。これは、弾性的性質と熱的性質を独立に扱う従来のパラメータ化に欠けていた性質である。

**2011年論文（SLB2011）**: マントル鉱物の端成分パラメータの包括的データベースを提供し、固溶体と相平衡を含むように枠組みを拡張した。

### 5.8.3 実用的影響

SLB 定式化の実用的影響は甚大であった。BurnMan オープンソースツールキット（Cottaar et al., 2014）は SLB2011 データベースを広い地球物理学コミュニティにアクセス可能にし、地震学的観測との比較のためのマントル物性のルーチン計算を可能にした。ThermoElasticCalculator プロジェクトは同じ定式化を C# でデスクトップ GUI 付きで実装し、Python の専門知識なしに教育・研究に利用可能としている。

SLB データベースは複数のグループによって更新・拡張されており、競合するデータベース（例：Holland and Powell, 2011; Xu et al., 2008）は類似しているが同一ではない理論的枠組みを使用し、データと理論の改善を推進する健全な科学的競争を生み出している。

---

## 5.9 未解決課題

### 5.9.1 高温における準調和 Debye モデルの限界

3000 K を超える温度では、本質的な非調和性や融解前効果が顕著になる。準調和 Debye モデルはこれらの効果を捉えることができない。SLB 自由エネルギーに陽な非調和補正項を追加すべきかどうかは、活発な研究の対象である。高温実験データと第一原理計算の比較から、3000 K 以上では熱膨張率や比熱に数%以上の系統的偏差が生じうることが示されている。

### 5.9.2 圧力依存の $q_0$

SLB2011 データベースは歪み多項式の熱膨張において圧力非依存の $q_0$ を仮定しているが、高圧実験は $q$ が圧縮とともに変化することを示唆している。圧力依存の $q$ が下部マントルの予測速度にどの程度影響するかは重要な問題である。特に、$q$ の圧力依存性は Gruneisen パラメータの深部マントルでの挙動を通じて、熱構造の推定に影響を与えうる。

### 5.9.3 フォノン分散データとの整合性

11パラメータの枠組みを、非弾性X線散乱によるフォノン分散曲線のような新しいデータ型に同時にフィットするように拡張できるか、あるいは Debye モデルを超えるモデルが必要かという問題がある。近年の放射光実験技術の進歩により、高圧下でのフォノン分散の直接測定が可能になりつつあるが、これらのデータの活用には理論的枠組みの拡張が求められる。

### 5.9.4 鉄含有鉱物のスピン遷移

bridgmanite や ferropericlase のような鉄含有鉱物では、Fe$^{2+}$ のスピン遷移が基本的な SLB 定式化では捉えられない異常弾性挙動を生じる。`SpinCrossoverCalculator` を SLB 自由エネルギーに自己無撞着に統合する方法は、依然として開かれた問題である。下部マントルの弾性的異常の解釈にとって、この問題の解決は不可欠である。

### 5.9.5 Landau モデルの臨界指数

変位型転移に対する Landau モデルは三重臨界指数（秩序パラメータに対して1/4）を使用している。これが SLB データベースのすべての転移に適切であるか、あるいは一部の転移（例：stishovite から post-stishovite CaCl$_2$ 型転移）では異なる臨界指数が必要かという問題がある。

### 5.9.6 相境界予測に対するパラメータ不確実性の影響

相境界予測（例：660 km 不連続面）が SLB パラメータの不確実性、特にしばしば最も制約が弱い $K'_0$ と $\gamma_0$ にどの程度感受的であるかは、定量的に評価される必要がある。Monte Carlo 的なパラメータ感度解析が、予測の信頼区間を評価するために有用である。

---

## 5.10 演習問題

### 演習問題1：基本パラメータの回復

`ThermoMineralParams` を $f = 0.0001$（ほぼゼロの圧縮）、$T = 300$ K で構築し、forsterite の常温常圧基準物性（$K_0$, $G_0$, $\gamma_0$, $\theta_0$, $V_0$）が1%以内で回復されることを検証せよ。

**ヒント**: 有限歪み $f \to 0$ で $\mu \to 1$、$\gamma \to \gamma_0$、$\theta \to \theta_0$ となることを歪み多項式から確認せよ。熱補正項は $\Delta E \to 0$（$T = T_{\text{ref}}$）であるため消失する。

### 演習問題2：Gruneisen パラメータの歪み依存性

Forsterite に対して、$f = 0$ から $f = 0.15$ までの範囲で Gruneisen パラメータ $\gamma(f)$ をプロットせよ。単純なべき乗則近似 $\gamma = \gamma_0 (V/V_0)^{q_0}$ と比較し、両者が乖離する条件を議論せよ。

**ヒント**: $V/V_0 = (1+2f)^{-3/2}$ の関係を用いよ。べき乗則では $\gamma = \gamma_0 (1+2f)^{3q_0/2}$ となる。歪み多項式の $A_{iikk}$ 項が二次の補正を導入する。

### 演習問題3：熱圧力の温度依存性

Mg-perovskite に対して、固定圧縮 $f = 0.05$ のもとで、温度 300–3000 K の範囲における熱圧力 $\Delta P$ を計算しプロットせよ。熱圧力が10 GPa を超える温度を求めよ。

**ヒント**: $\Delta P = (\gamma/V) \Delta E$ であり、$\Delta E$ は Debye 内部エネルギー差として計算される。高温（$T \gg \theta$）では $\Delta E \approx 3nk_B(T - T_{\text{ref}}) N_A / 1000$ [kJ/mol] の Dulong-Petit 近似が成り立つ。

### 演習問題4：$K_T$ の自由エネルギーからの導出

Helmholtz 自由エネルギー $F(V,T)$ から出発して、$K_T = V \cdot \partial^2 F / \partial V^2$ を計算することで $K_T$ の表式を導出せよ。これが `ThermoMineralParams.KT` で使用される式と等価であることを示せ。

**ヒント**: $\partial/\partial V = (\partial f/\partial V) \cdot \partial/\partial f$ の連鎖律を適用し、$\partial f/\partial V = -(1+2f)/(3V)$ の関係を用いよ。冷圧縮項と熱的項のそれぞれについて微分を実行せよ。

### 演習問題5：断熱地温勾配に沿った速度プロファイル

SLB2011 予測を用いて、断熱地温勾配（ポテンシャル温度1600 K）に沿った $V_p$ と $V_s$ を深さ0–800 km の範囲で計算し、PREM 基準モデルと比較せよ。主要な相転移とその速度への影響を特定せよ。

**ヒント**: 各深さに対して、圧力は PREM から取得し、温度は断熱地温勾配 $dT/dz = \alpha g T / C_p$ から計算する。olivine（0–410 km）、wadsleyite（410–520 km）、ringwoodite（520–660 km）、bridgmanite + ferropericlase（660 km 以深）の各相に対して SLB 計算を実行する。

### 演習問題6：$\eta_{S0}$ が剪断弾性率温度微分に与える影響

Forsterite に対して、$\eta_{S0} = 1.0$, $2.3$, $3.5$ の三つの値で $T = 1000$ K と $T = 2000$ K における剪断弾性率 $G$ を計算し、地震トモグラフィー解釈への示唆を議論せよ。

**ヒント**: $G = G^{\text{cold}} - \eta_S \Delta E / V$ であるため、$\eta_{S0}$ が大きいほど温度上昇による $G$ の減少が大きい。$dG/dT$ の絶対値が大きいことは、速度異常の温度解釈においてより小さな温度異常を必要とすることを意味する。

### 演習問題7：Maxwell 関係の数値検証

数値微分を用いて、以下の Maxwell 関係を `ThermoMineralParams` の物性から検証せよ：

$$\left(\frac{\partial \alpha}{\partial P}\right)_T = -\frac{1}{K_T^2} \left(\frac{\partial K_T}{\partial T}\right)_P$$

Forsterite に対して $P = 10$ GPa, $T = 1500$ K で数値微分を行い、左辺と右辺が一致することを確認せよ。

**ヒント**: 中心差分法を用いよ。$\Delta P = 0.1$ GPa, $\Delta T = 1$ K 程度の微小量が適切である。

### 演習問題8：Landau パラメータと相転移

Quartz の $\alpha$-$\beta$ 転移（Landau パラメータ $T_{c0}$, $V_D$, $S_D$ を使用）に対して、秩序パラメータ $Q$、過剰体積、過剰エントロピーを $P = 0$ および $P = 5$ GPa での温度の関数としてプロットせよ。転移温度が $dT_c/dP = V_D/S_D$ に従って圧力とともにシフトすることを検証せよ。

**ヒント**: 三重臨界 Landau 理論では $Q^4 = 1 - T/T_c$ であり、過剰体積は $V_{\text{excess}} = V_D \cdot Q^2$、過剰エントロピーは $S_{\text{excess}} = S_D \cdot Q^2$ で与えられる。

---

## 5.11 図表

### 図5.1 SLB2011 計算ワークフローの模式図

**内容**: 11個の入力パラメータから自己無撞着ループを経て出力物性に至る計算フローチャート。

**構成要素**:
- `MineralParams`（11パラメータ）: 入力ブロック
- `MieGruneisenEOSOptimizer`（$P$, $T$ ループ）: 反復フィードバックを強調
- `ThermoMineralParams`（すべての導出物性）: 出力ブロック
- 出力物性の一覧: $\rho$, $K_T$, $K_S$, $G$, $V_p$, $V_s$, $\alpha$, $F$, $G$, $S$

### 図5.2 Helmholtz 自由エネルギーの三つの寄与

**内容**: $T = 2000$ K における Mg-perovskite の Helmholtz 自由エネルギーの三つの寄与（$F_0$, $F_{\text{cold}}$, $F_{\text{thermal}}$）を圧力の関数として示す。

**軸**: 横軸 — 圧力（GPa, 0–140）、縦軸 — 自由エネルギー寄与（kJ/mol）

**注目点**:
- $F_{\text{cold}}$ は圧力とともに放物線的に増加
- $F_{\text{thermal}}$ は大きな負の寄与で、圧力に対してゆるやかに変化
- $F_0$ は定数オフセット
- 全 $F$ はこれらの和
- 相対的な大きさの標示

### 図5.3 弾性率に対する冷圧縮寄与と熱的寄与の比較

**内容**: 1600 K 断熱温度に沿った forsterite の $K_T$ と $G$ に対する冷圧縮寄与と熱補正項の比較。

**軸**: 横軸 — 圧力（GPa, 0–15）、縦軸 — 弾性率（GPa）

**注目点**:
- $K_{T,\text{cold}}(f)$, $K_T$ 熱補正, $K_T$ 全体を同じ軸上に表示
- $G$ についても同様
- 冷圧縮寄与が支配的であるが、熱補正は5–15%で系統的に300 K の値に対して両弾性率を減少させる

### 図5.4 Gruneisen パラメータと Debye 温度の歪み依存性

**内容**: 複数のマントル鉱物に対する $\gamma(f)$ と $\theta(f)$ の有限歪み依存性。

**軸**: 横軸 — 有限歪み $f$（0–0.15）、左縦軸 — $\gamma$（0.5–2.0）、右縦軸 — $\theta$（600–1400 K）

**注目点**:
- Forsterite, bridgmanite, ferropericlase を含む
- 圧縮に伴う $\gamma$ の減少と $\theta$ の増加
- 410 km と 660 km 不連続面圧力に対応する歪み値の標示

### 図5.5 MieGruneisenEOSOptimizer の収束挙動

**内容**: 圧力残差の反復回数依存性。

**軸**: 横軸 — 反復回数（0–15）、縦軸 — $|P_{\text{target}} - P_{\text{computed}}|$（GPa、対数スケール）

**注目点**:
- 低圧（5 GPa）、高圧（100 GPa）、高温（3000 K）の複数のケースを表示
- 収束が典型的に指数的であり5–10回の反復で達成されることを示す
- $10^{-5}$ GPa の許容値の標示

### 図5.6 パイロライト組成のマントル物性プロファイル

**内容**: マントル地温勾配に沿った密度、$K_S$、$G$、$V_p$、$V_s$、$V_\phi$ のプロファイル。

**軸**: 横軸 — 深さ（km）または圧力（GPa）、縦軸 — 各物性の単位

**注目点**:
- 複数パネル図ですべての物性を表示
- PREM を重ね描きして比較
- Olivine → wadsleyite、wadsleyite → ringwoodite、post-spinel 転移の標示
- SLB 定式化が主要な地震学的不連続面を再現することを示す

### 表5.1 SLB2011 主要端成分鉱物パラメータ

| 鉱物 | $V_0$ | $K_0$ | $K'_0$ | $G_0$ | $G'_0$ | $\theta_0$ | $\gamma_0$ | $q_0$ | $\eta_{S0}$ | $n$ | $M$ |
|---|---|---|---|---|---|---|---|---|---|---|---|
| | cm$^3$/mol | GPa | — | GPa | — | K | — | — | — | — | g/mol |
| Forsterite | 43.603 | 127.96 | 4.218 | 81.6 | 1.463 | 809.2 | 0.993 | 2.107 | 2.300 | 7 | 140.69 |
| Mg-Wadsleyite | 40.515 | 168.69 | 4.323 | 112.0 | 1.444 | 843.5 | 1.206 | 2.019 | 2.637 | 7 | 140.69 |
| Mg-Ringwoodite | 39.493 | 184.00 | 4.220 | 119.0 | 1.354 | 878.0 | 1.108 | 1.600 | 2.400 | 7 | 140.69 |
| Mg-Perovskite | 24.445 | 250.53 | 4.140 | 172.9 | 1.690 | 905.9 | 1.565 | 1.109 | 2.565 | 5 | 100.39 |
| Periclase | 11.244 | 160.18 | 3.913 | 130.3 | 2.088 | 773.0 | 1.524 | 1.650 | 2.800 | 2 | 40.30 |

注：値は SLB2011 データベース（Stixrude and Lithgow-Bertelloni, 2011, Table A1）から抜粋。完全なデータベースは60種以上の端成分を含む。

### 表5.2 歪み係数の定義

| 係数 | 定義 | 物理的意味 |
|---|---|---|
| $A_{ii}$ | $6\gamma_0$ | Debye 温度の一次歪み依存性 |
| $A_{iikk}$ | $-12\gamma_0 + 36\gamma_0^2 - 18\gamma_0 q_0$ | Debye 温度の二次歪み依存性 |
| $A_S$ | $-2\gamma_0 - 2\eta_{S0}$ | 剪断弾性率の熱的歪み依存性 |

---

## 5.12 参考文献

1. **Stixrude, L. and Lithgow-Bertelloni, C.** (2011). Thermodynamics of mantle minerals - II. Phase equilibria. *Geophysical Journal International*, 184, 1180-1213.
   - SLB2011 データベースの決定版。完全な数学的定式化、すべての端成分パラメータ（Table A1）、固溶体混合モデルを提供。ThermoElasticCalculator 実装の第一次資料。

2. **Stixrude, L. and Lithgow-Bertelloni, C.** (2005). Thermodynamics of mantle minerals - I. Physical properties. *Geophysical Journal International*, 162, 610-632.
   - SLB2011 の理論的基礎を詳述した伴論文。有限歪み展開、Debye モデル結合、Helmholtz 自由エネルギーからのすべての熱力学的性質の導出を提示。

3. **Birch, F.** (1978). Finite strain isotherm and velocities for single-crystal and polycrystalline NaCl at high pressures and 300 K. *Journal of Geophysical Research*, 83, 1257-1268.
   - SLB が拡張した弾性的性質の有限歪み枠組みを確立。$K$ と $G$ が Euler 有限歪みの多項式として表されることを示し、BM3KT と BM3GT の基礎となった。

4. **Cottaar, S., Heister, T., Rose, I., and Unterborn, C.** (2014). BurnMan: A lower mantle mineral physics toolkit. *Geochemistry, Geophysics, Geosystems*, 15, 1164-1179.
   - SLB2011 定式化のオープンソース Python 実装。ThermoElasticCalculator の検証基準として機能するコミュニティ標準の実装。

5. **Anderson, O.L.** (1995). *Equations of State of Solids for Geophysics and Ceramic Science*. Oxford University Press.
   - 熱状態方程式の包括的モノグラフ。SLB 定式化全体を通じて使用される Mie-Gruneisen モデル、Debye 理論、熱力学量間の関係の物理的背景を提供。

6. **Birch, F.** (1947). Finite elastic strain of cubic crystals. *Physical Review*, 71, 809-824.
   - 有限歪み理論の原論文。Euler 歪みに基づく弾性エネルギーの展開を導入。

7. **Birch, F.** (1952). Elasticity and constitution of the Earth's interior. *Journal of Geophysical Research*, 57, 227-286.
   - 有限歪み状態方程式の地球物理学への適用。Birch-Murnaghan 状態方程式の確立。

8. **Gruneisen, E.** (1926). Zustand des festen Korpers. *Handbuch der Physik*, 10, 1-59.
   - Gruneisen パラメータの定義と固体の熱的性質との関係を確立した古典的論文。

9. **Holland, T.J.B. and Powell, R.** (2011). An improved and extended internally consistent thermodynamic dataset for phases of petrological interest. *Journal of Metamorphic Geology*, 29, 333-383.
   - SLB と競合する内部整合熱力学データベース。異なる理論的枠組みに基づくが、同様の目的を持つ。

10. **Xu, W., Lithgow-Bertelloni, C., Stixrude, L., and Ritsema, J.** (2008). The effect of bulk composition and temperature on mantle seismic structure. *Earth and Planetary Science Letters*, 275, 70-79.
    - SLB 定式化のマントル地震構造への適用例。組成と温度の効果を系統的に調査。

---

## 5.13 他章との関連

### 第1章：高圧固体の熱力学（前提）

第1章は Birch-Murnaghan 状態方程式と有限歪み理論を導入しており、SLB 定式化の冷圧縮基盤を形成する。`MineralParams` の `GetPressure(f)`、`BM3KT(f)`、`BM3GT(f)` メソッドは第1章の理論の直接的実装である。SLB 定式化は、第1章の冷圧縮理論を熱的効果と統合する枠組みを提供する。

### 第2章：Debye モデルと熱的性質（前提）

第2章は Debye 関数 $D_3(x)$ とそれを用いた内部エネルギー、比熱、エントロピー、自由エネルギーの計算を扱う。`DebyeFunctionCalculator` クラスは第2章で導入されたすべての Debye 積分を実装し、`ThermoMineralParams` における熱的寄与を提供する。

### 第3章：Gruneisen パラメータと非調和性（前提）

第3章は Gruneisen パラメータ $\gamma$ とその体積依存性（$q_0$ によるパラメータ化）を定義する。SLB 定式化はこれを拡張し、$\gamma(f)$、$q(f)$、$\eta_S(f)$ を $\mu(f)$ 多項式を用いた有限歪みの陽な関数として表現する。第3章の理論は SLB 枠組みの熱的部分の基礎となる。

### 第4章：弾性率と地震波速度（前提/応用）

第4章は $K_T$、$K_S$、$G$ と地震波速度を導入する。第5章（本章）はこれらすべてを自己無撞着に計算する完全な枠組みを提供する。熱補正を含む $K_T$、$K_S$、$G$ の公式は、第1章–第4章の個別トピックを一つの統合的計算に合成する。

### 相平衡と Gibbs 最小化（発展）

`ThermoMineralParams` で計算される Gibbs 自由エネルギー $G$ は、相平衡計算（`GibbsMinimizer`、`SolutionCalculator`）への入力である。SLB 定式化は、相と組成を比較して安定な組み合わせを決定するために使用される自由エネルギーを提供する。

### 固溶体と混合モデル（発展）

`SLB2011Solutions` は、端成分パラメータが固溶体に対してどのように組み合わされるか（例：olivine における forsterite と fayalite の混合）を定義する。本章の端成分ごとの SLB 定式化は、混合モデルが構築される基盤である。

### 岩石物性と PREM 比較（応用）

`RockCalculator` と `MixtureCalculator` は、単一鉱物の SLB 結果を Voigt-Reuss-Hill 平均や Hashin-Shtrikman 限界を用いて集約し、地温勾配に沿った岩石物性を予測する。これらの結果は PREM のような地震学的観測と直接比較される。SLB 定式化の究極的な応用は、このような地球内部構造の計算物質科学的理解である。

---

**章末ノート**: 本章で提示した SLB2011 定式化は、マントル鉱物学における最も広く利用される自己無撞着な熱力学的枠組みである。11個の実験的パラメータから出発し、単一の Helmholtz 自由エネルギー関数を通じてすべての熱力学的・弾性的性質を導出するこのアプローチは、理論的厳密性と実用的有用性を兼ね備えている。ThermoElasticCalculator の実装と照合しながら学ぶことで、読者は抽象的な定式化と具体的な数値計算の間の橋渡しを体験できるであろう。
