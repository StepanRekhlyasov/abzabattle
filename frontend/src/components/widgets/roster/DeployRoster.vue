<template>
    <div class="deploy-roster">
        <div class="deploy-roster-title">
            Your faction: {{ selectedFaction }}
            <img src="@/assets/icons/empire.svg" alt="Imperial" v-if="selectedFaction === 'imperial'" width="50"/>
            <img src="@/assets/icons/rebel.svg" alt="Rebel" v-if="selectedFaction === 'rebel'" width="50"/>
        </div>
        <div class="deploy-roster-title">
            You have:
            <span class="pts-remaining" :class="{ 'pts-remaining--negative': ptsRemaining < 0 }">{{ ptsRemaining }} PTS Left</span>
        </div>
        <faction-roster v-if="selectedFaction" :faction="selectedFaction" />
    </div>
</template>
<script setup lang="ts">
import FactionRoster from './FactionRoster.vue';
import { useDraftStore } from '@/stores/draft.store';
import { storeToRefs } from 'pinia';

const { selectedFaction, ptsRemaining } = storeToRefs(useDraftStore());
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
.pts-remaining {
    color: #ffffff;
    &--negative { color: var(--color-sector-destroyed); }
}
</style>
