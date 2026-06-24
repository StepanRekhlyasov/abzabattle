<template>
    <div class="abzabattle-table">
        <div v-if="onlineUsers.length === 0" class="user-list-empty">
            No users online
        </div>
        <table>
            <thead>
                <tr>
                    <th>Users Online</th>
                </tr>
            </thead>
            <tbody>
                <tr
                    v-for="user in onlineUsers"
                    :key="user.name"
                >
                    <td><div class="user-list-item">
                        <div class="green-dot"></div>{{ user.name }}
                    </div></td>
                </tr>
            </tbody>
        </table>
    </div>
</template>

<script setup lang="ts">
import { useUserStore } from '@/stores/user.store';
import { storeToRefs } from 'pinia';
import { onMounted } from 'vue';

const userStore = useUserStore();
const { onlineUsers } = storeToRefs(userStore);

onMounted(async () => {
    if (onlineUsers.value.length === 0) {
        await userStore.getOnlineUsers();
    }
});
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

