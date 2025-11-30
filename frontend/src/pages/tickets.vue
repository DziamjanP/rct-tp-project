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

                <v-list-item>
                  <v-list-item-title>Email</v-list-item-title>
                  <v-list-item-subtitle> email will go here... </v-list-item-subtitle>
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
              <v-tab value="locks">Locks</v-tab>
              <v-tab value="archive">Ticket Archive</v-tab>
              <v-tab value="payments">Payment History</v-tab>
            </v-tabs>

            <v-divider></v-divider>

            <!-- Tab Content -->
            <v-card-text v-if="activeTab === 'tickets'">
              <h3>Tickets</h3>
              <p>List of tickets will go here.</p>
            </v-card-text>

            <v-card-text v-else-if="activeTab === 'locks'">
              <h3>Locks</h3>
              <p>List of locks will go here.</p>
            </v-card-text>

            <v-card-text v-else-if="activeTab === 'archive'">
              <h3>Ticket Archive</h3>
              <p>Archived tickets content here.</p>
            </v-card-text>

            <v-card-text v-else-if="activeTab === 'payments'">
              <h3>Payment History</h3>
              <p>Payment history content here.</p>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
    </v-container>
  </v-app>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import router from '@/router'

const auth = useAuthStore()

const user = computed(() => auth.user)
const isAdmin = computed(() => auth.isAdmin)

const activeTab = ref('tickets')

function logout() {
  auth.logout()
  router.push('/')
}
</script>

<style scoped>
.profile-page,
.fill-height {
  height: 80%;
  overflow: hidden;
}
</style>
