import axios from "axios"
import { toast } from "vuetify-sonner";

const api = axios.create({
    baseURL: '/api',
    headers: {
        "Content-type": "application/json",
    },
})

api.interceptors.response.use(response => {
    return response
},
error => {
    if (axios.isAxiosError(error)) {
        const data = error.response?.data;
        const detail = data?.detail;
        const errors = data?.errors;
        if(errors) {
            Object.values(errors).forEach((error: any) => {
                toast.error(error)
            })
        } else {
            toast.error(detail ?? error.response?.statusText)
        }
    }
    return Promise.reject(error)
})

export default api