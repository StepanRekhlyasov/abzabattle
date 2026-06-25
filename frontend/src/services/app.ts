import { useUserStore } from "@/stores/user.store";
import { useAppStore } from "@/stores/app.store";
import { useAuthStore } from "@/stores/auth.store";
import { useDraftStore } from "@/stores/draft.store";
import { useSessionStore } from "@/stores/session.store";

export const resetAllStores = async () => {
    const authStore = useAuthStore();
    const appStore = useAppStore();
    const userStore = useUserStore();
    const draftStore = useDraftStore();
    const sessionStore = useSessionStore();
    authStore.$reset();
    appStore.$reset();
    userStore.$reset();
    draftStore.$reset();
    sessionStore.$reset();
}