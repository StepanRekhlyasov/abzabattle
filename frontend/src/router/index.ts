import { useUserStore } from '@/stores/user.store';
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
    },
    {
      path: '/create',
      component: () => import('@/pages/CreatePage.vue'),
    },
  ],
})

router.beforeEach((to) => {
  const userStore = useUserStore();
  if(!userStore.currentUser && to.path !== '/login') {
    return '/login';
  }
  if(userStore.currentUser && to.path === '/login') {
    return '/';
  }
})

export default router
