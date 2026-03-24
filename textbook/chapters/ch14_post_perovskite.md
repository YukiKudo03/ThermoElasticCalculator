# 第14章 ポストペロブスカイト相転移

## 14.1 概要

ポストペロブスカイト（post-perovskite, pPv）相転移は、MgSiO$_3$ ブリッジマナイト（ペロブスカイト構造, Pv）が CaIrO$_3$ 型のポストペロブスカイト構造へと変態する一次構造相転移である。この転移は地球のマントル最下部——D'' 層——に対応する約 120--135 GPa、2500--3000 K の条件下で生じ、コア-マントル境界（CMB）から約 200--300 km 上方に位置する地震学的不連続面を鉱物物理学的に説明する画期的な発見であった。

この転移は 2004 年に Murakami et al. と Oganov & Ono の二つのグループによって独立に発見された。Murakami らはレーザー加熱ダイヤモンドアンビルセル（LHDAC）実験と放射光 X 線回折を組み合わせ、125 GPa を超える圧力下で MgSiO$_3$ の新しい結晶構造を同定した。同時期に Oganov と Ono は第一原理結晶構造探索と実験的検証により同じ転移を報告した。

構造的には、ペロブスカイトの角共有 SiO$_6$ 八面体の三次元骨格が、ポストペロブスカイトでは辺共有八面体シートからなる層状構造へと再編成される。この構造変化に伴い、密度、弾性率、地震波速度に測定可能な変化が生じる。特に、せん断波速度 $V_s$ のコントラストが縦波速度 $V_p$ のコントラストよりも大きいことが特徴的であり、D'' 不連続面における地震学的観測と整合する。

相境界は正のクラペイロン勾配（約 +8 ~ +10 MPa/K）を持つ。これは温度が高いほど転移圧力が高くなることを意味し、D'' 層の構造に深遠な帰結をもたらす。CMB 温度が水平方向に変動する領域では pPv 転移の深さも変動し、さらに CMB 直上の熱境界層における急峻な温度勾配との組み合わせにより、**ダブルクロッシング仮説**（double-crossing hypothesis）が成立する可能性がある。これは pPv が Pv に再び逆転移する現象であり、対をなす地震学的反射面を生成する。

計算科学的観点からは、pPv 相境界の検出はペロブスカイトとポストペロブスカイトの Gibbs 自由エネルギーの比較に帰着する。境界は $G_{\text{Pv}}(P, T) = G_{\text{pPv}}(P, T)$ を満たす圧力として定義され、二分法（bisection method）により数値的に求解される。本コードベースの `PostPerovskiteCalculator` クラスは SLB2011 鉱物データベースのパラメータを用いてこれらの計算を実装している。

---

## 14.2 前提知識

本章を十分に理解するためには、以下の知識が必要である。

1. **Gibbs 自由エネルギー最小化と相平衡**（第12章）: 相安定性は Gibbs 自由エネルギーの比較により決定される。$G(P, T) = H - TS = U + PV - TS$ の定義と、平衡条件としての $G$ 最小化の原理を理解していること。

2. **Mie-Gruneisen 状態方程式とその数値解法**（第6章）: `MieGruneisenEOSOptimizer` による高圧高温条件下の熱力学量（エントロピー $S$、体積 $V$、密度 $\rho$、弾性率）の計算手法。Pv と pPv の両方について、この解法が適用される。

3. **弾性率と地震波速度の高圧高温計算**（第4章）: 体積弾性率 $K_S$ とせん断弾性率 $G$ から地震波速度を計算する方法：

$$V_p = \sqrt{\frac{K_S + \frac{4}{3}G}{\rho}}, \qquad V_s = \sqrt{\frac{G}{\rho}}$$

4. **SLB2011 鉱物データベースのパラメータ**（第7章）: ペロブスカイトおよびポストペロブスカイトの端成分（mpv, fpv, apv, mppv, fppv, appv）のパラメータ体系。各端成分の参照体積 $V_0$、体積弾性率 $K_0$、その圧力微分 $K_0'$、Debye 温度 $\Theta_0$、Gruneisen パラメータ $\gamma_0$ 等。

5. **Clausius-Clapeyron の関係式**: 一次相転移の相境界における圧力と温度の関係を規定する基本的な熱力学的関係式。化学ポテンシャルの等価条件から導出される。

6. **結晶構造の基礎**: ペロブスカイト構造（空間群 $Pbnm$）と CaIrO$_3$ 型層状構造（空間群 $Cmcm$）の違い。角共有と辺共有の違い、配位数の変化。

7. **地震学の基礎**: D'' 層の定義、コア-マントル境界の性質、地震学的不連続面の検出方法（反射波、変換波の解析）。

8. **数値解法の基礎**: 二分法（bisection method）の原理と収束性。区間縮小法による非線形方程式の根の探索。Newton-Raphson 法の基本概念も参照（第6章で EOS 求解に使用）。

---

## 14.3 理論と方程式

### 14.3.1 相境界条件：Gibbs 自由エネルギーの等価

一次相転移の相境界は、二相の Gibbs 自由エネルギーが等しくなる $P$-$T$ 条件として定義される：

$$G_{\text{Pv}}(P, T) = G_{\text{pPv}}(P, T)$$

ここで各相の Gibbs 自由エネルギーは Mie-Gruneisen-Debye 状態方程式から計算される。具体的には、Helmholtz 自由エネルギー $F(V, T)$ を体積について最小化して平衡体積 $V_{\text{eq}}(P, T)$ を求め、Legendre 変換により Gibbs エネルギーを得る：

$$G(P, T) = F(V_{\text{eq}}, T) + P V_{\text{eq}}$$

相境界を検出するためには、Gibbs エネルギー差

$$\Delta G(P, T) = G_{\text{Pv}}(P, T) - G_{\text{pPv}}(P, T)$$

を圧力の関数として評価し、$\Delta G = 0$ となる圧力を求める。低圧側では $\Delta G < 0$（Pv が安定）、高圧側では $\Delta G > 0$（pPv が安定）であり、その間に必ず零点が存在する。

### 14.3.2 Clausius-Clapeyron の関係式

相境界の $P$-$T$ 空間における勾配は Clausius-Clapeyron の関係式で記述される：

$$\frac{dP}{dT} = \frac{\Delta S}{\Delta V}$$

ここで

- $\Delta S = S_{\text{pPv}} - S_{\text{Pv}}$：転移に伴うエントロピー変化（J/mol/K）
- $\Delta V = V_{\text{pPv}} - V_{\text{Pv}}$：転移に伴う体積変化（cm$^3$/mol）

である。この式は Gibbs-Duhem の関係 $dG = VdP - SdT$ から直接導かれる。相境界上では $dG_{\text{Pv}} = dG_{\text{pPv}}$ であるから：

$$V_{\text{Pv}} dP - S_{\text{Pv}} dT = V_{\text{pPv}} dP - S_{\text{pPv}} dT$$

整理すると：

$$(V_{\text{pPv}} - V_{\text{Pv}}) dP = (S_{\text{pPv}} - S_{\text{Pv}}) dT$$

$$\frac{dP}{dT} = \frac{S_{\text{pPv}} - S_{\text{Pv}}}{V_{\text{pPv}} - V_{\text{Pv}}} = \frac{\Delta S}{\Delta V}$$

pPv 転移では $\Delta S < 0$（pPv は Pv よりも低エントロピー）かつ $\Delta V < 0$（pPv は Pv よりも小体積）であるため、$dP/dT > 0$（正のクラペイロン勾配）となる。実験的・計算的推定値は $dP/dT \approx +8$ ~ $+10$ MPa/K の範囲にある。

### 14.3.3 Gibbs 自由エネルギーの構成要素

Mie-Gruneisen-Debye モデルにおける Gibbs 自由エネルギーの構成を明示的に記述する。各相の Helmholtz 自由エネルギーは以下の三つの寄与からなる：

$$F(V, T) = F_{\text{ref}}(V) + F_{\text{th}}(V, T) - F_{\text{th}}(V, T_{\text{ref}})$$

ここで $F_{\text{ref}}(V)$ は参照温度 $T_{\text{ref}} = 300$ K における冷圧縮エネルギー（Birch-Murnaghan EOS）、$F_{\text{th}}(V, T)$ は Debye モデルによる熱的寄与である。

冷圧縮項は第3次 Birch-Murnaghan EOS に基づき：

$$F_{\text{ref}}(V) = \frac{9 V_0 K_0}{2} \left[ \frac{1}{2}(f^2) + \frac{1}{3}(K_0' - 4) f^3 \right]$$

ここで $f = \frac{1}{2}\left[(V_0/V)^{2/3} - 1\right]$ は Eulerian 有限歪みである。

Debye モデルによる熱的寄与は：

$$F_{\text{th}}(V, T) = n k_B T \left[ 3 \ln\left(1 - e^{-\Theta_D/T}\right) - D(\Theta_D/T) \right]$$

ここで $\Theta_D(V)$ は体積依存の Debye 温度、$D(x)$ は Debye 関数、$n$ は式単位あたりの原子数である。

平衡体積 $V_{\text{eq}}$ は以下の条件から決定される：

$$P = -\left(\frac{\partial F}{\partial V}\right)_T$$

そして Gibbs エネルギーは：

$$G(P, T) = F(V_{\text{eq}}, T) + P V_{\text{eq}}$$

Pv と pPv では参照パラメータ（$V_0$, $K_0$, $K_0'$, $\Theta_0$, $\gamma_0$, $q_0$, $\eta_{s0}$）が異なるため、同じ $P$-$T$ 条件でも異なる $G$ 値を与える。相境界はこれらの差が零になる点である。

### 14.3.4 エントロピーと体積の計算

クラペイロン勾配の計算に必要な $\Delta S$ と $\Delta V$ は、それぞれの相について EOS を解いた後に得られる。

エントロピーは Helmholtz 自由エネルギーの温度微分から得られる：

$$S(V, T) = -\left(\frac{\partial F}{\partial T}\right)_V = n k_B \left[ 4 D(\Theta_D/T) - 3 \ln\left(1 - e^{-\Theta_D/T}\right) \right]$$

体積は上述の圧力条件 $P = -(\partial F/\partial V)_T$ を Newton-Raphson 法で解いて得る。

転移に伴う変化量は：

$$\Delta S = S_{\text{pPv}}(V_{\text{eq}}^{\text{pPv}}, T) - S_{\text{Pv}}(V_{\text{eq}}^{\text{Pv}}, T)$$

$$\Delta V = V_{\text{eq}}^{\text{pPv}} - V_{\text{eq}}^{\text{Pv}}$$

いずれも同一の $P$, $T$ 条件（相境界上）で評価される。

### 14.3.5 地震波速度コントラスト

転移前後の地震波速度のコントラストは、D'' 不連続面の地震学的シグネチャを鉱物物理学的に解釈するための鍵となる量である。せん断波速度のコントラストは以下で定義される：

$$\delta V_s = \frac{V_s^{\text{pPv}} - V_s^{\text{Pv}}}{V_s^{\text{Pv}}} \times 100\%$$

同様に、縦波速度のコントラストは：

$$\delta V_p = \frac{V_p^{\text{pPv}} - V_p^{\text{Pv}}}{V_p^{\text{Pv}}} \times 100\%$$

密度コントラストは：

$$\delta \rho = \frac{\rho_{\text{pPv}} - \rho_{\text{Pv}}}{\rho_{\text{Pv}}} \times 100\%$$

pPv は Pv に比べて密度が約 1--2% 増加する。これは辺共有構造がより密な充填を可能にするためである。$\delta V_s$ は一般に $\delta V_p$ よりも大きく、これは地震学的に D'' 不連続面が S 波の研究でより顕著に検出されるという観測事実と整合する。

この非対称性の物理的根拠は、弾性率の変化パターンにある。pPv 転移に伴うせん断弾性率 $G$ の変化は、体積弾性率 $K_S$ の変化よりも相対的に大きい。$V_s = \sqrt{G/\rho}$ はせん断弾性率のみに依存するのに対し、$V_p = \sqrt{(K_S + 4G/3)/\rho}$ は両方に依存するため、$G$ の変化が $V_s$ に直接反映される一方、$V_p$ への寄与は $K_S$ によって部分的に相殺される。

### 14.3.6 ダブルクロッシングの条件

ダブルクロッシングが生じるか否かは、地温勾配と pPv 相境界の相対的な位置関係に依存する。マントル最下部の地温勾配を $T(P)$ と表すとき、以下の条件が満たされればダブルクロッシングが生じる：

1. ある圧力 $P_1$ で地温勾配が pPv 安定領域に入る（第一の交差、Pv → pPv）
2. CMB 圧力 $P_{\text{CMB}} \approx 136$ GPa に達する前に、地温勾配が再び Pv 安定領域に戻る（第二の交差、pPv → Pv）

数学的には、第二の交差が存在する条件は：

$$P_{\text{boundary}}(T_{\text{CMB}}) > P_{\text{CMB}}$$

すなわち、CMB 温度 $T_{\text{CMB}}$ における pPv 転移圧力が CMB の実際の圧力を超えることである。正のクラペイロン勾配 $dP/dT \approx +9$ MPa/K の場合、CMB 温度が十分に高ければ（例えば $T_{\text{CMB}} > 3500$ K 程度）、この条件が満たされ得る。

---

## 14.4 物理概念

### 14.4.1 ポストペロブスカイトの結晶構造

ポストペロブスカイトは CaIrO$_3$ 型の層状結晶構造を持ち、空間群は $Cmcm$ である。SiO$_6$ 八面体が辺を共有してシートを形成し、これらのシートの間に Mg が配位する。ペロブスカイト（$Pbnm$）の角共有八面体の三次元ネットワークとは根本的に異なる結晶構造である。

**物理的直感**: 極端な圧力下では、より密な充填を許容する構造が熱力学的に安定になる。辺共有はより短い Si--Si 距離を可能にし、体積の減少と密度の増加に寄与する。ただし、辺共有は通常 Si--Si 間の静電反発を増大させるため、低圧では不安定であり、高圧でのみ安定化する。

**よくある誤解**: ポストペロブスカイトはペロブスカイトの「微小な歪み」ではない。対称性（$Cmcm$ vs $Pbnm$）も結合トポロジーも根本的に異なり、再構成型の一次相転移を経て形成される。

### 14.4.2 クラペイロン勾配の符号とその意味

クラペイロン勾配 $dP/dT = \Delta S / \Delta V$ は、相境界の $P$-$T$ 空間における傾きを決定する。pPv 転移では $\Delta S < 0$ かつ $\Delta V < 0$ であり、正の勾配を与える。

**物理的直感**: 温度が高い領域では、エントロピーの寄与が Pv をより安定にするため（Pv の方が高エントロピー）、pPv を安定化するにはより高い圧力（より大きな深さ）が必要になる。したがって、CMB 温度が水平方向に変動すると、pPv 転移の深さも系統的に変動する。冷たいスラブの下降流（地震波速度が速い領域）では転移が浅く生じ、高温のプルーム（地震波速度が遅い領域）では転移が深く生じる。

**よくある誤解**: 正のクラペイロン勾配はマントル対流に対して常に安定化（対流を促進）するわけではない。660 km 不連続面の負の勾配が対流を妨げるのとは対照的であるが、pPv 転移が対流に与える影響は幾何学的配置に依存し、単純ではない。

### 14.4.3 ダブルクロッシング仮説

ダブルクロッシング仮説は Hernlund et al. (2005) によって提唱された。正のクラペイロン勾配と CMB 直上の急峻な熱境界層が組み合わさることで、地温勾配が pPv 安定領域を貫通し再び Pv 安定領域に戻るシナリオである。

**物理的直感**: マントル内部では断熱的な地温勾配（比較的緩やか）に沿って温度が増加するが、CMB 直上の熱境界層では温度が急激に上昇する。クラペイロン勾配が正であるため、相境界の圧力は温度とともに増加するが、地温勾配の増加の方が速い場合、地温勾配は pPv 安定領域の外に出る。結果として、pPv は二つの Pv 領域に挟まれた「レンズ」状の領域に限定され、二つの地震学的反射面を生成する。

**よくある誤解**: ダブルクロッシングはどこでも普遍的に生じるわけではない。その発生は局所的な CMB 熱流量、正確なクラペイロン勾配の値、鉄含有量に敏感に依存する。中程度の CMB 温度を持つ領域でのみ生じる可能性がある。

### 14.4.4 D'' 地震学的不連続面

D'' 不連続面は CMB の約 200--300 km 上方で観測される地震学的速度の不連続であり、特に $V_s$ の増加が顕著である。

**物理的直感**: D'' は 1949 年に Bullen によって同定されたが、その起源は半世紀以上にわたって謎であった。pPv 転移の発見により、Pv から pPv への構造変化に伴う急激な速度と密度のジャンプが、予測される深さで自然に D'' 不連続面を説明することが示された。

**よくある誤解**: D'' は単一の均質な全球的層ではない。その性質は温度、組成（鉄含有量、アルミニウム含有量）、pPv 転移の有無に応じて水平方向に変動する。D'' のすべての特徴が必ずしも pPv 転移に起因するわけではなく、化学的不均質や部分溶融（ULVZ）も寄与し得る。

### 14.4.5 一次相転移としての pPv 転移

pPv 転移は一次（first-order）相転移であり、転移点において体積とエントロピーが不連続に変化する。これは第8章で扱った Landau 理論の連続的（二次・三重臨界）相転移とは本質的に異なる。

一次相転移の特徴：
- 転移点で潜熱 $L = T \Delta S$ が放出される
- 体積の不連続変化 $\Delta V \neq 0$ が生じる
- 秩序パラメータが転移点で不連続にジャンプする
- 二相が共存する条件は Gibbs エネルギーの等価条件 $G_1 = G_2$ で決まる
- 過熱・過冷却（準安定状態）が可能

Landau 理論は秩序パラメータの連続的変化を記述するのに適しているが、pPv 転移のような再構成型の一次転移には直接適用できない。代わりに、各相の Gibbs エネルギーを独立に計算して比較するアプローチが必要であり、これが `PhaseDiagramCalculator` の基本的な設計思想である。

### 14.4.6 Gibbs エネルギー二分法による相境界検出

相境界を数値的に検出するために、圧力区間に対する二分法（bisection method）が用いられる。

**物理的直感**: 低圧では Pv が低い Gibbs エネルギーを持ち安定であり、高圧では pPv が安定である。境界は二相のエネルギーが等しい点に対応する。二分法は中点における $\Delta G$ の符号を評価して区間を半減させることを反復し、効率的に境界圧力に収束する。

**よくある誤解**: 二分法は探索区間内に単一の零点が存在することを仮定している。組成効果等により Gibbs エネルギー曲線が複数回交差する場合、追加の交差を見逃す可能性がある。探索区間 $[P_{\text{min}}, P_{\text{max}}]$ が境界を適切に挟んでいることの確認が必要である。

---

## 14.5 計算例

### 14.5.1 計算例 1: pPv 相境界圧力とクラペイロン勾配の決定

**問題**: SLB2011 データベースの MgSiO$_3$ 端成分（mpv, mppv）を用いて、$T = 2500$ K における Pv-pPv 相境界圧力を求め、クラペイロン勾配を計算せよ。

**アプローチ**: `PostPerovskiteCalculator.FindBoundary` に mpv と mppv の鉱物パラメータを入力し、100--140 GPa の圧力範囲で二分法による探索を実行する。各二分法ステップにおいて、`GibbsMinimizer.ComputePhaseGibbs` が Mie-Gruneisen-Debye EOS を解いて Gibbs エネルギーを評価する。収束後、`GetClapeyronSlope` により境界点における $\Delta S / \Delta V$ を計算する。

**計算の流れ**:

1. 初期区間: $P_{\text{min}} = 100$ GPa, $P_{\text{max}} = 140$ GPa
2. $\Delta G(100\text{ GPa}, 2500\text{ K})$ を評価 → 負（Pv 安定）
3. $\Delta G(140\text{ GPa}, 2500\text{ K})$ を評価 → 正（pPv 安定）
4. 符号が異なるため、零点が区間内に存在
5. 中点 $P = 120$ GPa で $\Delta G$ を評価し、符号に応じて区間を半減
6. $|\Delta G| < 0.01$ kJ/mol または 50 反復で収束

**数値的結果**（概算）:

- 境界圧力: $P_{\text{boundary}} \approx 120$--$130$ GPa（2500 K）
- クラペイロン勾配: $dP/dT \approx 0.008$--$0.010$ GPa/K（8--10 MPa/K）

**解釈**: 2500 K における境界圧力は CMB 圧力（約 136 GPa）の約 200--300 km 上方に対応する。これは D'' 不連続面の観測深度と整合する。正のクラペイロン勾配は、高温領域では転移がより深く、低温領域ではより浅く生じることを意味し、D'' のトポグラフィーが温度不均質を反映することを予測する。

### 14.5.2 計算例 2: 転移前後の地震波速度・密度コントラスト

**問題**: 125 GPa、2500 K における Pv-pPv 転移前後の $\delta V_s$、$\delta V_p$、$\delta \rho$ を計算せよ。

**アプローチ**: `PostPerovskiteCalculator.CompareAcrossTransition` を使用する。内部的には、mpv と mppv のそれぞれについて `MieGruneisenEOSOptimizer` を実行し、`ThermoMineralParams`（密度、$V_s$、$V_p$ 等）を取得する。百分率変化を計算して返す。

**数値的結果**（概算）:

| 物性量 | Pv (mpv) | pPv (mppv) | コントラスト |
|--------|----------|------------|-------------|
| $\rho$ (g/cm$^3$) | ～5.3 | ～5.4 | $\delta \rho \approx +1$--$2\%$ |
| $V_s$ (km/s) | ～7.0 | ～7.2 | $\delta V_s \approx +$ 数 $\%$ |
| $V_p$ (km/s) | ～13.5 | ～13.6 | $\delta V_p \approx +$ 小 $\%$ |

**解釈**: $\delta V_s > \delta V_p$ という結果は、D'' 不連続面が S 波の研究でより顕著に検出されるという地震学的観測と定性的に一致する。密度増加は pPv がより密な充填を持つ高圧相であることを反映し、pPv を含む集合体がマントル最下部で重力的に安定であることを示唆する。

### 14.5.3 計算例 3: ダブルクロッシングの判定

**問題**: CMB 温度 $T_{\text{CMB}} = 3800$ K に達する地温勾配に沿って、ダブルクロッシングが生じるか評価せよ。

**アプローチ**: `FindBoundary` を複数の温度（2500, 3000, 3500, 3800 K）で実行し、相境界曲線 $P_{\text{boundary}}(T)$ を得る。これをモデル地温勾配と比較する。

**モデル地温勾配**:
- CMB から 300 km 上方では断熱温度 $T_{\text{adiabat}} \approx 2500$ K
- CMB 直上の熱境界層（厚さ ～200 km）で $T$ が 2500 K から 3800 K まで急上昇
- 圧力は約 120 GPa（300 km 上方）から 136 GPa（CMB）まで変化

**数値的結果**（概算）:

| $T$ (K) | $P_{\text{boundary}}$ (GPa) |
|---------|---------------------------|
| 2500 | ～125 |
| 3000 | ～129--130 |
| 3500 | ～133--134 |
| 3800 | ～136--137 |

クラペイロン勾配 $dP/dT \approx +9$ MPa/K より、1000 K の温度上昇に対して境界圧力は約 9 GPa 上昇する。

**判定**: $T_{\text{CMB}} = 3800$ K における境界圧力が $\approx 136$--$137$ GPa であり、CMB 圧力（$\approx 136$ GPa）とほぼ同じか僅かに超える。したがって、地温勾配が pPv 安定領域を通過した後、CMB 付近で再び Pv 安定領域に戻る可能性がある。

**解釈**: ダブルクロッシングが生じる場合、マントル最下部に pPv のレンズ状領域が形成され、二つの地震学的反射面（上方の Pv→pPv 転移と下方の pPv→Pv 逆転移）を生成する。このレンズの厚さはクラペイロン勾配と CMB 温度に敏感に依存し、D'' 層の熱的条件を診断するプローブとなり得る。

---

## 14.6 計算手法

### 14.6.1 PostPerovskiteCalculator の全体構成

`PostPerovskiteCalculator` クラスは、pPv 転移に関する三つの主要な計算ワークフローを提供する。内部では `PhaseDiagramCalculator` に委譲する設計となっている。

**FindBoundary**: 指定温度での相境界圧力の決定

- 入力: Pv と pPv の `MineralParams`、温度 $T$、探索圧力範囲 $[P_{\text{min}}, P_{\text{max}}]$（デフォルト 100--140 GPa）
- 処理: `PhaseDiagramCalculator.FindPhaseBoundary` に委譲。内部で Gibbs エネルギー差 $\Delta G = G_{\text{Pv}} - G_{\text{pPv}}$ に対する二分法を実行
- 収束条件: $|\Delta G| < 0.01$ kJ/mol（デフォルト）または最大 50 反復
- 出力: 境界圧力（GPa）。零点が探索区間内に存在しない場合は `NaN`

**GetClapeyronSlope**: 相境界上のクラペイロン勾配の計算

- 入力: Pv と pPv の `MineralParams`、圧力 $P$、温度 $T$
- 処理: `PhaseDiagramCalculator.ComputeClapeyronSlope` に委譲。各相について `MieGruneisenEOSOptimizer` で EOS を解き、$\Delta S$ と $\Delta V$ を取得
- 単位変換: $dP/dT = \Delta S / \Delta V \times 0.001$ で J/(mol$\cdot$cm$^3$) から GPa/K へ変換
- 出力: クラペイロン勾配（GPa/K）

**CompareAcrossTransition**: 転移前後の物性比較

- 入力: Pv と pPv の `MineralParams`、圧力 $P$、温度 $T$
- 処理: 各相で `MieGruneisenEOSOptimizer` を実行し、`ThermoMineralParams` を取得
- 出力: 両相の物性パラメータ、$\delta V_s$（%）、$\delta V_p$（%）、$\delta \rho$（%）

### 14.6.2 二分法による相境界探索の詳細

`PhaseDiagramCalculator.FindPhaseBoundary` の二分法アルゴリズムを詳述する。

**アルゴリズム**:

```
入力: phase1 (Pv), phase2 (pPv), T, pMin, pMax, tolerance
1. gDiffLow = G_Pv(pMin, T) - G_pPv(pMin, T)
2. gDiffHigh = G_Pv(pMax, T) - G_pPv(pMax, T)
3. if gDiffLow * gDiffHigh > 0 → return NaN (区間内に零点なし)
4. for i = 0 to 49:
     pMid = (pMin + pMax) / 2
     gDiffMid = G_Pv(pMid, T) - G_pPv(pMid, T)
     if |gDiffMid| < tolerance → return pMid
     if gDiffLow * gDiffMid < 0:
       pMax = pMid
     else:
       pMin = pMid; gDiffLow = gDiffMid
5. return (pMin + pMax) / 2
```

**収束性**: 50 回の二分法反復で、初期区間幅 40 GPa（100--140 GPa）は $40 / 2^{50} \approx 3.6 \times 10^{-14}$ GPa まで縮小されるが、実際にはトレランス $|\Delta G| < 0.01$ kJ/mol で早期に収束する。典型的には 30--50 回の Gibbs エネルギー評価（1 反復あたり 2 回の EOS 解）で十分な精度が得られる。

**計算コスト**: 支配的なコストは Mie-Gruneisen EOS の求解であり、各 EOS 評価には Newton-Raphson 法による体積の最適化（通常 5--10 反復）が含まれる。FindBoundary 全体では約 30--50 回の EOS 評価が必要である一方、GetClapeyronSlope と CompareAcrossTransition は各 2 回のみである。

### 14.6.3 TracePhaseBoundary による相境界曲線の追跡

`PhaseDiagramCalculator.TracePhaseBoundary` は、温度範囲 $[T_{\text{min}}, T_{\text{max}}]$ にわたって相境界を追跡し、$(P, T)$ 点のリストを返す。

- 温度を $n$ 等分し、各温度で `FindPhaseBoundary` を呼び出す
- pPv 転移の場合、$P$ の探索範囲は $[P_{\text{min}}, P_{\text{max}}] = [100, 140]$ GPa を使用
- 出力: $(P_1, T_1), (P_2, T_2), \ldots, (P_n, T_n)$ のリスト

この機能を用いて、相境界を $P$-$T$ ダイアグラム上に描画し、地温勾配との交差を視覚的に評価できる。

### 14.6.4 FindBoundaryTemperature による逆問題

`FindBoundaryTemperature` は、指定圧力での転移温度を求める逆問題を解く。

- 入力: 二相のパラメータ、圧力 $P$、温度探索範囲 $[T_{\text{min}}, T_{\text{max}}]$
- 処理: 温度に関する二分法。収束トレランスのデフォルトは 0.5 K
- 応用: 「CMB 圧力 136 GPa での pPv 転移温度は？」という問いに直接答える

### 14.6.5 CalculateDiagram による全相図計算

`PhaseDiagramCalculator.CalculateDiagram` は $P$-$T$ グリッド上で全相図を計算する。

- 各グリッド点で `GibbsMinimizer.Minimize` を呼び出し、候補相の中から最安定な相（集合体）を決定
- pPv 転移の場合、候補相として Pv と pPv の二相を指定
- 出力: `PhaseAssemblage[,]`（2D 配列、各点に安定な相集合体）

この方法は二分法に比べて計算コストが高い（各グリッド点で完全な Gibbs 最小化が必要）が、より一般的であり、三相以上の競合がある系にも適用可能である。

### 14.6.6 数値的安定性と注意点

pPv 相境界の数値計算に際して、以下の点に注意が必要である。

**探索区間の選択**: 二分法は探索区間 $[P_{\text{min}}, P_{\text{max}}]$ 内に零点が存在することを前提とする。区間の両端で $\Delta G$ の符号が同じ場合、`FindPhaseBoundary` は `NaN` を返す。pPv 転移では 100--140 GPa が適切な範囲であるが、鉄端成分や Al 含有端成分では転移圧力がシフトする可能性があるため、探索範囲の調整が必要になることがある。

**EOS 収束の失敗**: 極端な $P$-$T$ 条件では `MieGruneisenEOSOptimizer` の Newton-Raphson 反復が収束しない場合がある。特に、非常に高温（$> 4000$ K）や低圧（$< 100$ GPa）の組み合わせでは、Debye モデルの適用限界に近づく可能性がある。

**複数の零点**: 組成効果（Fe, Al の固溶）により Gibbs エネルギー差が圧力の単調関数でなくなる場合、二分法は最初に見つかった零点のみを返す。複数の交差が疑われる場合は、圧力範囲を分割して各部分区間で独立に探索するか、`CalculateDiagram` で全グリッドを計算する方が安全である。

**温度依存性の非線形性**: クラペイロン勾配は厳密には一定ではなく、温度に弱く依存する。`GetClapeyronSlope` は指定点でのローカルな値を返すため、広い温度範囲にわたる相境界の外挿には注意が必要である。

### 14.6.7 単位変換に関する注意

クラペイロン勾配の計算で用いられる単位変換について補足する。EOS ソルバーが返すエントロピーの単位は J/(mol$\cdot$K)、体積の単位は cm$^3$/mol である。したがって：

$$\frac{\Delta S}{\Delta V} \quad [\text{J/(mol·K)} \div \text{cm}^3\text{/mol}] = [\text{J/(cm}^3\text{·K)}]$$

1 J/cm$^3$ = $10^{-6}$ J/($10^{-6}$ m$^3$) = 1 J/m$^3$ ではなく、1 J/cm$^3$ = $10^6$ Pa = $10^{-3}$ GPa であるから：

$$\frac{dP}{dT} = \frac{\Delta S}{\Delta V} \times 0.001 \quad [\text{GPa/K}]$$

これがコード中の `* 0.001` 変換因子に対応する。

---

## 14.7 コード対応

本節では `PostPerovskiteCalculator` および関連クラスのコードベースにおける位置づけと使用法を示す。

### 14.7.1 主要ファイル

| ファイル | 役割 |
|---------|------|
| `src/ThermoElastic.Core/Calculations/PostPerovskiteCalculator.cs` | pPv 転移の中核計算クラス |
| `src/ThermoElastic.Core/Calculations/PhaseDiagramCalculator.cs` | 汎用相図計算・境界探索 |
| `src/ThermoElastic.Core/Calculations/MieGruneisenEOSOptimizer.cs` | EOS 求解器 |
| `src/ThermoElastic.Core/Calculations/GibbsMinimizer.cs` | Gibbs エネルギー最小化 |
| `src/ThermoElastic.Core/Database/SLB2011Endmembers.cs` | 鉱物パラメータ定義 |
| `src/ThermoElastic.Desktop/ViewModels/PostPerovskiteViewModel.cs` | GUI ビューモデル |

### 14.7.2 PostPerovskiteCalculator の API

```csharp
public class PostPerovskiteCalculator
{
    // 相境界圧力の探索 (GPa)
    public double FindBoundary(MineralParams pv, MineralParams ppv, double T,
        double pMin = 100.0, double pMax = 140.0);

    // クラペイロン勾配の計算 (GPa/K)
    public double GetClapeyronSlope(MineralParams pv, MineralParams ppv,
        double P, double T);

    // 転移前後の物性比較
    public (ThermoMineralParams pv_props, ThermoMineralParams ppv_props,
            double dVs_percent, double dVp_percent, double dRho_percent)
        CompareAcrossTransition(MineralParams pv, MineralParams ppv,
            double P, double T);
}
```

### 14.7.3 PhaseDiagramCalculator の関連 API

```csharp
public class PhaseDiagramCalculator
{
    // 二相間の相境界圧力 (二分法)
    public double FindPhaseBoundary(PhaseEntry phase1, PhaseEntry phase2,
        double T, double pMin, double pMax, double tolerance = 0.01);

    // 多相反応の境界圧力
    public double FindMultiPhaseBoundary(
        List<(PhaseEntry Phase, double Stoichiometry)> reactants,
        List<(PhaseEntry Phase, double Stoichiometry)> products,
        double T, double pMin, double pMax, double tolerance = 0.01);

    // 指定圧力での転移温度 (逆問題)
    public double FindBoundaryTemperature(PhaseEntry phase1, PhaseEntry phase2,
        double P, double tMin, double tMax, double tolerance = 0.5);

    // 相境界曲線の追跡
    public List<(double P, double T)> TracePhaseBoundary(PhaseEntry phase1,
        PhaseEntry phase2, double tMin, double tMax, int nPoints,
        double pMin = 0.1, double pMax = 30.0);

    // クラペイロン勾配 (GPa/K)
    public double ComputeClapeyronSlope(PhaseEntry phase1, PhaseEntry phase2,
        double P, double T);

    // P-T グリッド上の全相図
    public PhaseAssemblage[,] CalculateDiagram(
        List<PhaseEntry> candidatePhases,
        double[] pressures, double[] temperatures);
}
```

### 14.7.4 使用例

典型的な使用フローを以下に示す。

```csharp
// SLB2011 端成分の取得
var mpv = SLB2011Endmembers.GetMineral("mg_perovskite");   // MgSiO3 Pv
var mppv = SLB2011Endmembers.GetMineral("mg_post_perovskite"); // MgSiO3 pPv

var calc = new PostPerovskiteCalculator();

// 1. 2500 K での相境界圧力
double pBoundary = calc.FindBoundary(mpv, mppv, 2500.0);
// → ~125 GPa (概算)

// 2. クラペイロン勾配
double slope = calc.GetClapeyronSlope(mpv, mppv, pBoundary, 2500.0);
// → ~0.009 GPa/K (概算)

// 3. 転移前後の速度コントラスト
var (pvProps, ppvProps, dVs, dVp, dRho) =
    calc.CompareAcrossTransition(mpv, mppv, 125.0, 2500.0);
// dVs, dVp, dRho: 百分率
```

### 14.7.5 鉄含有端成分の利用

現実的な下部マントル組成では、(Mg,Fe)SiO$_3$ 固溶体を考慮する必要がある。SLB2011 データベースは以下の端成分を提供する：

| 端成分名 | 化学式 | 構造 |
|---------|--------|------|
| `mg_perovskite` (mpv) | MgSiO$_3$ | Pv |
| `fe_perovskite` (fpv) | FeSiO$_3$ | Pv |
| `mg_post_perovskite` (mppv) | MgSiO$_3$ | pPv |
| `fe_post_perovskite` (fppv) | FeSiO$_3$ | pPv |

鉄端成分 fpv/fppv を用いた計算では、相境界圧力と速度コントラストが Mg 端成分とは異なる値を示す。鉄含有量が pPv 転移に与える影響は未解決課題の一つである（14.10 節参照）。

---

## 14.8 歴史

### 14.8.1 D'' 層の発見と謎

マントル最下部の D'' 層は、1949 年に K.E. Bullen によって地球の地震学的構造のモデル化の中で同定された。Bullen は地球内部を A--G の層に分類し、D 層（下部マントル）の最下部に異常な性質を持つ領域として D'' を区別した。その後の研究により、D'' は CMB の約 200--300 km 上方で $V_s$ が急激に増加する不連続面を特徴とすることが明らかになった。

しかし、D'' 不連続面の起源は半世紀以上にわたって深部地球科学の大きな謎であり続けた。提案された仮説には以下のものがある：

- **化学的層状構造**: コアからの物質の浸透による組成変化
- **部分溶融**: CMB 直上の高温による部分溶融帯
- **コア-マントル反応生成物**: コアの鉄とマントルの珪酸塩の化学反応
- **構造相転移**: 未知の高圧相転移

いずれの仮説も、観測された地震学的特徴（特に $V_s$ の顕著なジャンプと $V_p$ の相対的に小さなジャンプ）を完全には説明できなかった。

### 14.8.2 2004 年の画期的発見

2004 年、二つの研究グループが独立にポストペロブスカイト相転移を発見した。

**Murakami et al. (2004, Science)**: 東京工業大学の廣瀬敬グループの村上元彦らは、レーザー加熱ダイヤモンドアンビルセルを用いた実験で、125 GPa を超える圧力下で MgSiO$_3$ ペロブスカイトが新しい結晶構造へ転移することを、SPring-8 のシンクロトロン X 線回折で確認した。この新しい構造は CaIrO$_3$ 型であると同定された。

**Oganov & Ono (2004, Nature)**: Oganov と Ono は第一原理進化的結晶構造探索（USPEX）を用いて、MgSiO$_3$ の高圧安定構造として CaIrO$_3$ 型構造を予測し、実験的にも確認した。

両論文はほぼ同時期に出版され、pPv 転移の存在を確立した。転移圧力（～125 GPa）と正のクラペイロン勾配は、D'' 不連続面の深さと性質に見事に合致した。

### 14.8.3 理論的精緻化

発見直後に、Tsuchiya et al. (2004) が密度汎関数理論（DFT）による詳細な第一原理計算を実行し、pPv 転移の圧力、クラペイロン勾配（$\approx +7.5$ MPa/K）、弾性的性質を計算した。これにより、地球力学モデリングに必要な基礎的パラメータが提供された。

### 14.8.4 ダブルクロッシング仮説の提唱

2005 年、Hernlund, Thomas, Tackley は正のクラペイロン勾配と CMB 近傍の急峻な地温勾配を組み合わせ、ダブルクロッシング仮説を提唱した。この仮説は、D'' で観測される対をなす地震学的反射面を優雅に説明し、pPv 研究を地震学から地球力学まで広範な分野に波及させた。

### 14.8.5 CaIrO$_3$ アナログ研究

pPv 構造は CaIrO$_3$ と同型であるため、CaIrO$_3$ はペロブスカイト→ポストペロブスカイト転移の低圧アナログとして広く研究されてきた。CaIrO$_3$ は常圧で合成可能であるため、pPv 構造の弾性的・レオロジー的性質を詳細に調べることができる。Walte et al. (2007) らは CaIrO$_3$ の変形実験から pPv 構造の滑り系を同定し、D'' の地震学的異方性の解釈に重要な制約を与えた。

他のアナログ物質（MgGeO$_3$, MnGeO$_3$ 等）も、より低い圧力でペロブスカイト→pPv 転移を示すことが確認されており、クラペイロン勾配の系統性や転移のメカニズムの理解に貢献している。

### 14.8.6 SLB2011 データベースへの統合

Stixrude & Lithgow-Bertelloni (2011) は、pPv 端成分（mppv, fppv, appv）を含む包括的な鉱物熱力学データベースを構築した。このデータベースは実験データと第一原理計算の結果を体系的に統合し、下部マントル全体にわたる相平衡計算を可能にした。本コードベースの `PostPerovskiteCalculator` は SLB2011 パラメータを直接使用しており、その精度はデータベースの品質に依存する。

### 14.8.7 その後の展開と現在の位置づけ

pPv 転移の発見は、D'' 研究を記述的な地震学的営みから定量的な鉱物物理学的問題へと変革した。温度モデルから D'' 構造を予測し、逆に D'' の地震学的観測から CMB の熱的条件を推定することが可能になった。鉄やアルミニウムの効果、格子選好配向による地震学的異方性、部分溶融との相互作用など、多くの研究方向が開拓された。

近年では、超高圧実験技術の進歩（特にダイナミック圧縮実験やマルチメガバール静的実験）により、pPv 転移の条件をより広い $P$-$T$ 範囲で検証することが可能になっている。また、機械学習ポテンシャルを用いた分子動力学シミュレーションにより、pPv 転移の動力学的側面（核生成と成長のメカニズム）の研究も進展している。

pPv 転移の発見は、21 世紀の鉱物物理学における最も重要な発見の一つであり、660 km 不連続面に対応するペロブスカイト転移の発見に匹敵するインパクトを持つ。

---

## 14.9 未解決課題

### 14.9.1 クラペイロン勾配の精密値

pPv 転移のクラペイロン勾配は $+5$ ~ $+13$ MPa/K の範囲で報告されており、文献値に大きなばらつきがある。主な報告値を以下にまとめる：

| 研究 | 手法 | $dP/dT$ (MPa/K) |
|------|------|-----------------|
| Tsuchiya et al. (2004) | DFT (LDA) | +7.5 |
| Oganov & Ono (2004) | DFT (GGA) + 実験 | +10 |
| Hirose et al. (2006) | LHDAC 実験 | +11.5 |
| Tateno et al. (2009) | LHDAC 実験 | +8--9 |

この値はダブルクロッシングの予測と D'' のトポグラフィーの推定に直接影響するため、その精密な決定は極めて重要である。例えば、$dP/dT = 7$ MPa/K ではダブルクロッシングが広い温度範囲で生じるが、$dP/dT = 13$ MPa/K では CMB 温度が極端に高い場合にしか生じない。実験的な困難（125 GPa 以上での精密な温度測定、圧力スケールの不確定性）と計算的な不確定性（交換相関汎関数の選択、有限温度効果、無調和項の寄与）が精度を制限している。

### 14.9.2 鉄の分配と転移圧力・幅への影響

Pv と pPv 間の鉄分配は、転移圧力を変化させるとともに、鋭い相転移を幅広い連続的変態に変える可能性がある。鉄含有量が転移にどのように影響するかは部分的にしか理解されておらず、特に CMB 近傍の鉄に富む領域での転移挙動は不明である。

### 14.9.3 ダブルクロッシングの実在性

ダブルクロッシングが実際に地球のマントルで生じているか、またどの程度普遍的であるかは未確定である。地震学的証拠は示唆的ではあるが決定的ではなく、その判定は不十分に制約された CMB 温度に依存する。

### 14.9.4 アルミニウムの役割

アルミニウムの固溶は pPv 相を安定化するか不安定化するかについて議論が分かれている。アルミニウム置換は転移圧力を上昇させ、Al に富む組成では pPv 形成が阻害される可能性がある。下部マントルの実際的な Al 含有量での転移挙動の理解が必要である。

### 14.9.5 部分溶融との相互作用

CMB 近傍の超低速度帯（ULVZ: Ultra-Low Velocity Zone）は部分溶融の存在を示唆する。溶融が pPv 転移にどのように影響するか（転移の抑制、遅延、あるいは促進）は未解明である。pPv と溶融の共存領域の熱力学は複雑であり、実験的にもアクセスが困難である。

### 14.9.6 地震学的異方性との関連

pPv の CaIrO$_3$ 型層状構造は高度に異方的な弾性定数を持つ。D'' で観測される強い地震学的異方性が、pPv の格子選好配向（LPO: Lattice Preferred Orientation）によって説明できるかは活発な研究課題である。変形下での pPv の滑り系の同定と、マントル流によるテクスチャ発達のモデル化が鍵となる。

### 14.9.7 鉄のスピン転移との結合

下部マントル圧力条件では鉄のスピンクロスオーバー（高スピン→低スピン転移）が生じる。この転移は pPv の安定性場と類似の圧力範囲で起こり得るため、構造相転移とスピン転移の結合効果の理解が重要である。

---

## 14.10 演習問題

### 演習 14.1: 相境界曲線の描画

`PostPerovskiteCalculator.FindBoundary` を用いて、$T = 2000, 2500, 3000, 3500$ K における pPv 相境界圧力を計算せよ。結果を $P$-$T$ 空間にプロットし、境界線の傾きからクラペイロン勾配を図的に求めよ。

**ヒント**: 四点の $(T, P_{\text{boundary}})$ データに対して最小二乗法で直線 $P = P_0 + (dP/dT) \cdot T$ をフィットすることで勾配を推定できる。

### 演習 14.2: クラペイロン勾配の二つの推定法の比較

演習 14.1 で得た相境界曲線の有限差分からクラペイロン勾配を推定し（$\Delta P / \Delta T$）、`GetClapeyronSlope` の Clausius-Clapeyron 関係式から得た値と比較せよ。両者の差異の原因を論じよ。

**考察のポイント**: 有限差分法では離散的な温度刻みに伴う誤差が生じる。一方、Clausius-Clapeyron 法は各点でのローカルな $\Delta S$ と $\Delta V$ を用いるため、これらの値の温度依存性を反映する。

### 演習 14.3: 速度コントラストの温度依存性

`CompareAcrossTransition` を用いて、$T = 2000$ K から $3500$ K まで 250 K 刻みで、相境界圧力における $\delta V_s$, $\delta V_p$, $\delta \rho$ を表にまとめよ。速度コントラストは温度によってどのように変化するか？

**発展**: $\delta V_s / \delta V_p$ の比は温度に依存するか？この比が地震学的観測からどのような制約を与え得るか考察せよ。

### 演習 14.4: ダブルクロッシングの判定

CMB から 300 km 上方を起点として、断熱温度 2500 K から CMB 温度 3800 K まで線形に増加する単純な地温勾配モデルを構築せよ。この地温勾配と演習 14.1 の pPv 相境界曲線を重ね合わせ、ダブルクロッシングが生じるか判定せよ。

**発展**: CMB 温度を 3000 K から 4500 K まで変化させ、ダブルクロッシングが生じる臨界 CMB 温度を推定せよ。

### 演習 14.5: 鉄端成分の効果

mpv/mppv を fpv/fppv（鉄端成分）に置き換えて相境界計算を繰り返せ。鉄含有量は転移圧力をどのように移動させるか？CMB 近傍の鉄に富む領域への含意を議論せよ。

### 演習 14.6: 全相図の計算

`PhaseDiagramCalculator.CalculateDiagram` を用いて、$P = 100$--$140$ GPa（1 GPa 刻み）、$T = 2000$--$4000$ K（100 K 刻み）のグリッド上で Pv と pPv の安定性場を計算し、相図を可視化せよ。

**注意**: グリッド上の全点で Gibbs 最小化を実行するため、計算時間は相当量を要する。最初は粗いグリッド（5 GPa、500 K 刻み）で試行し、必要に応じて精緻化せよ。

### 演習 14.7: D'' トポグラフィーの推定

クラペイロン勾配と D'' における温度の水平方向変動モデル（$\pm 500$ K）を用いて、pPv 相境界の起伏（トポグラフィー）を推定せよ。何 km の起伏に対応するか？

**ヒント**: 圧力変化と深さ変化の関係には $dP/dr \approx -\rho g$ を用い、マントル最下部での典型的な $\rho \approx 5500$ kg/m$^3$、$g \approx 10$ m/s$^2$ を仮定せよ。$\Delta P = (dP/dT) \times \Delta T$ から $\Delta r \approx \Delta P / (\rho g)$ を計算する。

### 演習 14.8: パラメータ感度解析

mppv の参照パラメータ（$V_0$, $K_0$, $K_0'$）をそれぞれ $\pm 5\%$ 変化させて `FindBoundary` を再計算し、境界圧力の変化を評価せよ。どのパラメータが最も大きな影響を与えるか？

**考察**: パラメータ感度解析は、実験的制約の優先順位を決定する上で重要である。最も感度の高いパラメータの精密測定が相境界の信頼性向上に最も効果的である。

### 演習 14.9: 圧力-深さ変換

pPv 転移圧力を地球内部の深さに変換せよ。PREM（Preliminary Reference Earth Model）の圧力-深さ関係を用いて、$T = 2500$ K における pPv 転移の深さと、CMB（深さ 2891 km）からの距離を計算せよ。

**ヒント**: PREM によるマントル最下部の圧力勾配は約 $dP/dr \approx -0.05$ GPa/km である。すなわち 1 GPa の圧力差は約 20 km の深さの差に対応する。

### 演習 14.10: エントロピー・体積変化の圧力依存性

`CompareAcrossTransition` から得られる `ThermoMineralParams` を用いて、相境界上の $\Delta S$ と $\Delta V$ を温度の関数として計算せよ（$T = 2000$--$3500$ K、500 K 刻み）。Clausius-Clapeyron の関係式から予測されるクラペイロン勾配の温度依存性を評価し、$dP/dT$ が定数で近似できる温度範囲を議論せよ。

**発展**: $\Delta S(T)$ と $\Delta V(T)$ のそれぞれの温度依存性を個別に評価し、クラペイロン勾配の変化がどちらの量の変化に支配されているかを特定せよ。

---

## 14.11 図表

### 図 14.1: P-T 相図と Pv-pPv 相境界

- **横軸**: 温度 $T$（K）、範囲 2000--4000 K
- **縦軸**: 圧力 $P$（GPa）、範囲 100--140 GPa
- **主要要素**:
  - 正の傾きを持つ相境界線（$dP/dT \approx +9$ MPa/K）
  - 境界線の下方（低圧側）: 「Pv 安定領域」のラベル
  - 境界線の上方（高圧側）: 「pPv 安定領域」のラベル
  - CMB 圧力 $P \approx 136$ GPa を水平破線で表示
  - 代表的な地温勾配を重ね描き（単一交差と二重交差の両ケース）
  - 実験的不確定性を示す陰影領域

### 図 14.2: 転移前後の地震波速度・密度コントラスト

- **横軸**: 温度 $T$（K）、範囲 2000--3500 K
- **縦軸**: 百分率変化（%）
- **主要要素**:
  - $\delta V_s$（実線）、$\delta V_p$（破線）、$\delta \rho$（点線）の三曲線
  - 各曲線は相境界圧力での値として評価
  - $\delta V_s > \delta V_p$ が視覚的に明確
  - 地震学的検出閾値を水平線で注記

### 図 14.3: D'' 層の模式断面図（ダブルクロッシング）

- **横軸**: 水平距離（模式的）
- **縦軸**: CMB からの深さ（km）、範囲 0--300 km
- **主要要素**:
  - 最下部に CMB を配置
  - 地温勾配プロファイル（熱境界層を含む）
  - pPv レンズ：二つの Pv 領域に挟まれた pPv の帯
  - 地震学的反射面の位置を矢印で示す
  - 冷たい下降流（単一交差）と高温領域（二重交差）の対比

### 図 14.4: Gibbs エネルギー差と二分法の収束

- **横軸**: 圧力 $P$（GPa）、範囲 100--140 GPa
- **縦軸**: $\Delta G = G_{\text{Pv}} - G_{\text{pPv}}$（kJ/mol）
- **主要要素**:
  - $T = 2500$ K での $\Delta G(P)$ 曲線
  - 低圧では $\Delta G < 0$（Pv 安定）、高圧では $\Delta G > 0$（pPv 安定）
  - 零点（相境界）にマーカー
  - 二分法の数ステップを中間点として表示し、収束の様子を図解

### 図 14.5: 結晶構造の比較

- **表現**: ブリッジマナイト Pv（$Pbnm$）と pPv（$Cmcm$）の多面体表示を並置
- **主要要素**:
  - Pv: SiO$_6$ 八面体が角を共有する三次元ネットワーク
  - pPv: SiO$_6$ 八面体が辺を共有するシートが Mg 層で隔てられた層状構造
  - Mg, Si, O の位置をラベル
  - 体積減少の方向を矢印で示す

### 表 14.1: SLB2011 端成分パラメータ（Pv・pPv 系）

| パラメータ | mpv | mppv | fpv | fppv |
|-----------|-----|------|-----|------|
| 化学式 | MgSiO$_3$ | MgSiO$_3$ | FeSiO$_3$ | FeSiO$_3$ |
| 構造 | Pv ($Pbnm$) | pPv ($Cmcm$) | Pv ($Pbnm$) | pPv ($Cmcm$) |
| $V_0$ (cm$^3$/mol) | SLB2011 値 | SLB2011 値 | SLB2011 値 | SLB2011 値 |
| $K_0$ (GPa) | SLB2011 値 | SLB2011 値 | SLB2011 値 | SLB2011 値 |
| $K_0'$ | SLB2011 値 | SLB2011 値 | SLB2011 値 | SLB2011 値 |

※ 具体的な数値は SLB2011Endmembers クラスを参照のこと。

---

## 14.12 参考文献

### 基本文献

1. **Murakami, M., Hirose, K., Kawamura, K., Sata, N., Ohishi, Y.** (2004). Post-perovskite phase transition in MgSiO$_3$. *Science*, 304, 855--858.
   - pPv 相転移の実験的発見。レーザー加熱ダイヤモンドアンビルセルで 125 GPa 以上の圧力下で CaIrO$_3$ 型構造を同定した画期的論文。

2. **Oganov, A.R., Ono, S.** (2004). Theoretical and experimental evidence for a post-perovskite phase of MgSiO$_3$ in Earth's D'' layer. *Nature*, 430, 445--448.
   - 第一原理計算と実験の双方から pPv 転移を独立に報告。Murakami et al. と相補的な計算的根拠を提供。

3. **Hernlund, J.W., Thomas, C., Tackley, P.J.** (2005). A doubling of the post-perovskite phase boundary and structure of Earth's lowermost mantle. *Nature*, 434, 882--886.
   - ダブルクロッシング仮説の提唱。D'' における対をなす地震学的反射面を pPv 転移の二重交差で説明した先駆的論文。

4. **Tsuchiya, T., Tsuchiya, J., Umemoto, K., Wentzcovitch, R.M.** (2004). Phase transition in MgSiO$_3$ perovskite in the earth's lower mantle. *Earth and Planetary Science Letters*, 224, 241--248.
   - 密度汎関数理論による pPv 転移圧力、クラペイロン勾配（$\approx +7.5$ MPa/K）、弾性的性質の第一原理計算。地球力学モデリングに広く使用されるパラメータを提供。

5. **Stixrude, L., Lithgow-Bertelloni, C.** (2011). Thermodynamics of mantle minerals -- II. Phase equilibria. *Geophysical Journal International*, 184, 1180--1213.
   - SLB2011 データベース。本コードベースの `PostPerovskiteCalculator` が使用するペロブスカイトおよびポストペロブスカイト端成分（mpv, fpv, apv, mppv, fppv, appv）の熱力学パラメータの出典。

### 総説・レビュー

6. **Hirose, K.** (2006). Postperovskite phase transition and its geophysical implications. *Reviews of Geophysics*, 44, RG3001.
   - pPv 転移の包括的レビュー。実験的制約、理論的予測、地球物理学的含意を網羅的にカバーしており、本分野の入門に最適。

---

## 14.13 他章関連

本章の内容は以下の章と密接に関連する。

### 前提となる章

| 関連章 | 関連の性質 | 説明 |
|--------|-----------|------|
| 第1章 高圧固体の熱力学 | 前提 | Birch-Murnaghan EOS と有限歪み理論が Pv と pPv の両相の冷圧縮エネルギーを提供する。相境界はこの熱力学的枠組みの上に構築された Gibbs エネルギーの比較により決定される。 |
| 第6章 Mie-Gruneisen EOS ソルバー | 前提 | `MieGruneisenEOSOptimizer` が Pv と pPv の両方について転移条件での熱力学量（エントロピー、体積、密度、速度）を計算する。 |
| 第7章 SLB2011 鉱物データベース | 前提 | mpv, fpv, apv, mppv, fppv, appv 端成分の鉱物パラメータが SLB2011 データベースから提供される。これらの正確なパラメータ化が信頼性の高い相境界予測に不可欠。 |
| 第12章 Gibbs 自由エネルギーと相安定性 | 前提 | Gibbs エネルギーの比較と最小化による相安定性決定の基本的枠組み。`PostPerovskiteCalculator` が使用する `PhaseDiagramCalculator` は `GibbsMinimizer` インフラストラクチャに依拠する。 |

### 発展的に関連する章

| 関連章 | 関連の性質 | 説明 |
|--------|-----------|------|
| 第4章 弾性率と地震波速度 | 応用 | pPv 転移前後の地震波速度コントラスト（$\delta V_s$, $\delta V_p$）は弾性率の枠組みを用いて計算される。これらのコントラストが鉱物物理学と地震学を結ぶ主要な観測量である。 |
| 第8章 Landau 相転移 | 対比・拡張 | pPv 転移は一次（不連続）相転移であり、Landau 理論が扱う連続的相転移とは対照的。Landau 理論（第8章）は石英の $\alpha$-$\beta$ 転移等に適用され、pPv 転移は Gibbs エネルギー等価条件で扱う。両手法の違いと適用範囲の理解が重要。 |
| 第10章 弾性混合モデル | 拡張 | 現実的な下部マントル組成では、Pv と pPv が Fe や Al と固溶体を形成する。(Mg,Fe)(Si,Al)O$_3$ 固溶体の物性を計算するには弾性混合モデルが必要。 |

### 相互参照の視点

pPv 転移の計算は、本コードベースの複数のモジュールを横断的に統合する典型的な応用例である。第6章の EOS ソルバーが各相の熱力学量を提供し、第7章のデータベースが鉱物パラメータを供給し、第12章の Gibbs 最小化フレームワークが相安定性を判定する。そして第4章の弾性率・速度計算が、得られた結果を地震学的観測量に変換する。この章はこれらの要素がどのように組み合わさって地球深部の具体的な問題に適用されるかを示す統合的な事例研究として位置づけられる。

### 計算パイプラインの全体像

pPv 転移の計算における各章の役割を、データフローとして以下にまとめる：

```
第7章 SLB2011 Database
  → MineralParams (mpv, mppv, fpv, fppv, ...)
      ↓
第6章 MieGruneisenEOSOptimizer
  → F(V,T), P(V,T), S(V,T), V_eq(P,T)
      ↓
第12章 GibbsMinimizer / PhaseDiagramCalculator
  → G(P,T) for each phase
  → Phase boundary: G_Pv = G_pPv
  → Clapeyron slope: dP/dT = ΔS/ΔV
      ↓
第4章 Elastic Moduli & Velocities
  → K_S, G, V_p, V_s, ρ for each phase
  → δV_s, δV_p, δρ across transition
      ↓
第14章 (本章) PostPerovskiteCalculator
  → Geophysical interpretation
  → D'' structure, double-crossing
```

この計算パイプラインは、鉱物パラメータから地震学的観測量までを一貫して結ぶものであり、鉱物物理学における「順問題」（forward problem）の典型的なフレームワークを体現している。逆に、地震学的観測から鉱物パラメータや温度条件を推定する「逆問題」（inverse problem）は、このパイプラインを逆方向に辿ることに相当し、より困難であるが地球科学的に極めて重要な課題である。
