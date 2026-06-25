<template>
    <div>
        <h1>User Board</h1>
        <div class="abzabattle-table">
            <table>
                <thead>
                    <tr>
                        <th>User</th>
                        <th>Wins</th>
                        <th>Loses</th>
                        <th>Games</th>
                    </tr>
                </thead>
                <tbody>
                    <tr
                        v-for="user in users"
                        :key="user.name"
                    >
                        <td>
                            <div class="user-list-item">
                                <div
                                    class="status-dot"
                                    :class="(user.status ?? 'offline') === 'online' ? 'status-dot--online' : 'status-dot--offline'"
                                />
                                <span>
                                    {{ user.name }}
                                    <template v-if="user.name === currentUser?.name"> (you)</template>
                                </span>
                            </div>
                        </td>
                        <td>{{ user.wins }}</td>
                        <td>{{ user.loses }}</td>
                        <td>{{ user.totalGames }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useUserStore } from '@/stores/user.store';
import { storeToRefs } from 'pinia';

const userStore = useUserStore();
const { users, currentUser } = storeToRefs(userStore);

onMounted(() => {
    void userStore.getUsers();
});
</script>

<style scoped lang="scss">
.user-list-item {
    display: flex;
    align-items: center;
    gap: 10px;
}
h1 {
    font-size: 30px;
    font-weight: 600;
    text-align: center;
    margin-bottom: 10px;
}
.status-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
    flex-shrink: 0;

    &--online {
        background-color: #4caf50;
    }

    &--offline {
        background-color: #f44336;
    }
}
</style>
