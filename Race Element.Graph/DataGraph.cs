using System.Collections.Concurrent;

using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;

using RaceElement.Graph.Edge;
using RaceElement.Graph.Node;

namespace RaceElement.Graph;

public sealed class DataGraph : ConcurrentBag<AbstractNode>
{
    public ConcurrentBag<AbstractEdge> Edges { get; private set; } = [];

    /// <summary>
    /// Empties the data graph.
    /// </summary>
    public void ClearGraph()
    {
        Clear();
        Edges.Clear();
    }

    public DataGraphBytes GetData()
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new AbstractNodeJsonConverter<AbstractNode>());
        options.Converters.Add(new AbstractNodeJsonConverter<AbstractEdge>());

        var nodes = JsonSerializer.Serialize(this.ToArray(), options);
        var edges = JsonSerializer.Serialize(Edges.ToArray(), options);

        string compressedNodes = Compression.ToBrotliAsync(nodes, CompressionLevel.Optimal).GetAwaiter().GetResult().Result.Value;
        string compressedEdges = Compression.ToBrotliAsync(edges, CompressionLevel.Optimal).GetAwaiter().GetResult().Result.Value;

        DataGraphBytes data = new() { Nodes = compressedNodes, Edges = compressedEdges };
        return data;
    }

    public void InsertData(DataGraphBytes data)
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new AbstractNodeJsonConverter<AbstractNode>());
        options.Converters.Add(new AbstractNodeJsonConverter<AbstractEdge>());

        string decompressedNodes = Compression.FromBrotliAsync(data.Nodes).GetAwaiter().GetResult();
        string decompressedEdges = Compression.FromBrotliAsync(data.Edges).GetAwaiter().GetResult();

        var nodes = JsonSerializer.Deserialize<AbstractNode[]>(decompressedNodes, options);
        var edges = JsonSerializer.Deserialize<AbstractEdge[]>(decompressedEdges, options);

        Parallel.For(0, nodes.Length, (i) => Add(nodes[i]));
        Parallel.For(0, edges.Length, (i) => TryAddEdge(edges[i]));
    }

    public bool TryAddEdge(AbstractEdge edge)
    {
        if (edge.ParentId == Guid.Empty) return false;
        Edges.Add(edge);
        return true;
    }

    public bool TryGetEdges(AbstractNode parent, AbstractNode child, out List<AbstractEdge> edges)
    {
        edges = [];
        if (parent == null || child == null) return false;

        var found = Edges.AsParallel().Where(x => x.ParentId == parent.Id && x.ChildId == child.Id);
        if (found.Any())
        {
            edges = found.ToList();
            return true;
        }

        return false;
    }

    public bool TryGetEdges(Guid parentId, Guid childId, out List<AbstractEdge> edges)
    {
        edges = [];
        if (parentId == Guid.Empty || childId == Guid.Empty) return false;

        var found = Edges.AsParallel().Where(x => x.ParentId == parentId && x.ChildId == childId);
        if (found.Any())
        {
            edges = found.ToList();
            return true;
        }

        return false;
    }

    public bool TryGetEdgesFrom(AbstractNode parent, out List<AbstractEdge> edges)
    {
        edges = [];
        if (parent == null) return false;
        return TryGetEdgesFrom(parent.Id, out edges);
    }

    public bool TryGetEdgesFrom(Guid parentId, out List<AbstractEdge> edges)
    {
        edges = [];
        if (parentId == Guid.Empty) return false;

        var found = Edges.AsParallel().Where(x => x.ParentId == parentId);
        if (found.Any())
        {
            edges = found.ToList();
            return true;
        }

        return false;
    }

    public bool TryGetEdgesTo(AbstractNode child, out List<AbstractEdge> edges)
    {
        edges = [];
        if (child == null) return false;
        return TryGetEdgesTo(child.Id, out edges);
    }

    public bool TryGetEdgesTo(Guid childId, out List<AbstractEdge> edges)
    {
        edges = [];
        if (childId == Guid.Empty) return false;

        var found = Edges.AsParallel().Where(x => x.ChildId == childId);
        if (found.Any())
        {
            edges = found.ToList();
            return true;
        }

        return false;
    }

}

public struct DataGraphBytes
{
    public string Nodes { get; set; }

    public string Edges { get; set; }
}

public class AbstractNodeJsonConverter<T> : JsonConverter<T>
{
    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using (var jsonDoc = JsonDocument.ParseValue(ref reader))
        {
            if (!jsonDoc.RootElement.TryGetProperty("Type", out var typeProp))
            {
                throw new JsonException("Missing 'Type' discriminator property.");
            }

            string typeName = typeProp.GetString();
            Type type = Type.GetType(typeName);

            if (type == null || !typeof(AbstractNode).IsAssignableFrom(type))
            {
                throw new JsonException($"Unknown or invalid type: {typeName}");
            }

            return (T)JsonSerializer.Deserialize(jsonDoc.RootElement.GetRawText(), type, options);
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Type", value.GetType().FullName);

        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using (var jsonDoc = JsonDocument.Parse(json))
        {
            foreach (var prop in jsonDoc.RootElement.EnumerateObject())
            {
                prop.WriteTo(writer);
            }
        }
        writer.WriteEndObject();
    }
}
