namespace UltimateCameraMod.Paz;

/// <summary>
/// Pure C# stream transform (RFC 7539 quarter-round, 256-bit key, 128-bit nonce).
/// No external dependency required.
/// </summary>
public static class StreamTransform
{
    public static byte[] Apply(byte[] data, byte[] key, byte[] nonce)
    {
        if (key.Length != 32) throw new ArgumentException("Key must be 32 bytes");
        if (nonce.Length != 16) throw new ArgumentException("Nonce must be 16 bytes");

        var output = new byte[data.Length];
        var state = new uint[16];
        var block = new byte[64];

        uint counter = Le32(nonce, 0);
        uint[] constants = { 0x61707865, 0x3320646e, 0x79622d32, 0x6b206574 };

        for (int offset = 0; offset < data.Length; offset += 64)
        {
            state[0] = constants[0];
            state[1] = constants[1];
            state[2] = constants[2];
            state[3] = constants[3];
            state[4] = Le32(key, 0);
            state[5] = Le32(key, 4);
            state[6] = Le32(key, 8);
            state[7] = Le32(key, 12);
            state[8] = Le32(key, 16);
            state[9] = Le32(key, 20);
            state[10] = Le32(key, 24);
            state[11] = Le32(key, 28);
            state[12] = counter;
            state[13] = Le32(nonce, 4);
            state[14] = Le32(nonce, 8);
            state[15] = Le32(nonce, 12);

            var working = (uint[])state.Clone();

            for (int i = 0; i < 10; i++)
            {
                QuarterRound(working, 0, 4, 8, 12);
                QuarterRound(working, 1, 5, 9, 13);
                QuarterRound(working, 2, 6, 10, 14);
                QuarterRound(working, 3, 7, 11, 15);
                QuarterRound(working, 0, 5, 10, 15);
                QuarterRound(working, 1, 6, 11, 12);
                QuarterRound(working, 2, 7, 8, 13);
                QuarterRound(working, 3, 4, 9, 14);
            }

            for (int i = 0; i < 16; i++)
                working[i] += state[i];

            for (int i = 0; i < 16; i++)
            {
                block[i * 4 + 0] = (byte)(working[i]);
                block[i * 4 + 1] = (byte)(working[i] >> 8);
                block[i * 4 + 2] = (byte)(working[i] >> 16);
                block[i * 4 + 3] = (byte)(working[i] >> 24);
            }

            int remaining = Math.Min(64, data.Length - offset);
            for (int i = 0; i < remaining; i++)
                output[offset + i] = (byte)(data[offset + i] ^ block[i]);

            counter++;
        }

        return output;
    }

    private static void QuarterRound(uint[] s, int a, int b, int c, int d)
    {
        s[a] += s[b]; s[d] ^= s[a]; s[d] = RotL(s[d], 16);
        s[c] += s[d]; s[b] ^= s[c]; s[b] = RotL(s[b], 12);
        s[a] += s[b]; s[d] ^= s[a]; s[d] = RotL(s[d], 8);
        s[c] += s[d]; s[b] ^= s[c]; s[b] = RotL(s[b], 7);
    }

    private static uint RotL(uint v, int n) => (v << n) | (v >> (32 - n));

    private static uint Le32(byte[] b, int off) =>
        (uint)b[off] | ((uint)b[off + 1] << 8) | ((uint)b[off + 2] << 16) | ((uint)b[off + 3] << 24);
}
