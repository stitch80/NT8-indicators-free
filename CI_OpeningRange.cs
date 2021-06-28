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
	public class CI_OpeningRange : Indicator
	{
		private CustomEnumNamespace.Ranges rangeType;
		private TimeSpan tradeLength;
		private TimeSpan drawObjectsLifetime;
		private DateTime EndRange;
		private bool isDayStarted;
		private bool isTriggered;
		private double validityHigh;
		private double validityLow;
		private double stopShort;
		private double stopLong;
		private double maxRiskLong;
		private double maxRiskShort;
		private double rangeSize;
		private double rangeAmount;
		private string dirBias;
		private double fiboHigh;
		private double fiboMid;
		private double fiboLow;
		private int triggerBarId;

		public CI_OpeningRange()
		{
			VendorLicense("CrystalIndicators", "OpeningRangeIndicator", "www.crystalindicators.com",
				"info@crystalindicators.com", null);
		}


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Opening Range Indicator";
				Name										= "CI Opening Range Indicator";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				EntryTicks									= 1;
				StopTicksBeyondRange						= 1;
				rangeType									= CustomEnumNamespace.Ranges.thirty;
				MarketOpen									= DateTime.Parse("21:30", System.Globalization.CultureInfo.InvariantCulture);
				ClearDrawObjectsTime						= 10;
				ORRangeRegionColor							= Brushes.DarkOliveGreen;
				ORRangeRegionOpacity						= 50;
				AlertSwitchedOn								= false;
				
				
				AddPlot(Brushes.Transparent, "ORHigh");
				AddPlot(Brushes.Transparent, "ORLow");
				
			}
			else if (State == State.Configure)
			{
				tradeLength = new TimeSpan(0, (int) rangeType, 0);
				drawObjectsLifetime = new TimeSpan(ClearDrawObjectsTime, 0, 0);
				EndRange = MarketOpen + tradeLength;
				isDayStarted = false;
				isTriggered = false;
				this.Name = "";
			}
		
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar <= BarsRequiredToPlot) return;
			
			//New day
			if (MarketOpen.Date != Time[0].Date && ToTime(MarketOpen) == ToTime(Time[1])) {
				MarketOpen = new DateTime(Time[0].Year, Time[0].Month, Time[0].Day, MarketOpen.Hour, MarketOpen.Minute, 0);
				EndRange = MarketOpen + tradeLength;
				isDayStarted = true;
				isTriggered = false;
			}
			
			if (isDayStarted && Time[0] > MarketOpen + drawObjectsLifetime) {
				isDayStarted = false;
				RemoveDrawObjects();
			}
			
			
			//Clearing range
			if ((ToTime(Time[0]) > ToTime(MarketOpen) && ToTime(Time[0]) <= ToTime(EndRange))) {
				//Calculating CR high and low
				int startBarsAgo = CurrentBar - Bars.GetBar(MarketOpen);
				double CRMax = MAX(High, startBarsAgo)[0];
				double CRMin = MIN(Low, startBarsAgo)[0];
				
				ORHigh[0] = CRMax;
				ORLow[0] = CRMin;
				
				Draw.Region(this, "CR", startBarsAgo, 0, ORHigh, ORLow, ORRangeRegionColor, ORRangeRegionColor, ORRangeRegionOpacity);
				
				//Calculating area of validity and fibo zone
				validityHigh = CRMax + EntryTicks * Bars.Instrument.MasterInstrument.TickSize;
				validityLow = CRMin - EntryTicks * Bars.Instrument.MasterInstrument.TickSize;
				stopShort = CRMax + StopTicksBeyondRange * Bars.Instrument.MasterInstrument.TickSize;
				stopLong = CRMin - StopTicksBeyondRange * Bars.Instrument.MasterInstrument.TickSize;
				maxRiskLong = Instrument.MasterInstrument.RoundToTickSize(Math.Abs(validityHigh - stopLong) * Bars.Instrument.MasterInstrument.PointValue);
				maxRiskShort = Instrument.MasterInstrument.RoundToTickSize(Math.Abs(validityLow - stopShort) * Bars.Instrument.MasterInstrument.PointValue);
				fiboHigh = Instrument.MasterInstrument.RoundToTickSize((CRMax - CRMin) * 0.618 + CRMin);
				fiboMid = Instrument.MasterInstrument.RoundToTickSize((CRMax - CRMin) * 0.5 + CRMin);
				fiboLow = Instrument.MasterInstrument.RoundToTickSize((CRMax - CRMin) * 0.382 + CRMin);
				DateTime startEntry = EndRange;
				DateTime endEntry = EndRange + tradeLength;
				
				//Draw area of validity and fibo zone
				Draw.Line(this, "vHigh", true, startEntry, validityHigh, endEntry, validityHigh, Brushes.Gray, DashStyleHelper.Solid, 1);
				Draw.Line(this, "vLow", true, startEntry, validityLow, endEntry, validityLow, Brushes.Gray, DashStyleHelper.Solid, 1);
				Draw.Line(this, "fiboMid", true, startEntry, fiboMid, endEntry, fiboMid, Brushes.White, DashStyleHelper.Dash, 1);
				Draw.Rectangle(this, "vArea", true, startEntry, validityHigh, endEntry, validityLow, Brushes.Transparent, Brushes.Green, 20);
				Draw.Rectangle(this, "fiboArea", true, startEntry, fiboHigh, endEntry, fiboLow, Brushes.Transparent, Brushes.Green, 50);
				
				
				//For RangeInfo label
				rangeSize = Instrument.MasterInstrument.RoundToTickSize(CRMax - CRMin);
				rangeAmount = rangeSize * Bars.Instrument.MasterInstrument.PointValue;
				
			}
			
			//After Clearing range
			if ((ToTime(Time[0]) > ToTime(EndRange) && ToTime(Time[0]) <= ToTime(EndRange + tradeLength))) {
				//Trigger
				NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont();
				if(!isTriggered) {
					if (High[0] >= validityHigh) {
						//Trigger line
						Draw.VerticalLine(this, "trigLong", Time[0], Brushes.Green, DashStyleHelper.Solid, 2);
						Draw.Text(this, "textLong", "LongTrade triggered", 0, validityHigh + (validityHigh - validityLow)/20, Brushes.Green);
						
						//For EntryInfo label
						isTriggered = true;
						dirBias = "long";
						
						triggerBarId = CurrentBar;
					} else if (Low[0] <= validityLow) {
						//Trigger line
						Draw.VerticalLine(this, "trigShort", Time[0], Brushes.Red, DashStyleHelper.Solid, 2);
						Draw.Text(this, "textShort", "ShortTrade triggered", 0, validityLow - (validityHigh - validityLow)/20, Brushes.Red);
						
						//For EntryInfo label
						isTriggered = true;
						dirBias = "short";
						
						triggerBarId = CurrentBar;
					}
					if (AlertSwitchedOn)
						PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert4.wav");
					
				}
				
					if (!dirBias.IsNullOrEmpty()) {
						if (dirBias.Equals("long")) {
							Draw.Diamond(this, "DiaUp", true, CurrentBar - triggerBarId, High[CurrentBar - triggerBarId], Brushes.Green);
						}
						else if (dirBias.Equals("short")) {
							Draw.Diamond(this, "DiaDown", true, CurrentBar - triggerBarId, Low[CurrentBar - triggerBarId], Brushes.Red);
						}
					}
				
			}
			
			
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			//SetZOrder(int.MaxValue);
			//Range Label
			if (isDayStarted) {
				string rangeInfo = "Range " + ((int)rangeType).ToString() + "m: " + String.Format("{0:0.#######}",rangeSize) + "pt = "
					+ rangeAmount + "$";
				
				
				SharpDX.Direct2D1.Brush textBrushDx = System.Windows.Media.Brushes.Black.ToDxBrush(RenderTarget);
				SharpDX.Direct2D1.Brush rangeAreaBrushDx = System.Windows.Media.Brushes.White.ToDxBrush(RenderTarget);
				SharpDX.Direct2D1.Brush smallAreaBorderDx = System.Windows.Media.Brushes.Gray.ToDxBrush(RenderTarget);
				
				
				NinjaTrader.Gui.Tools.SimpleFont simpleFont = chartControl.Properties.LabelFont ??  new NinjaTrader.Gui.Tools.SimpleFont();
				SharpDX.DirectWrite.TextFormat textFormat1 = simpleFont.ToDirectWriteTextFormat();
				SharpDX.Vector2 rangeLabel = new SharpDX.Vector2(ChartPanel.X + 15, ChartPanel.Y + 20);
				SharpDX.DirectWrite.TextLayout textLayout1 =
					new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
						rangeInfo, textFormat1, ChartPanel.X + ChartPanel.W,
						textFormat1.FontSize);
				SharpDX.RectangleF rect1 = new SharpDX.RectangleF(rangeLabel.X - 5, rangeLabel.Y - 5, textLayout1.Metrics.Width + 10,
					textLayout1.Metrics.Height + 10);
				RenderTarget.FillRectangle(rect1, rangeAreaBrushDx);
				RenderTarget.DrawRectangle(rect1, smallAreaBorderDx, 2);
				RenderTarget.DrawTextLayout(rangeLabel, textLayout1, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			
			
				//Triggered entry label
				
				if(isTriggered) {
					SharpDX.Direct2D1.Brush entryAreaBrushDx = System.Windows.Media.Brushes.Transparent.ToDxBrush(RenderTarget);
					string entryInfo = "";
					
					if (dirBias.Equals("long")) {
						entryAreaBrushDx = System.Windows.Media.Brushes.Green.ToDxBrush(RenderTarget);
						entryInfo = "Long Entry: " + validityHigh + " || Stop: " + stopLong + " || MaxRisk: " + maxRiskLong + "$ per contract"
									+ " || FiboHigh: " + fiboHigh + " || FiboMid: " + fiboMid + " || FiboLow: " + fiboLow;
					} else if (dirBias.Equals("short")) {
						entryAreaBrushDx = System.Windows.Media.Brushes.Red.ToDxBrush(RenderTarget);
						entryInfo = "Short Entry: " + validityLow + " Stop: " + stopShort + " MaxRisk: " + maxRiskShort + "$ per contract"
									+ " || FiboHigh: " + fiboHigh + " || FiboMid: " + fiboMid + " || FiboLow: " + fiboLow;
					}
					
					SharpDX.Vector2 entryLabel = new SharpDX.Vector2(rangeLabel.X + textLayout1.Metrics.Width + 15, ChartPanel.Y + 20);
					SharpDX.DirectWrite.TextLayout textLayout2 = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
						entryInfo, textFormat1, ChartPanel.X + ChartPanel.W, textFormat1.FontSize);
					
					SharpDX.RectangleF rect2 = new SharpDX.RectangleF(entryLabel.X - 5, entryLabel.Y - 5, textLayout2.Metrics.Width + 10,
					textLayout2.Metrics.Height + 10);
					RenderTarget.FillRectangle(rect2, entryAreaBrushDx);
					RenderTarget.DrawRectangle(rect2, smallAreaBorderDx, 2);
					RenderTarget.DrawTextLayout(entryLabel, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
					
					entryAreaBrushDx.Dispose();
					textLayout2.Dispose();
				}
				
				
				rangeAreaBrushDx.Dispose();
				smallAreaBorderDx.Dispose();
				textBrushDx.Dispose();
				textFormat1.Dispose();
				textLayout1.Dispose();
			
			}
		}
		
		

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Range Period", Order=1, GroupName="Parameters", Description="Choose a Range Period.")]
		public CustomEnumNamespace.Ranges RangeType
		{
			get { return rangeType; }
			set { rangeType = value; }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EntryTicks", Order=2, GroupName="Parameters")]
		public int EntryTicks
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopTicksBeyondRange", Order=3, GroupName="Parameters")]
		public int StopTicksBeyondRange
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="ORRangeRegionColor", Order=4, GroupName="Parameters")]
		public Brush ORRangeRegionColor
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="ORRangeRegionOpacity", Order=5, GroupName="Parameters")]
		public int ORRangeRegionOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="MarketOpen", Order=6, GroupName="Parameters")]
		public DateTime MarketOpen
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ClearDrawObjectsTime", Order=7, GroupName="Parameters", Description = "Time of drawing objects life in hours")]
		public int ClearDrawObjectsTime
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="AlertSwitchedOn", Order=8, GroupName="Parameters")]
		public bool AlertSwitchedOn
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ORHigh
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ORLow
		{
			get { return Values[1]; }
		}
		
		#endregion

	}
}

namespace CustomEnumNamespace
{
	public enum Ranges
	{
		five = 5,
		fifteen = 15,
		thirty = 30,
		sixty = 60
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CI_OpeningRange[] cacheCI_OpeningRange;
		public CI_OpeningRange CI_OpeningRange(CustomEnumNamespace.Ranges rangeType, int entryTicks, int stopTicksBeyondRange, Brush oRRangeRegionColor, int oRRangeRegionOpacity, DateTime marketOpen, int clearDrawObjectsTime, bool alertSwitchedOn)
		{
			return CI_OpeningRange(Input, rangeType, entryTicks, stopTicksBeyondRange, oRRangeRegionColor, oRRangeRegionOpacity, marketOpen, clearDrawObjectsTime, alertSwitchedOn);
		}

		public CI_OpeningRange CI_OpeningRange(ISeries<double> input, CustomEnumNamespace.Ranges rangeType, int entryTicks, int stopTicksBeyondRange, Brush oRRangeRegionColor, int oRRangeRegionOpacity, DateTime marketOpen, int clearDrawObjectsTime, bool alertSwitchedOn)
		{
			if (cacheCI_OpeningRange != null)
				for (int idx = 0; idx < cacheCI_OpeningRange.Length; idx++)
					if (cacheCI_OpeningRange[idx] != null && cacheCI_OpeningRange[idx].RangeType == rangeType && cacheCI_OpeningRange[idx].EntryTicks == entryTicks && cacheCI_OpeningRange[idx].StopTicksBeyondRange == stopTicksBeyondRange && cacheCI_OpeningRange[idx].ORRangeRegionColor == oRRangeRegionColor && cacheCI_OpeningRange[idx].ORRangeRegionOpacity == oRRangeRegionOpacity && cacheCI_OpeningRange[idx].MarketOpen == marketOpen && cacheCI_OpeningRange[idx].ClearDrawObjectsTime == clearDrawObjectsTime && cacheCI_OpeningRange[idx].AlertSwitchedOn == alertSwitchedOn && cacheCI_OpeningRange[idx].EqualsInput(input))
						return cacheCI_OpeningRange[idx];
			return CacheIndicator<CI_OpeningRange>(new CI_OpeningRange(){ RangeType = rangeType, EntryTicks = entryTicks, StopTicksBeyondRange = stopTicksBeyondRange, ORRangeRegionColor = oRRangeRegionColor, ORRangeRegionOpacity = oRRangeRegionOpacity, MarketOpen = marketOpen, ClearDrawObjectsTime = clearDrawObjectsTime, AlertSwitchedOn = alertSwitchedOn }, input, ref cacheCI_OpeningRange);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CI_OpeningRange CI_OpeningRange(CustomEnumNamespace.Ranges rangeType, int entryTicks, int stopTicksBeyondRange, Brush oRRangeRegionColor, int oRRangeRegionOpacity, DateTime marketOpen, int clearDrawObjectsTime, bool alertSwitchedOn)
		{
			return indicator.CI_OpeningRange(Input, rangeType, entryTicks, stopTicksBeyondRange, oRRangeRegionColor, oRRangeRegionOpacity, marketOpen, clearDrawObjectsTime, alertSwitchedOn);
		}

		public Indicators.CI_OpeningRange CI_OpeningRange(ISeries<double> input , CustomEnumNamespace.Ranges rangeType, int entryTicks, int stopTicksBeyondRange, Brush oRRangeRegionColor, int oRRangeRegionOpacity, DateTime marketOpen, int clearDrawObjectsTime, bool alertSwitchedOn)
		{
			return indicator.CI_OpeningRange(input, rangeType, entryTicks, stopTicksBeyondRange, oRRangeRegionColor, oRRangeRegionOpacity, marketOpen, clearDrawObjectsTime, alertSwitchedOn);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CI_OpeningRange CI_OpeningRange(CustomEnumNamespace.Ranges rangeType, int entryTicks, int stopTicksBeyondRange, Brush oRRangeRegionColor, int oRRangeRegionOpacity, DateTime marketOpen, int clearDrawObjectsTime, bool alertSwitchedOn)
		{
			return indicator.CI_OpeningRange(Input, rangeType, entryTicks, stopTicksBeyondRange, oRRangeRegionColor, oRRangeRegionOpacity, marketOpen, clearDrawObjectsTime, alertSwitchedOn);
		}

		public Indicators.CI_OpeningRange CI_OpeningRange(ISeries<double> input , CustomEnumNamespace.Ranges rangeType, int entryTicks, int stopTicksBeyondRange, Brush oRRangeRegionColor, int oRRangeRegionOpacity, DateTime marketOpen, int clearDrawObjectsTime, bool alertSwitchedOn)
		{
			return indicator.CI_OpeningRange(input, rangeType, entryTicks, stopTicksBeyondRange, oRRangeRegionColor, oRRangeRegionOpacity, marketOpen, clearDrawObjectsTime, alertSwitchedOn);
		}
	}
}

#endregion
