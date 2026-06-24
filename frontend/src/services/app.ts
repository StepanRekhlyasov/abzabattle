import { useUserStore } from "@/stores/user.store";
import { useAuthStore } from "@/stores/auth.store";
import { useDraftStore } from "@/stores/draft.store";
import { useSessionStore } from "@/stores/session.store";

export const resetAllStores = async () => {
    const authStore = useAuthStore();
    const userStore = useUserStore();
    const draftStore = useDraftStore();
    const sessionStore = useSessionStore();
    authStore.$reset();
    userStore.$reset();
    draftStore.$reset();
    sessionStore.$reset();
}