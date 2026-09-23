import React, { useEffect, useState } from 'react'
import Layout from '../layout/Layout'
import FilterBar from '../components/common/FilterBar'
import axios from 'axios'
import DataTable from '../components/common/DataTable'
import { Plus } from 'lucide-react'
import Form from '../components/common/Form'

const Students = () => {
  const [department, setDepartment] = useState([])
  const [selectDepartmet, setSelectDepartment] = useState('')

  const [academicProgram, setAcademicProgram] = useState([])
  const [selectAcademicProgram, setSelectAcademicProgram] = useState('')

  const [academicSession, setAcademicSession] = useState([])
  const [selectSession, setSelectSession] = useState('')

  const [semesters, setSemesters] = useState([])

  const [showAddStudentModel, setShowAddStudentModel] = useState(false)
  const [selectedStudent, setSelectedStudent] = useState(null)
  const [formState, setFormState] = useState({})

  const [student, setStudent] = useState([])
  const [searchTerm, setSearchTerm] = useState('')
  const [currentPage, setCurrentPage] = useState(1)

  const handleToggleStatus = async (row) => {
    if (!row.isRegistered) {
      alert("Cannot activate student: Registration has not been completed by the student yet.");
      return;
    }

    const newStatus = row.status === 'Active' ? 'Inactive' : 'Active';

    try {
      const token = localStorage.getItem("token");
      await axios.patch(
        `https://localhost:7272/api/Student/${row.dbId}/status`,
        { status: newStatus },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      setStudent((prev) =>
        prev.map((s) => (s.id === row.dbId ? { ...s, status: newStatus } : s))
      );
    } catch (error) {
      console.log("Failed to update student status", error);
      alert(error.response?.data?.message || "Failed to update student status");
    }
  };

  const handleDelete = async (row) => {
    const isConfirmed = window.confirm(`Are you sure you want to delete ${row.name || 'this student'}?`);
    if (!isConfirmed) return;

    try {
      const token = localStorage.getItem("token");

      await axios.delete(
        `https://localhost:7272/api/Student/${row.dbId}`,
        {
          headers: {
            Authorization: `Bearer ${token}`
          }
        }
      );

      setStudent(prev => {
        const updated = prev.filter(s => s.id !== row.dbId);
        const newTotalPages = Math.max(1, Math.ceil(updated.length / 10));
        setCurrentPage(curr => Math.min(curr, newTotalPages));
        return updated;
      });

    } catch (error) {
      console.log("Student Delete Failed", error);
      alert(error.response?.data?.message || "Failed to delete student");
    }
  };

  const fetchStudents = async () => {
    try {
      const token = localStorage.getItem("token");
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const [deptRes, progRes, sessRes, semRes, studRes] = await Promise.all([
        axios.get("https://localhost:7272/api/Department", { headers }),
        axios.get("https://localhost:7272/api/AcademicProgram", { headers }),
        axios.get("https://localhost:7272/api/AcademicSession", { headers }),
        axios.get("https://localhost:7272/api/Semester", { headers }),
        axios.get("https://localhost:7272/api/Student", { headers }),
      ]);

      setDepartment(deptRes.data || []);
      setAcademicProgram(progRes.data || []);
      setAcademicSession(sessRes.data || []);
      setSemesters(semRes.data || []);
      setStudent(studRes.data || []);
    } catch (error) {
      console.log("Failed to fetch students data", error);
    }
  };

  const handleAddClick = () => {
    setSelectedStudent(null);
    const activeSession = academicSession.find((s) => s.isActive);
    setFormState({
      studentName: '',
      rollNumber: '',
      departmentId: '',
      academicProgramId: '',
      academicSessionId: activeSession ? String(activeSession.id) : '',
      currentSemesterId: '',
      invitationEmail: '',
    });
    setShowAddStudentModel(true);
  };

  const handleEdit = (row) => {
    setSelectedStudent(row);
    setFormState({
      studentName: row.studentName || '',
      rollNumber: row.rollNumber || '',
      departmentId: row.departmentId ? String(row.departmentId) : '',
      academicProgramId: row.academicProgramId ? String(row.academicProgramId) : '',
      academicSessionId: row.academicSessionId ? String(row.academicSessionId) : '',
      currentSemesterId: row.currentSemesterId ? String(row.currentSemesterId) : '',
      invitationEmail: '',
    });
    setShowAddStudentModel(true);
  };

  const handleCloseModal = () => {
    setShowAddStudentModel(false);
    setSelectedStudent(null);
    setFormState({});
  };

  const handleFormChange = (updatedData) => {
    setFormState((prev) => {
      const next = { ...prev, ...updatedData };

      // If Department changed, clear Program and Semester if they don't match
      if (updatedData.departmentId !== prev.departmentId) {
        next.academicProgramId = '';
        next.currentSemesterId = '';
      }

      // If Program changed, clear Semester
      if (updatedData.academicProgramId !== prev.academicProgramId) {
        next.currentSemesterId = '';
      }

      // If Session changed, clear Semester
      if (updatedData.academicSessionId !== prev.academicSessionId) {
        next.currentSemesterId = '';
      }

      return next;
    });
  };

  const handleFormSubmit = async (formData) => {
    try {
      const token = localStorage.getItem("token");

      const payload = {
        studentName: formData.studentName?.trim(),
        rollNumber: formData.rollNumber?.trim(),
        departmentId: formData.departmentId ? Number(formData.departmentId) : null,
        academicProgramId: formData.academicProgramId ? Number(formData.academicProgramId) : null,
        academicSessionId: formData.academicSessionId ? Number(formData.academicSessionId) : null,
        currentSemesterId: formData.currentSemesterId ? Number(formData.currentSemesterId) : null,
        invitationEmail: formData.invitationEmail?.trim() || null,
      };

      if (selectedStudent) {
        // Edit Mode: PUT /api/Student/{id}
        await axios.put(
          `https://localhost:7272/api/Student/${selectedStudent.dbId}`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
      } else {
        // Create Mode: POST /api/Student
        await axios.post("https://localhost:7272/api/Student", payload, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
      }

      handleCloseModal();
      await fetchStudents();
    } catch (error) {
      console.log("Failed to save student", error);
      alert(error.response?.data?.message || "Failed to save student");
    }
  };

  useEffect(() => {
    fetchStudents();
  }, []);



  //User Should navigate to page one when filter applies

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, selectDepartmet, selectAcademicProgram, selectSession]);

  // Dropdown options based on dependencies
  // 1. Programs based on selected Department
  const modalPrograms = formState.departmentId
    ? academicProgram.filter(
        (p) => String(p.departmentId) === String(formState.departmentId)
      )
    : academicProgram;

  // 2. Semesters based on selected Program AND selected Session (Active only)
  const modalSemesters = semesters.filter((s) => {
    const matchProg = formState.academicProgramId
      ? String(s.academicProgramId) === String(formState.academicProgramId)
      : false;
    const matchSess = formState.academicSessionId
      ? String(s.academicSessionId) === String(formState.academicSessionId)
      : true;
    const matchStatus = s.status
      ? s.status === 'Active' || String(s.id) === String(formState.currentSemesterId)
      : true;
    return matchProg && matchSess && matchStatus;
  });

  const filterList = [
    {
      label: "All Departments",
      value: selectDepartmet,
      onChange: (e) => setSelectDepartment(e.target.value),
      options: [
        "All Departments",
        ...department.map((d) => d.departmentName),
      ],
    },
    {
      label: "All Programs",
      value: selectAcademicProgram,
      onChange: (e) => setSelectAcademicProgram(e.target.value),
      options: [
        "All Programs",
        ...academicProgram.map((d) => d.programName),
      ],
    },
    {
      label: "All Sessions",
      value: selectSession,
      onChange: (e) => setSelectSession(e.target.value),
      options: [
        "All Sessions",
        ...academicSession
          .filter((s) => !s.sessionName?.startsWith('TestSession_'))
          .map((s) => s.sessionName),
      ],
    },
  ];

  const currentDate = new Date().toLocaleDateString('en-GB', {
    weekday: "long",
    day: "2-digit",
    month: "long",
    year: "numeric"
  });

  const columns = [
    {
      key: 'serial',
      label: '#',
    },
    {
      key: 'name',
      label: 'Student',
      render: (name) => (
        <span className="font-medium text-gray-900">{name}</span>
      ),
    },
    {
      key: 'RollNo',
      label: 'Roll Number',
    },
    {
      key: 'dept',
      label: 'Department',
    },
    {
      key: 'email',
      label: 'Email',
    },
    {
      key: 'program',
      label: 'Program',
      align: 'center',
    },
    {
      key: 'semester',
      label: 'Semester',
      render: (_, row) => (
        <div className="text-xs">
          <span className="font-semibold text-gray-800">
            {row.semesterNumber ? `Semester ${row.semesterNumber}` : '-'}
          </span>
          {row.sessionName && (
            <span className="text-gray-500 block">{row.sessionName}</span>
          )}
        </div>
      ),
    },
    {
      key: 'status',
      label: 'Status',
      render: (status, row) => {
        const isActive = status === 'Active';
        const isRegistered = row?.isRegistered;

        if (!isRegistered) {
          return (
            <button
              type="button"
              onClick={() => handleToggleStatus(row)}
              title="Pending Registration - Student cannot be activated until they complete registration"
              className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-600 border border-rose-200/60 cursor-not-allowed opacity-80"
            >
              Inactive
            </button>
          );
        }

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

  const studentsData = student.map((item) => ({
    dbId: item.id,
    id: item.id || '-',
    name: item.studentName || '-',
    studentName: item.studentName || '',
    RollNo: item.rollNumber || '-',
    rollNumber: item.rollNumber || '',
    dept: item.departmentName || '-',
    departmentId: item.departmentId,
    email: item.email || '-',
    program: item.programName || '-',
    academicProgramId: item.academicProgramId,
    academicSessionId: item.academicSessionId,
    sessionName: item.sessionName || '',
    currentSemesterId: item.currentSemesterId,
    semesterNumber: item.semesterNumber || item.semester || '',
    status: item.status || 'Inactive',
    userId: item.userId,
    isRegistered: Boolean(item.userId && item.email),
  }));




  //Filtering While Searching

  const filterStudent = studentsData
    .filter((item) => {
      const search = searchTerm.toLowerCase();

      const matchSearch =
        item.name.toLowerCase().includes(search) ||
        item.RollNo.toLowerCase().includes(search) ||
        item.email.toLowerCase().includes(search);

      const matchDepartment =
        selectDepartmet === '' ||
        selectDepartmet === "All Departments" ||
        item.dept === selectDepartmet;

      const matchPrograms =
        selectAcademicProgram === '' ||
        selectAcademicProgram === "All Programs" ||
        item.program === selectAcademicProgram;

      const matchSession =
        selectSession === '' ||
        selectSession === "All Sessions" ||
        item.sessionName === selectSession;

      return matchSearch && matchPrograms && matchDepartment && matchSession;
    })
    .map((item, index) => ({
      ...item,
      serial: index + 1,
    }));

  const studentInputFields = selectedStudent
    ? [
        {
          name: "studentName",
          label: "Student Name",
          type: "text",
          placeholder: "Enter Student Name",
          required: true,
        },
        {
          name: "rollNumber",
          label: "Roll Number",
          type: "text",
          placeholder: "Enter Student Roll Number",
          required: true,
        },
      ]
    : [
        {
          name: "studentName",
          label: "Student Name",
          type: "text",
          placeholder: "Enter Student Name",
          required: true,
        },
        {
          name: "rollNumber",
          label: "Roll Number",
          type: "text",
          placeholder: "Enter Student Roll Number",
          required: true,
        },
        {
          name: "invitationEmail",
          label: "Invitation Email",
          type: "email",
          placeholder: "student@gmail.com",
          required: true,
        },
      ];

  const studentDropdownFields = [
    {
      name: "departmentId",
      label: "Department",
      placeholder: "Select Department",
      searchPlaceholder: "Search Department...",
      required: true,
      options: department.map((item) => ({
        label: item.departmentName,
        value: String(item.id),
      })),
    },
    {
      name: "academicProgramId",
      label: "Academic Program",
      placeholder: formState.departmentId ? "Select Program" : "Select Department First",
      searchPlaceholder: "Search Program...",
      required: true,
      disabled: !formState.departmentId,
      options: modalPrograms.map((item) => ({
        label: `${item.programName}${item.programCode ? ` (${item.programCode})` : ''}`,
        value: String(item.id),
      })),
    },
    {
      name: "academicSessionId",
      label: "Academic Session",
      placeholder: "Select Session",
      searchPlaceholder: "Search Session...",
      required: true,
      options: academicSession
        .filter((s) => !s.sessionName?.startsWith('TestSession_') && (s.isActive || String(s.id) === String(formState.academicSessionId)))
        .map((item) => ({
          label: `${item.sessionName}${item.isActive ? ' (Active)' : ''}`,
          value: String(item.id),
        })),
    },
    {
      name: "currentSemesterId",
      label: "Semester",
      placeholder: !formState.academicProgramId
        ? "Select Program First"
        : (!formState.academicSessionId
          ? "Select Session First"
          : (modalSemesters.length === 0 ? "No active semesters found" : "Select Semester")),
      searchPlaceholder: "Search Semester...",
      required: true,
      disabled: !formState.academicProgramId || !formState.academicSessionId || modalSemesters.length === 0,
      helperText: modalSemesters.length === 0 && formState.academicProgramId && formState.academicSessionId
        ? "No active semesters configured for this program and session."
        : undefined,
      options: modalSemesters.map((item) => ({
        label: `Semester ${item.semesterNumber}`,
        value: String(item.id),
      })),
    },
  ];

  return (
    <Layout>
      {/* page header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Students</h1>
          <p className="text-sm text-gray-500 mt-0.5">
            Manage student records, view details and perform actions.
          </p>
        </div>
        <div className="text-sm font-medium text-gray-600 sm:text-right">
          {currentDate}
        </div>
      </div>

      {/* Filters And Search Bar */}
      <FilterBar
        searchValue={searchTerm}
        onSearchChange={(e) => setSearchTerm(e.target.value)}
        filters={filterList}
        actionButton={{
          label: 'Add Students',
          icon: Plus,
          onClick: handleAddClick,
        }}
      />

      <DataTable
        title="All Students"
        count={filterStudent.length}
        columns={columns}
        data={filterStudent}
        showActions={true}
        onEdit={handleEdit}
        onDelete={handleDelete}
        pagination={true}
        pageSize={10}
        currentPage={currentPage}
        onPageChange={setCurrentPage}
      />

      {showAddStudentModel && (
        <Form
          title={selectedStudent ? "Edit Student" : "Add Student"}
          subtitle={
            selectedStudent
              ? "Update student academic details."
              : "Create student record and send registration invitation email."
          }
          primaryButtonText={selectedStudent ? "Save Changes" : "Add Student"}
          cancelButtonText="Cancel"
          initialValues={formState}
          onClose={handleCloseModal}
          onSubmit={handleFormSubmit}
          onFormChange={handleFormChange}
          inputFields={studentInputFields}
          dropdownFields={studentDropdownFields}
        />
      )}
    </Layout>
  )
}

export default Students