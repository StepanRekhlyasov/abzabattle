import axios from "axios"
import { toast } from "vuetify-sonner";
import { Errors } from "./errors";

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
            toast.error(detail ?? Errors[error.response?.status as unknown as keyof typeof Errors])
        }
    }
    return Promise.reject(error)
})

export default api