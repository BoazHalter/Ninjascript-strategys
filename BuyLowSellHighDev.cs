


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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class BuyLowSellHighDev : Strategy
	{
		private int barLength = 3; // Number of minutes per bar
        private int entryTicksAboveHigh = 5;
        private int entryTicksBelowLow = 5;
		private double lowPrice;
		private int lastThreeTrades 		= 0;  	// This variable holds our value for how profitable the last three trades were.
		private int priorNumberOfTrades 	= 0;	// This variable holds the number of trades taken. It will be checked every OnBarUpdate() to determine when a trade has closed.
		private int priorSessionTrades		= 0;	// This variable holds the number of trades taken prior to each session break.

		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Validate 1/3 sell move to stop to b/e.";
				Name										= "BuyLowSellHighDev";
				Calculate									= Calculate.OnBarClose;
				RestartsWithinMinutes	 					= 1;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
			    MaximumBarsLookBack 						= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.ImmediatelySubmitSynchronizeAccount;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
			    RealtimeErrorHandling                       = RealtimeErrorHandling.IgnoreAllErrors;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				StopLossTicks				   				= 30;
				ProfitTargetTicks			    			= 100;
				//AddChartIndicator(Indicator.HighLowAverage, Periods[barLength], LineStyles.Solid, Colors.Gray);
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
                
			}
			
			else if (State == State.Configure)
			{
				IsEnabled = true;
				SetStopLoss(CalculationMode.Ticks, StopLossTicks);
				SetProfitTarget("Long 1a",CalculationMode.Ticks, 10);
				SetProfitTarget("Long 1b",CalculationMode.Ticks, 15);
				SetProfitTarget("Long 1c",CalculationMode.Ticks, ProfitTargetTicks);

			}
		}
		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
                return;
			log("Real Time Trades LosingTrades: " + SystemPerformance.RealTimeTrades.LosingTrades.TradesCount  );
			log("AllTrades Losing Trades: " + SystemPerformance.AllTrades.LosingTrades.TradesCount );
			
			// Check for losing trade
	        if (SystemPerformance.RealTimeTrades.LosingTrades.TradesCount <= 3)
	        {
	            log("LosingTrades: " + SystemPerformance.RealTimeTrades.LosingTrades.TradesCount ); 
				entry();
				
				// Resets the stop loss to the original value when all positions are closed
				if (Position.MarketPosition == MarketPosition.Flat)
				{
					SetStopLoss(CalculationMode.Ticks, StopLossTicks);
				}
				
				// If a long position is open, allow for stop loss modification to breakeven
				else if (Position.MarketPosition == MarketPosition.Long)
				{
					// Once the bid price is greater than entry price+10 ticks, set stop loss to breakeven
					if (GetCurrentBid(0) > Position.AveragePrice + 10 * TickSize)
					{
						SetStopLoss("Long 1b",CalculationMode.Price, Position.AveragePrice,false);
						SetStopLoss("Long 1c",CalculationMode.Price, Position.AveragePrice,false);
					}
				}
		        }           
		        else
		        {
		            log("LosingTrades:" + SystemPerformance.AllTrades.LosingTrades.Count );
					log("Reached 3 consecutive losing trades. Stopping strategy...");
					log("Susspended");
					
		        }
		}
		private void entry()
		{
			if (Instrument.FullName.ToString().Contains("MES"));
			{
				log("Submitting Entry orders for: " + Instrument.FullName.ToString());
				
				//es entry
				log("Submmit Long 1a share at: " + (GetCurrentBid(0) - TickSize *2).ToString());
				EnterLongLimit(2,GetCurrentBid(0) - TickSize *2 ,"Long 1a");
				
				log("Submmit Long 1b share at: " + (GetCurrentBid(0) - TickSize *6).ToString());
				EnterLongLimit(1, GetCurrentBid(0) - TickSize * 6, "Long 1b");
				
				log("Submmit Long 1c share at: " + (Low[1] - TickSize * 13).ToString());
				EnterLongLimit(1, Low[1] - TickSize * 14, "Long 1c");

			}
				
			if (Instrument.FullName.ToString().Contains("MNQ"));
			{
           		log("Submitting Entry orders for: " + Instrument.FullName.ToString());				
				//nq enty
				EnterLongLimit(2, Low[1] - TickSize * 9,  "Long 1a");
				EnterLongLimit(1, Low[1] - TickSize * 13, "Long 1b");
				EnterLongLimit(1, Low[1] - TickSize * 18, "Long 1c");
			}	
		}
		
		private void exit(){
			
		}
		
		private void log(string message)
		{
			Print(String.Format("[{0}] [{1}] [{2}]", DateTime.Now.Date + DateTime.Now.TimeOfDay , this.GetType().Name ,message ));
		}	
		
		
		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="StopLossTicks", Description="Numbers of ticks away from entry price for the Stop Loss order", Order=1, GroupName="Parameters")]
		public int StopLossTicks
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="ProfitTargetTicks", Description="Number of ticks away from entry price for the Profit Target order", Order=2, GroupName="Parameters")]
		public int ProfitTargetTicks
		{ get; set; }
		#endregion

	}
}
