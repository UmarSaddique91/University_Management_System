import React from 'react'
import Login from "./pages/Auth/Login"
import DashBoard from './pages/Dashboards/AdminDashBoard'
import ForgotPassword from './pages/Auth/ForgotPassword'
import ResetPassword from './pages/Auth/ResetPassword'
import SignUp from './pages/Auth/SignUp'
import Teachers from './pages/Teachers'
import Students from './pages/Students'
import AcademicPrograms from './pages/AcademicPrograms'
import Departments from './pages/Departments'
import Courses from './pages/Courses'
import CourseAssignments from './pages/CourseAssignments'
import Admins from './pages/Admins'

import { Routes, Route } from 'react-router-dom'

const App = () => {
  return (
    <Routes>
      <Route path="/" element={<Login/>}/>
      <Route path="/dashboard" element={<DashBoard/>} />
      <Route path='/forgotPassword' element={<ForgotPassword/>}/>
      <Route path='/resetPassword' element={<ResetPassword/>}/>
      <Route path='/signUp' element={<SignUp/>}/>
      <Route path='/teachers' element={<Teachers/>}/>
      <Route path='/students' element={<Students/>} />
      <Route path='/academic-programs' element={<AcademicPrograms/>}/>
      <Route path='/academicPrograms' element={<AcademicPrograms/>}/>
      <Route path='/departments' element={<Departments/>} />
      <Route path='/courses' element={<Courses/>} />
      <Route path='/course-assignments' element={<CourseAssignments/>} />
      <Route path='/admins' element={<Admins/>} />
    </Routes>
  )
}

export default App