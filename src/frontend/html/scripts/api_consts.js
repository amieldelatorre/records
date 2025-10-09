import { API_HOST } from "./env.js";

export const JWT_URL = new URL("/api/v1/auth/jwt", API_HOST);