import { EntityType } from '@/types/entity';
import { getDeploymentUnitAsset } from '@/data/deploymentUnits';

export enum AbilityKind {
    DeployTieFighter = 'deploy-tie-fighter',
    PlaceSpaceMine = 'place-space-mine',
    SingleReactorIgnition = 'single-reactor-ignition',
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

export type PassiveAbility = {
    id: string;
    name: string;
    description: string;
    image: string;
};

export const TIE_FIGHTERS_PER_ABILITY = 3;
export const DEATH_STAR_DESTROY_CHANCE = 33;

const PASSIVE_ABILITY_DEFINITIONS: Partial<Record<EntityType, Omit<PassiveAbility, 'id' | 'image'>>> = {
    [EntityType.DeathStar]: {
        name: 'Fatal Construction Flaw',
        description: 'A fatal design flaw leaves the Death Star vulnerable: an X-Wing firing at the reactor (central sector) has a ' + DEATH_STAR_DESTROY_CHANCE + '% chance to destroy the entire station!',
    },
};

const ABILITY_DEFINITIONS: Partial<Record<EntityType, AbilityDefinition>> = {
    [EntityType.StarDestroyer]: {
        name: 'Tie Fighter Reinforcement',
        description: 'Deploy a Tie Fighter to the sector. Does not end your turn.',
        kind: AbilityKind.DeployTieFighter,
        target: 'own',
    },
    [EntityType.GozantiClassCruiser]: {
        name: 'Space Mines',
        description: 'Place a Space Mine on your map. Space Mine ends the opponent\'s turn on hit. Does not end your turn.',
        kind: AbilityKind.PlaceSpaceMine,
        target: 'own',
    },
    [EntityType.DeathStar]: {
        name: 'Single-reactor Ignition',
        description: 'Fire at a sector on the opponent map. Destroys the target sector and all adjacent sectors. Does not end your turn.',
        kind: AbilityKind.SingleReactorIgnition,
        target: 'opponent',
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
        name: 'One In a Million',
        description: 'Fire at a sector on the opponent map. Destroys Tie Fighters. Reveals other units without destroying them. Has a ' + DEATH_STAR_DESTROY_CHANCE + '% chance to destroy the Death Star when hitting the reactor (central sector). Does not end your turn.',
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

export function getPassiveAbilityDefinition(type: EntityType): Omit<PassiveAbility, 'id' | 'image'> | null {
    return PASSIVE_ABILITY_DEFINITIONS[type] ?? null;
}

export function toPassiveAbility(entityId: string, type: EntityType): PassiveAbility | null {
    const definition = getPassiveAbilityDefinition(type);
    if (!definition) return null;

    return {
        id: `${entityId}-passive`,
        name: definition.name,
        description: definition.description,
        image: getDeploymentUnitAsset(type).image,
    };
}

export function getAbilityDefinition(type: EntityType): AbilityDefinition | null {
    return ABILITY_DEFINITIONS[type] ?? null;
}

export type UnitDraftTrait = {
    label: 'Ability' | 'Passive Trait';
    name: string;
    description: string;
    passive: boolean;
};

export function getUnitDraftTraits(type: EntityType): UnitDraftTrait[] {
    const traits: UnitDraftTrait[] = [];
    const ability = getAbilityDefinition(type);

    if (ability) {
        traits.push({
            label: 'Ability',
            name: ability.name,
            description: ability.description,
            passive: false,
        });
    }

    const passive = getPassiveAbilityDefinition(type);
    if (passive) {
        traits.push({
            label: 'Passive Trait',
            name: passive.name,
            description: passive.description,
            passive: true,
        });
    }

    return traits;
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
