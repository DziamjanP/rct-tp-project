<template>
  <v-container>
    <v-row justify="center">
      <v-col cols="12" md="6">
        <v-card class="pa-4">
          <div class="text-h5 mb-4">Login (dev only)</div>

          <v-text-field v-model="phone" label="Phone" />
          <v-text-field v-model="password" type="password" label="Password" />

          <v-row class="mt-4">
            <v-col>
              <v-btn color="primary" @click="submit">Login</v-btn>
              <v-btn text variant="plain" to="/register">Register</v-btn>
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
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const phone = ref('')
const password = ref('')
const message = ref('')
const error = ref('')
const auth = useAuthStore()
const router = useRouter()

async function submit() {
  try {
    error.value = ''
    await auth.login({ phone: phone.value, password: password.value })
    message.value = 'Logged in, redirecting you now...'
    router.push('/')
  } catch (e) {
    message.value = ''
    error.value = e.message ?? String(e)
  }
}
</script>
