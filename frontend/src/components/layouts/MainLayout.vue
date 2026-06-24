<template>
    <div class="main-layout-header">
        <p>Current user: {{ userStore.currentUser?.name }}</p>
        <button @click="authStore.logout" class="generic-button transparent-button">
            <FontAwesomeIcon :icon="faRightFromBracket" />
        </button>
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
import { faRightFromBracket } from '@fortawesome/free-solid-svg-icons'

const userStore = useUserStore();
const authStore = useAuthStore();
</script>
<style scoped lang="scss">
.main-layout-header {
    display: flex;
    justify-content: space-between;
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
</style>