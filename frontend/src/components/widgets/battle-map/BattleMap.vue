<template>
    <div class="battle-grid" @mouseleave="emit('sector-leave')">
        <div v-for="(row, rowIndex) in battleMapData.sectors" :key="rowIndex" class="battle-grid-row">
            <battle-sector
                v-for="(sector, index) in row"
                :key="index"
                :sector-data="sector"
                :isEntityAlive="isEntityAlive(sector.entity.id)"
                :preview-state="getPreviewState(index, rowIndex)"
                :dense-mode="denseMode"
                @sector-click="emit('sector-click', { sector, x: index, y: rowIndex })"
                @sector-contextmenu="emit('sector-contextmenu', { sector, x: index, y: rowIndex })"
                @sector-hover="emit('sector-hover', { x: index, y: rowIndex })"
            />
        </div>
    </div>
</template>
<script setup lang="ts">
import type { BattleMap } from '@/types/map';
import type { BattleSector as BattleSectorType } from '@/types/map';
import BattleSector from './BattleSector.vue';
import { computed } from 'vue';
import { useBattleMap } from '@/composables/useBattleMap';

const props = defineProps<{
    battleMapData: BattleMap;
    previewCells?: string[];
    previewValid?: boolean;
}>();

const emit = defineEmits<{
    (e: 'sector-click', payload: { sector: BattleSectorType; x: number; y: number }): void;
    (e: 'sector-contextmenu', payload: { sector: BattleSectorType; x: number; y: number }): void;
    (e: 'sector-hover', payload: { x: number; y: number }): void;
    (e: 'sector-leave'): void;
}>();

const denseMode = computed(() => props.battleMapData.sectors.length > 12);

const getPreviewState = (x: number, y: number): 'valid' | 'invalid' | null => {
    const key = `${x},${y}`;
    if (!props.previewCells?.includes(key)) return null;
    return props.previewValid ? 'valid' : 'invalid';
};

const isEntityAlive = (entityId?: string) => {
    if(!entityId) return false;
    return useBattleMap().isEntityAlive(entityId, props.battleMapData);
};
</script>
<style scoped lang="scss">
.battle-grid {
    display: flex;
    flex-direction: column;
    gap: v-bind('denseMode ? "5px" : "10px"');
    .battle-grid-row {
        display: flex;
        gap: v-bind('denseMode ? "5px" : "10px"');
    }
}
</style>
