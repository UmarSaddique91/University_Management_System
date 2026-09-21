import React, { useState, useEffect } from 'react';
import Layout from '../layout/Layout';
import FilterBar from '../components/common/FilterBar';
import DataTable from '../components/common/DataTable';
import Form from '../components/common/Form';
import { Plus } from 'lucide-react';
import axios from 'axios';

const Departments = () => {
  const [departments, setDepartments] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [selectedDepartment, setSelectedDepartment] = useState(null);

  const fetchDepartments = async () => {
    try {
      const token = localStorage.getItem('token');
      const response = await axios.get('https://localhost:7272/api/Department', {
        headers: { Authorization: `Bearer ${token}` },
      });
      setDepartments(response.data);
    } catch (error) {
      console.log('Failed to fetch departments data', error);
    }
  };

  useEffect(() => {
    fetchDepartments();
  }, []);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm]);

  const handleAddClick = () => {
    setSelectedDepartment(null);
    setShowModal(true);
  };

  const handleEdit = (row) => {
    setSelectedDepartment(row);
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedDepartment(null);
  };

  const handleFormSubmit = async (formData) => {
    try {
      const token = localStorage.getItem('token');
      const payload = {
        departmentName: formData.departmentName?.trim(),
      };

      if (selectedDepartment) {
        await axios.put(
          `https://localhost:7272/api/Department/${selectedDepartment.dbId}`,
          payload,
          {
            headers: { Authorization: `Bearer ${token}` },
          }
        );
      } else {
        await axios.post('https://localhost:7272/api/Department', payload, {
          headers: { Authorization: `Bearer ${token}` },
        });
      }

      handleCloseModal();
      await fetchDepartments();
    } catch (error) {
      console.log('Failed to save department', error);
      alert(error.response?.data?.message || 'Failed to save department');
    }
  };

  const handleDelete = async (row) => {
    const isConfirmed = window.confirm(
      `Are you sure you want to delete ${row.departmentName}?`
    );
    if (!isConfirmed) return;

    try {
      const token = localStorage.getItem('token');
      await axios.delete(`https://localhost:7272/api/Department/${row.dbId}`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      setDepartments((prev) => {
        const updated = prev.filter((d) => d.id !== row.dbId);
        const newTotalPages = Math.max(1, Math.ceil(updated.length / 10));
        setCurrentPage((curr) => Math.min(curr, newTotalPages));
        return updated;
      });
    } catch (error) {
      console.log('Failed to delete department', error);
      alert(error.response?.data?.message || 'Failed to delete department');
    }
  };

  const currentDate = new Date().toLocaleDateString('en-GB', {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  });

  const columns = [
    {
      key: 'serial',
      label: '#',
    },
    {
      key: 'departmentName',
      label: 'Department Name',
      render: (name) => (
        <span className="font-semibold text-gray-900">{name}</span>
      ),
    },
    {
      key: 'programCount',
      label: 'Academic Programs',
      align: 'center',
      render: (count) => (
        <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200/60">
          {count}
        </span>
      ),
    },
    {
      key: 'studentCount',
      label: 'Students',
      align: 'center',
      render: (count) => (
        <span className="text-gray-700 font-medium">{count}</span>
      ),
    },
    {
      key: 'teacherCount',
      label: 'Teachers',
      align: 'center',
      render: (count) => (
        <span className="text-gray-700 font-medium">{count}</span>
      ),
    },
    {
      key: 'courseCount',
      label: 'Courses',
      align: 'center',
      render: (count) => (
        <span className="text-gray-700 font-medium">{count}</span>
      ),
    },
  ];

  const departmentsData = departments.map((item) => ({
    dbId: item.id,
    id: item.id,
    departmentName: item.departmentName || '-',
    programCount: item.programCount ?? 0,
    studentCount: item.studentCount ?? 0,
    teacherCount: item.teacherCount ?? 0,
    courseCount: item.courseCount ?? 0,
  }));

  const filterDepartments = departmentsData
    .filter((item) => {
      const search = searchTerm.toLowerCase();
      return item.departmentName.toLowerCase().includes(search);
    })
    .map((item, index) => ({
      ...item,
      serial: index + 1,
    }));

  const departmentInputFields = [
    {
      name: 'departmentName',
      label: 'Department Name',
      type: 'text',
      placeholder: 'e.g. Computer Science',
      required: true,
      colSpan: 2,
    },
  ];

  const formInitialValues = selectedDepartment
    ? {
        departmentName:
          selectedDepartment.departmentName !== '-'
            ? selectedDepartment.departmentName
            : '',
      }
    : {};

  return (
    <Layout>
      <div className="space-y-4">
        {/* Page Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Departments</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              Manage university departments and view their academic structure.
            </p>
          </div>
          <div className="text-sm font-medium text-gray-600 sm:text-right">
            {currentDate}
          </div>
        </div>

        {/* Search & Action Bar */}
        <FilterBar
          searchPlaceholder="Search departments by name..."
          searchValue={searchTerm}
          onSearchChange={(e) => setSearchTerm(e.target.value)}
          filters={[]}
          actionButton={{
            label: 'Add Department',
            icon: Plus,
            onClick: handleAddClick,
          }}
        />

        {/* Departments DataTable */}
        <DataTable
          title="All Departments"
          count={filterDepartments.length}
          columns={columns}
          data={filterDepartments}
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
            title={selectedDepartment ? 'Edit Department' : 'Add Department'}
            subtitle={
              selectedDepartment
                ? 'Update academic department details.'
                : 'Create a new university academic department.'
            }
            primaryButtonText={
              selectedDepartment ? 'Save Changes' : 'Add Department'
            }
            cancelButtonText="Cancel"
            initialValues={formInitialValues}
            onClose={handleCloseModal}
            onSubmit={handleFormSubmit}
            inputFields={departmentInputFields}
            dropdownFields={[]}
          />
        )}
      </div>
    </Layout>
  );
};

export default Departments;

