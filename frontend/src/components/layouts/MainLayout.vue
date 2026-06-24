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
        <p style="text-align: center;">Wins: {{ userStore.currentUser?.wins ?? 0 }}</p>
        <p style="text-align: center;">Loses: {{ userStore.currentUser?.loses ?? 0 }}</p>
        <p style="text-align: center;">Total games: {{ userStore.currentUser?.totalGames ?? 0 }}</p>
    </div>
</template>
<script setup lang="ts">
import { useUserStore } from '@/stores/user.store';
import { useAuthStore } from '@/stores/auth.store';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faHouse, faRightFromBracket } from '@fortawesome/free-solid-svg-icons'
import router from '@/router';

const userStore = useUserStore();
const authStore = useAuthStore();
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
    justify-content: center;
    align-items: center;
    gap: 10px;
}
</style>