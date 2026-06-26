import { defineStore } from 'pinia';

export const useAppStore = defineStore('app', {
    state: () => ({
        isLoading: false,
        version: 'v0.9.3b',
    }),
    actions: {
        setLoading(isLoading: boolean) {
            this.isLoading = isLoading;
        },
    },
});
