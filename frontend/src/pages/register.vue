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

          <v-alert v-if="error" type="error" class="mt-4">{{ error}}</v-alert>
          <v-alert v-if="message" type="info" class="mt-4">{{ message }}</v-alert>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { useRouter } from 'vue-router'

const auth = useAuthStore();

const name = ref('')
const surname = ref('')
const phone = ref('')
const password = ref('')
const message = ref('')
const error = ref('')
const router = useRouter()

async function submit() {
  try {
    error.value = ''
    const payload = { name: name.value, surname: surname.value, phone: phone.value, password: password.value }
    await auth.register(payload)
    message.value = 'Registered, redirecting you now...'
    router.push('/');
  } catch (e) {
    message.value = ''
    error.value = `Error: ${e.message ?? e}`
  }
}
</script>
