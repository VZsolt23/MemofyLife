using System.Text.Json;
using MemofyLife.Minesweeper.Engine;
using MemofyLife.Minesweeper.Engine.Enums;

namespace MemofyLife.Tests.Arcade.Minesweeper;

public class SnapshotTests
{
    // B elrendezés: két akna az átló két végén.
    private static MinesweeperBoard CreateLayoutB() =>
        new(new BoardSettings(3, 3, 2), new FixedMinePlacer((0, 0), (2, 2)));

    private static void AssertSameState(MinesweeperBoard expected, MinesweeperBoard actual)
    {
        Assert.Equal(expected.Status, actual.Status);
        Assert.Equal(expected.FlagsLeft, actual.FlagsLeft);

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Assert.Equal(expected[row, col], actual[row, col]);
            }
        }
    }

    [Fact]
    public void Restore_ReproducesAGameInProgress()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        board.ToggleFlag(0, 1);

        MinesweeperBoard restored = MinesweeperBoard.Restore(board.Snapshot(), new FixedMinePlacer());

        AssertSameState(board, restored);
    }

    [Fact]
    public void Restore_ReproducesALostGame()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        board.Reveal(0, 0);

        MinesweeperBoard restored = MinesweeperBoard.Restore(board.Snapshot(), new FixedMinePlacer());

        AssertSameState(board, restored);
        Assert.True(restored[2, 2].IsMine);
    }

    [Fact]
    public void RestoredGame_CanBeFinished()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        MinesweeperBoard restored = MinesweeperBoard.Restore(board.Snapshot(), new FixedMinePlacer());

        (int Row, int Column)[] rest = [(0, 1), (1, 0), (1, 2), (2, 1), (0, 2), (2, 0)];

        foreach ((int row, int col) in rest)
        {
            restored.Reveal(row, col);
        }

        Assert.Equal(GameStatus.Won, restored.Status);
    }

    [Fact]
    public void Restore_NotStartedBoard_PlacesMinesOnTheFirstReveal()
    {
        MinesweeperBoard board = CreateLayoutB();

        MinesweeperBoard restored = MinesweeperBoard.Restore(board.Snapshot(), new FixedMinePlacer((0, 0), (2, 2)));
        restored.Reveal(1, 1);

        Assert.Equal(2, restored[1, 1].AdjacentMines);
    }

    [Fact]
    public void Snapshot_SurvivesAJsonRoundTrip()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        board.ToggleFlag(0, 1);

        string json = JsonSerializer.Serialize(board.Snapshot());
        MinesweeperSnapshot loaded = JsonSerializer.Deserialize<MinesweeperSnapshot>(json)!;
        MinesweeperBoard restored = MinesweeperBoard.Restore(loaded, new FixedMinePlacer());

        AssertSameState(board, restored);
    }

    [Fact]
    public void Restore_WithUnknownSchemaVersion_Throws()
    {
        MinesweeperSnapshot snapshot = CreateLayoutB().Snapshot() with { SchemaVersion = 99 };

        Assert.Throws<NotSupportedException>(() => MinesweeperBoard.Restore(snapshot, new FixedMinePlacer()));
    }

    [Fact]
    public void Restore_WithWrongCellCount_Throws()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        MinesweeperSnapshot snapshot = board.Snapshot() with { Cells = [CellState.Hidden] };

        Assert.Throws<InvalidDataException>(() => MinesweeperBoard.Restore(snapshot, new FixedMinePlacer()));
    }

    [Fact]
    public void Restore_WithARevealedMineWhilePlaying_Throws()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        MinesweeperSnapshot valid = board.Snapshot();
        CellState[] cells = [.. valid.Cells];
        cells[valid.MineIndexes[0]] = CellState.Revealed;

        Assert.Throws<InvalidDataException>(
            () => MinesweeperBoard.Restore(valid with { Cells = cells }, new FixedMinePlacer()));
    }
}
