using LightStickyNote.App.Services;

namespace LightStickyNote.App.Tests;

public sealed class TaskReorderServiceTests
{
    [Fact]
    public void Move_moves_item_to_an_earlier_index()
    {
        var items = new List<string> { "A", "B", "C", "D" };

        var moved = TaskReorderService.Move(items, fromIndex: 2, toIndex: 0);

        Assert.True(moved);
        Assert.Equal(["C", "A", "B", "D"], items);
    }

    [Fact]
    public void Move_moves_item_to_a_later_index()
    {
        var items = new List<string> { "A", "B", "C", "D" };

        var moved = TaskReorderService.Move(items, fromIndex: 1, toIndex: 3);

        Assert.True(moved);
        Assert.Equal(["A", "C", "D", "B"], items);
    }

    [Fact]
    public void Move_returns_false_when_indices_do_not_change_order()
    {
        var items = new List<string> { "A", "B", "C" };

        var moved = TaskReorderService.Move(items, fromIndex: 1, toIndex: 1);

        Assert.False(moved);
        Assert.Equal(["A", "B", "C"], items);
    }

    [Theory]
    [InlineData(1, 0, 4, 0)]
    [InlineData(1, 3, 4, 2)]
    [InlineData(1, 4, 4, 3)]
    [InlineData(2, 2, 4, 2)]
    [InlineData(2, 3, 4, 2)]
    public void ToTargetIndexFromInsertionIndex_returns_the_expected_final_index(
        int currentIndex,
        int insertionIndex,
        int itemCount,
        int expected)
    {
        var actual = TaskReorderService.ToTargetIndexFromInsertionIndex(currentIndex, insertionIndex, itemCount);

        Assert.Equal(expected, actual);
    }
}
