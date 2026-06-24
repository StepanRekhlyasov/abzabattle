import type { BattleMap, BattleMapOptions } from '@/types/map';
import { EntityRotation, EntityType, type Entity, type MapPosition } from '@/types/entity';
import { getEntityDefinition } from '@/data/entityDefinitions';
import { getEntityCells } from '@/utils/entityFootprint';

function shuffle<T>(items: T[]): T[] {
    const result = [...items];
    for (let i = result.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [result[i], result[j]] = [result[j], result[i]];
    }
    return result;
}

function isInsideMap(battleMap: BattleMap, position: MapPosition) {
    return position.y >= 0 && position.y < battleMap.sectors.length && position.x >= 0 && position.x < battleMap.sectors[position.y].length;
}

function getSector(battleMap: BattleMap, position: MapPosition) {
    return battleMap.sectors[position.y][position.x];
}

function getEntityRotation(entity: Entity): EntityRotation {
    return entity.rotation ?? EntityRotation.R0;
}

function positionKey(position: MapPosition) {
    return `${position.x},${position.y}`;
}

function getAdjacentPositions(position: MapPosition): MapPosition[] {
    const adjacent: MapPosition[] = [];
    for (let dy = -1; dy <= 1; dy++) {
        for (let dx = -1; dx <= 1; dx++) {
            if (dx !== 0 || dy !== 0) adjacent.push({ x: position.x + dx, y: position.y + dy });
        }
    }
    return adjacent;
}

function isSectorEmpty(battleMap: BattleMap, position: MapPosition) {
    return isInsideMap(battleMap, position) && getSector(battleMap, position).entity.type === EntityType.Empty;
}

export const useBattleMap = () => {
    const generateBattleMap = (options: BattleMapOptions): BattleMap => ({
        options,
        sectors: Array.from({ length: options.size.y }, () => Array.from({ length: options.size.x }, () => ({
            entity: { type: EntityType.Empty } as Entity,
            hidden: false,
            destroyed: false,
        }))),
    });

    const canPlaceEntity = (entity: Entity, anchor: MapPosition, battleMap: BattleMap) => {
        if (entity.type === EntityType.Empty) return false;
        const cells = getEntityCells(entity.type, anchor, getEntityRotation(entity));
        const footprintKeys = new Set(cells.map(positionKey));
        if (!cells.every(position => isSectorEmpty(battleMap, position))) return false;
        return cells.every(position => getAdjacentPositions(position).every(adjacent => {
            if (footprintKeys.has(positionKey(adjacent)) || !isInsideMap(battleMap, adjacent)) return true;
            return isSectorEmpty(battleMap, adjacent);
        }));
    };

    const getPlacementPreview = (entity: Entity, anchor: MapPosition, battleMap: BattleMap) => {
        const cells = getEntityCells(entity.type, anchor, getEntityRotation(entity));
        return { cells, isValid: canPlaceEntity(entity, anchor, battleMap) };
    };

    const placeEntity = (entity: Entity, anchor: MapPosition, battleMap: BattleMap, isHidden = false) => {
        if (!canPlaceEntity(entity, anchor, battleMap)) return false;
        const definition = getEntityDefinition(entity.type);
        const placedEntity = { ...entity, id: entity.id ?? crypto.randomUUID(), content: entity.content ?? definition.content };
        getEntityCells(placedEntity.type, anchor, getEntityRotation(placedEntity)).forEach((position) => {
            const sector = getSector(battleMap, position);
            sector.entity = { ...placedEntity };
            sector.hidden = isHidden;
            sector.destroyed = false;
        });
        return true;
    };

    const randomlyDeployEntities = (entities: Entity[], battleMap: BattleMap) => {
        const availableAnchors: MapPosition[] = [];
        battleMap.sectors.forEach((row, y) => row.forEach((sector, x) => {
            if (sector.entity.type === EntityType.Empty) availableAnchors.push({ x, y });
        }));
        const shuffledAnchors = shuffle(availableAnchors);
        for (const entity of entities) {
            if (entity.type === EntityType.Empty) continue;
            const anchorIndex = shuffledAnchors.findIndex(anchor => canPlaceEntity(entity, anchor, battleMap));
            if (anchorIndex === -1) continue;
            const [anchor] = shuffledAnchors.splice(anchorIndex, 1);
            placeEntity(entity, anchor, battleMap, true);
        }
        return battleMap;
    };

    const prepareBattleMapForBattle = (battleMap: BattleMap): BattleMap => {
        const prepared = JSON.parse(JSON.stringify(battleMap)) as BattleMap;
        prepared.sectors.forEach(row => row.forEach(sector => {
            sector.hidden = true;
        }));
        return prepared;
    };

    return { generateBattleMap, randomlyDeployEntities, getPlacementPreview, placeEntity, prepareBattleMapForBattle };
};
