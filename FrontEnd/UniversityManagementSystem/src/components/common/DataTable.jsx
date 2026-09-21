import React from 'react';
import { ChevronDown, ChevronLeft, ChevronRight, Eye, Pencil, Trash2 } from 'lucide-react';

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
  pagination = false,
  pageSize = 10,
  currentPage = 1,
  onPageChange,
  expandable = false,
  expandedRowId = null,
  onRowExpand = null,
  renderExpandedRow = null,
  rowKey = 'id',
}) => {
  const totalItems = data.length;
  const totalPages = Math.max(1, Math.ceil(totalItems / pageSize));
  const activePage = Math.min(Math.max(1, currentPage), totalPages);
  const startIndex = (activePage - 1) * pageSize;
  const displayData = pagination ? data.slice(startIndex, startIndex + pageSize) : data;

  const getPageNumbers = () => {
    if (totalPages <= 7) {
      return Array.from({ length: totalPages }, (_, i) => i + 1);
    }
    if (activePage <= 4) {
      return [1, 2, 3, 4, 5, '...', totalPages];
    }
    if (activePage >= totalPages - 3) {
      return [1, '...', totalPages - 4, totalPages - 3, totalPages - 2, totalPages - 1, totalPages];
    }
    return [1, '...', activePage - 1, activePage, activePage + 1, '...', totalPages];
  };

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
            {displayData.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length + (showActions ? 1 : 0)}
                  className="py-8 text-center text-gray-500 text-sm"
                >
                  No records found
                </td>
              </tr>
            ) : (
              displayData.map((row, rowIdx) => {
                const isRowExpanded =
                  expandable &&
                  (row[rowKey] !== undefined ? row[rowKey] : (row.dbId ?? row.id)) === expandedRowId;

                return (
                  <React.Fragment key={row[rowKey] ?? row.dbId ?? row.id ?? rowIdx}>
                    <tr
                      className={`hover:bg-slate-50/60 transition-colors ${
                        isRowExpanded ? 'bg-blue-50/20' : ''
                      }`}
                    >
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
                            {onView && (
                              <button
                                type="button"
                                onClick={() => onView?.(row)}
                                title="View"
                                className="p-1.5 bg-gray-50 text-gray-600 hover:bg-gray-100 rounded-lg transition-colors cursor-pointer"
                              >
                                <Eye className="w-4 h-4" />
                              </button>
                            )}
                            {onEdit && (
                              <button
                                type="button"
                                onClick={() => onEdit?.(row)}
                                title="Edit"
                                className="p-1.5 bg-blue-50 text-blue-600 hover:bg-blue-100 rounded-lg transition-colors cursor-pointer"
                              >
                                <Pencil className="w-4 h-4" />
                              </button>
                            )}
                            {onDelete && (
                              <button
                                type="button"
                                onClick={() => onDelete?.(row)}
                                title="Delete"
                                className="p-1.5 bg-red-50 text-red-600 hover:bg-red-100 rounded-lg transition-colors cursor-pointer"
                              >
                                <Trash2 className="w-4 h-4" />
                              </button>
                            )}
                          </div>
                        </td>
                      )}
                    </tr>

                    {isRowExpanded && renderExpandedRow && (
                      <tr className="bg-slate-50/80 border-b border-gray-200">
                        <td
                          colSpan={columns.length + (showActions ? 1 : 0)}
                          className="p-0 border-t border-blue-100"
                        >
                          {renderExpandedRow(row)}
                        </td>
                      </tr>
                    )}
                  </React.Fragment>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination Controls */}
      {pagination && totalItems > 0 && (
        <div className="flex flex-col sm:flex-row items-center justify-between px-6 py-3.5 border-t border-gray-100 gap-3">
          <div className="text-xs text-gray-500">
            Showing <span className="font-semibold text-gray-800">{startIndex + 1}</span> to{' '}
            <span className="font-semibold text-gray-800">{Math.min(startIndex + pageSize, totalItems)}</span> of{' '}
            <span className="font-semibold text-gray-800">{totalItems}</span> results
          </div>

          <div className="flex items-center gap-1">
            <button
              type="button"
              onClick={() => onPageChange && onPageChange(activePage - 1)}
              disabled={activePage <= 1}
              className="inline-flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium text-gray-700 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors cursor-pointer"
            >
              <ChevronLeft className="w-3.5 h-3.5" />
              Previous
            </button>

            <div className="flex items-center gap-1 mx-1">
              {getPageNumbers().map((page, idx) =>
                page === '...' ? (
                  <span key={`ellipsis-${idx}`} className="px-2 py-1 text-xs text-gray-400">
                    ...
                  </span>
                ) : (
                  <button
                    key={`page-${page}`}
                    type="button"
                    onClick={() => onPageChange && onPageChange(page)}
                    className={`min-w-[28px] h-7 px-2 text-xs font-semibold rounded-lg transition-colors cursor-pointer ${
                      activePage === page
                        ? 'bg-blue-600 text-white shadow-2xs'
                        : 'text-gray-700 hover:bg-gray-100 border border-transparent'
                    }`}
                  >
                    {page}
                  </button>
                )
              )}
            </div>

            <button
              type="button"
              onClick={() => onPageChange && onPageChange(activePage + 1)}
              disabled={activePage >= totalPages}
              className="inline-flex items-center gap-1 px-2.5 py-1.5 text-xs font-medium text-gray-700 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors cursor-pointer"
            >
              Next
              <ChevronRight className="w-3.5 h-3.5" />
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default DataTable;
