import { useEffect, useMemo, useState } from 'react'
import { Layout, Menu, Select, Space, Tag, Typography, Dropdown, Button, type MenuProps } from 'antd'
import { LogoutOutlined, KeyOutlined, UserOutlined } from '@ant-design/icons'
import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import { adminMenu, receptionistMenu } from './menu'
import { api } from '../api/client'
import ChangePasswordModal from '../components/ChangePasswordModal'

const { Header, Sider, Content } = Layout

interface Branch {
  id: number
  name: string
}

export default function AppLayout() {
  const { user, logout, branchId, setBranchId } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const tabs = user?.isAdministrator ? adminMenu : receptionistMenu
  const [branches, setBranches] = useState<Branch[]>([])
  const [pwdOpen, setPwdOpen] = useState(false)

  // Determine active tab from the current path
  const activeTab = useMemo(() => {
    const found = tabs.find((t) => t.groups.some((g) => g.items.some((i) => location.pathname.startsWith(i.path))))
    return found?.key ?? tabs[0].key
  }, [tabs, location.pathname])
  const [tabKey, setTabKey] = useState(activeTab)
  useEffect(() => setTabKey(activeTab), [activeTab])

  useEffect(() => {
    if (!user?.isAdministrator) return
    api.get<Branch[]>('/branches').then((r) => setBranches(r.data)).catch(() => setBranches([]))
  }, [user?.isAdministrator])

  const sideItems: MenuProps['items'] = tabs
    .find((t) => t.key === tabKey)!
    .groups.map((g) => ({
      key: g.label,
      label: g.label,
      type: 'group',
      children: g.items.map((i) => ({ key: i.path, label: <Link to={i.path}>{i.label}</Link> })),
    }))

  const selectedKey = useMemo(() => {
    const all = tabs.flatMap((t) => t.groups.flatMap((g) => g.items.map((i) => i.path)))
    // longest matching prefix wins
    return all.filter((p) => location.pathname === p || location.pathname.startsWith(p + '/')).sort((a, b) => b.length - a.length)[0] ?? ''
  }, [tabs, location.pathname])

  const userMenu: MenuProps['items'] = [
    { key: 'pwd', icon: <KeyOutlined />, label: 'Đổi mật khẩu', onClick: () => setPwdOpen(true) },
    { type: 'divider' },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Đăng xuất',
      onClick: () => {
        logout()
        navigate('/login')
      },
    },
  ]

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Header style={{ display: 'flex', alignItems: 'center', gap: 24, padding: '0 16px', background: '#1f3b5c' }}>
        <Typography.Text strong style={{ color: '#fff', fontSize: 18, whiteSpace: 'nowrap' }}>
          Nha khoa Đức Hạnh
        </Typography.Text>
        <Menu
          theme="dark"
          mode="horizontal"
          selectedKeys={[tabKey]}
          onClick={(e) => setTabKey(e.key)}
          items={tabs.map((t) => ({ key: t.key, label: t.label }))}
          style={{ flex: 1, minWidth: 0, background: 'transparent' }}
        />
        <Space>
          {user?.isAdministrator ? (
            <Select
              style={{ width: 220 }}
              value={branchId}
              onChange={setBranchId}
              options={[{ value: 0, label: 'Tất cả chi nhánh' }, ...branches.filter((b) => b.id !== 0).map((b) => ({ value: b.id, label: b.name }))]}
            />
          ) : (
            <Tag color="blue">{user?.branchName}</Tag>
          )}
          <Dropdown menu={{ items: userMenu }} trigger={['click']}>
            <Button type="text" icon={<UserOutlined />} style={{ color: '#fff' }}>
              {user?.fullName || user?.userName}
            </Button>
          </Dropdown>
        </Space>
      </Header>
      <Layout>
        <Sider width={230} theme="light" style={{ borderRight: '1px solid #f0f0f0', overflow: 'auto' }}>
          <Menu mode="inline" selectedKeys={[selectedKey]} items={sideItems} style={{ borderInlineEnd: 0 }} />
        </Sider>
        <Content style={{ padding: 16, overflow: 'auto', background: '#f5f7fa' }}>
          <Outlet />
        </Content>
      </Layout>
      <ChangePasswordModal open={pwdOpen} onClose={() => setPwdOpen(false)} />
    </Layout>
  )
}
