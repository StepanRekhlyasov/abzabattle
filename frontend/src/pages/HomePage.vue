<template>
    <main-layout>
        <h1>Home Page</h1>
        <h2>Users Online</h2>
        <user-list />
        <h2>Sessions</h2>
        <session-list />
        <h2>Create Session</h2>
    </main-layout>
</template>
<script setup lang="ts">
import MainLayout from '@/components/layouts/MainLayout.vue';
import SessionList from '@/components/widgets/tables/SessionList.vue';
import UserList from '@/components/widgets/tables/UserList.vue';
</script>