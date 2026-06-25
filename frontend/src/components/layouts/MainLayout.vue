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
            <div class="footer-help">
                <button
                    type="button"
                    class="footer-help__button"
                    aria-label="Battle rules"
                >
                    <FontAwesomeIcon :icon="faCircleQuestion" />
                </button>
                <div class="footer-help__tooltip" role="tooltip">
                    <ul class="footer-help__list">
                        <li v-for="tip in battleRulesTips" :key="tip">{{ tip }}</li>
                    </ul>
                </div>
            </div>
            <p v-if="showReplayHint" class="footer-hint">
                Press <kbd>Space</kbd> to go to the next action
            </p>
            <p v-else-if="showDraftHint" class="footer-hint">
                <template v-if="showRotateHint">
                    Press <kbd>R</kbd> / <kbd>К</kbd> to rotate selected unit ({{ selectedRotation }}°).
                    Right-click a placed unit to remove it.
                </template>
                <template v-else>
                    Select a unit to deploy. Press <kbd>R</kbd> / <kbd>К</kbd> to rotate it.
                    Right-click a placed unit to remove it.
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
            <button
                v-if="showGiveUpButton"
                type="button"
                class="generic-button give-up-button"
                :disabled="isLoading"
                @click="handleGiveUp"
            >
                Give Up
            </button>
            <p>Wins: {{ userStore.currentUser?.wins ?? 0 }}</p>
            <p>Loses: {{ userStore.currentUser?.loses ?? 0 }}</p>
            <p>Total games: {{ userStore.currentUser?.totalGames ?? 0 }}</p>
        </div>
        <div class="main-layout-footer__end">
            Abzabattle {{ version }}
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
import { faHouse, faRightFromBracket, faCircleQuestion } from '@fortawesome/free-solid-svg-icons'
import router from '@/router';
import { DEATH_STAR_DESTROY_CHANCE } from '@/data/unitAbilities';

const battleRulesTips = [
    'Use your ship abilities first — they do not end your turn.',
    'If no ability is selected, you perform a basic attack. If it hits, you may repeat basic attacks up to 3 times in total.',
    'Missing with a basic attack ends your turn.',
    'The first hit on a shield reveals it, the second destroys the shield, and only the third hit destroys the target.',
    'Death Star has a fatal construction flaw: an X-Wing firing at the reactor (central sector) has a ' + DEATH_STAR_DESTROY_CHANCE + '%     chance to destroy the entire station!',
    'If you want to give feedback or report a bug, please contact me at @artifitialme on Telegram. I would love to hear from you!',
];

const userStore = useUserStore();
const authStore = useAuthStore();
const sessionStore = useSessionStore();
const appStore = useAppStore();
const draftStore = useDraftStore();
const { isLoading, version } = storeToRefs(appStore);
const { showRotateHint, selectedRotation, isDrafting } = storeToRefs(draftStore);
const isTurnHistoryOpen = ref(false);

const showReplayHint = computed(() => sessionStore.currentSession?.status === 'finished');

const showDraftHint = computed(() => {
    if (showReplayHint.value) return false;
    const session = sessionStore.currentSession;
    if (session?.status === 'in_progress' || session?.status === 'finished') {
        return false;
    }
    return isDrafting.value;
});

const showTurnHistoryButton = computed(() => {
    const session = sessionStore.currentSession;
    return !!session && (session.status === 'in_progress' || session.status === 'finished');
});

const showGiveUpButton = computed(() => {
    const session = sessionStore.currentSession;
    const playerName = userStore.currentUser?.name;
    if (!session || session.status !== 'in_progress' || !playerName) {
        return false;
    }

    return session.rebel.player?.name === playerName
        || session.imperial.player?.name === playerName;
});

const handleGiveUp = async () => {
    const session = sessionStore.currentSession;
    const playerName = userStore.currentUser?.name;
    if (!session || !playerName || isLoading.value) {
        return;
    }

    await sessionStore.giveUp(session.id, playerName);
};
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
    height: calc(100vh - 100px);
    height: calc(100dvh - 100px);
    background-image: url('@/assets/images/background.png');
    background-size: cover;
    display: flex;
    flex-direction: column;
    overflow: auto;
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
        gap: 10px;
        min-width: 0;
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

.footer-hint {
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

.give-up-button {
    background-color: #c62828;
    color: #fff;

    &:hover:not(:disabled) {
        background-color: #e53935;
        color: #fff;
    }

    &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
    }
}

.footer-help {
    position: relative;
    flex-shrink: 0;

    &::before {
        content: '';
        position: absolute;
        left: 0;
        bottom: 100%;
        width: 28px;
        height: 14px;
    }

    &__button {
        display: flex;
        align-items: center;
        justify-content: center;
        width: 28px;
        height: 28px;
        padding: 0;
        border: 1px solid rgba(255, 255, 255, 0.35);
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.08);
        color: rgba(255, 255, 255, 0.9);
        font-size: 16px;
        cursor: help;

        &:hover {
            background: rgba(255, 255, 255, 0.16);
            color: #fff;
        }
    }

    &__tooltip {
        position: absolute;
        left: 0;
        bottom: calc(100% + 10px);
        min-width: 280px;
        max-width: min(360px, calc(100vw - 24px));
        padding: var(--space-sm);
        border-radius: 8px;
        background-color: rgba(0, 0, 0, 0.94);
        border: 1px solid rgba(255, 255, 255, 0.2);
        opacity: 0;
        visibility: hidden;
        transform: translateY(4px);
        transition: opacity 0.15s ease, transform 0.15s ease, visibility 0.15s ease;
        pointer-events: none;
        z-index: 20;

        &::after {
            content: '';
            position: absolute;
            left: 0;
            right: 0;
            top: 100%;
            height: 14px;
        }

        li {
            list-style-type: disc;
        }
    }

    &:hover &__tooltip,
    &__tooltip:hover,
    &__button:focus-visible + &__tooltip {
        opacity: 1;
        visibility: visible;
        transform: translateY(0);
        pointer-events: auto;
    }
}

.footer-help__list {
    margin: 0;
    padding-left: 18px;
    color: rgba(255, 255, 255, 0.9);
    font-size: 12px;
    line-height: 1.45;

    li + li {
        margin-top: 6px;
    }
}
</style>
