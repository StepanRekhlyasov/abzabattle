import { defineStore } from "pinia";
import type { Player } from "@/types/player";
import type { PresenceUpdate } from "@/types/presence";
import api from "@/services/api";
import * as userApi from "@/services/userApi";

export const useUserStore = defineStore('user', {
    state: () => ({
        currentUser: null as Player | null,
        users: [] as Player[],
    }),

    actions: {
        async getUser(name: string) {
            const response = await api.get<Player>(`/user`, { params: { name } });
            if (response.status === 200) {
                this.currentUser = response.data;
                this.upsertUser(response.data);
            }
        },
        setUser(user: Player) {
            this.currentUser = user;
            this.upsertUser(user);
        },
        async getUsers() {
            const response = await api.get<Player[]>(`/users`);
            this.users = response.data;
            return response.data;
        },
        setUsers(users: Player[]) {
            this.users = users;
        },
        upsertUser(user: Player) {
            const index = this.users.findIndex(item => item.name === user.name);
            if (index === -1) {
                this.users.push(user);
                return;
            }

            this.users[index] = {
                ...this.users[index],
                ...user,
                status: user.status ?? this.users[index].status,
            };
        },
        updatePresence(update: PresenceUpdate) {
            const user = this.users.find(item => item.name === update.name);
            if (user) {
                user.status = update.status;
                return;
            }

            this.users.push({
                name: update.name,
                wins: 0,
                loses: 0,
                totalGames: 0,
                status: update.status,
            });
        },
        removeUser(name: string) {
            this.users = this.users.filter(user => user.name !== name);
        },
        async deleteUser(targetName: string, adminName: string) {
            await userApi.deleteUser(targetName, adminName);
            this.removeUser(targetName);
        },
    },
    persist: {
        pick: ['currentUser'],
    },
})
