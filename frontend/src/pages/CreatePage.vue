<template>
    <main-layout>
        <div class="wrapper">
            <label class="item">
                <span>Battle map size:</span>
                <select v-model="battleMapSize" class="generic-input" style="width: 100px;">
                    <option value="8">8x8</option>
                    <option value="12">12x12</option>
                    <option value="16">16x16</option>
                    <option value="20">20x20</option>
                    <option value="24">24x24</option>
                    <option value="28">28x28</option>
                    <option value="32">32x32</option>
                    <option value="36">36x36</option>
                    <option value="40">40x40</option>
                </select>
            </label>
            <label class="item">
                <span>PTS limit:</span>
                <input type="number" v-model="ptsLimit" placeholder="PTS Limit" class="generic-input">
            </label>
            <label class="item">
                <span>Pick faction:</span>
                <select v-model="selectedFaction" class="generic-input">
                    <option value="imperial">
                        Imperial
                    </option>
                    <option value="rebel">Rebel</option>
                </select>
            </label>
            <button @click="handleGenerateBattleMap" class="generic-button">Generate Battle Map</button>
        </div>
        <div class="wrapper" style="margin-top: var(--space-sm);"  v-if="battleMap">
            <battle-map :battle-map-data="battleMap" />
            <div class="roster-wrapper">    
                <deploy-roster />
                <button @click="handleCreateSession" class="generic-button">Create Session</button>
            </div>
        </div>
    </main-layout>
</template>
<script setup lang="ts">
import MainLayout from '@/components/layouts/MainLayout.vue';
import { ref } from 'vue';
import { Faction } from '@/types/session';
import { useBattleMap } from '@/composables/useBattleMap';
import type { BattleMap as BattleMapType } from '@/types/map';
import BattleMap from '@/components/widgets/battle-map/BattleMap.vue';
import DeployRoster from '@/components/widgets/battle-map/DeployRoster.vue';
import { useDraftStore } from '@/stores/draft.store';
import { storeToRefs } from 'pinia';

const draftStore = useDraftStore();
const { battleMap, selectedFaction, ptsLimit } = storeToRefs(draftStore);
const { generateBattleMap } = useBattleMap();

const battleMapSize = ref<string>('12');

const handleGenerateBattleMap = () => {
    battleMap.value = generateBattleMap({ size: { x: parseInt(battleMapSize.value), y: parseInt(battleMapSize.value) } });
}

const handleCreateSession = () => {
    console.log('createSession');
}
</script>
<style scoped lang="scss">
.wrapper {
    display: flex;    align-items: flex-start;
    justify-content: flex-start;
    gap: 10px;
    width: 100%;
    background-color: var(--color-settings-background);
    border-radius: 10px;
    padding: var(--space-sm);
    .item {
        span {
            color: #ffffff;
            margin-right: var(--space-m);
        }
        input {
            margin-right: var(--space-m);
        }
    }
}
.roster-wrapper {
    display: flex;
    align-items: center;
    justify-content: center;
    flex-direction: column;
    gap: 10px;
    width: 100%;
    height: 100%;
}
</style>