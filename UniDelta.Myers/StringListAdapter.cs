using System.Collections;
using System.Collections.Generic;

namespace UniDelta.Myers;

public class StringListAdapter : IReadOnlyList<char>
{
    private readonly string str;

    public StringListAdapter(string str)
    {
        this.str = str;
    }

    public char this[int index] => str[index];

    public int Count => str.Length;

    public IEnumerator<char> GetEnumerator() => str.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)str).GetEnumerator();
}
