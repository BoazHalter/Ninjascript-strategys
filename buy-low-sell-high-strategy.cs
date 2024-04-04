using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;

namespace NinjaTrader.NinjaScript.Strategies
{
    public class StopLimitBreakout : Strategy
    {
        private int barLength = 3; // Number of minutes per bar
        private int entryTicksAboveHigh = 5;
        private int entryTicksBelowLow = 5;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "StopLimitBreakout";
                CalculateOnBarClose = true;
                EntriesPerDirection = 1;
                IsExitOnSessionCloseStrategy = true;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoBars;
                AddChartIndicator(Indicator.HighLowAverage, Periods[barLength], LineStyles.Solid, Colors.Gray);
            }
            else if (State == State.Planning)
            {
                BarsRequired = MaximumBarsLookBack.Value;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequired)
                return;

            var prevBarHigh = Bars.GetValue(Close, 1);
            var prevBarLow = Bars.GetValue(Low, 1);

            // Place stop-limit orders
            EnterStopLimit(Condition.Above, prevBarHigh + (entryTicksAboveHigh * TickSize), true);
            EnterStopLimit(Condition.Below, prevBarLow - (entryTicksBelowLow * TickSize), false);
        }
    }
}
