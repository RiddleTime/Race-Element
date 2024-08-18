using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using static RaceElement.Data.Games.iRacing.SDK.IRacingSdkEnum;
using CarInfo = RaceElement.Data.Common.SimulatorData.CarInfo;

namespace RaceElement.HUD.Common.Overlays.OverlayBarSpotter;

[Overlay(Name = "Bar Spotter",
    Description = "Spotter indicating overlap with other drivers with bars. (BETA)",
    Version = 1.00,
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Driving,
Authors = ["Dirk Wolf"])]
internal sealed class BarSpotter : CommonAbstractOverlay
{
    private readonly BarSpotterConfiguration _config = new();
    
    //private CachedBitmap _cachedBackground;       
    private Color _color;
    private CachedBitmap _cachedColorBar;

    private float previewClosestCarDistance = -3.0F;
    private CarLeftRight previewSpotterCallout = CarLeftRight.CarLeft;

    public BarSpotter(Rectangle rectangle) : base(rectangle, "Bar Spotter")
    {
        this.RefreshRateHz = this._config.Bar.RefreshRate;
        this.Height = _config.Bar.Height + 1;
        // We need width for 2 bars "Distance" apart
        this.Width = (2 * _config.Bar.Width + _config.Bar.Distance) + 1;
    }

    public sealed override void SetupPreviewData()
    {
        SimDataProvider.Instance.SetupPreviewData();
    }

    public sealed override void BeforeStart()
    {                
        int cornerRadius = (int)(10 * this.Scale);
        _color = Color.FromArgb(_config.Colors.NormalOpacity, _config.Colors.NormalColor);
        
        /*
        _cachedColorBar = new CachedBitmap((int)(Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
            {
                Rectangle rectL = new(0, 1, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale));
                using HatchBrush hatchBrushL = new(HatchStyle.LightUpwardDiagonal, _color, Color.FromArgb(_color.A - 40, _color));
                g.FillRoundedRectangle(hatchBrushL, rectL, cornerRadius);
                Rectangle rectR = new(_config.Bar.Distance, 1, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale));
                using HatchBrush hatchBrushR = new(HatchStyle.LightUpwardDiagonal, _color, Color.FromArgb(_color.A - 40, _color));
                g.FillRoundedRectangle(hatchBrushL, rectR, cornerRadius);
            });
                
        
        _cachedBackground = new CachedBitmap((int)(Width * this.Scale + 1), (int)(_config.Bar.Height * this.Scale + 1), g =>
        {
            int midHeight = (int)(_config.Bar.Height * this.Scale) / 2;
            using LinearGradientBrush linerBrush = new(new Point(0, midHeight), new Point((int)(Width * this.Scale), midHeight), Color.FromArgb(160, 0, 0, 0), Color.FromArgb(230, 0, 0, 0));
            g.FillRoundedRectangle(linerBrush, new Rectangle(0, 0, (int)(Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
            using Pen pen = new(Color.Black, 1 * this.Scale);
            g.DrawRoundedRectangle(pen, new Rectangle(0, 0, (int)(Width * this.Scale), (int)(_config.Bar.Height * this.Scale)), cornerRadius);
        });        */
    }

    public sealed override void BeforeStop()
    {
        //_cachedBackground?.Dispose();        

        if (_cachedColorBar != null)
            _cachedColorBar.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        if (!SimDataProvider.HasTelemetry()) return;

        if (SimDataProvider.Instance.GetType() != typeof(IRacingDataProvider))
        {
            // TODO: can we give users error popups or make this only selectable for iRacing?
            Debug.WriteLine("Only supported for iRacing!");
            return;
        }

        //_cachedBackground?.Draw(g, _config.Bar.Width, _config.Bar.Height);

        DrawSpotterBars(g);

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.TextContrast = 1;
    }


    private void DrawSpotterBars(Graphics g)
    {
        IRacingDataProvider racingDataProvider = (IRacingDataProvider) SimDataProvider.Instance;
        CarLeftRight spotterCallout = racingDataProvider.GetSpotterCallout();
        
        // Off, Clear -> Nothing to show
        if (spotterCallout == CarLeftRight.Off || spotterCallout == CarLeftRight.Clear) return;               
        
        // Porsche 911 and Audi R8 are 4.5 meters. MX5 however is 3.9 and Dallara 4.8. We can see if this is part of metadata.
        float carLengthMeters = 5.0F;
                
        // cars we consider for spotting
        List<KeyValuePair<float, CarInfo>> spottableCars = [];

        CarInfo playerCar = SessionData.Instance.Cars[SessionData.Instance.FocusedCarIndex].Value;
        foreach (KeyValuePair<int, CarInfo> kvp in SessionData.Instance.Cars) {
            CarInfo carInfo = kvp.Value;
            
            if (carInfo == playerCar) continue;

            float distanceMeters = (carInfo.TrackPercentCompleted - playerCar.TrackPercentCompleted) * (float) SessionData.Instance.Track.Length;

            if (Math.Abs(distanceMeters) <= carLengthMeters / 2.0) {
                spottableCars.Add(KeyValuePair.Create(distanceMeters, carInfo));                
            }
        }
        var orderedSpottableCars = spottableCars.OrderBy(d => Math.Abs(d.Key));

        if (orderedSpottableCars.Count() == 0) return; 
       
        float closestCarDistance = spottableCars.First().Key;

        
        /* TODO preview doesn't work if (IsPreviewing)
         {
            closestCarDistance = previewClosestCarDistance;
            previewClosestCarDistance += 0.1F;
            spotterCallout = previewSpotterCallout;
            if (previewClosestCarDistance > 3.0F)
            {
                previewClosestCarDistance = -3.0F;
            previewSpotterCallout = previewSpotterCallout == CarLeftRight.CarLeft ? CarLeftRight.CarRight : CarLeftRight.CarLeft;
            }
        } */
        
        // CarLeft, CarRight: show overlap of one car clipping from start or end        
        // Use track length and fixed aberage car length to determine what the overlap is in terms of car lenths. And then either
        // show the overlap on the front or rear (on the correct side).
        int leftRectY = 0;
        int leftRectHeight= 0;
        int rightRectY = 0;
        int rightRectHeight = 0;
        float overlapMeters = (carLengthMeters / 2.0F - Math.Abs(closestCarDistance)) / carLengthMeters * 2.0F;
        if (spotterCallout == CarLeftRight.CarLeft)
        {
            if (closestCarDistance > 0)
            {                                
                leftRectHeight = (int)(_config.Bar.Height * this.Scale * overlapMeters);
            } else
            {                
                leftRectY = (int)(_config.Bar.Height * this.Scale * (1.0F - overlapMeters));
                leftRectHeight = (int)(_config.Bar.Height * this.Scale * overlapMeters);
            }
          
        } else if (spotterCallout == CarLeftRight.CarRight)
        {
            if (closestCarDistance > 0)
            {
                rightRectHeight = (int)(_config.Bar.Height * this.Scale * overlapMeters);                
            }
            else
            {
                rightRectY = (int)(_config.Bar.Height * this.Scale * (1.0F - overlapMeters));
                rightRectHeight = (int)(_config.Bar.Height * this.Scale * overlapMeters);
            }
        } else if (spotterCallout == CarLeftRight.Off || spotterCallout == CarLeftRight.Clear)
        {
            return;
        } else
        {
            // CarLeftRight: Show full bars in different color like iOverlay? We can figure out overlap, but don't know whether cars are left or right. https://youtu.be/RTWNssC6tQY?t=107 TODO: remove 
            throw new NotImplementedException("Spotter callout handling for " + spotterCallout + " not implemented yet.");
        }
        Debug.WriteLine("Car {0}  distance {1} overlap {2}  leftRectY{3} leftRectHeight{4} rightRectY{5} rightRectHeight{6} spotted {7}", spottableCars.First().Value.RaceNumber, closestCarDistance, overlapMeters,
            leftRectY, leftRectHeight, rightRectY, rightRectHeight, spotterCallout);


        /*        
        // TODO: calculate these using bar height
        int leftClipYStart = 0;
        int leftClipYEnd = _config.Bar.Height;
        int rightClipYStart = 0;
        int rightClipYEnd = _config.Bar.Height;

        g.SetClip(new Rectangle(0, leftClipYStart, _config.Bar.Width, leftClipYEnd));
        g.SetClip(new Rectangle(_config.Bar.Distance, rightClipYStart, _config.Bar.Width, rightClipYEnd), CombineMode.Union);
        // _cachedColorBar.Draw(g, 1, 0, Width - 1, _config.Bar.Height - 1);

        
        Rectangle rectL = new(0, 1, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale));
        using HatchBrush hatchBrushL = new(HatchStyle.LightUpwardDiagonal, _color, Color.FromArgb(_color.A - 40, _color));
        g.FillRoundedRectangle(hatchBrushL, rectL, cornerRadius);
        Rectangle rectR = new(_config.Bar.Distance, 1, (int)(_config.Bar.Width * this.Scale), (int)(_config.Bar.Height * this.Scale));
        using HatchBrush hatchBrushR = new(HatchStyle.LightUpwardDiagonal, _color, Color.FromArgb(_color.A - 40, _color));
        g.FillRoundedRectangle(hatchBrushL, rectR, cornerRadius);
        */

        // TODO: I could not get the cachged bitmaps and clipping above to work. Using drawn rectanges for now until I get help
        int cornerRadius = (int)(10 * this.Scale);
        Rectangle rectL = new(0, leftRectY, (int)(_config.Bar.Width * this.Scale), leftRectHeight);
        using HatchBrush hatchBrushL = new(HatchStyle.LightUpwardDiagonal, _color, Color.FromArgb(_color.A - 40, _color));
        g.FillRectangle(hatchBrushL, rectL);
        Rectangle rectR = new(_config.Bar.Distance, rightRectY, (int)(_config.Bar.Width * this.Scale), rightRectHeight);
        using HatchBrush hatchBrushR = new(HatchStyle.LightUpwardDiagonal, _color, Color.FromArgb(_color.A - 40, _color));
        g.FillRectangle(hatchBrushL, rectR);
    }
}