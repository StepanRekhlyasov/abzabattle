<template>
    <div class="battle-grid">
        <div v-for="row, rowIndex in battleMapData.sectors" :key="rowIndex" class="battle-grid-row">
            <battle-sector v-for="sector, index in row" :key="index" :sector-data="sector" class="battle-grid-sector-item" @sector-click="(data) => onSectorClick(data, sector)"></battle-sector>
        </div>
    </div>
</template>
<script setup lang="ts">
import type { BattleMap } from '@/types/map.ts';
import BattleSector from './BattleSector.vue';
import type { BattleSector as BattleSectorType } from '@/types/map';

const props = defineProps<{
    battleMapData: BattleMap;
}>();

const emit = defineEmits<{
    (e: 'sector-click', sector: BattleSectorType): void;
}>();

const onSectorClick = (data: any, sector: BattleSectorType) => {
    emit('sector-click', sector);
}
</script>
<style scoped lang="scss">
.battle-grid {
    display: flex;
    flex-direction: column;
    gap: 10px;
    .battle-grid-row {
        display: flex;
        gap: 10px;
    }
}
</style>
