import type { Entity } from '@/types/entity';
import { EntityRotation } from '@/types/entity';
import type { BattleMap } from '@/types/map';
import type { Faction } from '@/types/session';
import { defineStore } from 'pinia';
import { rotateEntityRotation } from '@/utils/entityFootprint';

export const useDraftStore = defineStore('draft', {
    state: () => ({
        selectedEntity: null as Entity | null,
        selectedRotation: EntityRotation.R0,
        ptsSpent: 0,
        ptsLimit: 100,
        selectedFaction: 'imperial' as Faction | null,
        battleMap: null as BattleMap | null,
    }),
    getters: {
        ptsRemaining: state => state.ptsLimit - state.ptsSpent,
        isPtsOverLimit: state => state.ptsLimit - state.ptsSpent < 0,
    },
    actions: {
        selectEntity(entity: Entity | null) {
            this.selectedEntity = entity;
        },
        rotateSelectedEntity() {
            this.selectedRotation = rotateEntityRotation(this.selectedRotation);
            if (this.selectedEntity) {
                this.selectedEntity = { ...this.selectedEntity, rotation: this.selectedRotation };
            }
        },
        setPtsSpent(ptsSpent: number) {
            this.ptsSpent = ptsSpent;
        },
        addPtsSpent(cost: number) {
            this.ptsSpent += cost;
        },
    },
    persist: true,
});
