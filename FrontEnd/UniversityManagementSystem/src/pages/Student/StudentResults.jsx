import React, { useState, useEffect } from 'react';
import Layout from '../../layout/Layout';
import { GraduationCap, Award, BookOpen, AlertCircle } from 'lucide-react';
import axios from 'axios';

const StudentResults = () => {
  const [results, setResults] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const fetchResults = async () => {
    setLoading(true);
    setError('');
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get('https://localhost:7272/api/StudentPortal/results', {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      setResults(response.data || []);
    } catch (err) {
      console.error('Failed to load student results:', err);
      const msg = err.response?.data?.message || 'Failed to load exam results. Please try again.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchResults();
  }, []);

  const anyCourseHasPractical = results.some((r) => r.hasPractical);

  return (
    <Layout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2">
          <div>
            <h1 className="text-xl font-bold text-gray-900">Academic Examination Results</h1>
            <p className="text-sm text-gray-500">View your Midterm, Final, and Practical marks recorded by your instructors.</p>
          </div>
          <div className="flex items-center gap-2">
            <span className="text-xs font-semibold px-3 py-1.5 bg-blue-50 text-blue-700 rounded-lg border border-blue-100 flex items-center gap-1.5">
              <Award className="w-4 h-4 text-blue-600" />
              Official Academic Transcript
            </span>
          </div>
        </div>

        {error && (
          <div className="p-4 bg-red-50 border border-red-200 rounded-xl text-red-700 text-sm flex items-center gap-2">
            <AlertCircle className="w-5 h-5 text-red-600 shrink-0" />
            <span>{error}</span>
          </div>
        )}

        {/* Results Table Card */}
        <div className="bg-white rounded-xl border border-gray-200 shadow-xs overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100 flex items-center justify-between">
            <div>
              <h2 className="text-base font-bold text-gray-900">Course Grades & Marks</h2>
              <p className="text-xs text-gray-500">Results are entered directly by respective course faculty</p>
            </div>
            <span className="text-xs font-semibold text-gray-600">
              {results.length} Enrolled {results.length === 1 ? 'Course' : 'Courses'}
            </span>
          </div>

          {loading ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <div className="animate-spin inline-block w-6 h-6 border-2 border-blue-600 border-t-transparent rounded-full mb-2"></div>
              <p>Loading your exam results...</p>
            </div>
          ) : results.length === 0 ? (
            <div className="py-16 text-center text-gray-500 text-sm">
              <GraduationCap className="w-10 h-10 mx-auto text-gray-300 mb-2" />
              <p className="font-medium text-gray-700">No course results available.</p>
              <p className="text-xs text-gray-400 mt-1">Results will appear here once submitted by your course teachers.</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse">
                <thead>
                  <tr className="border-b border-gray-200 bg-slate-50 text-xs font-semibold text-gray-600">
                    <th className="py-3.5 px-6">Course</th>
                    <th className="py-3.5 px-6">Teacher</th>
                    <th className="py-3.5 px-4 text-center">Semester</th>
                    <th className="py-3.5 px-4 text-center">
                      Midterm
                      <span className="block text-[11px] font-normal text-gray-400">Max 30</span>
                    </th>
                    <th className="py-3.5 px-4 text-center">
                      Final Exam
                      <span className="block text-[11px] font-normal text-gray-400">Max 50/70</span>
                    </th>
                    {anyCourseHasPractical && (
                      <th className="py-3.5 px-4 text-center">
                        Practical
                        <span className="block text-[11px] font-normal text-gray-400">Max 20</span>
                      </th>
                    )}
                    <th className="py-3.5 px-4 text-center">
                      Total Obtained
                      <span className="block text-[11px] font-normal text-gray-400">Max 100</span>
                    </th>
                    <th className="py-3.5 px-4 text-center">Grade</th>
                    <th className="py-3.5 px-6 text-center">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100 text-sm">
                  {results.map((r) => {
                    const isPassed = r.isGraded && r.gradeLetter !== 'F';
                    const isFailed = r.isGraded && r.gradeLetter === 'F';

                    return (
                      <tr key={r.courseOfferingId} className="hover:bg-slate-50/70 transition-colors">
                        <td className="py-4 px-6">
                          <div className="flex flex-col">
                            <span className="font-semibold text-gray-900 text-sm">{r.courseName}</span>
                            <span className="text-xs text-gray-500 font-mono mt-0.5">{r.courseCode || '—'}</span>
                          </div>
                        </td>
                        <td className="py-4 px-6 text-gray-700">
                          {r.teacherName && r.teacherName !== '-' ? (
                            <span className="font-medium text-gray-900">{r.teacherName}</span>
                          ) : (
                            <span className="text-gray-400 font-normal">—</span>
                          )}
                        </td>
                        <td className="py-4 px-4 text-center">
                          <span className="inline-block px-2.5 py-0.5 text-xs font-medium rounded-full bg-slate-100 text-slate-700">
                            Sem {r.semesterNumber}
                          </span>
                        </td>
                        <td className="py-4 px-4 text-center font-medium">
                          {r.midMarks !== null && r.midMarks !== undefined ? (
                            <span className="text-gray-900 font-semibold">{r.midMarks}</span>
                          ) : (
                            <span className="text-gray-400">—</span>
                          )}
                        </td>
                        <td className="py-4 px-4 text-center font-medium">
                          {r.finalMarks !== null && r.finalMarks !== undefined ? (
                            <span className="text-gray-900 font-semibold">{r.finalMarks}</span>
                          ) : (
                            <span className="text-gray-400">—</span>
                          )}
                        </td>
                        {anyCourseHasPractical && (
                          <td className="py-4 px-4 text-center font-medium">
                            {r.hasPractical ? (
                              r.practicalMarks !== null && r.practicalMarks !== undefined ? (
                                <span className="text-purple-700 font-semibold">{r.practicalMarks}</span>
                              ) : (
                                <span className="text-gray-400">—</span>
                              )
                            ) : (
                              <span className="text-gray-400 font-mono">-</span>
                            )}
                          </td>
                        )}
                        <td className="py-4 px-4 text-center font-bold">
                          {r.totalObtainedMarks !== null && r.totalObtainedMarks !== undefined ? (
                            <span className="text-gray-900 text-base">{r.totalObtainedMarks}</span>
                          ) : (
                            <span className="text-gray-400">—</span>
                          )}
                        </td>
                        <td className="py-4 px-4 text-center">
                          <span
                            className={`inline-block px-2.5 py-0.5 text-xs font-bold rounded-md ${
                              r.gradeLetter === 'A' || r.gradeLetter === 'A-'
                                ? 'bg-emerald-100 text-emerald-800'
                                : r.gradeLetter === 'B+' || r.gradeLetter === 'B' || r.gradeLetter === 'B-'
                                ? 'bg-blue-100 text-blue-800'
                                : r.gradeLetter === 'C+' || r.gradeLetter === 'C'
                                ? 'bg-amber-100 text-amber-800'
                                : r.gradeLetter === 'D'
                                ? 'bg-orange-100 text-orange-800'
                                : r.gradeLetter === 'F'
                                ? 'bg-red-100 text-red-800'
                                : 'bg-gray-100 text-gray-500'
                            }`}
                          >
                            {r.gradeLetter}
                          </span>
                        </td>
                        <td className="py-4 px-6 text-center">
                          {r.isGraded ? (
                            isPassed ? (
                              <span className="inline-block px-2.5 py-0.5 text-xs font-semibold rounded-full bg-emerald-100 text-emerald-800">
                                Passed
                              </span>
                            ) : (
                              <span className="inline-block px-2.5 py-0.5 text-xs font-semibold rounded-full bg-red-100 text-red-800">
                                Failed
                              </span>
                            )
                          ) : (
                            <span className="inline-block px-2.5 py-0.5 text-xs font-semibold rounded-full bg-gray-100 text-gray-600">
                              Pending
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
    </Layout>
  );
};

export default StudentResults;

