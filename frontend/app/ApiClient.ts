import { Api } from "@/gen/api";

const client = new Api({
  baseUrl: "http://localhost:5044",
});

export const api = client.api;
