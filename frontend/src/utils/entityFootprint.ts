import { EntityRotation, type CellOffset, type EntityType, type MapPosition } from '@/types/entity';
import { getEntityDefinition } from '@/data/entityDefinitions';

export function rotateOffset(offset: CellOffset, rotation: EntityRotation): CellOffset {
    const { x, y } = offset;
    switch (rotation) {
        case EntityRotation.R90: return { x: -y, y: x };
        case EntityRotation.R180: return { x: -x, y: -y };
        case EntityRotation.R270: return { x: y, y: -x };
        default: return { x, y };
    }
}

export function getFootprint(type: EntityType, rotation: EntityRotation = EntityRotation.R0): CellOffset[] {
    return getEntityDefinition(type).footprint.map(offset => rotateOffset(offset, rotation));
}

export function getEntityCells(type: EntityType, anchor: MapPosition, rotation: EntityRotation = EntityRotation.R0): MapPosition[] {
    return getFootprint(type, rotation).map(offset => ({ x: anchor.x + offset.x, y: anchor.y + offset.y }));
}

export function rotateEntityRotation(rotation: EntityRotation): EntityRotation {
    switch (rotation) {
        case EntityRotation.R0: return EntityRotation.R90;
        case EntityRotation.R90: return EntityRotation.R180;
        case EntityRotation.R180: return EntityRotation.R270;
        default: return EntityRotation.R0;
    }
}
