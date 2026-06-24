import type { BattleMap } from "./map";
import type { Player } from "./player";

export type Session = {
    id: string;
    name: string;
    battleMap: BattleMap;
    rebel: Player | null;
    imperial: Player | null;
    currentTurn: Faction;
    status: SessionStatus;
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