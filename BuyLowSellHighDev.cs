
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
		private Order stopLossOrder = null;
		private Order entryOrder = null;
		private int barLength = 3; // Number of minutes per bar
        private int entryTicksAboveHigh = 5;
        private int entryTicksBelowLow = 5;
		private double lowPrice;
		private int lastThreeTrades 		= 0;  	// This variable holds our value for how profitable the last three trades were.
		private int priorNumberOfTrades 	= 0;	// This variable holds the number of trades taken. It will be checked every OnBarUpdate() to determine when a trade has closed.
		private int priorSessionTrades		= 0;	// This variable holds the number of trades taken prior to each session break.
		private bool changedDirection;
		double plTotalStatus ;
		double plUnrealized ;
		
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
				//IsInstantiatedOnEachOptimizationIteration	= true;
				//changedDirection 							= false;
				
                
			}
			
			else if (State == State.Configure)
			{
				IsEnabled = true;
				//SetTrailStop(CalculationMode.Ticks,40);

			}
		}
		
		protected override void OnBarUpdate()
		{
			if (State == State.Historical)
    		  return;
			
//			if(Historical)
//			  return;		
			
			if (CurrentBar < BarsRequiredToTrade)
	          return;
			
			try{
			
				CloseErrorWindows();
				
				plTotalStatus = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) + SystemPerformance.AllTrades.TradesPerformance.NetProfit ;
				plUnrealized = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
				log("Real Time Trades LosingTrades: " + SystemPerformance.RealTimeTrades.LosingTrades.TradesCount  );
				log("Current Strategy profit: " + plTotalStatus);
				log("Open Position: " + plUnrealized);
				log("Position.AveragePrice: "+ Position.AveragePrice);
				
					
				
				if((plTotalStatus > -100.0) && (plUnrealized > -100.0))
					order(this.GetType().Name);
				
				if((plTotalStatus < -100.0) || (plUnrealized < -100.0))
					log("Test");
					//ExitLong();
					
				
				// Resets the stop loss to the original value when all positions are closed
				if (Position.MarketPosition == MarketPosition.Flat)
				{
					SetTrailStop(CalculationMode.Ticks,40);
				}
				
				// If a long position is open, allow for stop loss modification to breakeven
				if (Position.MarketPosition == MarketPosition.Long)
				{
					// Once the bid price is greater than entry price+10 ticks, set stop loss to breakeven
					if (GetCurrentBid(0) >= Position.AveragePrice + 7 * TickSize && Position.Quantity >= 1){
						
						log("Selling "+ (Position.Quantity -1) + " share at: " + Position.AveragePrice.ToString());
						
						SetProfitTarget("Long 1 Prtial", CalculationMode.Ticks, 6);
						SetStopLoss("Long 1 Runner",CalculationMode.Price, Position.AveragePrice + 2 * TickSize,false );
						//ExitLongStopLimit(GetCurrentBid(0),GetCurrentBid(0),"Partial Long 1a","Long 1 Prtial");
						
					}
				}
	
			}
			catch (Exception e)
			{
				// In case the indicator has already been Terminated, you can safely ignore errors
				if (State >= State.Terminated)
					return;

				/* With our caught exception we are able to generate log entries that go to the Control Center logs and also print more detailed information
				about the error to the Output Window. */
				
				// Submits an entry into the Control Center logs to inform the user of an error				
				log("SampleTryCatch Error: Please check your  errors.");
				
				// Prints the caught exception in the Output Window
				log(Time[0] + " " + e.ToString());
				CloseStrategy("Close Strategy");
			}
		}
		
		private void order(string strategy){
			
			if(Instrument.FullName.ToString().Contains("NQ")){							
				log("Submitting Entry orders for: " + Instrument.FullName.ToString());				
				if(strategy == "BuyLowSellHighDev"){
					if (entryOrder == null)
						//nq enty
					
						log("Submitting Long 1a quantity of 1 contract at: " + (GetCurrentBid(0) - TickSize * 2).ToString());
						EnterLongLimit(1,GetCurrentBid(0) - TickSize * 5,  "Long 1 Prtial");
						
						EnterLongLimit(1,GetCurrentBid(0) - TickSize * 5,  "Long 1 Runner");
					    
					    // Only enter if at least 10 bars has passed since our last entry
    					//if ((BarsSinceEntryExecution() > 2 || BarsSinceEntryExecution() == -1))
						if (Position.MarketPosition == MarketPosition.Long)
        					EnterLongLimit(2,Position.AveragePrice - 16 * TickSize,"Long Market 1ap");

						// Only enter if at least 10 bars has passed since our last entry
    					//if ((BarsSinceEntryExecution() > 4 || BarsSinceEntryExecution() == -1))
						if (Position.MarketPosition == MarketPosition.Long)
        					EnterLongLimit(3,Position.AveragePrice - 32 * TickSize,"Long Market 2ap");
						
					
//						log("Submitting Long 1ap quantity of 1 contract at: " + (GetCurrentBid(0) - TickSize * 4).ToString());
//						EnterLongLimit(1,GetCurrentBid(0) - TickSize * 6,  "Long 1ap");
			
//						ExitLongStopLimit(1,Position.AveragePrice + 8 * TickSize,Position.AveragePrice + 8 * TickSize,"Long 1a partial","Long 1ap");
//						SetProfitTarget("Long 1ap", CalculationMode.Ticks, 12);
//						log("Submitting Long 1b quantity of 1 contract at: " + (Low[1] - TickSize * 13).ToString());
//						EnterLongLimit(1, Low[1] - TickSize * 25 , "Long 1b");
				
//						log("Submitting Long 1c quantity of 1 contract at: " + (Low[1] - TickSize * 18).ToString());
//						EnterLongLimit(1, Low[0] - TickSize * 69, "Long 1c");
//						log(System.Reflection.MethodBase.GetCurrentMethod().Name);
					
//					if (stopLossOrder == null)
//						log((Position.Quantity).ToString() +" "+(Position.AveragePrice - 30 * TickSize).ToString());
//    					//stopLossOrder = ExitLongStopMarket(Position.Quantity,Position.AveragePrice - 30 * TickSize);
				}	
			}
			if(Instrument.FullName.ToString().Contains("ES")){
				
				log("Submitting Entry orders for: " + Instrument.FullName.ToString());
				if (entryOrder == null)
					//es entry
					log("Submmit Long 1a share at: " + (GetCurrentBid(0) - TickSize *2).ToString());
					EnterLongLimit(1,GetCurrentBid(0) - TickSize *2 ,"Long 1a");
					
					log("Submmit Long 1b share at: " + (GetCurrentBid(0) - TickSize *6).ToString());
					EnterLongLimit(2, GetCurrentBid(0) - TickSize * 6, "Long 1b");
					
					log("Submmit Long 1c share at: " + (Low[1] - TickSize * 13).ToString());
					EnterLongLimit(3, GetCurrentBid(0) - TickSize * 14, "Long 1c");
					
					log("Submmit: ExitLongStopLimit()");  
				    ExitLongLimit(1,Position.AveragePrice + TickSize *40, "Partial 1a","Long 1a");
				
				if (stopLossOrder == null)
    				stopLossOrder = ExitLongStopMarket(Position.Quantity,Position.AveragePrice - 30 * TickSize);		
		    }
	     
					 
		}
		
		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice,
                                    OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{
		  // Assign stopLossOrder in OnOrderUpdate() to ensure the assignment occurs when expected.
		  // This is more reliable than assigning Order objects in OnBarUpdate,
		  // as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
		  if (order.Name.Contains("Long 1") && orderState == OrderState.Filled)
		    stopLossOrder = order;
		 
		  if (stopLossOrder != null && stopLossOrder == order)
		  {
		    // Rejection handling
		    if (order.OrderState == OrderState.Rejected)
		    {
				log(order.OrderState.ToString() + "==" + OrderState.Rejected.ToString());
				log("order: " + order.Name + "Cancelled");
		        // Stop loss order was rejected !!!!
		        // Do something about it here
		    }
		  }
		}
		protected void CloseErrorWindows()
		{
			if (!Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.BeginInvoke(new Action(() => {
					foreach (Window window in Application.Current.Windows)
					{
						if (window.Title == "Error")
						{
							window.Close();
						}
					}
				}));
				return;
			}
		}
		
		private void theWolf(){ //Cleaning after the mess
			log("theWolf()");
			if(Position.MarketPosition == MarketPosition.Short)
			{
				ExitShort(Position.Quantity); 
			}
			if(Position.MarketPosition == MarketPosition.Long)
			{
				ExitLong(Position.Quantity);
			}
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
