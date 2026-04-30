public class RawMaterialGraphItem
{
    public int ItemRawId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; }
    public string RawMaterialName { get; set; }
    public string? RawMaterialInfo { get; set; }
    public string LotCode { get; set; }
    public int LotId { get; set; }
    public int RawMaterialId { get; set; }
}

public class GraphNode
{
    public string Id { get; set; }
    public string Label { get; set; }
    public string Type { get; set; }
    public int Group { get; set; }
    
    public object AdditionalData { get; set; }
}

public class GraphEdge
{
    public string Source { get; set; }
    public string Target { get; set; }
    public string Label { get; set; }
    public int Sequence { get; set; }
}

public class GraphDto
{
    public List<GraphNode> Nodes { get; } = new();
    public List<GraphEdge> Edges { get; } = new();
}