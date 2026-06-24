import type { Session } from "@/types/session";
import { defineStore } from "pinia";

export const useSessionStore = defineStore('session', {
    state: () => ({
        newSession: null as Session | null,
        onlineSessions: [] as Session[],
    }),
    actions: {
        async createSession() {
            console.log('createSession');
        },
    },
});
