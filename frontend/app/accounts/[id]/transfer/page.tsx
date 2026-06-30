"use client";

import { useState, FormEvent } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/contexts/AuthContext";
import { accountsApi } from "@/lib/api";

export default function TransferPage() {
  const { id } = useParams<{ id: string }>();
  const { user, loading: authLoading } = useAuth();
  const router = useRouter();
  const accountId = parseInt(id);
  const [targetAccount, setTargetAccount] = useState("");
  const [amount, setAmount] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [success, setSuccess] = useState(false);

  if (authLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="animate-spin h-8 w-8 border-4 border-blue-600 border-t-transparent rounded-full" />
      </div>
    );
  }

  if (!user) {
    router.push("/login");
    return null;
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    setSuccess(false);
    try {
      await accountsApi.transfer(accountId, targetAccount, parseFloat(amount), description || undefined);
      setSuccess(true);
      setTimeout(() => router.push(`/accounts/${accountId}`), 1500);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Transfer failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white shadow-sm">
        <div className="max-w-5xl mx-auto px-4 py-4 flex items-center justify-between">
          <Link href="/dashboard" className="text-xl font-bold text-gray-900 hover:text-blue-600">
            Online Banking
          </Link>
          <span className="text-sm text-gray-600">{user.username}</span>
        </div>
      </header>

      <main className="max-w-2xl mx-auto px-4 py-8">
        <Link href={`/accounts/${accountId}`} className="text-sm text-blue-600 hover:underline">
          &larr; Back to Account
        </Link>

        <div className="mt-6 bg-white p-8 rounded-xl shadow-md">
          <h1 className="text-xl font-semibold text-gray-900 mb-6">Transfer Funds</h1>

          {success && (
            <div className="bg-green-50 text-green-600 px-4 py-3 rounded-lg text-sm mb-6">
              Transfer successful! Redirecting...
            </div>
          )}

          {error && (
            <div className="bg-red-50 text-red-600 px-4 py-3 rounded-lg text-sm mb-6">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-6">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Target Account Number
              </label>
              <input
                type="text"
                value={targetAccount}
                onChange={(e) => setTargetAccount(e.target.value)}
                required
                placeholder="Enter the 10-digit account number"
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Amount
              </label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                required
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
              />
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Description (optional)
              </label>
              <input
                type="text"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
              />
            </div>

            <button
              type="submit"
              disabled={loading || success}
              className="w-full bg-purple-600 text-white py-2 px-4 rounded-lg hover:bg-purple-700 disabled:opacity-50 font-medium"
            >
              {loading ? "Processing..." : "Send Transfer"}
            </button>
          </form>
        </div>
      </main>
    </div>
  );
}
