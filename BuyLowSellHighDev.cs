
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
				

			}
			 
			// information related to market data is not available until at least State.DataLoaded
			else if (State == State.DataLoaded)
			{
			 
			}
		}
		
		protected override void OnBarUpdate()
		{
			if (State == State.Historical)
    		  return;

			if (CurrentBar < BarsRequiredToTrade)
	          return;
			
					
			CloseErrorWindows();
			
			plTotalStatus = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]) + SystemPerformance.AllTrades.TradesPerformance.NetProfit ;
			plUnrealized = Position.GetUnrealizedProfitLoss(PerformanceUnit.Currency, Close[0]);
			log("Real Time Trades LosingTrades: " + SystemPerformance.RealTimeTrades.LosingTrades.TradesCount  );
			log("Current Strategy profit: " + plTotalStatus);
			log("Open Position: " + plUnrealized);
			log("Position.AveragePrice: "+ Position.AveragePrice);
			
				
			
			if((plTotalStatus > -4000.0) && (plUnrealized > -3000.0))
				order(this.GetType().Name);
			
	
				
			
			// Resets the stop loss to the original value when all positions are closed
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				//SetTrailStop("Long 1 Runner",CalculationMode.Ticks,200,false);
			}
			
			// If a long position is open, allow for stop loss modification to breakeven
			if (Position.MarketPosition == MarketPosition.Long)
			{
				// Once the bid price is greater than entry price+10 ticks, set stop loss to breakeven
				if (GetCurrentBid(0) >= Position.AveragePrice + 12 * TickSize && Position.Quantity >= 2){
			
					log("Selling "+ (Position.Quantity -1) + " share at: " + Position.AveragePrice.ToString());
					ExitLongStopLimit(1,GetCurrentBid(0),GetCurrentBid(0));
				}
				if (GetCurrentBid(0) >= Position.AveragePrice + 80 * TickSize && Position.Quantity == 1){
					ExitLongStopLimit(1,GetCurrentBid(0),GetCurrentBid(0));
				}
				if((plTotalStatus < -500.0) || (plUnrealized < -250.0)){
				   log("Exit On: " + plTotalStatus + plUnrealized );
				   //ExitLong();
				}
			}
		}
		
		private void order(string strategy){
			
			if(Instrument.FullName.ToString().Contains("NQ")){							
				log("Submitting Entry orders for: " + Instrument.FullName.ToString());				
				if(strategy == "BuyLowSellHighDev"){
					if (entryOrder == null)
						//nq enty
					
						log("Submitting Long 1a quantity of 1 contract at: " + (GetCurrentBid(0) - TickSize * 2).ToString());
						EnterLongLimit(1,GetCurrentBid(0) - TickSize * 3,  "Long 1 Partial");
						
					    if (Position.MarketPosition == MarketPosition.Long)
        					EnterLongLimit(1,Position.AveragePrice - 3 * TickSize,"Long 1 Runner");

						//EnterLongLimit(1,GetCurrentBid(0) - TickSize * 3,  "Long 1 Runner");
					    
					    // Only enter if at least 10 bars has passed since our last entry
    					//if ((BarsSinceEntryExecution() > 2 || BarsSinceEntryExecution() == -1))
						if (Position.MarketPosition == MarketPosition.Long)
        					EnterLongLimit(2,Position.AveragePrice - 42 * TickSize,"Long Limit 1ap");

						// Only enter if at least 10 bars has passed since our last entry
    					//if ((BarsSinceEntryExecution() > 4 || BarsSinceEntryExecution() == -1))
						if (Position.MarketPosition == MarketPosition.Long)
        					EnterLongLimit(3,Position.AveragePrice - 160 * TickSize,"Long Limit 2ap");
						
						if (Position.MarketPosition == MarketPosition.Long)
        					EnterLongLimit(3,Position.AveragePrice - 800 * TickSize,"Long Limit 3ap");
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
