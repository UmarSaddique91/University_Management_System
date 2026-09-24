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
import ProtectedRoute from './components/common/ProtectedRoute'
import TeacherCourses from './pages/Teacher/TeacherCourses'
import TeacherExams from './pages/Teacher/TeacherExams'
import TeacherAttendance from './pages/Teacher/TeacherAttendance'
import StudentCourses from './pages/Student/StudentCourses'
import StudentResults from './pages/Student/StudentResults'
import StudentAttendance from './pages/Student/StudentAttendance'

import { Routes, Route } from 'react-router-dom'

const App = () => {
  return (
    <Routes>
      {/* Public Authentication Routes */}
      <Route path="/" element={<Login />} />
      <Route path="/forgotPassword" element={<ForgotPassword />} />
      <Route path="/resetPassword" element={<ResetPassword />} />
      <Route path="/signUp" element={<SignUp />} />

      {/* Protected Routes (Admin & SuperAdmin) */}
      <Route
        path="/dashboard"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <DashBoard />
          </ProtectedRoute>
        }
      />
      <Route
        path="/teachers"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <Teachers />
          </ProtectedRoute>
        }
      />
      <Route
        path="/students"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <Students />
          </ProtectedRoute>
        }
      />
      <Route
        path="/academic-programs"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <AcademicPrograms />
          </ProtectedRoute>
        }
      />
      <Route
        path="/academicPrograms"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <AcademicPrograms />
          </ProtectedRoute>
        }
      />
      <Route
        path="/departments"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <Departments />
          </ProtectedRoute>
        }
      />
      <Route
        path="/courses"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <Courses />
          </ProtectedRoute>
        }
      />
      <Route
        path="/course-assignments"
        element={
          <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}>
            <CourseAssignments />
          </ProtectedRoute>
        }
      />

      {/* SuperAdmin Only Route */}
      <Route
        path="/admins"
        element={
          <ProtectedRoute allowedRoles={["SuperAdmin"]}>
            <Admins />
          </ProtectedRoute>
        }
      />

      {/* Teacher Portal Routes */}
      <Route
        path="/teacher/courses"
        element={
          <ProtectedRoute allowedRoles={["Teacher"]}>
            <TeacherCourses />
          </ProtectedRoute>
        }
      />
      <Route
        path="/teacher/exams"
        element={
          <ProtectedRoute allowedRoles={["Teacher"]}>
            <TeacherExams />
          </ProtectedRoute>
        }
      />
      <Route
        path="/teacher/attendance"
        element={
          <ProtectedRoute allowedRoles={["Teacher"]}>
            <TeacherAttendance />
          </ProtectedRoute>
        }
      />

      {/* Student Portal Routes */}
      <Route
        path="/student/courses"
        element={
          <ProtectedRoute allowedRoles={["Student"]}>
            <StudentCourses />
          </ProtectedRoute>
        }
      />
      <Route
        path="/student/results"
        element={
          <ProtectedRoute allowedRoles={["Student"]}>
            <StudentResults />
          </ProtectedRoute>
        }
      />
      <Route
        path="/student/attendance"
        element={
          <ProtectedRoute allowedRoles={["Student"]}>
            <StudentAttendance />
          </ProtectedRoute>
        }
      />
    </Routes>
  )
}

export default App