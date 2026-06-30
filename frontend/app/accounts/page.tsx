"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/contexts/AuthContext";
import { accountsApi, AccountResponse } from "@/lib/api";

export default function AccountsPage() {
  const { user, loading: authLoading } = useAuth();
  const router = useRouter();
  const [accounts, setAccounts] = useState<AccountResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showCreate, setShowCreate] = useState(false);
  const [accountType, setAccountType] = useState("Checking");
  const [creating, setCreating] = useState(false);

  const fetchAccounts = () => {
    accountsApi.getAll().then(setAccounts).catch(console.error).finally(() => setLoading(false));
  };

  useEffect(() => {
    if (!authLoading && !user) router.push("/login");
    if (user) fetchAccounts();
  }, [user, authLoading, router]);

  const handleCreate = async () => {
    setCreating(true);
    try {
      await accountsApi.create(accountType);
      setShowCreate(false);
      fetchAccounts();
    } catch (err) {
      alert(err instanceof Error ? err.message : "Failed to create account");
    } finally {
      setCreating(false);
    }
  };

  if (authLoading || loading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="animate-spin h-8 w-8 border-4 border-blue-600 border-t-transparent rounded-full" />
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white shadow-sm">
        <div className="max-w-5xl mx-auto px-4 py-4 flex items-center justify-between">
          <Link href="/dashboard" className="text-xl font-bold text-gray-900 hover:text-blue-600">
            Online Banking
          </Link>
          <span className="text-sm text-gray-600">{user?.username}</span>
        </div>
      </header>

      <main className="max-w-5xl mx-auto px-4 py-8 space-y-6">
        <div className="flex items-center justify-between">
          <h1 className="text-xl font-semibold text-gray-900">Your Accounts</h1>
          <button
            onClick={() => setShowCreate(true)}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700"
          >
            + New Account
          </button>
        </div>

        {showCreate && (
          <div className="bg-white p-6 rounded-xl shadow-md space-y-4">
            <h2 className="font-semibold text-gray-900">Create New Account</h2>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Account Type
              </label>
              <select
                value={accountType}
                onChange={(e) => setAccountType(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
              >
                <option>Checking</option>
                <option>Savings</option>
                <option>Business</option>
              </select>
            </div>
            <div className="flex gap-3">
              <button
                onClick={handleCreate}
                disabled={creating}
                className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {creating ? "Creating..." : "Create"}
              </button>
              <button
                onClick={() => setShowCreate(false)}
                className="text-gray-600 px-4 py-2 rounded-lg text-sm hover:bg-gray-100"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        {accounts.length === 0 ? (
          <div className="bg-white p-6 rounded-xl shadow-md text-center">
            <p className="text-gray-500">No accounts yet. Create one to get started.</p>
          </div>
        ) : (
          <div className="space-y-4">
            {accounts.map((account) => (
              <Link
                key={account.id}
                href={`/accounts/${account.id}`}
                className="block bg-white p-5 rounded-xl shadow-md hover:shadow-lg transition-shadow"
              >
                <div className="flex items-center justify-between">
                  <div>
                    <p className="font-semibold text-gray-900">{account.accountType}</p>
                    <p className="text-sm text-gray-500 font-mono">
                      {account.accountNumber}
                    </p>
                  </div>
                  <p className="text-xl font-bold text-gray-900">
                    ${account.balance.toLocaleString("en-US", { minimumFractionDigits: 2 })}
                  </p>
                </div>
                <p className="text-xs text-gray-400 mt-2">
                  Opened {new Date(account.createdAt).toLocaleDateString()}
                </p>
              </Link>
            ))}
          </div>
        )}
      </main>
    </div>
  );
}
