<template>
  <v-card class="pa-6 mb-3 elevation-3">
    <v-row align="center" class="px-3">
      
      <!-- Left block: icon + info -->
      <div class="d-flex align-center">
        <v-icon size="40" class="mr-4">
          mdi-train
        </v-icon>

        <div>
          <div class="text-caption text-grey">
            {{ entry.trainTypeName }}
          </div>

          <div class="font-weight-medium">
            {{ entry.departureStationName }}
            <span class="mx-1">→</span>
            {{ entry.arrivalStationName }}
          </div>

          <div class="text-body-2 text-grey">
            {{ formatTime(entry.departureTime) }}
            —
            {{ formatTime(entry.arrivalTime) }}
          </div>
        </div>
      </div>

      <!-- Push buttons to right -->
      <v-spacer />

      <!-- Right block: vertical buttons -->
      <div class="d-flex flex-column align-end">
        <v-btn
          color="primary"
          density="comfortable"
          class="mt-1"
          @click="buyTicket"
        >
          Buy
        </v-btn>
      </div>

    </v-row>
  </v-card>
</template>


<script setup lang="ts">
import { useRouter } from 'vue-router'

const props = defineProps({
  entry: {
    type: Object,
    required: true
  }
})

const router = useRouter()

function openDetails() {
  router.push(`/timetable/${props.entry.entryId}`)
}

function buyTicket() {
  router.push(`/buy/${props.entry.entryId}`)
}

function formatTime(value: string | number | Date) {
  return new Date(value).toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit'
  })
}
</script>
