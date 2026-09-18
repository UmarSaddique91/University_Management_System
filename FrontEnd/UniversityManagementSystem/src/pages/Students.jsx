import React, { useEffect, useState } from 'react'
import Layout from '../layout/Layout'
import FilterBar from '../components/common/FilterBar'
import axios from 'axios'
import { AlignVerticalDistributeEnd } from 'lucide-react'
import DataTable from '../components/common/DataTable'


const Students = () => {






    const [department,setDepartment]= useState([])
    const [selectDepartmet,setSelectDepartment]= useState('')

    const [academicProgram,setAcademicProgram]= useState([])
    const [selectAcademicProgram,setSelectAcademicProgram]= useState('')


    const [academicSession, setAcademicSession]=useState([])
    const [selectAcademicSession,setSelectAcadmicSession]=useState('')


    const [student,setStudent]=useState([])


    const [searchTerm, setSearchTerm] = useState('');






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


    useEffect(()=>{
        const fetchStudents=async()=>{

            try {

                const token= localStorage.getItem("token")


                //Fetch department form backend

                const departmentResponse= await axios.get(

                    "https://localhost:7272/api/Department",
                    {
                        headers:{
                            Authorization:`Bearer ${token}`
                        }
                    }
                )

                console.log(departmentResponse.data)

                setDepartment(departmentResponse.data);




                //Fetch Academic Program 


                const academicProgramResponse= await axios.get(
                    "https://localhost:7272/api/AcademicProgram",
                    {
                        headers:{
                            Authorization:`Bearer ${token}`
                        }
                    }
                )
                console.log(academicProgramResponse.data)

                setAcademicProgram(academicProgramResponse.data);



                //Fetch Acadmic Session

                const sessionResponse= await axios.get(
                    "https://localhost:7272/api/AcademicSession",
                    {
                        headers:{
                            Authorization:`Bearer ${token}`
                        }
                    }
                )

                console.log(sessionResponse.data)
                setAcademicSession(sessionResponse.data)



                //Fetch Teachers Data

                const studentResponse= await axios.get(

                    "https://localhost:7272/api/Student",
                    {
                        headers:{
                            Authorization:`Bearer ${token}`
                        }
                    }
                )

                console.log(studentResponse.data)

                setStudent(studentResponse.data);

                
            } catch (error) {

                console.log("Student don't Found",error)
                
            }
        }

        fetchStudents();
    },[])





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
  
  ];



  //add the original teacher table in the stuent atble



  const studentsData = student.map((item, index) => ({
  serial: index + 1,
  dbId: item.id,

  id: item.studentId || '-',
  name: item.studentName || '-',
  RollNo: item.rollNumber || '-',
  dept: item.departmentName || '-',
  email: item.email || '-',
   program: item.programName || '-',
  semester: item.semester || '-',
  status: item.status || '-',
 

  avatar: '-'
}));



 //Filter Teacher 

  const filterStudent= studentsData.filter((item)=>{

    const search= searchTerm.toLowerCase();


    const matchSearch=
item.name.toLowerCase().includes(search)||
    item.RollNo.toLowerCase().includes(search)||
    item.email.toLowerCase().includes(search)



    const matchDepartment=

    selectDepartmet==='' ||
    selectDepartmet==="All Departments" ||
    item.dept===selectDepartmet;



  
    
const matchPrograms=

    selectAcademicProgram==='' ||
    selectAcademicProgram==="All Programs" ||
    item.dept===selectAcademicProgram;


   


    return matchSearch && matchPrograms  && matchDepartment;
    
    
  }
  
  
  )


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
        />



        <DataTable
  title="All Students"
  count={student.length}
  columns={columns}
  data={filterStudent}
  showActions={true}
  onDelete={handleDelete}
/>
    </Layout>
  )
}

export default Students