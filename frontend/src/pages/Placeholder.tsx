import { Result } from 'antd'
import { useLocation } from 'react-router-dom'

/** Temporary page shown for routes whose module has not been ported yet. */
export default function Placeholder({ title }: { title?: string }) {
  const location = useLocation()
  return <Result status="info" title={title ?? 'Đang phát triển'} subTitle={`Màn hình ${location.pathname} chưa được chuyển đổi.`} />
}
