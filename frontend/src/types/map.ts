import type { Entity } from './entity';
import type { Player } from './player';

export type BattleMap = {
    id?: string;
    options?: BattleMapOptions;
    sectors: BattleSector[][];
}

export type BattleSector<T extends Entity = Entity> = {
    id?: string;
    player?: Player;
    entity: T;
    hidden: boolean;
    destroyed: boolean;
}

export type BattleMapOptions = {
    size: {
        x: number;
        y: number;
    };
}