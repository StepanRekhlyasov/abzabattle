import { toUnitAbility } from '@/data/unitAbilities';
import type { UnitAbility } from '@/data/unitAbilities';
import { EntityType } from '@/types/entity';
import type { BattleMap } from '@/types/map';

export function getAbilitiesFromBattleMap(battleMap: BattleMap | null): UnitAbility[] {
    if (!battleMap) return [];

    const units = new Map<string, { type: EntityType; alive: boolean }>();

    battleMap.sectors.forEach((row, y) => {
        row.forEach((sector, x) => {
            const { entity, destroyed } = sector;
            if (entity.type === EntityType.Empty) return;

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

    return Array.from(units.entries())
        .filter(([, unit]) => unit.alive)
        .map(([entityId, unit]) => toUnitAbility(entityId, unit.type))
        .filter((ability): ability is UnitAbility => ability !== null);
}
