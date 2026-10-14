import { Bar, BarChart, CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'

const PRIMARY = '#6366f1' // indigo-500
const AXIS_FONT_SIZE = 12

interface LabeledCount {
  label: string
  count: number
}

// Uyum skoru dağılımı: kaç ilan hangi skor aralığında
export function ScoreDistributionChart({ data, seriesName }: { data: LabeledCount[]; seriesName: string }) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <BarChart data={data} margin={{ top: 8, right: 8, left: -16, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" vertical={false} />
        <XAxis dataKey="label" tickLine={false} fontSize={AXIS_FONT_SIZE} />
        <YAxis allowDecimals={false} tickLine={false} axisLine={false} fontSize={AXIS_FONT_SIZE} />
        <Tooltip cursor={{ fill: '#f1f5f9' }} />
        <Bar dataKey="count" name={seriesName} fill={PRIMARY} radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  )
}

// Zamana göre başvurular: aylık ilan sayısı
export function ApplicationsOverTimeChart({ data, seriesName }: { data: LabeledCount[]; seriesName: string }) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <LineChart data={data} margin={{ top: 8, right: 16, left: -16, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" vertical={false} />
        <XAxis dataKey="label" tickLine={false} fontSize={AXIS_FONT_SIZE} />
        <YAxis allowDecimals={false} tickLine={false} axisLine={false} fontSize={AXIS_FONT_SIZE} />
        <Tooltip />
        <Line type="monotone" dataKey="count" name={seriesName} stroke={PRIMARY} strokeWidth={2} dot={{ r: 3 }} />
      </LineChart>
    </ResponsiveContainer>
  )
}

// En sık eksik beceriler: yatay çubuklar (uzun beceri adları için okunaklı)
export function MissingSkillsChart({
  data,
  seriesName,
}: {
  data: { name: string; count: number }[]
  seriesName: string
}) {
  return (
    <ResponsiveContainer width="100%" height="100%">
      <BarChart data={data} layout="vertical" margin={{ top: 4, right: 16, left: 0, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" horizontal={false} />
        <XAxis type="number" allowDecimals={false} tickLine={false} fontSize={AXIS_FONT_SIZE} />
        <YAxis type="category" dataKey="name" width={110} interval={0} tickLine={false} fontSize={AXIS_FONT_SIZE} />
        <Tooltip cursor={{ fill: '#f1f5f9' }} />
        <Bar dataKey="count" name={seriesName} fill="#ef4444" radius={[0, 4, 4, 0]} />
      </BarChart>
    </ResponsiveContainer>
  )
}