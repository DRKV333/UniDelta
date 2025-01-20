using System;
using NUnit.Framework;

namespace UniDelta.Myers.Test;

[TestFixture]
public class EditScriptTests
{
    [Test]
    public void CharEditScriptReadAndWrite()
    {
        EditScript<char> editScript = new(
            [new EditScriptInsert<char>(0, ['a', 'b', 'c', '\n'])],
            [new EditScriptDelete(0, 4), new EditScriptDelete(5, 1)]
        );

        string str = $@"D0:4{Environment.NewLine}I0:abc\n{Environment.NewLine}D5:1{Environment.NewLine}";

        string written = CharEditScriptSerializer.WriteString(editScript);

        EditScript<char> read = CharEditScriptSerializer.ReadString(str);

        Assert.That(written, Is.EqualTo(str));

        EditScriptAssertHelpers.AssertEquals(read, editScript);
    }

    [Test]
    [TestCase("aaabbbaaa", "D3:3", "aaaaaa")]
    [TestCase("aaaaaa", "I3:bbb", "aaabbbaaa")]
    [TestCase("aaa", "I3:bbb", "aaabbb")]
    [TestCase("aaabbbaaa", "D3:3\nI3:ccc", "aaacccaaa")]
    public void Apply(string source, string script, string target)
    {
        EditScript<char> read = CharEditScriptSerializer.ReadString(script);
        Assert.That(string.Concat(read.Apply(source)), Is.EqualTo(target));
    }
}