<template>
    <div class="abzabattle-table">
        <table>
            <thead>
                <tr>
                    <th>Sessions Online</th>
                    <th>Rebel Player</th>
                    <th>Imperial Player</th>
                    <th>Status</th>
                </tr>
            </thead>
            <tbody>
                <tr
                    v-for="session in onlineSessions"
                    :key="session.id"
                >
                    <td>
                        <div class="user-list-item">
                            {{ session.name }}
                        </div>
                    </td>
                    <td>{{ session.rebel?.name ?? '—' }}</td>
                    <td>{{ session.imperial?.name ?? '—' }}</td>
                    <td>{{ session.status }}</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script setup lang="ts">
import { useSessionStore } from '@/stores/session.store';
import { storeToRefs } from 'pinia';

const sessionStore = useSessionStore();
const { onlineSessions } = storeToRefs(sessionStore);
</script>
<style scoped lang="scss">

.user-list-item {
    display: flex;
    align-items: center;
    gap: 10px;
}

.green-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    background-color: #4caf50;
}

</style>

