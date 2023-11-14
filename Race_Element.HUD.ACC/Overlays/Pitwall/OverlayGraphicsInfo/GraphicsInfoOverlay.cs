using RaceElement.HUD.ACC.Overlays.OverlayDebugInfo;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using static RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace RaceElement.HUD.ACC.Overlays.OverlayGraphicsInfo
{
    [Overlay(Name = "Graphics Info", Version = 1.00,
        Description = "Shared Memory Graphics Page", OverlayType = OverlayType.Pitwall)]
    internal sealed class GraphicsInfoOverlay : AbstractOverlay
    {
        private readonly DebugConfig _config = new DebugConfig();

        private GraphicsGrid _graphicsGrid;
        private readonly List<string> fieldNames = new List<string>();

        public GraphicsInfoOverlay(Rectangle rectangle) : base(rectangle, "Graphics Info")
        {
            this.AllowReposition = false;
            this.RefreshRateHz = 5;
        }

        private void Instance_WidthChanged(object sender, bool e)
        {
            if (e)
                this.X = DebugInfoHelper.Instance.GetX(this);
        }

        public sealed override void BeforeStart()
        {
            Font font = FontUtil.FontSegoeMono(8 * Scale);
            float fontHeight = font.GetHeight(120);
            int columnHeight = (int)fontHeight - 2;

            FieldInfo[] fields = pageGraphics.GetType().GetFields();

            foreach (FieldInfo member in fields)
            {
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) isObsolete = true;

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                    fieldNames.Add(member.Name);
            }
            int rows = fieldNames.Count;

            int maxNameLength = (int)Math.Ceiling(fieldNames.Max(x => x.Length) * font.SizeInPoints);

            _graphicsGrid = new GraphicsGrid(rows, 2);

            for (int row = 0; row < rows; row++)
            {
                DrawableTextCell headerCell = new DrawableTextCell(new RectangleF(0, columnHeight * row, maxNameLength, columnHeight), font);
                headerCell.CachedBackground.SetRenderer(g => { g.FillRectangle(Brushes.Black, new RectangleF(0, 0, headerCell.Rectangle.Width, headerCell.Rectangle.Height)); });
                headerCell.StringFormat.Alignment = StringAlignment.Near;
                headerCell.UpdateText(fieldNames[row]);
                _graphicsGrid.Grid[row][0] = headerCell;

                DrawableTextCell valueCell = new DrawableTextCell(new RectangleF(headerCell.Rectangle.Width, columnHeight * row, 400, columnHeight), font);
                valueCell.CachedBackground.SetRenderer(g => { g.FillRectangle(Brushes.Black, new RectangleF(0, 0, valueCell.Rectangle.Width, valueCell.Rectangle.Height)); });
                _graphicsGrid.Grid[row][1] = valueCell;
            }

            Height = rows * columnHeight;
            Width = maxNameLength + 400;

            if (this._config.Dock.Undock)
                this.AllowReposition = true;
            else
            {
                DebugInfoHelper.Instance.WidthChanged += Instance_WidthChanged;
                DebugInfoHelper.Instance.AddOverlay(this);
                this.X = DebugInfoHelper.Instance.GetX(this);
                this.Y = 0;
            }
        }


        public sealed override void BeforeStop()
        {
            if (!this._config.Dock.Undock)
            {
                DebugInfoHelper.Instance.RemoveOverlay(this);
                DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
            }
            _graphicsGrid?.Dispose();
        }

        public sealed override void Render(Graphics g)
        {
            int rowCount = 0;
            foreach (var member in pageGraphics.GetType().GetFields().Where(member => fieldNames.Contains(member.Name)))
            {
                var value = member.GetValue(pageGraphics);
                value = ReflectionUtil.FieldTypeValue(member, value);
                DrawableTextCell cell = (DrawableTextCell)_graphicsGrid.Grid[rowCount][1];
                cell.UpdateText(value.ToString());
                rowCount++;
            }

            _graphicsGrid.Draw(g, Scale);
        }

        public sealed override bool ShouldRender() => true;
    }
}
