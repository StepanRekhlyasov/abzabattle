<template>
    <div class="battle-view">
        <h2 v-if="!finished" class="turn-title">{{ turnLabel }}</h2>
        <div class="battle-view-maps">
            <template v-if="isSpectatorView">
                <div
                    v-if="rebelBattleMap"
                    class="battle-view-panel battle-view-panel--readonly"
                >
                    <h3 class="battle-view-title">{{ rebelMapTitle }}</h3>
                    <battle-map :battle-map-data="rebelBattleMap" />
                </div>
                <div
                    v-if="imperialBattleMap"
                    class="battle-view-panel battle-view-panel--readonly"
                >
                    <h3 class="battle-view-title">{{ imperialMapTitle }}</h3>
                    <battle-map :battle-map-data="imperialBattleMap" />
                </div>
            </template>
            <template v-else>
            <div
                class="battle-view-panel"
                :class="{
                    'battle-view-panel--inactive': !finished && isOwnMapInactive,
                    'battle-view-panel--readonly': finished,
                }"
                v-if="myBattleMap"
            >
                <h3 class="battle-view-title">Your map</h3>
                <battle-map
                    :battle-map-data="myBattleMap"
                    :preview-cells="ownMapPreviewCellKeys"
                    :preview-valid="ownMapPreviewIsValid"
                    @sector-click="handleMySectorClick"
                    @sector-hover="handleMySectorHover"
                    @sector-leave="ownMapHoverAnchor = null"
                />
            </div>
            <div
                class="battle-view-panel"
                :class="{
                    'battle-view-panel--inactive': !finished && isOpponentMapInactive,
                    'battle-view-panel--readonly': finished,
                }"
                v-if="opponentBattleMap"
            >
                <h3 class="battle-view-title">Opponent map</h3>
                <battle-map
                    :battle-map-data="opponentBattleMap"
                    @sector-click="handleOpponentSectorClick"
                />
            </div>
            </template>
            <div v-if="!finished && myAbilities.length" class="special-abilities">
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
import { useBattleMap } from '@/composables/useBattleMap';
import { useSessionStore } from '@/stores/session.store';
import { useUserStore } from '@/stores/user.store';
import { EntityRotation, EntityType } from '@/types/entity';
import type { BattleSector } from '@/types/map';
import { Faction } from '@/types/session';
import { getAbilitiesFromBattleMap } from '@/utils/mapAbilities';
import { canAttackSector, canPlaceShieldOnSector } from '@/utils/battleSectorRules';
import { storeToRefs } from 'pinia';

const props = withDefaults(defineProps<{
    finished?: boolean;
}>(), {
    finished: false,
});

const sessionStore = useSessionStore();
const userStore = useUserStore();
const { getPlacementPreview } = useBattleMap();
const { currentSession } = storeToRefs(sessionStore);
const isActionPending = ref(false);
const selectedAbilityId = ref<string | null>(null);
const usedAbilityIds = ref<Set<string>>(new Set());
const turnResetKey = ref<string | null>(null);
const ownMapHoverAnchor = ref<{ x: number; y: number } | null>(null);

const deployTieFighterEntity = {
    type: EntityType.TieFighter,
    rotation: EntityRotation.R0,
    content: 'TF',
} as const;

const myFaction = computed(() => {
    const name = userStore.currentUser?.name;
    if (!name || !currentSession.value) return null;
    return sessionStore.myFaction(name);
});

const isSpectator = computed(() => myFaction.value === null);

const isSpectatorView = computed(() => props.finished && isSpectator.value);

const rebelBattleMap = computed(() => currentSession.value?.rebel.battleMap ?? null);
const imperialBattleMap = computed(() => currentSession.value?.imperial.battleMap ?? null);

const rebelMapTitle = computed(() => {
    const playerName = currentSession.value?.rebel.player?.name;
    return playerName ? `Rebel map (${playerName})` : 'Rebel map';
});

const imperialMapTitle = computed(() => {
    const playerName = currentSession.value?.imperial.player?.name;
    return playerName ? `Imperial map (${playerName})` : 'Imperial map';
});

const isMyTurn = computed(() => {
    const name = userStore.currentUser?.name;
    if (!name) return false;
    return sessionStore.isMyTurn(name);
});

const turnLabel = computed(() => {
    const hits = currentSession.value?.hitsThisTurn ?? 0;
    if (isMyTurn.value) {
        if (hits === 1) return 'Your turn (Second shot!)';
        if (hits === 2) return 'Your turn (Third shot!)';
        return 'Your turn';
    }
    if (hits === 1) return "Opponent's turn (Second shot!)";
    if (hits === 2) return "Opponent's turn (Third shot!)";
    return "Opponent's turn";
});

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

const ownMapPlacementPreview = computed(() => {
    if (!myBattleMap.value || !ownMapHoverAnchor.value) return null;
    if (selectedAbility.value?.kind === AbilityKind.DeployTieFighter) {
        return getPlacementPreview(deployTieFighterEntity, ownMapHoverAnchor.value, myBattleMap.value);
    }
    if (selectedAbility.value?.kind === AbilityKind.PlaceShield) {
        const sector = myBattleMap.value.sectors[ownMapHoverAnchor.value.y]?.[ownMapHoverAnchor.value.x];
        const isValid = !!sector && canPlaceShieldOnSector(sector, selectedAbility.value.entityId);
        return {
            cells: [ownMapHoverAnchor.value],
            isValid,
        };
    }
    return null;
});

const ownMapPreviewCellKeys = computed(() =>
    ownMapPlacementPreview.value?.cells.map(cell => `${cell.x},${cell.y}`) ?? [],
);

const ownMapPreviewIsValid = computed(() => !!ownMapPlacementPreview.value?.isValid);

const isOwnMapInactive = computed(() => {
    if (props.finished) return true;
    if (!isMyTurn.value) return true;
    return selectedAbility.value?.target !== 'own';
});

const isOpponentMapInactive = computed(() => {
    if (props.finished) return true;
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
    ownMapHoverAnchor.value = null;
};

const handleMySectorHover = ({ x, y }: { x: number; y: number }) => {
    const ability = selectedAbility.value;
    if (!ability || ability.target !== 'own') {
        ownMapHoverAnchor.value = null;
        return;
    }
    if (ability.kind !== AbilityKind.DeployTieFighter && ability.kind !== AbilityKind.PlaceShield) {
        ownMapHoverAnchor.value = null;
        return;
    }
    ownMapHoverAnchor.value = { x, y };
};

const canDeployTieFighterAt = (x: number, y: number) => {
    if (!myBattleMap.value) return false;
    return getPlacementPreview(deployTieFighterEntity, { x, y }, myBattleMap.value)?.isValid ?? false;
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
    if (props.finished) return;
    const playerName = userStore.currentUser?.name;
    const sessionId = currentSession.value?.id;
    const ability = selectedAbility.value;

    if (!playerName || !sessionId || isActionPending.value || !isMyTurn.value || !ability) return;
    if (ability.target !== 'own') return;

    if (ability.kind === AbilityKind.DeployTieFighter) {
        if (!canDeployTieFighterAt(x, y)) return;

        isActionPending.value = true;
        try {
            await sessionStore.useAbility(sessionId, playerName, ability.kind, x, y);
            markAbilityUsed(ability.entityId);
        } finally {
            isActionPending.value = false;
        }
        return;
    }

    if (ability.kind === AbilityKind.PlaceShield) {
        if (!canPlaceShieldOnSector(sector, ability.entityId)) return;

        isActionPending.value = true;
        try {
            await sessionStore.useAbility(
                sessionId,
                playerName,
                ability.kind,
                x,
                y,
                ability.entityId,
            );
            markAbilityUsed(ability.entityId);
        } finally {
            isActionPending.value = false;
        }
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
    if (props.finished) return;
    const playerName = userStore.currentUser?.name;
    const sessionId = currentSession.value?.id;
    const ability = selectedAbility.value;

    if (!playerName || !sessionId || isActionPending.value || !isMyTurn.value) return;

    if (ability) {
        if (ability.target !== 'opponent') return;
        if (ability.kind !== AbilityKind.OpponentStrike
            && ability.kind !== AbilityKind.AirborneSuperiority
            && ability.kind !== AbilityKind.Bombardment) return;
        if (!canAttackSector(sector)) return;

        isActionPending.value = true;
        try {
            await sessionStore.useAbility(sessionId, playerName, ability.kind, x, y);
            markAbilityUsed(ability.entityId);
        } finally {
            isActionPending.value = false;
        }
        return;
    }

    if (!canAttackSector(sector)) return;

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

    &--readonly {
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
