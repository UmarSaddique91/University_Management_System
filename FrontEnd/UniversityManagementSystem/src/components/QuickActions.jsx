import React from 'react';
import { useNavigate } from 'react-router-dom';
import { UserPlus, BookOpen, GraduationCap, Landmark, ChevronRight } from 'lucide-react';

const QuickActions = () => {
  const navigate = useNavigate();

  // Navigation handler to navigate to the selected module
  const handleActionClick = (path) => {
    navigate(path);
  };

  return (
    // Quick Actions Card Container
    <div className="bg-white rounded-2xl p-6 border border-slate-100 shadow-sm w-full h-full 
    flex flex-col justify-between">
      
      {/* Card Header Title */}
      <h2 className="text-base font-bold text-slate-800 mb-4">
        Quick Actions
      </h2>

      {/* 2x2 Grid for Action Buttons */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 flex-1">
        
        {/* 1. Add Student Button (Blue Theme) */}
        <button
          type="button"
          onClick={() => handleActionClick('/students')}
          className="flex items-center justify-between px-4 py-3.5 rounded-xl bg-blue-50/80 hover:bg-blue-100 transition-colors cursor-pointer text-left group"
        >
          <div className="flex items-center gap-2.5">
            <UserPlus size={20} className="text-blue-600" />
            <span className="font-semibold text-sm text-blue-600">Add Student</span>
          </div>
          <ChevronRight size={18} className="text-blue-500 group-hover:translate-x-0.5 transition-transform" />
        </button>

        {/* 2. Add Course Button (Green Theme) */}
        <button
          type="button"
          onClick={() => handleActionClick('/courses')}
          className="flex items-center justify-between px-4 py-3.5 rounded-xl bg-green-50/80 hover:bg-green-100 transition-colors cursor-pointer text-left group"
        >
          <div className="flex items-center gap-2.5">
            <BookOpen size={20} className="text-green-700" />
            <span className="font-semibold text-sm text-green-700">Add Course</span>
          </div>
          <ChevronRight size={18} className="text-green-600 group-hover:translate-x-0.5 transition-transform" />
        </button>

        {/* 3. Add Teacher Button (Purple Theme) */}
        <button
          type="button"
          onClick={() => handleActionClick('/teachers')}
          className="flex items-center justify-between px-4 py-3.5 rounded-xl bg-purple-50/80 hover:bg-purple-100 transition-colors cursor-pointer text-left group"
        >
          <div className="flex items-center gap-2.5">
            <GraduationCap size={20} className="text-purple-600" />
            <span className="font-semibold text-sm text-purple-700">Add Teacher</span>
          </div>
          <ChevronRight size={18} className="text-purple-500 group-hover:translate-x-0.5 transition-transform" />
        </button>

        {/* 4. Add Department Button (Amber / Orange Theme) */}
        <button
          type="button"
          onClick={() => handleActionClick('/departments')}
          className="flex items-center justify-between px-4 py-3.5 rounded-xl bg-orange-50/80 hover:bg-orange-100 transition-colors cursor-pointer text-left group"
        >
          <div className="flex items-center gap-2.5">
            <Landmark size={20} className="text-orange-600" />
            <span className="font-semibold text-sm text-orange-700">Add Department</span>
          </div>
          <ChevronRight size={18} className="text-orange-500 group-hover:translate-x-0.5 transition-transform" />
        </button>

      </div>
    </div>
  );
};

export default QuickActions;

