<template>
  <v-container>

    <v-row>
      <v-col cols="12">
        <h2 class="mb-4">Admin dashboard</h2>
      </v-col>
    </v-row>

    <v-row>

      <!-- Stations -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="stations"
          detailsBase="stations"
          title="Stations"
          :headers="['Name', 'City', 'Description']"
          :keys="['name','cityName','description']"
          :defaultModel="{ name:'', cityName:'', description:'' }"
        />
      </v-col>

      <!-- Train Types -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="traintypes"
          detailsBase="traintypes"
          title="Train Types"
          :headers="['Name','Description']"
          :keys="['name','description']"
          :defaultModel="{ name:'', description:'' }"
        />
      </v-col>

      <!-- Perk Groups -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="perkgroups"
          detailsBase="perkgroups"
          title="Perk Groups"
          :headers="['Name','Discount','Fixed Price']"
          :keys="['name','discount','fixedPrice']"
          :defaultModel="{ name:'', description:'', discount:0, fixedPrice:0 }"
        />
      </v-col>

      <!-- Price Policies -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="pricepolicies"
          detailsBase="pricepolicies"
          title="Price Policies"
          :headers="['Per KM','Per Station','Fixed Price']"
          :keys="['pricePerKm','pricePerStation','fixedPrice']"
          :defaultModel="{ pricePerKm:0, pricePerStation:0, fixedPrice:0 }"
        />
      </v-col>

      <!-- Trains -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="trains"
          detailsBase="trains"
          title="Trains"
          :headers="['Source','Destination','Type']"
          :keys="['sourceId','destinationId','typeId']"
          :defaultModel="{ sourceId:null,destinationId:null,typeId:null }"
          :fks="{
            sourceId:      { entity: 'stations', label: s => `${s.name} (${s.cityName})` },
            destinationId: { entity: 'stations', label: s => `${s.name} (${s.cityName})` },
            typeId:        { entity: 'traintypes', label: t => t.name }
          }"
        />
      </v-col>

      <!-- Timetable -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="timetable"
          detailsBase="timetable"
          title="Time Table"
          :headers="['Train','Departure','Arrival']"
          :keys="['trainId','departure','arrival']"
          :defaultModel="{ trainId:null, departure:'', arrival:'' }"
          :fks="{
            trainId: { entity: 'trains', label: t => `Train #${t.id}` }
          }"
          :datetimes="['departure', 'arrival']"
        />
      </v-col>

      <!-- Users -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="users"
          detailsBase="users"
          title="Users"
          :headers="['Name','Phone']"
          :keys="['name','phone']"
          :defaultModel="{ name:'', surname:'', phone:'', password:'', salt:'' }"
        />
      </v-col>

      <!-- Tickets -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="tickets"
          detailsBase="tickets"
          title="Tickets"
          :headers="['Entry','User','Used']"
          :keys="['entryId','userId','used']"
          :defaultModel="{ entryId:null, userId:null, used:false }"
          :fks="{
            entryId: { entity: 'timetable', label: e => `#${e.id}: ${e.departure} → ${e.arrival}` },
            userId:  { entity: 'users', label: u => `${u.name} (${u.phone})` }
          }"
        />
      </v-col>

      <!-- Ticket Locks -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="ticketlocks"
          detailsBase="ticketlocks"
          title="Ticket Locks"
          :headers="['Entry','User','Paid','Sum']"
          :keys="['entryId','userId','paid','sum']"
          :defaultModel="{ entryId:null, userId:null, invoiceId:'', paid:false, sum:0 }"
          :fks="{
            entryId: { entity: 'timetable', label: e => `#${e.id}` },
            userId:  { entity: 'users', label: u => u.phone }
          }"
        />
      </v-col>

      <!-- Payments -->
      <v-col cols="12" md="6">
        <EntityEditor
          entity="payments"
          detailsBase="payments"
          title="Payments"
          :headers="['Lock','Success','Sum']"
          :keys="['lockId','successful','sum']"
          :defaultModel="{ lockId:null, invoiceId:'', successful:false, sum:0 }"
          :fks="{
            lockId: { entity: 'ticketlocks', label: l => `Lock #${l.id}` }
          }"
        />
      </v-col>

    </v-row>

  </v-container>
</template>

<script setup>
import EntityEditor from '@/components/EntityEditor.vue'
</script>
