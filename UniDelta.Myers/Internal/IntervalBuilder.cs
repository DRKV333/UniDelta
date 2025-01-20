using System;

namespace UniDelta.Myers.Internal;

internal struct IntervalBuilder
{
    private int otherStart = 0;
    private int start = 0;
    private int count = 0;

    private readonly Action<(int, int, int)> buildDelegate;

    public IntervalBuilder(Action<(int otherStart, int start, int count)> buildDelegate)
    {
        this.buildDelegate = buildDelegate;
    }

    public void AddSection(int sectionOtherStart, int sectionStart, int sectionCount)
    {
        if (sectionStart == start + count)
        {
            count += sectionCount;
        }
        else
        {
            Flush();
            otherStart = sectionOtherStart;
            start = sectionStart;
            count = sectionCount;
        }
    }

    public void Flush()
    {
        if (count > 0)
            buildDelegate((otherStart, start, count));

        otherStart = 0;
        start = 0;
        count = 0;
    }
}
