import { EntityType } from '@/types/entity';
import { getDeploymentUnitAsset } from '@/data/deploymentUnits';

export type AbilityDefinition = {
    name: string;
    description: string;
};

export type UnitAbility = AbilityDefinition & {
    entityId: string;
    image: string;
};

const ABILITY_DEFINITIONS: Partial<Record<EntityType, AbilityDefinition>> = {
    [EntityType.StarDestroyer]: {
        name: 'Tie Fighter Reinforcement',
        description: 'Deploy a Tie Fighter to the sector. Does not end your turn.',
    },
    [EntityType.TieFighter]: {
        name: 'Swarm Tactics',
        description: 'For each 5th Tie Fighter deployed on the map, you can attack a single sector on the opponent map. Does not end your turn.',
    },
    [EntityType.MonCalamari]: {
        name: 'Turbolaser Batteries',
        description: 'Unleash a concentrated volley of turbolasers at a single sector on the opponent map. Does not end your turn.',
    },
};

export function getAbilityDefinition(type: EntityType): AbilityDefinition | null {
    return ABILITY_DEFINITIONS[type] ?? null;
}

export function toUnitAbility(entityId: string, type: EntityType): UnitAbility | null {
    const definition = getAbilityDefinition(type);
    if (!definition) return null;

    return {
        entityId,
        name: definition.name,
        description: definition.description,
        image: getDeploymentUnitAsset(type).image,
    };
}
