<template>
  <v-container>
    <v-row justify="center">
      <v-col cols="12" md="10">
        <v-card class="pa-4">

          <div class="d-flex justify-space-between align-center mb-3">
            <div class="text-h5">
              {{ title }}
            </div>

            <v-btn icon variant="text" @click="reload">
              <v-icon>mdi-refresh</v-icon>
            </v-btn>
          </div>

          <v-alert v-if="loading" type="info" class="mb-2">
            Loading...
          </v-alert>

          <v-alert v-if="error" type="error" class="mb-2">
            {{ error }}
          </v-alert>

          <v-table v-if="item && !loading">
            <thead>
              <tr>
                <th style="width: 200px">Field</th>
                <th>Value</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(value, key) in item" :key="key">
                <td class="font-weight-bold">{{ key }}</td>
                <td>
                  <pre class="ma-0">{{ formatValue(value) }}</pre>
                </td>
              </tr>
            </tbody>
          </v-table>

        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/api'

const route = useRoute()

const id = computed(() => {
  if ('id' in route.params) {
    return route.params.id as string
  }
  return null
})

const type = computed(() => {
  if ('type' in route.params) {
    return route.params.type as string
  }
  return null
})

const loading = ref(false)
const error = ref<string | null>(null)
const item = ref<Record<string, any> | null>(null)

const title = computed(() =>
  `Details: ${type.value} #${id.value}`
)

async function reload() {
  loading.value = true
  error.value = null

  try {
    item.value = await api.get(type.value ?? "unknown", id.value ?? "")
  }
  catch (e: any) {
    error.value = e?.message ?? String(e) ?? 'Failed to load'
  }
  finally {
    loading.value = false
  }
}

// Auto reload when route changes
watch([type, id], reload, { immediate: true })

function formatValue(value: any) {
  if (typeof value === 'object') {
    return JSON.stringify(value, null, 2)
  }
  if (value === null) return 'null'
  return value.toString()
}
</script>

<style scoped>
pre {
  white-space: pre-wrap;
  font-family: monospace;
}
</style>
