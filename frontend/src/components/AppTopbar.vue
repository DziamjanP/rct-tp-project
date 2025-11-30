<template>
    <v-app-bar
      app
      dark
      density="comfortable"
    >
      <v-toolbar-title class="mr-6">
        Ticket service
      </v-toolbar-title>

      <!-- Top Nav Buttons -->
      <v-btn
        v-for="item in items"
        :key="item.title"
        :to="item.to"
        variant="plain"
        router
        exact
      >
        {{ item.title }}
      </v-btn>
  </v-app-bar>
</template>
<script lang="ts" setup>
  import { useAuthStore } from '@/stores/auth';
  const auth = useAuthStore()

  const items = computed(() => auth.isAuthenticated ? (auth.isAdmin ? [
    { to: '/', title: 'Home'},
    { to: '/admin', title: 'Admin'},
    { to: '/timetable', title: 'Timetable'},
    { to: '/tickets', title: 'My tickets'},
  ] : [
    { to: '/', title: 'Home'},
    { to: '/admin', title: 'Admin'},
    { to: '/timetable', title: 'Timetable'},
    { to: '/tickets', title: 'My tickets'},
  ]) : [
    { to: '/', title: 'Home'},
    { to: '/login', title: 'Login'},
    { to: '/register', title: 'Register'},
    { to: '/timetable', title: 'Timetable'},
  ]);
</script>
