import { defineStore } from 'pinia';

export const useAppStore = defineStore('app', {
    state: () => ({
        isLoading: false,
        version: 'v1.0.0',
    }),
    actions: {
        setLoading(isLoading: boolean) {
            this.isLoading = isLoading;
        },
    },
});
