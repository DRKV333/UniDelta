using System.Collections.Generic;

namespace UniDelta.Myers.Internal;

internal class DPathFinder<T>
{
    public struct Iterator
    {
        private readonly IEqualityComparer<T> comparer;
        private readonly int[] vector;
        private readonly int max;

        private readonly ListAccessor<T> source;
        private readonly ListAccessor<T> target;

        public int D { get; private set; } = -1;
        public int K { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }
        public int LastSnakeStartX { get; private set; }
        public int LastSnakeStartY { get; private set; }

        public int GetEndXOnDiagonal(int k) => Vector(k);

        public Iterator(DPathFinder<T> dPathFinder, int max, in ListAccessor<T> source, in ListAccessor<T> target)
        {
            this.source = source;
            this.target = target;
            this.max = max;
            comparer = dPathFinder.comparer;

            int neededCapacity = max * 2 + 1;
            if (dPathFinder.vector == null || dPathFinder.vector.Length < neededCapacity)
                dPathFinder.vector = new int[neededCapacity];

            vector = dPathFinder.vector;
            Vector(1) = 0;
        }

        public bool StepD()
        {
            D++;
            K = -D - 2;
            return D <= max;
        }

        public bool StepK()
        {
            K += 2;
            if (K <= D)
            {
                Find();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void Find()
        {
            if (K == -D ||
                K != D && Vector(K - 1) < Vector(K + 1))
            {
                X = Vector(K + 1);
            }
            else
            {
                X = Vector(K - 1) + 1;
            }

            Y = X - K;

            LastSnakeStartX = X;
            LastSnakeStartY = Y;

            while (X < source.Count && Y < target.Count &&
                    comparer.Equals(source[X], target[Y]))
            {
                X++;
                Y++;
            }

            Vector(K) = X;
        }

        private ref int Vector(int i) => ref vector[i + max];
    }

    private readonly IEqualityComparer<T> comparer;
    private int[] vector = null!;

    public DPathFinder(IEqualityComparer<T> comparer)
    {
        this.comparer = comparer;
    }

    public Iterator Begin(int max, in ListAccessor<T> source, in ListAccessor<T> target) => new Iterator(this, max, source, target);
}
