<template>
    <main-layout>
        <div class="wrapper" style="padding: 10px;">
            <div class="item sessions">
                <button @click="goToCreateSession" class="generic-button">
                    Create Session
                </button>
                <session-list />
            </div>
            <div class="item users">
                <user-list />
            </div>
        </div>
    </main-layout>
</template>
<script setup lang="ts">
import MainLayout from '@/components/layouts/MainLayout.vue';
import SessionList from '@/components/widgets/tables/SessionList.vue';
import UserList from '@/components/widgets/tables/UserList.vue';
import router from '@/router';

const goToCreateSession = () => {
    router.push('/create');
}
</script>
<style scoped lang="scss">
.wrapper {
    display: flex;
    gap: 10px;
    width: 100%;
    .item {
        display: flex;
        flex-direction: column;
        gap: 10px;
        &.users {
            width: 300px;
        }
        &.sessions {
            width: 100%;
        }
    }
}
</style>