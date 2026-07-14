import axios from 'axios'
import router from './router'

// Cria uma instância personalizada do Axios
const _axios = axios.create({
  // Define o endereço base da API através de uma variável de ambiente
  baseURL: import.meta.env.VITE_API_URL
})

// Interceta todos os pedidos antes de serem enviados
_axios.interceptors.request.use(config => {
  // Procura o token JWT guardado no navegador
  const token = localStorage.getItem('drivolution_token')

  // Se existir token, adiciona-o ao cabeçalho Authorization
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  // Devolve a configuração para o pedido continuar
  return config
})

// Interceta todas as respostas recebidas da API
_axios.interceptors.response.use(
  // Se a resposta correr bem, devolve-a sem alterações
  response => response,

  // Se ocorrer um erro, executa esta função
  error => {
    // Se a API devolver 401, o utilizador já não está autenticado
    if (error.response?.status === 401) {
      // Remove o token guardado
      localStorage.removeItem('drivolution_token')

      // Remove os dados do utilizador guardados
      localStorage.removeItem('drivolution_user')

      // Redireciona o utilizador para o login
      router.push('/login')
    }

    // O backend recusou o pedido porque a conta ainda tem password temporária
    // (ver MustChangePasswordMiddleware). Garante que o frontend não fica "preso"
    // a tentar chamadas que vão continuar a falhar e leva o user para o sítio certo.
    if (
      error.response?.status === 403 &&
      error.response?.data?.code === 'PASSWORD_CHANGE_REQUIRED'
    ) {
      // Obtém os dados do utilizador guardados no navegador
      const userStr = localStorage.getItem('drivolution_user')

      try {
        // Converte o texto JSON novamente num objeto
        const user = JSON.parse(userStr ?? 'null')

        // Se o utilizador existir, atualiza o estado local
        if (user) {
          // Indica que o utilizador tem de alterar a password
          user.mustChangePassword = true

          // Guarda novamente o objeto atualizado
          localStorage.setItem(
            'drivolution_user',
            JSON.stringify(user)
          )
        }
      } catch {
        // ignora; pior caso, o router vai depender só do token
      }

      // Evita redirecionar novamente caso o utilizador
      // já esteja na página de alteração da password
      if (
        router.currentRoute.value.name !== 'ChangePassword'
      ) {
        // Redireciona para a página de alteração da password
        router.push('/change-password')
      }
    }

    // Mantém o erro disponível para o componente ou service
    // que fez o pedido também o poder tratar
    return Promise.reject(error)
  }
)

// Exporta esta instância personalizada
// para ser usada nos services do frontend
export default _axios