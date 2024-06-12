//
// Copyright (C) 2023, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
    public class MovingAverageStrategy : Strategy
    {
        private SMA smaFast;
        private SMA smaSlow;

        // New York trading hours
        private readonly TimeSpan startTime = new TimeSpan(9, 30, 0);
        private readonly TimeSpan endTime = new TimeSpan(16, 0, 0);
		private DynamicSRLines dynamicSRLines;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {	
                Description = "MovingAverageStrategy";
                Name = "MovingAverageStrategy";
				EntryHandling = EntryHandling.UniqueEntries;	
                Calculate = Calculate.OnEachTick;
				Fast = 10;
                Slow = 100;
                ProfitTargetPoints = 220; // Default profit target set to 75 points
                StopLossPoints = 40;     // Default stop loss set to 75 points
                IsInstantiatedOnEachOptimizationIteration = false;
				PartialProfitTicks = 40;
				StopTargetHandling = StopTargetHandling.ByStrategyPosition;
            }
            else if (State == State.Configure)
            {
                // Set the profit target
                SetProfitTarget(CalculationMode.Ticks, ProfitTargetPoints);
                
                // Set the stop loss
                SetStopLoss(CalculationMode.Ticks, StopLossPoints);
            }
            else if (State == State.DataLoaded)
            {
                smaFast = SMA(Fast);
                smaSlow = SMA(Slow);
				dynamicSRLines = DynamicSRLines(5, 200, 10, 2, 3, false, Brushes.Blue, Brushes.Red);

                smaFast.Plots[0].Brush = Brushes.Goldenrod;
                smaSlow.Plots[0].Brush = Brushes.SeaGreen;

                AddChartIndicator(smaFast);
                AddChartIndicator(smaSlow);
				AddChartIndicator(dynamicSRLines);
            }
        }

        protected override void OnBarUpdate()
        {
			
            if (State == State.Historical)
    		  return;
			
			if (CurrentBar < BarsRequiredToTrade)
            {
                Print("CurrentBar < BarsRequiredToTrade");
                return;
            }
			
		
			double value = VOL()[0];
			Print("The current VOL value is " + value.ToString());

            // Convert the current bar time to New York time
            DateTime barTime = Time[0];
            TimeZoneInfo nyTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime nyTime = TimeZoneInfo.ConvertTime(barTime, nyTimeZone);
            TimeSpan currentTime = nyTime.TimeOfDay;

            // Check if the current time is within the trading window
            if (currentTime < startTime || currentTime > endTime)
            {
                Print(string.Format("Current time {0} is outside trading hours {1} - {2}", currentTime, startTime, endTime));
                return;
            }

            Print(string.Format("Current time {0} is within trading hours {1} - {2}", currentTime, startTime, endTime));
            Print(string.Format("Fast SMA: {0}, Slow SMA: {1}", smaFast[0], smaSlow[0]));
			
			// Use DynamicSRLines values in your trading logic
            double resistanceLevel = dynamicSRLines.ZoneTickSize; // Example: replace 'Resistance' with actual property if available
            double supportLevel = dynamicSRLines.PivotStrength; // Example: replace 'Support' with actual property if available

            Print(string.Format("Resistance Level: {0}, Support Level: {1}", resistanceLevel, supportLevel));

            if (CrossAbove(smaFast, smaSlow, 1))
            {
                Print("CrossAbove detected: Entering Long");
                EnterLong(3);
				
            }
			if (Position.MarketPosition  == MarketPosition.Long){						
				EnterLongLimit(2,Position.AveragePrice - 6 * TickSize,"Long 2 Runner");
				EnterLongLimit(3,Position.AveragePrice - 15 * TickSize,"Long 3 Runner");
			}
			if (Position.MarketPosition  == MarketPosition.Long){
				if(Close[0] > Position.AveragePrice && GetCurrentBid(0) > Position.AveragePrice + PartialProfitTicks * TickSize && Position.Quantity >= 3 ){
					ExitLong(2);
				}
			}

            else if (CrossBelow(smaFast, smaSlow, 1))
            {
                Print("CrossBelow detected: Entering Short");
                EnterShort(3);
						
            }
			if (Position.MarketPosition  == MarketPosition.Short)
			{						
				EnterShortLimit(2,Position.AveragePrice + 6 * TickSize,"Short 2 Runner");
				EnterShortLimit(3,Position.AveragePrice + 15 * TickSize,"Short 3 Runner");
				
				
			}
			if (Position.MarketPosition  == MarketPosition.Short && Position.Quantity >= 3)
			{
				if(Close[0] < Position.AveragePrice  && GetCurrentAsk(0) < Position.AveragePrice - PartialProfitTicks * TickSize ){
					ExitShort(2);
				}
			}						
        }

        #region Properties
        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Fast", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
        public int Fast { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "Slow", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
        public int Slow { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "ProfitTargetPoints", GroupName = "NinjaScriptStrategyParameters", Order = 2)]
        public int ProfitTargetPoints { get; set; }

        [Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "StopLossPoints", GroupName = "NinjaScriptStrategyParameters", Order = 3)]
        public int StopLossPoints { get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "PartialProfitTicks", GroupName = "NinjaScriptStrategyParameters", Order = 4)]
        public int PartialProfitTicks { get; set; }
        #endregion
    }
}
