<template>
    <div class="abzabattle-table">
        <table>
            <thead>
                <tr>
                    <th>Sessions Online</th>
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
                    <td>{{ session.status }}</td>
                    <td>
                        <button
                            v-if="isParticipant(session)"
                            class="generic-button"
                            title="Return to your session"
                            @click="handleOpen(session)"
                        >
                            Open
                        </button>
                        <button
                            v-else
                            class="generic-button"
                            :disabled="!session.canJoin"
                            :title="session.canJoin ? 'Join session' : 'No free slots'"
                            @click="handleJoin(session.id)"
                        >
                            Join
                        </button>
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
import { storeToRefs } from 'pinia';

const sessionStore = useSessionStore();
const userStore = useUserStore();
const { onlineSessions } = storeToRefs(sessionStore);

const isParticipant = (session: Session) => {
    const name = userStore.currentUser?.name;
    if (!name) return false;
    return session.rebel.player?.name === name || session.imperial.player?.name === name;
};

const handleOpen = async (session: Session) => {
    const name = userStore.currentUser?.name;
    if (!name) return;
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
