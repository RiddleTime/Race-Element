using RaceElement.Controls.Util.SetupImage;
using RaceElement.Data.Games;
using RaceElement.HUD.ACC;
using RaceElement.HUD.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RaceElement.Controls.HUD;

internal class PreviewCache
{
    internal static readonly Dictionary<string, CachedPreview> _cachedPreviews = [];
    private static readonly object[] DefaultOverlayArgs =
    [
        new System.Drawing.Rectangle(
            (int)SystemParameters.PrimaryScreenWidth / 2,
            (int)SystemParameters.PrimaryScreenHeight / 2,
            300, 150)
    ];

    public class CachedPreview
    {
        public int Width;
        public int Height;
        public CachedBitmap CachedBitmap;
    }

    /// <summary>
    /// Drops all cached previews and disposes bitmaps.
    /// Call after profile apply or game change so the next BuildOverlayPanel regenerates from current config.
    /// </summary>
    internal static void Clear()
    {
        foreach (var kv in _cachedPreviews)
        {
            try { kv.Value?.CachedBitmap?.Dispose(); }
            catch { /* ignore */ }
        }
        _cachedPreviews.Clear();
    }

    /// <summary>Drop one HUD preview so the next GeneratePreview redraws it.</summary>
    internal static void Invalidate(string overlayName)
    {
        if (string.IsNullOrWhiteSpace(overlayName))
            return;

        if (_cachedPreviews.TryGetValue(overlayName, out CachedPreview preview))
        {
            try { preview?.CachedBitmap?.Dispose(); }
            catch { /* ignore */ }
            _cachedPreviews.Remove(overlayName);
        }
    }

    internal static void UpdatePreviewImage(ListView listOverlays, Image previewImage, string overlayName)
    {
        if (listOverlays.SelectedIndex < 0)
            return;

        ListViewItem lvi = (ListViewItem)listOverlays.SelectedItem;
        TextBlock tb = (TextBlock)lvi.Content;
        string actualOverlayName = overlayName.Replace("Overlay", "").Trim();
        if (!tb.Text.Equals(actualOverlayName))
            return;

        GeneratePreview(actualOverlayName);
        if (_cachedPreviews.TryGetValue(actualOverlayName, out CachedPreview preview) && preview is not null)
        {
            previewImage.Stretch = Stretch.UniformToFill;
            previewImage.Width = preview.Width;
            previewImage.Height = preview.Height;
            previewImage.Source = ImageControlCreator.CreateImage(preview.Width + 1, preview.Height + 1, preview.CachedBitmap).Source;
        }
        else
        {
            previewImage.Source = null;
        }
    }

    /// <param name="cached">If true, keep an existing cache entry; if false, always redraw.</param>
    internal static void GeneratePreview(string overlayName, bool cached = false)
    {
        if (cached && _cachedPreviews.ContainsKey(overlayName))
            return;

        Type overlayType;
        if (GameManager.CurrentGame != Game.AssettoCorsaCompetizione)
            CommonHuds.AbstractOverlays.TryGetValue(overlayName, out overlayType);
        else
            OverlaysAcc.AbstractOverlays.TryGetValue(overlayName, out overlayType);

        if (overlayType is null)
            return;

        CommonAbstractOverlay abstractOverlay;
        try
        {
            abstractOverlay = (CommonAbstractOverlay)Activator.CreateInstance(overlayType, DefaultOverlayArgs);
        }
        catch (Exception)
        {
            return;
        }

        if (abstractOverlay is AbstractOverlay overlay)
        {
            overlay.pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(false);
            overlay.pageGraphics.NumberOfLaps = 30;
            overlay.pageGraphics.FuelXLap = 3.012f;
            overlay.pageGraphics.SessionType = ACCSharedMemory.AcSessionType.AC_RACE;
            overlay.pageGraphics.Status = ACCSharedMemory.AcStatus.AC_LIVE;
            overlay.pageGraphics.MandatoryPitDone = false;
            overlay.pageGraphics.NormalizedCarPosition = 0.0472972f;
            overlay.pageGraphics.IsValidLap = true;
            overlay.pageGraphics.WindDirection = 0.1f;
            overlay.pageGraphics.WindSpeed = 16.92f;
            overlay.pageGraphics.ExhaustTemperature = 325.24f;
            overlay.pageGraphics.currentTyreSet = 3;

            overlay.pagePhysics = ACCSharedMemory.Instance.ReadPhysicsPageFile(false);
            overlay.pagePhysics.SpeedKmh = 272.32f;
            overlay.pagePhysics.Fuel = 76.07f;
            overlay.pagePhysics.Rpms = 8500;
            overlay.pagePhysics.Gear = 3;
            overlay.pagePhysics.WheelPressure = [27.61f, 27.56f, 26.94f, 26.13f];
            overlay.pagePhysics.TyreCoreTemperature = [102.67f, 88.51f, 74.92f, 67.23f];
            overlay.pagePhysics.PadLife = [24f, 24f, 25f, 25f];
            overlay.pagePhysics.BrakeTemperature = [300f, 250f, 450f, 460f];
            overlay.pagePhysics.Gas = 0.78f;
            overlay.pagePhysics.Brake = 0.133f;
            overlay.pagePhysics.SteerAngle = 0.053f;
            overlay.pagePhysics.BrakeBias = 0.88f;
            overlay.pagePhysics.WaterTemp = 98.3f;
            overlay.pagePhysics.RoadTemp = 29.826f;
            overlay.pagePhysics.AirTemp = 36.2326f;

            overlay.pageStatic = ACCSharedMemory.Instance.ReadStaticPageFile(false);
            overlay.pageStatic.MaxFuel = 120f;
            overlay.pageStatic.MaxRpm = 9250;
            overlay.pageStatic.CarModel = "porsche_991ii_gt3_r";
        }

        abstractOverlay.SetupPreviewData();
        abstractOverlay.IsPreviewing = true;

        try
        {
            abstractOverlay.BeforeStart();
            CachedPreview cachedPreview = new()
            {
                Width = abstractOverlay.Width,
                Height = abstractOverlay.Height,
                CachedBitmap = new CachedBitmap(abstractOverlay.Width, abstractOverlay.Height, g => abstractOverlay.Render(g))
            };

            if (_cachedPreviews.ContainsKey(overlayName))
            {
                try { _cachedPreviews[overlayName]?.CachedBitmap?.Dispose(); }
                catch { /* ignore */ }
                _cachedPreviews[overlayName] = cachedPreview;
            }
            else
            {
                _cachedPreviews.Add(overlayName, cachedPreview);
            }

            abstractOverlay.BeforeStop();
        }
        catch (Exception) { }
        finally
        {
            abstractOverlay.Dispose();
        }
    }
}