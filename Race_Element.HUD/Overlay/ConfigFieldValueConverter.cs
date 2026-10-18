using System;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaceElement.HUD.Overlay.Configuration;

/// <summary>
/// Serializes ConfigField.Value as JSON primitives so round-trips never leave
/// JsonElement / JToken in the object. Used by OverlaySettings.JsonOptions.
/// </summary>
internal sealed class ConfigFieldValueConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(object);

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                if (reader.TryGetInt32(out int i))
                    return i;
                if (reader.TryGetInt64(out long l))
                    return l;
                if (reader.TryGetDouble(out double d))
                    return d;
                return System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.StartObject:
            case JsonTokenType.StartArray:
                // Legacy polluted payloads ({ ValueKind: 4 } / JObject). Keep text for S0 coerce.
                using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
                    return doc.RootElement.GetRawText();
            default:
                return System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case null:
                writer.WriteNullValue();
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case byte bt:
                writer.WriteNumberValue(bt);
                break;
            case sbyte sb:
                writer.WriteNumberValue(sb);
                break;
            case short s:
                writer.WriteNumberValue(s);
                break;
            case ushort us:
                writer.WriteNumberValue(us);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case uint ui:
                writer.WriteNumberValue(ui);
                break;
            case long lo:
                writer.WriteNumberValue(lo);
                break;
            case ulong ulo:
                writer.WriteNumberValue(ulo);
                break;
            case float f:
                writer.WriteNumberValue(f);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case decimal m:
                writer.WriteNumberValue(m);
                break;
            case string str:
                writer.WriteStringValue(str);
                break;
            case Enum e:
                writer.WriteStringValue(e.ToString());
                break;
            case Color c:
                writer.WriteStringValue($"Color [A={c.A}, R={c.R}, G={c.G}, B={c.B}]");
                break;
            case JsonElement je:
                Write(writer, UnwrapJsonElement(je), options);
                break;
            default:
                string typeName = value.GetType().FullName ?? string.Empty;
                if (typeName.StartsWith("Newtonsoft.Json.Linq.", StringComparison.Ordinal))
                {
                    object inner = value.GetType().GetProperty("Value")?.GetValue(value);
                    if (inner is not null)
                    {
                        Write(writer, inner, options);
                        return;
                    }
                    writer.WriteStringValue(value.ToString());
                    return;
                }

                writer.WriteStringValue(value.ToString());
                break;
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
                return element.GetRawText();
        }
    }
}