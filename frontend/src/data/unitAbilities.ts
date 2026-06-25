import { EntityType } from '@/types/entity';
import { getDeploymentUnitAsset } from '@/data/deploymentUnits';

export enum AbilityKind {
    DeployTieFighter = 'deploy-tie-fighter',
    PlaceSpaceMine = 'place-space-mine',
    OpponentStrike = 'opponent-strike',
    PlaceShield = 'place-shield',
    AirborneSuperiority = 'airborne-superiority',
    Bombardment = 'bombardment',
}

export type AbilityTarget = 'own' | 'opponent';

export type AbilityDefinition = {
    name: string;
    description: string;
    kind: AbilityKind;
    target: AbilityTarget;
};

export type UnitAbility = AbilityDefinition & {
    entityId: string;
    image: string;
};

export const TIE_FIGHTERS_PER_ABILITY = 3;

const ABILITY_DEFINITIONS: Partial<Record<EntityType, AbilityDefinition>> = {
    [EntityType.StarDestroyer]: {
        name: 'Tie Fighter Reinforcement',
        description: 'Deploy a Tie Fighter to the sector. Does not end your turn.',
        kind: AbilityKind.DeployTieFighter,
        target: 'own',
    },
    [EntityType.GozantiClassCruiser]: {
        name: 'Space Mines',
        description: 'Place a Space Mine on your map. Does not end your turn.',
        kind: AbilityKind.PlaceSpaceMine,
        target: 'own',
    },
    [EntityType.TieFighter]: {
        name: 'Swarm Tactics',
        description: 'For each '+ TIE_FIGHTERS_PER_ABILITY + 'th Tie Fighter deployed on the map, you can attack a single sector on the opponent map. Does not end your turn.',
        kind: AbilityKind.OpponentStrike,
        target: 'opponent',
    },
    [EntityType.MonCalamari]: {
        name: 'Turbolaser Batteries',
        description: 'Unleash a concentrated volley of turbolasers at a single sector on the opponent map. Does not end your turn.',
        kind: AbilityKind.OpponentStrike,
        target: 'opponent',
    },
    [EntityType.NebulonFrigate]: {
        name: 'Deflector Shield',
        description: 'Place a shield on a friendly hidden unit. Cannot target the Nebulon itself. Does not end your turn',
        kind: AbilityKind.PlaceShield,
        target: 'own',
    },
    [EntityType.XWing]: {
        name: 'Airborne Superiority',
        description: 'Fire at a sector on the opponent map. Destroys Tie Fighters. Reveals other units without destroying them. Does not end your turn.',
        kind: AbilityKind.AirborneSuperiority,
        target: 'opponent',
    },
    [EntityType.UWing]: {
        name: 'Bombardment',
        description: 'Fire at a sector on the opponent map. Reveals Tie Fighters without destroying them. Does not end your turn.',
        kind: AbilityKind.Bombardment,
        target: 'opponent',
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
        kind: definition.kind,
        target: definition.target,
        image: getDeploymentUnitAsset(type).image,
    };
}

export function toSwarmAbilities(type: EntityType, aliveCount: number, idPrefix: string): UnitAbility[] {
    const definition = getAbilityDefinition(type);
    if (!definition) return [];

    const abilityCount = Math.floor(aliveCount / TIE_FIGHTERS_PER_ABILITY);
    const image = getDeploymentUnitAsset(type).image;

    return Array.from({ length: abilityCount }, (_, index) => ({
        entityId: `${idPrefix}-${index}`,
        name: definition.name,
        description: definition.description,
        kind: definition.kind,
        target: definition.target,
        image,
    }));
}
