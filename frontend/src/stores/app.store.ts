import { defineStore } from 'pinia';

export const useAppStore = defineStore('app', {
    state: () => ({
        isLoading: false,
        version: 'v0.9.2b',
    }),
    actions: {
        setLoading(isLoading: boolean) {
            this.isLoading = isLoading;
        },
    },
});
