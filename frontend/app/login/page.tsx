'use client';

import { useState, FormEvent } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useAuth } from '@/lib/contexts/AuthContext';

export default function LoginPage() {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(false);
    const { login } = useAuth();
    const router = useRouter();

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        setError('');
        setLoading(true);
        try {
            await login(username, password);
            router.push('/dashboard');
        } catch (err: unknown) {
            setError(err instanceof Error ? err.message : 'Login failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className='flex min-h-screen items-center justify-center px-4'>
            <div className='w-full max-w-md space-y-8'>
                <div className='text-center'>
                    <h1 className='text-3xl font-bold text-gray-300'>
                        Online Banking UIB
                    </h1>
                    <p className='mt-2 text-gray-200'>
                        Sign in to your account
                    </p>
                </div>

                <form
                    onSubmit={handleSubmit}
                    className='space-y-6 bg-white p-8 rounded-xl shadow-md'>
                    {error && (
                        <div className='bg-red-50 text-red-600 px-4 py-3 rounded-lg text-sm'>
                            {error}
                        </div>
                    )}

                    <div>
                        <label className='block text-sm font-medium text-gray-700 mb-1'>
                            Username
                        </label>
                        <input
                            type='text'
                            value={username}
                            onChange={(e) => setUsername(e.target.value)}
                            required
                            className='w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-black'
                        />
                    </div>

                    <div>
                        <label className='block text-sm font-medium text-gray-700 mb-1'>
                            Password
                        </label>
                        <input
                            type='password'
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                            className='w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 outline-none text-black'
                        />
                    </div>

                    <button
                        type='submit'
                        disabled={loading}
                        className='w-full bg-blue-600 text-white py-2 px-4 rounded-lg hover:bg-blue-700 disabled:opacity-50 font-medium'>
                        {loading ? 'Signing in...' : 'Sign in'}
                    </button>

                    <p className='text-center text-sm text-gray-600'>
                        Don&apos;t have an account?{' '}
                        <Link
                            href='/register'
                            className='text-blue-600 hover:underline font-medium'>
                            Register
                        </Link>
                    </p>
                </form>
            </div>
        </div>
    );
}
