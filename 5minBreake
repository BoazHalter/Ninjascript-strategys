#region Using declarations
using System;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript.Strategies;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class Breakout5MinStrategy : Strategy
    {
        private double prevHigh;
        private double prevLow;

        [NinjaScriptProperty]
        [Range(0.1, 10)]
        [Display(Name = "Stop-Loss %", Order = 1, GroupName = "Risk Management")]
        public double StopLossPercent { get; set; } = 1;  // Default: 1% stop-loss

        [NinjaScriptProperty]
        [Range(0.1, 20)]
        [Display(Name = "Take-Profit %", Order = 2, GroupName = "Risk Management")]
        public double TakeProfitPercent { get; set; } = 2; // Default: 2% take-profit

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Breakout strategy for 5-minute candles with stop-loss and take-profit.";
                Name = "Breakout5MinStrategy";
                Calculate = Calculate.OnEachTick;  // React immediately when conditions are met
                IsOverlay = false;
                AddDataSeries(Data.BarsPeriodType.Minute, 5);  // Ensure we have 5-minute data
            }
        }

        protected override void OnBarUpdate()
        {
            // Ensure we are working with the secondary 5-minute bars
            if (BarsInProgress != 1)
                return;

            // Avoid errors on insufficient bars
            if (CurrentBars[1] < 2)
                return;

            // Store the previous 5-minute candle high and low
            prevHigh = Highs[1][1];
            prevLow = Lows[1][1];

            double stopLossDistance, takeProfitDistance;

            // Buy breakout: if price moves above the previous candle high
            if (Close[0] > prevHigh && Position.MarketPosition == MarketPosition.Flat)
            {
                double entryPrice = prevHigh;
                stopLossDistance = entryPrice * (StopLossPercent / 100);
                takeProfitDistance = entryPrice * (TakeProfitPercent / 100);

                EnterLong();
                SetStopLoss(CalculationMode.Price, entryPrice - stopLossDistance);
                SetProfitTarget(CalculationMode.Price, entryPrice + takeProfitDistance);
            }

            // Sell breakdown: if price moves below the previous candle low
            if (Close[0] < prevLow && Position.MarketPosition == MarketPosition.Flat)
            {
                double entryPrice = prevLow;
                stopLossDistance = entryPrice * (StopLossPercent / 100);
                takeProfitDistance = entryPrice * (TakeProfitPercent / 100);

                EnterShort();
                SetStopLoss(CalculationMode.Price, entryPrice + stopLossDistance);
                SetProfitTarget(CalculationMode.Price, entryPrice - takeProfitDistance);
            }
        }
    }
}
