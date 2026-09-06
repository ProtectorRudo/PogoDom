namespace PogoDom.Core
{
    public interface IBotBrain
    {
        Direction ChooseDirection(MatchState state, PlayerState bot, MatchConfig config, IRandomSource random);
    }
}
