using System.Collections.Generic;

namespace UniDelta.Myers.Internal;

internal class EditScriptBuilder<T>
{
    private IntervalBuilder insertBuilder;
    private IntervalBuilder deleteBuilder;

    private readonly List<EditScriptInsert<T>> inserts = new();
    private readonly List<EditScriptDelete> deletes = new();

    public EditScriptBuilder(IReadOnlyList<T> target)
    {
        insertBuilder = new(x => {
            inserts.Add(new EditScriptInsert<T>(x.otherStart, new ListAccessor<T>(target, x.start, x.count).ToArray()));
        });

        deleteBuilder = new(x => 
            deletes.Add(new EditScriptDelete(x.start, x.count))
        );
    }

    public void AddInsert(int sourceStart, int targetStart, int targetCount) =>
        insertBuilder.AddSection(sourceStart, targetStart, targetCount);

    public void AddInsert(in ListAccessor<T> source, in ListAccessor<T> target) =>
        AddInsert(source.Start, target.Start, target.Count);

    public void AddDelete(int targetStart, int sourceStart, int sourceCount) =>
        deleteBuilder.AddSection(targetStart, sourceStart, sourceCount);

    public void AddDelete(in ListAccessor<T> source, in ListAccessor<T> target) =>
        AddDelete(target.Start, source.Start, source.Count);

    public EditScript<T> GetEditScript()
    {
        insertBuilder.Flush();
        deleteBuilder.Flush();
        return new EditScript<T>(inserts, deletes);
    }
}
