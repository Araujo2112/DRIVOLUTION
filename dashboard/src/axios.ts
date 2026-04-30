import axios from "axios"
import router from "./router.ts"

const _axios = axios.create({
    baseURL: import.meta.env.VITE_API_URL
})

_axios.interceptors.request.use(function (config) {
    config.headers.Authorization = localStorage.getItem("texpact_token");

    return config;
})

_axios.interceptors.response.use(function (response) {
    return response
}, function (error) {
    if (error.response.status === 401) {
        localStorage.removeItem("texpact_token")
        localStorage.removeItem("texpact_user_data")
        router.push('/login').then(() => {})
    }
    return Promise.reject(error)
})

export default _axios