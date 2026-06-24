import { defineStore } from "pinia";
import { ref } from "vue";
import type { Player } from "@/types/player";
import api from "@/services/api";
import { useAuthStore } from "./auth.store";

export const useUserStore = defineStore('user', {
    state: () => ({
        currentUser: null as Player | null,
    }),   
    actions: {
        async getUser(name: string) {
            const response = await api.get<Player>(`/user`, { params: { name } });
            if(response.status === 200) {
                this.currentUser = response.data;
            }
        },
        setUser(user: Player) {
            this.currentUser = user;
        }
    },
    persist: true,
})  