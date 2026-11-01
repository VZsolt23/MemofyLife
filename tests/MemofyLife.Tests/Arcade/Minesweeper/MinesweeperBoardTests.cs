using MemofyLife.Minesweeper.Engine;
using MemofyLife.Minesweeper.Engine.Enums;

namespace MemofyLife.Tests.Arcade.Minesweeper;

public class MinesweeperBoardTests
{
    private static MinesweeperBoard CreateBoard(int rows, int columns, params (int Row, int Column)[] mines) =>
        new(new BoardSettings(rows, columns, mines.Length), new FixedMinePlacer(mines));

    // A elrendezés: egyetlen akna a bal felső sarokban.
    private static MinesweeperBoard CreateLayoutA() => CreateBoard(3, 3, (0, 0));

    // B elrendezés: két akna az átló két végén.
    private static MinesweeperBoard CreateLayoutB() => CreateBoard(3, 3, (0, 0), (2, 2));

    [Fact]
    public void NewBoard_IsNotStartedAndEveryCellIsHidden()
    {
        MinesweeperBoard board = CreateLayoutA();

        Assert.Equal(GameStatus.NotStarted, board.Status);
        Assert.Equal(1, board.FlagsLeft);
        Assert.Equal(CellState.Hidden, board[1, 1].State);
    }

    [Fact]
    public void Indexer_OutsideTheBoard_Throws()
    {
        MinesweeperBoard board = CreateLayoutA();

        Assert.Throws<ArgumentOutOfRangeException>(() => board[3, 0]);
    }

    // --- Reveal ---

    [Fact]
    public void FirstReveal_IsNeverAMine()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    var board = new MinesweeperBoard(
                        new BoardSettings(4, 4, 10),
                        new RandomMinePlacer(new Random(seed)));

                    board.Reveal(row, col);

                    Assert.NotEqual(GameStatus.Lost, board.Status);
                    Assert.Equal(CellState.Revealed, board[row, col].State);
                }
            }
        }
    }

    [Fact]
    public void FirstReveal_StartsTheGame()
    {
        MinesweeperBoard board = CreateLayoutA();

        board.Reveal(1, 1);

        Assert.Equal(GameStatus.Playing, board.Status);
    }

    [Fact]
    public void Reveal_NumberedCell_ShowsTheCountAndDoesNotSpread()
    {
        MinesweeperBoard board = CreateLayoutA();

        board.Reveal(1, 1);

        Assert.Equal(1, board[1, 1].AdjacentMines);
        Assert.Equal(CellState.Hidden, board[2, 2].State);
    }

    [Fact]
    public void Reveal_EmptyCell_SpreadsToNeighbors()
    {
        MinesweeperBoard board = CreateLayoutA();

        board.Reveal(2, 2);

        Assert.Equal(CellState.Revealed, board[1, 1].State);
        Assert.Equal(CellState.Revealed, board[0, 2].State);
        Assert.Equal(CellState.Revealed, board[2, 0].State);
        Assert.Equal(CellState.Hidden, board[0, 0].State);
    }

    [Fact]
    public void Reveal_Mine_LosesAndExposesEveryMine()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        Assert.False(board[2, 2].IsMine);

        board.Reveal(0, 0);

        Assert.Equal(GameStatus.Lost, board.Status);
        Assert.Equal(CellState.Revealed, board[0, 0].State);
        Assert.True(board[2, 2].IsMine);
    }

    [Fact]
    public void Reveal_AfterLoss_ChangesNothing()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        board.Reveal(0, 0);

        board.Reveal(0, 1);

        Assert.Equal(CellState.Hidden, board[0, 1].State);
        Assert.Equal(GameStatus.Lost, board.Status);
    }

    [Fact]
    public void Reveal_AllSafeCells_Wins()
    {
        MinesweeperBoard board = CreateLayoutB();

        // Előbb az 5 számozott cella (nem terjednek), utána a 2 üres sarok.
        (int Row, int Column)[] safeCells = [(0, 1), (1, 0), (1, 1), (1, 2), (2, 1), (0, 2), (2, 0)];

        foreach ((int row, int col) in safeCells)
        {
            Assert.NotEqual(GameStatus.Won, board.Status);
            board.Reveal(row, col);
        }

        Assert.Equal(GameStatus.Won, board.Status);
    }

    [Fact]
    public void Reveal_WhenPlacerPutsAMineOnTheSafeCell_Throws()
    {
        MinesweeperBoard board = CreateLayoutA();

        Assert.Throws<InvalidOperationException>(() => board.Reveal(0, 0));
    }

    [Fact]
    public void Reveal_WhenPlacerReturnsTheWrongMineCount_Throws()
    {
        var board = new MinesweeperBoard(new BoardSettings(3, 3, 2), new FixedMinePlacer((0, 0)));

        Assert.Throws<InvalidOperationException>(() => board.Reveal(2, 2));
    }

    // --- ToggleFlag ---

    [Fact]
    public void ToggleFlag_CyclesBetweenHiddenAndFlagged_AndUpdatesFlagsLeft()
    {
        MinesweeperBoard board = CreateLayoutA();

        board.ToggleFlag(1, 1);
        Assert.Equal(CellState.Flagged, board[1, 1].State);
        Assert.Equal(0, board.FlagsLeft);

        board.ToggleFlag(1, 1);
        Assert.Equal(CellState.Hidden, board[1, 1].State);
        Assert.Equal(1, board.FlagsLeft);
    }

    [Fact]
    public void ToggleFlag_BeforeTheFirstReveal_DoesNotStartTheGame()
    {
        MinesweeperBoard board = CreateLayoutA();

        board.ToggleFlag(1, 1);

        Assert.Equal(GameStatus.NotStarted, board.Status);
    }

    [Fact]
    public void ToggleFlag_OnRevealedCell_DoesNothing()
    {
        MinesweeperBoard board = CreateLayoutA();
        board.Reveal(1, 1);

        board.ToggleFlag(1, 1);

        Assert.Equal(CellState.Revealed, board[1, 1].State);
        Assert.Equal(1, board.FlagsLeft);
    }

    [Fact]
    public void ToggleFlag_AfterLoss_DoesNothing()
    {
        MinesweeperBoard board = CreateLayoutB();
        board.Reveal(1, 1);
        board.Reveal(0, 0);

        board.ToggleFlag(0, 1);

        Assert.Equal(CellState.Hidden, board[0, 1].State);
        Assert.Equal(2, board.FlagsLeft);
    }

    [Fact]
    public void Reveal_FlaggedCell_StaysFlagged()
    {
        MinesweeperBoard board = CreateLayoutA();
        board.ToggleFlag(1, 1);

        board.Reveal(1, 1);

        Assert.Equal(CellState.Flagged, board[1, 1].State);
    }

    [Fact]
    public void Spread_SkipsFlaggedCell_AndWinNeedsTheFlagRemoved()
    {
        MinesweeperBoard board = CreateLayoutA();
        board.ToggleFlag(1, 1);

        board.Reveal(2, 2);

        Assert.Equal(CellState.Flagged, board[1, 1].State);
        Assert.Equal(CellState.Revealed, board[2, 1].State);
        Assert.Equal(GameStatus.Playing, board.Status);

        board.ToggleFlag(1, 1);
        board.Reveal(1, 1);

        Assert.Equal(GameStatus.Won, board.Status);
    }
}
