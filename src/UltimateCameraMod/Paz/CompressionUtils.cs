using K4os.Compression.LZ4;

namespace UltimateCameraMod.Paz;

/// <summary>
/// LZ4 block compression/decompression matching the game's format (no frame header).
/// </summary>
public static class CompressionUtils
{
    public static byte[] Lz4Decompress(byte[] data, int originalSize)
    {
        byte[] output = new byte[originalSize];
        int decoded = LZ4Codec.Decode(data, 0, data.Length, output, 0, originalSize);
        if (decoded < 0)
            throw new InvalidOperationException($"LZ4 decompression failed (returned {decoded})");
        return output;
    }

    public static byte[] Lz4Compress(byte[] data)
    {
        int maxLen = LZ4Codec.MaximumOutputSize(data.Length);
        byte[] buffer = new byte[maxLen];
        int encoded = LZ4Codec.Encode(data, 0, data.Length, buffer, 0, buffer.Length);
        if (encoded < 0)
            throw new InvalidOperationException($"LZ4 compression failed (returned {encoded})");
        byte[] result = new byte[encoded];
        Array.Copy(buffer, result, encoded);
        return result;
    }
}
