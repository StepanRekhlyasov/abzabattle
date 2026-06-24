import { defineStore } from "pinia"
import api from "@/services/api"
import router from "@/router";
import { useUserStore } from "./user.store";
import { resetAllStores } from "@/services/app";

export const useAuthStore = defineStore('auth', {
    actions: {
        async login(name: string) {
            const response = await api.post<{ success: boolean }>('/login', { name });
            if (response.data.success) {
                await useUserStore().getUser(name);
                router.push('/');
            }
        },
        logout() {
            resetAllStores();
            router.push('/login');
        },
    },
    persist: true,
})
