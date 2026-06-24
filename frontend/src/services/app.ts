import { useUserStore } from "@/stores/user.store";
import { useAuthStore } from "@/stores/auth.store";

export const resetAllStores = async () => {
    const authStore = useAuthStore();
    const userStore = useUserStore();
    authStore.$reset();
    userStore.$reset();
}