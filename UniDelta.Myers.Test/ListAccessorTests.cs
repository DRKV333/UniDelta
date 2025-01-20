using System.Collections.Generic;
using NUnit.Framework;
using UniDelta.Myers.Internal;

namespace UniDelta.Myers.Test;

[TestFixture]
public class ListAccessorTests
{
    [Test]
    public void IndexForward()
    {
        string str = "Hello, this is a string!";
        ListAccessor<char> accessor = new(
            new StringListAdapter(str),
            Start: 7
        );

        Assert.Multiple(() =>
        {
            Assert.That(accessor, Has.Count.EqualTo(str.Length - 7));

            Assert.That(accessor[0], Is.EqualTo('t'));
            Assert.That(accessor[1], Is.EqualTo('h'));
            Assert.That(accessor[2], Is.EqualTo('i'));

            Assert.That(accessor[^1], Is.EqualTo('!'));
            Assert.That(accessor[^2], Is.EqualTo('g'));
            Assert.That(accessor[^3], Is.EqualTo('n'));
        });
    }

    [Test]
    public void IndexReverse()
    {
        string str = "Hello, this is a string!";
        ListAccessor<char> accessor = new(
            new StringListAdapter(str),
            Start: 2, Count: 10,
            Reverse: true
        );

        Assert.Multiple(() =>
        {
            Assert.That(accessor, Has.Count.EqualTo(10));

            Assert.That(accessor[0], Is.EqualTo('n'));
            Assert.That(accessor[1], Is.EqualTo('i'));
            Assert.That(accessor[2], Is.EqualTo('r'));

            Assert.That(accessor[^1], Is.EqualTo('i'));
            Assert.That(accessor[^2], Is.EqualTo('s'));
            Assert.That(accessor[^3], Is.EqualTo(' '));
        });
    }

    [Test]
    public void MakeReverse()
    {
        List<int> list = new(new int[10]);

        // 0 1 |2 3 4| 5 6 7 8 9
        ListAccessor<int> accessor = new(
            list,
            Start: 2,
            Count: 3
        );

        // 9 8 |7 6 5| 4 3 2 1 0
        ListAccessor<int> accessorReverse = accessor.MakeReverse();
        ListAccessor<int> accessorReverseReverse = accessorReverse.MakeReverse();

        Assert.Multiple(() =>
        {
            Assert.That(accessorReverse, Has.Count.EqualTo(3));
            Assert.That(accessorReverse.Start, Is.EqualTo(5));
            Assert.That(accessorReverse.Reverse, Is.True);

            Assert.That(accessorReverseReverse, Is.EqualTo(accessor));
        });
    }
}
