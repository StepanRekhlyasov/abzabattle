<template>
    <div class="abzabattle-table">
        <table>
            <thead>
                <tr>
                    <th>Sessions</th>
                    <th>Rebel Player</th>
                    <th>Imperial Player</th>
                    <th>Status</th>
                    <th></th>
                </tr>
            </thead>
            <tbody>
                <tr
                    v-for="session in onlineSessions"
                    :key="session.id"
                >
                    <td>{{ session.name }}</td>
                    <td>{{ session.rebel.player?.name ?? '—' }}</td>
                    <td>{{ session.imperial.player?.name ?? '—' }}</td>
                    <td>{{ getStatusText(session.status) }}</td>
                    <td class="session-actions">
                        <button
                            v-if="isParticipant(session)"
                            class="generic-button"
                            title="Return to your session"
                            @click="handleOpen(session)"
                        >
                            Open
                        </button>
                        <button
                            v-else-if="session.status === SessionStatusEnum.Finished"
                            class="generic-button"
                            title="View session result"
                            @click="handleView(session)"
                        >
                            View
                        </button>
                        <button
                            v-else-if="session.canJoin"
                            class="generic-button"
                            title="Join session"
                            @click="handleJoin(session.id)"
                        >
                            Join
                        </button>
                        <span
                            v-else
                            class="join-blocked-reason"
                        >
                            {{ session.joinBlockedReason ?? 'Cannot join this session' }}
                        </span>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script setup lang="ts">
import router from '@/router';
import { useSessionStore } from '@/stores/session.store';
import { useUserStore } from '@/stores/user.store';
import type { Session } from '@/types/session';
import { SessionStatusEnum } from '@/types/session';
import { storeToRefs } from 'pinia';

const sessionStore = useSessionStore();
const userStore = useUserStore();
const { onlineSessions } = storeToRefs(sessionStore);

const isParticipant = (session: Session) => {
    const name = userStore.currentUser?.name;
    if (!name) return false;
    return session.rebel.player?.name === name || session.imperial.player?.name === name;
};

const getStatusText = (status: SessionStatusEnum) => {
    return status === SessionStatusEnum.Pending ? 'Pending' : status === SessionStatusEnum.InProgress ? 'In Progress' : 'Finished';
};

const handleOpen = async (session: Session) => {
    const name = userStore.currentUser?.name;
    if (!name) return;
    await sessionStore.loadSession(session.id, name);
    router.push(`/session/${session.id}`);
};

const handleView = async (session: Session) => {
    const name = userStore.currentUser?.name;
    await sessionStore.loadSession(session.id, name);
    router.push(`/session/${session.id}`);
};

const handleJoin = async (sessionId: string) => {
    const name = userStore.currentUser?.name;
    if (!name) return;
    await sessionStore.joinSession(sessionId, name);
    router.push(`/session/${sessionId}`);
};
</script>

<style scoped lang="scss">
.session-actions {
    min-width: 180px;
    text-align: center;
}

.join-blocked-reason {
    color: #ffb74d;
    font-size: 0.85rem;
}
</style>
