/**
 * Print CSS for the examination screens (replacement for the legacy RDLC reports).
 * Hides the app chrome (header, side menu) and anything marked `.no-print`; shows `.print-only` blocks.
 */
export default function PrintStyles() {
  return (
    <style>{`
      .print-only { display: none; }
      @media print {
        @page { size: A4; margin: 12mm; }
        body, #root { background: #fff !important; }
        .ant-layout-header, .ant-layout-sider, .no-print { display: none !important; }
        .ant-layout-content { padding: 0 !important; overflow: visible !important; background: #fff !important; }
        .print-only { display: block !important; }
        .print-page { break-after: page; }
        .print-table { width: 100%; border-collapse: collapse; font-size: 12px; }
        .print-table th, .print-table td { border: 1px solid #555; padding: 3px 5px; }
        .print-table th { background: #eee; }
      }
      .print-table { width: 100%; border-collapse: collapse; font-size: 12px; }
      .print-table th, .print-table td { border: 1px solid #bbb; padding: 3px 5px; }
    `}</style>
  )
}
