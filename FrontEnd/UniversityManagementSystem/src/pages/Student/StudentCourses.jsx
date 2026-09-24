import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Layout from '../../layout/Layout';
import StatCard from '../../components/StatCard';
import { BookOpen, CalendarCheck, Percent, GraduationCap, Users } from 'lucide-react';
import axios from 'axios';

const StudentCourses = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [data, setData] = useState({
    student: null,
    summary: { enrolledCourses: 0, totalClasses: 0, overallAttendance: 0 },
    courses: []
  });

  const currentDate = new Date().toLocaleDateString('en-GB', {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric'
  });

  const fetchStudentCourses = async () => {
    setLoading(true);
    setError('');
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get('https://localhost:7272/api/StudentPortal/my-courses', {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      setData(response.data);
    } catch (err) {
      console.error('Failed to fetch student courses:', err);
      const msg = err.response?.data?.message || 'Failed to load enrolled courses. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchStudentCourses();
  }, []);

  const studentName = data.student?.studentName || localStorage.getItem('userName') || 'Student';

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2">
          <div>
            <h1 className="text-xl font-bold text-gray-900">My Enrolled Courses</h1>
            <p className="text-sm text-gray-500">
              Welcome back, <span className="font-semibold text-gray-700">{studentName}</span>! Here is your academic overview for this semester.
            </p>
          </div>
          <div className="text-xs sm:text-sm font-medium text-gray-600 bg-white border border-gray-200 px-3 py-1.5 rounded-lg shadow-xs">
            {currentDate}
          </div>
        </div>

        {error && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm">
            {error}
          </div>
        )}

        {/* Top 3 Summary Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-5">
          <StatCard
            Icon={BookOpen}
            Title="Enrolled Courses"
            Value={data.summary.enrolledCourses}
            Change=""
            SubText="Current active subjects"
            iconBg="bg-blue-500/20"
            cardBg="bg-white"
            iconcolor="text-blue-600"
          />

          <StatCard
            Icon={CalendarCheck}
            Title="Total Classes"
            Value={data.summary.totalClasses}
            Change=""
            SubText="Classes conducted to date"
            iconBg="bg-emerald-500/20"
            cardBg="bg-white"
            iconcolor="text-emerald-600"
          />

          <StatCard
            Icon={Percent}
            Title="Overall Attendance"
            Value={`${data.summary.overallAttendance}%`}
            Change=""
            SubText="Across all enrolled courses"
            iconBg="bg-purple-500/20"
            cardBg="bg-white"
            iconcolor="text-purple-600"
          />
        </div>

        {/* Enrolled Courses List */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-xs overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
            <div>
              <h2 className="text-base font-bold text-gray-900">Enrolled Courses</h2>
              <p className="text-xs text-gray-500">Course offerings in which you are actively enrolled</p>
            </div>
            <span className="text-xs font-semibold px-2.5 py-1 bg-blue-50 text-blue-700 rounded-full border border-blue-100">
              {data.courses.length} {data.courses.length === 1 ? 'Course' : 'Courses'}
            </span>
          </div>

          {loading ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <div className="animate-spin inline-block w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full mb-2"></div>
              <p>Loading enrolled courses...</p>
            </div>
          ) : data.courses.length === 0 ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <BookOpen className="w-10 h-10 mx-auto text-gray-300 mb-2" />
              <p className="font-medium text-gray-700">No active course enrollments found.</p>
              <p className="text-xs text-gray-400 mt-1">Please contact your university administration if this is unexpected.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-gray-200 bg-slate-50 text-xs font-semibold text-gray-600">
                    <th className="py-3.5 px-6">Course Code</th>
                    <th className="py-3.5 px-6">Course Name</th>
                    <th className="py-3.5 px-6">Teacher</th>
                    <th className="py-3.5 px-6">Program</th>
                    <th className="py-3.5 px-4 text-center">Semester</th>
                    <th className="py-3.5 px-6">Session</th>
                    <th className="py-3.5 px-4 text-center">Type</th>
                    <th className="py-3.5 px-6 text-center">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 text-sm">
                  {data.courses.map((co) => (
                    <tr key={co.courseOfferingId} className="hover:bg-slate-50/70 transition-colors">
                      <td className="py-4 px-6 font-semibold text-gray-900">
                        {co.courseCode || '—'}
                      </td>
                      <td className="py-4 px-6 font-medium text-gray-800">
                        {co.courseName}
                      </td>
                      <td className="py-4 px-6 text-gray-700">
                        {co.teacherName && co.teacherName !== '-' ? (
                          <span className="font-medium text-gray-900">{co.teacherName}</span>
                        ) : (
                          <span className="text-gray-400 font-normal">—</span>
                        )}
                      </td>
                      <td className="py-4 px-6 text-gray-600">
                        {co.programName || '—'}
                      </td>
                      <td className="py-4 px-4 text-center">
                        <span className="inline-block px-2.5 py-0.5 text-xs font-medium rounded-full bg-slate-100 text-slate-700">
                          Sem {co.semesterNumber}
                        </span>
                      </td>
                      <td className="py-4 px-6 text-gray-600 text-xs">
                        {co.sessionName || '—'}
                      </td>
                      <td className="py-4 px-4 text-center">
                        {co.hasPractical ? (
                          <span className="inline-block px-2.5 py-0.5 text-xs font-medium rounded-full bg-purple-50 text-purple-700 border border-purple-200">
                            Theory + Lab
                          </span>
                        ) : (
                          <span className="inline-block px-2.5 py-0.5 text-xs font-medium rounded-full bg-blue-50 text-blue-700 border border-blue-200">
                            Theory
                          </span>
                        )}
                      </td>
                      <td className="py-4 px-6 text-center">
                        <div className="flex items-center justify-center gap-2">
                          <button
                            type="button"
                            onClick={() => navigate('/student/results')}
                            className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-indigo-700 bg-indigo-50 hover:bg-indigo-100 border border-indigo-200 rounded-lg transition-colors cursor-pointer"
                            title="View Exam Results"
                          >
                            <GraduationCap className="w-3.5 h-3.5" />
                            Results
                          </button>
                          <button
                            type="button"
                            onClick={() => navigate(`/student/attendance?offeringId=${co.courseOfferingId}`)}
                            className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 rounded-lg transition-colors cursor-pointer"
                            title="View Attendance History"
                          >
                            <Users className="w-3.5 h-3.5" />
                            Attendance
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
};

export default StudentCourses;

