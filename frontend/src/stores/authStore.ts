import { defineStore } from "pinia"
import api from "@/services/api"
import type { Player } from "@/types/player";
import router from "@/router";

export const useAuthStore = defineStore('auth', {
    state: () => ({
        currentUser: null as Player | null,
    }),
    actions: {
        async login(name: string) {
            const response = await api.post('/login', { name });
            this.currentUser = response.data;
            router.push('/');
        },
        logout() {
            this.resetAllStores();
            router.push('/login');
        },
        resetAllStores() {
            this.$reset();
        },
    },
    persist: true,
})
