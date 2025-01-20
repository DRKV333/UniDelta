using System;
using System.IO;
using UniDelta.Myers;

if (args.Length != 3)
{
    Console.WriteLine("Usage:");
    Console.WriteLine("Diff.Cli.exe diff [source] [target]");
    Console.WriteLine("Diff.Cli.exe apply [source] [diff]");
    return 1;
}

if (args[0] == "diff")
{
    string source = await File.ReadAllTextAsync(args[1]);
    string target = await File.ReadAllTextAsync(args[2]);

    ShortestEditScriptDiffer<char> differ = new();

    EditScript<char> editScript = differ.FindEditScript(
        new StringListAdapter(source),
        new StringListAdapter(target)
    );

    await CharEditScriptSerializer.WriteAsync(editScript, Console.Out);

    return 0;
}
else if (args[0] == "apply")
{
    string source = await File.ReadAllTextAsync(args[1]);

    EditScript<char> editScript;
    using (StreamReader reader = File.OpenText(args[2]))
    {
        editScript = await CharEditScriptSerializer.ReadAsync(reader);
    }

    Console.WriteLine(string.Concat(editScript.Apply(source)));

    return 0;
}

return 1;