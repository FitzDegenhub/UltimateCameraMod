using System.Text;

namespace UltimateCameraMod.Paz;

/// <summary>
/// Key derivation and data encode/decode for PAZ archive assets.
/// </summary>
public static class AssetCodec
{
    private const uint HashInitVal = 0x000C5EDE;
    private const uint IvXor = 0x60616263;

    private static readonly uint[] XorDeltas =
    {
        0x00000000, 0x0A0A0A0A, 0x0C0C0C0C, 0x06060606,
        0x0E0E0E0E, 0x0A0A0A0A, 0x06060606, 0x02020202,
    };

    public static (byte[] Key, byte[] Iv) BuildParameters(string filename)
    {
        string basename = Path.GetFileName(filename).ToLowerInvariant();
        uint seed = NameHasher.ComputeHash(Encoding.UTF8.GetBytes(basename), HashInitVal);

        byte[] seedBytes = BitConverter.GetBytes(seed);
        byte[] iv = new byte[16];
        for (int i = 0; i < 4; i++)
            Array.Copy(seedBytes, 0, iv, i * 4, 4);

        uint keyBase = seed ^ IvXor;
        byte[] key = new byte[32];
        for (int i = 0; i < 8; i++)
        {
            uint val = keyBase ^ XorDeltas[i];
            byte[] valBytes = BitConverter.GetBytes(val);
            Array.Copy(valBytes, 0, key, i * 4, 4);
        }

        return (key, iv);
    }

    public static byte[] Decode(byte[] data, string filename)
    {
        var (key, iv) = BuildParameters(filename);
        return StreamTransform.Apply(data, key, iv);
    }

    public static byte[] Encode(byte[] data, string filename) => Decode(data, filename);
}
