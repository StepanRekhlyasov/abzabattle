import type { BattleMap } from "./map";
import type { Player } from "./player";

export type Session = {
    id: string;
    name: string;
    rebel: {
        player: Player;
        battleMap: BattleMap;
    }
    imperial: {
        player: Player;
        battleMap: BattleMap;
    }
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