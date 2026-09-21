import React, { useState, useEffect } from 'react';
import Layout from '../layout/Layout';
import StatCard from '../components/StatCard';
import FilterBar from '../components/common/FilterBar';
import DataTable from '../components/common/DataTable';
import Form from '../components/common/Form';
import { ShieldCheck, UserPlus, UserCheck, Clock, Plus } from 'lucide-react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

const Admins = () => {
  const navigate = useNavigate();
  const [admins, setAdmins] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectStatus, setSelectStatus] = useState('');
  const [selectRegistration, setSelectRegistration] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [selectedAdmin, setSelectedAdmin] = useState(null);
  const [isSuperAdmin, setIsSuperAdmin] = useState(true);

  // Check if current user has SuperAdmin role
  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) {
      navigate('/');
      return;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const roleClaim =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
        payload.role;

      const roles = Array.isArray(roleClaim) ? roleClaim : [roleClaim];
      if (!roles.includes('SuperAdmin')) {
        setIsSuperAdmin(false);
      }
    } catch {
      setIsSuperAdmin(false);
    }
  }, [navigate]);

  const fetchAdmins = async () => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const res = await axios.get('https://localhost:7272/api/Admin', { headers });
      setAdmins(res.data || []);
    } catch (error) {
      console.log('Failed to fetch admins', error);
    }
  };

  useEffect(() => {
    if (isSuperAdmin) {
      fetchAdmins();
    }
  }, [isSuperAdmin]);

  useEffect(() => {
    setCurrentPage(1);
  }, [searchTerm, selectStatus, selectRegistration]);

  const handleAddClick = () => {
    setSelectedAdmin(null);
    setShowModal(true);
  };

  const handleEdit = (row) => {
    setSelectedAdmin(row);
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedAdmin(null);
  };

  const handleFormSubmit = async (formData) => {
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      const payload = {
        adminName: formData.adminName?.trim(),
        email: formData.email?.trim(),
      };

      if (!payload.adminName || !payload.email) {
        alert('Please fill in Admin Name and Official Email');
        return;
      }

      if (selectedAdmin) {
        await axios.put(
          `https://localhost:7272/api/Admin/${selectedAdmin.id}`,
          payload,
          { headers }
        );
      } else {
        await axios.post('https://localhost:7272/api/Admin', payload, {
          headers,
        });
      }

      handleCloseModal();
      await fetchAdmins();
    } catch (error) {
      console.log('Failed to save admin', error);
      alert(error.response?.data?.message || 'Failed to save admin profile');
    }
  };

  const handleToggleStatus = async (row) => {
    if (!row.isRegistered) {
      alert('Pending Registration: The administrator cannot be activated until they complete account registration at /signUp.');
      return;
    }

    const nextStatus = row.status === 'Active' ? 'Inactive' : 'Active';
    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.patch(
        `https://localhost:7272/api/Admin/${row.id}/status`,
        { status: nextStatus },
        { headers }
      );

      setAdmins((prev) =>
        prev.map((a) => (a.id === row.id ? { ...a, status: nextStatus } : a))
      );
    } catch (error) {
      console.log('Failed to toggle admin status', error);
      alert(error.response?.data?.message || 'Failed to update admin status');
    }
  };

  const handleDelete = async (row) => {
    if (row.isRegistered) {
      alert('Registered administrators cannot be deleted directly. Deactivate the account instead.');
      return;
    }

    const isConfirmed = window.confirm(
      `Are you sure you want to delete pending admin profile for ${row.adminName} (${row.email})?`
    );
    if (!isConfirmed) return;

    try {
      const token = localStorage.getItem('token');
      const headers = token ? { Authorization: `Bearer ${token}` } : {};

      await axios.delete(`https://localhost:7272/api/Admin/${row.id}`, {
        headers,
      });

      setAdmins((prev) => prev.filter((a) => a.id !== row.id));
    } catch (error) {
      console.log('Failed to delete admin', error);
      alert(error.response?.data?.message || 'Failed to delete admin profile');
    }
  };

  if (!isSuperAdmin) {
    return (
      <Layout>
        <div className="p-8 text-center">
          <div className="max-w-md mx-auto bg-white p-8 rounded-2xl border border-gray-200 shadow-sm">
            <ShieldCheck className="w-12 h-12 text-rose-500 mx-auto mb-4" />
            <h2 className="text-xl font-bold text-gray-900 mb-2">Access Restricted</h2>
            <p className="text-sm text-gray-500 mb-6">
              Only Super Administrators have permission to view and manage University Administrators.
            </p>
            <button
              type="button"
              onClick={() => navigate('/dashboard')}
              className="px-4 py-2 bg-blue-600 text-white text-sm font-semibold rounded-xl hover:bg-blue-700"
            >
              Return to Dashboard
            </button>
          </div>
        </div>
      </Layout>
    );
  }

  // Stats
  const totalAdmins = admins.length;
  const registeredAdmins = admins.filter((a) => a.isRegistered).length;
  const pendingActivation = admins.filter((a) => !a.isRegistered).length;
  const activeAdmins = admins.filter((a) => a.status === 'Active').length;

  const filterList = [
    {
      label: 'All Status',
      value: selectStatus,
      onChange: (e) => setSelectStatus(e.target.value),
      options: ['All Status', 'Active', 'Inactive'],
    },
    {
      label: 'All Accounts',
      value: selectRegistration,
      onChange: (e) => setSelectRegistration(e.target.value),
      options: ['All Accounts', 'Registered', 'Pending Activation'],
    },
  ];

  const columns = [
    {
      key: 'serial',
      label: '#',
    },
    {
      key: 'adminId',
      label: 'Admin ID',
      render: (id) => (
        <span className="font-semibold text-blue-600 bg-blue-50 px-2.5 py-1 rounded-md text-xs">
          {id || '-'}
        </span>
      ),
    },
    {
      key: 'adminName',
      label: 'Admin Name',
      render: (name) => <span className="font-bold text-gray-900">{name}</span>,
    },
    {
      key: 'email',
      label: 'Official Email',
      render: (email) => <span className="text-gray-700 font-medium">{email}</span>,
    },
    {
      key: 'registration',
      label: 'Registration Status',
      align: 'center',
      render: (_, row) => (
        <span
          className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${
            row.isRegistered
              ? 'bg-emerald-50 text-emerald-700 border border-emerald-200'
              : 'bg-amber-50 text-amber-700 border border-amber-200'
          }`}
        >
          {row.isRegistered ? 'Registered' : 'Pending Activation'}
        </span>
      ),
    },
    {
      key: 'status',
      label: 'Account Status',
      align: 'center',
      render: (status, row) => {
        const isActive = status === 'Active';
        if (!row.isRegistered) {
          return (
            <button
              type="button"
              onClick={() => handleToggleStatus(row)}
              title="Pending Registration - Admin must complete signup at /signUp before activation"
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
      key: 'createdAt',
      label: 'Created Date',
      render: (dt) => (
        <span className="text-xs text-gray-500">
          {dt ? new Date(dt).toLocaleDateString('en-GB') : '-'}
        </span>
      ),
    },
  ];

  const filteredData = admins
    .filter((item) => {
      const search = searchTerm.toLowerCase();
      const matchSearch =
        (item.adminName && item.adminName.toLowerCase().includes(search)) ||
        (item.email && item.email.toLowerCase().includes(search)) ||
        (item.adminId && item.adminId.toLowerCase().includes(search));

      const matchStatus =
        selectStatus === '' ||
        selectStatus === 'All Status' ||
        item.status === selectStatus;

      const matchRegistration =
        selectRegistration === '' ||
        selectRegistration === 'All Accounts' ||
        (selectRegistration === 'Registered' ? item.isRegistered : !item.isRegistered);

      return matchSearch && matchStatus && matchRegistration;
    })
    .map((item, index) => ({
      ...item,
      serial: index + 1,
    }));

  const inputFields = [
    {
      name: 'adminName',
      label: 'Full Name',
      type: 'text',
      placeholder: 'e.g. Dr. Jonathan Vance',
      required: true,
      colSpan: 2,
    },
    {
      name: 'email',
      label: 'Official Email Address',
      type: 'email',
      placeholder: 'e.g. j.vance@university.edu',
      required: true,
      colSpan: 2,
    },
  ];

  return (
    <Layout>
      <div className="p-6">
        {/* Header */}
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 mb-6">
          <div>
            <div className="flex items-center gap-2">
              <ShieldCheck className="w-6 h-6 text-blue-600" />
              <h1 className="text-2xl font-bold text-gray-900 tracking-tight">
                Administrators Management
              </h1>
            </div>
            <p className="text-sm text-gray-500 mt-1">
              Pre-register and oversee administrative accounts with full role governance
            </p>
          </div>

          <button
            type="button"
            onClick={handleAddClick}
            className="inline-flex items-center gap-2 px-4 py-2.5 bg-blue-600 text-white text-sm font-semibold rounded-xl hover:bg-blue-700 cursor-pointer shadow-sm hover:shadow transition-all"
          >
            <Plus size={18} />
            <span>Pre-Register Admin</span>
          </button>
        </div>

        {/* Stats Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          <StatCard
            Title="Total Admins"
            Value={totalAdmins}
            Icon={ShieldCheck}
            iconBg="bg-blue-100/70"
            iconcolor="text-blue-600"
          />
          <StatCard
            Title="Active Admins"
            Value={activeAdmins}
            Icon={UserCheck}
            iconBg="bg-emerald-100/70"
            iconcolor="text-emerald-600"
          />
          <StatCard
            Title="Registered Accounts"
            Value={registeredAdmins}
            Icon={UserPlus}
            iconBg="bg-indigo-100/70"
            iconcolor="text-indigo-600"
          />
          <StatCard
            Title="Pending Activation"
            Value={pendingActivation}
            Icon={Clock}
            iconBg="bg-amber-100/70"
            iconcolor="text-amber-600"
          />
        </div>

        {/* FilterBar */}
        <FilterBar
          searchValue={searchTerm}
          onSearchChange={(e) => setSearchTerm(e.target.value)}
          searchPlaceholder="Search by admin ID, name, or official email..."
          filterList={filterList}
        />

        {/* DataTable */}
        <DataTable
          title="Administrators"
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

        {/* Form Modal */}
        {showModal && (
          <Form
            title={selectedAdmin ? 'Edit Administrator Profile' : 'Pre-Register Administrator'}
            subtitle={
              selectedAdmin
                ? selectedAdmin.isRegistered
                  ? 'Update administrator name (Email is locked for active registered accounts)'
                  : 'Update pre-registered administrator details'
                : 'Enter official administrator credentials for pre-approval. The admin can then complete signup at /signUp.'
            }
            inputFields={inputFields}
            initialValues={
              selectedAdmin
                ? {
                    adminName: selectedAdmin.adminName,
                    email: selectedAdmin.email,
                  }
                : { adminName: '', email: '' }
            }
            onClose={handleCloseModal}
            onSubmit={handleFormSubmit}
            primaryButtonText={selectedAdmin ? 'Save Changes' : 'Pre-Register Admin'}
          />
        )}
      </div>
    </Layout>
  );
};

export default Admins;

