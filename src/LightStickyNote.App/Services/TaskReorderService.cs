namespace LightStickyNote.App.Services;

public static class TaskReorderService
{
    public static int ToTargetIndexFromInsertionIndex(int currentIndex, int insertionIndex, int itemCount)
    {
        if (currentIndex < 0 || currentIndex >= itemCount)
        {
            throw new ArgumentOutOfRangeException(nameof(currentIndex));
        }

        if (insertionIndex < 0 || insertionIndex > itemCount)
        {
            throw new ArgumentOutOfRangeException(nameof(insertionIndex));
        }

        return insertionIndex > currentIndex
            ? insertionIndex - 1
            : insertionIndex;
    }

    public static bool Move<T>(IList<T> items, int fromIndex, int toIndex)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (fromIndex < 0 || fromIndex >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(fromIndex));
        }

        if (toIndex < 0 || toIndex >= items.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(toIndex));
        }

        if (fromIndex == toIndex)
        {
            return false;
        }

        var item = items[fromIndex];
        items.RemoveAt(fromIndex);
        items.Insert(toIndex, item);
        return true;
    }
}
