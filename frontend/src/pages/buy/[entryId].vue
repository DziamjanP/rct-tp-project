<template>
  <v-container>
    <v-row>
      <v-col cols="12" md="8">
        <v-card class="pa-4">
          <div class="text-h5 mb-2">Buy ticket for entry {{ entryId }}</div>

          <v-text-field v-model="userId" label="Your user id" />
          <v-text-field v-model="invoiceId" label="Invoice id (optional)" />
          <v-text-field v-model="sum" label="Amount" type="number" />

          <v-btn color="primary" @click="reserve">Reserve (create lock)</v-btn>

          <v-alert v-if="message" class="mt-3">{{ message }}</v-alert>

          <div v-if="lock">
            <div class="mt-3">Lock created: {{ lock.id }}</div>
            <v-btn color="success" class="mt-2" @click="pay">Simulate Pay & Confirm</v-btn>
          </div>

        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import api from '@/api'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const entryId = route.params.entryId
const userId = ref('')
const invoiceId = ref('')
const sum = ref(0)
const message = ref('')
const lock = ref(null)

async function reserve() {
  message.value = ''
  try {
    const payload = {
      entryId: Number(entryId),
      userId: Number(userId.value),
      invoiceId: invoiceId.value || null,
      paid: false,
      sum: Number(sum.value)
    }
    const l = await api.createTicketLock(payload)
    lock.value = l
    message.value = 'Lock created. Proceed to pay.'
  } catch (e) {
    message.value = 'Error creating lock: ' + (e.message ?? e)
  }
}

async function pay() {
  try {
    const payment = {
      lockId: lock.value.id,
      invoiceId: lock.value.invoiceId,
      successful: true,
      sum: lock.value.sum
    }
    const p = await api.createPayment({ ...payment, dateTime: new Date().toISOString() })
    // create actual ticket after successful payment
    const t = await api.createTicket({ entryId: Number(entryId), userId: Number(userId.value), used: false })
    message.value = `Payment done. Ticket created with id ${t.id}`
    setTimeout(() => router.push('/tickets'), 800)
  } catch (e) {
    message.value = 'Error making payment: ' + (e.message ?? e)
  }
}
</script>
