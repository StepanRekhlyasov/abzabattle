import { EntityType, type CellOffset, type EntityDefinition } from '@/types/entity';

const STAR_DESTROYER_FOOTPRINT: CellOffset[] = [
    { x: 0, y: -2 },
    { x: -1, y: -1 },
    { x: 0, y: -1 },
    { x: 1, y: -1 },
    { x: -2, y: 0 },
    { x: -1, y: 0 },
    { x: 0, y: 0 },
    { x: 1, y: 0 },
    { x: 2, y: 0 },
];

const DEATH_STAR_FOOTPRINT: CellOffset[] = [
    { x: -1, y: -2 }, { x: 0, y: -2 }, { x: 1, y: -2 },
    { x: -2, y: -1 }, { x: -1, y: -1 }, { x: 0, y: -1 }, { x: 1, y: -1 }, { x: 2, y: -1 },
    { x: -2, y: 0 }, { x: -1, y: 0 }, { x: 0, y: 0 }, { x: 1, y: 0 }, { x: 2, y: 0 },
    { x: -2, y: 1 }, { x: -1, y: 1 }, { x: 0, y: 1 }, { x: 1, y: 1 }, { x: 2, y: 1 },
    { x: -1, y: 2 }, { x: 0, y: 2 }, { x: 1, y: 2 },
];

const MON_CALAMARI_FOOTPRINT: CellOffset[] = [
    { x: -2, y: 0 },
    { x: -1, y: 0 },
    { x: 0, y: 0 },
    { x: 1, y: 0 },
    { x: 2, y: 0 },
];

const GOZANTI_CLASS_CRUISER_FOOTPRINT: CellOffset[] = [
    { x: -1, y: 0 },
    { x: 0, y: 0 },
    { x: 1, y: 0 },
];

const NEBULON_FRIGATE_FOOTPRINT: CellOffset[] = [
    { x: -1, y: 1 },
    { x: -1, y: 0 },
    { x: 0, y: 0 },
    { x: 1, y: 0 },
];

export const ENTITY_DEFINITIONS: Record<EntityType, EntityDefinition | null> = {
    [EntityType.Empty]: null,
    [EntityType.Letter]: { ptsCost: 0, footprint: [{ x: 0, y: 0 }] },
    [EntityType.StarDestroyer]: { ptsCost: 40, footprint: STAR_DESTROYER_FOOTPRINT, content: 'SD' },
    [EntityType.DeathStar]: { ptsCost: 100, footprint: DEATH_STAR_FOOTPRINT, content: '★' },
    [EntityType.MonCalamari]: { ptsCost: 30, footprint: MON_CALAMARI_FOOTPRINT, content: 'MC' },
    [EntityType.TieFighter]: { ptsCost: 10, footprint: [{ x: 0, y: 0 }], content: 'TF' },
    [EntityType.GozantiClassCruiser]: { ptsCost: 25, footprint: GOZANTI_CLASS_CRUISER_FOOTPRINT, content: 'GZ' },
    [EntityType.SpaceMine]: { ptsCost: 0, footprint: [{ x: 0, y: 0 }], content: 'SM' },
    [EntityType.NebulonFrigate]: { ptsCost: 20, footprint: NEBULON_FRIGATE_FOOTPRINT, content: 'NF' },
    [EntityType.XWing]: { ptsCost: 15, footprint: [{ x: 0, y: 0 }], content: 'XW' },
    [EntityType.UWing]: { ptsCost: 20, footprint: [{ x: 0, y: 0 }], content: 'UW' },
};

export function getEntityDefinition(type: EntityType): EntityDefinition {
    const definition = ENTITY_DEFINITIONS[type];
    if (!definition) throw new Error(`Entity definition not found for type: ${type}`);
    return definition;
}
