import { defineStore } from "pinia"
import api from "@/services/api"


export const useAuthStore = defineStore('auth', () => {
    const login = async (name: string) => {
        const response = await api.post('/login', {
            method: 'POST',
            body: JSON.stringify({ name }),
        });
        return response.data;
    }

    return {
        login
    }
})