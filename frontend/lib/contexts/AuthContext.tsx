"use client";

import { createContext, useContext, useState, ReactNode } from "react";
import { authApi, AuthResponse } from "@/lib/api";

interface AuthContextType {
  user: AuthResponse | null;
  token: string | null;
  login: (username: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | null>(null);

function getStoredUser(): AuthResponse | null {
  if (typeof window === "undefined") return null;
  try {
    const data = localStorage.getItem("user");
    return data ? JSON.parse(data) : null;
  } catch {
    return null;
  }
}

function getStoredToken(): string | null {
  if (typeof window === "undefined") return null;
  return localStorage.getItem("token");
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthResponse | null>(getStoredUser);
  const [token, setToken] = useState<string | null>(getStoredToken);
  const [loading] = useState(false);

  const login = async (username: string, password: string) => {
    const res = await authApi.login(username, password);
    localStorage.setItem("token", res.token);
    localStorage.setItem("user", JSON.stringify(res));
    setToken(res.token);
    setUser(res);
  };

  const register = async (username: string, email: string, password: string) => {
    const res = await authApi.register(username, email, password);
    localStorage.setItem("token", res.token);
    localStorage.setItem("user", JSON.stringify(res));
    setToken(res.token);
    setUser(res);
  };

  const logout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");
    setToken(null);
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, token, login, register, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be used within AuthProvider");
  return context;
}
