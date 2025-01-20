using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UniDelta.Myers;

public static class CharEditScriptSerializer
{
    public static async Task WriteAsync(EditScript<char> script, TextWriter writer)
    {
        int d = 0;

        foreach (EditScriptInsert<char> insert in script.Inserts)
        {
            while (d < script.Deletes.Count && script.Deletes[d].Index <= insert.Index)
            {
                await WriteDelete(script.Deletes[d++], writer);
            }

            await WriteInsert(insert, writer);
        }

        while (d < script.Deletes.Count)
        {
            await WriteDelete(script.Deletes[d++], writer);
        }
    }

    private static Task WriteInsert(EditScriptInsert<char> insert, TextWriter writer) =>
        writer.WriteLineAsync($"I{insert.Index}:{Regex.Escape(string.Concat(insert.Values))}");

    private static Task WriteDelete(EditScriptDelete delete, TextWriter writer) =>
        writer.WriteLineAsync($"D{delete.Index}:{delete.Lenght}");

    public static async Task<EditScript<char>> ReadAsync(TextReader reader)
    {
        List<EditScriptInsert<char>> inserts = new();
        List<EditScriptDelete> deletes = new();

        int lineNumber = 0;
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line[0] == 'I')
            {
                (int index, int restStart) = ReadIndex(lineNumber, line);
                inserts.Add(new EditScriptInsert<char>(index, Regex.Unescape(line[restStart..]).ToCharArray()));
            }
            else if (line[0] == 'D')
            {
                (int index, int restStart) = ReadIndex(lineNumber, line);
                if (!int.TryParse(line.AsSpan(restStart), out int count))
                    throw new FormatException($"Cound not parse delete count in line {lineNumber}");
                deletes.Add(new EditScriptDelete(index, count));
            }
            else
            {
                throw new FormatException($"Unknown start character '{line[0]}' for line {lineNumber}");
            }

            lineNumber++;
        }

        return new EditScript<char>(inserts, deletes);
    }

    public static string WriteString(EditScript<char> script)
    {
        StringWriter writer = new();
        WriteAsync(script, writer).Wait();
        return writer.ToString();
    }

    public static EditScript<char> ReadString(string str)
    {
        StringReader reader = new(str);
        return ReadAsync(reader).Result;
    }

    private static (int index, int restStart) ReadIndex(int lineNumber, string line)
    {
        int indexEnd = line.IndexOf(':');
        if (indexEnd == -1)
            throw new FormatException($"Did not find ':' in line {lineNumber}");

        if (!int.TryParse(line.AsSpan(1, indexEnd - 1), out int index))
            throw new FormatException($"Cound not parse index in line {lineNumber}");

        return (index, indexEnd + 1);
    }
}
