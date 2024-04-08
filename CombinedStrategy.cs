#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies
{
    public class CombinedStrategy : Strategy
    {
        private int tradesCounter = 0;
        private int lastThreeTrades = 0;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "Combined Strategy incorporating BuyLowSellHigh and SampleTradeObjects";
                Name = "CombinedStrategy";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                BarsRequiredToTrade = 20;
            }
            else if (State == State.Configure)
            {
                AddChartIndicator(CurrentDayOHL()); // Add the current day open, high, low indicator to visually see entry conditions
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < BarsRequiredToTrade)
                return;

            // Entry logic based on BuyLowSellHigh strategy
            EnterLongStopLimit(1, Low[1] - TickSize * 6, "Long 1a");
            EnterLongStopLimit(1, Low[1] - TickSize * 8, "Long 1b");
            EnterLongStopLimit(1, Low[1] - TickSize * 12, "Long 1c");
            EnterLongStopLimit(1, Low[1] - TickSize * 18, "Long 1d");
            EnterLongStopLimit(1, Low[1] - TickSize * 24, "Long 1e");
            EnterLongStopLimit(1, Low[1] - TickSize * 30, "Long 1f");
            EnterLongStopLimit(1, Low[1] - TickSize * 36, "Long 1g");
            EnterLongStopLimit(1, Low[1] - TickSize * 42, "Long 1h");

            // Entry logic based on SampleTradeObjects strategy
            if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
            {
                lastThreeTrades = 0;
                tradesCounter = SystemPerformance.AllTrades.Count;
            }

            if ((SystemPerformance.AllTrades.Count - tradesCounter) >= 3 && SystemPerformance.AllTrades.Count != tradesCounter)
            {
                lastThreeTrades = 0;
                tradesCounter = SystemPerformance.AllTrades.Count;
                
                for (int idx = 1; idx <= 3; idx++)
                {
                    Trade trade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - idx];

                    if (trade.ProfitCurrency > 0)
                        lastThreeTrades++;
                    else if (trade.ProfitCurrency < 0)
                        lastThreeTrades--;
                }
            }

            if (lastThreeTrades != -3)
            {
                if (Position.MarketPosition == MarketPosition.Flat)
                {
                    if (CurrentDayOHL().CurrentLow[0] < CurrentDayOHL().CurrentLow[1])
                        EnterShort();
                    else if (CurrentDayOHL().CurrentHigh[0] > CurrentDayOHL().CurrentHigh[1])
                        EnterLong();
                }
            }

            // Exit logic based on SampleTradeObjects strategy
            if (ADX(14)[0] < ADX(14)[1] && Position.MarketPosition != MarketPosition.Flat)
            {
                if (Position.MarketPosition == MarketPosition.Long)
                    ExitLong();
                else if (Position.MarketPosition == MarketPosition.Short)
                    ExitShort();
            }

            // Profit targets and trailing stops based on BuyLowSellHigh strategy
            SetProfitTarget("Long 1a", CalculationMode.Ticks, 4);
            SetProfitTarget("Long 1b", CalculationMode.Ticks, 8);
            SetProfitTarget("Long 1c", CalculationMode.Ticks, 12);
            SetProfitTarget("Long 1d", CalculationMode.Ticks, 16);
            SetProfitTarget("Long 1e", CalculationMode.Ticks, 20);
            SetProfitTarget("Long 1f", CalculationMode.Ticks, 24);

            SetTrailStop("Long 1a", CalculationMode.Ticks, 30, false);
            SetTrailStop("Long 1b", CalculationMode.Ticks, 30, false);
            SetTrailStop("Long 1c", CalculationMode.Ticks, 30, false);
            SetTrailStop("Long 1d", CalculationMode.Ticks, 30, false);
            SetTrailStop("Long 1e", CalculationMode.Ticks, 30, false);
            SetTrailStop("Long 1f", CalculationMode.Ticks, 30, false);
            SetTrailStop("Long 1g", CalculationMode.Ticks, 30, false);
            SetTrailStop("Long 1h", CalculationMode.Ticks, 30, false);
        }

        #region Properties
        [Display(GroupName = "Parameters", Description
