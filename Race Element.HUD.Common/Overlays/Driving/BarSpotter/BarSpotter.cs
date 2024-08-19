using RaceElement.Broadcast.Structs;
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
    
    private List<Color> colors = [];

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
        colors.Add(Color.FromArgb(_config.Colors.NormalOpacity, _config.Colors.NormalColor));
        colors.Add(Color.FromArgb(_config.Colors.NormalOpacity, _config.Colors.ThreeCarsColor));
    }

    public sealed override void BeforeStop()
    { 
    }

    public sealed override void Render(Graphics g)
    {
        if (!SimDataProvider.HasTelemetry()) return;

        if (SimDataProvider.Instance.GetType() != typeof(IRacingDataProvider))
        {
            // This HUD makes only sense if the sim provider gives the e.g. the "car left" and "three wide" callouts. This could be refactored
            // for other sims using the car world coordination, but then the radar is a better visualization.
            throw new NotImplementedException("Only supported for iRacing!");
        }        

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

        // As an example Porsche 911 and Audi R8 are 4.5 meters. MX5 however is 3.9 and Dallara 4.8.
        // We can see if this is part of metadata to make it more accurate
        float carLengthMeters = 4.5F;
                
        // cars we consider for spotting
        List<KeyValuePair<float, CarInfo>> spottableCars = [];
        

        CarInfo playerCar = SessionData.Instance.Cars[SessionData.Instance.FocusedCarIndex].Value;
        foreach (KeyValuePair<int, CarInfo> kvp in SessionData.Instance.Cars) {
            CarInfo carInfo = kvp.Value;
            
            if (carInfo == playerCar) continue;

            float distanceMeters = (carInfo.TrackPercentCompleted - playerCar.TrackPercentCompleted) * (float) SessionData.Instance.Track.Length;            
            if (Math.Abs(distanceMeters) <= carLengthMeters) {
                spottableCars.Add(KeyValuePair.Create(distanceMeters, carInfo));                
            }
        }
        var orderedSpottableCars = spottableCars.OrderBy(d => Math.Abs(d.Key));

        if (orderedSpottableCars.Count() == 0) return; 
       
        float closestCarDistance = spottableCars.First().Key;

                
        // CarLeft, CarRight: show overlap of one car clipping from start or end        
        // Use track length and fixed aberage car length to determine what the overlap is in terms of car lenths. And then either
        // show the overlap on the front or rear (on the correct side).
        int leftRectY = 0;
        int leftRectHeight= 0;
        int rightRectY = 0;
        int rightRectHeight = 0;

        float overlapMeters = carLengthMeters - Math.Abs(closestCarDistance);        
        float overlapPercent = overlapMeters / carLengthMeters;
        Color color = colors[0];
        if (spotterCallout == CarLeftRight.CarLeft)
        {
            if (closestCarDistance > 0)
            {                                
                leftRectHeight = (int)(_config.Bar.Height * this.Scale * overlapPercent);
            } else
            {                
                leftRectY = (int)(_config.Bar.Height * this.Scale * (1.0F - overlapPercent));
                leftRectHeight = (int)(_config.Bar.Height * this.Scale * overlapPercent);
            }
          
        } else if (spotterCallout == CarLeftRight.CarRight)
        {
            if (closestCarDistance > 0)
            {
                rightRectHeight = (int)(_config.Bar.Height * this.Scale * overlapPercent);                
            }
            else
            {
                rightRectY = (int)(_config.Bar.Height * this.Scale * (1.0F - overlapPercent));
                rightRectHeight = (int)(_config.Bar.Height * this.Scale * overlapPercent);
            }
        } else if (spotterCallout == CarLeftRight.Off || spotterCallout == CarLeftRight.Clear)
        {
            return;
        } else
        {
            // use three cars color and flash entire bars on both sides
            color = colors[1]; 
            leftRectHeight = (int)(_config.Bar.Height * this.Scale);            
            rightRectHeight = (int)(_config.Bar.Height * this.Scale);
        }

        Rectangle rectL = new(0, leftRectY, (int)(_config.Bar.Width * this.Scale), leftRectHeight);
        using HatchBrush hatchBrushL = new(HatchStyle.LightUpwardDiagonal, color, Color.FromArgb(color.A - 40, color));
        g.FillRectangle(hatchBrushL, rectL);
        Rectangle rectR = new(_config.Bar.Distance, rightRectY, (int)(_config.Bar.Width * this.Scale), rightRectHeight);
        using HatchBrush hatchBrushR = new(HatchStyle.LightUpwardDiagonal, color, Color.FromArgb(color.A - 40, color));
        g.FillRectangle(hatchBrushL, rectR);
    }
}