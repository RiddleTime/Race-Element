using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil.ProgressBars;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RaceElement.HUD.Common.Overlays.Driving.PushToPass;


[Overlay(
    Name = "Push To Pass",
    Description = "Shows the amount of energy left for push to pass.\n(In AMS2 this is called Temporary Boost)",
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Inputs,
    SupportedGames = Game.Automobilista2,
    Authors = ["Reinier Klarenberg"]
    )]
internal sealed class PushToPassOverlay : CommonAbstractOverlay
{
    private readonly PushToPassConfiguration _config = new();
    private sealed class PushToPassConfiguration : OverlayConfiguration
    {
        public PushToPassConfiguration()
        {
            this.GenericConfiguration.AllowRescale = true;
        }
    }

    private HorizontalProgressBar _bar;

    public PushToPassOverlay(Rectangle rectangle) : base(rectangle, "Push To Pass")
    {
    }


    public override void BeforeStart()
    {
        int width = 100, height = 50;

        this.Width = width;
        this.Height = height;
        _bar = new(width, height)
        {
            Min = 0f,
            Max = 100f,
            Value = 0f,
            Rounded = true,
        };
    }

    public override void BeforeStop()
    {
        _bar?.DisposeBitmaps();
    }

    public override void Render(Graphics g)
    {
        _bar.Value = SimDataProvider.LocalCar.Electronics.PushToPassLevel;
        
        _bar.Draw(g, 0, 0);
    }
}
