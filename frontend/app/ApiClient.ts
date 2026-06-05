import { Api } from "@/gen/api";

export const client = new Api({
  baseURL: "http://localhost:5044",
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
