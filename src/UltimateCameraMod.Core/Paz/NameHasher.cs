namespace UltimateCameraMod.Paz;

/// <summary>
/// Lookup3 name hash — returns the primary hash value (c).
/// Used for filename-based key derivation in PAZ archives.
/// </summary>
public static class NameHasher
{
    public static uint ComputeHash(byte[] data, uint initval = 0)
    {
        int length = data.Length;
        uint a, b, c;
        a = b = c = Add(0xDEADBEEF + (uint)length, initval);
        int off = 0;

        while (length > 12)
        {
            a = Add(a, Le32(data, off));
            b = Add(b, Le32(data, off + 4));
            c = Add(c, Le32(data, off + 8));

            a = Sub(a, c); a ^= Rot(c, 4); c = Add(c, b);
            b = Sub(b, a); b ^= Rot(a, 6); a = Add(a, c);
            c = Sub(c, b); c ^= Rot(b, 8); b = Add(b, a);
            a = Sub(a, c); a ^= Rot(c, 16); c = Add(c, b);
            b = Sub(b, a); b ^= Rot(a, 19); a = Add(a, c);
            c = Sub(c, b); c ^= Rot(b, 4); b = Add(b, a);

            off += 12;
            length -= 12;
        }

        byte[] tail = new byte[12];
        Array.Copy(data, off, tail, 0, length);

        if (length >= 12) c = Add(c, Le32(tail, 8));
        else if (length >= 9) { uint v = Le32(tail, 8); c = Add(c, v & (0xFFFFFFFF >> (8 * (12 - length)))); }
        if (length >= 8) b = Add(b, Le32(tail, 4));
        else if (length >= 5) { uint v = Le32(tail, 4); b = Add(b, v & (0xFFFFFFFF >> (8 * (8 - length)))); }
        if (length >= 4) a = Add(a, Le32(tail, 0));
        else if (length >= 1) { uint v = Le32(tail, 0); a = Add(a, v & (0xFFFFFFFF >> (8 * (4 - length)))); }
        else return c;

        c ^= b; c = Sub(c, Rot(b, 14));
        a ^= c; a = Sub(a, Rot(c, 11));
        b ^= a; b = Sub(b, Rot(a, 25));
        c ^= b; c = Sub(c, Rot(b, 16));
        a ^= c; a = Sub(a, Rot(c, 4));
        b ^= a; b = Sub(b, Rot(a, 14));
        c ^= b; c = Sub(c, Rot(b, 24));

        return c;
    }

    private static uint Rot(uint v, int k) => (v << k) | (v >> (32 - k));
    private static uint Add(uint a, uint b) => unchecked(a + b);
    private static uint Sub(uint a, uint b) => unchecked(a - b);

    private static uint Le32(byte[] b, int off) =>
        (uint)b[off] | ((uint)b[off + 1] << 8) | ((uint)b[off + 2] << 16) | ((uint)b[off + 3] << 24);
}
