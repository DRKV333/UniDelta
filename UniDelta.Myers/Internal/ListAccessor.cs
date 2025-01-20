using System.Collections.Generic;

namespace UniDelta.Myers.Internal;

internal readonly record struct ListAccessor<T>(
    IReadOnlyList<T> List,
    int Start,
    int Count,
    bool Reverse = false
)
{
    public ListAccessor(IReadOnlyList<T> List, int Start = 0, bool Reverse = false)
        : this(List, Start, List.Count - Start, Reverse)
    { }

    public T this[int i] => List[RealIndex(i)];

    public int RealIndex(int i)
    {
        if (Reverse)
            return List.Count - Start - i - 1;
        else
            return Start + i;
    }

    public ListAccessor<T> MakeReverse() => new ListAccessor<T>(
        List: List,
        Start: List.Count - Start - 1 - (Count - 1),
        Count: Count,
        Reverse: !Reverse
    );

    public T[] ToArray()
    {
        T[] arr = new T[Count];

        if (Reverse)
        {
            int i = RealIndex(0);
            for (int j = 0; j < Count; j++)
            {
                arr[j] = List[i--];
            }
        }
        else
        {
            int i = Start;
            for (int j = 0; j < Count; j++)
            {
                arr[j] = List[i++];
            }
        }

        return arr;
    }
}