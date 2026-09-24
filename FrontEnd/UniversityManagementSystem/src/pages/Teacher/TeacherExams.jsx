import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import Layout from '../../layout/Layout';
import { FileText, Save, CheckCircle2, AlertCircle, ChevronDown, Award } from 'lucide-react';
import axios from 'axios';

const calculateGradeLetter = (obtained, total) => {
  if (total <= 0 || obtained === null || obtained === undefined) return '—';
  const pct = (obtained / total) * 100.0;
  if (pct >= 85) return 'A';
  if (pct >= 80) return 'A-';
  if (pct >= 75) return 'B+';
  if (pct >= 70) return 'B';
  if (pct >= 65) return 'B-';
  if (pct >= 60) return 'C+';
  if (pct >= 55) return 'C';
  if (pct >= 50) return 'D';
  return 'F';
};

const TeacherExams = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const initialOfferingId = searchParams.get('offeringId');

  const [courses, setCourses] = useState([]);
  const [selectedOfferingId, setSelectedOfferingId] = useState(initialOfferingId || '');
  const [loadingCourses, setLoadingCourses] = useState(true);
  const [loadingExams, setLoadingExams] = useState(false);
  const [saving, setSaving] = useState(false);
  const [examData, setExamData] = useState(null);
  const [studentMarks, setStudentMarks] = useState([]);
  const [errors, setErrors] = useState({});
  const [feedback, setFeedback] = useState({ type: '', message: '' });

  // 1. Fetch teacher courses
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

  // Update query params when selected course changes
  const handleCourseChange = (offeringId) => {
    setSelectedOfferingId(offeringId);
    setSearchParams({ offeringId });
    setFeedback({ type: '', message: '' });
  };

  // 2. Fetch exam details for selected course offering
  useEffect(() => {
    if (!selectedOfferingId) return;

    const fetchExams = async () => {
      setLoadingExams(true);
      setErrors({});
      try {
        const token = localStorage.getItem('token');
        const res = await axios.get(`https://localhost:7272/api/TeacherPortal/course/${selectedOfferingId}/exams`, {
          headers: token ? { Authorization: `Bearer ${token}` } : {}
        });
        setExamData(res.data);

        // Populate local state for editing
        const mapped = (res.data.students || []).map((s) => ({
          studentId: s.studentId,
          rollNumber: s.rollNumber,
          studentName: s.studentName,
          midMarks: s.midMarks !== null && s.midMarks !== undefined ? s.midMarks : '',
          finalMarks: s.finalMarks !== null && s.finalMarks !== undefined ? s.finalMarks : '',
          practicalMarks: s.practicalMarks !== null && s.practicalMarks !== undefined ? s.practicalMarks : ''
        }));
        setStudentMarks(mapped);
      } catch (err) {
        console.error('Failed to load course exams:', err);
        const msg = err.response?.data?.message || 'Failed to load exams and student marks.';
        setFeedback({ type: 'error', message: msg });
        setExamData(null);
        setStudentMarks([]);
      } finally {
        setLoadingExams(false);
      }
    };

    fetchExams();
  }, [selectedOfferingId]);

  // Handle input change & live validation
  const handleMarkChange = (studentId, field, value) => {
    setFeedback({ type: '', message: '' });

    setStudentMarks((prev) =>
      prev.map((item) => {
        if (item.studentId === studentId) {
          return { ...item, [field]: value };
        }
        return item;
      })
    );

    // Validation
    const num = value === '' ? null : Number(value);
    let max = 0;
    if (field === 'midMarks') max = examData?.midTotalMarks || 30;
    else if (field === 'finalMarks') max = examData?.finalTotalMarks || 70;
    else if (field === 'practicalMarks') max = examData?.practicalTotalMarks || 20;

    const errorKey = `${studentId}_${field}`;
    if (num !== null && (isNaN(num) || num < 0 || num > max)) {
      setErrors((prev) => ({
        ...prev,
        [errorKey]: `Must be 0–${max}`
      }));
    } else {
      setErrors((prev) => {
        const copy = { ...prev };
        delete copy[errorKey];
        return copy;
      });
    }
  };

  // Submit marks
  const handleSaveMarks = async () => {
    if (Object.keys(errors).length > 0) {
      setFeedback({ type: 'error', message: 'Please correct the highlighted mark validation errors before saving.' });
      return;
    }

    setSaving(true);
    setFeedback({ type: '', message: '' });

    try {
      const payload = {
        students: studentMarks.map((s) => ({
          studentId: s.studentId,
          midMarks: s.midMarks === '' ? null : Number(s.midMarks),
          finalMarks: s.finalMarks === '' ? null : Number(s.finalMarks),
          practicalMarks: examData?.hasPractical && s.practicalMarks !== '' ? Number(s.practicalMarks) : null
        }))
      };

      const token = localStorage.getItem('token');
      const res = await axios.post(
        `https://localhost:7272/api/TeacherPortal/course/${selectedOfferingId}/save-marks`,
        payload,
        { headers: token ? { Authorization: `Bearer ${token}` } : {} }
      );

      setFeedback({
        type: 'success',
        message: res.data?.message || 'Student marks saved successfully!'
      });
    } catch (err) {
      console.error('Failed to save marks:', err);
      const msg = err.response?.data?.message || 'Failed to save marks. Please check input values.';
      setFeedback({ type: 'error', message: msg });
    } finally {
      setSaving(false);
    }
  };

  const selectedCourse = courses.find((c) => c.courseOfferingId.toString() === selectedOfferingId.toString());

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3">
          <div>
            <h1 className="text-xl font-bold text-gray-900">Exams & Marks Entry</h1>
            <p className="text-sm text-gray-500">Record and manage Midterm, Final, and Practical examination marks.</p>
          </div>

          {/* Course Selector Dropdown */}
          <div className="relative min-w-[280px]">
            <label className="block text-xs font-semibold text-gray-500 mb-1">Select Course Offering</label>
            <div className="relative">
              <select
                value={selectedOfferingId}
                onChange={(e) => handleCourseChange(e.target.value)}
                disabled={loadingCourses || courses.length === 0}
                className="w-full appearance-none bg-white border border-gray-300 text-gray-800 text-sm font-medium rounded-xl pl-3.5 pr-9 py-2.5 shadow-2xs focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500 cursor-pointer disabled:bg-gray-100"
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

        {/* Feedback Messages */}
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

        {/* Course Details & Exam Weightage Card */}
        {examData && (
          <div className="bg-white rounded-xl border border-gray-200 shadow-xs p-5">
            <div className="flex flex-col lg:flex-row lg:items-center justify-between gap-4 pb-4 border-b border-gray-100">
              <div>
                <div className="flex items-center gap-2">
                  <span className="px-2.5 py-0.5 text-xs font-bold rounded-md bg-blue-100 text-blue-800">
                    {examData.courseCode || 'COURSE'}
                  </span>
                  <h2 className="text-lg font-bold text-gray-900">{examData.courseName}</h2>
                </div>
                <p className="text-xs text-gray-500 mt-1">
                  {examData.programName} • Semester {examData.semesterNumber} • {examData.sessionName}
                </p>
              </div>

              {/* Weightage Badges */}
              <div className="flex flex-wrap items-center gap-2 text-xs font-semibold">
                <span className="px-3 py-1.5 rounded-lg bg-blue-50 text-blue-700 border border-blue-200">
                  Midterm: {examData.midTotalMarks} Marks
                </span>
                <span className="px-3 py-1.5 rounded-lg bg-indigo-50 text-indigo-700 border border-indigo-200">
                  Final Exam: {examData.finalTotalMarks} Marks
                </span>
                {examData.hasPractical ? (
                  <span className="px-3 py-1.5 rounded-lg bg-purple-50 text-purple-700 border border-purple-200">
                    Practical: {examData.practicalTotalMarks} Marks
                  </span>
                ) : (
                  <span className="px-3 py-1.5 rounded-lg bg-gray-50 text-gray-500 border border-gray-200">
                    No Practical Exam
                  </span>
                )}
                <span className="px-3 py-1.5 rounded-lg bg-emerald-50 text-emerald-700 border border-emerald-200">
                  Total: 100 Marks
                </span>
              </div>
            </div>

            {/* Save Button Header Action */}
            <div className="mt-4 flex items-center justify-between">
              <span className="text-xs text-gray-500">
                {studentMarks.length} Enrolled {studentMarks.length === 1 ? 'Student' : 'Students'}
              </span>
              <button
                type="button"
                onClick={handleSaveMarks}
                disabled={saving || studentMarks.length === 0}
                className="inline-flex items-center gap-2 px-4 py-2 text-sm font-semibold text-white bg-blue-600 hover:bg-blue-700 active:bg-blue-800 rounded-xl shadow-xs transition-colors cursor-pointer disabled:bg-blue-300"
              >
                <Save className="w-4 h-4" />
                {saving ? 'Saving Marks...' : 'Save Marks'}
              </button>
            </div>
          </div>
        )}

        {/* Marks Entry Table */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-xs overflow-hidden">
          {loadingExams ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <div className="animate-spin inline-block w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full mb-2"></div>
              <p>Loading course marks data...</p>
            </div>
          ) : !examData || studentMarks.length === 0 ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <FileText className="w-10 h-10 mx-auto text-gray-300 mb-2" />
              <p className="font-medium text-gray-700">No enrolled students found for this course offering.</p>
              <p className="text-xs text-gray-400 mt-1">Students enrolled in this course will appear here automatically.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-gray-200 bg-slate-50 text-xs font-semibold text-gray-600">
                    <th className="py-3.5 px-4 w-12 text-center">#</th>
                    <th className="py-3.5 px-4">Roll Number</th>
                    <th className="py-3.5 px-6">Student Name</th>
                    <th className="py-3.5 px-4 text-center">
                      Mid Exam
                      <span className="block text-[11px] font-normal text-gray-500">Max {examData.midTotalMarks}</span>
                    </th>
                    <th className="py-3.5 px-4 text-center">
                      Final Exam
                      <span className="block text-[11px] font-normal text-gray-500">Max {examData.finalTotalMarks}</span>
                    </th>
                    {examData.hasPractical && (
                      <th className="py-3.5 px-4 text-center">
                        Practical Exam
                        <span className="block text-[11px] font-normal text-gray-500">Max {examData.practicalTotalMarks}</span>
                      </th>
                    )}
                    <th className="py-3.5 px-4 text-center">
                      Total Obtained
                      <span className="block text-[11px] font-normal text-gray-500">Max 100</span>
                    </th>
                    <th className="py-3.5 px-4 text-center">Grade</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 text-sm">
                  {studentMarks.map((row, idx) => {
                    const midVal = row.midMarks !== '' ? Number(row.midMarks) : 0;
                    const finalVal = row.finalMarks !== '' ? Number(row.finalMarks) : 0;
                    const pracVal = examData.hasPractical && row.practicalMarks !== '' ? Number(row.practicalMarks) : 0;

                    const hasAnyEntry = row.midMarks !== '' || row.finalMarks !== '' || (examData.hasPractical && row.practicalMarks !== '');
                    const totalObtained = hasAnyEntry ? (midVal + finalVal + pracVal) : null;
                    const letterGrade = calculateGradeLetter(totalObtained, 100);

                    const midErr = errors[`${row.studentId}_midMarks`];
                    const finalErr = errors[`${row.studentId}_finalMarks`];
                    const practicalErr = errors[`${row.studentId}_practicalMarks`];

                    return (
                      <tr key={row.studentId} className="hover:bg-slate-50/60 transition-colors">
                        <td className="py-3.5 px-4 text-center text-xs text-gray-400 font-medium">
                          {idx + 1}
                        </td>
                        <td className="py-3.5 px-4 font-semibold text-gray-900 text-xs">
                          {row.rollNumber || '—'}
                        </td>
                        <td className="py-3.5 px-6 font-medium text-gray-800">
                          {row.studentName}
                        </td>

                        {/* Mid Exam Input */}
                        <td className="py-3.5 px-4 text-center">
                          <div className="inline-block">
                            <input
                              type="number"
                              min="0"
                              max={examData.midTotalMarks}
                              step="0.5"
                              value={row.midMarks}
                              onChange={(e) => handleMarkChange(row.studentId, 'midMarks', e.target.value)}
                              placeholder="0"
                              className={`w-20 text-center py-1.5 px-2 text-sm font-semibold rounded-lg border ${
                                midErr ? 'border-red-500 bg-red-50 text-red-700' : 'border-gray-300 bg-white text-gray-800'
                              } focus:outline-none focus:ring-2 focus:ring-blue-500`}
                            />
                            {midErr && <span className="block text-[10px] text-red-600 mt-0.5">{midErr}</span>}
                          </div>
                        </td>

                        {/* Final Exam Input */}
                        <td className="py-3.5 px-4 text-center">
                          <div className="inline-block">
                            <input
                              type="number"
                              min="0"
                              max={examData.finalTotalMarks}
                              step="0.5"
                              value={row.finalMarks}
                              onChange={(e) => handleMarkChange(row.studentId, 'finalMarks', e.target.value)}
                              placeholder="0"
                              className={`w-20 text-center py-1.5 px-2 text-sm font-semibold rounded-lg border ${
                                finalErr ? 'border-red-500 bg-red-50 text-red-700' : 'border-gray-300 bg-white text-gray-800'
                              } focus:outline-none focus:ring-2 focus:ring-blue-500`}
                            />
                            {finalErr && <span className="block text-[10px] text-red-600 mt-0.5">{finalErr}</span>}
                          </div>
                        </td>

                        {/* Practical Exam Input (if course has practical) */}
                        {examData.hasPractical && (
                          <td className="py-3.5 px-4 text-center">
                            <div className="inline-block">
                              <input
                                type="number"
                                min="0"
                                max={examData.practicalTotalMarks}
                                step="0.5"
                                value={row.practicalMarks}
                                onChange={(e) => handleMarkChange(row.studentId, 'practicalMarks', e.target.value)}
                                placeholder="0"
                                className={`w-20 text-center py-1.5 px-2 text-sm font-semibold rounded-lg border ${
                                  practicalErr ? 'border-red-500 bg-red-50 text-red-700' : 'border-gray-300 bg-white text-gray-800'
                                } focus:outline-none focus:ring-2 focus:ring-purple-500`}
                              />
                              {practicalErr && <span className="block text-[10px] text-red-600 mt-0.5">{practicalErr}</span>}
                            </div>
                          </td>
                        )}

                        {/* Total Marks Live Preview */}
                        <td className="py-3.5 px-4 text-center">
                          <span className={`inline-block font-bold text-sm ${totalObtained !== null ? 'text-gray-900' : 'text-gray-400'}`}>
                            {totalObtained !== null ? totalObtained : '—'}
                          </span>
                        </td>

                        {/* Grade Letter */}
                        <td className="py-3.5 px-4 text-center">
                          <span
                            className={`inline-block px-2.5 py-0.5 text-xs font-bold rounded-md ${
                              letterGrade === 'A' || letterGrade === 'A-'
                                ? 'bg-emerald-100 text-emerald-800'
                                : letterGrade === 'B+' || letterGrade === 'B' || letterGrade === 'B-'
                                ? 'bg-blue-100 text-blue-800'
                                : letterGrade === 'C+' || letterGrade === 'C'
                                ? 'bg-amber-100 text-amber-800'
                                : letterGrade === 'D'
                                ? 'bg-orange-100 text-orange-800'
                                : letterGrade === 'F'
                                ? 'bg-red-100 text-red-800'
                                : 'bg-gray-100 text-gray-500'
                            }`}
                          >
                            {letterGrade}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}

          {/* Footer Save Button */}
          {examData && studentMarks.length > 0 && (
            <div className="p-4 bg-slate-50 border-t border-gray-100 flex items-center justify-end">
              <button
                type="button"
                onClick={handleSaveMarks}
                disabled={saving}
                className="inline-flex items-center gap-2 px-5 py-2.5 text-sm font-semibold text-white bg-blue-600 hover:bg-blue-700 active:bg-blue-800 rounded-xl shadow-xs transition-colors cursor-pointer disabled:bg-blue-300"
              >
                <Save className="w-4 h-4" />
                {saving ? 'Saving...' : 'Save All Marks'}
              </button>
            </div>
          )}
        </div>
      </div>
    </Layout>
  );
};

export default TeacherExams;

