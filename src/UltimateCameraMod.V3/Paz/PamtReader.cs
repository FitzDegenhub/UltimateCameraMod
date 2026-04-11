using System.Text;

namespace UltimateCameraMod.Paz;

/// <summary>
/// Parses .pamt index files to discover file entries in PAZ archives.
/// </summary>
public static class PamtReader
{
    public static List<PazEntry> Parse(string pamtPath, string? pazDir = null)
    {
        byte[] data = File.ReadAllBytes(pamtPath);
        pazDir ??= Path.GetDirectoryName(pamtPath) ?? ".";
        string pamtStem = Path.GetFileNameWithoutExtension(pamtPath);

        int off = 0;
        off += 4; // skip magic

        uint pazCount = BitConverter.ToUInt32(data, off); off += 4;
        off += 8; // hash + zero

        for (int i = 0; i < pazCount; i++)
        {
            off += 4; // hash
            off += 4; // size
            if (i < pazCount - 1)
                off += 4; // separator
        }

        // Folder section
        uint folderSize = BitConverter.ToUInt32(data, off); off += 4;
        int folderEnd = off + (int)folderSize;
        string folderPrefix = "";
        while (off < folderEnd)
        {
            uint parent = BitConverter.ToUInt32(data, off);
            int slen = data[off + 4];
            string name = Encoding.UTF8.GetString(data, off + 5, slen);
            if (parent == 0xFFFFFFFF)
                folderPrefix = name;
            off += 5 + slen;
        }

        // Node section (path tree)
        uint nodeSize = BitConverter.ToUInt32(data, off); off += 4;
        int nodeStart = off;
        var nodes = new Dictionary<int, (uint Parent, string Name)>();
        while (off < nodeStart + (int)nodeSize)
        {
            int rel = off - nodeStart;
            uint parent = BitConverter.ToUInt32(data, off);
            int slen = data[off + 4];
            string name = Encoding.UTF8.GetString(data, off + 5, slen);
            nodes[rel] = (parent, name);
            off += 5 + slen;
        }

        string BuildPath(uint nodeRef)
        {
            var parts = new List<string>();
            uint cur = nodeRef;
            int guard = 0;
            while (cur != 0xFFFFFFFF && guard < 64)
            {
                int key = (int)cur;
                if (!nodes.TryGetValue(key, out var node))
                    break;
                parts.Add(node.Name);
                cur = node.Parent;
                guard++;
            }
            parts.Reverse();
            return string.Concat(parts);
        }

        // Record section
        uint folderCount = BitConverter.ToUInt32(data, off); off += 4;
        off += 4; // hash
        off += (int)folderCount * 16;

        // File records (20 bytes each)
        var entries = new List<PazEntry>();
        while (off + 20 <= data.Length)
        {
            uint nodeRef = BitConverter.ToUInt32(data, off);
            uint pazOffset = BitConverter.ToUInt32(data, off + 4);
            uint compSize = BitConverter.ToUInt32(data, off + 8);
            uint origSize = BitConverter.ToUInt32(data, off + 12);
            uint flags = BitConverter.ToUInt32(data, off + 16);
            off += 20;

            int pazIndex = (int)(flags & 0xFF);
            string nodePath = BuildPath(nodeRef);
            string fullPath = string.IsNullOrEmpty(folderPrefix)
                ? nodePath
                : $"{folderPrefix}/{nodePath}";

            int pazNum = int.Parse(pamtStem) + pazIndex;
            string pazFile = Path.Combine(pazDir, $"{pazNum}.paz");

            entries.Add(new PazEntry
            {
                Path = fullPath,
                PazFile = pazFile,
                Offset = (int)pazOffset,
                CompSize = (int)compSize,
                OrigSize = (int)origSize,
                Flags = flags,
                PazIndex = pazIndex,
            });
        }

        return entries;
    }
}
