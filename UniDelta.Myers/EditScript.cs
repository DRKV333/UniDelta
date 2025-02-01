using System.Collections.Generic;

namespace UniDelta.Myers;

public record struct EditScriptInsert<T>(
    int Index,
    IReadOnlyList<T> Values
);

public record struct EditScriptDelete(
    int Index,
    int Lenght
);

public record class EditScript<T>(
    IReadOnlyList<EditScriptInsert<T>> Inserts,
    IReadOnlyList<EditScriptDelete> Deletes
)
{
    public IEnumerable<T> Apply(IEnumerable<T> source)
    {
        int insertIndex = 0;
        int deleteIndex = 0;

        int deleteCount = 0;

        int i = 0;
        foreach (T item in source)
        {
            while (insertIndex < Inserts.Count && Inserts[insertIndex].Index == i)
            {
                foreach (T insertItem in Inserts[insertIndex].Values)
                {
                    yield return insertItem;
                }

                insertIndex++;
            }

            while (deleteIndex < Deletes.Count && Deletes[deleteIndex].Index == i)
            {
                deleteCount += Deletes[deleteIndex].Lenght;
                deleteIndex++;
            }

            if (deleteCount > 0)
                deleteCount--;
            else
                yield return item;

            i++;
        }

        while (insertIndex < Inserts.Count && Inserts[insertIndex].Index == i)
        {
            foreach (T insertItem in Inserts[insertIndex].Values)
            {
                yield return insertItem;
            }

            insertIndex++;
        }
    }
}
