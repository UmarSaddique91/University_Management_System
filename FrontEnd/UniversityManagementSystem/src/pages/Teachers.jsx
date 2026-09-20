import React, { useState, useEffect } from 'react';
import Layout from '../layout/Layout';
import StatCard from '../components/StatCard';
import FilterBar from '../components/common/FilterBar';
import DataTable from '../components/common/DataTable';
import Form from '../components/common/Form';
import { Users, UserRound, Landmark, Plus } from 'lucide-react';
import axios from 'axios';

const Teachers = () => {
  const [teacher, setTeacher] = useState([]);
  const [department, setDepartment] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectDepartment, setSelectDepartment] = useState('');
  const [selectStatus, setSelectStatus] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [selectedTeacher, setSelectedTeacher] = useState(null);

  const fetchTeachers = async () => {
    try {
      const token = localStorage.getItem("token");

      const [teacherResponse, departmentResponse] = await Promise.all([
        axios.get("https://localhost:7272/api/Teacher", {
          headers: { Authorization: `Bearer ${token}` },
        }),
        axios.get("https://localhost:7272/api/Department", {
          headers: { Authorization: `Bearer ${token}` },
        }),
      ]);

      setTeacher(teacherResponse.data);
      setDepartment(departmentResponse.data);
    } catch (error) {
      console.log("Failed to fetch teachers data", error);
    }
  };

  useEffect(() => {
    fetchTeachers();
  }, []);

  // Reset pagination to page 1 whenever filters change
  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, selectDepartment, selectStatus]);

  const handleAddClick = () => {
    setSelectedTeacher(null);
    setShowModal(true);
  };

  const handleEdit = (row) => {
    setSelectedTeacher(row);
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedTeacher(null);
  };

  const handleFormSubmit = async (formData) => {
    try {
      const token = localStorage.getItem("token");

      const payload = {
        teacherName: formData.teacherName,
        teacherId: formData.teacherId,
        departmentId: formData.departmentId ? Number(formData.departmentId) : null,
        specialization: formData.specialization || '',
        email: formData.email,
      };

      if (selectedTeacher) {
        // Edit Mode: PUT /api/Teacher/{id}
        await axios.put(
          `https://localhost:7272/api/Teacher/${selectedTeacher.dbId}`,
          payload,
          {
            headers: {
              Authorization: `Bearer ${token}`,
            },
          }
        );
      } else {
        // Create Mode: POST /api/Teacher
        await axios.post("https://localhost:7272/api/Teacher", payload, {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        });
      }

      handleCloseModal();
      await fetchTeachers();
    } catch (error) {
      console.log("Failed to save teacher", error);
      alert(error.response?.data?.message || "Failed to save teacher");
    }
  };

  const handleToggleStatus = async (row) => {
    if (!row.isRegistered) {
      alert("Cannot activate teacher: This teacher has not completed registration yet.");
      return;
    }

    const newStatus = row.status === 'Active' ? 'Inactive' : 'Active';

    try {
      const token = localStorage.getItem("token");
      await axios.patch(
        `https://localhost:7272/api/Teacher/${row.dbId}/status`,
        { status: newStatus },
        {
          headers: {
            Authorization: `Bearer ${token}`,
          },
        }
      );

      setTeacher((prev) =>
        prev.map((t) => (t.id === row.dbId ? { ...t, status: newStatus } : t))
      );
    } catch (error) {
      console.log("Failed to update teacher status", error);
      alert(error.response?.data?.message || "Failed to update teacher status");
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
        ...department.map((d) => d.departmentName),
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
      key: 'id',
      label: 'Teacher ID',
    },
    {
      key: 'name',
      label: 'Teacher Name',
      render: (name) => (
        <span className="font-medium text-gray-900">{name}</span>
      ),
    },
    {
      key: 'dept',
      label: 'Department',
    },
    {
      key: 'spec',
      label: 'Specialization',
    },
    {
      key: 'email',
      label: 'Email',
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
              title="Pending Registration - Teacher cannot be activated until they complete registration"
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
    {
      key: 'courses',
      label: 'Assigned Courses',
      align: 'center',
      render: (courses, row) => (
        <span className="text-gray-700 font-medium">
          {row.assignedCoursesCount > 0 ? row.assignedCoursesCount : '-'}
        </span>
      ),
    },
  ];

  const teachersData = teacher.map((item) => ({
    dbId: item.id,
    id: item.teacherId || '-',
    teacherId: item.teacherId || '',
    name: item.teacherName || '-',
    teacherName: item.teacherName || '',
    dept: item.departmentName || '-',
    departmentId: item.departmentId,
    spec: item.specialization || '-',
    specialization: item.specialization || '',
    email: item.email || '-',
    status: item.status || 'Inactive',
    assignedCoursesCount: item.assignedCoursesCount || 0,
    userId: item.userId,
    isRegistered: Boolean(item.userId && item.email),
  }));

  const filterTeacher = teachersData
    .filter((item) => {
      const search = searchTerm.toLowerCase();

      const matchSearch =
        item.name.toLowerCase().includes(search) ||
        item.id.toLowerCase().includes(search) ||
        item.email.toLowerCase().includes(search);

      const matchDepartment =
        selectDepartment === '' ||
        selectDepartment === 'All Departments' ||
        item.dept === selectDepartment;

      const matchStatus =
        selectStatus === '' ||
        selectStatus === 'All Status' ||
        item.status === selectStatus;

      return matchSearch && matchDepartment && matchStatus;
    })
    .map((item, index) => ({
      ...item,
      serial: index + 1,
    }));

  const teacherInputFields = selectedTeacher
    ? [
        {
          name: 'teacherName',
          label: 'Teacher Name',
          type: 'text',
          placeholder: 'Enter Teacher Name',
          required: true,
        },
        {
          name: 'teacherId',
          label: 'Teacher ID',
          type: 'text',
          placeholder: 'e.g. TCH-001',
          required: true,
        },
        {
          name: 'specialization',
          label: 'Specialization',
          type: 'text',
          placeholder: 'e.g. Artificial Intelligence',
          required: false,
        },
      ]
    : [
        {
          name: 'teacherName',
          label: 'Teacher Name',
          type: 'text',
          placeholder: 'Enter Teacher Name',
          required: true,
        },
        {
          name: 'teacherId',
          label: 'Teacher ID',
          type: 'text',
          placeholder: 'e.g. TCH-001',
          required: true,
        },
        {
          name: 'specialization',
          label: 'Specialization',
          type: 'text',
          placeholder: 'e.g. Artificial Intelligence',
          required: false,
        },
        {
          name: 'email',
          label: 'Official Email',
          type: 'email',
          placeholder: 'teacher@university.edu',
          required: true,
        },
      ];

  const teacherDropdownFields = [
    {
      name: 'departmentId',
      label: 'Department',
      placeholder: 'Select Department',
      searchPlaceholder: 'Search Department...',
      required: true,
      options: department.map((item) => ({
        label: item.departmentName,
        value: item.id,
      })),
    },
  ];

  const formInitialValues = selectedTeacher
    ? {
        teacherName: selectedTeacher.teacherName,
        teacherId: selectedTeacher.teacherId,
        specialization: selectedTeacher.specialization,
        departmentId: selectedTeacher.departmentId,
      }
    : {};

  // Real summary card metrics based on full teacher dataset
  const totalTeachersCount = teacher.length;
  const activeTeachersCount = teacher.filter((t) => t.status === 'Active').length;
  const departmentsCoveredCount = new Set(
    teacher.map((t) => t.departmentId).filter(Boolean)
  ).size;

  return (
    <Layout>
      <div className="space-y-4">
        {/* Page Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Teachers</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              Manage faculty records, assign courses, and oversee teacher information.
            </p>
          </div>
          <div className="text-sm font-medium text-gray-600 sm:text-right">
            {currentDate}
          </div>
        </div>

        {/* Filter & Action Bar */}
        <FilterBar
          searchPlaceholder="Search teachers by name, ID, or email..."
          searchValue={searchTerm}
          onSearchChange={(e) => setSearchTerm(e.target.value)}
          filters={filterList}
          actionButton={{
            label: 'Add Teacher',
            icon: Plus,
            onClick: handleAddClick,
          }}
        />

        {/* Statistics Cards Row */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8">
          <StatCard
            Title="Total Teachers"
            Value={totalTeachersCount}
            Icon={Users}
            iconBg="bg-blue-500/20"
            cardBg="bg-blue-500/5"
            iconcolor="text-blue-700"
          />
          <StatCard
            Title="Active Teachers"
            Value={activeTeachersCount}
            Icon={UserRound}
            iconBg="bg-emerald-700/20"
            cardBg="bg-emerald-700/5"
            iconcolor="text-emerald-700"
          />
          <StatCard
            Title="Departments Covered"
            Value={departmentsCoveredCount}
            Icon={Landmark}
            iconBg="bg-purple-700/20"
            cardBg="bg-purple-700/5"
            iconcolor="text-purple-700"
          />
        </div>

        {/* Teachers DataTable */}
        <DataTable
          title="All Teachers"
          count={filterTeacher.length}
          columns={columns}
          data={filterTeacher}
          showActions={true}
          onEdit={handleEdit}
          pagination={true}
          pageSize={10}
          currentPage={currentPage}
          onPageChange={setCurrentPage}
        />

        {/* Reusable Form Modal for Add & Edit */}
        {showModal && (
          <Form
            title={selectedTeacher ? 'Edit Teacher' : 'Add Teacher'}
            subtitle={
              selectedTeacher
                ? 'Update teacher academic and faculty details.'
                : 'Create teacher record with official university email.'
            }
            primaryButtonText={selectedTeacher ? 'Save Changes' : 'Add Teacher'}
            cancelButtonText="Cancel"
            initialValues={formInitialValues}
            onClose={handleCloseModal}
            onSubmit={handleFormSubmit}
            inputFields={teacherInputFields}
            dropdownFields={teacherDropdownFields}
          />
        )}
      </div>
    </Layout>
  );
};

export default Teachers;
