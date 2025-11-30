<template>
  <v-container>
    <h2>Timetable Entry {{ route.params.id }}</h2>
    <p>Details coming soon...</p>
  </v-container>
</template>

<script setup>
import { useRoute } from 'vue-router'
const route = useRoute()
</script>
