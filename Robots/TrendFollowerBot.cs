using cAlgo.API;
using cAlgo.API.Indicators;
using cAlgo.Indicators;
using System.Linq;

namespace cAlgo.Robots
{
    [Robot(TimeZone = TimeZones.UTC, AccessRights = AccessRights.None)]
    public class TrendFollowerBot : Robot
    {
        // Indicatori tecnici
        private MovingAverage ma50;
        private MovingAverage ma200;
        private RelativeStrengthIndex rsi;
        private MacdCrossOver macd;
        private BollingerBands bb;
        private StochasticOscillator stochastic;
        private ParabolicSAR psar;
        private IchimokuKinkoHyo ichimoku;
        private MomentumOscillator momentum;
        private CommodityChannelIndex cci;
        private AwesomeOscillator ao;
        private AcceleratorOscillator ac;
        private WilliamsPercentRange williamsR;
        private ADX adx;
        private FractalIndicator fractals;
        private ChaikinMoneyFlow cmf;
        private Envelopes envelopes;
        private HeikinAshi heikinAshi;
        private StandardDeviation stdDev;
        private RateOfChange roc;

        // Configurazione iniziale
        private double capitale = 100;
        private const double Obiettivo = 1000000;
        private const double StopLossPercent = 0.05;
        private const double TakeProfitPercent = 0.10;
        private const int MinConfirmations = 15;

        protected override void OnStart()
        {
            // Inizializza gli indicatori
            ma50 = Indicators.MovingAverage(MarketSeries.Close, 50, MovingAverageType.Exponential);
            ma200 = Indicators.MovingAverage(MarketSeries.Close, 200, MovingAverageType.Exponential);
            rsi = Indicators.RelativeStrengthIndex(MarketSeries.Close, 14);
            macd = Indicators.MacdCrossOver(12, 26, 9);
            bb = Indicators.BollingerBands(MarketSeries.Close, 20, 2, MovingAverageType.Simple);
            stochastic = Indicators.StochasticOscillator(5, 3, 3, MovingAverageType.Simple);
            psar = Indicators.ParabolicSAR(0.02, 0.2);
            ichimoku = Indicators.IchimokuKinkoHyo(9, 26, 52);
            momentum = Indicators.MomentumOscillator(MarketSeries.Close, 14);
            cci = Indicators.CommodityChannelIndex(MarketSeries.Close, 20);
            ao = Indicators.AwesomeOscillator();
            ac = Indicators.AcceleratorOscillator();
            williamsR = Indicators.WilliamsPercentRange(14);
            adx = Indicators.Adx(14);
            fractals = Indicators.FractalIndicator();
            cmf = Indicators.ChaikinMoneyFlow(20);
            envelopes = Indicators.Envelopes(MarketSeries.Close, 14, 0.1, MovingAverageType.Simple);
            heikinAshi = Indicators.HeikinAshi();
            stdDev = Indicators.StandardDeviation(MarketSeries.Close, 20, MovingAverageType.Simple);
            roc = Indicators.RateOfChange(MarketSeries.Close, 12);
        }

        protected override void OnTick()
        {
            // Controlla il trend attuale
            int confirmations = 0;

            if (MarketSeries.Close.LastValue > ma50.Result.LastValue) confirmations++;
            if (MarketSeries.Close.LastValue > ma200.Result.LastValue) confirmations++;
            if (rsi.Result.LastValue > 50) confirmations++;
            if (macd.Histogram.LastValue > 0) confirmations++;
            if (MarketSeries.Close.LastValue > bb.Top.LastValue) confirmations++;
            if (stochastic.PercentK.LastValue > stochastic.PercentD.LastValue) confirmations++;
            if (MarketSeries.Close.LastValue > psar.Result.LastValue) confirmations++;
            if (ichimoku.TenkanSen.LastValue > ichimoku.KijunSen.LastValue) confirmations++;
            if (momentum.Result.LastValue > 100) confirmations++;
            if (cci.Result.LastValue > 100) confirmations++;
            if (ao.Histogram.LastValue > 0) confirmations++;
            if (ac.Histogram.LastValue > 0) confirmations++;
            if (williamsR.Result.LastValue > -50) confirmations++;
            if (adx.Result.LastValue > 25) confirmations++;
            if (fractals.HighSeries.LastValue > MarketSeries.Close.LastValue) confirmations++;
            if (cmf.Result.LastValue > 0) confirmations++;
            if (envelopes.Top.LastValue > MarketSeries.Close.LastValue) confirmations++;
            if (heikinAshi.Close.LastValue > heikinAshi.Open.LastValue) confirmations++;
            if (stdDev.Result.LastValue > stdDev.Result.Minimum) confirmations++;
            if (roc.Result.LastValue > 0) confirmations++;

            // Se almeno 15 indicatori confermano il trend, entra in posizione
            if (confirmations >= MinConfirmations && capitale < Obiettivo)
            {
                double lotSize = capitale / MarketSeries.Close.LastValue;
                ExecuteMarketOrder(TradeType.Buy, SymbolName, lotSize, "TrendFollowerBot", StopLossPercent, TakeProfitPercent);
                Print("Ordine BUY aperto con lot size: " + lotSize);
            }
        }

        protected override void OnTradeResult(TradeResult result)
        {
            if (result.IsSuccessful)
            {
                if (result.Position.GrossProfit > 0)
                {
                    capitale += result.Position.GrossProfit;
                }
                else
                {
                    capitale -= result.Position.GrossLoss;
                }
                Print("Capitale attuale: " + capitale);

                if (capitale >= Obiettivo)
                {
                    Print("Obiettivo raggiunto! Il bot si fermerà.");
                    Stop();
                }
            }
        }
    }
}
