import React, { useState, useEffect } from 'react';
import Layout from '../layout/Layout';
import StatCard from '../components/StatCard';
import FilterBar from '../components/common/FilterBar';
import DataTable from '../components/common/DataTable';
import Form from '../components/common/Form';
import DeleteConfirmationModal from '../components/common/DeleteConfirmationModal';
import { BookOpen, Users, CheckCircle2, UserCheck, Plus, Briefcase, ChevronDown, ChevronUp } from 'lucide-react';
import axios from 'axios';

const CourseAssignments = () => {
  const [offerings, setOfferings] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [academicPrograms, setAcademicPrograms] = useState([]);
  const [academicSessions, setAcademicSessions] = useState([]);
  const [semesters, setSemesters] = useState([]);
  const [courses, setCourses] = useState([]);
  const [teachers, setTeachers] = useState([]);
  const [workload, setWorkload] = useState([]);

  const [searchTerm, setSearchTerm] = useState('');
  const [selectSession, setSelectSession] = useState('');
  const [selectDepartment, setSelectDepartment] = useState('');
  const [selectProgram, setSelectProgram] = useState('');
  const [selectTeacher, setSelectTeacher] = useState('');
  const [selectStatus, setSelectStatus] = useState('');
  const [currentPage, setCurrentPage] = useState(1);

  const [showModal, setShowModal] = useState(false);
  const [selectedOffering, setSelectedOffering] = useState(null);
  const [formState, setFormState] = useState({});
  const [showWorkloadView, setShowWorkloadView] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');

  const fetchData = async () => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const [offRes, deptsRes, progsRes, sessRes, semRes, crsRes, tchRes, wlRes] = await Promise.all([
        axios.get('https://localhost:7272/api/CourseOffering', { headers }),
        axios.get('https://localhost:7272/api/Department', { headers }),
        axios.get('https://localhost:7272/api/AcademicProgram', { headers }),
        axios.get('https://localhost:7272/api/AcademicSession', { headers }),
        axios.get('https://localhost:7272/api/Semester', { headers }),
        axios.get('https://localhost:7272/api/Course', { headers }),
        axios.get('https://localhost:7272/api/Teacher', { headers }),
        axios.get('https://localhost:7272/api/CourseOffering/workload', { headers }),
      ]);

      setOfferings(offRes.data || []);
      setDepartments(deptsRes.data || []);
      setAcademicPrograms(progsRes.data || []);
      setAcademicSessions(sessRes.data || []);
      setSemesters(semRes.data || []);
      setCourses(crsRes.data || []);
      setTeachers(tchRes.data || []);
      setWorkload(wlRes.data || []);
    } catch (error) {
      console.log('Failed to fetch assignments data', error);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, selectSession, selectDepartment, selectProgram, selectTeacher, selectStatus]);

  // Derived filter options
  const availableProgramsForFilter = selectDepartment && selectDepartment !== 'All Departments'
    ? academicPrograms.filter(
        (p) =>
          p.departmentName === selectDepartment ||
          p.department?.departmentName === selectDepartment
      )
    : academicPrograms;

  const handleDepartmentFilterChange = (e) => {
    const val = e.target.value;
    setSelectDepartment(val);
    if (val && val !== 'All Departments') {
      const allowed = academicPrograms.filter(
        (p) =>
          p.departmentName === val ||
          p.department?.departmentName === val
      );
      if (!allowed.some((p) => p.programName === selectProgram)) {
        setSelectProgram('');
      }
    }
  };

  const handleAddClick = () => {
    setSelectedOffering(null);
    const activeSession = academicSessions.find((s) => s.isActive);
    setFormState({
      departmentId: '',
      academicProgramId: '',
      academicSessionId: activeSession ? String(activeSession.id) : '',
      semesterId: '',
      courseId: '',
      teacherId: '',
      status: 'Active',
    });
    setShowModal(true);
  };

  const handleEdit = (row) => {
    setSelectedOffering(row);
    setFormState({
      departmentId: row.departmentId ? String(row.departmentId) : '',
      academicProgramId: row.academicProgramId ? String(row.academicProgramId) : '',
      academicSessionId: row.academicSessionId ? String(row.academicSessionId) : '',
      semesterId: row.semesterId ? String(row.semesterId) : '',
      courseId: row.courseId ? String(row.courseId) : '',
      teacherId: row.teacherId ? String(row.teacherId) : '',
      status: row.status || 'Active',
    });
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedOffering(null);
    setFormState({});
  };

  const handleFormChange = (updatedData) => {
    setFormState((prev) => {
      const next = { ...prev, ...updatedData };

      // If Department changed, clear Program, Course, Teacher if they don't match
      if (updatedData.departmentId !== prev.departmentId) {
        next.academicProgramId = '';
        next.semesterId = '';
        next.courseId = '';
        next.teacherId = '';
      }

      // If Program changed, clear Semester and Course
      if (updatedData.academicProgramId !== prev.academicProgramId) {
        next.semesterId = '';
        next.courseId = '';
      }

      // If Session changed, clear Semester
      if (updatedData.academicSessionId !== prev.academicSessionId) {
        next.semesterId = '';
      }

      return next;
    });
  };

  const handleFormSubmit = async (formData) => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const payload = {
        courseId: Number(formData.courseId),
        teacherId: formData.teacherId ? Number(formData.teacherId) : null,
        semesterId: Number(formData.semesterId),
        status: formData.status || 'Active',
      };

      if (!payload.courseId || !payload.semesterId) {
        alert('Please select Department, Academic Program, Session, Semester, and Course.');
        return;
      }

      if (selectedOffering) {
        await axios.put(
          `https://localhost:7272/api/CourseOffering/${selectedOffering.id}`,
          payload,
          { headers }
        );
      } else {
        await axios.post('https://localhost:7272/api/CourseOffering', payload, {
          headers,
        });
      }

      handleCloseModal();
      await fetchData();
    } catch (error) {
      console.log('Failed to save course offering', error);
      alert(error.response?.data?.message || 'Failed to save course assignment');
    }
  };

  const handleToggleStatus = async (row) => {
    const nextStatus = row.status === 'Active' ? 'Inactive' : 'Active';
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.patch(
        `https://localhost:7272/api/CourseOffering/${row.id}/status`,
        { status: nextStatus },
        { headers }
      );

      setOfferings((prev) =>
        prev.map((o) => (o.id === row.id ? { ...o, status: nextStatus } : o))
      );
      // Refresh workload
      const wlRes = await axios.get('https://localhost:7272/api/CourseOffering/workload', { headers });
      setWorkload(wlRes.data || []);
    } catch (error) {
      console.log('Failed to toggle status', error);
      alert(error.response?.data?.message || 'Failed to update assignment status');
    }
  };

  const handleDelete = (row) => {
    setDeleteTarget(row);
    setDeleteError('');
  };

  const handleConfirmDelete = async () => {
    if (!deleteTarget) return;
    setIsDeleting(true);
    setDeleteError('');

    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.delete(`https://localhost:7272/api/CourseOffering/${deleteTarget.id}`, {
        headers,
      });

      setOfferings((prev) => prev.filter((o) => o.id !== deleteTarget.id));
      await fetchData();
      setDeleteTarget(null);
    } catch (error) {
      console.log('Failed to delete assignment', error);
      setDeleteError(
        error.response?.data?.message ||
          'Failed to delete assignment. It may have student enrollments or attendance dependencies.'
      );
    } finally {
      setIsDeleting(false);
    }
  };

  // Stats calculation
  const totalAssignments = offerings.length;
  const activeAssignments = offerings.filter((o) => o.status === 'Active').length;
  const assignedTeacherIds = new Set(
    offerings.filter((o) => o.status === 'Active').map((o) => o.teacherId)
  );
  const teachersAssignedCount = assignedTeacherIds.size;
  const availableTeachersCount = teachers.filter(
    (t) => t.status === 'Active' && !assignedTeacherIds.has(t.id)
  ).length;

  const filterList = [
    {
      label: 'All Sessions',
      value: selectSession,
      onChange: (e) => setSelectSession(e.target.value),
      options: [
        'All Sessions',
        ...academicSessions
          .filter((s) => !s.sessionName?.startsWith('TestSession_'))
          .map((s) => s.sessionName),
      ],
    },
    {
      label: 'All Departments',
      value: selectDepartment,
      onChange: handleDepartmentFilterChange,
      options: [
        'All Departments',
        ...departments.map((d) => d.departmentName),
      ],
    },
    {
      label: 'All Academic Programs',
      value: selectProgram,
      onChange: (e) => setSelectProgram(e.target.value),
      options: [
        'All Academic Programs',
        ...availableProgramsForFilter.map((p) => p.programName),
      ],
    },
    {
      label: 'All Teachers',
      value: selectTeacher,
      onChange: (e) => setSelectTeacher(e.target.value),
      options: [
        'All Teachers',
        ...teachers.map((t) => t.teacherName),
      ],
    },
    {
      label: 'All Status',
      value: selectStatus,
      onChange: (e) => setSelectStatus(e.target.value),
      options: ['All Status', 'Active', 'Inactive'],
    },
  ];

  const columns = [
    {
      key: 'serial',
      label: '#',
    },
    {
      key: 'course',
      label: 'Course',
      render: (_, row) => (
        <div>
          <span className="font-semibold text-blue-600 bg-blue-50 px-2 py-0.5 rounded text-xs mr-2">
            {row.courseCode}
          </span>
          <span className="font-semibold text-gray-900">{row.courseName}</span>
        </div>
      ),
    },
    {
      key: 'departmentName',
      label: 'Department',
    },
    {
      key: 'programName',
      label: 'Academic Program',
      render: (prog, row) => (
        <span className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-slate-100 text-slate-700 border border-slate-200">
          {prog || row.programCode || '-'}
        </span>
      ),
    },
    {
      key: 'semesterSession',
      label: 'Semester & Session',
      render: (_, row) => (
        <div className="text-xs">
          <span className="font-semibold text-gray-800">
            Semester {row.semesterNumber}
          </span>
          {row.sessionName && (
            <span className="text-gray-500 block">{row.sessionName}</span>
          )}
        </div>
      ),
    },
    {
      key: 'teacher',
      label: 'Assigned Teacher',
      render: (_, row) => (
        <div>
          <span className={`font-medium block ${row.teacherName && row.teacherName !== '-' ? 'text-gray-900' : 'text-gray-400'}`}>
            {row.teacherName && row.teacherName !== '-' ? row.teacherName : '— (Unassigned)'}
          </span>
          {row.teacherEmail && row.teacherEmail !== '-' && (
            <span className="text-xs text-gray-500">{row.teacherEmail}</span>
          )}
        </div>
      ),
    },
    {
      key: 'status',
      label: 'Status',
      align: 'center',
      render: (status, row) => {
        const isActive = status === 'Active';
        return (
          <button
            type="button"
            onClick={() => handleToggleStatus(row)}
            title={`Click to set ${isActive ? 'Inactive' : 'Active'}`}
            className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold cursor-pointer transition-all hover:scale-105 active:scale-95 ${
              isActive
                ? 'bg-emerald-50 text-emerald-600 border border-emerald-200/60 hover:bg-emerald-100'
                : 'bg-rose-50 text-rose-600 border border-rose-200/60 hover:bg-rose-100'
            }`}
          >
            {status}
          </button>
        );
      },
    },
  ];

  // Filter pipeline
  const filteredData = offerings
    .filter((item) => {
      const search = searchTerm.toLowerCase();

      const matchSearch =
        (item.courseCode && item.courseCode.toLowerCase().includes(search)) ||
        (item.courseName && item.courseName.toLowerCase().includes(search)) ||
        (item.teacherName && item.teacherName.toLowerCase().includes(search)) ||
        (item.programName && item.programName.toLowerCase().includes(search)) ||
        (item.sessionName && item.sessionName.toLowerCase().includes(search));

      const matchSession =
        selectSession === '' ||
        selectSession === 'All Sessions' ||
        item.sessionName === selectSession;

      const matchDept =
        selectDepartment === '' ||
        selectDepartment === 'All Departments' ||
        item.departmentName === selectDepartment;

      const matchProgram =
        selectProgram === '' ||
        selectProgram === 'All Academic Programs' ||
        item.programName === selectProgram;

      const matchTeacher =
        selectTeacher === '' ||
        selectTeacher === 'All Teachers' ||
        item.teacherName === selectTeacher;

      const matchStatus =
        selectStatus === '' ||
        selectStatus === 'All Status' ||
        item.status === selectStatus;

      return matchSearch && matchSession && matchDept && matchProgram && matchTeacher && matchStatus;
    })
    .map((item, index) => ({
      ...item,
      serial: index + 1,
    }));

  // Dynamic dropdowns for Form modal
  // 1. Programs based on selected Department
  const modalPrograms = formState.departmentId
    ? academicPrograms.filter(
        (p) => String(p.departmentId) === String(formState.departmentId)
      )
    : [];

  // 2. Semesters based on selected Program AND selected Session (Active only)
  const modalSemesters = (formState.academicProgramId && formState.academicSessionId)
    ? semesters.filter((s) => {
        const matchProg = String(s.academicProgramId) === String(formState.academicProgramId);
        const matchSess = String(s.academicSessionId) === String(formState.academicSessionId);
        const matchStatus = s.status
          ? s.status === 'Active' || String(s.id) === String(formState.semesterId)
          : true;
        return matchProg && matchSess && matchStatus;
      })
    : [];

  // 3. Courses mapped to selected Academic Program
  const modalCourses = formState.academicProgramId
    ? courses.filter((c) =>
        (c.academicPrograms || []).some(
          (ap) => String(ap.id) === String(formState.academicProgramId)
        )
      )
    : [];

  // 4. Teachers belonging to selected Department (Active only)
  const modalTeachers = formState.departmentId
    ? teachers.filter((t) => {
        const active = t.status === 'Active';
        const deptMatch = String(t.departmentId) === String(formState.departmentId);
        return active && deptMatch;
      })
    : [];

  const dropdownFields = [
    {
      name: 'departmentId',
      label: 'Department',
      placeholder: 'Select Department',
      searchPlaceholder: 'Search Department...',
      required: true,
      colSpan: 1,
      options: departments.map((d) => ({
        label: d.departmentName,
        value: String(d.id),
      })),
    },
    {
      name: 'academicProgramId',
      label: 'Academic Program',
      placeholder: !formState.departmentId
        ? 'Select Department First'
        : (modalPrograms.length === 0 ? 'No programs in department' : 'Select Academic Program'),
      searchPlaceholder: 'Search Program...',
      required: true,
      colSpan: 1,
      disabled: !formState.departmentId || modalPrograms.length === 0,
      options: modalPrograms.map((p) => ({
        label: `${p.programName} (${p.programCode || ''})`,
        value: String(p.id),
      })),
    },
    {
      name: 'academicSessionId',
      label: 'Academic Session',
      placeholder: 'Select Academic Session',
      searchPlaceholder: 'Search Session...',
      required: true,
      colSpan: 1,
      options: academicSessions
        .filter((s) => !s.sessionName?.startsWith('TestSession_') && (s.isActive || String(s.id) === String(formState.academicSessionId)))
        .map((s) => ({
          label: `${s.sessionName}${s.isActive ? ' (Active)' : ''}`,
          value: String(s.id),
        })),
    },
    {
      name: 'semesterId',
      label: 'Semester',
      placeholder: !formState.academicProgramId
        ? 'Select Program First'
        : (!formState.academicSessionId
          ? 'Select Session First'
          : (modalSemesters.length === 0 ? 'No active semesters found' : 'Select Semester')),
      searchPlaceholder: 'Search Semester...',
      required: true,
      colSpan: 1,
      disabled: !formState.academicProgramId || !formState.academicSessionId || modalSemesters.length === 0,
      options: modalSemesters.map((s) => ({
        label: `Semester ${s.semesterNumber} - ${s.programName || ''} (${s.sessionName || ''})`,
        value: String(s.id),
      })),
    },
    {
      name: 'courseId',
      label: 'Course',
      placeholder: !formState.academicProgramId
        ? 'Select Program First'
        : (modalCourses.length === 0 ? 'No courses mapped to program' : 'Select Course'),
      searchPlaceholder: 'Search Course...',
      required: true,
      colSpan: 1,
      disabled: !formState.academicProgramId || modalCourses.length === 0,
      options: modalCourses.map((c) => ({
        label: `${c.courseCode} - ${c.courseName}`,
        value: String(c.id),
      })),
    },
    {
      name: 'teacherId',
      label: 'Assigned Teacher (Optional)',
      placeholder: !formState.departmentId
        ? 'Select Department First'
        : 'Select Teacher (Optional)',
      searchPlaceholder: 'Search Teacher...',
      required: false,
      colSpan: 1,
      disabled: !formState.departmentId,
      options: [
        { label: '— No Teacher Assigned —', value: '' },
        ...modalTeachers.map((t) => ({
          label: `${t.teacherName} (${t.departmentName || 'Dept'})`,
          value: String(t.id),
        })),
      ],
    },
    {
      name: 'status',
      label: 'Status',
      placeholder: 'Select Status',
      required: true,
      colSpan: 2,
      options: [
        { label: 'Active', value: 'Active' },
        { label: 'Inactive', value: 'Inactive' },
      ],
    },
  ];

  return (
    <Layout>
      <div className="p-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 mb-6">
          <div>
            <h1 className="text-2xl font-bold text-gray-900 tracking-tight">
              Course Assignments
            </h1>
            <p className="text-sm text-gray-500 mt-1">
              Assign active teachers to courses offered in academic semesters
            </p>
          </div>

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => setShowWorkloadView(!showWorkloadView)}
              className="inline-flex items-center gap-2 px-3.5 py-2.5 bg-white border border-gray-200 text-gray-700 text-sm font-semibold rounded-xl hover:bg-gray-50 cursor-pointer shadow-2xs transition-all"
            >
              <Briefcase size={16} className="text-blue-600" />
              <span>Teacher Workload</span>
              {showWorkloadView ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
            </button>

            <button
              type="button"
              onClick={handleAddClick}
              className="inline-flex items-center gap-2 px-4 py-2.5 bg-blue-600 text-white text-sm font-semibold rounded-xl hover:bg-blue-700 cursor-pointer shadow-sm hover:shadow transition-all"
            >
              <Plus size={18} />
              <span>Assign Teacher</span>
            </button>
          </div>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <StatCard
            Title="Total Assignments"
            Value={totalAssignments}
            Icon={BookOpen}
            iconBg="bg-blue-100/70"
            iconcolor="text-blue-600"
          />
          <StatCard
            Title="Active Assignments"
            Value={activeAssignments}
            Icon={CheckCircle2}
            iconBg="bg-emerald-100/70"
            iconcolor="text-emerald-600"
          />
          <StatCard
            Title="Assigned Teachers"
            Value={teachersAssignedCount}
            Icon={UserCheck}
            iconBg="bg-indigo-100/70"
            iconcolor="text-indigo-600"
          />
          <StatCard
            Title="Available Teachers"
            Value={availableTeachersCount}
            Icon={Users}
            iconBg="bg-amber-100/70"
            iconcolor="text-amber-600"
          />
        </div>

        {/* Collapsible Teacher Workload Section */}
        {showWorkloadView && (
          <div className="mb-6 bg-white border border-gray-200 rounded-2xl p-5 shadow-sm">
            <div className="flex items-center justify-between mb-4">
              <div>
                <h2 className="text-base font-bold text-gray-900">
                  Teacher Workload Overview
                </h2>
                <p className="text-xs text-gray-500">
                  Distinct courses and active course offerings currently handled by each teacher
                </p>
              </div>
              <span className="text-xs font-semibold px-2.5 py-1 bg-blue-50 text-blue-700 rounded-lg">
                {workload.length} Teachers
              </span>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs text-gray-700">
                <thead className="bg-gray-50 text-gray-600 font-semibold border-b border-gray-100">
                  <tr>
                    <th className="px-4 py-2.5">Teacher</th>
                    <th className="px-4 py-2.5">Department</th>
                    <th className="px-4 py-2.5 text-center">Distinct Courses</th>
                    <th className="px-4 py-2.5 text-center">Active Offerings</th>
                    <th className="px-4 py-2.5 text-center">Total Assigned</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {workload.map((w) => (
                    <tr key={w.teacherId} className="hover:bg-gray-50/60">
                      <td className="px-4 py-2.5 font-medium text-gray-900">
                        {w.teacherName}
                        {w.teacherEmail && (
                          <span className="text-gray-400 block text-[11px]">
                            {w.teacherEmail}
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-2.5 text-gray-600">
                        {w.departmentName || '-'}
                      </td>
                      <td className="px-4 py-2.5 text-center">
                        <span className="inline-flex items-center px-2 py-0.5 rounded-md font-semibold bg-blue-50 text-blue-700 border border-blue-200">
                          {w.distinctCoursesCount}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-center">
                        <span className="inline-flex items-center px-2 py-0.5 rounded-md font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                          {w.activeOfferingsCount}
                        </span>
                      </td>
                      <td className="px-4 py-2.5 text-center text-gray-500 font-medium">
                        {w.totalOfferingsCount}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        )}

        {/* FilterBar */}
        <FilterBar
          searchValue={searchTerm}
          onSearchChange={(e) => setSearchTerm(e.target.value)}
          searchPlaceholder="Search by course, teacher, program, session..."
          filterList={filterList}
        />

        {/* DataTable */}
        <DataTable
          title="Course Assignments"
          count={filteredData.length}
          columns={columns}
          data={filteredData}
          onEdit={handleEdit}
          onDelete={handleDelete}
          pagination={true}
          pageSize={10}
          currentPage={currentPage}
          onPageChange={(page) => setCurrentPage(page)}
        />

        {/* Add/Edit Modal */}
        {showModal && (
          <Form
            title={selectedOffering ? 'Edit Course Assignment' : 'Assign Teacher to Course'}
            subtitle={
              selectedOffering
                ? 'Update teacher or semester assignment'
                : 'Select Department, Academic Program, Session, Semester, Course, and Teacher'
            }
            dropdownFields={dropdownFields}
            initialValues={formState}
            onFormChange={handleFormChange}
            onClose={handleCloseModal}
            onSubmit={handleFormSubmit}
            primaryButtonText={selectedOffering ? 'Update Assignment' : 'Assign Teacher'}
          />
        )}

        {/* Delete Confirmation Modal */}
        <DeleteConfirmationModal
          isOpen={!!deleteTarget}
          title="Delete Course Assignment"
          itemName={deleteTarget ? `${deleteTarget.courseName} assigned to ${deleteTarget.teacherName}` : ''}
          isDeleting={isDeleting}
          errorMessage={deleteError}
          onConfirm={handleConfirmDelete}
          onClose={() => {
            if (!isDeleting) {
              setDeleteTarget(null);
              setDeleteError('');
            }
          }}
        />
      </div>
    </Layout>
  );
};

export default CourseAssignments;

