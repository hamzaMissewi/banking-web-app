"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import Link from "next/link";
import { useAuth } from "@/lib/contexts/AuthContext";
import { accountsApi, AccountResponse, TransactionResponse } from "@/lib/api";

export default function AccountDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { user, loading: authLoading } = useAuth();
  const router = useRouter();
  const accountId = parseInt(id);
  const [account, setAccount] = useState<AccountResponse | null>(null);
  const [transactions, setTransactions] = useState<TransactionResponse[]>([]);
  const [loading, setLoading] = useState(true);
  const [showDeposit, setShowDeposit] = useState(false);
  const [showWithdraw, setShowWithdraw] = useState(false);
  const [amount, setAmount] = useState("");
  const [desc, setDesc] = useState("");
  const [actionLoading, setActionLoading] = useState(false);

  const fetchData = async () => {
    try {
      const [acc, txs] = await Promise.all([
        accountsApi.get(accountId),
        accountsApi.getTransactions(accountId),
      ]);
      setAccount(acc);
      setTransactions(txs);
    } catch {
      router.push("/accounts");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!authLoading && !user) router.push("/login");
    if (user && !isNaN(accountId)) fetchData();
  }, [user, authLoading, accountId, router]);

  const handleDeposit = async () => {
    setActionLoading(true);
    try {
      await accountsApi.deposit(accountId, parseFloat(amount), desc || undefined);
      setShowDeposit(false);
      setAmount("");
      setDesc("");
      fetchData();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Deposit failed");
    } finally {
      setActionLoading(false);
    }
  };

  const handleWithdraw = async () => {
    setActionLoading(true);
    try {
      await accountsApi.withdraw(accountId, parseFloat(amount), desc || undefined);
      setShowWithdraw(false);
      setAmount("");
      setDesc("");
      fetchData();
    } catch (err: unknown) {
      alert(err instanceof Error ? err.message : "Withdrawal failed");
    } finally {
      setActionLoading(false);
    }
  };

  if (authLoading || loading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="animate-spin h-8 w-8 border-4 border-blue-600 border-t-transparent rounded-full" />
      </div>
    );
  }

  if (!account) return null;

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
        <Link href="/accounts" className="text-sm text-blue-600 hover:underline">&larr; All Accounts</Link>

        <div className="bg-white p-6 rounded-xl shadow-md">
          <div className="flex items-start justify-between">
            <div>
              <p className="text-sm text-gray-500">{account.accountType}</p>
              <p className="text-sm font-mono text-gray-400">{account.accountNumber}</p>
            </div>
            <p className="text-3xl font-bold text-gray-900">
              ${account.balance.toLocaleString("en-US", { minimumFractionDigits: 2 })}
            </p>
          </div>
          <div className="flex gap-3 mt-6">
            <button
              onClick={() => { setShowWithdraw(false); setShowDeposit(true); }}
              className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-green-700"
            >
              + Deposit
            </button>
            <button
              onClick={() => { setShowDeposit(false); setShowWithdraw(true); }}
              className="bg-orange-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-orange-700"
            >
              - Withdraw
            </button>
            <Link
              href={`/accounts/${accountId}/transfer`}
              className="bg-purple-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-purple-700"
            >
              Transfer
            </Link>
          </div>
        </div>

        {(showDeposit || showWithdraw) && (
          <div className="bg-white p-6 rounded-xl shadow-md space-y-4">
            <h2 className="font-semibold text-gray-900">
              {showDeposit ? "Make a Deposit" : "Make a Withdrawal"}
            </h2>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Amount</label>
              <input
                type="number"
                step="0.01"
                min="0.01"
                value={amount}
                onChange={(e) => setAmount(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Description (optional)</label>
              <input
                type="text"
                value={desc}
                onChange={(e) => setDesc(e.target.value)}
                className="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 outline-none"
              />
            </div>
            <div className="flex gap-3">
              <button
                onClick={showDeposit ? handleDeposit : handleWithdraw}
                disabled={actionLoading || !amount}
                className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700 disabled:opacity-50"
              >
                {actionLoading ? "Processing..." : "Submit"}
              </button>
              <button
                onClick={() => { setShowDeposit(false); setShowWithdraw(false); }}
                className="text-gray-600 px-4 py-2 rounded-lg text-sm hover:bg-gray-100"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        <div className="bg-white rounded-xl shadow-md">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="font-semibold text-gray-900">Transaction History</h2>
          </div>
          {transactions.length === 0 ? (
            <div className="p-6 text-center text-gray-500">No transactions yet.</div>
          ) : (
            <div className="divide-y divide-gray-100">
              {transactions.map((tx) => (
                <div key={tx.id} className="px-6 py-4 flex items-center justify-between">
                  <div className="flex items-center gap-3">
                    <span className={`text-xs font-bold uppercase ${
                      tx.type === "Deposit" ? "text-green-600" :
                      tx.type === "Withdrawal" ? "text-red-600" : "text-purple-600"
                    }`}>
                      {tx.type}
                    </span>
                    <div>
                      <p className="text-sm text-gray-900">{tx.description}</p>
                      <p className="text-xs text-gray-400">
                        {new Date(tx.createdAt).toLocaleString()}
                      </p>
                    </div>
                  </div>
                  <div className="text-right">
                    <p className={`text-sm font-semibold ${
                      tx.type === "Deposit" ? "text-green-600" : "text-red-600"
                    }`}>
                      {tx.type === "Deposit" ? "+" : "-"}${tx.amount.toFixed(2)}
                    </p>
                    <p className="text-xs text-gray-400">
                      Balance: ${tx.balanceAfter.toFixed(2)}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>
    </div>
  );
}
