import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import Layout from '../../layout/Layout';
import {
  CalendarCheck,
  Calendar,
  CheckCircle2,
  XCircle,
  AlertCircle,
  ChevronDown,
  ChevronRight,
  Users,
  Percent
} from 'lucide-react';
import axios from 'axios';

const StudentAttendance = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialOfferingId = searchParams.get('offeringId');

  const [coursesSummary, setCoursesSummary] = useState([]);
  const [selectedOfferingId, setSelectedOfferingId] = useState(initialOfferingId || '');
  const [loadingSummary, setLoadingSummary] = useState(true);
  const [loadingDetails, setLoadingDetails] = useState(false);
  const [courseDetails, setCourseDetails] = useState(null);
  const [error, setError] = useState('');

  // 1. Fetch overall course-wise attendance summary
  useEffect(() => {
    const fetchAttendanceSummary = async () => {
      setLoadingSummary(true);
      setError('');
      try {
        const token = localStorage.getItem('token');
        const res = await axios.get('https://localhost:7272/api/StudentPortal/attendance', {
          headers: token ? { Authorization: `Bearer ${token}` } : {}
        });
        const list = res.data || [];
        setCoursesSummary(list);

        if (!selectedOfferingId && list.length > 0) {
          setSelectedOfferingId(list[0].courseOfferingId.toString());
        }
      } catch (err) {
        console.error('Failed to load attendance summary:', err);
        const msg = err.response?.data?.message || 'Failed to load course attendance summary.';
        setError(msg);
      } finally {
        setLoadingSummary(false);
      }
    };

    fetchAttendanceSummary();
  }, []);

  const handleSelectCourse = (offeringId) => {
    setSelectedOfferingId(offeringId);
    setSearchParams({ offeringId });
  };

  // 2. Fetch class-by-class attendance history for selected course
  useEffect(() => {
    if (!selectedOfferingId) return;

    const fetchCourseDetails = async () => {
      setLoadingDetails(true);
      setError('');
      try {
        const token = localStorage.getItem('token');
        const res = await axios.get(
          `https://localhost:7272/api/StudentPortal/course/${selectedOfferingId}/attendance`,
          { headers: token ? { Authorization: `Bearer ${token}` } : {} }
        );
        setCourseDetails(res.data);
      } catch (err) {
        console.error('Failed to load course attendance details:', err);
        const msg = err.response?.data?.message || 'Failed to load attendance history for this course.';
        setError(msg);
        setCourseDetails(null);
      } finally {
        setLoadingDetails(false);
      }
    };

    fetchCourseDetails();
  }, [selectedOfferingId]);

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
          <div>
            <h1 className="text-xl font-bold text-gray-900">Attendance Tracker</h1>
            <p className="text-sm text-gray-500">Monitor your lecture attendance percentages and individual class session records.</p>
          </div>

          {/* Course Selector Dropdown */}
          <div className="relative min-w-[280px]">
            <label className="block text-xs font-semibold text-gray-500 mb-1">Select Course</label>
            <div className="relative">
              <select
                value={selectedOfferingId}
                onChange={(e) => handleSelectCourse(e.target.value)}
                disabled={loadingSummary || coursesSummary.length === 0}
                className="w-full appearance-none bg-white border border-gray-300 text-gray-800 text-sm font-medium rounded-xl pl-3.5 pr-9 py-2.5 shadow-2xs focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer disabled:bg-gray-100"
              >
                {coursesSummary.length === 0 ? (
                  <option value="">No enrolled courses</option>
                ) : (
                  coursesSummary.map((c) => (
                    <option key={c.courseOfferingId} value={c.courseOfferingId}>
                      {c.courseCode ? `${c.courseCode} — ` : ''}{c.courseName} ({c.attendancePercentage}%)
                    </option>
                  ))
                )}
              </select>
              <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500 pointer-events-none" />
            </div>
          </div>
        </div>

        {error && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm flex items-center gap-2">
            <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        {/* Selected Course Attendance Stats */}
        {courseDetails && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-xs p-5">
            <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 pb-4 border-b border-gray-100">
              <div>
                <div className="flex items-center gap-2">
                  <span className="px-2.5 py-0.5 text-xs font-bold rounded-md bg-blue-100 text-blue-800">
                    {courseDetails.courseCode || 'COURSE'}
                  </span>
                  <h2 className="text-lg font-bold text-gray-900">{courseDetails.courseName}</h2>
                </div>
                <p className="text-xs text-gray-500 mt-1">
                  Instructor: <span className="font-semibold text-gray-700">{courseDetails.teacherName && courseDetails.teacherName !== '-' ? courseDetails.teacherName : '— (Unassigned)'}</span> • Semester {courseDetails.semesterNumber} • {courseDetails.sessionName}
                </p>
              </div>

              {/* Status Badges */}
              <div className="flex flex-wrap items-center gap-3">
                <div className="text-center px-3 py-1.5 bg-slate-50 border border-gray-200 rounded-xl">
                  <span className="block text-[11px] text-gray-500 font-medium">Classes Held</span>
                  <span className="text-base font-bold text-gray-900">{courseDetails.totalClasses}</span>
                </div>

                <div className="text-center px-3 py-1.5 bg-emerald-50 border border-emerald-200 rounded-xl">
                  <span className="block text-[11px] text-emerald-700 font-medium">Present</span>
                  <span className="text-base font-bold text-emerald-800">{courseDetails.presentCount}</span>
                </div>

                <div className="text-center px-3 py-1.5 bg-red-50 border border-red-200 rounded-xl">
                  <span className="block text-[11px] text-red-700 font-medium">Absent</span>
                  <span className="text-base font-bold text-red-800">{courseDetails.absentCount}</span>
                </div>

                <div className="text-center px-4 py-1.5 bg-purple-50 border border-purple-200 rounded-xl">
                  <span className="block text-[11px] text-purple-700 font-medium">Percentage</span>
                  <span className="text-base font-bold text-purple-900">{courseDetails.attendancePercentage}%</span>
                </div>

                <span
                  className={`px-3 py-1.5 text-xs font-bold rounded-xl border ${
                    courseDetails.attendancePercentage >= 75
                      ? 'bg-emerald-100 border-emerald-300 text-emerald-800'
                      : courseDetails.attendancePercentage >= 60
                      ? 'bg-amber-100 border-amber-300 text-amber-800'
                      : 'bg-red-100 border-red-300 text-red-800'
                  }`}
                >
                  {courseDetails.attendancePercentage >= 75
                    ? 'Eligible (≥75%)'
                    : courseDetails.attendancePercentage >= 60
                    ? 'Attendance Warning'
                    : 'Attendance Shortage (<75%)'}
                </span>
              </div>
            </div>

            {/* Class-by-Class Attendance Records */}
            <div className="mt-5">
              <h3 className="text-sm font-bold text-gray-900 mb-3 flex items-center gap-1.5">
                <Calendar className="w-4 h-4 text-gray-500" />
                Class-by-Class Attendance History
              </h3>

              {loadingDetails ? (
                <div className="py-12 text-center text-gray-500 text-sm">
                  <div className="animate-spin inline-block w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full mb-2"></div>
                  <p>Loading class sessions...</p>
                </div>
              ) : !courseDetails.history || courseDetails.history.length === 0 ? (
                <div className="py-12 text-center text-gray-500 text-sm border border-gray-100 rounded-xl bg-slate-50/50">
                  <CalendarCheck className="w-8 h-8 mx-auto text-gray-300 mb-2" />
                  <p className="font-medium text-gray-700">No attendance sessions recorded for this course yet.</p>
                </div>
              ) : (
                <div className="border border-gray-200 rounded-xl overflow-hidden">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="bg-slate-50 border-b border-gray-200 text-xs font-semibold text-gray-600">
                        <th className="py-3 px-4 w-12 text-center">#</th>
                        <th className="py-3 px-6">Class Date</th>
                        <th className="py-3 px-6 text-center">Status</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100 text-sm">
                      {courseDetails.history.map((h, idx) => {
                        const dateStr = new Date(h.classDate).toLocaleDateString('en-GB', {
                          weekday: 'short',
                          day: '2-digit',
                          month: 'short',
                          year: 'numeric'
                        });
                        const isPresent = h.status === 'Present';

                        return (
                          <tr key={h.sessionId || idx} className="hover:bg-slate-50/60 transition-colors">
                            <td className="py-3 px-4 text-center text-xs text-gray-400 font-medium">
                              {idx + 1}
                            </td>
                            <td className="py-3 px-6 font-medium text-gray-800">
                              {dateStr}
                            </td>
                            <td className="py-3 px-6 text-center">
                              {isPresent ? (
                                <span className="inline-flex items-center gap-1.5 px-3 py-1 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">
                                  <CheckCircle2 className="w-3.5 h-3.5" />
                                  Present
                                </span>
                              ) : (
                                <span className="inline-flex items-center gap-1.5 px-3 py-1 text-xs font-semibold rounded-full bg-red-100 text-red-800">
                                  <XCircle className="w-3.5 h-3.5" />
                                  Absent
                                </span>
                              )}
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        )}

        {/* All Courses Summary Table */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-xs overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
            <div>
              <h2 className="text-base font-bold text-gray-900">All Courses Attendance Summary</h2>
              <p className="text-xs text-gray-500">Overview of classes attended across all enrolled subjects</p>
            </div>
          </div>

          {loadingSummary ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <div className="animate-spin inline-block w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full mb-2"></div>
              <p>Loading course summary...</p>
            </div>
          ) : coursesSummary.length === 0 ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <CalendarCheck className="w-10 h-10 mx-auto text-gray-300 mb-2" />
              <p className="font-medium text-gray-700">No course enrollments found.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-gray-200 bg-slate-50 text-xs font-semibold text-gray-600">
                    <th className="py-3.5 px-6">Course</th>
                    <th className="py-3.5 px-6">Instructor</th>
                    <th className="py-3.5 px-4 text-center">Semester</th>
                    <th className="py-3.5 px-4 text-center">Total Classes</th>
                    <th className="py-3.5 px-4 text-center">Present</th>
                    <th className="py-3.5 px-4 text-center">Attendance %</th>
                    <th className="py-3.5 px-6 text-center">Eligibility</th>
                    <th className="py-3.5 px-6 text-center">Action</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 text-sm">
                  {coursesSummary.map((c) => {
                    const isEligible = c.attendancePercentage >= 75;
                    const isWarning = c.attendancePercentage >= 60 && c.attendancePercentage < 75;

                    return (
                      <tr key={c.courseOfferingId} className="hover:bg-slate-50/70 transition-colors">
                        <td className="py-4 px-6 font-semibold text-gray-900">
                          {c.courseName}
                          <span className="block text-xs font-mono text-gray-500 mt-0.5">{c.courseCode || '—'}</span>
                        </td>
                        <td className="py-4 px-6 text-gray-700">
                          {c.teacherName && c.teacherName !== '-' ? (
                            <span className="font-medium text-gray-900">{c.teacherName}</span>
                          ) : (
                            <span className="text-gray-400 font-normal">—</span>
                          )}
                        </td>
                        <td className="py-4 px-4 text-center text-xs text-gray-600">
                          Sem {c.semesterNumber}
                        </td>
                        <td className="py-4 px-4 text-center font-medium text-gray-700">
                          {c.totalClasses}
                        </td>
                        <td className="py-4 px-4 text-center font-semibold text-emerald-700">
                          {c.presentCount}
                        </td>
                        <td className="py-4 px-4 text-center">
                          <span className="font-bold text-sm text-gray-900">
                            {c.attendancePercentage}%
                          </span>
                        </td>
                        <td className="py-4 px-6 text-center">
                          <span
                            className={`inline-block px-2.5 py-0.5 text-xs font-semibold rounded-full ${
                              isEligible
                                ? 'bg-emerald-100 text-emerald-800'
                                : isWarning
                                ? 'bg-amber-100 text-amber-800'
                                : 'bg-red-100 text-red-800'
                            }`}
                          >
                            {isEligible ? 'Eligible' : isWarning ? 'Warning' : 'Shortage'}
                          </span>
                        </td>
                        <td className="py-4 px-6 text-center">
                          <button
                            type="button"
                            onClick={() => handleSelectCourse(c.courseOfferingId.toString())}
                            className={`px-3 py-1.5 text-xs font-medium rounded-lg border transition-colors cursor-pointer ${
                              selectedOfferingId.toString() === c.courseOfferingId.toString()
                                ? 'bg-blue-600 text-white border-blue-600'
                                : 'bg-blue-50 text-blue-700 border-blue-200 hover:bg-blue-100'
                            }`}
                          >
                            {selectedOfferingId.toString() === c.courseOfferingId.toString() ? 'Selected' : 'View History'}
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
};

export default StudentAttendance;

