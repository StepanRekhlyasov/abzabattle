import { toSwarmAbilities, toUnitAbility, toPassiveAbility } from '@/data/unitAbilities';
import type { UnitAbility, PassiveAbility } from '@/data/unitAbilities';
import { EntityType } from '@/types/entity';
import type { BattleMap } from '@/types/map';

export function getAbilitiesFromBattleMap(battleMap: BattleMap | null): UnitAbility[] {
    if (!battleMap) return [];

    const units = new Map<string, { type: EntityType; alive: boolean }>();

    battleMap.sectors.forEach((row, y) => {
        row.forEach((sector, x) => {
            const { entity, destroyed } = sector;
            if (entity.type === EntityType.Empty || entity.type === EntityType.SpaceMine) return;

            const entityId = entity.id ?? `${entity.type}-${x}-${y}`;
            const existing = units.get(entityId);

            if (!existing) {
                units.set(entityId, { type: entity.type, alive: !destroyed });
                return;
            }

            if (!destroyed) {
                existing.alive = true;
            }
        });
    });

    const abilities: UnitAbility[] = [];
    let aliveTieFighters = 0;

    for (const [entityId, unit] of units.entries()) {
        if (!unit.alive) continue;

        if (unit.type === EntityType.TieFighter) {
            aliveTieFighters++;
            continue;
        }

        const ability = toUnitAbility(entityId, unit.type);
        if (ability) {
            abilities.push(ability);
        }
    }

    abilities.push(...toSwarmAbilities(EntityType.TieFighter, aliveTieFighters, 'tie-fighter-swarm'));

    return abilities;
}

export function getPassiveAbilitiesFromBattleMap(battleMap: BattleMap | null): PassiveAbility[] {
    if (!battleMap) return [];

    const units = new Map<string, { type: EntityType; alive: boolean }>();

    battleMap.sectors.forEach((row, y) => {
        row.forEach((sector, x) => {
            const { entity, destroyed } = sector;
            if (entity.type === EntityType.Empty || entity.type === EntityType.SpaceMine) return;

            const entityId = entity.id ?? `${entity.type}-${x}-${y}`;
            const existing = units.get(entityId);

            if (!existing) {
                units.set(entityId, { type: entity.type, alive: !destroyed });
                return;
            }

            if (!destroyed) {
                existing.alive = true;
            }
        });
    });

    const passives: PassiveAbility[] = [];

    for (const [entityId, unit] of units.entries()) {
        if (!unit.alive) continue;

        const passive = toPassiveAbility(entityId, unit.type);
        if (passive) {
            passives.push(passive);
        }
    }

    return passives;
}
