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
</script>
