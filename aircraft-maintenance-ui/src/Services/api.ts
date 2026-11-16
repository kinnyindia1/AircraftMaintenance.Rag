import axios from 'axios';

// Your API's base URL (check your Visual Studio launchSettings.json for the exact port)
const API_BASE_URL = 'https://localhost:7084/api';

const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});

// Add the JWT token to every request if it exists
apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('authToken');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

export interface LoginResponse {
    token: string;
}

export interface ChatResponse {
    answer: string;
    role: string;
}

export const login = async (username: string, password: string): Promise<LoginResponse> => {
    const response = await apiClient.post<LoginResponse>('/Auth/login', {
        username,
        password,
    });
    return response.data;
};

export const askQuestion = async (question: string): Promise<ChatResponse> => {
    const response = await apiClient.post<ChatResponse>('/Chat/ask', {
        question,
    });
    return response.data;
};

export const logout = () => {
    localStorage.removeItem('authToken');
};

export const isAuthenticated = (): boolean => {
    return !!localStorage.getItem('authToken');
};