<template>
    <main-layout>
        <h1>Home Page</h1>
        <h2>Sessions</h2>
        <session-list />
        <h2>Create Session</h2>
    </main-layout>
</template>
<script setup lang="ts">
import MainLayout from '@/components/layouts/MainLayout.vue';
import SessionList from '@/components/widgets/session-list/SessionList.vue';
</script>