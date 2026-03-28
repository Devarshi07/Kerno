import { useState } from 'react'
import { useAuth } from '../../contexts/AuthContext'
import { updateProfile } from '../../api/users'
import Card from '../../components/ui/Card'
import Badge from '../../components/ui/Badge'
import { formatDate } from '../../utils/formatters'
import { User } from 'lucide-react'
import toast from 'react-hot-toast'

export default function ProfilePage() {
  const { user, refreshUser } = useAuth()
  const [firstName, setFirstName] = useState(user?.firstName || '')
  const [lastName, setLastName] = useState(user?.lastName || '')
  const [loading, setLoading] = useState(false)

  const handleSave = async (e) => {
    e.preventDefault()
    setLoading(true)
    try {
      await updateProfile(firstName, lastName)
      await refreshUser()
      toast.success('Profile updated')
    } catch (err) {
      toast.error(err?.error || 'Failed to update')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="max-w-lg mx-auto space-y-6">
      <h2 className="text-2xl font-bold text-gray-900">Profile</h2>

      <Card className="flex items-center gap-4">
        <div className="p-4 bg-indigo-50 rounded-full"><User className="h-8 w-8 text-indigo-600" /></div>
        <div>
          <p className="font-semibold text-lg">{user?.firstName} {user?.lastName}</p>
          <p className="text-sm text-gray-500">{user?.email}</p>
          <Badge className={user?.role === 'Admin' ? 'bg-purple-100 text-purple-800 mt-1' : 'bg-blue-100 text-blue-800 mt-1'}>
            {user?.role}
          </Badge>
        </div>
      </Card>

      <Card>
        <p className="text-xs text-gray-500 mb-1">Member since</p>
        <p className="text-sm">{user?.createdAt ? formatDate(user.createdAt) : 'N/A'}</p>
      </Card>

      <form onSubmit={handleSave}>
        <Card className="space-y-4">
          <h3 className="font-semibold text-gray-700">Edit Profile</h3>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">First Name</label>
              <input value={firstName} onChange={(e) => setFirstName(e.target.value)} required
                className="w-full px-3 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Last Name</label>
              <input value={lastName} onChange={(e) => setLastName(e.target.value)} required
                className="w-full px-3 py-2 border border-gray-300 rounded-lg outline-none focus:ring-2 focus:ring-indigo-500" />
            </div>
          </div>
          <button type="submit" disabled={loading}
            className="bg-indigo-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-indigo-700 disabled:opacity-50">
            {loading ? 'Saving...' : 'Save Changes'}
          </button>
        </Card>
      </form>
    </div>
  )
}
