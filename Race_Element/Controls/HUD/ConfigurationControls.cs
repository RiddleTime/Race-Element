using RaceElement.HUD.Overlay.Configuration;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using RaceElement.Controls.Util.SetupImage;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;
using static RaceElement.Controls.HUD.PreviewCache;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;
using System.Collections.Generic;

namespace RaceElement.Controls.HUD
{
    internal class ConfigurationControls
    {
        internal static void SaveOverlayConfigFields(string overlayName, List<ConfigField> configFields)
        {
            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlayName);
            if (settings == null)
            {
                int screenMiddleX = (int)(SystemParameters.PrimaryScreenHeight / 2);
                int screenMiddleY = (int)(SystemParameters.PrimaryScreenHeight / 2);
                settings = new OverlaySettingsJson() { X = screenMiddleX, Y = screenMiddleY };
            }

            settings.Config = configFields;

            OverlaySettings.SaveOverlaySettings(overlayName, settings);

            // update preview image
            if (HudOptions.Instance.listOverlays.SelectedIndex >= 0)
            {
                ListViewItem lvi = (ListViewItem)HudOptions.Instance.listOverlays.SelectedItem;
                TextBlock tb = (TextBlock)lvi.Content;
                string actualOverlayName = overlayName.Replace("Overlay", "").Trim();
                if (tb.Text.Equals(actualOverlayName))
                {
                    PreviewCache.GeneratePreview(actualOverlayName);
                    PreviewCache._cachedPreviews.TryGetValue(actualOverlayName, out PreviewCache.CachedPreview preview);
                    if (preview != null)
                    {
                        HudOptions.Instance.previewImage.Stretch = Stretch.UniformToFill;
                        HudOptions.Instance.previewImage.Width = preview.Width;
                        HudOptions.Instance.previewImage.Height = preview.Height;
                        HudOptions.Instance.previewImage.Source = ImageControlCreator.CreateImage(preview.Width, preview.Height, preview.CachedBitmap).Source;
                    }
                    else
                    {
                        HudOptions.Instance.previewImage.Source = null;
                    }
                }
            }
        }


        internal static void SaveOverlayConfigField(ConfigField configField)
        {
            string overlayName = HudOptions.Instance.GetCurrentlyViewedOverlayName();

            OverlaySettingsJson settings = OverlaySettings.LoadOverlaySettings(overlayName);
            if (settings == null)
            {
                int screenMiddleX = (int)(SystemParameters.PrimaryScreenHeight / 2);
                int screenMiddleY = (int)(SystemParameters.PrimaryScreenHeight / 2);
                settings = new OverlaySettingsJson() { X = screenMiddleX, Y = screenMiddleY, Config = OverlayConfiguration.GetConfigFields(HudOptions.Instance.overlayConfig) };
            }

            ConfigField field = null;
            if (settings.Config == null)
                settings.Config = OverlayConfiguration.GetConfigFields(HudOptions.Instance.overlayConfig);

            field = settings.Config.Find(x => x.Name == configField.Name);
            if (field == null)
                settings.Config.Add(configField);
            else
            {
                settings.Config.Remove(field);
                field = configField;
                settings.Config.Add(configField);
            }

            OverlaySettings.SaveOverlaySettings(overlayName, settings);

            // update preview image
            if (HudOptions.Instance.listOverlays.SelectedIndex >= 0)
            {


                PreviewCache.GeneratePreview(overlayName);
                PreviewCache._cachedPreviews.TryGetValue(overlayName, out CachedPreview preview);
                if (preview != null)
                {
                    HudOptions.Instance.previewImage.Stretch = Stretch.UniformToFill;
                    HudOptions.Instance.previewImage.Width = preview.Width;
                    HudOptions.Instance.previewImage.Height = preview.Height;
                    HudOptions.Instance.previewImage.Source = ImageControlCreator.CreateImage(preview.Width, preview.Height, preview.CachedBitmap).Source;
                }
                else
                {
                    HudOptions.Instance.previewImage.Source = null;
                }

            }
        }


    }
}
