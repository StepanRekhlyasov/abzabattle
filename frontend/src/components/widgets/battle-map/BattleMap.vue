<template>
    <div class="battle-grid">
        <div v-for="row, rowIndex in battleMapData.sectors" :key="rowIndex" class="battle-grid-row">
            <battle-sector 
                v-for="sector, index in row" 
                :key="index" 
                :sector-data="sector" 
                class="battle-grid-sector-item" 
                @sector-click="onSectorClick(sector)"
                :dense-mode="denseMode"
            />
        </div>
    </div>
</template>
<script setup lang="ts">
import type { BattleMap } from '@/types/map.ts';
import BattleSector from './BattleSector.vue';
import type { BattleSector as BattleSectorType } from '@/types/map';
import { computed } from 'vue';

const props = defineProps<{
    battleMapData: BattleMap;
}>();

const emit = defineEmits<{
    (e: 'sector-click', sector: BattleSectorType): void;
}>();

const onSectorClick = (sector: BattleSectorType) => {
    emit('sector-click', sector);
}
const denseMode = computed(() => {
    return props.battleMapData.sectors.length > 12;
});
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
