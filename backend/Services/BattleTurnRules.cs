using backend.Models;

namespace backend.Services;

public static class BattleTurnRules
{
    public const int MaxHitsPerTurn = 3;

    public static void ApplyNormalAttackOutcome(GameSession session, StrikeOutcome outcome)
    {
        if (outcome == StrikeOutcome.Miss)
        {
            EndTurn(session);
            return;
        }

        if (outcome is StrikeOutcome.Hit or StrikeOutcome.ShieldReveal or StrikeOutcome.ShieldBreak)
        {
            session.HitsThisTurn++;
            if (session.HitsThisTurn >= MaxHitsPerTurn)
            {
                EndTurn(session);
            }
        }
    }

    public static void EndTurn(GameSession session)
    {
        session.CurrentTurn = Faction.Opposite(session.CurrentTurn);
        session.HitsThisTurn = 0;
    }
}
