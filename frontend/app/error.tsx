"use client";

export default function Error({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="flex h-screen flex-col items-center justify-center gap-4">
      <h1 className="text-2xl font-bold text-gray-900">Something went wrong</h1>
      <p className="text-gray-500">{error.message}</p>
      <button
        onClick={() => reset()}
        className="rounded-lg bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
      >
        Try again
      </button>
    </div>
  );
}
