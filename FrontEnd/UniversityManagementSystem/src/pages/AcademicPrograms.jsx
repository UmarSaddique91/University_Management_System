import React, { useState, useEffect } from 'react';
import Layout from '../layout/Layout';
import FilterBar from '../components/common/FilterBar';
import DataTable from '../components/common/DataTable';
import Form from '../components/common/Form';
import {
  Plus,
  ChevronDown,
  ChevronUp,
  Layers,
  Calendar,
  Users,
  UserCheck,
  BookOpen,
  Pencil,
  Trash2,
  CheckCircle2,
  AlertCircle
} from 'lucide-react';
import axios from 'axios';

const AcademicPrograms = () => {
  const [programs, setPrograms] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [academicSessions, setAcademicSessions] = useState([]);
  const [selectedSessionId, setSelectedSessionId] = useState('');
  const [expandedProgramId, setExpandedProgramId] = useState(null);
  const [expandedSemesters, setExpandedSemesters] = useState([]);
  const [semesterLoading, setSemesterLoading] = useState(false);

  const [searchTerm, setSearchTerm] = useState('');
  const [selectDepartment, setSelectDepartment] = useState('');
  const [currentPage, setCurrentPage] = useState(1);

  // Modal states
  const [showProgramModal, setShowProgramModal] = useState(false);
  const [selectedProgram, setSelectedProgram] = useState(null);

  const [showSemesterModal, setShowSemesterModal] = useState(false);
  const [selectedSemester, setSelectedSemester] = useState(null);
  const [activeProgramForSemester, setActiveProgramForSemester] = useState(null);

  const fetchInitialData = async () => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const [programsRes, deptsRes, sessionsRes] = await Promise.all([
        axios.get('https://localhost:7272/api/AcademicProgram', { headers }),
        axios.get('https://localhost:7272/api/Department', { headers }),
        axios.get('https://localhost:7272/api/AcademicSession', { headers }),
      ]);

      setPrograms(programsRes.data || []);
      setDepartments(deptsRes.data || []);
      const sessList = sessionsRes.data || [];
      setAcademicSessions(sessList);

      // Default to the active session if available, else first session
      const activeSess = sessList.find((s) => s.isActive) || sessList[0];
      if (activeSess) {
        setSelectedSessionId(String(activeSess.id));
      }
    } catch (error) {
      console.log('Failed to fetch initial data', error);
    }
  };

  useEffect(() => {
    fetchInitialData();
  }, []);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, selectDepartment]);

  // Fetch semesters when a program expands or session changes
  const fetchSemestersForProgram = async (progId, sessId) => {
    if (!progId) return;
    setSemesterLoading(true);
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};
      const params = { programId: progId };
      if (sessId) params.sessionId = sessId;

      const res = await axios.get('https://localhost:7272/api/Semester', {
        headers,
        params,
      });
      setExpandedSemesters(res.data || []);
    } catch (err) {
      console.log('Failed to fetch semesters for program', err);
      setExpandedSemesters([]);
    } finally {
      setSemesterLoading(false);
    }
  };

  const toggleProgramExpand = async (progId) => {
    if (expandedProgramId === progId) {
      setExpandedProgramId(null);
      setExpandedSemesters([]);
    } else {
      setExpandedProgramId(progId);
      await fetchSemestersForProgram(progId, selectedSessionId);
    }
  };

  const handleSessionChange = async (newSessionId) => {
    setSelectedSessionId(newSessionId);
    if (expandedProgramId) {
      await fetchSemestersForProgram(expandedProgramId, newSessionId);
    }
    // Also refresh program counters for the newly selected session
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};
      const res = await axios.get('https://localhost:7272/api/AcademicProgram', {
        headers,
        params: { sessionId: newSessionId },
      });
      setPrograms(res.data || []);
    } catch (err) {
      console.log('Failed to refresh programs count', err);
    }
  };

  // Program Add / Edit handlers
  const handleAddProgram = () => {
    setSelectedProgram(null);
    setShowProgramModal(true);
  };

  const handleEditProgram = (row) => {
    setSelectedProgram(row);
    setShowProgramModal(true);
  };

  const handleCloseProgramModal = () => {
    setShowProgramModal(false);
    setSelectedProgram(null);
  };

  const handleProgramSubmit = async (formData) => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const payload = {
        programName: formData.programName?.trim(),
        programCode: formData.programCode?.trim(),
        durationInSemesters: formData.durationInSemesters
          ? Number(formData.durationInSemesters)
          : null,
        departmentId: formData.departmentId
          ? Number(formData.departmentId)
          : null,
      };

      if (selectedProgram) {
        await axios.put(
          `https://localhost:7272/api/AcademicProgram/${selectedProgram.dbId}`,
          payload,
          { headers }
        );
      } else {
        await axios.post('https://localhost:7272/api/AcademicProgram', payload, {
          headers,
        });
      }

      handleCloseProgramModal();
      await fetchInitialData();
    } catch (error) {
      console.log('Failed to save academic program', error);
      alert(error.response?.data?.message || 'Failed to save academic program');
    }
  };

  const handleDeleteProgram = async (row) => {
    const isConfirmed = window.confirm(
      `Are you sure you want to delete ${row.programName || 'this program'} (${row.programCode})?`
    );
    if (!isConfirmed) return;

    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.delete(
        `https://localhost:7272/api/AcademicProgram/${row.dbId}`,
        { headers }
      );

      setPrograms((prev) => {
        const updated = prev.filter((p) => p.id !== row.dbId);
        const newTotalPages = Math.max(1, Math.ceil(updated.length / 10));
        setCurrentPage((curr) => Math.min(curr, newTotalPages));
        return updated;
      });
      if (expandedProgramId === row.dbId) {
        setExpandedProgramId(null);
        setExpandedSemesters([]);
      }
    } catch (error) {
      console.log('Failed to delete academic program', error);
      alert(error.response?.data?.message || 'Failed to delete academic program');
    }
  };

  // Semester Management handlers
  const handleAddSemester = (prog) => {
    setActiveProgramForSemester(prog);
    setSelectedSemester(null);
    setShowSemesterModal(true);
  };

  const handleEditSemester = (prog, sem) => {
    setActiveProgramForSemester(prog);
    setSelectedSemester(sem);
    setShowSemesterModal(true);
  };

  const handleCloseSemesterModal = () => {
    setShowSemesterModal(false);
    setSelectedSemester(null);
    setActiveProgramForSemester(null);
  };

  const handleSemesterSubmit = async (formData) => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const payload = {
        semesterNumber: Number(formData.semesterNumber),
        academicProgramId: activeProgramForSemester.dbId || activeProgramForSemester.id,
        academicSessionId: Number(formData.academicSessionId || selectedSessionId),
        status: formData.status || 'Active',
      };

      if (!payload.semesterNumber || !payload.academicSessionId) {
        alert('Please fill in all required fields.');
        return;
      }

      if (selectedSemester) {
        await axios.put(
          `https://localhost:7272/api/Semester/${selectedSemester.id}`,
          payload,
          { headers }
        );
      } else {
        await axios.post('https://localhost:7272/api/Semester', payload, {
          headers,
        });
      }

      handleCloseSemesterModal();
      // Refresh nested semesters
      await fetchSemestersForProgram(
        activeProgramForSemester.dbId || activeProgramForSemester.id,
        selectedSessionId
      );
      // Refresh main program counters
      const progRes = await axios.get('https://localhost:7272/api/AcademicProgram', {
        headers,
        params: { sessionId: selectedSessionId },
      });
      setPrograms(progRes.data || []);
    } catch (error) {
      console.log('Failed to save semester', error);
      alert(error.response?.data?.message || 'Failed to save semester');
    }
  };

  const handleToggleSemesterStatus = async (prog, sem) => {
    const nextStatus = sem.status === 'Active' ? 'Inactive' : 'Active';
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.patch(
        `https://localhost:7272/api/Semester/${sem.id}/status`,
        { status: nextStatus },
        { headers }
      );

      setExpandedSemesters((prev) =>
        prev.map((s) => (s.id === sem.id ? { ...s, status: nextStatus } : s))
      );
    } catch (error) {
      console.log('Failed to toggle semester status', error);
      alert(error.response?.data?.message || 'Failed to update semester status');
    }
  };

  const handleDeleteSemester = async (prog, sem) => {
    const isConfirmed = window.confirm(
      `Are you sure you want to delete Semester ${sem.semesterNumber}?`
    );
    if (!isConfirmed) return;

    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.delete(`https://localhost:7272/api/Semester/${sem.id}`, {
        headers,
      });

      // Refresh nested semesters & program counts
      await fetchSemestersForProgram(prog.dbId || prog.id, selectedSessionId);
      const progRes = await axios.get('https://localhost:7272/api/AcademicProgram', {
        headers,
        params: { sessionId: selectedSessionId },
      });
      setPrograms(progRes.data || []);
    } catch (error) {
      console.log('Failed to delete semester', error);
      alert(
        error.response?.data?.message ||
          'Failed to delete semester. It may have active course offerings, enrollments, attendance, exams, or grades. Deactivate it instead.'
      );
    }
  };

  const currentDate = new Date().toLocaleDateString('en-GB', {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  });

  const filterList = [
    {
      label: 'All Departments',
      value: selectDepartment,
      onChange: (e) => setSelectDepartment(e.target.value),
      options: [
        'All Departments',
        ...departments.map((d) => d.departmentName),
      ],
    },
  ];

  const columns = [
    {
      key: 'serial',
      label: '#',
    },
    {
      key: 'programCode',
      label: 'Program Code',
      render: (code) => (
        <span className="font-semibold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-md text-xs">
          {code}
        </span>
      ),
    },
    {
      key: 'programName',
      label: 'Program Name',
      render: (name) => <span className="font-medium text-gray-900">{name}</span>,
    },
    {
      key: 'departmentName',
      label: 'Department',
    },
    {
      key: 'semesters',
      label: 'Semesters',
      align: 'center',
      render: (_, row) => {
        const isExpanded = expandedProgramId === row.dbId;
        const configured = row.configuredSemestersCount ?? 0;
        const total = row.durationInSemesters ?? 8;
        const isComplete = configured >= total;

        return (
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              toggleProgramExpand(row.dbId);
            }}
            className={`inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all cursor-pointer border ${
              isExpanded
                ? 'bg-blue-600 text-white border-blue-600 shadow-sm'
                : isComplete
                ? 'bg-emerald-50 text-emerald-700 border-emerald-200 hover:bg-emerald-100'
                : 'bg-amber-50 text-amber-700 border-amber-200 hover:bg-amber-100'
            }`}
            title="Click to manage semesters for this program"
          >
            <Layers className="w-3.5 h-3.5" />
            <span>
              {configured} / {total} Semesters
            </span>
            {isExpanded ? (
              <ChevronUp className="w-3.5 h-3.5 ml-0.5" />
            ) : (
              <ChevronDown className="w-3.5 h-3.5 ml-0.5" />
            )}
          </button>
        );
      },
    },
  ];

  const programsData = programs.map((item) => ({
    dbId: item.id,
    id: item.id,
    programName: item.programName || '-',
    programCode: item.programCode || '-',
    durationInSemesters: item.durationInSemesters ?? 8,
    departmentId: item.departmentId,
    departmentName: item.departmentName || '-',
    configuredSemestersCount: item.configuredSemestersCount ?? 0,
  }));

  const filterPrograms = programsData
    .filter((item) => {
      const search = searchTerm.toLowerCase();

      const matchSearch =
        item.programName.toLowerCase().includes(search) ||
        item.programCode.toLowerCase().includes(search) ||
        item.departmentName.toLowerCase().includes(search);

      const matchDepartment =
        selectDepartment === '' ||
        selectDepartment === 'All Departments' ||
        item.departmentName === selectDepartment;

      return matchSearch && matchDepartment;
    })
    .map((item, index) => ({
      ...item,
      serial: index + 1,
    }));

  // Render the expanded nested Semester Management section
  const renderExpandedRow = (program) => {
    return (
      <div className="p-5 bg-gradient-to-b from-slate-50 to-slate-100/70 rounded-b-lg border-t border-blue-100 shadow-inner">
        {/* Top bar: Info, Session Selector, and Add Semester */}
        <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-4 border-b border-gray-200/80">
          <div>
            <div className="flex items-center gap-2">
              <span className="text-xs font-bold uppercase tracking-wider text-blue-600 bg-blue-50 px-2 py-0.5 rounded border border-blue-200">
                Semester Management
              </span>
              <h3 className="text-sm font-bold text-gray-900">
                {program.programName} ({program.programCode})
              </h3>
            </div>
            <p className="text-xs text-gray-500 mt-1">
              Configure semesters, track student enrollments, assigned teachers, and course offerings.
            </p>
          </div>

          <div className="flex items-center gap-3">
            {/* Academic Session Selector */}
            <div className="flex items-center gap-1.5 bg-white px-2.5 py-1.5 rounded-lg border border-gray-200 shadow-2xs">
              <Calendar className="w-3.5 h-3.5 text-gray-500" />
              <label className="text-xs font-medium text-gray-600">Session:</label>
              <select
                value={selectedSessionId}
                onChange={(e) => handleSessionChange(e.target.value)}
                className="text-xs font-semibold text-gray-800 bg-transparent focus:outline-none cursor-pointer"
              >
                {academicSessions.map((sess) => (
                  <option key={sess.id} value={sess.id}>
                    {sess.sessionName} {sess.isActive ? '(Active)' : ''}
                  </option>
                ))}
              </select>
            </div>

            {/* Add Semester Button */}
            <button
              type="button"
              onClick={() => handleAddSemester(program)}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-blue-600 hover:bg-blue-700 text-white rounded-lg text-xs font-semibold shadow-2xs transition-colors cursor-pointer"
            >
              <Plus className="w-3.5 h-3.5" />
              Add Semester
            </button>
          </div>
        </div>

        {/* Nested Semesters Table */}
        <div className="mt-4 bg-white rounded-xl border border-gray-200 shadow-2xs overflow-hidden">
          {semesterLoading ? (
            <div className="p-8 text-center text-xs text-gray-500">
              Loading semester details...
            </div>
          ) : expandedSemesters.length === 0 ? (
            <div className="p-8 text-center">
              <AlertCircle className="w-8 h-8 text-amber-500 mx-auto mb-2" />
              <p className="text-xs font-semibold text-gray-700">
                No semester records found for this program in the selected session.
              </p>
              <p className="text-xs text-gray-500 mt-1">
                Click "+ Add Semester" above to configure Semester 1 up to Semester {program.durationInSemesters}.
              </p>
            </div>
          ) : (
            <table className="w-full text-left border-collapse text-xs">
              <thead>
                <tr className="border-b border-gray-200 bg-slate-50 text-gray-600 font-semibold uppercase tracking-wider">
                  <th className="py-2.5 px-3.5">#</th>
                  <th className="py-2.5 px-3.5">Semester</th>
                  <th className="py-2.5 px-3.5 text-center">Enrolled Students</th>
                  <th className="py-2.5 px-3.5 text-center">Assigned Teachers</th>
                  <th className="py-2.5 px-3.5 text-center">Courses Offered</th>
                  <th className="py-2.5 px-3.5 text-center">Status</th>
                  <th className="py-2.5 px-3.5 text-center">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {expandedSemesters.map((sem, sIdx) => {
                  const isActive = sem.status === 'Active';
                  return (
                    <tr key={sem.id} className="hover:bg-slate-50/70 transition-colors">
                      <td className="py-2.5 px-3.5 text-gray-500">{sIdx + 1}</td>
                      <td className="py-2.5 px-3.5 font-bold text-gray-900">
                        Semester {sem.semesterNumber}
                      </td>
                      <td className="py-2.5 px-3.5 text-center">
                        <span
                          className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full font-semibold text-xs bg-indigo-50 text-indigo-700 border border-indigo-200"
                          title={`Enrolled: ${sem.studentCount} student(s)${
                            sem.profileStudentCount !== undefined
                              ? ` | Program-profile: ${sem.profileStudentCount}`
                              : ''
                          }`}
                        >
                          <Users className="w-3 h-3 text-indigo-500" />
                          {sem.studentCount} Students
                        </span>
                      </td>
                      <td className="py-2.5 px-3.5 text-center">
                        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full font-semibold text-xs bg-purple-50 text-purple-700 border border-purple-200">
                          <UserCheck className="w-3 h-3 text-purple-500" />
                          {sem.assignedTeachersCount} Teachers
                        </span>
                      </td>
                      <td className="py-2.5 px-3.5 text-center">
                        <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full font-semibold text-xs bg-blue-50 text-blue-700 border border-blue-200">
                          <BookOpen className="w-3 h-3 text-blue-500" />
                          {sem.courseCount} Courses
                        </span>
                      </td>
                      <td className="py-2.5 px-3.5 text-center">
                        <button
                          type="button"
                          onClick={() => handleToggleSemesterStatus(program, sem)}
                          className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold cursor-pointer border transition-colors ${
                            isActive
                              ? 'bg-emerald-50 text-emerald-700 border-emerald-200 hover:bg-emerald-100'
                              : 'bg-gray-100 text-gray-600 border-gray-300 hover:bg-gray-200'
                          }`}
                          title={`Click to set ${isActive ? 'Inactive' : 'Active'}`}
                        >
                          {isActive ? (
                            <CheckCircle2 className="w-3 h-3 text-emerald-500" />
                          ) : (
                            <span className="w-2 h-2 rounded-full bg-gray-400 mr-0.5" />
                          )}
                          {sem.status || 'Active'}
                        </button>
                      </td>
                      <td className="py-2.5 px-3.5 text-center">
                        <div className="flex items-center justify-center gap-1.5">
                          <button
                            type="button"
                            onClick={() => handleEditSemester(program, sem)}
                            title="Edit Semester"
                            className="p-1 bg-blue-50 text-blue-600 hover:bg-blue-100 rounded transition-colors cursor-pointer"
                          >
                            <Pencil className="w-3.5 h-3.5" />
                          </button>
                          <button
                            type="button"
                            onClick={() => handleDeleteSemester(program, sem)}
                            title="Delete Semester"
                            className="p-1 bg-red-50 text-red-600 hover:bg-red-100 rounded transition-colors cursor-pointer"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          )}
        </div>
      </div>
    );
  };

  // Program Modal Config
  const programInputFields = [
    {
      name: 'programName',
      label: 'Program Name',
      type: 'text',
      placeholder: 'e.g. BS Computer Science',
      required: true,
      colSpan: 2,
    },
    {
      name: 'programCode',
      label: 'Program Code',
      type: 'text',
      placeholder: 'e.g. BSCS',
      required: true,
      colSpan: 1,
    },
    {
      name: 'durationInSemesters',
      label: 'Duration In Semesters',
      type: 'number',
      placeholder: 'e.g. 8',
      required: true,
      colSpan: 1,
    },
  ];

  const programDropdownFields = [
    {
      name: 'departmentId',
      label: 'Department',
      placeholder: 'Select Department',
      searchPlaceholder: 'Search Department...',
      required: true,
      colSpan: 2,
      options: departments.map((item) => ({
        label: item.departmentName,
        value: item.id,
      })),
    },
  ];

  const programInitialValues = selectedProgram
    ? {
        programName:
          selectedProgram.programName !== '-' ? selectedProgram.programName : '',
        programCode:
          selectedProgram.programCode !== '-' ? selectedProgram.programCode : '',
        durationInSemesters: selectedProgram.durationInSemesters ?? 8,
        departmentId: selectedProgram.departmentId,
      }
    : {};

  // Semester Modal Config
  const maxSemesters =
    activeProgramForSemester?.durationInSemesters &&
    activeProgramForSemester.durationInSemesters > 0
      ? activeProgramForSemester.durationInSemesters
      : 8;

  const hasDeps = !!selectedSemester?.hasDependencies;

  const semesterInputFields = [
    {
      name: 'semesterNumber',
      label: 'Semester Number',
      type: 'number',
      placeholder: `1 - ${maxSemesters}`,
      required: true,
      colSpan: 1,
      disabled: hasDeps,
      helperText: hasDeps
        ? 'Locked because this semester has active course offerings or academic records.'
        : `Allowed range: 1 to ${maxSemesters}.`,
    },
  ];

  const semesterDropdownFields = [
    {
      name: 'academicSessionId',
      label: 'Academic Session',
      placeholder: 'Select Academic Session',
      required: true,
      colSpan: 1,
      disabled: hasDeps,
      helperText: hasDeps ? 'Locked due to dependent records.' : '',
      options: academicSessions.map((s) => ({
        label: `${s.sessionName}${s.isActive ? ' (Active)' : ''}`,
        value: s.id,
      })),
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

  const semesterInitialValues = selectedSemester
    ? {
        semesterNumber: selectedSemester.semesterNumber,
        academicSessionId: selectedSemester.academicSessionId,
        status: selectedSemester.status || 'Active',
      }
    : {
        semesterNumber: '',
        academicSessionId: selectedSessionId ? Number(selectedSessionId) : '',
        status: 'Active',
      };

  return (
    <Layout>
      <div className="space-y-4">
        {/* Page Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Academic Programs</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              Manage academic programs, departments, and integrated semester structures.
            </p>
          </div>
          <div className="text-sm font-medium text-gray-600 sm:text-right">
            {currentDate}
          </div>
        </div>

        {/* Filters and Search Bar */}
        <FilterBar
          searchPlaceholder="Search by program name, code, or department..."
          searchValue={searchTerm}
          onSearchChange={(e) => setSearchTerm(e.target.value)}
          filters={filterList}
          actionButton={{
            label: 'Add Program',
            icon: Plus,
            onClick: handleAddProgram,
          }}
        />

        {/* Academic Programs DataTable with Expandable Rows */}
        <DataTable
          title="All Programs"
          count={filterPrograms.length}
          columns={columns}
          data={filterPrograms}
          showActions={true}
          onEdit={handleEditProgram}
          onDelete={handleDeleteProgram}
          pagination={true}
          pageSize={10}
          currentPage={currentPage}
          onPageChange={setCurrentPage}
          expandable={true}
          expandedRowId={expandedProgramId}
          onRowExpand={toggleProgramExpand}
          renderExpandedRow={renderExpandedRow}
          rowKey="dbId"
        />

        {/* Modal for Program Add & Edit */}
        {showProgramModal && (
          <Form
            title={selectedProgram ? 'Edit Academic Program' : 'Add Academic Program'}
            subtitle={
              selectedProgram
                ? 'Update academic program details and duration.'
                : 'Create a new degree program under a department.'
            }
            primaryButtonText={selectedProgram ? 'Save Changes' : 'Add Program'}
            cancelButtonText="Cancel"
            initialValues={programInitialValues}
            onClose={handleCloseProgramModal}
            onSubmit={handleProgramSubmit}
            inputFields={programInputFields}
            dropdownFields={programDropdownFields}
          />
        )}

        {/* Modal for Semester Add & Edit */}
        {showSemesterModal && (
          <Form
            title={
              selectedSemester
                ? `Edit Semester ${selectedSemester.semesterNumber} — ${activeProgramForSemester?.programName}`
                : `Add Semester — ${activeProgramForSemester?.programName}`
            }
            subtitle={
              selectedSemester
                ? 'Modify semester status. Semester number and session are locked if dependencies exist.'
                : `Configure a new semester for ${activeProgramForSemester?.programName}.`
            }
            primaryButtonText={selectedSemester ? 'Save Changes' : 'Add Semester'}
            cancelButtonText="Cancel"
            initialValues={semesterInitialValues}
            onClose={handleCloseSemesterModal}
            onSubmit={handleSemesterSubmit}
            inputFields={semesterInputFields}
            dropdownFields={semesterDropdownFields}
          />
        )}
      </div>
    </Layout>
  );
};

export default AcademicPrograms;
