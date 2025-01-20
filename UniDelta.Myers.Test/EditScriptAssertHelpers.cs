using System.Linq;
using NUnit.Framework;

namespace UniDelta.Myers.Test;

public static class EditScriptAssertHelpers
{
    // TODO: Make these classes actually Equals correctly.
    public static void AssertEquals(EditScript<char> actual, EditScript<char> expected)
    {
        string str = CharEditScriptSerializer.WriteString(actual);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Deletes, Is.EqualTo(expected.Deletes), $"Deletes are different: {str}");

            Assert.That(actual.Inserts, Has.Count.EqualTo(expected.Inserts.Count), $"Insert count is wrong: {str}");
            foreach (var (first, second) in actual.Inserts.Zip(expected.Inserts))
            {
                Assert.That(first.Index, Is.EqualTo(second.Index), $"Insert index is wrong: {str}");
                Assert.That(first.Values, Is.EqualTo(second.Values), $"Insert values are wrong: {str}");
            }
        });
    }
}
