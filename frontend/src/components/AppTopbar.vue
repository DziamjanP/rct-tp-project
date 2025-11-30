<template>
    <v-app-bar
      app
      dark
      density="comfortable"
    >
      <v-toolbar-title class="mr-6">
        Ticket service
      </v-toolbar-title>

      <v-spacer />

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
  const items = [
    { to: '/', title: 'Home'},
    { to: '/login', title: 'Login'},
    { to: '/register', title: 'Register'},
    { to: '/admin', title: 'Admin'},
    { to: '/timetable', title: 'Timetable'},
    { to: '/buy/:entryId', title: 'BuyTicket'},
    { to: '/details/:type/:id', title: 'Details' },
  ];
</script>
