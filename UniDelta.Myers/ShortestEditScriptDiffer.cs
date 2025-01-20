using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniDelta.Myers.Internal;

namespace UniDelta.Myers;

public class ShortestEditScriptDiffer<T>
{
    internal readonly record struct MiddleSnake(
        int ScriptLength,
        int StartX,
        int StartY,
        int EndX,
        int EndY,
        int StartFirstSnake,
        int EndFirstSnake
    );

    private bool running = false;

    private readonly DPathFinder<T> forwardFinder;
    private readonly DPathFinder<T> backwardFinder;

    public ShortestEditScriptDiffer(IEqualityComparer<T>? comparer = null)
    {
        comparer ??= EqualityComparer<T>.Default;
        forwardFinder = new DPathFinder<T>(comparer);
        backwardFinder = new DPathFinder<T>(comparer);
    }

    public int FindShortestEditScriptLength(IReadOnlyList<T> source, IReadOnlyList<T> target, int max = -1)
    {
        if (source.Count == 0)
            return target.Count;

        if (target.Count == 0)
            return source.Count;

        CheckRunning();
        try
        {
            if (max < 0)
                max = source.Count + target.Count;

            DPathFinder<T>.Iterator iterator = forwardFinder.Begin(
                max,
                new ListAccessor<T>(source),
                new ListAccessor<T>(target)
            );

            while (iterator.StepD())
            {
                while (iterator.StepK())
                {
                    if (iterator.X >= source.Count && iterator.Y >= target.Count)
                        return iterator.D;
                }
            }

            return -1;
        }
        finally
        {
            running = false;
        }
    }

    private void CheckRunning()
    {
        if (running)
            throw new InvalidOperationException("Diffing is already in progress");
        running = true;
    }

    internal MiddleSnake FindMiddleSnake(in ListAccessor<T> source, in ListAccessor<T> target)
    {
        int sourceCount = source.Count;
        int targetCount = target.Count;

        int lenghtSum = sourceCount + targetCount;
        int halfDMax = DivCeil(lenghtSum, 2);

        int delta = sourceCount - targetCount;
        bool overlapCheckOnForwardPass = delta % 2 != 0;

        DPathFinder<T>.Iterator forwardIterator = forwardFinder.Begin(halfDMax, source, target);

        ListAccessor<T> reverseSource = source.MakeReverse();
        ListAccessor<T> reverseTarget = target.MakeReverse();

        DPathFinder<T>.Iterator backwardIterator = backwardFinder.Begin(halfDMax, reverseSource, reverseTarget);

        forwardIterator.StepD();
        forwardIterator.StepK();

        backwardIterator.StepD();
        backwardIterator.StepK();

        if (CheckOverlap(forwardIterator, backwardIterator, 0, delta, sourceCount))
        {
            return new MiddleSnake(
                0,
                source.RealIndex(0), target.RealIndex(0),
                source.RealIndex(source.Count), target.RealIndex(target.Count),
                sourceCount, sourceCount
            );
        }

        int startFirstSnake = forwardIterator.X;
        int endFirstSnake = backwardIterator.X;

        for (int d = 1; d <= halfDMax; d++)
        {
            forwardIterator.StepD();
            while (forwardIterator.StepK())
            {
                if (overlapCheckOnForwardPass && CheckOverlap(forwardIterator, backwardIterator, d - 1, delta, sourceCount))
                {
                    return new MiddleSnake(
                        2 * d - 1,
                        source.RealIndex(forwardIterator.LastSnakeStartX),
                        target.RealIndex(forwardIterator.LastSnakeStartY),
                        source.RealIndex(forwardIterator.X),
                        target.RealIndex(forwardIterator.Y),
                        startFirstSnake, endFirstSnake
                    );
                }
            }

            backwardIterator.StepD();
            while (backwardIterator.StepK())
            {
                if (!overlapCheckOnForwardPass && CheckOverlap(backwardIterator, forwardIterator, d, delta, sourceCount))
                {
                    return new MiddleSnake(
                        2 * d,
                        reverseSource.RealIndex(backwardIterator.X) + 1,
                        reverseTarget.RealIndex(backwardIterator.Y) + 1,
                        reverseSource.RealIndex(backwardIterator.LastSnakeStartX) + 1,
                        reverseTarget.RealIndex(backwardIterator.LastSnakeStartY) + 1,
                        startFirstSnake, endFirstSnake
                    );
                }
            }
        }

        throw new UnreachableException();
    }

    private static bool CheckOverlap(in DPathFinder<T>.Iterator iterator, in DPathFinder<T>.Iterator otherIterator, int d, int delta, int sourceCount)
    {
        int k = iterator.K;
        int otherK = -(k - delta);

        if (otherK >= -d && otherK <= d)
        {
            int otherX = sourceCount - otherIterator.GetEndXOnDiagonal(otherK);
            return iterator.X >= otherX;
        }
        else
        {
            return false;
        }
    }

    public EditScript<T> FindEditScript(IReadOnlyList<T> source, IReadOnlyList<T> target)
    {
        EditScriptBuilder<T> builder = new(target); 

        Stack<(ListAccessor<T> source, ListAccessor<T> target)> sections = new();
        
        if (source.Count > 0 || target.Count > 0)
            sections.Push((new ListAccessor<T>(source), new ListAccessor<T>(target)));

        while (sections.TryPop(out var section))
        {
            if (section.source.Count == 0)
            {
                builder.AddInsert(section.source, section.target);
                continue;
            }

            if (section.target.Count == 0)
            {
                builder.AddDelete(section.source, section.target);
                continue;
            }

            MiddleSnake snake = FindMiddleSnake(section.source, section.target);

            if (snake.ScriptLength == 0)
            {
                continue;
            }
            else if (snake.ScriptLength == section.source.Count + section.target.Count)
            {
                builder.AddInsert(section.source, section.target);
                builder.AddDelete(section.source, section.target);
            }
            else
            {
                int rightLengthX = section.source.Start + section.source.Count - snake.EndX;
                int rightLengthY = section.target.Start + section.target.Count - snake.EndY;

                int rightSnake = Math.Min(Math.Min(rightLengthX, rightLengthY), snake.EndFirstSnake);

                if (rightLengthX > 0 || rightLengthY > 0)
                {
                    sections.Push((
                        section.source with { Start = snake.EndX, Count = rightLengthX - rightSnake },
                        section.target with { Start = snake.EndY, Count = rightLengthY - rightSnake }
                    ));
                }

                int leftLengthX = snake.StartX - section.source.Start;
                int leftLengthY = snake.StartY - section.target.Start;

                int leftSnake = Math.Min(Math.Min(leftLengthX, leftLengthY), snake.StartFirstSnake);

                if (leftLengthX > 0 || leftLengthY > 0)
                {
                    sections.Push((
                        section.source with { Start = section.source.Start + leftSnake, Count = leftLengthX - leftSnake },
                        section.target with { Start = section.target.Start + leftSnake, Count = leftLengthY - leftSnake }
                    ));
                }
            }
        }

        return builder.GetEditScript();
    }

    private static int DivCeil(int a, int b) => ((a - 1) / b) + 1;
}
