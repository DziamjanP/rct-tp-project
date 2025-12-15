<template>
  <v-container class="pa-4">
    <v-card class="pa-4" v-if="entry">

      <v-row>
        <v-col cols="12">
          <h2>Book Ticket for Train {{ entry?.trainId }}</h2>
          <p><strong>Departure:</strong> {{ formatDateTime(entry?.departureTime ?? Date.now().toString()) }}</p>
          <p><strong>Arrival:</strong> {{ formatDateTime(entry?.arrivalTime ?? Date.now().toString()) }}</p>
          <p><strong>From:</strong> {{ entry?.train.sourceId }}</p>
          <p><strong>To:</strong> {{ entry?.train.destinationId }}</p>
        </v-col>
      </v-row>

      <!-- Stops -->
      <!--v-row v-if="entry?.stops?.length">
        <v-col cols="12">
          <h3>Stops</h3>
          <v-list dense>
            <v-list-item v-for="stop in entry.stops" :key="stop.id">
              <v-list-item-title>{{ stop.station.name }}</v-list-item-title>
              <v-list-item-subtitle>
                Arrival: {{ formatTime(stop.arrivalTime) }},
                Departure: {{ formatTime(stop.departureTime) }}
              </v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-col>
      </v-row-->

      <v-row v-if="entry?.perkGroups?.length">
        <v-col cols="12" md="6">
          <v-autocomplete
            v-model="selectedPerkGroup"
            :items="entry.perkGroups"
            item-title="name"
            item-value="id"
            label="Select Perk Group"
            clearable
          />
        </v-col>
      </v-row>

      <v-autocomplete
        v-model="selectedPerkGroup"
        :items="perkGroups"
        item-title="name"
        item-value="id"
        return-object
        label="Select PerkGroup"
        clearable
        dense
        outlined
        :search-input.sync="search"
      >
        
      </v-autocomplete>


      <v-row>
        <v-col cols="12">
          <v-btn
            color="primary"
            :loading="loading"
            @click="lockTicket"
          >
            Book Ticket
          </v-btn>
        </v-col>
      </v-row>

    </v-card>
    <v-card v-else>
      Something went wrong
    </v-card>

    <v-dialog v-model="successDialog" max-width="400">
      <v-card>
        <v-card-title class="headline">Ticket Locked!</v-card-title>
        <v-card-text>
          Your ticket has been successfully booked, you have 30 minutes to pay for it. Pay for your tickets in the "My Tickets" page.
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="goToTickets">Pay Now</v-btn>
          <v-btn color="primary" @click="successDialog=false">Later</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  </v-container>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '@/api';
import { useAuthStore } from '@/stores/auth';

const auth = useAuthStore();

interface Stop {
  id: number
  station: { name: string }
  arrivalTime: string
  departureTime: string
}

interface PerkGroup {
  id: number
  name: string
}

interface Entry {
  id: number
  departureStationName: string
  arrivalStationName: string 
  departureTime: string
  arrivalTime: string
  perkGroups?: PerkGroup[]
  trainId: number
  train: {
    id: number,
    sourceId: number,
    destinationId: number,
    typeId: number,
    stops?: Stop[]
  }
  pricePolicyId: number
  pricePolicy?: {
    id: number
    pricePerKm: number
    fixedPrice: number 
  }
}

interface TimeTableResponse {
  entryId: number
  trainId: number
  trainType: string
  departureStationName: string
  arrivalStationName: string
  departureTime: string
  arrivalTime: string
  pricePolicyId?: number
}

interface PerkGroup {
  id: number;
  name: string;
  description?: string;
  fixedPrice?: number;
  discount?: number;
}

const search = ref('');
const perkGroups = ref<PerkGroup[]>([]);
const selectedPerkGroup = ref<PerkGroup | null>(null);

const route = useRoute("/buy/[entryId]")
const router = useRouter()

const entry = ref<Entry | null>(null)
const loading = ref(false)
const successDialog = ref(false)

const entryId = route.params.entryId as string

console.log(entryId);

onMounted(async () => {
  let ttEntry: TimeTableResponse = await api.get('timetable', entryId)
  if (ttEntry) {
    entry.value = {
      id: ttEntry.entryId,
      departureStationName: ttEntry.departureStationName,
      arrivalStationName: ttEntry.arrivalStationName,
      departureTime: ttEntry.departureTime,
      arrivalTime: ttEntry.arrivalTime,
      trainId: ttEntry.trainId,
      train: await api.get('trains', ttEntry.trainId),
      pricePolicyId: ttEntry.pricePolicyId ?? 0,
      pricePolicy: await api.get('pricepolicies', ttEntry.pricePolicyId ?? 0)
    }
  }
  console.log(entry);
  perkGroups.value = await api.list<PerkGroup>('perkgroups');
})

function formatDateTime(dt: string) {
  const d = new Date(dt)
  const day = String(d.getDate()).padStart(2, '0')
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const year = d.getFullYear()
  const hours = String(d.getHours()).padStart(2, '0')
  const minutes = String(d.getMinutes()).padStart(2, '0')
  return `${day}-${month}-${year} ${hours}:${minutes}`
}

function formatTime(dt: string) {
  const d = new Date(dt)
  const hours = String(d.getHours()).padStart(2, '0')
  const minutes = String(d.getMinutes()).padStart(2, '0')
  return `${hours}:${minutes}`
}

async function lockTicket() {
  if (!entry.value) return
  loading.value = true
  try {
    const payload: any = {
      entryId: entry.value.id,
      userId: auth.user?.id,
    }
    if (selectedPerkGroup.value) payload.perkGroupId = selectedPerkGroup.value.id
    await api.create(`buy`, payload)

    successDialog.value = true
  } catch (err) {
    console.error(err)
    alert('Failed to lock ticket')
  } finally {
    loading.value = false
  }
}

function goToTickets() {
  successDialog.value = false
  router.push('/tickets')
}
</script>
