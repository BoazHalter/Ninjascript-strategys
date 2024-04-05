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
	public class BuyLowSellHigh : Strategy
	{
		private int barLength = 3; // Number of minutes per bar
        private int entryTicksAboveHigh = 5;
        private int entryTicksBelowLow = 5;
		private double lowPrice;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"BuyLowSellHigh Strategy.";
				Name										= "BuyLowSellHigh";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
			    MaximumBarsLookBack 						= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				//AddChartIndicator(Indicator.HighLowAverage, Periods[barLength], LineStyles.Solid, Colors.Gray);
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
                
			}
			else if (State == State.Configure)
			{
				IsEnabled = true;
			    // Set a trail stop of 10 ticks
				SetStopLoss(CalculationMode.Ticks,30);
				SetProfitTarget(CalculationMode.Ticks, 6);
			}
		}
		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
                return;
			   
            var prevBarHigh = Bars.GetHigh(0);
            var prevBarLow = Bars.GetLow(0);
			Print("BuyLowSellHigh : Strategy | Low[0]: " + Low[0] + " vs Low[0] - 5 * TickSize :" + (Low[0] - 5 * TickSize));
			

            // Place stop-limit 
			double stopPrice = Low[0] - TickSize * 12;

    		// Place the EnterLongStopLimit order
    		EnterLongLimit(1,Open[0] - TickSize *5);
		
		}
	}
}
