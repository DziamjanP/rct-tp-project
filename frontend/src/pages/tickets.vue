<template>
  <v-app class="profile-page">
    <v-container fluid class="fill-height pa-4">
      <v-row class="fill-height">
        <v-col md="3">
          <v-card class="pa-4">
            <v-card-title class="justify-center">
              <v-avatar size="120">
                <v-icon large size="120">mdi-account</v-icon>
              </v-avatar>
            </v-card-title>

            <v-card-subtitle class="text-center">
              {{ user?.name }} {{ user?.surname }}
            </v-card-subtitle>

            <v-card-text>
              <v-list dense>
                <v-list-item>
                  <v-list-item-title>Phone</v-list-item-title>
                  <v-list-item-subtitle>{{ user?.phone }}</v-list-item-subtitle>
                </v-list-item>

                <v-list-item v-if="isAdmin">
                  <v-list-item-title>Admin Status</v-list-item-title>
                  <v-list-item-subtitle>Yes</v-list-item-subtitle>
                </v-list-item>
              </v-list>
            </v-card-text>

            <v-card-actions class="justify-center">
              <v-btn color="red" @click="logout">Logout</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>

        <v-col cols="12" md="9">
          <v-card>
            <v-tabs
              v-model="activeTab"
              background-color="primary"
              dark
              grow
            >
              <v-tab value="tickets">Tickets</v-tab>
              <v-tab value="locks">Bookings</v-tab>
              <v-tab value="archive">Ticket Archive</v-tab>
              <v-tab value="payments">Payment History</v-tab>
            </v-tabs>

            <v-divider></v-divider>

            <v-card-text v-if="activeTab === 'tickets'">
              <h3>Tickets</h3>
              <v-card
                v-for="ticket in tickets"
                :key="ticket.id"
                class="mb-3 pa-3"
                elevation="2"
                variant="tonal"
                @click="openTicketDetails(ticket)"
              >
                <v-row align="center">

                  <v-col cols="1" class="text-center">
                    <v-icon size="36">mdi-train</v-icon>
                  </v-col>

                  <v-col cols="8">
                    <div class="text-caption text-grey">
                      {{ ticket.entry.train.type }}
                    </div>

                    <div class="font-weight-medium">
                      {{ ticket.entry.train.source.name }}
                      →
                      {{ ticket.entry.train.destination.name }}
                    </div>

                    <div class="text-body-2 text-grey">
                      {{ formatDateTime(ticket.entry.departureTime) }}
                      —
                      {{ formatDateTime(ticket.entry.arrivalTime) }}
                    </div>
                  </v-col>

                </v-row>
              </v-card>
            </v-card-text>

            <v-card-text v-else-if="activeTab === 'locks'">
              <v-row
                justify="space-between"
                align="center"
                class="pa-3"
              >
                <h3>Bookings</h3>
                <v-checkbox
                  label="Hide inactive"
                  v-model="hideInactiveLocks"
                  @update:modelValue="reformatList('locks')"
                  hide-details
                  density="compact"
                  class="mr-2"
                ></v-checkbox>
              </v-row>
              <v-card
                v-for="lock in locks"
                :key="lock.id"
                class="mb-3 pa-3"
                elevation="2"
                variant="tonal"
                @click="openLockDetails(lock)"
              >
                <v-row align="center">

                  <v-col cols="1" class="text-center">
                    <v-icon size="36">mdi-train</v-icon>
                  </v-col>

                  <v-col cols="8">
                    <div class="text-caption text-grey">
                      {{ lock.entry.train.type }}
                    </div>

                    <div class="font-weight-medium">
                      {{ lock.entry.train.source.name }}
                      →
                      {{ lock.entry.train.destination.name }}
                    </div>

                    <div class="text-body-2 text-grey">
                      {{ formatDateTime(lock.entry.departureTime) }}
                      —
                      {{ formatDateTime(lock.entry.arrivalTime) }}
                    </div>
                  </v-col>

                  <v-col cols="3" class="text-right">
                    <div class="font-weight-bold">
                      €{{ lock.sum.toFixed(2) }}
                    </div>

                    <v-chip
                      :color="lock.paid ? 'green' : 'orange'"
                      size="small"
                      class="mt-1"
                    >
                      {{ lock.paid ? 'Paid' : 'Unpaid' }}
                    </v-chip>

                    <div class="text-right mr-2" v-if="!lock.paid">
                      <span v-if="!isExpired(lock)">
                        {{ formatRemaining(timeLeft(lock)) }}
                      </span>
                      <span v-else class="text-red">
                        Expired
                      </span>
                    </div>
                    
                  </v-col>

                </v-row>
              </v-card>
              <v-divider class="my-4" />
              <v-row justify="end">
                <v-btn
                  color="primary"
                  :disabled="unpaidLocks.length === 0"
                  @click="openPayment(unpaidLocks)"
                >
                  Buy all unpaid
                </v-btn>
              </v-row>
            </v-card-text>

            <v-card-text v-else-if="activeTab === 'archive'">
              <h3>Ticket Archive</h3>
              <v-card
                v-for="ticket in ticketsArhived"
                :key="ticket.id"
                class="mb-3 pa-3"
                elevation="2"
                variant="tonal"
                @click="openTicketDetails(ticket)"
              >
                <v-row align="center">

                  <v-col cols="1" class="text-center">
                    <v-icon size="36">mdi-train</v-icon>
                  </v-col>

                  <v-col cols="8">
                    <div class="text-caption text-grey">
                      {{ ticket.entry.train.type }}
                    </div>

                    <div class="font-weight-medium">
                      {{ ticket.entry.train.source.name }}
                      →
                      {{ ticket.entry.train.destination.name }}
                    </div>

                    <div class="text-body-2 text-grey">
                      {{ formatDateTime(ticket.entry.departure) }}
                      —
                      {{ formatDateTime(ticket.entry.arrival) }}
                    </div>
                  </v-col>

                </v-row>
              </v-card>
            </v-card-text>

            <!--v-card-text v-else-if="activeTab === 'payments'">
              <h3>Payment History</h3>
              <p>Payment history content here.</p>
            </v-card-text-->
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </v-app>

  <v-dialog v-model="ticketDialog" max-width="600">
    <v-card v-if="selectedTicket">
      <v-card-title>
        Ticket details
      </v-card-title>

      <v-card-text>
        <div class="text-caption text-grey mb-1">
          {{ selectedTicket.entry.train.name }}
        </div>

        <div class="font-weight-medium">
          {{ selectedTicket.entry.train.source.name }}
          →
          {{ selectedTicket.entry.train.destination.name }}
        </div>

        <div class="text-body-2 text-grey mb-3">
          {{ formatDateTime(selectedTicket.entry.departure) }}
          —
          {{ formatDateTime(selectedTicket.entry.arrival) }}
        </div>

        <div>
          Used: {{ selectedTicket.used }}
        </div>

      </v-card-text>

      <v-card-actions>
        <v-spacer />
    
        <v-btn v-if="!selectedTicket.used" variant="text" @click="refundTicket(selectedTicket)">
          Refund
        </v-btn>

        <v-btn variant="text" color="primary" @click="ticketDialog = false">
          Ok
        </v-btn>
    
      </v-card-actions>
    </v-card>
  </v-dialog>


  <v-dialog v-model="lockDialog" max-width="600">
    <v-card v-if="selectedLock">
      <v-card-title>
        Booking Details
      </v-card-title>

      <v-card-text>
        <div class="text-caption text-grey mb-1">
          {{ selectedLock.entry.train.name }}
        </div>

        <div class="font-weight-medium">
          {{ selectedLock.entry.train.source.name }}
          →
          {{ selectedLock.entry.train.destination.name }}
        </div>

        <div class="text-body-2 text-grey mb-3">
          {{ formatDateTime(selectedLock.entry.departureTime) }}
          —
          {{ formatDateTime(selectedLock.entry.arrivalTime) }}
        </div>

        <v-divider class="my-2" />

        <div class="d-flex justify-space-between">
          <span>Price</span>
          <strong>€{{ selectedLock.sum.toFixed(2) }}</strong>
        </div>

        <div class="d-flex justify-space-between mt-1">
          <span>Status</span>
          <v-chip
            :color="selectedLock.paid ? 'green' : 'orange'"
            size="small"
          >
            {{ selectedLock.paid ? 'Paid' : 'Unpaid' }}
          </v-chip>
        </div>
        <v-divider class="my-3" />
  
        <div class="d-flex justify-space-between align-center">
          <span>Time remaining</span>
    
          <v-chip
          :color="isExpired(selectedLock) ? 'red' : 'orange'"
            size="small"
            >
            <span v-if="!isExpired(selectedLock)">
              {{ formatRemaining(timeLeft(selectedLock)) }}
            </span>
            <span v-else>
              Expired
            </span>
          </v-chip>
        </div>
      </v-card-text>

      <v-card-actions>
        <v-spacer />
    
        <v-btn variant="text" @click="lockDialog = false">
          Close
        </v-btn>
    
        <v-btn
          v-if="!selectedLock.paid"
          color="primary"
          :disabled="isExpired(selectedLock)"
          @click="buyFromDetails"
        >
          Buy
        </v-btn>
    
      </v-card-actions>
    </v-card>
  </v-dialog>


  <v-dialog v-model="paymentDialog" max-width="500">
  <v-card>
    <v-card-title>Payment</v-card-title>

    <v-card-text>
      <div
        v-for="lock in selectedLocks"
        :key="lock.id"
        class="mb-2"
      >
        {{ lock.entry.train.source.name }}
        →
        {{ lock.entry.train.destination.name }}
        —
        €{{ lock.sum.toFixed(2) }}
      </div>

      <v-divider class="my-2" />

      <div class="font-weight-bold text-right">
        Total: €{{ totalSum.toFixed(2) }}
      </div>
    </v-card-text>

    <v-card-actions>
      <v-spacer />
      <v-btn variant="text" @click="paymentDialog = false">
        Cancel
      </v-btn>
      <v-btn color="primary" @click="confirmPayment">
        Pay
      </v-btn>
    </v-card-actions>
  </v-card>
</v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted, onUnmounted  } from 'vue'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'
import api from '@/api'

interface TicketLock {
  id: number
  entryId: number
  userId: number
  invoiceId: string | null
  paid: boolean
  sum: number
  createdAt: string
  train: any
  entry: any
}

interface Ticket {
  id: number
  entryId: number
  userId: number
  used: boolean
  entry: any
}

const auth = useAuthStore()

const user = computed(() => auth.user)
const isAdmin = computed(() => auth.isAdmin)

const activeTab = ref('tickets')

const hideInactiveLocks = ref(false);

const locks = ref<TicketLock[]>([])
const locksRetrieved = ref<TicketLock[]>([]);

const tickets = ref<Ticket[]>([])
const ticketsRetrieved = ref<Ticket[]>([]);
const ticketsArhived = ref<Ticket[]>([]);

const paymentDialog = ref(false)
const selectedLocks = ref<TicketLock[]>([])

const unpaidLocks = computed(() =>
locksRetrieved.value.filter(l => !l.paid)
)

const totalSum = computed(() =>
selectedLocks.value.reduce((sum, l) => sum + l.sum, 0)
)

const lockDialog = ref(false)
const selectedLock = ref<TicketLock | null>(null)

const ticketDialog = ref(false)
const selectedTicket = ref<Ticket | null>(null)

const now = ref(Date.now())
let timer: number

onMounted(() => {
  timer = window.setInterval(() => {
    now.value = Date.now()
  }, 1000);
  loadTickets();
  auth.updateUser();
})

onUnmounted(() => {
  clearInterval(timer)
})

function expiresAt(lock: TicketLock) {
  return new Date(lock.createdAt).getTime() + 30 * 60 * 1000
}

function timeLeft(lock: TicketLock) {
  const diff = expiresAt(lock) - now.value
  return diff > 0 ? diff : 0
}

function isExpired(lock: TicketLock) {
  return timeLeft(lock) === 0
}

function formatRemaining(ms: number) {
  const totalSeconds = Math.floor(ms / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${minutes}:${seconds.toString().padStart(2, '0')}`
}


watch(activeTab, (tab) => {
  if (tab.toLowerCase() == 'locks'){
    loadLocks();
  }
  else if (tab.toLowerCase() == 'tickets'){
    loadTickets();
  }
});

function reformatList(list_name: string) {
  list_name = list_name.toLowerCase();
  if (list_name == 'locks') {
    locks.value = [];
    if (!hideInactiveLocks.value) {
      locks.value = locksRetrieved.value;
      return;
    }
    locks.value = locksRetrieved.value.filter(l => l.paid == false);
  }
}

async function loadLocks() {
  locks.value = await api.list('ticketlocks');
  locksRetrieved.value = locks.value;
  reformatList('locks');
}

async function loadTickets() {
  ticketsRetrieved.value = await api.list('tickets');
  tickets.value = ticketsRetrieved.value.filter(t => t.used == false);
  ticketsArhived.value = ticketsRetrieved.value.filter(t => t.used == true);
  //reformatList('locks');
}

function formatDateTime(value: string) {
  return new Date(value).toLocaleString([], {
    dateStyle: 'medium',
    timeStyle: 'short'
  })
}

function openLockDetails(lock: TicketLock) {
  selectedLock.value = lock
  lockDialog.value = true
}

function openTicketDetails(lock: Ticket) {
  selectedTicket.value = lock
  ticketDialog.value = true
}

function buyFromDetails() {
  if (!selectedLock.value) return

  lockDialog.value = false
  openPayment([selectedLock.value])
}


function openPayment(locks: TicketLock[]) {
  selectedLocks.value = locks
  paymentDialog.value = true
}

async function confirmPayment() {
  try {
    await api.createPayment({
      BookingIds: selectedLocks.value.map(l => l.id)
    })
    paymentDialog.value = false
    await loadLocks()
    await loadTickets()
  } catch (e) {
    console.error(e)
  }
}


async function refundTicket(ticket: Ticket) {
  try {
    await api.refundPayment(
    ticket.id,
    {})
    ticketDialog.value = false
    await loadLocks()
    await loadTickets()
  } catch (e) {
    console.error(e)
  }
}

if (!auth.isAuthenticated) {
  router.push('/')
}

function logout() {
  auth.logout()
  router.go(0)
}
</script>

<style scoped>
.profile-page,
.fill-height {
  height: 80%;
  overflow: hidden;
}
</style>
