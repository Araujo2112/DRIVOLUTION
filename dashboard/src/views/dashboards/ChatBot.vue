<script setup lang="ts">
import { ref, nextTick } from 'vue'

const userInput = ref('')
const chatHistory = ref([
  { role: 'system', content: 'Olá! Sou o assistente do sistema TexP@ct.' }
])
const loading = ref(false)
const chatBox = ref<HTMLElement | null>(null)

async function sendMessage() {
  const text = userInput.value.trim()
  if (!text) return

  chatHistory.value.push({ role: 'user', content: text })
  userInput.value = ''
  loading.value = true

  await nextTick(scrollToBottom)

  try {

    const filteredHistory = chatHistory.value.filter(m => m.role !== 'system')

    if (filteredHistory.length === 0 || filteredHistory[filteredHistory.length - 1].role !== 'user') {
      chatHistory.value.push({ role: 'assistant', content: 'Erro: a última mensagem deve ser do utilizador.' })
      loading.value = false
      return
    }

    const res = await fetch('http://localhost:5181/api/chat', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ messages: filteredHistory })
    })
    if (!res.ok) {
      const errText = await res.text()
      chatHistory.value.push({ role: 'assistant', content: `Erro do servidor: ${errText}` })
    } else {
      const data = await res.json()
      chatHistory.value.push({ role: 'assistant', content: data.response })
    }
  } catch (error) {
    chatHistory.value.push({ role: 'assistant', content: 'Erro ao contactar o servidor.' })
  } finally {
    loading.value = false
    await nextTick(scrollToBottom)
  }
}



function scrollToBottom() {
  if (chatBox.value) {
    chatBox.value.scrollTop = chatBox.value.scrollHeight
  }
}
</script>

<template>
  <div class="flex flex-col h-screen bg-white">
    <header class="bg-primary-500 shadow p-4 text-xl font-semibold text-white">
      Assistente TexP@ct
    </header>

    <section class="bg-gray-50 px-6 py-4 border-b border-gray-200">
      <p class="font-medium mb-2">Perguntas Rápidas:</p>
      <div class="flex flex-wrap gap-2">

        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Nif deste cliente João Almeida?'">
          NIF do cliente João Silva
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Listar clientes'">
          Listar clientes
        </button>

        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Informação do contentor Alfa'">
          Info do contentor Alfa
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Listar contentores'">
          Listar contentores
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Quais os contentores que passaram pela secção 3?'">
          Contentores na secção 3
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Por onde passou o contentor 12?'">
          Percurso do contentor 12
        </button>

        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Listar matérias-primas'">
          Listar matérias-primas
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Detalhes da matéria-prima Cimento'">
          Detalhes de “Cimento”
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Matérias-primas da ordem 5'">
          Matérias-primas da ordem 5
        </button>

        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Listar produtos'">
          Listar produtos
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Detalhes do produto X1'">
          Detalhes do produto X1
        </button>

        <!-- Secções -->
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Listar secções'">
          Listar secções
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Detalhes da secção Receção'">
          Detalhes da secção Receção
        </button>

        <!-- Checkpoints -->
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Listar checkpoints'">
          Listar checkpoints
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Detalhes do checkpoint 4'">
          Detalhes do checkpoint 4
        </button>

        <!-- Ordens de fabrico -->
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Listar ordens de fabrico'">
          Listar ordens de fabrico
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Fases da ordem 3'">
          Fases da ordem 3
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Histórico da ordem 3'">
          Histórico da ordem 3
        </button>
        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Treinar modelo'">
          Treinar modelo
        </button>

        <button
            class="px-4 py-1 border border-primary-500 rounded-full text-sm text-primary-600
             hover:bg-primary-500 hover:text-white transition"
            @click="userInput = 'Quanto tempo demora a produção do produto Polo para o cliente Maria Rodrigues na fase de Tingimento e lavagem com um lote de 300 unidades na segunda-feira às 14h?'">
          Pergunta previsão
        </button>

      </div>
    </section>

    <main
        ref="chatBox"
        class="flex-1 overflow-auto p-6 space-y-4 bg-white"
        aria-live="polite"
    >
      <div
          v-for="(msg, idx) in chatHistory"
          :key="idx"
          :class="[
    'max-w-3/4 px-4 py-3 rounded-lg break-words whitespace-pre-wrap',
    msg.role === 'user' ? 'ml-auto bg-primary-600 text-white text-right' : '',
    msg.role === 'assistant' ? 'mr-auto bg-background-900 text-gray-900' : '',
    msg.role === 'system' ? 'hidden' : ''
  ]"
      >
        {{ msg.content }}
      </div>


      <div v-if="loading" class="text-gray-500 italic text-center">A escrever<span class="animate-pulse">...</span></div>
    </main>

    <footer class="bg-white p-4 flex gap-2 border-t border-gray-200">
      <textarea
          v-model="userInput"
          rows="2"
          class="flex-1 border rounded-lg p-2 resize-none focus:outline-none focus:ring-2 focus:ring-primary-500"
          placeholder="Escreve uma pergunta..."
          :disabled="loading"
          @keyup.enter.exact.prevent="sendMessage"
      ></textarea>
      <button
          class="bg-primary-500 hover:bg-primary-700 text-white px-5 py-2 rounded-lg transition disabled:opacity-50"
          @click="sendMessage"
          :disabled="loading"
      >
        Enviar
      </button>
    </footer>
  </div>
</template>
