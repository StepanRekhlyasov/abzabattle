import type { Entity } from "@/types/entity";
import type { BattleMap } from "@/types/map";
import type { Faction } from "@/types/session";
import { defineStore } from "pinia";

export const useDraftStore = defineStore('draft', {
    state: () => ({
        selectedEntity: null as Entity | null,
        ptsSpent: 0,
        ptsLimit: 100,
        selectedFaction: 'imperial' as Faction | null,
        battleMap: null as BattleMap | null,
    }),
    actions: {
        selectEntity(entity: Entity) {
            this.selectedEntity = entity;
        },
        setPtsSpent(ptsSpent: number) {
            this.ptsSpent = ptsSpent;
        },
        setPtsLimit(ptsLimit: number) {
            this.ptsLimit = ptsLimit;
        },
        resetDraft() {
            this.$reset();
        },
    },
}); 