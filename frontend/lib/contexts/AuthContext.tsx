"use client";

import { createContext, useContext, useState, useEffect, ReactNode } from "react";
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

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthResponse | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem("token");
    const userData = localStorage.getItem("user");
    if (stored && userData) {
      setToken(stored);
      setUser(JSON.parse(userData));
    }
    setLoading(false);
  }, []);

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
