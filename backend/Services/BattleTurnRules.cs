using backend.Models;

namespace backend.Services;

public static class BattleTurnRules
{
    public const int MaxHitsPerTurn = 3;

    public static void ApplyNormalAttackOutcome(GameSession session, StrikeOutcome outcome)
    {
        if (outcome is StrikeOutcome.Miss or StrikeOutcome.MineHit)
        {
            EndTurn(session);
            return;
        }

        if (outcome is StrikeOutcome.Hit or StrikeOutcome.ShieldBreak)
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
