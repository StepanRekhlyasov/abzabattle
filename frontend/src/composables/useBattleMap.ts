import type { BattleMap, BattleMapOptions } from "@/types/map";
import { EntityType, type Entity } from "@/types/entity";

function shuffle<T>(items: T[]): T[] {
    const result = [...items];

    for (let i = result.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [result[i], result[j]] = [result[j], result[i]];
    }

    return result;
}

export const useBattleMap = () => {
    const generateBattleMap = (options: BattleMapOptions) => {
        const map: BattleMap = {
            sectors: Array.from({ length: options.size.y }, () => Array.from({ length: options.size.x }, () => ({ entity: { type: EntityType.Empty } as Entity, hidden: true, destroyed: false }))),
        }
        return map;
    }

    const deployEntities = (entities: Entity[], battleMap: BattleMap) => {
        const positions: { y: number; x: number }[] = [];

        battleMap.sectors.forEach((row, y) => {
            row.forEach((sector, x) => {
                if (sector.entity.type === EntityType.Empty) {
                    positions.push({ y, x });
                }
            });
        });

        const shuffledPositions = shuffle(positions);
        const deployCount = Math.min(entities.length, shuffledPositions.length);

        for (let i = 0; i < deployCount; i++) {
            const { y, x } = shuffledPositions[i];
            battleMap.sectors[y][x].entity = entities[i];
        }

        return battleMap;
    }

    return {
        generateBattleMap,
        deployEntities,
    }
}