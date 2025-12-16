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

  let privelege_map: Record<string, Array<Record<string, string>>> = {
    "unknown": [
        { to: '/', title: 'Home'},
        { to: '/login', title: 'Login'},
        { to: '/register', title: 'Register'},
        { to: '/timetable', title: 'Timetable'},
    ],
    "user": [
      { to: '/', title: 'Home'},
      { to: '/timetable', title: 'Timetable'},
      { to: '/tickets', title: 'My Profile'},
    ],
    "support": [
      { to: '/', title: 'Home'},
      { to: '/admin', title: 'Admin'},
      { to: '/timetable', title: 'Timetable'},
      { to: '/tickets', title: 'My Profile'},
    ],
    "admin": [
      { to: '/', title: 'Home'},
      { to: '/reports', title: 'Reports'},
      { to: '/admin', title: 'Admin'},
      { to: '/timetable', title: 'Timetable'},
      { to: '/tickets', title: 'My Profile'},
    ]
  }
  let access = (auth.isAuthenticated ? (["user", "support", "admin"][auth.accessLevel] ?? "unknown") : "unknown")
  const items = computed(() => privelege_map[access])
</script>
