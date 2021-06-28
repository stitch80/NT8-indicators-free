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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ST_PropulsionDots : Indicator
	{
		private Series<int>			stateSeries;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ST PropulsionDots";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				PaintPriceMarkers							= false;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				FastEMA					= 8;
				SlowEMA					= 21;
							
				AddPlot(new Stroke(Brushes.Aqua, 2), PlotStyle.Dot, "BuyDot");
				AddPlot(new Stroke(Brushes.Yellow, 2), PlotStyle.Dot, "BuyStopDot");
				AddPlot(new Stroke(Brushes.Aqua, 2), PlotStyle.Dot, "SellDot");
				AddPlot(new Stroke(Brushes.Yellow, 2), PlotStyle.Dot, "SellStopDot");
			}
			else if (State == State.Configure)
			{
				this.Name = "";
				
				
			}
			else if (State == State.DataLoaded)
			{
				stateSeries	= new Series<int>(this);
				
			}
		}

		protected override void OnBarUpdate()
		{	
			double FastEMAValue = EMA(Close, FastEMA)[0];
			double SlowEMAValue = EMA(Close, SlowEMA)[0];
			
			
			
			stateSeries[0] = GetNextState(stateSeries[1], FastEMAValue, SlowEMAValue);	
			
			if (stateSeries[0] == 1 && stateSeries[1] != 1) {
				BuyDot[0] = Low[0];
			}
			
			if (stateSeries[0] == 1 || (stateSeries[0] == 0 && stateSeries[1] == 1)) {
				BuyStopDot[0] = SlowEMAValue;
			}
			
			if (stateSeries[0] == -1 && stateSeries[1] != -1) {
				SellDot[0] = High[0];
			}
			
			
			if (stateSeries[0] == -1 || (stateSeries[0] == 0 && stateSeries[1] == -1)) {
				SellStopDot[0] = SlowEMAValue;
			}
			
			
		}
		
		

		#region Miscellaneous
		private int GetNextState(int state, double FastEMAValue, double SlowEMAValue)
		{			
			switch (state)
			{
				case 0:
					if (FastEMAValue > SlowEMAValue && Low[1] > FastEMAValue && Low[0] <= FastEMAValue && Low[0] >= SlowEMAValue) {
						return 1;
					} else if (SlowEMAValue > FastEMAValue && High[1] < FastEMAValue && High[0] >= FastEMAValue && High[0] <= SlowEMAValue) {
						return -1;
					} else {
						return 0;
					}
					
				case 1:
					if (Low[0] <= SlowEMAValue) {
						return 0;
					} else {
						return 1;
					}

				case -1:
					if (High[0] >= SlowEMAValue) {
						return 0;
					} else {
						return -1;
					}

				default:			// Should not happen
					return state;
			}

		}
		#endregion

		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastEMA", Order=1, GroupName="Parameters")]
		public int FastEMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowEMA", Order=2, GroupName="Parameters")]
		public int SlowEMA
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyDot
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuyStopDot
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellDot
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellStopDot
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ST_PropulsionDots[] cacheST_PropulsionDots;
		public ST_PropulsionDots ST_PropulsionDots(int fastEMA, int slowEMA)
		{
			return ST_PropulsionDots(Input, fastEMA, slowEMA);
		}

		public ST_PropulsionDots ST_PropulsionDots(ISeries<double> input, int fastEMA, int slowEMA)
		{
			if (cacheST_PropulsionDots != null)
				for (int idx = 0; idx < cacheST_PropulsionDots.Length; idx++)
					if (cacheST_PropulsionDots[idx] != null && cacheST_PropulsionDots[idx].FastEMA == fastEMA && cacheST_PropulsionDots[idx].SlowEMA == slowEMA && cacheST_PropulsionDots[idx].EqualsInput(input))
						return cacheST_PropulsionDots[idx];
			return CacheIndicator<ST_PropulsionDots>(new ST_PropulsionDots(){ FastEMA = fastEMA, SlowEMA = slowEMA }, input, ref cacheST_PropulsionDots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ST_PropulsionDots ST_PropulsionDots(int fastEMA, int slowEMA)
		{
			return indicator.ST_PropulsionDots(Input, fastEMA, slowEMA);
		}

		public Indicators.ST_PropulsionDots ST_PropulsionDots(ISeries<double> input , int fastEMA, int slowEMA)
		{
			return indicator.ST_PropulsionDots(input, fastEMA, slowEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ST_PropulsionDots ST_PropulsionDots(int fastEMA, int slowEMA)
		{
			return indicator.ST_PropulsionDots(Input, fastEMA, slowEMA);
		}

		public Indicators.ST_PropulsionDots ST_PropulsionDots(ISeries<double> input , int fastEMA, int slowEMA)
		{
			return indicator.ST_PropulsionDots(input, fastEMA, slowEMA);
		}
	}
}

#endregion
