import { useEffect, useState } from 'react'
import { Button, Empty, Image, Popconfirm, Space, Upload, message } from 'antd'
import { DeleteOutlined, UploadOutlined } from '@ant-design/icons'
import { examApi } from './api'
import { errorMessage } from '../../api/client'

interface Props {
  prescriptionId: number
  appointmentNo: number
  pictureIds: number[]
  onChanged: () => void
  readOnly?: boolean
}

/** Port of ImageManager / ImageViewerForm ("Hình ảnh"): X-ray / intra-oral pictures stored as bytea. */
export default function PicturesPanel({ prescriptionId, appointmentNo, pictureIds, onChanged, readOnly }: Props) {
  const [urls, setUrls] = useState<Record<number, string>>({})
  const [uploading, setUploading] = useState(false)

  useEffect(() => {
    let cancelled = false
    const created: string[] = []
    Promise.all(
      pictureIds.map((id) =>
        examApi
          .pictureBlob(id)
          .then((b) => {
            const u = URL.createObjectURL(b)
            created.push(u)
            return [id, u] as const
          })
          .catch(() => [id, ''] as const),
      ),
    ).then((pairs) => {
      if (!cancelled) setUrls(Object.fromEntries(pairs))
    })
    return () => {
      cancelled = true
      created.forEach((u) => URL.revokeObjectURL(u))
    }
  }, [pictureIds])

  return (
    <div>
      {!readOnly && (
        <Upload
          accept="image/*"
          multiple
          showUploadList={false}
          beforeUpload={async (file) => {
            setUploading(true)
            try {
              await examApi.addPicture(prescriptionId, appointmentNo, file)
              message.success(`Đã thêm ${file.name}`)
              onChanged()
            } catch (e) {
              message.error(errorMessage(e))
            } finally {
              setUploading(false)
            }
            return false
          }}
        >
          <Button icon={<UploadOutlined />} loading={uploading}>
            Thêm hình ảnh
          </Button>
        </Upload>
      )}
      {pictureIds.length === 0 ? (
        <Empty description="Chưa có hình ảnh" style={{ marginTop: 16 }} />
      ) : (
        <Image.PreviewGroup>
          <Space wrap style={{ marginTop: 12 }}>
            {pictureIds.map((id) => (
              <div key={id} style={{ textAlign: 'center' }}>
                <Image width={160} height={120} style={{ objectFit: 'cover' }} src={urls[id]} />
                {!readOnly && (
                  <div>
                    <Popconfirm
                      title="Xóa hình này?"
                      okText="Xóa"
                      cancelText="Hủy"
                      onConfirm={async () => {
                        try {
                          await examApi.deletePicture(id)
                          onChanged()
                        } catch (e) {
                          message.error(errorMessage(e))
                        }
                      }}
                    >
                      <Button size="small" type="link" danger icon={<DeleteOutlined />}>
                        Xóa
                      </Button>
                    </Popconfirm>
                  </div>
                )}
              </div>
            ))}
          </Space>
        </Image.PreviewGroup>
      )}
    </div>
  )
}
