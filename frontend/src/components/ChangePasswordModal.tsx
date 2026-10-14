import { Form, Input, Modal, message } from 'antd'
import { api, errorMessage } from '../api/client'

export default function ChangePasswordModal({ open, onClose }: { open: boolean; onClose: () => void }) {
  const [form] = Form.useForm()

  const submit = async () => {
    const values = await form.validateFields()
    try {
      await api.post('/auth/change-password', { oldPassword: values.oldPassword, newPassword: values.newPassword })
      message.success('Đã đổi mật khẩu')
      form.resetFields()
      onClose()
    } catch (e) {
      message.error(errorMessage(e))
    }
  }

  return (
    <Modal title="Đổi mật khẩu" open={open} onOk={submit} onCancel={onClose} okText="Lưu" cancelText="Hủy" destroyOnHidden>
      <Form form={form} layout="vertical">
        <Form.Item name="oldPassword" label="Mật khẩu cũ" rules={[{ required: true, message: 'Nhập mật khẩu cũ' }]}>
          <Input.Password />
        </Form.Item>
        <Form.Item name="newPassword" label="Mật khẩu mới" rules={[{ required: true, message: 'Nhập mật khẩu mới' }]}>
          <Input.Password />
        </Form.Item>
        <Form.Item
          name="confirm"
          label="Nhập lại mật khẩu mới"
          dependencies={['newPassword']}
          rules={[
            { required: true, message: 'Nhập lại mật khẩu mới' },
            ({ getFieldValue }) => ({
              validator: (_, v) => (v === getFieldValue('newPassword') ? Promise.resolve() : Promise.reject(new Error('Mật khẩu không khớp'))),
            }),
          ]}
        >
          <Input.Password />
        </Form.Item>
      </Form>
    </Modal>
  )
}
