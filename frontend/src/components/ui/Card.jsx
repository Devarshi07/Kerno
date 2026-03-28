import { cn } from '../../utils/cn'

export default function Card({ children, className, ...props }) {
  return (
    <div className={cn('bg-white rounded-lg border border-gray-200 shadow-sm p-6', className)} {...props}>
      {children}
    </div>
  )
}
