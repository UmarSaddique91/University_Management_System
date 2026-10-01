import React, { useState, useEffect } from 'react';
import Layout from '../layout/Layout';
import FilterBar from '../components/common/FilterBar';
import DataTable from '../components/common/DataTable';
import Form from '../components/common/Form';
import DeleteConfirmationModal from '../components/common/DeleteConfirmationModal';
import { Plus } from 'lucide-react';
import axios from 'axios';

const Courses = () => {
  const [courses, setCourses] = useState([]);
  const [departments, setDepartments] = useState([]);
  const [academicPrograms, setAcademicPrograms] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectDepartment, setSelectDepartment] = useState('');
  const [selectProgram, setSelectProgram] = useState('');
  const [selectStatus, setSelectStatus] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [selectedCourse, setSelectedCourse] = useState(null);
  const [formDepartmentId, setFormDepartmentId] = useState(null);
  const [deleteTarget, setDeleteTarget] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState('');

  const fetchCoursesAndMetadata = async () => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const [coursesRes, deptsRes, progsRes] = await Promise.all([
        axios.get('https://localhost:7272/api/Course', { headers }),
        axios.get('https://localhost:7272/api/Department', { headers }),
        axios.get('https://localhost:7272/api/AcademicProgram', { headers }),
      ]);

      setCourses(coursesRes.data || []);
      setDepartments(deptsRes.data || []);
      setAcademicPrograms(progsRes.data || []);
    } catch (error) {
      console.log('Failed to fetch courses data', error);
    }
  };

  useEffect(() => {
    fetchCoursesAndMetadata();
  }, []);

  // Reset pagination on filter or search change
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, selectDepartment, selectProgram, selectStatus]);


    const currentDate = new Date().toLocaleDateString('en-GB', {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  });

  const [formState, setFormState] = useState({
    courseCode: '',
    courseName: '',
    creditHours: '',
    level: '',
    departmentId: '',
    academicProgramIds: [],
  });

  const handleAddClick = () => {
    setSelectedCourse(null);
    setFormDepartmentId(null);
    setFormState({
      courseCode: '',
      courseName: '',
      creditHours: '',
      level: '',
      departmentId: '',
      academicProgramIds: [],
    });
    setShowModal(true);
  };

  const handleEdit = (row) => {
    setSelectedCourse(row);
    const deptId = row.departmentId ? Number(row.departmentId) : null;
    setFormDepartmentId(deptId);
    setFormState({
      courseCode: row.courseCode !== '-' ? row.courseCode : '',
      courseName: row.courseName !== '-' ? row.courseName : '',
      creditHours: row.creditHours ?? '',
      level: row.level !== '-' ? row.level : '',
      departmentId: row.departmentId ? String(row.departmentId) : '',
      academicProgramIds: (row.academicPrograms || []).map((p) => p.id),
    });
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedCourse(null);
    setFormDepartmentId(null);
    setFormState({
      courseCode: '',
      courseName: '',
      creditHours: '',
      level: '',
      departmentId: '',
      academicProgramIds: [],
    });
  };

  const handleFormChange = (updatedData) => {
    setFormState((prev) => ({ ...prev, ...updatedData }));
  };

  const handleFormSubmit = async (formData) => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const selectedProgramIds = Array.isArray(formData.academicProgramIds)
        ? formData.academicProgramIds.map(Number)
        : [];

      const payload = {
        courseCode: formData.courseCode?.trim(),
        courseName: formData.courseName?.trim(),
        creditHours: formData.creditHours ? Number(formData.creditHours) : null,
        level: formData.level?.trim(),
        departmentId: formData.departmentId ? Number(formData.departmentId) : null,
        status: selectedCourse ? selectedCourse.status : 'Active',
        academicProgramIds: selectedProgramIds,
      };

      if (selectedCourse) {
        await axios.put(
          `https://localhost:7272/api/Course/${selectedCourse.dbId}`,
          payload,
          { headers }
        );
      } else {
        await axios.post('https://localhost:7272/api/Course', payload, {
          headers,
        });
      }

      handleCloseModal();
      await fetchCoursesAndMetadata();
    } catch (error) {
      console.log('Failed to save course', error);
      alert(error.response?.data?.message || 'Failed to save course');
    }
  };

  const handleToggleStatus = async (row) => {
    const nextStatus = row.status === 'Active' ? 'Inactive' : 'Active';

    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.patch(
        `https://localhost:7272/api/Course/${row.dbId}/status`,
        { status: nextStatus },
        { headers }
      );

      setCourses((prev) =>
        prev.map((c) => (c.id === row.dbId ? { ...c, status: nextStatus } : c))
      );
    } catch (error) {
      console.log('Failed to update course status', error);
      alert(error.response?.data?.message || 'Failed to update course status');
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

      await axios.delete(`https://localhost:7272/api/Course/${deleteTarget.dbId}`, {
        headers,
      });

      setCourses((prev) => {
        const updated = prev.filter((c) => c.id !== deleteTarget.dbId);
        const newTotalPages = Math.max(1, Math.ceil(updated.length / 10));
        setCurrentPage((curr) => Math.min(curr, newTotalPages));
        return updated;
      });
      setDeleteTarget(null);
    } catch (error) {
      console.log('Failed to delete course', error);
      setDeleteError(error.response?.data?.message || 'Failed to delete course');
    } finally {
      setIsDeleting(false);
    }
  };






  const availableProgramsForFilter =
    selectDepartment && selectDepartment !== 'All Departments'
      ? academicPrograms.filter(
          (p) =>
            p.departmentName === selectDepartment ||
            p.department?.departmentName === selectDepartment
        )
      : academicPrograms;

  const handleDepartmentFilterChange = (e) => {
    const newDept = e.target.value;
    setSelectDepartment(newDept);
    if (newDept && newDept !== 'All Departments') {
      const allowed = academicPrograms.filter(
        (p) =>
          p.departmentName === newDept ||
          p.department?.departmentName === newDept
      );
      if (!allowed.some((p) => p.programName === selectProgram)) {
        setSelectProgram('');
      }
    }
  };

  const filterList = [
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
      key: 'courseCode',
      label: 'Course Code',
      render: (code) => (
        <span className="font-semibold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-md text-xs">
          {code}
        </span>
      ),
    },
    {
      key: 'courseName',
      label: 'Course Name',
      render: (name) => (
        <span className="font-semibold text-gray-900">{name}</span>
      ),
    },
    {
      key: 'departmentName',
      label: 'Department',
    },
    {
      key: 'academicPrograms',
      label: 'Academic Programs',
      render: (progs) => {
        if (!progs || progs.length === 0) {
          return <span className="text-gray-400 text-xs">-</span>;
        }
        return (
          <div className="flex flex-wrap gap-1.5 max-w-xs">
            {progs.map((prog) => (
              <span
                key={prog.id}
                className="inline-flex items-center px-2 py-0.5 rounded-md text-xs font-medium bg-slate-100 text-slate-700 border border-slate-200"
              >
                {prog.programName}
              </span>
            ))}
          </div>
        );
      },
    },
    {
      key: 'creditHours',
      label: 'Credit Hours',
      align: 'center',
      render: (ch) => (
        <span className="text-gray-700 font-medium">{ch ?? '-'}</span>
      ),
    },
    {
      key: 'level',
      label: 'Level',
      align: 'center',
      render: (lvl) => (
        <span className="text-gray-700 font-medium">{lvl || '-'}</span>
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

  const coursesData = courses.map((item) => ({
    dbId: item.id,
    id: item.id,
    courseCode: item.courseCode || '-',
    courseName: item.courseName || '-',
    departmentId: item.departmentId,
    departmentName: item.departmentName || '-',
    academicPrograms: item.academicPrograms || [],
    creditHours: item.creditHours,
    level: item.level || '-',
    status: item.status || 'Active',
  }));

  const filterCourses = coursesData
    .filter((item) => {
      const search = searchTerm.toLowerCase();

      const matchSearch =
        item.courseCode.toLowerCase().includes(search) ||
        item.courseName.toLowerCase().includes(search) ||
        item.departmentName.toLowerCase().includes(search) ||
        (item.academicPrograms || []).some(
          (p) =>
            (p.programName && p.programName.toLowerCase().includes(search)) ||
            (p.programCode && p.programCode.toLowerCase().includes(search))
        );

      const matchDept =
        selectDepartment === '' ||
        selectDepartment === 'All Departments' ||
        item.departmentName === selectDepartment;

      const matchProgram =
        selectProgram === '' ||
        selectProgram === 'All Academic Programs' ||
        (item.academicPrograms || []).some(
          (p) => p.programName === selectProgram
        );

      const matchStatus =
        selectStatus === '' ||
        selectStatus === 'All Status' ||
        item.status === selectStatus;

      return matchSearch && matchDept && matchProgram && matchStatus;
    })
    .map((item, index) => ({
      ...item,
      serial: index + 1,
    }));

  const courseInputFields = [
    {
      name: 'courseCode',
      label: 'Course Code',
      type: 'text',
      placeholder: 'e.g. CS-301',
      required: true,
      colSpan: 1,
    },
    {
      name: 'courseName',
      label: 'Course Name',
      type: 'text',
      placeholder: 'e.g. Database Systems',
      required: true,
      colSpan: 1,
    },
    {
      name: 'creditHours',
      label: 'Credit Hours',
      type: 'number',
      placeholder: 'e.g. 3',
      required: true,
      colSpan: 1,
    },
    {
      name: 'level',
      label: 'Level',
      type: 'text',
      placeholder: 'e.g. 3 or Undergraduate',
      required: true,
      colSpan: 1,
    },
  ];

  const courseDropdownFields = [
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
    {
      name: 'academicProgramIds',
      label: 'Academic Programs',
      placeholder: 'Select Academic Program(s)...',
      searchPlaceholder: 'Search Academic Programs...',
      required: false,
      isMulti: true,
      colSpan: 2,
      options: academicPrograms.map((prog) => ({
        label: `${prog.programName} (${prog.programCode || ''})`,
        value: prog.id,
      })),
      helperText: 'Select all academic programs where this course is offered.',
    },
  ];

  return (
    <Layout>
      <div className="space-y-4">
        {/* Page Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Courses</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              Manage university courses, credit hours and academic departments.
            </p>
          </div>
          <div className="text-sm font-medium text-gray-600 sm:text-right">
            {currentDate}
          </div>
        </div>

        {/* Filters and Search Bar */}
        <FilterBar
          searchPlaceholder="Search by course code, name, department, or program..."
          searchValue={searchTerm}
          onSearchChange={(e) => setSearchTerm(e.target.value)}
          filters={filterList}
          actionButton={{
            label: 'Add Course',
            icon: Plus,
            onClick: handleAddClick,
          }}
        />

        {/* Courses DataTable */}
        <DataTable
          title="All Courses"
          count={filterCourses.length}
          columns={columns}
          data={filterCourses}
          showActions={true}
          onEdit={handleEdit}
          onDelete={handleDelete}
          pagination={true}
          pageSize={10}
          currentPage={currentPage}
          onPageChange={setCurrentPage}
        />

        {/* Reusable Form Modal for Add & Edit */}
        {showModal && (
          <Form
            title={selectedCourse ? 'Edit Course' : 'Add Course'}
            subtitle={
              selectedCourse
                ? 'Update course title, code, level, department and associated programs.'
                : 'Create a new university course or subject and assign academic programs.'
            }
            primaryButtonText={selectedCourse ? 'Save Changes' : 'Add Course'}
            cancelButtonText="Cancel"
            initialValues={formState}
            onClose={handleCloseModal}
            onSubmit={handleFormSubmit}
            onFormChange={handleFormChange}
            inputFields={courseInputFields}
            dropdownFields={courseDropdownFields}
          />
        )}

        {/* Delete Confirmation Modal */}
        <DeleteConfirmationModal
          isOpen={!!deleteTarget}
          title="Delete Course"
          itemName={deleteTarget ? `${deleteTarget.courseName} (${deleteTarget.courseCode})` : ''}
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

export default Courses;
