# 第13章 相図と相境界

## 13.1 概要

相図は計算鉱物物理学の中心的な成果物であり、競合する鉱物相の安定領域を圧力と温度の関数としてマッピングする。第12章で確立した Gibbs 自由エネルギーの枠組みを基盤として、本章では相境界の体系的な計算に焦点を当てる。相境界とは、2つ（またはそれ以上）の相が Gibbs エネルギーの等しさ（$G_1 = G_2$）により共存する $P$-$T$ 空間上の曲線である。

`PhaseDiagramCalculator` クラスが完全なツールキットを提供する：

- **`CalculateDiagram`**: $P$-$T$ グリッド全体で Gibbs 最小化を評価し、各点での安定な集合体を同定
- **`FindPhaseBoundary`**: 二分法を用いて、所与の温度で $G_1 = G_2$ となる正確な圧力を特定
- **`TracePhaseBoundary`**: 温度範囲にわたってスイープし、連続的な境界曲線を生成
- **`FindMultiPhaseBoundary`**: 化学量論係数を含む多相反応（例：ringwoodite $\rightarrow$ bridgmanite + ferropericlase）に一般化

Clapeyron 傾斜 $dP/dT = \Delta S / \Delta V$ は相境界の最も重要な定量的記述子であり、熱力学的情報と地球力学的帰結の両方を符号化する。410 km 不連続面（olivine-wadsleyite 転移）での正の Clapeyron 傾斜（$\sim +3.6$ MPa/K）は境界を越えるマントル流を促進し、660 km 不連続面（ポストスピネル転移）での負の Clapeyron 傾斜（$\sim -2$ 〜 $-3$ MPa/K）は沈み込むスラブの停滞を引き起こしうる浮力障壁を生成する。

SLB2011 データベースから計算された相図は、マルチアンビルおよびダイヤモンドアンビルセル実験からの実験的相平衡データ、ならびに不連続面の深度と鮮明さに関する地震学的観測と直接比較できる。

---

## 13.2 前提知識

本章を理解するために、以下の前提知識が必要である。

1. **Gibbs 自由エネルギーの計算**: Helmholtz 自由エネルギーからの $G = F + PV$ の Legendre 変換（第12章）。

2. **Mie-Gruneisen 状態方程式ソルバー**: 対象 $(P, T)$ での平衡体積を求める EOS オプティマイザ（第6章）。

3. **SLB 枠組み**: $F(V, T)$ のすべてのエネルギー寄与を含む定式化（第5章）。

4. **SLB2011 鉱物データベース**: 相間比較のための $F_0$ 参照エネルギー（第7章）。

5. **エントロピーの計算**: $F$ の数値微分によるエントロピー（第12章）。

6. **弾性混合モデル**: 多相集合体の物性計算（第10章）。

7. **二分法**: 連続関数における符号変化を探索する根探索アルゴリズム。

8. **固溶体の熱力学**: 理想混合エントロピーと過剰 Gibbs エネルギー（第10章）。

9. **Landau 理論**: Gibbs エネルギー地形を修正する変位型転移（第8章）。

---

## 13.3 理論と方程式

### 13.3.1 二相平衡条件（相境界）

相境界は、2つの相が等しい Gibbs エネルギーを持つ $(P, T)$ 点の軌跡である：

$$\boxed{G_1(P^*, T) = G_2(P^*, T)}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $G_1$ | 相1の Gibbs 自由エネルギー | kJ/mol |
| $G_2$ | 相2の Gibbs 自由エネルギー | kJ/mol |
| $P^*$ | 平衡（境界）圧力 | GPa |
| $T$ | 温度 | K |

`PhaseDiagramCalculator.FindPhaseBoundary` は二分法で $P^*$ を探索する：$P_{\min}$ と $P_{\max}$ で $\Delta G = G_1 - G_2$ を評価し、符号変化を確認し、最大50回の反復で区間を半分にして $|\Delta G| < 0.01$ kJ/mol の許容誤差に収束する。プライベートメソッド `ComputeGDiff` が各相の `GibbsMinimizer.ComputePhaseGibbs` を呼び出す。

### 13.3.2 多相反応の平衡

ringwoodite $\rightarrow$ bridgmanite + ferropericlase のような多相反応では、平衡条件は化学量論係数で重み付けされる：

$$\boxed{\sum_i \nu_i^{\text{prod}} G_i^{\text{prod}}(P^*, T) = \sum_j \nu_j^{\text{react}} G_j^{\text{react}}(P^*, T)}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $\nu_i$ | 相 $i$ の化学量論係数 | 無次元 |
| $G_i^{\text{prod}}$ | 生成相 $i$ の Gibbs エネルギー | kJ/mol |
| $G_j^{\text{react}}$ | 反応相 $j$ の Gibbs エネルギー | kJ/mol |
| $P^*$ | 平衡圧力 | GPa |

`FindMultiPhaseBoundary` は反応の Gibbs エネルギー差を化学量論的に重み付けして計算し、$\Delta G_{\text{reaction}} = 0$ となる圧力を二分法で求める。

### 13.3.3 反応の Gibbs エネルギー

反応の自発性は反応 Gibbs エネルギーで判定される：

$$\boxed{\Delta G_{\text{reaction}}(P, T) = \sum_i \nu_i^{\text{prod}} G_i(P, T) - \sum_j \nu_j^{\text{react}} G_j(P, T)}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $\Delta G_{\text{reaction}}$ | 反応の Gibbs エネルギー変化 | kJ/mol |
| $\nu_i$ | 化学量論係数 | 無次元 |
| $G_i$ | 各相の Gibbs エネルギー | kJ/mol |

- $\Delta G_{\text{reaction}} < 0$: 反応は正方向に自発的に進行（生成物が安定）
- $\Delta G_{\text{reaction}} > 0$: 反応物が安定
- $\Delta G_{\text{reaction}} = 0$: 相境界

`ComputeMultiPhaseGDiff` が反応物と生成物のリストを巡回して重み付き和を計算する。

### 13.3.4 Clapeyron 方程式（$dP/dT$ 形式）

$$\boxed{\frac{dP}{dT} = \frac{\Delta S}{\Delta V}}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $dP/dT$ | $P$-$T$ 空間における相境界の傾き | GPa/K |
| $\Delta S$ | 転移にわたるエントロピー変化 $S_2 - S_1$ | J/(mol$\cdot$K) |
| $\Delta V$ | 転移にわたる体積変化 $V_2 - V_1$ | cm$^3$/mol |

`PhaseDiagramCalculator.ComputeClapeyronSlope` が $(P, T)$ で両相の `MieGruneisenEOSOptimizer` を実行し、$(\Delta S / \Delta V) \times 0.001$ を返す。$|\Delta V| < 10^{-10}$ の場合は `NaN` を返す。

### 13.3.5 Clapeyron 方程式（$dT/dP$ 形式、逆形式）

$$\boxed{\frac{dT}{dP} = \frac{\Delta V}{\Delta S}}$$

| 変数 | 意味 | 単位 |
|------|------|------|
| $dT/dP$ | 逆 Clapeyron 傾斜 | K/GPa |
| $\Delta V$ | 体積変化 $V_2 - V_1$ | cm$^3$/mol |
| $\Delta S$ | エントロピー変化 $S_2 - S_1$ | J/(mol$\cdot$K) |

`GibbsMinimizer.ClapeyronSlope` が静的メソッドとして $\Delta V / \Delta S \times 1000$ [K/GPa] を返す。$|\Delta S| < 10^{-10}$ の場合は $0$ を返す。この形式は境界温度が圧力とともにどう変化するかを計算する際により自然である。

---

## 13.4 物理概念

### 13.4.1 Gibbs エネルギー交差としての相境界

相境界は、2つの競合する相の Gibbs 自由エネルギー曲線が交差する $P$-$T$ 空間上の点の軌跡である。

**物理的直観**: 固定温度で2つの多形体の $G$ vs $P$ をプロットすることを想像せよ。両曲線は圧力とともに減少する（$dG/dP = V > 0$）が、より密な相は体積が小さいため圧力あたりの減少が速い。低圧では密度の低い相が低い $G$ を持ち安定である。ある交差圧力で曲線が交差し、密な相が安定となる。`FindPhaseBoundary` の二分法アルゴリズムは $G_1 - G_2$ の符号変化を探索することでこの交差を検出する。

> **よくある誤解**:
>
> - **自然界での鮮明な境界線**: 実際には速度論的障壁が平衡安定領域を超えた相の準安定的存続を生じさせうる。特に冷たい沈み込むスラブにおいて顕著。
> - **純端成分 vs 固溶体**: Fe を含む $(Mg, Fe)_2SiO_4$ 系では境界は単変量の線ではなく二相ループとなり、組成分配の考慮が必要。
> - **相境界の直線性**: Clapeyron 方程式は局所的な傾きを制約するが、$\Delta S$ と $\Delta V$ は $P$ と $T$ で変化するため境界は一般に曲線である。

### 13.4.2 多相反応と 660 km 不連続面

660 km 不連続面を形成するポストスピネル反応は、単純な多形転移とは異なる不均化反応である：

$$\text{Ringwoodite (Mg}_2\text{SiO}_4\text{)} \rightarrow \text{Bridgmanite (MgSiO}_3\text{)} + \text{Ferropericlase (MgO)}$$

1つの反応物がより単純な2つの生成相に分解する。平衡条件は生成物集合体の全 Gibbs エネルギー（化学量論的に重み付け）が反応物の Gibbs エネルギーに等しいことを要求する。

$$G_{\text{ri}} = 1 \cdot G_{\text{bm}} + 1 \cdot G_{\text{pe}}$$

`FindMultiPhaseBoundary` は各側の $\sum(\nu \cdot G)$ を計算し、二分法で平衡圧力を求める。

**負の Clapeyron 傾斜**（$\sim -2$ 〜 $-3$ MPa/K）は、境界圧力が温度の増加とともに減少することを意味する。冷たい沈み込むスラブは転移をより高い圧力（深い深度）で経験し、浮力を与える相変態が遅延する。これは多くの沈み込み帯の下で地震トモグラフィーに観測されるスラブ停滞に寄与する抵抗を生む。

> **注意**:
> - 多相反応を単純な二相境界として扱うことは不正確。化学量論係数を $G$ のバランスに考慮しなければ境界圧力が誤る。
> - 660 km での負の Clapeyron 傾斜が境界を越える流動を完全に阻止するわけではない。浮力障壁は有限であり、十分に強いスラブは下部マントルに貫入できる。
> - 実際のマントルには鉄が含まれ、bridgmanite と ferropericlase 間で不均一に分配されるため、不連続面の幅が広がり深度がシフトする。

### 13.4.3 Clapeyron 傾斜と地球力学的影響

Clapeyron 傾斜の符号が対流パターンに及ぼす影響は定量的に重要である：

**正の Clapeyron 傾斜**（例：410 km、$\sim +3.6$ MPa/K）:
- 冷たい下降流物質は転移をより浅い深度（低い $P$、低い $T$ が境界を低い $P$ にシフト）で経験
- より密な高圧相が冷たいスラブ中でより早く形成される
- 負の浮力が追加されスラブを下方に引く --- 境界を越える流動を促進する**正のフィードバック**

**負の Clapeyron 傾斜**（例：660 km、$\sim -2$ 〜 $-3$ MPa/K）:
- 冷たい物質は転移をより深い深度（高い $P$）で経験
- 浮力を与える相変態が遅延し浮力障壁を形成
- 地球力学モデリングでは、$\sim -4$ MPa/K より負の Clapeyron 傾斜が層状対流を生じうることが示されている

> **符号の慣例**: $dP/dT = \Delta S / \Delta V$ において $\Delta$ は高圧相マイナス低圧相と定義。660 km 転移では $\Delta V < 0$（生成物がより密）かつ $\Delta S > 0$（二相混合物の無秩序により生成物のエントロピーが高い）であるため、$dP/dT < 0$ となる。

### 13.4.4 相図のグリッド計算

`PhaseDiagramCalculator.CalculateDiagram` は $P$-$T$ グリッドの各点で `GibbsMinimizer.Minimize` を呼び出し、2次元の安定領域マップを生成する。

最小化器はすべての候補相を等量（`Amount = 1.0`）で開始し、不安定な相の量をゼロに駆動しつつ安定な相に物質を集中させる。2つの純端成分に対しては `MinimizeTwoPhase` が単純に $G$ 値を比較する。多相系や固溶体系では `MinimizeGeneral` が反復的射影勾配降下法を使用する。

| パラメータ | 値 |
|-----------|-----|
| 初期相量 | 1.0（全候補相で等量） |
| 相除去閾値 | $\epsilon = 10^{-10}$ |
| 収束基準 | $|\sum(\text{方向} \times \mu)| < 10^{-8}$ |
| 最大反復数 | 200 |
| 直線探索ステップ | $\min(0.1, 0.9 \times \text{最大許容ステップ})$ |

> **グリッド解像度の重要性**: 粗いグリッドは狭い安定領域を見逃したりギザギザの境界を生成したりする。`TracePhaseBoundary` が特定の二相平衡に対してより高解像度の境界トレースを提供する。

### 13.4.5 核-マントル境界でのポストペロブスカイト転移

Bridgmanite（MgSiO$_3$ ペロブスカイト）からポストペロブスカイト（CaIrO$_3$ 型構造）への変態は $\sim 125$ GPa、$2500$ K 付近で生じ、マントル底部の D'' 地震波不連続面を説明する可能性がある。

`PostPerovskiteCalculator` は `PhaseDiagramCalculator` をラップして pv-ppv 転移を専門的にモデル化する：

- **`FindBoundary`**: $P = 100$-$140$ GPa で境界を探索
- **`CompareAcrossTransition`**: 物性コントラスト（$\Delta V_S\%$, $\Delta V_P\%$, $\Delta\rho\%$）を返す

**急な正の Clapeyron 傾斜**（$\sim 8$-$10$ MPa/K）は転移が強い温度依存性を持つことを意味する。核-マントル境界付近の $\sim 1000$ K の横方向温度変動内で境界圧力が $8$-$10$ GPa シフトしうるため、強い熱勾配の領域で境界の二重交差（ダブルクロッシング）が生じる可能性がある。

> **ポストペロブスカイトの存在範囲**: 正の Clapeyron 傾斜と横方向温度変動により、ポストペロブスカイトはマントル底部の全域には存在しない可能性がある。冷たい領域（沈み込むスラブの下）では安定であるが、高温領域（大規模低せん断速度域の下）では bridgmanite が安定のままでありうる。

---

## 13.5 計算例

### 13.5.1 計算例1: Olivine-Wadsleyite 相境界（410 km 不連続面）

**問題**: $T = 1600$ K における forsterite-wadsleyite（fo-mw）相境界圧力を `FindPhaseBoundary` で求め、Clapeyron 傾斜を計算せよ。

**解法**:

1. SLB2011 データベースから forsterite と Mg-wadsleyite の `PhaseEntry` オブジェクトを生成
2. $T = 1600$ K、$P = 5$-$25$ GPa で `FindPhaseBoundary` を呼び出す
3. 5 GPa で $G_{\text{fo}} - G_{\text{mw}}$ を評価（正の期待 --- forsterite が安定）
4. 25 GPa で $G_{\text{fo}} - G_{\text{mw}}$ を評価（負の期待 --- wadsleyite が安定）
5. 符号変化を検出し、最大50回の二分法で $|\Delta G| < 0.01$ kJ/mol に収束
6. 収束した境界圧力で `ComputeClapeyronSlope` を呼び出す

**パラメータ値**:

| パラメータ | Forsterite | Mg-Wadsleyite |
|-----------|-----------|---------------|
| $V_0$ | 43.6 cm$^3$/mol | 40.52 cm$^3$/mol |
| $K_0$ | 128.0 GPa | 169.0 GPa（不確定） |
| $F_0$ | $-2055.403$ kJ/mol | --- |

**予想される結果**:

| 物理量 | 予想値 |
|--------|--------|
| 境界圧力 | $\sim 13$-$14$ GPa（$\sim 410$ km 深度） |
| 二分法ステップ数 | $\sim 15$-$20$ |
| Clapeyron 傾斜 | $\sim 0.003$-$0.004$ GPa/K $= 3$-$4$ MPa/K（正） |
| 実験値（Katsura et al., 2004） | $\sim 3.6$ MPa/K |

**物理的解釈**: 計算された $\sim 13$-$14$ GPa の境界は地震学的に観測される 410 km 不連続面に一致する。正の Clapeyron 傾斜は、冷たい沈み込むスラブ（平均より $\sim 200$ K 低温）では転移が $\sim 0.6$-$0.8$ GPa 浅く生じ、不連続面が $\sim 20$ km 上昇することを意味する。これは沈み込み帯下での不連続面挙上の地震学的観測と整合する。

### 13.5.2 計算例2: ポストスピネル反応（660 km 不連続面）

**問題**: $T = 1900$ K における ringwoodite $\rightarrow$ bridgmanite + ferropericlase の反応境界を `FindMultiPhaseBoundary` で求め、Clapeyron 傾斜を決定せよ。

**解法**:

1. 反応を設定：反応物 = ringwoodite（Mg$_2$SiO$_4$, 化学量論 = 1）、生成物 = bridgmanite（MgSiO$_3$, 化学量論 = 1）+ periclase（MgO, 化学量論 = 1）
2. $T = 1900$ K、$P = 18$-$28$ GPa で `FindMultiPhaseBoundary` を呼び出す
3. `ComputeMultiPhaseGDiff` が各二分法ステップで $[1 \cdot G_{\text{bm}} + 1 \cdot G_{\text{pe}}] - [1 \cdot G_{\text{ri}}]$ を評価

**パラメータ値**:

| 相 | 構造 | 化学量論 |
|----|------|---------|
| Ringwoodite (ri) | Mg$_2$SiO$_4$ スピネル構造 | 反応物、$\nu = 1$ |
| Bridgmanite (bm) | MgSiO$_3$ ペロブスカイト構造 | 生成物、$\nu = 1$ |
| Periclase (pe) | MgO | 生成物、$\nu = 1$ |

**予想される結果**:

| 物理量 | 予想値 |
|--------|--------|
| 境界圧力 | $\sim 23$-$24$ GPa（$\sim 660$ km 深度） |
| Clapeyron 傾斜 | $\sim -0.002$ 〜 $-0.003$ GPa/K $= -2$ 〜 $-3$ MPa/K（負、不確定） |
| 実験値（Ito and Takahashi, 1989） | $\sim -2.8$ MPa/K |

**物理的解釈**: 660 km 不連続面での負の Clapeyron 傾斜は地球力学的に重大な意味を持つ。冷たい沈み込むスラブは境界をより高い圧力（深い深度）で経験し、浮力を与える相変態が遅延する。これがスラブの下部マントルへの貫入に対する抵抗を生み、多くの沈み込み帯下で地震トモグラフィーに観測されるスラブ停滞に寄与する。

### 13.5.3 計算例3: ポストペロブスカイト転移（D'' 層）

**問題**: $T = 2500$ K で bridgmanite-ポストペロブスカイト境界を `PostPerovskiteCalculator` で求め、転移にわたる弾性特性コントラストを比較せよ。

**解法**:

1. SLB2011 から bridgmanite と post-perovskite の `MineralParams` を用いて `PostPerovskiteCalculator` を生成
2. `FindBoundary` で $T = 2500$ K、$P = 100$-$140$ GPa の境界を探索
3. `CompareAcrossTransition` で境界の $(P, T)$ における速度・密度コントラストを計算
4. 両相の `MieGruneisenEOSOptimizer` を実行しパーセント差を算出

**予想される結果**:

| 物理量 | 予想値 | 備考 |
|--------|--------|------|
| 境界圧力 | $\sim 120$-$130$ GPa | 不確定 |
| $\Delta V_S$ | $1$-$3\%$（ppv のせん断速度が高い） | |
| $\Delta V_P$ | $\sim 0.5$-$1\%$ | 不確定 |
| $\Delta\rho$ | $\sim 1$-$2\%$ | 不確定 |
| Clapeyron 傾斜 | $\sim 8$-$10$ MPa/K（正、急峻） | 不確定 |

**物理的解釈**: pv-ppv 転移にわたる大きなせん断速度コントラストは、$\sim 2700$ km 深度で地震学的に観測される D'' 不連続面と整合する。急峻な正の Clapeyron 傾斜は、核-マントル境界付近の $\sim 1000$ K の横方向温度範囲内で境界圧力が $8$-$10$ GPa シフトしうることを意味し、強い熱勾配の領域で境界のダブルクロッシングを許容する可能性がある。

---

## 13.6 計算手法

### 13.6.1 二分法に基づく相境界探索

`FindPhaseBoundary` と `FindMultiPhaseBoundary` は固定温度での $\Delta G$ のゼロ交差を二分法で探索する：

**アルゴリズム**:

```
1. 端点で ΔG を評価: ΔG(P_min), ΔG(P_max)
2. if 同符号 → NaN を返す（範囲内に境界なし）
3. for i = 1 to 50:
     P_mid = (P_min + P_max) / 2
     ΔG_mid = ΔG(P_mid)
     if |ΔG_mid| < 0.01 kJ/mol → P_mid を返す
     if ΔG_mid と ΔG(P_min) が同符号:
       P_min = P_mid
     else:
       P_max = P_mid
4. P_mid を返す
```

多相バリアント: $\Delta G = \sum(\nu_{\text{prod}} \cdot G_{\text{prod}}) - \sum(\nu_{\text{react}} \cdot G_{\text{react}})$

`FindBoundaryTemperature` は固定圧力での温度軸に沿った類似探索を提供し、デフォルト許容誤差 $0.5$ K を持つ。

### 13.6.2 境界トレース

`TracePhaseBoundary` は $T_{\min}$ から $T_{\max}$ まで等間隔の温度で `FindPhaseBoundary` を呼び出し、$(P, T)$ 境界点のリストを生成する：

$$\Delta T = \frac{T_{\max} - T_{\min}}{n_{\text{points}} - 1}$$

これは単純なスイープ方式であり、予測子-修正子法や ODE ベースのトレース法ではない。そのため精度はステップ幅に対する境界の曲率に依存する。$n_{\text{points}} \geq 2$ が必要。

### 13.6.3 グリッドベースの相図

`CalculateDiagram` は $P$-$T$ グリッドの各点で完全な `GibbsMinimizer.Minimize` を評価し、`PhaseAssemblage` オブジェクトの2次元配列を返す。

**2相純端成分の場合**: `MinimizeTwoPhase` が単純に $G$ 値を比較。

**多相・固溶体系の場合**: `MinimizeGeneral` が反復的射影勾配降下法を使用：

1. 化学ポテンシャル $\mu_i = G_i$（+ 固溶体の過剰 Gibbs と配置エントロピー）を計算
2. 平均を減算して方向ベクトルを形成
3. 直線探索で相量を更新（ステップ幅 $= \min(0.1, 0.9 \times \text{最大許容ステップ})$）
4. 量 $< \epsilon = 10^{-10}$ の相を除去
5. $|\sum(\text{方向} \times \mu)| < 10^{-8}$ で収束、最大200反復

### 13.6.4 Clapeyron 傾斜の計算

`ComputeClapeyronSlope` は境界 $(P, T)$ で両相の `MieGruneisenEOSOptimizer` を独立に実行し、エントロピー [J/(mol$\cdot$K)] と体積 [cm$^3$/mol] を抽出して以下を計算する：

$$\frac{dP}{dT} = \frac{S_2 - S_1}{V_2 - V_1} \times 0.001 \text{ [GPa/K]}$$

$0.001$ の因子は J/(cm$^3$) から GPa への変換。逆形式は `GibbsMinimizer.ClapeyronSlope` で提供：

$$\frac{dT}{dP} = \frac{V_2 - V_1}{S_2 - S_1} \times 1000 \text{ [K/GPa]}$$

### 13.6.5 PostPerovskiteCalculator

裸の `MineralParams` から `PhaseEntry` オブジェクトを生成し、`PhaseDiagramCalculator` メソッドに委譲する専門的なラッパー。追加機能として両相の完全 EOS を実行しパーセント差を取ることで物性コントラスト（$\Delta V_S\%$, $\Delta V_P\%$, $\Delta\rho\%$）を計算する。

### 13.6.6 エラー処理と性能

**エラー処理**: `ComputePhaseGibbs` は極端な条件で計算できない相に対して try/catch で `double.MaxValue` を返す。

**性能特性**:

| 操作 | 所要 $G$ 評価回数 |
|------|-----------------|
| 1回の `FindPhaseBoundary` | $\sim 30$（2相 $\times$ $\sim 15$ 二分法ステップ） |
| `TracePhaseBoundary`（50点） | $\sim 1500$ |
| `CalculateDiagram`（$50 \times 50$、5候補相） | $12500$ |
| 1回の $G$ 評価 | 1回の EOS 解（$\sim 10$-$30$ Newton 反復） |

---

## 13.7 コード対応

本章で解説した概念は、ThermoElasticCalculator の以下のクラスおよびメソッドに対応する。

| 概念 | クラス / メソッド |
|------|------------------|
| 二相境界探索 | `PhaseDiagramCalculator.FindPhaseBoundary` |
| 多相反応境界 | `PhaseDiagramCalculator.FindMultiPhaseBoundary` |
| 境界トレース | `PhaseDiagramCalculator.TracePhaseBoundary` |
| グリッド相図 | `PhaseDiagramCalculator.CalculateDiagram` |
| Clapeyron 傾斜（$dP/dT$） | `PhaseDiagramCalculator.ComputeClapeyronSlope` |
| Clapeyron 傾斜（$dT/dP$） | `GibbsMinimizer.ClapeyronSlope` |
| 多相 Gibbs 差 | `PhaseDiagramCalculator.ComputeMultiPhaseGDiff` |
| Gibbs 最小化 | `GibbsMinimizer.Minimize` |
| 二相比較 | `GibbsMinimizer.MinimizeTwoPhase` |
| 一般最小化 | `GibbsMinimizer.MinimizeGeneral` |
| 個別相の Gibbs | `GibbsMinimizer.ComputePhaseGibbs` |
| ポストペロブスカイト | `PostPerovskiteCalculator.FindBoundary` |
| 物性コントラスト | `PostPerovskiteCalculator.CompareAcrossTransition` |
| 温度境界探索 | `PhaseDiagramCalculator.FindBoundaryTemperature` |
| UI インターフェース | `PhaseDiagramExplorerView` / `PhaseDiagramExplorerViewModel` |

**ワークフロー（階層的）**:

1. **最下層**: `MieGruneisenEOSOptimizer` が各 $(P, T)$ 点で体積を解く
2. **中間層**: `ThermoMineralParams` が $F$ と $G$ を計算
3. **比較層**: `GibbsMinimizer` が候補相にわたって $G$ を比較
4. **最上層**: `PhaseDiagramCalculator` が二分法探索またはグリッドスイープを統括

---

## 13.8 歴史

### 13.8.1 CALPHAD 法の確立

熱力学的第一原理からの相図の体系的計算は、冶金学と岩石学において長い歴史を持つ。CALPHAD（CALculation of PHAse Diagrams）法は1970年代に Kaufman と Bernstein によって開発され、実験データにフィットした Gibbs エネルギーモデルから相平衡を計算するパラダイムを確立した。

### 13.8.2 Ringwood と高圧実験

鉱物物理学では、Ted Ringwood による1960年代-1970年代の先駆的高圧実験が初めてマントル鉱物の基本的相関係を確立した。Olivine-spinel（現在の olivine-wadsleyite）転移とスピネル-ペロブスカイト+酸化物転移が、410 km および 660 km の地震波不連続面に対応することが同定された。

### 13.8.3 Clapeyron 傾斜の地球力学的重要性

Clapeyron 傾斜の概念は Christensen and Yuen（1985）の研究を通じて特に地球力学的重要性を獲得した。彼らは数値対流モデリングにより、660 km での十分に負の Clapeyron 傾斜が層状マントル対流を誘起しうることを実証した。この研究は Clapeyron 傾斜を純粋に熱力学的な量から惑星規模のダイナミクスを制御する鍵パラメータへと変容させた。

全マントル対流 vs 層状対流の論争は、660 km Clapeyron 傾斜の大きさに部分的に支配され、1990年代から2000年代にかけて続いた。地震トモグラフィーは最終的に、間欠的な停滞を伴う主として全マントル流を支持した。

### 13.8.4 ポストペロブスカイト相転移の発見

Murakami et al.（2004）によるポストペロブスカイト相転移の発見は画期的な出来事であり、長年謎であった D'' 地震波不連続面の鉱物物理学的説明を提供した。急峻な正の Clapeyron 傾斜（$\sim 8$-$10$ MPa/K）と大きなせん断速度コントラストにより、深部マントルの地震学的観測の解釈に即座に関連するものとなった。

### 13.8.5 自己無撞着な計算アプローチ

計算的に、単一の熱力学定式化から相図を自己無撞着に計算するアプローチ（境界を個別にフィッティングするのではなく）は Ita and Stixrude（1992）によって前進し、SLB2011 論文で完全に成熟した。ThermoElasticCalculator の `PhaseDiagramCalculator` はこのアプローチを、教育および研究応用にアクセス可能な実用的二分法ベースの境界探索とグリッドベースの安定性マッピングで実装している。

---

## 13.9 補足: 主要マントル相転移の定量的まとめ

マントルの主要な相転移について、計算結果と実験的制約を以下にまとめる。

### 表 13.1: 主要マントル相転移の特性

| 転移 | 深度 | $P$ [GPa] | 反応 | Clapeyron 傾斜 [MPa/K] | 符号の意味 |
|------|------|-----------|------|------------------------|-----------|
| 410 km | $\sim 410$ km | $\sim 13$-$14$ | fo $\rightarrow$ mw | $+3$ 〜 $+4$ | 対流促進 |
| 520 km | $\sim 520$ km | $\sim 17$-$18$ | mw $\rightarrow$ ri | $+4$ 〜 $+6$ | 対流促進 |
| 660 km | $\sim 660$ km | $\sim 23$-$24$ | ri $\rightarrow$ bm + pe | $-2$ 〜 $-3$ | 浮力障壁 |
| D'' | $\sim 2700$ km | $\sim 120$-$130$ | pv $\rightarrow$ ppv | $+8$ 〜 $+10$ | 強い温度依存 |

ここで fo = forsterite、mw = Mg-wadsleyite、ri = ringwoodite、bm = bridgmanite、pe = periclase（ferropericlase）、pv = bridgmanite（perovskite）、ppv = post-perovskite である。

### 符号の地球力学的影響の詳細

410 km 不連続面での正の Clapeyron 傾斜は、沈み込むスラブが冷たいため転移がより浅く生じ、より密な wadsleyite がスラブ内でより早く形成されて下向きの浮力を増大させることを意味する。これは遷移帯を越える物質移動を促進する。

一方、660 km での負の Clapeyron 傾斜は逆の効果を持つ。冷たいスラブでは ringwoodite の分解がより大きな深度で生じるため、密度増加が遅延し上向きの相対的浮力が生じる。この「浮力障壁」がスラブの下部マントルへの貫入を妨げる。ただし、十分に厚く高速で沈み込むスラブはこの障壁を突破でき、これが全マントル対流と層状対流の間の動的均衡を決定する。

D'' 付近のポストペロブスカイト転移は、その急峻な正の Clapeyron 傾斜により独特の地球物理学的シグネチャーを持つ。核-マントル境界付近の大きな横方向温度変動（$\sim 1000$ K）は、同一深度で bridgmanite とポストペロブスカイトの両方が存在する複雑なモザイク状の分布を生み出しうる。

---

## 13.10 未解決課題

1. **Fe の組成分配**: 共存相間（例：bridgmanite と ferropericlase 間の Fe 分配）の組成分配を相境界計算とどう結合すべきか。境界位置と鮮明さへの影響は何か。

2. **Newton-Raphson 法による改善**: `FindPhaseBoundary` の二分法を、Clapeyron 傾斜を局所微分として用いる Newton-Raphson 法で改善し、境界点あたりの EOS 評価回数を削減できるか。

3. **水（水素）の影響**: 410 km および 660 km 相境界に対する水の定量的効果は何か。修正された $F_0$ 値を通じて SLB 枠組みに組み込めるか。

4. **Clapeyron 傾斜の実験値との比較**: 計算された Clapeyron 傾斜は、特にポストスピネル転移（報告値が $-0.5$ 〜 $-3$ MPa/K の範囲）について、in-situ シンクロトロン X 線回折実験からの最新の実験決定とどの程度一致するか。

5. **適応的グリッド**: グリッドベースの `CalculateDiagram` を適応的にし、相境界付近では解像度を精緻化し安定領域内部では粗いグリッドを使用して、複雑な多成分系の効率を改善できるか。

6. **速度論的効果**: 冷たい沈み込むスラブにおける相の準安定的存続について、粒径依存の速度論的効果を平衡相図予測とどう並行して組み込むべきか。

---

## 13.10 演習問題

### 演習 13.1: 相境界の温度依存性

`FindPhaseBoundary` を用いて $T = 1000, 1400, 1600, 1800, 2200$ K における forsterite-wadsleyite 境界を求めよ。5つの $(P, T)$ 点をプロットし、線形フィットから Clapeyron 傾斜を計算せよ。中点温度での `ComputeClapeyronSlope` の結果と比較せよ。

### 演習 13.2: ポストスピネル反応境界

ポストスピネル反応の多相境界計算を実装せよ：反応物（ringwoodite, 化学量論 1）と生成物（bridgmanite + periclase, 化学量論各 1）を設定し、$T = 1600, 1800, 2000$ K で `FindMultiPhaseBoundary` を使用せよ。Clapeyron 傾斜を決定し、Ito and Takahashi（1989）の実験値（$-2.8$ MPa/K）と比較せよ。

### 演習 13.3: 境界トレースとグリッド法の比較

`TracePhaseBoundary` を用いて $1000$-$2500$ K で30点の fo-mw 境界をトレースせよ。$P$-$T$ 空間でプロットせよ。境界は直線か、Clapeyron 傾斜は温度とともに有意に変化するか。

### 演習 13.4: グリッド相図

`CalculateDiagram` で forsterite と wadsleyite を候補相として $P = 5$-$25$ GPa、$T = 1000$-$2500$ K の $40 \times 40$ グリッド上の相図を計算せよ。安定領域を可視化し、グリッドから相境界を同定せよ。`TracePhaseBoundary` との境界精度を比較せよ。

### 演習 13.5: ポストペロブスカイト転移の物性コントラスト

`PostPerovskiteCalculator.CompareAcrossTransition` を用いて $T = 2000, 2500, 3000$ K での $\Delta V_S$, $\Delta V_P$, $\Delta\rho$ を計算せよ。物性コントラストは温度とともにどう変化するか。D'' 不連続面の可視性への影響を議論せよ。

### 演習 13.6: $F_0$ の感度解析

Wadsleyite の $F_0$ を $\pm 5$ kJ/mol 摂動させて $T = 1600$ K での 410 km 境界を再計算せよ。境界は何 GPa（何 km）シフトするか。$F_0$ の実験的不確定性と比較せよ。

### 演習 13.7: Clapeyron 傾斜の2つの形式の整合性

$P = 125$ GPa、$T = 2500$ K での bridgmanite-ポストペロブスカイト転移について、`PhaseDiagramCalculator.ComputeClapeyronSlope`（$dP/dT$）と `GibbsMinimizer.ClapeyronSlope`（$dT/dP$）の両方を計算せよ。一方が他方の逆数であることを検証せよ。MPa/K に変換し実験報告値（$8$-$10$ MPa/K）と比較せよ。

### 演習 13.8: 地温勾配に沿った相転移深度

地温勾配 $T(z) = 1600 + 0.3z$（K、$z$ は km）に沿って、410 km と 660 km 転移の `FindPhaseBoundary` 結果を組み合わせるスクリプトを作成せよ。各転移はどの深度で生じるか。地温勾配が $200$ K 高温または低温になるとこれらの深度はどう変化するか。

---

## 13.11 図表

### 図 13.1: 410 km 相境界（Forsterite vs Wadsleyite）

**内容**: `TracePhaseBoundary` でトレースした forsterite-wadsleyite 相境界

- **横軸**: 温度 [K]（範囲: 1000-2500）
- **縦軸**: 圧力 [GPa]（範囲: 8-20）
- **主要な特徴**: 正の Clapeyron 傾斜を持つ境界曲線。低圧側を「forsterite 安定」、高圧側を「wadsleyite 安定」と表示。Clapeyron 傾斜を MPa/K で注記。典型的なマントル地温勾配を重ねて $\sim 13$-$14$ GPa（$\sim 410$ km）での交差を表示。地震学的に観測される不連続面深度範囲をマーク。

### 図 13.2: 660 km 多相反応境界

**内容**: ポストスピネル反応（ringwoodite $\rightarrow$ bridgmanite + ferropericlase）の境界

- **横軸**: 温度 [K]（範囲: 1500-2500）
- **縦軸**: 圧力 [GPa]（範囲: 20-28）
- **主要な特徴**: 負の Clapeyron 傾斜を持つ境界曲線。反応物側（ringwoodite, 上部マントル集合体）と生成物側（bridgmanite + ferropericlase, 下部マントル集合体）を表示。負の傾斜を注記。410 km の正の傾斜との対比を大縮尺の挿入図で表示。

### 図 13.3: Gibbs エネルギー差の圧力依存性

**内容**: 3温度（$T = 1200, 1600, 2000$ K）での fo-mw 転移の $\Delta G$ vs $P$

- **横軸**: 圧力 [GPa]（範囲: 5-25）
- **縦軸**: $\Delta G = G_{\text{fo}} - G_{\text{mw}}$ [kJ/mol]
- **主要な特徴**: 各温度でゼロを異なる圧力で交差する3本の曲線。各ゼロ交差点を相境界としてマーク。二分法の区間縮小の様子を図示。$\Delta G > 0$ は wadsleyite 安定、$\Delta G < 0$ は forsterite 安定を意味。

### 図 13.4: 完全 $P$-$T$ 相図

**内容**: `CalculateDiagram` で計算した MgO-SiO$_2$ 系の多重安定領域

- **横軸**: 温度 [K]（範囲: 1000-3000）
- **縦軸**: 圧力 [GPa]（範囲: 0-140）
- **主要な特徴**: olivine, wadsleyite, ringwoodite, bridgmanite + ferropericlase, post-perovskite + ferropericlase の色分けされた安定領域。$\sim 410$ km, $\sim 520$ km, $\sim 660$ km 深度および D'' 境界の相境界。表面から核-マントル境界までのマントル地温勾配を重畳。各安定領域と境界にラベル。

### 図 13.5: 主要マントル相転移の Clapeyron 傾斜比較

**内容**: 計算値 vs 実験値の比較

- **横軸**: 転移名（410, 520, 660, D''）
- **縦軸**: Clapeyron 傾斜 $dP/dT$ [MPa/K]
- **主要な特徴**: `ComputeClapeyronSlope` の結果と実験データを比較する棒グラフまたは点+誤差プロット。410 km と D'' で正の値、660 km で負の値。実験値に誤差棒を含む。層状対流の地球力学的有意閾値（$\sim -4$ MPa/K）を強調。

### 図 13.6: ポストペロブスカイト転移の物性コントラスト

**内容**: `CompareAcrossTransition` からの温度依存物性コントラスト

- **横軸**: 温度 [K]（範囲: 2000-3500）
- **縦軸**: 物性コントラスト [%]（$\Delta V_S$, $\Delta V_P$, $\Delta\rho$）
- **主要な特徴**: 転移圧力で $\Delta V_S\%$, $\Delta V_P\%$, $\Delta\rho\%$ が温度とともにどう変化するかを示す3本の曲線。$\Delta V_S$ が最大コントラスト（$\sim 2$-$3\%$）で、D'' 不連続面が主としてせん断波で可視であることを示す。D'' 反射振幅の地震学的観測と比較。

---

## 13.12 参考文献

1. **Stixrude, L. and Lithgow-Bertelloni, C.** (2011). "Thermodynamics of mantle minerals - II. Phase equilibria," *Geophysical Journal International*, 184(3), 1180-1213.
   - Gibbs 自由エネルギー最小化からの相図計算の完全な枠組みを定義。すべての端成分比較に必要な $F_0$ 参照エネルギーを含む。`PhaseDiagramCalculator` と `GibbsMinimizer` の実装がこの論文の方法論に直接基づく。

2. **Ito, E. and Takahashi, E.** (1989). "Postspinel transformations in the system Mg$_2$SiO$_4$-Fe$_2$SiO$_4$ and some geophysical implications," *Journal of Geophysical Research*, 94(B8), 10637-10646.
   - ポストスピネル（660 km）反応境界とその負の Clapeyron 傾斜に関する決定的な実験的制約を提供。測定傾斜 $\sim -2.8$ MPa/K が `FindMultiPhaseBoundary` からの計算予測の検証ベンチマーク。

3. **Katsura, T. et al.** (2004). "Olivine-wadsleyite transition in the system (Mg,Fe)$_2$SiO$_4$," *Journal of Geophysical Research*, 109(B2), B02209.
   - $(Mg, Fe)_2SiO_4$ 系における 410 km 不連続面境界と Clapeyron 傾斜（$\sim 3.6$ MPa/K）の高精度実験決定。`FindPhaseBoundary` を fo-mw 転移に適用した際の主要な検証対象。

4. **Murakami, M., Hirose, K., Kawamura, K., Sata, N., and Ohishi, Y.** (2004). "Post-perovskite phase transition in MgSiO$_3$," *Science*, 304(5672), 855-858.
   - ポストペロブスカイト相転移の発見。`PostPerovskiteCalculator` がモデル化するために設計された転移。D'' 不連続面を説明する転移圧力（$\sim 125$ GPa）と正の Clapeyron 傾斜を確立。

5. **Bina, C. R. and Helffrich, G.** (1994). "Phase transition Clapeyron slopes and transition zone seismic discontinuity topography," *Journal of Geophysical Research*, 99(B8), 15853-15860.
   - Clapeyron 傾斜が地震学で観測可能な不連続面地形にどう変換されるかを定量的に実証。`ComputeClapeyronSlope` の結果を不連続面深度の横方向変動として解釈するための枠組み。

6. **Christensen, U. R. and Yuen, D. A.** (1985). "Layered convection induced by phase transitions," *Journal of Geophysical Research*, 90(B12), 10291-10300.
   - 660 km での負の Clapeyron 傾斜が数値モデルで層状マントル対流を誘起しうることを実証した画期的論文。`ComputeClapeyronSlope` で計算される熱力学量と大規模マントルダイナミクスの直接的関連を確立。

---

## 13.13 他章関連

### 第12章: Gibbs 自由エネルギーと相安定性（前提）

個々の相の $G = F + PV$ の計算方法と、最低 $G$ の相が安定であるという原理を確立。本章はこれに基づいて $P$-$T$ 空間にわたる $G_1 = G_2$ の系統的トレースと Clapeyron 傾斜の計算に発展する。

### 第6章: Mie-Gruneisen 状態方程式ソルバー（前提）

`FindPhaseBoundary` でのすべての Gibbs エネルギー評価が `MieGruneisenEOSOptimizer` の実行を必要とする。EOS ソルバーは完全な相図グリッドまたは境界トレースの計算時に数千回呼び出される。

### 第7章: SLB2011 鉱物データベース（前提）

相間の $G$ 比較に必要な $F_0$ 参照エネルギーとすべての熱弾性パラメータを提供する。`PhaseDiagramExplorerViewModel` は `SLB2011Endmembers.GetAll()` から直接鉱物を読み込む。正確な $F_0$ 値なしでは相境界位置は無意味。

### 第10章: 弾性混合モデル（発展）

相図から各 $(P, T)$ での安定な集合体が決定されると、弾性混合モデルが多相集合体のバルク物性（速度、密度）を計算する。これにより相平衡と地震学的観測量が結びつく。

### 第11章: 岩石組成と集合体（応用）

相図は所与のバルク組成の岩石が所与の深度でどの鉱物が安定かを決定する。第11章の岩石集合体が入力組成であり、相図枠組みがこれらの集合体が地温勾配に沿ってどう変化するかを決定する。

### 第4章: 弾性率と地震波速度（発展）

相境界は地震波速度に不連続面を生じる。`PostPerovskiteCalculator.CompareAcrossTransition` で計算される速度コントラストは、相図予測を反射や波動インピーダンスコントラストの地震学的観測と直接結びつける。

### 第8章: Landau 相転移（前提）

変位型転移を受ける鉱物（例：stishovite から CaCl$_2$-SiO$_2$ への転移）の Gibbs エネルギーを修正する Landau 自由エネルギー寄与。これらの寄与が相境界位置をシフトさせるため、正確な相図に含める必要がある。

### 第5章: Stixrude-Lithgow-Bertelloni 定式化（前提）

$G$ 計算の基盤となるすべてのエネルギー寄与を含む $F(V, T)$ の完全な定式化を提供。相図の精度は $F$ の各成分の正確さに直接依存する。
