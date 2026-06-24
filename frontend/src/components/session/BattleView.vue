<template>
    <div class="battle-view">
        <h2 class="turn-title">{{ turnLabel }}</h2>
        <div class="battle-view-maps">
            <div
                class="battle-view-panel"
                :class="{ 'battle-view-panel--inactive': isOwnMapInactive }"
                v-if="myBattleMap"
            >
                <h3 class="battle-view-title">Your map</h3>
                <battle-map
                    :battle-map-data="myBattleMap"
                    @sector-click="handleMySectorClick"
                />
            </div>
            <div
                class="battle-view-panel"
                :class="{ 'battle-view-panel--inactive': isOpponentMapInactive }"
                v-if="opponentBattleMap"
            >
                <h3 class="battle-view-title">Opponent map</h3>
                <battle-map
                    :battle-map-data="opponentBattleMap"
                    @sector-click="handleOpponentSectorClick"
                />
            </div>
            <div v-if="myAbilities.length" class="special-abilities">
                <ability
                    v-for="ability in myAbilities"
                    :key="ability.entityId"
                    :ability="ability"
                    :selected="selectedAbilityId === ability.entityId"
                    :used="usedAbilityIds.has(ability.entityId)"
                    :disabled="!isMyTurn || isActionPending"
                    @select="handleAbilitySelect"
                />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import BattleMap from '@/components/widgets/battle-map/BattleMap.vue';
import Ability from '@/components/widgets/Ability.vue';
import { AbilityKind } from '@/data/unitAbilities';
import { useSessionStore } from '@/stores/session.store';
import { useUserStore } from '@/stores/user.store';
import { EntityType } from '@/types/entity';
import type { BattleSector } from '@/types/map';
import { Faction } from '@/types/session';
import { getAbilitiesFromBattleMap } from '@/utils/mapAbilities';
import { storeToRefs } from 'pinia';

const sessionStore = useSessionStore();
const userStore = useUserStore();
const { currentSession } = storeToRefs(sessionStore);
const isActionPending = ref(false);
const selectedAbilityId = ref<string | null>(null);
const usedAbilityIds = ref<Set<string>>(new Set());
const turnResetKey = ref<string | null>(null);

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

const myAbilities = computed(() => getAbilitiesFromBattleMap(myBattleMap.value));

const selectedAbility = computed(() =>
    myAbilities.value.find(ability => ability.entityId === selectedAbilityId.value) ?? null,
);

const isOwnMapInactive = computed(() => {
    if (!isMyTurn.value) return true;
    return selectedAbility.value?.target !== 'own';
});

const isOpponentMapInactive = computed(() => {
    if (!isMyTurn.value) return true;
    return selectedAbility.value?.target === 'own';
});

watch(
    () => currentSession.value
        ? `${currentSession.value.id}:${currentSession.value.currentTurn}`
        : null,
    (key) => {
        if (!key || key === turnResetKey.value) return;
        turnResetKey.value = key;
        usedAbilityIds.value = new Set();
        selectedAbilityId.value = null;
    },
);

const handleAbilitySelect = (entityId: string) => {
    if (!isMyTurn.value || usedAbilityIds.value.has(entityId)) return;
    selectedAbilityId.value = selectedAbilityId.value === entityId ? null : entityId;
};

const markAbilityUsed = (entityId: string) => {
    usedAbilityIds.value = new Set([...usedAbilityIds.value, entityId]);
    selectedAbilityId.value = null;
};

const handleMySectorClick = async ({
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
    const ability = selectedAbility.value;

    if (!playerName || !sessionId || isActionPending.value || !isMyTurn.value || !ability) return;
    if (ability.target !== 'own' || ability.kind !== AbilityKind.DeployTieFighter) return;
    if (sector.entity.type !== EntityType.Empty) return;

    isActionPending.value = true;
    try {
        await sessionStore.useAbility(sessionId, playerName, ability.kind, x, y);
        markAbilityUsed(ability.entityId);
    } finally {
        isActionPending.value = false;
    }
};

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
    const ability = selectedAbility.value;

    if (!playerName || !sessionId || isActionPending.value || !isMyTurn.value) return;

    if (ability) {
        if (ability.target !== 'opponent' || ability.kind !== AbilityKind.OpponentStrike) return;
        if (!sector.hidden || sector.destroyed) return;

        isActionPending.value = true;
        try {
            await sessionStore.useAbility(sessionId, playerName, ability.kind, x, y);
            markAbilityUsed(ability.entityId);
        } finally {
            isActionPending.value = false;
        }
        return;
    }

    if (!sector.hidden || sector.destroyed) return;

    isActionPending.value = true;
    try {
        await sessionStore.attackSector(sessionId, playerName, x, y);
    } finally {
        isActionPending.value = false;
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

.special-abilities {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-sm);
    width: 100%;
    flex-wrap: wrap;
    color: #ffffff;
    padding: var(--space-sm);
}
</style>
