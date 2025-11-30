<template>
  <div>
    <v-card class="pa-4 mb-4">
      <div class="d-flex justify-space-between align-center">
        <div class="text-h6">{{ title }}</div>
        <v-btn color="primary" @click="openCreate">Add</v-btn>
      </div>

      <v-table class="mt-4">
        <thead>
          <tr>
            <th v-for="h in headers" :key="h">{{ h }}</th>
            <th>Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in items" :key="item.id">
            <td v-for="k in keys" :key="k">{{ formatField(k, item[k]) }}</td>
            <td>
              <v-btn small text variant="plain" color="error" @click="removeItem(item.id)">Delete</v-btn>
              <v-btn small text variant="plain" color="primary" @click="goToDetails(item)">View</v-btn>
            </td>
          </tr>
        </tbody>
      </v-table>
    </v-card>

    <v-dialog v-model="showCreate" width="600">
      <v-card>
        <v-card-title>{{ createTitle }}</v-card-title>
        <v-card-text>
          <v-card-text>
            <slot
              name="form"
              :model="model"
              :update="updateModel"
            >

              <v-row dense>

                <v-col
                  v-for="(value, key) in model"
                  :key="key"
                  cols="12"
                >

                  <v-autocomplete
                    v-if="fks[key]"
                    :label="key"
                    :items="fkOptions[key] || []"
                    item-title="__label"
                    item-value="id"
                    :model-value="model[key]"
                    searchable
                    clearable
                    @update:modelValue="val => updateModel({ [key]: val })"
                  />

                  <v-text-field
                    v-else-if="datetimes.includes(key)"
                    :label="key"
                    readonly
                    :model-value="model[key].display"
                    prepend-inner-icon="mdi-calendar-clock"
                    @click="openDateTimePicker(key, model[key])"
                  />

                  <v-switch
                    v-else-if="typeof value === 'boolean'"
                    :label="key"
                    :model-value="model[key]"
                    @update:modelValue="val => updateModel({ [key]: val })"
                  />

                  <v-text-field
                    v-else-if="typeof value === 'number'"
                    type="number"
                    :label="key"
                    :model-value="model[key]"
                    @update:modelValue="val => updateModel({ [key]: val })"
                  />

                  <v-text-field
                    v-else
                    :label="key"
                    :model-value="model[key]"
                    @update:modelValue="val => updateModel({ [key]: val })"
                  />

                </v-col>

              </v-row>

            </slot>
          </v-card-text>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn text @click="showCreate = false">Cancel</v-btn>
          <v-btn color="primary" @click="save">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>

  <v-dialog v-model="dtDialog" width="380">
    <v-card>
      <v-card-title>Select date and time</v-card-title>

      <v-card-text>

        <v-date-picker
          v-model="dtDate"
          hide-header
          show-adjacent-months
        />

        <v-time-picker
          v-model="dtTime"
          format="24hr"
        />

      </v-card-text>

      <v-card-actions>
        <v-spacer />
        <v-btn text @click="dtDialog=false">Cancel</v-btn>
        <v-btn color="primary" @click="saveDateTime">Save</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>
<script setup lang="ts">
import { ref, toRaw, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/api'

const errEmit = defineEmits(['error', 'refresh'])

type ID = string | number
type AnyObj = Record<string, any>

interface FkConfig {
  entity: string
  label?: (item: AnyObj) => string
  [key: string]: any
}

interface Props {
  entity?: string
  title?: string
  headers?: any[]
  keys?: string[]
  createTitle?: string
  defaultModel?: Record<string, any>
  detailsBase?: string | null
  datetimes?: string[]
  fks?: Record<string, FkConfig>
}

const props = withDefaults(defineProps<Props>(), {
  headers: () => [],
  keys: () => [],
  createTitle: 'Create',
  defaultModel: () => ({}),
  detailsBase: null,
  datetimes: () => [],
  fks: () => ({}),
})
//const emit = defineEmits<{ edit: (payload: any) => void }>()

const router = useRouter()

const fkOptions = ref<Record<string, unknown[]>>({})

async function loadFk(key: string, config: FkConfig) {
  if (fkOptions.value[key]) return
  const data = await api.list(config.entity)
  fkOptions.value[key] = data
}

const items = ref<AnyObj[]>([])
const showCreate = ref<boolean>(false)
const model = ref<AnyObj>({ ...props.defaultModel })

const dtDialog = ref<boolean>(false)
const dtKey = ref<string | null>(null)
const dtDate = ref<string | null>(null)
const dtTime = ref<string>('12:00')

watch(showCreate, async (opened) => {
  if (!opened) return

  // load fk options and decorate with __label
  for (const [key, cfg] of Object.entries(props.fks || {})) {
    await loadFk(key, cfg)

    const list = fkOptions.value[key] as AnyObj[]

    fkOptions.value[key] = list.map(item => ({
      ...item,
      __label: cfg.label ? cfg.label(item) : `#${item.id}`,
    }))
  }
})

function openDateTimePicker(key: string, currentValue?: string | null) {
  dtKey.value = key

  if (currentValue && typeof currentValue === 'string' && currentValue.includes('T')) {
    const parts = currentValue.split('T')
    dtDate.value = parts[0] || null
    dtTime.value = parts[1]?.substring(0, 5) ?? '12:00'
  } else {
    dtDate.value = new Date().toISOString().slice(0, 10)
    dtTime.value = '12:00'
  }

  dtDialog.value = true
}

function saveDateTime() {
  console.log(dtDate.value);
  console.log(Date.parse(dtDate.value ?? ""));
  console.log(new Date(Date.parse(dtDate.value ?? "")).toISOString().slice(0, 10));
  const d = new Date(Date.parse(dtDate.value ?? "")).toISOString().slice(0, 10) ?? new Date().toISOString().slice(0, 10)
  const t = dtTime.value ?? '12:00'

  const display = `${d} ${t}`
  const value = `${d}T${t}:00Z`

  updateModel({
    [dtKey.value as string]: {
      display,
      value,
    },
  })

  dtDialog.value = false
}

async function load() {
  const ent = props.entity ?? 'unknown'
  items.value = await api.list(ent)
}

onMounted(() => {
  load()
})

function openCreate() {
  model.value = { ...(props.defaultModel ?? {}) }
  showCreate.value = true
}

function updateModel(p: AnyObj) {
  model.value = { ...model.value, ...p }
}

async function save() {
  (props.datetimes || []).forEach((field) => {
    if (model.value[field] && typeof model.value[field] === 'object' && 'value' in model.value[field]) {
      model.value[field] = model.value[field].value
    }
  })

  await createItem(toRaw(model.value))
  showCreate.value = false
  await load()
}

async function createItem(item: AnyObj) {
  try {
    await api.create(props.entity ?? 'unknown', item)
    errEmit('refresh')
  } catch (err) {
    errEmit('error', err)
  }
}


async function removeItem(id: ID) {
  await api.remove(props.entity ?? 'unknown', id)
  await load()
}

function formatField(field: string, val: any) {
  if (!val || typeof val !== 'string') return val
  if (field.toLowerCase() === 'description') {
    return `${val.substring(0, 16)}...`
  }
  return val
}

function goToDetails(item: AnyObj) {
  const base = props.detailsBase ?? props.entity
  router.push(`/details/${encodeURIComponent(base ?? '')}/${encodeURIComponent(String(item.id))}`)
}

function detailsRoute(item: AnyObj): string {
  return props.detailsBase ? `/${props.detailsBase}/${item.id}` : '/'
}
</script>
