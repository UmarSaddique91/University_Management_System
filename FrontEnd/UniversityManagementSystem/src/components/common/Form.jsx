import React, { useState, useEffect } from "react";
import { X, Search, ChevronDown, Check } from "lucide-react";

/**
 * Reusable Form Modal Component
 * 
 * Used for generic forms (Students, Teachers, Courses, Departments, etc.)
 * Manages generic form values, initial values, input changes, and searchable dropdown behaviors.
 */
const Form = ({
  title = "Form Title",
  subtitle = "",
  inputFields = [],
  dropdownFields = [],
  initialValues = null,
  onClose,
  onSubmit,
  primaryButtonText = "Save Changes",
  cancelButtonText = "Cancel",
}) => {
  const [formData, setFormData] = useState(initialValues || {});
  const [openDropdown, setOpenDropdown] = useState(null);
  const [searchQueries, setSearchQueries] = useState({});

  useEffect(() => {
    setFormData(initialValues || {});
    setOpenDropdown(null);
    setSearchQueries({});
  }, [initialValues]);

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleOptionSelect = (fieldName, optionValue) => {
    setFormData((prev) => ({
      ...prev,
      [fieldName]: optionValue,
    }));
    setOpenDropdown(null);
  };

  const handleSearchChange = (fieldName, value) => {
    setSearchQueries((prev) => ({
      ...prev,
      [fieldName]: value,
    }));
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    if (onSubmit) {
      onSubmit(formData);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-xs">
      {/* Modal Dialog Card */}
      <div
        className="relative w-full max-w-2xl bg-white rounded-2xl shadow-2xl border border-gray-100 flex flex-col max-h-[90vh] overflow-hidden"
        onClick={(e) => e.stopPropagation()}
      >
        


        {/* Model Header */}
        <div className="px-6 py-4 border-b border-gray-100 flex items-start justify-between gap-4">
          <div>
            <h3 className="text-lg font-bold text-gray-900 leading-6">
              {title}
            </h3>
            {subtitle && (
              <p className="text-xs text-gray-500 mt-1">{subtitle}</p>
            )}
          </div>

          <button
            type="button"
            aria-label="Close"
            className="text-gray-400 hover:text-gray-600 p-1.5 rounded-lg hover:bg-gray-100 transition-colors cursor-pointer"
            onClick={onClose}
          >
            <X className="w-5 h-5 pointer-events-none" />
          </button>
        </div>






{/* MOdel Body  */}
        <form onSubmit={handleSubmit} className="flex flex-col flex-1 overflow-hidden">
          <div className="px-6 py-5 overflow-y-auto flex-1 space-y-6">
            
            {/* Dynamic Regular Input Fields (2-column responsive grid) */}
            {inputFields && inputFields.length > 0 && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {inputFields.map((field, index) => {
                  const isFullWidth = field.colSpan === 2 || field.fullWidth;

                  return (
                    <div
                      key={index}
                      className={isFullWidth ? "col-span-1 md:col-span-2" : "col-span-1"}
                    >
                      <label className="block text-xs font-semibold text-gray-700 mb-1.5 uppercase tracking-wider">
                        {field.label}
                        {field.required && (
                          <span className="text-rose-500 ml-1">*</span>
                        )}
                      </label>

                      <input
                        type={field.type || "text"}
                        name={field.name}
                        value={formData[field.name] ?? field.defaultValue ?? ""}
                        onChange={handleInputChange}
                        placeholder={field.placeholder || ""}
                        required={field.required}
                        className="w-full px-3.5 py-2.5 bg-white border border-gray-200 rounded-lg text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors shadow-2xs"
                      />

                      {field.helperText && (
                        <p className="text-xs text-gray-400 mt-1">
                          {field.helperText}
                        </p>
                      )}
                    </div>
                  );
                })}
              </div>
            )}

            {/* Dynamic Searchable Dropdown Fields (2-column responsive grid) */}
            {dropdownFields && dropdownFields.length > 0 && (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {dropdownFields.map((dropdown, index) => {
                  const isFullWidth = dropdown.colSpan === 2 || dropdown.fullWidth;
                  const isOpen = openDropdown === dropdown.name;
                  const selectedVal = formData[dropdown.name];

                  // Find label of selected option for trigger box display
                  const selectedOption = (dropdown.options || []).find((opt) => {
                    const optVal = typeof opt === "object" ? opt.value : opt;
                    return selectedVal !== undefined && selectedVal !== null && String(optVal) === String(selectedVal);
                  });

                  const displayLabel = selectedOption
                    ? typeof selectedOption === "object"
                      ? selectedOption.label
                      : selectedOption
                    : "";

                  // Filter options by search query
                  const query = (searchQueries[dropdown.name] || "").toLowerCase();
                  const filteredOptions = (dropdown.options || []).filter((opt) => {
                    const optLabel = typeof opt === "object" ? opt.label : String(opt);
                    return optLabel.toLowerCase().includes(query);
                  });

                  return (
                    <div
                      key={index}
                      className={`relative ${isFullWidth ? "col-span-1 md:col-span-2" : "col-span-1"}`}
                    >
                      <label className="block text-xs font-semibold text-gray-700 mb-1.5 uppercase tracking-wider">
                        {dropdown.label}
                        {dropdown.required && (
                          <span className="text-rose-500 ml-1">*</span>
                        )}
                      </label>

                      {/* Dropdown Trigger Box */}
                      <div
                        onClick={() => setOpenDropdown(isOpen ? null : dropdown.name)}
                        className="flex items-center justify-between w-full px-3.5 py-2.5 bg-white border border-gray-200 rounded-lg text-sm text-gray-700 shadow-2xs cursor-pointer hover:border-gray-300 transition-colors"
                      >
                        <span className={displayLabel ? "text-gray-900 font-medium" : "text-gray-400"}>
                          {displayLabel || dropdown.placeholder || "Select option..."}
                        </span>
                        <ChevronDown className={`w-4 h-4 text-gray-400 transition-transform ${isOpen ? "rotate-180" : ""}`} />
                      </div>

                      {/* Searchable Options Menu */}
                      {isOpen && (
                        <div className="absolute top-full left-0 right-0 mt-1.5 bg-white border border-gray-200 rounded-lg shadow-xl overflow-hidden z-20">
                          {/* Dropdown Search Input */}
                          <div className="relative p-2 border-b border-gray-100 bg-gray-50/70">
                            <Search className="w-3.5 h-3.5 text-gray-400 absolute left-4 top-1/2 -translate-y-1/2 pointer-events-none" />
                            <input
                              type="text"
                              value={searchQueries[dropdown.name] || ""}
                              onChange={(e) => handleSearchChange(dropdown.name, e.target.value)}
                              placeholder={dropdown.searchPlaceholder || "Search..."}
                              autoFocus
                              className="w-full pl-7 pr-3 py-1.5 text-xs bg-white border border-gray-200 rounded-md text-gray-700 placeholder-gray-400 focus:outline-none focus:border-blue-500"
                            />
                          </div>

                          {/* Options List */}
                          <ul className="max-h-36 overflow-y-auto py-1 divide-y divide-gray-50">
                            {filteredOptions.length > 0 ? (
                              filteredOptions.map((option, optIdx) => {
                                const optionLabel = typeof option === "object" ? option.label : option;
                                const optionValue = typeof option === "object" ? option.value : option;
                                const isSelected = selectedVal !== undefined && selectedVal !== null && String(selectedVal) === String(optionValue);

                                return (
                                  <li
                                    key={optIdx}
                                    onClick={() => handleOptionSelect(dropdown.name, optionValue)}
                                    className={`px-3.5 py-2 text-xs cursor-pointer flex items-center justify-between transition-colors ${
                                      isSelected
                                        ? "bg-blue-50 text-blue-700 font-medium"
                                        : "text-gray-700 hover:bg-gray-50 hover:text-gray-900"
                                    }`}
                                  >
                                    <span>{optionLabel}</span>
                                    {isSelected && (
                                      <Check className="w-3.5 h-3.5 text-blue-600" />
                                    )}
                                  </li>
                                );
                              })
                            ) : (
                              <li className="px-3 py-2 text-xs text-gray-400 text-center">
                                No options found
                              </li>
                            )}
                          </ul>
                        </div>
                      )}

                      {dropdown.helperText && (
                        <p className="text-xs text-gray-400 mt-1">
                          {dropdown.helperText}
                        </p>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* Model Footer  */}
          <div className="px-6 py-4 border-t border-gray-100 bg-gray-50/50 flex items-center justify-end gap-3">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-200 hover:bg-gray-50 rounded-lg transition-colors cursor-pointer shadow-2xs"
            >
              {cancelButtonText}
            </button>

            <button
              type="submit"
              className="px-5 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-lg transition-colors cursor-pointer shadow-2xs"
            >
              {primaryButtonText}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default Form;
