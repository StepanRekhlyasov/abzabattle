import type { BattleMap } from "./map";
import type { Player } from "./player";

export type Session = {
    id: string;
    battleMap: BattleMap;
    players: Player[];
    currentPlayer: Player;
    currentRound: number;
}