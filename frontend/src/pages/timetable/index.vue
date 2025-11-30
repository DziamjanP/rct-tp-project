<template>
  <v-container>
    <v-card class="pa-4 mb-5">
      <v-row>

        <v-col cols="12" md="4">
          <v-text-field
            label="Departure date"
            type="date"
            v-model="search.date"
          />
        </v-col>

        <v-col cols="12" md="4">
          <v-autocomplete
            label="Departure station"
            :items="stations"
            item-title="name"
            item-value="id"
            clearable
            v-model="search.fromStation"
          />
        </v-col>

        <v-col cols="12" md="4">
          <v-autocomplete
            label="Arrival station"
            :items="stations"
            item-title="name"
            item-value="id"
            clearable
            v-model="search.toStation"
          />
        </v-col>

      </v-row>

      <v-btn
        color="primary"
        class="mt-2"
        @click="searchTimetable"
      >
        Search
      </v-btn>
    </v-card>

    <div v-if="results.length === 0" class="text-center text-grey">
      No results found
    </div>

    <div v-for="group in groupedResults" :key="group.date">
      <v-divider class="my-4">
        <strong>{{ formatDate(group.date) }}</strong>
      </v-divider>

      <TrainTimetableCard
        v-for="entry in group.entries"
        :key="entry.id"
        :entry="entry"
      />
    </div>

  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import api from '@/api'
import TrainTimetableCard from '@/components/TrainTimetableCard.vue'

const stations = ref<any[]>([])
const results = ref<any[]>([])

const search = ref({
  date: new Date().toISOString().substring(0, 10),
  fromStation: null,
  toStation: null
})

onMounted(loadStations)

async function loadStations() {
  stations.value = await api.list('stations')
}

async function searchTimetable() {
  const params: any = {}

  if (search.value.date)
    params.after = search.value.date

  if (search.value.fromStation)
    params.fromStationId = search.value.fromStation

  if (search.value.toStation)
    params.toStationId = search.value.toStation

  results.value = await api.searchTimetable(params)
}

// -----------------
// Group by date
// -----------------
const groupedResults = computed(() => {
  const map: Record<string, any[]> = {}

  results.value.forEach(entry => {
    const date = entry.departureTime.split('T')[0]
    if (!map[date]) map[date] = []
    map[date].push(entry)
  })

  return Object.entries(map)
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([date, entries]) => ({
      date,
      entries: entries.sort((e1, e2) => e1.departureTime.localeCompare(e2.departureTime))
    }))
})

function formatDate(isoDate: string) {
  const d = new Date(isoDate)
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const year = d.getFullYear()
  return `${day}-${month}-${year}`
}
</script>