<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <div class="d-flex align-center justify-space-between">
          <div class="text-h5">Timetable</div>
          <v-btn color="primary" @click="load">Refresh</v-btn>
        </div>
      </v-col>

      <v-col cols="12" md="4">
        <v-autocomplete
        :items="fkOptions['stations'] || []"
          v-model="q.source"
          label="Departure Station"
          item-title="__label"
          item-value="id"
          searchable
          clearable  
        />
        <v-autocomplete
        :items="fkOptions['stations'] || []"
          item-title="__label"
          item-value="id"
          searchable
          clearable  
          v-model="q.destination"
          label="Destination Station"
        />
        
        <v-btn color="primary" @click="load">Search</v-btn>
      </v-col>

      <v-col cols="12">
        <v-table>
          <thead>
            <tr><th>Id</th><th>Train</th><th>Departure</th><th>Arrival</th><th>Actions</th></tr>
          </thead>
          <tbody>
            <tr v-for="e in entries" :key="e.id">
              <td>{{ e.id }}</td>
              <td>{{ e.trainId }}</td>
              <td>{{ e.departure }}</td>
              <td>{{ e.arrival }}</td>
              <td>
                <v-btn small variant="plain" :to="`/details/timetable/${e.id}`">Details</v-btn>
                <v-btn small variant="plain" color="primary" :to="`/buy/${e.id}`">Buy</v-btn>
              </td>
            </tr>
          </tbody>
        </v-table>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import api from '@/api'
import { ref } from 'vue'

const entries = ref([])
const q = ref({ source: '', destination: '' })

const fkOptions = ref({})

async function loadFk(key, config) {
  if (fkOptions.value[key]) return
  const data = await api.list(config.entity)
  fkOptions.value[key] = data
}

async function loadStations() {
  let key = 'stations';
  let cfg = { entity: 'stations', label: s => `${s.name} (${s.cityName})` };
  await loadFk(key, cfg)
  
  fkOptions.value[key] =
  fkOptions.value[key]
  .map(item => ({
    ...item,
    __label: cfg.label ? cfg.label(item) : `#${item.id}`
  }))
}

loadStations()

async function load() {
  // backend returns all timetable; we do client-side filter for prototype
  const data = await api.list('timetable')
  let res = data
  if (q.value.source) res = res.filter(x => String(x.trainId) === String(q.value.source) || x.sourceId == q.value.source)
  if (q.value.destination) res = res.filter(x => String(x.trainId) === String(q.value.destination) || x.destinationId == q.value.destination)
  entries.value = res
}

load()
</script>
