<template>
    <main-layout>
        <div v-if="loading" class="session-loading">Loading session...</div>
        <div v-else-if="error" class="session-error">{{ error }}</div>
        <template v-else-if="currentSession && currentUser">
            <waiting-view
                v-if="waitingText"
                :message="waitingText"
            />
            <deploy-panel
                v-else-if="sessionStore.isDeployPhase(currentUser.name)"
                read-only-settings
                :locked-faction="joinerFaction!"
                :fixed-map-size="currentSession.mapSize"
                :fixed-pts-limit="currentSession.ptsLimit"
                action-label="Start the battle"
                :action-disabled="!battleMap"
                :show-reset-button="true"
                auto-generate
                @action="handleStartBattle"
            />
            <battle-view v-else-if="sessionStore.isBattlePhase()" />
            <div
                v-else-if="sessionStore.isFinishedPhase()"
                class="session-finished"
            >
                <div v-if="replayLoading && !replayInitialized" class="session-replay-loading">
                    Loading replay...
                </div>
                <template v-else>
                    <p v-if="replayError" class="session-replay-error">{{ replayError }}</p>
                    <battle-view
                        finished
                        replay-mode
                        :replay-turn-label="replayTurnLabel"
                        :can-go-previous="canGoPrevious"
                        :can-go-next="canGoNext"
                        :replay-rebel-battle-map="currentReplayFrame?.snapshot.rebelBattleMap ?? null"
                        :replay-imperial-battle-map="currentReplayFrame?.snapshot.imperialBattleMap ?? null"
                        @previous-action="goPrevious"
                        @next-action="goNext"
                    />
                </template>
            </div>
        </template>
    </main-layout>
</template>

<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import MainLayout from '@/components/layouts/MainLayout.vue';
import BattleView from '@/components/session/BattleView.vue';
import DeployPanel from '@/components/session/DeployPanel.vue';
import WaitingView from '@/components/session/WaitingView.vue';
import { useDraftStore } from '@/stores/draft.store';
import { useSessionStore } from '@/stores/session.store';
import { useUserStore } from '@/stores/user.store';
import { useBattleMap } from '@/composables/useBattleMap';
import { getReplayTurnLabel, useSessionReplay } from '@/composables/useSessionReplay';
import { isEditableTarget } from '@/utils/rotateKey';
import { Faction, oppositeFaction } from '@/types/session';
import { storeToRefs } from 'pinia';

const route = useRoute();
const sessionStore = useSessionStore();
const userStore = useUserStore();
const draftStore = useDraftStore();
const { prepareBattleMapForBattle } = useBattleMap();
const { currentSession } = storeToRefs(sessionStore);
const { currentUser } = storeToRefs(userStore);
const { battleMap } = storeToRefs(draftStore);

const loading = ref(true);
const error = ref<string | null>(null);
const sessionId = computed(() => route.params.id as string);

const {
    currentFrame: currentReplayFrame,
    loading: replayLoading,
    error: replayError,
    initialized: replayInitialized,
    canGoPrevious,
    canGoNext,
    loadReplay,
    goPrevious,
    goNext,
} = useSessionReplay(() => currentSession.value);

const replayTurnLabel = computed(() => {
    const session = currentSession.value;
    const frame = currentReplayFrame.value;
    if (!session || !frame) return 'Replay';
    return getReplayTurnLabel(frame.snapshot, session, currentUser.value?.name);
});

watch(
    () => currentSession.value?.status,
    (status) => {
        if (status === 'finished') {
            void loadReplay();
        }
    },
    { immediate: true },
);
const waitingText = computed(() => {
    if (!currentUser.value) return 'Current user not found';
    if (sessionStore.isWaitingForOpponent(currentUser.value.name)) return 'Waiting for opponent...';
    if (sessionStore.isWaitingForDeploy(currentUser.value.name)) return 'Opponent is deploying...';
    return null;
});

const joinerFaction = computed(() => {
    if (!currentSession.value || !currentUser.value) return null;
    const creatorFaction = currentSession.value.rebel.player?.name === currentSession.value.creatorPlayerName
        ? Faction.Rebel
        : Faction.Imperial;
    return oppositeFaction(creatorFaction);
});

const isParticipant = (session = currentSession.value) => {
    if (!session || !currentUser.value) return false;
    const name = currentUser.value.name;
    return session.rebel.player?.name === name || session.imperial.player?.name === name;
};

const loadSession = async () => {
    loading.value = true;
    error.value = null;
    try {
        const session = await sessionStore.loadSession(sessionId.value, currentUser.value?.name);
        if (!isParticipant(session) && session.status !== 'finished') {
            error.value = 'You are not a participant of this session';
        }
    } catch {
        error.value = 'Session not found';
    } finally {
        loading.value = false;
    }
};

const handleStartBattle = async () => {
    if (!currentUser.value || !battleMap.value) return;
    await sessionStore.startBattle(
        sessionId.value,
        currentUser.value.name,
        prepareBattleMapForBattle(battleMap.value),
    );
};

const handleReplayKeyDown = (event: KeyboardEvent) => {
    if (currentSession.value?.status !== 'finished' || replayLoading.value) return;
    if (event.code !== 'Space' || isEditableTarget(event.target)) return;
    event.preventDefault();
    if (canGoNext.value) {
        void goNext();
    }
};

onMounted(() => {
    void loadSession();
    window.addEventListener('keydown', handleReplayKeyDown);
});
onUnmounted(() => {
    sessionStore.setCurrentSession(null);
    window.removeEventListener('keydown', handleReplayKeyDown);
});
</script>

<style scoped lang="scss">
.session-loading,
.session-error {
    color: #ffffff;
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background-color: #000;
    padding: var(--space-sm);
}

.session-finished {
    position: relative;
    width: 100%;
    height: 100%;
}

.session-replay-loading,
.session-replay-error {
    color: #ffffff;
    text-align: center;
    padding: var(--space-md);
}

.session-replay-error {
    color: #ffb74d;
}
</style>
