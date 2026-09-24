import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import Layout from '../../layout/Layout';
import {
  CalendarCheck,
  Plus,
  UsersRound,
  ChevronDown,
  Calendar,
  CheckCircle2,
  AlertCircle,
  X,
  History,
  Check,
  UserCheck,
  UserX,
  Eye
} from 'lucide-react';
import axios from 'axios';

const TeacherAttendance = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialOfferingId = searchParams.get('offeringId');

  const [courses, setCourses] = useState([]);
  const [selectedOfferingId, setSelectedOfferingId] = useState(initialOfferingId || '');
  const [loadingCourses, setLoadingCourses] = useState(true);
  const [loadingHistory, setLoadingHistory] = useState(false);
  const [activeTab, setActiveTab] = useState('history'); // 'history' | 'students'
  const [historyData, setHistoryData] = useState(null);
  const [feedback, setFeedback] = useState({ type: '', message: '' });

  // Take Attendance Modal State
  const [showTakeModal, setShowTakeModal] = useState(false);
  const [classDate, setClassDate] = useState(() => new Date().toISOString().split('T')[0]);
  const [enrolledStudents, setEnrolledStudents] = useState([]);
  const [attendanceRecords, setAttendanceRecords] = useState({}); // { [studentId]: 'Present' | 'Absent' }
  const [submittingAttendance, setSubmittingAttendance] = useState(false);

  // Edit / View Session Modal State
  const [selectedSessionId, setSelectedSessionId] = useState(null);
  const [showEditModal, setShowEditModal] = useState(false);
  const [editSessionData, setEditSessionData] = useState(null);
  const [editRecords, setEditRecords] = useState({});
  const [savingEdit, setSavingEdit] = useState(false);

  // 1. Fetch Teacher Courses
  useEffect(() => {
    const fetchCourses = async () => {
      setLoadingCourses(true);
      try {
        const token = localStorage.getItem('token');
        const res = await axios.get('https://localhost:7272/api/TeacherPortal/my-courses', {
          headers: token ? { Authorization: `Bearer ${token}` } : {}
        });
        const courseList = res.data.courses || [];
        setCourses(courseList);

        if (!selectedOfferingId && courseList.length > 0) {
          setSelectedOfferingId(courseList[0].courseOfferingId.toString());
        }
      } catch (err) {
        console.error('Failed to fetch courses:', err);
        setFeedback({ type: 'error', message: 'Failed to load assigned courses.' });
      } finally {
        setLoadingCourses(false);
      }
    };

    fetchCourses();
  }, []);

  const handleCourseChange = (offeringId) => {
    setSelectedOfferingId(offeringId);
    setSearchParams({ offeringId });
    setFeedback({ type: '', message: '' });
  };

  // 2. Fetch Attendance History & Student Percentages
  const fetchAttendanceHistory = async () => {
    if (!selectedOfferingId) return;

    setLoadingHistory(true);
    try {
      const token = localStorage.getItem('token');
      const res = await axios.get(
        `https://localhost:7272/api/TeacherPortal/course/${selectedOfferingId}/attendance-history`,
        { headers: token ? { Authorization: `Bearer ${token}` } : {} }
      );
      setHistoryData(res.data);
    } catch (err) {
      console.error('Failed to fetch attendance history:', err);
      const msg = err.response?.data?.message || 'Failed to load attendance history.';
      setFeedback({ type: 'error', message: msg });
      setHistoryData(null);
    } finally {
      setLoadingHistory(false);
    }
  };

  useEffect(() => {
    fetchAttendanceHistory();
  }, [selectedOfferingId]);

  // Open "Take Attendance" modal
  const handleOpenTakeModal = async () => {
    if (!selectedOfferingId) return;

    setFeedback({ type: '', message: '' });
    setClassDate(new Date().toISOString().split('T')[0]);

    try {
      const token = localStorage.getItem('token');
      const res = await axios.get(
        `https://localhost:7272/api/TeacherPortal/course/${selectedOfferingId}/students`,
        { headers: token ? { Authorization: `Bearer ${token}` } : {} }
      );
      const students = res.data || [];
      setEnrolledStudents(students);

      // Default all to 'Present'
      const initialMap = {};
      students.forEach((s) => {
        initialMap[s.studentId] = 'Present';
      });
      setAttendanceRecords(initialMap);
      setShowTakeModal(true);
    } catch (err) {
      console.error('Failed to load students for attendance:', err);
      setFeedback({ type: 'error', message: 'Failed to load enrolled students.' });
    }
  };

  const handleMarkAll = (status) => {
    const updated = {};
    enrolledStudents.forEach((s) => {
      updated[s.studentId] = status;
    });
    setAttendanceRecords(updated);
  };

  const handleToggleStudent = (studentId, status) => {
    setAttendanceRecords((prev) => ({
      ...prev,
      [studentId]: status
    }));
  };

  // Submit new attendance session
  const handleSubmitAttendance = async () => {
    if (!classDate) {
      alert('Please select a class date.');
      return;
    }

    setSubmittingAttendance(true);
    try {
      const payload = {
        classDate: new Date(classDate).toISOString(),
        records: Object.keys(attendanceRecords).map((studentId) => ({
          studentId: Number(studentId),
          status: attendanceRecords[studentId]
        }))
      };

      const token = localStorage.getItem('token');
      await axios.post(
        `https://localhost:7272/api/TeacherPortal/course/${selectedOfferingId}/take-attendance`,
        payload,
        { headers: token ? { Authorization: `Bearer ${token}` } : {} }
      );

      setShowTakeModal(false);
      setFeedback({ type: 'success', message: 'Attendance marked successfully!' });
      fetchAttendanceHistory();
    } catch (err) {
      console.error('Failed to submit attendance:', err);
      const msg = err.response?.data?.message || 'Failed to record attendance.';
      alert(msg);
    } finally {
      setSubmittingAttendance(false);
    }
  };

  // Open "Edit Session" modal
  const handleOpenEditSession = async (sessionId) => {
    setSelectedSessionId(sessionId);
    try {
      const token = localStorage.getItem('token');
      const res = await axios.get(
        `https://localhost:7272/api/TeacherPortal/attendance-session/${sessionId}`,
        { headers: token ? { Authorization: `Bearer ${token}` } : {} }
      );
      setEditSessionData(res.data);

      const statusMap = {};
      (res.data.records || []).forEach((r) => {
        statusMap[r.studentId] = r.status;
      });
      setEditRecords(statusMap);
      setShowEditModal(true);
    } catch (err) {
      console.error('Failed to load session details:', err);
      alert('Failed to load attendance session details.');
    }
  };

  // Save updated session attendance
  const handleSaveEditSession = async () => {
    if (!selectedSessionId) return;

    setSavingEdit(true);
    try {
      const payload = {
        records: Object.keys(editRecords).map((studentId) => ({
          studentId: Number(studentId),
          status: editRecords[studentId]
        }))
      };

      const token = localStorage.getItem('token');
      await axios.put(
        `https://localhost:7272/api/TeacherPortal/attendance-session/${selectedSessionId}`,
        payload,
        { headers: token ? { Authorization: `Bearer ${token}` } : {} }
      );

      setShowEditModal(false);
      setFeedback({ type: 'success', message: 'Attendance session updated successfully!' });
      fetchAttendanceHistory();
    } catch (err) {
      console.error('Failed to update session:', err);
      alert('Failed to update attendance session.');
    } finally {
      setSavingEdit(false);
    }
  };

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header & Course Selector */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
          <div>
            <h1 className="text-xl font-bold text-gray-900">Attendance Management</h1>
            <p className="text-sm text-gray-500">Record class sessions and monitor student attendance rates.</p>
          </div>

          <div className="relative min-w-[280px]">
            <label className="block text-xs font-semibold text-gray-500 mb-1">Select Course Offering</label>
            <div className="relative">
              <select
                value={selectedOfferingId}
                onChange={(e) => handleCourseChange(e.target.value)}
                disabled={loadingCourses || courses.length === 0}
                className="w-full appearance-none bg-white border border-gray-300 text-gray-800 text-sm font-medium rounded-xl pl-3.5 pr-9 py-2.5 shadow-2xs focus:outline-none focus:ring-2 focus:ring-blue-500 cursor-pointer disabled:bg-gray-100"
              >
                {courses.length === 0 ? (
                  <option value="">No courses assigned</option>
                ) : (
                  courses.map((c) => (
                    <option key={c.courseOfferingId} value={c.courseOfferingId}>
                      {c.courseCode ? `${c.courseCode} — ` : ''}{c.courseName} (Sem {c.semesterNumber})
                    </option>
                  ))
                )}
              </select>
              <ChevronDown className="absolute right-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500 pointer-events-none" />
            </div>
          </div>
        </div>

        {/* Feedback Message */}
        {feedback.message && (
          <div
            className={`p-4 rounded-xl text-sm flex items-center gap-2 ${
              feedback.type === 'success'
                ? 'bg-emerald-50 border border-emerald-200 text-emerald-800'
                : 'bg-red-50 border border-red-200 text-red-800'
            }`}
          >
            {feedback.type === 'success' ? (
              <CheckCircle2 className="w-5 h-5 text-emerald-600 shrink-0" />
            ) : (
              <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
            )}
            <span>{feedback.message}</span>
          </div>
        )}

        {/* Course Banner & Quick Action */}
        {historyData && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-xs p-5">
            <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
              <div>
                <div className="flex items-center gap-2">
                  <span className="px-2.5 py-0.5 text-xs font-bold rounded-md bg-blue-100 text-blue-800">
                    {historyData.courseCode || 'COURSE'}
                  </span>
                  <h2 className="text-lg font-bold text-gray-900">{historyData.courseName}</h2>
                </div>
                <p className="text-xs text-gray-500 mt-1">
                  {historyData.programName} • Semester {historyData.semesterNumber} • {historyData.sessionName}
                </p>
              </div>

              <div className="flex items-center gap-3">
                <div className="text-right hidden sm:block">
                  <span className="block text-xs text-gray-500 font-medium">Classes Conducted</span>
                  <span className="text-lg font-bold text-gray-900">{historyData.sessions?.length || 0}</span>
                </div>

                <button
                  type="button"
                  onClick={handleOpenTakeModal}
                  className="inline-flex items-center gap-2 px-4 py-2 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 active:bg-emerald-800 rounded-xl shadow-xs transition-colors cursor-pointer"
                >
                  <Plus className="w-4 h-4" />
                  Take Attendance
                </button>
              </div>
            </div>

            {/* Navigation Tabs */}
            <div className="flex items-center gap-4 mt-6 border-b border-gray-200">
              <button
                type="button"
                onClick={() => setActiveTab('history')}
                className={`pb-3 text-sm font-semibold border-b-2 transition-colors cursor-pointer inline-flex items-center gap-2 ${
                  activeTab === 'history'
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                <History className="w-4 h-4" />
                Attendance History ({historyData.sessions?.length || 0})
              </button>
              <button
                type="button"
                onClick={() => setActiveTab('students')}
                className={`pb-3 text-sm font-semibold border-b-2 transition-colors cursor-pointer inline-flex items-center gap-2 ${
                  activeTab === 'students'
                    ? 'border-blue-600 text-blue-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700'
                }`}
              >
                <UsersRound className="w-4 h-4" />
                Student Percentages ({historyData.studentPercentages?.length || 0})
              </button>
            </div>
          </div>
        )}

        {/* Tab 1: Attendance History */}
        {activeTab === 'history' && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-xs overflow-hidden">
            {loadingHistory ? (
              <div className="py-16 text-center text-gray-500 text-sm">
                <div className="animate-spin inline-block w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full mb-2"></div>
                <p>Loading attendance history...</p>
              </div>
            ) : !historyData || historyData.sessions.length === 0 ? (
              <div className="py-16 text-center text-gray-500 text-sm">
                <CalendarCheck className="w-10 h-10 mx-auto text-gray-300 mb-2" />
                <p className="font-medium text-gray-700">No attendance sessions recorded yet.</p>
                <p className="text-xs text-gray-400 mt-1">Click "Take Attendance" above to mark attendance for a class session.</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="border-b border-gray-200 bg-slate-50 text-xs font-semibold text-gray-600">
                      <th className="py-3.5 px-6">Class Date</th>
                      <th className="py-3.5 px-6">Recorded At</th>
                      <th className="py-3.5 px-4 text-center">Enrolled</th>
                      <th className="py-3.5 px-4 text-center">Present</th>
                      <th className="py-3.5 px-4 text-center">Absent</th>
                      <th className="py-3.5 px-4 text-center">Turnout</th>
                      <th className="py-3.5 px-6 text-center">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 text-sm">
                    {historyData.sessions.map((s) => {
                      const total = s.totalStudents || 0;
                      const pct = total > 0 ? Math.round((s.presentCount / total) * 100) : 0;
                      const dateStr = new Date(s.classDate).toLocaleDateString('en-GB', {
                        day: '2-digit',
                        month: 'short',
                        year: 'numeric'
                      });
                      const createdStr = new Date(s.createdAt).toLocaleTimeString('en-US', {
                        hour: '2-digit',
                        minute: '2-digit'
                      });

                      return (
                        <tr key={s.sessionId} className="hover:bg-slate-50/70 transition-colors">
                          <td className="py-4 px-6 font-semibold text-gray-900">
                            {dateStr}
                          </td>
                          <td className="py-4 px-6 text-xs text-gray-500">
                            {createdStr}
                          </td>
                          <td className="py-4 px-4 text-center font-medium text-gray-700">
                            {s.totalStudents}
                          </td>
                          <td className="py-4 px-4 text-center">
                            <span className="inline-block px-2.5 py-0.5 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">
                              {s.presentCount}
                            </span>
                          </td>
                          <td className="py-4 px-4 text-center">
                            <span className="inline-block px-2.5 py-0.5 text-xs font-semibold rounded-full bg-red-100 text-red-800">
                              {s.absentCount}
                            </span>
                          </td>
                          <td className="py-4 px-4 text-center">
                            <span className="font-bold text-xs text-gray-800">
                              {pct}%
                            </span>
                          </td>
                          <td className="py-4 px-6 text-center">
                            <button
                              type="button"
                              onClick={() => handleOpenEditSession(s.sessionId)}
                              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-medium text-blue-700 bg-blue-50 hover:bg-blue-100 border border-blue-200 rounded-lg transition-colors cursor-pointer"
                              title="View / Edit Session"
                            >
                              <Eye className="w-3.5 h-3.5" />
                              View / Edit
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
        )}

        {/* Tab 2: Student Attendance Percentages */}
        {activeTab === 'students' && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-xs overflow-hidden">
            {loadingHistory ? (
              <div className="py-16 text-center text-gray-500 text-sm">
                <div className="animate-spin inline-block w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full mb-2"></div>
                <p>Loading student percentages...</p>
              </div>
            ) : !historyData || historyData.studentPercentages.length === 0 ? (
              <div className="py-16 text-center text-gray-500 text-sm">
                <UsersRound className="w-10 h-10 mx-auto text-gray-300 mb-2" />
                <p className="font-medium text-gray-700">No student enrollment records found.</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse">
                  <thead>
                    <tr className="border-b border-gray-200 bg-slate-50 text-xs font-semibold text-gray-600">
                      <th className="py-3.5 px-4 w-12 text-center">#</th>
                      <th className="py-3.5 px-4">Roll Number</th>
                      <th className="py-3.5 px-6">Student Name</th>
                      <th className="py-3.5 px-4 text-center">Total Classes</th>
                      <th className="py-3.5 px-4 text-center">Attended</th>
                      <th className="py-3.5 px-4 text-center">Percentage</th>
                      <th className="py-3.5 px-4 text-center">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-gray-100 text-sm">
                    {historyData.studentPercentages.map((sp, idx) => {
                      const isEligible = sp.attendancePercentage >= 75;
                      const isWarning = sp.attendancePercentage >= 60 && sp.attendancePercentage < 75;

                      return (
                        <tr key={sp.studentId} className="hover:bg-slate-50/70 transition-colors">
                          <td className="py-3.5 px-4 text-center text-xs text-gray-400 font-medium">
                            {idx + 1}
                          </td>
                          <td className="py-3.5 px-4 font-semibold text-gray-900 text-xs">
                            {sp.rollNumber || '—'}
                          </td>
                          <td className="py-3.5 px-6 font-medium text-gray-800">
                            {sp.studentName}
                          </td>
                          <td className="py-3.5 px-4 text-center text-gray-600">
                            {sp.totalClasses}
                          </td>
                          <td className="py-3.5 px-4 text-center font-semibold text-gray-800">
                            {sp.presentCount}
                          </td>
                          <td className="py-3.5 px-4 text-center">
                            <span className="font-bold text-sm text-gray-900">
                              {sp.attendancePercentage}%
                            </span>
                          </td>
                          <td className="py-3.5 px-4 text-center">
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
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        )}

        {/* Modal: Take Attendance */}
        {showTakeModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-xs">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-150">
              {/* Modal Header */}
              <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between bg-slate-50">
                <div>
                  <h3 className="text-base font-bold text-gray-900">Take Class Attendance</h3>
                  <p className="text-xs text-gray-500">Record attendance for enrolled students</p>
                </div>
                <button
                  type="button"
                  onClick={() => setShowTakeModal(false)}
                  className="p-1.5 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100 transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              {/* Modal Body */}
              <div className="p-6 overflow-y-auto space-y-4">
                {/* Date Selection & Quick Actions */}
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 bg-slate-50 p-3.5 rounded-xl border border-gray-200">
                  <div className="flex items-center gap-2">
                    <Calendar className="w-4 h-4 text-gray-500" />
                    <label className="text-xs font-semibold text-gray-700">Class Date:</label>
                    <input
                      type="date"
                      value={classDate}
                      onChange={(e) => setClassDate(e.target.value)}
                      className="px-3 py-1 text-sm bg-white border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 font-medium text-gray-800"
                    />
                  </div>

                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={() => handleMarkAll('Present')}
                      className="px-2.5 py-1 text-xs font-semibold text-emerald-700 bg-emerald-50 hover:bg-emerald-100 border border-emerald-200 rounded-lg transition-colors cursor-pointer"
                    >
                      All Present
                    </button>
                    <button
                      type="button"
                      onClick={() => handleMarkAll('Absent')}
                      className="px-2.5 py-1 text-xs font-semibold text-red-700 bg-red-50 hover:bg-red-100 border border-red-200 rounded-lg transition-colors cursor-pointer"
                    >
                      All Absent
                    </button>
                  </div>
                </div>

                {/* Students List */}
                <div className="border border-gray-200 rounded-xl overflow-hidden">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="bg-slate-50 border-b border-gray-200 text-xs font-semibold text-gray-600">
                        <th className="py-2.5 px-4 w-12 text-center">#</th>
                        <th className="py-2.5 px-4">Roll No</th>
                        <th className="py-2.5 px-4">Student Name</th>
                        <th className="py-2.5 px-4 text-center">Status</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100 text-sm">
                      {enrolledStudents.map((s, idx) => {
                        const status = attendanceRecords[s.studentId] || 'Present';
                        const isPresent = status === 'Present';

                        return (
                          <tr key={s.studentId} className="hover:bg-slate-50/60">
                            <td className="py-2.5 px-4 text-center text-xs text-gray-400">
                              {idx + 1}
                            </td>
                            <td className="py-2.5 px-4 font-semibold text-xs text-gray-900">
                              {s.rollNumber || '—'}
                            </td>
                            <td className="py-2.5 px-4 font-medium text-gray-800">
                              {s.studentName}
                            </td>
                            <td className="py-2.5 px-4 text-center">
                              <div className="inline-flex rounded-lg border border-gray-200 p-0.5 bg-gray-50">
                                <button
                                  type="button"
                                  onClick={() => handleToggleStudent(s.studentId, 'Present')}
                                  className={`px-3 py-1 text-xs font-bold rounded-md transition-all cursor-pointer ${
                                    isPresent
                                      ? 'bg-emerald-600 text-white shadow-xs'
                                      : 'text-gray-500 hover:text-gray-800'
                                  }`}
                                >
                                  Present
                                </button>
                                <button
                                  type="button"
                                  onClick={() => handleToggleStudent(s.studentId, 'Absent')}
                                  className={`px-3 py-1 text-xs font-bold rounded-md transition-all cursor-pointer ${
                                    !isPresent
                                      ? 'bg-red-600 text-white shadow-xs'
                                      : 'text-gray-500 hover:text-gray-800'
                                  }`}
                                >
                                  Absent
                                </button>
                              </div>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Modal Footer */}
              <div className="px-6 py-4 border-t border-gray-200 bg-slate-50 flex items-center justify-end gap-3">
                <button
                  type="button"
                  onClick={() => setShowTakeModal(false)}
                  className="px-4 py-2 text-sm font-semibold text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-xl transition-colors cursor-pointer"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  onClick={handleSubmitAttendance}
                  disabled={submittingAttendance}
                  className="px-5 py-2 text-sm font-semibold text-white bg-emerald-600 hover:bg-emerald-700 active:bg-emerald-800 rounded-xl shadow-xs transition-colors cursor-pointer disabled:bg-emerald-300"
                >
                  {submittingAttendance ? 'Saving Attendance...' : 'Save Attendance'}
                </button>
              </div>
            </div>
          </div>
        )}

        {/* Modal: View / Edit Session Attendance */}
        {showEditModal && editSessionData && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-xs">
            <div className="bg-white rounded-2xl shadow-xl w-full max-w-2xl max-h-[90vh] flex flex-col overflow-hidden animate-in fade-in zoom-in-95 duration-150">
              <div className="px-6 py-4 border-b border-gray-200 flex items-center justify-between bg-slate-50">
                <div>
                  <h3 className="text-base font-bold text-gray-900">
                    Session Attendance: {new Date(editSessionData.classDate).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })}
                  </h3>
                  <p className="text-xs text-gray-500">{editSessionData.courseName}</p>
                </div>
                <button
                  type="button"
                  onClick={() => setShowEditModal(false)}
                  className="p-1.5 text-gray-400 hover:text-gray-600 rounded-lg hover:bg-gray-100 transition-colors"
                >
                  <X className="w-5 h-5" />
                </button>
              </div>

              <div className="p-6 overflow-y-auto space-y-4">
                <div className="border border-gray-200 rounded-xl overflow-hidden">
                  <table className="w-full text-left border-collapse">
                    <thead>
                      <tr className="bg-slate-50 border-b border-gray-200 text-xs font-semibold text-gray-600">
                        <th className="py-2.5 px-4 w-12 text-center">#</th>
                        <th className="py-2.5 px-4">Roll No</th>
                        <th className="py-2.5 px-4">Student Name</th>
                        <th className="py-2.5 px-4 text-center">Status</th>
                      </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-100 text-sm">
                      {editSessionData.records.map((r, idx) => {
                        const status = editRecords[r.studentId] || r.status || 'Present';
                        const isPresent = status === 'Present';

                        return (
                          <tr key={r.studentId} className="hover:bg-slate-50/60">
                            <td className="py-2.5 px-4 text-center text-xs text-gray-400">
                              {idx + 1}
                            </td>
                            <td className="py-2.5 px-4 font-semibold text-xs text-gray-900">
                              {r.rollNumber || '—'}
                            </td>
                            <td className="py-2.5 px-4 font-medium text-gray-800">
                              {r.studentName}
                            </td>
                            <td className="py-2.5 px-4 text-center">
                              <div className="inline-flex rounded-lg border border-gray-200 p-0.5 bg-gray-50">
                                <button
                                  type="button"
                                  onClick={() => setEditRecords((p) => ({ ...p, [r.studentId]: 'Present' }))}
                                  className={`px-3 py-1 text-xs font-bold rounded-md transition-all cursor-pointer ${
                                    isPresent
                                      ? 'bg-emerald-600 text-white shadow-xs'
                                      : 'text-gray-500 hover:text-gray-800'
                                  }`}
                                >
                                  Present
                                </button>
                                <button
                                  type="button"
                                  onClick={() => setEditRecords((p) => ({ ...p, [r.studentId]: 'Absent' }))}
                                  className={`px-3 py-1 text-xs font-bold rounded-md transition-all cursor-pointer ${
                                    !isPresent
                                      ? 'bg-red-600 text-white shadow-xs'
                                      : 'text-gray-500 hover:text-gray-800'
                                  }`}
                                >
                                  Absent
                                </button>
                              </div>
                            </td>
                          </tr>
                        );
                      })}
                    </tbody>
                  </table>
                </div>
              </div>

              <div className="px-6 py-4 border-t border-gray-200 bg-slate-50 flex items-center justify-end gap-3">
                <button
                  type="button"
                  onClick={() => setShowEditModal(false)}
                  className="px-4 py-2 text-sm font-semibold text-gray-600 hover:text-gray-800 hover:bg-gray-100 rounded-xl transition-colors cursor-pointer"
                >
                  Cancel
                </button>
                <button
                  type="button"
                  onClick={handleSaveEditSession}
                  disabled={savingEdit}
                  className="px-5 py-2 text-sm font-semibold text-white bg-blue-600 hover:bg-blue-700 active:bg-blue-800 rounded-xl shadow-xs transition-colors cursor-pointer disabled:bg-blue-300"
                >
                  {savingEdit ? 'Updating...' : 'Update Attendance'}
                </button>
              </div>
            </div>
          </div>
        )}
      </div>
    </Layout>
  );
};

export default TeacherAttendance;

