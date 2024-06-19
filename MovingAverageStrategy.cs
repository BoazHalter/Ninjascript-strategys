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
        private readonly TimeSpan startTime = new TimeSpan(01, 	0 , 0);
        private readonly TimeSpan endTime = new TimeSpan(23, 59, 0);
		private DynamicSRLines dynamicSRLines;
		private int lastThreeTrades 		= 0;  	// This variable holds our value for how profitable the last three trades were.
		private int priorNumberOfTrades 	= 0;	// This variable holds the number of trades taken. It will be checked every OnBarUpdate() to determine when a trade has closed.
		private int priorSessionTrades		= 0;	// This variable holds the number of trades taken prior to each session break.

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
				//550
                ProfitTargetPoints = 550; // Default profit target set to 75 points
                StopLossPoints = 81;     // Default stop loss set to 75 points
                IsInstantiatedOnEachOptimizationIteration = true;
				PartialProfitTicks = 300;
				SecondPartialProfitTicks = 50;
				StopTargetHandling = StopTargetHandling.ByStrategyPosition;
				RealtimeErrorHandling = RealtimeErrorHandling.IgnoreAllErrors;
            }
            else if (State == State.Configure)
            {
				AddDataSeries(Data.BarsPeriodType.Minute, 5);
                // Set the profit target
                //SetProfitTarget(CalculationMode.Ticks, ProfitTargetPoints);
                //SetProfitTarget("Long 2 Runner",CalculationMode.Ticks,PartialProfitTicks);
				
                // Set the stop loss
                //SetStopLoss(CalculationMode.Ticks, StopLossPoints);
                //SetTrailStop(CalculationMode.Ticks, StopLossPoints);
				
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
			double valueRIND = RIND(9, 36)[5];
			//Print("The current VOL value is " + value.ToString());

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

            //Print(string.Format("Current time {0} is within trading hours {1} - {2}", currentTime, startTime, endTime));
            //Print(string.Format("Fast SMA: {0}, Slow SMA: {1}", smaFast[0], smaSlow[0]));
			
			// Use DynamicSRLines values in your trading logic

			Print("RIND value of 5 bars ago is " + valueRIND.ToString());
			Print("Real Time Trades LosingTrades: " + SystemPerformance.RealTimeTrades.LosingTrades.TradesCount );
			Print("Profit factor is: " + SystemPerformance.AllTrades.TradesPerformance.ProfitFactor);
				if(SystemPerformance.AllTrades.TradesPerformance.ProfitFactor < 1){
					smaFast = SMA(BarsArray[1],Fast);
                	smaSlow = SMA(BarsArray[1],Slow);
				}
            //Print(string.Format("Resistance Level: {0}, Support Level: {1}", resistanceLevel, supportLevel));
			if (Position.MarketPosition == MarketPosition.Flat)
			{
					//SetStopLoss(CalculationMode.Ticks, StopLossPoints);
			}
			
			
	            if (CrossAbove(smaFast, smaSlow, 1))
	            {
					
	                Print("CrossAbove detected: Entering Long");
	                EnterLong(3,"Long 1 Runner");
					//EnterLongLimit(2,Position.AveragePrice + 15 * TickSize,"Long 2 Runner");
					//EnterLongLimit(3,Position.AveragePrice + 30 * TickSize,"Long 3 Runner");
					//SetStopLoss(CalculationMode.Ticks, StopLossPoints);
					//SetStopLoss(CalculationMode.Ticks, StopLossPoints);
	            }
				else if (CrossBelow(smaFast, smaSlow, 1))
	            {
					
					
	                Print("CrossBelow detected: Entering Short");
	                EnterShort(3,"Short 1 Runner");
					//EnterShortLimit(2,Position.AveragePrice - 15 * TickSize,"Short 2 Runner");
					//EnterShortLimit(3,Position.AveragePrice - 30 * TickSize,"Short 3 Runner");	
					//SetStopLoss(CalculationMode.Ticks, StopLossPoints);
					//SetStopLoss(CalculationMode.Ticks, StopLossPoints);		
	            }
				
				if (Position.MarketPosition == MarketPosition.Long)
				{
					if (Close[0] < Position.AveragePrice - StopLossPoints * TickSize)
				    {
				        ExitLong();
						//SetStopLoss(CalculationMode.Price, Position.AveragePrice + 60);						
				        Print("Stop loss");
				    }
				    if (Close[0] > Position.AveragePrice + PartialProfitTicks * TickSize)
				    {
				        ExitLongLimit(1,Position.AveragePrice + 60 * TickSize);
						//SetStopLoss(CalculationMode.Price, Position.AveragePrice + 60);						
				        Print("First Long Partial");
				    }
				    if (Close[0] > Position.AveragePrice + SecondPartialProfitTicks * TickSize)
				    {
						ExitLongLimit(1,Position.AveragePrice + 30 * TickSize);
				        //SetStopLoss(CalculationMode.Price, Position.AveragePrice + 30);						
				        Print("Second Long Partial");
				    }
				}


	            
				if (Position.MarketPosition == MarketPosition.Short)
				{
					if (Close[0] > Position.AveragePrice + StopLossPoints  * TickSize)
				    {
				        ExitShort();
						
				        //SetStopLoss(CalculationMode.Price, Position.AveragePrice - 60);
				        Print("stop loss");
				    }
				    if (Close[0] < Position.AveragePrice - PartialProfitTicks * TickSize)
				    {
				        ExitShortLimit(1,Position.AveragePrice - 60 * TickSize);
						
				        //SetStopLoss(CalculationMode.Price, Position.AveragePrice - 60);
				        Print("First Short stop loss");
				    }
				    if (Close[0] < Position.AveragePrice - SecondPartialProfitTicks * TickSize)
				    {
				        ExitShortLimit(1,Position.AveragePrice - 30 * TickSize);
				        Print("Second Short stop loss");
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
        
		[Range(1, int.MaxValue), NinjaScriptProperty]
        [Display(ResourceType = typeof(Custom.Resource), Name = "SecondPartialProfitTicks", GroupName = "NinjaScriptStrategyParameters", Order = 4)]
        public int SecondPartialProfitTicks { get; set; }
  
		#endregion
		
		
    }
}











