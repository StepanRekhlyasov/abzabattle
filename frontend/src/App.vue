<template>
  <v-app>
    <VSonner position="top-center" />
    <v-main>
      <router-view />
    </v-main>
  </v-app>
</template>

<script lang="ts" setup>
import 'vuetify-sonner/style.css'
import '@/styles/vuetify-sonner-overrides.scss'
import { VSonner } from 'vuetify-sonner'
import { useUserStore } from '@/stores/user.store.ts'
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth.store.ts'

const userStore = useUserStore();
const authStore = useAuthStore();

onMounted(async () => {
  if (userStore.currentUser) {
    await authStore.login(userStore.currentUser.name, false);
  }
})
</script>
