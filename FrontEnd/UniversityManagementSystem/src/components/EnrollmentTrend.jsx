import React from 'react';
import { ChevronDown } from 'lucide-react';
// Recharts components for responsive area chart
import {
  ResponsiveContainer,
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts';

// Static Dummy Data for Enrollment Trend
const data = [
  { month: 'Mar', students: 620 },
  { month: 'Apr', students: 700 },
  { month: 'May', students: 800 },
  { month: 'Jun', students: 900 },
  { month: 'Jul', students: 1000 },
  { month: 'Aug', students: 1120 },
  { month: 'Sep', students: 1200 },
];

const EnrollmentTrend = () => {
  return (
    // Main Card Container
    <div className="bg-white rounded-2xl p-6 border border-slate-100 shadow-sm w-full">
      
      {/* 1. Header: Title & Dropdown UI */}
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-base font-bold text-slate-800">
          Student Enrollment Trend
        </h2>

        {/* Dropdown List (UI Only - No Logic) */}
        <div className="relative">
          <select
            defaultValue="6 months"
            className="appearance-none pl-3 pr-8 py-1.5 border border-slate-200 rounded-lg 
            text-xs font-medium text-slate-600 bg-white cursor-pointer focus:outline-none 
            focus:ring-1 focus:ring-blue-500"
          >
            <option value="1 year">1 Year</option>
            <option value="6 months">Last 6 Months</option>
            <option value="3 months">3 Months</option>
            <option value="1 month">1 Month</option>
            <option value="1 week">1 Week</option>
          </select>
          <ChevronDown
            size={14}
            className="text-slate-500 absolute right-2.5 top-1/2 -translate-y-1/2 pointer-events-none"
          />
        </div>
      </div>

      {/* 2. Chart Section */}
      <div className="w-full h-64">
        {/* ResponsiveContainer: Automatically resizes chart to fill its parent container */}
        <ResponsiveContainer width="100%" height="100%">
          
          {/* AreaChart: Main wrapper component that connects data to the chart */}
          <AreaChart
            data={data}
            margin={{ top: 10, right: 10, left: -10, bottom: 0 }}
          >
            {/* Gradient definition for the light blue shaded area below the line */}
            <defs>
              <linearGradient id="enrollmentAreaGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="#3b82f6" stopOpacity={0.25} />
                <stop offset="100%" stopColor="#3b82f6" stopOpacity={0.01} />
              </linearGradient>
            </defs>

            {/* CartesianGrid: Renders horizontal grid lines behind the data */}
            <CartesianGrid vertical={false} stroke="#f1f5f9" />

            {/* XAxis: Displays month labels on the horizontal axis */}
            <XAxis
              dataKey="month"
              axisLine={false}
              tickLine={false}
              tick={{ fill: '#94a3b8', fontSize: 12 }}
              dy={10}
            />

            {/* YAxis: Displays student enrollment counts (0 to 1,250) on the vertical axis */}
            <YAxis
              domain={[0, 1250]}
              ticks={[0, 250, 500, 750, 1000, 1250]}
              axisLine={false}
              tickLine={false}
              tick={{ fill: '#94a3b8', fontSize: 12 }}
              tickFormatter={(val) => val.toLocaleString()}
            />

            {/* Area: Draws the blue line, circular dots on points, and the shaded gradient */}
            <Area
              type="monotone"
              dataKey="students"
              stroke="#2563eb"
              strokeWidth={2.5}
              fill="url(#enrollmentAreaGradient)"
              dot={{ fill: '#2563eb', r: 4.5, strokeWidth: 0 }}
              activeDot={{ r: 6, fill: '#2563eb' }}
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>

    </div>
  );
};

export default EnrollmentTrend;

