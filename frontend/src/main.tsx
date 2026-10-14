import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ConfigProvider } from 'antd'
import viVN from 'antd/locale/vi_VN'
import dayjs from 'dayjs'
import 'dayjs/locale/vi'
import customParseFormat from 'dayjs/plugin/customParseFormat'
import './index.css'
import App from './App'

dayjs.locale('vi')
dayjs.extend(customParseFormat)

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider
      locale={viVN}
      theme={{ token: { colorPrimary: '#1f6fb2', borderRadius: 4 }, components: { Table: { cellPaddingBlockSM: 4 } } }}
    >
      <App />
    </ConfigProvider>
  </StrictMode>,
)
