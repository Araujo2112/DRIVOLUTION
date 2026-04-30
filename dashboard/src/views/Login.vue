<script setup lang="ts">

import Button from "@/components/Button.vue"
import {ref} from "vue"
import axios from "@/axios.ts"
import router from "@/router.ts"

const isLoading = ref(false)

const username = ref("")
const password = ref("")

async function login() {
  isLoading.value = true

  try {
    const res = await axios.post(import.meta.env.VITE_API_URL + "/Employee/authenticate", {
      username: username.value,
      password: password.value
    })

    localStorage.setItem("texpact_token", `Bearer ${res.data.token}`)
    localStorage.setItem("texpact_user_data", JSON.stringify(res.data.employee))

    await router.push("/dashboard")
  } catch (e) {

    console.error(e)
  }

  isLoading.value = false
}

</script>

<template>
  <div class="flex w-full h-full justify-center items-center">
    <div class="w-96 py-2 px-4">

      <div class="h-full rounded-2xl flex flex-col bg-background-900 p-4">
        <div class="flex w-full px-4 py-6 object-center object-contain  items-center overflow-hidden gap-0">
          <img src="@/assets/icons/texpact-logo.png" alt="Eco build icon"/>
        </div>

        <div class="flex flex-col items-center gap-2">
          <input type="text" placeholder="Utilizador" v-model="username">
          <input type="password" placeholder="Palavra-passe" v-model="password">
          <Button @click="login()">Entrar</Button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>

</style>