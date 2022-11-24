using ACCManager.HUD.Overlay.Configuration;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ACCManager.Controls.Util.SetupImage;
using static ACCManager.HUD.Overlay.Configuration.OverlaySettings;
using static ACCManager.Controls.HUD.PreviewCache;
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;
using System.Collections.Generic;

namespace ACCManager.Controls.HUD
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
            // fix exception when tab is different, needs different list (debug overlays)
            ListViewItem lvi = (ListViewItem)HudOptions.Instance.listOverlays.SelectedItem;
            string overlayName = ((TextBlock)lvi.Content).Text;

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
