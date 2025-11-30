<template>
  <v-container>
    <v-row justify="center">
      <v-col cols="12" md="6">
        <v-card class="pa-4">
          <div class="text-h5 mb-4">Register (dev only)</div>

          <v-text-field v-model="name" label="Name" />
          <v-text-field v-model="surname" label="Surname" />
          <v-text-field v-model="phone" label="Phone" />
          <v-text-field v-model="password" type="password" label="Password" />

          <v-row class="mt-4">
            <v-col>
              <v-btn color="primary" @click="submit">Register</v-btn>
              <v-btn text to="/login">Back to login</v-btn>
            </v-col>
          </v-row>

          <v-alert v-if="message" type="success" class="mt-4">{{ message }}</v-alert>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import api from '@/api'
import { useRouter } from 'vue-router'

const name = ref('')
const surname = ref('')
const phone = ref('')
const password = ref('')
const message = ref('')
const router = useRouter()

async function submit() {
  try {
    const payload = { name: name.value, surname: surname.value, phone: phone.value, password: password.value, salt: '' }
    await api.create('users', payload)
    message.value = 'Registered. You can login now.'
    setTimeout(() => router.push('/login'), 800)
  } catch (e) {
    message.value = `Error: ${e.message ?? e}`
  }
}
</script>
