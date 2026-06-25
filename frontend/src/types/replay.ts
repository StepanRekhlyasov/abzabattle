import type { BattleMap } from './map';
import type { Faction } from './session';
import type { SessionActionLog } from './turnHistory';

export type ReplaySnapshot = {
    rebelBattleMap: BattleMap | null;
    imperialBattleMap: BattleMap | null;
    currentTurn: Faction;
    hitsThisTurn: number;
};

export type ReplayFrame = {
    log: SessionActionLog;
    message: string;
    snapshot: ReplaySnapshot;
};

export type SessionActionPayload = {
    snapshot?: ReplaySnapshot;
};
