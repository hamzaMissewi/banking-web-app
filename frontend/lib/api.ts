const API_BASE = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5070/api";

interface ApiError {
  message: string;
}

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const token =
    typeof window !== "undefined" ? localStorage.getItem("token") : null;

  const headers: Record<string, string> = {
    "Content-Type": "application/json",
    ...((options.headers as Record<string, string>) || {}),
  };

  if (token) headers["Authorization"] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}${endpoint}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    const error: ApiError = await res.json();
    throw new Error(error.message || "An error occurred");
  }

  return res.json();
}

export interface AuthResponse {
  token: string;
  userId: number;
  username: string;
  email: string;
}

export interface AccountResponse {
  id: number;
  accountNumber: string;
  accountType: string;
  balance: number;
  createdAt: string;
}

export interface TransactionResponse {
  id: number;
  type: "Deposit" | "Withdrawal" | "Transfer";
  amount: number;
  balanceBefore: number;
  balanceAfter: number;
  description: string;
  targetAccountId: number | null;
  createdAt: string;
}

export const authApi = {
  login: (username: string, password: string) =>
    request<AuthResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify({ username, password }),
    }),

  register: (username: string, email: string, password: string) =>
    request<AuthResponse>("/auth/register", {
      method: "POST",
      body: JSON.stringify({ username, email, password }),
    }),
};

export const accountsApi = {
  getAll: () => request<AccountResponse[]>("/accounts"),

  create: (accountType: string = "Checking") =>
    request<AccountResponse>("/accounts", {
      method: "POST",
      body: JSON.stringify({ accountType }),
    }),

  get: (id: number) => request<AccountResponse>(`/accounts/${id}`),

  deposit: (id: number, amount: number, description?: string) =>
    request<TransactionResponse>(`/accounts/${id}/deposit`, {
      method: "POST",
      body: JSON.stringify({ amount, description: description || "Deposit" }),
    }),

  withdraw: (id: number, amount: number, description?: string) =>
    request<TransactionResponse>(`/accounts/${id}/withdraw`, {
      method: "POST",
      body: JSON.stringify({ amount, description: description || "Withdrawal" }),
    }),

  transfer: (
    id: number,
    targetAccountNumber: string,
    amount: number,
    description?: string
  ) =>
    request<TransactionResponse>(`/accounts/${id}/transfer`, {
      method: "POST",
      body: JSON.stringify({
        targetAccountNumber,
        amount,
        description: description || "Transfer",
      }),
    }),

  getTransactions: (id: number) =>
    request<TransactionResponse[]>(`/accounts/${id}/transactions`),
};
