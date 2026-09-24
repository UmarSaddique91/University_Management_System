import React from 'react';
import { Trash2, AlertTriangle, X, Loader2 } from 'lucide-react';

/**
 * Reusable Professional Delete Confirmation Modal
 * Replaces ugly browser alert/window.confirm dialogs across the system.
 */
const DeleteConfirmationModal = ({
  isOpen,
  title = "Confirm Deletion",
  itemName = "",
  message,
  confirmText = "Delete",
  cancelText = "Cancel",
  isDeleting = false,
  errorMessage = "",
  onConfirm,
  onClose,
}) => {
  if (!isOpen) return null;

  return (
    <div 
      className="fixed inset-0 z-50 bg-black/40 backdrop-blur-xs flex items-center justify-center p-4 overflow-y-auto animate-in fade-in duration-200"
      onClick={!isDeleting ? onClose : undefined}
    >
      <div 
        className="bg-white rounded-2xl shadow-xl w-full max-w-md border border-gray-100 overflow-hidden my-auto transform transition-all animate-in zoom-in-95 duration-200"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header Section */}
        <div className="p-6 pb-4">
          <div className="flex items-start justify-between">
            <div className="w-12 h-12 rounded-full bg-rose-50 border border-rose-100 flex items-center justify-center shrink-0 text-rose-600">
              <Trash2 className="w-6 h-6 text-rose-600" />
            </div>
            <button
              type="button"
              onClick={onClose}
              disabled={isDeleting}
              className="p-1 rounded-lg text-gray-400 hover:text-gray-600 hover:bg-gray-100 transition-colors disabled:opacity-50 cursor-pointer"
            >
              <X className="w-5 h-5" />
            </button>
          </div>

          <div className="mt-4">
            <h3 className="text-lg font-bold text-gray-900">
              {title}
            </h3>
            <p className="text-sm text-gray-600 mt-2 leading-relaxed">
              {message || (
                <>
                  Are you sure you want to delete{" "}
                  {itemName ? <span className="font-semibold text-gray-900">"{itemName}"</span> : "this item"}?{" "}
                  This action cannot be undone.
                </>
              )}
            </p>
          </div>

          {/* Inline Error Alert if delete fails */}
          {errorMessage && (
            <div className="mt-4 p-3 rounded-xl bg-rose-50 border border-rose-200 text-rose-700 text-xs flex items-start gap-2">
              <AlertTriangle className="w-4 h-4 shrink-0 mt-0.5" />
              <span className="leading-snug">{errorMessage}</span>
            </div>
          )}
        </div>

        {/* Footer Actions */}
        <div className="px-6 py-4 bg-gray-50 border-t border-gray-100 flex items-center justify-end gap-3">
          <button
            type="button"
            onClick={onClose}
            disabled={isDeleting}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 hover:bg-gray-100 rounded-xl transition-colors disabled:opacity-50 cursor-pointer shadow-2xs"
          >
            {cancelText}
          </button>
          <button
            type="button"
            onClick={onConfirm}
            disabled={isDeleting}
            className="px-4 py-2 text-sm font-medium text-white bg-rose-600 hover:bg-rose-700 rounded-xl transition-colors disabled:opacity-50 cursor-pointer flex items-center gap-2 shadow-2xs"
          >
            {isDeleting && <Loader2 className="w-4 h-4 animate-spin" />}
            {isDeleting ? "Deleting..." : confirmText}
          </button>
        </div>
      </div>
    </div>
  );
};

export default DeleteConfirmationModal;

