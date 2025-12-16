<template>
  <v-container>
    <v-card class="pa-8 mb-8">
        <v-row class="mb-4" align="center" justify="start">
            <v-col cols="6" md="3">
                <v-text-field
                v-model="fromDate"
                type="date"
                label="From"
                outlined
                
                />
            </v-col>
            <v-col cols="6" md="3">
                <v-text-field
                v-model="toDate"
                type="date"
                label="To"
                outlined
                
                />
            </v-col>
            <v-col cols="6" md="2">
                <v-text-field
                v-model.number="topK"
                type="number"
                min="1"
                label="Top K Trains"
                outlined
                
                />
            </v-col>
        </v-row>
        <v-row justify="start">
            <v-btn color="primary" class="mt-2" @click="fetchReport">
                Get Report
            </v-btn>
        </v-row>
    </v-card>

    <v-card v-if="report" class="pa-4">
      <v-row>
        <v-col cols="12" md="3">
          <strong>Total Payments:</strong> {{ report.totalPayments }}
        </v-col>
        <v-col cols="12" md="3">
          <strong>Successful Payments:</strong> {{ report.successfulPayments }}
        </v-col>
        <v-col cols="12" md="3">
          <strong>Total Revenue:</strong> {{ report.totalRevenue.toFixed(2) }}
        </v-col>
        <v-col cols="12" md="3">
          <strong>Average Payment:</strong> {{ report.averagePayment.toFixed(2) }}
        </v-col>
      </v-row>

      <v-row class="mt-4">
        <v-col cols="12" md="6">
          <strong>Success %:</strong> {{ report.successPercentage.toFixed(2) }}%
        </v-col>
        <v-col cols="12" md="6">
          <strong>Failure %:</strong> {{ report.failurePercentage.toFixed(2) }}%
        </v-col>
      </v-row>

      <v-divider class="my-4"></v-divider>

      <v-row>
        <v-col cols="12">
          <strong>Top Trains:</strong>
          <v-list two-line>
            <v-list-item
              v-for="train in report.topTrains"
              :key="train.trainId"
            >
              <v-list-item-content>
                <v-list-item-title>Train {{ train.trainId }}</v-list-item-title>
                <v-list-item-subtitle>
                  Payments: {{ train.paymentsCount }}
                </v-list-item-subtitle>
              </v-list-item-content>
            </v-list-item>
          </v-list>
        </v-col>
      </v-row>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import router from '@/router';
import { ref } from 'vue';
import api from '@/api'
import { useAuthStore } from '@/stores/auth'

interface TopTrainDto {
  trainId: number;
  paymentsCount: number;
}

interface PaymentReportDto {
  totalPayments: number;
  successfulPayments: number;
  averagePayment: number;
  totalRevenue: number;
  topTrains: TopTrainDto[];
  successPercentage: number;
  failurePercentage: number;
}

const fromDate = ref('');
const toDate = ref('');
const topK = ref(5);

const report = ref<PaymentReportDto | null>(null);

const auth = useAuthStore();

if (!auth.isAdmin) {
  router.push('/');
}

async function fetchReport() {
  try {
    const response = await api.getReport<PaymentReportDto>({
        from: fromDate.value || null,
        to: toDate.value || null,
        topK: topK.value
    });
    report.value = response;
  } catch (error) {
    console.error(error);
    alert('Failed to fetch report');
  }
}
</script>
