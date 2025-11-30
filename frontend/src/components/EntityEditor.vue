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
              <!--v-btn small text variant="tonal" @click="$emit('edit', item)">Edit</v-btn-->
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

              <!-- Auto-generated form -->
              <v-row dense>

                <v-col
                  v-for="(value, key) in model"
                  :key="key"
                  cols="12"
                >

                  <!-- Foreign-key picker -->
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

                  <!-- DateTime Picker Field -->
                  <v-text-field
                    v-else-if="datetimes.includes(key)"
                    :label="key"
                    readonly
                    :model-value="model[key]"
                    prepend-inner-icon="mdi-calendar-clock"
                    @click="openDateTimePicker(key, model[key])"
                  />

                  <!-- Boolean -->
                  <v-switch
                    v-else-if="typeof value === 'boolean'"
                    :label="key"
                    :model-value="model[key]"
                    @update:modelValue="val => updateModel({ [key]: val })"
                  />

                  <!-- Number -->
                  <v-text-field
                    v-else-if="typeof value === 'number'"
                    type="number"
                    :label="key"
                    :model-value="model[key]"
                    @update:modelValue="val => updateModel({ [key]: val })"
                  />

                  <!-- String fallback -->
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

<script setup>
import { ref, toRaw } from 'vue'
import api from '@/api'
import { useRouter } from 'vue-router'
const router = useRouter()

const fkOptions = ref({})

async function loadFk(key, config) {
  if (fkOptions.value[key]) return
  const data = await api.list(config.entity)
  fkOptions.value[key] = data
}

const props = defineProps({
  entity: String,
  title: String,
  headers: Array,
  keys: Array,
  createTitle: { type: String, default: 'Create' },
  defaultModel: { type: Object, default: () => ({}) },
  detailsBase: { type: String, default: null },
  datetimes: {
    type: Array,
    default: () => []
  },
  fks: {
    type: Object,
    default: () => ({})
  },
})

const emit = defineEmits(['edit'])
const items = ref([])
const showCreate = ref(false)
const model = ref({ ...props.defaultModel })

watch(showCreate, async opened => {
  if (!opened) return

  for (const [key, cfg] of Object.entries(props.fks)) {
    // decorate option items with labels
    await loadFk(key, cfg)

    fkOptions.value[key] =
      fkOptions.value[key]
        .map(item => ({
          ...item,
          __label: cfg.label ? cfg.label(item) : `#${item.id}`
        }))
  }
})

const dtDialog = ref(false)
const dtKey = ref(null)
const dtDate = ref(null)
const dtTime = ref('12:00')

function openDateTimePicker(key, currentValue) {
  dtKey.value = key
  dtDate.value = currentValue.length > 0 ? currentValue.split('T')[0] : new Date().toISOString().slice(0,10)
  dtTime.value = currentValue?.split?.('T')?.[1]?.substring(0,5) ?? '12:00'
  dtDialog.value = true
}

function saveDateTime() {
  const value = `${dtDate.value}T${dtTime.value}:00Z`
  updateModel({ [dtKey.value]: value })
  dtDialog.value = false
}

async function load() {
  items.value = await api.list(props.entity ?? "unknown")
}
load()

function openCreate() {
  model.value = { ...props.defaultModel }
  showCreate.value = true
}

function updateModel(p) {
  model.value = { ...model.value, ...p }
}

async function save() {
  await api.create(props.entity ?? "unknown", toRaw(model.value))
  showCreate.value = false
  await load()
}

async function removeItem(id) {
  await api.remove(props.entity ?? "unknown", id)
  await load()
}

function formatField(field, val){
  if (field.toLowerCase() == 'description') {
    return `${val.substring(0, 16)}...`;
  }
  return val;
}

function goToDetails(item) {
  const base = props.detailsBase ?? props.entity
  router.push(`/details/${encodeURIComponent(base)}/${encodeURIComponent(item.id)}`)
}

function detailsRoute(item) {
  return props.detailsBase ? `/${props.detailsBase}/${item.id}` : '/'
}
</script>
