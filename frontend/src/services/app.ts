import { useUserStore } from "@/stores/user.store";
import { useAuthStore } from "@/stores/auth.store";
import { disconnectWs } from "@/services/ws";

export const resetAllStores = async () => {
    const authStore = useAuthStore();
    const userStore = useUserStore();
    authStore.$reset();
    userStore.$reset();
    await disconnectWs();

}