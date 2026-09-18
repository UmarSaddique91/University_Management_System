import React from 'react';
import { Search, ChevronDown, Plus } from 'lucide-react';

const FilterBar = ({
  searchPlaceholder = "Search...",
  searchValue = "",
  onSearchChange,
  filters = [],
  actionButton,
}) => {
  return (
    <div className="flex flex-col lg:flex-row items-stretch lg:items-center justify-between gap-3 my-4">
      {/* Search Bar */}
      <div className="relative flex-1 min-w-[280px]">
        <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
        <input
          type="text"
          value={searchValue}
          onChange={onSearchChange}
          placeholder={searchPlaceholder}
          
          className="w-full pl-10 pr-4 py-2.5 bg-white border border-gray-200 rounded-lg text-sm text-gray-800 placeholder-gray-400 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-colors shadow-2xs"
        />
      </div>


      

      {/* Filter Dropdowns & Action Button */}
      <div className="flex items-center gap-3 flex-wrap">
        {filters.map((filter, index) => (
          <div key={index} className="relative min-w-[150px]">
            <select
              value={filter.value}
              onChange={filter.onChange}
              className="w-full appearance-none bg-white border border-gray-200 text-gray-700 text-sm font-normal rounded-lg pl-3.5 pr-9 py-2.5 focus:outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 cursor-pointer shadow-2xs"
            >
              {filter.options ? (
                filter.options.map((opt, optIdx) => (
                  <option key={optIdx} value={typeof opt === 'object' ? opt.value : opt}>
                    {typeof opt === 'object' ? opt.label : opt}
                  </option>
                ))
              ) : (
                <option>{filter.label}</option>
              )}
            </select>
            <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400 pointer-events-none" />
          </div>
        ))}

        
      </div>
    </div>
  );
};

export default FilterBar;

