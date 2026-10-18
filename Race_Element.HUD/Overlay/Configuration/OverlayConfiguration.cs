using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaceElement.HUD.Overlay.Configuration;

/// <summary>
/// Provides an abstraction for an overlay configuration.
/// 
/// Constructor requires public visibility.
/// </summary>
public abstract class OverlayConfiguration
{
    [ConfigGrouping("HUD", "General settings")]
    public GenericConfig GenericConfiguration { get; set; } = new GenericConfig();
    public sealed class GenericConfig
    {
        /// <summary>Sets the Visibility of the <see cref="Scale"/>, when true it will show the Scale option in the GUI, make sure you test Scaling before enabling this option.</summary>
        public bool AllowRescale = false;

        [ToolTip("Defines the scale of the overlay.")]
        [FloatRange(0.650f, 3.000f, 0.001f, 3)]
        public float Scale { get; set; } = 1.00f;

        [ToolTip("Sets the transparency of the HUD. This will become noticeable once you active the HUD.")]
        [FloatRange(0.25f, 1f, 0.01f, 2)]
        public float Opacity { get; set; } = 1f;

        [ToolTip("Allows other software to to detect this overlay as a Window, can be used for streaming apps.")]
        public bool Window { get; set; } = false;

        [ToolTip("When streaming with Window enabled turn this off when you don't want to see the actual overlay on top of your game.")]
        public bool AlwaysOnTop { get; set; } = true;
    }

    public OverlayConfiguration()
    {
    }

    public class ConfigField
    {
        public string Name { get; set; }

        [JsonConverter(typeof(ConfigFieldValueConverter))]
        public object Value { get; set; }
    }

    public static List<ConfigField> GetConfigFields(OverlayConfiguration overlayConfiguration)
    {
        List<ConfigField> configFields = [];
        var runtimeProperties = overlayConfiguration.GetType().GetRuntimeProperties();
        foreach (PropertyInfo nested in runtimeProperties)
        {
            ConfigGroupingAttribute groupingAttribute;
            if ((groupingAttribute = nested.GetCustomAttribute<ConfigGroupingAttribute>()) != null)
            {
                var nestedValue = nested.GetValue(overlayConfiguration);
                foreach (PropertyInfo subNested in nested.PropertyType.GetRuntimeProperties())
                {
                    configFields.Add(new ConfigField() { Name = $"{nested.Name}.{subNested.Name}", Value = subNested.GetValue(nestedValue) });
                }
            }
            else
            {
                configFields.Add(new ConfigField() { Name = nested.Name, Value = nested.GetValue(overlayConfiguration) });
            }
        }

        return configFields;
    }

    /// <summary>
    /// Applies persisted config fields onto this configuration instance.
    /// Values may arrive as CLR primitives, Newtonsoft JToken, or System.Text.Json JsonElement
    /// after load/save round-trips — ConvertConfigValue normalizes them before SetValue.
    /// One bad field is logged and skipped; other fields still apply.
    /// </summary>
    internal void SetConfigFields(List<ConfigField> configFields)
    {
        if (configFields == null)
            return;

        Type type = this.GetType();
        var runtimeProperties = type.GetRuntimeProperties();

        foreach (var field in configFields)
        {
            if (field == null || string.IsNullOrEmpty(field.Name))
                continue;

            try
            {
                bool isGrouped = field.Name.Contains('.');
                if (isGrouped)
                {
                    string[] groupSplit = field.Name.Split('.');
                    if (groupSplit.Length < 2)
                        continue;

                    string groupName = groupSplit[0];
                    string propName = groupSplit[1];

                    foreach (var prop in runtimeProperties)
                    {
                        if (prop.Name != groupName)
                            continue;

                        var nestedValue = prop.GetValue(this);
                        if (nestedValue == null)
                            break;

                        foreach (PropertyInfo subNested in nestedValue.GetType().GetRuntimeProperties())
                        {
                            if (subNested.Name != propName)
                                continue;

                            object converted = ConvertConfigValue(field.Value, subNested.PropertyType);
                            if (converted != null || IsNullableTarget(subNested.PropertyType))
                                subNested.SetValue(nestedValue, converted);
                            break;
                        }
                        break;
                    }
                }
                else
                {
                    foreach (var prop in runtimeProperties)
                    {
                        if (prop.Name != field.Name)
                            continue;

                        object converted = ConvertConfigValue(field.Value, prop.PropertyType);
                        if (converted != null || IsNullableTarget(prop.PropertyType))
                            prop.SetValue(this, converted);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[OverlayConfiguration] SetConfigFields failed for '{field.Name}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Coerces a raw deserialized value to <paramref name="targetType"/>.
    /// Handles: null, CLR primitives, JsonElement (STJ), Newtonsoft JToken (via reflection),
    /// and string forms used by Color / Enum.
    /// </summary>
    private static object ConvertConfigValue(object raw, Type targetType)
    {
        if (targetType == null)
            return null;

        if (raw == null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        // Unwrap System.Text.Json.JsonElement early.
        if (raw is JsonElement element)
            raw = UnwrapJsonElement(element);

        // Unwrap Newtonsoft.Json.Linq.JToken without a hard compile-time dependency.
        raw = UnwrapNewtonsoftToken(raw);

        if (raw == null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlying == typeof(bool))
            {
                if (raw is bool b)
                    return b;
                if (raw is string sBool && bool.TryParse(sBool, out bool parsedBool))
                    return parsedBool;
                if (bool.TryParse(raw.ToString(), out parsedBool))
                    return parsedBool;
                return false;
            }

            if (underlying == typeof(float) || underlying == typeof(Single))
            {
                if (raw is float f)
                    return f;
                if (raw is double d)
                    return (float)d;
                if (raw is decimal m)
                    return (float)m;
                if (raw is int i)
                    return (float)i;
                if (float.TryParse(raw.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedF))
                    return parsedF;
                if (float.TryParse(raw.ToString(), NumberStyles.Float, CultureInfo.CurrentCulture, out parsedF))
                    return parsedF;
                return 0f;
            }

            if (underlying == typeof(int))
            {
                if (raw is int ii)
                    return ii;
                if (raw is long l)
                    return (int)l;
                if (raw is double dd)
                    return (int)dd;
                if (int.TryParse(raw.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedI))
                    return parsedI;
                return 0;
            }

            if (underlying == typeof(byte))
            {
                if (raw is byte bb)
                    return bb;
                if (byte.TryParse(raw.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out byte parsedB))
                    return parsedB;
                return (byte)0;
            }

            if (underlying == typeof(string))
                return raw as string ?? raw.ToString();

            if (underlying == typeof(Color))
                return ColorFromToStringStatic(raw.ToString());

            if (underlying.IsEnum)
            {
                if (raw.GetType() == underlying)
                    return raw;
                string name = raw.ToString();
                if (Enum.TryParse(underlying, name, ignoreCase: true, out object enumValue))
                    return enumValue;
                // Fallback: match by name in defined values
                foreach (var enumItem in Enum.GetValues(underlying))
                {
                    if (string.Equals(enumItem.ToString(), name, StringComparison.OrdinalIgnoreCase))
                        return enumItem;
                }
                return Activator.CreateInstance(underlying);
            }

            // Already correct type
            if (underlying.IsInstanceOfType(raw))
                return raw;

            return Convert.ChangeType(raw, underlying, CultureInfo.InvariantCulture);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[OverlayConfiguration] ConvertConfigValue({underlying.Name}) failed for '{raw}': {ex.Message}");
            return underlying.IsValueType ? Activator.CreateInstance(underlying) : null;
        }
    }

    private static object UnwrapJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.True:
                return true;
            case JsonValueKind.False:
                return false;
            case JsonValueKind.Number:
                if (element.TryGetInt32(out int i))
                    return i;
                if (element.TryGetInt64(out long l))
                    return l;
                if (element.TryGetDouble(out double d))
                    return d;
                return element.GetRawText();
            case JsonValueKind.String:
                return element.GetString();
            case JsonValueKind.Null:
            case JsonValueKind.Undefined:
                return null;
            default:
                // Object/Array — keep raw text so Color/Enum string paths can still try
                return element.GetRawText();
        }
    }

    /// <summary>
    /// If <paramref name="raw"/> is a Newtonsoft JToken, peel to the inner CLR value or ToString().
    /// Uses reflection so this file does not require Newtonsoft types at compile time after S1.
    /// </summary>
    private static object UnwrapNewtonsoftToken(object raw)
    {
        if (raw == null)
            return null;

        Type t = raw.GetType();
        string fullName = t.FullName ?? string.Empty;
        if (!fullName.StartsWith("Newtonsoft.Json.Linq.", StringComparison.Ordinal))
            return raw;

        // JValue exposes .Value
        PropertyInfo valueProp = t.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public);
        if (valueProp != null)
        {
            object inner = valueProp.GetValue(raw);
            if (inner != null)
                return inner;
        }

        // JObject / other tokens: ToString() is better than SetValue(token)
        return raw.ToString();
    }

    private static bool IsNullableTarget(Type targetType)
    {
        if (targetType == null)
            return false;
        if (!targetType.IsValueType)
            return true;
        return Nullable.GetUnderlyingType(targetType) != null;
    }

    public List<PropertyInfo> GetProperties()
    {
        List<PropertyInfo> properties = this.GetType().GetRuntimeProperties().ToList();
        return properties;
    }

    private System.Drawing.Color ColorFromToString(string value) => ColorFromToStringStatic(value);

    private static System.Drawing.Color ColorFromToStringStatic(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Color.Red;

        if (value.Contains("#"))
        {
            value = value.Replace("Color [", "");
            value = value.Replace("]", "");
            return (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(value);
        }

        if (value.Contains("A") && value.Contains("R") && value.Contains("G") && value.Contains("B"))
        {
            try
            {
                int a = int.Parse(value.Split('A')[1].Split(',')[0].Replace("=", ""));
                int r = int.Parse(value.Split('R')[1].Split(',')[0].Replace("=", ""));
                int g = int.Parse(value.Split('G')[1].Split(',')[0].Replace("=", ""));
                int b = int.Parse(value.Split('B')[1].Split(']')[0].Replace("=", ""));
                return System.Drawing.Color.FromArgb(a, r, g, b);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return Color.Red;
            }
        }
        else
        {
            try
            {
                string[] split = value.Split(',');
                int r = int.Parse(split[0]);
                int g = int.Parse(split[1]);
                int b = int.Parse(split[2]);
                return System.Drawing.Color.FromArgb(255, r, g, b);
            }
            catch (Exception ea)
            {
                Debug.WriteLine(ea);
                return Color.Red;
            }
        }
    }
}
