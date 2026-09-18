import React from 'react';
import { ChevronDown, Eye, Pencil, Trash2 } from 'lucide-react';

const DataTable = ({
  title,
  count,
  sortOptions = [],
  currentSort = "",
  onSortChange,
  columns = [],
  data = [],
  showActions = true,
  onView,
  onEdit,
  onDelete,
}) => {
  return (
    <div className="bg-white rounded-xl border border-gray-200 shadow-sm overflow-hidden mt-4">
      {/* Table Header Controls */}
      {(title || sortOptions.length > 0) && (
        <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100">
          <h2 className="text-base font-bold text-gray-900">
            {title} {count !== undefined && <span className="font-semibold text-gray-900">({count})</span>}
          </h2>

          {sortOptions.length > 0 && (
            <div className="relative">
              <select
                value={currentSort}
                onChange={onSortChange}
                className="appearance-none bg-white border border-gray-200 text-gray-700 text-xs font-medium rounded-lg pl-3 pr-7 py-2 focus:outline-none focus:border-blue-500 cursor-pointer shadow-2xs"
              >
                {sortOptions.map((opt, idx) => (
                  <option key={idx} value={typeof opt === 'object' ? opt.value : opt}>
                    {typeof opt === 'object' ? opt.label : opt}
                  </option>
                ))}
              </select>
              <ChevronDown className="absolute right-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5 text-gray-400 pointer-events-none" />
            </div>
          )}
        </div>
      )}

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-collapse">
          <thead>
            <tr className="border-b border-gray-200 bg-slate-50">
              {columns.map((col, index) => (
                <th
                  key={index}
                  className={`py-3.5 px-4 text-xs font-semibold text-gray-600 ${
                    col.align === 'center' ? 'text-center' : col.align === 'right' ? 'text-right' : 'text-left'
                  }`}
                >
                  {col.label}
                </th>
              ))}
              {showActions && (
                <th className="py-3.5 px-4 text-xs font-semibold text-gray-600 text-center">
                  Actions
                </th>
              )}
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 text-sm">
            {data.map((row, rowIdx) => (
              <tr key={rowIdx} className="hover:bg-slate-50/60 transition-colors">
                {columns.map((col, colIdx) => (
                  <td
                    key={colIdx}
                    className={`py-3.5 px-4 text-gray-700 ${
                      col.align === 'center' ? 'text-center' : col.align === 'right' ? 'text-right' : 'text-left'
                    }`}
                  >
                    {col.render ? col.render(row[col.key], row) : row[col.key]}
                  </td>
                ))}

                {showActions && (
                  <td className="py-3.5 px-4 text-center">
                    <div className="flex items-center justify-center gap-2">
                      
                      
                      <button
                        type="button"
                        onClick={() => onDelete && onDelete(row)}
                        title="Delete"
                        className="p-1.5 bg-red-50 text-red-600 hover:bg-red-100 rounded-lg transition-colors cursor-pointer"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default DataTable;

