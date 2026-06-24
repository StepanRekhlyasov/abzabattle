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
    padding: 10px;
    height: calc(100% - 50px);
    background-image: url('@/assets/images/background.png');
    background-size: cover;
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
</style>