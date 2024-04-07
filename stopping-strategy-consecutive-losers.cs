// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
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
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion


// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	public class SampleTradeObjects : Strategy
	{
		private int aDXPeriod 				= 14; 	// Default setting for ADXPeriod
		private int lastThreeTrades 		= 0;  	// This variable holds our value for how profitable the last three trades were.
		private int priorNumberOfTrades 	= 0;	// This variable holds the number of trades taken. It will be checked every OnBarUpdate() to determine when a trade has closed.
		private int priorSessionTrades		= 0;	// This variable holds the number of trades taken prior to each session break.

		protected override void OnStateChange()
		{
			if(State == State.SetDefaults)
			{
				ADXPeriod	= 14;
				Calculate 	= Calculate.OnBarClose;
				Name		= "Sample Trade Objects";
				BarsRequiredToTrade = 20;
			}
			else if(State == State.Configure)
			{
				AddChartIndicator(CurrentDayOHL()); // Add the current day open, high, low indicator to visually see entry conditions.
			}
		}

		protected override void OnBarUpdate()
		{
			// Make sure there are enough bars.
			if (CurrentBar < BarsRequiredToTrade)
				return;
			
			// Reset the trade profitability counter every day and get the number of trades taken in total.
			if (Bars.IsFirstBarOfSession && IsFirstTickOfBar)
			{
				lastThreeTrades 	= 0;
				priorSessionTrades 	= SystemPerformance.AllTrades.Count;
			}
			
			/* Here, SystemPerformance.AllTrades.Count - priorSessionTrades checks to make sure there have been three trades today.
			   priorNumberOfTrades makes sure this code block only executes when a new trade has finished. */
			if ((SystemPerformance.AllTrades.Count - priorSessionTrades) >= 3 && SystemPerformance.AllTrades.Count != priorNumberOfTrades)
			{
				// Reset the counter.
				lastThreeTrades = 0;
				
				// Set the new number of completed trades.
				priorNumberOfTrades = SystemPerformance.AllTrades.Count;
				// Loop through the last three trades and check profit/loss on each.
				for (int idx = 1; idx <= 3; idx++)
				{
					/* The SystemPerformance.AllTrades array stores the most recent trade at the highest index value. If there are a total of 10 trades,
					   this loop will retrieve the 10th trade first (at index position 9), then the 9th trade (at 8), then the 8th trade. */
					Trade trade = SystemPerformance.AllTrades[SystemPerformance.AllTrades.Count - idx];

					/* If the trade's profit is greater than 0, add one to the counter. If the trade's profit is less than 0, subtract one.
						This logic means break-even trades have no effect on the counter. */
					if (trade.ProfitCurrency > 0)
					{
						lastThreeTrades++;
					}
					
					else if (trade.ProfitCurrency < 0)
					{
						lastThreeTrades--;
					}
				}
			}
			
			/* If lastThreeTrades = -3, that means the last three trades were all losing trades.
			   Don't take anymore trades if this is the case. This counter resets every new session, so it only stops trading for the current day. */
			if (lastThreeTrades != -3)
			{
				if (Position.MarketPosition == MarketPosition.Flat)
				{
					// If a new low is made, enter short
					if (CurrentDayOHL().CurrentLow[0] < CurrentDayOHL().CurrentLow[1])
						EnterShort();
					
					// If a new high is made, enter long
					else if (CurrentDayOHL().CurrentHigh[0] > CurrentDayOHL().CurrentHigh[1])
						EnterLong();
				}
			}

			/* Exit a position if "the trend has ended" as indicated by ADX.
			If the current ADX value is less than the previous ADX value, the trend strength is weakening. */
			if (ADX(ADXPeriod)[0] < ADX(ADXPeriod)[1] && Position.MarketPosition != MarketPosition.Flat)
			{
				if (Position.MarketPosition == MarketPosition.Long)
					ExitLong();
				else if (Position.MarketPosition == MarketPosition.Short)
					ExitShort();
			}
		}

		#region Properties
		[Display(GroupName="Parameters", Description="Period for the ADX indicator")]
		public int ADXPeriod
		{
			get { return aDXPeriod; }
			set { aDXPeriod = Math.Max(1, value); }
		}
		#endregion
	}
}
