<template>
    <div class="deploy-roster">
        <div class="deploy-roster-title">
            Your faction: {{ selectedFaction }}
            <img src="@/assets/icons/empire.svg" alt="Imperial" v-if="selectedFaction === 'imperial'" width="50"/>
            <img src="@/assets/icons/rebel.svg" alt="Rebel" v-if="selectedFaction === 'rebel'" width="50"/>
        </div>
    </div>
</template>
<script setup lang="ts">
import type { Faction } from '@/types/session';
import type { BattleMap } from '@/types/map';

const props = defineProps<{
    selectedFaction: Faction;
    ptsLimit: number;
    battleMap: BattleMap;
}>();
</script>
<style scoped lang="scss">
.deploy-roster {
    width: 100%;
    height: 100%;
    background-color: var(--color-sector-exposed);
    border-radius: 10px;
}
.deploy-roster-title {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    font-size: 20px;
    font-weight: 500;
    color: #ffffff;
}
</style>