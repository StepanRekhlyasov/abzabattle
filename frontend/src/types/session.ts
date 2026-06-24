import type { BattleMap } from './map';
import type { Player } from './player';

export type SessionSide = {
    player: Player | null;
    battleMap: BattleMap | null;
}

export type Session = {
    id: string;
    name: string;
    ptsLimit: number;
    mapSize: number;
    rebel: SessionSide;
    imperial: SessionSide;
    currentTurn: Faction;
    status: SessionStatus;
    canJoin?: boolean;
    creatorPlayerName: string;
}

export enum SessionStatus {
    Pending = 'pending',
    InProgress = 'in_progress',
    Finished = 'finished',
}

export enum Faction {
    Imperial = 'imperial',
    Rebel = 'rebel',
}

export function oppositeFaction(faction: Faction): Faction {
    return faction === Faction.Imperial ? Faction.Rebel : Faction.Imperial;
}
