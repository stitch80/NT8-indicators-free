//
// Copyright (C) 2019, NinjaTrader LLC <www.ninjatrader.com>.
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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ST_Darvas2 : Indicator
	{
		private double			boxBottom				= double.MaxValue;
		private double			boxTop					= double.MinValue;
		private double			currentBarHigh			= double.MinValue;
		private double			currentBarLow			= double.MaxValue;
		private int				savedCurrentBar			= -1;
		private int				startBarActBox;
		
		private int				state;

		private Series<double>	boxBottomSeries;
		private Series<double>	boxTopSeries;
		private Series<double>	currentBarHighSeries;
		private Series<double>	currentBarLowSeries;
		private Series<int>		startBarActBoxSeries;
		private Series<int>		stateSeries;
		
		private bool 			highFirst;
		private bool			lowFirst;
		private Brush 			curBrush = Brushes.Transparent;
		private bool			breakoutLong;
		private bool			breakoutShort;
		
		private int				drawObjectsCounter;
		private const int		drawObjectsMax = 30;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description	= "Darvas Box 2.0";
				Name		= "ST Darvas2.0";
				IsOverlay	= true;
				Calculate	= Calculate.OnBarClose;

				AddPlot(new Stroke(Brushes.Crimson,		2), PlotStyle.Square, "Lower");
				AddPlot(new Stroke(Brushes.DarkCyan,	2), PlotStyle.Square, "Upper");
				
			}
			else if (State == State.Configure)
			{
				this.Name = "";
			}
			else if (State == State.DataLoaded)
			{
				if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
				{
					boxBottomSeries			= new Series<double>(this);
					boxTopSeries			= new Series<double>(this);
					currentBarHighSeries	= new Series<double>(this);
					currentBarLowSeries		= new Series<double>(this);
					startBarActBoxSeries	= new Series<int>(this);
					stateSeries				= new Series<int>(this);
				}
			}
		}

		protected override void OnBarUpdate()
		{

			if (savedCurrentBar == -1)
			{
				currentBarHigh	= High[0];
				currentBarLow	= Low[0];
				state			= GetNextState();
				savedCurrentBar = CurrentBar;
			}
			else if (savedCurrentBar != CurrentBar)
			{
				// Check for new bar
				currentBarHigh	= High[0];
				currentBarLow	= Low[0];
				
				//Get the state for current bar
				state = GetNextState();
				
				//Change the color of the Darvas box
				if (state == 5) {
					if (highFirst)
					curBrush = Brushes.Yellow;
				else if (lowFirst)
					curBrush = Brushes.Purple;
				}
				else 
					curBrush = Brushes.Transparent;
				

				//Draw breakout arrows
				if (breakoutLong) {
					drawObjectsManipulation();
					string curTag = "breakoutLong" + drawObjectsCounter;
					Draw.ArrowUp(this, curTag, true, 0, Low[0] - TickSize, Brushes.Green);
				}
				if (breakoutShort) {
					drawObjectsManipulation();
					string curTag = "breakoutShort" + drawObjectsCounter;
					Draw.ArrowDown(this, curTag, true, 0, High[0] + TickSize, Brushes.Red);
				}
				
				//Draw Darvas Box
				for (int i = CurrentBar - startBarActBox; i >= 0; i--)
				{
					Upper[i] = boxTop;
					Lower[i] = boxBottom;
					PlotBrushes[0][i] = curBrush;
					PlotBrushes[1][i] = curBrush;
				}
			}
		}

		#region Miscellaneous
		private void drawObjectsManipulation() {
			drawObjectsCounter++;
			if (drawObjectsCounter >= drawObjectsMax)
				drawObjectsCounter = 0;
			if (DrawObjects.Count >= drawObjectsMax) {
				RemoveDrawObject(DrawObjects.First().Tag);
			}
		}
		
		private int GetNextState()
		{
			switch (state)
			{
				case 0:
					boxTop		= currentBarHigh;
					boxBottom	= currentBarLow;
					breakoutLong = false;
					breakoutShort = false;
					highFirst = true;
					lowFirst = false;
					return 1;

				//Search for highFirst Max/ firstLow Min
				case 1:
					breakoutLong = false;
					breakoutShort = false;
					if (highFirst) {
						if (currentBarHigh > boxTop) {
							boxTop = currentBarHigh;
							return 1;
						}
						else {
							return 2;
						}
					}
					else {
						if (currentBarLow < boxBottom) {
							boxBottom = currentBarLow;
							return 1;
						}
						else {
							return 2;
						}
					}

				//highFirst Max/ firstLow Min found
				case 2:
					breakoutLong = false;
					breakoutShort = false;
					if(highFirst) {
						if (currentBarHigh > boxTop) {
							boxTop = currentBarHigh;
							return 1;
						}
						else {
							boxBottom = currentBarLow;
							return 3;
						}
					}
					else {
						if (currentBarLow < boxBottom) {
							boxBottom = currentBarLow;
							return 1;
						}
						else {
							boxTop = currentBarHigh;
							return 3;
						}
					}

				//Search for highFirst Min/ firstLow Max
				case 3:
					breakoutLong = false;
					breakoutShort = false;
					if (highFirst) {
						if (currentBarHigh > boxTop) {
							boxTop = currentBarHigh;
							return 1;
						}
						else if (currentBarLow < boxBottom) {
							boxBottom = currentBarLow;
							return 3;
						}
						else {
							return 4;
						}
					}
					else {
						if (currentBarLow < boxBottom) {
							boxBottom = currentBarLow;
							return 1;
						}
						else if (currentBarHigh > boxTop) {
							boxTop = currentBarHigh;
							return 3;
						}
						else {
							return 4;
						}
					}

				//firstHigh Min/ firstLow Max found
				//Darvas Box borders found
				case 4:
					breakoutLong = false;
					breakoutShort = false;
					if(highFirst) {
						if (currentBarHigh > boxTop) {
							boxTop = currentBarHigh;
							return 1;
						}
						else if (currentBarLow < boxBottom) {
							boxBottom = currentBarLow;
							return 3;
						}
						else {
							return 5;
						}
					}
					else {
						if (currentBarLow < boxBottom) {
							boxBottom = currentBarLow;
							return 1;
						}
						else if (currentBarHigh > boxTop) {
							boxTop = currentBarHigh;
							return 3;
						}
						else {
							return 5;
						}
					}

				//Serach for breaking Darvas Box borders
				case 5:
					if (currentBarHigh > boxTop || currentBarLow < boxBottom) {
						if (currentBarHigh > boxTop) {
							breakoutLong = true;
							highFirst = true;
							lowFirst = false;
							startBarActBox	 = CurrentBar;
							
						} 
						else if (currentBarLow < boxBottom) {
							breakoutShort = true;
							highFirst = false;
							lowFirst = true;
							startBarActBox	 = CurrentBar;
						}
						
						boxTop = currentBarHigh;
						boxBottom = currentBarLow;
						return 1;
					}
					else {
						breakoutLong = false;
						breakoutShort = false;
						return 5;
					}
					

				default:			// Should not happen
					return state;
			}

		}
		#endregion

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
		{
			get { return Values[0]; }
		}


		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ST_Darvas2[] cacheST_Darvas2;
		public ST_Darvas2 ST_Darvas2()
		{
			return ST_Darvas2(Input);
		}

		public ST_Darvas2 ST_Darvas2(ISeries<double> input)
		{
			if (cacheST_Darvas2 != null)
				for (int idx = 0; idx < cacheST_Darvas2.Length; idx++)
					if (cacheST_Darvas2[idx] != null &&  cacheST_Darvas2[idx].EqualsInput(input))
						return cacheST_Darvas2[idx];
			return CacheIndicator<ST_Darvas2>(new ST_Darvas2(), input, ref cacheST_Darvas2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ST_Darvas2 ST_Darvas2()
		{
			return indicator.ST_Darvas2(Input);
		}

		public Indicators.ST_Darvas2 ST_Darvas2(ISeries<double> input )
		{
			return indicator.ST_Darvas2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ST_Darvas2 ST_Darvas2()
		{
			return indicator.ST_Darvas2(Input);
		}

		public Indicators.ST_Darvas2 ST_Darvas2(ISeries<double> input )
		{
			return indicator.ST_Darvas2(input);
		}
	}
}

#endregion
