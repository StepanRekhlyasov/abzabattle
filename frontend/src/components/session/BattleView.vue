<template>
    <div class="battle-view">
        <div v-if="replayMode" class="turn-header">
            <button
                type="button"
                class="generic-button turn-nav-button"
                :disabled="!canGoPrevious"
                @click="emit('previous-action')"
            >
                Previous Action
            </button>
            <h2 class="turn-title">{{ replayTurnLabel }}</h2>
            <button
                type="button"
                class="generic-button turn-nav-button"
                :disabled="!canGoNext"
                @click="emit('next-action')"
            >
                Next Action
            </button>
        </div>
        <h2 v-else-if="!finished" class="turn-title">{{ turnLabel }}</h2>
        <div class="battle-view-maps">
            <template v-if="isSpectatorView">
                <div
                    v-if="rebelBattleMap"
                    class="battle-view-panel battle-view-panel--readonly"
                >
                    <h3 class="battle-view-title">{{ rebelMapTitle }}</h3>
                    <battle-map-component :battle-map-data="rebelBattleMap" />
                </div>
                <div
                    v-if="imperialBattleMap"
                    class="battle-view-panel battle-view-panel--readonly"
                >
                    <h3 class="battle-view-title">{{ imperialMapTitle }}</h3>
                    <battle-map-component :battle-map-data="imperialBattleMap" />
                </div>
            </template>
            <template v-else>
            <div
                class="battle-view-panel"
                :class="{
                    'battle-view-panel--inactive': !finished && isOwnMapInactive,
                    'battle-view-panel--readonly': finished || replayMode,
                }"
                v-if="myBattleMap"
            >
                <h3 class="battle-view-title">Your map</h3>
                <battle-map-component
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
                    'battle-view-panel--readonly': finished || replayMode,
                }"
                v-if="opponentBattleMap"
            >
                <h3 class="battle-view-title">Opponent map</h3>
                <battle-map-component
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
                    :used="!!currentSession && sessionStore.isAbilityUsed(currentSession.id, ability.entityId)"
                    :disabled="!isMyTurn || isLoading"
                    @select="handleAbilitySelect"
                />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import BattleMapComponent from '@/components/widgets/battle-map/BattleMap.vue';
import Ability from '@/components/widgets/Ability.vue';
import { AbilityKind } from '@/data/unitAbilities';
import { useBattleMap } from '@/composables/useBattleMap';
import { useSessionStore } from '@/stores/session.store';
import { useAppStore } from '@/stores/app.store';
import { useUserStore } from '@/stores/user.store';
import { EntityRotation, EntityType } from '@/types/entity';
import type { BattleMap } from '@/types/map';
import type { BattleSector } from '@/types/map';
import { Faction } from '@/types/session';
import { getAbilitiesFromBattleMap } from '@/utils/mapAbilities';
import { canAttackSector, canPlaceShieldOnSector } from '@/utils/battleSectorRules';
import { storeToRefs } from 'pinia';

const props = withDefaults(defineProps<{
    finished?: boolean;
    replayMode?: boolean;
    replayTurnLabel?: string;
    canGoPrevious?: boolean;
    canGoNext?: boolean;
    replayRebelBattleMap?: BattleMap | null;
    replayImperialBattleMap?: BattleMap | null;
}>(), {
    finished: false,
    replayMode: false,
    replayTurnLabel: '',
    canGoPrevious: false,
    canGoNext: false,
    replayRebelBattleMap: null,
    replayImperialBattleMap: null,
});

const emit = defineEmits<{
    (e: 'previous-action'): void;
    (e: 'next-action'): void;
}>();

const sessionStore = useSessionStore();
const appStore = useAppStore();
const userStore = useUserStore();
const { getPlacementPreview, revealAllSectors } = useBattleMap();
const { currentSession } = storeToRefs(sessionStore);
const { isLoading } = storeToRefs(appStore);
const selectedAbilityId = ref<string | null>(null);
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

const isSpectatorView = computed(() => {
    if (props.replayMode) {
        return isSpectator.value;
    }
    return props.finished && isSpectator.value;
});

const withReplayReveal = (map: BattleMap | null) => {
    if (!map) return null;
    return props.replayMode ? revealAllSectors(map) : map;
};

const rebelBattleMap = computed(() => {
    if (props.replayMode) {
        return withReplayReveal(props.replayRebelBattleMap);
    }
    return currentSession.value?.rebel.battleMap ?? null;
});

const imperialBattleMap = computed(() => {
    if (props.replayMode) {
        return withReplayReveal(props.replayImperialBattleMap);
    }
    return currentSession.value?.imperial.battleMap ?? null;
});

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
    if (props.replayMode) {
        if (!myFaction.value) return null;
        const map = myFaction.value === Faction.Rebel
            ? props.replayRebelBattleMap
            : props.replayImperialBattleMap;
        return withReplayReveal(map);
    }
    if (!currentSession.value || !myFaction.value) return null;
    return myFaction.value === Faction.Rebel
        ? currentSession.value.rebel.battleMap
        : currentSession.value.imperial.battleMap;
});

const opponentBattleMap = computed(() => {
    if (props.replayMode) {
        if (!myFaction.value) return null;
        const map = myFaction.value === Faction.Rebel
            ? props.replayImperialBattleMap
            : props.replayRebelBattleMap;
        return withReplayReveal(map);
    }
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
    () => {
        selectedAbilityId.value = null;
        ownMapHoverAnchor.value = null;
    },
);

const handleAbilitySelect = (entityId: string) => {
    const sessionId = currentSession.value?.id;
    if (!sessionId || !isMyTurn.value || sessionStore.isAbilityUsed(sessionId, entityId)) return;
    selectedAbilityId.value = selectedAbilityId.value === entityId ? null : entityId;
};

const markAbilityUsed = (entityId: string) => {
    const sessionId = currentSession.value?.id;
    if (!sessionId) return;
    sessionStore.markAbilityUsed(sessionId, entityId);
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

    if (!playerName || !sessionId || isLoading.value || !isMyTurn.value || !ability) return;
    if (ability.target !== 'own') return;

    if (ability.kind === AbilityKind.DeployTieFighter) {
        if (!canDeployTieFighterAt(x, y)) return;

        await sessionStore.useAbility(sessionId, playerName, ability.kind, x, y);
        markAbilityUsed(ability.entityId);
        return;
    }

    if (ability.kind === AbilityKind.PlaceShield) {
        if (!canPlaceShieldOnSector(sector, ability.entityId)) return;

        await sessionStore.useAbility(
            sessionId,
            playerName,
            ability.kind,
            x,
            y,
            ability.entityId,
        );
        markAbilityUsed(ability.entityId);
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

    if (!playerName || !sessionId || isLoading.value || !isMyTurn.value) return;

    if (ability) {
        if (ability.target !== 'opponent') return;
        if (ability.kind !== AbilityKind.OpponentStrike
            && ability.kind !== AbilityKind.AirborneSuperiority
            && ability.kind !== AbilityKind.Bombardment) return;
        if (!canAttackSector(sector)) return;

        await sessionStore.useAbility(sessionId, playerName, ability.kind, x, y);
        markAbilityUsed(ability.entityId);
        return;
    }

    if (!canAttackSector(sector)) return;

    await sessionStore.attackSector(sessionId, playerName, x, y);
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
    margin: 0 20px;
    font-size: 24px;
    font-weight: 600;
    text-align: center;
}

.turn-header {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: var(--space-md);
    width: 100%;
}

.turn-nav-button {
    min-width: 150px;

    &:disabled {
        opacity: 0.45;
        cursor: not-allowed;
    }
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
