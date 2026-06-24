import { defineStore } from "pinia";
import type { Player } from "@/types/player";
import type { PresenceUpdate } from "@/types/presence";
import api from "@/services/api";

export const useUserStore = defineStore('user', {
    state: () => ({
        currentUser: null as Player | null,
        onlineUsers: [] as Player[],
    }),
    actions: {
        async getUser(name: string) {
            const response = await api.get<Player>(`/user`, { params: { name } });
            if (response.status === 200) {
                this.currentUser = response.data;
            }
        },
        setUser(user: Player) {
            this.currentUser = user;
        },
        async getOnlineUsers() {
            const response = await api.get<Player[]>(`/users/online`);
            this.onlineUsers = response.data;
            return response.data;
        },
        setOnlineUsers(users: Player[]) {
            this.onlineUsers = users;
        },
        updatePresence(update: PresenceUpdate) {
            if (update.status === 'online') {
                if (!this.onlineUsers.some(user => user.name === update.name)) {
                    this.onlineUsers.push({ name: update.name });
                }
                return;
            }

            this.onlineUsers = this.onlineUsers.filter(user => user.name !== update.name);
        },
        clearOnlineUsers() {
            this.onlineUsers = [];
        },
    },
    persist: {
        pick: ['currentUser'],
    },
})
