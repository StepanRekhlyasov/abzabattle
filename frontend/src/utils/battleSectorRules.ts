import type { BattleSector } from '@/types/map';
import { EntityType } from '@/types/entity';
import { AbilityKind } from '@/data/unitAbilities';

export function canAttackSector(sector: BattleSector, abilityKind?: AbilityKind): boolean {
    if (!sector.destroyed) return true;

    if (abilityKind === AbilityKind.AirborneSuperiority || abilityKind === AbilityKind.Bombardment) {
        return sector.entity.type === EntityType.DeathStar;
    }

    return false;
}

export function canPlaceShieldOnSector(
    sector: BattleSector,
    nebulonEntityId: string,
): boolean {
    if (sector.destroyed) return false;
    if (sector.entity.type === EntityType.Empty) return false;
    if (sector.entity.type === EntityType.NebulonFrigate) return false;
    if (sector.shielded) return false;
    if (sector.entity.id === nebulonEntityId) return false;
    return true;
}

export function isShieldVisible(sector: BattleSector): boolean {
    return !!sector.shielded && !sector.destroyed && !sector.hidden;
}
