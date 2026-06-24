<template>
    <div class="battle-view">
        <h2 class="turn-title">{{ turnLabel }}</h2>
        <div class="battle-view-maps">
            <div class="battle-view-panel" v-if="myBattleMap">
                <h3 class="battle-view-title">Your map</h3>
                <battle-map :battle-map-data="myBattleMap" />
            </div>
            <div
                class="battle-view-panel"
                :class="{ 'battle-view-panel--inactive': !isMyTurn }"
                v-if="opponentBattleMap"
            >
                <h3 class="battle-view-title">Opponent map</h3>
                <battle-map
                    :battle-map-data="opponentBattleMap"
                    @sector-click="handleOpponentSectorClick"
                />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import BattleMap from '@/components/widgets/battle-map/BattleMap.vue';
import { useSessionStore } from '@/stores/session.store';
import { useUserStore } from '@/stores/user.store';
import type { BattleSector } from '@/types/map';
import { Faction } from '@/types/session';
import { storeToRefs } from 'pinia';

const sessionStore = useSessionStore();
const userStore = useUserStore();
const { currentSession } = storeToRefs(sessionStore);
const isAttacking = ref(false);

const myFaction = computed(() => {
    const name = userStore.currentUser?.name;
    if (!name || !currentSession.value) return null;
    return sessionStore.myFaction(name);
});

const isMyTurn = computed(() => {
    const name = userStore.currentUser?.name;
    if (!name) return false;
    return sessionStore.isMyTurn(name);
});

const turnLabel = computed(() => isMyTurn.value ? 'Your turn' : "Opponent's turn");

const myBattleMap = computed(() => {
    if (!currentSession.value || !myFaction.value) return null;
    return myFaction.value === Faction.Rebel
        ? currentSession.value.rebel.battleMap
        : currentSession.value.imperial.battleMap;
});

const opponentBattleMap = computed(() => {
    if (!currentSession.value || !myFaction.value) return null;
    return myFaction.value === Faction.Rebel
        ? currentSession.value.imperial.battleMap
        : currentSession.value.rebel.battleMap;
});

const handleOpponentSectorClick = async ({
    sector,
    x,
    y,
}: {
    sector: BattleSector;
    x: number;
    y: number;
}) => {
    const playerName = userStore.currentUser?.name;
    const sessionId = currentSession.value?.id;
    if (!playerName || !sessionId || isAttacking.value || !isMyTurn.value) return;
    if (!sector.hidden || sector.destroyed) return;

    isAttacking.value = true;
    try {
        await sessionStore.attackSector(sessionId, playerName, x, y);
    } finally {
        isAttacking.value = false;
    }
};
</script>

<style scoped lang="scss">
.battle-view {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-md);
    width: 100%;
    background: #000;
    height: 100%;
    padding: var(--space-sm);
}

.turn-title {
    color: #ffffff;
    margin: 0;
    font-size: 24px;
    font-weight: 600;
}

.battle-view-maps {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-md);
    width: 100%;
    flex-wrap: wrap;
}

.battle-view-panel {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: var(--space-sm);
    flex: 1;
    min-width: 280px;

    &--inactive {
        opacity: 0.6;
        pointer-events: none;
    }
}

.battle-view-title {
    color: #ffffff;
    margin: 0;
    font-size: 18px;
}
</style>
