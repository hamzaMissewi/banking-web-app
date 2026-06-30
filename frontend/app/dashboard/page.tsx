'use client';

import { useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '@/lib/contexts/AuthContext';
import { accountsApi, AccountResponse } from '@/lib/api';

export default function DashboardPage() {
    const { user, logout, loading: authLoading } = useAuth();
    const router = useRouter();
    const [accounts, setAccounts] = useState<AccountResponse[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        if (!authLoading && !user) {
            router.push('/login');
            return;
        }
        if (user) {
            accountsApi
                .getAll()
                .then(setAccounts)
                .catch(console.error)
                .finally(() => setLoading(false));
        }
    }, [user, authLoading, router]);

    if (authLoading || loading) {
        return (
            <div className='flex h-screen items-center justify-center'>
                <div className='animate-spin h-8 w-8 border-4 border-blue-600 border-t-transparent rounded-full' />
            </div>
        );
    }

    const totalBalance = accounts.reduce((sum, a) => sum + a.balance, 0);

    return (
        <div className='min-h-screen bg-gray-50'>
            <header className='bg-white shadow-sm'>
                <div className='max-w-5xl mx-auto px-4 py-4 flex items-center justify-between'>
                    <h1 className='text-xl font-bold text-gray-900'>
                        Online Banking UIB
                    </h1>
                    <div className='flex items-center gap-4'>
                        <span className='text-sm text-gray-600'>
                            Welcome, {user?.username}
                        </span>
                        <button
                            onClick={() => {
                                logout();
                                router.push('/login');
                            }}
                            className='text-sm text-red-600 hover:underline'>
                            Sign out
                        </button>
                    </div>
                </div>
            </header>

            <main className='max-w-5xl mx-auto px-4 py-8 space-y-8'>
                <div className='bg-white p-6 rounded-xl shadow-md'>
                    <p className='text-sm text-gray-500'>Total Balance</p>
                    <p className='text-3xl font-bold text-gray-900'>
                        $
                        {totalBalance.toLocaleString('en-US', {
                            minimumFractionDigits: 2,
                        })}
                    </p>
                </div>

                <div className='flex items-center justify-between'>
                    <h2 className='text-lg font-semibold text-gray-900'>
                        Your Accounts
                    </h2>
                    <Link
                        href='/accounts'
                        className='bg-blue-600 text-white px-4 py-2 rounded-lg text-sm hover:bg-blue-700'>
                        Manage Accounts
                    </Link>
                </div>

                {accounts.length === 0 ? (
                    <div className='bg-white p-6 rounded-xl shadow-md text-center'>
                        <p className='text-gray-500'>No accounts yet.</p>
                        <Link
                            href='/accounts'
                            className='inline-block mt-3 text-blue-600 hover:underline'>
                            Create your first account
                        </Link>
                    </div>
                ) : (
                    <div className='grid gap-4 sm:grid-cols-2'>
                        {accounts.map((account) => (
                            <Link
                                key={account.id}
                                href={`/accounts/${account.id}`}
                                className='bg-white p-5 rounded-xl shadow-md hover:shadow-lg transition-shadow'>
                                <div className='flex items-center justify-between mb-2'>
                                    <span className='text-sm font-medium text-gray-500'>
                                        {account.accountType}
                                    </span>
                                    <span className='text-xs font-mono text-gray-400'>
                                        ****{account.accountNumber.slice(-4)}
                                    </span>
                                </div>
                                <p className='text-2xl font-bold text-gray-900'>
                                    $
                                    {account.balance.toLocaleString('en-US', {
                                        minimumFractionDigits: 2,
                                    })}
                                </p>
                            </Link>
                        ))}
                    </div>
                )}
            </main>
        </div>
    );
}
