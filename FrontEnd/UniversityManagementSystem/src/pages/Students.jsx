import React, { useEffect, useState } from 'react'
import Layout from '../layout/Layout'
import FilterBar from '../components/common/FilterBar'
import axios from 'axios'
import { AlignVerticalDistributeEnd } from 'lucide-react'
import DataTable from '../components/common/DataTable'
import { Plus } from 'lucide-react'
import Form from '../components/common/Form'


const Students = () => {






    const [department,setDepartment]= useState([])
    const [selectDepartmet,setSelectDepartment]= useState('')

    const [academicProgram,setAcademicProgram]= useState([])
    const [selectAcademicProgram,setSelectAcademicProgram]= useState('')

    const [academicSession, setAcademicSession] = useState([]);


    const [showAddStudentModel, setShowAddStudentModel]= useState(false)
    const [selectedStudent, setSelectedStudent] = useState(null);

    const [student,setStudent]=useState([])
    const [searchTerm, setSearchTerm] = useState('');
    const [currentPage, setCurrentPage] = useState(1);

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






    //handle delete


    const handleDelete = async (row) => {
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

    setStudent(prev =>
      prev.filter(s => s.id !== row.dbId)
    );

  } catch (error) {
    console.log("Student Delete Failed", error);
  }
};


    const fetchStudents = async () => {
      try {
        const token = localStorage.getItem("token");

        const departmentResponse = await axios.get(
          "https://localhost:7272/api/Department",
          { headers: { Authorization: `Bearer ${token}` } }
        );
        setDepartment(departmentResponse.data);

        const academicProgramResponse = await axios.get(
          "https://localhost:7272/api/AcademicProgram",
          { headers: { Authorization: `Bearer ${token}` } }
        );
        setAcademicProgram(academicProgramResponse.data);

        const sessionResponse = await axios.get(
          "https://localhost:7272/api/AcademicSession",
          { headers: { Authorization: `Bearer ${token}` } }
        );
        setAcademicSession(sessionResponse.data);

        const studentResponse = await axios.get(
          "https://localhost:7272/api/Student",
          { headers: { Authorization: `Bearer ${token}` } }
        );
        setStudent(studentResponse.data);
      } catch (error) {
        console.log("Failed to fetch students data", error);
      }
    };

    const handleEdit = (row) => {
      setSelectedStudent(row);
      setShowAddStudentModel(true);
    };

    const handleAddClick = () => {
      setSelectedStudent(null);
      setShowAddStudentModel(true);
    };

    const handleCloseModal = () => {
      setShowAddStudentModel(false);
      setSelectedStudent(null);
    };

    const handleFormSubmit = async (formData) => {
      try {
        const token = localStorage.getItem("token");

        const payload = {
          studentName: formData.studentName,
          rollNumber: formData.rollNumber,
          departmentId: formData.departmentId ? Number(formData.departmentId) : null,
          academicProgramId: formData.academicProgramId ? Number(formData.academicProgramId) : null,
          semester: formData.semester ? Number(formData.semester) : null,
          invitationEmail: formData.invitationEmail,
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

    useEffect(() => {
      setCurrentPage(1);
    }, [searchTerm, selectDepartmet, selectAcademicProgram]);





    //Filter List form Drop Downs

    const filterList=[

        //For Departmetns
        {
            label:"All Departments",
            value:selectDepartmet,
            onChange:(e)=>setSelectDepartment(e.target.value),

            options:[
                "All Departments",
                ...department.map(d=>d.departmentName)
            ]
        },


        //For Academic Programs

        {
            label:"All Programs",
            value:selectAcademicProgram,
            onChange:(e)=>setSelectAcademicProgram(e.target.value),

            options:[
                "All Programs",
                ...academicProgram.map(d=> d.programName)
            ]
        },


        //For Academic Session


        


    ]



     

    //Current Date

    const currentDate= new Date().toLocaleDateString('en-GB',{
        weekday:"long",
        day:"2-digit",
        month:"long",
        year:"numeric"
    })




    //Columns



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
        key:'semester',
        label:'Semester'
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



  // Map students data

  const studentsData = student.map((item) => ({
    dbId: item.id,
    id: item.studentId || '-',
    name: item.studentName || '-',
    studentName: item.studentName || '',
    RollNo: item.rollNumber || '-',
    rollNumber: item.rollNumber || '',
    dept: item.departmentName || '-',
    departmentId: item.departmentId,
    email: item.email || '-',
    program: item.programName || '-',
    academicProgramId: item.academicProgramId,
    semester: item.semester ?? '',
    status: item.status || 'Inactive',
    userId: item.userId,
    isRegistered: Boolean(item.userId && item.email),
  }));



  // Filter Students

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

      return matchSearch && matchPrograms && matchDepartment;
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
        {
          name: "semester",
          label: "Semester",
          type: "number",
          placeholder: "Enter Current Semester",
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
          name: "semester",
          label: "Semester",
          type: "number",
          placeholder: "Enter Current Semester",
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
        value: item.id,
      })),
    },
    {
      name: "academicProgramId",
      label: "Academic Program",
      placeholder: "Select Program",
      searchPlaceholder: "Search Program...",
      required: true,
      options: academicProgram.map((item) => ({
        label: item.programName,
        value: item.id,
      })),
    },
  ];

  const formInitialValues = selectedStudent
    ? {
        studentName: selectedStudent.studentName,
        rollNumber: selectedStudent.rollNumber,
        semester: selectedStudent.semester,
        departmentId: selectedStudent.departmentId,
        academicProgramId: selectedStudent.academicProgramId,
      }
    : {};

  return (
    <Layout>


        {/* page header  */}


        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-2">
          <div>
            <h1 className="text-2xl font-bold text-gray-900">Students</h1>
            <p className="text-sm text-gray-500 mt-0.5">
              Manage studetns records, view details and perform actions.
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
    initialValues={formInitialValues}
    onClose={handleCloseModal}
    onSubmit={handleFormSubmit}
    inputFields={studentInputFields}
    dropdownFields={studentDropdownFields}
  />
)}

    </Layout>
  )
}

export default Students