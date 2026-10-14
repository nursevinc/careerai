interface LogoProps {
  variant?: 'dark' | 'light' // light: koyu/renkli zemin üstünde
}

export default function Logo({ variant = 'dark' }: LogoProps) {
  const isLight = variant === 'light'

  return (
    <span className="inline-flex items-center gap-2">
      <svg viewBox="0 0 32 32" className="h-8 w-8" aria-hidden="true">
        <rect width="32" height="32" rx="8" fill={isLight ? '#ffffff' : '#4f46e5'} />
        <path
          d="M9 16.5l4.5 4.5L23 11.5"
          fill="none"
          stroke={isLight ? '#4f46e5' : '#ffffff'}
          strokeWidth="3"
          strokeLinecap="round"
          strokeLinejoin="round"
        />
      </svg>
      <span className={`text-lg font-semibold tracking-tight ${isLight ? 'text-white' : 'text-slate-900'}`}>
        Career<span className={isLight ? 'text-indigo-200' : 'text-indigo-600'}>AI</span>
      </span>
    </span>
  )
}