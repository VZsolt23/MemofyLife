namespace MemofyLife.Features.Arcade.Shared;

public interface IGame
{
    string Key { get; }

    string DisplayName { get; }

    string Icon { get; }

    string Route { get; }

    IReadOnlyList<GameVariant> Variants { get; }

    BestScoreKind BestScoreKind { get; }
}

public record GameVariant(string Key, string DisplayName);

public enum BestScoreKind
{
    HigherIsBetter = 1,
    LowerIsBetter = 2,
}
