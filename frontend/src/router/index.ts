import { useAuthStore } from '@/stores/authStore';
import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      component: () => import('@/pages/LoginPage.vue'),
    },
    {
      path: '/',
      component: () => import('@/pages/HomePage.vue'),
    }
  ],
})

router.beforeEach((to) => {
  const authStore = useAuthStore();
  if(!authStore.currentUser && to.path !== '/login') {
    return '/login';
  }
  if(authStore.currentUser && to.path === '/login') {
    return '/';
  }
})

export default router
