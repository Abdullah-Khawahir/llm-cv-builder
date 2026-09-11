import { Api } from "@/gen/api";

export const client = new Api({
  baseURL: process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5045",
  securityWorker: () => {
    const token = localStorage.getItem("access_token");
    if (token) {
      return {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      }
    }
    else return {}
  },
});

export const api = client.api;
