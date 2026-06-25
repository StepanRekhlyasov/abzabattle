<template>
    <div class="main-layout-header">
        <div class="main-layout-header-user" style="width: 200px;">
            <FontAwesomeIcon :icon="faHouse" @click="router.push('/')"/>
            <p style="line-height: 1px;">Current user: {{ userStore.currentUser?.name }}</p>
        </div>
        <p style="width:100%; text-align: center;">Please use browser zoom to adjust the battle map size.</p>
        <div style="width: 200px;text-align: right;">
            <button @click="authStore.logout" class="generic-button transparent-button">
                <FontAwesomeIcon :icon="faRightFromBracket" />
            </button>
        </div>
    </div>
    <div class="main-layout-content">
        <slot></slot>
    </div>
    <div class="main-layout-footer">
        <div class="main-layout-footer__start">
            <p v-if="isDrafting" class="draft-hint">
                <template v-if="showRotateHint">
                    Press <kbd>R</kbd> / <kbd>К</kbd> to rotate selected unit ({{ selectedRotation }}°)
                </template>
                <template v-else>
                    Select a unit to deploy. Press <kbd>R</kbd> / <kbd>К</kbd> to rotate it.
                </template>
            </p>
        </div>
        <div class="main-layout-footer__center">
            <button
                v-if="showTurnHistoryButton"
                type="button"
                class="generic-button"
                @click="isTurnHistoryOpen = true"
            >
                Turn History
            </button>
            <p>Wins: {{ userStore.currentUser?.wins ?? 0 }}</p>
            <p>Loses: {{ userStore.currentUser?.loses ?? 0 }}</p>
            <p>Total games: {{ userStore.currentUser?.totalGames ?? 0 }}</p>
        </div>
        <div class="main-layout-footer__end">
            <v-progress-circular
                v-if="isLoading"
                indeterminate
                color="primary"
                size="24"
                width="2"
            />
        </div>
    </div>
    <turn-history-modal
        :open="isTurnHistoryOpen"
        :session-id="sessionStore.currentSession?.id ?? null"
        @close="isTurnHistoryOpen = false"
    />
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { storeToRefs } from 'pinia';
import { useUserStore } from '@/stores/user.store';
import { useAuthStore } from '@/stores/auth.store';
import { useSessionStore } from '@/stores/session.store';
import { useAppStore } from '@/stores/app.store';
import { useDraftStore } from '@/stores/draft.store';
import TurnHistoryModal from '@/components/session/TurnHistoryModal.vue';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faHouse, faRightFromBracket } from '@fortawesome/free-solid-svg-icons'
import router from '@/router';

const userStore = useUserStore();
const authStore = useAuthStore();
const sessionStore = useSessionStore();
const appStore = useAppStore();
const draftStore = useDraftStore();
const { isLoading } = storeToRefs(appStore);
const { showRotateHint, selectedRotation, isDrafting } = storeToRefs(draftStore);
const isTurnHistoryOpen = ref(false);

const showTurnHistoryButton = computed(() => {
    const session = sessionStore.currentSession;
    return !!session && (session.status === 'in_progress' || session.status === 'finished');
});
</script>

<style scoped lang="scss">
.main-layout-header {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 10px;
    background-color: #000;
    height: 50px;
}
.main-layout-content {
    height: calc(100% - 100px);
    background-image: url('@/assets/images/background.png');
    background-size: cover;
    display: flex;
    flex-direction: column;
}
.main-layout-header-user {
    display: flex;
    align-items: center;
    gap: 10px;
    cursor: pointer;
    color: #ffffff;
    white-space: nowrap;
    &:hover {
        color: var(--color-sector-exposed);
    }
}
.main-layout-footer {
    height: 50px;
    background-color: #000;
    display: flex;
    align-items: center;
    padding: 0 10px;
    color: #ffffff;

    &__start {
        flex: 1;
        display: flex;
        align-items: center;
    }

    &__center {
        flex: 0 0 auto;
        display: flex;
        align-items: center;
        gap: 10px;
    }

    &__end {
        flex: 1;
        display: flex;
        align-items: center;
        justify-content: flex-end;
        min-width: 24px;
        min-height: 24px;
    }

    p {
        margin: 0;
        text-align: center;
    }
}

.draft-hint {
    font-size: 0.9rem;
    color: rgba(255, 255, 255, 0.85);

    kbd {
        display: inline-block;
        min-width: 1.25rem;
        padding: 2px 6px;
        border: 1px solid rgba(255, 255, 255, 0.35);
        border-radius: 4px;
        background: rgba(255, 255, 255, 0.1);
        font-family: inherit;
        font-size: 0.85rem;
        line-height: 1.2;
        text-align: center;
    }
}
</style>
