using NUnit.Framework;
using NUnit.Framework.Internal;
using UniDelta.Myers.Internal;

namespace UniDelta.Myers.Test;

[TestFixture]
public class ShortestEditScriptDifferTests
{
    [Test]
    [TestCase("ABCABBA", "CBABAC", -1, 5)]
    [TestCase("CBABAC", "ABCABBA", -1, 5)]
    [TestCase("", "", -1, 0)]
    [TestCase("a", "", -1, 1)]
    [TestCase("", "a", -1, 1)]
    [TestCase("a", "b", -1, 2)]
    [TestCase("a", "a", -1, 0)]
    [TestCase("aa", "a", -1, 1)]
    [TestCase("a", "ab", -1, 1)]
    [TestCase("aaaaaaaaa", "aaaaaaaaa", -1, 0)]
    [TestCase("aaaaaaaaa", "aaaaabaaaa", -1, 1)]
    [TestCase("aaaaaaaaa", "aaaaabbbaaaa", -1, 3)]
    [TestCase("aaaaaaaaa", "aaaaabbbaaaa", 3, 3)]
    [TestCase("aaaaaaaaa", "aaaaabbbaaaa", 2, -1)]
    [TestCase("aaaaaaaaa", "aaaaabbbaaaa", 1, -1)]
    [TestCase("aaaaaaaaa", "aaaaaaaa", -1, 1)]
    [TestCase("aaaaaaaaa", "bbbbbbbbb", -1, 18)]
    [TestCase("aaaaaaaaa", "bbbbbbbbb", 18, 18)]
    [TestCase("aaaaaaaaa", "bbbbbbbbb", 7, -1)]
    [TestCase("a", "aa", -1, 1)]
    [TestCase("ab", "a", -1, 1)]
    [TestCase("aaaaaaaaa", "aaaaaaaaa", -1, 0)]
    [TestCase("aaaaabaaaa", "aaaaaaaaa", -1, 1)]
    [TestCase("aaaaabbbaaaa", "aaaaaaaaa", -1, 3)]
    [TestCase("aaaaabbbaaaa", "aaaaaaaaa", 3, 3)]
    [TestCase("aaaaabbbaaaa", "aaaaaaaaa", 2, -1)]
    [TestCase("aaaaabbbaaaa", "aaaaaaaaa", 1, -1)]
    [TestCase("aaaaaaaa", "aaaaaaaaa", -1, 1)]
    [TestCase("bbbbbbbbb", "aaaaaaaaa", -1, 18)]
    [TestCase("bbbbbbbbb", "aaaaaaaaa", 18, 18)]
    [TestCase("bbbbbbbbb", "aaaaaaaaa", 7, -1)]
    public void FindShortestEditScriptLength(string source, string target, int max, int expected)
    {
        ShortestEditScriptDiffer<char> differ = new();

        int result = differ.FindShortestEditScriptLength(
            new StringListAdapter(source),
            new StringListAdapter(target),
            max
        );

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    [TestCase("ABCABBA", "CBABAC", 5, 3, 2, 5, 4, 0, 0)]
    [TestCase("CBABAC", "ABCABBA", 5, 1, 4, 2, 5, 0, 0)]
    [TestCase("bbb", "bbb", 0, 0, 0, 3, 3, 3, 3)]
    [TestCase("aaaaa", "bbbbb", 10, 5, 0, 5, 0, 0, 0)]
    [TestCase("a", "b", 2, 1, 0, 1, 0, 0, 0)]
    [TestCase("aabbbbbaa", "ccbbbbbcc", 8, 2, 2, 7, 7, 0, 0)]
    [TestCase("bbbbb", "ccbbbbbcc", 4, 0, 2, 5, 7, 0, 0)]
    [TestCase("ccbbbbbcc", "bbbbb", 4, 2, 0, 7, 5, 0, 0)]
    [TestCase("abbbbb", "ccbbbbbcc", 5, 1, 2, 6, 7, 0, 0)]
    [TestCase("ccbbbbbcc", "abbbbb", 5, 2, 1, 7, 6, 0, 0)]
    [TestCase("ab", "b", 1, 1, 0, 2, 1, 0, 1)]
    [TestCase("b", "ab", 1, 0, 1, 1, 2, 0, 1)]
    [TestCase("aaa", "bb", 5, 1, 2, 1, 2, 0, 0)]
    [TestCase("bb", "aaa", 5, 0, 3, 0, 3, 0, 0)]
    [TestCase("b", "aaa", 4, 1, 1, 1, 1, 0, 0)]
    [TestCase("aaa", "b", 4, 2, 0, 2, 0, 0, 0)]
    [TestCase("aaab", "aaa", 1, 4, 3, 4, 3, 3, 0)]
    [TestCase("aaa", "aaab", 1, 3, 4, 3, 4, 3, 0)]
    public void FindMiddleSnake(string source, string target, int scriptLength, int startX, int startY, int endX, int endY, int startFirstSnake, int endFirstSnake)
    {
        ShortestEditScriptDiffer<char> differ = new();

        var result = differ.FindMiddleSnake(
            new ListAccessor<char>(new StringListAdapter(source)),
            new ListAccessor<char>(new StringListAdapter(target))
        );

        Assert.That(result, Is.EqualTo(new ShortestEditScriptDiffer<char>.MiddleSnake(scriptLength, startX, startY, endX, endY, startFirstSnake, endFirstSnake)));
    }

    [Test]
    [TestCase("ABCABBA", "CBABAC", "D0:1\nI0:C\nD2:1\nD5:1\nI7:C")]
    [TestCase("Hello, this is some text!", "Hi, this is a different text!", "D1:4\nI1:i\nD15:3\nI15:a diff\nI19:rent")]
    [TestCase("aaaaaa", "aaabbbaaa", "I3:bbb")]
    [TestCase("aaabbbaaa", "aaaaaa", "D3:3")]
    [TestCase("aaabbbaaa", "aaacccaaa", "D3:3\nI6:ccc")]
    [TestCase("aaa", "aa", "D2:1")]
    [TestCase("aaa", "bb", "D0:3\nI0:bb")]
    [TestCase("bb", "aaa", "D0:2\nI0:aaa")]
    [TestCase("bx", "aaax", "D0:1\nI0:aaa")]
    [TestCase("a", "aab", "I0:a\nI1:b")]
    [TestCase("a", "baa", "I0:ba")]
    public void FindEditScript(string source, string target, string deltaStr)
    {
        ShortestEditScriptDiffer<char> differ = new();

        EditScript<char> expected = CharEditScriptSerializer.ReadString(deltaStr);
        EditScript<char> actual = differ.FindEditScript(new StringListAdapter(source), new StringListAdapter(target));

        EditScriptAssertHelpers.AssertEquals(actual, expected);

        string applied = string.Concat(actual.Apply(source));
        Assert.That(applied, Is.EqualTo(target));
    }

    [Test]
    public void FindEditScript_Random()
    {
        ShortestEditScriptDiffer<char> differ = new();

        Randomizer rng = TestExecutionContext.CurrentContext.RandomGenerator;
        string source = rng.GetString(1_000);
        string target = rng.GetString(1_000);

        EditScript<char> script = differ.FindEditScript(
            new StringListAdapter(source),
            new StringListAdapter(target)
        );

        Assert.That(string.Concat(script.Apply(source)), Is.EqualTo(target));
    }
}