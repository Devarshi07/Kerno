export default function Spinner({ className = 'h-8 w-8' }) {
  return (
    <div className="flex items-center justify-center p-8">
      <div className={`animate-spin border-4 border-indigo-600 border-t-transparent rounded-full ${className}`} />
    </div>
  )
}
