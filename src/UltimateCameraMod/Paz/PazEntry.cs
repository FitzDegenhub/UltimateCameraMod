namespace UltimateCameraMod.Paz;

/// <summary>
/// A single file entry in a PAZ archive, parsed from the PAMT index.
/// </summary>
public sealed class PazEntry
{
    public string Path { get; init; } = "";
    public string PazFile { get; init; } = "";
    public int Offset { get; init; }
    public int CompSize { get; init; }
    public int OrigSize { get; init; }
    public uint Flags { get; init; }
    public int PazIndex { get; init; }

    public bool Compressed => CompSize != OrigSize;
    public int CompressionType => (int)((Flags >> 16) & 0x0F);
    public bool IsXml => Path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase);
}
