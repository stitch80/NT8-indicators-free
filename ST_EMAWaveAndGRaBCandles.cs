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
	public class ST_EMAWaveAndGRaBCandles : Indicator
	{
		private string eightThirteenTwentyOneStatus;
		private string waveStatus;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Wave of 34 EMAs and GRaB Candles";
				Name										= "ST EMA Wave And GRaB Candles";
				Calculate									= Calculate.OnPriceChange;
				PaintPriceMarkers							= false;
				IsOverlay									= true;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				EMAPeriod							= 34;
				
				GreenUpCandleColor					= Brushes.Lime;
				GreenDownCandleColor				= Brushes.DarkGreen;
				RedUpCandleColor					= Brushes.Red;
				RedDownCandleColor					= Brushes.Maroon;
				BlueUpCandleColor					= Brushes.DodgerBlue;
				BlueDownCandleColor					= Brushes.Navy;
				
				AddPlot(Brushes.Green, "EMA34High");
				AddPlot(Brushes.Blue, "EMA34Close");
				AddPlot(Brushes.Red, "EMA34Low");
			}
			else if (State == State.Configure)
			{
				this.Name = "";
				eightThirteenTwentyOneStatus = "";
				waveStatus = "";
			}
		}

		protected override void OnBarUpdate()
		{
			double EMA34HighValue = EMA(High, EMAPeriod)[0];
			double EMA34CloseValue = EMA(Close, EMAPeriod)[0];
			double EMA34LowValue = EMA(Low, EMAPeriod)[0];
			
			EMA34High[0] = EMA34HighValue;
			EMA34Close[0] = EMA34CloseValue;
			EMA34Low[0] = EMA34LowValue;
			
			if (Close[0] > EMA34HighValue && Open[0] < Close[0]) {
				BarBrush = GreenUpCandleColor;
			} else if (Close[0] > EMA34HighValue && Open[0] >= Close[0]) {
				BarBrush = GreenDownCandleColor;
			} else if (Close[0] < EMA34LowValue && Open[0] < Close[0]) {
				BarBrush = RedUpCandleColor;
			} else if (Close[0] < EMA34LowValue && Open[0] >= Close[0]) {
				BarBrush = RedDownCandleColor;
			} else if (Open[0] < Close[0]) {
				BarBrush = BlueUpCandleColor;
			} else if (Open[0] >= Close[0]) {
				BarBrush = BlueDownCandleColor;
			} else {
				BarBrush = BlueDownCandleColor;
			}
			
			if(EMA(8)[0] > EMA(13)[0] && EMA(13)[0] > EMA(21)[0]) {
				eightThirteenTwentyOneStatus = "bullish";
			} else if (EMA(8)[0] < EMA(13)[0] && EMA(13)[0] < EMA(21)[0]) {
				eightThirteenTwentyOneStatus = "bearish";
			} else {
				eightThirteenTwentyOneStatus = "slop";
			}
			
			if(EMA(8)[0] > EMA(13)[0] && EMA(13)[0] > EMA(21)[0] && EMA(21)[0] > EMA(High, 34)[0]) {
				waveStatus = "bullish";
			} else if (EMA(8)[0] < EMA(13)[0] && EMA(13)[0] < EMA(21)[0] && EMA(21)[0] < EMA(Low,34)[0]) {
				waveStatus = "bearish";
			} else {
				waveStatus = "sideways";
			}
			
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			
			base.OnRender(chartControl, chartScale);
			
			
			SharpDX.Direct2D1.Brush textBrushDx = System.Windows.Media.Brushes.Black.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush eightThirteenTwentyOneAreaBrushDx = System.Windows.Media.Brushes.Transparent.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush waveAreaBrushDx = System.Windows.Media.Brushes.Transparent.ToDxBrush(RenderTarget);
			SharpDX.Direct2D1.Brush areaBorderDx = System.Windows.Media.Brushes.Gray.ToDxBrush(RenderTarget);
			
			NinjaTrader.Gui.Tools.SimpleFont simpleFont = chartControl.Properties.LabelFont ??  new NinjaTrader.Gui.Tools.SimpleFont();
			SharpDX.DirectWrite.TextFormat textFormat = simpleFont.ToDirectWriteTextFormat();
			
			//Print(eightThirteenTwentyOneStatus);
			
			if(eightThirteenTwentyOneStatus.Equals("bullish")) {
				eightThirteenTwentyOneAreaBrushDx = System.Windows.Media.Brushes.Green.ToDxBrush(RenderTarget);
			} else if (eightThirteenTwentyOneStatus.Equals("bearish")) {
				eightThirteenTwentyOneAreaBrushDx = System.Windows.Media.Brushes.Red.ToDxBrush(RenderTarget);
			} else {
				eightThirteenTwentyOneAreaBrushDx = System.Windows.Media.Brushes.Yellow.ToDxBrush(RenderTarget);
			}
			
			if(waveStatus.Equals("bullish")) {
				waveAreaBrushDx = System.Windows.Media.Brushes.Green.ToDxBrush(RenderTarget);
			} else if (waveStatus.Equals("bearish")) {
				waveAreaBrushDx = System.Windows.Media.Brushes.Red.ToDxBrush(RenderTarget);
			} else {
				waveAreaBrushDx = System.Windows.Media.Brushes.Yellow.ToDxBrush(RenderTarget);
			}
			
			//SetZOrder(int.MinValue);
			//8:13:21 Label
			SharpDX.Vector2 eightThirteenTwentyOneLabel = new SharpDX.Vector2(ChartPanel.X + 15, ChartPanel.Y + 50);
			SharpDX.DirectWrite.TextLayout textLayout1 =
				new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
					"8:13:21: " + eightThirteenTwentyOneStatus, textFormat, ChartPanel.X + ChartPanel.W, textFormat.FontSize);
			SharpDX.RectangleF rect1 = new SharpDX.RectangleF(eightThirteenTwentyOneLabel.X - 5, eightThirteenTwentyOneLabel.Y - 5,
				textLayout1.Metrics.Width + 10, textLayout1.Metrics.Height + 10);
			RenderTarget.FillRectangle(rect1, eightThirteenTwentyOneAreaBrushDx);
			RenderTarget.DrawRectangle(rect1, areaBorderDx, 2);
			RenderTarget.DrawTextLayout(eightThirteenTwentyOneLabel, textLayout1, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			
			//Wave Label
			
			SharpDX.Vector2 waveLabel = new SharpDX.Vector2(eightThirteenTwentyOneLabel.X + textLayout1.Metrics.Width + 15, ChartPanel.Y + 50);
			SharpDX.DirectWrite.TextLayout textLayout2 =
				new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
					"Wave: " + waveStatus, textFormat, ChartPanel.X + ChartPanel.W, textFormat.FontSize);
			SharpDX.RectangleF rect2 = new SharpDX.RectangleF(waveLabel.X - 5, waveLabel.Y - 5,
				textLayout2.Metrics.Width + 10, textLayout2.Metrics.Height + 10);
			RenderTarget.FillRectangle(rect2, waveAreaBrushDx);
			RenderTarget.DrawRectangle(rect2, areaBorderDx, 2);
			RenderTarget.DrawTextLayout(waveLabel, textLayout2, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			
			
			
			eightThirteenTwentyOneAreaBrushDx.Dispose();
			waveAreaBrushDx.Dispose();
			areaBorderDx.Dispose();
			textBrushDx.Dispose();
			textFormat.Dispose();
			textLayout1.Dispose();
			textLayout2.Dispose();
	
		}
		

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EMAPeriod", Order=1, GroupName="Parameters")]
		public int EMAPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="GreenUpCandleColor", Order=2, GroupName="Parameters")]
		public Brush GreenUpCandleColor
		{ get; set; }

		[Browsable(false)]
		public string GreenUpCandleColorSerializable
		{
			get { return Serialize.BrushToString(GreenUpCandleColor); }
			set { GreenUpCandleColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="GreenDownCandleColor", Order=3, GroupName="Parameters")]
		public Brush GreenDownCandleColor
		{ get; set; }

		[Browsable(false)]
		public string GreenDownCandleColorSerializable
		{
			get { return Serialize.BrushToString(GreenDownCandleColor); }
			set { GreenDownCandleColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="RedUpCandleColor", Order=4, GroupName="Parameters")]
		public Brush RedUpCandleColor
		{ get; set; }

		[Browsable(false)]
		public string RedUpCandleColorSerializable
		{
			get { return Serialize.BrushToString(RedUpCandleColor); }
			set { RedUpCandleColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="RedDownCandleColor", Order=5, GroupName="Parameters")]
		public Brush RedDownCandleColor
		{ get; set; }

		[Browsable(false)]
		public string RedDownCandleColorSerializable
		{
			get { return Serialize.BrushToString(RedDownCandleColor); }
			set { RedDownCandleColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BlueUpCandleColor", Order=6, GroupName="Parameters")]
		public Brush BlueUpCandleColor
		{ get; set; }

		[Browsable(false)]
		public string BlueUpCandleColorSerializable
		{
			get { return Serialize.BrushToString(BlueUpCandleColor); }
			set { BlueUpCandleColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BlueDownCandleColor", Order=7, GroupName="Parameters")]
		public Brush BlueDownCandleColor
		{ get; set; }

		[Browsable(false)]
		public string BlueDownCandleColorSerializable
		{
			get { return Serialize.BrushToString(BlueDownCandleColor); }
			set { BlueDownCandleColor = Serialize.StringToBrush(value); }
		}			

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA34High
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA34Close
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> EMA34Low
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ST_EMAWaveAndGRaBCandles[] cacheST_EMAWaveAndGRaBCandles;
		public ST_EMAWaveAndGRaBCandles ST_EMAWaveAndGRaBCandles(int eMAPeriod, Brush greenUpCandleColor, Brush greenDownCandleColor, Brush redUpCandleColor, Brush redDownCandleColor, Brush blueUpCandleColor, Brush blueDownCandleColor)
		{
			return ST_EMAWaveAndGRaBCandles(Input, eMAPeriod, greenUpCandleColor, greenDownCandleColor, redUpCandleColor, redDownCandleColor, blueUpCandleColor, blueDownCandleColor);
		}

		public ST_EMAWaveAndGRaBCandles ST_EMAWaveAndGRaBCandles(ISeries<double> input, int eMAPeriod, Brush greenUpCandleColor, Brush greenDownCandleColor, Brush redUpCandleColor, Brush redDownCandleColor, Brush blueUpCandleColor, Brush blueDownCandleColor)
		{
			if (cacheST_EMAWaveAndGRaBCandles != null)
				for (int idx = 0; idx < cacheST_EMAWaveAndGRaBCandles.Length; idx++)
					if (cacheST_EMAWaveAndGRaBCandles[idx] != null && cacheST_EMAWaveAndGRaBCandles[idx].EMAPeriod == eMAPeriod && cacheST_EMAWaveAndGRaBCandles[idx].GreenUpCandleColor == greenUpCandleColor && cacheST_EMAWaveAndGRaBCandles[idx].GreenDownCandleColor == greenDownCandleColor && cacheST_EMAWaveAndGRaBCandles[idx].RedUpCandleColor == redUpCandleColor && cacheST_EMAWaveAndGRaBCandles[idx].RedDownCandleColor == redDownCandleColor && cacheST_EMAWaveAndGRaBCandles[idx].BlueUpCandleColor == blueUpCandleColor && cacheST_EMAWaveAndGRaBCandles[idx].BlueDownCandleColor == blueDownCandleColor && cacheST_EMAWaveAndGRaBCandles[idx].EqualsInput(input))
						return cacheST_EMAWaveAndGRaBCandles[idx];
			return CacheIndicator<ST_EMAWaveAndGRaBCandles>(new ST_EMAWaveAndGRaBCandles(){ EMAPeriod = eMAPeriod, GreenUpCandleColor = greenUpCandleColor, GreenDownCandleColor = greenDownCandleColor, RedUpCandleColor = redUpCandleColor, RedDownCandleColor = redDownCandleColor, BlueUpCandleColor = blueUpCandleColor, BlueDownCandleColor = blueDownCandleColor }, input, ref cacheST_EMAWaveAndGRaBCandles);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ST_EMAWaveAndGRaBCandles ST_EMAWaveAndGRaBCandles(int eMAPeriod, Brush greenUpCandleColor, Brush greenDownCandleColor, Brush redUpCandleColor, Brush redDownCandleColor, Brush blueUpCandleColor, Brush blueDownCandleColor)
		{
			return indicator.ST_EMAWaveAndGRaBCandles(Input, eMAPeriod, greenUpCandleColor, greenDownCandleColor, redUpCandleColor, redDownCandleColor, blueUpCandleColor, blueDownCandleColor);
		}

		public Indicators.ST_EMAWaveAndGRaBCandles ST_EMAWaveAndGRaBCandles(ISeries<double> input , int eMAPeriod, Brush greenUpCandleColor, Brush greenDownCandleColor, Brush redUpCandleColor, Brush redDownCandleColor, Brush blueUpCandleColor, Brush blueDownCandleColor)
		{
			return indicator.ST_EMAWaveAndGRaBCandles(input, eMAPeriod, greenUpCandleColor, greenDownCandleColor, redUpCandleColor, redDownCandleColor, blueUpCandleColor, blueDownCandleColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ST_EMAWaveAndGRaBCandles ST_EMAWaveAndGRaBCandles(int eMAPeriod, Brush greenUpCandleColor, Brush greenDownCandleColor, Brush redUpCandleColor, Brush redDownCandleColor, Brush blueUpCandleColor, Brush blueDownCandleColor)
		{
			return indicator.ST_EMAWaveAndGRaBCandles(Input, eMAPeriod, greenUpCandleColor, greenDownCandleColor, redUpCandleColor, redDownCandleColor, blueUpCandleColor, blueDownCandleColor);
		}

		public Indicators.ST_EMAWaveAndGRaBCandles ST_EMAWaveAndGRaBCandles(ISeries<double> input , int eMAPeriod, Brush greenUpCandleColor, Brush greenDownCandleColor, Brush redUpCandleColor, Brush redDownCandleColor, Brush blueUpCandleColor, Brush blueDownCandleColor)
		{
			return indicator.ST_EMAWaveAndGRaBCandles(input, eMAPeriod, greenUpCandleColor, greenDownCandleColor, redUpCandleColor, redDownCandleColor, blueUpCandleColor, blueDownCandleColor);
		}
	}
}

#endregion
