import React from 'react';
import Layout from '../layout/Layout';
import StatCard from '../components/StatCard';
import FilterBar from '../components/common/FilterBar';
import DataTable from '../components/common/DataTable';
import { Users, UserRound, Landmark, UserPlus, Plus } from 'lucide-react';

import {useState,useEffect} from 'react';
import axios from 'axios';

const Teachers = () => {

const [teacher,setTeacher]=useState([]);
const [department,setDepartment]=useState([]);
const [teacherThisMonth, setTeacherThisMonth]= useState(0);
const [teacherGrwothThisMonth, setTeacherGrowthThisMonth]=useState(0);
const [searchTerm,setSearchTerm]= useState('');
const [selectDepartment, setSelectDepartment]=useState('');
const [selectStatus,setSelectStatus]= useState('');







// Axios Code 
useEffect(()=>{
  const fetchTeachers=async()=>{
    try {
      const token=localStorage.getItem("token")


      //Teacher
      const Teacherresponse= await axios.get(
        "https://localhost:7272/api/Teacher",
        {
          headers:{
            Authorization:`Bearer ${token}`
          }
        }
      );


      //Total Teacher This Month


      const teacherThisMonth= await axios.get(
        "https://localhost:7272/api/DashboardSummary/summary",
        {
          headers:{
            Authorization:`Bearer ${token}`
          }
        }
      )


      const departmentResponse=await axios.get(
        "https://localhost:7272/api/Department",

        {
          headers:{
            Authorization:`Bearer ${token}`
          }
        }
      )






      // Teacher 
      setTeacher(Teacherresponse.data);
      console.log(Teacherresponse.data);


      //Teacher this Month

      setTeacherThisMonth(teacherThisMonth.data)

      //teacher Growth this month

      setTeacherGrowthThisMonth(teacherThisMonth.data)

      //Deprtment

      setDepartment(departmentResponse.data)
      
    } 
    
    
    catch (error) {
      console.log("Teacher Nor Found",error)
    }
  }
  fetchTeachers();
},[])


const handleDelete = async (row) => {
  try {
    const token = localStorage.getItem("token");

    await axios.delete(
      `https://localhost:7272/api/Teacher/${row.dbId}`,
      {
        headers: {
          Authorization: `Bearer ${token}`
        }
      }
    );

    setTeacher(prev =>
      prev.filter(t => t.id !== row.dbId)
    );

  } catch (error) {
    console.log("Teacher Delete Failed", error);
  }
};



//Edit Row








  // Current date formatted according to the design
  const currentDate = new Date().toLocaleDateString('en-GB', {
    weekday: 'long',
    day: '2-digit',
    month: 'long',
    year: 'numeric',
  });








  // Filter dropdown configurations (UI only)
  const filterList = [
    {
      label: 'All Departments',
      value:selectDepartment,
      onChange:(e)=>setSelectDepartment(e.target.value),

      options: [
        'All Departments',
        ...department.map(d=>d.departmentName)
      ],
    },
    {
      label: 'All Status',
      value: selectStatus,
      onChange:(e)=>setSelectStatus(e.target.value),
      options: ['All Status', 'Active', 'On Leave'],
    },
  ];









  // Column definitions for the DataTable
  const columns = [
    {
      key: 'id',
      label: 'Teacher ID',
    },
    {
      key: 'name',
      label: 'Teacher Name',
      render: (name, row) => (
        <div className="flex items-center gap-3">
          <img
            src={row.avatar}
            alt={name}
            className="w-8 h-8 rounded-full object-cover border border-gray-200"
          />
          <span className="font-medium text-gray-900">{name}</span>
        </div>
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
      render: (status) => {
        const isActive = status === 'Active';
        return (
          <span
            className={`inline-flex items-center px-3 py-0.5 rounded-full text-xs font-semibold ${
              isActive
                ? 'bg-emerald-50 text-emerald-600 border border-emerald-200/60'
                : 'bg-rose-50 text-rose-600 border border-rose-200/60'
            }`}
          >
            {status}
          </span>
        );
      },
    },
    {
      key: 'courses',
      label: 'Assigned Courses',
      align: 'center',
    },
  ];







//add the original teacher table into table 
  const teachersData= teacher.map((item)=>({
    dbId:item.id,
    id: item.teacherId || '-',
    name: item.teacherName || '-',
    dept: item.departmentName || '-',
    spec: item.specialization || '-',
    email:item.email || '-',
    status: item.status || '-',
    courses: '-',
    avatar:'-'
  }));










  //Filter Teacher 

  const filterTeacher= teachersData.filter((item)=>{

    const search= searchTerm.toLowerCase();


    const matchSearch=
item.name.toLowerCase().includes(search)||
    item.id.toLowerCase().includes(search)||
    item.email.toLowerCase().includes(search)



    const matchDepartment=

    selectDepartment==='' ||
    selectDepartment==="All Departments" ||
    item.dept===selectDepartment;



    const matchStatus=

    selectStatus==='' ||
    selectStatus==="All Status" ||
    item.status===selectStatus;
    



    return matchSearch && matchDepartment && matchStatus;
    
    
  }
  
  
  )
  
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
          onSearchChange={(e)=>setSearchTerm(e.target.value)}
          filters={filterList}
          actionButton={{
            label: 'Add Teacher',
            icon: Plus,
          }}
        />

        {/* Statistics Cards Row */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8">
          <StatCard
            Title="Total Teachers"
            Value={teacher.length}
            Change={`↑ ${teacherThisMonth.teacherGrowthPercentage}`}
            SubText={`+${teacherThisMonth.teacherThisMonth} this month`}
            Icon={Users}
            iconBg="bg-blue-500/20"
            cardBg="bg-blue-500/5"
            iconcolor="text-blue-700"
          />
          <StatCard
            Title="Active Teachers"
            Value={teacher.filter(t=> t.status==="Active").length}
            
            Icon={UserRound}
            iconBg="bg-green-700/20"
            cardBg="bg-green-700/5"
            iconcolor="text-green-700"
          />
          <StatCard
            Title="Departments Covered"
            Value={department.length}
            Change=""
            SubText="No change"
            Icon={Landmark}
            iconBg="bg-purple-700/20"
            cardBg="bg-purple-700/5"
            iconcolor="text-purple-700"
          />
          
        </div>

        {/* Teachers DataTable */}
        <DataTable
          title="All Teachers"
          count={teacher.length}
        onDelete={handleDelete}
          columns={columns}
          data={filterTeacher}
          showActions={true}
        />
      </div>
    </Layout>
  );
};

export default Teachers;

